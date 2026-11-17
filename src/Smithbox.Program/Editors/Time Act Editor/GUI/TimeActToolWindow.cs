using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace StudioCore.Editors.TimeActEditor;

public class TimeActToolWindow
{
    public TimeActEditorView View;
    public ProjectEntry Project;

    private ToolSections Sections = new();

    public TimeActToolWindow(TimeActEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void Display(float width, float height)
    {
        Sections.Begin();

        // General

        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_General"));


        View.FiltersTool.OnToolWindow();

        View.HistoryTool.OnToolWindow();

        // Search & Edit

        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Search"));


        View.LocalSearchTool.OnToolWindow();

        View.GlobalSearchTool.OnToolWindow();

        View.MassEditTool.OnToolWindow();

        // Animation

        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Animation"));


        View.ComboBuilderTool.OnToolWindow();

        // Preview

        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Preview"));


        View.CharacterPreview.OnToolWindow();

        // Player characters are dressed by the Character Preview, as their masks come from what they wear
        if (!CharacterComposition.IsComposite(View.Selection.SelectedTimeActModelWrapper?.Name))
        {
            View.MaskToggler.OnToolWindow();
        }

        // Game Memory

        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Game_Memory"));


        View.CharacterReloader.Display();

        // Viewport

        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Viewport"));


        View.GridTool.OnToolWindow();

        View.ResourceListTool.Display("timeActEditor", View.Universe);

        // Rendering: sets its own category, so it comes last
        View.RenderingTool.OnToolWindow();

        Sections.End();
    }
}
