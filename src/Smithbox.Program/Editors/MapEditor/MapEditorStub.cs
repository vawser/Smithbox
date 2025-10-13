using Hexa.NET.ImGui;
using Silk.NET.SDL;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Editors.MapEditor.Core;
using StudioCore.Editors.MapEditor.Tools;
using StudioCore.Editors.MapEditor.Tools.SelectionGroups;
using StudioCore.Editors.MapEditor.Tools.WorldMap;
using StudioCore.Editors.ModelEditor;
using StudioCore.Interface;
using StudioCore.Resource;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Veldrid;
using Veldrid.Sdl2;

namespace StudioCore.Editors.MapEditor;

public class MapEditorStub
{
    public Smithbox BaseEditor;
    public ProjectEntry Project;

    public MapEditorStub(Smithbox baseEditor, ProjectEntry project)
    {
        BaseEditor = baseEditor;
        Project = project;
    }

    public string EditorName = "Map Editor";

    public string CommandEndpoint = "map";

    public unsafe void Display(float dt, string[] commands)
    {
        if (!Project.EnableMapEditor)
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

            if(Project.MapEditor != null)
            {
                Project.MapEditor.OnGUI(commands);
            }
            else
            {
                ImGui.Text("");
                ImGui.Text("   Editor is loading...");
            }

            ImGui.End();

            if (Project.MapEditor != null)
            {
                Project.FocusedEditor = Project.MapEditor;
                Project.MapEditor.Update(dt);
            }
        }
        else
        {
            if (Project.MapEditor != null)
            {
                Project.MapEditor.OnDefocus();
            }

            ImGui.PopStyleColor(1);
            ImGui.PopStyleVar(1);
            ImGui.End();
        }
    }

    public void EditorResized(Sdl2Window window, GraphicsDevice device)
    {
        if (Project.MapEditor != null && Project.FocusedEditor is MapEditorScreen)
        {
            Project.MapEditor.EditorResized(window, device);
        }
    }

    public void Draw(GraphicsDevice device, CommandList cl)
    {
        if (Project.MapEditor != null && Project.FocusedEditor is MapEditorScreen)
        {
            Project.MapEditor.Draw(device, cl);
        }
    }
}
