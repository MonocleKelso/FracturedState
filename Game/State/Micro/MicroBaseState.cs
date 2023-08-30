
namespace FracturedState.Game.AI
{
    public abstract class MicroBaseState : IState
    {
        protected UnitManager owner;

        public MicroBaseState(UnitManager owner)
        {
            this.owner = owner;
        }

        public virtual void Enter()
        {
            owner.IsMicroing = true;
        }

        public virtual void Execute() { }

        public virtual void Exit()
        {
            owner.IsMicroing = false;
        }
    }
}