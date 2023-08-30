using FracturedState.Game.AI;
using FracturedState.Game.Data;
using FracturedState.Game.Management;
using UnityEngine;
using UnityEngine.Networking;

namespace FracturedState.Game.Network
{
    public class UnitMessages : NetworkBehaviour
    {
        private UnitManager unitManager;
        public NetworkIdentity NetworkId { get; private set; }

        public UnitManager OwnerUnit { get; private set; }

        private void Awake()
        {
            unitManager = GetComponent<UnitManager>();
            NetworkId = GetComponent<NetworkIdentity>();
        }

        private static void RegisterLockStepMessage(ILockStepMessage msg)
        {
            LockStepProcessor.Instance.AddMessage(msg);
            FracNet.Instance.NetworkActions.CmdLockStepReceived(msg.Id);
        }

        #region Creation

        public void LocalCreate(string unitName, int ownerId)
        {
            unitManager.CreateNetworkedUnit(unitName, ownerId);
        }

        public void LocalAICreate(string unitName, string ownerName)
        {
            unitManager.CreateNetworkedAIUnit(unitName, ownerName);
        }

        [Command]
        public void CmdCreateUnit(string unitName, int ownerId)
        {
            RpcCreateUnit(unitName, ownerId);
        }

        [ClientRpc]
        private void RpcCreateUnit(string unitName, int ownerId)
        {
            unitManager.CreateNetworkedUnit(unitName, ownerId);
        }

        [Command]
        public void CmdCreateAIUnit(string unitName, string ownerName)
        {
            RpcCreateAIUnit(unitName, ownerName);
        }

        [ClientRpc]
        private void RpcCreateAIUnit(string unitName, string ownerName)
        {
            unitManager.CreateNetworkedAIUnit(unitName, ownerName);
        }

        [Command]
        public void CmdSetNavForSpawnedUnit(int navState)
        {
            RpcSetNavForSpawnedUnit(navState);
        }

        [ClientRpc]
        private void RpcSetNavForSpawnedUnit(int navState)
        {
            unitManager.WorldState = (Nav.State)navState;
            if (unitManager.IsMine)
            {
                var layer = unitManager.WorldState == Nav.State.Exterior ? GameConstants.ExteriorUnitLayer : GameConstants.InteriorUnitLayer;
                unitManager.gameObject.SetLayerRecursively(layer, "Vision");
            }
            else
            {
                var layer = unitManager.WorldState == Nav.State.Exterior ? GameConstants.ExteriorEnemyLayer : GameConstants.InteriorEnemyLayer;
                unitManager.gameObject.SetLayerRecursively(layer, "Vision");
            }
        }

        #endregion

        #region Movement

        [Command]
        public void CmdSyncLocation(Vector3 location)
        {
            var msgId = LockStepProcessor.Instance.GetNextMessageId();
            RpcSyncLocation(msgId, location);
        }

        [ClientRpc]
        private void RpcSyncLocation(uint msgId, Vector3 location)
        {
            RegisterLockStepMessage(new UnitSyncLocationMessage(msgId, unitManager, location));
        }
        
        [Command]
        public void CmdMove(Vector3 destination)
        {
            var msgId = LockStepProcessor.Instance.GetNextMessageId();
            RpcMove(msgId, destination);
        }

        [ClientRpc]
        private void RpcMove(uint msgId, Vector3 destination)
        {
            var msg = new UnitMoveMessage(unitManager, destination) {Id = msgId};
            RegisterLockStepMessage(msg);
        }

        [Command]
        public void CmdSetFacing(Quaternion facing)
        {
            var msgId = LockStepProcessor.Instance.GetNextMessageId();
            RpcSetFacing(msgId, facing);
        }

        [ClientRpc]
        private void RpcSetFacing(uint msgId, Quaternion facing)
        {
            var msg = new SquadSetFacingMessage(unitManager, facing) {Id = msgId};
            RegisterLockStepMessage(msg);
        }

        [Command]
        public void CmdEnterStructure(int structureId)
        {
            var msgId = LockStepProcessor.Instance.GetNextMessageId();
            RpcEnterStructure(msgId, structureId);
        }

        [ClientRpc]
        private void RpcEnterStructure(uint msgId, int structureId)
        {
            var msg = new UnitEnterStructureMessage(unitManager, structureId) {Id = msgId};
            RegisterLockStepMessage(msg);
        }

        [Command]
        public void CmdExitStructure(Vector3 destination)
        {
            var msgId = LockStepProcessor.Instance.GetNextMessageId();
            RpcExitStructure(msgId, destination);
        }

        [ClientRpc]
        private void RpcExitStructure(uint msgId, Vector3 destination)
        {
            var msg = new UnitExitStructureMessage(unitManager, destination) {Id = msgId};
            RegisterLockStepMessage(msg);
        }

        [Command]
        public void CmdSyncStructureEnter(int structureId, Vector3 position)
        {
            var msgId = LockStepProcessor.Instance.GetNextMessageId();
            RpcSyncStructureEnter(msgId, structureId, position);
        }

        [ClientRpc]
        private void RpcSyncStructureEnter(uint msgId, int structureId, Vector3 position)
        {
            var msg = new UnitStructureEnterSync(unitManager, structureId, position) {Id = msgId};
            RegisterLockStepMessage(msg);
        }

        [Command]
        public void CmdSuppressPoint(int structureId, string pointName)
        {
            var msgId = LockStepProcessor.Instance.GetNextMessageId();
            RpcSuppressPoint(msgId, structureId, pointName);
        }

        [ClientRpc]
        private void RpcSuppressPoint(uint msgId, int structureId, string pointName)
        {
            var msg = new UnitSuppressMessage(unitManager, structureId, pointName) {Id = msgId};
            RegisterLockStepMessage(msg);
        }

        #endregion

        #region Attack/Damage/Heal

        [Command]
        public void CmdSetTarget(NetworkIdentity target)
        {
            var msgId = LockStepProcessor.Instance.GetNextMessageId();
            RpcSetTarget(msgId, target);
        }

        [ClientRpc]
        private void RpcSetTarget(uint msgId, NetworkIdentity target)
        {
            var t = target != null ? target.GetComponent<UnitManager>() : null;
            var msg = new UnitSetTargetMessage(unitManager, t) {Id = msgId};
            RegisterLockStepMessage(msg);
        }
       
        // this is intentionally not lockstepped because it's a graphical effect
        // and any target acquisition that results from it is lockstepped
        [ClientRpc]
        public void RpcMiss(NetworkIdentity attacker)
        {
            if (unitManager != null && unitManager.DamageProcessor != null)
            {
                unitManager.DamageProcessor.Miss(attacker.GetComponent<UnitManager>());
            }
        }

        [Command]
        public void CmdHeal(int amount)
        {
            if (unitManager.DamageProcessor == null) return;
            
            var healMod = unitManager.Stats.HealModifier;
            if (!Mathf.Approximately(healMod, 0))
            {
                amount = Mathf.RoundToInt(amount + (amount * (100f / healMod)));
            }
            var msgId = LockStepProcessor.Instance.GetNextMessageId();
            RpcHeal(msgId, amount);
        }

        [ClientRpc]
        private void RpcHeal(uint msgId, int amount)
        {
            var msg = new UnitHealMessage(unitManager, amount) {Id = msgId};
            RegisterLockStepMessage(msg);
        }

        [Command]
        public void CmdTakeDamage(int amount, NetworkIdentity attacker, string weapon)
        {
            if (unitManager.DamageProcessor == null) return;
            
            var msgId = LockStepProcessor.Instance.GetNextMessageId();
            RpcTakeDamage(msgId, amount, attacker, weapon);
        }

        [ClientRpc]
        private void RpcTakeDamage(uint msgId, int amount, NetworkIdentity attacker, string weapon)
        {
            var atk = attacker != null ? attacker.GetComponent<UnitManager>() : null;
            var msg = new UnitTakeDamageMessage(unitManager, atk, amount, weapon) {Id = msgId};
            RegisterLockStepMessage(msg);
        }

        [Command]
        public void CmdTakeProjectileDamage(int amount, NetworkIdentity attacker, Vector3 position, string weapon)
        {
            if (unitManager.DamageProcessor == null) return;
            
            var msgId = LockStepProcessor.Instance.GetNextMessageId();
            RpcTakeProjectileDamage(msgId, amount, attacker, position, weapon);
        }

        [ClientRpc]
        private void RpcTakeProjectileDamage(uint msgId, int amount, NetworkIdentity attacker, Vector3 position, string weapon)
        {
            var atk = attacker != null ? attacker.GetComponent<UnitManager>() : null;
            var msg = new UnitTakeProjectileDamageMessage(unitManager, atk, position, amount, weapon) {Id = msgId};
            RegisterLockStepMessage(msg);
        }

        #endregion

        #region Transport

        [Command]
        public void CmdEnterTransport(NetworkIdentity transportId)
        {
            var msgId = LockStepProcessor.Instance.GetNextMessageId();
            RpcEnterTransport(msgId, transportId);
        }

        [ClientRpc]
        private void RpcEnterTransport(uint msgId, NetworkIdentity transportId)
        {
            var msg = new UnitEnterTransportMessage(unitManager, transportId.GetComponent<UnitManager>()) {Id = msgId};
            RegisterLockStepMessage(msg);
        }

        [Command]
        public void CmdExitTransport()
        {
            var msgId = LockStepProcessor.Instance.GetNextMessageId();
            RpcExitTransport(msgId);
        }

        [ClientRpc]
        private void RpcExitTransport(uint msgId)
        {
            var msg = new UnitExitTransportMessage(unitManager) {Id = msgId};
            RegisterLockStepMessage(msg);
        }

        #endregion

        #region Cover

        [Command]
        public void CmdTakeCover(int coverId, string pointName)
        {
            var msgId = LockStepProcessor.Instance.GetNextMessageId();
            RpcTakeCover(msgId, coverId, pointName);
        }

        [ClientRpc]
        private void RpcTakeCover(uint msgId, int coverId, string pointName)
        {
            var cover = ObjectUIDLookUp.Instance.GetCoverManager(coverId);
            var point = cover.GetPointByName(pointName);
            var msg = new UnitTakeCoverMessage(unitManager, cover, point) {Id = msgId};
            RegisterLockStepMessage(msg);
        }

        #endregion

        #region Fire Point

        [Command]
        public void CmdTakeFirePoint(string pointName)
        {
            var msgId = LockStepProcessor.Instance.GetNextMessageId();
            RpcTakeFirePoint(msgId, pointName);
        }

        [ClientRpc]
        private void RpcTakeFirePoint(uint msgId, string firePointName)
        {
            var firePoint = unitManager.CurrentStructure.FindFirePoint(firePointName);
            var msg = new UnitTakeFirePointMessage(unitManager, firePoint) {Id = msgId};
            RegisterLockStepMessage(msg);
        }

        #endregion

        #region Micro

        [Command]
        public void CmdChangeStance(int stance)
        {
            RpcChangeStance(LockStepProcessor.Instance.GetNextMessageId(), stance);
        }

        [ClientRpc]
        private void RpcChangeStance(uint msgId, int stance)
        {
            RegisterLockStepMessage(new SquadChangeStanceMessage(msgId, unitManager, (SquadStance) stance));
        }
        
        [Command]
        public void CmdMicroMove(Vector3 destination)
        {
            var msgId = LockStepProcessor.Instance.GetNextMessageId();
            RpcMicroMove(msgId, destination);
        }

        [ClientRpc]
        private void RpcMicroMove(uint msgId, Vector3 destination)
        {
            var msg = new MicroMoveMessage(unitManager, destination) {Id = msgId};
            RegisterLockStepMessage(msg);
        }

        [Command]
        public void CmdUseAbility(string abilityName, Vector3 position, NetworkIdentity target)
        {
            var msgId = LockStepProcessor.Instance.GetNextMessageId();
            RpcUseAbility(msgId, abilityName, position, target);
        }

        [ClientRpc]
        private void RpcUseAbility(uint msgId, string abilityName, Vector3 position, NetworkIdentity target)
        {
            var ability = XmlCacheManager.Abilities[abilityName];
            UnitExecuteAbilityMessage msg;
            if (ability.Targetting == TargetType.None)
            {
                msg = new UnitExecuteAbilityMessage(unitManager, abilityName);
            }
            else if (ability.Targetting == TargetType.Ground || ability.Targetting == TargetType.Structure)
            {
                msg = new UnitExecuteAbilityMessage(unitManager, abilityName, position);
            }
            else
            {
                msg = new UnitExecuteAbilityMessage(unitManager, abilityName, target);
            }
            msg.Id = msgId;
            RegisterLockStepMessage(msg);
        }

        #endregion

        #region Structure

        [Command]
        public void CmdCaptureStructure(int structureId)
        {
            var msgId = LockStepProcessor.Instance.GetNextMessageId();
            RpcCaptureStructure(msgId, structureId);
        }

        [ClientRpc]
        private void RpcCaptureStructure(uint msgId, int structureId)
        {
            var msg = new CaptureStructureMessage(structureId, unitManager.OwnerTeam) {Id = msgId};
            RegisterLockStepMessage(msg);
        }

        #endregion

        #region Buff/Debuff

        [Command]
        public void CmdApplyBuff(int buffType, float amount, float duration, string fx)
        {
            var msgId = LockStepProcessor.Instance.GetNextMessageId();
            RpcApplyBuff(msgId, buffType, amount, duration, fx);
        }

        [ClientRpc]
        private void RpcApplyBuff(uint msgId, int buffType, float amount, float duration, string fx)
        {
            var msg = new UnitApplyBuffMessage(unitManager, (BuffType) buffType, amount, duration, fx) {Id = msgId};
            RegisterLockStepMessage(msg);
        }

        [Command]
        public void CmdStun(float duration)
        {
            var msgId = LockStepProcessor.Instance.GetNextMessageId();
            RpcStun(msgId, duration);
        }

        [ClientRpc]
        private void RpcStun(uint msgId, float duration)
        {
            var msg = new UnitStunMessage(unitManager, duration) {Id = msgId};
            RegisterLockStepMessage(msg);
        }

        [Command]
        public void CmdUnStun()
        {
            var msgId = LockStepProcessor.Instance.GetNextMessageId();
            RpcUnStun(msgId);
        }

        [ClientRpc]
        private void RpcUnStun(uint msgId)
        {
            var msg = new UnitUnStunMessage(unitManager) {Id = msgId};
            RegisterLockStepMessage(msg);
        }

        #endregion

        [ClientRpc]
        public void RpcSetOwnerUnit(NetworkIdentity owner)
        {
            OwnerUnit = owner == null ? null : owner.GetComponent<UnitManager>();
            if (OwnerUnit != null)
            {
                OwnerUnit.Squad?.AddSquadUnit(unitManager);
            }
            unitManager.StateMachine.ChangeState(new UnitIdleState(unitManager));
        }
    }
}