using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;

namespace StudioCore.Editors.ModelEditor;

public class ModelEditorStub : IEditorStub
{
    public ProjectEntry Project;

    public ModelEditorStub(ProjectEntry project)
    {
        Project = project;
    }

    public string EditorName = "Model Editor";

    public string CommandEndpoint = "model";

    public unsafe void Display(float dt, string[] commands)
    {
        if (!Project.Descriptor.EnableModelEditor)
            return;

        if (!ProjectUtils.SupportsModelEditor(Project.Descriptor.ProjectType))
            return;

        if (commands != null && commands[0] == CommandEndpoint)
        {
            commands = commands[1..];
            ImGui.SetNextWindowFocus();
        }

        if (Smithbox.Instance._context.Device == null)
        {
            ImGui.PushStyleColor(ImGuiCol.WindowBg, *ImGui.GetStyleColorVec4(ImGuiCol.WindowBg));
        }
        else
        {
            ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(0.0f, 0.0f, 0.0f, 0.0f));
        }

        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0.0f, 0.0f));

        if (ImGui.Begin(EditorName, ImGuiWindowFlags.MenuBar))
        {
            ImGui.PopStyleColor(1);
            ImGui.PopStyleVar(1);

            if (Project.Handler.ModelEditor != null)
            {
                Project.Handler.ModelEditor.OnGUI(commands);
            }
            else
            {
                ImGui.Text("");
                ImGui.Text("   Editor is loading...");
            }

            ImGui.End();

            if (Project.Handler.ModelEditor != null)
            {
                Project.Handler.FocusedEditor = Project.Handler.ModelEditor;
                Project.Handler.ModelEditor.Update(dt);
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
        if (Project.Handler.ModelEditor != null && Project.Handler.FocusedEditor is ModelEditorScreen)
        {
            Project.Handler.ModelEditor.EditorResized(window, device);
        }
    }

    public void Draw(GraphicsDevice device, CommandList cl)
    {
        if (Project.Handler.ModelEditor != null && Project.Handler.FocusedEditor is ModelEditorScreen)
        {
            Project.Handler.ModelEditor.Draw(device, cl);
        }
    }
}
