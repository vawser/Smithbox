using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Renderer;

namespace StudioCore.Editors.MapEditor;

public class BasicFilters
{
    private MapEditorScreen Editor;
    private RenderScene RenderScene;

    public BasicFilters(MapEditorScreen screen)
    {
        Editor = screen;
        RenderScene = screen.MapViewportView.RenderScene;
    }

    public void Display()
    {
        bool ticked;

        // Map Piece
        if (ImGui.MenuItem("Map Pieces"))
        {
            RenderScene.ToggleDrawFilter(RenderFilter.MapPiece);
        }
        ticked = RenderScene.DrawFilter.HasFlag(RenderFilter.MapPiece);
        UIHelper.ShowActiveStatus(ticked);
        UIHelper.Tooltip("Toggle the display of map objects classified as 'Map pieces'.");

        var name = "Objects";
        if (Editor.Project.ProjectType is ProjectType.ER or ProjectType.AC6 or ProjectType.NR)
        {
            name = "Assets";
        }
        // Object
        if (ImGui.MenuItem(name))
        {
            RenderScene.ToggleDrawFilter(RenderFilter.Object);
        }
        ticked = RenderScene.DrawFilter.HasFlag(RenderFilter.Object);
        UIHelper.ShowActiveStatus(ticked);
        UIHelper.Tooltip($"Toggle the display of map objects classified as '{name}'.");

        // Character
        if (ImGui.MenuItem("Characters"))
        {
            RenderScene.ToggleDrawFilter(RenderFilter.Character);
        }
        ticked = RenderScene.DrawFilter.HasFlag(RenderFilter.Character);
        UIHelper.ShowActiveStatus(ticked);
        UIHelper.Tooltip($"Toggle the display of map objects classified as 'Characters'.");

        // Region
        if (ImGui.MenuItem("Regions"))
        {
            RenderScene.ToggleDrawFilter(RenderFilter.Region);
        }
        ticked = RenderScene.DrawFilter.HasFlag(RenderFilter.Region);
        UIHelper.ShowActiveStatus(ticked);
        UIHelper.Tooltip($"Toggle the display of map objects classified as 'Regions'.");

        // Light
        if (ImGui.MenuItem("Lights"))
        {
            RenderScene.ToggleDrawFilter(RenderFilter.Light);
        }
        ticked = RenderScene.DrawFilter.HasFlag(RenderFilter.Light);
        UIHelper.ShowActiveStatus(ticked);
        UIHelper.Tooltip($"Toggle the display of map objects classified as 'Lights'.");

        // Collision
        if (ImGui.MenuItem("Collisions"))
        {
            RenderScene.ToggleDrawFilter(RenderFilter.Collision);
        }
        ticked = RenderScene.DrawFilter.HasFlag(RenderFilter.Collision);
        UIHelper.ShowActiveStatus(ticked);
        UIHelper.Tooltip($"Toggle the display of map objects classified as 'Collisions'.");

        // Navmesh
        if (ImGui.MenuItem("Navmeshes"))
        {
            RenderScene.ToggleDrawFilter(RenderFilter.Navmesh);
        }
        ticked = RenderScene.DrawFilter.HasFlag(RenderFilter.Navmesh);
        UIHelper.ShowActiveStatus(ticked);
        UIHelper.Tooltip($"Toggle the display of map objects classified as 'Navmeshes'.");

        // Speed Trees
        if (ImGui.MenuItem("Speed Trees"))
        {
            RenderScene.ToggleDrawFilter(RenderFilter.SpeedTree);
        }
        ticked = RenderScene.DrawFilter.HasFlag(RenderFilter.SpeedTree);
        UIHelper.ShowActiveStatus(ticked);
        UIHelper.Tooltip($"Toggle the display of map objects classified as 'Speed Trees'.");

        // AutoInvade
        if (Editor.Project.ProjectType is ProjectType.ER)
        {
            if (ImGui.MenuItem("Invasion Points"))
            {
                RenderScene.ToggleDrawFilter(RenderFilter.AutoInvade);
            }
            ticked = RenderScene.DrawFilter.HasFlag(RenderFilter.AutoInvade);
            UIHelper.ShowActiveStatus(ticked);
            UIHelper.Tooltip($"Toggle the display of map objects classified as 'Invasion Points'.");
        }

        // Light Probe
        if (Editor.Project.ProjectType is ProjectType.DS3 or ProjectType.BB)
        {
            if (ImGui.MenuItem("Light Probes"))
            {
                RenderScene.ToggleDrawFilter(RenderFilter.LightProbe);
            }
            ticked = RenderScene.DrawFilter.HasFlag(RenderFilter.LightProbe);
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
