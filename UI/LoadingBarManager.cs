using FracturedState.Game.Management;
using System.Collections.Generic;
using UnityEngine;

namespace FracturedState.UI
{
    public class LoadingBarManager : MonoBehaviour
    {
        private static Dictionary<Team, LoadingBar> barLookup;

        [SerializeField] private LoadingBar loadingBar;

        private void Awake()
        {
            barLookup = new Dictionary<Team, LoadingBar>();
            for (int i = 0; i < SkirmishVictoryManager.SkirmishTeams.Count; i++)
            {
                var team = SkirmishVictoryManager.SkirmishTeams[i];
                if (team.IsHuman)
                {
                    var bar = Instantiate(loadingBar, transform);
                    bar.SetTeam(team);
                    barLookup[team] = bar;
                }
            }
        }

        public static LoadingBar GetBar(Team team)
        {
            if (barLookup == null) return null;
            if (barLookup.ContainsKey(team))
            {
                return barLookup[team];
            }
            return null;
        }

        private void OnDestroy()
        {
            if (barLookup == null) return;
            barLookup.Clear();
            barLookup = null;
        }
    }
}