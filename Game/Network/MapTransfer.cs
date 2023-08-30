using FracturedState.Game.Data;
using System.Text;

namespace FracturedState
{
    public class MapTransfer
    {
        StringBuilder data;

        public MapTransfer()
        {
            data = new StringBuilder();
        }

        public void AddMapData(string dataChunk)
        {
            data.Append(dataChunk);
        }

        public RawMapData LoadMap()
        {
            RawMapData map = DataUtil.DeserializeXmlString<RawMapData>(data.ToString());
            data.Length = 0;
            data.Capacity = 0;
            return map;
        }
    }
}