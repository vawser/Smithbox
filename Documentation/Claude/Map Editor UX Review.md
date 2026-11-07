# Map Editor UX Review

Date: 2026-11-07
Scope: the Map Editor's main panels (Map List, Map Contents, Map Groups, Properties, viewport context menu, menu bar,
Tools window) and its shortcut handling. Nothing was run. Every finding comes from reading the code.

This builds on two earlier passes, and does not repeat them:
- `Map Editor Review.md` (2026-11-05): bugs B1–B27 and usability items U1–U10. U1–U10 are implemented.
- `Map Editor Action Tabs Assessment.md` (2026-11-06): the Tools window regrouping. Implemented.

Verdicts:
- **Confirmed**: the code path was traced end to end.
- **Plausible**: depends on runtime state I could not see (ImGui focus or ID stacks, input timing).

---

## 1. Defects that hurt the experience

These are bugs, found while looking at the UX. Each is small and local.

| # | Where | Issue | Verdict |
|---|---|---|---|
| X1 | `GUI/Properties/MapMsbPropertyView.cs:49-53, 75-79` | Two early-return paths call `ImGui.End()` and `ImGui.PopStyleColor(2)`, but `Display()` only opened a child. Nothing pushed those colours, and the window belongs to `MapEditorView`, which calls `End()` again. Selecting several param-row objects (DS2 generators) or an object whose `WrappedObject` is null closes the Properties window early and unbalances the ImGui stacks. ImGui asserts on that. Fix: keep only `EndChild()` before the `return`. | Confirmed |
| X2 | `GUI/MapGroupsView.cs:93-99` | With the Groups panel focused, **Delete** deletes the group's *map objects* and then the group entry. Deleting the objects can be undone. Removing the entry can't: it is saved to JSON straight away. So Ctrl+Z brings the objects back but the group is gone. Users who expect Delete to remove the list entry lose map data. Fix: make Delete remove only the group entry. Move "Delete group's objects" to the context menu and ask for confirmation there. | Confirmed |
| X3 | `Keybinds/InputManager.cs:493` + `Framework/MapShortcuts.cs:54-79` | `IsBindingDown` checks that the binding's modifiers are held, but not that other modifiers are *not* held. Pressing **Ctrl+Alt+Z** (Undo Repeat) also matches **Ctrl+Z** (Undo), so the first press undoes twice. Redo has the same problem. More generally, any Ctrl+Shift+K binding also fires the Ctrl+K one. Fix: match modifiers exactly, or have `Monitor` check the more specific binding first and `else` the rest. The input code is shared, so check the other editors too. | Confirmed |
| X4 | `GUI/MapContentView.cs:710` | The eye's "apply to all" uses `InputManager.IsPressed(Apply_to_All)` (Shift+A). `IsPressed` only returns true on the frame where A goes *down*, so the click has to land in that same frame. In practice the feature can't be reached. Earlier bug B14 still applies too: it flips each sibling instead of setting them all to one state, and it only walks `RootObject.Children`. Fix: on Shift+click, set every object of the type to `!e.EditorVisible`. | Confirmed |
| X5 | `MapShortcuts.cs:32-34` | The shortcut monitor only stops when the *viewport* has keyboard focus and an item is active. Save, Undo and Redo don't check `ImGui.GetIO().WantTextInput`. So Ctrl+Z while typing in a Properties field undoes the last map action *and* the text edit. | Plausible |
| X6 | `GUI/Properties/MapPropertyView.cs:90` | The Havok header toggles (type column, mesh data) are chosen by `CollisionViewMode` only. In navmesh HKX mode the header shows the MSB toggles instead. Fix: check `NavmeshViewMode` when `IsNavmeshType()`. | Confirmed |
| X7 | `MapMsbPropertyView.cs:669` | Community Name reads and writes under `View.Selection.SelectedMapID`, not the object's own `MapID`. `SyncActiveMapToSelection` allows the user to pick another map in the Map List while keeping the selection, and the alias is then stored under the wrong map. | Confirmed |

Still open from the earlier docs: the dead View-menu toggles (Viewport, Data Transfer, Selection Groups, Viewport
Configuration) and the `PropertiesActionTool` Clear Display / Draw Groups / Collision Masks undo bug.

---

## 2. Usability improvements

Ordered by how much I expect each one to help, within each area.

### A. Selection and Properties feedback

**A1. Show what is selected.** Nothing in the editor shows the selection count or what it contains. Add a line at the
top of Properties: "3 selected: 2 Enemy, 1 Asset · m10_00_00_00". You can only tell how many objects a Delete or
Scramble will hit by scrolling the content tree.

**A2. Explain mixed-type selections.** When the selection holds more than one type, `PropEditorSelectedEntities`
draws Name and Community Name and then returns (`MapMsbPropertyView.cs:754`) without saying why the fields stopped.
- Minimum: a line saying "Mixed types: only shared fields are shown", plus one button per type ("Enemy (2)",
  "Asset (1)") that narrows the selection to that type.
- Better: show the fields shared through `IMsbPart` / `IMsbRegion` (Position, Rotation, Scale, EntityID), since
  multi-edit already uses `PropMultChangeAction`.

**A3. Multi-map selections.** "Map ID" shows the first object's map only. When the selection spans maps, list them
all ("m60_42_36_00, m60_43_36_00"). Cross-map edits are where the earlier wrong-map bugs (B2, B18) came from.

**A4. Empty states.** While maps load, Properties shows `ImGui.Text("")` (`:93`). Show "Loading maps…". The
multi-edit banner uses a hard-coded green (`:56`) instead of a `UI.Current` theme colour.

### B. Map List

**B1. Double-clicking a loaded map unloads it** (`MapListView.cs:216-218`). Double-click to load works well, but
double-clicking the same row again (for example, to "open" it) unloads it at once when it has no unsaved
changes. It drops the collision, navmesh and render state, and the next load takes a long time on ER tiles. Make
double-click on a loaded map select it and frame it (`FrameMap` already exists in `MapViewportView`). Leave unloading
to the context menu.

**B2. A single click on an unloaded map does nothing.** Only loaded maps take the selection. With
`MapEditor_Map_List_Enable_Load_on_Double_Click` off, the context menu is the only way to load. Highlight the row on
click and show a small load icon on hover, or put "Double-click or right-click to load" in the tooltip.

**B3. Batch load and unload.** There is no multi-select. Ctrl/Shift-click with "Load Selected" / "Unload Selected"
in the context menu would cover the common case of opening a dungeon and its neighbours without the World Map.

**B4. Search behaviour.**
- The text filter only applies on Enter or blur (`:80`). The search now runs over cached aliases and is cheap, so
  filtering as you type would feel more natural.
- The filter applies only to the unloaded section (`:374`), so loaded maps never hide. If that is intended, add a
  hint. Otherwise, filter both sections.

**B5. Ambiguous toggles.** The Chalice (BB) and Asset MSB (DB) toggles both use `Icons.Adjust` and show no on/off
state. Use `GUI.DisplayToggleButton` like the other panel headers.

**B6. Misleading label.** "Add to Map Filter" (Map List and content-tree root) calls
`GlobalSearchTool.TargetMap`, so it adds to the Global Search's map filter, not the Map List filter. Rename it
"Add to Global Search".

**B7. Other views' maps.** The loaded section lists maps loaded by any view (the `NOTE` at `:363`). Their undo
history lives in their `OwnerView`, so edits made here after selecting one are recorded in the wrong view. Mark
those rows ("(View 2)") or leave them out.

### C. Map Contents tree

**C1. One map at a time.** The tree shows only `SelectedMapContainer`. With ER "Load Related Maps" (9+ tiles), the
user has to switch maps in the Map List for every lookup. An option to show every loaded map as its own root node
would fix this. The map root node and its context menu already exist, so the change is mostly a loop.

**C2. Counts on nodes.** Category and type nodes show no counts. "Enemy (124)" helps users find their way and costs
nothing, since `GetSortedTypeView` already holds the lists.

**C3. Empty categories.** Categories and types with no objects are drawn as plain indented text (`:567, :576`). It
reads like a disabled node. Hide them, or grey them out with "(0)".

**C4. Show / Hide All ignore the search.** The eye buttons in the header act on the whole map. Acting only on the
objects that match the current search would turn "search `c4500` → hide all" into a one-click workflow. Use the
existing `ContentFilter`.

**C5. Name display cycling.** One button cycles through six name modes, and you only see the current mode in the
tooltip. A small popup listing the six with the current one checked would show all the options at once.

**C6. Tree / Flat lives in Options.** The Tree / Flat switch is under Options › Content Display. The tree's other
display toggles are header buttons. Move it next to them.

**C7. Flat view isn't clipped.** `FlatView` draws every object. Use the same `ImGuiListClipper` path as
`DisplayTypeList`.

### D. Context menus

**D1. Fill the gaps in the selection menu.** The viewport's selection menu has no Copy, and no "Go to in List"
(`GotoAction.OnContext` is an empty stub). Both are common after a pick in the viewport. Copy is only offered when
nothing is selected, where it can't be used. Paste is in that menu.

**D2. One menu builder.** The viewport menu (`MapViewportView.ViewportContextMenu`) and the content-tree menu
(`MapContentView.DisplayMapObjectContextMenu`) list almost the same items in the same order. They differ in that
the tree also has Reorder and Map Groups, and the viewport chooses a representative entity. Move them into one
`MapObjectContextMenu.Draw(View, ent, MenuSource)`, so later additions reach both menus.

### E. Menus and Tools window

**E1. Tidy the Tools window's View menu** (`MapToolWindow.ViewMenu`).
- The first block follows the new categories. The block after the separator is in no particular order.
- Remove the four dead toggles.
- Use the header's label for every toggle. Today "Display Groups" toggles "Render Groups", and "Prefabs",
  "Load Lists" and others are raw strings.
- Group the toggles by category with separators or sub-menus that match the list. `ToolToggle` already does the
  labelling.

**E2. Undo and Redo always look enabled.** Pass `CanUndo()` / `CanRedo()` as the `enabled` argument of
`ImGui.MenuItem` so the items grey out when there is nothing to undo or redo.

**E3. Save scope.** File › Save saves every loaded map. The Map List context menu's "Save Map" saves one. A
"Save Active Map" entry in File (with the active map's name) would make the difference clear.

### F. Localisation sweep

About 50 user-facing literals in `GUI/` skip `LOC.Get`: Map List and content-tree context menus, panel titles
("Map List", "Contents", "Groups"), Properties labels ("Object Type", "Map ID", "Rename"), the Tools menu
("Tools", "Miscellaneous", "World Map") and most of the Tools window's View menu. Fix them file by file with the
English text unchanged. As with U9, window IDs keep their `##` suffix, so dock layouts are kept.

---

## 3. Suggested order

1. **X1–X4, X6, X7**: small fixes. X1 can crash, and X2 loses group data.
2. **B1, B6, E1, E2**: changes of a line or two that remove surprises.
3. **A1, A2, C2, C3**: selection and tree feedback. All read from data that already exists.
4. **D1, D2**: the shared context menu, then add Copy and Go to.
5. **C1, C4, B3**: workflow features. C1 is the largest. Plan it separately.
6. **X5, B2, B4, B5, B7, C5–C7, E3, F**: as time allows.

## 4. What to check in the app

| Item | Steps | Defect if… |
|---|---|---|
| X1 | DS2: load a map, select two generator rows in the content tree. | ImGui assertion or a broken Properties window. |
| X2 | Make a Map Group, focus the Groups panel, select the group, press Delete, then Ctrl+Z. | The objects come back but the group is gone. |
| X3 | Move an object three times, then press Ctrl+Alt+Z once. | Two moves are undone. |
| X4 | In the content tree, hold Shift+A and click an eye. | Only that object toggles. |
| X5 | Move an object, type in a Properties text field, press Ctrl+Z. | The move is undone as well. |
| X6 | ER: select a navmesh part, switch Properties to the navmesh HKX view. | The header shows Unknown / Padding / References toggles. |
| X7 | Select an object in map A, click map B in the Map List, edit Community Name, reselect via map A. | The new alias doesn't show for the object. |
| B1 | Load a map, double-click its row again. | The map unloads. |
