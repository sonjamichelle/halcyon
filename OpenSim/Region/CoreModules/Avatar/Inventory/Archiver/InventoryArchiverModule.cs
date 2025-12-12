/*
 * Ported from OpenSimulator Inventory Archiver to Halcyon.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Mono.Addins;
using NDesk.Options;
using Nini.Config;
using OpenMetaverse;
using OpenSim.Framework;
using OpenSim.Framework.Console;
using OpenSim.Region.Framework.Interfaces;
using OpenSim.Region.Framework.Scenes;
using OpenSim.Services.Interfaces;

namespace OpenSim.Region.CoreModules.Avatar.Inventory.Archiver
{
    [Extension(Path = "/OpenSim/RegionModules", NodeName = "RegionModule", Id = "InventoryArchiverModule")]
    public class InventoryArchiverModule : ISharedRegionModule, IInventoryArchiverModule
    {
        private static readonly log4net.ILog m_log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public event InventoryArchiveSaved OnInventoryArchiveSaved;
        public event InventoryArchiveLoaded OnInventoryArchiveLoaded;

        protected const string DEFAULT_INV_BACKUP_FILENAME = "user-inventory.iar";

        protected List<UUID> m_pendingConsoleTasks = new List<UUID>();
        private Dictionary<UUID, Scene> m_scenes = new Dictionary<UUID, Scene>();
        private Scene m_aScene;
        private IUserAccountService m_UserAccountService;

        public void Initialise(IConfigSource source) { }

        public void AddRegion(Scene scene)
        {
            if (m_scenes.Count == 0)
            {
                scene.RegisterModuleInterface<IInventoryArchiverModule>(this);
                OnInventoryArchiveSaved += SaveInvConsoleCommandCompleted;
                OnInventoryArchiveLoaded += LoadInvConsoleCommandCompleted;

                scene.AddCommand(
                    "Archiving", this, "load iar",
                    "load iar [-m|--merge] <first> <last> <inventory path> <password> [<IAR path>]",
                    "Load user inventory archive (IAR).",
                    "-m|--merge merges into existing folders where possible."
                    + "<first> user's first name.\n"
                    + "<last> user's last name.\n"
                    + "<inventory path> target path inside the user's inventory.\n"
                    + "<password> user's password.\n"
                    + "<IAR path> filesystem path or URI; defaults to user-inventory.iar in current directory.",
                    HandleLoadInvConsoleCommand);

                scene.AddCommand(
                    "Archiving", this, "save iar",
                    "save iar [-h|--home=<url>] [--noassets | --skipbadassets] [--perm=<CTM>] <first> <last> <inventory path> <password> [<IAR path>]",
                    "Save user inventory archive (IAR).",
                    "<first> user's first name.\n"
                    + "<last> user's last name.\n"
                    + "<inventory path> path inside the user's inventory for folder/item to save.\n"
                    + "<IAR path> filesystem path to save; defaults to user-inventory.iar in current directory.\n"
                    + "-h|--home=<url> adds profile service URL to saved info.\n"
                    + "--noassets skips saving assets.\n"
                    + "--skipbadassets skips items with missing main assets.\n"
                    + "--perm=<CTM> require perms (Copy/Transfer/Modify) to include item.",
                    HandleSaveInvConsoleCommand);

                m_aScene = scene;
            }

            m_scenes[scene.RegionInfo.RegionID] = scene;
        }

        public void RemoveRegion(Scene scene) { }
        public void Close() { }
        public void RegionLoaded(Scene scene) { }
        public void PostInitialise() { }
        public Type ReplaceableInterface { get { return null; } }
        public string Name { get { return "Inventory Archiver Module"; } }

        protected IUserAccountService UserAccountService
        {
            get
            {
                if (m_UserAccountService == null)
                {
                    foreach (Scene s in m_scenes.Values)
                    {
                        m_UserAccountService = s.RequestModuleInterface<IUserAccountService>();
                        if (m_UserAccountService != null)
                            break;
                    }
                }
                return m_UserAccountService;
            }
        }

        protected internal void TriggerInventoryArchiveSaved(
            UUID id, bool succeeded, UserAccount userInfo, string invPath, Stream saveStream,
            Exception reportedException, int saveCount, int filteredCount)
        {
            var handler = OnInventoryArchiveSaved;
            if (handler != null)
                handler(id, succeeded, userInfo, invPath, saveStream, reportedException, saveCount, filteredCount);
        }

        protected internal void TriggerInventoryArchiveLoaded(
            UUID id, bool succeeded, UserAccount userInfo, string invPath, Stream loadStream,
            Exception reportedException, int loadCount)
        {
            var handler = OnInventoryArchiveLoaded;
            if (handler != null)
                handler(id, succeeded, userInfo, invPath, loadStream, reportedException, loadCount);
        }

        public bool ArchiveInventory(
            UUID id, string firstName, string lastName, string invPath, string pass, Stream saveStream)
        {
            return ArchiveInventory(id, firstName, lastName, invPath, pass, saveStream, new Dictionary<string, object>());
        }

        public bool ArchiveInventory(
            UUID id, string firstName, string lastName, string invPath, string pass, Stream saveStream,
            Dictionary<string, object> options)
        {
            if (m_scenes.Count == 0)
                return false;

            UserAccount userInfo = GetUserInfo(firstName, lastName, pass);
            if (userInfo == null)
                return false;

            try
            {
                InventoryArchiveWriteRequest iarReq = new InventoryArchiveWriteRequest(id, this, m_aScene, userInfo, invPath, saveStream);
                iarReq.Execute(options, UserAccountService);
            }
            catch (EntryPointNotFoundException e)
            {
                m_log.Error("[INVENTORY ARCHIVER]: zlib/mono mismatch when creating compression stream", e);
                return false;
            }

            return true;
        }

        public void ArchiveInventory(UUID id, string firstName, string lastName, string invPath, string savePath)
        {
            FileStream fs = new FileStream(savePath, FileMode.Create);
            ArchiveInventory(id, firstName, lastName, invPath, "notused", fs);
            fs.Close();
        }

        public bool DearchiveInventory(UUID id, string firstName, string lastName, string invPath, string pass, Stream loadStream, bool merge)
        {
            if (m_scenes.Count == 0)
                return false;

            UserAccount userInfo = GetUserInfo(firstName, lastName, pass);
            if (userInfo == null)
                return false;

            InventoryArchiveReadRequest request =
                new InventoryArchiveReadRequest(id, this, m_aScene.InventoryService, m_aScene.AssetService, UserAccountService, userInfo, invPath, loadStream, merge);
            request.Execute();
            return true;
        }

        public bool DearchiveInventory(UUID id, string firstName, string lastName, string invPath, string pass, string loadPath, bool merge)
        {
            using (StreamReader reader = new StreamReader(loadPath))
            {
                return DearchiveInventory(id, firstName, lastName, invPath, pass, reader.BaseStream, merge);
            }
        }

        private UserAccount GetUserInfo(string firstName, string lastName, string pass)
        {
            if (UserAccountService == null)
            {
                m_log.Error("[INVENTORY ARCHIVER]: No user account service");
                return null;
            }

            UserAccount user = UserAccountService.GetUserAccount(m_aScene.RegionInfo.ScopeID, firstName, lastName);
            if (user == null)
            {
                m_log.ErrorFormat("[INVENTORY ARCHIVER]: failed to find user {0} {1}", firstName, lastName);
                return null;
            }

            if (!m_aScene.AuthenticateUser(user.PrincipalID, pass, 30))
            {
                m_log.Error("[INVENTORY ARCHIVER]: authentication failed");
                return null;
            }

            return user;
        }

        #region Console handlers

        protected void HandleSaveInvConsoleCommand(string module, string[] args)
        {
            // save iar [-h|--home=<url>] [--noassets | --skipbadassets] [--perm=<CTM>] <first> <last> <inventory path> <password> [<IAR path>]
            if (args.Length < 6)
            {
                m_log.Error("Usage: save iar [-h|--home=<url>] [--noassets | --skipbadassets] [--perm=<CTM>] <first> <last> <inventory path> <password> [<IAR path>]");
                return;
            }

            string first = String.Empty, last = String.Empty, invPath = String.Empty, pass = String.Empty, iarPath = DEFAULT_INV_BACKUP_FILENAME;
            bool skipAssets = false;
            bool skipBad = false;
            string perm = null;
            OptionSet opts = new OptionSet()
                .Add("h|home=", v => { /* ignored */ })
                .Add("noassets", v => skipAssets = true)
                .Add("skipbadassets", v => skipBad = true)
                .Add("perm=", v => perm = v);

            List<string> extra = opts.Parse(new List<string>(args).GetRange(2, args.Length - 2));
            if (extra.Count < 4)
            {
                m_log.Error("Usage: save iar ... <first> <last> <inventory path> <password> [<IAR path>]");
                return;
            }

            first = extra[0];
            last = extra[1];
            invPath = extra[2];
            pass = extra[3];
            if (extra.Count > 4)
                iarPath = extra[4];

            UUID id = UUID.Random();
            m_pendingConsoleTasks.Add(id);
            FileStream fs = new FileStream(iarPath, FileMode.Create);

            ArchiveInventory(id, first, last, invPath, pass, fs, new Dictionary<string, object>
            {
                { "perm", perm },
                { "skipassets", skipAssets },
                { "skipbadassets", skipBad }
            });
        }

        protected void HandleLoadInvConsoleCommand(string module, string[] args)
        {
            // load iar [-m|--merge] <first> <last> <inventory path> <password> [<IAR path>]
            if (args.Length < 6)
            {
                m_log.Error("Usage: load iar [-m|--merge] <first> <last> <inventory path> <password> [<IAR path>]");
                return;
            }

            bool merge = false;
            OptionSet opts = new OptionSet()
                .Add("m|merge", v => merge = true);

            List<string> extra = opts.Parse(new List<string>(args).GetRange(2, args.Length - 2));
            if (extra.Count < 4)
            {
                m_log.Error("Usage: load iar [-m|--merge] <first> <last> <inventory path> <password> [<IAR path>]");
                return;
            }

            string first = extra[0];
            string last = extra[1];
            string invPath = extra[2];
            string pass = extra[3];
            string iarPath = DEFAULT_INV_BACKUP_FILENAME;
            if (extra.Count > 4)
                iarPath = extra[4];

            UUID id = UUID.Random();
            m_pendingConsoleTasks.Add(id);
            using (FileStream fs = new FileStream(iarPath, FileMode.Open))
            {
                DearchiveInventory(id, first, last, invPath, pass, fs, merge);
            }
        }

        private void SaveInvConsoleCommandCompleted(UUID id, bool succeeded, UserAccount userInfo, string invPath, object saveStream, Exception reportedException, int saveCount, int filteredCount)
        {
            if (m_pendingConsoleTasks.Contains(id))
            {
                m_pendingConsoleTasks.Remove(id);
                if (!succeeded)
                    m_log.ErrorFormat("[INVENTORY ARCHIVER]: failed to save iar for {0} at {1}: {2}", userInfo.Name, invPath, reportedException);
                else
                    m_log.InfoFormat("[INVENTORY ARCHIVER]: saved iar for {0} at {1}. Items saved: {2}, filtered: {3}", userInfo.Name, invPath, saveCount, filteredCount);
            }
        }

        private void LoadInvConsoleCommandCompleted(UUID id, bool succeeded, UserAccount userInfo, string invPath, object loadStream, Exception reportedException, int loadCount)
        {
            if (m_pendingConsoleTasks.Contains(id))
            {
                m_pendingConsoleTasks.Remove(id);
                if (!succeeded)
                    m_log.ErrorFormat("[INVENTORY ARCHIVER]: failed to load iar for {0} at {1}: {2}", userInfo.Name, invPath, reportedException);
                else
                    m_log.InfoFormat("[INVENTORY ARCHIVER]: loaded iar for {0} at {1}. Items restored: {2}", userInfo.Name, invPath, loadCount);
            }
        }

        #endregion
    }
}
