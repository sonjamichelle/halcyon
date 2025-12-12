/*
 * Copyright (c) Contributors, http://opensimulator.org/
 */
using System;
using System.Collections.Generic;
using OpenMetaverse;
using OpenSim.Framework;
using OpenSim.Server.Base;
using OpenSim.Services.Connectors.Hypergrid;
using OpenSim.Services.Connectors.Friends;
using OpenSim.Services.Interfaces;
using GridRegion = OpenSim.Services.Interfaces.GridRegion;

namespace OpenSim.Services.HypergridService
{
    public class UserAgentService : UserAgentServiceBase
    {
        protected GatekeeperServiceConnector m_GatekeeperConnector;
        protected UserAgentServiceConnector m_UserAgentConnector;
        protected HGFriendsServicesConnector m_HGFriendsServiceConnector;
        protected bool m_ForeignTripsAllowed = true;

        public UserAgentService(OpenSimConfigSource config)
        {
            // Placeholder for configuration-based initialization of connectors and policies.
        }

        public override bool LoginAgentToGrid(GridRegion source, AgentCircuitData agent, GridRegion gatekeeper, GridRegion finalDestination, bool fromLogin, out string reason)
        {
            reason = "not implemented";
            return false;
        }

        public override void LogoutAgent(UUID userID, UUID sessionID)
        {
        }

        public override GridRegion GetHomeRegion(UUID userID, out Vector3 position, out Vector3 lookAt)
        {
            position = Vector3.Zero;
            lookAt = Vector3.Zero;
            return null;
        }

        public override Dictionary<string, object> GetServerURLs(UUID userID)
        {
            return new Dictionary<string, object>();
        }

        public override Dictionary<string, object> GetUserInfo(UUID userID)
        {
            return new Dictionary<string, object>();
        }

        public override string LocateUser(UUID userID)
        {
            return String.Empty;
        }

        public override string GetUUI(UUID userID, UUID targetUserID)
        {
            return String.Empty;
        }

        public override UUID GetUUID(string first, string last)
        {
            return UUID.Zero;
        }

        public override List<UUID> StatusNotification(List<string> friends, UUID userID, bool online)
        {
            return new List<UUID>();
        }

        public override bool IsAgentComingHome(UUID sessionID, string thisGridExternalName)
        {
            return false;
        }

        public override bool VerifyAgent(UUID sessionID, string token)
        {
            return false;
        }

        public override bool VerifyClient(UUID sessionID, string reportedIP)
        {
            return false;
        }
    }
}
