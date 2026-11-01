using Hexa.NET.ImGui;
using StudioCore.Editors.ModelEditor;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.Text;

namespace StudioCore.Editors.TimeActEditor;

public class TimeActViewportFilters
{
    public TimeActEditorView View;
    public ProjectEntry Project;

    public TimeActViewportFilters(TimeActEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    /// <summary>
    /// Keeps the scene in line with the saved toggles, which all the views of the editor share
    /// </summary>
    public void Update()
    {
        View.RenderScene.SetDrawFilter(RenderFilter.Dummies, CFG.Current.TimeActEditor_DisplayDummies);
        View.RenderScene.SetDrawFilter(RenderFilter.Nodes, CFG.Current.TimeActEditor_DisplayNodes);
    }

    public void Display()
    {
        bool ticked;

        if (ImGui.BeginMenu($"{LOC.Get("MODEL_Framework_Filters_Header")}##filtersMenuHeader"))
        {
            // Meshes
            if (ImGui.MenuItem($"{LOC.Get("MODEL_Framework_Filter_Mesh")}##meshToggle"))
            {
                View.RenderScene.ToggleDrawFilter(RenderFilter.Meshes);
            }
            ticked = View.RenderScene.DrawFilter.HasFlag(RenderFilter.Meshes);
            GUI.Tooltip(LOC.Get("MODEL_Framework_Filter_Mesh_TT"));
            GUI.ShowActiveStatus(ticked);

            // Dummies
            if (ImGui.MenuItem($"{LOC.Get("MODEL_Framework_Filter_DummyPoly")}##dummyPolyToggle"))
            {
                CFG.Current.TimeActEditor_DisplayDummies = !CFG.Current.TimeActEditor_DisplayDummies;
            }
            ticked = CFG.Current.TimeActEditor_DisplayDummies;
            GUI.Tooltip(LOC.Get("MODEL_Framework_Filter_DummyPoly_TT"));
            GUI.ShowActiveStatus(ticked);

            // Dummy Labels
            if (ImGui.MenuItem($"{LOC.Get("MODEL_Framework_Filter_DummyPoly_Labels")}##dummyPolyLabelsToggle"))
            {
                CFG.Current.TimeActEditor_DisplayDummyLabels = !CFG.Current.TimeActEditor_DisplayDummyLabels;
            }
            ticked = CFG.Current.TimeActEditor_DisplayDummyLabels;
            GUI.Tooltip(LOC.Get("MODEL_Framework_Filter_DummyPoly_Labels_TT"));
            GUI.ShowActiveStatus(ticked);

            // Nodes
            if (ImGui.MenuItem($"{LOC.Get("MODEL_Framework_Filter_Bone")}##boneToggle"))
            {
                CFG.Current.TimeActEditor_DisplayNodes = !CFG.Current.TimeActEditor_DisplayNodes;
            }
            ticked = CFG.Current.TimeActEditor_DisplayNodes;
            GUI.Tooltip(LOC.Get("MODEL_Framework_Filter_Bone_TT"));
            GUI.ShowActiveStatus(ticked);

            // Bone Labels
            if (ImGui.MenuItem($"{LOC.Get("MODEL_Framework_Filter_Bone_Labels")}##boneLabelsToggle"))
            {
                CFG.Current.TimeActEditor_DisplayNodeLabels = !CFG.Current.TimeActEditor_DisplayNodeLabels;
            }
            ticked = CFG.Current.TimeActEditor_DisplayNodeLabels;
            GUI.Tooltip(LOC.Get("MODEL_Framework_Filter_Bone_Labels_TT"));
            GUI.ShowActiveStatus(ticked);

            ImGui.EndMenu();
        }

    }
}
