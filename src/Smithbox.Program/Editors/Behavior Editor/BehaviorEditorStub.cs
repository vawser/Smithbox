using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System.Numerics;

namespace StudioCore.Editors.BehaviorEditor;

public class BehaviorEditorStub : IEditorStub
{
    public ProjectEntry Project;

    public BehaviorEditorStub(ProjectEntry project)
    {
        Project = project;
    }

    public string EditorName = "Behavior Editor##BehaviorEditor";

    public string CommandEndpoint = "behavior";

    public unsafe void Display(float dt, string[] commands)
    {
        if (!Project.Descriptor.EnableBehaviorEditor)
            return;

        if (!EditorConditions.SupportsBehaviorEditor(Project.Descriptor.ProjectType))
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
        if (ImGui.Begin($"{LOC.Get("BHV_Editor_Stub")}###behaviorEditor", ImGuiWindowFlags.MenuBar))
        {
            ImGui.PopStyleColor(1);
            ImGui.PopStyleVar(1);

            if (Project.Handler.BehaviorEditor != null)
            {
                Project.Handler.BehaviorEditor.OnGUI(commands);
            }
            else
            {
                GUI.Spacer();
                ImGui.Text(LOC.Get("EDITOR_Editor_Is_Loading"));
            }

            ImGui.End();

            if (Project.Handler.BehaviorEditor != null)
            {
                Project.Handler.FocusedEditor = Project.Handler.BehaviorEditor;
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
