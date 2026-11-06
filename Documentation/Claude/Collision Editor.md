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
