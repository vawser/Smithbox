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

        if (ImGui.BeginMenu("Filters"))
        {
            // Meshes
            if (ImGui.MenuItem("Meshes"))
            {
                View.RenderScene.ToggleDrawFilter(RenderFilter.Meshes);
            }
            ticked = View.RenderScene.DrawFilter.HasFlag(RenderFilter.Meshes);
            UIHelper.ShowActiveStatus(ticked);
            UIHelper.Tooltip("Toggle the display of meshes.");

            // Dummies
            if (ImGui.MenuItem("Dummy Polygons"))
            {
                View.RenderScene.ToggleDrawFilter(RenderFilter.Dummies);
            }
            ticked = View.RenderScene.DrawFilter.HasFlag(RenderFilter.Dummies);
            UIHelper.ShowActiveStatus(ticked);
            UIHelper.Tooltip("Toggle the display of dummy polygons.");


            // Nodes
            if (ImGui.MenuItem("Bones"))
            {
                View.RenderScene.ToggleDrawFilter(RenderFilter.Nodes);
            }
            ticked = View.RenderScene.DrawFilter.HasFlag(RenderFilter.Nodes);
            UIHelper.ShowActiveStatus(ticked);
            UIHelper.Tooltip("Toggle the display of bones.");

            // Collision
            if (ImGui.MenuItem("Collision"))
            {
                View.RenderScene.ToggleDrawFilter(RenderFilter.Collision);
            }
            ticked = View.RenderScene.DrawFilter.HasFlag(RenderFilter.Collision);
            UIHelper.ShowActiveStatus(ticked);
            UIHelper.Tooltip("Toggle the display of collision.");

            ImGui.EndMenu();
        }

    }
}
