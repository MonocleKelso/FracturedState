using FracturedState.Game;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FracturedState.UI
{
    [RequireComponent(typeof(Dropdown))]
    public class FactionSelect : MonoBehaviour
    {
        public Dropdown DropDown { get; private set; }

        public void Populate(bool isHumanPlayer)
        {
            DropDown = GetComponent<Dropdown>();
            DropDown.ClearOptions();
            var factionNames = new List<string>();
            foreach (var faction in XmlCacheManager.Factions.Values)
            {
                factionNames.Add(LocalizedString.GetString(faction.Name));
            }
            if (isHumanPlayer)
            {
                factionNames.Add(LocalizedString.GetString("faction.spectator"));
            }
            DropDown.AddOptions(factionNames);
        }
    }
}