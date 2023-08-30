using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

namespace FracturedState.Game.Data
{
    public enum AbilityType { PerSquad = 0, PerUnit = 1, PassivePerUnit = 2, PassivePerSquad = 3 }
    public enum TargetType { None, Enemy, Friendly, Ground, Structure }
    public enum Constraint {  None, Exterior, Interior }

    [XmlRoot("abilities")]
    public class AbilityList
    {
        [XmlElement("ability")]
        public Ability[] Abilities { get; set; }
    }

    public class Ability : IComparable<Ability>
    {
        private const string folderPath = "icons/abilities/";

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlElement("displayName")]
        public string DisplayName { get; set; }

        [XmlElement("description")]
        public string Description { get; set; }

        [XmlElement("type")]
        public AbilityType Type { get; set; }

        [XmlElement("targetType")]
        public TargetType Targetting { get; set; }

        [XmlElement("constraint")]
        public Constraint ConstrainType { get; set; }

        [XmlElement("iconAtlas")]
        public string IconAtlas { get; set; }

        [XmlElement("icon")]
        public string IconPath { get; set; }

        [XmlElement("indicator")]
        public string IndicatorPath { get; set; }

        [XmlElement("script")]
        public string Script { get; set; }

        [XmlElement("cooldown")]
        public float CooldownTime { get; set; }

        [XmlElement("priority")]
        public int Priority { get; set; }

        [XmlArray("barks")]
        [XmlArrayItem("bark")]
        public string[] Barks { get; set; }
        
        [XmlElement("hotkey")]
        public string Hotkey { get; set; }
        
        [XmlElement("noStateVal")]
        public bool SkipStateValidation { get; set; }

        private static readonly Dictionary<string, Sprite> iconLookup = new Dictionary<string, Sprite>();

        public static Sprite GetIcon(string atlas, string icon)
        {
            Sprite sprite;
            if (iconLookup.TryGetValue(icon, out sprite))
            {
                return sprite;
            }
            var icons = DataUtil.LoadBuiltInUiSpritesFromAtlas(folderPath + atlas);
            foreach (var ic in icons)
            {
                if (ic.name != icon) continue;
                iconLookup[icon] = ic;
                return ic;
            }
            return null;
        }

        public Ability() { }

        public Ability(Ability copy)
        {
            Name = copy.Name;
            DisplayName = copy.DisplayName;
            Type = copy.Type;
            Targetting = copy.Targetting;
            IconAtlas = copy.IconAtlas;
            IconPath = copy.IconPath;
            IndicatorPath = copy.IndicatorPath;
            Script = copy.Script;
            CooldownTime = copy.CooldownTime;
            ConstrainType = copy.ConstrainType;
            Priority = copy.Priority;
            Barks = copy.Barks;
            Hotkey = copy.Hotkey;
            SkipStateValidation = copy.SkipStateValidation;
        }

        public int CompareTo(Ability other)
        {
            var c = Type.CompareTo(other.Type);
            return c == 0 ? Name.CompareTo(other.Name) : c;
        }
    }
}