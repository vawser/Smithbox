# Navmesh Generation Plan (Elden Ring)

2026-11-07. Generating a navmesh model (its `n` and `o` files and NVA entries) from a collision model. This is Phase 4
of `Navigation Editor Plan.md` ("Regeneration from collision"), the largest phase. This document gathers what the
game's data says a generated mesh must look like, what Smithbox already has, and a phased plan.

## 1. What the game's navmeshes look like next to their collision

NavSurvey `navcol [regex] [max]` (10 maps: legacy dungeons, catacombs, open world):

| Finding | Value |
|---|---|
| Navmesh models with a collision model of the same ID (`n…_001000` ↔ `h…_001000`) | 132 of 132 |
| Faces | convex polygons, wound CCW seen from above. n: 56% triangles, 22% quads, 12% pentagons, the rest 6 to 8+ edges |
| Edge length | n: p50 1.5 m, p90 4.2 m. o: p50 2.8 m, p90 8.0 m (coarser) |
| Face slope | n: p50 14°, p90 46°; some faces are steeper, up to vertical (ladders, ledges) |
| Height of the vertices over the collision below | p10 0, p50 0.26 m (n) / 0.43 m (o). 7% are over no collision, and p90 is 12 m: navmeshes also cover walkable **assets** (bridges, stairs, lifts), which map collision doesn't hold |
| Erosion radius in the files | 0 in every file; any agent radius is already applied to the geometry |
| o against n | not a plain erosion. The area ratio o/n is p50 1.0 (0.2 to 4.4), and the o boundary is p50 0.3 m, p90 6.7 m from the n boundary |
| Clearance cache ceiling | 1.53 m in n, 5.1 m in o (Navigation Editor Plan, Phase 3) |

So:
- A generated model gets the **collision model's ID**.
- n is a finer mesh for normal characters; o is coarser and suits large ones. Both need their own agent parameters.
  Exact parity with FromSoftware's generator isn't the aim: the comparison below measures how close we get.
- Map collision alone misses asset surfaces. Including the map's assets' collision (from their own collision models,
  placed by the MSB) is a later step.

## 2. What a generated file must hold

From the game's files (Navigation Editor Plan sections 3.2 and Phase 3):

1. **`hkaiNavMesh`**:
   - faces with their edge ranges, and edges with `m_a`/`m_b`;
   - `m_oppositeEdge`/`m_oppositeFace` for inner edges, and 0xFFFFFFFF on the boundary;
   - flags 4 (ORIGINAL);
   - faceData 0 and edgeData 0 (striding 1), no user edges;
   - the AABB, erosion radius 0, no streaming sets.
2. **`hkaiStaticTreeNavMeshQueryMediator`**: an `hkcdStaticAabbTree` (`Aabb6BytesTree`) over the faces.
   - The hk2014 builder has a 6-byte codec encoder (`BVH.BuildAxis6Tree`, `hkcdStaticTreeCodec3Axis6`).
   - Smithbox's hk2018 compressed mesh builder builds static trees in C#. One of them is ported to the hk2018 `Aabb6BytesTree`.
3. **Clearance cache seeding** (two initial caches):
   - the layout is known (mcp per edge, vertex clearances, per-face T(k)+1 pair blocks); the clearance values aren't;
   - empty files ship without seeding. Whether the game accepts a non-empty mesh without it is **unknown**.
     The first version leaves it out and tests that in game; computing it is a later step.
4. **NVA:**
   - the model goes into a free placeholder pair (as Duplicate does), with the placeholder's ModelID set to the collision model's ID;
   - the face counts are set and FaceDataIndex recomputed, so nothing is renumbered;
   - no gate nodes (open, see section 5).
5. **Connections:** the Stitch tab joins it to its neighbours (NVA and NVC).

## 3. What Smithbox already has

| Piece | Where |
|---|---|
| ER collision triangles | `Havok.Shared.CollisionGeometry` (`Submeshes`, `CompressedTriangles`); `HavokCollisionBank.MapDocuments` holds the loaded map's collision |
| Recast, native | `src/native/NavGen` (vendored Recast and a BVH builder) → `NavGen.dll`, used by the DS3 hk2014 `hkaiNavMeshBuilder`. It builds triangles only (`maxVertsPerPoly = 3`), with quantised ushort vertices. Visual Studio's C++ tools are installed, so it can be rebuilt |
| Recast, C# | not in the repo. DotRecast (MIT, pure C#) 2026.3.1 is on NuGet: `DotRecast.Core`, `DotRecast.Recast` |
| 6-byte AABB tree encoder | hk2014 `BVH.BuildAxis6Tree` |
| Placing a mesh in a free slot | `NavModelEdit.Duplicate` and `HavokNavmeshBank.SetDocument` (Phase 3) |
| Joining it to its neighbours | `NavStitchPlanner` (Phase 3) |

## 4. Plan

Each step ends in something the user can check in the app.

**G1. Generator core** (renderer-free, linked into NavSurvey):
- Collision triangles → Recast heightfield → regions → contours → convex polygon mesh (up to 6 vertices), in the collision's map space.
- Agent parameters for each set: cell size, cell height, max slope, agent height, climb and radius, min region area, and max edge length.
- Convert the polygons to an `hkaiNavMesh` with adjacency, CCW winding, flags 4, and zero face and edge data.

**G2. File:** build the document from a template (a clone of one of the map's navmesh files, emptied), with the mesh
and a new mediator tree. NavSurvey checks that it writes and reads back, that the tree's leaves cover every face, and
that the validator is happy.

**G3. Tool: Models tab → Generate:**
- pick a collision model of the loaded map (by default the selected navmesh model's ID) and a free slot;
- set the n and o parameters;
- generate both sets into the slot as one undo, then re-stitch.

**G4. Comparison and tuning** (NavSurvey `navgen`): for every model with collision, generate n and o and compare them with
the game's: covered area, how much of the game's mesh the generated one covers and the other way round, vertex heights,
face sizes and slopes. Tune the default parameters on that.

**G5. In-game test** (needs the user): replace one model's navmesh with a generated one, with and without the clearance
cache, and see whether enemies path on it.

**Later:**
- the clearance cache, computed;
- gate nodes for the generated model, and how the game uses them;
- asset collision from the MSB;
- face data from the collision's materials and from `AssetGeometryParam` navimesh flags.

## 4a. Built 2026-11-07: G1 to G4 (not yet checked in the app)

The generator is DotRecast 2026.3.1 (`DotRecast.Recast`, MIT, from NuGet), referenced by Smithbox.Program and NavSurvey.

**What was built**
- **`Tools/Navigation/NavigationGenerator.cs`:**
  - `NavigationGenerator.Generate(triangles, parameters, template)`: Recast (watershed partitions, convex polygons of up to 6 corners).
  - Each vertex is then moved onto the walkable collision surface nearest it vertically, within the climb.
  - The polygons are wound counter-clockwise from above, and inner edges are paired by their vertices.
  - Values the game's files carry (cluster index, padding, mesh flags, AABB w) come from the template mesh.
  - Face data and edge data are 0, edge flags 4, erosion radius 0.
  - `NavGenParameters`: cell size and height, slope, agent height, radius and climb, region areas, edge length and
    error, corners per face. `DefaultN` and `DefaultO` are tuned (below).
  - `NavGenFile.Build(template, mesh)`: a copy of one of the map's navmesh files with its mesh replaced and a new face
    tree, and without clearance seeding or user edge setups.
- **`Tools/Navigation/NavigationTree.cs`:** the face tree codec of the query mediator (`Aabb6BytesTree`):
  - `Build`: one face per leaf, a median split, boxes rounded outwards;
  - `Decode` and `Check`.
- **`NavModelEdit.Generate`:**
  - **into a free placeholder:** optionally renamed to the collision model's ID, if no other entry has it;
  - **in place of a model:** its connections and node bank faces are dropped, and its gate nodes kept.
  - Undone by `NavModelState`, which now also restores ModelIDs.
- **Tool, Models tab → Generate:**
  - the map's high collision models, with the selected model's own ticked;
  - the target (Replace the selected model's navmesh, or Into a free placeholder, with Give it the collision model's ID);
  - the n and o settings, with Reset;
  - **Generate** runs in the background, then makes the edit as one undo.

**Checks** (NavSurvey):
- `treecheck` (2,472 game files):
  - the codec reads every game tree with every face once, inside its leaf: one leaf per face, 2n − 1 nodes;
  - the built trees pass the same check, with the same node counts. Their boxes are about 48% looser, which only costs query speed.
- `navgen` (G4):
  - every generated mesh is valid: corners chain, faces are convex and counter-clockwise, and opposite edges pair back;
  - every file writes and reads back, with a tree that covers every face;
  - **into a slot** and **replace** each give no new validator errors, and restoring is byte-identical.

**How close it gets** (`navgen`, with the map's collision within each game mesh's bounds as input):

| Map kind | n: game area covered | o: game area covered |
|---|---|---|
| `m10_01`, `m12_01` (walkable surfaces are map collision) | p50 87%, p90 96% | p50 66%, p90 80% |
| `m11_00`, `m30_02` (built from assets) | p50 about 0–25% | lower |
| open world `m60_42_36_00` | 36% | |

- **Generated area that lies on one of the game's navmeshes:** p50 about 35%. The rest is collision the game's navmeshes
  leave out: roofs, wall tops, ledges and other places a character can't reach.
- **Collision model IDs:** they match navmesh model IDs (132 of 132). But in `m30_02_00_00` the matching collision
  models (000100 to 000300) are one-triangle stubs, and the walkable geometry is in assets.
- **Tuning:**
  - n: cell 0.2 m, agent radius 0.1 m, height 1.8 m, climb 0.8 m, slope 60°;
  - o: cell 0.3 m, radius 0.3 m, height 2.5 m, climb 1.2 m.
  - A radius of 0.1 m covered most more (70% → 84%). 0.3 and 0.5 m give the same result, since both round to 2 voxels of 0.25 m.
    Slope made no difference on these maps.
- **AABB W:** the game's AABB W values are ±2.4 (n) and ±8 (o). 2.4 is NVA `Unk44`. Generated files take them from the template mesh.

**For the user to check in the app** (Havok and collision loading on, on a project copy):
1. Load `m10_01_00_00`. Navigation → Models: select model 001000, and in Generate keep its collision model (h…_001000)
   ticked, with **Replace model 001000's navmesh**. Click Generate.
   - The message should give the n and o face counts.
   - In Overlay, the model's faces should now follow the floor of that area.
2. Ctrl+Z should bring back the game's mesh.
3. Generate again. In Stitch, pick one of its faces, tick "Only the picked face's navmesh", untick "Only join open
   edges", then Find and Apply. Cyan joins should appear where it meets its neighbours.
4. Save. Check → Validate should report no new issues.
5. **In game (G5):** whether enemies of `m10_01_00_00` still path across that area. This also answers whether a navmesh
   without clearance seeding and without gate nodes works.

**Next:** asset collision (MSB asset parts placed with their own collision models), which covers what map collision misses.
Then clearance seeding, gate nodes, and dropping unreachable areas (keep only regions joined to a seed).

## 5. Open questions

| Question | How to settle |
|---|---|
| Does the game accept a navmesh without clearance seeding? | In game (G5) |
| Do gate nodes matter for paths that cross a generated model? | In game; trace the reads of NVA +D8 |
| Which agent parameters FromSoftware used for n and o | G4's comparison over the game's maps |
| Which collision materials should not be walkable (water, death planes) | Compare the game's navmesh coverage with the collision materials under it |
