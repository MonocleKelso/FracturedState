using System.Collections.Generic;
using FracturedState.Game.Data;
using UnityEngine;

namespace FracturedState.Scripting
{
    public class TransportChargeMonitor : MonoBehaviour
    {
        [SerializeField] private CapsuleCollider col;
        
        private float lifeTime = 30;
        private Dictionary<UnitManager, float> hitTimes;
        private UnitManager unit;
        
        public void SetOwner(UnitManager owner)
        {
            unit = owner;
            hitTimes = new Dictionary<UnitManager, float>();
        }
        
        private void Update()
        {
            if (unit == null || !unit.IsAlive)
            {
                Destroy(gameObject);
            }

            transform.position = unit.transform.position;
            transform.rotation = unit.transform.rotation;

            lifeTime -= Time.deltaTime;
            if (lifeTime <= 0)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!unit.IsMine || unit.AISimulate) return;
            
            var hit = other.GetComponent<UnitManager>();
            if (hit == null || hit == unit || hit.Transport == unit) return;

            float time;
            if (!hitTimes.TryGetValue(hit, out time) || Time.time - time > 2)
            {
                hitTimes[hit] = Time.time;
                hit.NetMsg.CmdStun(1);
                hit.NetMsg.CmdTakeDamage(10, unit.NetMsg.NetworkId, Weapon.DummyName);
                hit.NetMsg.CmdSyncLocation(hit.transform.position + ((hit.transform.position - transform.position).normalized * (hit.Data.Physics.PathRadius * 5)));
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (!unit.IsMine || unit.AISimulate) return;
            
            var hit = other.GetComponent<UnitManager>();
            if (hit == null || hit == unit || hit.Transport == unit) return;
            
            hit.NetMsg.CmdSyncLocation(hit.transform.position + ((hit.transform.position - transform.position).normalized * (hit.Data.Physics.PathRadius * 5)));
        }

        private void OnDestroy()
        {
            hitTimes.Clear();
            hitTimes = null;
        }
    }
}