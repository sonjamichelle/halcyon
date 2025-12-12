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
using OpenSim.Data;
using OpenSim.Services.Base;
using OpenMetaverse;

namespace OpenSim.Services.HypergridService
{
    public class UserAgentServiceBase : ServiceBase
    {
        protected IHGTravelingData m_Database = null;

        public UserAgentServiceBase(IConfigSource config)
            : base(config)
        {
            string dllName = string.Empty;
            string connString = string.Empty;
            string realm = "hg_traveling_data";

            IConfig dbConfig = config.Configs["DatabaseService"];
            if (dbConfig is not null)
            {
                if (dllName.Length == 0)
                    dllName = dbConfig.GetString("StorageProvider", string.Empty);
                if (connString.Length == 0)
                    connString = dbConfig.GetString("ConnectionString", string.Empty);
            }

            IConfig gridConfig = config.Configs["UserAgentService"];
            if (gridConfig is not null)
            {
                dllName = gridConfig.GetString("StorageProvider", dllName);
                connString = gridConfig.GetString("ConnectionString", connString);
                realm = gridConfig.GetString("Realm", realm);
            }

            if (!string.IsNullOrEmpty(dllName))
            {
                m_Database = LoadPlugin<IHGTravelingData>(dllName, new Object[] { connString, realm });
            }

            if (m_Database is null)
            {
                // Fallback to an in-memory store so HG can function without DB wiring.
                m_Database = new InMemoryHGTravelingData();
            }
        }
    }

    internal class InMemoryHGTravelingData : IHGTravelingData
    {
        private readonly Dictionary<UUID, HGTravelingData> _bySession = new Dictionary<UUID, HGTravelingData>();
        private readonly Dictionary<UUID, List<UUID>> _sessionsByUser = new Dictionary<UUID, List<UUID>>();
        private readonly object _lock = new object();

        public HGTravelingData Get(UUID sessionID)
        {
            lock (_lock)
            {
                _bySession.TryGetValue(sessionID, out var data);
                return data;
            }
        }

        public HGTravelingData[] GetSessions(UUID userID)
        {
            lock (_lock)
            {
                if (!_sessionsByUser.TryGetValue(userID, out var list))
                    return Array.Empty<HGTravelingData>();
                List<HGTravelingData> res = new List<HGTravelingData>();
                foreach (var sid in list)
                {
                    if (_bySession.TryGetValue(sid, out var d))
                        res.Add(d);
                }
                return res.ToArray();
            }
        }

        public bool Store(HGTravelingData data)
        {
            if (data == null)
                return false;
            lock (_lock)
            {
                _bySession[data.SessionID] = data;
                if (!_sessionsByUser.TryGetValue(data.UserID, out var list))
                {
                    list = new List<UUID>();
                    _sessionsByUser[data.UserID] = list;
                }
                if (!list.Contains(data.SessionID))
                    list.Add(data.SessionID);
            }
            return true;
        }

        public bool Delete(UUID sessionID)
        {
            lock (_lock)
            {
                if (_bySession.TryGetValue(sessionID, out var data))
                {
                    _bySession.Remove(sessionID);
                    if (_sessionsByUser.TryGetValue(data.UserID, out var list))
                        list.Remove(sessionID);
                    return true;
                }
            }
            return false;
        }

        public void DeleteOld()
        {
            // No-op for in-memory; nothing to age out.
        }
    }
}
