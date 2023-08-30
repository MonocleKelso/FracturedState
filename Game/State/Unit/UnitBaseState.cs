
using UnityEngine;

namespace FracturedState.Game.AI
{
    public abstract class UnitBaseState : IState
    {
        protected readonly UnitManager Owner;

        protected UnitBaseState(UnitManager owner)
        {
            Owner = owner;
        }

        public virtual void Enter() { }

        public virtual void Execute() { }

        public virtual void Exit() { }

        protected void InfantrySpace()
        {
            if (Owner.LocoMotor == null) return;
            if (Owner.InCover) return;
            if (Owner.CurrentFirePoint != null) return;
            
            var mask = Owner.WorldState == Nav.State.Exterior
                ? GameConstants.ExteriorUnitAllMask
                : GameConstants.InteriorUnitAllMask;
            var nearby = Physics.OverlapSphere(Owner.transform.position, Owner.Data.Physics.PathRadius * 1.5f, mask);
            var count = 0;
            var avg = Vector3.zero;
            foreach (var n in nearby)
            {
                if (n.transform == Owner.transform) continue;
                avg += n.transform.position;
                count++;
            }
            if (count == 0) return;
            
            avg /= count;
            var pos = Owner.transform.position;
            var y = pos.y;
            pos += (pos - avg).normalized * Time.deltaTime;
            // prevent units from climbing off the ground
            pos.y = y;
            
            // if the unit is outside then prevent nudging into an exterior collider or off the map
            var ray = new Ray(pos + Vector3.up * 100, -Vector3.up);
            if (Owner.WorldState == Nav.State.Exterior)
            {
                if (RaycastUtil.RayCheckExterior(ray)) return;
                if (!RaycastUtil.IsUnderTerrain(pos + Vector3.up)) return;
            }
            // if unit is inside then prevent nudging into interior collider or out of building
            else
            {
                if (!RaycastUtil.RayCheckExterior(ray)) return;
                if (RaycastUtil.RayCheckInterior(ray)) return;
            }
            // if we made it here than update unit position to nudge
            Owner.transform.position = pos;
        }
        
        public override string ToString()
        {
            return GetType().ToString();
        }
    }
}