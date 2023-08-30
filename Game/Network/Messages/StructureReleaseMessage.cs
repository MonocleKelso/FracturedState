using FracturedState.Game.Management;

namespace FracturedState.Game.Network
{
    public class StructureReleaseMessage : ILockStepMessage
    {
        public uint Id { get; set; }
        Team team;
        StructureManager structure;

        public StructureReleaseMessage(Team team, StructureManager structure)
        {
            this.team = team;
            this.structure = structure;
        }

        public void Process()
        {
            TerritoryManager.Instance.ReleaseStructure(structure, team);
        }
    }
}