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

        // On setup, pre-toggle the filters based on the CFG vars so we can persist these filters between sessions.
        if(!CFG.Current.MapEditor_BasicFilter_MapPiece)
        {
            View.ViewportHandler.ActiveViewport.RenderScene.ToggleDrawFilter(RenderFilter.MapPiece);
        }

        if (!CFG.Current.MapEditor_BasicFilter_Object)
        {
            View.ViewportHandler.ActiveViewport.RenderScene.ToggleDrawFilter(RenderFilter.Object);
        }

        if (!CFG.Current.MapEditor_BasicFilter_Character)
        {
            View.ViewportHandler.ActiveViewport.RenderScene.ToggleDrawFilter(RenderFilter.Character);
        }

        if (!CFG.Current.MapEditor_BasicFilter_Region)
        {
            View.ViewportHandler.ActiveViewport.RenderScene.ToggleDrawFilter(RenderFilter.Region);
        }

        if (!CFG.Current.MapEditor_BasicFilter_Light)
        {
            View.ViewportHandler.ActiveViewport.RenderScene.ToggleDrawFilter(RenderFilter.Light);
        }

        if (!CFG.Current.MapEditor_BasicFilter_Collision)
        {
            View.ViewportHandler.ActiveViewport.RenderScene.ToggleDrawFilter(RenderFilter.Collision);
        }

        if (!CFG.Current.MapEditor_BasicFilter_ConnectCollision)
        {
            View.ViewportHandler.ActiveViewport.RenderScene.ToggleDrawFilter(RenderFilter.ConnectCollision);
        }

        // This one is off by default, but RenderFilter.All in the stored filter includes it, so it can't be toggled
        if (CFG.Current.MapEditor_BasicFilter_ObjectCollision)
        {
            View.ViewportHandler.ActiveViewport.RenderScene.DrawFilter |= RenderFilter.ObjectCollision;
        }
        else
        {
            View.ViewportHandler.ActiveViewport.RenderScene.DrawFilter &= ~RenderFilter.ObjectCollision;
        }

        // Same as the above
        if (CFG.Current.MapEditor_BasicFilter_Grass)
        {
            View.ViewportHandler.ActiveViewport.RenderScene.DrawFilter |= RenderFilter.Grass;
        }
        else
        {
            View.ViewportHandler.ActiveViewport.RenderScene.DrawFilter &= ~RenderFilter.Grass;
        }

        if (!CFG.Current.MapEditor_BasicFilter_Navmesh)
        {
            View.ViewportHandler.ActiveViewport.RenderScene.ToggleDrawFilter(RenderFilter.Navmesh);
        }

        if (!CFG.Current.MapEditor_BasicFilter_SpeedTree)
        {
            View.ViewportHandler.ActiveViewport.RenderScene.ToggleDrawFilter(RenderFilter.SpeedTree);
        }

        if (!CFG.Current.MapEditor_BasicFilter_InvasionPoint)
        {
            View.ViewportHandler.ActiveViewport.RenderScene.ToggleDrawFilter(RenderFilter.AutoInvade);
        }
    }

    public void Display()
    {
        bool ticked;

        // Map Piece
        if (ImGui.MenuItem($"{LOC.Get("MAP_Filters_Map_Pieces_Title")}##toggleFilter_MapPiece"))
        {
            CFG.Current.MapEditor_BasicFilter_MapPiece = !CFG.Current.MapEditor_BasicFilter_MapPiece;
            View.ViewportHandler.ActiveViewport.RenderScene.ToggleDrawFilter(RenderFilter.MapPiece);
            View.DelayPicking();
        }
        ticked = View.ViewportHandler.ActiveViewport.RenderScene.DrawFilter.HasFlag(RenderFilter.MapPiece);
        GUI.ShowActiveStatus(ticked);
        GUI.Tooltip(LOC.Get("MAP_Filters_Map_Pieces_TT"));

        if (View.Project.Descriptor.ProjectType is ProjectType.ER 
            or ProjectType.AC6 
            or ProjectType.NR
            or ProjectType.DB_NT
            or ProjectType.DB)
        {
            // Asset
            if (ImGui.MenuItem($"{LOC.Get("MAP_Filters_Assets_Title")}##toggleFilter_Asset"))
            {
                CFG.Current.MapEditor_BasicFilter_Object = !CFG.Current.MapEditor_BasicFilter_Object;
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
                CFG.Current.MapEditor_BasicFilter_Object = !CFG.Current.MapEditor_BasicFilter_Object;
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
            CFG.Current.MapEditor_BasicFilter_Character = !CFG.Current.MapEditor_BasicFilter_Character;
            View.ViewportHandler.ActiveViewport.RenderScene.ToggleDrawFilter(RenderFilter.Character);
            View.DelayPicking();
        }
        ticked = View.ViewportHandler.ActiveViewport.RenderScene.DrawFilter.HasFlag(RenderFilter.Character);
        GUI.ShowActiveStatus(ticked);
        GUI.Tooltip(LOC.Get("MAP_Filters_Characters_TT"));

        // Region
        if (ImGui.MenuItem($"{LOC.Get("MAP_Filters_Regions_Title")}##toggleFilter_Region"))
        {
            CFG.Current.MapEditor_BasicFilter_Region = !CFG.Current.MapEditor_BasicFilter_Region;
            View.ViewportHandler.ActiveViewport.RenderScene.ToggleDrawFilter(RenderFilter.Region);
            View.DelayPicking();
        }
        ticked = View.ViewportHandler.ActiveViewport.RenderScene.DrawFilter.HasFlag(RenderFilter.Region);
        GUI.ShowActiveStatus(ticked);
        GUI.Tooltip(LOC.Get("MAP_Filters_Regions_TT"));

        // Light
        if (ImGui.MenuItem($"{LOC.Get("MAP_Filters_Lights_Title")}##toggleFilter_Light"))
        {
            CFG.Current.MapEditor_BasicFilter_Light = !CFG.Current.MapEditor_BasicFilter_Light;
            View.ViewportHandler.ActiveViewport.RenderScene.ToggleDrawFilter(RenderFilter.Light);
            View.DelayPicking();
        }
        ticked = View.ViewportHandler.ActiveViewport.RenderScene.DrawFilter.HasFlag(RenderFilter.Light);
        GUI.ShowActiveStatus(ticked);
        GUI.Tooltip(LOC.Get("MAP_Filters_Lights_TT"));

        // Collision
        if (ImGui.MenuItem($"{LOC.Get("MAP_Filters_Collisions_Title")}##toggleFilter_Collision"))
        {
            CFG.Current.MapEditor_BasicFilter_Collision = !CFG.Current.MapEditor_BasicFilter_Collision;
            View.ViewportHandler.ActiveViewport.RenderScene.ToggleDrawFilter(RenderFilter.Collision);
            View.DelayPicking();
        }
        ticked = View.ViewportHandler.ActiveViewport.RenderScene.DrawFilter.HasFlag(RenderFilter.Collision);
        GUI.ShowActiveStatus(ticked);
        GUI.Tooltip(LOC.Get("MAP_Filters_Collisions_TT"));

        // Collision
        if (ImGui.MenuItem($"{LOC.Get("MAP_Filters_Connect_Collisions_Title")}##toggleFilter_ConnectCollision"))
        {
            CFG.Current.MapEditor_BasicFilter_ConnectCollision = !CFG.Current.MapEditor_BasicFilter_ConnectCollision;
            View.ViewportHandler.ActiveViewport.RenderScene.ToggleDrawFilter(RenderFilter.ConnectCollision);
            View.DelayPicking();
        }
        ticked = View.ViewportHandler.ActiveViewport.RenderScene.DrawFilter.HasFlag(RenderFilter.ConnectCollision);
        GUI.ShowActiveStatus(ticked);
        GUI.Tooltip(LOC.Get("MAP_Filters_Connect_Collisions_TT"));

        // Object / Asset Collision
        var usesAssets = View.Project.Descriptor.ProjectType is ProjectType.ER
            or ProjectType.AC6
            or ProjectType.NR
            or ProjectType.DB_NT
            or ProjectType.DB;

        var objectCollisionTitle = usesAssets
            ? LOC.Get("MAP_Filters_Asset_Collisions_Title")
            : LOC.Get("MAP_Filters_Object_Collisions_Title");

        if (ImGui.MenuItem($"{objectCollisionTitle}##toggleFilter_ObjectCollision"))
        {
            View.ViewportHandler.ActiveViewport.RenderScene.ToggleDrawFilter(RenderFilter.ObjectCollision);
            CFG.Current.MapEditor_BasicFilter_ObjectCollision = View.ViewportHandler.ActiveViewport.RenderScene.DrawFilter.HasFlag(RenderFilter.ObjectCollision);
            View.DelayPicking();
        }
        ticked = View.ViewportHandler.ActiveViewport.RenderScene.DrawFilter.HasFlag(RenderFilter.ObjectCollision);
        GUI.ShowActiveStatus(ticked);
        GUI.Tooltip(usesAssets
            ? LOC.Get("MAP_Filters_Asset_Collisions_TT")
            : LOC.Get("MAP_Filters_Object_Collisions_TT"));

        // Grass
        if (ImGui.MenuItem($"{LOC.Get("MAP_Filters_Grass_Title")}##toggleFilter_Grass"))
        {
            View.ViewportHandler.ActiveViewport.RenderScene.ToggleDrawFilter(RenderFilter.Grass);
            CFG.Current.MapEditor_BasicFilter_Grass = View.ViewportHandler.ActiveViewport.RenderScene.DrawFilter.HasFlag(RenderFilter.Grass);
            View.DelayPicking();
        }
        ticked = View.ViewportHandler.ActiveViewport.RenderScene.DrawFilter.HasFlag(RenderFilter.Grass);
        GUI.ShowActiveStatus(ticked);
        GUI.Tooltip(LOC.Get("MAP_Filters_Grass_TT"));

        // Navmesh
        if (ImGui.MenuItem($"{LOC.Get("MAP_Filters_Navmeshes_Title")}##toggleFilter_Navmesh"))
        {
            CFG.Current.MapEditor_BasicFilter_Navmesh = !CFG.Current.MapEditor_BasicFilter_Navmesh;
            View.ViewportHandler.ActiveViewport.RenderScene.ToggleDrawFilter(RenderFilter.Navmesh);
            View.DelayPicking();
        }
        ticked = View.ViewportHandler.ActiveViewport.RenderScene.DrawFilter.HasFlag(RenderFilter.Navmesh);
        GUI.ShowActiveStatus(ticked);
        GUI.Tooltip(LOC.Get("MAP_Filters_Navmeshes_TT"));

        // Speed Trees
        if (ImGui.MenuItem($"{LOC.Get("MAP_Filters_Speed_Trees_Title")}##toggleFilter_SpeedTree"))
        {
            CFG.Current.MapEditor_BasicFilter_SpeedTree = !CFG.Current.MapEditor_BasicFilter_SpeedTree;
            View.ViewportHandler.ActiveViewport.RenderScene.ToggleDrawFilter(RenderFilter.SpeedTree);
            View.DelayPicking();
        }
        ticked = View.ViewportHandler.ActiveViewport.RenderScene.DrawFilter.HasFlag(RenderFilter.SpeedTree);
        GUI.ShowActiveStatus(ticked);
        GUI.Tooltip(LOC.Get("MAP_Filters_Speed_Trees_TT"));

        // Invasion Points
        if (View.Project.Descriptor.ProjectType is ProjectType.ER)
        {
            if (ImGui.MenuItem($"{LOC.Get("MAP_Filters_Invasion_Points_Title")}##toggleFilter_InvasionPoint"))
            {
                CFG.Current.MapEditor_BasicFilter_InvasionPoint = !CFG.Current.MapEditor_BasicFilter_InvasionPoint;
                View.ViewportHandler.ActiveViewport.RenderScene.ToggleDrawFilter(RenderFilter.AutoInvade);
                View.DelayPicking();
            }
            ticked = View.ViewportHandler.ActiveViewport.RenderScene.DrawFilter.HasFlag(RenderFilter.AutoInvade);
            GUI.ShowActiveStatus(ticked);
            GUI.Tooltip(LOC.Get("MAP_Filters_Invasion_Points_TT"));
        }
    }

}
