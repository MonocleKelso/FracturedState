using FracturedState.Game.AI;
using FracturedState.Game.Data;
using UnityEngine;
using UnityEngine.UI;

namespace FracturedState.Game
{
    public class WorldIndicatorManager : MonoBehaviour, ISkillIndicatorManager, ISkillUnitBinder
    {
        [SerializeField] protected Button button;
        [SerializeField] protected Text cooldown;
        [SerializeField] protected GraphicRaycaster raycaster;
        
        protected Ability skill;
        protected UnitManager unit;

        private static Camera lookCamera;
        
        public void SetSkill(Ability skill)
        {
            this.skill = skill;
            button.image.sprite = Ability.GetIcon(skill.IconAtlas, skill.IconPath);
        }

        public void SetUnit(UnitManager unit)
        {
            this.unit = unit;
            if (lookCamera == null) lookCamera = Camera.main;
            if (unit != null) Position();
        }

        public void ExecuteSkill()
        {
            if (skill == null || unit == null) return;

            if (!string.IsNullOrEmpty(skill.IndicatorPath))
            {
                var indicator = DataUtil.LoadGroundIndicator(skill.IndicatorPath);
                var man = indicator.GetComponent<ISkillIndicatorManager>();
                man?.SetSkill(skill);
                var unitBind = indicator.GetComponent<ISkillUnitBinder>();
                unitBind?.SetUnit(unit);
            }
            else
            {
                unit.SetMicroState(new MicroUseAbilityState(unit, skill.Name));
                unit.PropagateMicroState();
            }
        }
        
        protected virtual void Update()
        {
            if (Input.GetMouseButtonUp(0) && !RaycastUtil.IsMouseInUI() || Input.GetKeyDown(KeyCode.Escape))
            {
                SkillManager.ResetSkill();
                Destroy(gameObject);
                return;
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
            cooldown.gameObject.SetActive(coolTime > 0);
            button.interactable = coolTime <= 0;
            raycaster.enabled = coolTime <= 0;
            if (coolTime > 0)
            {
                cooldown.text = coolTime < 2 ? coolTime.ToString("0.0") : Mathf.CeilToInt(coolTime).ToString();
            }
        }
    }
}