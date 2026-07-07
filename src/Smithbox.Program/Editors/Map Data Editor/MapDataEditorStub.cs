using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapDataEditor;

public class MapDataEditorStub : IEditorStub
{
    public ProjectEntry Project;

    public MapDataEditorStub(ProjectEntry project)
    {
        Project = project;
    }

    public string EditorName = "Map Data Editor";

    public string CommandEndpoint = "mapdata";

    public unsafe void Display(float dt, string[] commands)
    {
        if (!Project.Descriptor.EnableMapDataEditor)
            return;

        if (!ProjectUtils.SupportsMapDataEditor(Project.Descriptor.ProjectType))
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
        if (ImGui.Begin(EditorName, ImGuiWindowFlags.MenuBar))
        {
            ImGui.PopStyleColor(1);
            ImGui.PopStyleVar(1);

            if (Project.Handler.MapDataEditor != null)
            {
                Project.Handler.MapDataEditor.OnGUI(commands);
            }
            else
            {
                ImGui.Text("");
                ImGui.Text("   Editor is loading...");
            }

            ImGui.End();

            if (Project.Handler.MapDataEditor != null)
            {
                Project.Handler.FocusedEditor = Project.Handler.MapDataEditor;
            }
        }
        else
        {
            if (Project.Handler.MapDataEditor != null)
            {
                Project.Handler.MapDataEditor.OnDefocus();
            }

            ImGui.PopStyleColor(1);
            ImGui.PopStyleVar(1);
            ImGui.End();
        }
    }
}
