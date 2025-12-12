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

## Files Touched
- `OpenSim/Region/CoreModules/Capabilities/InventoryCapsModule.cs`
- `OpenSim/Region/Framework/Scenes/Scene.Inventory.cs`
- `InWorldz/InWorldz.Phlox.Engine/LSLSystemAPI.cs`
- `OpenSim/Region/ClientStack/LindenUDP/LLClientView.cs`
- `OpenSim/Framework/InventoryItemBase.cs`

## Validation (manual steps recommended)
- Edit/save a notecard inside a prim (object inventory) and confirm it persists after closing/reopening and after region restart.
- Copy an embedded item from a notecard via viewer “Copy to Inventory” and verify it arrives with expected permissions.
- Verify script task updates still succeed via `UpdateScriptTaskInventory`.
