using System;
using System.Xml.Serialization;

namespace FracturedState
{
	/// <summary>
	/// A string representation of a Vector3 struct
	/// </summary>
    [Serializable]
	public struct Vec3String
	{
        [XmlAttribute("x")]
		public string X;

        [XmlAttribute("y")]
		public string Y;

        [XmlAttribute("z")]
		public string Z;

		public Vec3String(string x, string y, string z)
		{
            X = x;
            Y = y;
            Z = z;
		}
		
		public Vec3String(Vec3String vec)
		{
            X = vec.X;
            Y = vec.Y;
            Z = vec.Z;
		}
		
		public Vec3String(UnityEngine.Vector3 vec)
		{
            X = vec.x.ToString();
            Y = vec.y.ToString();
            Z = vec.z.ToString();
		}
		
		/// <summary>
		/// Attempts to parse the values in this Vec3String to a valid Vector3 struct.
		/// </summary>
		/// <returns>A boolean value representing the success of the conversion</returns>
		public bool TryVector3(out UnityEngine.Vector3 v)
		{
			float x, y, z;
			if (float.TryParse(X, out x) && float.TryParse(Y, out y) && float.TryParse(Z, out z))
			{
				v = new UnityEngine.Vector3(x, y, z);
				return true;
			}
            v = UnityEngine.Vector3.zero;
			return false;
		}
		
		public static bool operator ==(Vec3String lhs, Vec3String rhs)
		{
			if (System.Object.ReferenceEquals(lhs, rhs))
				return true;
				
			if (((object)lhs == null) || ((object)rhs == null))
				return false;
			
			return lhs.X == rhs.X && lhs.Y == rhs.Y && lhs.Z == rhs.Z;
		}
		
		public static bool operator !=(Vec3String lhs, Vec3String rhs)
		{
			return !(lhs == rhs);
		}

        public override bool Equals(object obj)
        {
            if (obj.GetType() != typeof(Vec3String))
                return false;

            Vec3String v = (Vec3String)obj;
            return X == v.X && Y == v.Y && Z == v.Z;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 29 + X.GetHashCode();
                hash = hash * 29 + Y.GetHashCode();
                hash = hash * 29 + Z.GetHashCode();
                return hash;
            }
        }
	}
}