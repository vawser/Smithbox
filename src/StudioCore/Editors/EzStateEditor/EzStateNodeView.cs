using Hexa.NET.ImGui;
using StudioCore.Configuration;
using StudioCore.Core.ProjectNS;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.EzStateEditorNS;

public class EzStateNodeView
{
    public Project Project;
    public EzStateEditor Editor;
    public EzStateNodeView(Project curProject, EzStateEditor editor)
    {
        Project = curProject;
        Editor = editor;
    }

    public void Draw()
    {
        if (Editor.Selection.SelectedStateGroups == null)
            return;

        Editor.EditorFocus.SetFocusContext(EzStateEditorContext.StateNode);

        Editor.Filters.DisplayStateGroupFilterSearch();

        ImGui.BeginChild("NodeListSection");

        ImGuiListClipper clipper = new ImGuiListClipper();
        clipper.Begin(Editor.Selection.SelectedStateGroups.Count);

        while (clipper.Step())
        {
            for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
            {
                var curEntry = Editor.Selection.SelectedStateGroups.ElementAt(i);

                var displayName = $"{curEntry.Key}";
                var aliasName = displayName;

                var isSelected = Editor.Selection.IsStateGroupNodeSelected(i);

                if (ImGui.Selectable($"[{i}]:{displayName}##stateNodeEntry{i}", isSelected))
                {
                    Editor.Selection.SelectStateGroupNode(i, curEntry.Value);
                }

                // Arrow Selection
                if (ImGui.IsItemHovered() && Editor.Selection.SelectNextStateNode)
                {
                    Editor.Selection.SelectNextStateNode = false;
                    Editor.Selection.SelectStateGroupNode(i, curEntry.Value);
                }
                if (ImGui.IsItemFocused() && (InputTracker.GetKey(Veldrid.Key.Up) || InputTracker.GetKey(Veldrid.Key.Down)))
                {
                    Editor.Selection.SelectNextStateNode = true;
                }

                UIHelper.DisplayAlias(aliasName);
            }
        }

        ImGui.EndChild();
    }
}
