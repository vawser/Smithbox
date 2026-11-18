using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System.Numerics;

namespace StudioCore.Editors.FaceGenEditor;

public class FaceGenEditorStub : IEditorStub
{
    public ProjectEntry Project;

    public FaceGenEditorStub(ProjectEntry project)
    {
        Project = project;
    }

    public string EditorName = "FaceGen Editor";

    public string CommandEndpoint = "facegen";

    public void Display(float dt, string[] commands)
    {
        if (!Project.Descriptor.EnableFaceGenEditor)
            return;

        if (!EditorConditions.SupportsFaceGenEditor(Project.Descriptor.ProjectType))
            return;

        if (commands != null && commands.Length > 0 && commands[0] == CommandEndpoint)
        {
            commands = commands[1..];
            ImGui.SetNextWindowFocus();
        }

        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0.0f, 0.0f));

        if (EditorHost.Begin($"{LOC.Get("FACEGEN_Window_FaceGen_Editor")}###faceGenEditor", ImGuiWindowFlags.MenuBar))
        {
            ImGui.PopStyleVar(1);

            if (Project.Handler.FaceGenEditor != null)
            {
                Project.Handler.FaceGenEditor.OnGUI(commands);
            }
            else
            {
                GUI.Spacer();
                ImGui.Text(LOC.Get("EDITOR_Editor_Is_Loading"));
            }

            ImGui.End();

            if (Project.Handler.FaceGenEditor != null)
                Project.Handler.FocusedEditor = Project.Handler.FaceGenEditor;
        }
        else
        {
            ImGui.PopStyleVar(1);
            ImGui.End();
        }
    }
}
