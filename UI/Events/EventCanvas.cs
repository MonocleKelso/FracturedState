using System.Collections;
using FracturedState.Game.Data;
using FracturedState.Game.Management;
using UnityEngine;
using UnityEngine.UI;

namespace FracturedState.UI.Events
{
    public class EventCanvas : MonoBehaviour
    {
        [SerializeField] private float holdTime;
        [SerializeField] private float slideTime;
        [SerializeField] private RectTransform rightParent;
        
        [SerializeField] private TerritoryBolsterWidget territoryBolster;
        [SerializeField] private UnitUnlockWidget unitUnlock;
        [SerializeField] private TerritoryCaptureWidget territoryCapture;
        [SerializeField] private LoseStructureWidget loseStructure;
        [SerializeField] private UnitLockWidget unitLock;
        [SerializeField] private TerritoryLostWidget territoryLost;
        [SerializeField] private PlayerCountdownWidget playerCountdown;
        [SerializeField] private PlayerDefeatedWidget playerDefeated;
        [SerializeField] private GameObject textEvent;
        
        private static EventCanvas instance;

        private void Awake()
        {
            instance = this;
        }

        public static void TerritoryBolster(string territoryName)
        {
            var e = MakeWidget(instance.territoryBolster);
            e.SetTerritoryName(territoryName);
        }

        public static void UnlockUnit(UnitObject unit)
        {
            var e = MakeWidget(instance.unitUnlock);
            e.SetUnitData(unit);
        }

        public static void CaptureTerritory(string territoryName)
        {
            var e = MakeWidget(instance.territoryCapture);
            e.SetTerritoryName(territoryName);
        }

        public static void CaptureTerritory(string playerName, string territoryName)
        {
            var e = MakeWidget(instance.territoryCapture);
            e.SetTerritoryName(territoryName);
            e.SetPlayerName(playerName);
        }

        public static void LoseStructure(string structureName)
        {
            var e = MakeWidget(instance.loseStructure);
            e.SetStructureName(structureName);
        }

        public static void LockUnit(UnitObject unit)
        {
            var e = MakeWidget(instance.unitLock);
            e.SetUnitData(unit);
        }

        public static void TerritoryLost(string territoryName)
        {
            var e = MakeWidget(instance.territoryLost);
            e.SetTerritoryName(territoryName);
        }

        public static void CountdownPlayer(Team team)
        {
            var e = Instantiate(instance.playerCountdown, instance.rightParent);
            e.SetTeam(team);
        }

        public static void PlayerDefeated(string playerName)
        {
            var e = MakeWidget(instance.playerDefeated);
            e.SetPlayerName(playerName);
        }

        public static void TextEvent(string text)
        {
            var widget = Instantiate(instance.textEvent, instance.transform);
            instance.StartCoroutine(instance.WidgetLife(widget.transform as RectTransform));
            widget.GetComponentInChildren<Text>().text = text;
        }
        
        private static T MakeWidget<T>(T prefab) where T : MonoBehaviour
        {
            var widget = Instantiate(prefab, instance.transform);
            instance.StartCoroutine(instance.WidgetLife(widget.transform as RectTransform));
            return widget;
        }

        private IEnumerator WidgetLife(RectTransform rectTransform)
        {
            yield return new WaitForSeconds(holdTime);
            var width = rectTransform.sizeDelta.x * -1;
            var t = 0f;
            while (t < slideTime)
            {
                var pos = rectTransform.position;
                t += Time.deltaTime;
                pos.x = Mathf.Lerp(0, width, t / slideTime);
                rectTransform.position = pos;
                yield return null;
            }
            
            Destroy(rectTransform.gameObject);
        }
    }
}