namespace FracturedState.Game.Mutators
{
    public interface IMutator
    {
        int Cost { get; }
        
        bool CanMutate(UnitManager unit);
        
        void Mutate(UnitManager unit);
    }
}