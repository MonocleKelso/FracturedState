using System.Collections;
using Code.Game.Management;
using FracturedState.Game.Data;
using UnityEngine;

namespace FracturedState.Scripting
{
    public class BoostMorale : SelfAbility
    {
        private float time;
        
        public BoostMorale(UnitManager caster, Ability ability) : base(caster, ability) { }

        public override void ExecuteAbility()
        {
            base.ExecuteAbility();
            caster.Data.CapturePoints += 5;
            time = 3;
            CoroutineRunner.RunCoroutine(TrackBuff());
        }

        private IEnumerator TrackBuff()
        {
            while (time > 0)
            {
                time -= Time.deltaTime;
                yield return null;
            }
            
            if (caster != null)
            {
                caster.Data.CapturePoints -= 5;
            }
        }
    }
}