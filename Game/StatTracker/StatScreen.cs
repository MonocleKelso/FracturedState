using FracturedState.Game.Management;
using UnityEngine;
using UnityEngine.UI;

namespace FracturedState.Game.StatTracker
{
    public class StatScreen : MonoBehaviour
    {
        [SerializeField] private RectTransform gridParent;
        [SerializeField] private GameObject header;
        [SerializeField] private StatRow row;

        private void Awake()
        {
            var h = Instantiate(header, gridParent);
            h.transform.Find("MatchTime").GetComponentInChildren<Text>().text = $"Match Time {SkirmishVictoryManager.GameTimeSnapshot}";
            
            var stats = MatchStatTracker.GetStats();
            foreach (var stat in stats)
            {
                var r = Instantiate(row, gridParent);
                r.Init(stat);
            }
            MatchStatTracker.Reset();
        }
    }
}