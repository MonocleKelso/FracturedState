using System.Xml.Serialization;
using UnityEngine;

namespace FracturedState.Game.Data
{
    [XmlRoot("houseColors")]
    public class HouseColorList
    {
        [XmlElement("houseColor")]
        public HouseColor[] HouseColors { get; set; }
    }

    public class HouseColor
    {
        private const int textureWidth = 37;
        private const int textureHeight = 11;

        [XmlAttribute("name")]
        public string Name { get; set; }
        [XmlAttribute("r")]
        public int Red { get; set; }
        [XmlAttribute("g")]
        public int Green { get; set; }
        [XmlAttribute("b")]
        public int Blue { get; set; }

        [XmlIgnore]
        public Color UnityColor
        {
            get
            {
                float r = Red / 255f;
                float g = Green / 255f;
                float b = Blue / 255f;
                
                return new Color(r, g, b);
            }
        }

        private Texture2D colorTexture;
        [XmlIgnore]
        public Texture2D ColorTexture
        {
            get
            {
                if (colorTexture == null)
                {
                    colorTexture = new Texture2D(textureWidth, textureHeight);
                    Color[] colors = new Color[textureWidth * textureHeight];
                    Color c = UnityColor;
                    for (int i = 0; i < colors.Length; i++)
                    {
                        colors[i] = c;
                    }
                    colorTexture.SetPixels(colors);
                    colorTexture.Apply();
                }
                return colorTexture;
            }
        }
    }
}