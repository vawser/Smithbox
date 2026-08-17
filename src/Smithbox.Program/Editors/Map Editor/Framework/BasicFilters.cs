using Hexa.NET.ImGui;
using SoulsFormats;
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
        if (ImGui.MenuItem($"{LOC.Get("MAP_Filters_Map_Pieces_Title")}##toggleFilter_MapPiece"))
        {
            View.ViewportHandler.ActiveViewport.RenderScene.ToggleDrawFilter(RenderFilter.MapPiece);
            View.DelayPicking();
        }
        ticked = View.ViewportHandler.ActiveViewport.RenderScene.DrawFilter.HasFlag(RenderFilter.MapPiece);
        GUI.ShowActiveStatus(ticked);
        GUI.Tooltip(LOC.Get("MAP_Filters_Map_Pieces_TT"));

        if (View.Project.Descriptor.ProjectType is ProjectType.ER or ProjectType.AC6 or ProjectType.NR)
        {
            // Asset
            if (ImGui.MenuItem($"{LOC.Get("MAP_Filters_Assets_Title")}##toggleFilter_Asset"))
            {
                View.ViewportHandler.ActiveViewport.RenderScene.ToggleDrawFilter(RenderFilter.Object);
                View.DelayPicking();
            }
            ticked = View.ViewportHandler.ActiveViewport.RenderScene.DrawFilter.HasFlag(RenderFilter.Object);
            GUI.ShowActiveStatus(ticked);
            GUI.Tooltip(LOC.Get("MAP_Filters_Assets_TT"));
        }
        else
        {
            // Object
            if (ImGui.MenuItem($"{LOC.Get("MAP_Filters_Objects_Title")}##toggleFilter_Object"))
            {
                View.ViewportHandler.ActiveViewport.RenderScene.ToggleDrawFilter(RenderFilter.Object);
                View.DelayPicking();
            }
            ticked = View.ViewportHandler.ActiveViewport.RenderScene.DrawFilter.HasFlag(RenderFilter.Object);
            GUI.ShowActiveStatus(ticked);
            GUI.Tooltip(LOC.Get("MAP_Filters_Objects_TT"));
        }

        // Character
        if (ImGui.MenuItem($"{LOC.Get("MAP_Filters_Characters_Title")}##toggleFilter_Character"))
        {
            View.ViewportHandler.ActiveViewport.RenderScene.ToggleDrawFilter(RenderFilter.Character);
            View.DelayPicking();
        }
        ticked = View.ViewportHandler.ActiveViewport.RenderScene.DrawFilter.HasFlag(RenderFilter.Character);
        GUI.ShowActiveStatus(ticked);
        GUI.Tooltip(LOC.Get("MAP_Filters_Characters_TT"));

        // Region
        if (ImGui.MenuItem($"{LOC.Get("MAP_Filters_Regions_Title")}##toggleFilter_Region"))
        {
            View.ViewportHandler.ActiveViewport.RenderScene.ToggleDrawFilter(RenderFilter.Region);
            View.DelayPicking();
        }
        ticked = View.ViewportHandler.ActiveViewport.RenderScene.DrawFilter.HasFlag(RenderFilter.Region);
        GUI.ShowActiveStatus(ticked);
        GUI.Tooltip(LOC.Get("MAP_Filters_Regions_TT"));

        // Light
        if (ImGui.MenuItem($"{LOC.Get("MAP_Filters_Lights_Title")}##toggleFilter_Light"))
        {
            View.ViewportHandler.ActiveViewport.RenderScene.ToggleDrawFilter(RenderFilter.Light);
            View.DelayPicking();
        }
        ticked = View.ViewportHandler.ActiveViewport.RenderScene.DrawFilter.HasFlag(RenderFilter.Light);
        GUI.ShowActiveStatus(ticked);
        GUI.Tooltip(LOC.Get("MAP_Filters_Lights_TT"));

        // Collision
        if (ImGui.MenuItem($"{LOC.Get("MAP_Filters_Collisions_Title")}##toggleFilter_Collision"))
        {
            View.ViewportHandler.ActiveViewport.RenderScene.ToggleDrawFilter(RenderFilter.Collision);
            View.DelayPicking();
        }
        ticked = View.ViewportHandler.ActiveViewport.RenderScene.DrawFilter.HasFlag(RenderFilter.Collision);
        GUI.ShowActiveStatus(ticked);
        GUI.Tooltip(LOC.Get("MAP_Filters_Collisions_TT"));

        // Collision
        if (ImGui.MenuItem($"{LOC.Get("MAP_Filters_Connect_Collisions_Title")}##toggleFilter_ConnectCollision"))
        {
            View.ViewportHandler.ActiveViewport.RenderScene.ToggleDrawFilter(RenderFilter.ConnectCollision);
            View.DelayPicking();
        }
        ticked = View.ViewportHandler.ActiveViewport.RenderScene.DrawFilter.HasFlag(RenderFilter.ConnectCollision);
        GUI.ShowActiveStatus(ticked);
        GUI.Tooltip(LOC.Get("MAP_Filters_Connect_Collisions_TT"));


        // Navmesh
        if (ImGui.MenuItem($"{LOC.Get("MAP_Filters_Navmeshes_Title")}##toggleFilter_Navmesh"))
        {
            View.ViewportHandler.ActiveViewport.RenderScene.ToggleDrawFilter(RenderFilter.Navmesh);
            View.DelayPicking();
        }
        ticked = View.ViewportHandler.ActiveViewport.RenderScene.DrawFilter.HasFlag(RenderFilter.Navmesh);
        GUI.ShowActiveStatus(ticked);
        GUI.Tooltip(LOC.Get("MAP_Filters_Navmeshes_TT"));

        // Speed Trees
        if (ImGui.MenuItem($"{LOC.Get("MAP_Filters_Speed_Trees_Title")}##toggleFilter_SpeedTree"))
        {
            View.ViewportHandler.ActiveViewport.RenderScene.ToggleDrawFilter(RenderFilter.SpeedTree);
            View.DelayPicking();
        }
        ticked = View.ViewportHandler.ActiveViewport.RenderScene.DrawFilter.HasFlag(RenderFilter.SpeedTree);
        GUI.ShowActiveStatus(ticked);
        GUI.Tooltip(LOC.Get("MAP_Filters_Speed_Trees_TT"));

        // AutoInvade
        if (View.Project.Descriptor.ProjectType is ProjectType.ER)
        {
            if (ImGui.MenuItem($"{LOC.Get("MAP_Filters_Invasion_Points_Title")}##toggleFilter_InvasionPoint"))
            {
                View.ViewportHandler.ActiveViewport.RenderScene.ToggleDrawFilter(RenderFilter.AutoInvade);
                View.DelayPicking();
            }
            ticked = View.ViewportHandler.ActiveViewport.RenderScene.DrawFilter.HasFlag(RenderFilter.AutoInvade);
            GUI.ShowActiveStatus(ticked);
            GUI.Tooltip(LOC.Get("MAP_Filters_Invasion_Points_TT"));
        }

        // Light Probe
        //if (View.Project.Descriptor.ProjectType is ProjectType.DS3 or ProjectType.BB)
        //{
        //    if (ImGui.MenuItem($"{LOC.Get("MAP_Filters_Light_Probes_Title")}##toggleFilter_LightProbe"))
        //    {
        //        View.ViewportHandler.ActiveViewport.RenderScene.ToggleDrawFilter(RenderFilter.LightProbe);
        //        View.DelayPicking();
        //    }
        //    ticked = View.ViewportHandler.ActiveViewport.RenderScene.DrawFilter.HasFlag(RenderFilter.LightProbe);
        //    GUI.ShowActiveStatus(ticked);
        //    GUI.Tooltip(LOC.Get("MAP_Filters_Light_Probes_TT"));
        //}
    }

}
