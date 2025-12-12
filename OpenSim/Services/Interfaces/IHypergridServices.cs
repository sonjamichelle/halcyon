/*
 * Copyright (c) Contributors, http://opensimulator.org/
 *
 * Hypergrid service interfaces (port from OpenSim 0.9.3.0).
 */

using System;
using System.Collections.Generic;
using OpenMetaverse;
using OpenSim.Framework;

namespace OpenSim.Services.Interfaces
{
    public interface IGatekeeperService
    {
        bool LinkLocalRegion(string regionDescriptor, out UUID regionID, out ulong regionHandle, out string externalName, out string imageURL, out string reason, out int sizeX, out int sizeY);
        GridRegion GetHyperlinkRegion(UUID regionID, UUID agentID, string agentHomeURI, out string message);
        bool LoginAgent(GridRegion source, AgentCircuitData aCircuit, GridRegion destination, out string reason);
    }

    public interface IUserAgentService
    {
        bool LoginAgentToGrid(GridRegion source, AgentCircuitData agent, GridRegion gatekeeper, GridRegion finalDestination, bool fromLogin, out string reason);
        void LogoutAgent(UUID userID, UUID sessionID);
        GridRegion GetHomeRegion(UUID userID, out Vector3 position, out Vector3 lookAt);
        Dictionary<string, object> GetServerURLs(UUID userID);
        Dictionary<string, object> GetUserInfo(UUID userID);
        string LocateUser(UUID userID);
        string GetUUI(UUID userID, UUID targetUserID);
        UUID GetUUID(string first, string last);
        [Obsolete]
        List<UUID> StatusNotification(List<string> friends, UUID userID, bool online);
        bool IsAgentComingHome(UUID sessionID, string thisGridExternalName);
        bool VerifyAgent(UUID sessionID, string token);
        bool VerifyClient(UUID sessionID, string reportedIP);
    }

    public interface IInstantMessage
    {
        bool IncomingInstantMessage(GridInstantMessage im);
        bool OutgoingInstantMessage(GridInstantMessage im, string url, bool foreigner);
    }

    public interface IFriendsSimConnector
    {
        bool StatusNotify(UUID userID, UUID friendID, bool online);
        bool LocalFriendshipOffered(UUID toID, GridInstantMessage im);
        bool LocalFriendshipApproved(UUID userID, string userName, UUID friendID);
    }

    public interface IHGFriendsService
    {
        int GetFriendPerms(UUID userID, UUID friendID);
        bool NewFriendship(FriendInfo finfo, bool verified);
        bool DeleteFriendship(FriendInfo finfo, string secret);
        bool FriendshipOffered(UUID from, string fromName, UUID to, string message);
        bool ValidateFriendshipOffered(UUID fromID, UUID toID);
        List<UUID> StatusNotification(List<string> friends, UUID userID, bool online);
    }

    public interface IInstantMessageSimConnector
    {
        bool SendInstantMessage(GridInstantMessage im);
    }
}
