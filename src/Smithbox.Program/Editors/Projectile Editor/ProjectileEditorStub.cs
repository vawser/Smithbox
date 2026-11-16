using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;

namespace StudioCore.Editors.ProjectileEditor;

public class ProjectileEditorStub : IEditorStub
{
    public ProjectEntry Project;

    public ProjectileEditorStub(ProjectEntry project)
    {
        Project = project;
    }

    public string EditorName = "Projectile Editor";

    public string CommandEndpoint = "projectile";

    public unsafe void Display(float dt, string[] commands)
    {
        if (!Project.Descriptor.EnableProjectileEditor)
            return;

        if (!EditorConditions.SupportsProjectileEditor(Project.Descriptor.ProjectType))
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

        if (EditorHost.Begin($"{LOC.Get("PROJECTILE_Window_Projectile_Editor")}###projectileEditor", ImGuiWindowFlags.MenuBar))
        {
            ImGui.PopStyleColor(1);
            ImGui.PopStyleVar(1);

            if (Project.Handler.ProjectileEditor != null)
            {
                Project.Handler.ProjectileEditor.OnGUI(commands);
            }
            else
            {
                GUI.Spacer();
                ImGui.Text(LOC.Get("EDITOR_Editor_Is_Loading"));
            }

            ImGui.End();

            if (Project.Handler.ProjectileEditor != null)
            {
                Project.Handler.FocusedEditor = Project.Handler.ProjectileEditor;
                Project.Handler.ProjectileEditor.Update(dt);
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
        if (Project.Handler.ProjectileEditor != null && Project.Handler.FocusedEditor is ProjectileEditorScreen)
        {
            Project.Handler.ProjectileEditor.EditorResized(window, device);
        }
    }

    public void Draw(GraphicsDevice device, CommandList cl)
    {
        if (Project.Handler.ProjectileEditor != null && Project.Handler.FocusedEditor is ProjectileEditorScreen)
        {
            Project.Handler.ProjectileEditor.Draw(device, cl);
        }
    }
}
