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

public class EzStateGroupView
{
    public Project Project;
    public EzStateEditor Editor;
    public EzStateGroupView(Project curProject, EzStateEditor editor)
    {
        Project = curProject;
        Editor = editor;
    }

    public void Draw()
    {
        if (Editor.Selection.SelectedScript == null)
            return;

        Editor.EditorFocus.SetFocusContext(EzStateEditorContext.StateGroup);

        Editor.Filters.DisplayStateGroupFilterSearch();

        ImGui.BeginChild("GroupListSection");

        ImGuiListClipper clipper = new ImGuiListClipper();
        clipper.Begin(Editor.Selection.SelectedScript.StateGroups.Count);

        while (clipper.Step())
        {
            for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
            {
                var curEntry = Editor.Selection.SelectedScript.StateGroups.ElementAt(i);

                var stateId = curEntry.Key;
                var stateGroups = curEntry.Value;

                var displayName = $"{curEntry.Key}";
                var aliasName = displayName;

                var isSelected = Editor.Selection.IsStateGroupSelected(i);

                if (ImGui.Selectable($"[{i}]:{displayName}##stateGroupEntry{i}", isSelected))
                {
                    Editor.Selection.SelectStateGroup(i, stateGroups);
                }

                // Arrow Selection
                if (ImGui.IsItemHovered() && Editor.Selection.SelectNextStateGroup)
                {
                    Editor.Selection.SelectNextStateGroup = false;
                    Editor.Selection.SelectStateGroup(i, stateGroups);
                }
                if (ImGui.IsItemFocused() && (InputTracker.GetKey(Veldrid.Key.Up) || InputTracker.GetKey(Veldrid.Key.Down)))
                {
                    Editor.Selection.SelectNextStateGroup = true;
                }

                UIHelper.DisplayAlias(aliasName);
            }
        }

        ImGui.EndChild();
    }
}
