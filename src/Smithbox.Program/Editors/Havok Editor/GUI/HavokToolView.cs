using Hexa.NET.ImGui;

namespace StudioCore.Editors.HavokEditor;

public class HavokToolView
{
    public HavokEditorView View;
    public ProjectEntry Project;

    public CollisionGeneratorTool CollisionGeneratorTool;

    public HavokScriptReloader HavokScriptReloader;

    public HavokToolView(HavokEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;

        CollisionGeneratorTool = new(view, project);
        HavokScriptReloader = new(view, project);
    }

    public void DisplayMenu()
    {
        if (ImGui.BeginMenu($"{LOC.Get("HAVOK_Tools_Header_Tools")}##toolsMenuHeader"))
        {

            ImGui.EndMenu();
        }
    }

    public void Draw()
    {
        if (ImGui.BeginMenuBar())
        {
            // View
            if (ImGui.BeginMenu($"{LOC.Get("HAVOK_Tools_Header_View")}##viewMenuHeader"))
            {
                // Collision Generator
                if (ImGui.MenuItem($"{LOC.Get("HAVOK_Tools_Collision_Generator_Tool")}##toggleCollisionGeneratorToolVis"))
                {
                    CFG.Current.HavokEditor_ToolVisibility_CollisionGenerator = !CFG.Current.HavokEditor_ToolVisibility_CollisionGenerator;
                }
                GUI.ShowActiveStatus(CFG.Current.HavokEditor_ToolVisibility_CollisionGenerator);

                // Collision Generator
                if (ImGui.MenuItem($"{LOC.Get("HAVOK_ScriptReloader_Title")}##toggleScriptReloaderVis"))
                {
                    CFG.Current.HavokEditor_ToolVisibility_ScriptReloader = !CFG.Current.HavokEditor_ToolVisibility_ScriptReloader;
                }
                GUI.ShowActiveStatus(CFG.Current.HavokEditor_ToolVisibility_ScriptReloader);

                ImGui.EndMenu();
            }

            ImGui.EndMenuBar();
        }

        CollisionGeneratorTool.Display();
        HavokScriptReloader.Display();
    }

    public void Shortcuts()
    {

    }
}