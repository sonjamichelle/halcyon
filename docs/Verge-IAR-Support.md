# Verge IAR Support (Halcyon)

Status: IAR save/load restored via region console commands. Archives default to `/bin/user-inventory.iar` unless a path/URL is provided. Assets are pulled/pushed through the configured asset service (WHIP).

Commands (region console)
- `save iar [-h|--home=<url>] [--noassets | --skipbadassets] [--perm=<CTM>] <first> <last> <inventory path> <password> [<IAR path>]`
  - `--noassets`: skip embedding assets.
  - `--skipbadassets`: skip items with missing main assets.
  - `--perm=<CTM>`: require Copy/Transfer/Modify to export.
  - `<inventory path>`: folder or item; `/*` or `/` for full inventory.
- `load iar [-m|--merge] <first> <last> <inventory path> <password> [<IAR path>]`
  - `--merge`: reuse existing folders where possible (recommended for full restores).
  - `<IAR path>`: local file or HTTP URL; defaults to `/bin/user-inventory.iar`.

Notes
- Linkset data (when `EnableLinksetData` is on) is serialized via XML2 and round-trips through IARs.
- Authentication uses the supplied user password.
- Assets flow through WHIP/asset cache; ensure asset service is reachable.

Manual smoke checklist
1) Save a small folder: `save iar Test User Objects/Test password test.iar` (expect `/bin/test.iar`).
2) Load into another account with merge: `load iar --merge Other User Objects password test.iar` (verify contents appear, no duplicate root folders).
3) Rez/click loaded items: textures/scripts/notecards present; linkset data intact if applicable.
4) Optional: host `test.iar` at an HTTP URL and `load iar --merge Other User Objects password http://.../test.iar` to confirm remote fetch.
