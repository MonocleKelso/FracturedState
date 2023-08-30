using FracturedState.Game.Management;
using FracturedState.Game.Nav;
using UnityEngine;

namespace FracturedState.Game.AI
{
    public class DefendTile : AtomicGoal<DefendTile>
    {
        Squad squad;
        GameObject tile;
        Vector3 destination;

        public DefendTile(Team ownerTeam, Squad squad, GameObject tile) : base(ownerTeam)
        {
            this.squad = squad;
            this.tile = tile;
        }

        public override void Activate()
        {
            base.Activate();
            // get squad member with biggest radius
            float radius = 0;
            for (int i = 0; i < squad.Members.Count; i++)
            {
                var m = squad.Members[i];
                if (m != null && m.Data.Physics.PathRadius > radius)
                {
                    radius = m.Data.Physics.PathRadius;
                }
            }
            // pick a random point around the middle of the tile
            float x = Random.Range(0, 20);
            float z = Random.Range(0, 20);
            Vector3 position = tile.transform.position + new Vector3(x, 0, z);
            var point = AStarPather.Instance.ExteriorGrid.GetClosestPoint(position, radius);
            if (point != null)
            {
                destination = point.WorldPosition;
                for (int i = 0; i < squad.Members.Count; i++)
                {
                    var m = squad.Members[i];
                    if (m != null && m.IsAlive && m.AcceptInput)
                    {
                        m.NetMsg.CmdMove(destination);
                    }
                }
            }
            else
            {
                // we failed to find a place to navigate to
                Status = GoalState.Failed;
            }
        }
    }
}