using System;
using System.Xml.Serialization;

namespace FracturedState.Game.Data
{
	[XmlRoot("map")]
    [Serializable]
	public class RawMapData
	{
		[XmlElement("name")]
		public string MapName { get; set; }

        [XmlElement("xUpper")]
        public int XUpperBound { get; set; }
        [XmlElement("xLower")]
        public int XLowerBound { get; set; }
        [XmlElement("zUpper")]
        public int ZUpperBound { get; set; }
        [XmlElement("zLower")]
        public int ZLowerBound { get; set; }

        [XmlArray("territories")]
        [XmlArrayItem("territory")]
        public TerritoryData[] Territories { get; set; }

        [XmlArray("startingPoints")]
        [XmlArrayItem("startingPoint")]
        public Vec3String[] StartingPoints { get; set; }

        [XmlArray("terrains")]
        [XmlArrayItem("terrain")]
        public CustomMapData[] Terrains { get; set; }

        [XmlArray("structures")]
        [XmlArrayItem("structure")]
        public CustomMapData[] Structures { get; set; }

        [XmlArray("props")]
        [XmlArrayItem("prop")]
        public CustomMapData[] Props { get; set; }
	    
	    [XmlArray("decals")]
	    [XmlArrayItem("decal")]
	    public MapDecal[] Decals { get; set; }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 13;
                hashCode = (hashCode * 397) ^ MapName.GetHashCode();
                hashCode = (hashCode * 397) ^ XUpperBound;
                hashCode = (hashCode * 397) ^ XLowerBound;
                hashCode = (hashCode * 397) ^ ZUpperBound;
                hashCode = (hashCode * 397) ^ ZLowerBound;

                if (Territories != null)
                {
                    for (int i = 0; i < Territories.Length; i++)
                    {
                        hashCode = (hashCode * 397) ^ Territories[i].GetHashCode();
                    }
                }

                if (StartingPoints != null)
                {
                    for (int i = 0; i < StartingPoints.Length; i++)
                    {
                        hashCode = (hashCode * 397) ^ StartingPoints[i].GetHashCode();
                    }
                }

                if (Terrains != null)
                {
                    for (int i = 0; i < Terrains.Length; i++)
                    {
                        if (Terrains[i] != null)
                            hashCode = (hashCode * 397) ^ Terrains[i].GetHashCode();
                    }
                }

                if (Structures != null)
                {
                    for (int i = 0; i < Structures.Length; i++)
                    {
                        if (Structures[i] != null)
                            hashCode = (hashCode * 397) ^ Structures[i].GetHashCode();
                    }
                }

                if (Props != null)
                {
                    for (int i = 0; i < Props.Length; i++)
                    {
                        if (Props[i] != null)
                            hashCode = (hashCode * 397) ^ Props[i].GetHashCode();
                    }
                }

                if (Decals != null)
                {
                    for (int i = 0; i < Decals.Length; i++)
                    {
                        if (Decals[i] != null)
                            hashCode = (hashCode * 397) ^ Decals[i].GetHashCode();
                    }
                }

                return hashCode;
            }
        }
    }
}