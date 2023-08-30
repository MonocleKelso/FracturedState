using FracturedState.Game.Management;
using UnityEngine;
using UnityEngine.UI;

namespace FracturedState.UI
{
    public class LoadingBar : MonoBehaviour
    {
        [SerializeField] private Text playerName;

        [SerializeField] private UnityEngine.UI.Slider progress;

        [SerializeField] private UnityEngine.UI.Image barImage;

        public Team PlayerTeam { get; private set; }

        public float Progress { get { return progress.value; } }

        public void SetTeam(Team team)
        {
            PlayerTeam = team;
            playerName.text = team.PlayerName;
            if (team.IsSpectator)
            {
                barImage.color = Color.white;
            }
            else
            {
                barImage.color = team.TeamColor.UnityColor;
            }
            SetProgress(0);
        }

        public void SetProgress(float prog)
        {
            progress.value = prog;
        }
    }
}