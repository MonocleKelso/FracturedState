using System.Collections.Generic;
using FracturedState.Game;
using FracturedState.Game.AI;
using FracturedState.Game.Data;
using FracturedState.Game.Nav;
using FracturedState.Game.Network;
using UnityEngine;

namespace FracturedState.Scripting
{
    public class FanaticMindControl : LocationAbility
    {
        public const float Range = 8;
        private const int MaxUnits = 4;
        public const string AttackAbilityName = "FanaticConfusionAttack";

        private const string EffectName = "Fanatic/FanaticMindControl";
        private const float EffectLife = 3;
        
        // unique set of units who are currently affected by this ability
        public static HashSet<UnitManager> AffectedUnits = new HashSet<UnitManager>();
        
        public FanaticMindControl(UnitManager caster, Vector3 location, Ability ability) : base(caster, location, ability) { }

        public override void ExecuteAbility()
        {
            base.ExecuteAbility();
            DoEffect();
            if (!FracNet.Instance.IsHost) return;

            var mask = caster.WorldState == State.Exterior
                ? GameConstants.ExteriorUnitAllMask
                : GameConstants.InteriorUnitAllMask;

            var nearby = Physics.OverlapSphere(location, Range, mask);

            if (nearby == null || nearby.Length == 0) return;

            var count = 0;
            foreach (var col in nearby)
            {
                if (count == MaxUnits) return;

                var unit = col.GetComponent<UnitManager>();
                if (unit == null || !unit.IsAlive || !unit.Data.IsSelectable || unit.OwnerTeam == caster.OwnerTeam) continue;
                
                // make sure the unit isn't casting an ability so we don't break channels or otherwise interrupt
                if (unit.StateMachine.CurrentState is MicroUseAbilityState) continue;
                
                // make sure the unit isn't already affected by this
                if (AffectedUnits.Contains(unit)) continue;
                
                // we send the caster ID here even though the ability won't use it just to make sure we build the
                // right kind of ability state on the other end
                unit.NetMsg.CmdUseAbility(AttackAbilityName, Vector3.zero, caster.NetMsg.NetworkId);
                count++;
            }
        }

        private void DoEffect()
        {
            var fx = ParticlePool.Instance.GetSystem(EffectName);
            fx.transform.position = location;
            fx.gameObject.SetLayerRecursively(caster.gameObject.layer);
            var life = fx.GetComponent<Lifetime>() ?? fx.gameObject.AddComponent<Lifetime>();
            life.SetLifetime(EffectLife);
        }
    }
}