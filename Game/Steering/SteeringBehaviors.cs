using UnityEngine;
using FracturedState.Game.Data;

namespace FracturedState.Game.AI
{
    public static class SteeringBehaviors
    {
        private const float AvoidanceForce = 0.3f;

        public static Vector3 CalculateUnitSteering(UnitManager unit, Vector3 point)
        {
            return Arrive(unit, point, unit.CurrentVelocity) + UnitAvoid(unit, point, unit.CurrentVelocity);
        }

        private static Vector3 UnitAvoid(UnitManager unit, Vector3 node, Vector3 velocity)
        {
            var mag = velocity.magnitude;
            if (mag <= 0) return Vector3.zero;
            
            RaycastHit hit;
            if (Physics.Raycast(new Ray(unit.transform.position, unit.transform.forward), out hit, mag, GameConstants.ExteriorUnitAllMask))
            {
                if (hit.transform == unit.transform)
                    return Vector3.zero;

                var speed = unit.Squad?.MoveSpeed ?? unit.Data.Physics.MaxSpeed;
                return (hit.transform.position - velocity).normalized * (speed * AvoidanceForce);
            }

            return Vector3.zero;
        }

        private static Vector3 Arrive(UnitManager unit, Vector3 node, Vector3 velocity)
        {
            var speed = unit.Squad?.MoveSpeed ?? unit.Data.Physics.MaxSpeed;
            var toTarget = node - unit.transform.position;
            var normToTarget = toTarget.normalized;
            var desiredVelocity = normToTarget * speed * (toTarget.magnitude / ConfigSettings.Instance.Values.CloseEnoughThreshold);
            return Vector3.ClampMagnitude(desiredVelocity - velocity, speed);
        }
    }
}