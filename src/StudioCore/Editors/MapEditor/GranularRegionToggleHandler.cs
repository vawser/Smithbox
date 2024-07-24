using ImGuiNET;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Interface;
using StudioCore.MsbEditor;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor;

public class GranularRegionToggleHandler
{
    public Universe Universe { get; set; }

    public GranularRegionToggleHandler(Universe universe)
    {
        Universe = universe;
    }

    public void DisplayOptions()
    {
        switch (Smithbox.ProjectType)
        {
            // Supported Project Types
            case ProjectType.DS2:
            case ProjectType.DS2S:
                HandleGranularRegionToggles_DS2();
                break;
            case ProjectType.DS3:
                HandleGranularRegionToggles_DS3();
                break;
            case ProjectType.SDT:
                HandleGranularRegionToggles_SDT();
                break;
            case ProjectType.ER:
                HandleGranularRegionToggles_ER();
                break;
            case ProjectType.AC6:
                HandleGranularRegionToggles_AC6();
                break;
            default: break;
        }
    }

    private void HandleGranularRegionToggles_DS2()
    {
        DisplayCommonToggles();

        DisplayIndividualToggle(typeof(MSB2.Region.Region0), "Region0");
        DisplayIndividualToggle(typeof(MSB2.Region.Light), "Light");
        DisplayIndividualToggle(typeof(MSB2.Region.StartPoint), "Start Point");
        DisplayIndividualToggle(typeof(MSB2.Region.Sound), "Sound");
        DisplayIndividualToggle(typeof(MSB2.Region.SFX), "SFX");
        DisplayIndividualToggle(typeof(MSB2.Region.Wind), "Wind");
        DisplayIndividualToggle(typeof(MSB2.Region.EnvLight), "Env Light");
        DisplayIndividualToggle(typeof(MSB2.Region.Fog), "Fog");
    }
    private void HandleGranularRegionToggles_DS3()
    {
        DisplayCommonToggles();

        DisplayIndividualToggle(typeof(MSB3.Region.InvasionPoint), "Invasion Point");
        DisplayIndividualToggle(typeof(MSB3.Region.EnvironmentMapPoint), "Environment Map Point");
        DisplayIndividualToggle(typeof(MSB3.Region.Sound), "Sound");
        DisplayIndividualToggle(typeof(MSB3.Region.SFX), "SFX");
        DisplayIndividualToggle(typeof(MSB3.Region.WindSFX), "Wind SFX");
        DisplayIndividualToggle(typeof(MSB3.Region.SpawnPoint), "Spawn Point");
        DisplayIndividualToggle(typeof(MSB3.Region.Message), "Message");
        DisplayIndividualToggle(typeof(MSB3.Region.PatrolRoute), "Patrol Route");
        DisplayIndividualToggle(typeof(MSB3.Region.MovementPoint), "Movement Point");
        DisplayIndividualToggle(typeof(MSB3.Region.WarpPoint), "Warp Point");
        DisplayIndividualToggle(typeof(MSB3.Region.ActivationArea), "Activation Area");
        DisplayIndividualToggle(typeof(MSB3.Region.Event), "Event");
        DisplayIndividualToggle(typeof(MSB3.Region.Logic), "Logic");
        DisplayIndividualToggle(typeof(MSB3.Region.EnvironmentMapEffectBox), "Environment Map Effect Box");
        DisplayIndividualToggle(typeof(MSB3.Region.WindArea), "Wind Area");
        DisplayIndividualToggle(typeof(MSB3.Region.MufflingBox), "Muffling Box");
        DisplayIndividualToggle(typeof(MSB3.Region.MufflingPortal), "Muffling Portal");
        DisplayIndividualToggle(typeof(MSB3.Region.Other), "Other");
    }

    private void HandleGranularRegionToggles_SDT()
    {
        DisplayCommonToggles();

        DisplayIndividualToggle(typeof(MSBS.Region.InvasionPoint), "Invasion Point");
        DisplayIndividualToggle(typeof(MSBS.Region.EnvironmentMapPoint), "Environment Map Point");
        DisplayIndividualToggle(typeof(MSBS.Region.Sound), "Sound");
        DisplayIndividualToggle(typeof(MSBS.Region.SFX), "SFX");
        DisplayIndividualToggle(typeof(MSBS.Region.WindSFX), "Wind SFX");
        DisplayIndividualToggle(typeof(MSBS.Region.SpawnPoint), "Spawn Point");
        DisplayIndividualToggle(typeof(MSBS.Region.PatrolRoute), "Patrol Route");
        DisplayIndividualToggle(typeof(MSBS.Region.WarpPoint), "Warp Point");
        DisplayIndividualToggle(typeof(MSBS.Region.ActivationArea), "Activation Area");
        DisplayIndividualToggle(typeof(MSBS.Region.Event), "Event");
        DisplayIndividualToggle(typeof(MSBS.Region.Logic), "Logic");
        DisplayIndividualToggle(typeof(MSBS.Region.EnvironmentMapEffectBox), "Environment Map Effect Box");
        DisplayIndividualToggle(typeof(MSBS.Region.WindArea), "Wind Area");
        DisplayIndividualToggle(typeof(MSBS.Region.MufflingBox), "Muffling Box");
        DisplayIndividualToggle(typeof(MSBS.Region.MufflingPortal), "Muffling Portal");
        DisplayIndividualToggle(typeof(MSBS.Region.SoundSpaceOverride), "Sound Space Override");
        DisplayIndividualToggle(typeof(MSBS.Region.MufflingPlane), "Muffling Plane");
        DisplayIndividualToggle(typeof(MSBS.Region.PartsGroupArea), "Parts Group Area");
        DisplayIndividualToggle(typeof(MSBS.Region.AutoDrawGroupPoint), "Auto Draw Group Point");
        DisplayIndividualToggle(typeof(MSBS.Region.Other), "Other");
    }

    private void HandleGranularRegionToggles_ER()
    {
        DisplayCommonToggles();

        DisplayIndividualToggle(typeof(MSBE.Region.InvasionPoint), "Invasion Point");
        DisplayIndividualToggle(typeof(MSBE.Region.EnvironmentMapPoint), "Environment Map Point");
        DisplayIndividualToggle(typeof(MSBE.Region.Sound), "Sound");
        DisplayIndividualToggle(typeof(MSBE.Region.SFX), "SFX");
        DisplayIndividualToggle(typeof(MSBE.Region.WindSFX), "Wind SFX");
        DisplayIndividualToggle(typeof(MSBE.Region.SpawnPoint), "Spawn Point");
        DisplayIndividualToggle(typeof(MSBE.Region.Message), "Message");
        DisplayIndividualToggle(typeof(MSBE.Region.EnvironmentMapEffectBox), "Environment Map Effect Box");
        DisplayIndividualToggle(typeof(MSBE.Region.WindArea), "Wind Area");
        DisplayIndividualToggle(typeof(MSBE.Region.Connection), "Connection");
        DisplayIndividualToggle(typeof(MSBE.Region.PatrolRoute22), "Patrol Route 22");
        DisplayIndividualToggle(typeof(MSBE.Region.BuddySummonPoint), "Buddy Summon Point");
        DisplayIndividualToggle(typeof(MSBE.Region.DisableTumbleweed), "Disable Tumbleweed");
        DisplayIndividualToggle(typeof(MSBE.Region.MufflingBox), "Muffling Box");
        DisplayIndividualToggle(typeof(MSBE.Region.MufflingPortal), "Muffling Portal");
        DisplayIndividualToggle(typeof(MSBE.Region.SoundRegion), "Sound Region");
        DisplayIndividualToggle(typeof(MSBE.Region.MufflingPlane), "Muffling Plane");
        DisplayIndividualToggle(typeof(MSBE.Region.PatrolRoute), "Patrol Route");
        DisplayIndividualToggle(typeof(MSBE.Region.MapPoint), "Map Point");
        DisplayIndividualToggle(typeof(MSBE.Region.WeatherOverride), "Weather Override");
        DisplayIndividualToggle(typeof(MSBE.Region.AutoDrawGroupPoint), "Auto Draw Group Point");
        DisplayIndividualToggle(typeof(MSBE.Region.GroupDefeatReward), "Group Defeat Reward");
        DisplayIndividualToggle(typeof(MSBE.Region.MapPointDiscoveryOverride), "Map Point Discovery Override");
        DisplayIndividualToggle(typeof(MSBE.Region.MapPointParticipationOverride), "Map Point Participation Override");
        DisplayIndividualToggle(typeof(MSBE.Region.Hitset), "Hitset");
        DisplayIndividualToggle(typeof(MSBE.Region.FastTravelRestriction), "Fast Travel Restriction");
        DisplayIndividualToggle(typeof(MSBE.Region.WeatherCreateAssetPoint), "Weather Create Asset Point");
        DisplayIndividualToggle(typeof(MSBE.Region.PlayArea), "Play Area");
        DisplayIndividualToggle(typeof(MSBE.Region.EnvironmentMapOutput), "Environment Map Output");
        DisplayIndividualToggle(typeof(MSBE.Region.MountJump), "Mount Jump");
        DisplayIndividualToggle(typeof(MSBE.Region.Dummy), "Dummy");
        DisplayIndividualToggle(typeof(MSBE.Region.FallPreventionRemoval), "Fall Prevention Removal");
        DisplayIndividualToggle(typeof(MSBE.Region.NavmeshCutting), "Navmesh Cutting");
        DisplayIndividualToggle(typeof(MSBE.Region.MapNameOverride), "Map Name Override");
        DisplayIndividualToggle(typeof(MSBE.Region.MountJumpFall), "Mount Jump Fall");
        DisplayIndividualToggle(typeof(MSBE.Region.LockedMountJump), "Locked Mount Jump");
        DisplayIndividualToggle(typeof(MSBE.Region.LockedMountJumpFall), "Locked Mount Jump Fall");
        DisplayIndividualToggle(typeof(MSBE.Region.Other), "Other");
    }

    private void HandleGranularRegionToggles_AC6()
    {
        DisplayCommonToggles();

        DisplayIndividualToggle(typeof(MSB_AC6.Region.None), "None");
        DisplayIndividualToggle(typeof(MSB_AC6.Region.EntryPoint), "Entry Point");
        DisplayIndividualToggle(typeof(MSB_AC6.Region.EnvMapPoint), "Environment Map Point");
        DisplayIndividualToggle(typeof(MSB_AC6.Region.Sound), "Sound");
        DisplayIndividualToggle(typeof(MSB_AC6.Region.SFX), "SFX");
        DisplayIndividualToggle(typeof(MSB_AC6.Region.WindSFX), "Wind SFX");
        DisplayIndividualToggle(typeof(MSB_AC6.Region.EnvMapEffectBox), "Environment Map Effect Box");
        DisplayIndividualToggle(typeof(MSB_AC6.Region.WindPlacement), "Wind Placement");
        DisplayIndividualToggle(typeof(MSB_AC6.Region.MufflingBox), "Muffling Box");
        DisplayIndividualToggle(typeof(MSB_AC6.Region.MufflingPortal), "Muffling Portal");
        DisplayIndividualToggle(typeof(MSB_AC6.Region.SoundOverride), "Sound Override");
        DisplayIndividualToggle(typeof(MSB_AC6.Region.Patrol), "Patrol");
        DisplayIndividualToggle(typeof(MSB_AC6.Region.FeMapDisplay), "Fe Map Display");
        DisplayIndividualToggle(typeof(MSB_AC6.Region.OperationalArea), "Operational Area");
        DisplayIndividualToggle(typeof(MSB_AC6.Region.AiInformationSharing), "AI Information Sharing");
        DisplayIndividualToggle(typeof(MSB_AC6.Region.AiTarget), "AI Target");
        DisplayIndividualToggle(typeof(MSB_AC6.Region.WwiseEnvironmentSound), "Wwise Environment Sound");
        DisplayIndividualToggle(typeof(MSB_AC6.Region.NaviGeneration), "Navi Generation");
        DisplayIndividualToggle(typeof(MSB_AC6.Region.TopdownView), "Topdown View");
        DisplayIndividualToggle(typeof(MSB_AC6.Region.CharacterFollowing), "Character Following");
        DisplayIndividualToggle(typeof(MSB_AC6.Region.NaviCvCancel), "Navi Cv Cancel");
        DisplayIndividualToggle(typeof(MSB_AC6.Region.NavmeshCostControl), "Navmesh Cost Control");
        DisplayIndividualToggle(typeof(MSB_AC6.Region.ArenaAppearance), "Arena Appearance");
        DisplayIndividualToggle(typeof(MSB_AC6.Region.GarageCamera), "Garage Camera");
        DisplayIndividualToggle(typeof(MSB_AC6.Region.JumpEdgeRestriction), "Jump Edge Restriction");
        DisplayIndividualToggle(typeof(MSB_AC6.Region.CutscenePlayback), "Cutscene Playback");
        DisplayIndividualToggle(typeof(MSB_AC6.Region.FallPreventionWallRemoval), "Fall Prevention Wall Removal");
        DisplayIndividualToggle(typeof(MSB_AC6.Region.BigJump), "Big Jump");
        DisplayIndividualToggle(typeof(MSB_AC6.Region.Other), "Other");
    }

    public void DisplayCommonToggles()
    {
        ImguiUtils.ShowMenuIcon($"{ForkAwesome.Eye}");
        if (ImGui.MenuItem("Toggle Region Visibility: OFF"))
        {
            foreach (var entry in Universe.LoadedObjectContainers.Values)
            {
                if (entry is MapContainer)
                {
                    foreach (var child in entry.RootObject.Children)
                    {
                        if (child.WrappedObject is MSB_AC6.Region or MSBE.Region or MSBS.Region or MSB3.Region or MSB2.Region)
                        {
                            child.EditorVisible = false;
                        }
                    }
                }
            }
        }
        ImguiUtils.ShowHoverTooltip("Toggle the visibility of regions of all types to invisible.");

        ImguiUtils.ShowMenuIcon($"{ForkAwesome.Eye}");
        if (ImGui.MenuItem("Toggle Region Visibility: ON"))
        {
            foreach (var entry in Universe.LoadedObjectContainers.Values)
            {
                if (entry is MapContainer)
                {
                    foreach (var child in entry.RootObject.Children)
                    {
                        if (child.WrappedObject is MSB_AC6.Region or MSBE.Region or MSBS.Region or MSB3.Region or MSB2.Region)
                        {
                            child.EditorVisible = true;
                        }
                    }
                }
            }
        }
        ImguiUtils.ShowHoverTooltip("Toggle the visibility of regions of all types to visible.");

        ImGui.Separator();
    }

    public void DisplayIndividualToggle(Type regionType, string name)
    {
        var show = false;

        // Only show if region type is present
        foreach (var entry in Universe.LoadedObjectContainers.Values)
        {
            if (entry is MapContainer)
            {
                foreach (var child in entry.RootObject.Children)
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
            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Eye}");
            if (ImGui.MenuItem($"Toggle Region Visibility: {name}"))
            {
                foreach (var entry in Universe.LoadedObjectContainers.Values)
                {
                    if (entry is MapContainer)
                    {
                        foreach (var child in entry.RootObject.Children)
                        {
                            if (child.WrappedObject.GetType() == regionType)
                            {
                                child.EditorVisible = !child.EditorVisible;
                            }
                        }
                    }
                }
            }
            ImguiUtils.ShowHoverTooltip($"Toggle the visibility of regions of the {name} type.");
        }
    }
}
