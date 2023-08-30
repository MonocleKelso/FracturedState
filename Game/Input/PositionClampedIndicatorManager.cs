using FracturedState.Game.Data;
using UnityEngine;

namespace FracturedState.Game
{
    public abstract class PositionClampedIndicatorManager : MonoBehaviour, ISkillIndicatorManager, ISkillUnitBinder
    {
        protected Ability Skill;
        protected UnitManager Unit;
        
        public void SetSkill(Ability skill)
        {
            Skill = skill;
        }

        public void SetUnit(UnitManager unit)
        {
            Unit = unit;
        }

        private void Update()
        {
            if (Unit == null || !Unit.IsAlive || Input.GetMouseButtonUp(1) || Input.GetKeyDown(KeyCode.Escape))
            {
                Remove();
                return;
            }
            
            RotateToMouse();
            if (Input.GetMouseButtonUp(0))
            {
                ExecuteAbility();
                Remove();
            }
        }

        private void Remove()
        {
            SkillManager.ResetSkill();
            Destroy(gameObject);
        }

        private void RotateToMouse()
        {
            transform.position = Unit.transform.position;
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, GameConstants.TerrainMask))
            {
                var p = hit.point;
                p.y = transform.position.y;
                transform.LookAt(p);
            }
        }

        protected abstract void ExecuteAbility();
    }
}