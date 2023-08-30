using System.Collections.Generic;
using FracturedState.Game.Data;
using System.Linq;
using FracturedState.Game.Management;

namespace FracturedState.Game
{
    public class ReinforcementManager
    {
        public List<SquadRequest> PendingRequests { get; }
        public int RequestPopCost => currentRequest?.SpecialCost ?? 0;
        public float RequestRecruitCost => currentRequest?.TimeCost ?? 0;

        private readonly List<UnitObject> AiUnitList;
        private SquadRequest currentRequest;
        public SquadRequest CurrentRequest => currentRequest;
        private readonly Team owner;

        public ReinforcementManager()
        {
            PendingRequests = new List<SquadRequest>();
        }

        public ReinforcementManager(Team owner) : this()
        {
            this.owner = owner;
            AiUnitList = new List<UnitObject>();
            if (!owner.IsHuman)
            {
                var faction = XmlCacheManager.Factions.Values[owner.FactionIndex];
                for (var i = 0; i < faction.TrainableUnits.Length; i++)
                {
                    if (faction.TrainableUnits[i] != faction.SquadUnit)
                    {
                        AiUnitList.Add(XmlCacheManager.Units[faction.TrainableUnits[i]]);
                    }
                }
            }
        }

        public void CreateRequest()
        {
            currentRequest = owner != null ? new SquadRequest(owner) : new SquadRequest();
        }

        public void GenerateMixedRequest()
        {
            CreateRequest();
            // pick a random unit and add a random amount of that unit
            var availUnits = AiUnitList.Where(
                s => s.PrerequisiteStructures == null || s.PrerequisiteStructures.Length == 0 || StructureManager.HasStructure(s.PrerequisiteStructures[0])).ToArray();

            if (availUnits.Length > 0)
            {
                var unit = availUnits[UnityEngine.Random.Range(0, availUnits.Length)];
                var maxCount = UnityEngine.Mathf.FloorToInt(ConfigSettings.Instance.Values.SquadPopulationCap / (float)unit.PopulationCost);
                var randCount = UnityEngine.Random.Range(0, maxCount);
                for (var i = 0; i < randCount; i++)
                {
                    AddUnit(unit);
                }
            }
        }

        public void RemoveRequest(SquadRequest request)
        {
            PendingRequests.Remove(request);
        }

        private bool UnitFits(UnitObject unit)
        {
            return currentRequest.SpecialCost + unit.PopulationCost <= ConfigSettings.Instance.Values.SquadPopulationCap;
        }

        public void AddUnit(UnitObject unit)
        {
            if (UnitFits(unit))
            {
                currentRequest.AddUnit(unit);
            }
        }

        public void RemoveUnit(UnitObject unit)
        {
            currentRequest.RemoveUnit(unit);
        }

        public void SetTerritory(string territory)
        {
            currentRequest.Territory = territory;
        }

        public SquadRequest[] GetRequestsByTerritory(string territory)
        {
            return PendingRequests.Where(req => req.Territory == territory).ToArray();
        }


        public UnitObject GetUnitAtIndex(int index)
        {
            if (index < currentRequest.Units.Count)
            {
                return currentRequest.Units[index];
            }
            return null;
        }

        public void QueueRequest()
        {
            PendingRequests.Add(currentRequest);
            CreateRequest();
        }

        public void UpdateRequestTimes(float elapsedTime)
        {
            var territories = new HashSet<string>();
            var complete = new List<SquadRequest>();
            for (var i = 0; i < PendingRequests.Count; i++)
            {
                if (!territories.Contains(PendingRequests[i].Territory))
                {
                    territories.Add(PendingRequests[i].Territory);
                    PendingRequests[i].RemoveElapsedTime(elapsedTime);
                    if (PendingRequests[i].TimeCost <= 0)
                    {
                        complete.Add(PendingRequests[i]);
                    }
                }
            }
            for (var i = 0; i < complete.Count; i++)
            {
                Network.FracNet.Instance.CallReinforcements(complete[i]);
                if (owner == null || owner.IsHuman)
                {
                    UnitBarkManager.Instance.NewSquadBark(complete[i].RequestFaction);
                }
                PendingRequests.Remove(complete[i]);
            }
        }

        public void RemovePendingRequestsByTerritory(string territory)
        {
            for (var i = PendingRequests.Count - 1; i >= 0; i--)
            {
                if (PendingRequests[i].Territory == territory)
                {
                    PendingRequests.RemoveAt(i);
                }
            }
        }

        public void ClearAllPendingRequests()
        {
            PendingRequests.Clear();
        }
    }
}