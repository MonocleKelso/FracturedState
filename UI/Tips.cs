using FracturedState.Game;
using FracturedState.Game.Data;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.UI;

namespace FracturedState.UI
{
    [RequireComponent(typeof(Text))]
    public class Tips : MonoBehaviour
    {
        [XmlRoot("tips")]
        public class TipData
        {
            [XmlElement("tip")]
            public string[] TipEntries { get; set; }
        }

        private void Awake()
        {
            TipData tips = DataUtil.DeserializeXml<TipData>(DataLocationConstants.GameRootPath + DataLocationConstants.TipDataFile);
            GetComponent<Text>().text = LocalizedString.GetString(tips.TipEntries[Random.Range(0, tips.TipEntries.Length)]);
        }
    }
}