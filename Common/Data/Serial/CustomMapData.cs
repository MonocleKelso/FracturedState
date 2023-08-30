using System.Xml.Serialization;

namespace FracturedState.Game.Data
{
    [System.Serializable]
	public class CustomMapData
	{
		[XmlElement("name")]
		public string BaseObjectName { get; set; }

        [XmlElement("position")]
        public Vec3String PositionString { get; set; }
		
		[XmlElement("rotation")]
		public Vec3String RotationString { get; set; }

        [XmlElement("uid")]
        public int UID { get; set; }

        [XmlElement("layer")]
        public int Layer { get; set; }

        [XmlElement("territory")]
        public int TerritoryID { get; set; }

        public override int GetHashCode()
        {
            int hashCode = 13;
            hashCode = (hashCode * 397) ^ BaseObjectName.GetHashCode();
            hashCode = (hashCode * 397) ^ PositionString.GetHashCode();
            hashCode = (hashCode * 397) ^ RotationString.GetHashCode();
            hashCode = (hashCode * 397) ^ UID;
            hashCode = (hashCode * 397) ^ Layer;
            hashCode = (hashCode * 397) ^ TerritoryID;
            return hashCode;
        }
    }
}