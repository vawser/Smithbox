using Hexa.NET.ImGui;
using StudioCore.Core.ProjectNS;

namespace StudioCore.Editors.CutsceneEditorNS;

public class CutsceneDataView
{
    public Project Project;
    public CutsceneEditor Editor;

    public CutsceneDataView(Project curProject, CutsceneEditor editor)
    {
        Project = curProject;
        Editor = editor;
    }

    public void Draw()
    {
        if (Editor.Selection._selectedCutscene != null)
        {
            if (ImGui.CollapsingHeader("Cuts"))
            {
                ImGuiListClipper clipper = new ImGuiListClipper();
                clipper.Begin(Editor.Selection._selectedCutscene.Cuts.Count);

                while (clipper.Step())
                {
                    for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
                    {
                        var curEntry = Editor.Selection._selectedCutscene.Cuts[i];

                        if (ImGui.Selectable($"[{i}]:{curEntry.Name}##cutEntry{i}"))
                        {

                        }
                    }
                }
            }

            if (ImGui.CollapsingHeader("Resources"))
            {
                ImGuiListClipper clipper = new ImGuiListClipper();
                clipper.Begin(Editor.Selection._selectedCutscene.Resources.Count);

                while (clipper.Step())
                {
                    for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
                    {
                        var curEntry = Editor.Selection._selectedCutscene.Resources[i];

                        if (ImGui.Selectable($"[{i}]:{curEntry.Name}##resourceEntry{i}"))
                        {

                        }
                    }
                }
            }

        }
    }
}
