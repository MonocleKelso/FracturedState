using FracturedState.Game.Data;
using UnityEngine;
using UnityEngine.UI;

namespace FracturedState.Game
{
    public class WorldCooldownIndicator : MonoBehaviour
    {
        [SerializeField] protected Text cooldown;
        
        protected Ability skill;
        protected UnitManager unit;
        
        private static Camera lookCamera;
        
        public void SetSkill(Ability skill)
        {
            this.skill = skill;
        }

        public void SetUnit(UnitManager unit)
        {
            this.unit = unit;
            if (lookCamera == null) lookCamera = Camera.main;
            if (unit != null) Position();
        }
        
        private void Update()
        {
            if (SkillManager.HoverSkill != skill)
            {
                Destroy(gameObject);
            }
            
            if (unit != null && unit.IsAlive && unit.HasAbility(skill.Name))
            {
                Position();
                Cooldown();
                return;
            }
            
            Destroy(gameObject);
        }
        
        private void Position()
        {
            var pos = unit.transform.position + Vector3.up * unit.Data.StatusIconHeight;
            pos = lookCamera.WorldToScreenPoint(pos);
            transform.position = pos;
        }

        private void Cooldown()
        {
            var coolTime = unit.GetRemainingAbilityTime(skill.Name);
            if (coolTime > 0)
            {
                cooldown.text = coolTime < 2 ? coolTime.ToString("0.0") : Mathf.CeilToInt(coolTime).ToString();
            }
            else
            {
                cooldown.text = "0";
            }
        }
    }
}