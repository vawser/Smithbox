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

        // On setup, set the filters from the CFG vars so we can persist these filters between sessions.
        // Each is set outright rather than toggled, as the stored filter may not agree with the CFG vars.
        var scene = View.ViewportHandler.ActiveViewport.RenderScene;

        scene.SetDrawFilter(RenderFilter.MapPiece, CFG.Current.MapEditor_BasicFilter_MapPiece);
        scene.SetDrawFilter(RenderFilter.Object, CFG.Current.MapEditor_BasicFilter_Object);
        scene.SetDrawFilter(RenderFilter.Character, CFG.Current.MapEditor_BasicFilter_Character);
        scene.SetDrawFilter(RenderFilter.DummyEnemy, CFG.Current.MapEditor_BasicFilter_DummyEnemy);
        scene.SetDrawFilter(RenderFilter.DummyAsset, CFG.Current.MapEditor_BasicFilter_DummyAsset);
        scene.SetDrawFilter(RenderFilter.Region, CFG.Current.MapEditor_BasicFilter_Region);
        scene.SetDrawFilter(RenderFilter.Light, CFG.Current.MapEditor_BasicFilter_Light);
        scene.SetDrawFilter(RenderFilter.Collision, CFG.Current.MapEditor_BasicFilter_Collision);
        scene.SetDrawFilter(RenderFilter.ConnectCollision, CFG.Current.MapEditor_BasicFilter_ConnectCollision);
        scene.SetDrawFilter(RenderFilter.ObjectCollision, CFG.Current.MapEditor_BasicFilter_ObjectCollision);
        scene.SetDrawFilter(RenderFilter.Grass, CFG.Current.MapEditor_BasicFilter_Grass);
        scene.SetDrawFilter(RenderFilter.EdgePoint, CFG.Current.MapEditor_BasicFilter_EdgePoint);
        scene.SetDrawFilter(RenderFilter.Wall, CFG.Current.MapEditor_BasicFilter_Wall);
        scene.SetDrawFilter(RenderFilter.Navmesh, CFG.Current.MapEditor_BasicFilter_Navmesh);
        scene.SetDrawFilter(RenderFilter.NavVolume, CFG.Current.MapEditor_BasicFilter_NavVolume);
        scene.SetDrawFilter(RenderFilter.SpeedTree, CFG.Current.MapEditor_BasicFilter_SpeedTree);
        scene.SetDrawFilter(RenderFilter.AutoInvade, CFG.Current.MapEditor_BasicFilter_InvasionPoint);
        scene.SetDrawFilter(RenderFilter.MultiPlayerWall, CFG.Current.MapEditor_BasicFilter_MultiPlayerWall);
    }

    /// <summary>
    /// Store the current filters in the CFG vars. Call this after any change to the draw filter, from any UI,
    /// so the stored state is always the shown state.
    /// </summary>
    public void StoreFilterState()
    {
        var filter = View.ViewportHandler.ActiveViewport.RenderScene.DrawFilter;

        CFG.Current.MapEditor_BasicFilter_MapPiece = filter.HasFlag(RenderFilter.MapPiece);
        CFG.Current.MapEditor_BasicFilter_Object = filter.HasFlag(RenderFilter.Object);
        CFG.Current.MapEditor_BasicFilter_Character = filter.HasFlag(RenderFilter.Character);
        CFG.Current.MapEditor_BasicFilter_DummyEnemy = filter.HasFlag(RenderFilter.DummyEnemy);
        CFG.Current.MapEditor_BasicFilter_DummyAsset = filter.HasFlag(RenderFilter.DummyAsset);
        CFG.Current.MapEditor_BasicFilter_Region = filter.HasFlag(RenderFilter.Region);
        CFG.Current.MapEditor_BasicFilter_Light = filter.HasFlag(RenderFilter.Light);
        CFG.Current.MapEditor_BasicFilter_Collision = filter.HasFlag(RenderFilter.Collision);
        CFG.Current.MapEditor_BasicFilter_ConnectCollision = filter.HasFlag(RenderFilter.ConnectCollision);
        CFG.Current.MapEditor_BasicFilter_ObjectCollision = filter.HasFlag(RenderFilter.ObjectCollision);
        CFG.Current.MapEditor_BasicFilter_Grass = filter.HasFlag(RenderFilter.Grass);
        CFG.Current.MapEditor_BasicFilter_EdgePoint = filter.HasFlag(RenderFilter.EdgePoint);
        CFG.Current.MapEditor_BasicFilter_Wall = filter.HasFlag(RenderFilter.Wall);
        CFG.Current.MapEditor_BasicFilter_Navmesh = filter.HasFlag(RenderFilter.Navmesh);
        CFG.Current.MapEditor_BasicFilter_NavVolume = filter.HasFlag(RenderFilter.NavVolume);
        CFG.Current.MapEditor_BasicFilter_SpeedTree = filter.HasFlag(RenderFilter.SpeedTree);
        CFG.Current.MapEditor_BasicFilter_InvasionPoint = filter.HasFlag(RenderFilter.AutoInvade);
        CFG.Current.MapEditor_BasicFilter_MultiPlayerWall = filter.HasFlag(RenderFilter.MultiPlayerWall);

        CFG.Current.LastSceneFilter = filter;
    }

    public void Display()
    {
        bool ticked;

        // Map Piece
        if (ImGui.MenuItem($"{LOC.Get("MAP_Filters_Map_Pieces_Title")}##toggleFilter_MapPiece"))
        {
            View.ViewportHandler.ActiveViewport.RenderScene.ToggleDrawFilter(RenderFilter.MapPiece);
            StoreFilterState();
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
                View.ViewportHandler.ActiveViewport.RenderScene.ToggleDrawFilter(RenderFilter.Object);
                StoreFilterState();
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
                StoreFilterState();
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
            StoreFilterState();
            View.DelayPicking();
        }
        ticked = View.ViewportHandler.ActiveViewport.RenderScene.DrawFilter.HasFlag(RenderFilter.Character);
        GUI.ShowActiveStatus(ticked);
        GUI.Tooltip(LOC.Get("MAP_Filters_Characters_TT"));

        var usesAssets = View.Project.Descriptor.ProjectType is ProjectType.ER
            or ProjectType.AC6
            or ProjectType.NR
            or ProjectType.DB_NT
            or ProjectType.DB;

        if (View.Project.Descriptor.ProjectType is not (ProjectType.DS2 or ProjectType.DS2S))
        {
            // Dummy Enemy
            if (ImGui.MenuItem($"{LOC.Get("MAP_Filters_Dummy_Enemies_Title")}##toggleFilter_DummyEnemy"))
            {
                View.ViewportHandler.ActiveViewport.RenderScene.ToggleDrawFilter(RenderFilter.DummyEnemy);
                StoreFilterState();
                View.DelayPicking();
            }
            ticked = View.ViewportHandler.ActiveViewport.RenderScene.DrawFilter.HasFlag(RenderFilter.DummyEnemy);
            GUI.ShowActiveStatus(ticked);
            GUI.Tooltip(LOC.Get("MAP_Filters_Dummy_Enemies_TT"));

            // Dummy Asset / Object
            var dummyAssetTitle = usesAssets
                ? LOC.Get("MAP_Filters_Dummy_Assets_Title")
                : LOC.Get("MAP_Filters_Dummy_Objects_Title");

            if (ImGui.MenuItem($"{dummyAssetTitle}##toggleFilter_DummyAsset"))
            {
                View.ViewportHandler.ActiveViewport.RenderScene.ToggleDrawFilter(RenderFilter.DummyAsset);
                StoreFilterState();
                View.DelayPicking();
            }
            ticked = View.ViewportHandler.ActiveViewport.RenderScene.DrawFilter.HasFlag(RenderFilter.DummyAsset);
            GUI.ShowActiveStatus(ticked);
            GUI.Tooltip(usesAssets
                ? LOC.Get("MAP_Filters_Dummy_Assets_TT")
                : LOC.Get("MAP_Filters_Dummy_Objects_TT"));
        }

        // Region
        if (ImGui.MenuItem($"{LOC.Get("MAP_Filters_Regions_Title")}##toggleFilter_Region"))
        {
            View.ViewportHandler.ActiveViewport.RenderScene.ToggleDrawFilter(RenderFilter.Region);
            StoreFilterState();
            View.DelayPicking();
        }
        ticked = View.ViewportHandler.ActiveViewport.RenderScene.DrawFilter.HasFlag(RenderFilter.Region);
        GUI.ShowActiveStatus(ticked);
        GUI.Tooltip(LOC.Get("MAP_Filters_Regions_TT"));

        // Light
        if (ImGui.MenuItem($"{LOC.Get("MAP_Filters_Lights_Title")}##toggleFilter_Light"))
        {
            View.ViewportHandler.ActiveViewport.RenderScene.ToggleDrawFilter(RenderFilter.Light);
            StoreFilterState();
            View.DelayPicking();
        }
        ticked = View.ViewportHandler.ActiveViewport.RenderScene.DrawFilter.HasFlag(RenderFilter.Light);
        GUI.ShowActiveStatus(ticked);
        GUI.Tooltip(LOC.Get("MAP_Filters_Lights_TT"));

        // Collision
        if (ImGui.MenuItem($"{LOC.Get("MAP_Filters_Collisions_Title")}##toggleFilter_Collision"))
        {
            View.ViewportHandler.ActiveViewport.RenderScene.ToggleDrawFilter(RenderFilter.Collision);
            StoreFilterState();
            View.DelayPicking();
        }
        ticked = View.ViewportHandler.ActiveViewport.RenderScene.DrawFilter.HasFlag(RenderFilter.Collision);
        GUI.ShowActiveStatus(ticked);
        GUI.Tooltip(LOC.Get("MAP_Filters_Collisions_TT"));

        // Collision
        if (ImGui.MenuItem($"{LOC.Get("MAP_Filters_Connect_Collisions_Title")}##toggleFilter_ConnectCollision"))
        {
            View.ViewportHandler.ActiveViewport.RenderScene.ToggleDrawFilter(RenderFilter.ConnectCollision);
            StoreFilterState();
            View.DelayPicking();
        }
        ticked = View.ViewportHandler.ActiveViewport.RenderScene.DrawFilter.HasFlag(RenderFilter.ConnectCollision);
        GUI.ShowActiveStatus(ticked);
        GUI.Tooltip(LOC.Get("MAP_Filters_Connect_Collisions_TT"));

        // Object / Asset Collision
        var objectCollisionTitle = usesAssets
            ? LOC.Get("MAP_Filters_Asset_Collisions_Title")
            : LOC.Get("MAP_Filters_Object_Collisions_Title");

        if (ImGui.MenuItem($"{objectCollisionTitle}##toggleFilter_ObjectCollision"))
        {
            View.ViewportHandler.ActiveViewport.RenderScene.ToggleDrawFilter(RenderFilter.ObjectCollision);
            StoreFilterState();
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
            StoreFilterState();
            View.DelayPicking();
        }
        ticked = View.ViewportHandler.ActiveViewport.RenderScene.DrawFilter.HasFlag(RenderFilter.Grass);
        GUI.ShowActiveStatus(ticked);
        GUI.Tooltip(LOC.Get("MAP_Filters_Grass_TT"));

        // Edge Points
        if (View.Project.Descriptor.ProjectType is ProjectType.SDT)
        {
            if (ImGui.MenuItem($"{LOC.Get("MAP_Filters_Edge_Points_Title")}##toggleFilter_EdgePoint"))
            {
                View.ViewportHandler.ActiveViewport.RenderScene.ToggleDrawFilter(RenderFilter.EdgePoint);
                StoreFilterState();
                View.DelayPicking();
            }
            ticked = View.ViewportHandler.ActiveViewport.RenderScene.DrawFilter.HasFlag(RenderFilter.EdgePoint);
            GUI.ShowActiveStatus(ticked);
            GUI.Tooltip(LOC.Get("MAP_Filters_Edge_Points_TT"));

            if (ImGui.MenuItem($"{LOC.Get("MAP_Filters_Walls_Title")}##toggleFilter_Wall"))
            {
                View.ViewportHandler.ActiveViewport.RenderScene.ToggleDrawFilter(RenderFilter.Wall);
                StoreFilterState();
                View.DelayPicking();
            }
            ticked = View.ViewportHandler.ActiveViewport.RenderScene.DrawFilter.HasFlag(RenderFilter.Wall);
            GUI.ShowActiveStatus(ticked);
            GUI.Tooltip(LOC.Get("MAP_Filters_Walls_TT"));
        }

        // Navmesh
        if (ImGui.MenuItem($"{LOC.Get("MAP_Filters_Navmeshes_Title")}##toggleFilter_Navmesh"))
        {
            View.ViewportHandler.ActiveViewport.RenderScene.ToggleDrawFilter(RenderFilter.Navmesh);
            StoreFilterState();
            View.DelayPicking();
        }
        ticked = View.ViewportHandler.ActiveViewport.RenderScene.DrawFilter.HasFlag(RenderFilter.Navmesh);
        GUI.ShowActiveStatus(ticked);
        GUI.Tooltip(LOC.Get("MAP_Filters_Navmeshes_TT"));

        // Nav Volumes
        if (View.Project.Descriptor.ProjectType is ProjectType.AC6)
        {
            if (ImGui.MenuItem($"{LOC.Get("MAP_Filters_Nav_Volumes_Title")}##toggleFilter_NavVolume"))
            {
                View.ViewportHandler.ActiveViewport.RenderScene.ToggleDrawFilter(RenderFilter.NavVolume);
                StoreFilterState();
                View.DelayPicking();
            }
            ticked = View.ViewportHandler.ActiveViewport.RenderScene.DrawFilter.HasFlag(RenderFilter.NavVolume);
            GUI.ShowActiveStatus(ticked);
            GUI.Tooltip(LOC.Get("MAP_Filters_Nav_Volumes_TT"));
        }

        // Speed Trees
        if (ImGui.MenuItem($"{LOC.Get("MAP_Filters_Speed_Trees_Title")}##toggleFilter_SpeedTree"))
        {
            View.ViewportHandler.ActiveViewport.RenderScene.ToggleDrawFilter(RenderFilter.SpeedTree);
            StoreFilterState();
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
                View.ViewportHandler.ActiveViewport.RenderScene.ToggleDrawFilter(RenderFilter.AutoInvade);
                StoreFilterState();
                View.DelayPicking();
            }
            ticked = View.ViewportHandler.ActiveViewport.RenderScene.DrawFilter.HasFlag(RenderFilter.AutoInvade);
            GUI.ShowActiveStatus(ticked);
            GUI.Tooltip(LOC.Get("MAP_Filters_Invasion_Points_TT"));

            if (ImGui.MenuItem($"{LOC.Get("MAP_Filters_Multiplayer_Walls_Title")}##toggleFilter_MultiPlayerWall"))
            {
                View.ViewportHandler.ActiveViewport.RenderScene.ToggleDrawFilter(RenderFilter.MultiPlayerWall);
                StoreFilterState();
                View.DelayPicking();
            }
            ticked = View.ViewportHandler.ActiveViewport.RenderScene.DrawFilter.HasFlag(RenderFilter.MultiPlayerWall);
            GUI.ShowActiveStatus(ticked);
            GUI.Tooltip(LOC.Get("MAP_Filters_Multiplayer_Walls_TT"));
        }
    }

}
