using Hexa.NET.ImGui;
using StudioCore.Application;
using System.Numerics;

namespace StudioCore.Editors.GparamEditor;

public class GparamEditorStub
{
    public Smithbox BaseEditor;
    public ProjectEntry Project;

    public GparamEditorStub(Smithbox baseEditor, ProjectEntry project)
    {
        BaseEditor = baseEditor;
        Project = project;
    }

    public string EditorName = "Gparam Editor##GparamEditor";

    public string CommandEndpoint = "gparam";

    public unsafe void Display(float dt, string[] commands)
    {
        if (!Project.EnableGparamEditor)
            return;

        if (!ProjectUtils.SupportsGraphicsParamEditor(Project.ProjectType))
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

            if (Project.GparamEditor != null)
            {
                Project.GparamEditor.OnGUI(commands);
            }
            else
            {
                ImGui.Text("");
                ImGui.Text("   Editor is loading...");
            }

            ImGui.End();

            if (Project.GparamEditor != null)
            {
                Project.FocusedEditor = Project.GparamEditor;
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