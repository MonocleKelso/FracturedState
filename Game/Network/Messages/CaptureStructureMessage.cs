using FracturedState.Game.Management;

namespace FracturedState.Game.Network
{
    public class CaptureStructureMessage : ILockStepMessage
    {
        public uint Id { get; set; }
        private readonly StructureManager structure;
        private readonly Team owner;

        public CaptureStructureMessage(int structureId, Team owner)
        {
            structure = ObjectUIDLookUp.Instance.GetStructureManager(structureId);
            this.owner = owner;
        }

        public void Process()
        {
            TerritoryManager.Instance.CaptureStructure(structure, owner);
            var localTeam = FracNet.Instance.LocalTeam;
            if (owner == localTeam || owner.Side == localTeam.Side)
            {
                var ui = structure.GetComponent<CaptureProgressUI>();
                if (ui != null)
                {
                    ui.enabled = false;
                }

                if (owner == localTeam)
                {
                    InterfaceSoundPlayer.PlayCapture();   
                    StructureManager.AddOwnedStructure(structure);
                    var territory = TerritoryManager.Instance.GetStructureAssignment(structure);
                    if (territory != null)
                    {
                        MultiplayerEventBroadcaster.CaptureBuilding(territory.Name);
                    }
                }
            }
            if (FracNet.Instance.IsHost)
            {
                structure.SpawnGarrison(owner);
            }
        }
    }
}