using FracturedState.Game;
using FracturedState.Game.Data;
using FracturedState.Game.Management;
using FracturedState.Game.Network;
using UnityEngine;
using UnityEngine.Events;

namespace FracturedState.UI
{
    public class MapSelect : MonoBehaviour
    {
        [SerializeField] private MapChoice mapChoice;

        private static string[] mapNames;
        private static int[] mapHashes;
        private static int[] playerCounts;

        public static string CurrentMapName { get; private set; }
        public static int CurrentMapPop { get; private set; }
        public static int CurrentMapHash { get; private set; }
        public static bool NeedsMapTransfer { get; private set; }

        public static readonly UnityEvent<string, int> OnMapChanged = new MapChangedEvent();

        private void Start()
        {
            Init();
            for (var i = 0; i < mapNames.Length; i++)
            {
                var choice = Instantiate(mapChoice, transform);
                choice.SetMapName(mapNames[i]);
                choice.SetPlayerCount(playerCounts[i].ToString());
                choice.GetComponent<UnityEngine.UI.Button>().interactable = FracNet.Instance.IsHost;
            }
        }

        public static void Init()
        {
            if (mapNames != null) return;
            mapNames = DataUtil.GetMapFileListForDisplay();
            DataUtil.GetMapFileDetails(out playerCounts, out mapHashes);
            if (FracNet.Instance.IsHost && string.IsNullOrEmpty(CurrentMapName))
            {
                SetCurrentMap(mapNames[0], mapHashes[0]);
            }
        }

        public static void UpdateMapChoice(int index)
        {
            if (!FracNet.Instance.IsHost) return;
            FracNet.Instance.NetworkActions.CmdChangeMap(mapNames[index], mapHashes[index]);
            SkirmishVictoryManager.CurrentMap =  DataUtil.DeserializeXml<RawMapData>(DataLocationConstants.GameRootPath +
                                                                                     DataLocationConstants.MapDirectory + "/" + mapNames[index] + "/" + "map.xml");
        }

        public static bool SetCurrentMap(string map, int mapHash)
        {
            var hasMap = false;
            for (var i = 0; i < mapNames.Length; i++)
            {
                if (mapNames[i] == map && mapHashes[i] == mapHash)
                {
                    CurrentMapName = map;
                    CurrentMapPop = playerCounts[i];
                    CurrentMapHash = mapHash;
                    hasMap = true;
                    OnMapChanged?.Invoke(CurrentMapName, CurrentMapPop);
                    break;
                }
            }
            NeedsMapTransfer = !hasMap;
            return hasMap;
        }
    }
}