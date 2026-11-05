using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;

namespace StudioCore.Editors.GparamEditor;

public class GparamEditorStub : IEditorStub
{
    public ProjectEntry Project;

    public GparamEditorStub(ProjectEntry project)
    {
        Project = project;
    }

    public string EditorName = "";

    public string CommandEndpoint = "gparam";

    public unsafe void Display(float dt, string[] commands)
    {
        if (!Project.Descriptor.EnableGparamEditor)
            return;

        if (!EditorConditions.SupportsGraphicsParamEditor(Project.Descriptor.ProjectType))
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

        ImGui.SetNextWindowClass(ref GUI.DockGroup_EditorView);
        if (ImGui.Begin($"{LOC.Get("GPARAM_Editor_Stub")}##gparamEditorStub", ImGuiWindowFlags.MenuBar))
        {
            ImGui.PopStyleColor(1);
            ImGui.PopStyleVar(1);

            if (Project.Handler.GparamEditor != null)
            {
                Project.Handler.GparamEditor.OnGUI(commands);
            }
            else
            {
                ImGui.Text("");
                ImGui.Text(LOC.Get("SYS_Editor_Loading"));
            }

            ImGui.End();

            if (Project.Handler.GparamEditor != null)
            {
                Project.Handler.FocusedEditor = Project.Handler.GparamEditor;
                Project.Handler.GparamEditor.Update(dt);
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
        if (Project.Handler.GparamEditor != null && Project.Handler.FocusedEditor is GparamEditorScreen)
        {
            Project.Handler.GparamEditor.EditorResized(window, device);
        }
    }

    public void Draw(GraphicsDevice device, CommandList cl)
    {
        if (Project.Handler.GparamEditor != null && Project.Handler.FocusedEditor is GparamEditorScreen)
        {
            Project.Handler.GparamEditor.Draw(device, cl);
        }
    }
}
