using UnityEngine;
using System.Collections.Generic;

namespace FracturedState.Game
{
    public class ParticlePool
    {
        private static ParticlePool instance;
        public static ParticlePool Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ParticlePool();
                }
                return instance;
            }
        }

        private Transform poolParent;
        private Dictionary<string, List<ParticleSystem>> lookup;

        private ParticlePool()
        {
            poolParent = new GameObject("ParticlePool").transform;
            lookup = new Dictionary<string, List<ParticleSystem>>();
        }

        public ParticleSystem GetSystem(string name)
        {
            var n = name.Replace('/', '_');
            if (lookup.ContainsKey(n))
            {
                List<ParticleSystem> particles = lookup[n];
                if (particles.Count > 0)
                {
                    ParticleSystem pSys = particles[0];
                    particles.RemoveAt(0);
                    if (pSys.transform.parent != poolParent)
                    {
                        pSys.transform.parent.gameObject.SetActive(true);
                    }
                    else
                    {
                        pSys.gameObject.SetActive(true);
                    }
                    return pSys;
                }
            }

            GameObject particle = Data.DataUtil.LoadBuiltInParticleSystem(name);
            var p = particle.GetComponent<ParticleSystem>();
            if (p == null)
            {
                p = particle.GetComponentInChildren<ParticleSystem>();
            }
            if (!p.main.loop)
            {
                particle.AddComponent<ParticleManager>();
            }
            return p;
        }

        public void ReturnSystem(ParticleSystem system)
        {
            var t = system.transform;
            // if the returned system is a child of an empty for rotational purposes then use that transform instead
            if (system.transform.parent != null && system.transform.parent != poolParent)
            {
                t = system.transform.parent;
            }
            t.position = Vector3.zero;
            t.parent = poolParent;
            t.gameObject.SetActive(false);
            List<ParticleSystem> pList;
            if (lookup.TryGetValue(system.gameObject.name, out pList))
            {
                pList.Add(system);
            }
            else
            {
                pList = new List<ParticleSystem>();
                pList.Add(system);
                lookup[t.gameObject.name] = pList;
            }
        }
    }
}