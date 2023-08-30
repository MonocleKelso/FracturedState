using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FracturedState.Game.AI;
using FracturedState.Game.Network;
using FracturedState.Scripting;
using UnityEngine;

namespace Code.Game.Management
{
    public static class VotarySpawner
    {
        private const float SpawnRate = 2;
        private const int MaxSpawnCount = 5;
        
        private static readonly Dictionary<UnitManager, List<UnitManager>> Shrines = new Dictionary<UnitManager, List<UnitManager>>();
        private static readonly Dictionary<UnitManager, float> ShrineSpawnTimes = new Dictionary<UnitManager, float>();

        public static void CheckAndSpawn(UnitManager shrine)
        {
            if (!FracNet.Instance.IsHost) return;
            
            float lastSpawned;
            if (ShrineSpawnTimes.TryGetValue(shrine, out lastSpawned))
            {
                var spawnRate = shrine.StateMachine.CurrentState is ShrineAttackState ? SpawnRate + 1 : SpawnRate;
                
                if (lastSpawned + spawnRate < Time.time)
                {
                    List<UnitManager> spawns;
                    if (Shrines.TryGetValue(shrine, out spawns))
                    {
                        // enumerate back through votaries and clean out dead ones
                        for (var i = spawns.Count - 1; i >= 0; i--)
                        {
                            if (spawns[i] == null || !spawns[i].IsAlive)
                            {
                                spawns.RemoveAt(i);
                            }
                        }
                        
                        if (spawns.Count < MaxSpawnCount)
                        {
                            Spawn(shrine);
                        }
                    }
                    else
                    {
                        Spawn(shrine);
                    }
                    
                    ShrineSpawnTimes[shrine] = Time.time;
                }

                return;
            }
            
            ShrineSpawnTimes[shrine] = Time.time;
        }

        public static List<UnitManager> GetVotaries(UnitManager shrine)
        {
            return shrine.Squad?.Members.Where(m => m.NetMsg.OwnerUnit == shrine).ToList();
        }

        private static void Spawn(UnitManager shrine)
        {
            List<UnitManager> spawns;
            if (!Shrines.TryGetValue(shrine, out spawns))
            {
                spawns = new List<UnitManager>();
                Shrines[shrine] = spawns;
            }
            
            var votary = Spawner.SpawnUnit("Votary", shrine.transform.position, shrine.transform.rotation, shrine.OwnerTeam, shrine.WorldState);
            var u = votary.GetComponent<UnitManager>();
            spawns.Add(u);
            Loader.Instance.StartCoroutine(WaitForOwner(u, shrine));
        }
        
        private static IEnumerator WaitForOwner(UnitManager votary, UnitManager shrine)
        {
            while (votary.NetMsg == null)
            {
                yield return null;
            }
            
            votary.NetMsg.RpcSetOwnerUnit(shrine.NetMsg.NetworkId);
        }
    }
}