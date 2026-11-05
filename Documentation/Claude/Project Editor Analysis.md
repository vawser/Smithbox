# Project Editor Analysis

Scope: `src/Smithbox.Program/Editors/Project Editor/` (orchestrator, entry, screen, configure menu, tool window,
handler, VFS, locator head, utilities). Static read only; nothing was run.

Severity: **High** = data loss / resource leak / wrong project affected. **Medium** = wrong state, recoverable.
**Low** = latent, cosmetic or platform-specific.

---

## 1. Bugs

### High

#### B1. Loading or reloading from the Project Editor never disposes the previous project
`ProjectScreen.cs:515`, `:528`, `:562` set `orchestrator.SelectedProject = project` **before** calling
`StartupProject` / `ReloadProject`. `LoadProject` (`ProjectOrchestrator.cs:294`) then reads
`oldProject = SelectedProject`, which is now the *new* project, so it disposes the new (empty) project and leaves the
real previous one alive.

Result: switching project A → B through the context menu or "Load" leaves A fully initialized (editors, banks,
VFS archive handles) and still listed under Active Projects. `ResourceManager.ClearAllPools()` is still called, so A's
GPU resources are dropped from under it — clicking A in the Active list later re-selects a project whose resources are
gone. The auto-load and Create paths don't pre-set `SelectedProject`, so they are fine.

Fix: remove the pre-assignment in the three UI call sites (the orchestrator selects on completion), or capture
`oldProject` in the UI before assigning.

> **Correction (2026-11-05):** several projects being active at once is intended. The real defects were that
> `LoadProject` *tried* to dispose the selected project (only the pre-assignment stopped it) and that
> `ClearAllPools` dropped the shared resource pools while other projects were still active. The fix now keeps
> every active project loaded; see the fix-status table.

#### B2. Deleting any project unselects the active one, without disposing it
`ProjectUtils.DeleteProject` (`ProjectUtils.cs:65`) sets `SelectedProject = null` regardless of which project is
being deleted. Deleting an unrelated, never-loaded project from the Available list makes the open project vanish
(its `Update`/`Draw` stop) while its resources stay allocated. Deleting the loaded project also skips `Dispose()`.
There is also no confirmation dialog.

#### B3. A deleted project can be resurrected
After `DeleteProjectAction`, `ConfigureMenu` stays in Edit mode on the deleted `ProjectEntry`, and
`ProjectScreen.SelectedLoadedEntry/SelectedAvaliableEntry` still reference it. Clicking **Update** calls
`UpdateProject` → `SaveProject`, which rewrites `{GUID}.json`, and then loads a project that is no longer in
`Projects`. It reappears on next start-up. Fix: after delete, `ConfigureMenu.SetupForCreation()` and clear both
selection fields.

#### B4. After "Update", the configuration form edits the live descriptor directly
`UpdateProject` (`ProjectOrchestrator.cs:262`) assigns `project.Descriptor = newProjectDescriptor`, and
`newProjectDescriptor` *is* `ConfigureMenu.Descriptor`. From then on, typing in the name/path fields mutates the live
project descriptor without pressing Update, and the next `SaveProject` (auto-load assignment, exit) persists it —
including a half-typed or invalid path. Fix: store a `Clone()` in `UpdateProject`, or re-run `SetupForEdit` after
updating.

#### B5. "Update" reverts pins made since the project was selected
`SetupForEdit` clones the descriptor at selection time (`ProjectConfigureMenu.cs:58`). The clone includes
`PinnedParams/PinnedRows/PinnedFields` and `AutoSelect`. Pins added in the Param Editor afterwards are on the live
descriptor; pressing Update writes the stale clone back over them. Fix: in `UpdateProject`, copy only the fields the
form edits onto the live descriptor (or carry pins/AutoSelect over from the live one).

#### B6. Save failures are reported as plain log lines
`ProjectUtils.WriteWithBackup` (`ProjectUtils.cs:291-294`) catches every exception and calls `Smithbox.Log` (not
`LogError`), and returns normally. Callers (all param saves) believe the save succeeded; a locked `regulation.bin`
(game running) only produces an ordinary log message. Should be `LogError` with high priority, ideally returning a
bool so the caller can avoid printing "saved".

### Medium

#### B7. Init failures are ignored — a project that failed validation is still selected
`ProjectEntry.Init` returns `false` for an undefined type or missing project/data directory
(`ProjectEntry.cs:130-149`), but `LoadProject` ignores the result (`ProjectOrchestrator.cs:320`) and selects it.
The old project has already been disposed (`:300-304`), so the user ends up with nothing usable and a "Complete"
progress message. Likewise `Handler.InitializeData`'s bool is discarded and every stage's exception is swallowed
before `Initialized = true`. Fix: `if (!await curProject.Init(...)) { RunOnMainThread(EndProjectLoad); return false; }`
and validate before disposing the old project.

#### B8. `UpdateProject` has no loading guard and can switch projects unexpectedly
`UpdateProject` calls `ReloadProject` unconditionally (`ProjectOrchestrator.cs:266`), unlike every other caller that
checks `IsProjectLoading`. It also reloads the *edited* project even if it isn't the open one: editing project B's
name from the Available list while A is open disposes A and opens B. Consider only reloading if
`project == SelectedProject`, and just saving otherwise.

#### B9. Grouped projects never appear in the Active Projects list
`DisplayLoadedProjectEntries` (`ProjectScreen.cs:221-223`) skips every project with a non-empty `FolderTag`
("handled above"), but nothing above handles them — the folder grouping lives only in the Available list path.
A loaded project with a group tag is invisible in Active Projects.

#### B10. Auxiliary projects show up as selectable "active" projects
Projects loaded through `LoadAuxiliaryProject` (map/param/text comparison) become `Initialized` and therefore
appear in Active Projects. Selecting one makes a partially initialized project (e.g. `ParamEditorOnly`) the main
project, again without disposing the current one. Also, for such a project the context-menu **Load** is disabled
(`!project.Initialized`), so the only way to fully load it is Reload. Suggest a `IsAuxiliary` flag on `ProjectEntry`
and filtering it out of the Active list.

#### B11. MaterialData race in the handler
`ProjectEditorHandler.cs:141-145` always starts the MaterialData `Task.Run`, but only awaits it when Map/Model/TimeAct
are enabled. For a param-only or text-only init, `InitializeEditors` (`:321`) checks `MaterialData != null`, which is
assigned *inside* the un-awaited task — so whether a Material Editor is created is timing-dependent, and the setup
work (and any exception) is unobserved. Either await it always, or only start it when it's needed.

#### B12. Handler.Dispose misses `CommonData` and `TimeActData`
Both are `IDisposable` (`CommonData.cs:83`, `TimeActData.cs:382`) but aren't disposed in
`ProjectEditorHandler.Dispose` (`:345-362`). Leaks on every project switch/unload.

#### B13. PTDE file system is appended to every project's VFS
`ProjectVFS.cs:92-96` adds `PTDE_FS` to the compound FS for *all* project types when `PTDE_Data_Path` is set. An ER
project lookup that misses in project + vanilla falls through to the DS1 PTDE folder. Should be gated to
`ProjectType.DS1R`.

#### B14. "Create Project" can be pressed repeatedly and silently skips loading
After `CreateProjectAction`, the form stays in Create mode with the same descriptor, so a second click creates a
duplicate (new GUID, same path). And if a load is in progress, `CreateProject` adds the project but doesn't load it,
with no feedback (`ProjectOrchestrator.cs:254`). Switch to Edit mode on the new entry after creation.

#### B15. Legacy JSON import can create two projects
`CreateProjectFromLegacyJson` (`ProjectOrchestrator.cs:495-521`) wraps `CreateProject` in the first `try`. If
deserialization succeeds but `CreateProject`/`SaveProject` throws (e.g. IO), the fallback branch runs and calls
`CreateProject` again — the first entry was already added to `Projects`. Only the deserialize call should be inside
the first try. Also: legacy imports set `AutoSelect = true` without clearing it on other projects (multiple
auto-select projects; start-up picks whichever file enumerates first), and the legacy pinned params/rows/fields are
read but dropped.

#### B16. "Delete Backup Files" can delete the only copy of vanilla originals
`WriteWithBackup` writes `<file>.bak` only when writing to a non-project FS (i.e. directly into the game folder,
`ProjectUtils.cs:171`). `DeleteBackupFilesAction` recursively deletes every `.bak`/`.prev` under `ProjectPath`. If the
project path *is* the game folder, this deletes the pristine-original backups. The confirmation dialog also appears
even when zero files were found. Suggest excluding `.bak` when `ProjectPath == DataPath`, or listing the two
kinds separately.

### Low

| # | Location | Issue |
|---|---|---|
| B17 | `ProjectEntry.cs:373-376, 395-398, 418-421` | OSX/Linux branches use `DataPath` instead of `Descriptor.DataPath` — won't compile on those targets. |
| B18 | `ProjectConfigureMenu.cs:641` | `FindGameExecutable` already returns the *directory* (`SteamUtils.cs:120`), so `GetDirectoryName` on it sets `PTDE_Data_Path` to the install root, one level above `DATA`. Looks like a leftover from when it returned the exe path. |
| B19 | `ProjectUtils.cs:229-262` | BXF branch writes `assetPath.temp` (the BDT) but only moves the BHD; the BDT temp is never moved into place. No current callers pass a BXF, so latent. |
| B20 | `ProjectUtils.cs:156` | Comprehensive-backup timestamp isn't zero-padded (`2026-11-5_9-3-7`) so backup folders don't sort chronologically. Use `DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")`. |
| B21 | `ProjectFileLocator.cs:146, 182, 198` | Progress goes 0.2 → 0.5 → 0.25 → 0.3 → 0.4; the bar jumps backwards. |
| B22 | `ProjectFileLocator.cs` `BuildProjectFileDictionary` | Directory scan includes `.smithbox`, `.backup`, `*.prev`, `*.bak`, `*.temp`. In non-strict mode these are added to the file dictionary (e.g. a `regulation.bin.prev` entry with extension `prev`). |
| B23 | `ModEngineHandler.cs:35, 61` | ME3 profile filename is built from `ProjectName`; renaming the project orphans the profile, and names with `:`/`?`/`"` throw from a UI button (uncaught). `LaunchME3Mod` redirects stdout/stderr but never reads them (can block ME3 if it writes enough) and doesn't dispose the `Process`. |
| B24 | `ProjectConfigureMenu.cs:208-226` | "Create Profile" with no profile directory set only asks for the directory; the user must click again to actually create the profile. |
| B25 | `ProjectOrchestrator.cs:467` | `UnloadProject` resets the window title even when unloading a non-selected project. |
| B26 | `ProjectEditorHandler.cs:221-272` | `LogDataResults` doesn't report `CommonData`, and data failures are logged with `Log`, not `LogError` (except ProjectData). |
| B27 | `ProjectConfigureMenu.cs:571, 601` | "Open directory" uses `explorer.exe` directly (Windows-only); with an empty path it opens Documents. |

---

## 2. Usability improvements

Ordered roughly by value for effort.

1. **Confirm destructive actions.** Delete Project should use the same OK/Cancel `MessageBox` pattern already used
   by Delete Backup Files, and state that only the Smithbox registration is removed (mod files are untouched).
2. **Show load state in the lists.** Mark the open project (bold / icon) in both lists, and show the project type and
   path as a tooltip. Today the Available list gives no indication of which entry is open.
3. **Double-click to load** in the Available list; currently loading is context-menu only (`LoadSelectedProjectAction`
   exists but isn't bound to anything visible in this screen).
4. **Make Create → Edit transition explicit.** After creating, select the new project in Edit mode (fixes B14) and
   show a short "Project created" status.
5. **Dirty indicator on the configuration form.** Compare the form descriptor with the live one and show
   "Unsaved changes" / enable Update only when something changed. Add a **Revert** button (re-runs `SetupForEdit`).
6. **Validate common mistakes.** Warn (not block) when project path == data path, when the project path is inside
   the game folder, when another project already uses the same project path, and when the data path doesn't contain
   the expected executable for the selected type (the Steam table in `FindSteamExecutables` already knows the names).
   Also trim names and reject whitespace-only names (`AllowCreation` checks `== ""` twice, never whitespace).
7. **Don't overwrite a typed data path when switching type.** Choosing a type replaces `DataPath` with the Steam path
   (`ProjectConfigureMenu.cs:436-482`) even if the user already entered one. Only auto-fill when the field is empty,
   or offer a "Use Steam install" button next to the path.
8. **New project default type.** `ProjectDescriptor.ProjectType` defaults to `DS1`, so the combo shows DS1 but no
   Steam auto-fill has happened and the "invalid type" warning never fires. Defaulting to `Undefined` makes the user
   choose and triggers the auto-fill.
9. **Report load failures in the UI.** When `LoadProject` fails (B7), show a popup with the reason (bad path etc.)
   instead of only a log line; the load modal currently just closes.
10. **Tools window empty states.** `ProjectToolWindow` shows nothing when a project is loaded but has no Param Editor,
    or when Project Merge is toggled off. Show "Project Merge requires the Param Editor" / "No tools enabled".
11. **Editor toggles: "All / None" buttons** and grey-out with a tooltip for editors the type doesn't support, rather
    than hiding them, so users understand why an editor is missing.
12. **Reduce the per-type tab code.** `DisplayAvailableProjectEntries` repeats the same block 13 times; looping over
    `ProjectTypeOrder.Order` would also make the tab order match the combo order (currently DS3 precedes BB in tabs
    but follows it in the combo).
13. **Backup cleanup preview.** Show counts by kind (`.bak` vs `.prev`) and total size in the dialog, and include the
    `.backup/<timestamp>` folders as a separate opt-in.

---

## 3. Suggested order of work

1. B1, B2, B3 — wrong-project disposal/selection; small, local fixes in `ProjectScreen` and `ProjectUtils`.
2. B4, B5 — descriptor aliasing in `UpdateProject`.
3. B7, B8 — honour `Init` result, guard `UpdateProject`.
4. B6, B12, B13, B11.
5. Usability items 1–6.
6. Remaining low items as convenient.

## 4. Fix status (2026-11-05)

All bugs B1–B27 were addressed. Notes on how:

| Bug | Fix |
|---|---|
| B1 | Multiple active projects are supported. `LoadProject` never disposes other projects; a reload re-initializes only that project (`Init` releases its old parts). `ProjectEntry.IsActive` = initialized and not auxiliary. `ResourceManager.ClearAllPools` only runs when no other project is active (reload/unload). Unloading the selected project selects another active one (`ProjectOrchestrator.SelectProject`). The UI no longer pre-assigns `SelectedProject`. |
| B2, B3 | New `ProjectOrchestrator.RemoveProject` unloads/disposes only the removed project and calls `ProjectScreen.OnProjectRemoved`, which clears selections and resets the form. Delete asks for confirmation. |
| B4, B5 | `ProjectDescriptor.ApplyConfiguration` copies only the form-edited fields onto the live descriptor; the form keeps its own clone. |
| B6 | `WriteWithBackup` returns `bool` and logs failures with `LogError` (High). Existing callers ignore the result. |
| B7 | `ProjectEntry.ValidateForInit` runs before the open project is disposed; `Init`'s result is honoured. `Init` also disposes previous parts before re-initializing. |
| B8 | `UpdateProject` only reloads the selected project and not while loading (logs a note instead). |
| B9 | Active list no longer skips group-tagged projects. |
| B10 | `ProjectEntry.IsAuxiliary`, set by `LoadAuxiliaryProject`, cleared by a full load; aux projects are hidden from Active Projects and can be fully loaded. `LoadAuxiliaryProject` won't re-initialize the open project. |
| B11 | MaterialData only started when needed and always awaited; the Material Editor is only created for full loads. |
| B12 | `CommonData` and `TimeActData` disposed. |
| B13 | PTDE FS only added for DS1R. |
| B14 | After Create, the form switches to Edit on the new project. Create while loading logs that it wasn't loaded. |
| B15 | Only deserialization is retried; legacy imports don't set `AutoSelect`; pins carried over (params only for the list form). |
| B16 | `.bak` files kept when the project path is the game path or inside it; "no backups" message when empty. |
| B17–B27 | As described in the table above (paths, PTDE dir, BXF move, timestamp, progress, scan exclusions, ME3 GUID lookup / safe names / no redirect, profile create after picking dir, title reset, data logging, cross-platform directory open). |

### Usability items (section 2) — all implemented

| # | Implementation |
|---|---|
| 1 | Delete Project confirmation (`DeleteProjectAction`). |
| 2 | Active projects shown green with " (Active)" in the Available list; tooltip shows type, project and data paths. |
| 3 | Double-click an Available entry to load it, or to switch to it if already active. |
| 4 | Create switches the form to Edit on the new project. |
| 5 | `ProjectDescriptor.ConfigurationEquals` drives an "Unsaved changes" note; Update is enabled only with changes; new Revert button. |
| 6 | Non-blocking warnings: project dir is/inside the game dir, another project uses the same dir, data dir lacks the game's exe (PC games only). Cached on their inputs. Whitespace-only names rejected. |
| 7 | Changing type only fills an *empty* data dir; new "Use Steam Install" button. |
| 8 | New projects start as `Undefined` (set in `SetupForCreation`; the class default is unchanged so stored projects are unaffected). |
| 9 | Load failures (validation, init, exceptions) show a "Project Load Failed" modal with the reason. |
| 10 | Tools window says when no tools are enabled, or that Project Merge needs the Param Editor. |
| 11 | Enable All / Disable All buttons; unsupported editors shown disabled with a tooltip. |
| 12 | Available-list tabs loop over `ProjectTypeOrder.Order` (BB now precedes DS3, matching the combo). |
| 13 | Backup cleanup dialog shows `.bak`/`.prev` counts and total size with project-relative paths; new "Delete Timestamped Backups" action for `.backup/<timestamp>` folders. |

## 5. What to check after fixes (manual)

- Open A, then load B from the Available context menu: A must disappear from Active Projects and memory should drop.
- Delete an unrelated project while A is open: A stays open; the form returns to Create mode.
- Edit the name field after pressing Update, then restart without pressing Update again: the old name persists.
- Pin a row in the Param Editor, then press Update in the Project Editor: the pin survives.
- Load a project whose data path was removed: an error is shown and the previous project is still open.
- Give a loaded project a group tag: it still appears in Active Projects.
- Load A, then B: both are listed under Active Projects; clicking A switches back to it with its editors intact.
- Unload the selected project while another is active: the other becomes selected. Reload one: the other's map/model viewports keep their resources.
- Available list: active projects are green with "(Active)"; hovering shows paths; double-clicking loads or switches.
- Edit a field: "Unsaved changes" appears, Update enables; Revert restores the saved values.
- Set the project dir to the game dir: the warning appears. Point the data dir at a wrong folder: the missing-exe warning appears.
- New project: type shows Undefined; picking a type fills the data dir only if it was empty.
- Switch type to one without e.g. the Havok Editor: its checkbox is greyed with a tooltip.
- Load a project whose data dir was removed: the "Project Load Failed" popup appears.
- Tools window with Project Merge toggled off, or with the Param Editor disabled: an explanatory message shows.
- Delete Backup Files: dialog shows counts and size. Delete Timestamped Backups: dialog shows folder count, size and date range.
