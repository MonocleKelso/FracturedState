namespace FracturedState.Game.AI
{
    public interface ICustomStatePackage
    {
        UnitManager Owner { get; }
    }

    public abstract class CustomState : IState
    {
        protected UnitManager owner;

        public CustomState(ICustomStatePackage initPackage)
        {
            owner = initPackage.Owner;
        }

        public virtual void Enter() { }
        public virtual void Execute() { }
        public virtual void Exit() { }
    }
}