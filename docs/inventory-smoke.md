# Inventory Smoke Test (Halcyon + Firestorm)

Quick manual checks to verify inventory fetch compatibility with newer Firestorm viewers:

1) Log in with Firestorm on Halcyon with a known-good account (non-empty inventory).
2) Open inventory and wait for the initial fetch to complete:
   - Ensure folders populate without “broken/missing” indicators.
   - Confirm links (link and link-folder) resolve correctly.
3) Verify a folder fetch refresh:
   - Right-click a mid-level folder and choose “Empty trash” (if safe) or “Reload”/“Recent Items”.
   - Confirm folder contents and subfolders remain intact; no items disappear.
4) Create and save a notecard in inventory:
   - New notecard → add text → save; reopen to confirm content.
5) Copy an embedded item from a notecard:
   - Drop an item into a notecard, copy to inventory; confirm the copied item arrives with correct permissions.
6) Region restart check:
   - Relog after a region restart; confirm the same folders/items and links are present.

Notes:
- Halcyon intentionally omits AISv3 caps; Firestorm stays on legacy inventory caps (`FetchInventoryDescendents2`).
- Server now always sends `asset_id` (all types) and `linked_id` for links; folders include `parent_id`, `name`, `type`, `preferred_type`, `descendents`, `version`.
