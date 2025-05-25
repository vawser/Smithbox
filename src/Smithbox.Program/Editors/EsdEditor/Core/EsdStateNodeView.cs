using Hexa.NET.ImGui;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Interface;

namespace StudioCore.EzStateEditorNS;

/// <summary>
/// Handles the state node selection, viewing and editing.
/// </summary>
public class EsdStateNodeView
{
    public EsdEditorScreen Editor;
    public ProjectEntry Project;

    public EsdStateNodeView(EsdEditorScreen editor, ProjectEntry project)
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
        ImGui.Begin("State Node Selection##EsdStateNodeSelectView");
        Editor.Selection.SwitchWindowContext(EsdEditorContext.StateNode);

        var stateGroups = Editor.Selection.SelectedGroup;
        var stateNodeKey = Editor.Selection.SelectNodeIndex;

        Editor.Filters.DisplayStateFilterSearch();

        ImGui.BeginChild("StateNodeSection");
        Editor.Selection.SwitchWindowContext(EsdEditorContext.StateNode);

        if (stateGroups != null)
        {
            foreach (var (key, entry) in stateGroups)
            {
                var displayName = $"{key}";
                var aliasName = displayName;

                if (Editor.Filters.IsStateFilterMatch(displayName, aliasName))
                {
                    if (ImGui.Selectable($@" {key}", key == stateNodeKey))
                    {
                        Editor.Selection.SetStateGroupNode(key, entry);
                    }

                    // Arrow Selection
                    if (ImGui.IsItemHovered() && Editor.Selection.SelectNextNode)
                    {
                        Editor.Selection.SelectNextNode = false;
                        Editor.Selection.SetStateGroupNode(key, entry);
                    }
                    if (ImGui.IsItemFocused() && (InputTracker.GetKey(Veldrid.Key.Up) || InputTracker.GetKey(Veldrid.Key.Down)))
                    {
                        Editor.Selection.SelectNextNode = true;
                    }

                    // Only apply to selection
                    if (Editor.Selection.SelectNodeIndex != -1)
                    {
                        if (Editor.Selection.SelectNodeIndex == key)
                        {
                            Editor.ContextMenu.StateNodeContextMenu(entry);
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
