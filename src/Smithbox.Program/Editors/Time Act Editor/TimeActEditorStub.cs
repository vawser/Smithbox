using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using StudioCore.Editors.ModelEditor;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Veldrid;
using Veldrid.Sdl2;

namespace StudioCore.Editors.TimeActEditor;

public class TimeActEditorStub : IEditorStub
{
    public ProjectEntry Project;

    public TimeActEditorStub(ProjectEntry project)
    {
        Project = project;
    }

    public string EditorName = "Time Act Editor";

    public string CommandEndpoint = "tae";

    public unsafe void Display(float dt, string[] commands)
    {
        if (!Project.Descriptor.EnableTimeActEditor)
            return;

        if (!EditorConditions.SupportsTimeActEditor(Project.Descriptor.ProjectType))
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
        if (ImGui.Begin($"{LOC.Get("TAE_Editor_Title")}###timeActEditor", ImGuiWindowFlags.MenuBar))
        {
            ImGui.PopStyleColor(1);
            ImGui.PopStyleVar(1);

            if (Project.Handler.TimeActEditor != null)
            {
                Project.Handler.TimeActEditor.OnGUI(commands);
            }
            else
            {
                GUI.Spacer();
                ImGui.Text(LOC.Get("EDITOR_Editor_Is_Loading"));
            }

            ImGui.End();

            if (Project.Handler.TimeActEditor != null)
            {
                Project.Handler.FocusedEditor = Project.Handler.TimeActEditor;
                Project.Handler.TimeActEditor.Update(dt);
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
        if (Project.Handler.TimeActEditor != null && Project.Handler.FocusedEditor is TimeActEditorScreen)
        {
            Project.Handler.TimeActEditor.EditorResized(window, device);
        }
    }

    public void Draw(GraphicsDevice device, CommandList cl)
    {
        if (Project.Handler.TimeActEditor != null && Project.Handler.FocusedEditor is TimeActEditorScreen)
        {
            Project.Handler.TimeActEditor.Draw(device, cl);
        }
    }
}
