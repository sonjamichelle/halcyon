/*
 * Copyright (c) Halcyon Developers
 * Based on OpenSimulator interfaces for inventory archiving.
 */

using System;
using OpenMetaverse;
using OpenSim.Framework;
using OpenSim.Services.Interfaces;

namespace OpenSim.Region.Framework.Interfaces
{
    /// <summary>
    /// Used for the OnInventoryArchiveSaved event.
    /// </summary>
    public delegate void InventoryArchiveSaved(
        UUID id, bool succeeded, UserAccount userInfo, string invPath, object saveStream, Exception reportedException, int saveCount, int filteredCount);

    /// <summary>
    /// Used for the OnInventoryArchiveLoaded event.
    /// </summary>
    public delegate void InventoryArchiveLoaded(
        UUID id, bool succeeded, UserAccount userInfo, string invPath, object loadStream, Exception reportedException, int loadCount);

    public interface IInventoryArchiverModule
    {
        event InventoryArchiveSaved OnInventoryArchiveSaved;
        event InventoryArchiveLoaded OnInventoryArchiveLoaded;

        bool DearchiveInventory(UUID id, string firstName, string lastName, string invPath, string pass, string loadPath, bool merge);
        bool DearchiveInventory(UUID id, string firstName, string lastName, string invPath, System.IO.Stream loadStream, bool merge);
        bool DearchiveInventory(UUID id, string firstName, string lastName, string invPath, string pass, System.IO.Stream loadStream, bool merge);
        void ArchiveInventory(UUID id, string firstName, string lastName, string invPath, string savePath);
        bool ArchiveInventory(UUID id, string firstName, string lastName, string invPath, string pass, System.IO.Stream saveStream);
        bool ArchiveInventory(UUID id, string firstName, string lastName, string invPath, string pass, System.IO.Stream saveStream, System.Collections.Generic.Dictionary<string, object> options);
    }
}
