namespace FracturedState.Game.AI
{
    public class SurgeonAttackState : CustomState
    {
        protected UnitManager target;
        IState childState;

        public SurgeonAttackState(AttackStatePackage initPackage) : base(initPackage)
        {
            target = initPackage.Target;
        }

        public override void Enter()
        {
            if (owner.HasAbility(SurgeonIdleState.HealAbilityName))
            {
                childState = new SurgeonIdleState(new IdleStatePackage(owner));
            }
            else
            {
                childState = new UnitAttackState(owner, target);
            }
            childState.Enter();
            owner.IsIdle = false;
        }

        public override void Execute()
        {
            childState.Execute();
        }

        public override void Exit()
        {
            childState.Exit();
        }
    }
}