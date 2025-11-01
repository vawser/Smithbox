using Hexa.NET.ImGui;
using StudioCore.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.TextEditor;

public class TextEditorStub
{
    public Smithbox BaseEditor;
    public ProjectEntry Project;

    public TextEditorStub(Smithbox baseEditor, ProjectEntry project)
    {
        BaseEditor = baseEditor;
        Project = project;
    }

    public string EditorName = "Text Editor";

    public string CommandEndpoint = "text";

    public unsafe void Display(float dt, string[] commands)
    {
        if (!Project.EnableTextEditor)
            return;

        if (!ProjectUtils.SupportsTextEditor(Project.ProjectType))
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

            if (Project.TextEditor != null)
            {

                Project.TextEditor.OnGUI(commands);
            }
            else
            {
                ImGui.Text("");
                ImGui.Text("   Editor is loading...");
            }

            ImGui.End();

            if (Project.TextEditor != null)
            {
                Project.FocusedEditor = Project.TextEditor;
            }
        }
        else
        {
            ImGui.PopStyleColor(1);
            ImGui.PopStyleVar(1);
            ImGui.End();
        }
    }
}
