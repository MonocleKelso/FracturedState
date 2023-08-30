using UnityEngine;
using System.Collections.Generic;

namespace FracturedState.Game
{
    public class StateEffectManager
    {
        public UnitManager Owner { get; private set; }

        ParticleSystem[] currentSystems;

        ParticleSystem[] idleSystems;
        ParticleSystem[] moveSystems;

        public StateEffectManager(UnitManager owner)
        {
            Owner = owner;
            PopulateEffectSystem(owner.Data.StatefulEffects.IdleEffects, ref idleSystems);
            PopulateEffectSystem(owner.Data.StatefulEffects.MoveEffects, ref moveSystems);
        }

        void PopulateEffectSystem(string[] effects, ref ParticleSystem[] system)
        {
            if (effects != null && effects.Length > 0)
            {
                List<ParticleSystem> tmp = new List<ParticleSystem>(effects.Length);
                for (int i = 0; i < effects.Length; i++)
                {
                    Transform t = Owner.transform.GetChildByName(effects[i]);
                    if (t != null)
                    {
                        var p = t.GetComponent<ParticleSystem>();
                        if (p != null)
                        {
                            tmp.Add(p);
                        }
                    }
                }
                system = tmp.ToArray();
            }
        }

        public void StopCurrentSystem()
        {
            PlaySystems(null);
        }

        public void PlayIdleSystems()
        {
            PlaySystems(idleSystems);
        }

        public void PlayMoveSystems()
        {
            PlaySystems(moveSystems);
        }

        void PlaySystems(ParticleSystem[] systems)
        {
            if (currentSystems != null)
            {
                foreach (var sys in currentSystems)
                {
                    sys.Stop();
                }
            }

            if (systems != null)
            {
                foreach (var sys in systems)
                {
                    sys.Play();
                }
                currentSystems = systems;
            }
        }
    }
}