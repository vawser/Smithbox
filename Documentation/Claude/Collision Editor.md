# Collision Editor

A Map Editor tool (Tools window) for reshaping existing map collision by moving its vertices in the viewport.

## Scope

- **Games**: ER, NR and AC6. These are the games whose map collision the Map Editor holds as editable documents
  (`HavokCollisionBank.MapDocuments`) and writes back (`HavokCollisionBank.SaveMapCollision`). Their meshes are
  `fsnpCustomParamCompressedMeshShape` (hk2018 / hk2019), which the shared `CollisionShapeBuilder` rebuilds.
  SDT holds its collision (`TagContainers`) but has no collision save path; DS3/BB map collision isn't held at all.
- **Shapes**: bodies whose shape is a FromSoftware compressed mesh. Other bodies of the file are kept as they are.
- **Types**: high, low and fall protection can be edited, one layer at a time (see Layers).

## Workflow

1. **Start Collision Edit** turns on the cursor. **Stop Collision Edit** turns it off; pending edits are kept.
2. The cursor is a sphere that sits where the mouse ray hits the edited layer: a wireframe over a translucent fill
   (Cursor Opacity, default 20%), so the collision under a large cursor stays visible. Both are depth tested. The
   vertex of the hit triangle nearest the hit point is the target, marked by a small solid sphere drawn over
   everything, at most 0.1 across so it doesn't hide the vertices around it.
3. Keys, while the mouse is over the viewport and no mouse button is held (so the camera's Q/E flight still works):
   - **Q** raises the target vertex by the step, **E** lowers it. Held, they repeat.
   - **G** held drags the target vertex across the horizontal plane at its height, following the mouse.
   The keys are bindings (`MapEditor_CollisionEditor_Raise/Lower/Drag`), so they can be changed in the keybind settings.
4. A **Radius** above zero moves the vertices around the target as well, with a smooth falloff.
5. **Commit Changes** (valid once something is edited) rebuilds each edited file's meshes from the edited vertices,
   puts them in the document as an undoable action (marking the map's collision as modified), writes the collision
   binder into the project, and refreshes the displayed mesh.
6. **Discard Changes** drops the pending edits.

## DS3 and SDT

Both games' map collision files have the same layout as ER's: one body per file, a `fsnpCustomParamCompressedMeshShape`
with the same FromSoftware parameters (materials by shape key), and bodies named `h000100` / `l000100`
(`CollisionSurvey DS3 m30_00_00_00 3 h`, `SDT m10_00_00_00 3 h`). Their binders have no area folder:
`map/m30_00_00_00/h30_00_00_00.hkxbhd`. What differs is how the Map Editor holds them:

- **DS3 (hk2014 packfiles, no compendium)**: not held at all before. Now listed when the map loads and read when first
  asked for, as ER is (`MapDocuments`), and saved like ER: every read file of an edited binder is written.
- **SDT (hk2016 tagfiles with a compendium)**: already read at map load into `TagContainers`, for the WALL painting
  and the viewport, but never saved. Saving now writes only the files the Collision Editor changed or created, so the
  rest of the binder keeps its bytes.
- **Building**: hk2014 meshes are built by `HavokCompressedMeshGenerator_hk2014` with the old shape's convex radius and
  flags (its `Variant`); hk2016 by its own `CollisionShapeBuilder`. Both go through `HavokCompressedMeshBuilder.Build`,
  whose result gives each triangle's new shape key.
- **SDT walls**: WALL and NOJUMPWALL files mark triangles of the low collision by shape key, and a rebuild renumbers the
  keys. On commit, each edited low collision's marks are moved from each triangle's old key (as decoded) to its new key
  (from the build); marks of triangles left out as degenerate are dropped. The remap is part of the commit's action, so
  undo puts the old marks back, and the WALL files are written with the binders. The wall geometry cache of the file is
  dropped so the displayed walls follow the new shape.
- **Create**: the same as ER, with the binder paths above. A new SDT low collision has no wall marks.
- Fall protection doesn't exist in either game.

### Checked against game files

- `CollisionSurvey SDT m10_00_00_00 400 remap`: each l file is decoded as the editor decodes it, a vertex moved,
  rebuilt with `CollisionEditorShapes`, and each triangle looked up by its new key. 174/174 rebuilt files find every
  triangle, with distinct keys; all 73,102 WALL marks of the map are on keys the decode gives (the map has no
  NOJUMPWALL files). One file, `l10_00_00_00_000001` (202,359 triangles, 123,478 vertices), can't be rebuilt:
  the builder stops with "too many vertices shared between sections", so committing an edit to it logs a build
  failure and changes nothing. Very large meshes in any game can hit this.
- `CollisionSurvey DS3 m30_00_00_00 400 remap`: 94/94 rebuilt files find every triangle (DS3 has no walls).
- `create` for both: DS3 binders have no compendium and DCX_DFLT_10000_44_9 files, SDT's a compendium and DCX_KRAK;
  in both the new h and l files read back with the next free ID, the binder's compression, renamed bodies
  (`h123456`, `l123456`), the input triangles exactly and both materials. SDT's l template is `l10_00_00_00_000000`.
- Not checked: DS3 or SDT loading the new or rebuilt files in game.

## Create tab

The section has two tabs: **Edit** (everything above) and **Create**, which makes a new collision for the active
map (the map the content tree shows) in one click.

- **Shape**: Square, Triangle, Circle and Semi-Circle with the Havok Editor generator's parameters
  (`HavokShapeGenerator`), or **Selected FLVERs**: the FLVERs of every selected map piece and asset of the active map
  (other selected objects are ignored), each with a checklist of its meshes (a tree node per part when there are
  several). Greyed out unless at least one is selected. FLVERs are read once per model (map pieces keyed by map) and a
  part that stays selected keeps its mesh choices. Facing and Collision Material as in the generator
  (`CollisionMaterialPicker`); a FLVER mesh whose material is named `hkm_Name_Property` keeps that material. A preview
  shows the triangles (`MeshPreviewRenderer`).
- **Combining FLVERs**: the **anchor** is the first readable source in map order (not selection order, so it is stable).
  Each source's vertices go through `source world * inverse(anchor world without its scale)`; the anchor's own matrix
  is just its scale, exactly as the single-FLVER build was. A mirroring matrix (negative determinant) reverses that
  source's winding. Vertices are welded across sources on a 1 mm grid. The preview key holds each source's matrix
  *relative to the anchor*, so dragging the whole selection doesn't rebuild.
- An internal triangle cull option (per-source fast winding numbers) was built and then removed at the user's request;
  the class and its `CullCheck` test tool are in `.archive/Collision Internal Culling`.
- **Model ID**: the new collision's number (h + 6 digits), suggested as the first unused number after the map's
  highest below 900000, and checked against the map's collision files and parts.
- **Files**: each new collision is a **new file in the map's binders**: `h<map>_<id>.hkx.dcx` in the h binder and
  `l<map>_<id>.hkx.dcx` in the l binder (if the map has one), with the same mesh, so linked editing works on it.
  Each document is a fresh read of an existing file of the same binder (one whose body is a compressed mesh), so it
  carries that binder's compendium types and FromSoftware fields, with the body's shape replaced by the new mesh,
  placed at the origin and named for the new model. The binders are written into the project straight away
  (`HavokCollisionBank.AddMapCollision` registers the file; `SaveMapCollision` appends files the binder lacks, with the
  next free ID and the flags, folder and compression of the binder's other collision files).
- **Part**: a Collision part is added with `EntAddAction` (so it can be undone), cloned from an existing collision part
  of the map so its fields match the map's, with its own name (`h<id>_0000`), model name, no entity ID or entity
  groups, and scale 1. A basic shape is placed at the placement orb. Selected FLVERs takes the anchor part's position,
  rotation and parent, with each source's scale baked into the vertices, as collision isn't scaled.
- **Viewport**: before the part is added, the new files are loaded from the binder with only their names asked for,
  so the part's own load joins that load rather than reading every collision of the binder again.
- **Undo** removes the part; the files stay in the binders. The MSB's model list is filled in from the parts on save.

### Checked against game files (`CollisionSurvey ER m10_00_00_00 0 create`)

The create mode runs `CollisionEditorShapes` (linked into the tool) on Stormveil's binders: the first single-body
file is made into a two-material square, appended as `AddNewFiles` does, and the binder written and read back.

- h binder: 117 collisions, IDs 0..117, every file Flag1 in folder `m10_00_00_00`, with a compendium. Template
  `h10_00_00_00_000001`, body `h000001` renamed `h123456`. The new file reads back as ID 118, the others intact,
  DCX_KRAK as the binder's, 2 triangles with no corner error, materials 1 and 29.
- l binder: the same, with bodies named `l...` (`l000001` became `l123456`), so renaming by the number covers both.
- Not checked: the game loading the new files, and whether a map needs anything beyond the binders and the part
  (for example navmesh or other per-collision files) for a new collision to work in game.

## Layers

High and low collision are the same ground at two levels of detail; fall protection is a barrier of its own.

- **Edited layer**: the cursor only hits the chosen layer (`MapEditor_CollisionEditor_Layer`, default Low). The layers
  lie on top of each other, so hitting the nearest of them would switch between them from frame to frame.
- **Linked layers** (`MapEditor_CollisionEditor_Link_Layers`, on by default): an edit of high also moves low, and the
  other way round, then Commit rebuilds and writes both files. Fall protection isn't linked. The linked file is decoded
  whether or not its layer is displayed; its preview only shows while it is.
- **Which vertices follow**: those of the other layer within the link distance (default 0.5) of the edited vertex
  follow fully; past it, the radius fades them out as it does on the edited layer. Each takes the same world move as
  the edited vertex, including while dragging.

### Survey (`CollisionSurvey ER m10_00_00_00 400 link`, 107 Stormveil models with both layers)

- Low vertices are rarely high vertices: 4.6% are exactly the same position. Linking has to go by distance.
- Where the layers are the same ground they lie close: of low vertices, 59% are within 0.01 of the high surface,
  90% within 0.25, 94% within 0.5, 96% within 1.
- They don't cover the same ground everywhere: in h/l 002400, 68% of low vertices have no high surface within 8;
  in 009700, most of high has no low within 8. So vertices with nothing close don't follow at all.

## Fall Protection tab

Generates the fall protection (f) collision of the selected collision part (ER and NR only): flat vertical walls
along the outer edges of the part's high or low collision, optionally only those facing one world direction.

- **Outer edges**: the source file's triangles (`CollisionGeometry.Submeshes`, in the part's space) are welded at 1 mm;
  an edge used by a single triangle is outer. It faces away from its triangle (the third corner, flattened into the
  world); a triangle standing straight up above its edge uses its own facing. North is +Z and east is +X; an edge
  counts for the direction nearest its facing. Vertical edges are skipped.
- **Drop Edges Only** (on by default): an outer edge keeps its wall only if its triangle faces up enough to stand on
  (world normal Y >= 0.5, slopes to about 60 degrees) and, 0.25 past its middle, the highest ground below a point
  0.5 above the edge is more than the minimum drop (default 1.0) below it, or missing. Ground is the part's own
  collision plus the same layer of every other visible, loaded collision part whose drawn bounds reach the probe
  area (`FallGround`: up-facing world triangles on a 2 m grid). This removes open seams, wall and ceiling edges.
  In Stormveil (`CollisionSurvey ER m10_00_00_00 0 fallcompare`, ground all of the map's h files): every outer edge
  gives 321,908 walls, drops only 21,121 (game: 9,594). Within 1 m of a game wall: 46% of game wall bottoms near a
  generated one with every edge, 28.5% with drops only; generated walls near a game one 2.7% -> 6.3%. The game protects
  only some drops, so this can't score the filter, only show the seams going. Without the upward-facing check the
  drop filter alone left 88,067. An open-world tile on its own shows its border as drops, which is why the tab takes
  the neighbouring parts (other tiles' collision) as ground.
- **Walls**: connected edges form runs (a run carries on through a vertex with two edges); each run of up to 32
  edges is a body. A body's extern mesh is a strip: a bottom vertex on each point and a top vertex the wall height
  (default 1.8, as every game wall) above it, along the world's up. The quad triangles are ordered as the game's.
- **File**: the part's model's f file (`f10_00_00_00_001234.hkx.dcx` for `h001234`). If the model has none, one is
  made from the first f file of the binder with a single physics system (the game has empty ones), with its bodies
  cleared. The walls replace the file's bodies, or are added to them (Keep Existing Walls), as one
  `FallProtectionAction`; undo puts the old bodies back. The f binder is written straight away.
- **Display**: the walls are previewed in the viewport (fall protection colour) while the tab is open. After
  generating, the resource's submeshes are rebuilt (`RefreshCollisionMesh` with `allowCountChange`, as the body
  count changes) and the parts drawing the file get new meshes (`AssignDrawable`). A new file is loaded by name.

### The game's f files (`CollisionSurvey ER|NR <map> 0 fall|falldetail|fallrebuild|fallcompare`)

- ER and NR: one physics system ("Default Physics System Data", one material, one motion properties), a body per
  wall run (`bodyCinfoWithAttachment`, unnamed, at the origin, material 0, filter 0 or 1, motion 0x7FFFFFFF, mass -1,
  motion properties 0xFFFF, desired body 0xFFFFFF, attached -1). Each shape is an `hknpExternMeshShape` with
  `hknpDefaultExternMeshShapeGeometry`: vertices with w 1 in bottom/top pairs exactly 1.8 apart, triangles with
  material -1, no convex radius, shape tag codec 0xFFFFFFFF, and key bits of the triangle count's bit length plus one
  (2009/2009). The shared `BuildExternMeshShape` doesn't match (log2 key bits, non-compact tree, leaf sentinel), so the
  tab builds its own.
- The simd tree is compact: node 0 an empty inner sentinel, node 1 the root (inner even for one leaf), breadth first,
  inner lanes holding child indices (0 empty), leaf lanes triangle indices (0xFFFFFFFF empty), empty boxes
  +/-float.MaxValue. The tab's tree follows this layout but not the game's exact ordering: rebuilt from each game body's
  triangles, 4349/4349 trees are valid (every triangle once, boxes hold them) and 0 identical; every other shape and
  body field equals the game's (ER m10_00_00_00, m60_42_36_00, NR m10_00_00_00). A generated file writes and reads
  back with no field differences.
- Filter 0 and 1 both appear (ER legacy dungeons: 1 only; open world and NR: both); what they block isn't known.
- The game's walls are not on the h mesh's outer edges: 1.2% of their bottom vertices lie within 5 cm of a generated
  wall. Vanilla h meshes have many open seams (h001200 of Stormveil: 20,880 outer edges), so generating from a vanilla
  mesh gives walls along those seams too. The game's walls likely follow the walkable boundary (the navmesh's).
- AC6's f files differ: `hknpCompoundShape` bodies of extern mesh instances (with plain extern bodies too), and walls
  10 high. Not supported.

## Design

- **Welding**: a compressed mesh stores triangles per section; vertices shared between sections are stored once as
  shared vertices, so decoded corners that are the same vertex decode to the same position. Corners with exactly the
  same position are welded, as `Tools/CollisionSurvey`'s `Rebuild.Decode` does, so moving a vertex moves every
  triangle using it.
- **Materials**: each triangle keeps its collision material. `m_triangleIndexToShapeKey` gives each authored triangle's
  shape key and `m_pParam.m_triangleDataArray` its material group, so the material is looked up by the key that
  `CollisionGeometry.CompressedTriangles` reports for each decoded triangle. This is the mapping `CollisionSurvey`'s
  rebuild mode checks against the game's files (`material mismatch`).

## Files

- `Editors/Map Editor/Tools/Collision Editor/CollisionEditorTool.cs`: UI, cursor, keys, commit.
- `Editors/Map Editor/Tools/Collision Editor/CollisionEditorMesh.cs`: decoded bodies (welding, raycast, falloff, preview).
- `Editors/Map Editor/Tools/Collision Editor/CollisionEditorShapes.cs`: the hk2018/hk2019 class-set code.
- `Editors/Map Editor/Actions/Map Objects/CollisionEditorAction.cs`: the undoable shape swap.
- `Editors/Map Editor/Tools/Collision Editor/CollisionEditorFallProtection.cs`: the Fall Protection tab.
- `Editors/Map Editor/Tools/Collision Editor/CollisionEditorFallShapes.cs`: outer edges, wall strips, the f bodies,
  shapes and simd tree (linked into CollisionSurvey for `fallcompare`).
- `Editors/Map Editor/Actions/Map Objects/FallProtectionAction.cs`: the undoable body swap of an f file.
- `HavokCollisionMeshBuilder.RefreshCollisionMesh`: re-uploads a loaded collision's GPU meshes from its document.
- Settings in `CFG.cs` (`MapEditor_CollisionEditor_*`, `Interface_MapEditor_Tool_CollisionEditor`), keybinds in
  `Keybinds.cs`, strings in `Map-Editor-Tools-Collision_Editor.json` and `Input-Keybinds.json`.
- **Rebuild**: `CollisionShapeBuilder.BuildCompressedMeshShape` with the old convex radius; user data and properties are
  copied from the old shape. Degenerate triangles (two corners welded together) are left out. Rebuilding requantises
  every vertex (packed step up to 0.02), so unedited vertices can move by up to about a centimetre.
- **Preview**: the GPU collision mesh is built once at load. While a file is edited, its original mesh is hidden and
  the edited triangles are drawn with `RawMeshProvider` meshes in chunks of 4096 triangles; only chunks with a moved
  vertex are rebuilt, at most once a frame.
- **No gaps while editing**: a rebuilt chunk's geometry is uploaded a frame or more after it is made, and a
  constructed renderable (`AreRenderablesConstructed`) isn't necessarily drawn yet. So a rebuilt chunk is hidden
  until it is constructed, then drawn alongside the old chunk for `OverlapFrames` (2) frames, and only then does the
  old chunk go. Swapping as soon as it was constructed left one-frame gaps (the first version flickered). A chunk moved
  again while its rebuild is on its way waits for it, then rebuilds with the latest vertices, so a long drag never
  keeps restarting the upload. The loaded mesh is only hidden once every chunk of the file has been drawn.
- **No gap on commit**: the rebuilt loaded mesh has to upload too, so a committed file's preview stays drawn (and its
  loaded mesh hidden) until the loaded mesh's renderables are constructed, or 300 frames pass. Then both are drawn
  for `OverlapFrames` frames before the preview goes. Undo and redo rebuild the loaded mesh without a preview to
  cover it, so they can still show a brief gap.
- **After commit**: the collision resource's submeshes are re-uploaded from the new document and the proxies'
  renderables rebuilt, then the preview is removed and the original mesh shown again.

## Not covered

- Navmesh and other data derived from collision (e.g. WALL files) are not updated.
- In-game behaviour of rebuilt meshes relies on the Collision Generator's builder, checked by `CollisionSurvey rebuild`.
