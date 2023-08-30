using FracturedState.Game;
using UnityEngine;

namespace FracturedState.UI
{
    public class SquadStanceSwap : MonoBehaviour
    {
        [SerializeField] private SquadStance stance;

        public void ButtonClick()
        {
            InterfaceSoundPlayer.PlayButtonClick();
            foreach (var unit in SelectionManager.Instance.SelectedUnits)
            {
                if (unit != null && unit.IsAlive)
                {
                    unit.NetMsg.CmdChangeStance((int) stance);
                }
            }
        }
    }
}