/*
 * Copyright (c) Contributors, http://opensimulator.org/
 */
using System;
using OpenSim.Services.Interfaces;
using OpenMetaverse;

namespace OpenSim.Services.Connectors.Hypergrid
{
    public class GatekeeperServiceConnector
    {
        public GatekeeperServiceConnector(string serverURI)
        {
            ServerURI = serverURI;
        }

        public string ServerURI { get; private set; }

        public GridRegion GetHyperlinkRegion(GridRegion gatekeeper, UUID regionID, UUID agentID, string agentHomeURI, out string message)
        {
            message = "not implemented";
            return null;
        }

        public bool LoginAgent(GridRegion source, AgentCircuitData aCircuit, GridRegion destination, out string reason)
        {
            reason = "not implemented";
            return false;
        }
    }
}
