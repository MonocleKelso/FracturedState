using FracturedState.Game;
using FracturedState.Game.Management;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FracturedState.UI
{
    [RequireComponent(typeof(Dropdown))]
    public class TeamColorSelect : MonoBehaviour
    {
        public Dropdown DropDown { get; private set; }

        private static List<bool> availableColorIndex;

        public static void Reset()
        {
            if (availableColorIndex == null) return;
            for (int i = 0; i < availableColorIndex.Count; i++)
            {
                availableColorIndex[i] = true;
            }
        }

        public static int GetNextAvailableColor()
        {
            for (int i = 0; i < availableColorIndex.Count; i++)
            {
                if (availableColorIndex[i])
                    return i;
            }
            return -1;
        }

        public static bool IsColorAvailable(int index)
        {
            if (index >= 0 && index < availableColorIndex.Count)
            {
                return availableColorIndex[index];
            }
            return false;
        }

        public static void SwapTeamColor(int current, int next)
        {
            if (current >= 0 && current < availableColorIndex.Count)
                availableColorIndex[current] = true;
            if (next >= 0 && next < availableColorIndex.Count)
                availableColorIndex[next] = false;
        }

        public void SetTeam(Team team)
        {
            DropDown.value = team.HouseColorIndex;
        }

        private void Awake()
        {
            DropDown = GetComponent<Dropdown>();
            if (availableColorIndex == null)
            {
                availableColorIndex = new List<bool>();
                foreach (var col in XmlCacheManager.HouseColors.Values)
                {
                    availableColorIndex.Add(true);
                }
            }
        }
    }
}