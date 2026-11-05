using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using System.Numerics;

namespace StudioCore.Application;

/// <summary>
/// What is about to happen that would lose the unsaved changes
/// </summary>
public enum UnsavedChangesAction
{
    Exit,
    Unload,
    Reload,
    Remove
}

/// <summary>
/// An editor of a project with unsaved changes: its name, the files it lists, and how to save them
/// </summary>
public record UnsavedEditor(string Name, List<string> Files, Action Save);

/// <summary>
/// Asks what to do with unsaved changes before Smithbox closes or a project is unloaded, reloaded or removed:
/// save them all, discard them, or cancel. Covers every editor of the affected projects, through
/// <see cref="EditorScreen.GetUnsavedFiles"/> and <see cref="EditorScreen.SaveUnsaved"/>, and the Project Metadata
/// editor's aliases and enums (see <see cref="ProjectEntry.GetUnsavedChanges"/>).
/// </summary>
/// <remarks>
/// A save can finish on a later frame (the Param and Gparam editors write on a worker thread), so after Save All
/// the prompt waits until no editor reports a running save, then checks again. Anything still unsaved means a save
/// failed: the prompt stays open listing it, rather than the action going ahead and losing it.
/// </remarks>
public class UnsavedChangesPrompt
{
    private enum PromptState
    {
        Closed,
        Asking,
        Saving
    }

    private const string PopupId = "##UnsavedChangesPrompt";

    private PromptState _state = PromptState.Closed;

    private UnsavedChangesAction _action;
    private List<ProjectEntry> _projects = new();
    private Action _onProceed;

    private List<(ProjectEntry Project, List<UnsavedEditor> Editors)> _unsaved = new();

    // Set when a Save All left something unsaved
    private bool _saveIncomplete;

    public bool IsOpen => _state != PromptState.Closed;

    /// <summary>
    /// Runs the action straight away if the projects have nothing unsaved; otherwise opens the prompt, which runs it
    /// once the changes are saved or discarded. Ignored while the prompt is already open.
    /// </summary>
    public void Request(UnsavedChangesAction action, IEnumerable<ProjectEntry> projects, Action onProceed)
    {
        if (IsOpen)
            return;

        // Not only the active projects: the configuration form can hold edits of a project that isn't loaded
        var candidates = projects.Where(p => p != null).Distinct().ToList();

        _action = action;

        var unsaved = Gather(candidates);

        if (unsaved.Count == 0)
        {
            onProceed();
            return;
        }

        _projects = candidates;
        _onProceed = onProceed;
        _unsaved = unsaved;
        _saveIncomplete = false;
        _state = PromptState.Asking;
    }

    // Exit and unload are the actions that drop the configuration form's edits (unload resets the form)
    private bool IncludesConfiguration => _action is UnsavedChangesAction.Exit or UnsavedChangesAction.Unload;

    private List<(ProjectEntry, List<UnsavedEditor>)> Gather(List<ProjectEntry> projects)
    {
        var result = new List<(ProjectEntry, List<UnsavedEditor>)>();

        foreach (var project in projects)
        {
            // Editor changes are only listed for active projects, so a project unloaded meanwhile drops out
            var editors = project.GetUnsavedChanges(IncludesConfiguration);

            if (editors.Count > 0)
                result.Add((project, editors));
        }

        return result;
    }

    /// <summary>
    /// Draws the prompt. Called every frame by the orchestrator, outside any other popup.
    /// </summary>
    public void Draw()
    {
        if (_state == PromptState.Closed)
            return;

        var title = $"{LOC.Get("UNSAVED_Header")}{PopupId}";

        if (!ImGui.IsPopupOpen(title))
            ImGui.OpenPopup(title);

        var viewport = ImGui.GetMainViewport();
        ImGui.SetNextWindowPos(viewport.Pos + viewport.Size * 0.5f, ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));

        if (!ImGui.BeginPopupModal(title, ImGuiWindowFlags.AlwaysAutoResize))
        {
            // Closed by something other than the buttons, such as Escape: treated as Cancel, but not mid-save
            if (_state == PromptState.Asking)
                Close(false);

            return;
        }

        if (_state == PromptState.Saving)
        {
            UpdateSaving();
        }

        // UpdateSaving can close the prompt or return it to asking
        if (_state == PromptState.Saving)
        {
            DisplaySaving();
        }
        else if (_state == PromptState.Asking)
        {
            DisplayAsking();
        }

        ImGui.EndPopup();
    }

    private void UpdateSaving()
    {
        if (_projects.Any(p => p.IsActive && p.IsSavingUnsaved()))
            return;

        _unsaved = Gather(_projects);

        if (_unsaved.Count == 0)
        {
            Proceed();
            return;
        }

        _saveIncomplete = true;
        _state = PromptState.Asking;
    }

    private void DisplaySaving()
    {
        ImGui.TextUnformatted(LOC.Get("UNSAVED_Saving"));

        // Keeps the popup the size it was, so it doesn't jump while the saves run
        ImGui.Dummy(new Vector2(500.0f * DPI.UIScale(), 0));
    }

    private void DisplayAsking()
    {
        var wrapWidth = 500.0f * DPI.UIScale();

        ImGui.PushTextWrapPos(wrapWidth);

        ImGui.TextUnformatted(GetActionMessage());

        if (_saveIncomplete)
        {
            ImGui.Spacing();
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("UNSAVED_Save_Incomplete"));
        }

        ImGui.PopTextWrapPos();

        ImGui.Spacing();

        DisplayUnsavedList(wrapWidth);

        ImGui.Spacing();

        var buttonSize = new Vector2(160.0f * DPI.UIScale(), 0);

        if (ImGui.Button($"{LOC.Get("UNSAVED_Action_Save_All")}##unsavedSaveAll", buttonSize))
        {
            SaveAll();
        }
        GUI.Tooltip(LOC.Get("UNSAVED_Action_Save_All_TT"));

        ImGui.SameLine();

        if (ImGui.Button($"{GetDiscardLabel()}##unsavedDiscard", buttonSize))
        {
            Proceed();
        }
        GUI.Tooltip(LOC.Get("UNSAVED_Action_Discard_TT"));

        ImGui.SameLine();

        if (ImGui.Button($"{LOC.Get("UNSAVED_Action_Cancel")}##unsavedCancel", buttonSize))
        {
            Close();
        }
        GUI.Tooltip(LOC.Get("UNSAVED_Action_Cancel_TT"));
    }

    private void DisplayUnsavedList(float width)
    {
        var showProjects = _unsaved.Count > 1;
        var height = Math.Min(300.0f * DPI.UIScale(), ImGui.GetTextLineHeightWithSpacing() * (CountRows() + 1));

        ImGui.BeginChild("##unsavedChangesList", new Vector2(width, height), ImGuiChildFlags.Borders);

        foreach (var (project, editors) in _unsaved)
        {
            if (showProjects)
            {
                ImGui.TextUnformatted(project.Descriptor.ProjectName);
                ImGui.Indent();
            }

            foreach (var (name, files, _) in editors)
            {
                var label = LOC.Get("UNSAVED_Editor_Entry", name, files.Count);

                if (ImGui.TreeNodeEx($"{label}##unsaved_{project.Descriptor.ProjectGUID}_{name}"))
                {
                    foreach (var file in files)
                    {
                        ImGui.TextUnformatted(file);
                    }

                    ImGui.TreePop();
                }
            }

            if (showProjects)
                ImGui.Unindent();
        }

        ImGui.EndChild();
    }

    private int CountRows()
    {
        return _unsaved.Sum(e => e.Editors.Count + (_unsaved.Count > 1 ? 1 : 0));
    }

    private string GetActionMessage()
    {
        var projectName = _projects.Count == 1 ? _projects[0].Descriptor.ProjectName : "";

        return _action switch
        {
            UnsavedChangesAction.Exit => LOC.Get("UNSAVED_Message_Exit"),
            UnsavedChangesAction.Unload => LOC.Get("UNSAVED_Message_Unload", projectName),
            UnsavedChangesAction.Reload => LOC.Get("UNSAVED_Message_Reload", projectName),
            UnsavedChangesAction.Remove => LOC.Get("UNSAVED_Message_Remove", projectName),
            _ => ""
        };
    }

    private string GetDiscardLabel()
    {
        return _action switch
        {
            UnsavedChangesAction.Exit => LOC.Get("UNSAVED_Action_Discard_Exit"),
            UnsavedChangesAction.Unload => LOC.Get("UNSAVED_Action_Discard_Unload"),
            UnsavedChangesAction.Reload => LOC.Get("UNSAVED_Action_Discard_Reload"),
            UnsavedChangesAction.Remove => LOC.Get("UNSAVED_Action_Discard_Remove"),
            _ => LOC.Get("UNSAVED_Action_Discard_Exit")
        };
    }

    private void SaveAll()
    {
        _state = PromptState.Saving;
        _saveIncomplete = false;

        foreach (var (_, editors) in _unsaved)
        {
            foreach (var editor in editors)
            {
                try
                {
                    editor.Save();
                }
                catch (Exception e)
                {
                    // Left unsaved, so the prompt lists it again once the other saves finish
                    Smithbox.LogError(this, LOC.Get("UNSAVED_Save_Failed_Log", editor.Name), e);
                }
            }
        }
    }

    // Called from inside the popup
    private void Proceed()
    {
        var onProceed = _onProceed;

        Close();

        onProceed?.Invoke();
    }

    /// <param name="insidePopup">False when the popup has already been closed by ImGui</param>
    private void Close(bool insidePopup = true)
    {
        if (insidePopup)
            ImGui.CloseCurrentPopup();

        _state = PromptState.Closed;
        _projects = new();
        _unsaved = new();
        _onProceed = null;
        _saveIncomplete = false;
    }
}
