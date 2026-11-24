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
    public HavokBehaviorMergeTool BehaviorMerge;
    public HavokBehaviorBookmarksTool BehaviorBookmarks;
    public HavokBehaviorTemplatesTool BehaviorTemplates;

    private ToolSections Sections = new();

    public HavokToolView(HavokEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;

        CharacterReloader = new(project);
        BehaviorSearch = new(view, project);
        BehaviorProblems = new(view, project);
        BehaviorChanges = new(view, project);
        BehaviorMerge = new(view, project);
        BehaviorBookmarks = new(view, project);
        BehaviorTemplates = new(view, project);
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

            // Actions
            if (CFG.Current.Interface_BehaviorEditor_Tool_Templates)
            {
                ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Actions"));

                // Templates
                BehaviorTemplates.OnToolWindow();
            }

            // Browse
            ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Browse"));

            // Changes
            if (CFG.Current.Interface_BehaviorEditor_Tool_Changes)
                BehaviorChanges.OnToolWindow();

            // Merge
            if (CFG.Current.Interface_BehaviorEditor_Tool_Merge)
                BehaviorMerge.OnToolWindow();

            // Bookmarks
            if (CFG.Current.Interface_BehaviorEditor_Tool_Bookmarks)
                BehaviorBookmarks.OnToolWindow();

            // Validation
            ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Validation"));

            // Problems
            if (CFG.Current.Interface_BehaviorEditor_Tool_Problems)
                BehaviorProblems.OnToolWindow();

            // Viewport
            if (View.Editor.Viewer != null &&
                (CFG.Current.Interface_BehaviorEditor_Tool_Character_Selector || CFG.Current.Interface_BehaviorEditor_Tool_Character_Preview))
            {
                ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Viewport"));

                // Character Selector
                if (CFG.Current.Interface_BehaviorEditor_Tool_Character_Selector)
                    View.Editor.Viewer.DisplayCharacterSelector();

                // Character Preview
                if (CFG.Current.Interface_BehaviorEditor_Tool_Character_Preview)
                    View.Editor.Viewer.DisplayCharacterPreview();
            }
        }

        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Game_Memory"));

        CharacterReloader.Display();

        Sections.End();
    }

    public void Shortcuts()
    {

    }
}
