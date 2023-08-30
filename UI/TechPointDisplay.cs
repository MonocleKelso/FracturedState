using FracturedState.Game.Management;
using UnityEngine;
using UnityEngine.UI;

namespace FracturedState.UI
{
    public class TechPointDisplay : MonoBehaviour
    {
        [SerializeField] private string prefix;
        [SerializeField] private Text text;

        private int currentPoints;

        private void Start()
        {
            currentPoints = Team.TotalMutatorCost;
            text.text = $"{prefix}{currentPoints}";
        }

        public void UpdateCost(int diff)
        {
            currentPoints -= diff;
            text.text = $"{prefix}{currentPoints}";
        }
    }
}