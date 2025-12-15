using Hexa.NET.ImGui;
using StudioCore.Application;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;

namespace StudioCore.Editors.ModelEditor;

public class ModelEditorStub
{
    public Smithbox BaseEditor;
    public ProjectEntry Project;

    public ModelEditorStub(Smithbox baseEditor, ProjectEntry project)
    {
        BaseEditor = baseEditor;
        Project = project;
    }

    public string EditorName = "Model Editor";

    public string CommandEndpoint = "model";

    public unsafe void Display(float dt, string[] commands)
    {
        if (!Project.EnableModelEditor)
            return;

        if (!ProjectUtils.SupportsModelEditor(Project.ProjectType))
            return;

        if (commands != null && commands[0] == CommandEndpoint)
        {
            commands = commands[1..];
            ImGui.SetNextWindowFocus();
        }

        if (BaseEditor._context.Device == null)
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

            if (Project.ModelEditor != null)
            {
                Project.ModelEditor.OnGUI(commands);
            }
            else
            {
                ImGui.Text("");
                ImGui.Text("   Editor is loading...");
            }

            ImGui.End();

            if (Project.ModelEditor != null)
            {
                Project.FocusedEditor = Project.ModelEditor;
                Project.ModelEditor.Update(dt);
            }
        }
        else
        {
            if (Project.ModelEditor != null)
            {
                Project.ModelEditor.OnDefocus();
            }

            ImGui.PopStyleColor(1);
            ImGui.PopStyleVar(1);
            ImGui.End();
        }
    }

    public void EditorResized(Sdl2Window window, GraphicsDevice device)
    {
        if (Project.ModelEditor != null && Project.FocusedEditor is ModelEditorScreen)
        {
            Project.ModelEditor.EditorResized(window, device);
        }
    }

    public void Draw(GraphicsDevice device, CommandList cl)
    {
        if (Project.ModelEditor != null && Project.FocusedEditor is ModelEditorScreen)
        {
            Project.ModelEditor.Draw(device, cl);
        }
    }
}
