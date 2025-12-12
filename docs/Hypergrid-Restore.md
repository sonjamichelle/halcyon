# Hypergrid Restore Plan (Halcyon)

Reference source: OpenSim 0.9.3.0 (`D:\src\opensim-vergegrd`).

Goal: re-enable Hypergrid with a default-off config gate.

Phases
1) Config gate
   - Add `EnableHypergrid = false` (Startup/User) and keep legacy behavior when off.
2) Core HG services (from OpenSim HG modules)
   - Gatekeeper/Login: HG agent login/session validation, seed caps, home URL parsing.
   - HG grid/user info services: remote profile, region lookups.
   - Hyperlink routing: implement `IHyperlink` with HG region handles, inject into `CreateCommsManager`.
   - Map search: allow HG destinations; respect `EnableHypergrid`.
3) Teleport/auth glue
   - HG TP in/out (agent circuits, session/auth tokens, home/remote caps).
   - Attachments/wearables handling across HG.
4) Inventory/asset flow
   - HG-aware inventory fetch caps; enforce perms on export/import.
   - Asset delivery via WHIP: restrict to HG-permitted fetches; ensure asset redirects work.
5) Caps & viewer compatibility
   - Seed caps for HG agents, HG-specific caps where required.
   - Test with current Firestorm HG viewer.
6) Docs & samples
   - Add HG config sample sections with required endpoints.
   - Usage doc: enabling HG, known limitations.
7) Smoke
   - TP outbound to remote HG region and back; inventory fetch; rez object; script/notecard edit; map search.

Key OpenSim files to consult (0.9.3.0)
- `OpenSim/Server/Handlers/Hypergrid/*` (gatekeeper/login handlers)
- `OpenSim/Region/CoreModules/ServiceConnectors/Hypergrid/*` (HG service connectors)
- `OpenSim/Region/CoreModules/ServiceConnectors/Interregion/RESTInterregionComms.cs` (hyperlink detection)
- `OpenSim/Services/HypergridService/*` (HG-specific services)
- `OpenSim/Framework/Communications/Hypergrid` (if present; hyperlink region logic)
- `OpenSim/Framework/Services/*` HG helpers

Halcyon integration notes
- WHIP asset service must filter HG export/import per perms.
- Keep HG off by default; enable only when endpoints configured.
- Ensure new config does not break standalone/non-HG grids.
