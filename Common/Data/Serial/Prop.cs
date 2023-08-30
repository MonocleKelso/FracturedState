using System.Xml.Serialization;

namespace FracturedState.Game.Data
{
    /// <summary>
    /// Serialization class for props placed on a map that can conditionally provide cover to units.
    /// </summary>
    [XmlRoot("prop")]
    public class Prop : BaseObject
    {
        [XmlElement("category")]
        public string Category { get; set; }
        
        [XmlArray("coverPoints")]
        [XmlArrayItem("coverPoint")]
        public CoverPoint[] CoverPoints { get; set; }

        /// <summary>
        /// True if this prop can provide cover for enemy and friendly units at the same time
        /// otherwise false.  This should be true for larger props where opposite sides are far enough
        /// away so that simultaneous ocupation makes sense
        /// </summary>
        [XmlElement("simultCover")]
        public bool SimultCover { get; set; }

        [XmlIgnore]
        public bool ProvidesCover { get { return CoverPoints != null && CoverPoints.Length > 0; } }
    }
}