using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System.Numerics;

namespace StudioCore.Editors.TextureViewer;

public class TextureViewerStub : IEditorStub
{
    public ProjectEntry Project;

    public TextureViewerStub(ProjectEntry project)
    {
        Project = project;
    }

    public string EditorName = "Texture Viewer##TextureViewerEditor";

    public string CommandEndpoint = "texture";

    public unsafe void Display(float dt, string[] commands)
    {
        if (!Project.Descriptor.EnableTextureViewer)
            return;

        if (!ProjectUtils.SupportsTextureViewer(Project.Descriptor.ProjectType))
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

        if (ImGui.Begin(EditorName, ImGuiWindowFlags.MenuBar))
        {
            ImGui.PopStyleColor(1);
            ImGui.PopStyleVar(1);

            if (Project.Handler.TextureViewer != null)
            {
                Project.Handler.TextureViewer.OnGUI(commands);
            }
            else
            {
                ImGui.Text("");
                ImGui.Text("   Editor is loading...");
            }

            ImGui.End();

            if (Project.Handler.TextureViewer != null)
            {
                Project.Handler.FocusedEditor = Project.Handler.TextureViewer;
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
