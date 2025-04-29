using Hexa.NET.ImGui;
using StudioCore.Configuration;
using StudioCore.Core.ProjectNS;
using StudioCore.Editors.EventScriptEditorNS;
using StudioCore.Interface;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.EzStateEditorNS;

public class EzStateFileList
{
    public Project Project;
    public EzStateEditor Editor;

    public EzStateFileList(Project curProject, EzStateEditor editor)
    {
        Project = curProject;
        Editor = editor;
    }

    public void Draw()
    {
        Editor.EditorFocus.SetFocusContext(EzStateEditorContext.File);

        Editor.Filters.DisplayFileFilterSearch();

        ImGui.BeginChild("ezStateFileList");

        var entries = Project.EzStateData.EzStateFiles.Entries;

        if (entries.Count > 0)
        {
            ImGuiListClipper clipper = new ImGuiListClipper();
            clipper.Begin(entries.Count);

            while (clipper.Step())
            {
                for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
                {
                    var curEntry = entries[i];
                    var filename = curEntry.Filename;
                    var isSelected = Editor.Selection.IsFileSelected(i, curEntry.Filename);
                    var displayName = $"{filename}";
                    var aliasName = AliasUtils.GetMapNameAlias(filename);

                    if (!Editor.Filters.IsFileFilterMatch(displayName, aliasName))
                        continue;

                    if (ImGui.Selectable($"{displayName}##fileEntry{i}", isSelected))
                    {
                        Editor.Selection.SelectFile(i, curEntry.Filename);

                        Project.EzStateData.PrimaryBank.LoadBinder(curEntry.Filename, curEntry.Path);
                    }

                    // Arrow Selection
                    if (ImGui.IsItemHovered() && Editor.Selection.SelectNextScript)
                    {
                        Editor.Selection.SelectNextScript = false;
                        Editor.Selection.SelectFile(i, curEntry.Filename);
                    }

                    if (ImGui.IsItemFocused() && (InputTracker.GetKey(Veldrid.Key.Up) || InputTracker.GetKey(Veldrid.Key.Down)))
                    {
                        Editor.Selection.SelectNextScript = true;
                    }

                    UIHelper.DisplayAlias(aliasName);
                }
            }
        }

        ImGui.EndChild();
    }
}
