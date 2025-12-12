/*
 * Copyright (c) Contributors, http://opensimulator.org/
 * See CONTRIBUTORS.TXT for a full list of copyright holders.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *     * Redistributions of source code must retain the above copyright
 *       notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     * Neither the name of the OpenSimulator Project nor the
 *       names of its contributors may be used to endorse or promote products
 *       derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE DEVELOPERS ``AS IS'' AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE CONTRIBUTORS BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Collections.Generic;
using Nini.Config;
using OpenMetaverse;
using OpenSim.Framework;
using OpenSim.Server.Base;
using OpenSim.Services.Interfaces;

namespace OpenSim.Services.HypergridService
{
    public class HGFriendsService : IHGFriendsService
    {
        private readonly IFriendsService m_LocalService;

        public HGFriendsService(IConfigSource config)
        {
            IConfig serviceConfig = config.Configs["HGFriendsService"];
            if (serviceConfig == null)
                throw new Exception("HGFriendsService missing from configuration");

            string localService = serviceConfig.GetString("LocalServiceModule", string.Empty);
            if (string.IsNullOrEmpty(localService))
                throw new Exception("No LocalServiceModule in HGFriendsService configuration");

            Object[] args = new Object[] { config };
            m_LocalService = ServerUtils.LoadPlugin<IFriendsService>(localService, args);
            if (m_LocalService == null)
                throw new Exception("HGFriendsService could not load LocalServiceModule");
        }

        public int GetFriendPerms(UUID userID, UUID friendID)
        {
            return m_LocalService.GetFriendPerms(userID, friendID);
        }

        public bool NewFriendship(FriendInfo finfo, bool verified)
        {
            return m_LocalService.StoreFriend(ref finfo, verified);
        }

        public bool DeleteFriendship(FriendInfo finfo, string secret)
        {
            FriendInfo friend = GetFriend(finfo.PrincipalID, finfo.Friend);
            if (friend == null || (friend.TheirFlags != -1 && friend.TheirFlags != int.Parse(secret)))
                return false;

            FriendInfo finfo1 = new FriendInfo(finfo.PrincipalID, finfo.Friend, finfo.MyFlags, finfo.TheirFlags);
            FriendInfo finfo2 = new FriendInfo(new UUID(finfo.Friend), finfo.PrincipalID.ToString(), finfo.TheirFlags, finfo.MyFlags);

            m_LocalService.Delete(finfo1.PrincipalID, finfo1.Friend);
            m_LocalService.Delete(finfo2.PrincipalID, finfo2.Friend);

            return true;
        }

        public bool FriendshipOffered(UUID from, string fromName, UUID to, string message)
        {
            FriendInfo finfo = GetFriend(to, from.ToString());
            if (finfo != null && finfo.TheirFlags != -1)
                return false;

            GridInstantMessage msg = new GridInstantMessage(
                    null, from, fromName, to, (byte)InstantMessageDialog.FriendshipOffered,
                    message, false, Vector3.Zero);

            return (new InstantMessageServerConnector()).SendInstantMessage(msg, null);
        }

        public bool ValidateFriendshipOffered(UUID fromID, UUID toID)
        {
            FriendInfo friend = GetFriend(toID, fromID.ToString());

            if (friend == null || friend.TheirFlags != -1)
                return false;

            return true;
        }

        public List<UUID> StatusNotification(List<string> friends, UUID userID, bool online)
        {
            List<string> users = new List<string>();
            foreach (string f in friends)
            {
                FriendInfo finfo = GetFriend(userID, f);
                if (finfo != null && finfo.TheirFlags != -1 && (finfo.TheirFlags & (int)FriendRights.CanSeeOnline) != 0)
                    users.Add(f);
            }

            return m_LocalService.StatusNotification(users, userID, online);
        }

        private FriendInfo GetFriend(UUID ownerID, string friendID)
        {
            FriendInfo[] friends = m_LocalService.GetFriends(ownerID);
            if (friends != null)
            {
                foreach (FriendInfo f in friends)
                    if (f.Friend == friendID)
                        return f;
            }
            return null;
        }
    }
}
