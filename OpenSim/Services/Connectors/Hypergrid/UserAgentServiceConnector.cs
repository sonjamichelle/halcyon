/*
 * Copyright (c) Contributors, http://opensimulator.org/
 */
using System;
using System.Collections.Generic;
using OpenMetaverse;
using OpenSim.Framework;
using OpenSim.Services.Interfaces;
using GridRegion = OpenSim.Services.Interfaces.GridRegion;

namespace OpenSim.Services.Connectors.Hypergrid
{
    public class UserAgentServiceConnector : IUserAgentService
    {
        public UserAgentServiceConnector(string serverURI)
        {
            ServerURI = serverURI;
        }

        public string ServerURI { get; private set; }

        public bool LoginAgentToGrid(GridRegion source, AgentCircuitData agent, GridRegion gatekeeper, GridRegion finalDestination, bool fromLogin, out string reason)
        {
            reason = "not implemented";
            return false;
        }

        public void LogoutAgent(UUID userID, UUID sessionID)
        {
        }

        public GridRegion GetHomeRegion(UUID userID, out Vector3 position, out Vector3 lookAt)
        {
            position = Vector3.Zero;
            lookAt = Vector3.Zero;
            return null;
        }

        public Dictionary<string, object> GetServerURLs(UUID userID)
        {
            return new Dictionary<string, object>();
        }

        public Dictionary<string, object> GetUserInfo(UUID userID)
        {
            return new Dictionary<string, object>();
        }

        public string LocateUser(UUID userID)
        {
            return String.Empty;
        }

        public string GetUUI(UUID userID, UUID targetUserID)
        {
            return String.Empty;
        }

        public UUID GetUUID(string first, string last)
        {
            return UUID.Zero;
        }

        public List<UUID> StatusNotification(List<string> friends, UUID userID, bool online)
        {
            return new List<UUID>();
        }

        public bool IsAgentComingHome(UUID sessionID, string thisGridExternalName)
        {
            return false;
        }

        public bool VerifyAgent(UUID sessionID, string token)
        {
            return false;
        }

        public bool VerifyClient(UUID sessionID, string reportedIP)
        {
            return false;
        }
    }
}
