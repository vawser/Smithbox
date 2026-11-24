using Hexa.NET.ImGui;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.HavokEditor;

public class HavokToolView
{
    public HavokEditorView View;
    public ProjectEntry Project;

    public CharacterReloaderTool CharacterReloader;

    // Behavior Editor only
    public HavokBehaviorSearchTool BehaviorSearch;
    public HavokBehaviorProblemsTool BehaviorProblems;
    public HavokBehaviorChangesTool BehaviorChanges;

    private ToolSections Sections = new();

    public HavokToolView(HavokEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;

        CharacterReloader = new(project);
        BehaviorSearch = new(view, project);
        BehaviorProblems = new(view, project);
        BehaviorChanges = new(view, project);
    }

    public void Draw()
    {
        Sections.Begin();

        if (View.Editor.BehaviorMode)
        {
            // Search & Edit
            ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Search"));

            // Search
            if (CFG.Current.Interface_BehaviorEditor_Tool_Search)
                BehaviorSearch.OnToolWindow();

            // Browse
            ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Browse"));

            // Changes
            if (CFG.Current.Interface_BehaviorEditor_Tool_Changes)
                BehaviorChanges.OnToolWindow();

            // Validation
            ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Validation"));

            // Problems
            if (CFG.Current.Interface_BehaviorEditor_Tool_Problems)
                BehaviorProblems.OnToolWindow();
        }

        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Game_Memory"));

        CharacterReloader.Display();

        Sections.End();
    }

    public void Shortcuts()
    {

    }
}
