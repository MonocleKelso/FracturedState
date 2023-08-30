using FracturedState.Game.Data;
using FracturedState.Game.Management;
using FracturedState.Game.Network;
using System.Collections.Generic;

namespace FracturedState.Game
{
    public class SquadRequest
    {
        public List<UnitObject> Units { get; }
        private readonly Dictionary<UnitObject, int> loadout;
        private int popCost;
        public int SpecialCost { get; private set; }
        public float TimeCost { get; private set; }
        public string Territory { get; set; }

        public UnityEngine.Vector3 RallyPoint { get; set; } // for squads not rallying to a specific territory

        public Team Owner { get; }

        private readonly UnitObject fillerUnit;

        public Faction RequestFaction => XmlCacheManager.Factions[Owner.Faction];

        public SquadRequest()
        {
            Units = new List<UnitObject>();
            loadout = new Dictionary<UnitObject, int>();
            Owner = FracNet.Instance.NetworkActions.LocalTeam;
            fillerUnit = XmlCacheManager.Units[XmlCacheManager.Factions[Owner.Faction].SquadUnit];
            loadout[fillerUnit] = 0;
            FillUnits();
        }

        public SquadRequest(Team team)
        {
            Units = new List<UnitObject>();
            loadout = new Dictionary<UnitObject, int>();
            Owner = team;
            fillerUnit = XmlCacheManager.Units[XmlCacheManager.Factions[Owner.Faction].SquadUnit];
            loadout[fillerUnit] = 0;
            FillUnits();
        }

        public SquadRequest(Team team, List<UnitObject> units)
        {
            Units = units;
            Owner = team;
        }

        public void AddUnit(UnitObject unit)
        {
            Units.Add(unit);
            while (Units[0].Name == fillerUnit.Name)
            {
                RemoveUnitNoFill(Units[0]);
            }
            popCost += unit.PopulationCost;
            SpecialCost += unit.PopulationCost;
            TimeCost += unit.RecruitTime;
            if (loadout.ContainsKey(unit))
            {
                loadout[unit]++;
            }
            else
            {
                loadout[unit] = 1;
            }
            if (unit != fillerUnit)
            {
                FillUnits();
            }
        }

        private void RemoveUnitNoFill(UnitObject unit)
        {
            Units.Remove(unit);
            popCost -= unit.PopulationCost;
            TimeCost -= unit.RecruitTime;
            loadout[unit]--;
        }

        public void RemoveUnit(UnitObject unit)
        {
            Units.Remove(unit);
            popCost -= unit.PopulationCost;
            TimeCost -= unit.RecruitTime;
            if (unit.Name != fillerUnit.Name)
            {
                SpecialCost -= unit.PopulationCost;
            }
            loadout[unit]--;
            FillUnits();
        }

        private void FillUnits()
        {
            while (fillerUnit.PopulationCost + popCost <= ConfigSettings.Instance.Values.SquadPopulationCap)
            {
                Units.Insert(0, fillerUnit);
                popCost += fillerUnit.PopulationCost;
                TimeCost += fillerUnit.RecruitTime;
                loadout[fillerUnit]++;
            }
        }

        public void RemoveElapsedTime(float time)
        {
            TimeCost -= time;
        }
    }
}