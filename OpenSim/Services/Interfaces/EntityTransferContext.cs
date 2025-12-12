/*
 * Minimal EntityTransferContext used by Hypergrid connectors.
 */

using OpenMetaverse.StructuredData;

namespace OpenSim.Services.Interfaces
{
    public class EntityTransferContext
    {
        public float OutboundVersion { get; set; }
        public float InboundVersion { get; set; }
        public int WearablesCount { get; set; }

        public OSDMap Pack()
        {
            OSDMap map = new OSDMap();
            Pack(map);
            return map;
        }

        public void Pack(OSDMap map)
        {
            if (map == null) return;
            map["OutboundVersion"] = OutboundVersion;
            map["InboundVersion"] = InboundVersion;
            map["WearablesCount"] = WearablesCount;
        }

        public void Unpack(OSDMap map)
        {
            if (map == null) return;
            if (map.ContainsKey("OutboundVersion"))
                OutboundVersion = (float)map["OutboundVersion"].AsReal();
            if (map.ContainsKey("InboundVersion"))
                InboundVersion = (float)map["InboundVersion"].AsReal();
            if (map.ContainsKey("WearablesCount"))
                WearablesCount = map["WearablesCount"].AsInteger();
        }
    }
}
