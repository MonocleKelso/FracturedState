namespace FracturedState.Game.Network
{
    /// <summary>
    /// A common interface for all network messages that need to be procesed as part
    /// of the lock step simulation. Things like unit movement, attacking, garrisoning, using abilitites, dying etc.
    /// Effects related messages do not fall into this category necessarily.
    /// </summary>
    public interface ILockStepMessage
    {
        uint Id { get; }
        void Process();
    }
}