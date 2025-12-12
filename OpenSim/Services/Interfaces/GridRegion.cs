/*
 * Minimal GridRegion port from OpenSim 0.9.x for Hypergrid support.
 */

using System;
using System.Net;
using OpenMetaverse;

namespace OpenSim.Services.Interfaces
{
    public class GridRegion
    {
        public UUID RegionID { get; set; }
        public string RegionName { get; set; }
        public ulong RegionHandle { get; set; }
        public IPEndPoint ExternalEndPoint { get; set; }
        public IPEndPoint InternalEndPoint { get; set; }
        public string ExternalHostName { get; set; }
        public uint HttpPort { get; set; }
        public string ServerURI { get; set; }
        public string GatekeeperURI { get; set; }
        public uint RegionSizeX { get; set; }
        public uint RegionSizeY { get; set; }
        public int RegionLocX { get; set; }
        public int RegionLocY { get; set; }
        public string RawServerURI { get; set; }

        public GridRegion()
        {
            RegionID = UUID.Zero;
            RegionName = string.Empty;
            ExternalHostName = string.Empty;
            ServerURI = string.Empty;
            GatekeeperURI = string.Empty;
            RegionSizeX = 256;
            RegionSizeY = 256;
            RawServerURI = string.Empty;
        }

        public GridRegion(GridRegion other)
        {
            if (other == null)
                return;

            RegionID = other.RegionID;
            RegionName = other.RegionName;
            RegionHandle = other.RegionHandle;
            ExternalEndPoint = other.ExternalEndPoint;
            InternalEndPoint = other.InternalEndPoint;
            ExternalHostName = other.ExternalHostName;
            HttpPort = other.HttpPort;
            ServerURI = other.ServerURI;
            GatekeeperURI = other.GatekeeperURI;
            RegionSizeX = other.RegionSizeX;
            RegionSizeY = other.RegionSizeY;
            RegionLocX = other.RegionLocX;
            RegionLocY = other.RegionLocY;
            RawServerURI = other.RawServerURI;
        }
    }
}
