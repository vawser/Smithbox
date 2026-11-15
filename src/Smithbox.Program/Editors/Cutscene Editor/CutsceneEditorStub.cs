using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;

namespace StudioCore.Editors.CutsceneEditor;

public class CutsceneEditorStub : IEditorStub
{
    public ProjectEntry Project;

    public CutsceneEditorStub(ProjectEntry project)
    {
        Project = project;
    }

    public string EditorName = "Cutscene Editor";

    public string CommandEndpoint = "cutscene";

    public unsafe void Display(float dt, string[] commands)
    {
        if (!Project.Descriptor.EnableCutsceneEditor)
            return;

        if (!EditorConditions.SupportsCutsceneEditor(Project.Descriptor.ProjectType))
            return;

        if (commands != null && commands.Length > 0 && commands[0] == CommandEndpoint)
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

        if (EditorHost.Begin($"{LOC.Get("CUTSCENE_Window_Cutscene_Editor")}###cutsceneEditor", ImGuiWindowFlags.MenuBar))
        {
            ImGui.PopStyleColor(1);
            ImGui.PopStyleVar(1);

            if (Project.Handler.CutsceneEditor != null)
            {
                Project.Handler.CutsceneEditor.OnGUI(commands);
            }
            else
            {
                GUI.Spacer();
                ImGui.Text(LOC.Get("EDITOR_Editor_Is_Loading"));
            }

            ImGui.End();

            if (Project.Handler.CutsceneEditor != null)
            {
                Project.Handler.FocusedEditor = Project.Handler.CutsceneEditor;
                Project.Handler.CutsceneEditor.Update(dt);
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
        if (Project.Handler.CutsceneEditor != null && Project.Handler.FocusedEditor is CutsceneEditorScreen)
        {
            Project.Handler.CutsceneEditor.EditorResized(window, device);
        }
    }

    public void Draw(GraphicsDevice device, CommandList cl)
    {
        if (Project.Handler.CutsceneEditor != null && Project.Handler.FocusedEditor is CutsceneEditorScreen)
        {
            Project.Handler.CutsceneEditor.Draw(device, cl);
        }
    }
}
