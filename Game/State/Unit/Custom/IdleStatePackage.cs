namespace FracturedState.Game.AI
{
    public class IdleStatePackage : ICustomStatePackage
    {
        public UnitManager Owner { get; protected set; }

        public IdleStatePackage(UnitManager owner)
        {
            this.Owner = owner;
        }
    }
}