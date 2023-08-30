using FracturedState.Game.Data;
using FracturedState.Game.Network;
using UnityEngine;
using UnityEngine.Networking;

namespace FracturedState.Game.Modules
{
    public class NetworkDamageModule : NetworkBehaviour
    {
        private DamageModule _damageModule;

        [ClientRpc]
        public void RpcSetArt(string path)
        {
            var art = DataUtil.LoadPrefab(path);
            if (art != null)
            {
                _damageModule = art.GetComponent<DamageModule>();
                if (_damageModule == null)
                {
                    Destroy(gameObject);
                    throw new FracturedStateException("Networked Damage Modules must have a Damage Module component attached");
                }
                art.transform.position = transform.position;
                art.transform.rotation = transform.rotation;
                art.transform.SetParent(transform);
            }
        }

        [ClientRpc]
        public void RpcSetOwner(NetworkIdentity owner)
        {
            if (_damageModule == null) return;
            var unit = owner.GetComponent<UnitManager>();
            if (unit == null) return;
            _damageModule.SetOwner(unit);
        }
        
        public void TakeDamage(int damageAmount, NetworkIdentity id, string weapon)
        {
            var messageId = LockStepProcessor.Instance.GetNextMessageId();
            RpcTakeDamage(messageId, damageAmount, id, weapon);
        }

        [ClientRpc]
        public void RpcTakeDamage(uint messageId, int damageAmount, NetworkIdentity id, string weapon)
        {
            var attacker = id != null ? id.GetComponent<UnitManager>() : null;
            WeaponBlockerDamageMessage msg = new WeaponBlockerDamageMessage(_damageModule, damageAmount, attacker, weapon);
            msg.Id = messageId;
            LockStepProcessor.Instance.AddMessage(msg);
            FracNet.Instance.NetworkActions.CmdLockStepReceived(messageId);
        }

        public UnitManager GetOwner()
        {
            return _damageModule != null ? _damageModule.GetOwner() : null;
        }
    }
}