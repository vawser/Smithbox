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

    private bool RenderMapPiece = false;
    private bool RenderObject = false;
    private bool RenderCharacter = false;
    private bool RenderDummyEnemy = false;
    private bool RenderDummyAsset = false;
    private bool RenderRegion = false;
    private bool RenderLight = false;
    private bool RenderCollision = false;
    private bool RenderConnectCollision = false;
    private bool RenderObjectCollision = false;
    private bool RenderGrass = false;
    private bool RenderEdgePoint = false;
    private bool RenderNavmesh = false;
    private bool RenderNavVolume = false;
    private bool RenderSpeedTree = false;
    private bool RenderInvasionPoint = false;
    private bool RenderMultiPlayerWall = false;

    private bool DisplayRegionFilters = true;
    private List<bool> RegionVisibilityTruth { get; set; } = new();

    private PatrolRouteVisType PatrolVisType = PatrolRouteVisType.Hidden;

    public ViewportFiltersAction(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;

        var drawFilter = View.ViewportHandler.ActiveViewport.RenderScene.DrawFilter;

        // Setup initial bool state
        RenderMapPiece = drawFilter.HasFlag(RenderFilter.MapPiece);
        RenderObject = drawFilter.HasFlag(RenderFilter.Object);
        RenderCharacter = drawFilter.HasFlag(RenderFilter.Character);
        RenderDummyEnemy = drawFilter.HasFlag(RenderFilter.DummyEnemy);
        RenderDummyAsset = drawFilter.HasFlag(RenderFilter.DummyAsset);
        RenderRegion = drawFilter.HasFlag(RenderFilter.Region);
        RenderLight = drawFilter.HasFlag(RenderFilter.Light);
        RenderCollision = drawFilter.HasFlag(RenderFilter.Collision);
        RenderConnectCollision = drawFilter.HasFlag(RenderFilter.ConnectCollision);
        RenderObjectCollision = drawFilter.HasFlag(RenderFilter.ObjectCollision);
        RenderGrass = drawFilter.HasFlag(RenderFilter.Grass);
        RenderEdgePoint = drawFilter.HasFlag(RenderFilter.EdgePoint);
        RenderNavmesh = drawFilter.HasFlag(RenderFilter.Navmesh);
        RenderNavVolume = drawFilter.HasFlag(RenderFilter.NavVolume);
        RenderSpeedTree = drawFilter.HasFlag(RenderFilter.SpeedTree);
        RenderInvasionPoint = drawFilter.HasFlag(RenderFilter.AutoInvade);
        RenderMultiPlayerWall = drawFilter.HasFlag(RenderFilter.MultiPlayerWall);

        SetupTruthList(true);
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

        if (ImGui.Checkbox("Map Piece", ref RenderMapPiece))
        {
            scene.ToggleDrawFilter(RenderFilter.MapPiece);
            RenderMapPiece = scene.DrawFilter.HasFlag(RenderFilter.MapPiece);
        }

        if (ImGui.Checkbox(objName, ref RenderObject))
        {
            scene.ToggleDrawFilter(RenderFilter.Object);
            RenderObject = scene.DrawFilter.HasFlag(RenderFilter.Object);
        }

        if (ImGui.Checkbox("Character", ref RenderCharacter))
        {
            scene.ToggleDrawFilter(RenderFilter.Character);
            RenderCharacter = scene.DrawFilter.HasFlag(RenderFilter.Character);
        }

        if (Project.Descriptor.ProjectType is not (ProjectType.DS2 or ProjectType.DS2S))
        {
            if (ImGui.Checkbox("Dummy Enemy", ref RenderDummyEnemy))
            {
                scene.ToggleDrawFilter(RenderFilter.DummyEnemy);
                RenderDummyEnemy = scene.DrawFilter.HasFlag(RenderFilter.DummyEnemy);
                CFG.Current.MapEditor_BasicFilter_DummyEnemy = RenderDummyEnemy;
            }

            if (ImGui.Checkbox($"Dummy {objName}", ref RenderDummyAsset))
            {
                scene.ToggleDrawFilter(RenderFilter.DummyAsset);
                RenderDummyAsset = scene.DrawFilter.HasFlag(RenderFilter.DummyAsset);
                CFG.Current.MapEditor_BasicFilter_DummyAsset = RenderDummyAsset;
            }
        }

        if (ImGui.Checkbox("Region", ref RenderRegion))
        {
            scene.ToggleDrawFilter(RenderFilter.Region);
            RenderRegion = scene.DrawFilter.HasFlag(RenderFilter.Region);
        }

        if (ImGui.Checkbox("Light", ref RenderLight))
        {
            scene.ToggleDrawFilter(RenderFilter.Light);
            RenderLight = scene.DrawFilter.HasFlag(RenderFilter.Light);
        }

        if (ImGui.Checkbox("Map Collision", ref RenderCollision))
        {
            scene.ToggleDrawFilter(RenderFilter.Collision);
            RenderCollision = scene.DrawFilter.HasFlag(RenderFilter.Collision);
        }

        if (ImGui.Checkbox("Connect Collision", ref RenderConnectCollision))
        {
            scene.ToggleDrawFilter(RenderFilter.ConnectCollision);
            RenderConnectCollision = scene.DrawFilter.HasFlag(RenderFilter.ConnectCollision);
        }

        if (ImGui.Checkbox($"{objName} Collision", ref RenderObjectCollision))
        {
            scene.ToggleDrawFilter(RenderFilter.ObjectCollision);
            RenderObjectCollision = scene.DrawFilter.HasFlag(RenderFilter.ObjectCollision);
            CFG.Current.MapEditor_BasicFilter_ObjectCollision = RenderObjectCollision;
        }

        if (ImGui.Checkbox("Grass", ref RenderGrass))
        {
            scene.ToggleDrawFilter(RenderFilter.Grass);
            RenderGrass = scene.DrawFilter.HasFlag(RenderFilter.Grass);
            CFG.Current.MapEditor_BasicFilter_Grass = RenderGrass;
        }

        if (Project.Descriptor.ProjectType is ProjectType.SDT)
        {
            if (ImGui.Checkbox("Edge Point", ref RenderEdgePoint))
            {
                scene.ToggleDrawFilter(RenderFilter.EdgePoint);
                RenderEdgePoint = scene.DrawFilter.HasFlag(RenderFilter.EdgePoint);
                CFG.Current.MapEditor_BasicFilter_EdgePoint = RenderEdgePoint;
            }
        }

        if (ImGui.Checkbox("Navmesh", ref RenderNavmesh))
        {
            scene.ToggleDrawFilter(RenderFilter.Navmesh);
            RenderNavmesh = scene.DrawFilter.HasFlag(RenderFilter.Navmesh);
        }

        if (Project.Descriptor.ProjectType is ProjectType.AC6)
        {
            if (ImGui.Checkbox("Nav Volume", ref RenderNavVolume))
            {
                scene.ToggleDrawFilter(RenderFilter.NavVolume);
                RenderNavVolume = scene.DrawFilter.HasFlag(RenderFilter.NavVolume);
                CFG.Current.MapEditor_BasicFilter_NavVolume = RenderNavVolume;
            }
        }

        if (ImGui.Checkbox("Speed Tree", ref RenderSpeedTree))
        {
            scene.ToggleDrawFilter(RenderFilter.SpeedTree);
            RenderSpeedTree = scene.DrawFilter.HasFlag(RenderFilter.SpeedTree);
        }

        if (ImGui.Checkbox("Invasion Point", ref RenderInvasionPoint))
        {
            scene.ToggleDrawFilter(RenderFilter.AutoInvade);
            RenderInvasionPoint = scene.DrawFilter.HasFlag(RenderFilter.AutoInvade);
        }

        if (ImGui.Checkbox("Multiplayer Wall", ref RenderMultiPlayerWall))
        {
            scene.ToggleDrawFilter(RenderFilter.MultiPlayerWall);
            RenderMultiPlayerWall = scene.DrawFilter.HasFlag(RenderFilter.MultiPlayerWall);
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

            if (ImGui.Checkbox($"{curType.GetDisplayName()}##colType{curType}", ref visible))
            {
                View.HavokCollisionBank.ToggleCollisionType(curType);
                View.DelayPicking();
            }
        }

        if (!(Project.Descriptor.ProjectType is ProjectType.DS2 or ProjectType.DS2S))
        {
            GUI.Spacer();
            GUI.SimpleHeader("Patrol Routes", "Toggle whether patrol routes are displayed.");

            GUI.SetInputWidth();
            if (ImGui.BeginCombo("##patrolVisType", PatrolVisType.GetDisplayName()))
            {
                foreach (var entry in Enum.GetValues(typeof(PatrolRouteVisType)))
                {
                    var curType = (PatrolRouteVisType)entry;

                    if (ImGui.Selectable($"{curType.GetDisplayName()}", curType == PatrolVisType))
                    {
                        PatrolVisType = curType;

                        if(PatrolVisType is PatrolRouteVisType.Visible)
                        {
                            View.PatrolDrawManager.Generate();
                        }
                        else if (PatrolVisType is PatrolRouteVisType.Hidden)
                        {
                            View.PatrolDrawManager.Clear();
                        }
                    }
                }

                ImGui.EndCombo();
            }
        }

        GUI.Spacer();
        GUI.ConditionalHeader("Region Filters", "Toggle individual region types.", ref DisplayRegionFilters);

        if (DisplayRegionFilters)
        {
            if (Project.Descriptor.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
            {
                DisplayIndividualToggle(typeof(MSB2.Region.Region0), "Region0", 0);
                DisplayIndividualToggle(typeof(MSB2.Region.Light), "Light", 1);
                DisplayIndividualToggle(typeof(MSB2.Region.StartPoint), "Start Point", 2);
                DisplayIndividualToggle(typeof(MSB2.Region.Sound), "Sound", 3);
                DisplayIndividualToggle(typeof(MSB2.Region.SFX), "SFX", 4);
                DisplayIndividualToggle(typeof(MSB2.Region.Wind), "Wind", 5);
                DisplayIndividualToggle(typeof(MSB2.Region.EnvLight), "Env Light", 6);
                DisplayIndividualToggle(typeof(MSB2.Region.Fog), "Fog", 7);
            }
            else if (Project.Descriptor.ProjectType is ProjectType.DS3)
            {
                DisplayIndividualToggle(typeof(MSB3.Region.InvasionPoint), "Invasion Point", 0);
                DisplayIndividualToggle(typeof(MSB3.Region.EnvironmentMapPoint), "Environment Map Point", 1);
                DisplayIndividualToggle(typeof(MSB3.Region.Sound), "Sound", 2);
                DisplayIndividualToggle(typeof(MSB3.Region.SFX), "SFX", 3);
                DisplayIndividualToggle(typeof(MSB3.Region.WindSFX), "Wind SFX", 4);
                DisplayIndividualToggle(typeof(MSB3.Region.SpawnPoint), "Spawn Point", 5);
                DisplayIndividualToggle(typeof(MSB3.Region.Message), "Message", 6);
                DisplayIndividualToggle(typeof(MSB3.Region.PatrolRoute), "Patrol Route", 7);
                DisplayIndividualToggle(typeof(MSB3.Region.MovementPoint), "Movement Point", 8);
                DisplayIndividualToggle(typeof(MSB3.Region.WarpPoint), "Warp Point", 9);
                DisplayIndividualToggle(typeof(MSB3.Region.ActivationArea), "Activation Area", 10);
                DisplayIndividualToggle(typeof(MSB3.Region.Event), "Event", 11);
                DisplayIndividualToggle(typeof(MSB3.Region.Logic), "Logic", 12);
                DisplayIndividualToggle(typeof(MSB3.Region.EnvironmentMapEffectBox), "Environment Map Effect Box", 13);
                DisplayIndividualToggle(typeof(MSB3.Region.WindArea), "Wind Area", 14);
                DisplayIndividualToggle(typeof(MSB3.Region.MufflingBox), "Muffling Box", 15);
                DisplayIndividualToggle(typeof(MSB3.Region.MufflingPortal), "Muffling Portal", 16);
                DisplayIndividualToggle(typeof(MSB3.Region.Other), "Other", 17);
            }
            else if (Project.Descriptor.ProjectType is ProjectType.SDT)
            {
                DisplayIndividualToggle(typeof(MSBS.Region.InvasionPoint), "Invasion Point", 0);
                DisplayIndividualToggle(typeof(MSBS.Region.EnvironmentMapPoint), "Environment Map Point", 1);
                DisplayIndividualToggle(typeof(MSBS.Region.Sound), "Sound", 2);
                DisplayIndividualToggle(typeof(MSBS.Region.SFX), "SFX", 3);
                DisplayIndividualToggle(typeof(MSBS.Region.WindSFX), "Wind SFX", 4);
                DisplayIndividualToggle(typeof(MSBS.Region.SpawnPoint), "Spawn Point", 5);
                DisplayIndividualToggle(typeof(MSBS.Region.PatrolRoute), "Patrol Route", 6);
                DisplayIndividualToggle(typeof(MSBS.Region.WarpPoint), "Warp Point", 7);
                DisplayIndividualToggle(typeof(MSBS.Region.ActivationArea), "Activation Area", 8);
                DisplayIndividualToggle(typeof(MSBS.Region.Event), "Event", 9);
                DisplayIndividualToggle(typeof(MSBS.Region.Logic), "Logic", 10);
                DisplayIndividualToggle(typeof(MSBS.Region.EnvironmentMapEffectBox), "Environment Map Effect Box", 11);
                DisplayIndividualToggle(typeof(MSBS.Region.WindArea), "Wind Area", 12);
                DisplayIndividualToggle(typeof(MSBS.Region.MufflingBox), "Muffling Box", 13);
                DisplayIndividualToggle(typeof(MSBS.Region.MufflingPortal), "Muffling Portal", 14);
                DisplayIndividualToggle(typeof(MSBS.Region.SoundSpaceOverride), "Sound Space Override", 15);
                DisplayIndividualToggle(typeof(MSBS.Region.MufflingPlane), "Muffling Plane", 16);
                DisplayIndividualToggle(typeof(MSBS.Region.PartsGroupArea), "Parts Group Area", 17);
                DisplayIndividualToggle(typeof(MSBS.Region.AutoDrawGroupPoint), "Auto Draw Group Point", 18);
                DisplayIndividualToggle(typeof(MSBS.Region.Other), "Other", 19);
            }
            else if (Project.Descriptor.ProjectType is ProjectType.ER)
            {
                DisplayIndividualToggle(typeof(MSBE.Region.InvasionPoint), "Invasion Point", 0);
                DisplayIndividualToggle(typeof(MSBE.Region.EnvironmentMapPoint), "Environment Map Point", 1);
                DisplayIndividualToggle(typeof(MSBE.Region.Sound), "Sound", 2);
                DisplayIndividualToggle(typeof(MSBE.Region.SFX), "SFX", 3);
                DisplayIndividualToggle(typeof(MSBE.Region.WindSFX), "Wind SFX", 4);
                DisplayIndividualToggle(typeof(MSBE.Region.SpawnPoint), "Spawn Point", 5);
                DisplayIndividualToggle(typeof(MSBE.Region.Message), "Message", 6);
                DisplayIndividualToggle(typeof(MSBE.Region.EnvironmentMapEffectBox), "Environment Map Effect Box", 7);
                DisplayIndividualToggle(typeof(MSBE.Region.WindArea), "Wind Area", 8);
                DisplayIndividualToggle(typeof(MSBE.Region.Connection), "Connection", 9);
                DisplayIndividualToggle(typeof(MSBE.Region.PatrolRoute22), "Patrol Route 22", 10);
                DisplayIndividualToggle(typeof(MSBE.Region.BuddySummonPoint), "Buddy Summon Point", 11);
                DisplayIndividualToggle(typeof(MSBE.Region.DisableTumbleweed), "Disable Tumbleweed", 12);
                DisplayIndividualToggle(typeof(MSBE.Region.MufflingBox), "Muffling Box", 13);
                DisplayIndividualToggle(typeof(MSBE.Region.MufflingPortal), "Muffling Portal", 14);
                DisplayIndividualToggle(typeof(MSBE.Region.SoundRegion), "Sound Region", 15);
                DisplayIndividualToggle(typeof(MSBE.Region.MufflingPlane), "Muffling Plane", 16);
                DisplayIndividualToggle(typeof(MSBE.Region.PatrolRoute), "Patrol Route", 17);
                DisplayIndividualToggle(typeof(MSBE.Region.MapPoint), "Map Point", 18);
                DisplayIndividualToggle(typeof(MSBE.Region.WeatherOverride), "Weather Override", 19);
                DisplayIndividualToggle(typeof(MSBE.Region.AutoDrawGroupPoint), "Auto Draw Group Point", 20);
                DisplayIndividualToggle(typeof(MSBE.Region.GroupDefeatReward), "Group Defeat Reward", 21);
                DisplayIndividualToggle(typeof(MSBE.Region.MapPointDiscoveryOverride), "Map Point Discovery Override", 22);
                DisplayIndividualToggle(typeof(MSBE.Region.MapPointParticipationOverride), "Map Point Participation Override", 23);
                DisplayIndividualToggle(typeof(MSBE.Region.Hitset), "Hitset", 24);
                DisplayIndividualToggle(typeof(MSBE.Region.FastTravelRestriction), "Fast Travel Restriction", 25);
                DisplayIndividualToggle(typeof(MSBE.Region.WeatherCreateAssetPoint), "Weather Create Asset Point", 26);
                DisplayIndividualToggle(typeof(MSBE.Region.PlayArea), "Play Area", 27);
                DisplayIndividualToggle(typeof(MSBE.Region.EnvironmentMapOutput), "Environment Map Output", 28);
                DisplayIndividualToggle(typeof(MSBE.Region.MountJump), "Mount Jump", 29);
                DisplayIndividualToggle(typeof(MSBE.Region.Dummy), "Dummy", 30);
                DisplayIndividualToggle(typeof(MSBE.Region.FallPreventionRemoval), "Fall Prevention Removal", 31);
                DisplayIndividualToggle(typeof(MSBE.Region.NavmeshCutting), "Navmesh Cutting", 32);
                DisplayIndividualToggle(typeof(MSBE.Region.MapNameOverride), "Map Name Override", 33);
                DisplayIndividualToggle(typeof(MSBE.Region.MountJumpFall), "Mount Jump Fall", 34);
                DisplayIndividualToggle(typeof(MSBE.Region.LockedMountJump), "Locked Mount Jump", 35);
                DisplayIndividualToggle(typeof(MSBE.Region.LockedMountJumpFall), "Locked Mount Jump Fall", 36);
                DisplayIndividualToggle(typeof(MSBE.Region.Other), "Other", 37);
            }
            else if (Project.Descriptor.ProjectType is ProjectType.AC6)
            {
                DisplayIndividualToggle(typeof(MSB_AC6.Region.EntryPoint), "Entry Point", 1);
                DisplayIndividualToggle(typeof(MSB_AC6.Region.EnvMapPoint), "Environment Map Point", 2);
                DisplayIndividualToggle(typeof(MSB_AC6.Region.Sound), "Sound", 3);
                DisplayIndividualToggle(typeof(MSB_AC6.Region.SFX), "SFX", 4);
                DisplayIndividualToggle(typeof(MSB_AC6.Region.WindSFX), "Wind SFX", 5);
                DisplayIndividualToggle(typeof(MSB_AC6.Region.EnvMapEffectBox), "Environment Map Effect Box", 6);
                DisplayIndividualToggle(typeof(MSB_AC6.Region.WindPlacement), "Wind Placement", 7);
                DisplayIndividualToggle(typeof(MSB_AC6.Region.MufflingBox), "Muffling Box", 8);
                DisplayIndividualToggle(typeof(MSB_AC6.Region.MufflingPortal), "Muffling Portal", 9);
                DisplayIndividualToggle(typeof(MSB_AC6.Region.SoundOverride), "Sound Override", 10);
                DisplayIndividualToggle(typeof(MSB_AC6.Region.Patrol), "Patrol", 11);
                DisplayIndividualToggle(typeof(MSB_AC6.Region.FeMapDisplay), "Fe Map Display", 12);
                DisplayIndividualToggle(typeof(MSB_AC6.Region.OperationalArea), "Operational Area", 13);
                DisplayIndividualToggle(typeof(MSB_AC6.Region.AiInformationSharing), "AI Information Sharing", 14);
                DisplayIndividualToggle(typeof(MSB_AC6.Region.AiTarget), "AI Target", 15);
                DisplayIndividualToggle(typeof(MSB_AC6.Region.WwiseEnvironmentSound), "Wwise Environment Sound", 16);
                DisplayIndividualToggle(typeof(MSB_AC6.Region.NaviGeneration), "Navi Generation", 17);
                DisplayIndividualToggle(typeof(MSB_AC6.Region.TopdownView), "Topdown View", 18);
                DisplayIndividualToggle(typeof(MSB_AC6.Region.CharacterFollowing), "Character Following", 19);
                DisplayIndividualToggle(typeof(MSB_AC6.Region.NavmeshCostControl), "Navmesh Cost Control", 21);
                DisplayIndividualToggle(typeof(MSB_AC6.Region.ArenaAppearance), "Arena Appearance", 22);
                DisplayIndividualToggle(typeof(MSB_AC6.Region.GarageCamera), "Garage Camera", 23);
                DisplayIndividualToggle(typeof(MSB_AC6.Region.JumpEdgeRestriction), "Jump Edge Restriction", 24);
                DisplayIndividualToggle(typeof(MSB_AC6.Region.CutscenePlayback), "Cutscene Playback", 25);
                DisplayIndividualToggle(typeof(MSB_AC6.Region.FallPreventionWallRemoval), "Fall Prevention Wall Removal", 26);
                DisplayIndividualToggle(typeof(MSB_AC6.Region.BigJump), "Big Jump", 27);
                DisplayIndividualToggle(typeof(MSB_AC6.Region.Other), "Other", 28);
            }
            else
            {
                GUI.WrappedText("Not supported for this project type.");
            }
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
            if (enabled && !RenderNavVolume)
            {
                View.ViewportHandler.ActiveViewport.RenderScene.DrawFilter |= RenderFilter.NavVolume;
                RenderNavVolume = true;
                CFG.Current.MapEditor_BasicFilter_NavVolume = true;
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

    public void DisplayIndividualToggle(Type regionType, string name, int truthIndex)
    {
        var show = false;
        var curBool = RegionVisibilityTruth[truthIndex];

        // Only show if region type is present
        foreach (var entry in View.Project.Handler.MapData.PrimaryBank.Maps)
        {
            if (entry.Value.MapContainer != null)
            {
                foreach (var child in entry.Value.MapContainer.RootObject.Children)
                {
                    if (child.WrappedObject.GetType() == regionType)
                    {
                        show = true;
                        break;
                    }
                }
            }
        }

        if (show)
        {
            if (ImGui.Checkbox($"{name}", ref curBool))
            {
                foreach (var entry in View.Project.Handler.MapData.PrimaryBank.Maps)
                {
                    if (entry.Value.MapContainer != null)
                    {
                        foreach (var child in entry.Value.MapContainer.RootObject.Children)
                        {
                            if (child.WrappedObject.GetType() == regionType)
                            {
                                child.EditorVisible = !child.EditorVisible;
                                RegionVisibilityTruth[truthIndex] = child.EditorVisible;
                            }
                        }
                    }
                }

                View.DelayPicking();
            }
            GUI.Tooltip($"Toggle the visibility of regions of the {name} type.");
        }
    }

    public void SetupTruthList(bool defaultValue)
    {
        switch (View.Project.Descriptor.ProjectType)
        {
            // Supported Project Types
            case ProjectType.DS2:
            case ProjectType.DS2S:
                RegionVisibilityTruth = Enumerable.Repeat(defaultValue, 8).ToList();
                break;
            case ProjectType.DS3:
                RegionVisibilityTruth = Enumerable.Repeat(defaultValue, 18).ToList();
                break;
            case ProjectType.SDT:
                RegionVisibilityTruth = Enumerable.Repeat(defaultValue, 20).ToList();
                break;
            case ProjectType.ER:
                RegionVisibilityTruth = Enumerable.Repeat(defaultValue, 38).ToList();
                break;
            case ProjectType.AC6:
                RegionVisibilityTruth = Enumerable.Repeat(defaultValue, 29).ToList();
                break;
            default: break;
        }
    }
}

public enum PatrolRouteVisType
{
    [Display(Name = "Visible")]
    Visible,
    [Display(Name = "Hidden")]
    Hidden
}