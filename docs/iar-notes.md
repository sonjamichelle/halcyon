# Inventory Archives (IAR) on Halcyon

Status: IAR support restored (save/load via region console), using the OpenSim archiver pipeline. Archives default to `user-inventory.iar` in `/bin` unless a path/URL is provided.

Commands (region console)
- `save iar [-h|--home=<url>] [--noassets | --skipbadassets] [--perm=<CTM>] <first> <last> <inventory path> <password> [<IAR path>]`
  - `<inventory path>` can be a folder or item; `/*` or `/` saves entire inventory (use `--merge` on load).
  - `--noassets` excludes asset blobs; `--skipbadassets` skips items with missing main assets.
  - `--perm=<CTM>` require Copy/Transfer/Modify to export.
- `load iar [-m|--merge] <first> <last> <inventory path> <password> [<IAR path>]`
  - `--merge` reuses existing folders when possible (avoid duplicating root folders when restoring full backups).
  - `<IAR path>` can be a local file or HTTP URL.

Notes
- Archives include assets via the configured asset service (WHIP).
- Authentication uses the supplied user password.
- Linkset data is serialized with objects (XML2) and round-trips through IARs.
- Default archive location is `/bin` if no filename is given.
