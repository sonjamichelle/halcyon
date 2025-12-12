# Verge Base Fixes (Branch: verge-base-fixes)

## Notecard Reliability
- Added dedicated CAPS handler `UpdateNotecardTaskInventory` with `NoteCardTaskInventory` + `TaskInventoryItemUpdater` so task notecard uploads go to the correct endpoint instead of the script uploader.
- Task notecard uploads now persist: server stores the new asset, updates the prim’s inventory item, and returns proper LLSD (`new_asset`, `new_inventory_item`, `state`).
- Split script vs notecard task update paths to avoid collisions; kept existing cap names for viewer compatibility.
- Increased default notecard line read limit from 255 to 1023 (configurable; still capped at 65535) to prevent truncated reads.
- Agent inventory updates for notecards/scripts unchanged; prim task notecards now use the correct uploader and asset update flow.

## Covenant Persistence
- Fixed estate covenant change handling: the viewer sends UUID and timestamp in separate params, but server parsed each param as both fields, causing the final param (timestamp) to overwrite the covenant with UUID.Zero. Now we parse once (param[0]=UUID, param[1]=timestamp or current time fallback) and fire a single update, so covenant IDs persist across reopen/restart.
- Covenant asset retrieval is permitted via `LLTST_SIM_ESTATE`; with the corrected UUID persistence, existing covenant notecard assets remain referenced properly.

## Inventory Compatibility (Firestorm)
- Always serialize `asset_id` for inventory items (including objects) and include `linked_id` for link/link-folder items to match newer Firestorm inventory expectations and avoid items being treated as broken/removed on fetch.
- This addresses inventory loss seen with newer Firestorm builds when connecting to Halcyon (inventory fetch/serialization mismatch).
- Inventory folder fetch (`FetchInventoryDescendents2`) now includes `parent_id`, `name`, `type`, and `preferred_type` in the top-level folder map so viewers receive complete folder metadata.
- Overall: fetch responses now carry the key fields Firestorm expects for folders and items to prevent client-side pruning or "broken" entries during login/background fetch.
- Seed caps now explicitly omit AISv3 caps (`InventoryAPIv3`, `LibraryAPIv3`) so Firestorm stays on legacy inventory APIs Halcyon supports, avoiding viewer attempts to use unsupported AIS v3 endpoints.
- Added a manual inventory smoke checklist (`docs/inventory-smoke.md`) to validate Firestorm compatibility (folders/items populate, links resolve, notecards save, embedded items copy, post-restart consistency).

## Inventory Archives (IAR)
- Restored IAR save/load support via region console commands `save iar` / `load iar` (archives default to `/bin/user-inventory.iar` if no path given); assets flow through the configured asset service (WHIP).
- Linkset data is serialized in XML2 and round-trips through IARs.
- Usage and options documented in `docs/iar-notes.md` and `docs/Verge-IAR-Support.md` (perm filters, noassets/skipbadassets, merge loads, smoke steps).

## Hypergrid Restore (work plan)
- Added planning doc `docs/Hypergrid-Restore.md` outlining steps to re-enable HG behind a config gate (default off), components to port from OpenSim 0.9.3.0, and test matrix.
- Added `EnableHypergrid` config flag (Startup, default false) and surfaced it on Scene for future HG module gating.

## Linkset Data (behind flag)
- Added config flag `EnableLinksetData` (Startup section, default false) to enable per-linkset key/value storage persisted with objects.
- SceneObjectGroup now stores linkset data, serialized in XML2; deserialization preserves data.
- Phlox implements `llLinksetDataRead/Write/Delete/Reset/Count/FindKeys` with permission checks (requires object modify rights); reads gated by the config flag.

## CI/Local Smoke
- Added `scripts/local-smoke.ps1` to run a quick msbuild of `Halcyon.sln` (default Release) and fail fast on build errors.
- Added `docs/ci-smoke.md` to describe the smoke scripts/checklists and how to run them locally.

## Files Touched
- `OpenSim/Region/CoreModules/Capabilities/InventoryCapsModule.cs`
- `OpenSim/Region/Framework/Scenes/Scene.Inventory.cs`
- `InWorldz/InWorldz.Phlox.Engine/LSLSystemAPI.cs`
- `OpenSim/Region/ClientStack/LindenUDP/LLClientView.cs`
- `OpenSim/Framework/InventoryItemBase.cs`
- `docs/inventory-smoke.md`
- `scripts/local-smoke.ps1`
- `docs/ci-smoke.md`
- `OpenSim/Region/Framework/Scenes/Scene.cs`
- `OpenSim/Region/Framework/Scenes/SceneObjectGroup.cs`
- `OpenSim/Region/Framework/Scenes/Serialization/SceneObjectSerializer.cs`
- `InWorldz/InWorldz.Phlox.Engine/LSLSystemAPI.cs`
- `OpenSim/Region/Framework/Interfaces/IInventoryArchiverModule.cs`
- `OpenSim/Region/CoreModules/Avatar/Inventory/Archiver/*`
- `docs/iar-notes.md`
- `docs/Verge-IAR-Support.md`
- `docs/Hypergrid-Restore.md`

## Validation (manual steps recommended)
- Edit/save a notecard inside a prim (object inventory) and confirm it persists after closing/reopening and after region restart.
- Copy an embedded item from a notecard via viewer “Copy to Inventory” and verify it arrives with expected permissions.
- Verify script task updates still succeed via `UpdateScriptTaskInventory`.
