using System.Collections;
using FracturedState.Game;
using FracturedState.Game.AI;
using FracturedState.Game.Data;
using FracturedState.Game.Modules;
using FracturedState.Game.Network;
using UnityEngine;
using UnityEngine.Networking;

namespace FracturedState.Scripting
{
    public class WardenShield : LocationAbility, IMonitorAbility
    {
        internal class MoveToPlaceState : UnitMoveState
        {
            private readonly WardenShield _wardenShield;
            public bool Arrived { get; private set; }
            
            public MoveToPlaceState(UnitManager owner, Vector3 destination, WardenShield wardenShield)
                : base(owner, destination)
            {
                _wardenShield = wardenShield;
                Arrived = false;
            }

            protected override void AttackMoveEnemySearch()
            {
                // intentionally empty so we don't stop units
            }
            
            protected override void OnArrival()
            {
                Arrived = true;
                _wardenShield.PlaceShield();
            }
        }

        private const string DropSkillName = "WardenShieldDrop";
        
        private GameObject shieldContainer;
        private DamageModule shield;
        private MoveToPlaceState internalState;
        private bool animWaiting;
        private bool ownerSet = false;
        
        public WardenShield(UnitManager caster, Vector3 location, Ability ability) : base(caster, location, ability) { }

        public override void ExecuteAbility()
        {
            internalState = new MoveToPlaceState(caster, location, this);
            internalState.Enter();
        }

        public void PlaceShield()
        {
            if (caster.IsMine)
            {
                UnitBarkManager.Instance.AbilityBark(ability);
            }
            caster.AcceptInput = false;
            caster.transform.position = location;
            caster.AnimControl.Play("Aim", PlayMode.StopAll);
            caster.StartCoroutine(AnimWait());
        }
        
        private IEnumerator AnimWait()
        {
            animWaiting = true;
            yield return new WaitForSeconds(caster.AnimControl.GetClip("Aim").length);
            animWaiting = false;
            caster.AnimControl.Play("Fire", PlayMode.StopAll);
            caster.AddAbility(DropSkillName);
            UpdateSkillBar();

            if (caster.IsAlive && FracNet.Instance.IsHost)
            {
                shieldContainer = Object.Instantiate(PrefabManager.DamageModuleContainer, caster.transform.position, Quaternion.identity);
                NetworkServer.Spawn(shieldContainer);
                var ndm = shieldContainer.GetComponent<NetworkDamageModule>();
                ndm.RpcSetArt("Effects/Warden/Shield/WardenShieldv2");
                ndm.RpcSetOwner(caster.NetMsg.NetworkId);
            }
        }

        public void SetShield(GameObject sc)
        {
            shieldContainer = sc;
            ownerSet = true;
        }

        public void Update()
        {
            if (!internalState.Arrived)
            {
                internalState.Execute();
                return;
            }

            if (animWaiting || !ownerSet)
                return;

            // wait here for RPC to propagate so we can get the damage module
            if (shieldContainer != null && shield == null)
            {
                shield = shieldContainer.GetComponentInChildren<DamageModule>();
                return;
            }
            
            if (!shield.IsAlive)
            {
                caster.StateMachine.ChangeState(new UnitIdleState(caster));
            }
        }

        private void UpdateSkillBar()
        {
            // trigger eval of skill bar if this unit is still selected
            if (caster.IsMine && SelectionManager.Instance.SelectedUnits.Contains(caster))
            {
                SelectionManager.Instance.OnSelectionChanged.Invoke();
            }
        }
        
        void IMonitorAbility.Finish()
        {
            // do this here to start cooldown when the shield drops instead of when he starts the skill
            caster.UseAbility(ability.Name);
            caster.AcceptInput = true;
            caster.RemoveAbility(DropSkillName);
            UpdateSkillBar();
            if (FracNet.Instance.IsHost)
                Object.Destroy(shieldContainer);
        }
    }
}