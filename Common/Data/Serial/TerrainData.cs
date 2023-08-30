using System.Xml.Serialization;
using UnityEngine;

namespace FracturedState.Game.Data
{
    [XmlRoot("terrain")]
    [System.Serializable]
    public class TerrainList
    {
        [XmlElement("obj")]
        public TerrainEntry[] Entries { get; set; }
    }

    [System.Serializable]
    public class TerrainEntry
    {
        [XmlAttribute("id")]
        public int Id { get; set; }
        
        [XmlAttribute("model")]
        public string ModelName { get; set; }

        [XmlAttribute("displayName")]
        public string DisplayName { get; set; }
        
        [XmlAttribute("category")]
        public string EditorCategory { get; set; }
        
        [XmlAttribute("hide")]
        public bool Hidden { get; set; }
    }

    [System.Serializable]
    public class TerritoryData
    {
        [XmlElement("name")]
        public string Name { get; set; }

        [XmlElement("color")]
        public Vec3String DisplayColor { get; set; }

        [XmlElement("pop")]
        public int PopulationBonus { get; set; }

        [XmlElement("recruit")]
        public bool Recruit { get; set; }

        [XmlElement("rallyPoint")]
        public Vec3String RallyPoint { get; set; }

        [System.NonSerialized]
        private Color unityColor;

        [XmlIgnore()]
        public Color UnityColor
        {
            get
            {
                if (unityColor == default(Color))
                {
                    Vector3 c;
                    if (DisplayColor.TryVector3(out c))
                    {
                        unityColor = new Color(c.x, c.y, c.z);
                    }
                    else
                    {
                        unityColor = Color.black;
                    }
                }
                return unityColor;
            }
            set { unityColor = value; }
        }

        [System.NonSerialized]
        private Texture2D editorTexture;
        [XmlIgnore()]
        public Texture2D EditorTexture
        {
            get
            {
                if (editorTexture == null)
                {
                    editorTexture = new Texture2D(1, 1);
                    editorTexture.SetPixel(0, 0, UnityColor);
                    editorTexture.Apply();
                }
                return editorTexture;
            }
            set { editorTexture = value; }
        }

        public TerritoryData() { }

        public TerritoryData(string name)
        {
            Name = name;
            unityColor = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
            DisplayColor = new Vec3String(new Vector3(UnityColor.r, UnityColor.g, UnityColor.b));
            RallyPoint = new Vec3String(Vector3.zero);
            editorTexture = new Texture2D(1, 1);
            editorTexture.SetPixel(0, 0, UnityColor);
            editorTexture.Apply();
        }

        public void UpdateColor(Color c)
        {
            unityColor = c;
            EditorTexture.SetPixel(0, 0, c);
            EditorTexture.Apply();
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 13;
                hashCode = (hashCode * 397) ^ Name.GetHashCode();
                hashCode = (hashCode * 397) ^ DisplayColor.GetHashCode();
                hashCode = (hashCode * 397) ^ PopulationBonus;
                int r = Recruit ? 1 : 0;
                hashCode = (hashCode * 397) ^ r;
                hashCode = (hashCode * 397) ^ RallyPoint.GetHashCode();

                return hashCode;
            }
        }
    }
}