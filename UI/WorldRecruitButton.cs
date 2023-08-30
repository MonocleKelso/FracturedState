using FracturedState.Game.Data;
using FracturedState.Game.Management;
using FracturedState.Game.Network;
using UnityEngine;
using UnityEngine.UI;

namespace FracturedState.UI
{
    public class WorldRecruitButton : MonoBehaviour
    {
        [SerializeField] private UnityEngine.UI.Image _button;
        [SerializeField] private UnityEngine.UI.Image _img;

        private Button _click;
        private TerritoryData _territoryData;

        private void Awake()
        {
            _click = _button.GetComponent<Button>();
        }
        
        public void SetTerritory(TerritoryData territoryData)
        {
            _territoryData = territoryData;
            var look = Vector3.zero;
            _territoryData.RallyPoint.TryVector3(out look);
            transform.LookAt(look);
            _click.interactable = false;
            TerritoryManager.Instance.OnOwnerChanged.AddListener(OwnerChanged);
        }

        public void OpenPanel()
        {
            CompassUI.Instance.SetTerritory(_territoryData);
            if (!CompassUI.Instance.RecruitPanel)
                CompassUI.Instance.ToggleRecruitPanel();
        }

        private void OwnerChanged(TerritoryData territory, Team owner)
        {
            if (territory != _territoryData) return;
            var color = owner?.TeamColor.UnityColor ?? Color.white;
            _button.color = color;
            _img.color = color;
            _click.interactable = owner == FracNet.Instance.LocalTeam;
        }
    }
}