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

            // Nodes
            if (ImGui.MenuItem($"{LOC.Get("MODEL_Framework_Filter_Bone")}##boneToggle"))
            {
                View.RenderScene.ToggleDrawFilter(RenderFilter.Nodes);
            }
            ticked = View.RenderScene.DrawFilter.HasFlag(RenderFilter.Nodes);
            GUI.Tooltip(LOC.Get("MODEL_Framework_Filter_Bone_TT"));
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
