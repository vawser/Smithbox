using Hexa.NET.ImGui;
using Octokit;
using StudioCore.Core;
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

        // Character
        if (ImGui.MenuItem("Characters"))
        {
            RenderScene.ToggleDrawFilter(RenderFilter.Character);
        }
        ticked = RenderScene.DrawFilter.HasFlag(RenderFilter.Character);
        UIHelper.ShowActiveStatus(ticked);

        // Region
        if (ImGui.MenuItem("Regions"))
        {
            RenderScene.ToggleDrawFilter(RenderFilter.Region);
        }
        ticked = RenderScene.DrawFilter.HasFlag(RenderFilter.Region);
        UIHelper.ShowActiveStatus(ticked);

        // Light
        if (ImGui.MenuItem("Lights"))
        {
            RenderScene.ToggleDrawFilter(RenderFilter.Light);
        }
        ticked = RenderScene.DrawFilter.HasFlag(RenderFilter.Light);
        UIHelper.ShowActiveStatus(ticked);

        // Collision
        if (ImGui.MenuItem("Collisions"))
        {
            RenderScene.ToggleDrawFilter(RenderFilter.Collision);
        }
        ticked = RenderScene.DrawFilter.HasFlag(RenderFilter.Collision);
        UIHelper.ShowActiveStatus(ticked);

        // Navmesh
        if (ImGui.MenuItem("Navmeshes"))
        {
            RenderScene.ToggleDrawFilter(RenderFilter.Navmesh);
        }
        ticked = RenderScene.DrawFilter.HasFlag(RenderFilter.Navmesh);
        UIHelper.ShowActiveStatus(ticked);

        // Speed Trees
        if (ImGui.MenuItem("Speed Trees"))
        {
            RenderScene.ToggleDrawFilter(RenderFilter.SpeedTree);
        }
        ticked = RenderScene.DrawFilter.HasFlag(RenderFilter.SpeedTree);
        UIHelper.ShowActiveStatus(ticked);

        // AutoInvade
        if (ImGui.MenuItem("Invasion Points"))
        {
            RenderScene.ToggleDrawFilter(RenderFilter.AutoInvade);
        }
        ticked = RenderScene.DrawFilter.HasFlag(RenderFilter.AutoInvade);
        UIHelper.ShowActiveStatus(ticked);

        // Debug
        //if (ImGui.MenuItem("Debug"))
        //{
        //    RenderScene.ToggleDrawFilter(RenderFilter.Debug);
        //}
        //ticked = RenderScene.DrawFilter.HasFlag(RenderFilter.Debug);
        //UIHelper.ShowActiveStatus(ticked);

    }
}
