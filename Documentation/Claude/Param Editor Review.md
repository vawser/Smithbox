# Param Editor Review: Bugs and Usability

Scope: `src/Smithbox.Program/Editors/Param Editor/**`, plus `Editors/Common/CacheBank.cs`, `ActionManager.cs` and `FocusManager.cs`, which the editor depends on.
Method: I read the code; I didn't run anything. Each bug gives steps to reproduce it so you can confirm it in the app. Items marked *(verify)* rest on ImGui or data behaviour I couldn't fully confirm from the code alone.

Severity key: **High** = corrupts or loses data, or crashes; **Medium** = wrong behaviour users will run into; **Low** = edge case or latent issue.

Paths below are relative to `src/Smithbox.Program/Editors/Param Editor/` unless stated otherwise.

---

## 1. Bugs

### 1.1 Data integrity (High)

> **Status (2026-11-05): B1–B3 fixed, not yet verified in the app.** The project builds.
> - **B1:** the save still writes on a worker thread. `ParamDirtyTracker` now keeps an edit version per param. `ParamBank.Save` snapshots the versions before writing and afterwards clears only the params that didn't change (`ClearDirtyIfUnchanged`); any that did are logged and stay marked `*`. The save lock is now an `Interlocked` flag released in `finally`, and the table names are written on the UI thread before the worker starts. While a save runs, `ActionManager.ExecutionGuard` (new, and unset in other editors) refuses every action except `PropertiesChangedAction`, which covers row add, delete, reorder and duplicate, plus undo/redo of those. `MassEdit.ExecuteMassEdit` refuses too. Tools that change params without going through an action (upgrader, merge, CSV import code that bypasses actions) aren't covered.
> - **B2:** `PerformMassEdit` undoes its child manager on a parse failure or exception, and the result message says so.
> - **B3:** `AddParamsAction` and `AddRowsToTableGroup` create their clones on the first Execute only, and record each insert/replace step. Undo reverses the steps exactly; redo replays them with the same row instances. This also fixes an index drift in the old replace-mode undo, which removed all the clones before re-inserting the replaced rows.

#### B1. Save runs on a worker thread and can drop edits made during the save
`ParamEditorScreen.Save()` is `async void` and starts with `await Task.Yield()`. The app has no `SynchronizationContext`, so everything after the yield runs on a thread-pool thread. That includes `WriteTableGroupNames()` and `ParamBank.Save()`, which then calls `ClearDirtyState()` ([ParamEditorScreen.cs:309-333](../../src/Smithbox.Program/Editors/Param%20Editor/ParamEditorScreen.cs), [ParamBank.cs:119-173](../../src/Smithbox.Program/Editors/Param%20Editor/Data/ParamBank.cs)). Meanwhile the UI thread keeps editing and drawing the same `Param` objects.
- An edit made while the save is writing marks its param dirty, and then `ClearDirtyState()` clears that mark. The edit isn't in the file, but the param no longer shows `*`. The next save logs "skipped, nothing modified", so the edit is lost unless *Allow Unmodified Save* is on.
- Pasting or deleting rows during a save changes `Param.Rows` while the writer is enumerating it, which can throw.
- `SaveLock` is a plain bool set without a lock, and there's no `try/finally`. If `WriteTableGroupNames()` throws (for example, the JSON file is locked), `SaveLock` stays `true`, and every later save logs "Save in progress" until restart.

*Repro:* turn on automatic save with a short interval, then keep editing a field in a large param such as ER `EquipParamWeapon`. Watch for the param's `*` disappearing while the value isn't on disk. For the lock: make a `Param Table Names/<lang>/*.json` file read-only and save twice.
*Fix direction:* snapshot the dirty set before writing, and clear only the params that were written and haven't changed since (a per-param edit counter does this). Either run the save on the UI thread, or block edits while it runs. Wrap the lock in `try/finally` and use `Interlocked.Exchange`.

#### B2. A mass-edit script that fails partway keeps its earlier edits, with no undo
`PerformMassEdit` runs each `;`-separated command through `childManager.ExecuteAction(...)`, so the data changes immediately. If a later command fails, it returns `(result, null)`, and the exception path does the same ([MassParamEditRegex.cs:97-118](../../src/Smithbox.Program/Editors/Param%20Editor/Mass%20Edit/Edit%20Engine/MassParamEditRegex.cs)). `ExecuteMassEdit` only pushes the child manager when it isn't null ([MassEdit.cs:74-79](../../src/Smithbox.Program/Editors/Param%20Editor/Mass%20Edit/MassEdit.cs)). The earlier commands stay applied but never reach the undo stack.

*Repro:* `selection: someField: = 5; selection: notAField: = 1` → the first edit sticks and Ctrl+Z does nothing.
*Fix:* on failure, undo `childManager` (call `UndoAllAction()`) before returning, or push it anyway and say so in the result text.

#### B3. Redoing a paste or duplicate orphans any later edits to the new rows
`AddParamsAction.Execute()` clones new `Param.Row` instances each time it runs, and `Undo()` clears `Clones` ([AddParamsAction.cs:41-200](../../src/Smithbox.Program/Editors/Param%20Editor/Actions/AddParamsAction.cs)). `AddRowsToTableGroup` works the same way. A `PropertiesChangedAction` recorded against the first clone keeps a reference to that clone. After Undo, Undo, Redo, Redo, the edit applies to a row that's no longer in the param.

*Repro:* paste a row, change a field on the pasted row, press Ctrl+Z twice, then Ctrl+Y twice. The pasted row comes back with the original value, and no error is shown.
*Fix:* create the clones once (first `Execute`) and re-insert the same instances on redo, the way `DeleteParamsAction` keeps its `Deletables`.

> **Status (2026-11-05): B4–B8 fixed, not yet verified in the app.** The project builds.
> - **B4:** `SetActiveRow`, `AddRowToSelection` and `ToggleRowInSelection` never add `null`. `ApplyDelete` returns early with no active param, and only deletes rows still in that param. Copy and Duplicate skip null entries too.
> - **B5:** the Name Manipulation menu is hidden for the Name and ID rows.
> - **B6:** the deep-copy offset now lives in `AddParamsAction.ApplyDeepCopyOffset`. It returns early for params without meta, offsets only fields whose `DeepCopyTarget` matches a ticked option, and converts through `long` before writing the value back in the field's own type (a value that would overflow is left alone). Only `-1` is skipped, as before.
> - **B7:** table-group Duplicate and Delete return early when the group has no rows. After a successful delete, `CurrentTableGroupID` resets to `-1`.
> - **B8:** unnamed rows copy as an empty name.

#### B4. `null` gets into the selection, and Copy, Duplicate and Delete then crash
`SetActiveRow(row, ...)` always does `selectionRows.Clear(); selectionRows.Add(row)`, even when `row` is `null` ([ParamSelection.cs:199-201](../../src/Smithbox.Program/Editors/Param%20Editor/Framework/ParamSelection.cs)). `param/select/...` calls `SetActiveRow(null, ...)` first, and only replaces it if the ID resolves to a row ([ParamCommandQueue.cs:84-102](../../src/Smithbox.Program/Editors/Param%20Editor/Framework/ParamCommandQueue.cs)). For a missing ID the selection ends up as `[null]`, and `RowSelectionExists()` returns true:
- Ctrl+C → `new Param.Row(null)` → `NullReferenceException` ([ParamClipboard.cs:59-62](../../src/Smithbox.Program/Editors/Param%20Editor/Framework/ParamClipboard.cs))
- Ctrl+D → same, in `ApplyDuplicate`
- Delete → `IndexOfRow(null)` throws `ArgumentException`

*Repro:* follow a param reference whose target row doesn't exist (a broken ID in a reference field, or a stale command), then press Ctrl+C.
*Fix:* `if (row != null) s.selectionRows.Add(row);`

#### B5. "Set Target Field" on the Name or ID row crashes
The Name and ID info rows call `FieldRow(..., col: null, ...)`. The name-column context menu then calls `SetNameManpulationTargetField(col.Def.InternalName)` without a null check ([ParamFieldWindow.cs:1606-1616](../../src/Smithbox.Program/Editors/Param%20Editor/GUI/ParamFieldWindow.cs)).
*Repro:* right-click the *Name* label in the Fields window, then choose Name Manipulation → Set Target Field.

#### B6. Duplicate crashes on params with no meta, and each "Affect X field" option changes every reference type
In `AddParamsAction.Execute()`, when `IsDuplicate` is set ([AddParamsAction.cs:51-100](../../src/Smithbox.Program/Editors/Param%20Editor/Actions/AddParamsAction.cs)):
- `GetParamFieldMeta` returns `null` when the param has no meta, and `meta.DeepCopyTargetType` then throws. This happens on every Duplicate (Ctrl+D) for such a param.
- The five `Param_Toolbar_Duplicate_Affect*Field` flags are never compared with the field's `DeepCopyTargetType`. Turning on any one of them offsets every deep-copy field.
- `(int)cell.Value` throws for `uint`, `short` and other non-`int` fields.
- *Correction:* the first version of this review said no meta file defines `DeepCopyTarget`. That was wrong: the meta XMLs are UTF-16, and the plain `grep` I used couldn't read them. There are 486 attributes across 46 files, all with one of the values `Source` (273), `SpEffect` (181), `Bullet` (15), `Attack` (9) or `Behavior` (8), matching the five options.

*Fix:* use `meta?.DeepCopyTargetType`, match each flag to its type string, and use `Convert.ToInt64`.

#### B7. Duplicating a table group after deleting it throws
`DeleteTableGroup` doesn't reset `CurrentTableGroupID` ([ParamTableWindow.cs:456-479](../../src/Smithbox.Program/Editors/Param%20Editor/GUI/ParamTableWindow.cs)). The table shortcuts only check `CurrentTableGroupID != -1`, so `DuplicateTableGroup` calls `targetRows.First()` on an empty list (line 433). A second Delete pushes an empty `DeleteParamsAction` onto the undo stack.
*Repro:* in table-group mode (ER `ItemTableParam`), select a group, press Delete, then press Ctrl+D.

#### B8. Copy Name / Copy ID and Name crash on unnamed rows
`entry.Name.ToString()` throws when `Name` is null ([ParamRowWindow.cs:1168, 1182](../../src/Smithbox.Program/Editors/Param%20Editor/GUI/ParamRowWindow.cs)).
*Repro:* select a row with no name, right-click it, then choose Information → Copy Name.

### 1.2 Editing behaviour (Medium)

> **Status (2026-11-05): B9–B11 fixed, not yet verified in the app.** The project builds.
> - **B9:** the paste popup now names its target ("Paste N row(s) into X"). Rows still go only into the param they were copied from. If another param is selected, the popup explains this and offers *Go to X* instead of pasting. Ctrl+V now opens the popup in that case too, matching the menus. Row/Offset input is validated with `TryParse`, and IDs outside the `int` range are rejected with a message, so Submit no longer just disappears. The popup previews the resulting ID range. *Paste after selection* inserts below the bottom-most selected row (by index), and finds free IDs with a set instead of a per-ID row lookup. Errors are logged rather than swallowed.
> - **B10:** Alt-click Name/ID editing now changes only the clicked row. The input gets keyboard focus straight away; Enter or click-away commits and leaves edit mode; Escape cancels; clicking away without editing just leaves edit mode. The edit buffers persist across frames, so the committed value no longer depends on the frame-by-frame buffer reset. Two leaks in the edit branches are fixed too: an unpopped `PushItemWidth`, and the selected-row header colour that was never popped. The right-click ID/Name inputs still apply to the whole selection (needed for renaming a batch or regrouping table rows), and now show the row count in a tooltip.
> - **B11:** all four inputs call the new `ParamRowOperations.SetRowName` / `SetRowID`. These build one `PropertiesChangedAction` per changed row in a `CompoundAction`, so names containing `;` or starting with `##` are kept exactly, and an unchanged commit adds no undo entry. The save guard now also allows compound actions made only of property changes (`CompoundAction.ContainsOnly<T>`), so renames still work during a save.

> **Status (2026-11-05): B12–B18 fixed, not yet verified in the app.** The project builds.
> - **B12:** grouped mode no longer draws the pinned fields a second time; `DisplayLayoutTable`'s top table already shows them.
> - **B13:** the row-level link buttons now draw in the ID row's value column, chosen by "this is the row's `ID` property" (`col == null && proprow.Name == "ID"`) instead of `imguiId == 1`. That index belonged to the ID row before the info rows moved to a separate ID range.
> - **B14:** the Edit menu and the shortcuts both call the new `ParamEditorScreen.Undo/UndoAll/Redo`, which refresh the diff cache. `Undo_Repeat` / `Redo_Repeat` are handled, and each is OR'd with its normal binding so a Ctrl+Alt+Z press undoes once. Keybind modifiers are a "these must be held" check, so Ctrl+Z also matches while Alt is held.
> - **B15:** each view decides which windows to show from its own `Selection.ActiveStayParam`. A view becomes active when one of its windows has keyboard focus or is clicked (left or right), not when it's hovered. The empty `IsInTableGroupMode` block is gone.
> - **B16:** selecting a stay param no longer calls `CleanAllSelectionState`. The normal param keeps its rows, searches and history, and isn't highlighted while the stay param is open. Row shortcuts (select all, copy, paste, duplicate, delete, jump) are ignored while a stay param is open, so they can't act on the hidden selection.
> - **B17:** `DisplayParamList` calls `SetScrollHereY()` on the active param's entry itself; the per-category trailing `SetScrollFromPosY` is removed.
> - **B18:** the Edit → Param Row items are disabled when they can't run: no active param, a stay param open, no selection, or (for Paste) an empty clipboard. Undo, Undo All and Redo are disabled when their stack is empty (this also covers U7's enable-state part).

#### B9. Paste goes into the copied-from param, but "paste after selection" positions rows using the current param
The submit button always targets `Params[ClipboardParam]`. In "paste after selection" mode, the insert index and the free-ID search are computed against the *active* param's selection ([ParamPasteMenu.cs:107-136](../../src/Smithbox.Program/Editors/Param%20Editor/GUI/ParamPasteMenu.cs)). The shortcut path checks `HasClipboardContents` (clipboard param == active param), but the Edit-menu and right-click Paste don't ([ParamEditorScreen.cs:260-266](../../src/Smithbox.Program/Editors/Param%20Editor/ParamEditorScreen.cs), [ParamRowWindow.cs:936-940](../../src/Smithbox.Program/Editors/Param%20Editor/GUI/ParamRowWindow.cs)).
*Repro:* copy a row in `SpEffectParam`, switch to `Bullet`, select a row, then right-click → Paste with "after selection" ticked. The row is inserted into `SpEffectParam` at `Bullet`'s index, under an ID that was only checked against `Bullet`.
The same mode also uses `rows.Last()`, which is the last row *clicked*, not the bottom-most selected row (lines 111 and 118). The fix there is `SortSelection()` first.

#### B10. Alt-click rename and re-ID apply to the selection, not to the row you clicked
Alt-click only sets `Name_EditRow` / `ID_EditRow`. The input then commits `selection: Name := …` or `selection: ID := …` ([ParamRowWindow.cs:550-556, 639-645, 866-873](../../src/Smithbox.Program/Editors/Param%20Editor/GUI/ParamRowWindow.cs)). The right-click "row name input" does the same (lines 884-919).
- Alt-clicking row B while row A is selected renames row A.
- With several rows selected, `ID := X` gives them all the same ID, and nothing warns you.
- The edit box stays open after you commit. Escape doesn't cancel it; it only closes when you click another row.

#### B11. Renaming through the mass-edit grammar breaks some names
The new name is inserted into a mass-edit command, and `PerformMassEdit` splits commands on `;` and skips lines that start with `##` ([MassParamEditRegex.cs:59-71](../../src/Smithbox.Program/Editors/Param%20Editor/Mass%20Edit/Edit%20Engine/MassParamEditRegex.cs)). A name like `Sword; Fire` is saved as `Sword` and logs a parse error, and `## WIP` does nothing. Leading and trailing spaces are trimmed.
*Fix:* build a `PropertiesChangedAction` (or a compound action per row) directly instead of going through a command string.

#### B12. Pinned fields show twice in Field Layout mode
With *Pinned stay visible* on, `DisplayLayoutTable` draws the pinned fields in the top table (lines 275-286), and then `DisplayFields` draws them again in grouped mode (lines 576-581) ([ParamFieldWindow.cs](../../src/Smithbox.Program/Editors/Param%20Editor/GUI/ParamFieldWindow.cs)).

#### B13. Map and model link buttons probably never appear *(verify)*
`ReturnPointParam`, `BonfireWarpParam`, `GameAreaParam`, `ItemLotParam`, `AssetGeometryParam` and `BuddyStoneParam` only draw when `imguiId == 1` ([ParamFieldWindow.cs:1140, 1152](../../src/Smithbox.Program/Editors/Param%20Editor/GUI/ParamFieldWindow.cs)). `RenderField` passes its own `idx` as `imguiId`, and `idx` goes up twice per field: once in `FieldRow` (line 1284) and once in `RenderField` (line 846). The sequence runs 0, 2, 4, … and only hits 1 after a hidden padding field or a `-` separator.
*Check:* open ER `AssetEnvironmentGeometryParam` or BB `ReturnPointParam` with the link options on, and see whether the buttons appear, and where.

#### B14. Undo and Redo from the menu behave differently from the shortcuts
- The Ctrl+Z / Ctrl+Y shortcuts call `RefreshParamDifferenceCacheTask()` after undo/redo, but the Edit-menu Undo, Undo All and Redo don't ([ParamEditorScreen.cs:200-224](../../src/Smithbox.Program/Editors/Param%20Editor/ParamEditorScreen.cs) vs [ParamShortcuts.cs:43-60](../../src/Smithbox.Program/Editors/Param%20Editor/Framework/ParamShortcuts.cs)). After a menu undo, the modified-row colours are stale.
- The menu shows the `Undo_Repeat` / `Redo_Repeat` hints (Ctrl+Alt+Z / Ctrl+Alt+Y), but `ParamShortcuts` never handles them. The Map, Model and Text editors do (`IsPressedOrRepeated`).

#### B15. Hovering changes the active view, and every view reads the active view's stay-param state
`ParamEditorView.Display` sets `ViewHandler.ActiveView = this` whenever one of its windows is merely *hovered* ([ParamEditorView.cs:77-80](../../src/Smithbox.Program/Editors/Param%20Editor/GUI/ParamEditorView.cs)). Two effects:
- Keyboard shortcuts act on the view under the mouse, not the one you clicked in. For example, Ctrl+C with the mouse resting over view 2 copies view 2's selection.
- The Rows, Fields and Stay-Param windows decide whether to show by reading `Editor.ViewHandler.ActiveView.Selection.ActiveStayParam` instead of `this.Selection` (lines 89, 114, 135, 158). In DS3, selecting a stay param in view 1 hides the Rows and Fields windows of view 2 whenever view 1 is active, and the windows flicker as the mouse moves between views.

#### B16. Selecting a stay param wipes the view's selection and history
`SelectStayParam` calls `CleanAllSelectionState()`, which clears every param's active row, row selection, search strings, compare row and the back history ([ParamListWindow.cs:596-602](../../src/Smithbox.Program/Editors/Param%20Editor/GUI/ParamListWindow.cs)). Setting `ActiveStayParam` alone would be enough.

#### B17. "Focus" scrolling in the param list is wrong when categories are on
`DisplayParamList` calls `SetScrollFromPosY` at the end of *every* call, and it's called once per category. Only the call that contains the active param has a real `scrollTo`; every later call scrolls to its own stale value ([ParamListWindow.cs:459-473](../../src/Smithbox.Program/Editors/Param%20Editor/GUI/ParamListWindow.cs)).
*Repro:* turn categories on, then follow a reference into a param in an early category. The list jumps to the wrong place. *(verify)*

#### B18. Edit-menu row actions run with no param selected
Edit → Param Row → Delete calls `Params[GetActiveParam()]` with a `null` key and throws `ArgumentNullException` ([ParamRowDelete.cs:15-17](../../src/Smithbox.Program/Editors/Param%20Editor/Tools/ParamRowDelete.cs)). Jump and Paste aren't guarded either. None of these menu items are disabled when they can't run.

#### B19. Smaller correctness issues

> **Status (2026-11-05): B19 fixed, not yet verified in the app.** The project builds.
> - **a:** `param/view/N/...` falls back to the active view when slot N is a closed view (`null`).
> - **b:** the prompt names the *Import* language (from `RowImportLanguages`), and that value is the one passed to the import. If the language isn't in the list, the raw setting is shown instead of throwing.
> - **c:** ID and ID;Name modes copy every selected row; only Name mode skips unnamed rows. Lines are joined once, so duplicate row references no longer confuse the "last line" check.
> - **d:** fixed with P1.
> - **e:** **Unpin All** now clears only the current param, which is what the row tooltip says. A new **Unpin All in All Params** item keeps the old behaviour, in both the row and field context menus.
> - **f:** the community-name lookup (and the wiki icon) require `p?.AppliedParamdef != null`.
> - **g:** table-list Duplicate/Delete shortcuts only fire in the active view.
> - **h:** `RemoveIndices` is cleared at the start of each `Execute`.
> - **i:** `AddParamsAction.Undo` refreshes the diff cache.
> - **j:** the "ignore Name diff" rule checks the `Name` property, not the localised label.
> - **k:** *not a bug.* The `+1` is the separator that is only drawn after pins, so `(1 + null) ?? 0 = 0` is right when there are no pins. The expression is replaced by `GetPinnedFrozenRowCount` to make this explicit; behaviour is unchanged.
> - **l:** fixed with the B9 rewrite of the paste menu (`TryParse`, ID range check, errors logged).
| # | Issue | Location |
|---|---|---|
| a | `param/view/N/...` targeting a closed view: `ParamEditorViews[N]` is `null`, and `.Selection` throws | [ParamCommandQueue.cs:51-62](../../src/Smithbox.Program/Editors/Param%20Editor/Framework/ParamCommandQueue.cs) |
| b | The row-name import prompt names the *Annotation* language, but the import uses the *Import* language. It also throws if the annotation language isn't in the list (`curLanguage.Key`) | [ParamEditorScreen.cs:134-145](../../src/Smithbox.Program/Editors/Param%20Editor/ParamEditorScreen.cs) |
| c | Copy to the OS clipboard skips unnamed rows even in *ID* mode (the `if (r.Name != null)` check wraps all three modes) | [ParamClipboard.cs:84-101](../../src/Smithbox.Program/Editors/Param%20Editor/Framework/ParamClipboard.cs) |
| d | *(Fixed with P1.)* Two views showing the same row share the field-search result: the `fieldFilter` cache is keyed on the row only | [ParamFieldWindow.cs:177-181, 218-222](../../src/Smithbox.Program/Editors/Param%20Editor/GUI/ParamFieldWindow.cs) |
| e | "Unpin All" (rows and fields) clears pins for **every** param, while the tooltip says "currently pinned rows" | [ParamRowWindow.cs:1103-1106](../../src/Smithbox.Program/Editors/Param%20Editor/GUI/ParamRowWindow.cs), [ParamFieldWindow.cs:1575-1578](../../src/Smithbox.Program/Editors/Param%20Editor/GUI/ParamFieldWindow.cs) |
| f | `DisplayParamList` checks `p != null`, then uses `p.AppliedParamdef` unguarded when community names are on | [ParamListWindow.cs:403-430](../../src/Smithbox.Program/Editors/Param%20Editor/GUI/ParamListWindow.cs) |
| g | In table-group mode, Delete in one view's table list deletes the current group in *every* view, because each view's `Shortcuts()` checks the global focus | [ParamTableWindow.cs:396-413](../../src/Smithbox.Program/Editors/Param%20Editor/GUI/ParamTableWindow.cs) |
| h | `DeleteParamsAction.RemoveIndices` keeps growing on each redo (it's never cleared). This is harmless today because it reads from the front | [DeleteParamsAction.cs:28-31](../../src/Smithbox.Program/Editors/Param%20Editor/Actions/DeleteParamsAction.cs) |
| i | `AddParamsAction.Undo` doesn't refresh the diff cache, while `Execute` does | [AddParamsAction.cs:182-202](../../src/Smithbox.Program/Editors/Param%20Editor/Actions/AddParamsAction.cs) |
| j | `internalName == "Name"` compares against the *localised* label passed in by `DisplayRowFields`, so the "ignore Name diff" rule fails in other languages | [ParamFieldWindow.cs:474, 1082](../../src/Smithbox.Program/Editors/Param%20Editor/GUI/ParamFieldWindow.cs) |
| k | `(1 + pinnedFields?.Count ?? 0)` evaluates to 0, not 1, when there are no pins, so one fewer row is frozen | [ParamFieldWindow.cs:136, 234](../../src/Smithbox.Program/Editors/Param%20Editor/GUI/ParamFieldWindow.cs) |
| l | The paste offset is parsed with `long.Parse` inside a catch-all, so a half-typed `-` silently hides the Submit button, and errors from the paste itself are swallowed. `(int)(r.ID + offset)` can overflow unnoticed | [ParamPasteMenu.cs:61-160](../../src/Smithbox.Program/Editors/Param%20Editor/GUI/ParamPasteMenu.cs) |

---

## 2. Performance

These show up as lag on large params (ER `ItemLotParam_map`, `NpcParam`, `SpEffectParam`) and get worse over a long session.

> **Status (2026-11-05): P1 and P2 fixed, not yet verified in the app.** The project builds.
> - **P1:** `ParamFieldWindow.GetFilteredColumns` builds all three field-filter caches from stable keys: the current row as the context, plus a key string with the view index (and, for aux, the loaded aux bank names). Nothing is keyed on the per-frame `auxRows` list any more, and nothing is keyed on the vanilla row, which is `null` for every row the mod added. This also fixes **B19d**: two views now keep separate field-search results. `CacheBank.RemoveCache` still scans every entry, but with the leak gone the entry count stays small.
> - **P2:** `ParamRowWindow.DisplayRows` uses an `ImGuiListClipper`. Table-group rows are filtered *before* clipping, and the selection cache and shift-click ranges now index that filtered list (the cache version includes the group ID). Rows targeted by Go to row, Jump to selected, or a search/selection focus are force-drawn with `IncludeItemByIndex`, so their `SetScrollHereY` / `GetCursorPosY` calls still run. With *line wrapping* on, rows can differ in height, so every row is still drawn in that mode.

#### P1. The `CacheBank` grows every frame (memory leak)
The aux-field cache uses `auxRows` as its key, and `auxRows` is a new `List` built every frame ([ParamFieldWindow.cs:64-69, 183-184, 224-225](../../src/Smithbox.Program/Editors/Param%20Editor/GUI/ParamFieldWindow.cs)). `CacheBank` compares keys by reference ([CacheBank.cs:27-40](../../src/Smithbox.Program/Editors/Common/CacheBank.cs)), so each frame adds an entry that's never read again, even with no aux banks loaded. The entries are only freed by a global `ClearCaches()`, which happens on a search change or an edit. Every `RemoveCache` call scans all entries with a LINQ `Where`, and the selection cache calls `RemoveCache` whenever the selection is dirty, so that scan gets slower over time too.
*Fix:* key on `(activeParam, curRow)` or similar stable values, and replace the `RemoveCache` scan with a dictionary keyed by context.

#### P2. The row list has no `ImGuiListClipper`
Every row in the param is submitted every frame: word-wrap measurement, the FMG decorator, aux diff checks and the context-menu set-up ([ParamRowWindow.cs:408-430](../../src/Smithbox.Program/Editors/Param%20Editor/GUI/ParamRowWindow.cs)). The Param Editor is the only list-heavy editor that doesn't use a clipper (29 other files in `Editors/` do). Table-group mode filters rows inside the loop, so it needs a pre-filtered list for the clipper to work.

#### P3. Per-row work that grows quadratically
| Where | Cost |
|---|---|
| `AuxDiffCaches.Where(cache => cache.Item1.Any(row => row.ID == r.ID))`: a linear scan of a `HashSet` for each displayed row ([ParamRowWindow.cs:660-668](../../src/Smithbox.Program/Editors/Param%20Editor/GUI/ParamRowWindow.cs)) | rows × aux diff rows, every frame |
| `GetSelectionCache`: `rows.Select(x => GetSelectedRows().Contains(x))` on a `List` ([ParamSelection.cs:349](../../src/Smithbox.Program/Editors/Param%20Editor/Framework/ParamSelection.cs)) | rows × selected rows; Select All on 10k rows ≈ 10⁸ comparisons |
| `SortSelection`: `IndexOfRow` inside the comparer (line 385) | m log m × n |
| Table list: `vanillaDiffCache.Any(e => e.ID == id)` and two `GetTableName` lookups per entry ([ParamTableWindow.cs:129-167](../../src/Smithbox.Program/Editors/Param%20Editor/GUI/ParamTableWindow.cs)) | groups × (diffs + groups), every frame |
| Stay params: `param.Value.Write().SequenceEqual(vanilla.Write())` ([ParamListWindow.cs:650](../../src/Smithbox.Program/Editors/Param%20Editor/GUI/ParamListWindow.cs)) | serialises every stay param twice per frame |
| Clipboard text is built with `lines = lines + …` ([ParamClipboard.cs:72-101](../../src/Smithbox.Program/Editors/Param%20Editor/Framework/ParamClipboard.cs)) | O(n²) string copying |

#### P4. Each search keystroke clears every cache
The param, row and field search bars call `CacheBank.ClearCaches()` on every change ([ParamListWindow.cs:113](../../src/Smithbox.Program/Editors/Param%20Editor/GUI/ParamListWindow.cs), [ParamRowWindow.cs:153](../../src/Smithbox.Program/Editors/Param%20Editor/GUI/ParamRowWindow.cs), [ParamFieldWindow.cs:354, 369](../../src/Smithbox.Program/Editors/Param%20Editor/GUI/ParamFieldWindow.cs)). That forces every view, and the other editors sharing `CacheBank`, to rebuild. `RemoveCache(Editor, key)` for the affected entry would be enough.

---

## 3. Usability improvements

> **Status (2026-11-05): U1–U17 done, not yet verified in the app.** The project builds.
> - **U1:** right-clicking a row outside the selection selects it first; right-clicking inside a multi-selection keeps it (chosen option).
> - **U2:** inline edits commit on Enter/click-away, and Escape cancels (B10/B11). The context-menu ID input warns when it would give several selected rows the same ID. Writes go straight through `PropertiesChangedAction`, not the mass-edit grammar (B11).
> - **U3:** the ID and Name selectables share one handler (`HandleRowSelectableInput`). Shift+Arrow extends a range from the active row, and Home/End/PageUp/PageDown move the selection (with Shift, they extend the range). The target row is force-drawn through the clipper, scrolled to, and given nav focus so arrows continue from it.
> - **U4:** `DeleteParamsAction` now owns the selection: Execute removes the deleted rows from every view and selects the next surviving row (or the previous one at the end). Undo restores the deleted rows as the selection. Redo does the same as the first run, which also fixes deleted rows staying selected after a redo.
> - **U5:** pinned rows and pinned params size from `GetFrameHeightWithSpacing()` and scroll past 10 entries.
> - **U6:** match counts next to the row search ("312 / 4,120 rows"), the param search, and the field search (only while a field search is active).
> - **U7:** greying out was done in B18. Undo/Redo now name the next action ("Undo: Delete Rows").
> - **U8:** Undo All asks for confirmation and gives the action count.
> - **U9:** done as B19e.
> - **U10:** while saving, File → Save reads "Saving..." and is disabled. The log gets "Saving params...", then after a save the list of params that had changes (the first 15, then "and N more").
> - **U11:** done as B9.
> - **U12:** the reference-offset options (Attack/Bullet/Behavior/SpEffect/Source) were **never reachable from the UI**, so the B6 fix had no way to be turned on. They're now in the row context menu's Duplicate submenu, showing only the kinds the active param has, with a list of the fields that will be offset (or a note that the offset is 0).
> - **U13:** "Reset to Vanilla (N rows)", where N counts the selected rows that exist in vanilla. It's disabled when N is 0.
> - **U14:** pinned params share the main list's entry code (`DisplayParamEntry`), so they get community names and the modified colour.
> - **U15:** the field menu's Mass Edit header uses the existing (previously unused) `PARAM_FieldWindow_Context_MassEdit_Header` key.
> - **U16:** done as B15.
> - **U17:** rollback was done in B2. Failures now also quote the failing command.

### 3.1 Row list
- **U1. Right-click should act on the row you clicked.** Copy, Delete, Duplicate, Pin and Revert currently act on the existing selection, while *Set Compare Row* uses the clicked row. Either select the row on right-click when it isn't already selected (the usual convention), or label the menu "N selected rows".
- **U2. Inline rename and re-ID** (see B10 and B11): edit the clicked row, Enter commits and leaves edit mode, Escape cancels, warn before giving several rows the same ID, and write the change directly rather than through the mass-edit grammar.
- **U3. Keyboard navigation:** add Shift+Arrow range selection, Home/End and PageUp/PageDown. The ID and Name selectables repeat about 80 lines of click and selection logic (`DisplayRowID` / `DisplayRow`); merging them makes this easier to add.
- **U4. After Delete,** select the next row. **After undoing a delete,** restore the selection.
- **U5. Pinned rows panel:** the height is a fixed `20 + 20·n` px and isn't DPI-scaled, so it clips at high DPI or with compact mode off ([ParamRowWindow.cs:292](../../src/Smithbox.Program/Editors/Param%20Editor/GUI/ParamRowWindow.cs)). The pinned-params panel has the same problem ([ParamListWindow.cs:188](../../src/Smithbox.Program/Editors/Param%20Editor/GUI/ParamListWindow.cs)). Use `ImGui.GetFrameHeightWithSpacing()`.
- **U6.** Show a match count next to each search bar (for example, "312 / 4,120 rows").

### 3.2 Menus and safety
- **U7.** Grey out Undo, Redo, Paste, Delete, Duplicate and Jump when they can't run (pass `enabled` to `MenuItem`), and show the name of the next undo or redo action.
- **U8.** Ask for confirmation before **Undo All**: it reverts the whole session in one click.
- **U9.** Scope **Unpin All** to the current param, and add a separate "Unpin in all params" item (B19e).
- **U10.** Show save progress (a status line and a disabled Save while it runs), and after a save, log which params were written.

### 3.3 Paste and Duplicate
- **U11.** Name the target param in the paste popup. Block cross-param paste unless the user opts in (B9). Validate the row and offset fields inline instead of letting an exception hide the button, and preview the resulting ID range.
- **U12.** The Duplicate offset and amount sit inside a right-click sub-menu. The "Affect Attack/Bullet/… field" toggles now work per type (B6), but nothing shows which fields a duplicate will change; a preview list would help.

### 3.4 Fields window
- **U13.** "Reset to Vanilla" on a field applies to the whole selection (`selection && !added`). Show the count in the label, for example "Reset to Vanilla (3 rows)".
- **U14.** Make the pinned-params list match the main list: it currently has no community names and no modified-colour text.
- **U15.** Localise the hard-coded `"Mass Edit"` menu header ([ParamFieldWindow.cs:1505](../../src/Smithbox.Program/Editors/Param%20Editor/GUI/ParamFieldWindow.cs)).

### 3.5 Multi-view and mass edit
- **U16.** Make the active view follow a click or keyboard focus, not hover (B15).
- **U17.** When a mass-edit script fails, roll it back and name the failing line and command (B2). Today the message only gives a line number, and the edits before it stay applied.

---

## 4. Suggested order

1. **B1, B2, B3** (silent data loss), then the crashes **B4–B8** and **B18**. Most of these are one-to-ten-line fixes.
2. **P1** (leak) and **P2** (clipper): the biggest responsiveness gain on large params.
3. **B9–B11** together with **U1/U2** (paste target, rename and right-click semantics), since they touch the same code paths.
4. The rest of the Medium bugs, then the remaining usability items.

## 5. What to check in the app (for the *(verify)* items)
- **B13** and **B17**: now fixed; see their checks below.
- **B1:** with automatic save on, edit continuously through a save, then check that the param's `*` mark still matches what's on disk (reload the project to compare).

### Checks for the B1–B3 fixes
1. **Save keeps mid-save edits dirty:** in ER, edit a field in `EquipParamWeapon`, press Ctrl+S, and change another field right away while the "Saving" log is still pending. After "Params saved." the param should still show `*`, and the log should say "1 param(s) were edited while saving". Save again, reload the project, and both edits should be there.
2. **Row changes refused during a save:** press Ctrl+S and immediately press Ctrl+D, Delete or Ctrl+Z. The log should warn "Params are being saved…", and the row list should be unchanged. Once the save finishes, the same keys should work again.
3. **Save lock recovers from a failure:** make a file under `.smithbox/Project/Param Table Names/<lang>/` read-only, then save in a table-group param (ER `ItemTableParam`). An error is logged. Restore the file and save again: it should save, not report "Param saving already in progress".
4. **Mass-edit rollback:** select a row, then run `selection: ID := 999999; selection: notAField: = 1` in the mass-edit window. The result should show the parse error plus "The commands before this one were undone…", and the row's ID should be unchanged. Ctrl+Z shouldn't be needed.
5. **Redo keeps edits on pasted rows:** copy a row, paste it, change a field on the pasted row, then press Ctrl+Z twice and Ctrl+Y twice. The pasted row should come back *with* the changed field. Repeat with Ctrl+D (duplicate) and with a table-group duplicate in `ItemTableParam`.
6. **Replace-mode undo:** in the paste or commutative-duplicate path with "replace existing rows" on, paste over two existing rows and undo. Both originals should be back at their original positions.

### Checks for the B4–B8 fixes
1. **B4:** follow a reference to a row ID that doesn't exist (type a missing ID into a reference field, then use its go-to-reference link). Then press Ctrl+C, Ctrl+D and Delete: nothing should happen, and there should be no crash. Normal copy, duplicate and delete should still work after you click a row.
2. **B5:** right-click the *Name* label, then the *ID* label, in the Fields window. The Name Manipulation sub-menu shouldn't appear for either. It should still appear on normal fields, and Set Target Field should work there.
3. **B6:** in DS3 `Bullet`, set a Duplicate offset of 10 and tick only *Affect SpEffect*. Duplicate a row: only its SpEffect reference fields should change (+10), and its Attack and Bullet references should stay the same. Then tick only *Affect Bullet* and repeat. Also duplicate a row in a param with no meta file; it shouldn't crash.
4. **B7:** in ER `ItemTableParam` (table list on), select a group, press Delete, then Ctrl+D and Delete again. There should be no crash and no extra undo entry. The table list should show no selected group.
5. **B8:** select a mix of named and unnamed rows, then use Information → Copy Name and Copy ID and Name. You should get one line per row, with empty names for unnamed rows.

### Checks for the B9–B11 fixes
1. **Paste target:** copy a row in `SpEffectParam`, switch to `Bullet`, and press Ctrl+V (and try Edit → Paste). The popup should say the rows came from `SpEffectParam` and offer *Go to SpEffectParam*, with no Submit button. Click it, then paste normally: the row should land in `SpEffectParam`.
2. **Paste input:** in the popup, clear the Offset box or type `-`. A "must be whole numbers" message should appear, and nothing should crash. Then type a valid offset: the "New row IDs" preview should update.
3. **Paste after selection:** select a row near the bottom, then Ctrl-click one near the top (in that order). Paste with *Paste after selection*: the rows should go below the *bottom* row, with IDs after it.
4. **Alt-click edit:** select row A, then Alt-click row B's name. The input should be focused immediately. Type a new name and press Enter: only B is renamed, and the input closes. Alt-click again and press Escape: nothing changes. Repeat on the ID column.
5. **Names with syntax characters:** rename a row (Alt-click, and separately via right-click → Name input) to `Sword; Fire`, then to `## WIP`. Both should be saved exactly. Ctrl+Z should undo each in one step.
6. **Batch rename / regroup:** select three rows and use the right-click Name input. All three should be renamed, and the tooltip should say "3 selected row(s)". In `ItemTableParam`, use the right-click ID input on two selected rows: both should move to that group ID.
7. **During a save:** start a save, then immediately rename a row with Alt-click. The rename should go through, and the param should stay marked `*` afterwards.

### Checks for the B12–B18 fixes
1. **B12:** with Field Layouts on and *Pinned stay visible* on, pin two fields in ER `EquipParamWeapon`. Each should appear once at the top, not again above the groups.
2. **B13:** with the map link option on, open ER `BonfireWarpParam` or `ItemLotParam_map`, and with the model link option on, open ER `AssetEnvironmentGeometryParam`. The *View in Map* / *View Model* button should appear once, in the ID row. In-line buttons such as `BulletParam`'s asset fields should be unchanged.
3. **B14:** make an edit, then undo it from Edit → Undo. The row's modified colour should update immediately. Hold Ctrl+Alt+Z after a few edits: it should undo repeatedly while held, and a single tap should undo exactly one edit. Undo and Redo should be greyed out when there's nothing to undo or redo.
4. **B15:** open two views. Click in view 1, rest the mouse over view 2, and press Ctrl+C: view 1's selection should be copied. Right-click a row in view 2: view 2 becomes active. In DS3, open a stay param in view 1: view 2's Rows and Fields windows should stay visible and steady.
5. **B16:** in DS3, select a few rows in `EquipParamWeapon` and type a row search. Open a stay param, then click `EquipParamWeapon` again: the selection and search should still be there. While the stay param is open, Delete and Ctrl+D should do nothing.
6. **B17:** with categories on, follow a field reference into a param in each category (top, general, bottom). The param list should scroll to the param each time.
7. **B18:** with no param selected, open Edit → Param Row. Duplicate, Delete, Copy and Jump should be greyed out, and Paste too unless rows have been copied. Select a row: they should enable.

### Checks for the B19 fixes
1. **B19b:** on a fresh project, set Annotation language and Import language to different values before opening the Param Editor. The row-name prompt should name the Import language.
2. **B19c:** set Row Copy Behaviour to *ID*, select some rows including an unnamed one, and press Ctrl+C. The OS clipboard should hold one ID per selected row. *Name* mode should skip the unnamed row.
3. **B19e:** pin rows and fields in two params. **Unpin All** in one param should leave the other param's pins alone; **Unpin All in All Params** should clear both.
4. **B19g:** with two views open in ER `ItemTableParam` (table-group mode), click a group in view 1's table list and press Delete. Only view 1's group should be deleted.
5. **B19j:** switch the UI language away from English and open a renamed row. The Name field shouldn't be coloured as modified just for having a name.
6. **B19i:** paste some rows, then undo. The param list and row colours should drop the modified highlight. (Editor undo already refreshed after B14; this fix matters for undo paths that bypass it, like a mass-edit rollback that undoes added rows.)

### Checks for the usability items
1. **U1:** select row A, then right-click row B and choose Delete. Only B should be deleted. Then select A–C, right-click B, and Copy: all three should be copied.
2. **U3 arrows:** click a row, then hold Shift and press Down a few times. The range should grow; Shift+Up back past the anchor should flip it. Plain Down afterwards should drop back to a single row.
3. **U3 page keys:** in a large param, press End, Home, PageDown and PageUp with the row list focused. The selection should jump and the list scroll to it, and the Down arrow should then continue from the new row. Shift+PageDown should extend the range. Also check that Home/End inside the row search box still move the text cursor (they shouldn't move the selection).
4. **U4:** select three rows in the middle of a param and press Delete. The row after them should be selected. Undo: the three rows should come back selected. Redo: the next row should be selected again, and none of the deleted rows.
5. **U5:** pin 1, 5 and 12 rows (and params) with compact mode on and off. The panels should fit their entries without clipping, and scroll beyond 10.
6. **U6:** type a row search; the count next to it should update (it lags one frame, which shouldn't be visible). Do the same with a param and a field search.
7. **U7/U8:** after a field edit, Edit should show "Undo: Edit Value". Undo All should ask first, and Cancel should change nothing.
8. **U10:** save with a few params edited. The log should list exactly those params. While the save runs (it's quick on small games; try ER), File → Save should be greyed out.
9. **U12:** in ER `EquipParamWeapon`, right-click → Duplicate. The reference options should appear with a list of fields; tick one, set the offset to 1, and duplicate. Those fields on the copy should be +1 (unless they were -1). Try a param without references; it should say so.
10. **U13:** select 3 vanilla rows and 1 added row, then right-click a field. The menu should read "Reset to Vanilla (3 rows)".
11. **U14:** pin a modified param. It should be coloured like it is in the main list, and use the community name when that toggle is on.
12. **U17:** run a two-command mass edit whose second command has a typo. The error should quote the second command and say the first was undone.

### Checks for the P1/P2 fixes
1. **Row list speed:** open a large param (ER `ItemLotParam_map` or `SpEffectParam`) and scroll quickly. It should feel noticeably smoother than before, and every row should still render with its ID, name, colours and FMG decorator.
2. **Scroll targets:** scroll to the top, then use a field reference to a row far down the list (or type its ID into the row search). The list should scroll to it and select it. Then scroll away, select a row from a reference, and press the Jump keybind / location-arrow button: it should scroll back.
3. **Keyboard navigation:** click a row and hold the Down arrow past the bottom edge. The selection should keep moving, and the list should keep scrolling.
4. **Selection:** shift-click a range spanning a screen and a half, Ctrl-click a few rows, then scroll. Selected highlights should stay correct, and Ctrl+C should copy every selected row.
5. **Table-group mode:** in ER `ItemTableParam`, switch between groups. Only that group's rows should show, with correct selection highlights and chance percentages.
6. **Line wrapping:** turn on row line wrapping. Long names should still wrap to up to three lines, and nothing should overlap.
7. **Field search per view (B19d):** open two views on the same row and type different field searches in each. Each view should keep its own filtered fields.
8. **Leak:** leave the editor on a row with the Fields window open for a few minutes. Memory use in Task Manager should stay flat rather than climbing steadily.
