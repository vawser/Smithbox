# Map Editor Review — Bugs and Usability

Date: 2026-11-05
Scope: `src/Smithbox.Program/Editors/Map Editor` (~43k lines). I read the core files in full: the screen, the view handler, the universe, the editor view, the viewport view, the map list, the content tree, the object actions and their tool wrappers, the command queue and the rename and property paths. I sampled the rest. Nothing was run. Each finding comes from reading the code.

Verdicts:
- **Confirmed**: the code path was traced end to end.
- **Plausible**: the code reads that way, but it depends on runtime state I could not see (ImGui ID stacks, thread timing, crash handler).

---

## Fix status (2026-11-05)

B1–B7, B9, B16 and B17 are fixed in the working tree (not committed). The project builds, but nothing has been tested in the app.

| # | Fix |
|---|---|
| B1 | `EntReorderAction` finds the selection's real index and the positions of every object of its type, wherever they are in the list. Up/Down swap with the neighbouring object of the same type. Top/Bottom remove the selection and re-insert it before the first or after the last object of its type. Nothing else in the list is overwritten. If the selection is already at the end it moves towards, the action does nothing (and its undo is a no-op). |
| B2 | `DuplicateAction` and `MapGroupsView.DuplicateMapGroupEntry` pass `targetMap: null, targetBTL: null`, so every clone goes into its source's map and its source's parent (lights stay in their own BTL). Both paths no longer read `SelectedMapContainer`, which removes the null crash. |
| B3 | `EntDuplicateAction.GetInsertIndex`: a clone goes after its source when the source is in the target map. Otherwise it goes after the last object of the same type, or at the end if there is none. Name uniqueness is now keyed by the target map, and Duplicate-to-Map parents the clone under the container looked up by map ID instead of the possibly stale one passed in. |
| B4 | Multi-rename builds the names in the same order as the entity list, as `{name}-{Type}_{n}` with `n` counting within each type. |
| B6 | New `MapSelection.ResolveTargetMap`: it looks up a tool's target map by ID and returns `null` when nothing is picked or the map has been unloaded since. Create (all four uses, plus the shortcut path), Duplicate-to-Map, Move-to-Map, Entity ID Check and Rename Map Objects use it instead of the broken `(null, null)` check or the stored container. Duplicate-to-Map with no target now logs an error instead of duplicating in place, and it ignores a BTL picked for a different map. |
| B7 | The viewport context menu, Quick View and the Model Selector take the first `Entity` in the selection (`OfType<Entity>().FirstOrDefault()`) instead of hard-casting the first item. When there is none, the entity-specific items are skipped (context menu), nothing is shown (Quick View) or "select a valid map object" is shown (Model Selector). |
| B9 | `HandleSelectCommand` now only runs for `select`, and `HandleLoadCommand` checks the argument count. There is now a `map/idselect/{kind}/{mapID}/{entityID}` handler for the Param Editor's "View in Map" buttons (bonfires, lamps, bosses, spirit summon stones). It selects the object with that entity ID, preferring an enemy part for the `enemy` kind. Commands arrive one per frame, so a `select` or `idselect` sent right after a `load` used to arrive while the map was still loading and find nothing. Both now wait: the selection is kept as pending until no map is loading, then resolved, the map is made the content tree's map, and the viewport frames the object. The pending selection starts the load itself if no load is under way, or if the `load` command was refused because another map was loading. It gives up with an error after 2 minutes. A newer request replaces an older pending one. |
| B16 | Map List Filter patterns are compiled once each and cached. An invalid pattern is logged once and matches nothing. |
| B17 | Select by ceremony returns with an error when the selection has no map objects. |
| B5 | `EntRenameAction` now uses its `reference` flag (`MapEditor_Properties_Enable_Referenced_Rename`). Before renaming, it collects every `[MSBReference]` field (a string, or an element of a `string[]`/`List<string>`) in the same map that holds the old name of a renamed object of the field's `ReferenceType`. It rewrites them to the new name and keeps their old values for undo. The referring objects are marked dirty and the map's reference maps are rebuilt. Like `CollectReferenceNames`, it only looks at top-level properties, and references from other maps are not updated. |

## Usability status (2026-11-05)

U1–U10 are implemented (not committed), along with applying Map List filter edits when an input is deactivated. Design notes are in `Map Editor Usability Plan.md`. Nothing has been tested in the app.

| # | What changed |
|---|---|
| Filter | Map List Filter → Edit now edits a working copy taken when the set's menu opens. Each entry is applied to the live filter when its input is deactivated, not on every keystroke. The menu's Name and Match Type edits now stick: they used to be reset every frame, and the combo used to show the Create menu's type. Deleting the active filter clears it. Toggling the search bar's exact-match checkbox re-filters the list (B15). |
| U1 | When the selection changes, the selected object's map becomes the active map (content tree, Paste, tools set to "Active Map"). New viewport overlay line "Active Map: …", toggled under Overlay. |
| U2 | Shared `MsbReferences` scanner, also used by rename. Delete and Move to Map list the references that would break and offer Clear / Keep / Cancel; clearing is part of the same undo step (`ClearReferencesAction`). The save-failure dialog names the field and the missing name, and offers Clear (undoable) / Select / Cancel. |
| U3 | Per-map undo. `ViewportAction.TouchedScopes` holds the IDs of the maps the action changed, recorded through `MapContainer.MarkDirty` while the action runs. `UnloadMap` calls `RemoveActionsTouching(mapID)` and only takes that map's objects out of the selection. Actions that recorded no map are dropped on any unload. Unload Current / Unload All / Unload Map (Map List, References To) and closing a view now offer Save / Discard / Cancel, listing the unsaved maps. Global Mass Edit only unloads the maps it loaded. It also no longer leaves rendering off and save exceptions ignored when nothing matches (it had an early return before the restore). Unload All runs one GC collection instead of two per map (B27). |
| U4 | Create places objects at the placement orb, in coordinates local to their parent. The Create shortcut no longer needs a selection, and Create's target map defaults to the active map. New option "Paste at Placement Orb" (on by default): pasted objects keep their layout, centred on the orb, in the paste's undo step. "Place at List End" now works (B10). |
| U5 | Map loads requested during another load are queued, not refused. The Map List shows "Loading … (n queued)" and marks rows (Loading…) / (Queued). `IsLoading` is a counter, so concurrent "Load Related Maps" loads no longer clear it early (B22). `LoadMap` no longer reloads collision for a map that is already loaded or loading (B21). `map/load` and "Load Map" from References select the map once it has loaded. |
| U6 | `GetMapContainerFromMapID` is a dictionary lookup. The content tree's sorted categories are cached until objects are added or removed. Plain type lists of 100+ objects are clipped, with the Goto target kept in range. The Map List's filtered entries are rebuilt only when the search, filters, toggles or loaded maps change. |
| U7 | A right-click with nothing selected offers Create New Object, Paste, Frame Active Map and Show All Objects. With a selection, the menu takes a part or region as its representative, so selections that start with an event still show the transform items. (Those items already acted on the whole selection; the representative only decides which are shown.) |
| U8 | Reorder works on multiple selections, across types and maps: the selected objects of each type move as a block. The Edit-menu items are disabled when nothing selected can be reordered. |
| U9 | Shared `MapTargetPicker` (with an "Active Map" entry) in Create, Duplicate to Map, Move to Map, Prefab, Entity ID Check and Rename Map Objects. Create, Prefab, Entity ID Check and Rename default to the active map. Window and tab titles go through `LOC`, with unchanged English text and IDs, so dock layouts are kept. The Edit menu says "Duplicate to Map" with the shortcut hint. The Move to Map button reads "Move" (B13). The Duplicate-to-Map shortcut and context item work with the Tools window hidden (B12): the action popups are drawn by `MapEditorView`. |
| U10 | An autosave failure only logs. A map whose last save failed is shown in red in the Map List, with the error in the tooltip, until it saves. |

---

## 1. Bugs

### High: data loss or corrupt saves

#### B1. "Move to Top/Bottom" overwrites another object when the selection is outside its type's first run (Confirmed)
`Actions/Map Objects/EntReorderAction.cs:72-204`

- The action takes the type's span as the first contiguous run of `map.Objects` with the same `WrappedObject` type (`indexStart`..`indexEnd`).
- New objects are not added inside that run:
  - `EntAddAction` (Create) appends with `Map.Objects.Add` (`EntAddAction.cs:79`).
  - `EntMoveAction` (Move to Map) appends with `Target.Objects.Add` (`EntMoveAction.cs:81`).
  - Paste and Duplicate-to-Map insert at index 0 (see B3).
- If the selected object is outside the run, `rootIndex` stays `0`:
  - **Top**: `Objects[indexStart] = curSel` overwrites whatever was there. That object disappears from the list and the selection now appears twice.
  - **Bottom**: the loop shifts `Objects[0..indexEnd]` up by one, which drops `Objects[0]`, then writes `curSel` at `indexEnd`.
- `SerializeToMSB` (`MapContainer.cs:703`) walks `Objects`, so on save the lost object is gone and the duplicate is written twice.
- **Repro**: create a new Part, keep it selected, choose Move to Top, then save. One existing part of that type is gone.
- **Fix**: find `curSel`'s real index with `IndexOf`. Build the span from all objects of that type, not just the first run. Bail out if the object isn't found.

#### B2. Duplicate (Ctrl+D) puts the clone in the map picked in the Map List, not the object's own map (Confirmed)
`Tools/Common/DuplicateAction.cs:179-196`, `EntDuplicateAction.cs:102-145`

- `ApplyDuplicate` passes `targetMap: View.Selection.SelectedMapContainer`.
- `SelectedMapContainer` is only set when you click a map in the Map List, or by `LoadMap(..., selectOnLoad: true)`. Picking an object in the viewport never changes it.
- `EntDuplicateAction` uses `TargetMap` whenever it is non-null. With two or more maps loaded (common with ER "Load Related Maps"), duplicating an object from map B while map A is selected:
  - inserts the clone into **map A's** object list at `IndexOf(source)+1`, which is `0` because the source isn't in A;
  - parents it under the source's parent in **map B** (because `duplicateToMap` is false);
  - checks name uniqueness against A's names but keys them by B's map ID.
- The result is an object that saves into map A while showing in B's tree.
- `SelectedMapContainer` can also be `null` (nothing picked yet, or the picked map was unloaded). Then `mapContainer.BTLParents` throws a NullReferenceException.
- **Fix**: pass `targetMap: null` for in-place duplicates, so each clone goes into its source's own map.

#### B3. Paste and Duplicate-to-Map insert at index 0 (Confirmed)
`EntDuplicateAction.cs:139-147`

- Both branches use `map.Objects.Insert(map.Objects.IndexOf(Clonables[i]) + 1, newobj)`. When the target is another map, or the source is a clipboard clone that isn't in any map, `IndexOf` returns `-1`, so the clone goes to index 0.
- Pasted parts end up ahead of every other object. That changes MSB part indices and sets up B1.
- **Fix**: append (or place at the end of the type's run) when the source isn't in the target map.

#### B4. Renaming several objects with "Rename" (with references) gives them identical names (Confirmed)
`GUI/Properties/MapMsbPropertyView.cs:630-634`

```csharp
group.Select((ent, index) => $"{editName.name}-{group.Key.Name}")
```

- `index` is never used, so every selected object of the same type gets exactly the same name.
- Duplicate names break name-based references and the Map Groups name mapping.
- **Fix**: include `index`, like the direct-rename path does (`{newValue}_{i}`).

#### B5. Renaming never updates the references that point at the object (Confirmed)
`Actions/Map Objects/EntRenameAction.cs`, `PropChangeAction.RenameEntity`

- The "Rename" field's tooltip says it updates name references (`MapMsbPropertyView.cs:601`). `EntRenameAction` ignores its `reference` argument, though. It only updates Map Groups and light-atlas entries.
- Regions, events and parts that refer to the old name keep it. The next save throws `MissingReferenceException`, or SoulsFormats resolves the stale name to nothing.
- `References` and `ReferencingObjects` are not rebuilt after a rename either, so the "Referenced By" panel shows old data.
- **Fix**: before renaming, walk `GetReferencingObjects()` and rewrite every matching string field, all inside the same undoable action. Then call `BuildReferenceMap()` on the container.

### Medium: crashes and wrong behaviour

#### B6. The Create shortcut crashes before a target map is chosen (Confirmed)
`Tools/Common/CreateAction.cs:44-53, 277-343`

- `TargetMap` starts as `("None", null)`, but the guard checks `TargetMap == (null, null)`, so it never trips.
- Pressing the Create shortcut with any selection reaches `AddNewEntity(..., map: null)`, and `parent ??= map.RootObject` throws a NullReferenceException.
- The same broken guard is in `DuplicateToMapAction.cs:132`, `EntityRenameAction.cs:124` and `EntityIdCheckAction.cs:105`.
- In Duplicate-to-Map, with a light selected and no map chosen, opening the "Target Lights" combo dereferences a null `targetMap.BTLParents`. Clicking the button with no map chosen duplicates in place without saying so.
- **Fix**: test `TargetMap.Item2 == null` everywhere. Also re-resolve the container by ID before using it, so a stale container from an unloaded map is never used (see B11).

#### B7. Hard `(Entity)` casts crash when the selection holds a map reference (Confirmed)
- `GUI/MapViewportView.cs:231`: viewport context menu
- `Tools/QuickView.cs:36`: Quick View tooltip, runs every frame while the viewport has focus
- `Tools/ModelSelectorTool.cs:98`

`ObjectContainerReference` is an `ISelectable` but not an `Entity`. Three paths put one into the selection:
- the "References To" map button (`PropInfo_ReferencesTo.cs:108-110`), when that map isn't loaded;
- the map root in the content tree, when `RootObject` is null;
- the `select` command (see B9).

After that, right-clicking in the viewport, hovering the viewport or opening the Model Selector throws `InvalidCastException`. **Fix**: use `as Entity` and handle `null`, as `MapEditorDecorations.cs:46` already does.

#### B8. With two or more views in Tabbed Panels mode, the last view is always the active one (Confirmed)
`GUI/MapEditorView.cs:369-440`

- In Windows mode, `Editor.ViewHandler.ActiveView = this` only runs while the window is hovered.
- In Tabbed Panels mode it runs every frame for every open tab, so the last view drawn always wins.
- `MapEditorScreen.Update` only updates `ActiveView.ViewportWindow`, and shortcuts go to `ActiveView`. So the camera and shortcuts in every other view stop working.
- **Fix**: wrap those assignments in `ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows)`, as `DisplayWindows` does.

#### B9. The `select` command runs for every command (Confirmed)
`Framework/MapCommandQueue.cs:62-93`

- `HandleSelectCommand` never checks `commands[0] == "select"`, so it also runs for `load/<map>` and `propsearch/...`.
- For `load/<unloaded map>` it selects an `ObjectContainerReference` (which feeds B7).
- `HandleLoadCommand` reads `commands[1]` without checking the length, so a bare `map/load` throws IndexOutOfRangeException.
- `load` also calls `LoadMap(mapid)` without `selectOnLoad`, so the loaded map doesn't show up in the content tree.

#### B10. "Place at List End" does nothing (Confirmed)
`DuplicateAction.cs:121` shows `CFG.Current.Toolbar_Duplicate_Place_at_List_End`, but nothing reads it (`grep` finds only the CFG field and the checkbox).

#### B11. Several unload paths skip the unsaved-changes prompt (Confirmed)
`RequestUnloadMap` / `ConfirmDiscardChanges` exist, but these paths call `UnloadMap` directly:
- Options > Map List > **Unload Current** and **Unload All** (`MapEditorScreen.cs:410, 425`). The generic yes/no dialog doesn't say which maps have unsaved edits.
- The "References To" context menu, **Unload Map** (`PropInfo_ReferencesTo.cs:132`).
- Global Mass Edit unloads every map it loaded (`MassEditTool.cs:593-596`), including maps the user already had open with unsaved edits.

Also, `UnloadMap` calls `ViewportActionManager.Clear()` (`MapUniverse.cs:342`), so unloading one map wipes the undo history for every map in that view. Combined with the paths above, one click can lose both unsaved edits and undo history.

#### B12. Duplicate-to-Map from the context menu or shortcut needs the Tools window to be open (Confirmed for the menu, Plausible for the shortcut)
- `DuplicateToMapAction.OnContext` and `MoveToMapAction` only set `DisplayPopup = true`. The popup is only opened in `OnGui()`, which only `MapToolWindow.Display()` calls. With the Tools window hidden, or (in tabbed mode) its tab not in front, the menu item does nothing.
- `OnShortcut` calls `ImGui.OpenPopup` from `MapShortcuts.Monitor()`, which runs under a different ImGui ID stack from the `BeginPopup` in the Tools window. The popup ID probably never matches, so the shortcut probably does nothing.
- **Fix**: set `DisplayPopup = true` in the shortcut too, and call `OnGui()` once per frame from `MapEditorView.Display`.

#### B13. The Move to Map button is labelled "Duplicate" (Confirmed)
`Tools/Actions/MoveToMapAction.cs:138`. The button calls `MoveToMap`, but it reads "Duplicate", and the header tooltip also says "duplicate".

### Low

| # | Where | Issue |
|---|---|---|
| B14 | `GUI/MapContentView.cs:663-675` | Shift-clicking the eye ("apply to all") flips each sibling separately, so a mixed set swaps states instead of all matching the clicked one. It also only walks `RootObject.Children`, which misses objects under `MapOffsetNode`. |
| B15 | `GUI/MapListView.cs:69-73` | The map list search only refreshes on `IsItemDeactivatedAfterEdit`, so toggling the exact-match checkbox has no effect until the text is edited again. |
| B16 | `GUI/MapListView.cs:216, 232` | Map List Filter patterns go straight into `Regex.Match` every frame. An invalid pattern throws every frame. Compile them once, catch `ArgumentException` and show the error in the filter editor. |
| B17 | `SelectAllAction.cs:460` | `sel.First()` throws if the filtered `MsbEntity` selection is empty, for example when only a map reference is selected. |
| B18 | `SelectAllAction.cs:377-478` | Select by model, type or ceremony searches `SelectedMapContainer`, not the selected objects' own maps (same root cause as B2). |
| B19 | `EntMoveAction.cs:48` | Logs `MAP_Actions_Duplicate_Failed` when a move fails. |
| B20 | `MapViewHandler.cs:82-125` | `RemoveView` unloads the view's maps but never disposes the view's render scene, viewports or tool resources, so GPU and CPU memory leaks with every closed view. |
| B21 | `MapUniverse.cs:44-86` | `LoadMap` loads Havok collision and navmesh synchronously, *before* `LoadMapAsync` checks "already loaded". Loading a map that is already loaded reloads its collision anyway. |
| B22 | `MapUniverse.cs:88-302` | With `allowConcurrentMapLoad` (Load Related Maps), several `LoadMapAsync` continuations run at once on pool threads and write to shared project-level banks (FVB, BTAB, GPARAM, navmesh) and shared fields (`HasProcessedMapLoad`, `IsLoading`). The first load to finish clears `IsLoading` while the others are still running. **Plausible** race. |
| B23 | `MapUniverse.cs:648` | `LoadDS2Generators` calls `registParams.Add` and `generatorParams.Add`. A duplicate row ID in a DS2 per-map param throws and aborts the whole map load. |
| B24 | `MapUniverse.cs:865-888` | `SaveDS2Generators` returns silently when any serialise step fails, *after* the MSB was already written, which can leave the MSB and the generator params out of sync. |
| B25 | `MapEditorScreen.cs:565-573` | A map-load exception is logged and then rethrown on the main thread. If the global handler treats it as fatal, one bad map ends the session. **Plausible**; check what the top-level handler does. |
| B26 | `MapEditorScreen.cs:618-648` | When an autosave hits `MissingReferenceException`, `HandleSaveException` opens a modal Yes/No box. This repeats every autosave interval until the reference is fixed. |
| B27 | `MapUniverse.cs:390-392` | `UnloadMap` runs `GC.Collect` / `WaitForPendingFinalizers` / `GC.Collect` once per map, so Unload All over N maps does 2N blocking collections. Collect once in `UnloadAllMaps`. |

---

## 2. Usability improvements

These are ordered by how much I expect each one to help.

### U1. Make "the current map" follow the selection
B2, B3, B18 and the empty content tree with multiple maps loaded all come from the same thing: `SelectedMapContainer` only changes through the Map List.
- When the user picks an object (viewport, search, goto), set `SelectedMapID` and `SelectedMapContainer` to that object's map.
- Have the content tree switch to it, so Goto works across maps.
- Show the active map's name in the viewport overlay.

### U2. Make map-object references survive editing
- Rename: implement B5. Show "N references will be updated" before renaming.
- Delete and Move to Map: warn when the selection is referenced by objects that will stay behind ("3 events reference this part"). Offer to cancel, or to clear those references. Right now the user only finds out at save time through `MissingReferenceException`.
- On a save failure, keep the "jump to the object" behaviour, but also show the field name and the missing name, and offer to blank the reference.

### U3. Safer unloading and undo
- Route every unload through `RequestUnloadMap` (B11). In the dialog, list the dirty maps and offer **Save and Unload** as well as Discard and Cancel.
- Keep each map's undo history separate: on unload, drop only the actions that touch that map instead of clearing the whole stack.
- Do the same when closing a view (`MapViewHandler.RemoveView`): offer Save as well as Yes/No.

### U4. Placement of new objects
- Create and Paste put objects at the origin or their copied position. Place them at the placement orb (`MapViewportView.GetPlacementPosition`), the way Prefab placement does.
- Paste should append to the end of the type's list, or honour "Place at List End" (B10).
- The Create shortcut needs a non-empty selection for no reason (`CreateAction.cs:46`). Drop that condition and default the target map to the active map (U1).

### U5. Map loading feedback
- A second load while one is in progress is refused with a log line (`MAP_Universe_Log_Map_Already_Loading`). Queue it instead, and show "Loading m10_00 (2 queued)" in the Map List header.
- Show a spinner or "Loading…" on the map's Map List row while its `LoadState` is Loading.
- After `load/<map>` commands and "Load Map" from references, select the map (`selectOnLoad: true`).

### U6. Content tree at scale
- The tree draws every object of the selected map every frame, without an `ImGuiListClipper`. For each object it also calls `AliasHelper.UpdateEntityAliasName` and `ContentFilter`, and it re-sorts the categories with `OrderBy`. Clip the flat lists, and cache the sorted category order until `ObjectAddedRemoved`.
- `GetMapContainerFromMapID` is a linear `FirstOrDefault` over every map in the project, and the delete, duplicate, move and reorder actions call it per object. Add a `Dictionary<string, MapWrapper>` lookup.
- The Map List rebuilds both lists every frame, using `List.Contains` (O(n²) with a World Map pre-filter) and per-frame regexes. Rebuild only when the filters or load states change.

### U7. Viewport context menu
- Right-clicking with nothing selected shows nothing. Offer Create, Paste, Frame All and the visibility toggles there.
- The context menu only uses the first selected object for Scramble, Replicate, Frame and Select All. Label these items "(first selected)" or make them work on the whole selection.

### U8. Reorder
- Multi-selection reorder is refused with an error (`EntReorderAction.cs:36`). Supporting contiguous blocks would cover the common case.
- Grey out the Reorder menu items for lights and multi-selections instead of logging an error after the click.

### U9. Consistency fixes
- Use one sentinel for "no target map" across Create, Duplicate-to-Map, Move-to-Map, Prefab, Entity ID Check and Rename (B6). A small `TargetMapPicker` widget shared by all six would also give every tool the same combo, with aliases and a "use the active map" default.
- Write the Tabbed Panels labels ("Map List", "Map Contents", "Properties", "Tools") and the Windows-mode titles with `LOC.Get`, like the rest of the menu bar.
- Use the same label for the Edit-menu "Duplicate Selected to Map" and the context-menu "Duplicate to Map", and show the shortcut hint in both.
- Tooltips: "Rename" promises reference updates (B5), and the Move to Map header says "duplicate" (B13).

### U10. Autosave
- Don't open modal dialogs during an autosave (B26). Log the error, and mark the map row in the Map List red with a tooltip until the next manual save.

---

## 3. Suggested order of work

1. B1, B2/B3 and B4: these silently damage MSBs. Each is a small, local fix.
2. B6, B7 and B9: crashes reachable from normal input.
3. B11 and U3: avoid losing unsaved work.
4. U1: one change that removes a whole class of wrong-map bugs (B2, B18, cross-map Goto).
5. B5 and U2: keeping references intact on rename, delete and move. This is the largest piece; plan it separately.
6. The rest as time allows.

## 4. What to check in the app

None of this has been run. To confirm the high-priority items:

| Item | Steps | Bug if… |
|---|---|---|
| B1 | Create a new Part, keep it selected, Move to Top, save, reload the map. | One existing part of the same type is missing and the new part appears twice. |
| B2 | In ER, Load Related Maps on an m60 tile. Pick a neighbour tile in the Map List, select an object from the *centre* tile in the viewport, Ctrl+D, save. | The clone was saved into the neighbour tile's MSB. |
| B4 | Select two assets, enter a name in the Rename field. | Both get the same name. |
| B6 | Open a map, select any object, press the Create shortcut without opening the Create tool. | NullReferenceException. |
| B7 | Select a Part whose "References To" lists an unloaded map, click that map's button, then right-click in the viewport. | InvalidCastException. |
| B8 | Switch to Tabbed Panels, add a second view, try to fly the camera in view 0. | The camera doesn't respond. |
