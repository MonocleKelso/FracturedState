namespace FracturedState.Game.AI
{
    public class AttackStatePackage : ICustomStatePackage
    {
        public UnitManager Owner { get; }
        public UnitManager Target { get; }

        public AttackStatePackage(UnitManager owner, UnitManager target)
        {
            Owner = owner;
            Target = target;
        }
    }
}