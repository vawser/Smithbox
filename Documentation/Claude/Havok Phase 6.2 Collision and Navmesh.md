# Phase 6.2: map collision and navmesh rendering on the unified Havok library

Part of the [Unified Havok Library Plan](Unified%20Havok%20Library%20Plan.md), Phase 6 step 2. Follows [6.1](Havok%20Phase%206.1%20Renderer%20Animation.md).

## Scope (decided 2026-11-06: rendering only)

**In:** the read-only geometry paths.
- `Renderer/Resource Types/HavokCollisionResource.cs` and `HavokNavmeshResource.cs`.
- The four mesh builders `HKX_Helper`, `HKX2_Helper`, `HKLib_Helper` and `HKLib2019_Helper` (`Editors/Map Editor/Utils`).
- `WallBank`'s wall geometry.
- `HavokCollisionBank`'s Sekiro tagfile cache (HKX2).

**Out, and why:**
- The ER/AC6 containers in `HavokCollisionBank` and `HavokNavmeshBank` stay on HKLib for now. `MapHavokPropertyView` edits them and the banks save them, so they move with the Havok Editor in 6.3.
- DS2 (original) stays on SoulsFormats `HKX`. It's `hk_2012` 32-bit, which nothing in the unified library reads.
- `HavokNavmeshResource.ResourceFromNavmeshRoot` keeps an HKX2 overload for the DS3 navmesh builder's output until the builders move (6.4).

## Today

| Game | Collision read by | Mesh built by | Navmesh read by |
|---|---|---|---|
| DeS, DS1, DS2S | SoulsFormats `HKX` | `HKX_Helper` (hkp mesh storage) | — (NVM) |
| DS1R | SoulsFormats `HKX`: only works with the "use PTDE collision" option, because the DS1R tagfiles don't read | `HKX_Helper` | — |
| BB | SoulsFormats `HKX` (BB variation) | `HKX_Helper` (hknp compressed meshes) | HKX2 packfile |
| DS3 | HKX2 packfile | `HKX2_Helper` | HKX2 packfile |
| SDT | HKX2 tagfile (bank cache + compendium) | `HKX2_Helper` | — |
| ER, NR, DB, DB_NT | HKLib 2018 (+ binder compendium) | `HKLib_Helper` | HKLib 2018 |
| AC6 | HKLib 2019 (+ binder compendium) | `HKLib2019_Helper` | HKLib 2019 (mesh, volume, graph) |

The builders differ in what they handle:
- **ER:** throws on an unresolved vertex, and has no compound shapes.
- **AC6:** drops a primitive whose vertex can't be resolved, merges compound shapes, and treats a zero rotation as identity.
- **DS3/SDT:** only `fsnpCustomParamCompressedMeshShape`, not the base `hknpCompressedMeshShape`.
- **BB:** a loop bug draws each compressed mesh several times, without its body transform.

## Design (shared interfaces, as in 6.1)

**`Havok.Shared/Physics.cs`**, data views implemented in each class set's `Manual/Physics.cs`:

| View | Classes | SDKs |
|---|---|---|
| `IHavokPhysicsScene` / `IHavokPhysicsSystem` (bodies) | `hknpPhysicsSceneData`, `hknpPhysicsSystemData` | 2014, 2016, 2018, 2019 |
| `IHavokBody` (shape, position, orientation) | `hknpBodyCinfo` | same |
| `IHavokCompressedMesh` (normalised sections, primitives, packed/shared vertices, domain, bits per key) | `hknpCompressedMeshShape` | same |
| `IHavokTriangleMesh` (vertices, triangles) | `hknpExternMeshShape` (default geometry) | same |
| `IHavokConvexPolygons` (vertices, faces, indices) | `hknpConvexPolytopeShape` (2014–2018), `hknpConvexShape`'s hull (2019) | same |
| `IHavokCompoundShape` (instances with transforms) | `hknpCompoundShape` | 2014, 2018, 2019 |
| `IHavokMeshStorageShape` (its subpart storages as triangle meshes) | `hkpStorageExtendedMeshShape` | 5.5, 2010, 2014, 2015 |
| `IHavokNavMesh`, `IHavokDirectedGraph` | `hkaiNavMesh`, `hkaiDirectedGraphExplicitCost` | 2014, 2016, 2018, 2019 |

**`Havok.Shared/CollisionGeometry.cs`**: one algorithm for every SDK.
- Bodies' world transforms.
- Shapes to triangles: compound recursion, compressed mesh decode (packed 11/11/10-bit and shared 21/21/22-bit vertices; quads split into abc + acd), triangle meshes, convex fans.
- Wall shape keys.

The union of the old builders' behaviour:
- Every shape kind.
- A primitive with an unresolved vertex is dropped.
- A zero rotation is identity.
- Bodies come from the physics scene, or from loose physics systems (BB).
- hkp files get one untransformed submesh per mesh storage, as before.

**Smithbox:**
- `HavokCollisionResource` and `HavokNavmeshResource` hold a `HavokDocument` read by `HavokFile.Read`.
- `HavokCompendiumSource` also parses the binder's compendium for the unified library (lazily, once).
- One `HavokCollisionMeshBuilder` replaces the four helpers' mesh paths.
- The navmesh mesh and graph builders take the two views. AC6's nav volume reads `Havok.hk2019.hkaiNavVolume` directly, since only 2019 has one.
- `HavokCollisionBank`'s Sekiro cache holds unified documents. `WallBank` builds walls from them.

## Verification

- **`Tools/CollisionReadCompare`**: the old builders' geometry code against the shared one, per file, over a sample of each game's map collision, navmesh and object/asset collision. ER goes through the VFS.
  - Triangles are compared per submesh, after sorting submeshes into a canonical order.
  - It reports exact matches, matches within 1e-4 (a different float formula: HKLib's 2018 decode went through `double`), and differences.
  - Files where the old path failed or misbehaved (BB duplicates, DS1R tagfiles) are counted separately.
- Smithbox builds.
- In the app: map collision (high/low/fall), connect collision, asset collision and navmesh in each game. Sekiro walls.
