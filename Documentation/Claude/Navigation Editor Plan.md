# Navigation Editor Plan (Elden Ring)

2026-11-07. This is an analysis of every ER navigation format, checked against the game files (via the VFS) and against
`Documentation/Games/eldenring.exe`, followed by a phased plan for a navigation editor.
The companion format write-up for NVC and ONAV is `NVC and ONAV Formats.md`.

All numbers below come from `Tools/NavSurvey`, run from `src/Smithbox.Data` with `MSYS_NO_PATHCONV=1`:

| Mode | What it checks |
|---|---|
| `mapnav [regex] [max]` | NVA against its nvmhktbnd HKX files, per map and across maps |
| `navrows <map>` | each NVA navmesh with its NameID and the files its ModelID names |
| `navvalues <regex> <max>` | face and edge data values of the `n`/`o` meshes |
| `validate [regex] [max] [verbose]` | the Map Editor's NavigationMap and NavigationValidator on every map, a remap round trip, and a self test |
| `overlay [regex] [max]` | the Navigation tool's overlay geometry and face picking on game maps |
| `names [regex]` | the navmesh file name the Map Editor's drawing asks for, against the one the binder has |
| `havokedit [regex] [max]` | face data, edge data and edge flag edits survive HavokFile.Write, with nothing else changed |
| `useredges [regex] [max]`, `cachedump <map> <n>` | how the game lays out user edges, their vertices, pairs and clearance cache entries |
| `linkedit [regex] [max]` | adding and removing links (NavUserEdges) on every navmesh file: layout check, write round trip, byte-identical undo |
| `stitchsurvey`, `stitch [regex] [max]` | how the game's NVA and NVC connections fit their edges; the matcher's recall and extras over a grid of parameters |
| `stitchedit [regex] [max] [all]` | re-stitching (NavStitchPlanner) on every map: the game data, after damage, and a byte-identical restore |
| `roundtrip`, `check` | NVC/ONAV round trip; NVC against the NVAs |
| `exe <exe> magic/str/strings/calls/fn/win/jt/disp` | exe scans |

---

## 1. The file family

Per map `mAA_BB_CC_DD`:

| File | Count | Role | SoulsFormats / Havok |
|---|---|---|---|
| `/map/mAA/<map>/<map>.nva.dcx` | 997 | Navmesh instances, in-map connectors, gate nodes (the high-level graph), node banks, map UID | `NVA_ER`, round-trips 997/997 |
| `/map/mAA/<map>/<map>.nvmhktbnd.dcx` | 997 | BND4 of `hkaiNavMesh` HKX files: `n…`, `o…`, `q…_0..3` | Havok.Files (hk2018) |
| `/map/nvc/<mapA>_<mapB>.nvc.dcx` | 3,674 | Navmesh connections between neighbouring maps | `NVC`, round-trips 3,674/3,674 |
| `/map/onav/<map>.onav.dcx` | 997 (57 non-empty) | Occlusion / area-layer graph | `ONAV`, round-trips 997/997 |

Also involved:
- **MSB (MSBE):**
  - `Region.NavmeshCutting` has no type data, so it is shape only.
  - `Event.Navmesh` references a region.
  - `Region.PatrolRoute`, and `Part.Enemy.WalkRouteName`, are AI routes.
  - The `MountJump*` regions.
  - ER has no navmesh parts; navmesh placement lives only in the NVA.
- **Params:**
  - `AssetGeometryParam.navimeshFlag[_after]`: None / Obstacle / Impassable / High Cost. It has `…FlagApply`, and `assetNavimeshNoCombine` is a generate attribute.
  - `NpcParam.chrNavimeshFlag_Alive/Dead`.
  - `NpcThinkParam.enableNaviFlg_*`: Edge, LargeSpace, Ladder, Hole, Door, InSideWall, Lava, Edge_Ordinary.
- **Not navigation**, but often confused with it: `/other/autoinvadepoint*.aipbnd`.

Two maps list nav files that don't exist: `m60_47_42_10` and `m60_42_40_10`.

## 2. Runtime classes (exe RTTI)

| Area | Classes |
|---|---|
| Navmesh | `CSHkAiNvmbndFileCap`, `CSHkAiNvmResCap`, `CSHkAiNvmRepository`, `CSHkAiNaviMesh`, `CSWorldAreaNvm`, `CSWorldNvmManager`, `CSNvmPhysicsListener`, `CSHkAiNaviMeshCostModifierBase`, `CSChrNavimeshCostEffectModule` |
| NVA | `NvaFileCap`, `NvaResCap`, `NvaRepository`, `CSNvaPatchedImage` |
| NVC | `NvcFileCap`, `NvcResCap`, `NvcRepository` |
| ONAV | `OnavFileCap`, `OnavResCap`, `OnavRepository`; readers under `Source\MSP\MSPOcclusionGraph*.cpp`; `DLMOW::detail::MSPLayerPathFinder_AStar` |
| Routing | `CSRouteManager`, `CSRouteNavigatorInterface` (implemented by the route manager and by `CSWorldNvmManagerImp`) |
| MSB | `CSMsbPointOcclusionBox`, `CSMsbPointOcclusionPortal` |
| Dev only | `ModuleNaviMeshTestStep`, `ModuleNaviMeshGenerateTestStep`, `CSHkAiDebugNaviMeshDisp` |

## 3. Findings per format

### 3.1 NVA (`NVA_ER`)

**Exe reader `0x140C8D990`** (in `NvaResCap`):
- It needs magic `NVMA` and version ≥ 7, so ER (8) and AC6 (7) both pass.
- Section types 0–13 are dispatched through a jump table at `0x140C8DC78`. Each case stores a data pointer and a count:

| Type | Stored | Notes |
|---|---|---|
| 0 Navmeshes | +90 / count +10C | Section versions 2, 3 or 4; a flag at +118 is set for 2/3 and cleared for 4. Stride 0x90 |
| 1 FaceData | +98 / +E8 | |
| 2 NodeBanks | +A0 / +EC | |
| 3 Entries3 | +A8 / +F0 | |
| 4 Connectors | +B8 / +F8 | |
| 5 NavMeshConnections | +C0 / +FC | |
| 6 GraphConnections | +C8 / +100 | |
| 7 LevelConnectors | +D0 / +104 | |
| 8 GateNodes | +D8 / +108 | |
| 9 Entries9 | +E0 | only stored when count > 0 |
| 10 Entries10 | **not read here** | |
| 11 map UID | +88 | no count |
| 12 | **not read** | |
| 13 Entries13 | +B0 / +F4 | |

- For each navmesh, `0x140BDA2A0` tests `(NameID / 1,000,000,000) & 1` and counts the hits at +114, which marks open-world navmeshes (see below).
- **Exe writer `0x1401FDA80`** emits `NVMA` version 8 with 14 sections, including an empty section 12 (type 0xC, version 1).
  So the game builds an NVA image itself; this is presumably `CSNvaPatchedImage`.

**Data findings (997 maps, 9,480 navmesh entries):**

- **Every navmesh is listed twice.** The first half of `Navmeshes` uses NameID prefix 0/1 and has the FaceCount of the
  `n` HKX file. The second half mirrors it with the same ModelID, NameID + 2,000,000,000, and the `o` file's FaceCount.
  This holds in 4,722 of 4,740 cases; 12 entries break the half split.
- **NameID is decimal-structured:** `P · 10⁹ + D · 10³ + index`.

  | Map kind | Prefix P | Digits D | Example |
  |---|---|---|---|
  | Legacy dungeons | 0 (n half), 2 (o half) | `AABBCC` | `m10_01_00_00` → 100100000 |
  | Open world m60/m61 | 1 (n half), 3 (o half) | `BBCCDD` | `m60_42_36_00` → 1423600000 |

  The odd prefix is exactly what the game's `/1e9 & 1` test picks out. `index` is the position within the half (9,458 of 9,480).
  m61 reuses m60's digit space, so 1,144 NameIDs repeat across maps. **NameIDs are not globally unique.**
- **Placeholders:** `Unk4C = 1` marks empty navmeshes (0 faces; ModelIDs like 20100, 900000, 423650…). Their HKX files are empty or absent.
  3,640 entries have no `n`/`o` file at all.
- **Constant or near-constant fields:**
  - `Position`, `Scale` and the W values are constant (0, 1, 1/0/0). `Rotation` is nonzero in 176 entries.
  - `Unk3C` is 0. `Unk44` is 2.4, or 2.0 in 18 entries. `Unk50` is 0xFF01 (other values in 52 entries). `Unk54`–`5C` are 0, and the v4 int block is 0.
- `FaceDataIndex` is the running total of FaceCount (9,480/9,480), but the **FaceData section is empty in every ER file**.
  The index addresses a per-face array that is presumably built at runtime (the patched image).
- **Connectors** (5,884) are all within the map, both halves included, with no graph connections.
  Cross-map connections live only in NVC.
- **Other sections:**
  - LevelConnectors are always empty.
  - Entries9 is always one zero entry.
  - Entries11 is the map UID. 45 UIDs are shared by `_00`/`_10` tile pairs.
  - NodeBanks appear in 89 maps; their faces carry a CollisionID.
  - Entries3 (8 × Vector4) appears in 51 maps, Entries10 in 305 (one int plus −1s), Entries13 in 13.
  - Their meanings are open.

### 3.2 nvmhktbnd (Havok `hkaiNavMesh`, hk2018)

- **Contents:** 3,390 `n` + 3,390 `o` files, one pair per ModelID, plus 1,025 sets of `q…_<model>_0..3`.
  - The `q` sets are on open-world tiles. 4,092 of 4,100 have no faces; 8 are real meshes with faceData flags 0x10/0x11.
  - The game mounts the binder generically (`hkxbnd:/%s/%s.hkx`, `0x140705E60`). The `n`/`o`/`q` choice is made by its caller (not traced).
- **Classes in a non-empty file:**
  - `hkaiNavMesh` with Face, Edge and vertices.
  - `hkaiStaticTreeNavMeshQueryMediator` with `hkcdStaticAabbTree` / `Aabb6BytesCodec`.
  - `hkaiNavMeshClearanceCache` with seeding `CacheDataSet` (in 4,676 of 5,840 files).
  - `McpDataInteger`.
  - Sometimes `hkaiUserEdgeSetupArray`.

  Empty files hold only the `hkaiNavMesh`.
- **No streaming sets** and **no `hkaiDirectedGraphExplicitCost`** in any file (0 of 5,840). The engine joins sections from NVA
  connectors (in-map) and NVC (cross-map). The hierarchical graph is NVA's gate nodes.
- **faceData** (striding 1) is a bitfield. Seen bits: 0x1, 0x40, 0x1000, 0x10000, 0x20000, 0x100000, 0x1000000; `n` uses all of them, `o` only the low ones.
  It plausibly corresponds to the `NpcThinkParam.enableNaviFlg_*` / `AssetGeometryParam.navimeshFlag` attributes; the mapping is unverified.
- **Edges** use only these flags:
  - 4 (ORIGINAL).
  - 20 (ORIGINAL|USER).
  - 52 (|BLOCKED).

  `edgeData` (striding 1) is nonzero only on user edges, i.e. jump, ladder and drop links, with values like `0x0X04`, `0x0X14`, `0x02`, `0x12`.
  `userEdgeCost` is always 0. `erosionRadius` is 0, there are no clusters, `flags` is 0, and `userData` is 0 in all but 38 files.
- **n vs o:** they differ in faces and vertices in 77% of models, and their AABBs always differ. `o` usually has fewer faces and fewer user edges.
  My working hypothesis is that these are two agent sizes (normal and large characters; `enableNaviFlg_LargeSpace`). This is **unverified**.
  Smithbox already exposes the `n`/`o` choice as `MapNavmeshEditType`.

### 3.3 NVC and ONAV

See `NVC and ONAV Formats.md`.
- **NVC:** a standalone copy of NVA's map-UID, connector and connection sections for a pair of maps.
  Connector NameIDs resolve into the two NVAs (3,299/3,299), and face indices fall within FaceCount (83,424/83,456).
- **ONAV:** a graph of areas → layers → links, with 4096-bit cell masks over a grid of the map's bounds. Read by the `MSPOcclusionGraph` code,
  and the exe RTTI names an A* layer pathfinder (`MSPLayerPathFinder_AStar`). It is non-empty only in 57 maps, mostly legacy dungeons.

## 4. What Smithbox has today

- `HavokNavmeshBank`:
  - loads NVA (and AC6 NVVA) as Map Editor entities (`MapContainer.LoadNVA`: navmesh instances and level connectors);
  - loads every nvmhktbnd file as a `HavokDocument` and saves both.
- `MapHavokPropertyView`: raw field editing of the `n` or `o` HKX (the `N`/`O` toggle).
- Renderer: `HavokNavmeshResource` draws the hkaiNavMesh (Phase 6.2), with a "Navmesh" render filter.
- `NvaValidator` (dev tool), `NavigationRegion` (DS1 MCP only), and the DS3 Recast builder `Havok.hk2014.Builders.hkaiNavMeshBuilder` (`NavGen.dll`, hk2014 only).
- NVC and ONAV: SoulsFormats only, not loaded by Smithbox.

## 5. Invariants an editor must keep

These follow from the data and are what the validator checks:

1. **NVA halves:** for each model, one entry in the first half (n) and its mirror in the second half (o).
   Same ModelID, NameID + 2·10⁹, and the same index in each half. Placeholder entries have `Unk4C = 1` and FaceCount 0.
2. `Navmesh.FaceCount` = the face count of its `n` (first half) or `o` (second half) file.
   `FaceDataIndex` = the running sum of FaceCount, kept in NVA order.
3. **NameID** = prefix + map digits + index, following the rule in 3.1.
4. **Connector** (NVA, in-map) and **NVC** (cross-map) face/edge indices must point at boundary edges of the right mesh.
   Any edit that renumbers faces or edges must remap them, in this map's NVA and in every NVC that names this map, on either side.
5. **User edges:** they live in the faces' user-edge ranges (`startUserEdgeIndex` / `numUserEdges`) with flags 20/52 and their `edgeData`.
   Inserting one shifts edge indices, which triggers invariant 4.
6. **Derived Havok data:** the query mediator AABB tree, the clearance cache and the HKX AABB must be rebuilt after any geometry change.
   Whether the game tolerates their absence is open. 1,164 files ship without clearance seeding, but those may all be empty meshes; to check.
7. **Gate nodes:** each navmesh's gate nodes (position, neighbour costs) must stay consistent with its faces and connectors after edits.
   How the game uses them is untraced.
8. **NVC map UIDs** = the two NVAs' `Entries11[0].Unk00`. The file name is the source of the packed map IDs.

## 6. Editor plan

The editor is a Map Editor tool window ("Navigation"), built on the existing Tools-window conventions (`Tool UI Styling.md`).
It works on the loaded map plus its neighbours. Each phase is usable on its own and ends with a check by the user in the app.

**Tool layout** (2026-11-07): the old View tab is now four tabs: **Overlay** (overlay and layers), **Face Data**
(picking, the face inspector, painting and the legend), **Links** (adding links, and the picked face's links with Blocked,
edge data and Remove link) and **Stitch**, followed by **Check**. The check steps below that say "View" or "View tab"
mean the matching tab.

### Phase 0: Data layer and validator: DONE 2026-11-07 (not yet checked in the app)

**What was built**

- **Model** (`Editors/Map Editor/Tools/Navigation/NavigationModel.cs`): `NavigationMap`, `NavmeshModel`, `NavmeshFile`, `NvcFile`,
  and `NavigationRemap`, which remaps face/edge indices across NVA connectors and NVC files and drops connections mapped to -1.
- **Reader** (`NavigationReader.cs`): reads a map's navigation data from a file system. It is used for the vanilla baseline,
  and NavSurvey compiles it.
- **Validator** (`NavigationValidator.cs`): 18 issue codes, plus `MarkInherited`, which marks issues the vanilla data also has.
- **Bank** (`Data/NavigationBank.cs`, in `MapData`):
  - builds `NavigationMap` from the live NVA entity and `HavokNavmeshBank`'s documents, via the new `GetMapNavmeshNames`;
  - reads ONAV and NVC on demand, and the vanilla maps in the background;
  - saves NVC/ONAV (new `MapFileType.NVC`/`ONAV`) only after `MarkEdited`/`MarkOnavEdited`, and skips files that are byte-identical.
- **Tool** (`NavigationTool.cs`): Map Editor → Tool window → Map Data → Navigation, ER only.
  It shows a map picker, Validate/Clear, a summary, and an issue table. Inherited issues are hidden unless asked for, and the
  counts and baseline state stay outside the list toggle.
  Settings: `Interface_MapEditor_Tool_Navigation` and a View-menu toggle.
  Text: `Map-Editor-Tools-Navigation.json`.
- **NavSurvey `validate [regex] [max] [verbose]`**: compiles the model, reader and validator, and runs them on every map. It also:
  - checks that an identity remap, and a shift that is then undone, leave the NVA and NVC bytes identical;
  - injects 10 kinds of breakage, all of which must be reported as new issues against the baseline;
  - checks that maps with vanilla issues show none new against themselves.

**Results on the vanilla game** (997 maps, 318,660 connection ends):
- 976 maps have no issues. The remap round trip is byte-identical on all 997, and every self-test case is caught.
- The 854 remaining errors are all in 26 vanilla NVC files that pair an open-world `_00` tile with a `_10` tile.
  Their UID or their face/edge indices match another version of one side (34 files have a UID mismatch).
  The game ships them this way, so they show as inherited.

**Corrections to section 3 found while building it**
- **Quarters:** the `q…_0..3` files are the `n` mesh split into four. When they are used, the `n` file is empty and NVA has
  four more n-set entries after the o half: NameID index = position in the list (12–15 in `m60_49_37_00`), same ModelID,
  and `Unk50` = 0x0001, 0x0101, 0x0201, 0x0301. So the quarter is `Unk50 >> 8`, which is 0xFF for unsplit entries.
- **Sets are found by NameID prefix** (0/1 = n, 2/3 = o), not by position. An o entry is its n entry's NameID + 2·10⁹.
  An n entry's index is its position in the whole list.
- **`_10` name base:** some open-world `_10` tiles name their files and NameIDs after the `_00` tile (`m60_44_36_10` →
  `q60_44_36_00_443600_0`), and others after themselves (`m60_45_36_10`). The model reads the base from the binder's file names.

**For the user to check in the app**
1. Load `m10_01_00_00` (Havok loading on), open Tool window → Navigation, then Validate.
   - Expect 28 navmesh entries for 14 models, and "No new issues".
   - After the baseline finishes, expect 0 inherited.
2. Load `m60_51_36_00` and `m60_52_36_10` together, then Validate `m60_51_36_00`.
   - Expect no new issues, and inherited ones listed under "Show inherited issues".
3. Load `m60_49_37_00`. The summary should show 1 model split into quarters.
4. Saving a map without navigation edits must not write NVC/ONAV files to the project.

### Phase 0 plan (as written before the work)
- **`NavigationBank`** (or an extended `HavokNavmeshBank`): per loaded map it holds the NVA, the n/o/q documents, the ONAV, and every NVC that names the map.
  NVC files can be found from the file dictionary (`/map/nvc/<map>_*` and `*_<map>`). Saving goes through the project FS like the others.
- **Model:**
  - a `NavmeshModel` per ModelID that pairs its two NVA entries with the n and o documents;
  - a resolved `Connection` (mesh A face/edge ↔ mesh B face/edge) that unifies NVA connectors and NVC entries;
  - an edge-index remap service for invariants 4 and 5.
- **Validator:** the invariants of section 5 as a Map Editor action, ported from NavSurvey's `mapnav`/`check`.
  It reports per map and runs before save.
- **Tests:** a NavSurvey `roundtrip-all` that loads through the bank and saves unchanged must produce byte-identical files.

### Phase 1: Viewer: DONE 2026-11-07 (not yet checked in the app)

**What was built**

- **Overlay geometry** (`Tools/Navigation/NavigationOverlay.cs`, renderer-free, linked into NavSurvey):
  - `NavigationOverlayBuilder.Build` turns a map's n or o set into colour batches:
    - faces, fanned into triangles, one batch per faceData value;
    - inner edges and open boundary edges;
    - edges connected by an NVA connector or an NVC file;
    - user edges (the edge plus an arc and arrow to the face it leads to), with blocked ones separate;
    - gate node crosses;
    - ONAV bounds, area positions, area links, and cells.
  - The n set includes the quarter (q) entries.
  - `Pick` does a ray–face test in each navmesh's own space; `Highlight` outlines the picked face.
- **Line primitive** `Renderer/Debug Primitives/DbgPrimLineBatch.cs`: a single-colour line list (up to 16,000 lines), built like
  `DbgPrimWallCells`. The debug wire shader colours only by material, so every colour needs its own primitive.
- **Renderer** `NavigationOverlayRenderer.cs`: unpickable proxies (translucent face fills, line batches) chunked to the 16-bit
  index limit, with a face-data palette and legend. The default `RenderFilter.All` keeps the overlay visible when the Navmesh
  render filter hides the Map Editor's own navmesh drawing.
- **Tool**: the Navigation tool now has **View** and **Check** tabs. View has:
  - Show overlay;
  - the n/o set and face opacity;
  - layer toggles with colour swatches;
  - the face-data legend (values with their set bits);
  - "Pick faces in the viewport", where a click picks instead of selecting (`VulkanViewport` calls `NavigationTool.HandleViewportInput`);
  - the face inspector: navmesh, file, face, faceData, and each edge or user edge with its flags, edgeData and what it leads to
    (a face, an NVA connection, an NVC connection with the neighbour map, or open).
- **NavSurvey `overlay [regex] [max]`**: layer counts, user edges without a target, and a pick test, which casts down onto
  random face centres and must hit that face.

**Results on game maps**
- All 55,676 user edges in the sample point into their own navmesh (packed section 0), so every arc resolves.
- Picking hits the exact face in 98.3% of random tests, and a face at the same point in another 0.8%.
  The misses are near-vertical faces, which a straight-down test ray can't hit.
- Heavy layers: ONAV cells (~900k lines on six maps, off by default), and user edges on large open-world tiles
  (~13 lines per edge; m60_42_36_00 has ~27k user edges).

**Fixed: the Map Editor's own navmesh drawing loaded the wrong files.** `MapContainer.LoadNVA` (ER) loaded the `n` file
named after the map for every NVA entry, which was wrong for 4,962 of 9,480 entries:
- o-half entries drew a second copy of the n mesh;
- quarter entries drew the empty n file;
- `_10` tiles named after their `_00` tile drew nothing.

It now asks for `NavigationMap.FileNameFromEntry`. That takes the set from the NameID prefix and the quarter from `Unk50`, and
the name base from the NameID's own digits, because the drawing doesn't have the binder's file names.
NavSurvey `names` checks it on every map:
- it matches the binder-based name for all 9,480 entries;
- all 4,564 entries with faces find their file, with the same face count.

AC6 keeps the old naming.
Still to do: the Havok property view's N/O toggle (`MapHavokPropertyView`) builds names from the map and ModelID, so it can't
reach `q` files or `_10` tiles named after `_00`.

**For the user to check in the app** (Havok loading on):
1. `m10_01_00_00`: Navigation → View → Show overlay.
   - Faces should sit just above the navmesh, coloured by face data, with the legend listing the values.
   - Cyan edges are where navmeshes join within the map. Red edges are open.
   - Yellow arcs are jump and ladder links.
2. Switch to the o set. It should be coarser, with fewer arcs.
3. Turn on "Pick faces in the viewport" and click a face.
   - A white outline should appear.
   - The Face group should list its edges. A cyan edge should read "connected to face … of navmesh …".
4. `m60_33_41_00` with `m60_33_42_00` loaded: magenta edges are the NVC connections to the neighbour.
   Picking one should name `m60_33_42_00`.
5. `m30_02_00_00`: turn on ONAV cells. If the boxes don't sit over the areas, the bit-order guess is wrong; report what you see.
6. With picking off, clicks must select entities as before.

### Phase 1 plan (as written before the work)
- Draw the n set and the o set with a toggle. Colour faces by faceData bit, with a legend.
- Show user edges (jump, ladder and drop links) as arcs coloured by edgeData type, and blocked edges.
- Show boundary edges, with connected ones highlighted: in-map connectors in one colour, NVC cross-map ones in another (neighbour maps loaded).
- Show gate nodes and their neighbour links, ONAV areas (bounds, cell masks as voxels, links between areas), and MSB `NavmeshCutting` regions.
- Picking a face shows its index, flags, edges, connections, owning navmesh, and NVA entry (both halves).
- **Verification target:** `m10_01_00_00` (28 entries, 14 models), `m60_42_36_00` (open world with `q` files), and the NVC pair `m60_33_41_00` / `m60_33_42_00`.

### Phase 2: Attribute editing: face and user-edge data DONE 2026-11-07 (not yet checked in the app)

**What was built**

- **`Actions/Map Objects/NavigationEditAction.cs`**: an undoable action holding face-data changes (`NavFaceDataChange`) and
  edge flag/data changes (`NavEdgeChange`). It edits the navmesh bank's Havok documents directly, through the same
  `NavmeshFile.Mesh` objects the overlay reads, and marks the map `MapFileType.Navmesh`, so `SaveHavokNavmeshModels` writes it.
  Each apply redraws the overlay.
- **View tab → Paint**: "Paint face data in the viewport" (turns picking off). It has three modes:
  - Set: the face gets exactly the value;
  - Add bits;
  - Remove bits.

  The value is a hex input plus a checkbox for each bit the drawn faces use, and the legend's "Use" button copies a value.
  Dragging paints each face once per stroke. Changes are applied live and recorded as one action on release, as Wall Paint does.
- **Inspector**: the face's data is a hex input. Each user edge has a Blocked checkbox (EDGE_BLOCKED) and an edge-data hex input.
  Every change is one undo.
- The overlay redraws after an edit, undo or redo even while the tool window is closed. The picked face stays picked.
- **NavSurvey `havokedit [regex] [max]`**: flips a face's data bit, an edge's data and its blocked flag, writes the file with
  `HavokFile.Write`, reads it back, and compares the whole mesh.
  - 238 of 238 files keep exactly the edit, with nothing else changed.
  - All 316 unedited files round-trip to the same mesh.

**Not in Phase 2 yet**
- NVA navmesh fields and gate nodes: these are edited as before, through the property panel of the navmesh entities.
- ONAV fields and cell masks: ONAV isn't an entity, so it needs its own editor. `NavigationBank.MarkOnavEdited` and
  `SaveONAV` are ready for that.

**For the user to check in the app** (Havok loading on, on a project copy):
1. `m10_01_00_00`: View → Show overlay. Note a legend value, e.g. `01000000` (n set).
2. Paint: tick "Paint face data", choose Set, and click Use on the `00000000` legend entry. Drag over a few `01000000` faces.
   On release they should turn the no-flags colour, and the legend counts should change.
3. Ctrl+Z should restore them in one step, and redo should repaint them.
4. Untick Paint, tick Pick, and click a face with user edges. Toggle Blocked on one: its arc should turn from yellow to orange,
   and Ctrl+Z should undo it.
5. Save the map. The project should now have `m10_01_00_00.nvmhktbnd.dcx`. Reload the map: the painted faces and the
   blocked edge should still be there, and Check → Validate should report no new issues.
6. In game, not needed yet: what the bits do is still unknown. Painting is a tool for finding out.

### Phase 2 plan (as written before the work)
- Face-flag painting (faceData bits) on n or o, with brush and select modes.
- Editing user-edge `edgeData`, the blocked flag and costs.
- NVA navmesh fields (Unk44, Unk4C, Unk50, rotation), gate node positions and costs, node banks, ONAV fields and cell masks.
- Every edit is an undoable action, following the existing Havok property actions.
  No indices change, so connections stay valid by construction.

### Phase 3: Connectivity editing: add/remove links DONE 2026-11-07 (not yet checked in the app)

**How the game's files hold user edges** (NavSurvey `useredges` on all 997 maps, 3.9M user edges, and `cachedump <map> <n>`):
- **Edge array:** every user edge comes after all the original edges. The original ranges are packed in face order; the
  user ranges are not (2,177 of 2,286 files). A face without user edges has `m_startUserEdgeIndex = -1`.
- **Vertices:** each user edge has two vertices of its own, used by nothing else, appended after the original vertices.
  They sit inside the face (XZ), not on its boundary: a short segment.
- **Pairs:** 3,922,362 of 3,924,697 user edges point at a user edge of their target face, which points back. The two have
  different vertices and the same edgeData. Flags are 20/20 (both ways, 606k) or 20/52 (one way, the edge back blocked,
  357k pairs). The other 2,335 have no opposite edge and an opposite face with section 0x3FF.
- **Connections almost never name a user edge** (`linkedit`): only in 2 of 4,510 files (e.g. `n60_52_53_00_525300`).
  So inserting or removing one moves only user edges, and the NVA and NVC files need no remapping. The tool refuses
  link edits in the 2 files where a connection does.
- **Clearance cache:** every non-empty file has a clearance cache seeding with one or two initial caches (mostly the same).
  - `m_mcpData` has one entry per edge, user edges included.
  - `m_vertexClearances` has one byte per vertex; it is 0xFF for every user edge vertex.
  - Per face, `m_faceOffsets[f]` starts a block of **T(k) + 1 bytes**, where k is the number of the face's *linked* edges
    (those with an opposite face: original edges first, then user edges, in order). This holds for 3,407,969 of 3,408,371 faces.
    The block is the clearance of each pair of linked edges, as a lower triangle by column ((1,0) (2,0) (2,1) (3,0) …), then 0xFF.
    A pair's value is the smaller of its two edges' mcp clearances.
- **n vs o (open question 1):** the cache ceiling is 1.53 m in `n` files and 5.1 m in `o` files. This strongly suggests
  `n` is for normal-sized characters and `o` for large ones.
- `hkaiUserEdgeSetupArray` is in 213 files; the edits leave it alone (it looks like authoring data).

**What was built**
- **`Tools/Navigation/NavigationUserEdges.cs`** (renderer-free, linked into NavSurvey):
  - `NavUserEdges.Add` adds a link between two faces of one mesh. It creates a user edge on each face, at the end of
    that face's user range, each with two new vertices: a 0.5 m segment across the link at the picked point. The edges point at each other.
    The edge back is blocked unless the link goes both ways. Both edges get the chosen edgeData. It also adds the cache
    entries: an mcp entry per edge (the chosen clearance), 0xFF vertex clearances, and the new column of each face's
    block, before its 0xFF. Later block offsets are shifted, and so are all references to edges that moved up.
  - `NavUserEdges.Remove` removes a user edge and its pair, their vertices if unused, their mcp entries and their row and
    column of the face blocks, and shifts everything back.
  - Both are refused, with nothing changed, if a face's block isn't T(k) + 1 (402 vanilla faces) or the link stays on one face.
  - `NavMeshState` copies a mesh's faces, edges, vertices, face/edge data, AABB and caches, and puts them back.
  - `NavUserEdges.Check` checks the layout above, and `ConnectionsNameEdgesFrom` checks the "no connection names a user edge" rule.
- **`Actions/Map Objects/NavigationLinkEditAction.cs`**: undo and redo put back the mesh state from before or after.
  The map is marked `MapFileType.Navmesh`.
- **Tool, View tab → Links:**
  - "Add links in the viewport" (turns picking and painting off). The first click picks the start face (outlined),
    the second click the target face of the same navmesh file.
  - Options: Both ways, edge data (hex plus the Jump/Fall/Extra cost bits), and Clearance (0–254).
  - The face inspector has **Remove link** on each user edge. Every add and remove is one undo.
- **NavSurvey `linkedit [regex] [max]`**, run on every non-empty navmesh file:
  - the game file passes `Check`, and whether a connection names a user edge;
  - a link added between a face and its neighbour passes `Check`, keeps every cache block the right size, has the asked
    shape, and survives `HavokFile.Write` and a re-read;
  - removing it again gives back the file **byte for byte**;
  - removing one of the game's links passes `Check`, and restoring the `NavMeshState` gives back the file byte for byte.

**Results on all 997 maps** (4,510 non-empty navmesh files):
- every game file passes `Check`. 19 files have faces whose cache block isn't T(k) + 1; links on those faces are refused;
- add: 4,510 of 4,510 pass the check, keep every block's size, have the asked shape, and survive the write;
- add then remove: 4,510 of 4,510 byte-identical;
- removing a game link: 2,286 of 2,286 pass the check, and restoring is byte-identical for all of them.

**For the user to check in the app** (Havok loading on, on a project copy):
1. `m10_01_00_00`: View → Show overlay, n set. Tick "Add links in the viewport".
   - Click a floor face, then a face on a ledge a few metres away.
   - A yellow arc should appear from the first face to the second (Both ways off: the edge back is orange, blocked).
2. Turn on Pick, and click the start face. The new user edge should be listed, "to face …", with data 2.
   Its pair should be on the target face, Blocked ticked.
3. Ctrl+Z should remove the arc, and Ctrl+Y should bring it back.
4. Click **Remove link** on one of the game's user edges. Its arc and the arc back should both go. Undo it.
5. Save, reload the map, and check the link is still there. Then Check → Validate: no new issues.
6. In game: whether characters use the new link. This needs an enemy whose path crosses it; a jump down from a ledge
   (Fall bit 0x4, one way) is the easiest to see.

### Phase 3: Connectivity editing: re-stitch DONE 2026-11-07 (not yet checked in the app)

**How the game's connections sit** (NavSurvey `stitchsurvey`, `stitch`):
- They join **boundary edges** (no opposite face, flags 4), n with n and o with o, never across sets.
- **NVA:** every connection is stored in both directions (113,972 of 113,972), each in the connector of its
  MainNameID → TargetNameID. Connectors are sorted by unsigned (MainNameID, TargetNameID). The connections inside a
  connector are in no order. Unk14 and Unk1C are 0, and there are no graph connections.
- **NVC:** one direction only, from the file's first map to its second. There is no mirrored file. Every NVC file pairs two
  small open-world tiles of the same area (2,641 m60, 1,045 m61), including diagonal neighbours. The tile offset formula
  (256 m per tile, as `MapConnections_ER` places maps) puts the paired edges together.
- **Geometry:** the edges run exactly opposite ways. 65% meet end to end within 1 mm; the rest overlap in part along a
  shared line (an edge can have several connections).
  | | separation p50 / p90 / p99 | step p50 / p90 / p99 | largest gap |
  |---|---|---|---|
  | NVA | 0 / 2.3 cm / 0.35 m | 0 / 5 cm / 0.46 m | 1.39 m |
  | NVC | 0 / 0.7 cm / 0.28 m | 0 / 0.24 m / 1.2 m | 2.8 m (o set) |
- **Matching** with Havok-style rules (0.1 m apart, 0.5 m step, 0.02 m overlap, 0.98 alignment) finds 87% of the game's
  in-map connections and 90% of its cross-map ones, plus about 3.5% that the game doesn't have. FromSoftware's generator
  can't be reproduced exactly from the meshes, so re-stitching is built to change as little as possible.

**What was built**
- **`Tools/Navigation/NavigationStitch.cs`** (renderer-free, linked into NavSurvey): `NavigationStitch.Find` joins
  boundary edges of different placed meshes (a 4 m grid, then `Fit`: alignment, separation, overlap and step), and `Gap`
  is the closest distance between two edges.
- **`Tools/Navigation/NavigationStitchPlan.cs`:**
  - `NavStitchPlanner.Plan` looks at one set of a map, optionally one navmesh only, with its loaded NVC neighbours placed by `TileOffset`.
    - **Broken:** a connection whose face is gone, whose edge isn't its face's own or isn't on the boundary, or whose edges are more than 5 m apart.
    - **Missing reverse:** an NVA connection stored in one direction only gets the other.
    - **Added:** pairs of boundary edges that fit and aren't joined yet. By default only edges with no connection at all
      ("Only join open edges"); otherwise any edge, but never a pair that exists.
      Across maps, only through an existing NVC file, in the direction it stores.
  - `NavStitchPlanner.Apply` makes the changes, keeps the connector order, and drops connectors left empty.
  - `NavConnectionState` copies and restores the NVA's and NVC files' connectors.
- **`Actions/Map Objects/NavigationStitchAction.cs`:** undo and redo restore the connectors, and the action marks the NVA and the changed NVC files.
- **Tool, View tab → Stitch:**
  - options: Only the picked face's navmesh, Include loaded neighbours, Only join open edges, and the three distances;
  - **Find** shows the counts, the neighbours used and a list of the changes;
  - **Apply** has Remove broken and Add new, and is one undo.
- **NavSurvey `stitchedit [regex] [max] [all]`:** plans the game's data, then damages each map, re-stitches it and checks it.
  The damage: 5% of the in-map pairs and 10% of the NVC connections deleted, and up to 5 connections pointed at edges that don't exist.

**Results on all 997 maps** (default parameters, open edges only):
- **Game data:**
  - 304,270 connections checked; no reverse is missing.
  - A plan would add 2,366 in-map and 266 cross-map pairs that the game leaves apart.
  - It finds 496 broken connections, all in a few NVC files: the stale `_00`↔`_10` tile pairs the validator already
    reports as inherited, and some m61 pairs (e.g. `m61_44_52_00_m61_44_53_00`). Their indices don't fit the meshes,
    or their edges are 180–290 m apart. Untick Remove broken to leave them as the game has them.
- **Damaged:**
  - all 112 corrupted connections are found;
  - 17,973 of 31,631 deleted connections are put back (57%). With `all` (open edges only off) it was 90% on a sample, with about twice the extra pairs;
  - validator issues go from 884 (854 of them inherited) to 403;
  - a second plan is empty on every map, and restoring is byte-identical on all 997.

**For the user to check in the app** (Havok loading on, on a project copy):
1. `m10_01_00_00`: View → Show overlay, n set. In Stitch, click **Find**.
   - It should report the connections it checked, 0 broken, and some new pairs. Open Changes to see them.
2. Click **Apply**. A few red (open) edges should turn cyan. Ctrl+Z should turn them red again.
3. Load `m60_44_36_00` with its neighbours, then click Find with Include loaded neighbours on. The neighbours line should name them,
   and the counts should include pairs across maps. Apply, then save. The project should get the NVA and the changed NVC files.
4. Check → Validate after saving: no new issues.
5. Test with an edit: on a project copy, untick Only join open edges, Find and Apply, and check in game that enemies
   still path normally across the joins.

### Phase 3: Connectivity editing: remove and duplicate models DONE 2026-11-07 (not yet checked in the app)

**What refers to a navmesh entry** (NavSurvey `modelsurvey`, `rootdump`):
- **By NameID:** NVA and NVC connectors, and node bank faces (n set only, 3,841 of 3,841 resolve).
- **By list position:** gate nodes. `ConnectedNavmeshIndex` always names a navmesh connected to the owner (9,243 of 9,243),
  and `NodeSubID` names that navmesh's gate that points back (8,782; the rest are -1).
  Each gate's `NeighbourGateNodeCosts` is 16 shorts: the cost to each of the owner's gates (0 for itself), then -1.
- **A NameID's index** is the n entry's position, so inserting or deleting entries would renumber NameIDs and gate indices.
- **Placeholders** (4,916 entries) come in pairs across the sets. They have no faces and no gate nodes; Unk4C is 0 or 1;
  the file is empty or absent.
- **Empty files** are either only an `hkaiNavMesh` with no seeding, or that plus an empty mediator and seeding.
  The binder numbers its files 10000+ for n, 20000+ for o and 40000+ for q, with full `N:\GR\…\bind6\` names, no per-file DCX, and Flag1.
- **Duplicate ModelIDs:** some maps list a ModelID twice (443590 in `m60_44_35_00`), so a placeholder can share its file with a real model.
- **User edge setups** keep their translation in M41..M43, in world or local space.

**What was built**
- **`Tools/Navigation/NavigationModels.cs`:**
  - **`NavModelEdit.Remove`** turns a model into a placeholder, as the game's are:
    - both entries get no faces and Unk4C 1;
    - its gate nodes, and other navmeshes' gates leading to it, are removed (with their cost columns and the NodeSubIDs after them);
    - every connection naming it is removed, in the NVA and in the NVC files on this map's side, with its node bank faces;
    - its files become empty (only the `hkaiNavMesh`).
  - **`NavModelEdit.Duplicate`** copies a model's n and o files into a free placeholder pair, moved by an offset:
    - it moves the vertices, the bounds, the mediator tree's root box (nodes are stored relative to their parents) and world-space user edge setups;
    - the placeholder keeps its ModelID and NameIDs, so nothing is renumbered;
    - the copy has no connections or gate nodes.
  - `FaceDataIndex` is recomputed as the running total; on the game's data that changes nothing on any map.
  - Models and slots whose files another entry also names are never offered.
  - `NavModelState` copies and restores the NVA's face counts, Unk4C, gate nodes, node banks and connectors.
  - File changes are whole new documents (`HavokFile.Write` walks from the root).
- **`HavokNavmeshBank.SetDocument`:** replaces, adds or removes a file. The save adds new files to the binder in the game's
  layout and drops removed ones.
- **`NavigationModelEditAction`:** one undo. It restores the NVA and NVC values and swaps the documents back.
- **Tool, Models tab:** a list of the models (n and o face counts, state), Select the picked face's model, Remove model,
  and Duplicate (a free placeholder plus an offset).

**Results on all 997 maps** (`modeledit`; one random model per map, and all 951 models of a sample with `all`):
- **Remove:** no new validator errors, nothing names the model, no gate points at it, all 7,998 remaining gates pair, every
  gate keeps 16 costs, and the files write and read back. Restore is byte-identical.
- **Duplicate:** 527 maps have a free slot. The copy and the mediator root box move by the offset, and the face counts match.
  Restore is byte-identical. The two duplicate-ModelID failures in `m60_44_35_00` are fixed by the shared-file check above.
- **A copy in place of the original:** a re-stitch with "Only join open edges" off joins 4,009 pairs, against the
  original's 3,803 in-map n connections.

**Open:** the copy has no gate nodes, so the hierarchical graph doesn't route through it. Gate node generation is still open.

**For the user to check in the app** (Havok loading on, on a project copy):
1. In `m10_01_00_00`, open the Models tab. Expect 14 models, with counts for placeholders and free slots.
2. Select a model with faces and click **Remove model**. Its overlay faces should go and the list should show it as a
   placeholder. Ctrl+Z should bring it back.
3. If the map has a free slot: select a model, enter an offset (e.g. 0, 5, 0), and click **Duplicate**. A copy should
   draw 5 m above the original. Pick one of its faces, then in Stitch tick "Only the picked face's navmesh" and click Find.
4. Save and reload. The binder should hold the new n/o files, and Check → Validate should report no new issues.

### Phase 3 plan (as written before the work)
- **Add/remove user edges** (jump links between two faces): insert into the face's user-edge range, then remap every connector and NVC via the Phase 0 service.
- **Re-stitch connections:** recompute NVA connectors and NVC entries by matching coincident boundary edges between meshes or maps, within a tolerance.
  This is the same edge-matching idea as Havok's `hkaiNavMeshEdgeMatchingParameters`.
  It repairs connections after geometry edits, and creates them for new navmeshes.
- **Add/remove a navmesh model:** this creates both NVA halves with correct NameIDs/indices, the `n`/`o` files and any placeholders.
  It renumbers the later indices and fixes every reference to them.

### Phase 4: Geometry
- **Generation from collision: G1–G4 DONE 2026-11-07 (not yet checked in the app).** See `Navmesh Generation Plan.md`.
- **Vertex editing that keeps topology:** move vertices and update AABBs. The mediator AABB tree and clearance cache are rebuilt
  (or dropped, if the game accepts that).
  The AABB tree builder can reuse the Collision Generator's static-tree experience (`Compressed Mesh Format.md`).
- **Regeneration from collision:**
  - port the DS3 Recast path (`NavGen.dll`) to an hk2018 `hkaiNavMesh` builder;
  - build n and o with different agent parameters, then faceData from AssetGeometryParam flags and from MSB/collision attributes;
  - re-stitch (Phase 3) and recompute gate nodes.
  - This is the largest and riskiest phase: it reaches parity with FromSoftware's pipeline only approximately.

### Phase 5: New content
- Navmesh data for new maps: an NVA with halves, a map UID and Entries9; an empty or authored ONAV; the NVC files to neighbours.

## 7. Open questions, and how to settle each

| Question | How |
|---|---|
| What `n` vs `o` are (agent size?) | The clearance cache ceiling is 1.53 m in n and 5.1 m in o (Phase 3), so very likely agent size. To confirm: Trace the caller of `0x140705E60` in `CSHkAiNvmbndFileCap`/`ResCap`, and what it does with each name. Compare the `o` files' vertex spacing and erosion. |
| What the `q…_0..3` files are | Trace the same caller. 8 are non-empty; check them and their faceData 0x10 bit |
| What the faceData bits mean | Find the reads of `m_faceData` in `CSHkAiNaviMesh*` (cost modifiers, NpcThinkParam `enableNaviFlg` checks) |
| edgeData layout (`0x0X04` …) | The same code paths, plus the user-edge setup (`hkaiUserEdgeSetupArray`) in the 39 files that ship it |
| How gate nodes are used | Trace reads of NVA +D8 (gate nodes) and +0x48 (gate index/count) |
| Entries3 / 10 / 13, and section 10's other reader | Disassemble consumers of +A8 / +F0, +B0; find the section-10 reader |
| Whether the game needs the mediator/clearance cache | Test in game: a map saved without them (needs the user) |
| How `Event.Navmesh` / `NavmeshCutting` link to faces | MSB consumer tracing (`MSBE Consumer Tracing Plan.md` methods) |

## 8. Recommendation

Start with Phases 0 and 1. They carry no format risk: everything round-trips today, and the validator protects later phases.
Phase 2 delivers the most useful editing (flag painting and jump-link data) without touching indices.
Phases 3–4 depend on the open questions above, which the Phase 0/1 viewer will make much easier to answer.
