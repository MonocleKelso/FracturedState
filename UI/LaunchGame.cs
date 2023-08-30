using FracturedState.Game;
using FracturedState.Game.Data;
using FracturedState.Game.Management;
using FracturedState.Game.Network;
using System.Linq;
using UnityEngine;

namespace FracturedState.UI
{
    public class LaunchGame : MonoBehaviour
    {
        public void Launch()
        {
            // check for good loadout
            var mutators = FracNet.Instance.NetworkActions.LocalTeam.Mutators; 
            if (mutators == null || mutators.Count == 0)
            {
                Chat.AddMessage("You can't be ready if you haven't picked a loadout!");
                return;
            }
            
            var mutatorCost = FracNet.Instance.NetworkActions.LocalTeam.Mutators.Select(m => m.Cost).Aggregate((total, cost) => total + cost);

            if (mutatorCost > Team.TotalMutatorCost)
            {
                Chat.AddMessage("You've picked too many loadout options. Don't be greedy!");
                return;
            }
            
            FracNet.Instance.StopLan();
            if (FracNet.Instance.IsHost)
            {
                var humanCount = SkirmishVictoryManager.SkirmishTeams.Count(t => t.IsHuman);
                var readyCount = SkirmishVictoryManager.SkirmishTeams.Count(t => t.IsHuman && t.IsReady);
                if (readyCount == humanCount - 1)
                {
                    SkirmishVictoryManager.CurrentMap = DataUtil.DeserializeXml<RawMapData>(DataLocationConstants.GameRootPath + DataLocationConstants.MapDirectory + "/" + MapSelect.CurrentMapName + "/" + "map.xml");
                    if (SkirmishVictoryManager.CurrentMap.StartingPoints.Length < SkirmishVictoryManager.SkirmishTeams.Count(t => !t.IsSpectator))
                    {
                        MultiplayerEventBroadcaster.TooManyPlayers();
                    }
                    else
                    {
                        FracNet.Instance.LoadMap(MapSelect.CurrentMapName);
                    }
                }
                else
                {
                    MultiplayerEventBroadcaster.HostWantsStart();
                }
            }
            else
            {
                FracNet.Instance.MakeReady();
            }
        }
    }
}