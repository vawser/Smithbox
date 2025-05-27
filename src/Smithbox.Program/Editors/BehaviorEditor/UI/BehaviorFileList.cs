using Hexa.NET.ImGui;
using StudioCore.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BehaviorEditorNS;

public class BehaviorFileList
{
    public BehaviorEditorScreen Editor;
    public ProjectEntry Project;

    public BehaviorFileList(BehaviorEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }
    public void OnGui()
    {
        DisplayFileList();
    }
    private void DisplayFileList()
    {
        if (Editor.Selection.SelectedBinderContents == null)
            return;

        ImGui.BeginChild("behaviorBinderFileList");

        for (int i = 0; i < Editor.Selection.SelectedBinderContents.Files.Count; i++)
        {
            var curEntry = Editor.Selection.SelectedBinderContents.Files[i];
            var displayName = BehaviorUtils.GetInternalFileTitle(curEntry.Name);

            var isSelected = Editor.Selection.IsFileSelected(curEntry);

            if (ImGui.Selectable($"{displayName}##internalFileEntry{i}", isSelected))
            {
                Editor.Selection.SelectFile(i, curEntry);
            }
        }

        ImGui.EndChild();
    }
}
