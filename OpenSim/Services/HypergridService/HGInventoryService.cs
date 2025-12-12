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
using OpenMetaverse;
using log4net;
using Nini.Config;
using System.Reflection;
using OpenSim.Services.Base;
using OpenSim.Services.Interfaces;
using OpenSim.Services.InventoryService;
using OpenSim.Data;
using OpenSim.Framework;
using OpenSim.Server.Base;

namespace OpenSim.Services.HypergridService
{
    public class HGInventoryService : XInventoryService, IInventoryService
    {
        private static readonly ILog m_log =
                LogManager.GetLogger(
                MethodBase.GetCurrentMethod().DeclaringType);

        private string m_HomeURL;
        private IUserAccountService m_UserAccountService;

        private UserAccountCache m_Cache;

        public HGInventoryService(IConfigSource config, string configName)
            : base(config, configName)
        {
            m_log.Debug("[HGInventory Service]: Starting");
            if (configName != string.Empty)
                m_ConfigName = configName;

            IConfig invConfig = config.Configs[m_ConfigName];
            if (invConfig != null)
            {
                string userAccountsDll = invConfig.GetString("UserAccountsService", string.Empty);
                if (userAccountsDll.Length == 0)
                    throw new Exception("Please specify UserAccountsService in HGInventoryService configuration");

                Object[] args = new Object[] { config };
                m_UserAccountService = ServerUtils.LoadPlugin<IUserAccountService>(userAccountsDll, args);
                if (m_UserAccountService == null)
                    throw new Exception(String.Format("Unable to create UserAccountService from {0}", userAccountsDll));

                m_HomeURL = Util.GetConfigVarFromSections<string>(config, "HomeURI",
                    new string[] { "Startup", "Hypergrid", m_ConfigName }, String.Empty);

                m_Cache = UserAccountCache.CreateUserAccountCache(m_UserAccountService);
            }

            m_log.Debug("[HG INVENTORY SERVICE]: Starting...");
        }

        public override bool CreateUserInventory(UUID principalID)
        {
            return false;
        }

        public override List<InventoryFolderBase> GetInventorySkeleton(UUID principalID)
        {
            return new List<InventoryFolderBase>();
        }

        public override InventoryFolderBase GetRootFolder(UUID principalID)
        {
            XInventoryFolder[] folders = m_Database.GetFolders(
                    new string[] { "agentID", "folderName" },
                    new string[] { principalID.ToString(), "My Suitcase" });

            if (folders.Length > 0)
                return ConvertToOpenSim(folders[0]);

            XInventoryFolder suitcase = CreateFolder(principalID, UUID.Zero, (int)FolderType.Suitcase, "My Suitcase");
            return ConvertToOpenSim(suitcase);
        }

        public override InventoryFolderBase GetFolderForType(UUID principalID, FolderType type)
        {
            return GetRootFolder(principalID);
        }

        public override InventoryCollection[] GetMultipleFoldersContent(UUID principalID, UUID[] folderID)
        {
            return new InventoryCollection[0];
        }

        private List<InventoryFolderBase> GetDescendents(List<InventoryFolderBase> lst, UUID root)
        {
            List<InventoryFolderBase> direct = lst.FindAll(delegate (InventoryFolderBase f) { return f.ParentID == root; });
            if (direct == null)
                return new List<InventoryFolderBase>();

            List<InventoryFolderBase> indirect = new List<InventoryFolderBase>();
            foreach (InventoryFolderBase f in direct)
                indirect.AddRange(GetDescendents(lst, f.ID));

            direct.AddRange(indirect);
            return direct;
        }

        public override bool DeleteFolders(UUID principalID, List<UUID> folderIDs)
        {
            return false;
        }

        public override bool PurgeFolder(InventoryFolderBase folder)
        {
            return false;
        }

        public override InventoryItemBase GetItem(UUID principalID, UUID itemID)
        {
            InventoryItemBase it = base.GetItem(principalID, itemID);
            if (it != null)
            {
                UserAccount user = m_Cache.GetUser(it.CreatorId);

                if (user != null && it != null && string.IsNullOrEmpty(it.CreatorData))
                    it.CreatorData = m_HomeURL + ";" + user.FirstName + " " + user.LastName;
            }
            return it;
        }
    }
}
