using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;

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

    public unsafe void Display(float dt, string[] commands)
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

        // The viewport draws behind the window, so the window is see-through where it is
        if (Smithbox.Instance._context.Device == null)
        {
            ImGui.PushStyleColor(ImGuiCol.WindowBg, *ImGui.GetStyleColorVec4(ImGuiCol.WindowBg));
        }
        else
        {
            ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(0.0f, 0.0f, 0.0f, 0.0f));
        }

        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0.0f, 0.0f));

        if (EditorHost.Begin($"{LOC.Get("FACEGEN_Window_FaceGen_Editor")}###faceGenEditor", ImGuiWindowFlags.MenuBar))
        {
            ImGui.PopStyleColor(1);
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
            {
                Project.Handler.FocusedEditor = Project.Handler.FaceGenEditor;
                Project.Handler.FaceGenEditor.Update(dt);
            }
        }
        else
        {
            ImGui.PopStyleColor(1);
            ImGui.PopStyleVar(1);
            ImGui.End();
        }
    }

    public void EditorResized(Sdl2Window window, GraphicsDevice device)
    {
        if (Project.Handler.FaceGenEditor != null && Project.Handler.FocusedEditor is FaceGenEditorScreen)
        {
            Project.Handler.FaceGenEditor.EditorResized(window, device);
        }
    }

    public void Draw(GraphicsDevice device, CommandList cl)
    {
        if (Project.Handler.FaceGenEditor != null && Project.Handler.FocusedEditor is FaceGenEditorScreen)
        {
            Project.Handler.FaceGenEditor.Draw(device, cl);
        }
    }
}
