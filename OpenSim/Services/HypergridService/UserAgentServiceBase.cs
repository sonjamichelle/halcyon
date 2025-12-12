/*
 * Copyright (c) Contributors, http://opensimulator.org/
 */
using System;
using OpenSim.Services.Interfaces;
using OpenSim.Framework;
using OpenMetaverse;

namespace OpenSim.Services.HypergridService
{
    public class UserAgentServiceBase : IUserAgentService
    {
        public virtual bool LoginAgentToGrid(GridRegion source, AgentCircuitData agent, GridRegion gatekeeper, GridRegion finalDestination, bool fromLogin, out string reason)
        {
            reason = "not implemented";
            return false;
        }

        public virtual void LogoutAgent(UUID userID, UUID sessionID) {}

        public virtual GridRegion GetHomeRegion(UUID userID, out Vector3 position, out Vector3 lookAt)
        {
            position = Vector3.Zero;
            lookAt = Vector3.Zero;
            return null;
        }

        public virtual System.Collections.Generic.Dictionary<string, object> GetServerURLs(UUID userID)
        {
            return new System.Collections.Generic.Dictionary<string, object>();
        }

        public virtual System.Collections.Generic.Dictionary<string, object> GetUserInfo(UUID userID)
        {
            return new System.Collections.Generic.Dictionary<string, object>();
        }

        public virtual string LocateUser(UUID userID)
        {
            return String.Empty;
        }

        public virtual string GetUUI(UUID userID, UUID targetUserID)
        {
            return String.Empty;
        }

        public virtual UUID GetUUID(string first, string last)
        {
            return UUID.Zero;
        }

        public virtual System.Collections.Generic.List<UUID> StatusNotification(System.Collections.Generic.List<string> friends, UUID userID, bool online)
        {
            return new System.Collections.Generic.List<UUID>();
        }

        public virtual bool IsAgentComingHome(UUID sessionID, string thisGridExternalName)
        {
            return false;
        }

        public virtual bool VerifyAgent(UUID sessionID, string token)
        {
            return false;
        }

        public virtual bool VerifyClient(UUID sessionID, string reportedIP)
        {
            return false;
        }
    }
}
