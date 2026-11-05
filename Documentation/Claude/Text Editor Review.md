# Text Editor Review: Bugs and Usability

Scope: `src/Smithbox.Program/Editors/Text Editor/**`, plus `Andre/SoulsFormats/.../FMG.cs` and `Keybinds/InputManager.cs`, which the editor depends on.
Method: I read the code; I didn't run anything. Each bug gives steps to reproduce it so you can confirm it in the app. Items marked *(verify)* rest on ImGui or data behaviour I couldn't fully confirm from the code alone.

Severity key: **High** = corrupts or loses data, or crashes; **Medium** = wrong behaviour users will run into; **Low** = edge case or latent issue.

Paths below are relative to `src/Smithbox.Program/Editors/Text Editor/` unless stated otherwise.

---

## 1. Bugs

### 1.1 Data integrity and crashes (High)

> **Status (2026-11-05): B1, B2, B4, B5 and B7 fixed, not yet verified in the app.** `Smithbox.Program` builds.
> - **B1:** `TextShortcuts.Monitor()` returns before the undo/redo and entry-list shortcuts while `ImGui.GetIO().WantTextInput` is set. Save and the tool-window toggle still work everywhere. Hover-based focus (U1) is unchanged.
> - **B2:** `AddFmgEntry`, `DuplicateFmgEntry` and `AddAssociatedEntry` work out their sorted insertion index when they execute (`FmgEntryUtils.GetSortedInsertionIndex`) and undo with `Remove(entry)` by reference. `DeleteFmgEntry` finds its entry with `IndexOf` when it executes. A save re-sorting the list no longer changes which entry an undo touches. `FMG.Write` still sorts in place, so drag-reorder is still reverted on save (U6), and B3 (save on a worker thread) is still open.
> - **B4:** `DeleteFmgEntry` re-inserts the original entry object instead of a clone, and duplicate IDs can't make it remove the wrong entry.
> - **B5:** falls out of B2. The new associated entry goes after the last lower ID, or at the end of the list.
> - **B7:** `SortFmgList` snapshots the previous order and restores it on undo, and its sort is now stable for duplicate IDs. Language sync no longer calls `Entries.First()`, so an empty target FMG doesn't throw. `GetVanillaSrcWrapper` loads the source language's vanilla FMGs on demand. When the vanilla container doesn't exist at all, the log still reports success (no new loc key was added).
>
> **Status (2026-11-05, second pass): B3, B6, B8 and most M/L items fixed, not yet verified in the app.** `Smithbox.Program` builds. New loc keys exist in English only.
> - **B3:** `TextBank.SaveTextFiles/SaveFmgContainer/SaveLooseFmg` and `TextEditorScreen.Save/SaveAll` are now synchronous, so they run on the UI thread (autosave already calls them from the frame loop). `RefreshFmgTextCache` is null-safe.
> - **B6:** one `Regex` (pattern + options, no `ExplicitCapture`) is built for both preview and apply, and *Ignore case* is a regex option. The replacement string is no longer validated as a pattern. Null-text entries are skipped. Hidden unused containers are excluded from both preview and apply. The preview lists only entries whose text would change, shows a count, and has a before/after tooltip. Apply recomputes from the current text, then clears the preview, so a second click can't re-apply.
> - **B8:** the import scope follows the menu: Container = the whole container; FMG = the selected FMG; Entries = the selected FMG plus its associated group FMGs, since entry exports can include them. Stored FMGs must match by binder ID *and* name. Duplicate IDs no longer throw (first wins). Imported entries are fresh objects. Diff colours are refreshed, and the result is logged with counts.
> - **M1:** the shortcut sets `TextDuplicatePopup.OpenRequested`, and the popup opens itself inside the view window. Submit no longer depends on hover focus, closes the popup, and rejects a non-numeric offset. When *Allow Duplicate ID* is off, colliding duplicates are skipped and the skip is logged.
> - **M2:** the grouped editor no longer returns early when there's no Title. The ID row falls back to the selected entry, and *Add Title Entry* is reachable.
> - **M3:** `SelectFileContainer` always auto-selects the first FMG and refreshes decorators, including on a container's first load. `TrackFmgDifferences` returns early when no FMG is selected.
> - **M4:** Shift-click with no anchor (`-1`) acts as a normal click.
> - **M5:** Ctrl+click deselect moves the primary selection to a still-selected row. After Delete, the row below the deleted ones is selected.
> - **M6:** `TextMultiselection.Revalidate` re-keys stored entries by their current index (dropping removed ones) every frame and before Delete, Duplicate and Copy.
> - **M7:** a shared `TextEntryList.IsEntryVisible` (filter, match commands and the null-entry toggle) is used by the list, Select All, Shift-range, auto-select and Copy.
> - **M9:** duplicate IDs are checked across the whole group's FMGs (`GetGroupFmgs`/`IsIdFree`) for Ctrl+D, the popup and ID edits (basic mode now checks too). A refused ID edit logs a warning naming the FMG.
> - **M10:** the text input buffer is `max(2000, bytes + 4096)`.
> - **M11:** *Copy Path* copies the path. The DS2 *Talk* header has its own ID. Containers are selected and highlighted by `FileDictionaryEntry`, and row IDs use the file path.
> - **M12:** FMG rows are selected and given a context menu by wrapper reference, so DS2's `ID = -1` works.
> - **M13:** jumps try `_dlc02`, `_dlc2`, `_dlc01`, `_dlc1`, then the base name, in that order.
> - **M14:** `TextFinder` cache: `InvalidateCache()` is bumped by `MarkDirty` (every action and undo) and by FMG loads. Keys include the project and primary language, and misses are cached. Lookups search used containers before unused ones.
> - **M15:** the vanilla container is matched by filename + language (+ DS2 sub-category).
> - **M16:** the view tab popup is closed with `EndPopup`. Separate undo stacks per view remain, but undo is now identity-based, which removes the corruption mode.
> - **M17:** Global Search loads unopened languages (synchronously, so a first "All categories" search can hitch).
> - **L1:** the descriptor log keys are no longer swapped, and the aux bank failure uses its own key. **L2:** Copy Text runs on the UI thread. **L3:** selection uses its own `Project`. **L5:** the creator clears fields the new entry lacks and null-checks the group. **L7:** the per-frame async preload was removed; `SyncLanguage` already loads synchronously.
>
> - **M8 (decided: follow the toggle):** `ProcessDelete`, `ProcessDuplicate` and `GetGroupFmgs` cascade to the associated entries only when *Grouped Entries* is on. With it off, Delete, Duplicate and the ID-collision check touch only the selected entry's FMG.
>
>
> **Status (2026-11-05, third pass): L4, L6, the aux bank and U8/U9/U11 done, not yet verified in the app.** `Smithbox.Program` builds.
> - **L4:** `FmgGroupedAction` opens a `TextDifferenceManager` batch, so a grouped action rebuilds the diff cache once instead of once per sub-action. Language sync now uses `FmgGroupedAction` too. `GetGroupings()` is cached per view. Bulk Delete/Duplicate share one `FmgEntryLookup` (ID index per FMG) for group resolution and ID-free checks. The Contents panel caches its entry group until the entry, the FMG or the text data changes (`TextFinder.CurrentDataVersion`). Language sync uses an ID map per target FMG.
> - **Found while doing L4:** `EntryGroupAssociation` turned a missing slot into binder ID `0` (`Convert.ToInt32(null)`), so a group without an Effect FMG treated an FMG with binder ID 0 as its Effect. IDs are now computed once, with `int.MinValue` for an empty slot, and `SetGroupEntry` skips empty slots.
> - **L6:** `HandleFmgView` returns whether it drew the row; the DLC priority tooltips are only attached to drawn rows.
> - **Aux bank:** it enumerates the auxiliary project's `Locator.TextFiles`, falling back to the main project's list if that one is empty.
> - **U8:** the entry-list search bar tooltip explains the `modified`/`unique` filters and the row colours.
> - **U9:** each text box shows a hover tooltip with character and UTF-8 byte counts, and the Title box grows for multi-line titles.
> - **U11:** language sync logs the number of entries added and changed, and warns per FMG when no vanilla source exists (instead of reporting success).
>
> - **U6 (decided: remove):** drag-to-reorder is removed from the entry list. `ReorderEntryAction.cs` was moved to `.archive/src/.../Text Editor/Actions/`, and `FmgEntryUtils.ReorderFmgEntryGroup` was removed. `FMG.Write` still sorts in place; with identity-based undo that no longer affects which entries undo touches.
>
> - **U2 (decided: save all changed):** the Save keybind runs `SaveAll()`, which writes only containers marked `*`. The File menu shows the shortcut on *Save All*. The project autosave calls `SaveAll(true)` as well.
>
> - **U1 (decided: last clicked panel):** the five panel windows set the focus context with `IsWindowFocused(ChildWindows)` instead of `IsWindowHovered(ChildWindows)`. *(verify)* The entry-row context menu should still count as the entry list, because ImGui includes popups in the opener's focus hierarchy by default.
>
> - **U5 (decided: confirm 2+):** `DeleteEntries` asks first when two or more entries are selected, showing the selected count and how many associated entries the delete will also remove. The new `TextDeleteConfirmPopup` is opened from the view window. Escape or Cancel aborts; single deletes are unchanged. `ExecuteDelete()` does the actual delete.
>
> - **U4 (decided: jump to the changed entry):** the text actions implement `ITextActionTarget` (container, FMG, entry). `TextEditorView.UndoAndReveal/RedoAndReveal`, used by the keybinds and the Edit menu, select the first target's container, FMG and entry after an undo or redo, and scroll to them. Nothing moves when the entry is already selected or part of the multi-selection, or when the undo was refused. *Undo All* doesn't reveal.
> - **U10:** exports record `SourceContainer`/`SourceCategory` (optional JSON fields; older exports still load). The import menu shows the source as a tooltip, and importing into a different container logs a warning. The import isn't refused; the ID + name match decides what is imported.
> - **U12:** in the entry list, Page Up/Down and Home/End select the row ImGui's keyboard navigation moves to, the same way the arrow keys already did.
>
>
> **Status (2026-11-05, fourth pass): remaining parts of U3, U7 and U9 done, not yet verified in the app.** `Smithbox.Program` builds and the edited loc JSON files parse.
> - **U7:** Global Replace preview results each have a checkbox (on by default), with *Select All*/*Select None* and an "N of M selected" count kept above the list. Apply skips unticked results. The list uses `ImGuiListClipper`.
> - **U3:** a refused ID change is shown under the ID field, in the warning colour, until another entry is selected; it is also still logged.
> - **U9 (decided: preview under each box):** the new `TextMarkupPreview` draws a read-only, word-wrapped copy of each text box's live text. Tags (`<...>`, including `<?...?>`) are dimmed, and text inside `<font color="#RRGGBB">...</font>` is drawn in that colour (nesting supported). Text is drawn with `TextUnformatted`, so `%` is safe. An eye button at the top of the Contents panel toggles previews (`CFG.TextEditor_Contents_Show_Text_Preview`, on by default). The text inputs themselves can't wrap in the ImGui version used (Hexa.NET.ImGui 2.2.7 has no wrapping input flag).
> - **Conventions pass:** the delete confirmation modal now sets its width (`Tool UI Styling.md`, Modals) and uses `GUI.WrappedText`.
> - **Pre-existing, not changed:** `Text-Editor-Tools-Language-Sync.json` defines `TEXT_LanguageSync_Action_Sync_TT` twice, with two different texts, at HEAD.
>
> **All review items are now addressed.** Earlier "still open" notes below are superseded.
>
> **Previously open:** U4 (undo visibility), U5 (confirm bulk deletes), U10 (stored source name in the import list) and U12 (keyboard navigation). These change behaviour and are waiting on decisions.

#### B1. Editor shortcuts fire while you type in a text box
`TextShortcuts.Monitor()` ([Framework/TextShortcuts.cs:21-99](../../src/Smithbox.Program/Editors/Text%20Editor/Framework/TextShortcuts.cs)) never checks `ImGui.IsAnyItemActive()` or `WantTextInput`. The Map, Model and TAE editors do ([MapShortcuts.cs:33](../../src/Smithbox.Program/Editors/Map%20Editor/Framework/MapShortcuts.cs), [ModelShortcuts.cs:36](../../src/Smithbox.Program/Editors/Model%20Editor/Framework/ModelShortcuts.cs)). Focus is also set by *hovering* a window ([GUI/TextEditorView.cs:86-147](../../src/Smithbox.Program/Editors/Text%20Editor/GUI/TextEditorView.cs)), and the entry-list search bar is inside the entry-list window. So:
- **Delete** in the entry-list search bar deletes the selected entries (and their associated summary/description entries, see M8).
- **Ctrl+Z / Ctrl+Y** in the Contents text box undo or redo the last *editor* action as well as ImGui's own text undo.
- **Ctrl+D** and **Insert** duplicate entries or open the creator while you're typing in the search bar.

*Repro:* select an entry, click into the entry-list search bar, type `abc`, press Delete. The entry is gone.
*Fix:* return early from the entry-list shortcuts (and undo/redo) when `ImGui.GetIO().WantTextInput` is true. Consider setting focus on click instead of hover (U1).

#### B2. Saving sorts the live entry list, which breaks index-based undo
`FMG.Write` sorts `Entries` in place ([FMG.cs:191](../../src/Andre/SoulsFormats/SoulsFormats/Formats/FMG.cs)). It runs on the FMG the editor is showing, not a copy. Every structural action stores a list *index* when it's built and undoes by index: `AddFmgEntry`, `DuplicateFmgEntry` and `AddAssociatedEntry` undo with `RemoveAt(InsertionIndex)`, and `DeleteFmgEntry` with `Insert(InsertionIndex, …)` ([Actions/](../../src/Smithbox.Program/Editors/Text%20Editor/Actions/)). Anything that leaves the list out of ID order gets re-sorted on save, so later undos hit the wrong row. The things that leave it out of order are drag-reorder, `AddAssociatedEntry` (B5), and `AddFmgEntry` batches built against a stale list (import, language sync).
- Undo then removes a *different* entry, and the one you added stays.
- Drag-reorder is undone by the next save. It never reaches the file anyway, because the file is always written sorted (see U6).

*Repro:* turn on grouped entries in ER. In Item › Weapons, pick the highest-ID weapon that has no Summary and click *Add Summary* (it goes in at index 0, B5). Save, then press Ctrl+Z. The Summary FMG's original first entry disappears and the new empty one stays.
*Fix:* make the actions identity-based: undo with `Remove(entry)`, and insert at the sorted position worked out at execute time. Alternatively, have the save write a sorted copy so it doesn't mutate `Entries`.

#### B3. Save runs on a worker thread
This is the same pattern as Param Editor B1. `TextEditorScreen.Save()` and `SaveAll()` are `async void`, and `TextBank.SaveFmgContainer` / `SaveLooseFmg` start with `await Task.Yield()` ([TextEditorScreen.cs:195-269](../../src/Smithbox.Program/Editors/Text%20Editor/TextEditorScreen.cs), [Data/TextBank.cs:313-533](../../src/Smithbox.Program/Editors/Text%20Editor/Data/TextBank.cs)). The app has no `SynchronizationContext`, so the write, the in-place sort from B2, `ClearDirtyState()`, `SaveConfiguration()` and `RefreshFmgTextCache()` all run on a pool thread while the UI keeps drawing and editing the same lists.
- An edit made during the save gets its `*` cleared by `ClearDirtyState()` even though it isn't on disk.
- The sort reorders `Entries` under the UI's `for` loops, so rows jump around and selection indices go stale.
- `RefreshFmgTextCache()` dereferences `ParamEditor.ViewHandler.ActiveView` without a null check ([TextEditorScreen.cs:271-276](../../src/Smithbox.Program/Editors/Text%20Editor/TextEditorScreen.cs)).

*Fix:* run the save on the UI thread (text containers are small), or snapshot first and only clear dirty flags that didn't change during the write.

#### B4. Undoing a delete restores a copy, so older undo steps edit an orphan
`DeleteFmgEntry` stores `OldEntry = entry.Clone()` and re-inserts the *clone* on undo ([Actions/DeleteFmgEntry.cs:23, 47](../../src/Smithbox.Program/Editors/Text%20Editor/Actions/DeleteFmgEntry.cs)). Any earlier `ChangeFmgEntryText` or `ChangeFmgEntryID` on the stack still points at the original object, which is no longer in the FMG. The constructor's ID loop also has no `break`, so with duplicate IDs it removes the *last* entry with that ID, not the one you selected.

*Repro:* change entry 100's text to `X`, delete entry 100, then press Ctrl+Z twice. The entry comes back with `X` instead of its original text.
*Fix:* store and re-insert the original `entry`, and find its index with `IndexOf(entry)`.

#### B5. "Add Title/Summary/…" inserts at index 0 when the new ID is the highest
`AddAssociatedEntry` starts `InsertionIndex` at `0` rather than `Fmg.Entries.Count` ([Actions/AddAssociatedEntry.cs:22](../../src/Smithbox.Program/Editors/Text%20Editor/Actions/AddAssociatedEntry.cs)). If no entry has a larger ID, the new entry goes to the top of the list. This feeds B2.
*Fix:* start at `Count`, as `DuplicateFmgEntry` does.

#### B6. Global Replace changes text the preview didn't show
In [Tools/GlobalTextReplacement.cs](../../src/Smithbox.Program/Editors/Text%20Editor/Tools/GlobalTextReplacement.cs) and [Tools/TextFinder.cs:190-276](../../src/Smithbox.Program/Editors/Text%20Editor/Tools/TextFinder.cs):
- **Apply twice = replaced twice.** `replacementResults` isn't refreshed after Apply, so a second click runs the replacement again. For example, `Sword`→`Great Sword` becomes `Great Great Sword`.
- **Preview and apply use different regex settings.** With *Ignore case* on, the preview lowercases the pattern itself, which turns `\S`/`\D`/`\W` into `\s`/`\d`/`\w`. Apply uses `RegexOptions.IgnoreCase` on the original pattern. The preview also ignores the Multiline, Singleline and IgnorePatternWhitespace options.
- **`$1` doesn't work.** Apply always adds `RegexOptions.ExplicitCapture`, so unnamed groups don't capture.
- **The replacement string is validated as a regex** (`Regex.Match("example", replaceTerm)`), so a replacement such as `(` or `[` is rejected as invalid.
- **Crash on null text.** With Match Type *All* or *ID*, entries with null text match by ID, and `Regex.Replace(null, …)` throws `ArgumentNullException`.
- **Hidden results are still replaced.** The preview hides results from unused containers (ER `item`/`menu`/`_dlc01`), but Apply still replaces them.

*Fix:* build one `Regex` (pattern plus options) and use it for both preview and apply. Skip null text. Drop the ExplicitCapture flag and the replacement validation. Re-run the preview after Apply, and apply the same container filter as the preview.

#### B7. Undoing Language Sync can delete the wrong entries; a sync into an empty FMG crashes
In [Tools/TextLanguageSyncTool.cs:298-394](../../src/Smithbox.Program/Editors/Text%20Editor/Tools/TextLanguageSyncTool.cs):
- Each FMG's actions end with `SortFmgList`, whose `Undo()` does nothing ([Actions/SortFmgList.cs:22](../../src/Smithbox.Program/Editors/Text%20Editor/Actions/SortFmgList.cs)). The `AddFmgEntry` undos then `RemoveAt` the pre-sort indices, which removes other entries (same mechanism as B2).
- `syncTargetWrapper.File.Entries.First()` throws if the target FMG is empty.
- If the source language's vanilla FMG isn't loaded (the vanilla bank only loads the primary language at startup), `GetVanillaSrcWrapper` returns null. Nothing is synced for that FMG, but the log still says the sync succeeded.

#### B8. FMG Import ignores the menu you chose it from, and crashes on duplicate IDs
In [Tools/FmgImporter.cs:64-174](../../src/Smithbox.Program/Editors/Text%20Editor/Tools/FmgImporter.cs):
- `DisplayImportList(importType)` never uses `importType`. *Import Text* from an **FMG** or **entry** context menu imports into every matching FMG in the whole container.
- Nothing checks that the stored container is the same file. An `item` export imported into `menu` writes into any FMG whose binder ID happens to match.
- `ToDictionary(e => e.ID)` throws `ArgumentException` if the target FMG, or the stored file, has duplicate IDs. The *Allow Duplicate ID* option makes that state reachable.
- The imported `FMG.Entry` objects *are* the cached `ImportSources` objects, and `storedEntry.Parent` is overwritten. Importing the same source into two places shares one entry object between them.
- Import skips the diff tracking and never refreshes it, so the modified/unique colours stay wrong until you reselect.

### 1.2 Wrong behaviour (Medium)

**M1. Alt+D (configurable duplicate) never opens** *(verify)*. `ImGui.OpenPopup("textDuplicatePopup")` runs in `Monitor()` at editor-window level, but `BeginPopup` runs inside the `TextEditorView##N` window ([TextShortcuts.cs:78](../../src/Smithbox.Program/Editors/Text%20Editor/Framework/TextShortcuts.cs), [GUI/TextDuplicatePopup.cs:23](../../src/Smithbox.Program/Editors/Text%20Editor/GUI/TextDuplicatePopup.cs)). The popup IDs differ, so the popup never shows. If it does open, Submit only works when hover-focus is still "entry list", and the popup doesn't close after Submit. The popup duplicate also never checks for ID collisions.

**M2. Grouped mode shows an empty Contents panel when the Title entry is missing.** `DisplayGroupedTextInput` returns early when `fmgEntryGroup.Title == null` ([GUI/TextContents.cs:77-80](../../src/Smithbox.Program/Editors/Text%20Editor/GUI/TextContents.cs)), so the *Add Title Entry* button at lines 185-198 can never show. *Repro:* in a Description FMG, select an entry whose ID has no Name entry.

**M3. Undo after opening a new container can crash** *(verify)*. On the first selection of a container whose FMGs aren't loaded yet (any non-primary language), `SelectFileContainer` loads it but doesn't auto-select an FMG, so `SelectedFmgWrapper` stays null ([Framework/TextViewSelection.cs:56-70](../../src/Smithbox.Program/Editors/Text%20Editor/Framework/TextViewSelection.cs)). Any undo or redo then calls `TrackFmgDifferences`, which reads `SelectedFmgWrapper.ID` ([Framework/TextDifferenceManager.cs:55](../../src/Smithbox.Program/Editors/Text%20Editor/Framework/TextDifferenceManager.cs)) → `NullReferenceException`. *Repro:* edit an English entry, show all categories, click a French container for the first time, press Ctrl+Z.

**M4. Shift-click with no current selection crashes.** `HandleMultiselect` loops from `_selectedFmgEntryIndex`, which can be `-1` (for example after selecting an FMG while the filter matches nothing). `k < Count` passes, and then `Entries[-1]` throws ([Framework/TextMultiselection.cs:40-57](../../src/Smithbox.Program/Editors/Text%20Editor/Framework/TextMultiselection.cs)).

**M5. Selection survives the entries it points at.**
- After Delete, `_selectedFmgEntry` still points at the deleted entry. The Contents panel keeps showing it and accepts edits, which mark the container dirty but change nothing visible.
- Ctrl+click on the primary selected row removes it from `StoredEntries`, but `_selectedFmgEntryIndex` is then set to that same index, so the row still looks selected.

**M6. The multi-selection is keyed by list index.** `StoredEntries` is `SortedDictionary<int index, Entry>`. After a duplicate, delete, reorder or undo, the highlighted rows are no longer the stored entries. **Copy Text** selects by index (`IsMultiselected(i)`, [Framework/TextActionHandler.cs:420](../../src/Smithbox.Program/Editors/Text%20Editor/Framework/TextActionHandler.cs)), so it copies whatever entries now sit at those positions. *Repro:* Ctrl+click entries at rows 5 and 10, press Ctrl+D, then Copy Text. The second copied line is the old row 9.

**M7. Hidden null entries are included in bulk actions.** `DisplayEntryList` hides null-text entries when the toggle is off, but Select All, Shift-range, auto-select and Copy apply only the search filter. *Repro:* hide null entries, click Select All, then Delete. The hidden null entries are deleted too.

**M8. Delete and Duplicate still cascade when grouped entries are turned off.** `ProcessDelete` and `ProcessDuplicate` check only `fmgEntryGroup.SupportsGrouping`, not `CFG.TextEditor_Text_Entry_Enable_Grouped_Entries` ([Framework/TextActionHandler.cs:200-353](../../src/Smithbox.Program/Editors/Text%20Editor/Framework/TextActionHandler.cs)). With grouping off, the editor shows a single entry, yet deleting a weapon name also deletes its summary, description and effect.

**M9. ID collisions aren't checked consistently.**
- `GetNextValidId` checks only the selected FMG, so a grouped duplicate can create a duplicate ID in the Summary or Description FMG.
- The grouped ID edit checks only the Title FMG. The basic ID edit doesn't check at all.
- When a change is refused, nothing tells the user. The field just snaps back.

**M10. Text longer than 2000 bytes is truncated when you edit it** *(verify)*. Every `InputTextMultiline` uses `maxLength: 2000` ([GUI/TextContents.cs:171, 218, 264, 310, 397](../../src/Smithbox.Program/Editors/Text%20Editor/GUI/TextContents.cs)). That limit is in UTF-8 bytes, which is about 660 CJK characters.

**M11. Container list issues** ([GUI/TextContainerList.cs](../../src/Smithbox.Program/Editors/Text%20Editor/GUI/TextContainerList.cs)):
- *Copy Path* copies the filename (line 321).
- DS2: the *Talk* header reuses `##{name}_common`, the same ID as *Common* (lines 98, 140), so toggling one header toggles both.
- DS2: a container's selection index only advances inside expanded headers, so collapsing a header moves the highlight to a different container, or highlights two.

**M12. DS2 FMGs have no context menu.** Loose DS2 FMGs have `ID = -1`, and the menu and focus code requires `SelectedFmgKey != -1` ([GUI/TextFileList.cs:371](../../src/Smithbox.Program/Editors/Text%20Editor/GUI/TextFileList.cs)). Language sync, import and export are unreachable at the FMG level. The `-1` sentinel also makes the row look selected before you've selected it.

**M13. Jumping from the Param Editor or search lands in an arbitrary container.** `TextCommandQueue.Parse` accepts `name`, `name_dlc01` or `name_dlc02` and takes whichever comes first in `ConcurrentDictionary` order ([Framework/TextCommandQueue.cs:39-82](../../src/Smithbox.Program/Editors/Text%20Editor/Framework/TextCommandQueue.cs)). In ER that can be the obsolete `item` container. `fileIndex` counts in that same unordered order, so it doesn't match the sorted index the list highlights. *Fix:* prefer dlc02, then dlc01, then base, and select by `FileDictionaryEntry` rather than index.

**M14. The Param Editor's text lookup cache is never cleared.** `TextFinder.CachedResults` is `static` and is never invalidated ([Tools/TextFinder.cs:11-101](../../src/Smithbox.Program/Editors/Text%20Editor/Tools/TextFinder.cs)). Param-editor FMG decorators keep showing deleted entries, entries whose ID has changed, the old primary language, and even another project's text after you switch projects. Misses aren't cached, so every missing reference rescans every primary container each frame.

**M15. Modified/unique colours are missing for non-primary languages.** `SelectFileContainer` finds the vanilla container by `Filename` only ([TextViewSelection.cs:64](../../src/Smithbox.Program/Editors/Text%20Editor/Framework/TextViewSelection.cs)). That's often another language's container, so the container you're looking at never gets its vanilla FMGs loaded and the comparison finds nothing.

**M16. Multiple views corrupt each other's undo.** Every `TextEditorView` has its own `ActionManager` over the same `TextBank`. Combined with index-based undo (B2), undoing in view 1 after structural edits in view 2 removes the wrong rows. Separately, the view tab's context popup is closed with `ImGui.EndMenu()` instead of `EndPopup()` ([Framework/TextViewHandler.cs:136-153](../../src/Smithbox.Program/Editors/Text%20Editor/Framework/TextViewHandler.cs)), which unbalances ImGui's stack.

**M17. Global Search with "All categories" doesn't search unopened languages.** `GetGlobalTextResult` doesn't load unloaded FMG wrappers, unlike `GetReplacementResult`, so other languages return no results and nothing says why.

### 1.3 Edge cases (Low)

- **L1.** `TextData.Setup` logs the FMG-descriptor result backwards: the error path uses the `_PASS` key and the success path uses `_FAIL` ([Data/TextData.cs:42-49](../../src/Smithbox.Program/Editors/Text%20Editor/Data/TextData.cs)). `LoadAuxBank` reports its failure with the vanilla-bank key, and the aux bank enumerates the *main* project's `Locator.TextFiles`.
- **L2.** `CopyEntryTextToClipboard` runs on a `TaskManager` background task. It iterates the live list and sets the clipboard off the UI thread.
- **L3.** `SelectFileContainer` uses `Smithbox.Orchestrator.SelectedProject` instead of its own `Project`, and doesn't null-check the Param Editor's `ActiveView`.
- **L4.** Performance:
  - Every sub-action of a grouped action rebuilds the full diff cache, so Select All plus Duplicate on 5k entries does 5k full rebuilds.
  - `GetEntryGroup` builds `GetGroupings()` and scans four FMGs every frame, and again for every entry in bulk actions.
  - Opening a container, and the global replace preview, load FMGs synchronously on the UI thread.
- **L5.** The Entry Creator keeps the previous entry's Summary or Description text when the new selection has none (`UpdateParameters` only overwrites non-null fields).
- **L6.** The DLC-priority tooltips in the grouped file list are attached after `HandleFmgView`. When the filter hides that row, the tooltip lands on the previous item.
- **L7.** `LanguageSync` triggers `LoadFmgWrappersAsync` only when `FmgWrappers == null`, but BND containers always start with an empty list, so it never fires.

---

## 2. Usability improvements

**U1. Focus on click, not hover.** Hover-focus is what makes B1 and M1 so easy to hit. Moving the mouse across the Contents panel on the way to a button changes which shortcuts are live. Set the focus context when a window is clicked or focused (`IsWindowFocused(ChildWindows)`).

**U2. Ctrl+S saves only the selected container.** Global Replace, Language Sync and Import change many containers, and the `*` markers show this, but Ctrl+S saves just one. Make Ctrl+S save all dirty containers (as the other editors' save does), or add a Save All keybind and show it in the menu.

**U3. Tell the user when an edit is refused.** Show an inline error when an ID change collides ("ID 1000 already exists in Weapon Name"). Apply the same check in basic and grouped mode, across every associated FMG.

**U4. Make undo visible.** Undo history is per view and covers every FMG, so Ctrl+Z after navigating changes data you can't see. Select the affected container, FMG and entry on undo/redo, or show the action name in the Edit menu ("Undo: Delete 3 entries").

**U5. Confirm bulk deletes.** Show "Delete 42 entries (+ 84 associated entries)" before a multi-delete, especially since it cascades into other FMGs (M8).

**U6. Drag-reorder.** The file is always written sorted by ID (B2), so reordering does nothing in game and is reverted on save. Either remove drag-reorder, or replace it with something meaningful, such as "drag to re-ID", or at least a tooltip saying order isn't saved.

**U7. Global Replace preview.**
- Show *before → after* for each result, with a count and a checkbox per result.
- Refresh the preview automatically after Apply, and disable Apply until the preview is current.
- Document `$1`/`${name}` support once the B6 fixes are in.

**U8. Make the filter commands discoverable.** The entry-list filter supports `modified` and `unique`, but nothing in the UI says so. Add toggle buttons, or list them in the search-bar tooltip, and add a legend for the row colours.

**U9. Text box ergonomics.**
- The Title box is a fixed 32 px, and other boxes grow with content but never shrink below 100 px.
- Add word wrap, a character/byte counter (helpful with M10), and highlighting for the game's markup (`<?…?>` tags, `<font color>`).

**U10. Import/export clarity.** Show the source container name for each stored import, warn when it doesn't match the target, and report a count afterwards ("12 added, 30 changed").

**U11. Language Sync feedback.** Report counts per FMG, and warn instead of logging success when the source vanilla FMG isn't loaded (B7).

**U12. Keyboard navigation.** Arrow selection depends on the hovered/focused-row trick (`SelectNextFmgEntry`). Add Page Up/Down and Home/End, and keep the selection scrolled into view when moving with the keyboard.

---

## 3. Suggested fix order

1. **B1** (shortcut guard): a one-line guard, and it stops accidental deletes.
2. **B2, B4, B5, B7** together: switch the structural actions to identity-based undo and give `SortFmgList` a real undo, or remove it.
3. **B3**: save on the UI thread.
4. **B6**: share one regex between preview and apply.
5. **M2–M7**: selection and grouping crashes and inconsistencies.
6. **B8, M13–M15**: lookup correctness.
7. The usability items, starting with U1–U3.

## 4. What to check after fixes
- Hover the entry list, type in its search bar and press Delete: no entries are removed.
- Add Summary on the highest-ID weapon, save, then undo: the new entry is removed and the first entry stays.
- Edit text, delete the entry, undo twice: the original text is back.
- In Global Replace, apply `(\w+) Sword` → `$1 Blade` with Ignore case on: the preview matches what gets replaced, and a second Apply changes nothing.
- Alt+D in the entry list opens the duplicate popup.
