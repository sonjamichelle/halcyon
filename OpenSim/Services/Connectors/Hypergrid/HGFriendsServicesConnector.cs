/*
 * Copyright (c) Contributors, http://opensimulator.org/
 */
using System;
using System.Collections.Generic;
using OpenMetaverse;
using OpenSim.Services.Interfaces;

namespace OpenSim.Services.Connectors.Hypergrid
{
    public class HGFriendsServicesConnector : IHGFriendsService
    {
        public HGFriendsServicesConnector(string serverURI)
        {
            ServerURI = serverURI;
        }

        public string ServerURI { get; private set; }

        public int GetFriendPerms(UUID userID, UUID friendID) { return 0; }
        public bool NewFriendship(FriendInfo finfo, bool verified) { return false; }
        public bool DeleteFriendship(FriendInfo finfo, string secret) { return false; }
        public bool FriendshipOffered(UUID from, string fromName, UUID to, string message) { return false; }
        public bool ValidateFriendshipOffered(UUID fromID, UUID toID) { return false; }
        public List<UUID> StatusNotification(List<string> friends, UUID userID, bool online) { return new List<UUID>(); }
    }
}
