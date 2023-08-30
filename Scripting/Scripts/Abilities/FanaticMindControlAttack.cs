using System.Collections;
using FracturedState.Game;
using FracturedState.Game.AI;
using FracturedState.Game.Data;
using FracturedState.Game.Nav;
using FracturedState.Game.Network;
using UnityEngine;

namespace FracturedState.Scripting
{
    public class FanaticMindControlAttack : TargetAbility
    {
        private const float Duration = 8;
        private float _lifeTime = -1;

        private const string EffectName = "Fanatic/MindControlStatus";
        private ParticleSystem _status;

        public FanaticMindControlAttack(UnitManager caster, UnitManager target, Ability ability) : base(caster, target, ability) { }

        public override void ExecuteAbility()
        {
            FanaticMindControl.AffectedUnits.Add(caster);
            caster.AcceptInput = false;
            target = null;
            GetTarget();
            _lifeTime = Duration;
            _status = ParticlePool.Instance.GetSystem(EffectName);
            _status.gameObject.SetLayerRecursively(caster.gameObject.layer);
            Loader.Instance.StartCoroutine(MonitorStatus());
        }

        private IEnumerator MonitorStatus()
        {
            while (_lifeTime > 0 && caster != null && caster.IsAlive)
            {
                Update();
                yield return null;
            }

            Finish();
        }
        
        private void Update()
        {
            if (caster == null) return;
            
            _status.transform.position = caster.transform.position + Vector3.up * caster.Data.StatusIconHeight;
            _lifeTime -= Time.deltaTime;
            if (_lifeTime <= 0) return;

            if (target == null || !target.IsAlive || target.WorldState != caster.WorldState || caster.IsIdle)
            {
                GetTarget();
            }
        }

        private void Finish()
        {
            if (_status != null)
            {
                ParticlePool.Instance.ReturnSystem(_status);
            }
            if (caster == null) return;
            caster.AcceptInput = true;
            FanaticMindControl.AffectedUnits.Remove(caster);
            if (!caster.IsAlive) return;
            caster.StateMachine.ChangeState(new UnitIdleState(caster));
        }

        private void GetTarget()
        {
            if (!FracNet.Instance.IsHost) return;
            
            var mask = caster.WorldState == State.Exterior
                ? GameConstants.ExteriorUnitAllMask
                : GameConstants.InteriorUnitAllMask;

            var nearby = Physics.OverlapSphere(caster.transform.position, FanaticMindControl.Range, mask);
            if (nearby == null) return;
            foreach (var n in nearby)
            {
                var unit = n.GetComponent<UnitManager>();
                if (unit != null && unit.IsAlive && unit != caster && unit.Data.IsSelectable && caster.OwnerTeam == unit.OwnerTeam)
                {
                    target = unit;
                    caster.OnTargetChanged(target);
                    return;
                }
            }
        }
    }
}