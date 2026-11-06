# Project Editor Presentation Plan

Goal: the Project Editor should explain itself. With no project, the user is led straight to creating one. With a
project loaded, everything about it — settings, editors, aliases and enums — is in one place, labelled with the
project it belongs to, and every empty state says what to do next.

The Project Metadata Editor is removed; its Aliases and Enums editors move into the Project Editor (decision 6).

Follows on from `Project Editor Analysis.md` (bugs and the first usability pass, all done) and uses the conventions
in `Tool UI Styling.md`. Static read only; nothing was run.

---

## 1. How it looks today

### Project Editor (`Editors/Project Editor/ProjectScreen.cs`)

- Four windows — **Active Projects**, **Available Projects**, **Project Configuration**, **Project Tools** — are all
  docked into the same node with `ImGuiCond.FirstUseEver`, so on a fresh layout they are four tabs and only one is
  visible. Which tab is in front depends on draw order (likely **Project Tools**, the last one drawn, which says
  "No project has been selected yet."). *To check on a fresh `imgui.ini`.*
- **No projects at all:** Active Projects is an empty bordered box; Available Projects is a filter bar and an empty
  box (the tab bar has no tabs). Nothing says "create a project", or where.
- **Project Configuration** is one form used for two jobs. Clicking a list entry silently switches it to Edit mode;
  the "Create New Project" button at the top switches it back. There is no title saying which project is being
  edited, or whether that project is loaded.
- The Create form asks for name → type → project dir → data dir → group tag in that order, though the type drives
  the data dir (Steam auto-fill) and the data dir is what most users actually have. Problems are listed at the
  bottom ("You cannot create a project due to the following issues:"), away from the fields they belong to.
- In Edit mode, **Update / Revert** sit in the same button stack as **Delete Project**, **Delete Backup Files** and
  **Delete Timestamped Backups**.
- The editor toggles (column 2) take half the width even though most users never change them.
- Active and Available are two separate lists; an active project appears in both (green "(Active)" in Available).
- After a load, `ProjectOrchestrator.Update` sets `ProjectEditor.RequestFocus`, so the user is brought back to the
  Project Editor rather than to the editors they just loaded.

### Project Metadata Editor (`Editors/Project Metadata Editor/ProjectMetadataScreen.cs`)

- A second top-level editor tab that only edits the *selected project's* aliases and enums, separate from the
  Project Editor where that project is chosen.
- The mode (Project / Param Editor) is a combo **inside** a "Mode" menu in the menu bar — easy to miss.
- Param Editor mode shows two "WIP" placeholder windows (`ParamDefMenu`, `ParamMetaMenu`).
- Project mode shows **Enums** and **Aliases** as tabs. Neither says which project it is editing.
- With no project: each window repeats five guard checks and prints "A valid project must be selected to use this
  editor." / "A loaded project must be selected…", with no way to act on it.
- The guards test `ProjectEditor.SelectedLoadedEntry`, a Project Editor UI field, rather than the orchestrator's
  `SelectedProject`. `SelectProject` keeps them in step today, but it is the wrong dependency.
- Aliases: the data source (Base / Project) is a toggle hidden in a "Data Source" menu; the window doesn't show
  which sources are active, which file **Save** writes, or that there are unsaved alias types
  (`ProjectEntry.UnsavedAliasTypes` is tracked but not shown).
- Enums: when the Param Editor is disabled the window shows a plain-text hint, but nothing to fix it.

References to the metadata editor outside its folder:

| Where | Use |
|---|---|
| `ProjectOrchestrator.cs:51, 58, 71` | Field, construction, `OnGUI` call |
| `ProjectOrchestrator.cs:670-671`, `ProjectScreen.cs:381-382` | Clear the alias/enum undo stacks on project switch |
| `ProjectEntry.cs:392-402` | Unsaved-changes prompt: alias and enum entries and their save actions |
| `ModelSourceList.cs:427` | Model Editor "Update Alias" saves through `AliasMenu.SaveIndividualAlias` |
| `FocusManager.cs:146-153, 197-200` | `IsInProjectMetadataEditor`, `Metadata_*` focus contexts |
| `GUI.cs:47, 139` | `DockGroup_ProjectMetadataEditor` / `…View` window classes |

The namespace `StudioCore.Editors.MetadataEditor` is also imported by 23 files outside the folder for `ProjectAliasType`, the enum
types and the edit actions; those types stay.

---

## 2. Target design

### 2.1 Project Editor: one window, four states

Replace the four tabs with a **Projects** sidebar and a **detail pane**, drawn by `ProjectScreen` in one window
(a table with a resizable split, as `ProjectConfigureMenu.DisplayMenu` already does). The Tools window stays a
separate dockable window, hidden by default until a project is loaded.

```
┌ Projects ───────────────┬ Detail ──────────────────────────────────────────────┐
│ [+ New Project]         │  (depends on state, see below)                       │
│ [Import project.json]   │                                                      │
│ filter…                 │                                                      │
│ ▾ Open                  │                                                      │
│   ● My ER Mod   (ER)    │                                                      │
│ ▾ Elden Ring            │                                                      │
│   My ER Mod   ✓         │                                                      │
│   Test Mod              │                                                      │
│ ▾ Dark Souls III        │                                                      │
│   …                     │                                                      │
└─────────────────────────┴──────────────────────────────────────────────────────┘
```

- **Sidebar:** one list. An **Open** group at the top (the active projects, `●` on the selected one), then one
  collapsible group per game in `ProjectTypeOrder.Order`, then group tags inside each game. Click selects into the
  detail pane; double-click loads/switches (as today); the context menu is unchanged.
  Replaces `DisplayActiveProjectList` and `DisplayAvailableProjectList`.
- **Detail pane** — chosen from state, in this order:

| State | Shown |
|---|---|
| **A. No projects registered** | *Get Started* panel |
| **B. "New Project" pressed** (or A's button) | *Create Project* form |
| **C. Projects exist, none selected** | *Open a Project* panel |
| **D. A project selected** | Header card + tabs **Overview · Aliases · Enums** |

#### State A — Get Started

- Title and two sentences: what a project is (your mod folder + the game's data folder), and that Smithbox never
  writes to the game folder unless it is the project folder.
- **Detected games**: one button per game Steam found (`FindSteamExecutables` already does this):
  "Create an Elden Ring project". It opens the Create form with the type and data directory filled in.
- **Create New Project** (empty form) and **Import project.json** buttons.
- Drawn with a new shared `GUI.EmptyState(title, body)` helper (centered title, wrapped body), reused below.

#### State B — Create Project

Same `ProjectConfigureMenu` data, reordered as steps with the check next to each field:

1. **Game** — combo. Fills the data directory if it is empty (existing rule).
2. **Game data directory** — path + Select / Use Steam Install. ✓ when the game's exe is found, otherwise the
   missing-exe warning right under the field.
3. **Project directory** — path + Select. Offer "Create folder `<Default Mod Dir>\<Name>`" when
   `Project_Default_Mod_Directory` is set. Project-inside-game-dir and duplicate-path warnings under the field.
4. **Name** — defaults to the project folder's name when left empty.
5. **Advanced** (collapsed): group tag, editor toggles.

One primary button, **Create and Load**, enabled when all four steps pass; when disabled, its tooltip names the first
failing step. **Cancel** returns to the previous state. Per-field status replaces the block of problems at the
bottom (`DisplayProjectActions`).

#### State C — Open a Project

`GUI.EmptyState`: "Select a project on the left, or double-click to load it." If an auto-load project exists but
isn't loaded (auto-load off, or it failed), a **Load `<name>`** button. Plus **New Project**.

#### State D — Selected project

```
┌ Detail ────────────────────────────────────────────────────────────────┐
│ My ER Mod · Elden Ring · Loaded and selected        [Unload] [Reload]  │
│ Project: D:\Mods\MyERMod 📂   Data: …\ELDEN RING\Game 📂               │
│ Unsaved: 2 alias types, enums                                          │
│ ┌ Overview ┬ Aliases ┬ Enums ┐                                        │
│ │ …                                                                    │
└────────────────────────────────────────────────────────────────────────┘
```

- **Header card** (always visible above the tabs, so every tab says which project it edits): name, game, status
  (*Loaded and selected* / *Loaded* / *Not loaded*), project and data paths with Open-folder icons, and the unsaved
  alias/enum note from `UnsavedAliasTypes` / `HasUnsavedEnums`. Primary action by status: **Load**, **Switch to**,
  or **Unload / Reload**.
- **Overview tab:**
  - **Settings** (collapsible, open by default): name, game (read-only, decision 4), directories, group tag —
    the current Edit form with Update / Revert and the "Unsaved changes" note directly above them.
  - **Editors** (collapsible): the toggles, with "changes take effect on reload".
  - **Mod Engine 3** (only for ME3 projects): Launch / Create Profile.
  - **Maintenance** (collapsed, at the bottom): Delete Backup Files, Delete Timestamped Backups, then
    **Remove Project** in the warning colour. Separated from Update so it can't be mis-clicked.
- **Aliases** and **Enums** tabs: see §2.2.

#### Focus after load

Stop pulling focus to the Project Editor after a load; focus the project's first editor instead
(`Handler.FocusedEditor`), unless the load was started from the Get Started / Create flow, where the overview is
the confirmation (decision 3).

### 2.2 Aliases and Enums tabs

The two editors keep their code (`ProjectAliasMenu`, `ProjectEnumMenu`, the edit actions); only where they are
drawn and who owns them changes.

- **Ownership:** `ProjectScreen` owns `AliasMenu` and `EnumMenu` (`Smithbox.Orchestrator.ProjectEditor.AliasMenu`).
  Every reference in the table in §1 moves to that path.
- **Availability:** the tabs edit the loaded data of `Orchestrator.SelectedProject`. The tabs are always shown;
  when the sidebar selection isn't that project, the tab body is a `GUI.EmptyState` with the fix as a button:
  - not loaded → "Load this project to edit its aliases." + **Load**;
  - loaded but not selected → "Switch to this project to edit its aliases." + **Switch to**;
  - loading → "Loading `<name>`…".
  This single guard, in `ProjectScreen`, replaces the five checks at the top of each menu's `Display`. It reads
  `Orchestrator.SelectedProject` and `IsActive`, not `SelectedLoadedEntry`.
- **Menu bars:** each tab body is a child window with `ImGuiWindowFlags.MenuBar`, so the menus' existing
  `BeginMenuBar` code (File / Edit / Data Source / Options) works unchanged.
- **Aliases:** the type list shows a count per type and a `*` on unsaved types. Before a type is picked, the right
  pane explains what aliases are and where they show. The active data sources (`Base` / `Project`) are shown as
  badges next to the filter, not only as menu ticks. File → Save says where it writes
  (`<project>\.smithbox\Assets\Aliases\<Type>.json`) in its tooltip.
- **Enums:** when the Param Editor is disabled, the hint gets an **Enable the Param Editor** button that switches
  to the Overview tab and opens its Editors section.
- **Shortcuts and focus:** hovering a tab body sets the focus context; `ProjectScreen.Shortcuts` calls
  `AliasMenu.Shortcuts()` / `EnumMenu.Shortcuts()`. The contexts are renamed `Project_AliasEditor` /
  `Project_EnumEditor` and `IsInProjectEditor` includes them; the other `Metadata_*` contexts and
  `IsInProjectMetadataEditor` go.

### 2.3 Removing the Project Metadata Editor

- `ProjectOrchestrator` no longer creates or draws it; `GUI.DockGroup_ProjectMetadataEditor` / `…View` go.
- Moved to `.archive` (per CLAUDE.md, not deleted): `ProjectMetadataScreen.cs`, `Framework/MetadataSelection.cs`,
  `Enums/MetadataEditorMode.cs`, `GUI/ParamDefMenu.cs`, `GUI/ParamMetaMenu.cs`.
- The remaining files move from `Editors/Project Metadata Editor/` to `Editors/Project Editor/Metadata/`
  (Actions, Enums, `ProjectAliasMenu`, `ProjectEnumMenu`). The namespace `StudioCore.Editors.MetadataEditor` is kept,
  so the 23 files that import it are untouched.
- Localization keys used only by archived files (`PROJECT_Window_Project_Metadata_*`, `META_Mode_*`, `META_Window_*`,
  `META_Header_Param_*`, `META_Editor_WIP`, the two `*_Error_*_Project` pairs) are removed from the English JSON.
- The saved `###ProjectMetadataEditor` window in users' `imgui.ini` is dropped by ImGui, as in decision 5.

### 2.4 Cross-editor navigation

A small API on `ProjectScreen` so other editors can deep-link: `ShowCreate(ProjectType? type = null)`,
`ShowProject(ProjectEntry, ProjectDetailTab tab, section = null)`. Each sets the detail state and
`RequestFocus = true`. Used by the Enums "Enable the Param Editor" button, and available to e.g. the Model
Editor's alias actions for an "Edit in Project Editor" link.

---

## 3. Work phases

Each phase builds and can be checked on its own.

1. **Shared pieces** — `GUI.EmptyState`, the `ProjectScreen` detail-state and tab enums, `ShowCreate` /
   `ShowProject`. Localization file `Project-GUI-Overview.json` for the new keys.
2. **Project Editor layout** — sidebar + detail pane in one window; remove the Active/Available/Configuration
   windows (their config toggles in the View menu go with them; Tools stays). States A and C.
3. **Create form** — steps, per-field checks, Create and Load, detected-game shortcuts, default name/folder.
4. **Project Overview** — header card, Overview tab sections, Maintenance separation, focus-after-load change,
   read-only game type.
5. **Aliases and Enums tabs** — move ownership to `ProjectScreen`, draw them as tabs with the shared guard,
   update the references in §1, focus contexts and shortcuts, alias/enum hints.
6. **Remove the Metadata Editor** — orchestrator and `GUI` cleanup, archive the five files, move the folder,
   prune localization keys.
7. **Docs** — add a status table here and update the checklist below.

Files touched: `ProjectScreen.cs`, `ProjectConfigureMenu.cs` (split into `ProjectCreateView` and
`ProjectOverviewView` if it grows past ~800 lines), `ProjectOrchestrator.cs`, `ProjectEntry.cs`,
`ProjectDescriptor.cs`, `ProjectAliasMenu.cs`, `ProjectEnumMenu.cs`, `ModelSourceList.cs`, `FocusManager.cs`,
`GUI.cs`, `CFG.cs` (drop the three Project Editor window toggles), English localization JSON.

---

## 4. Decisions

1. **Project Editor layout** — **Decided (2026-11-06): single window, sidebar + detail pane** (§2.1). Rejected:
   keeping the docked windows with state-aware panels; keeping tabs with empty states only.
2. **Param Editor mode** — **Decided (2026-11-06): hidden until ParamDef/Meta editing exists.** Superseded in its
   details by decision 6: with the Metadata Editor removed, `MetadataEditorMode`, `ParamDefMenu` and
   `ParamMetaMenu` are archived rather than kept unreachable. ParamDef/Meta editing, when built, gets its own place.
3. **Focus after load** — **Decided (2026-11-06): focus the project's first editor, except after Create**, where
   the Project Editor stays in front as confirmation. Implementation: `LoadProject` takes a "focus project
   editor" flag (set by `CreateProject`); otherwise `FinishedProjectLoad` focuses `Handler.FocusedEditor` instead of
   setting `ProjectEditor.RequestFocus`.
4. **Game type after creation** — **Decided (2026-11-06): read-only in the overview**, with a note that a
   different game needs a new project. `ProjectDescriptor.ApplyConfiguration` (line 152) stops copying
   `ProjectType`, so the rule holds even if another path edits the form.
5. **Existing layouts** — **Decided (2026-11-06): let ImGui drop the removed windows.** No migration code; the
   new content draws inside the existing `###ProjectEditor` window, and the Tools window keeps its ID so its saved
   placement survives.
6. **Metadata Editor** — **Decided (2026-11-06): removed; Aliases and Enums become tabs of the selected project
   in the detail pane** (§2.2, §2.3). Rejected: sidebar sub-items under the open project; separate dockable
   windows inside the Project Editor.

All decisions are made.

---

## 4a. Implementation status (2026-11-06)

Phases 1–6 are implemented and `Smithbox.Program` builds with no new warnings. Not yet checked in the app.

| Piece | Where |
|---|---|
| `GUI.EmptyState`, `GUI.ExplainedButton` (disabled button whose tooltip still shows) | `Interface/GUI.cs` |
| Layout, states, tabs, guard for loaded-data tabs, navigation (`ShowCreate`, `ShowProject`, `SelectEntry`, `OnProjectSelected/Created/Removed`), load/switch/reload/unload | `Project Editor/ProjectScreen.cs` |
| Sidebar | `Project Editor/GUI/ProjectSidebar.cs` |
| Create form | `Project Editor/GUI/ProjectCreateView.cs` |
| Header card and Overview tab | `Project Editor/GUI/ProjectOverviewView.cs` |
| Form state, per-field checks and warnings, field widgets, actions | `Project Editor/GUI/ProjectConfigureMenu.cs` |
| Focus after load (`LoadFocus`), `ExistingProjectsRead` | `ProjectOrchestrator.cs`, `ProjectEditorHandler.FocusEditorWindow` |
| Game type fixed | `ProjectDescriptor.ApplyConfiguration` / `ConfigurationEquals` |
| Alias/enum editors moved to `Project Editor/Metadata/`, guards removed, counts, `*` unsaved, source line, save-path tooltip | `ProjectAliasMenu.cs`, `ProjectEnumMenu.cs` |
| Focus contexts `Project_AliasEditor` / `Project_EnumEditor` | `FocusManager.cs` |
| Archived | `.archive/Project Metadata Editor removal/` (screen, selection, mode enum, ParamDef/Meta menus, three English localization files); empty folder in `.archive/stray-empty-folders/` |
| Removed settings | `CFG.Interface_ProjectEditor_ProjectList / _ProjectConfiguration / _Tools` |
| Localization | new `Project-GUI-Overview.json`; unused keys pruned from `Project-GUI*.json` and the alias/enum menu files; added the missing `PROJECT_Tools_View_Project_Merge` |

Differences from the plan above:

- **Tools is a fourth tab** (Overview · Aliases · Enums · Tools) instead of a separate dockable window. A second window docked into the same node would have been a hidden tab again; as a tab it gets the same "load / switch to this project" guard as Aliases and Enums.
- **Create steps are Game → Data → Name → Project directory.** Name comes before the folder so "Create Folder in Default Mod Directory" (shown when that preference is set) can name the folder after the project. An empty name still takes the folder's name.
- **Focusing the first editor** uses the editor stubs' window names (`ProjectEditorHandler.FocusEditorWindow`), not the command queue, as the screens don't expect an endpoint-only command. Reloads (from the header, context menu or Update) keep the Project Editor in front.
- The header card replaces the planned context bar, as planned in §2.2; the "Sources: Base, Project" line sits in the Aliases tab's filter row.

---

## 5. What to check (manual, after implementation)

- Fresh install (no project JSONs): Project Editor opens on *Get Started*; detected games are listed; one click
  fills type and data dir in the Create form.
- Create form: each step shows its own ✓ or warning; Create and Load is disabled with a tooltip naming the first
  problem; after creating, the overview shows the project as loaded.
- Projects exist, none loaded: *Open a Project* panel; double-click in the sidebar loads.
- Overview: status and primary action match the project (not loaded / loaded / selected). Remove Project is only
  under Maintenance. The game can't be changed.
- The Project Metadata Editor tab no longer appears in the main editor tabs, and no errors are logged about it.
- Aliases / Enums tabs on a not-loaded project and on a loaded-but-not-selected one: the matching message and
  button; pressing it loads / switches and the tab then shows the editor.
- Aliases on the selected project: the header card names it; editing an alias marks its type `*` and the header's
  unsaved note; saving clears both. Undo/redo shortcuts work while hovering the tab.
- Switch to another project: the alias/enum undo history is cleared (as today).
- Unsaved-changes prompt on exit still lists and saves aliases and enums.
- Model Editor → source list → Update Alias still saves the alias file.
- Param Editor disabled on the project: Enums shows the enable button, which opens the Overview's Editors section.
