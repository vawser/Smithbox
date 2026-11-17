using Hexa.NET.ImGui;

namespace StudioCore.Editors.HavokEditor;

public class HavokToolView
{
    public HavokEditorView View;
    public ProjectEntry Project;

    public CollisionGeneratorTool CollisionGeneratorTool;

    public HavokCharacterReloader HavokCharacterReloader;

    private ToolSections Sections = new();

    public HavokToolView(HavokEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;

        CollisionGeneratorTool = new(view, project);
        HavokCharacterReloader = new(view, project);
    }

    public void Draw()
    {
        Sections.Begin();

        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Collision"));

        CollisionGeneratorTool.Display();

        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Game_Memory"));

        HavokCharacterReloader.Display();

        Sections.End();
    }

    public void Shortcuts()
    {

    }
}
