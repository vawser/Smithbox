using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.GparamEditor;
using StudioCore.Editors.TextEditor;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor;

public class ViewportFiltersAction
{
    public MapEditorView View;
    public ProjectEntry Project;

    private bool DisplayRegionFilters = true;

    public ViewportFiltersAction(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void OnToolWindow()
    {
        var scene = View.ViewportHandler.ActiveViewport.RenderScene;

        var objName = "Object";
        if (Project.Descriptor.ProjectType is ProjectType.ER 
            or ProjectType.AC6 
            or ProjectType.NR
            or ProjectType.DB_NT
            or ProjectType.DB)
        {
            objName = "Asset";
        }

        GUI.WrappedText("Configure what types of map objects are rendered within the viewport.");

        GUI.Spacer();
        GUI.SimpleHeader("Basic Filters", "Toggle the display of various basic map object forms.");

        FilterCheckbox(scene, "Map Piece", RenderFilter.MapPiece);
        FilterCheckbox(scene, objName, RenderFilter.Object);
        FilterCheckbox(scene, "Character", RenderFilter.Character);

        if (Project.Descriptor.ProjectType is not (ProjectType.DS2 or ProjectType.DS2S))
        {
            FilterCheckbox(scene, "Dummy Enemy", RenderFilter.DummyEnemy);
            FilterCheckbox(scene, $"Dummy {objName}", RenderFilter.DummyAsset);
        }

        FilterCheckbox(scene, "Region", RenderFilter.Region);
        FilterCheckbox(scene, "Light", RenderFilter.Light);
        FilterCheckbox(scene, "Map Collision", RenderFilter.Collision);
        FilterCheckbox(scene, "Connect Collision", RenderFilter.ConnectCollision);
        FilterCheckbox(scene, $"{objName} Collision", RenderFilter.ObjectCollision);
        FilterCheckbox(scene, "Grass", RenderFilter.Grass);

        if (Project.Descriptor.ProjectType is ProjectType.SDT)
        {
            FilterCheckbox(scene, "Edge Point", RenderFilter.EdgePoint);
        }

        FilterCheckbox(scene, "Navmesh", RenderFilter.Navmesh);

        if (Project.Descriptor.ProjectType is ProjectType.AC6)
        {
            FilterCheckbox(scene, "Nav Volume", RenderFilter.NavVolume);
            FilterCheckbox(scene, "Mission Area Limit", RenderFilter.MapLimit);
        }

        FilterCheckbox(scene, "Speed Tree", RenderFilter.SpeedTree);
        if (Project.Descriptor.ProjectType is ProjectType.ER)
        {
            FilterCheckbox(scene, "Invasion Point", RenderFilter.AutoInvade);
            FilterCheckbox(scene, "Multiplayer Wall", RenderFilter.MultiPlayerWall);
        }

        if (Project.Descriptor.ProjectType is ProjectType.AC6)
        {
            DisplayNavVolumeSlice();
        }

        GUI.Spacer();
        GUI.SimpleHeader("Collision Type", "Toggle which types of collision mesh are displayed. Several can be displayed at once.");

        foreach (var curType in View.HavokCollisionBank.GetSupportedCollisionTypes())
        {
            var visible = View.HavokCollisionBank.IsCollisionTypeVisible(curType);

            if (ImGui.Checkbox($"{LOC.Get(curType.GetDisplayName())}##colType{curType}", ref visible))
            {
                View.HavokCollisionBank.ToggleCollisionType(curType);
                View.DelayPicking();
            }
        }

        if (!(Project.Descriptor.ProjectType is ProjectType.DS2 or ProjectType.DS2S))
        {
            GUI.Spacer();
            GUI.SimpleHeader("Patrol Routes", "Toggle whether patrol routes are displayed.");

            // Read from the manager, as the filter menu and the shortcut also show and hide the routes
            var patrolVisType = View.PatrolDrawManager.IsVisible ? PatrolRouteVisType.Visible : PatrolRouteVisType.Hidden;

            GUI.SetInputWidth();
            if (ImGui.BeginCombo("##patrolVisType", patrolVisType.GetDisplayName()))
            {
                foreach (var entry in Enum.GetValues(typeof(PatrolRouteVisType)))
                {
                    var curType = (PatrolRouteVisType)entry;

                    if (ImGui.Selectable($"{curType.GetDisplayName()}", curType == patrolVisType))
                    {
                        if (curType is PatrolRouteVisType.Visible)
                        {
                            View.PatrolDrawManager.Generate();
                        }
                        else if (curType is PatrolRouteVisType.Hidden)
                        {
                            View.PatrolDrawManager.Clear();
                        }

                        View.DelayPicking();
                    }
                }

                ImGui.EndCombo();
            }
        }

        GUI.Spacer();
        GUI.ConditionalHeader("Region Filters", "Toggle individual region types.", ref DisplayRegionFilters);

        if (DisplayRegionFilters)
        {
            View.RegionFilters.DisplayToolWindowOptions();
        }
    }

    /// <summary>
    /// The state is read from the scene each frame, as the filter menu and presets change it too
    /// </summary>
    private void FilterCheckbox(RenderScene scene, string name, RenderFilter filter)
    {
        var enabled = scene.DrawFilter.HasFlag(filter);

        if (ImGui.Checkbox($"{name}##basicFilter{filter}", ref enabled))
        {
            scene.SetDrawFilter(filter, enabled);
            View.BasicFilters.StoreFilterState();
            View.DelayPicking();
        }
    }

    private void DisplayNavVolumeSlice()
    {
        GUI.Spacer();
        GUI.SimpleHeader("Nav Volume Slice", "Cut the nav volumes at a height, hiding everything above it, to see the space that can be flown through at that height.");

        var enabled = NavVolumeSlice.Enabled;
        if (ImGui.Checkbox("Slice##navVolumeSlice", ref enabled))
        {
            NavVolumeSlice.Enabled = enabled;

            // Nothing is shown of the volume while its filter is off
            var scene = View.ViewportHandler.ActiveViewport.RenderScene;
            if (enabled && !scene.DrawFilter.HasFlag(RenderFilter.NavVolume))
            {
                scene.SetDrawFilter(RenderFilter.NavVolume, true);
                View.BasicFilters.StoreFilterState();
            }

            // Starts in the middle of the volumes, as the last height may be outside of them
            if (enabled && GetNavVolumeHeightRange() is (float low, float high)
                && (NavVolumeSlice.Height < low || NavVolumeSlice.Height > high))
            {
                NavVolumeSlice.Height = (low + high) / 2.0f;
            }

            View.DelayPicking();
        }
        GUI.Tooltip("Hide the parts of the nav volumes above the slice height.");

        if (!NavVolumeSlice.Enabled)
            return;

        var height = NavVolumeSlice.Height;
        GUI.SetInputWidth();

        if (GetNavVolumeHeightRange() is (float min, float max))
        {
            if (ImGui.SliderFloat("Height##navVolumeSliceHeight", ref height, min, max, "%.1f"))
            {
                NavVolumeSlice.Height = height;
            }
        }
        else if (ImGui.DragFloat("Height##navVolumeSliceHeight", ref height, 1.0f))
        {
            NavVolumeSlice.Height = height;
        }
        GUI.Tooltip("The world height the nav volumes are cut at.");

        if (ImGui.Button("Use Camera Height##navVolumeSliceCamera"))
        {
            NavVolumeSlice.Height = View.ViewportHandler.ActiveViewport.Viewport.ViewportCamera.CameraTransform.Position.Y;
        }
        GUI.Tooltip("Cut the nav volumes at the height of the camera.");

        var crossSection = NavVolumeSlice.ShowCrossSection;
        if (ImGui.Checkbox("Show Cross-Section##navVolumeCrossSection", ref crossSection))
        {
            NavVolumeSlice.ShowCrossSection = crossSection;
        }
        GUI.Tooltip("Draw where the slice passes through open space as a flat orange surface, with gaps where flight is blocked. It is only drawn while the camera is above the slice.");
    }

    /// <summary>
    /// The world heights covered by the loaded nav volumes, or null if none are loaded yet
    /// </summary>
    private (float Min, float Max)? GetNavVolumeHeightRange()
    {
        float? min = null;
        float? max = null;

        foreach (var entry in View.Project.Handler.MapData.PrimaryBank.Maps)
        {
            var parent = entry.Value.MapContainer?.NavVolumeParent;

            if (parent == null)
                continue;

            foreach (var child in parent.Children)
            {
                if (child.RenderSceneMesh is not MeshRenderableProxy mesh)
                    continue;

                var bounds = mesh.GetBounds();

                // The bounds are empty until the volume has loaded
                if (bounds.Max.Y <= bounds.Min.Y)
                    continue;

                min = Math.Min(min ?? bounds.Min.Y, bounds.Min.Y);
                max = Math.Max(max ?? bounds.Max.Y, bounds.Max.Y);
            }
        }

        return min is float low && max is float high ? (low, high) : null;
    }
}

public enum PatrolRouteVisType
{
    [Display(Name = "Visible")]
    Visible,
    [Display(Name = "Hidden")]
    Hidden
}