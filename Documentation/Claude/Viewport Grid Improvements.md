# Viewport Grid Improvements

Survey of the viewport grid shared by the Map, Model and Time Act editors.

Files:
- `src/Smithbox.Program/Editors/Viewport/Tools/ViewportGrid.cs` (per-frame update, world matrix)
- `src/Smithbox.Program/Editors/Viewport/Tools/ViewportGridTool.cs` (tool window UI)
- `src/Smithbox.Program/Editors/Viewport/Tools/ViewportGridConfig.cs` (CFG ref mapping)
- `src/Smithbox.Program/Renderer/Debug Primitives/DbgPrimGridPlane.cs` (quad)
- `src/Smithbox.Data/Assets/Shaders/ViewportGrid.vert/.frag` (procedural lines)
- `src/Smithbox.Program/Editors/Map Editor/Tools/Actions/AdjustToGridAction.cs` (snap to grid)

## A. Bugs

1. **Snap Y uses the Z checkbox** - `AdjustToGridAction.SnapPositionToGrid`:
   `snappedY = applyZ ? ...` should be `applyY`.
2. **Hotkey snaps use the wrong grid's transform** - `GetGridTransform` reads the
   `CurrentTargetGrid` field (the popup combo) instead of the grid passed to
   `AdjustSelectionToGrid`. "Move to Secondary Grid" uses the Secondary flags but the
   Primary origin, rotation and square size unless the popup was last set to Secondary.
3. **Rotation order differs between drawing and snapping** - the grid is drawn with
   `Transform.EulerToQuaternion` (Y * Z * X), the snap uses
   `Quaternion.CreateFromYawPitchRoll` (Y * X * Z). With rotation on two or more axes,
   objects snap to a plane that is not the one drawn. Build the snap matrix from the
   same `Transform` used in `ViewportGrid.Update`.
4. **Time Act shares the Model Editor's grid settings** - `ViewportGridConfig.Get` maps
   `TimeAct` to the `ModelEditor_*` fields. Toggling or moving the grid in one editor
   changes the other. Either intended (document it) or add `TimeActEditor_*` fields.

## B. Rendering (shader)

1. **Major lines** - every Nth line (configurable, default 10) drawn brighter/thicker.
   Cheap: a second `fract` on `cell / N`. Keeps scale readable when minor lines fade out.
2. **Axis lines** - draw the local X and Z lines through the origin in red/blue
   (or the gizmo colours) so the grid's orientation is obvious after rotating it.
3. **Distance fade** - fade alpha by distance from the camera (or from the grid centre)
   so a large grid doesn't end in a hard edge on the horizon. Needs the camera position,
   or view-space depth passed from the vertex shader.
4. **Line thickness** - the line width is fixed at ~1 pixel (`min(pixelsToLine, 1)`).
   Add a width in pixels to the material (`pixelsToLine / width`).
5. **Opacity** - colour is `ColorEdit3`, so alpha is always 255. Switch to `ColorEdit4`
   (Vector4 config) to allow a faint grid.
6. **Z-fighting / hidden grid** - a grid placed on a floor flickers with it. Options:
   a small depth bias, or a "draw over geometry" mode that draws the occluded part at
   low alpha (two passes or depth test off + reduced alpha where depth fails).
7. **Follow camera (infinite grid)** - option to recentre the quad under the camera,
   snapped to the square size so lines don't slide. Useful in large maps where a fixed
   grid is either too small or needs a huge `Size`.

## C. Tool window UX

1. **Grid movement mode** - replaces the position/rotation inputs, see section E.
2. **Grid selector** - three "view X grid" buttons with no indication of which is
   current. A combo or tab bar showing the current grid (and its visibility) is clearer.
3. **Per-grid visibility checkbox** next to each grid, rather than a toggle button that
   only affects the current one.
4. **Placement actions**:
   - Move grid to selection (position only / position + rotation)
   - Move grid to camera focus point
   - Reset grid to defaults
5. **Input validation** - clamp `SectionSize` and `Size` in the UI, not only in
   `ViewportGrid.Update`, so the displayed value matches what is drawn.
6. **Size label** - `Size` is the half-extent in squares (the plane spans
   `-Size..Size`); the tooltip should say so, or show the resulting width in metres.

## D. Integration

1. **Gizmo snapping** - snap translation increments to the active grid's square size
   (in grid space) while a modifier is held, instead of only the after-the-fact
   "Adjust to Grid" action.
2. **Snap for Model/Time Act** - `AdjustToGridAction` exists only in the Map Editor.
3. **Config dedup** - `ViewportGridConfig.Get` and `AdjustToGridAction` repeat the same
   fields for each grid. A `ViewportGridSettings` class with three instances per editor
   in CFG would remove ~300 lines of switch/if chains and make bugs A1/A2 impossible.
   (Requires a CFG migration for existing settings.)

## E. Grid movement mode (gizmo)

Replaces the six position/rotation `InputFloat`s in `ViewportGridTool` with a toggle
button. While on, the viewport gizmo is drawn at the centre of the current grid and
drags move/rotate the grid instead of the selection.

### Current gizmo coupling

`Gizmos.Update` (`Editors/Viewport/Framework/Gizmos.cs`) is hard-wired to the selection:
- `OriginalTransform` is built from the selected entities.
- Visibility requires `IsFilteredSelection<Entity>()`.
- On release it builds `ViewportAction`s from `ProjectTransformDelta(sel)`.

The drag maths in between (axis projection, rotation angle) only uses
`OriginalTransform` / `CurrentTransform` and is reusable as-is.

### Design

1. **Gizmo target abstraction** - add an `IGizmoTarget` with:
   - `bool HasTarget`
   - `Transform GetTransform()` (gizmo origin + orientation)
   - `void Preview(Transform current)` (called every drag frame)
   - `void Commit(Transform original, Transform current)` (on release)
   - `bool AllowScale`

   `SelectionGizmoTarget` wraps the existing selection code unchanged
   (`SetTemporaryTransform` / `ViewportCompoundAction`). `GridGizmoTarget` writes to the
   grid's CFG fields through `ViewportGridConfig`. `Gizmos` keeps a `Target` property;
   `VulkanViewport` picks the grid target while grid movement mode is on.
2. **Grid target behaviour**
   - Origin = grid position; orientation = `Transform.EulerToQuaternion(rotation)`, the
     same conversion `ViewportGrid.Update` draws with.
   - Preview writes position directly, and rotation back as degrees via
     `Transform.QuaternionToEuler` (the inverse of `EulerToQuaternion`'s Y*Z*X order).
     The grid updates live because `ViewportGrid.Update` reads CFG every frame.
   - Scale mode is ignored (or falls back to translate) while targeting the grid.
   - The gizmo space is forced to local, so translating along an arrow moves the grid
     within its own plane after rotating it.
3. **Tool window**
   - "Move Grid" toggle button (highlighted while active) under the grid selector;
     it acts on the grid currently shown in the tool (Primary/Secondary/Tertiary).
   - Read-only position/rotation text below it for reference.
   - "Reset Position" / "Reset Rotation" buttons, plus "Move Grid to Selection"
     (C4) so exact placement is still possible without typed values.
4. **Mode exit** - turning the toggle off, hiding the grid, switching editor or
   switching the grid selector ends the mode. Selection clicks in the viewport still
   work; the gizmo stays on the grid until the mode ends.
5. **Gizmo hotkeys** - the existing translate/rotate mode keybinds switch the grid
   gizmo between move and rotate, so no new keybinds are needed.
6. **Snapping** - optional: while dragging, round translation to the grid's own
   square size and rotation to 15 degree steps when a modifier is held.

### Decision: no undo

Grid moves are not undoable. Drags write straight to CFG and `Commit` does nothing
beyond triggering the picking cooldown; the Reset / Move to Selection buttons cover
recovery.

## Status (2026-11-04)

- A1-A3 fixed in `AdjustToGridAction` (the snap now builds its grid transform the same way
  `ViewportGrid` draws it).
- E implemented: `IGizmoTarget` / `SelectionGizmoTarget` (`Viewport/Framework/GizmoTargets.cs`),
  `ViewportGridMovement` + `ViewportGridGizmoTarget` (`Viewport/Tools/ViewportGridMovement.cs`),
  tool window Grid Placement section, `VulkanViewport.UpdateGizmoTarget`. Drag snapping (E6) not done.
- B1 + B2 implemented as shared settings (`Viewport_Grid_Major_Line_Interval`,
  `Viewport_Grid_Display_Axis_Lines`), passed in the spare `DbgMaterial` slots. Axis colours
  are fixed in the shader (material is 16 bytes). Per-grid versions wait for D3.
- B3 + B5 implemented as shared settings (`Viewport_Grid_Opacity`, `Viewport_Grid_Fade_Distance`).
  Opacity is the material colour's alpha; the fade uses view-space distance from the vertex
  shader. The major interval moved into the high 16 bits of the grid flags to free a slot.
- B6 implemented as a pipeline depth bias on `DbgPrimGridPlane` (constant 1024, slope 2, same as
  mesh decals), via new `IDbgPrim.DepthBiasConstant` / `DepthBiasSlope`. The "draw over
  geometry" mode was not added.
- D3 implemented: `ViewportGridSettings` / `ViewportGridSet` (`Viewport/Tools/ViewportGridSettings.cs`)
  replace the 81 flat CFG fields with `MapEditor_Grids`, `ModelEditor_Grids`, `TimeActEditor_Grids`.
  `ViewportGridSettingsMigration` reads the old fields from the raw JSON in `CFG.Load` when a set is
  missing. Checked against the real config with `Tools/GridSettingsMigrationCheck` (all passed).
- A4 resolved: Time Act has its own grids (copied from the Model Editor's on migration) and a Grid
  Configuration section in its tool window.
- D1 + E6 implemented: hold Ctrl while dragging the gizmo. Translation snaps the gizmo pivot to the
  square corners of the grid shown in the grid tool (only the grid axes the drag runs along, grid
  captured at drag start so a grid moving itself snaps to its start). Rotation snaps to
  `Viewport_Gizmo_Snap_Rotation_Step` (default 15, slider in the Gizmos menu).
- New-config default for the Map secondary grid is now Rotation X 90 (was Position X 90, a typo);
  existing configs keep their values.

## Suggested order

A1-A3 (small, real bugs) -> E (grid movement mode) -> B1 + B2 (largest readability
gain) -> C2/C4 -> D3 refactor before adding any further per-grid settings.
