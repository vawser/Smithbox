using Hexa.NET.ImGui;

namespace StudioCore.Editors.HavokEditor;

public class HavokToolView
{
    public HavokEditorView View;
    public ProjectEntry Project;

    public CollisionGeneratorTool CollisionGeneratorTool;

    public HavokScriptReloader HavokScriptReloader;

    private ToolSections Sections = new();

    public HavokToolView(HavokEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;

        CollisionGeneratorTool = new(view, project);
        HavokScriptReloader = new(view, project);
    }

    public void Draw()
    {
        Sections.Begin();

        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Collision"));

        CollisionGeneratorTool.Display();

        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Behavior"));

        HavokScriptReloader.Display();

        Sections.End();
    }

    public void Shortcuts()
    {

    }
}
