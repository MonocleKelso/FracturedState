using System.Xml.Serialization;
using FracturedState;

namespace FracturedState.Game.Data
{
    public class MapDecal
    {
        [XmlElement("name")]
        public string Name { get; set; }
        
        [XmlElement("position")]
        public Vec3String Position { get; set; }
        
        [XmlElement("rotation")]
        public Vec3String Rotation { get; set; }
        
        [XmlElement("scale")]
        public Vec3String Scale { get; set; }
        
        [XmlElement("alpha")]
        public float Alpha { get; set; }
        
        [XmlElement("structure")]
        public int StructureId { get; set; }
        
        [XmlElement("limitExterior")]
        public bool LimitExterior { get; set; }
        
        [XmlElement("terrainLimited")]
        public bool TerrainLimited { get; set; }
    }
}