# Unsaved Changes Prompt (app-wide)

Origin: Havok Editor Review, item M11. Unsaved edits are lost silently when Smithbox closes or a project is
unloaded. This adds one prompt that covers every editor.

## Current state

- `Smithbox.Run()` loops `while (_context.Window.Exists)`. SDL `Quit` and the window's `Close` event both call
  `Sdl2Window.Close()` directly (`Veldrid.SDL2/Sdl2Window.cs`, `HandleEvent` / `HandleWindowEvent`), which destroys the
  window. The `Closing` event fires after the decision is made, so it can't cancel.
- Shutdown (`Orchestrator.Exit()`, `Smithbox.Exit()`) only writes project descriptors and config.
- No ImGui platform windows are created (the renderers only set clipboard callbacks), so the main SDL window is the
  only window that can be closed.
- Several projects can be active at once. Selecting another active project keeps the others loaded, so switching
  loses nothing. Edits are lost on: **unload**, **reload** (context menu, and `UpdateProject` when the configuration of
  an active project changes), **remove**, and **exit**.

## Dirty state per editor

| Editor | Dirty marker | Save-all path | Notes |
|---|---|---|---|
| Map | `MapContainer.HasUnsavedChanges`; view tools `LoadListTool` / `EntryFileListTool` / `PartsTokenIdListTool` modified files | `MapEditorScreen.Save()` (saves all maps + each view's tools) | Needs an active view; throws `SavingFailedException` |
| Model | `ModelBank.Models[*].Models[*].IsDirty` | `ModelBank.SaveAllModels()` | DES can't save |
| Time Act | `TimeActContainerWrapper.HasUnsavedTimeActs` | `TimeActEditorScreen.SaveAll()` | |
| Param | `ParamBank.IsDirty()` (primary bank) | `ParamEditorScreen.Save()` — `async void`, writes on a worker thread; `IsSaving` | |
| Text | `TextBank.Containers[*].IsDirty()` | `TextBank.SaveTextFiles()` | |
| Gparam | `GparamBank.Entries[*].IsDirty` | `GparamBank.SaveAllGraphicsParams()` — async | |
| Material | `MTDWrapper.IsDirty()` / `MATBINWrapper.IsDirty()` | **none**: only the selected file of the active view can be saved | Needs a new view-independent save |
| Havok | `HavokBank` file wrappers `IsDirty` | `HavokBank.SaveAllHavokFiles()` | `SaveAll()` needlessly needs an active view |
| Texture Viewer, File Browser | read-only | — | |

## Design

### 1. Cancelable window close (vendored Veldrid)

Add to `Sdl2Window`:

```csharp
/// Asked before the window closes from a user request (SDL Quit or the window's close button).
/// Return false to keep the window open.
public Func<bool> CloseRequested;
```

`HandleEvent` (Quit) and `HandleWindowEvent` (Close) call `RequestClose()`, which closes only if the handler is null or
returns true. `AppTerminating` (the OS killing the app) still closes unconditionally, as does a direct `Close()` call.

`Smithbox` sets `CloseRequested = () => UnsavedChanges.RequestExit()`. With nothing unsaved it returns true and exit
is as now; otherwise it opens the modal and returns false.

### 2. Per-editor reporting

New optional interface members on `EditorScreen` (default: nothing unsaved):

```csharp
/// The files with unsaved changes, for the unsaved changes prompt
IEnumerable<string> GetUnsavedItems() => [];

/// Starts saving everything with unsaved changes. Saves may finish on later frames.
void SaveUnsaved() { }

/// True while a save started by SaveUnsaved is still running
bool IsSavingUnsaved => false;
```

Each editor implements these from its existing markers (table above). Material gains a `MaterialBank.SaveAllDirty()`
that writes each dirty file through its own wrapper, without a view.

### 3. One modal — `UnsavedChangesPrompt` (new, `Editors/Common` or `Core`)

- `RequestExit()` / `RequestProjectAction(ProjectEntry, Action onProceed)` gather the unsaved items of the affected
  projects (all active projects for exit, one project otherwise). Nothing unsaved → run immediately.
- Modal (drawn each frame from `ProjectOrchestrator.Update`): lists project → editor → files (files under a tree node),
  with **Save All**, **Discard** (Exit without saving / Unload without saving), **Cancel**.
- **Save All**: calls `SaveUnsaved()` on each listed editor, then shows "Saving..." until no editor reports
  `IsSavingUnsaved`. Then it re-gathers: nothing left → proceed (`Window.Close()` or the project action); anything left
  (a save failed) → the modal stays open listing what is still unsaved, so nothing is lost silently.

### 4. Hook points

- Exit: `Sdl2Window.CloseRequested`.
- `ProjectScreen` context menu: Reload, Unload.
- `ProjectConfigureMenu` → `UpdateProject` when the project is active (reload).
- `ProjectUtils.DeleteProject` → `RemoveProject`.
- Selecting another project: no prompt (nothing is lost).

### Localization

New keys in a new `Smithbox.Data/Assets/Localization/English/` file for the modal (title, buttons, "Saving...",
"still unsaved" message).

## Out of scope / follow-ups

- Selecting another project in the Project list resets the configuration form, dropping its unapplied edits, without
  a prompt (switching never prompts).
- The Map Editor's native `MessageBox` prompt for unloading maps / closing a view stays as it is.

## Implemented

- `Sdl2Window.CloseRequested` (vendored Veldrid) → `Smithbox.HandleCloseRequest` (after `Update`) →
  `ProjectOrchestrator.RequestExit`.
- `EditorScreen.GetUnsavedFiles / SaveUnsaved / IsSavingUnsaved`, implemented by Map, Model, Time Act, Param, Text,
  Gparam, Material, Havok. `ProjectEntry.GetEditors / GetUnsavedChanges / IsSavingUnsaved`.
- `UnsavedChangesPrompt` (`Editors/Project Editor`), drawn from `ProjectOrchestrator.Update`.
- `MaterialBank.GetDirtyFiles / SaveAllDirty` + `MaterialBinderSave` (view-independent save).
- `HavokBank.GetDirtyFiles`; Havok `SaveUnsaved` saves from the bank without an active view.
- Hooks: ProjectScreen Reload / Unload, ProjectConfigureMenu Update (active project) / Delete.
- Loc: `Project-GUI-Unsaved-Changes.json`.

## What to check

1. No edits → close the window: closes at once, no prompt.
2. Edit a param, a text entry and a map object → close: the prompt lists Param / Text / Map Editor with their files.
   **Cancel** keeps Smithbox open with the edits; **Exit Without Saving** closes; **Save All** shows "Saving...",
   then closes, and the files are written (reopen to confirm).
3. Gparam and Material edit → Save All: saved (Material saves files not selected in any view too).
4. Havok edit with the Havok Editor never focused → Save All writes it.
5. Unload / Reload a project from the Project list context menu with edits: the prompt appears for that project only;
   Cancel leaves it loaded. With two active projects, exit lists both under project names.
6. Changing an active project's configuration and pressing Update with edits: the prompt appears before the reload.
7. Failure path: make a save fail (e.g. a read-only project file) → Save All → the prompt reappears with the warning
   and lists only what's still unsaved.
8. Escape while the prompt is open acts as Cancel.
9. Project Metadata editor: edit an alias type and an enum, then close. "Project Aliases" lists the alias type and
   "Project Param Enums" appears. Save All writes them to `<project>/.smithbox/Assets/Aliases` and `.../Param Enums`
   (never the base files in the Smithbox source). Saving with Ctrl+S in the metadata editor clears them from the prompt.
10. Configuration form: change a setting without pressing Update, then close (or Unload that project). "Project
    Configuration" is listed; Save All writes the project file without reloading, and the setting is there next launch.
    An invalid form (e.g. a missing data path) isn't applied, so the prompt reappears. Not listed for Reload or Remove.

## Decisions

1. **Triggers**: data-loss only. Exit, unload, reload (including a configuration change that reloads) and remove.
   Switching projects doesn't prompt.
2. **Save failure**: keep the modal open, listing only what is still unsaved; the exit/unload doesn't happen.
3. **Project Metadata**: covered. Edited alias types and enums are tracked per project (`ProjectEntry.UnsavedAliasTypes`,
   `HasUnsavedEnums`); Save All writes them to the project (local) only.
4. **Configuration form**: covered for exit and unload. Save All applies the form and writes the project file without
   reloading (`ProjectConfigureMenu.ApplyWithoutReload`).
