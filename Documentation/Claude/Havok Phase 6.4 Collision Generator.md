# Phase 6.4: the Collision Generator on one implementation

Part of the [Unified Havok Library Plan](Unified%20Havok%20Library%20Plan.md), Phase 6 step 4. Follows [6.3](Havok%20Phase%206.3%20Havok%20Editor.md).

Status: **done 2026-11-06: checked with game files, and in the app (the user confirmed every item in "To check in the app").**

## The generator merge (G1)

- **The algorithms moved into the library.** `HavokCompressedMeshBuilder` and `HavokShapeGenerator_Shared` (pure: no Smithbox types) are now in `src/Havok/Havok.Shared/Builders`, namespace `Havok.Builders`.
- **One source for 2018 and 2019.** `src/Havok/Classes/Shared/CollisionShapeBuilder.cs` is compiled into both `Havok.hk2018` and `Havok.hk2019` (each project defines `HK2018`/`HK2019` and links the file), giving `Havok.hk2018.Builders.CollisionShapeBuilder` and `Havok.hk2019.Builders.CollisionShapeBuilder`. They're typed against each SDK's classes, so nothing is set by name. The two SDK differences are `#if` blocks (`m_materialTable` in 2018, `m_isMutable` in 2019).
  - `BuildExternMeshShape`, `BuildCompressedMeshShape`/`ToShape`.
  - `ReplaceMeshShape` returns the swap as `(Apply, Undo)`, which the Collision Generator wraps in a `HavokDelegateAction`. The library has no editor types.
  - Dropped: the unused material palette parameter, `MakeMaterialDescriptor`, `ReplaceExternMeshShape` and `ReplaceAllExternMeshShapes` (no callers).
- **Archived** to `.archive/Havok Phase 6.4`: `HavokShapeGenerator_HKX3`, `_HKX3_2019`, `HavokCompressedMeshGenerator_HKX3`, `_HKX3_2019`, and the old copies of the two algorithm files.
- **DS3 stays its own typed copy** (`HavokShapeGenerator_HKX2`, `HavokCompressedMeshGenerator_HKX2`), as decided: its 2014 compressed mesh is laid out differently. HKX2's unused tree encoder in it is untouched.
- **Adding a class set** to the shared builder means linking the file and adding a symbol. The source shows what the SDK must have.

### Verified

- **`Tools/CollisionBuilderEquivalence 300`** (new): builds 300 meshes with the archived copies and with the shared builder, and compares the object graphs field by field, floats by bits. The meshes run from 2 to ~6,000 triangles, half with per-triangle materials. Each is built as a 2018 and a 2019 extern mesh and compressed mesh: **1,200 / 1,200 shapes equal.**
- **`CollisionSurvey <GAME> <map> 0 h . rebuild`** (decode every vanilla compressed mesh, rebuild it, write the file through `HavokFile`, re-read, check every triangle, tree and key): ER `m10_00_00_00` **117/117**, NR `m60_44_36_00` **6/6**, DS3 `m30_00_00_00` 87/93 (unchanged; the 6 are empty vanilla meshes).

### To check in the app

1. **Collision Generator, ER:** replace a map collision's shape with a compressed mesh (with a material), then an extern mesh. Both draw, undo puts the old shape back, and save and reload keeps the new one.
2. **The same in AC6 and DS3.**
3. **Map Editor:** navmeshes still draw in DS3, ER and AC6 (the navmesh resource lost its builder fallback).

## Done so far

- **NavmeshBuilder removed** (user's decision: old and unused). `NavmeshBuilderTool` was constructed by `MapEditorView` but never drawn, and on a successful build it would have thrown on a `readonly` null preview mesh.
  - `NavmeshBuilderTool.cs` and its P/Invoke wrapper `Utilities/Havok/NavGen.cs` are in `.archive/Havok Phase 6.4`.
  - `HavokNavmeshResource` loses `ResourceFromNavmeshRoot` and the HKX2 view adapters it kept for the builder's output.
  - `NavGen.dll` stays in `Smithbox.Data`'s content: HKX2's and `Havok.hk2014`'s `hkaiNavMeshBuilder` still declare it. It can go with HKX2 in Phase 7.
- **`Tools/CollisionSurvey` fixed.** It compiles the generators' source from Smithbox, which 6.3a moved to the generated classes, so it had stopped building. It now reads and writes through `HavokFile`, as the Havok Editor does. HKLib and HKX2 are no longer referenced.
  - **Rebuild check, before (HKX2/HKLib) → after:** ER `m10_00_00_00` h: 117/117 meshes rebuild cleanly. DS3 `m30_00_00_00` h: 87/93, the same 6 failing in both versions. Those are vanilla meshes with no triangles ("needs at least one triangle"). The AC6 survey reads `m03_40_00_00`.

## The generators today

| File | Classes | Lines | What it builds |
|---|---|---|---|
| `HavokShapeGenerator_HKX3` | hk2018 | 336 | extern mesh (with a material palette), BVH trees, replace action |
| `HavokShapeGenerator_HKX3_2019` | hk2019 | 250 | the same, without a material table (2019 has none), with `m_isMutable` |
| `HavokShapeGenerator_HKX2` | hk2014 | 608 | the same for DS3, plus an unused aabbTree/simdTree encoder (its calls are commented out) |
| `HavokCompressedMeshGenerator_HKX3` / `_HKX3_2019` | hk2018 / hk2019 | 204 / 203 | fills a `HavokCompressedMeshBuilder` build; the two differ by `m_materialTable = null` |
| `HavokCompressedMeshGenerator_HKX2` | hk2014 | 246 | the same for DS3's 2014 layout (Axis5 tree nodes, packed section words) |

The algorithms are already shared: `HavokCompressedMeshBuilder` (830 lines) and `HavokShapeGenerator_Shared`. What's left in triplicate is filling each SDK's classes. The hk2018 and hk2019 copies are line for line the same apart from those two fields.

## Decisions

- **G1. How to merge the generators.** **Decided (2026-11-06): merge 2018 and 2019 only**, typed. hk2014 (DS3) keeps its own copy.
