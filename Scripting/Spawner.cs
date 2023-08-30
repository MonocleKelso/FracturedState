using FracturedState.Game;
using FracturedState.Game.Management;
using FracturedState.Game.Nav;
using FracturedState.Game.Network;
using UnityEngine;
using UnityEngine.Networking;

namespace FracturedState.Scripting
{
    /// <summary>
    /// A static utility class that allows scripts to spawn objects into the world
    /// </summary>
    public static class Spawner
    {
        public static GameObject SpawnUnit(string unitName, Vector3 location, Quaternion rotation, Team owner, State worldState)
        {
            var playerObj = GlobalNetworkActions.GetActions(owner).gameObject;
            var newUnit = Object.Instantiate(PrefabManager.NetworkUnitContainer, location, rotation);
            NetworkServer.SpawnWithClientAuthority(newUnit, playerObj);
            var msg = newUnit.GetComponent<UnitMessages>();
            msg.CmdCreateUnit(unitName, owner.NetworkedPlayerId);
            msg.CmdSetNavForSpawnedUnit((int)worldState);
            return newUnit;
        }

        public static GameObject SpawnedOwnedUnit(UnitManager ownerUnit, string unitName, Vector3 location, Quaternion rotation, Team owner, State worldState)
        {
            var playerObj = GlobalNetworkActions.GetActions(owner).gameObject;
            var newUnit = Object.Instantiate(PrefabManager.NetworkUnitContainer, location, rotation);
            NetworkServer.SpawnWithClientAuthority(newUnit, playerObj);
            var msg = newUnit.GetComponent<UnitMessages>();
            msg.CmdCreateUnit(unitName, owner.NetworkedPlayerId);
            msg.CmdSetNavForSpawnedUnit((int)worldState);
            msg.RpcSetOwnerUnit(ownerUnit.NetMsg.NetworkId);
            return newUnit;
        }

        public static GameObject SpawnObject(string effectName, State worldState)
        {
            var effect = ObjectPool.Instance.GetPooledObject(effectName);
            effect.SetLayerRecursively(worldState == State.Exterior ? GameConstants.ExteriorLayer : GameConstants.InteriorLayer);
            return effect;
        }
    }
}