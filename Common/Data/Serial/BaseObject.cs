using System.Xml.Serialization;
using System.Linq;
using UnityEngine;

namespace FracturedState.Game.Data
{
    public enum CoverPointStance
    {
        [XmlEnum("crouch")]
        Crouch, 
        [XmlEnum("stand")]
        Stand
    }

    /// <summary>
    /// The base class for all objects driven by XML data
    /// </summary>
    public class BaseObject
    {
        [XmlElement("name")]
        public string Name { get; set; }

        [XmlElement("editorExclude")]
        public bool ExcludeFromEditor { get; set; }
		
		[XmlElement("modelInfo")]
		public ModelInfo Model { get; set; }

        [XmlElement("boundingBox")]
        public BoundingBox BoundsBox { get; set; }

    }

    public class BoundingBox
    {
        [XmlElement("center")]
        public Vec3String Center { get; set; }

        [XmlElement("bounds")]
        public Vec3String Bounds { get; set; }
    }
	
    /// <summary>
    /// A container class for XML data representing the artwork assoiated with a given object
    /// </summary>
	public class ModelInfo
	{
		[XmlElement("outsideModel")]
		public string ExteriorModel { get; set; }
		
		[XmlElement("insideModel")]
		public string InteriorModel { get; set; }
		
		[XmlElement("editorModel")]
		public string EditorModel { get; set; }
	}

    /// <summary>
    /// A serialization class containing data about an individual point of cover on an object
    /// </summary>
    public class CoverPoint
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("parent")]
        public string ParentName { get; set; }

        [XmlAttribute("stance")]
        public CoverPointStance Stance { get; set; }

        [XmlElement("range")]
        public CoverPointValue[] Values { get; set; }

        public int GetBonus(Transform target, Vector3 attackPosition)
        {
            Vector3 toPoint = (attackPosition - target.position).normalized;
            float dot = Vector3.Dot(target.forward, toPoint);
            for (var i = 0; i < Values.Length; i++)
            {
                if (dot <= Values[i].Max && dot >= Values[i].Min)
                {
                    return Values[i].Bonus;
                }
            }
            return 0;
        }

        public int GetDirectionalBonus(Transform target, Vector3 direction)
        {
            float dot = Vector3.Dot(target.forward, direction);
            for (int i = 0; i < Values.Length; i++)
            {
                if (dot <= Values[i].Max && dot >= Values[i].Min)
                {
                    return Values[i].Bonus;
                }
            }
            return 0;
        }
    }

    public class CoverPointValue
    {
        [XmlAttribute("min")]
        public float Min { get; set; }

        [XmlAttribute("max")]
        public float Max { get; set; }

        [XmlAttribute("bonus")]
        public int Bonus { get; set; }
    }
}