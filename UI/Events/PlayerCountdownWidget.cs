using FracturedState.Game.Management;
using UnityEngine;
using UnityEngine.UI;

namespace FracturedState.UI.Events
{
    public class PlayerCountdownWidget : MonoBehaviour
    {
        [SerializeField] private Text playerName;
        [SerializeField] private Text timer;

        private Team team;
        private float time;

        public void SetTeam(Team team)
        {
            this.team = team;
            playerName.text = team.PlayerName;
        }
        
        private void Awake()
        {
            time = 60;
        }

        private void Update()
        {
            if (team.SurrenderTime > 0 || !SkirmishVictoryManager.IsTeamEliminated(team))
            {
                Destroy(gameObject);
                return;
            }

            time -= Time.deltaTime;
            timer.text = time.ToString("00.0");
            if (time <= 0)
            {
                Destroy(gameObject);
            }
        }
    }
}