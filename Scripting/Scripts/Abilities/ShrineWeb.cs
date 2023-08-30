using System.Collections.Generic;
using Code.Game.Management;
using FracturedState.Game;
using FracturedState.Game.AI;
using FracturedState.Game.Data;
using FracturedState.Game.Network;
using UnityEngine;

namespace FracturedState.Scripting
{
    public class ShrineWeb : SelfAbility, IMonitorAbility
    {
        private const float Lifetime = 5;
        private const string EffectPath = "Shrine/votaryWeb";
        private const float DamageTick = 0.25f;

        private float timeRemaining = 0;
        private List<UnitManager> votaries;
        private readonly List<particleAttractorLinear> attractors = new List<particleAttractorLinear>();
        private readonly Dictionary<UnitManager, float> damageTimes = new Dictionary<UnitManager, float>();
        
        public ShrineWeb(UnitManager caster, Ability ability) : base(caster, ability) { }

        public override void ExecuteAbility()
        {
            base.ExecuteAbility();
            votaries = VotarySpawner.GetVotaries(caster);
            if (votaries == null) return;

            foreach (var a in votaries)
            {
                foreach (var b in votaries)
                {
                    if (a == b) continue;
                    if (a == null || b == null) continue;

                    var fx = DataUtil.LoadBuiltInParticleSystem(EffectPath);
                    fx.transform.position = a.transform.position;
                    fx.SetLayerRecursively(a.gameObject.layer);
                    fx.transform.parent = a.transform;
                    var att = fx.GetComponent<particleAttractorLinear>();
                    att.target = b.transform;
                    attractors.Add(att);
                }
            }

            timeRemaining = Lifetime;
        }

        public void Update()
        {
            if (caster == null || !caster.IsAlive)
            {
                Cleanup();
                return;
            }
            
            timeRemaining -= Time.deltaTime;
            if (timeRemaining <= 0)
            {
                Cleanup();
                caster.StateMachine.ChangeState(new UnitIdleState(caster));
                return;
            }
            
            // clean up dead votary effects
            for (var i = attractors.Count - 1; i >= 0; i--)
            {
                var att = attractors[i];
                if (att == null) continue;
                
                if (att.target == null)
                {
                    att.GetComponent<ParticleSystem>().Stop();
                    Object.Destroy(att.gameObject);
                    attractors.RemoveAt(i);
                }
                else
                {
                    var unit = att.target.GetComponent<UnitManager>();
                    if (!unit.IsAlive)
                    {
                        att.GetComponent<ParticleSystem>().Stop();
                        Object.Destroy(att.gameObject);
                        attractors.RemoveAt(i);
                    }
                }
            }
            
            // cast between votaries and deal damage
            if (!FracNet.Instance.IsHost) return;
            
            foreach (var a in votaries)
            {
                foreach (var b in votaries)
                {
                    if (a == null || b == null || a == b) continue;

                    var origin = a.transform.position;
                    var toNext = (b.transform.position - origin);
                    var direction = toNext.normalized;
                    var distance = toNext.magnitude;

                    var hits = Physics.SphereCastAll(origin, 0.25f, direction, distance, GameConstants.ExteriorUnitAllMask);
                    foreach (var hit in hits)
                    {
                        var unit = hit.collider.gameObject.GetComponent<UnitManager>();
                        if (unit == null || !unit.IsAlive || unit.OwnerTeam == caster.OwnerTeam ||
                            unit.OwnerTeam.Side == caster.OwnerTeam.Side) continue;

                        float dCheck;
                        if (damageTimes.TryGetValue(unit, out dCheck))
                        {
                            if (dCheck + DamageTick < Time.time)
                            {
                                damageTimes[unit] = Time.time;
                                unit.ProcessDamageInterrupts(1, caster);
                            }
                        }
                        else
                        {
                            damageTimes[unit] = Time.time;
                            unit.ProcessDamageInterrupts(1, caster);
                        }
                    }
                }
            }
        }

        private void Cleanup()
        {
            foreach (var fx in attractors)
            {
                if (fx == null) continue;
                
                fx.GetComponent<ParticleSystem>().Stop();
                Object.Destroy(fx.gameObject);
            }
            attractors.Clear();
        }

        public void Finish() { }
    }
}