# Collision Maker

A Map Editor tool (Tools window) for reshaping existing map collision by moving its vertices in the viewport.

## Scope

- **Games**: ER, NR and AC6. These are the games whose map collision the Map Editor holds as editable documents
  (`HavokCollisionBank.MapDocuments`) and writes back (`HavokCollisionBank.SaveMapCollision`). Their meshes are
  `fsnpCustomParamCompressedMeshShape` (hk2018 / hk2019), which the shared `CollisionShapeBuilder` rebuilds.
  SDT holds its collision (`TagContainers`) but has no collision save path; DS3/BB map collision isn't held at all.
- **Shapes**: bodies whose shape is a FromSoftware compressed mesh. Other bodies of the file are kept as they are.
- **Types**: every displayed collision type (high, low, fall protection) can be edited. Each file is edited on its own.

## Workflow

1. **Start Collision Edit** turns on the cursor. **Stop Collision Edit** turns it off; pending edits are kept.
2. The cursor is a sphere that sits where the mouse ray hits the nearest displayed collision. The vertex of the hit
   triangle nearest the hit point is the target, marked by a second, smaller sphere.
3. Keys, while the mouse is over the viewport and no mouse button is held (so the camera's Q/E flight still works):
   - **Q** raises the target vertex by the step, **E** lowers it. Held, they repeat.
   - **G** held drags the target vertex across the horizontal plane at its height, following the mouse.
   The keys are bindings (`MapEditor_CollisionMaker_Raise/Lower/Drag`), so they can be changed in the keybind settings.
4. A **Radius** above zero moves the vertices around the target as well, with a smooth falloff.
5. **Commit Changes** (valid once something is edited) rebuilds each edited file's meshes from the edited vertices,
   puts them in the document as an undoable action (marking the map's collision as modified), writes the collision
   binder into the project, and refreshes the displayed mesh.
6. **Discard Changes** drops the pending edits.

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

- `Editors/Map Editor/Tools/Collision Maker/CollisionMakerTool.cs`: UI, cursor, keys, commit.
- `Editors/Map Editor/Tools/Collision Maker/CollisionMakerMesh.cs`: decoded bodies (welding, raycast, falloff, preview).
- `Editors/Map Editor/Tools/Collision Maker/CollisionMakerShapes.cs`: the hk2018/hk2019 class-set code.
- `Editors/Map Editor/Actions/Map Objects/CollisionMakerAction.cs`: the undoable shape swap.
- `HavokCollisionMeshBuilder.RefreshCollisionMesh`: re-uploads a loaded collision's GPU meshes from its document.
- Settings in `CFG.cs` (`MapEditor_CollisionMaker_*`, `Interface_MapEditor_Tool_CollisionMaker`), keybinds in
  `Keybinds.cs`, strings in `Map-Editor-Tools-Collision_Maker.json` and `Input-Keybinds.json`.
- **Rebuild**: `CollisionShapeBuilder.BuildCompressedMeshShape` with the old convex radius; user data and properties are
  copied from the old shape. Degenerate triangles (two corners welded together) are left out. Rebuilding requantises
  every vertex (packed step up to 0.02), so unedited vertices can move by up to about a centimetre.
- **Preview**: the GPU collision mesh is built once at load. While a file is edited, its original mesh is hidden and
  the edited triangles are drawn with `RawMeshProvider` meshes in chunks of 4096 triangles; only chunks with a moved
  vertex are rebuilt, at most once a frame.
- **After commit**: the collision resource's submeshes are re-uploaded from the new document and the proxies'
  renderables rebuilt, then the preview is removed and the original mesh shown again.

## Not covered

- Navmesh and other data derived from collision (e.g. WALL files) are not updated.
- In-game behaviour of rebuilt meshes relies on the Collision Generator's builder, checked by `CollisionSurvey rebuild`.
