using FracturedState.Game.AI;
using FracturedState.Game.Data;
using HighlightingSystem;
using UnityEngine;
using Vectrosity;

namespace FracturedState.Game
{
    [RequireComponent(typeof(IndicatorFollow))]
    public class GroundIndicatorManager : MonoBehaviour, ISkillIndicatorManager
    {
        protected const float ClosestRecalcThreshold = 0.5f;

        [SerializeField] private Color lineColor;
        private float lineOffset = 0.75f;
        
        protected Ability skill;
        protected UnitManager castUnit;
        private Vector3 lastPos;
        private IndicatorFollow follow;

        private VectorLine line;
        
        public void SetSkill(Ability skill)
        {
            this.skill = skill;
            follow = GetComponent<IndicatorFollow>();
            if (!follow.enabled)
                follow.enabled = true;
        }

        protected virtual void OnEnable()
        {
            lastPos = Vector3.zero;
            line = VectorLine.SetLine3D(lineColor, transform.position, transform.position);
            line.lineWidth = 4;
            line.Draw3DAuto();
        }

        private void OnDestroy()
        {
            if (castUnit != null)
            {
                SwapColor(castUnit.OwnerTeam.TeamColor.UnityColor);
            }
            if (line != null)
            {
                line.StopDrawing3DAuto();
                VectorLine.Destroy(ref line);
            }
        }

        protected virtual void Update()
        {
            if (follow.enabled)
            {
                var tHit = RaycastUtil.RaycastTerrainAtMouse();
                if (tHit.collider == null) return;
                
                lastPos = tHit.point;
                GetClosestUnit();
                
                if (castUnit != null)
                {
                    line.points3[0] = transform.position + Vector3.up * lineOffset;
                    line.points3[1] = castUnit.transform.position + Vector3.up * lineOffset;
                }
                else
                {
                    line.points3[0] = transform.position;
                    line.points3[1] = transform.position;
                }
                if (Input.GetMouseButtonUp(0))
                {
                    if (castUnit == null)
                    {
                        SkillBarManager.ShowNoCast();
                        return;
                    }
                    follow.enabled = false;
                    SwapColor(castUnit.OwnerTeam.TeamColor.UnityColor);
                    castUnit.SetMicroState(new MicroUseAbilityState(castUnit, skill.Name, tHit.point));
                    castUnit.PropagateMicroState();
                    SkillManager.ResetSkill();
                }
            }

            // unit is casting so monitor
            if (!follow.enabled && castUnit != null)
            {
                line.points3[1] = castUnit.transform.position + Vector3.up * lineOffset;
                if (!castUnit.HasAbility(skill.Name) || castUnit.GetRemainingAbilityTime(skill.Name) > 0 || (!castUnit.IsMicroPrepped && !(castUnit.StateMachine.CurrentState is MicroUseAbilityState)))
                {
                    Destroy(gameObject);
                }
            }
            // casting unit died on their way to the location so clear
            else if (!follow.enabled && castUnit == null)
            {
                Destroy(gameObject);
            }
            // right or escape click cancel
            else if (follow.enabled && Input.GetMouseButtonUp(1) || Input.GetKeyDown(KeyCode.Escape))
            {
                Destroy(gameObject);
                SkillManager.ResetSkill();
            }
        }

        protected void GetClosestUnit()
        {
            if (castUnit != null)
            {
                SwapColor(castUnit.OwnerTeam.TeamColor.UnityColor);
                castUnit = null;
            }
            var structureHit = RaycastUtil.RaycastExteriorAtMouse();
            StructureManager structure = null;
            if (structureHit.transform != null)
            {
                structure = structureHit.transform.GetAbsoluteParent().GetComponent<StructureManager>();
            }
            var dist = float.MaxValue;
            for (int i = 0; i < SelectionManager.Instance.SelectedUnits.Count; i++)
            {
                var unit = SelectionManager.Instance.SelectedUnits[i];
                if (unit == null || !unit.IsAlive || !unit.HasAbility(skill.Name) || unit.GetRemainingAbilityTime(skill.Name) > 0 || unit.StateMachine.CurrentState is MicroUseAbilityState) 
                    continue;

                // if we're checking inside then make sure the unit is inside and in the same building
                if (structure != null && unit.WorldState == Nav.State.Exterior || unit.CurrentStructure != structure)
                    continue;
                else if (structure == null && unit.WorldState == Nav.State.Interior)
                    continue;
                
                var toPos = (unit.transform.position - lastPos).sqrMagnitude;
                if (toPos < dist)
                {
                    castUnit = unit;
                    dist = toPos;
                }
            }
            if (castUnit != null)
            {
                SwapColor(Color.white);
            }
        }

        private void SwapColor(Color color)
        {
            var highlight = castUnit.GetComponent<Highlighter>();
            if (highlight != null)
            {
                highlight.ConstantOn(color);
            }
        }
    }
}