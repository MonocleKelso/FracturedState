using FracturedState.Game.Data;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.Networking;

namespace FracturedState.Game.Network
{
    public class MapTransfer : MessageBase
    {
        public byte[] MapData;

        public void WriteMap(RawMapData mapData)
        {
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, mapData);
                MapData = ms.ToArray();
            }
        }

        public RawMapData ReadMap()
        {
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream(MapData))
            {
                return (RawMapData)bf.Deserialize(ms);
            }
        }
    }
}