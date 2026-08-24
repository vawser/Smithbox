using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace StudioCore.Editors.HavokEditor;

public class HavokToolView
{
    public HavokEditorView View;
    public ProjectEntry Project;

    public CollisionGeneratorTool CollisionGeneratorTool;

    public HavokToolView(HavokEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;

        CollisionGeneratorTool = new(view, project);
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

                ImGui.EndMenu();
            }

            ImGui.EndMenuBar();
        }

        CollisionGeneratorTool.Display();
    }

    public void Shortcuts()
    {

    }
}