using FracturedState.Game.Data;
using FracturedState.Game.Management;
using FracturedState.Game.Network;
using System.Collections.Generic;
using UnityEngine;

namespace FracturedState.Game.AI
{
    public class FlamethrowerPassengerAttackState : CustomAttackState
    {
        ParticleSystem flame;
        ParticleSystem exhaust;
        ParticleSystem idleExhaust;

        protected Dictionary<UnitManager, float> attackTimes;

        public FlamethrowerPassengerAttackState(AttackStatePackage package) : base(package)
        {
            attackTimes = new Dictionary<UnitManager, float>();
        }

        public override void Enter()
        {
            base.Enter();
            flame = owner.transform.GetChildByName("FlamethrowerFlame").GetComponent<ParticleSystem>();
            exhaust = owner.transform.GetChildByName("FlamethrowerExhaustHigh").GetComponent<ParticleSystem>();
            idleExhaust = owner.transform.GetChildByName("FlamethrowerExhaustLow").GetComponent<ParticleSystem>();
            idleExhaust.Stop();
        }

        public override void Execute()
        {
            if (owner.Transport != null && owner.PassengerSlot != null)
            {
                owner.transform.position = owner.PassengerSlot.position;
            }
            if (target != null && target.IsAlive && owner.PassengerSlot != null && VisibilityChecker.Instance.HasSight(owner, target))
            {
                Vector3 toTarget = (target.transform.position - owner.transform.position);
                bool inVision = Vector3.Dot(owner.PassengerSlot.forward, toTarget.normalized) > ConfigSettings.Instance.Values.FirePointVisionThreshold;
                bool inRange = toTarget.magnitude < owner.ContextualWeapon.Range;
                if (inVision && inRange)
                {
                    owner.transform.LookAt(target.transform);
                    if (!flame.isPlaying)
                    {
                        flame.Play();
                    }
                    if (!exhaust.isPlaying)
                    {
                        exhaust.Play();
                    }
                    if (FracNet.Instance.IsHost)
                    {
                        RaycastHit[] hits = Physics.CapsuleCastAll(owner.transform.position, owner.transform.forward * weapon.Range + owner.transform.forward * 4,
                                    weapon.MuzzleRadius, owner.transform.forward, weapon.Range, owner.GetEnemyLayerMask());

                        foreach (var hit in hits)
                        {
                            var unit = hit.transform.GetComponent<UnitManager>();
                            if (unit != null && unit.IsAlive && unit.OwnerTeam != owner.OwnerTeam)
                            {
                                bool attack = true;
                                if (attackTimes.ContainsKey(unit))
                                {
                                    attack = Time.time - attackTimes[unit] >= weapon.FireRate;
                                }
                                if (attack)
                                {
                                    attackTimes[unit] = Time.time;
                                    EvalTarget(unit);
                                }
                            }
                        }
                    }
                }
                else
                {
                    owner.StateMachine.ChangeState(new PassengerIdleState(owner, owner.Transport, owner.PassengerSlot));
                }
            }
            else
            {
                owner.StateMachine.ChangeState(new PassengerIdleState(owner, owner.Transport, owner.PassengerSlot));
            }
        }

        public override void Exit()
        {
            base.Exit();
            flame.Stop();
            exhaust.Stop();
            idleExhaust.Play();
        }
    }
}