using Hexa.NET.ImGui;
using StudioCore.Configuration;
using StudioCore.Core.ProjectNS;
using StudioCore.Interface;
using StudioCore.Utilities;

namespace StudioCore.Editors.EventScriptEditorNS;

public class EventScriptFileList
{
    public Project Project;
    public EventScriptEditor Editor;

    public EventScriptFileList(Project curProject, EventScriptEditor editor)
    {
        Project = curProject;
        Editor = editor;
    }

    public void Draw()
    {
        Editor.EditorFocus.SetFocusContext(EventScriptEditorContext.File);

        Editor.Filters.DisplayFileFilterSearch();

        ImGui.BeginChild("eventScriptFileList");

        var entries = Project.EventScriptData.EventScriptFiles.Entries;

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

                        Project.EventScriptData.PrimaryBank.LoadEventScript(curEntry.Filename, curEntry.Path);
                    }

                    // Arrow Selection
                    if (ImGui.IsItemHovered() && Editor.Selection.SelectNextScript)
                    {
                        Editor.Selection.SelectNextScript = false;
                        Editor.Selection.SelectFile(i, curEntry.Filename);

                        Project.EventScriptData.PrimaryBank.LoadEventScript(curEntry.Filename, curEntry.Path);
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
