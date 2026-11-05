# Havok Editor Review: Bugs and Usability

Scope: `src/Smithbox.Program/Editors/Havok Editor/**`, plus the code it depends on: `Keybinds/InputManager.cs`, `Editors/Common/Utils/PropFinderUtil.cs` and the autosave hook in `Editors/Project Editor/ProjectEntry.cs`. `GUI/MapHavokPropertyView.cs` lives in this folder but belongs to the Map Editor, so I only checked it where it shares code with the Havok Editor.
Method: I read the code and didn't run anything. Each bug has steps to reproduce it in the app. Items marked *(verify)* depend on ImGui, HKLib or game behaviour I couldn't confirm from the code alone.

Severity key: **High** = corrupts or loses data, or crashes; **Medium** = wrong behaviour users will run into; **Low** = edge case or latent issue.

Supported games (`EditorConditions.SupportsHavokEditor`): ER, AC6, NR, DS3, DB, DB_NT. That makes the BND3/BXF3 branches in `HavokBank` dead code for now.

Paths below are relative to `src/Smithbox.Program/Editors/Havok Editor/` unless stated otherwise.

---

## 1. Bugs

### 1.1 Data loss and crashes (High)

> **Status (2026-11-05): B1–B6 fixed, plus M1, M2 (partly) and L2 as side effects. Not yet verified in the app.** `Smithbox.Program` builds. The new loc keys exist in English only.
> - **B1:** `HavokShortcuts.Monitor()` returns after the Save check while `ImGui.GetIO().WantTextInput` is set. Undo/redo and the per-panel shortcuts (file Copy/Paste/Delete) no longer fire while you type. Save still works everywhere.
> - **B2 + M1:** `HavokPropertyView.GetSourceObject()` returns `SelectedFileWrapper?.Root`. *Set to Free ID* now searches the real graph. The Collision Generator builds an undoable `HavokDelegateAction` with the new `CreateReplaceExternMeshShapeAction` in each shape generator (HKX2 also restores `m_referencedObjects` on undo). It runs the action through the view's `ActionManager`, so the file gets its `*` and is saved. If there's no replaceable mesh body, it logs `HAVOK_CollisionGen_Generate_No_Replaceable_Shape` instead of failing silently. The old `ReplaceExternMeshShape` methods are now unused but kept.
> - **B3:** `Save`/`SaveAll` are synchronous and run on the UI thread (`RunSave` replaces `RunSaveTask`), so an edit can't land mid-serialisation and `IsDirty = false` can't clear a later edit. `OnSaveComplete` runs on the UI thread, only when something was written, and null-checks the Map Editor view (**L2**).
> - **M2 (partly):** autosave returns at once when `HavokEditor_Allow_Automatic_Save` is off, and no longer logs "no binder/file selected". Autosave still only saves the selected binder (see U2).
> - **B4:** every Havok action implements `IHavokFileAction` (it exposes its `FileWrapper`), and `ActionManager.GetHistory()` lists the undo and redo stacks. `HavokBank.GetReferencedWrappers` (set by the view) gives the files in the history. `PopulateFiles` never evicts a binder or replaces a wrapper that the history refers to. `LoadHavokFile` no longer re-reads a file that's already loaded. Undo and redo therefore always apply to the displayed objects. Unedited files are still evicted on binder switches, so memory stays bounded. Deleting a file still drops its wrapper, so undo for that file becomes a no-op on a copy that's no longer shown (as before).
> - **B5:** the list *Add* button works out the type to create (`GetNewListEntryType`). Abstract or interface element types use the first non-null entry's type: this extends the old `hkbGenerator` special case to every abstract type. When nothing can be created (empty abstract list, or no parameterless constructor), the button is disabled and a tooltip explains why (`HAVOK_PropertyView_Add_List_Entry_Abstract_TT`).
> - **B6:** clip alias cache uses `TryAdd`. The State Machine, Selector and Clip tabs treat a null `m_name` as `""`.

**B1. Delete, Paste and Undo keys fire while you're typing in a text box. Delete removes files from disk.**
`HavokShortcuts.Monitor()` and `HavokFileView.HandleShortcuts()` call `InputManager.IsPressed`, and nothing checks `ImGui.GetIO().WantTextInput`. Focus is set by *hovering* a window (`HavokEditorView.Display`), and the file list's search bar is inside the File List window.
- Repro: select a file in the File List. Click into the File List search bar, type `abc`, then press **Delete** to remove a character. The selected file is removed from the binder, and the binder is written to the project straight away (`HavokBank.DeleteFiles` → `ModifyContainer` → `WriteContainer`). There's no confirmation and no undo.
- Same route: **Ctrl+V** in the search bar pastes the clipboard files into the binder and writes it. **Ctrl+Z** inside a property text field also undoes the last action on the view's stack.
- Fix: return early from the file/undo shortcuts while `WantTextInput` is set (the Text Editor got the same fix, see *Text Editor Review* B1). Save can stay global.

**B2. Collision Generator: *Generate* throws `InvalidCastException`.**
`HavokPropertyView.GetSourceObject()` returns the `HavokFileWrapper` (`data.XBank[binder][file]`), not its `Root`. `CollisionGeneratorTool.BuildAndReplaceShape` casts that object to `hkRootLevelContainer`.
- Repro: Map Collision or Asset Collision → select a binder and a file → Tools → Collision Generator → Square → Generate. The cast throws. *(verify: where the exception surfaces, either as a crash or a caught log entry)*
- The `sourceObject == null` guard in `DisplayGeneratorPanel` never trips either, because the wrapper exists even when the file isn't loaded.
- Even with the cast fixed, the replacement doesn't go through the `ActionManager`, so there's no undo and `IsDirty` isn't set. `SaveBinder` then skips the file as unmodified unless *Allow Unmodified Save* is on, so the generated shape is silently never saved.

**B3. Save serialises the object graph on a worker thread while the UI keeps editing it.**
`HavokEditorScreen.RunSaveTask` runs `SaveHavokFile`/`SaveAllHavokFiles` inside `Task.Run`. The property view keeps mutating the same `Root` objects on the UI thread, and the Save keybind and autosave can both start a save mid-edit.
- Consequence 1: Adding or removing a list entry, or dragging a value, during a save can throw `Collection was modified` or write a half-edited file. *(verify)*
- Consequence 2: `SaveBinder` sets `IsDirty = false` *after* writing. An edit made while the write was running is cleared too. The `*` disappears, the next Save logs "skip", and that edit never reaches disk unless the file is edited again.
- Consequence 3: File actions (Delete/Rename/Insert/Compress) read-modify-write the same container on the UI thread. If one overlaps a background save of that binder, the last writer wins, and the file action or the edits are reverted.
- Consequence 4: Smithbox has no `SynchronizationContext` (I grepped and found none), so the code after `await` in the `async void` `Save`/`SaveAll` runs on a thread-pool thread. `OnSaveComplete` therefore clears `MapEditor...HavokCollisionBank` and calls `SaveConfiguration()` off the UI thread, racing the Map Editor's renderer.
- Fix: save synchronously on the UI thread, as the Text Editor now does (*Text Editor Review* B3). Or snapshot the bytes on the UI thread and only write them to disk in the background.

**B4. Undo/redo after a binder refresh edits a discarded object graph.**
`HavokBinderView` calls `PopulateFileList(true)` on every binder click, as do Delete, Rename, Insert, Export, Compress and Change Compression. With `clearCache`, `PopulateFiles` removes every non-dirty binder and replaces every non-dirty wrapper with a new, unloaded one. The view's `ActionManager` still holds `HavokChangeField`/`HavokAddListEntry` actions that point at the old objects and the old wrapper.
- Repro: edit a field, Save (the `*` clears), click the same binder again (or any other one and back), reselect the file, then press Ctrl+Z. Nothing visible changes and no `*` appears: the undo changed the orphaned copy. Redo behaves the same way.
- Fix: clear the undo stack when a binder is evicted. Alternatively, don't evict loaded wrappers on reselection, and only drop the `Root` on an explicit reload.

**B5. Adding an entry to a list of abstract types inserts `null`.**
`PropEditorGenericList` → `PropFinderUtil.CreateDefaultListElement` returns `null` for abstract or interface element types. The `hkbGenerator` special case only applies when the list already has an element. For an empty `List<hkbGenerator>`, or any other abstract element type (`hknpShape`, `hkbModifier`, ...), *Add* inserts `null`, which the tag/packfile writer or the game may not accept. *(verify on save)*
- Repro: Behavior → a state machine with an empty generator list (or a new `hkbModifierList`) → expand the list → Add → Save.
- Fix: when the element type is abstract, offer a type picker, or disable *Add* with a tooltip.

**B6. The clip generator alias cache can throw on a duplicate or null name.**
`HavokAnimationClipView.DisplayTabContents` calls `_aliasCache.Add(entryName, alias)` for every clip during a rebuild. Two clip generators with the same `m_name` throw `ArgumentException`, and a clip with a null `m_name` (an entry created with *Add*) throws `ArgumentNullException`. *(verify: whether shipped behbnds have duplicate clip names; null names happen whenever a clip is added in the editor)*
- Fix: use `_aliasCache.TryAdd(entryName ?? "", ...)`.

### 1.2 Wrong behaviour (Medium)

> **Status (2026-11-05, second pass): M3, M4, M6, M7, M8, M9, M10 and L9 fixed. Not yet verified in the app.** `Smithbox.Program` builds. The new loc keys exist in English only. **M5, the rest of M2 and U2 decided and fixed (below). M11 moved to a separate app-wide task.**
> - **M2 rest + U2 (decided: save all changed):** the Save keybind and the project autosave call `SaveAll`, which writes only binders with `*` changes. The File menu shows the shortcut on *Save All*. *Save* still saves the selected binder, including the *Allow Unmodified Save* behaviour. `SaveAllHavokFiles` now returns whether any binder was written, so an autosave tick with nothing to save doesn't clear the Map Editor collision cache or rewrite the configuration.
> - **M11 (decided: app-wide prompt, separate task):** on exit, Smithbox only saves project descriptors and configuration (`ProjectOrchestrator.Exit`, `Smithbox.Exit`). No editor saves or prompts, and the main loop can't cancel a window close. An "unsaved changes" prompt on close and on project switch would cover every editor, so it's tracked as its own task rather than as a Havok Editor change. No change here.
> - **M3:** `HavokEditorScreen.HavokBank` is the one bank, and `HavokEditorView.HavokBank` forwards to it. Every view loads, edits, marks and saves the same objects, and `SaveAll` covers every view's edits. The bank no longer writes file aliases into the active view. `HavokBinderView.RebuildFileAliasCache()` rebuilds each view's own aliases after a populate.
> - **M4:** the bank's `GetPinnedWrappers` (built by the screen) now covers every view's undo/redo history *and* every view's selected file, so a refresh never replaces the file a panel is showing. File actions go through `HavokFileView.RefreshAfterFileAction`. For every view showing that binder, it first maps the selection through the renames the bank recorded (`FileAction.Renames`, filled in by `ModifyContainer`), then refreshes, then prunes selections of files that no longer exist (re-loading the selected file if needed). Rename and Compress keep the selection under the new name instead of clearing it. Change Compression keeps it too. Export no longer refreshes.
> - **L9 (with M4):** Ctrl+click in the file list toggles instead of adding duplicates.
> - **M6:** Paste and Insert use `HavokBinderUtils.GetNextFileId` (highest ID + 1, or 0 for an empty binder).
> - **M7:** Insert gives each file a unique name with `GetUniqueFileName`, like Paste, and refuses an empty binder with a log entry, since it copies an existing file's folder and flags. Rename trims the name, then rejects an empty name, path separators, invalid characters, or a name another file already uses (case-insensitive). Each rejection is logged.
> - **M8:** `IHavokFileAction.ChangesStructure` is true for list add/remove, the collision shape replacement, and field changes whose value is an object reference. After such an action or its undo/redo, every view re-runs its behavior tree search on the next frame. (The stacks are checked a frame later because `ActionPerformed` fires before the action is pushed.) The tabs drop selections that are no longer in the graph. `SetupBehaviorView` resets `IsBehaviorGraph` first. Editing a community name rebuilds the clip alias column in every view. Plain value edits don't trigger a rebuild, so dragging a value doesn't walk the graph every frame.
> - **M9:** `HavokPropEditGeneric` uses the meta of the object's own type for its fields and its community name row. It's looked up once per object instead of once per field.
> - **M5 (decided: use unsaved edits):** Paste and Export use `HavokBank.GetCurrentFileBytes`. A loaded file with unsaved changes is re-serialized with the same writer as Save (loading the binder compendium when the files refer to it), then re-compressed with its stored DCX type. Each use is logged (`HAVOK_FileView_ContextAction_Used_Unsaved_Changes_Log`). Clean files still copy the stored bytes. The pasted copy is written to disk at once, like every other file action; the source file keeps its `*` until it's saved.
> - **M10:** `Quaternion` (raw XYZW), `Matrix4x4` (4 rows), HKLib `Matrix3x3` (3 rows) and `double` now have editors. The enum combo works for every underlying type; the numbers in the labels are truncated to 32 bits for 64-bit enums.

**M1. *Set to Free ID* always sets 0.**
`HavokPropertyDecorators.ClipGenInternalID` searches `GetSourceObject()`, which is the wrapper (see B2), using `PropertyCache.GetCachedHavokFields`, which only returns public fields. `HavokFileWrapper` has no public fields (its members are auto-properties), so the search finds no clip generators. The "used IDs" list is empty, so the first free ID is always 0. This makes duplicate `m_animationInternalId` values that collide with the real clip 0.
- Repro: Behavior → Clip Generators tab → select any clip → *Set to Free ID* next to `m_animationInternalId`. The value becomes 0.
- Fix: make `GetSourceObject()` return `.Root` (this fixes B2 and M1 together). It's also called twice per field row per frame, so it could be cached for the frame.

**M2. Autosave logs an error every tick, and runs save side-effects even when Havok autosave is off.**
`ProjectEntry` calls `HavokEditor.Save(true)` whenever the Havok Editor is focused. In `Save()`, the "no binder selected" and "no file selected" errors are logged *before* `HavokEditor_Allow_Automatic_Save` is checked. `OnSaveComplete()` runs whether or not anything was saved.
- Repro: open the Havok Editor without selecting a binder, enable project autosave and wait one interval. A red error appears in the log each tick. With a file selected but Havok autosave off, each tick still clears the Map Editor collision cache and rewrites the configuration JSON.
- Also: `Save()` saves only the *selected* binder, so autosave never saves edits in other binders (see U2).

**M3. Every editor view has its own `HavokBank`.**
`HavokEditorView` builds `HavokBank = new(project)` for each view. If the same binder is open in two views, each view has its own copy. Saving view 1 writes its copy, and saving view 2 later overwrites it. `SaveAll` only saves the active view's bank. The `*` markers in the other view stay stale. `HavokBank.PopulateFiles` also writes file aliases into `HavokEditor.ViewHandler.ActiveView.Selection`, not the view that owns the bank.
- Repro: View → Add New View. Open the same binder and file in both views, edit a different field in each, then Save in one view and Save in the other. Only the second view's edit survives on disk.
- Fix: put one shared bank on `HavokEditorScreen` (or `Project.Handler`), as `MapData` does for the Map Editor.

**M4. After Delete, Rename, Insert, Export or a compression change, the property panel goes blank, and the selection may point at a file that no longer exists.**
These actions call `PopulateFileList(true)`, which replaces the selected non-dirty wrapper with an unloaded one (see B4). `Selection.FilePath` isn't updated.
- Export: the panel shows "file not loaded" until you click the file again, even though Export doesn't change the binder. Export doesn't need to refresh at all.
- Rename: `Selection.FilePath` and `SoftSelectEntries` keep the old name. The panel shows "binder missing file", and the rename input still holds the old name.
- Delete: `SoftSelectEntries` keeps the deleted names. The next Ctrl+click builds on that stale list, and `DeleteFiles`/`ExportFiles` then return silently because `MultipleFilePaths.Any(e => !files.ContainsKey(e))`.
- Fix: after a file action, prune the soft selection to names that still exist, map the primary selection through the rename, and reload the selected file.

**M5. Copy/Paste and Export use the on-disk bytes, not unsaved edits.**
`PasteFiles` and `ExportFiles` read the container from disk. Copying or exporting a file marked `*` produces the last saved version. *(Intent: this may be by design, but nothing tells the user.)*
- Fix: either serialise dirty wrappers for paste/export, or warn ("*X* has unsaved changes; the saved version will be used").

**M6. Paste and Insert compute new binder IDs from the last file, not the highest ID.**
`PasteFiles` uses `targetBinder.Files.Last().ID + 1` and `GetInsertFile` uses `lastFile.ID + 1`. If the binder's files aren't sorted by ID, the new ID can collide with an existing one. In an empty binder, `Last()` throws, which is caught and logged as a write failure.
- Fix: use `Files.Count == 0 ? 0 : Files.Max(f => f.ID) + 1`.

**M7. Insert and Rename don't check for name collisions.**
Paste deduplicates with `GetUniqueFileName`, but Insert adds the file under its own name even if that name already exists in the binder. Rename accepts an empty name or the name of another file. Both produce duplicate binder entries, and `PopulateFiles` silently skips duplicates (`if (files.ContainsKey(file.Name)) continue;`), so the second copy is hidden in the editor.
- Repro: Insert the same `.hkx` twice, or rename `a000_000000` to `a000_000001` when that name already exists.

**M8. Behavior tabs don't refresh after structural edits.**
The State Machines, Selectors and Clip Generators lists are built once per file (`HavokSelection.ApplyFileSpecificTreeSearches`, guarded by `AppliedHavokTreeSearch`). Adding or removing generators, undoing, or pasting a clip doesn't update them, so a removed clip is still listed and editable, and an added one isn't listed. The alias column (`RebuildAliasCache`) isn't rebuilt after a community name is edited either.
- Fix: rebuild the tree search on `ActionManager` execute/undo, or keep a data version counter like the Text Editor's `TextFinder.CurrentDataVersion`.

**M9. The community name row uses the parent's class meta for nested objects.**
`HavokPropEditGeneric(o, classMeta)` passes the *parent's* meta down for nested classes, list elements and array elements. `DisplayCommunityNameInput` runs before `classMeta` is reassigned for the child's type. So a nested object's community name row only appears when the child happens to have a field with the parent's `NameField` name. The child's own `NameField` is never used. The behavior tab property views pass the right meta, so it works there.

**M10. Quaternion, Matrix4x4, double and 16/64-bit enum fields are read-only.**
`PropertyRow` has no branch for `Quaternion`, `Matrix4x4` or `double`, and enum editing only covers `byte`/`sbyte`/`int`/`uint` underlying types. These show `ImplementMe`. HKLib uses `Quaternion`/`Matrix4x4`/`double` in about 116 classes, including `hkQsTransformf`, `hknpBody`, `hknpMotion`, `hkbKeyframeBonesModifier` and `hkaBoneAttachment`, so transforms can't be edited.

**M11. Unsaved Havok edits aren't protected on exit or project switch.** *(verify)*
Nothing outside the editor checks `IsBinderDirty` (the only matches are in this folder). I couldn't find an unsaved-changes prompt that covers the Havok Editor, so closing Smithbox drops pending edits silently.

### 1.3 Edge cases (Low)

> **Status (2026-11-05, third pass): every Low item fixed except L13 (needs an in-game check). Plus one new bug, N1. Not yet verified in the app.** `Smithbox.Program` builds. The new loc keys exist in English only.
> - **L1:** the view tab popup closes with `EndPopup`. **L2:** fixed in the first pass. **L3:** `EndTabBar` is only called when `BeginTabBar` returned true.
> - **L4:** `ParamRefRow`, `ClipGenFlags` and `ClipGenInternalID` always advance the type column before returning a result, so the rows stay aligned on the frame of a change or while the context menu is open. `ParamRefRow` is shared with the Map Editor's Havok property view, which gets the same fix.
> - **L5:** the unused `AddVariableBindingSet` was removed.
> - **L6:** `GetBdtPath` only replaces a trailing `bhd`.
> - **L7 + U12:** Export asks for a folder each time, starting from the last one used (or the project folder). It keeps the file's own name and extensions, and logs when it replaces an existing file.
> - **L8:** the file list matches `.hkx`/`.hkx.dcx` case-insensitively. Under the list, a dim line with a tooltip counts the files that aren't shown (such as the compendium).
> - **L9:** fixed in the second pass.
> - **L10:** the *Copy Name* tooltip no longer shows the Copy keybind.
> - **L11:** file aliases are looked up by the bare name, so they show with *Full Path* on and in Behavior mode. The file search also matches aliases.
> - **L12:** the stub checks `commands.Length` (the other editors' stubs share the pattern and were left alone).
> - **L14:** the fallback DCX type for an unreadable entry is the game's default (`GetGameDefaultDCXType`, shared with `DetectBinderDCXType`).
> - **L15:** changing an inclusion, *Toggle All* or *Clear All* re-syncs the watchers. `HavokData.Dispose` disposes every view's reloader, and closing a view disposes its own. Two views watching the same script still both reload it.
> - **L16:** the remote thread handle is closed. The buffers are released with `MEM_RELEASE` and size 0, but only if the thread finished within the timeout; freeing them under a running thread would crash the game. The 1.07+ code takes its offsets from `EldenRing_WorldChrManStructOffset` (offset, offset, +8, +0x10), the same values as before.
> - **L17:** the watcher failure uses the `LogError(..., ex)` overload.
> - **L13 (still open, verify):** ER/NR/DB animation and map collision are read with the compendium but written without it. Changing this alters the output format, so it needs an in-game test before anything changes.
> - **N1 (new, found during L16; Medium, verify):** `HavokMemoryConsts.GetHexFromString("0x1E668")` failed to parse because of the `0x` prefix, but returned **0** instead of null. So the ER reload read its data pointer from `WorldChrMan + 0` instead of `+ 0x1E668`, and the `?? 0x1E668` fallback never applied. It now strips `0x` and returns null when parsing fails. To check: the ER script reload still works, and preferably works more reliably.

- **L1.** The view tab context menu is opened with `BeginPopupContextItem` but closed with `ImGui.EndMenu()` (`HavokViewHandler.HandleViews`). This should be `EndPopup()` (same as *Text Editor Review* M16).
- **L2.** `OnSaveComplete` dereferences `MapEditor.ViewHandler.ActiveView` without a null check.
- **L3.** `HavokBehaviorView.DisplayBehaviorTabs` ignores the return value of `BeginTabBar` and always calls `EndTabBar`.
- **L4.** `ClipGenFlags` and `ParamRefRow` return early (on a change, or with the context popup open) before advancing the type column, so the rows below are misaligned for that frame.
- **L5.** `AddVariableBindingSet` is never called, and its *Remove* button replaces the binding set with a new empty one (its tooltip says "Adds"). Remove it or fix it.
- **L6.** `GetBdtPath` uses `Replace("bhd", "bdt")` on the whole path, so a folder name containing `bhd` is rewritten too. Only the extension should change.
- **L7.** `ExportFiles` always writes to the project root, overwrites silently, and forces a `.hkx` extension on non-`.dcx` files.
- **L8.** The file list only shows names ending in lowercase `.hkx`/`.hkx.dcx` (`EndsWith` is case-sensitive), and `.compendium` files are hidden with no hint that they exist.
- **L9.** Ctrl+click in the file list and in the behavior tabs only *adds* to the selection: a second Ctrl+click adds a duplicate instead of deselecting. Shift-range selection isn't supported.
- **L10.** The *Copy Name* tooltip shows the Copy keybind, but that key copies *files*, not the name.
- **L11.** File aliases are keyed by the bare file name, but the lookup uses `displayName`. With *Full Path* on, or in Behavior mode (which prefixes "Behavior Data: "), aliases never show.
- **L12.** `HavokEditorStub.Display` indexes `commands[0]` without checking `Length > 0`.
- **L13.** HKX3 2018 (ER/NR/DB) loads Animation and Map Collision with the binder compendium (`loadCompendium: true`), but `SaveBinder` only loads it when `UsesBinderCompendium` is set (AC6 only). The saved ER file is therefore written without a compendium reference, unlike the original. *(verify that ER accepts it; at minimum the file is larger)*
- **L14.** `GetExistingDCXType` falls back to `DCX_KRAK` for every game, while `DetectBinderDCXType` falls back per game. An unreadable DS3 entry would be recompressed as KRAK.
- **L15.** Script Reloader: ticking a script, *Toggle All* or *Clear All* while *Detect File Changes* is on doesn't set `WatchersDirty`, so no watcher is created until the checkbox is toggled again. Each view has its own reloader and watchers, so with two views a change reloads twice. `HavokData.Dispose` only disposes the active view's watchers.
- **L16.** Script Reloader: `VirtualFreeEx(..., 256, 2)` uses an invalid free type. `MEM_RELEASE` is `0x8000` with size 0. Each reload therefore leaks 512 bytes of RWX memory in the game process, and the remote thread handle is never closed. The 1.07+ shellcode hard-codes `0x1E668` instead of using `EldenRing_WorldChrManStructOffset`.
- **L17.** `LOC.Get("HAVOK_ScriptReloader_Error_Failed_Create_Watcher", ex)` passes the exception as a format argument instead of using the `LogError(..., ex)` overload.

---

## 2. Usability improvements

Ordered roughly by value.

> **Status (2026-11-05, third pass): U2, U9 and U12 were done in earlier passes. U5, U6, U7, U8, U10 and U11 are done now. U1, U3 and U4 decided and done (below). Every usability item is now done.**
> - **U4 (decided: auto-open matches):** while a property filter is active, `HavokPropEditGeneric` and `PropEditorGenericList` hide fields, array/list entries and nodes with no matching field anywhere below them, and open the nodes that have one. The matching uses the same rules as the drawn rows (`DisplayProperty`: field name, or `val:`, with the raw-data and property-bag toggles). `SubtreeHasMatch` caches its result per object. The cache is cleared when the filter, its toggles or the shown file change, after a structural edit in any view, and, for a `val:` filter, after any edit. Known limits: a reference back up the graph (a cycle) counts as "no match" for the object inside the cycle; matching nodes stay open while the filter is active; and a `val:` search still compares lists of values by their `ToString()` (the existing TODO).
> - **U3 (decided: jump to the file):** Undo and Redo, from the keybinds and the Edit menu, go through `HavokEditorView.Undo/Redo`. When the action applies to a file or binder other than the one shown, those first select its category, binder and file (`HavokBank.FindWrapper` / `FindBinder`) and log which file was switched to (`HAVOK_Undo_Showing_File` / `_Binder`), then run the undo or redo. The single stack per view is unchanged. *Undo All* doesn't jump. The property view's internal undo while dragging a value doesn't go through this.
> - **U1 (decided: stage until Save, with undo):** Paste, Delete, Rename, Insert, Compress and Change Compression are now `HavokBinderAction`s. They change an in-memory working copy of the binder, can be undone and redone, and are written on Save like field edits. The binder shows `*` until then, Save All and the unsaved-changes prompt include it, and staged files can be opened, edited, copied and exported before they're saved. Every view's selection follows renames and drops removed files across undo/redo. Export still writes immediately. The second pass's rename map and refresh step (`FileAction.Renames`, `RefreshAfterFileAction`) were replaced by this. The design and its limits are in `Documentation/Claude/Havok File Actions Staging.md`. Main limit: while a binder has a working copy, an outside change to that binder file on disk isn't picked up, and the next Havok save overwrites it. Not yet verified in the app. `Smithbox.Program` builds.
> - **U5:** right-clicking a file that isn't selected selects it and opens its menu. Right-clicking empty space in the file list offers *Paste*. The Paste shortcut only needs a selected binder now; Copy and Delete still need a selected file.
> - **U6:** *None* is no longer listed as a category, and the empty *Tools* menu was removed from the menu bar. The Variables tab was left alone; it's still commented out.
> - **U7:** the property panel shows `binder / file` under its title, with `*` for unsaved changes. The tooltip gives the full internal path.
> - **U8:** the binder search matches aliases (map, character, part and asset names).
> - **U10:** the Collision Generator inputs are floats and are stored as they're typed, so *Generate* right after typing uses the new value. Sizes are kept ≥ 0.01, circle segments ≥ 3 and semicircle segments ≥ 2. A 0 size used to throw inside the generator.
> - **U11:** with more than one file selected, the file list header shows "N selected", with a tooltip saying what that count applies to.

- **U1. Make file actions undoable, or at least confirm them.** Delete, Rename, Insert, Paste and the compression actions write the binder to disk immediately, outside the undo stack, while field edits wait for Save. Options: (a) stage file actions in the bank and write them on Save, with undo (consistent with field edits); (b) keep writing immediately but add a confirmation for Delete and a log line saying the binder was written. (a) is the bigger change but removes the two save models.
- **U2. Make Ctrl+S save all changed binders.** Save only saves the selected binder, and so does autosave. The Text Editor moved to "Save keybind = Save All (only `*` containers)" (*Text Editor Review* U2), and the same fits here, since the binder list already marks dirty binders with `*`.
- **U3. Use one undo stack per file, or show which file an undo will touch.** The view's single stack covers every file you've edited, so Ctrl+Z in file B can silently undo an edit in file A, which isn't on screen. At minimum, select the affected file when undoing.
- **U4. Make property search reach into collapsed nodes.** The filter only tests leaf rows that are already drawn. Tree, list and array nodes are always shown, and their children are only checked once they're open. Searching for `m_animationInternalId` in a big behavior graph finds nothing until you expand the right nodes. Option: when a filter is active, auto-open nodes whose subtree has a match and hide the rest. The `val:` mode should also handle array and list elements (there's already a TODO for this).
- **U5. Allow right-clicking an unselected file.** The file context menu only exists on selected rows, so right-clicking another row does nothing. Paste needs a selected file too (the shortcut returns if `FilePath == null`), so you can't paste into a binder without first selecting a file in it.
- **U6. Clean up empty and placeholder UI.** The category list includes **None**, and selecting it just clears everything. The menu bar has an empty **Tools** menu (`HavokToolView.DisplayMenu`). The Variables behavior tab is commented out but `HavokVariableView` is still constructed. Hide or remove these.
- **U7. Show the file and binder in the property header.** When several views or files are open, the property panel doesn't say which file it's editing or whether that file has unsaved changes.
- **U8. Let the binder search match aliases.** The binder list draws aliases (map names, character names) but `EditorFilters.IsMatch` only receives the file name. Pass the alias, as the clip generator list already does.
- **U9. Add an editor for Quaternion and Matrix fields** (see M10). For example, show a quaternion as Euler angles with a raw-xyzw toggle, and a matrix as a 4×4 grid or TRS.
- **U10. Collision Generator.** The inputs only commit on `IsItemDeactivatedAfterEdit`, so clicking *Generate* right after typing uses the old value if focus moves straight to the button *(verify)*. Integer-only sizes rule out sub-metre shapes. Also, there's no preview and no undo (see B2).
- **U11. Show a log or tooltip when Ctrl+click selects.** Multi-select is silent and only matters for the context actions. A count in the File List header ("3 selected") would make it clear what Delete or Export will act on.
- **U12. Let Export choose a destination folder** (see L7), defaulting to the last-used folder.

---

## 3. Suggested order

1. **B1** (shortcuts while typing). This is the most likely way to lose data. Small fix.
2. **B2 + M1.** `GetSourceObject()` → `.Root`. A one-line fix that unblocks the Collision Generator and *Set to Free ID*. Then route the generator through an action that sets `IsDirty`.
3. **B3 + M2.** Synchronous save on the UI thread. Check the autosave flag before logging. Only run `OnSaveComplete` when something was written.
4. **B4 + M4.** Stop evicting loaded wrappers on reselection, and fix up selection after file actions.
5. **M3.** One shared bank per editor.
6. **U1 / U2.** Decide on the save model for file actions, and on what Ctrl+S saves.
7. The rest by convenience.

## 4. What to check in the app (for any fixes)

- B1: type in the File List search bar and press Delete or Ctrl+V. No file is removed or pasted.
- B2/M1: ER Map Collision → `h10_00_00_00` → any file → Collision Generator → Square → Generate. The file gets a `*`, and Save writes it. In ER `c0000` behavior → Clip Generators → *Set to Free ID* gives a value not used by any other clip.
- B3: drag a float in the property view while autosave fires (set a short interval). The `*` must survive if the value changed after the save started.
- B4: edit, Save, click the binder again, Ctrl+Z. Either the undo stack is empty or the undo visibly applies.
