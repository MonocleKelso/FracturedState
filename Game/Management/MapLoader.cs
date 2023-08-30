using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FracturedState.Game.Data;
using FracturedState.Game.Management;
using FracturedState.Game.Network;
using FracturedState.UI;
using UnityEngine;

namespace FracturedState.Game
{
    public class MapLoader : MonoBehaviour
    {
        public string MapName { get; set; }

        private IEnumerator Start()
        {
            CompassUI.Instance.Init();
            yield return null;
            // if we have a custom map due to a map transfer then use it and ignore the map name passed in
            if (SkirmishVictoryManager.CustomMap != null)
            {
                SkirmishVictoryManager.CurrentMap = SkirmishVictoryManager.CustomMap;
                SkirmishVictoryManager.CustomMap = null;
            }
            else
            {
                SkirmishVictoryManager.CurrentMap = DataUtil.DeserializeXml<RawMapData>($"{DataLocationConstants.GameRootPath}{DataLocationConstants.MapDirectory}/{MapName}/map.xml");
            }
            yield return StartCoroutine(DataUtil.LoadMap(SkirmishVictoryManager.CurrentMap));
            
            while (!Loader.Instance.ProbeRenderComplete)
            {
                yield return null;
            }
            FracNet.Instance.NetworkActions.CmdUpdateMapProgress(1);
            yield return null;

            if (FracNet.Instance.IsHost)
            {
                var ready = false;
                // ensure everyone has loaded the map before proceeding
                while (!ready)
                {
                    ready = true;
                    for (var i = 0; i < SkirmishVictoryManager.SkirmishTeams.Count; i++)
                    {
                        var team = SkirmishVictoryManager.SkirmishTeams[i];
                        if (team.IsHuman)
                        {
                            var bar = LoadingBarManager.GetBar(team);
                            if (bar == null || bar.Progress != 1)
                            {
                                ready = false;
                            }
                        }
                    }
                    yield return null;
                }
                // reset readines again to check before starting match
                SkirmishVictoryManager.ResetTeamReadiness();
                var startPoints = new List<int>();
                var s = 0;
                while (s < SkirmishVictoryManager.CurrentMap.StartingPoints.Length)
                {
                    startPoints.Add(s++);
                }
                for (var i = 0; i < SkirmishVictoryManager.SkirmishTeams.Count; i++)
                {
                    var team = SkirmishVictoryManager.SkirmishTeams[i];
                    var actions = GlobalNetworkActions.GetActions(team);
                    if (!team.IsSpectator)
                    {
                        var faction = XmlCacheManager.Factions[team.Faction];
                        // set starting camera position for human players
                        Vector3 startPos;
                        var randPoint = Random.Range(0, startPoints.Count);
                        SkirmishVictoryManager.CurrentMap.StartingPoints[startPoints[randPoint]].TryVector3(out startPos);
                        startPoints.RemoveAt(randPoint);
                        if (team.IsHuman)
                        {
                            actions.RpcSetStartingLocation(startPos);
                        }
                        // spawn starting units
                        var startingUnits = new List<UnitObject>();
                        foreach (var unit in faction.StartingUnits)
                        {
                            for (var u = 0; u < unit.Count; u++)
                            {
                                startingUnits.Add(XmlCacheManager.Units[unit.Name]);
                            }
                        }
                        var sr = new SquadRequest(team, startingUnits);
                        sr.RallyPoint = startPos;
                        FracNet.Instance.CallReinforcements(sr);
                    }
                    else
                    {
                        // spectators just get their camera positions set to the first starting location
                        Vector3 startPos;
                        SkirmishVictoryManager.CurrentMap.StartingPoints[0].TryVector3(out startPos);
                        actions.RpcSetStartingLocation(startPos);
                    }
                }
                // loop all teams again and ping readiness which will just send a reply back
                for (var i = 0; i < SkirmishVictoryManager.SkirmishTeams.Count; i++)
                {
                    if (SkirmishVictoryManager.SkirmishTeams[i].IsHuman)
                        GlobalNetworkActions.GetActions(SkirmishVictoryManager.SkirmishTeams[i]).RpcPingReady();
                }
                // wait to make sure all clients have received all messages
                var humanCount = SkirmishVictoryManager.SkirmishTeams.Count(t => t.IsHuman);
                while (SkirmishVictoryManager.SkirmishTeams.Count(t => t.IsHuman && t.IsReady) != humanCount)
                {
                    yield return null;
                }
                FracNet.Instance.NetworkActions.RpcBeginMatch();
            }
            Destroy(gameObject);
        }
    }
}