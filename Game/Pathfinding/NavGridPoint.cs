using UnityEngine;

namespace FracturedState.Game.Nav
{
    public class NavGridPoint
    {
        public Vector3 WorldPosition { get; protected set; }
        public int Xindex { get; protected set; }
        public int Zindex { get; protected set; }
        public bool IsOccupied { get; set; }
        public double Radius { get; set; }

        public NavGridPoint(Vector3 position, int x, int z, double radius)
        {
            WorldPosition = position;
            Xindex = x;
            Zindex = z;
            Radius = radius;
        }
    }
}