using System.Xml.Serialization;

namespace FracturedState.Game.Data
{
	[XmlRoot("factions")]
	public class FactionContainer
	{
		[XmlElement("faction")]
		public Faction[] Factions { get; set; }
	}

	public class Faction
	{
		[XmlElement("name")]
		public string Name { get; set; }
		
		[XmlArray("startingUnits")]
		[XmlArrayItem("unit")]
		public StartingUnit[] StartingUnits { get; set; }

        [XmlArray("trainableUnits")]
        [XmlArrayItem("unit")]
        public string[] TrainableUnits { get; set; }

        [XmlElement("squadUnit")]
        public string SquadUnit { get; set; }

        [XmlElement("newSquadSound")]
        public string NewSquadSound { get; set; }

        [XmlElement("victoryScreen")]
        public string VictoryScreen { get; set; }

        [XmlElement("defeatScreen")]
        public string DefeatScreen { get; set; }
	}
	
	public class StartingUnit
	{
		[XmlAttribute("name")]
		public string Name { get; set; }
		
		[XmlAttribute("count")]
		public int Count { get; set; }
	}
}