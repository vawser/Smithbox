using Hexa.NET.ImGui;
using StudioCore.Interface;
using StudioCore.Scene;
using StudioCore.Scene.Enums;

namespace StudioCore.Editors.MapEditorNS;
public class BasicFilters
{
    private MapEditor Editor;

    public BasicFilters(MapEditor editor)
    {
        Editor = editor;
    }

    public void Display()
    {
        bool ticked;

        // Map Piece
        if (ImGui.MenuItem("Map Piece"))
        {
            Editor.RenderScene.ToggleDrawFilter(RenderFilter.MapPiece);
        }
        ticked = Editor.RenderScene.DrawFilter.HasFlag(RenderFilter.MapPiece);
        UIHelper.ShowActiveStatus(ticked);

        // Collision
        if (ImGui.MenuItem("Collision"))
        {
            Editor.RenderScene.ToggleDrawFilter(RenderFilter.Collision);
        }
        ticked = Editor.RenderScene.DrawFilter.HasFlag(RenderFilter.Collision);
        UIHelper.ShowActiveStatus(ticked);

        // Object
        if (ImGui.MenuItem("Object"))
        {
            Editor.RenderScene.ToggleDrawFilter(RenderFilter.Object);
        }
        ticked = Editor.RenderScene.DrawFilter.HasFlag(RenderFilter.Object);
        UIHelper.ShowActiveStatus(ticked);

        // Character
        if (ImGui.MenuItem("Character"))
        {
            Editor.RenderScene.ToggleDrawFilter(RenderFilter.Character);
        }
        ticked = Editor.RenderScene.DrawFilter.HasFlag(RenderFilter.Character);
        UIHelper.ShowActiveStatus(ticked);

        // Navmesh
        if (ImGui.MenuItem("Navmesh"))
        {
            Editor.RenderScene.ToggleDrawFilter(RenderFilter.Navmesh);
        }
        ticked = Editor.RenderScene.DrawFilter.HasFlag(RenderFilter.Navmesh);
        UIHelper.ShowActiveStatus(ticked);

        // Region
        if (ImGui.MenuItem("Region"))
        {
            Editor.RenderScene.ToggleDrawFilter(RenderFilter.Region);
        }
        ticked = Editor.RenderScene.DrawFilter.HasFlag(RenderFilter.Region);
        UIHelper.ShowActiveStatus(ticked);

        // Light
        if (ImGui.MenuItem("Light"))
        {
            Editor.RenderScene.ToggleDrawFilter(RenderFilter.Light);
        }
        ticked = Editor.RenderScene.DrawFilter.HasFlag(RenderFilter.Light);
        UIHelper.ShowActiveStatus(ticked);

        // Debug
        if (ImGui.MenuItem("Debug"))
        {
            Editor.RenderScene.ToggleDrawFilter(RenderFilter.Debug);
        }
        ticked = Editor.RenderScene.DrawFilter.HasFlag(RenderFilter.Debug);
        UIHelper.ShowActiveStatus(ticked);

        // Speed Tree
        if (ImGui.MenuItem("Speed Tree"))
        {
            Editor.RenderScene.ToggleDrawFilter(RenderFilter.SpeedTree);
        }
        ticked = Editor.RenderScene.DrawFilter.HasFlag(RenderFilter.SpeedTree);
        UIHelper.ShowActiveStatus(ticked);
    }
}
