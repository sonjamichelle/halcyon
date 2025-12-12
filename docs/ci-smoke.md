# CI/Local Smoke Guidance

Goals:
- Catch basic regressions (build + inventory/notecard handling) before pushing.

Scripts:
- `scripts/local-smoke.ps1`: runs `msbuild` on `Halcyon.sln` (default `Configuration=Release`). Exits non-zero on failure.
- `docs/inventory-smoke.md`: manual viewer checklist for Firestorm/Halcyon inventory sanity (folders/items, links, notecards).

Suggested local run:
1) `powershell -ExecutionPolicy Bypass -File scripts/local-smoke.ps1`
2) If build succeeds, run through `docs/inventory-smoke.md` on a test account.

Future CI hook (optional):
- Add a CI job to invoke `scripts/local-smoke.ps1` on Windows runners (MSBuild installed) to gate PRs.
