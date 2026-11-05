# Gparam Editor: Bugs in the File, Group, Field and Value List Actions

Scope: the four list panels (`GUI/GparamFileList.cs`, `GparamGroupList.cs`, `GparamFieldList.cs`, `GparamValueList.cs`), what they call (`Framework/GparamSelection.cs`, the `Actions/` folder, `Tools/GparamDataTransferTool.cs` import/export, the Edit menu in `GparamEditorScreen.cs`), and the value cells in `GparamPropertyEditor.cs`.

I found these by reading the code. None has been reproduced in the app yet. Each one has a **Repro** line so it can be checked before and after its fix.

Some background that several of the bugs depend on:

- `GPARAM.Write` writes `Field<T>.Capacity` as the value count, and `Read` reads exactly `Capacity` values (`GPARAM.cs:1088-1146`). If `Capacity` doesn't match `Values.Count`, the saved file is corrupt.
- `UnkParamExtras` is one entry per group: a `GroupIndex` and the list of value IDs used in that group. The value actions (`AddValueAction`, `DeleteValueAction`, `EditValueIdAction`) rebuild it. The group and field actions don't.
- The undo stack (`ActionManager`) holds actions that keep **object references** (`Data`, `TargetGroup`, `TargetField`). Any action that swaps an object for a copy cuts off every older action that points at the original.
- Selection keeps an **index** (used for the row highlight and to gate the context menu) and a **key** (used by `GetSelectedGroup()`/`GetSelectedField()`). Nothing keeps the two in sync after an add, delete or undo.

---

## Critical: crashes or corrupt saves

### C1. Shift-click as the first click in a field crashes
`GparamSelection.HandleMultiselection` (`GparamSelection.cs:229-258`) builds a range from `_selectedFieldValueIndex`. Selecting a field resets that to `-1`, so the loop starts at `k = -1`. The guard `k < fieldList.Values.Count` lets it through, and `fieldList.Values.ElementAt(-1)` throws.
**Repro:** select a field, then Shift+click any value row.
**Fix:** if the anchor is `< 0`, treat the click as a normal one. Clamp `start` to `0`.

### C2. Import Value (append mode) writes a corrupt file
`GparamFieldExtensions.AddValue` (`GparamFieldExtensions.cs:106`) appends a value without updating `Capacity`. It also ignores its `index` argument, and the cast `(FieldValue<T>)newValue` throws if the JSON is for a different field type.
**Repro:** turn off Import Overwrite, import a value JSON into a field, save, and reopen the file. The field's values come back misread.
**Fix:**
- Insert at `index + 1`.
- Set `Capacity = Values.Count`.
- Check the type before the cast and log an error instead of throwing.
- Make undo remove that exact value; see C3.

### C3. Import undo/redo replaces the live GPARAM and breaks undo history
`ImportGparamAction`, `ImportParamAction`, `ImportFieldAction` and `InsertFieldValueAction` all undo by setting `wrapper.Gparam = StoredGparam`, which is a **clone**. The results:
- Every older action still points at the original GPARAM, so undoing past an import edits an object nothing displays or saves. `MarkAsDirty(Data)` no longer finds a wrapper either.
- `ImportFieldAction` and `InsertFieldValueAction` redo into the original `Param`/`Field`, which are now detached, so redo looks like it does nothing.
- `ImportParamAction` undo sets `CurrentGparam = StoredGparam`, so redo writes the imported group into the snapshot itself. The next undo is then a no-op.

**Repro:** edit a value, import a group, undo twice. The value edit isn't reverted. Separately: import a field, undo, redo. Nothing changes.
**Fix:** rewrite all four as in-place edits that keep object identity:
- Group: replace or insert at an index; undo puts back the old object or removes the inserted one.
- Field: same at field level.
- Value: same at value level.
- GPARAM: copy the header and the `Params`/`UnkParamExtras` list contents into the existing instance, and keep the old contents for undo.

None of them should swap `wrapper.Gparam`.

### C4. Deleting with nothing selected leaves a null that crashes after undo
The Delete keybind (`GparamGroupList.Shortcuts`, `GparamFieldList.Shortcuts`) and the Edit menu (`DeleteGroupsShortcut`, `DeleteFieldsShortcut`) pass `new List { GetSelectedGroup() }`. That can be `null` when nothing is selected or the selected key went stale (H1). `Execute` skips it. `Undo` then **inserts the null** at the fallback index: `DeleteGroupAction.cs:61-65`, `DeleteFieldAction.cs:61-65`.

After that, `GetSelectedGroup()`/`GetSelectedField()` throw on `e.Key`, and saving throws in `Param.Write`. The fallback in `DeleteGroupAction` also uses `StoredGroups.Count` where it should use `Data.Params.Count`, which can be out of range.
**Repro:** select a file, focus the group list without selecting a group, press Delete, then Undo.
**Fix:**
- Don't create an action when the target is null.
- In the Delete actions, store `(index, object)` pairs for the items actually found. Undo restores those pairs in ascending order, and nothing else.

### C5. Duplicate and delete leave a stale value selection, which duplicates values on undo
`SelectedFieldValues` isn't cleared after `DeleteValueAction`. Pressing Delete again creates an action whose targets are no longer in the list. Nothing is removed, `StoredIndices` stays empty, and **Undo inserts every target at the end** (`DeleteValueAction.cs:113-120`). Undoing the first delete then puts them back a second time.
**Repro:** select a value, press Delete twice, Undo twice. The value appears twice.
**Fix:** use the same `(index, object)` pairs as C4, skip the action when no target is present, and reconcile the selection after the action (H1).

### C6. Edit-menu and keybind actions dereference null
- `AddGroupsShortcut` with no file selected: `data.Params` throws.
- `AddFieldsShortcut` with no group selected: `PopulateAddOptions(null)` throws on `param.Key`.
- `AddValuesShortcut`/`DeleteValuesShortcut` with no field selected: the action runs `UpdateGroupIndexes(null)`.
- `PopulateAddOptions` in both lists returns early and leaves `AddOptions == null` when no annotation set matches `GparamEditor_Annotation_Language`. `AddGroupMenu`, `AddFieldMenu` and both Add shortcuts then throw.

**Repro:** with no file selected, open Edit > Groups > Add All Missing.
**Fix:** check the required selection at the top of each `*Shortcut` method and Add path, and grey out the menu items with `ImGui.MenuItem(..., enabled)`. Make `PopulateAddOptions` always leave a list behind, empty if there's nothing to add.

### C7. Copy As can crash and mishandles several cases
`GparamFileList.CopyAsMenu` (`GparamFileList.cs:261-311`):
- `File.Copy` throws when the target exists or its folder isn't in the project yet (a new project with no `/param/drawparam`). Nothing catches it, and the new entry has **already been added** to the locator and bank.
- The source is looked up with `File.Exists` on `ProjectPath`/`DataPath`. For an unpacked ER install (VFS archives) the vanilla file isn't on disk, so Copy As fails unless the file has already been saved into the project. CLAUDE.md says to use the VFS for ER.
- `Path.Replace(oldName, newName)` replaces every match in the path, including folder names.
- For a BB `gparambnd`, the binder's inner file keeps the old name. `LoadGraphicsParam` looks the inner file up by the new name, never finds it, and the copy never loads.
- It copies the file on disk, not the in-memory GPARAM, so unsaved edits are left out. That may be intended, but the tooltip should say so.
- It doesn't check for an existing entry with the same filename.

**Fix:**
- Read the source through `Project.VFS.FS`.
- Rename only the file-name part of the path.
- Create the folder.
- Refuse when the target exists.
- Rename the inner binder file for `gparambnd`.
- Wrap it in try/catch and add the entry only after the write succeeds.

---

## High: wrong results or broken undo, no crash

### H1. Index and key selection drift apart after add, delete or undo
After `DeleteGroupAction`, the selection still holds index *i* and the deleted key. The row now at *i* is highlighted, `GetSelectedGroup()` returns null so the field list goes blank, and the context menu (gated on `index == _selectedParamGroupIndex`) opens on the **neighbouring** row and acts on it. Fields and values behave the same way. Undoing an add while the added row is selected does the same thing.
**Fix:** add `GparamSelection.Reconcile()`. It re-resolves each level's index from its key (for values, by object reference), resets a level whose key is gone, and drops values no longer in the field from `SelectedFieldValues` while re-keying the rest to their new indices. Call it from an `ActionManager.ActionPerformed` handler registered in `GparamEditorView`, so every execute, undo and redo is covered.

### H2. Group and field add/delete don't rebuild `UnkParamExtras`
Deleting group *k* leaves every extra with `GroupIndex > k` pointing at the wrong group. Adding a group or field adds value ID 0 that no extra lists. `ImportParamAction` has the same gap. The value actions do rebuild, so whether the extras are correct depends on whether a value was edited afterwards.
**Fix:** move the three copies of `UpdateGroupIndexes` into one `GparamUtils.RebuildParamExtras(GPARAM)`. Call it from all structural actions, keeping a snapshot for undo. See **Decision D2** for what to keep.

### H3. "Add value" on an empty field can't be undone and breaks older undo steps
`GparamValueList.AddNewValue` runs `ReplaceFieldAction`, whose `Undo` does nothing ("we just don't support undoing this action"), yet it's still pushed onto the undo stack. It also **replaces the field object**. So when an earlier `DeleteValueAction` emptied that field, undoing it re-adds the value to the detached old field.
**Repro:** delete a field's only value, click the empty-row "add" entry, Undo twice. The field stays at one default value, and the original value never comes back.
**Fix:** drop `ReplaceFieldAction` here. Build one default `FieldValue<T>` for the field's type (ID from D1, value from the annotation default) and add it with `AddValueAction`, so the field object stays the same and undo works. The annotation is then only needed for the default value, not for the field to exist.

### H4. Deleting a file is permanent and leaves the editor in a bad state
`DeleteGparamFile` (`GparamFileList.cs:326-337`):
- It runs `File.Delete` straight from the context menu, with no confirmation and no undo.
- It removes the entry from the locator and bank even when a vanilla copy still exists. The file disappears from the list until the project reloads, when it should go back to the vanilla version.
- It doesn't reset the selection, so the group list keeps showing the deleted GPARAM.
- Undo history for that file stays live and edits an object nothing is attached to.

**Fix:** ask for confirmation with a modal. After deleting, if the VFS still has the file, keep the entry and reload it (`IsDirty = false`, `Gparam = null`, then `LoadGraphicsParam`); otherwise remove it. Reset the selection when the deleted file is selected, and clear the view's `ActionManager`, since its actions can't be retargeted. See **D3**.

### H5. File selection races with the async load
`SetFileSelection` is `async void`. It resets the group and field, `await`s `LoadGraphicsParam` (whose first line is `Task.Yield()`), and only then sets `_selectedGparam`.
- `GparamCommandQueue` calls it and then checks `IsFileSelected()` on the same frame. For a file that isn't loaded yet, that check sees the **previous** file. The group and field are picked against the old GPARAM, and the field add options aren't invalidated.
- Fast arrow-key browsing can start several loads at once. They finish in any order, so the selection can end up on a file the user skipped past.
- The continuation can run off the UI thread and modify `Entries` while `DisplayFileList` is iterating it.

**Fix:** none of the load work is actually async, so add a synchronous `LoadGraphicsParamSync` and have selection use it. Set `_selectedGparam = null` before loading. The command queue then works as written.

### H6. Duplicating a value gives it the same ID as the source by default
`SetGparamFieldValue` sets `DuplicateValueID = entry.ID`, and `DuplicateValueOffset` defaults to 0. Both the context-menu duplicate (fixed ID) and the keybind duplicate (ID + offset) therefore create a second value with the **same ID in the same field**. The game reads values by ID, so the copy is ambiguous. The ID cell (`EditValueIdAction`) also allows duplicate IDs without a warning. See **D1**.

### H7. Import mishandles cancel and bad files
- `ImportGPARAM` calls `ResetGparamFileSelection()` even when the dialog is cancelled. `ImportGparamAction` and `ImportParamAction` also reset the whole selection on execute, so after importing a group the file is deselected.
- None of the `*FromJson` calls is wrapped in try/catch. Picking the wrong kind of JSON, such as a field file in Import Group, throws.

**Fix:** reset only on success, and point the selection at the imported object instead of clearing it. Catch parse errors and log them.

---

## Medium: UX and consistency

| # | Issue | Where | Fix |
|---|---|---|---|
| M1 | Right-clicking a row that isn't selected does nothing; every list's context menu only opens on the selected row. | all four `ContextMenu` | Select the row on `ImGui.IsItemClicked(ImGuiMouseButton.Right)`, then open the popup. |
| M2 | Empty actions go onto the undo stack and mark the file dirty: Submit with nothing ticked, Duplicate or Delete with no values selected. | `AddNewGroups`, `AddNewFields`, `AddValues`, `DeleteValues` | Return early when the target list is empty. |
| M3 | Edit > Fields > **Duplicate** runs `AddFieldsShortcut`, which adds *all missing fields*, and shows the Duplicate keybind hint. The Add keybind on the group and field lists also adds every missing entry with no confirmation. | `GparamEditorScreen.cs:183`, both `Shortcuts` | Rename it to "Add All Missing Fields" with the Add hint. See **D4**. |
| M4 | Field add options aren't invalidated when the file changes, when the command queue navigates, or when the annotation language changes. Group add options are built once and never rebuilt. | `PopulateAddOptions` ×2 | Key the cache on (language, group key) and rebuild when either changes. |
| M5 | Context-menu Delete and Duplicate act only on the clicked value, while the keybind acts on the whole multi-selection. | `GparamValueList.ContextMenu` | When the clicked row is in the multi-selection, act on all of it. |
| M6 | "Toggle All" flips each checkbox, so a mixed set stays mixed. | `AddGroupMenu` | If any are unticked, tick all; otherwise untick all. Add the same control to the field add menu. |
| M7 | Add-menu checkbox IDs use the annotation display name, so two names that collide share an ID. | `AddGroupMenu`, `AddFieldMenu` | Use `{Name}##{ID}`. |
| M8 | With "hide empty groups" on, a file whose groups are all empty shows no rows and no dummy row, so there's nowhere to right-click to add a group. | `DisplayGroupList` | Show the dummy row when nothing is visible, not only when `Params.Count == 0`. |
| M9 | For BB, the selection is tracked by `Filename` only. `x.gparam` and `x.gparambnd` share a name, so after switching the BND toggle the highlighted row isn't the GPARAM being shown. | `GparamFileList.cs:130`, `SetFileSelection` | Compare `FileDictionaryEntry` (path or reference), not the filename. |
| M10 | Exports and Copy Value don't handle errors or culture: no try/catch on `File.WriteAllText`, and `value.Value.ToString()` formats with the current culture. | `Export*File`, Copy Value | Add try/catch and format with `CultureInfo.InvariantCulture`. |
| M11 | Every frame, `DisplayFileList` calls `Entries.ElementAt(i)` on a dictionary, which is O(n²). | `GparamFileList.cs:75-77` | `foreach` over a snapshot (`Entries.ToList()`), which also makes it safe when Delete or Copy As changes the dictionary mid-loop. |
| M12 | The file `Actions/ImportFieldValueAction .cs` has a space in its name. | | Rename it. |

---

## Decisions needed (one at a time, before the phase that needs it)

- **D1: ID for duplicated and new values** (H6, H3). Options: next free ID in the field (recommended); keep the source ID but warn; or always ask.
- **D2: `UnkParamExtras` rebuild** (H2). Options: rebuild from scratch, as the value actions already do (resets `Unk0c`); or rebuild while keeping each surviving group's `Unk0c` and ID order, mapping old group index to new (recommended).
- **D3: File delete** (H4). Options: revert to vanilla when one exists, otherwise remove (recommended); or always remove the entry.
- **D4: The Add keybind on the group and field lists** (M3). Options: keep "add all missing"; or open the add menu instead (recommended).

---

## Phased plan

**Phase 1: Shared fixes (needed by everything after it).** Status: implemented 2026-11-05 and builds; not yet checked in the app. D2 = keep each surviving group's `Unk0c` and ID order. `Reconcile()` runs every frame at the top of `GparamEditorView.Display` and also on `ActionPerformed`, so edits from other views and tools are covered too. Group/field actions start calling `RebuildParamExtras` in Phase 2.
1. `GparamSelection.Reconcile()`, hooked to `ActionManager.ActionPerformed` (H1).
2. Null and empty guards in every `*Shortcut` and Add/Delete entry point, with menu items greyed out (C4 part 1, C6, M2).
3. `PopulateAddOptions` always returns a list; options cache keyed on language and group (C6, M4).
4. `GparamUtils.RebuildParamExtras` shared by all actions (H2, after D2).
5. Shift-range anchor fix (C1).

**Phase 2: Fix the actions.** Status: implemented 2026-11-05 and builds; not yet checked in the app. D1 = next free ID. The add actions build their new objects once, so redo puts back the same objects later redo steps refer to. Value list edits go through new `GparamFieldExtensions` helpers (`InsertValueAt`/`RemoveValue`/`ReplaceValueAt`, which keep `Capacity` in step, plus `AcceptsValue` and `CreateDefaultValue`). `ReplaceFieldAction` was moved to `.archive`. Not covered: `GPARAM.Clone()` in SoulsFormats doesn't copy `Unk40`; nothing in the editor's undo uses it any more.
1. Delete group, field and value: store `(index, object)` pairs, skip missing targets, undo restores exactly (C4, C5).
2. Group and field add/delete rebuild the extras, with an undo snapshot (H2).
3. Replace `ReplaceFieldAction` with a default value added through `AddValueAction` (H3, after D1).
4. Rewrite the four import actions as in-place edits; fix `AddValue`'s capacity, index and type check (C2, C3, H7).
5. Duplicate ID handling (H6, after D1).

**Phase 3: File list.** Status: implemented 2026-11-05 and builds; not yet checked in the app. D3 = revert to vanilla. Copy As writes the editor's GPARAM (unsaved changes included) when the file is loaded, and the file read through the VFS when it isn't; the tooltip says so. Delete clears the undo history of every view. The Data Finder's jump-to-result now goes through the selection setters, so the anchor value that `Reconcile()` tracks is set; before, Phase 1 would have cleared its value highlight. The LOC changes were mirrored into the tracked `Smithbox.Release/Output` copy. Not covered: with BB's `x.gparam` and `x.gparambnd`, the command queue still matches on the file name, so the last one listed wins.
1. Synchronous load for selection; command-queue order (H5).
2. Copy As through the VFS, with folder creation, a collision check, BND inner-file rename and error handling (C7).
3. Delete with confirmation, revert to vanilla, selection reset and history clear (H4, after D3).
4. Selection identity by entry, not filename (M9); `foreach` over a snapshot (M11).

**Phase 4: UX.** Status: implemented 2026-11-05 and builds; not yet checked in the app. D4 = the Add key opens the add menu as a popup at the mouse. The Edit menu's "Add All Missing" items no longer show the Add key as their shortcut, and the mislabelled Fields "Duplicate" item is now "Add All Missing Fields" (new LOC keys; Chinese falls back to English for them). Toggle All uses `NoAutoClosePopups`, which also stops it closing the add menu on each click. `ImportFieldValueAction .cs` was renamed to `InsertFieldValueAction.cs` with `git mv`, so the rename is staged.
M1, M3 (after D4), M5, M6, M7, M8, M10, M12.

---

## Checks after each phase (in the app)

Use an ER project with `m60_00_00_00.gparam.dcx` or any map GPARAM. Add BB for Phase 3 if a BB project is available.

- **Phase 1:**
  - Shift+click the first value row after selecting a field: no crash.
  - Delete a group, then check the highlight. It's cleared, or on the right group, and the field list matches.
  - Edit > Groups > Add All Missing with no file selected: the item is greyed out.
- **Phase 2:**
  - Delete a value twice, then undo twice: one copy, back in its original place.
  - Delete a group, undo: the same group is back at the same position; save and reopen, and it loads.
  - Empty a field, add a value, then undo twice: the original value is restored.
  - Edit a value, import a group, undo twice: both changes are reverted. Import a field, undo, redo: the field is replaced again.
  - Import a value (append), save, reopen: the value count matches.
- **Phase 3:**
  - Copy As on an ER vanilla-only file: the copy appears and loads.
  - Copy As onto an existing name: error in the log, no crash.
  - Delete a project-edited file: confirmation appears, the vanilla version comes back in the list, and the group list clears.
  - Hold the down arrow through the file list: the final selection matches the highlighted row.
- **Phase 4:** right-click an unselected row: it gets selected and the menu opens on it.
