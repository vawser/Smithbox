using Hexa.NET.ImGui;
using StudioCore.Interface;
using StudioCore.MsbEditor;
using StudioCore.Scene;
using StudioCore.Scene.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor.Framework;

public class BasicFilters
{
    private MapEditorScreen Screen;
    private RenderScene RenderScene;

    public BasicFilters(MapEditorScreen screen)
    {
        Screen = screen;
        RenderScene = screen.MapViewportView.RenderScene;
    }

    public void Display()
    {
        bool ticked;

        // Map Piece
        if (ImGui.MenuItem("Map Piece"))
        {
            RenderScene.ToggleDrawFilter(RenderFilter.MapPiece);
        }
        ticked = RenderScene.DrawFilter.HasFlag(RenderFilter.MapPiece);
        UIHelper.ShowActiveStatus(ticked);

        // Collision
        if (ImGui.MenuItem("Collision"))
        {
            RenderScene.ToggleDrawFilter(RenderFilter.Collision);
        }
        ticked = RenderScene.DrawFilter.HasFlag(RenderFilter.Collision);
        UIHelper.ShowActiveStatus(ticked);

        // Object
        if (ImGui.MenuItem("Object"))
        {
            RenderScene.ToggleDrawFilter(RenderFilter.Object);
        }
        ticked = RenderScene.DrawFilter.HasFlag(RenderFilter.Object);
        UIHelper.ShowActiveStatus(ticked);

        // Character
        if (ImGui.MenuItem("Character"))
        {
            RenderScene.ToggleDrawFilter(RenderFilter.Character);
        }
        ticked = RenderScene.DrawFilter.HasFlag(RenderFilter.Character);
        UIHelper.ShowActiveStatus(ticked);

        // Navmesh
        if (ImGui.MenuItem("Navmesh"))
        {
            RenderScene.ToggleDrawFilter(RenderFilter.Navmesh);
        }
        ticked = RenderScene.DrawFilter.HasFlag(RenderFilter.Navmesh);
        UIHelper.ShowActiveStatus(ticked);

        // Region
        if (ImGui.MenuItem("Region"))
        {
            RenderScene.ToggleDrawFilter(RenderFilter.Region);
        }
        ticked = RenderScene.DrawFilter.HasFlag(RenderFilter.Region);
        UIHelper.ShowActiveStatus(ticked);

        // Light
        if (ImGui.MenuItem("Light"))
        {
            RenderScene.ToggleDrawFilter(RenderFilter.Light);
        }
        ticked = RenderScene.DrawFilter.HasFlag(RenderFilter.Light);
        UIHelper.ShowActiveStatus(ticked);

        // Debug
        if (ImGui.MenuItem("Debug"))
        {
            RenderScene.ToggleDrawFilter(RenderFilter.Debug);
        }
        ticked = RenderScene.DrawFilter.HasFlag(RenderFilter.Debug);
        UIHelper.ShowActiveStatus(ticked);

        // Speed Tree
        if (ImGui.MenuItem("Speed Tree"))
        {
            RenderScene.ToggleDrawFilter(RenderFilter.SpeedTree);
        }
        ticked = RenderScene.DrawFilter.HasFlag(RenderFilter.SpeedTree);
        UIHelper.ShowActiveStatus(ticked);
    }
}
