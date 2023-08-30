using FracturedState.Game.Data;
using FracturedState.Game.Management;
using System.Collections.Generic;
using System.Linq;

namespace FracturedState.Game.AI
{
    public enum IdleMode { UseIdle, ForceAll }

    public class CaptureStructure : AtomicGoal<CaptureStructure>
    {
        private readonly TerritoryData territory;
        private StructureManager structure;
        private readonly IdleMode mode;
        private Squad squad;

        public CaptureStructure(Team ownerTeam, TerritoryData territory, IdleMode mode = IdleMode.UseIdle)
            : base(ownerTeam)
        {
            this.territory = territory;
            this.mode = mode;
        }

        public CaptureStructure(Team ownerTeam, TerritoryData territory, Squad squad)
            : base(ownerTeam)
        {
            this.territory = territory;
            this.squad = squad;
        }

        public override void Activate()
        {
            base.Activate();
            var structures = TerritoryManager.Instance.GetStructuresInTerritory(territory);
            var unownedStructures = structures?.Where(s => 
                s.StructureData.CanBeCaptured && (s.OwnerTeam == null || (s.OwnerTeam != OwnerTeam && s.OwnerTeam.Side != OwnerTeam.Side))).ToArray();
            if (unownedStructures?.Length > 0)
            {
                structure = unownedStructures[UnityEngine.Random.Range(0, unownedStructures.Length)];
            }

            if (structure == null) return;
            
            // if squad wasn't passed in ctor then find one based on what type of goal this is
            if (squad == null)
            {
                squad = OwnerTeam.GetIdleSquad();
                if (squad == null && mode == IdleMode.ForceAll)
                {
                    // if we're forcing non-idle squads to capture then find the first one that isn't in a building already
                    if (OwnerTeam.Squads != null)
                    {
                        for (var i = 0; i < OwnerTeam.Squads.Count; i++)
                        {
                            if (OwnerTeam.Squads[i].Members.Count(u => u.CurrentStructure == null) == OwnerTeam.Squads[i].Members.Count)
                            {
                                squad = OwnerTeam.Squads[i];
                                break;
                            }
                        }
                    }
                }
            }

            if (squad == null) return;
            
            for (var i = 0; i < squad.Members.Count; i++)
            {
                var m = squad.Members[i];
                if (m != null && m.IsAlive && m.AcceptInput && m.NetMsg != null)
                {
                    m.NetMsg.CmdEnterStructure(structure.GetComponent<Identity>().UID);
                }
            }
        }

        public override GoalState Process()
        {
            Status = base.Process();

            if (structure == null)
            {
                Status = GoalState.Completed;
            }
            else if (squad == null || squad.Members.Count == 0)
            {
                Status = GoalState.Failed;
            }
            else if (structure != null)
            {
                var idleOutsideCount = 0;
                for (var i = 0; i < squad.Members.Count; i++)
                {
                    if (squad.Members[i].IsIdle && squad.Members[i].WorldState == Nav.State.Exterior)
                    {
                        idleOutsideCount++;
                    }
                }
                if (idleOutsideCount == squad.Members.Count)
                {
                    Status = GoalState.Failed;
                }

                if (Status != GoalState.Failed)
                {
                    if (structure.OwnerTeam == null || structure.OwnerTeam != OwnerTeam)
                    {
                        Status = GoalState.Active;
                    }
                    else if (structure.OwnerTeam == OwnerTeam)
                    {
                        if (structure.CurrentPoints < structure.StructureData.CapturePoints)
                        {
                            Status = GoalState.Active;
                        }
                        else
                        {
                            Status = GoalState.Completed;
                        }
                    }
                }
            }
            return Status;
        }
    }
}