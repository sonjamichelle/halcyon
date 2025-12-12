Below is:

1. A **ROADMAP file** you can literally save as `docs/ROADMAP_PhysX_Migration.md`
2. A **migration matrix** sketch for Codex to expand
3. A **proposed project layout**
4. Skeletons for the **abstraction layer** and **new bindings layer**
5. An **incremental migration strategy** that keeps Halcyon running while you do surgery

I am treating PhysX 5 as the real target, with room to support 4.1 if you want an interim stepping stone.

---

## 1. Drop-in roadmap file

Save this as `docs/ROADMAP_PhysX_Migration.md` in your Halcyon fork (or VergeGrid variant).

````markdown
# Halcyon PhysX Migration Roadmap

Goal:
Break Halcyon physics out of the Windows only IslandzVW / StillDesign PhysX.Net C++/CLI wrapper and move to a cross platform PhysX 4.1 / 5 implementation with a clean C# abstraction layer.

PhysX references:
- StillDesign PhysX.Net 4.1.2 wrapper (C++/CLI, PhysX 4.1.2) :contentReference[oaicite:0]{index=0}
- PhysX 5 SDK and docs from NVIDIA Omniverse :contentReference[oaicite:1]{index=1}
- MagicPhysX: .NET PhysX 5 binding using a C API, supports win, linux, osx in 64 bit :contentReference[oaicite:2]{index=2}

---

## Phase 0 - Recon and baselines

- [ ] Clone and index IslandzVW/PhysX.Net fork and the Halcyon repo.
- [ ] Locate all namespaces and types from PhysX.Net used inside Halcyon, for example:
      - `StillDesign.PhysX.Scene`
      - `StillDesign.PhysX.Actor`
      - `StillDesign.PhysX.Controllers.ControllerManager`
      - Any custom IslandzVW classes inside their PhysX.Net fork.
- [ ] Generate a call graph / usage map (ReSharper, Rider, VSCode extensions) of PhysX entry points used by Halcyon.
- [ ] Capture current behavior:
      - [ ] Run a Windows Halcyon instance with PhysX enabled.
      - [ ] Create test scenes:
            - [ ] Stacks of primitives
            - [ ] Avatars walking, jumping, falling
            - [ ] Vehicles or physical attachments (if any)
      - [ ] Record:
            - [ ] Stability (no crashes or leaks in multi hour tests)
            - [ ] Expected collision responses
            - [ ] Any quirks relied on by content (e.g., bounce, friction, step height).

Deliverable:
- `docs/PhysX_Legacy_Usage.md`
- `docs/PhysX_Test_Scenarios.md`

---

## Phase 1 - Introduce a C# abstraction layer

Objective:
Hide PhysX.Net behind Halcyon specific interfaces so you can swap implementations without touching core simulation logic everywhere.

### 1.1 Create a new project

- [ ] Add project `src/Halcyon.Physics.Abstractions/`:
      - .NET Framework / .NET version compatible with Halcyon.
      - Only C# interfaces and plain DTOs. No PhysX references.

Suggested interfaces:
- `IPhysicsEngine`
- `IPhysicsScene`
- `IPhysicsActor`
- `ICharacterController`
- `IShape` (box, sphere, capsule, mesh)
- `IMaterial`
- `IJoint`
- Utility DTOs:
  - `PhysicsVec3`
  - `PhysicsQuat`
  - `PhysicsTransform`
  - `PhysicsFilterData` (collision groups, masks)

### 1.2 Wire legacy PhysX.Net to the abstraction

- [ ] Add `src/Halcyon.Physics.PhysXNetLegacy/` project.
- [ ] Reference both:
      - `Halcyon.Physics.Abstractions`
      - Current IslandzVW PhysX.Net fork.
- [ ] Implement adapters:
      - `PhysXNetLegacyEngine : IPhysicsEngine`
      - `PhysXNetLegacyScene : IPhysicsScene`
      - etc.
- [ ] Replace direct `StillDesign.PhysX` usage in Halcyon with calls through the abstraction interfaces.

Milestone:
- [ ] Halcyon builds and runs on Windows using the abstraction layer, but still calls the original PhysX.Net under the hood.

Deliverable:
- `src/Halcyon.Physics.Abstractions`
- `src/Halcyon.Physics.PhysXNetLegacy`
- `docs/PhysX_Abstraction_Design.md`

---

## Phase 2 - Choose and integrate a modern PhysX backend

Objective:
Pick a modern backend and design the new bridge.

Options:
1. PhysX 4.1.2 via updated C++/CLI `PhysX.Net` from StillDesign (still Windows only, nice for parity) :contentReference[oaicite:3]{index=3}
2. PhysX 5 via:
   - MagicPhysX (.NET binding with a C API, supports Linux and Windows) :contentReference[oaicite:4]{index=4}
   - Your own C ABI wrapper around PhysX 5 C++ core using P/Invoke.

For long term Linux and cross platform, the recommended path is:

- Use **PhysX 5 SDK** from NVIDIA Omniverse as the low level physics core. :contentReference[oaicite:5]{index=5}
- Use a C ABI and P/Invoke, either by:
  - leveraging MagicPhysX directly, or
  - writing your own minimal C wrapper for only the PhysX features Halcyon needs.

### 2.1 Decide scope

- [ ] Decide: PhysX 4.1 interim, PhysX 5 end goal, or go straight to PhysX 5.
- [ ] List features used by Halcyon:
      - Rigid bodies
      - Character controllers
      - Triggers
      - Raycasts
      - Joints (which types)
- [ ] Map those features to PhysX 5 capabilities (docs). :contentReference[oaicite:6]{index=6}

Deliverable:
- `docs/PhysX_Feature_Coverage.md`

---

## Phase 3 - New native bridge design

Objective:
Define a C friendly API that can be called from C# using P/Invoke or via MagicPhysX.

If using MagicPhysX as the basis:
- MagicPhysX already exposes a C style API and cross platform native binaries. You build a thin C# facade on top to match your `IPhysics*` interfaces. :contentReference[oaicite:7]{index=7}

If writing your own wrapper:
- [ ] Create `native/halcyon_physx_bridge/`:
      - CMake or your preferred build system.
      - Link to PhysX 5 SDK.
- [ ] Define a simple C API, for example:
      - Engine:
        - `hp_px_init(...)`
        - `hp_px_shutdown()`
      - Foundation and physics:
        - `hp_px_create_foundation()`
        - `hp_px_release_foundation()`
        - `hp_px_create_physics(...)`
        - `hp_px_release_physics(...)`
      - Scene:
        - `hp_px_create_scene(...)`
        - `hp_px_release_scene(...)`
        - `hp_px_step_scene(sceneHandle, float dt)`
      - Actors / shapes:
        - `hp_px_create_rigid_dynamic(...)`
        - `hp_px_create_rigid_static(...)`
        - `hp_px_add_box_shape(...)`
        - `hp_px_set_actor_pose(...)`
      - Character controllers:
        - `hp_px_create_character_controller(...)`
        - `hp_px_move_character_controller(...)`
      - Raycasts:
        - `hp_px_ray_cast(...)`

Deliverables:
- `native/halcyon_physx_bridge/include/halcyon_physx_bridge.h`
- `native/halcyon_physx_bridge/src/*.cpp`
- `docs/PhysX_Bridge_C_API.md`

---

## Phase 4 - Managed binding layer

Objective:
Expose the native bridge via safe C# code, and adapt it into your abstraction.

### 4.1 P/Invoke layer

- [ ] Add project `src/Halcyon.Physics.PhysX5.NativeInterop/`.
- [ ] Define `static class NativeMethods` with `[DllImport]` signatures for all `hp_px_*` functions.
- [ ] Keep this very thin. No business logic here, only marshaling.

### 4.2 PhysX 5 implementation of abstractions

- [ ] Add `src/Halcyon.Physics.PhysX5/`.
- [ ] Implement:
      - `PhysX5Engine : IPhysicsEngine`
      - `PhysX5Scene : IPhysicsScene`
      - `PhysX5Actor : IPhysicsActor`
      - etc.
- [ ] PhysX5 classes call into `NativeMethods` and convert between:
      - `PhysicsVec3` and PhysX vectors
      - `PhysicsTransform` and PhysX transforms
      - collision groups, masks and filter data

Deliverables:
- `src/Halcyon.Physics.PhysX5.NativeInterop`
- `src/Halcyon.Physics.PhysX5`
- `docs/PhysX5_Interop_Design.md`

---

## Phase 5 - Incremental migration inside Halcyon

Objective:
Switch Halcyon to use the new abstraction and backends without breaking everything all at once.

### 5.1 Backend selection

- [ ] Add a configuration option, for example in `Halcyon.ini`:

```ini
[Physics]
EngineBackend = PhysXNetLegacy   ; or PhysX5
````

* [ ] Implement a small factory in a new `Halcyon.Physics.Bootstrap` project:

```csharp
public static class PhysicsEngineFactory
{
    public static IPhysicsEngine CreateFromConfig(IConfigSource config)
    {
        var backend = config.GetString("Physics", "EngineBackend", "PhysXNetLegacy");

        switch (backend)
        {
            case "PhysX5":
                return new PhysX5Engine(/* deps */);
            case "PhysXNetLegacy":
            default:
                return new PhysXNetLegacyEngine(/* deps */);
        }
    }
}
```

### 5.2 Swap call sites gradually

* [ ] Locate all places in Halcyon where:
  - PhysX.Net types are directly referenced
  - static methods from PhysX.Net are called
* [ ] Replace them with:
  - `IPhysicsEngine` and `IPhysicsScene` calls
  - instances from `PhysicsEngineFactory`

Strategy:

* [ ] For each subsystem (avatars, prim physics, vehicles), do:
  - [ ] Wrap raw PhysX.Net usage behind new interface methods.
  - [ ] Add tests where possible.
  - [ ] Once wrapped, you can flip the backend in config and compare behavior.

Deliverables:

* `src/Halcyon.Physics.Bootstrap`
* Updated configuration and docs.

---

## Phase 6 - Testing and parity

Objective:
Ensure the new physics behaves acceptably compared to the legacy PhysX.Net based implementation.

* [ ] Create automated regression tests for:
  - [ ] Basic gravity and falling
  - [ ] Simple stacking and stability
  - [ ] Walking up small steps and slopes
  - [ ] Colliding with static geometry, terrain
  - [ ] Triggers and sensors (e.g., region entry, script triggers)
* [ ] Add at least one long running soak test:
  - [ ] Single region running for 24 hours with scripted activity.
* [ ] Compare:
  - [ ] Performance: CPU, memory
  - [ ] Stability: crashes, leaks
  - [ ] Behavior: content still works, avatar movement still feels right.

Deliverables:

* `tests/Halcyon.Physics.RegressionTests`
* `docs/PhysX_Migration_Test_Report.md`

---

## Phase 7 - Cleanup and legacy removal

Once PhysX5 backend is stable:

* [ ] Deprecate PhysX.Net usage in docs.
* [ ] Mark `Halcyon.Physics.PhysXNetLegacy` as legacy only.
* [ ] Eventually remove:
  - [ ] IslandzVW PhysX.Net fork from the active solution
  - [ ] Any Windows only assumptions around C++/CLI runtime DLLs.

Deliverables:

* Updated docs and release notes.

````

That gives you a structured, checkable plan that Codex can follow task by task.

---

## 2. Migration matrix skeleton

Save this as `docs/PhysX_Migration_Matrix.md`. Codex can flesh it out once you feed it the actual usages from your fork.

```markdown
# PhysX Migration Matrix

This file maps existing PhysX.Net types and usages to the new abstraction interfaces and then to the modern PhysX 4.1 / 5 implementation.

| Layer                    | Legacy PhysX.Net (Islandz / StillDesign)            | Abstraction Interface         | New PhysX 5 implementation example             |
|--------------------------|-----------------------------------------------------|-------------------------------|-----------------------------------------------|
| Engine                   | `StillDesign.PhysX.Physics`                         | `IPhysicsEngine`             | `PhysX5Engine` wrapping `hp_px_init` etc.     |
| Scene                    | `StillDesign.PhysX.Scene`                           | `IPhysicsScene`              | `PhysX5Scene` mapping to `PxScene*`           |
| Rigid Dynamic Actor      | `StillDesign.PhysX.RigidDynamicActor`               | `IPhysicsActor`              | `PhysX5Actor` wrapping `PxRigidDynamic*`      |
| Rigid Static Actor       | `StillDesign.PhysX.RigidStaticActor`                | `IPhysicsActor`              | `PhysX5Actor` wrapping `PxRigidStatic*`       |
| Box Shape                | `StillDesign.PhysX.BoxShapeDescription` or similar  | `IShape`                     | `PhysX5Shape` wrapping `PxShape*` box         |
| Sphere Shape             | `StillDesign.PhysX.SphereShapeDescription`          | `IShape`                     | `PhysX5Shape` wrapping `PxShape*` sphere      |
| Capsule Shape            | `StillDesign.PhysX.CapsuleShapeDescription`         | `IShape`                     | `PhysX5Shape` wrapping `PxShape*` capsule     |
| Material                 | `StillDesign.PhysX.Material`                        | `IMaterial`                  | `PhysX5Material` wrapping `PxMaterial*`       |
| Character Controller     | `StillDesign.PhysX.Controllers.Controller`          | `ICharacterController`       | `PhysX5CharacterController` (PxController*)   |
| Controller Manager       | `StillDesign.PhysX.Controllers.ControllerManager`   | handled inside engine        | Internal to `PhysX5Engine` / `PhysX5Scene`    |
| Raycast                  | `Scene.RayCast(...)`                                | `IPhysicsScene.Raycast()`    | `hp_px_ray_cast` / `PxScene::raycast`        |
| Joints                   | `StillDesign.PhysX.Joint` subclasses                | `IJoint`                     | `PhysX5Joint` wrapping `PxJoint*`             |
| Filter Data / Groups     | `Shape.GroupsMask`, `Shape.Group`                   | `PhysicsFilterData`          | Custom mapping to `PxFilterData`              |
| Simulation Step          | `Scene.Simulate(dt); Scene.FetchResults(true);`     | `IPhysicsScene.Step(dt)`     | `hp_px_step_scene(scene, dt)`                 |

Action items:
- [ ] For each row, fill in:
      - concrete Halcyon usage
      - any behavior quirks (e.g. restitution, friction magic values)
      - PhysX 5 equivalents (PxRigidDynamic, PxScene, etc. from the SDK docs). :contentReference[oaicite:8]{index=8}
````

---

## 3. Suggested project layout

You can create this structure and then let Codex fill things in file by file.

```text
/halcyon-root
  /docs
    ROADMAP_PhysX_Migration.md
    PhysX_Legacy_Usage.md
    PhysX_Test_Scenarios.md
    PhysX_Abstraction_Design.md
    PhysX_Bridge_C_API.md
    PhysX5_Interop_Design.md
    PhysX_Migration_Matrix.md
  /native
    /halcyon_physx_bridge
      CMakeLists.txt
      include/halcyon_physx_bridge.h
      src/halcyon_physx_bridge.cpp
  /src
    /Halcyon.Physics.Abstractions
      IPhysicsEngine.cs
      IPhysicsScene.cs
      ...
    /Halcyon.Physics.PhysXNetLegacy
      PhysXNetLegacyEngine.cs
      PhysXNetLegacyScene.cs
      ...
    /Halcyon.Physics.PhysX5.NativeInterop
      NativeMethods.cs
    /Halcyon.Physics.PhysX5
      PhysX5Engine.cs
      PhysX5Scene.cs
      ...
    /Halcyon.Physics.Bootstrap
      PhysicsEngineFactory.cs
  /tests
    /Halcyon.Physics.RegressionTests
```

---

## 4. Abstraction layer skeletons

These are minimal skeletons you can drop into `Halcyon.Physics.Abstractions` and let Codex auto fill details.

```csharp
// src/Halcyon.Physics.Abstractions/PhysicsTypes.cs
namespace Halcyon.Physics
{
    public struct PhysicsVec3
    {
        public float X;
        public float Y;
        public float Z;

        public PhysicsVec3(float x, float y, float z)
        {
            X = x; Y = y; Z = z;
        }
    }

    public struct PhysicsQuat
    {
        public float X;
        public float Y;
        public float Z;
        public float W;

        public PhysicsQuat(float x, float y, float z, float w)
        {
            X = x; Y = y; Z = z; W = w;
        }
    }

    public struct PhysicsTransform
    {
        public PhysicsVec3 Position;
        public PhysicsQuat Rotation;

        public PhysicsTransform(PhysicsVec3 pos, PhysicsQuat rot)
        {
            Position = pos;
            Rotation = rot;
        }
    }

    public struct PhysicsFilterData
    {
        public uint Group;
        public uint Mask;
    }
}
```

```csharp
// src/Halcyon.Physics.Abstractions/IPhysicsEngine.cs
namespace Halcyon.Physics
{
    public interface IPhysicsEngine
    {
        IPhysicsScene CreateScene(PhysicsEngineSceneDesc desc);
        void Shutdown();
    }

    public class PhysicsEngineSceneDesc
    {
        public PhysicsVec3 Gravity;
        // extend as needed
    }
}
```

```csharp
// src/Halcyon.Physics.Abstractions/IPhysicsScene.cs
namespace Halcyon.Physics
{
    public interface IPhysicsScene
    {
        IPhysicsActor CreateRigidDynamic(PhysicsTransform transform, IShape shape, IMaterial material, float density);
        IPhysicsActor CreateRigidStatic(PhysicsTransform transform, IShape shape, IMaterial material);

        ICharacterController CreateCharacterController(CharacterControllerDesc desc);

        void Step(float deltaTime);

        bool Raycast(PhysicsVec3 origin, PhysicsVec3 direction, float distance, out RaycastHit hit);
    }

    public struct RaycastHit
    {
        public PhysicsVec3 Position;
        public PhysicsVec3 Normal;
        public float Distance;
        public IPhysicsActor Actor;
    }

    public class CharacterControllerDesc
    {
        public float Height;
        public float Radius;
        public PhysicsVec3 Position;
        // etc
    }
}
```

```csharp
// src/Halcyon.Physics.Abstractions/IPhysicsActor.cs
namespace Halcyon.Physics
{
    public interface IPhysicsActor
    {
        void SetTransform(PhysicsTransform transform);
        PhysicsTransform GetTransform();

        void AddForce(PhysicsVec3 force);
        void AddImpulse(PhysicsVec3 impulse);

        void SetKinematic(bool isKinematic);
        void SetUserData(object userData);
        object? GetUserData();
    }
}
```

You can keep going with `IMaterial`, `IShape`, `ICharacterController`, `IJoint` in the same style.

---

## 5. Native bridge and interop skeletons

### C header skeleton

`native/halcyon_physx_bridge/include/halcyon_physx_bridge.h`:

```c
#pragma once

#ifdef _WIN32
  #ifdef HP_PHYSX_EXPORTS
    #define HP_API __declspec(dllexport)
  #else
    #define HP_API __declspec(dllimport)
  #endif
#else
  #define HP_API
#endif

#ifdef __cplusplus
extern "C" {
#endif

typedef void* HpFoundationHandle;
typedef void* HpPhysicsHandle;
typedef void* HpSceneHandle;
typedef void* HpActorHandle;
typedef void* HpMaterialHandle;
typedef void* HpControllerHandle;

typedef struct HpVec3
{
    float x, y, z;
} HpVec3;

typedef struct HpQuat
{
    float x, y, z, w;
} HpQuat;

typedef struct HpTransform
{
    HpVec3 position;
    HpQuat rotation;
} HpTransform;

HP_API int hp_px_init();
HP_API void hp_px_shutdown();

HP_API HpFoundationHandle hp_px_create_foundation();
HP_API void hp_px_release_foundation(HpFoundationHandle foundation);

HP_API HpPhysicsHandle hp_px_create_physics(HpFoundationHandle foundation);
HP_API void hp_px_release_physics(HpPhysicsHandle physics);

HP_API HpSceneHandle hp_px_create_scene(HpPhysicsHandle physics, HpVec3 gravity);
HP_API void hp_px_release_scene(HpSceneHandle scene);
HP_API void hp_px_step_scene(HpSceneHandle scene, float deltaTime);

HP_API HpActorHandle hp_px_create_rigid_dynamic(HpSceneHandle scene, HpTransform transform);
HP_API HpActorHandle hp_px_create_rigid_static(HpSceneHandle scene, HpTransform transform);

HP_API void hp_px_add_box_shape(HpActorHandle actor, float hx, float hy, float hz, HpMaterialHandle material);
HP_API void hp_px_set_actor_transform(HpActorHandle actor, HpTransform transform);
HP_API void hp_px_get_actor_transform(HpActorHandle actor, HpTransform* outTransform);

#ifdef __cplusplus
}
#endif
```

You can later let Codex fill in the PhysX 5 internals using the PhysX 5 docs and samples. ([NVIDIA Omniverse][1])

### P/Invoke skeleton

`src/Halcyon.Physics.PhysX5.NativeInterop/NativeMethods.cs`:

```csharp
using System;
using System.Runtime.InteropServices;

namespace Halcyon.Physics.PhysX5.NativeInterop
{
    internal static class NativeMethods
    {
        private const string DllName = "halcyon_physx_bridge";

        [StructLayout(LayoutKind.Sequential)]
        internal struct HpVec3
        {
            public float x, y, z;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct HpQuat
        {
            public float x, y, z, w;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct HpTransform
        {
            public HpVec3 position;
            public HpQuat rotation;
        }

        internal struct HpHandle
        {
            public IntPtr Ptr;
        }

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int hp_px_init();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void hp_px_shutdown();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern HpHandle hp_px_create_foundation();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void hp_px_release_foundation(HpHandle foundation);

        // etc: add the rest of the C functions as needed
    }
}
```

Then `PhysX5Engine` translates between `PhysicsVec3` and `HpVec3`, and so on.

---

## 6. Incremental migration strategy in plain steps

This is the “do this in this order” list you can literally paste as comments in the roadmap or a `TODO_PhysX.txt` for Codex.

1. **Create abstraction project**

   * Add `Halcyon.Physics.Abstractions` and define the interfaces and value types.
2. **Implement PhysXNetLegacy adapter**

   * Create `Halcyon.Physics.PhysXNetLegacy` and wrap the IslandzVW PhysX.Net calls to the new interfaces.
   * Change Halcyon to go through `IPhysicsEngine` instead of directly hitting `StillDesign.PhysX`.
3. **Add backend factory + config setting**

   * `PhysicsEngineFactory` and `EngineBackend = PhysXNetLegacy` as default.
4. **Create native bridge skeleton**

   * Add `halcyon_physx_bridge` project with the header and empty implementations that just return null / error.
5. **Add P/Invoke layer**

   * `Halcyon.Physics.PhysX5.NativeInterop` with `NativeMethods` wired to the bridge.
6. **Add PhysX5 backend project**

   * `Halcyon.Physics.PhysX5` implementing `IPhysicsEngine` etc., even if methods initially throw `NotImplementedException`.
7. **Wire up PhysX 5 step by step**

   * Implement foundation and physics creation.
   * Implement scene creation and stepping.
   * Implement basic rigid actors and a simple box shape.
   * Implement character controller or avatar collisions next.
8. **Run dual backend tests**

   * Start Halcyon on Windows with `EngineBackend = PhysXNetLegacy` and record behavior.
   * Flip to `EngineBackend = PhysX5` and compare behavior region by region.
9. **Stabilize, then think about Linux deployment**

   * Once Windows with PhysX5 is stable, build and deploy the bridge for Linux.
   * Switch your Linux Halcyon builds to use `EngineBackend = PhysX5`.
