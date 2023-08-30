namespace FracturedState.Game.AI
{
    public class LandmineAttackState : CustomState
    {
        bool triggered;

        public LandmineAttackState(AttackStatePackage initPackage) : base(initPackage) { }

        public override void Execute()
        {
            if (!triggered)
            {
                triggered = true;
                if (owner.IsMine)
                {
                    owner.NetMsg.CmdTakeDamage(2, null, owner.Data.WeaponData.Name);
                }
            }
        }
    }
}