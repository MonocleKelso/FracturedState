using UnityEngine;
using Vectrosity;
using System.Collections.Generic;

namespace FracturedState.Game
{
    public sealed class LineManager
    {
        public static readonly Vector3 LineAdjustment = new Vector3(0, 0.3f, 0);

        private const string MoveLine = "MoveArrow";
        private const string MoveLineName = "MoveLineHelper";

        private const string EnterLine = "EnterArrow";
        private const string EnterLineName = "EnterLineHelper";

        private static LineManager instance;
        public static LineManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new LineManager();

                return instance;
            }
        }

        private Dictionary<string, List<VectorLine>> lines = new Dictionary<string, List<VectorLine>>();

        private LineManager()
        {
            
            VectorLine.SetCamera3D();
        }

        public VectorLine GetMoveLine()
        {
            List<VectorLine> moveLines;
            VectorLine line;
            if (lines.TryGetValue(MoveLine, out moveLines))
            {
                if (moveLines.Count > 0)
                {
                    line = moveLines[0];
                    moveLines.RemoveAt(0);
                    line.active = true;
                    return line;
                }
            }

            line = new VectorLine(MoveLineName, new List<Vector3>(2), 2.2f, LineType.Continuous);
            line.Draw3DAuto();
            return line;
        }

        public VectorLine GetEnterLine()
        {
            List<VectorLine> enterLines;
            VectorLine line;
            if (lines.TryGetValue(EnterLine, out enterLines))
            {
                if (enterLines.Count > 0)
                {
                    line = enterLines[0];
                    enterLines.RemoveAt(0);
                    line.active = true;
                    return line;
                }
            }

            line = new VectorLine(MoveLineName, new List<Vector3>(), 2.2f, LineType.Continuous);
            line.Draw3DAuto();
            return line;
        }

        public void ReturnLine(VectorLine line)
        {
            line.active = false;
            List<VectorLine> lineList;
            if (!lines.TryGetValue(line.name, out lineList))
            {
                lineList = new List<VectorLine>();
                lines[line.name] = lineList;
            }

            lineList.Add(line);
        }
    }
}