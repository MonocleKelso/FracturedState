using System.Collections.Generic;

namespace FracturedState.Game.Nav
{
    public class Path
    {
        public List<NavMeshPoint> Points { get; private set; }

        public Path()
        {
            Points = new List<NavMeshPoint>();
        }

        public void AddPoint(NavMeshPoint point)
        {
            Points.Add(point);
        }
    }
}