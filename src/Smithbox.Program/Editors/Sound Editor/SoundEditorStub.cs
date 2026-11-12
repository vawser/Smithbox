using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System.Numerics;

namespace StudioCore.Editors.SoundEditor;

public class SoundEditorStub : IEditorStub
{
    public ProjectEntry Project;

    public SoundEditorStub(ProjectEntry project)
    {
        Project = project;
    }

    public string EditorName = "Sound Editor##SoundEditor";

    public string CommandEndpoint = "sound";

    public unsafe void Display(float dt, string[] commands)
    {
        if (!Project.Descriptor.EnableSoundEditor)
            return;

        if (!EditorConditions.SupportsSoundEditor(Project.Descriptor.ProjectType))
            return;

        if (commands != null && commands[0] == CommandEndpoint)
        {
            commands = commands[1..];
            ImGui.SetNextWindowFocus();
        }
        else
        {
            commands = null;
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
        if (ImGui.Begin($"{LOC.Get("SOUND_Editor_Stub")}###soundEditor", ImGuiWindowFlags.MenuBar))
        {
            ImGui.PopStyleColor(1);
            ImGui.PopStyleVar(1);

            if (Project.Handler.SoundEditor != null)
            {
                Project.Handler.SoundEditor.OnGUI(commands);
            }
            else
            {
                GUI.Spacer();
                ImGui.Text(LOC.Get("EDITOR_Editor_Is_Loading"));
            }

            ImGui.End();

            if (Project.Handler.SoundEditor != null)
            {
                Project.Handler.FocusedEditor = Project.Handler.SoundEditor;
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
