using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Renderer;

namespace StudioCore.Editors.MapEditor;

public class BasicFilters
{
    private MapEditorView View;

    public BasicFilters(MapEditorView view)
    {
        View = view;
    }

    public void Display()
    {
        bool ticked;

        // Map Piece
        if (ImGui.MenuItem("Map Pieces"))
        {
            View.RenderScene.ToggleDrawFilter(RenderFilter.MapPiece);
        }
        ticked = View.RenderScene.DrawFilter.HasFlag(RenderFilter.MapPiece);
        UIHelper.ShowActiveStatus(ticked);
        UIHelper.Tooltip("Toggle the display of map objects classified as 'Map pieces'.");

        var name = "Objects";
        if (View.Project.Descriptor.ProjectType is ProjectType.ER or ProjectType.AC6 or ProjectType.NR)
        {
            name = "Assets";
        }
        // Object
        if (ImGui.MenuItem(name))
        {
            View.RenderScene.ToggleDrawFilter(RenderFilter.Object);
        }
        ticked = View.RenderScene.DrawFilter.HasFlag(RenderFilter.Object);
        UIHelper.ShowActiveStatus(ticked);
        UIHelper.Tooltip($"Toggle the display of map objects classified as '{name}'.");

        // Character
        if (ImGui.MenuItem("Characters"))
        {
            View.RenderScene.ToggleDrawFilter(RenderFilter.Character);
        }
        ticked = View.RenderScene.DrawFilter.HasFlag(RenderFilter.Character);
        UIHelper.ShowActiveStatus(ticked);
        UIHelper.Tooltip($"Toggle the display of map objects classified as 'Characters'.");

        // Region
        if (ImGui.MenuItem("Regions"))
        {
            View.RenderScene.ToggleDrawFilter(RenderFilter.Region);
        }
        ticked = View.RenderScene.DrawFilter.HasFlag(RenderFilter.Region);
        UIHelper.ShowActiveStatus(ticked);
        UIHelper.Tooltip($"Toggle the display of map objects classified as 'Regions'.");

        // Light
        if (ImGui.MenuItem("Lights"))
        {
            View.RenderScene.ToggleDrawFilter(RenderFilter.Light);
        }
        ticked = View.RenderScene.DrawFilter.HasFlag(RenderFilter.Light);
        UIHelper.ShowActiveStatus(ticked);
        UIHelper.Tooltip($"Toggle the display of map objects classified as 'Lights'.");

        // Collision
        if (ImGui.MenuItem("Collisions"))
        {
            View.RenderScene.ToggleDrawFilter(RenderFilter.Collision);
        }
        ticked = View.RenderScene.DrawFilter.HasFlag(RenderFilter.Collision);
        UIHelper.ShowActiveStatus(ticked);
        UIHelper.Tooltip($"Toggle the display of map objects classified as 'Collisions'.");

        // Collision
        if (ImGui.MenuItem("Connect Collisions"))
        {
            View.RenderScene.ToggleDrawFilter(RenderFilter.ConnectCollision);
        }
        ticked = View.RenderScene.DrawFilter.HasFlag(RenderFilter.ConnectCollision);
        UIHelper.ShowActiveStatus(ticked);
        UIHelper.Tooltip($"Toggle the display of map objects classified as 'Connect Collisions'.");


        // Navmesh
        if (ImGui.MenuItem("Navmeshes"))
        {
            View.RenderScene.ToggleDrawFilter(RenderFilter.Navmesh);
        }
        ticked = View.RenderScene.DrawFilter.HasFlag(RenderFilter.Navmesh);
        UIHelper.ShowActiveStatus(ticked);
        UIHelper.Tooltip($"Toggle the display of map objects classified as 'Navmeshes'.");

        // Speed Trees
        if (ImGui.MenuItem("Speed Trees"))
        {
            View.RenderScene.ToggleDrawFilter(RenderFilter.SpeedTree);
        }
        ticked = View.RenderScene.DrawFilter.HasFlag(RenderFilter.SpeedTree);
        UIHelper.ShowActiveStatus(ticked);
        UIHelper.Tooltip($"Toggle the display of map objects classified as 'Speed Trees'.");

        // AutoInvade
        if (View.Project.Descriptor.ProjectType is ProjectType.ER)
        {
            if (ImGui.MenuItem("Invasion Points"))
            {
                View.RenderScene.ToggleDrawFilter(RenderFilter.AutoInvade);
            }
            ticked = View.RenderScene.DrawFilter.HasFlag(RenderFilter.AutoInvade);
            UIHelper.ShowActiveStatus(ticked);
            UIHelper.Tooltip($"Toggle the display of map objects classified as 'Invasion Points'.");
        }

        // Light Probe
        if (View.Project.Descriptor.ProjectType is ProjectType.DS3 or ProjectType.BB)
        {
            if (ImGui.MenuItem("Light Probes"))
            {
                View.RenderScene.ToggleDrawFilter(RenderFilter.LightProbe);
            }
            ticked = View.RenderScene.DrawFilter.HasFlag(RenderFilter.LightProbe);
            UIHelper.ShowActiveStatus(ticked);
            UIHelper.Tooltip($"Toggle the display of map objects classified as 'Light Probes'.");
        }

        // Debug
        //if (ImGui.MenuItem("Debug"))
        //{
        //    RenderScene.ToggleDrawFilter(RenderFilter.Debug);
        //}
        //ticked = RenderScene.DrawFilter.HasFlag(RenderFilter.Debug);
        //UIHelper.ShowActiveStatus(ticked);

    }
}
