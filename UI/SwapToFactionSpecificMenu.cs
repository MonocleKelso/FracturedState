using System;
using System.Collections.Generic;
using System.Linq;
using FracturedState.Game.Network;
using UnityEngine;

namespace FracturedState.UI
{
    [Serializable]
    public class FactionMenuBinding
    {
        [SerializeField] public string Faction;

        [SerializeField] public GameObject Menu;
    }
    
    public class SwapToFactionSpecificMenu : SwapCurrentMenuButton
    {
        [SerializeField] private List<FactionMenuBinding> menus;

        /// <summary>
        /// Swaps out the current menu for one matching the currently selected faction of the player
        /// </summary>
        public void FactionSpecificSwap()
        {
            var menu = menus.Single(m => m.Faction == FracNet.Instance.LocalTeam.Faction);
            Swap(menu.Menu);
        }
    }
}