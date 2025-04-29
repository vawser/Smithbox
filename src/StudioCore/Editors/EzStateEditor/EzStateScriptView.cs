using Hexa.NET.ImGui;
using HKLib.hk2018.hkaiCollisionAvoidance;
using StudioCore.Configuration;
using StudioCore.Core.ProjectNS;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.EzStateEditorNS;

public class EzStateScriptView
{
    public Project Project;
    public EzStateEditor Editor;

    public EzStateScriptView(Project curProject, EzStateEditor editor)
    {
        Project = curProject;
        Editor = editor;
    }

    public void Draw()
    {
        if (Editor.Selection.SelectedFileIndex == -1)
            return;

        if (!Project.EzStateData.PrimaryBank.Binders.ContainsKey(Editor.Selection.SelectedFilename))
            return;

        Editor.EditorFocus.SetFocusContext(EzStateEditorContext.Script);

        var binder = Project.EzStateData.PrimaryBank.Binders[Editor.Selection.SelectedFilename];

        Editor.Filters.DisplayScriptFilterSearch();

        ImGui.BeginChild("ScriptListSection");

        ImGuiListClipper clipper = new ImGuiListClipper();
        clipper.Begin(binder.Files.Count);

        while (clipper.Step())
        {
            for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
            {
                var curEntry = binder.Files[i];

                var isSelected = Editor.Selection.IsInternalFileSelected(i, curEntry.Name);

                var displayName = $"{curEntry.Name}";
                var aliasName = displayName;

                if (ImGui.Selectable($"[{i}]:{displayName}##internalFileEntry{i}", isSelected))
                {
                    Editor.Selection.SelectScript(i, curEntry.Name, curEntry);
                }

                // Arrow Selection
                if (ImGui.IsItemHovered() && Editor.Selection.SelectNextScript)
                {
                    Editor.Selection.SelectNextScript = false;
                    Editor.Selection.SelectScript(i, curEntry.Name, curEntry);
                }
                if (ImGui.IsItemFocused() && (InputTracker.GetKey(Veldrid.Key.Up) || InputTracker.GetKey(Veldrid.Key.Down)))
                {
                    Editor.Selection.SelectNextScript = true;
                }

                UIHelper.DisplayAlias(aliasName);
            }
        }

        ImGui.EndChild();
    }
}
