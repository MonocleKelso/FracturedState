using System.Collections.Generic;
using System.Linq;
using FracturedState.Game.Data;
using FracturedState.Game.Management.StructureBonus;
using FracturedState.Game.Network;
using FracturedState.Game.StatTracker;
using UnityEngine;
using UnityEngine.Events;

namespace FracturedState.Game.Management
{
    public class TerritoryOwnerChangedEvent : UnityEvent<TerritoryData, Team> { }
    
    public sealed class TerritoryManager
    {
        private class TeamTerritoryScore
        {
            public Team Owner { get; }
            public int Score { get; set; }

            public TeamTerritoryScore(Team owner)
            {
                Owner = owner;
            }
        }

        private static TerritoryManager instance;
        public static TerritoryManager Instance => instance ?? (instance = new TerritoryManager());

        public readonly TerritoryOwnerChangedEvent OnOwnerChanged = new TerritoryOwnerChangedEvent();

        /// <summary>
        /// Returns the point on the edge of the map closest to the given point
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static Vector3 GetClosestMapEdge(Vector3 point)
        {
            Vector3 startPoint;
            var offset = ConfigSettings.Instance.Values.StartingUnitSpawnRadius;
            // determine closest map edge from territory location
            // is this point closer to the left or right side of the map
            var leftSide = Mathf.Abs(point.x - SkirmishVictoryManager.CurrentMap.XLowerBound) < Mathf.Abs(point.x - SkirmishVictoryManager.CurrentMap.XUpperBound);
            // is this point closer to the top or the bottom of the map
            var bottomSide = Mathf.Abs(point.z - SkirmishVictoryManager.CurrentMap.ZLowerBound) < Mathf.Abs(point.z - SkirmishVictoryManager.CurrentMap.ZUpperBound);
            var x = leftSide ? SkirmishVictoryManager.CurrentMap.XLowerBound - offset : SkirmishVictoryManager.CurrentMap.XUpperBound + offset;
            var z = bottomSide ? SkirmishVictoryManager.CurrentMap.ZLowerBound - offset : SkirmishVictoryManager.CurrentMap.ZUpperBound + offset;
            var xTest = new Vector3(x, point.y, point.z);
            var zTest = new Vector3(point.x, point.y, z);
            if ((xTest - point).sqrMagnitude < (zTest - point).sqrMagnitude)
            {
                startPoint = xTest;
            }
            else
            {
                startPoint = zTest;
            }
            return startPoint;
        }

        public static void Reset()
        {
            if (instance.helperParent != null)
            {
                instance.helperParent.SetActive(true);
            }
            instance.OnOwnerChanged.RemoveAllListeners();
            instance = new TerritoryManager();
        }

        // a list of every territory on the map
        public List<TerritoryData> AllTerritories { get; private set; }

        // a lookup for which territory a terrain piece belongs to
        private readonly Dictionary<GameObject, TerritoryData> terrainLookup;

        // a lookup to get all of the structures inside a given territory
        private readonly Dictionary<TerritoryData, List<StructureManager>> structureLookup;

        // a lookup to get which territory a structure belongs to
        private readonly Dictionary<StructureManager, TerritoryData> territoryLookup;

        // a lookup to get each team's ownership score for a given territory
        private readonly Dictionary<TerritoryData, List<TeamTerritoryScore>> scoreLookup;

        // a lookup to get the owner of a given territory
        private readonly Dictionary<TerritoryData, Team> ownerLookup;

        // a lookup to get all the territories owned by a team
        private readonly Dictionary<Team, List<TerritoryData>> ownedTerritoryLookup;

        // a lookup to get the player that owns a structure
        private readonly Dictionary<StructureManager, Team> structureOwnerLookup;

        // a lookup to get the topmost parent for highlighting helper objects
        private readonly Dictionary<TerritoryData, GameObject> ownerHelperLookup;

        private readonly GameObject helperParent;

        private TerritoryManager()
        {
            terrainLookup = new Dictionary<GameObject,TerritoryData>();
            structureLookup = new Dictionary<TerritoryData, List<StructureManager>>();
            territoryLookup = new Dictionary<StructureManager, TerritoryData>();
            scoreLookup = new Dictionary<TerritoryData, List<TeamTerritoryScore>>();
            ownerLookup = new Dictionary<TerritoryData, Team>();
            ownedTerritoryLookup = new Dictionary<Team, List<TerritoryData>>();
            structureOwnerLookup = new Dictionary<StructureManager, Team>();
            ownerHelperLookup = new Dictionary<TerritoryData, GameObject>();
            helperParent = GameObject.Find(GameConstants.TerritoryHelperName);
        }

        /// <summary>
        /// Returns the territory with the given name or null if there is no match
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public TerritoryData GetTerritoryByName(string name)
        {
            return AllTerritories.SingleOrDefault(t => t.Name == name);
        }
        
        /// <summary>
        /// Assigns the given terrain to the given territory
        /// </summary>
        public void AddTerrainAssignment(GameObject terrain, TerritoryData territory)
        {
            terrainLookup[terrain] = territory;

            if (AllTerritories == null)
            {
                AllTerritories = new List<TerritoryData> {territory};
            }
            else if (!AllTerritories.Contains(territory))
            {
                AllTerritories.Add(territory);
            }

            GameObject helper;
            if (!ownerHelperLookup.TryGetValue(territory, out helper))
            {
                helper = new GameObject(territory.Name + "_helper");
                ownerHelperLookup[territory] = helper;
            }

            var meshFilter = terrain.GetComponent<MeshFilter>();
            var h = new GameObject("helper");
            h.transform.position = meshFilter.transform.position;
            h.transform.rotation = meshFilter.transform.rotation;
            h.transform.position += Vector3.up * 0.1f;
            var hMeshFilter = h.AddComponent<MeshFilter>();
            h.AddComponent<MeshRenderer>();
            hMeshFilter.mesh = meshFilter.mesh;
            h.transform.parent = helper.transform;
        }

        public void FinalizeTerritories()
        {
            var props = GameObject.Find("TerrainParent").GetComponent<TerritoryHelperProperties>();
            var keys = ownerHelperLookup.Keys;
            foreach (var key in keys)
            {
                var helper = ownerHelperLookup[key];
                var children = helper.GetComponentsInChildren<Transform>().Where(t => t != helper.transform).ToArray();
                var avgPos = Vector3.zero;
                foreach (var child in children)
                {
                    child.transform.parent = null;
                    child.GetComponent<Renderer>().material = props.HelperMaterial;
                    avgPos += child.transform.position;
                }
                avgPos /= children.Length;
                helper.transform.position = avgPos;
                foreach (var child in children)
                {
                    child.transform.parent = helper.transform;
                }
                helper.transform.localScale = new Vector3(0.995f, 0.995f, 0.995f);
                helper.transform.parent = helperParent.transform;
                helperParent.SetLayerRecursively(GameConstants.TerrainLayer);
                if (key.Recruit)
                {
                    Vector3 rally;
                    key.RallyPoint.TryVector3(out rally);
                    var point = GetClosestMapEdge(rally);
                    var btn = Object.Instantiate(Loader.Instance.RecruitButton, point, Quaternion.identity);
                    btn.SetTerritory(key);
                }
            }
            helperParent.SetActive(false);
        }

        /// <summary>
        /// Looks up which territory the terrain under the given structure belongs to and assigns the structure
        /// to that territory. Does nothing if the terrain doesn't belong to a territory
        /// </summary>
        public void AddStructureAssignment(StructureManager structure, GameObject terrain)
        {
            TerritoryData territory;
            if (terrainLookup.TryGetValue(terrain, out territory))
            {
                territoryLookup[structure] = territory;
                List<StructureManager> tStructures;
                if (structureLookup.TryGetValue(territory, out tStructures))
                {
                    tStructures.Add(structure);
                }
                else
                {
                    structureLookup[territory] = new List<StructureManager>() { structure };
                }
            }
        }

        /// <summary>
        /// Returns the rally point for the given territory
        /// </summary>
        /// <param name="territoryName"></param>
        /// <returns></returns>
        public Vector3 GetTerritoryRallyPoint(string territoryName)
        {
            var territory = AllTerritories.FirstOrDefault(t => t.Name == territoryName);
            var location = Vector3.zero;
            territory?.RallyPoint.TryVector3(out location);
            return location;
        }

        /// <summary>
        /// Returns a list of structures in the given territory or null if no structures are present in that territory
        /// </summary>
        public List<StructureManager> GetStructuresInTerritory(TerritoryData territory)
        {
            List<StructureManager> structures;
            return structureLookup.TryGetValue(territory, out structures) ? structures : null;
        }

        /// <summary>
        /// Returns the territory that the given terrain object belongs to or null if it belongs to no territory
        /// </summary>
        public TerritoryData GetTerrainAssignment(GameObject terrain)
        {
            return terrainLookup.ContainsKey(terrain) ? terrainLookup[terrain] : null;
        }

        public List<GameObject> GetTerrainsInTerritory(TerritoryData territory)
        {
            var keys = terrainLookup.Keys.GetEnumerator();
            var goList = new List<GameObject>();
            while (keys.MoveNext())
            {
                if (keys.Current != null && terrainLookup[keys.Current] == territory)
                {
                    goList.Add(keys.Current);
                }
            }
            keys.Dispose();
            return goList;
        }

        /// <summary>
        /// Returns the number of teams who are occupying the territory the given terrain belongs to.
        /// Always returns 0 if the terrain does not belong to a territory
        /// </summary>
        public int GetTerritoryTeamCount(GameObject terrain)
        {
            var territory = GetTerrainAssignment(terrain);
            if (territory == null) return 0;
            
            List<TeamTerritoryScore> scores;
            if (scoreLookup.TryGetValue(territory, out scores))
            {
                return scores.Count;
            }
            return 0;
        }

        /// <summary>
        /// Returns the territory that the given structure belongs to or null if it belongs to no territory
        /// </summary>
        public TerritoryData GetStructureAssignment(StructureManager structure)
        {
            if (territoryLookup.ContainsKey(structure))
            {
                return territoryLookup[structure];
            }
            return null;
        }

        /// <summary>
        /// Returns the team that currently owns the given structure
        /// </summary>
        public Team GetStructureOwner(StructureManager structure)
        {
            Team owner;
            if (structureOwnerLookup.TryGetValue(structure, out owner))
            {
                return owner;
            }
            return null;
        }

        /// <summary>
        /// Returns the name of the territory associated with the given piece of terrain
        /// </summary>
        public Team GetTerritoryOwner(GameObject terrain)
        {
            var t = GetTerrainAssignment(terrain);
            if (t == null) return null;
            
            Team owner;
            if (ownerLookup.TryGetValue(t, out owner))
            {
                return owner;
            }

            return null;

        }

        /// <summary>
        /// Returns a List of territories owned by the given team or null if the team owns no territories
        /// </summary>
        public List<TerritoryData> GetOwnedTerritories(Team team)
        {
            List<TerritoryData> t;
            return ownedTerritoryLookup.TryGetValue(team, out t) ? t : null;
        }

        /// <summary>
        /// Returns the number of territories owned by the given team
        /// </summary>
        public int GetOwnedTerritoryCount(Team team)
        {
            var t = GetOwnedTerritories(team);
            return t?.Count ?? 0;
        }

        /// <summary>
        /// Returns the number of territories owned by the team with the most territories
        /// </summary>
        public int GetMaxOwnedTerritories()
        {
            if (ownedTerritoryLookup.Keys.Count == 0)
                return 0;

            var max = 0;
            var keys = ownedTerritoryLookup.Keys;
            foreach (var key in keys)
            {
                var count = ownedTerritoryLookup[key].Count;
                if (count > max)
                    max = count;
            }
            return max;
        }

        /// <summary>
        /// Removes ownership of the given structure from the given team and re-evaluates territory ownership
        /// based on new scores
        /// </summary>
        public void ReleaseStructure(StructureManager structure, Team team)
        {
            if (structure.OwnerTeam != team) return;
            
            structure.Reset();
            structure.SpawnGarrison(null);
            TerritoryData territory;
            // get territory that this structure belongs to
            if (!territoryLookup.TryGetValue(structure, out territory)) return;
            
            // remove structure from ownership lookup
            structureOwnerLookup.Remove(structure);

            // get teams current score for this territory
            var scores = scoreLookup[territory];
            var teamScore = scores.SingleOrDefault(ts => ts.Owner == team);
            if (teamScore != null)
            {
                var structures = structureLookup[territory];
                teamScore.Score = structures.Count(s => GetStructureOwner(s) == team);
                // if the team score approaches 0 then remove them from ownership evaluation for this territory
                if (teamScore.Score == 0)
                {
                    scores.Remove(teamScore);
                }
            }
                
            if (scores.Count > 0)
            {
                // get the max score
                var maxScore = scores.Max(sc => sc.Score);
                // get the number of teams with that score
                // if more than one team has the high score for this territory then no one owns it
                var top = scores.Count(t => t.Score == maxScore);
                if (top == 1)
                {
                    var theOwner = scores.Single(sc => sc.Score == maxScore);
                    if (!ownerLookup.ContainsKey(territory) || (ownerLookup.ContainsKey(territory) && ownerLookup[territory] != theOwner.Owner))
                    {
                        RemoveTeamTerritory(team, territory);
                        AddTerritoryToTeam(theOwner.Owner, territory);
                    }
                }
            }
            else
            {
                // if this team owned the territory uncontested but left the last building they had then territory gets released
                Team o;
                if (ownerLookup.TryGetValue(territory, out o) && o == team)
                {
                    RemoveTeamTerritory(team, territory);
                }
            }

            structure.OwnerTeam = null;
            SkirmishVictoryManager.UpdateMatchStatus();
        }

        /// <summary>
        /// Captures the given structure for the given team. This contributes to the team's territory score
        /// for whatever territory the structure belongs to - if any.
        /// </summary>
        public void CaptureStructure(StructureManager structure, Team team)
        {
            TerritoryData territory;
            // get territory that this structure belongs to
            if (!territoryLookup.TryGetValue(structure, out territory)) return;
            
            // if this structure had a previous owner - release it and decrease that owner's territory score
            Team prevOwner;
            if (structureOwnerLookup.TryGetValue(structure, out prevOwner))
            {
                // remove any bonuses this structure was providing
                StructureBonusManager.RemoveBonus(prevOwner, structure.StructureData.Name);
                
                var s = scoreLookup[territory];
                var prevScore = s.SingleOrDefault(ts => ts.Owner == prevOwner);
                if (prevScore != null)
                {
                    var structures = structureLookup[territory];
                    prevScore.Score = structures.Count(st => GetStructureOwner(st) == team);
                }
                // if this team is the local team then remove an instance of the name for unit prereq validation
                if (prevOwner.IsHuman && prevOwner == FracNet.Instance.LocalTeam)
                {
                    StructureManager.RemoveOwnedStructure(structure);
                }
            }

            // assign ownership lookup of this structure to new team
            structureOwnerLookup[structure] = team;
            
            // apply any structure bonuses to this team
            StructureBonusManager.AddBonus(team, structure.StructureData.Name);
            
            // update stats to reflect max number of structures captured
            var ownedCount = 0;
            foreach (var s in structureOwnerLookup.Keys)
            {
                if (structureOwnerLookup[s] == team)
                {
                    ownedCount++;
                }
            }
            MatchStatTracker.CheckMaxStructures(team, ownedCount);

            // get the team's current score for this territory or create it
            // if this is the first structure they are taking here
            List<TeamTerritoryScore> scores;
            TeamTerritoryScore teamScore;
            if (scoreLookup.TryGetValue(territory, out scores))
            {
                teamScore = scores.SingleOrDefault(ts => ts.Owner == team);
                // if territory has scores but this team is taking a structure for the first time
                if (teamScore == null)
                {
                    teamScore = new TeamTerritoryScore(team);
                    scores.Add(teamScore);
                }
            }
            else
            {
                // this territory has no present teams so create an entry
                teamScore = new TeamTerritoryScore(team);
                scores = new List<TeamTerritoryScore>() { teamScore };
                scoreLookup[territory] = scores;
            }

            // add to team's score for capturing building
            teamScore.Score = structureLookup[territory].Count(s => GetStructureOwner(s) == team);

            // get the current owner of the territory if there is one
            Team currentOwner;
            if (ownerLookup.TryGetValue(territory, out currentOwner))
            {
                // if given team doesn't already own this territory then check to see if
                // capturing the given building would give someone else ownership
                if (currentOwner != team)
                {
                    // get the top score
                    var topScore = scores.Max(s => s.Score);
                    
                    // get all players with a score equal to the max score
                    var maxScorePlayers = scores.Where(s => s.Score == topScore).Select(s => s.Owner).ToArray();
                    var maxCount = maxScorePlayers.Length;
                    
                    // if only 1 player has the max score and they aren't the current owner then give them the territory
                    if (maxCount == 1)
                    {
                        var newOwner = maxScorePlayers.First();
                        if (newOwner != currentOwner)
                        {
                            RemoveTeamTerritory(currentOwner, territory);
                            AddTerritoryToTeam(newOwner, territory);
                        }
                    }
                    // if multiple players have the top score then take away the territory from the owner because of a tie
                    else if (maxCount > 1)
                    {
                        RemoveTeamTerritory(currentOwner, territory);
                    }
                }
            }
            else
            {
                // no entry means no one owns this territory
                // which also means the team that just captured this building
                // now owns the territory
                ownerLookup[territory] = team;
                AddTerritoryToTeam(team, territory);
            }

            SkirmishVictoryManager.UpdateMatchStatus();
        }

        /// <summary>
        /// Removes ownership of the given territory from the given team and sets it to neutral
        /// </summary>
        public void RemoveTeamTerritory(Team team, TerritoryData territory)
        {
            ownerLookup.Remove(territory);
            ownedTerritoryLookup[team].Remove(territory);
            // if the local player is the one who lost the territory then update UI
            if (team.IsHuman && team == FracNet.Instance.LocalTeam)
            {
                CompassUI.Instance.DecreaseTerritoryCounter();
                team.RecruitManager.RemovePendingRequestsByTerritory(territory.Name);
                MultiplayerEventBroadcaster.LoseTerritory(territory.Name);
            }
            // if an AI player lost a territory and we are the host
            else if (!team.IsHuman && FracNet.Instance.IsHost)
            {
                team.RecruitManager.RemovePendingRequestsByTerritory(territory.Name);
                var terrs = GetOwnedTerritories(team);
                team.TeamPopulation = 0;
                foreach (var t in terrs)
                {
                    team.TeamPopulation += t.PopulationBonus;
                }
            }

            UpdateHelperColor(territory, Color.white);
            OnOwnerChanged.Invoke(territory, null);
        }

        public int GetTeamScoreForTerritory(TerritoryData territory, Team team)
        {
            if (territory == null) return 0;
            List<TeamTerritoryScore> scores;
            if (scoreLookup.TryGetValue(territory, out scores))
            {
                var score = scores.SingleOrDefault(s => s.Owner == team);
                return score?.Score ?? 0;
            }

            return 0;
        }

        private void AddTerritoryToTeam(Team team, TerritoryData territory)
        {
            ownerLookup[territory] = team;
            List<TerritoryData> ownedTerritories;
            if (!ownedTerritoryLookup.TryGetValue(team, out ownedTerritories))
            {
                ownedTerritories = new List<TerritoryData>();
                ownedTerritoryLookup[team] = ownedTerritories;
            }
            ownedTerritories.Add(territory);
            SkirmishVictoryManager.UneliminateTeam(team);
            
            // update stats
            MatchStatTracker.CheckMaxTerritories(team, ownedTerritories.Count);
            
            // if the local player is the capturing team then update the UI
            if (team.IsHuman && team == FracNet.Instance.LocalTeam)
            {
                CompassUI.Instance.IncreaseTerritoryCounter();
                MultiplayerEventBroadcaster.GainTerritory(territory.Name);
            }
            else if (!team.IsHuman && FracNet.Instance.IsHost)
            {
                var terrs = GetOwnedTerritories(team);
                team.TeamPopulation = 0;
                foreach (var t in terrs)
                {
                    team.TeamPopulation += t.PopulationBonus;
                }
            }
            
            // if the team isn't a local player then raise event
            if (team != FracNet.Instance.LocalTeam)
            {
                MultiplayerEventBroadcaster.GainTerritory(team.PlayerName, territory.Name);
            }

            UpdateHelperColor(territory, team.TeamColor.UnityColor);
            OnOwnerChanged.Invoke(territory, team);
        }

        private void UpdateHelperColor(TerritoryData territory, Color color)
        {
            color.a = 110f / 255f;
            GameObject helper;
            if (!ownerHelperLookup.TryGetValue(territory, out helper)) return;
            
            var renders = helper.GetComponentsInChildren<Renderer>(true);
            foreach (var t in renders)
            {
                t.material.color = color;
            }
        }
    }
}