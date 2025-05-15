using Hexa.NET.ImGui;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Interface;

namespace StudioCore.EzStateEditorNS;

/// <summary>
/// Handles the state group selection, viewing and editing.
/// </summary>
public class EsdStateGroupView
{
    public EsdEditorScreen Editor;
    public ProjectEntry Project;

    public EsdStateGroupView(EsdEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    /// <summary>
    /// Reset view state on project change
    /// </summary>
    public void OnProjectChanged()
    {

    }

    /// <summary>
    /// The main UI for the view
    /// </summary>
    public void Display()
    {
        ImGui.Begin("State Group Selection##EsdStateGroupSelectView");
        Editor.Selection.SwitchWindowContext(EsdEditorContext.StateGroup);

        var script = Editor.Selection.SelectedScript;
        var stateGroupKey = Editor.Selection.SelectedGroupIndex;

        Editor.Filters.DisplayStateGroupFilterSearch();

        ImGui.BeginChild("StateGroupSection");
        Editor.Selection.SwitchWindowContext(EsdEditorContext.StateGroup);

        if (script != null)
        {
            foreach (var entry in script.StateGroups)
            {
                var stateId = entry.Key;
                var stateGroups = entry.Value;

                var displayName = $"{entry.Key}";
                var aliasName = displayName;

                if (Editor.Filters.IsStateGroupFilterMatch(displayName, aliasName))
                {
                    if (ImGui.Selectable($@" {stateId}", stateGroupKey == stateId))
                    {
                        Editor.Selection.ResetStateGroupNode();

                        Editor.Selection.SetStateGroup(stateId, stateGroups);
                    }

                    // Arrow Selection
                    if (ImGui.IsItemHovered() && Editor.Selection.SelectNextGroup)
                    {
                        Editor.Selection.SelectNextGroup = false;
                        Editor.Selection.SetStateGroup(stateId, stateGroups);
                    }
                    if (ImGui.IsItemFocused() && (InputTracker.GetKey(Veldrid.Key.Up) || InputTracker.GetKey(Veldrid.Key.Down)))
                    {
                        Editor.Selection.SelectNextGroup = true;
                    }

                    // Only apply to selection
                    if (Editor.Selection.SelectedGroupIndex != -1)
                    {
                        if (Editor.Selection.SelectedGroupIndex == entry.Key)
                        {
                            Editor.ContextMenu.StateGroupContextMenu(entry);
                        }
                    }

                    UIHelper.DisplayAlias(aliasName);
                }
            }
        }

        ImGui.EndChild();

        ImGui.End();
    }
}
