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

    public HavokToolView(HavokEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
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
                ImGui.EndMenu();
            }

            ImGui.EndMenuBar();
        }
    }

    public void Shortcuts()
    {

    }
}