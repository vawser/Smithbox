using Hexa.NET.ImGui;
using StudioCore.Core.ProjectNS;

namespace StudioCore.Editors.EventScriptEditorNS;

public class EventScriptDataView
{
    public Project Project;
    public EventScriptEditor Editor;

    public EventScriptDataView(Project curProject, EventScriptEditor editor)
    {
        Project = curProject;
        Editor = editor;
    }

    public void Draw()
    {
        if (Editor.Selection.SelectedScript != null)
        {
            if (ImGui.CollapsingHeader("Events"))
            {
                ImGuiListClipper clipper = new ImGuiListClipper();
                clipper.Begin(Editor.Selection.SelectedScript.Events.Count);

                while (clipper.Step())
                {
                    for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
                    {
                        var curEntry = Editor.Selection.SelectedScript.Events[i];

                        if (ImGui.Selectable($"[{i}]:{curEntry.Name}##eventEntry{i}"))
                        {

                        }
                    }
                }
            }
        }
    }
}
