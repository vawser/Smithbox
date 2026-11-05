# Model Editor Review — Bugs and Usability

Scope: `src/Smithbox.Program/Editors/Model Editor/**` (plus `ModelEntity`, `PropFinderUtil` where the editor depends on them).
Method: static read of the code. Nothing here has been run; each bug lists the steps to reproduce it so it can be confirmed in the app.

Severity key: **High** = corrupts saved data or crashes; **Medium** = wrong behaviour the user will hit; **Low** = edge case / latent.

---

## 1. Bugs

### 1.1 Data integrity (High)

> **Status (2026-11-05): B1–B3 fixed, not yet verified in the app.** `Framework/ModelIndexRemapper.cs` rewrites material/node references (meshes, bone tables, vertex bone indices / NormalW on post-0x2000D FLVERs, dummies, node links, skeleton set) after Delete and Reorder, with undo. Duplicates are appended to the end of their type, so nothing shifts. Deleting a material or node that is still referenced is refused, and the log lists the references. Reorder, Delete and Duplicate ignore the model root.

#### B1. Delete / Duplicate / Reorder do not remap FLVER indices
`UpdateFLVER()` ([ModelBank.cs:868](../../src/Smithbox.Program/Editors/Model%20Editor/Data/ModelBank.cs)) rebuilds `FLVER.Materials`, `FLVER.Nodes` etc. from `Container.Objects` in list order. Nothing rewrites the indices that point into those lists:

| Moved / removed list | Indices left stale |
|---|---|
| Materials | `Mesh.MaterialIndex` |
| Nodes | `Mesh.NodeIndex`, `Mesh.BoneIndices`, vertex bone indices, `Dummy.ParentBoneIndex` / `AttachBoneIndex`, `Node.ParentIndex` / `FirstChildIndex` / `NextSiblingIndex` / `PreviousSiblingIndex`, `SkeletonSet` bone indices |

`CloneModelObjectAction` makes this worse: it inserts the clone **directly after** the original ([CloneModelObjectAction.cs:37](../../src/Smithbox.Program/Editors/Model%20Editor/Actions/CloneModelObjectAction.cs)), so every later material/node shifts by one.

*Repro:* load a character, duplicate material 0, save, reload. Meshes that used material 1 now use the clone (material 0's copy); materials are shifted for every mesh after it. The same happens with a node: skinning breaks.

*Fix direction:* append clones at the end of their category; for delete/reorder, build an old→new index map and rewrite the references above inside the action (and undo it on Undo), or block delete/reorder of Materials/Nodes that are still referenced.

#### B2. Reorder with the model root selected overwrites the first object
After a load the root is selected by default ([ModelUniverse.cs:95](../../src/Smithbox.Program/Editors/Model%20Editor/ModelUniverse.cs)). The root isn't in `Container.Objects`, so `OrderModelObjectAction` finds no category and falls back to `indexStart = 0`, `indexEnd = Count-1` ([OrderModelObjectAction.cs:99-107](../../src/Smithbox.Program/Editors/Model%20Editor/Actions/OrderModelObjectAction.cs)).
- **Move to Top** → `Container.Objects[0] = root`, so the first object (usually the first material, or the CLM2/HKX entry) is dropped from the saved FLVER.
- **Move to Bottom** → the loop reads `Objects[i + 1]` up to `i == Count-1`, which throws `ArgumentOutOfRangeException`.

*Repro:* load any model and, without clicking anything, press the Reorder Top keybind (or Edit → Move to Top), then save.

#### B3. Undo then Redo of a reorder throws
`Undo()` sets `Container.Objects = StoredObjectOrder`, so both names point at one list. `Execute()` on redo then runs `foreach (var entry in Container.Objects) StoredObjectOrder.Add(entry)` — adding to the list it's iterating → `InvalidOperationException` ([OrderModelObjectAction.cs:59-62, 223](../../src/Smithbox.Program/Editors/Model%20Editor/Actions/OrderModelObjectAction.cs)).
*Fix:* capture the order once (`if (StoredObjectOrder.Count == 0)`), and on Undo copy the stored list's contents back in (`Objects.Clear(); Objects.AddRange(...)`) instead of swapping the list.

### 1.2 Multi-view (High / Medium)

> **Status (2026-11-05): B4, B5, B10 fixed (plus B6–B9, B11, B19), not yet verified in the app.** Loading and unloading go through `ModelEditorView.LoadModel`/`UnloadModel`. `ModelWrapper.Unload` acts on the view that owns the container (`Container.View`). A model open in another view is refused and tagged `[Open in View N]` in the File List. Closing a view unloads its models (unsaved edits stay in the bank). The auto-load now runs from `ModelEditorView.Display` via `ModelFileList.ProcessPendingLoad`, so it no longer depends on the File List being drawn.

`ModelWrapper.Container` lives on the shared bank, but each view has its own `RenderScene`, selection and history. `ModelWrapper.Load()`/`Unload()` always act on `ViewHandler.ActiveView` ([ModelBank.cs:419-426, 853-865](../../src/Smithbox.Program/Editors/Model%20Editor/Data/ModelBank.cs)), not on the view that owns the container.

#### B4. The same model in two views breaks the first view
View A loads `c1000`. View B selects `c1000`: `Universe.LoadModel` logs "already loaded" and stops, so B shows nothing. If B then picks another model, `Unload()` tears down **A's** container through B's universe. A's viewport goes blank and its panels say "No selection", even though A still has the model selected.

#### B5. Closing a view orphans its model
`RemoveView` ([ModelViewHandler.cs:81](../../src/Smithbox.Program/Editors/Model%20Editor/Framework/ModelViewHandler.cs)) only nulls the slot. The loaded wrapper keeps `Container != null`, so that model reports "already loaded" in every other view. The view's `RenderScene` / viewport resources are never released either.
*Fix direction for B4/B5:* store the owning view on the container (or wrapper) and route Load/Unload through it; unload the view's model in `RemoveView`; refuse, or clone, when the model is already open in another view.

### 1.3 Loading (Medium)

#### B6. Picking a model does nothing until the File List is visible
Source List only sets `FileList.ApplyAutoLoadFirst/ApplyAutoSelectPass` ([ModelSourceList.cs:316-326](../../src/Smithbox.Program/Editors/Model%20Editor/GUI/ModelSourceList.cs)). Those flags are only read inside `ModelFileList.DisplayModelSelectionList()`. In **Tabbed Panels** mode (Source List and File List are tabs in the same panel), or when the File List window is a hidden dock tab, the model doesn't load until you open that tab.
*Fix:* move the auto-load step out of the File List's draw code into `ModelEditorView.Display` (or into `LoadModel` itself).

#### B7. "Require double click" still loads on single click
[ModelSourceList.cs:257-263](../../src/Smithbox.Program/Editors/Model%20Editor/GUI/ModelSourceList.cs) checks `ImGui.IsMouseClicked` rather than `IsMouseDoubleClicked`, so the toggle has no effect.

#### B8. Auto-load loads single-file containers twice
With *Auto-Select First Entries* on and a container holding one FLVER, both the `ApplyAutoLoadFirst` and `ApplyAutoSelectPass` blocks run in the same frame. That's a full Unload → Load → Unload → Load cycle, including GC and `ClearUnusedResources`.

#### B9. Asset models are gated on the texture toggle
[ModelUniverse.cs:196](../../src/Smithbox.Program/Editors/Model%20Editor/ModelUniverse.cs) reads `ModelEditor_TextureLoad_Objects` where it should read `ModelEditor_ModelLoad_Objects`. Turning off asset *textures* also stops the asset *model* list from being built, and the "Objects" model toggle does nothing.

#### B10. File List → "Unload" on a model that isn't loaded drops the loaded one
[ModelFileList.cs:173-178](../../src/Smithbox.Program/Editors/Model%20Editor/GUI/ModelFileList.cs) sets `SelectedModelWrapper = null` before unloading the clicked wrapper. Right-clicking a *different* entry and choosing Unload clears the selection without unloading the real model. That model's container is orphaned (see B5), it stays drawn, and Save does nothing.

#### B11. `ModelUniverse` overwrites the user's "Enable Rendering" preference
The constructor sets `CFG.Current.Viewport_Enable_Rendering = true` because `RenderScene` is never null ([ModelUniverse.cs:48-55](../../src/Smithbox.Program/Editors/Model%20Editor/ModelUniverse.cs)). Every new Model Editor view, including the one created at startup, turns rendering back on for all editors.

### 1.4 UI / crashes (Medium / Low)

> **Status (2026-11-05): B12–B18 and B20 fixed, not yet verified in the app.** Also fixed: the export tool's mesh checkboxes were never passed to the OBJ export, so every mesh was always exported. `FlverToObj.Convert` now takes the selection. B15 adds a *Refresh and Search* button and re-reads the maps when *Target Project Files* changes; the scan still runs on the UI thread. B21 is still open.

#### B12. Export tool crashes after saving a model with an added/removed mesh
`SelectAndLoadCurrentFlver` keeps a reference to `wrapper.FLVER` and sizes `FlverMeshSelection` once. Saving runs `UpdateFLVER`, which `Clear()`s and refills the same `Meshes` list. If the count changed (duplicate or delete a mesh), [ModelExportTool.cs:93-99](../../src/Smithbox.Program/Editors/Model%20Editor/Tools/Model%20Export/ModelExportTool.cs) indexes past the array → `IndexOutOfRangeException` on the next frame. Before saving, the export also silently ignores unsaved add/delete edits.

#### B13. `model/select` command reads `commands[2]` without a length check
[ModelCommandQueue.cs:116](../../src/Smithbox.Program/Editors/Model%20Editor/Framework/ModelCommandQueue.cs) checks only `Length <= 1`, and `int.Parse` throws on bad input. A malformed `model/select/dummy` command crashes the editor. Use `TryParse` and check `Length > 2`.

#### B14. "Apply to all" visibility misses nested entries
[ModelContentsView.cs:626](../../src/Smithbox.Program/Editors/Model%20Editor/GUI/ModelContentsView.cs) iterates `container.RootObject.Children`. Nodes with a parent and dummies attached to a bone aren't root children, so modifier-clicking the eye skips most bones and dummies. Iterate `container.Objects` instead.

#### B15. Instance Finder never re-reads maps
`SetupSearch` is cleared after the first search ([ModelInstanceFinder.cs:122-126](../../src/Smithbox.Program/Editors/Model%20Editor/Tools/ModelInstanceFinder.cs)). Toggling *Target Project Files* afterwards, or editing a map, has no effect until restart. The first search also reads every MSB on the UI thread (a long freeze on ER), and DB returns nothing (TODO).

#### B16. `GRASS` sets `EDGE_Parent`
Copy-paste slip in [ModelContainer.cs:119](../../src/Smithbox.Program/Editors/Model%20Editor/Data/ModelContainer.cs). It's harmless today because nothing reads `EDGE_Parent`, but it'll bite once something does.

#### B17. List-of-list properties are drawn twice
In `PropEditorGenericList` the second branch is `if` rather than `else if` ([ModelPropertyView.cs:664](../../src/Smithbox.Program/Editors/Model%20Editor/GUI/ModelPropertyView.cs)). A `List<List<T>>` element passes both checks: it's drawn twice, with duplicate ImGui IDs and two Remove buttons. The nested-list variant already uses `else if`.

#### B18. Stray `End()` / `PopStyleColor(2)` in the property view
[ModelPropertyView.cs:127-133](../../src/Smithbox.Program/Editors/Model%20Editor/GUI/ModelPropertyView.cs): when the selected entity has a null `WrappedObject`, the code ends the *Properties window* and pops two colours it never pushed, which corrupts the ImGui stack. You can't reach it from the tree today (null skeletons are filtered out by the type cache), but any other selection path, such as a command or the viewport, would assert.

#### B19. Context popup closed with `EndMenu()`
[ModelViewHandler.cs:139-155](../../src/Smithbox.Program/Editors/Model%20Editor/Framework/ModelViewHandler.cs) pairs `BeginPopupContextItem` with `ImGui.EndMenu()`. It should be `EndPopup()`.

#### B20. Type column shows the owning class, not the field type
Value rows print `prop.ReflectedType.FullName` ([ModelPropertyView.cs:457-462](../../src/Smithbox.Program/Editors/Model%20Editor/GUI/ModelPropertyView.cs)), and array/class rows print the parent `type.FullName`. Every row of a Material says `SoulsFormats.FLVER2+Material`. "Copy Type" does the same. You almost certainly want `prop.PropertyType`.

#### B21. Smaller items

> **Status (2026-11-05): all fixed, not yet verified in the app.** Notes: copied meshes are drawn from the submesh of the mesh they were copied from (their geometry is identical). Nested edits use the object as drawn when one entity is selected; a multi-selection still searches each entity by index.

- `DeriveMapID` checks `parts.Length >= 3` and then reads `parts[3]` (and `>= 2` before `parts[2]`, and `parts[3]` for DS2). Each guard is off by one ([ModelBank.cs:178-194](../../src/Smithbox.Program/Editors/Model%20Editor/Data/ModelBank.cs)).
- `ModelUniverse.Tasks` is never cleared, so it grows by up to 9 completed tasks per model load for the life of the view. `LoadModel` is `async void`, so a fast model switch can't cancel an in-flight load.
- Delete / Duplicate with only the root selected still runs an action. That marks the model dirty (`*`), and Duplicate inserts a clone of the root's transform node into `Objects`.
- Duplicated entities get no `RenderSceneMesh` (`ModelEntity.DuplicateEntity` only wraps the object). Cloned meshes, dummies and bones are invisible and can't be picked in the viewport until reload.
- `PropertyRow` shows "ImplementMe" for `Vector4`, `double`, `ulong`, enums with `short`/`ushort`/`long` underlying types, etc.
- Nested property edits find their target again by reflection from the entity root, with a single index (`PropFinderUtil.GetPropData`). For objects nested two or more list levels deep (HKX trees), the outer index is lost, so an edit can land on the wrong element. This one needs a targeted test.

---

## 2. Usability improvements

Ordered by likely value to users.

> **Status (2026-11-05): items 1–14 done except the "ask before Unload" part of item 10 and the per-instance list in item 12; none verified in the app yet.** Item 1 was covered by B1, and item 14 by the B4/B5 fix.

1. **Make reordering, deleting and duplicating Materials/Nodes safe** (see B1). Until index remapping exists, at least warn before Delete/Reorder on a referenced material or node.
2. **Consistent load gesture.** Source List loads on one click; File List needs a double click (and its Load context item). Pick one rule, honour the double-click toggle in both, and fix B7.
3. **Live filtering in the Source List.** The filter only applies on Enter or loss of focus (`IsItemDeactivatedAfterEdit`), unlike the Contents and Properties filters, which update as you type.
4. **Frame the whole model.** With the root selected (the default after load), Frame does nothing because the root has no `RenderSceneMesh`. Use `ModelContainer.TryGetMeshBounds` for the root, and combine bounds for multi-selections (currently only the first entity is framed).
5. **Show the bone hierarchy.** `HierarchyView` / the `hierarchial` path in `ModelObjectSelectable` exist but `TypeView` never uses them, so Nodes are a flat list. A "Hierarchy" toggle for Nodes (with dummies under their bones) would match how rigs are read.
6. **Go To should open the target's category.** `GotoTreeTarget` only scrolls when the item is drawn, and it's cleared every frame. If the Dummy/Node/Mesh category is collapsed, Go To (and the binocular buttons in the property view) silently do nothing. Call `SetNextItemOpen(true)` on the category containing the target.
7. **Select the duplicates after Duplicate**, as the Map Editor does. Currently the originals stay selected, so a follow-up edit hits the source.
8. **Explain empty states.**
   - A multi-selection of mixed types shows an empty Properties panel (`PropertyTreeHeader` returns silently). Show "Selection contains N types — select one type to edit".
   - Reorder with more than one item selected only logs an error. Disable the menu items instead, and disable Reorder/Delete/Duplicate when only the root is selected.
9. **Contents filter.** Hide categories with no matches and show match counts. Today every category header stays visible even when empty.
10. **Unsaved work visibility.** Dirty markers exist per container and per file, but Save only saves the selected model. Consider showing "N unsaved models" in the File menu next to *Save All*, and asking before *Unload* drops a dirty model's undo history (history is cleared on every model switch).
11. **Param references in properties.** `ParamRefRow` only works when the view is a `MapEditorView`, so `ParamRef` metadata never shows in the Model Editor. Add a `ModelEditorView` branch.
12. **Instance Finder.** Run the scan off the UI thread with a progress line, add a *Refresh* button, and list each instance (not only the first per map) so you can jump to a specific part.
13. **Discoverability of `val:` search** in the Properties filter. Mention it in the filter tooltip.
14. **Multi-view guard.** Until B4/B5 are fixed, grey out or tag (e.g. "open in View 0") models already loaded in another view.

---

## 3. Suggested fix order

| Order | Items | Why |
|---|---|---|
| 1 | B2, B3, B1 (at least clone-append + block remove/reorder of referenced items) | Silent data corruption in saved files |
| 2 | B4, B5, B10 | Lost viewport state, orphaned containers |
| 3 | B6, B7, B8, B9, B11 | Loading behaviour users hit daily |
| 4 | B12, B13, B14, B15, B17-B20 | Crashes and incorrect UI |
| 5 | Usability list | Polish |

## 4. What to check in the app (to confirm)

- **B1:** Duplicate material 0 on a model with several materials, save, reload. Mesh materials shift.
- **B2:** Load a model, press Reorder Top straight away, save. The first material is gone. Press Reorder Bottom → exception in the log.
- **B3:** Select a mesh, Move Down, Undo, Redo → exception.
- **B4:** Add a second view, load the same model in both, then pick another model in view 2. View 1 goes blank.
- **B6:** Switch View → Tabbed Panels, click a character in the Source List. Nothing loads until you open the File List tab.
- **B7:** Toggle the Source List load mode to double click. A single click still loads.
- **B9:** Turn off Asset texture loading. Asset models stop loading too.
- **B12:** Export tool → Select Current FLVER, duplicate a mesh, save → exception.
