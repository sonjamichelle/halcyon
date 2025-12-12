/*
 * Copyright (c) Contributors, http://opensimulator.org/
 */
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using OpenMetaverse;
using OpenSim.Framework;
using OpenSim.Server.Base;
using OpenSim.Services.Connectors.Hypergrid;
using OpenSim.Services.Connectors.InstantMessage;
using OpenSim.Services.Interfaces;
using GridRegion = OpenSim.Services.Interfaces.GridRegion;

namespace OpenSim.Services.HypergridService
{
    public class GatekeeperService : IGatekeeperService
    {
        protected UserAccountServiceConnector m_UserAccountServiceConnector;
        protected HGInventoryServiceConnector m_HGInventoryServiceConnector;
        protected HGAssetServiceConnector m_HGAssetServiceConnector;
        protected HGFriendsServicesConnector m_HGFriendsServiceConnector;
        protected GatekeeperServiceConnector m_GatekeeperConnector;
        protected UserAgentServiceConnector m_UserAgentConnector;

        protected bool m_ForeignAgentsAllowed = true;
        protected List<string> m_HomeURIs = new List<string>();
        protected List<string> m_AllowedExceptions = new List<string>();
        protected List<string> m_DeniedExceptions = new List<string>();

        public GatekeeperService(OpenSimConfigSource config)
        {
            // Placeholder: in full integration, construct connectors using config endpoints.
        }

        public bool LinkLocalRegion(string regionDescriptor, out UUID regionID, out ulong regionHandle, out string externalName, out string imageURL, out string reason, out int sizeX, out int sizeY)
        {
            regionID = UUID.Zero;
            regionHandle = 0;
            externalName = String.Empty;
            imageURL = String.Empty;
            reason = "not implemented";
            sizeX = 256;
            sizeY = 256;
            return false;
        }

        public GridRegion GetHyperlinkRegion(UUID regionID, UUID agentID, string agentHomeURI, out string message)
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
