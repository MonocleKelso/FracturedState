using FracturedState.Game.Network;
using System.Collections.Generic;
using FracturedState.Game.Data;
using UnityEngine;

namespace FracturedState.Game.AI
{
    public class FlamethrowerAttackState : CustomAttackState
    {
        private ParticleSystem flame;
        private ParticleSystem exhaust;
        private ParticleSystem idleExhaust;

        private AudioClip flameLoop;
        
        private readonly Dictionary<UnitManager, float> attackTimes;

        public FlamethrowerAttackState(AttackStatePackage initPackage) : base(initPackage)
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
            flameLoop = DataUtil.LoadBuiltInSound("Flamethrower/FlameLoop");
        }

        public override void Execute()
        {
            if (CheckTargetEligibility())
            {
                if (!CheckTargetSight())
                {
                    return;
                }
                
                // loop flame after initial sound plays
                var audio = owner.GetComponent<AudioSource>();
                if (flame.isPlaying && !audio.isPlaying)
                {
                    audio.clip = flameLoop;
                    audio.Play();
                }
                
                if (owner.LastFiredTime + weapon.FireRate > Time.time) return;
                
                // if out of range
                if (owner.Squad.Stance == SquadStance.Standard && (target.transform.position - owner.transform.position).magnitude > weapon.Range)
                {
                    if (path != null)
                    {
                        flame.Stop();
                        exhaust.Stop();
                        MoveOnPath();
                    }
                    else
                    {
                        if (!pathRequestSent)
                        {
                            CalcPathToTarget();
                        }
                    }
                }
                else
                {
                    
                    owner.transform.LookAt(target.transform);
                    if (!flame.isPlaying)
                    {
                        flame.Play();
                        audio.clip = DataUtil.LoadBuiltInSound(weapon.SoundEffects[0]);
                        audio.pitch = Random.Range(0.80f, 1.2f);
                        audio.Play();
                    }
                    if (!exhaust.isPlaying)
                    {
                        exhaust.Play();
                    }
                    if (!owner.AnimControl.IsPlaying("Fire"))
                    {
                        owner.AnimControl.Play("Fire");
                    }
                    
                    if (!FracNet.Instance.IsHost) return;
                    
                    var hits = Physics.CapsuleCastAll(owner.transform.position, target.transform.position + (owner.transform.forward * 4),
                        weapon.MuzzleRadius, (target.transform.position - owner.transform.position), weapon.Range, owner.GetEnemyLayerMask());
                                
                    foreach (var hit in hits)
                    {
                        var unit = hit.transform.GetComponent<UnitManager>();
                        if (unit != null && unit.IsAlive && unit.OwnerTeam != owner.OwnerTeam && unit.Transport == null)
                        {
                            var attack = true;
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
                owner.StateMachine.ChangeState(new UnitIdleState(owner));
            }
        }

        public override void Exit()
        {
            base.Exit();
            flame.Stop();
            exhaust.Stop();
            idleExhaust.Play();
            owner.GetComponent<AudioSource>().Stop();
        }
    }
}