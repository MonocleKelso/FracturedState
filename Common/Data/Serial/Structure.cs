using System.Xml.Serialization;

namespace FracturedState.Game.Data
{
	[XmlRoot("structure")]
	public class Structure : BaseObject
	{
        [XmlElement("displayName")]
        public string DisplayName { get; set; }

	    [XmlElement("capturePoints")]
        public float CapturePoints { get; set; }

	    public bool CanBeCaptured => CapturePoints > 0;

	    [XmlArray("flavorTexts")]
        [XmlArrayItem("flavorText")]
        public FactionFlavorText[] FlavorTexts { get; set; }

        [XmlArray("entrances")]
        [XmlArrayItem("entrance")]
        public WaypointLink[] Entrances { get; set; }

        [XmlArray("exits")]
        [XmlArrayItem("exit")]
        public WaypointLink[] Exits { get; set; }

        [XmlElement("floorOffset")]
        public float FloorOffset { get; set; }

        [XmlElement("bib")]
        public string Bib { get; set; }

        public bool IsEnterable => (Entrances != null && Entrances.Length > 0);

	    [XmlArray("firePoints")]
        [XmlArrayItem("firePoint")]
        public FirePointLink[] FirePoints { get; set; }

        [XmlElement("unlocks")]
        public Unlock Unlockables { get; set; }

        [XmlArray("garrisonInfo")]
        [XmlArrayItem("garrison")]
        public GarrisonInfo[] Garrisons { get; set; }
	}

    public class FirePointLink
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("particleBone")]
        public string ParticleBone { get; set; }
    }

    public class WaypointLink
    {
        [XmlAttribute("name")]
        public string Point { get; set; }

        [XmlAttribute("goesTo")]
        public string GoesTo { get; set; }
    }

    public class FactionFlavorText
    {
        [XmlAttribute("faction")]
        public string Faction { get; set; }

        [XmlElement("text")]
        public string Text { get; set; }
    }

    public class Unlock
    {
        [XmlElement("unit")]
        public string[] Units { get; set; }
    }

    public class GarrisonInfo
    {
        [XmlAttribute("faction")]
        public string Faction { get; set; }

        [XmlArray("pointInfo")]
        [XmlArrayItem("point")]
        public GarrisonPointInfo[] Points { get; set; }
    }

    public class GarrisonPointInfo
    {
        [XmlAttribute("point")]
        public string Point { get; set; }

        [XmlAttribute("unit")]
        public string Unit { get; set; }

        [XmlAttribute("respawn")]
        public float RespawnTime { get; set; }
    }
}