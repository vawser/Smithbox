using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Renderer;

namespace StudioCore.Editors.ModelEditor;

public class ModelViewportFilters
{
    public ModelEditorView View;
    public ProjectEntry Project;

    public ModelViewportFilters(ModelEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
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
                View.RenderScene.ToggleDrawFilter(RenderFilter.Dummies);
            }
            ticked = View.RenderScene.DrawFilter.HasFlag(RenderFilter.Dummies);
            GUI.Tooltip(LOC.Get("MODEL_Framework_Filter_DummyPoly_TT"));
            GUI.ShowActiveStatus(ticked);

            // Dummy Labels
            if (ImGui.MenuItem($"{LOC.Get("MODEL_Framework_Filter_DummyPoly_Labels")}##dummyPolyLabelsToggle"))
            {
                CFG.Current.ModelEditor_DisplayDummyLabels = !CFG.Current.ModelEditor_DisplayDummyLabels;
            }
            ticked = CFG.Current.ModelEditor_DisplayDummyLabels;
            GUI.Tooltip(LOC.Get("MODEL_Framework_Filter_DummyPoly_Labels_TT"));
            GUI.ShowActiveStatus(ticked);

            // Nodes
            if (ImGui.MenuItem($"{LOC.Get("MODEL_Framework_Filter_Bone")}##boneToggle"))
            {
                View.RenderScene.ToggleDrawFilter(RenderFilter.Nodes);
            }
            ticked = View.RenderScene.DrawFilter.HasFlag(RenderFilter.Nodes);
            GUI.Tooltip(LOC.Get("MODEL_Framework_Filter_Bone_TT"));
            GUI.ShowActiveStatus(ticked);

            // Bone Labels
            if (ImGui.MenuItem($"{LOC.Get("MODEL_Framework_Filter_Bone_Labels")}##boneLabelsToggle"))
            {
                CFG.Current.ModelEditor_DisplayNodeLabels = !CFG.Current.ModelEditor_DisplayNodeLabels;
            }
            ticked = CFG.Current.ModelEditor_DisplayNodeLabels;
            GUI.Tooltip(LOC.Get("MODEL_Framework_Filter_Bone_Labels_TT"));
            GUI.ShowActiveStatus(ticked);

            // Collision
            if (ImGui.MenuItem($"{LOC.Get("MODEL_Framework_Filter_Collision")}##collisionToggle"))
            {
                View.RenderScene.ToggleDrawFilter(RenderFilter.Collision);
            }
            ticked = View.RenderScene.DrawFilter.HasFlag(RenderFilter.Collision);
            GUI.Tooltip(LOC.Get("MODEL_Framework_Filter_Collision_TT"));
            GUI.ShowActiveStatus(ticked);

            ImGui.EndMenu();
        }

    }
}
