/*
 * Copyright (c) Halcyon Developers
 * Based on OpenSimulator interfaces for inventory archiving.
 */

using System;
using OpenMetaverse;
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

        void DearchiveInventory(UUID id, string firstName, string lastName, string invPath, string loadPath, bool merge);
        void DearchiveInventory(UUID id, string firstName, string lastName, string invPath, System.IO.Stream loadStream, bool merge);
        void ArchiveInventory(UUID id, string firstName, string lastName, string invPath, string savePath);
        void ArchiveInventory(UUID id, string firstName, string lastName, string invPath, System.IO.Stream saveStream, string perm = null, bool skipAssets = false, bool skipNoPerms = false);
    }
}
