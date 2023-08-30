using System.Xml.Serialization;

namespace FracturedState.Game.Data
{
    [XmlRoot("avatars")]
    public class AvatarList
    {
        [XmlElement("avatar")]
        public Avatar[] Avatars { get; private set; }
    }

    public class Avatar
    {
        [XmlAttribute("x")]
        public int X { get; set; }
        [XmlAttribute("y")]
        public int Y { get; set; }
        [XmlAttribute("w")]
        public int Width { get; set; }
        [XmlAttribute("h")]
        public int Height { get; set; }
    }
}