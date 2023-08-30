using FracturedState.Game.Modules;
using UnityEngine.Networking;

namespace FracturedState.Game.Network
{
    public class WeaponBlockerDamageMessage : ILockStepMessage
    {
        public uint Id { get; set; }

        private DamageModule _module;
        private int _amount;
        private UnitManager _attacker;
        private string _weapon;
        
        public WeaponBlockerDamageMessage(DamageModule module, int amount, UnitManager attacker, string weapon)
        {
            _module = module;
            _amount = amount;
            _attacker = attacker;
            _weapon = weapon;
        }
        
        public void Process()
        {
            if (_module == null) return;
            _module.TakeDamage(_amount, _attacker, _weapon);
        }
    }
}