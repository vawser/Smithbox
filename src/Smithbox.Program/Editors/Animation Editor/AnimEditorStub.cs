using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.ModelEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Veldrid;
using Veldrid.Sdl2;

namespace StudioCore.Editors.AnimEditor;

public class AnimEditorStub : IEditorStub
{
    public ProjectEntry Project;

    public AnimEditorStub(ProjectEntry project)
    {
        Project = project;
    }

    public string EditorName = "Animation Editor";

    public string CommandEndpoint = "anim";

    public unsafe void Display(float dt, string[] commands)
    {
        if (!Project.Descriptor.EnableAnimEditor)
            return;

        if (!ProjectUtils.SupportsAnimEditor(Project.Descriptor.ProjectType))
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

            if (Project.Handler.AnimEditor != null)
            {
                Project.Handler.AnimEditor.OnGUI(commands);
            }
            else
            {
                ImGui.Text("");
                ImGui.Text("   Editor is loading...");
            }

            ImGui.End();

            if (Project.Handler.AnimEditor != null)
            {
                Project.Handler.FocusedEditor = Project.Handler.AnimEditor;
                Project.Handler.AnimEditor.Update(dt);
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
        if (Project.Handler.AnimEditor != null && Project.Handler.FocusedEditor is ModelEditorScreen)
        {
            Project.Handler.AnimEditor.EditorResized(window, device);
        }
    }

    public void Draw(GraphicsDevice device, CommandList cl)
    {
        if (Project.Handler.AnimEditor != null && Project.Handler.FocusedEditor is ModelEditorScreen)
        {
            Project.Handler.AnimEditor.Draw(device, cl);
        }
    }
}
