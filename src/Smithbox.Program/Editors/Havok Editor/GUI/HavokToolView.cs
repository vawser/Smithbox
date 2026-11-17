using Hexa.NET.ImGui;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.HavokEditor;

public class HavokToolView
{
    public HavokEditorView View;
    public ProjectEntry Project;

    public CollisionGeneratorTool CollisionGeneratorTool;

    public CharacterReloaderTool CharacterReloader;

    private ToolSections Sections = new();

    public HavokToolView(HavokEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;

        CollisionGeneratorTool = new(view, project);
        CharacterReloader = new(project);
    }

    public void Draw()
    {
        Sections.Begin();

        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Collision"));

        CollisionGeneratorTool.Display();

        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Game_Memory"));

        CharacterReloader.Display();

        Sections.End();
    }

    public void Shortcuts()
    {

    }
}
