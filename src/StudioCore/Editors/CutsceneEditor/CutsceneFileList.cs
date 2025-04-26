using Hexa.NET.ImGui;
using StudioCore.Core.ProjectNS;

namespace StudioCore.Editors.CutsceneEditorNS;

public class CutsceneFileList
{
    public Project Project;
    public CutsceneEditor Editor;

    public CutsceneFileList(Project curProject, CutsceneEditor editor)
    {
        Project = curProject;
        Editor = editor;
    }

    public void Draw()
    {
        var entries = Project.CutsceneData.CutsceneFiles.Entries;

        if (entries.Count > 0)
        {
            ImGuiListClipper clipper = new ImGuiListClipper();
            clipper.Begin(entries.Count);

            while (clipper.Step())
            {
                for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
                {
                    var curEntry = entries[i];

                    var isSelected = Editor.Selection.IsFileSelected(i, curEntry.Filename);

                    var displayName = $"{curEntry.Filename}";

                    if (ImGui.Selectable($"{displayName}##fileEntry{i}", isSelected))
                    {
                        Editor.Selection.SelectFile(i, curEntry.Filename);

                        Project.CutsceneData.PrimaryBank.LoadCutscene(curEntry.Filename, curEntry.Path);
                    }
                }
            }
        }
    }
}
