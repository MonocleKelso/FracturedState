using FracturedState.Game.AI;
using FracturedState.Game.Data;
using UnityEngine;
using UnityEngine.Networking;

namespace FracturedState.Game.Network
{
    public class UnitExecuteAbilityMessage : ILockStepMessage
    {
        public uint Id { get; set; }
        UnitManager unit;
        string abilityName;
        Vector3? position;
        UnitManager target;

        public UnitExecuteAbilityMessage(UnitManager unit, string abilityName)
        {
            this.unit = unit;
            this.abilityName = abilityName;
        }

        public UnitExecuteAbilityMessage(UnitManager unit, string abilityName, Vector3 position)
        {
            this.unit = unit;
            this.abilityName = abilityName;
            this.position = position;
        }

        public UnitExecuteAbilityMessage(UnitManager unit, string abilityName, NetworkIdentity targetId)
        {
            this.unit = unit;
            this.abilityName = abilityName;
            target = targetId.GetComponent<UnitManager>();
        }

        public void Process()
        {
            if (target != null)
            {
                unit.SetMicroState(new MicroUseAbilityState(unit, abilityName, target));
            }
            else if (position != null)
            {
                unit.SetMicroState(new MicroUseAbilityState(unit, abilityName, position.Value));
            }
            else
            {
                unit.SetMicroState(new MicroUseAbilityState(unit, abilityName));
            }
            unit.ExecuteMicroState();
        }
    }
}