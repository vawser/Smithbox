using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.MapEditor;

public class RegionFilters
{
    private MapEditorView View;

    public List<bool> RegionVisibilityTruth { get; set; }

    private Dictionary<Type, string> RegionTypes_DS2 = new Dictionary<Type, string>()
    {
        { typeof(MSB2.Region.Region0), "MAP_Filters_DS2_Region0" },
        { typeof(MSB2.Region.Light), "MAP_Filters_DS2_Light" },
        { typeof(MSB2.Region.StartPoint), "MAP_Filters_DS2_Start_Point" },
        { typeof(MSB2.Region.Sound), "MAP_Filters_DS2_Sound" },
        { typeof(MSB2.Region.SFX), "MAP_Filters_DS2_SFX" },
        { typeof(MSB2.Region.Wind), "MAP_Filters_DS2_Wind" },
        { typeof(MSB2.Region.EnvLight), "MAP_Filters_DS2_Env_Light" },
        { typeof(MSB2.Region.Fog), "MAP_Filters_DS2_Fog" }
    };

    private Dictionary<Type, string> RegionTypes_DS3 = new Dictionary<Type, string>()
    {
        { typeof(MSB3.Region.InvasionPoint), "MAP_Filters_DS3_Invasion_Point" },
        { typeof(MSB3.Region.EnvironmentMapPoint), "MAP_Filters_DS3_Environment_Map_Point" },
        { typeof(MSB3.Region.Sound), "MAP_Filters_DS3_Sound" },
        { typeof(MSB3.Region.SFX), "MAP_Filters_DS3_SFX" },
        { typeof(MSB3.Region.WindSFX), "MAP_Filters_DS3_Wind_SFX" },
        { typeof(MSB3.Region.SpawnPoint), "MAP_Filters_DS3_Spawn_Point" },
        { typeof(MSB3.Region.Message), "MAP_Filters_DS3_Message" },
        { typeof(MSB3.Region.PatrolRoute), "MAP_Filters_DS3_Patrol_Route" },
        { typeof(MSB3.Region.MovementPoint), "MAP_Filters_DS3_Movement_Point" },
        { typeof(MSB3.Region.WarpPoint), "MAP_Filters_DS3_Warp_Point" },
        { typeof(MSB3.Region.ActivationArea), "MAP_Filters_DS3_Activation_Area" },
        { typeof(MSB3.Region.Event), "MAP_Filters_DS3_Event" },
        { typeof(MSB3.Region.Logic), "MAP_Filters_DS3_Logic" },
        { typeof(MSB3.Region.EnvironmentMapEffectBox), "MAP_Filters_DS3_Environment_Map_Effect_Box" },
        { typeof(MSB3.Region.WindArea), "MAP_Filters_DS3_Wind_Area" },
        { typeof(MSB3.Region.MufflingBox), "MAP_Filters_DS3_Muffling_Box" },
        { typeof(MSB3.Region.MufflingPortal), "MAP_Filters_DS3_Muffling_Portal" },
        { typeof(MSB3.Region.Other), "MAP_Filters_DS3_Other" }
    };

    private Dictionary<Type, string> RegionTypes_SDT = new Dictionary<Type, string>()
    {
        { typeof(MSBS.Region.InvasionPoint), "MAP_Filters_SDT_Invasion_Point" },
        { typeof(MSBS.Region.EnvironmentMapPoint), "MAP_Filters_SDT_Environment_Map_Point" },
        { typeof(MSBS.Region.Sound), "MAP_Filters_SDT_Sound" },
        { typeof(MSBS.Region.SFX), "MAP_Filters_SDT_Wind_SFX" },
        { typeof(MSBS.Region.WindSFX), "MAP_Filters_SDT_Wind_SFX" },
        { typeof(MSBS.Region.SpawnPoint), "MAP_Filters_SDT_Spawn_Point" },
        { typeof(MSBS.Region.PatrolRoute), "MAP_Filters_SDT_Patrol_Route" },
        { typeof(MSBS.Region.WarpPoint), "MAP_Filters_SDT_Warp_Point" },
        { typeof(MSBS.Region.ActivationArea), "MAP_Filters_SDT_Activation_Area" },
        { typeof(MSBS.Region.Event), "MAP_Filters_SDT_Event" },
        { typeof(MSBS.Region.Logic), "MAP_Filters_SDT_Logic" },
        { typeof(MSBS.Region.EnvironmentMapEffectBox), "MAP_Filters_SDT_Environment_Map_Effect" },
        { typeof(MSBS.Region.WindArea), "MAP_Filters_SDT_Wind_Area" },
        { typeof(MSBS.Region.MufflingBox), "MAP_Filters_SDT_Muffling_Box" },
        { typeof(MSBS.Region.MufflingPortal), "MAP_Filters_SDT_Muffling_Portal" },
        { typeof(MSBS.Region.SoundSpaceOverride), "MAP_Filters_SDT_Sound_Space_Override" },
        { typeof(MSBS.Region.MufflingPlane), "MAP_Filters_SDT_Muffling_Plane" },
        { typeof(MSBS.Region.PartsGroupArea), "MAP_Filters_SDT_Parts_Group_Area" },
        { typeof(MSBS.Region.AutoDrawGroupPoint), "MAP_Filters_SDT_Auto_Draw_Group_Point" },
        { typeof(MSBS.Region.Other), "MAP_Filters_SDT_Other" },
    };

    private Dictionary<Type, string> RegionTypes_ER = new Dictionary<Type, string>()
    {
        { typeof(MSBE.Region.InvasionPoint), "MAP_Filters_ER_Invasion_Point" },
        { typeof(MSBE.Region.EnvironmentMapPoint), "MAP_Filters_ER_Environment_Map_Point" },
        { typeof(MSBE.Region.Sound), "MAP_Filters_ER_Sound" },
        { typeof(MSBE.Region.SFX), "MAP_Filters_ER_SFX" },
        { typeof(MSBE.Region.WindSFX), "MAP_Filters_ER_Wind_SFX" },
        { typeof(MSBE.Region.SpawnPoint), "MAP_Filters_ER_Spawn_Point" },
        { typeof(MSBE.Region.Message), "MAP_Filters_ER_Message" },
        { typeof(MSBE.Region.EnvironmentMapEffectBox), "MAP_Filters_ER_Environment_Map_Effect_Box" },
        { typeof(MSBE.Region.WindArea), "MAP_Filters_ER_Wind_Area" },
        { typeof(MSBE.Region.Connection), "MAP_Filters_ER_Connection" },
        { typeof(MSBE.Region.PatrolRoute22), "MAP_Filters_ER_Patrol_Route_22" },
        { typeof(MSBE.Region.BuddySummonPoint), "MAP_Filters_ER_Buddy_Summon_Point" },
        { typeof(MSBE.Region.DisableTumbleweed), "MAP_Filters_ER_Disable_Tumbleweed" },
        { typeof(MSBE.Region.MufflingBox), "MAP_Filters_ER_Muffling_Box" },
        { typeof(MSBE.Region.MufflingPortal), "MAP_Filters_ER_Muffling_Portal" },
        { typeof(MSBE.Region.SoundRegion), "MAP_Filters_ER_Sound_Region" },
        { typeof(MSBE.Region.MufflingPlane), "MAP_Filters_ER_Muffling_Plane" },
        { typeof(MSBE.Region.PatrolRoute), "MAP_Filters_ER_Patrol_Route" },
        { typeof(MSBE.Region.MapPoint), "MAP_Filters_ER_Map_Point" },
        { typeof(MSBE.Region.WeatherOverride), "MAP_Filters_ER_Weather_Override" },
        { typeof(MSBE.Region.AutoDrawGroupPoint), "MAP_Filters_ER_Auto_Draw_Group_Point" },
        { typeof(MSBE.Region.GroupDefeatReward), "MAP_Filters_ER_Group_Defeat_Reward" },
        { typeof(MSBE.Region.MapPointDiscoveryOverride), "MAP_Filters_ER_Map_Point_Discovery_Override" },
        { typeof(MSBE.Region.MapPointParticipationOverride), "MAP_Filters_ER_Map_Point_Participation_Override" },
        { typeof(MSBE.Region.Hitset), "MAP_Filters_ER_Hitset" },
        { typeof(MSBE.Region.FastTravelRestriction), "MAP_Filters_ER_Fast_Travel_Restriction" },
        { typeof(MSBE.Region.WeatherCreateAssetPoint), "MAP_Filters_ER_Weather_Create_Asset_Point" },
        { typeof(MSBE.Region.PlayArea), "MAP_Filters_ER_Play_Area" },
        { typeof(MSBE.Region.EnvironmentMapOutput), "MAP_Filters_ER_Environment_Map_Output" },
        { typeof(MSBE.Region.MountJump), "MAP_Filters_ER_Mount_Jump" },
        { typeof(MSBE.Region.Dummy), "MAP_Filters_ER_Dummy" },
        { typeof(MSBE.Region.FallPreventionRemoval), "MAP_Filters_ER_Fall_Prevention_Removal" },
        { typeof(MSBE.Region.NavmeshCutting), "MAP_Filters_ER_Navmesh_Cutting" },
        { typeof(MSBE.Region.MapNameOverride), "MAP_Filters_ER_Map_Name_Override" },
        { typeof(MSBE.Region.MountJumpFall), "MAP_Filters_ER_Mount_Jump_Fall" },
        { typeof(MSBE.Region.LockedMountJump), "MAP_Filters_ER_Locked_Mount_Jump" },
        { typeof(MSBE.Region.LockedMountJumpFall), "MAP_Filters_ER_Locked_Mount_Jump_Fall" },
        { typeof(MSBE.Region.Other), "MAP_Filters_ER_Other" },
    };

    private Dictionary<Type, string> RegionTypes_AC6 = new Dictionary<Type, string>()
    {
        { typeof(MSB_AC6.Region.EntryPoint), "MAP_Filters_AC6_Entry_Point" },
        { typeof(MSB_AC6.Region.EnvMapPoint), "MAP_Filters_AC6_Environment_Map_Point" },
        { typeof(MSB_AC6.Region.Sound), "MAP_Filters_AC6_Sound" },
        { typeof(MSB_AC6.Region.SFX), "MAP_Filters_AC6_SFX" },
        { typeof(MSB_AC6.Region.WindSFX), "MAP_Filters_AC6_Wind_SFX" },
        { typeof(MSB_AC6.Region.EnvMapEffectBox), "MAP_Filters_AC6_Environment_Map_Effect_Box" },
        { typeof(MSB_AC6.Region.WindPlacement), "MAP_Filters_AC6_Wind_Placement" },
        { typeof(MSB_AC6.Region.MufflingBox), "MAP_Filters_AC6_Muffling_Box" },
        { typeof(MSB_AC6.Region.MufflingPortal), "MAP_Filters_AC6_Muffling_Portal" },
        { typeof(MSB_AC6.Region.SoundOverride), "MAP_Filters_AC6_Sound_Override" },
        { typeof(MSB_AC6.Region.Patrol), "MAP_Filters_AC6_Patrol" },
        { typeof(MSB_AC6.Region.FeMapDisplay), "MAP_Filters_AC6_FE_Map_Display" },
        { typeof(MSB_AC6.Region.OperationalArea), "MAP_Filters_AC6_Operational_Area" },
        { typeof(MSB_AC6.Region.AiInformationSharing), "MAP_Filters_AC6_AI_Information_Sharing" },
        { typeof(MSB_AC6.Region.AiTarget), "MAP_Filters_AC6_AI_Target" },
        { typeof(MSB_AC6.Region.WwiseEnvironmentSound), "MAP_Filters_AC6_Wwise_Environment_Sound" },
        { typeof(MSB_AC6.Region.NaviGeneration), "MAP_Filters_AC6_Navi_Generation" },
        { typeof(MSB_AC6.Region.TopdownView), "MAP_Filters_AC6_Topdown_View" },
        { typeof(MSB_AC6.Region.CharacterFollowing), "MAP_Filters_AC6_Character_Following" },
        { typeof(MSB_AC6.Region.NavmeshCostControl), "MAP_Filters_AC6_Navmesh_Cost_Control" },
        { typeof(MSB_AC6.Region.ArenaAppearance), "MAP_Filters_AC6_Arena_Appearance" },
        { typeof(MSB_AC6.Region.GarageCamera), "MAP_Filters_AC6_Garage_Camera" },
        { typeof(MSB_AC6.Region.JumpEdgeRestriction), "MAP_Filters_AC6_Jump_Edge_Restriction" },
        { typeof(MSB_AC6.Region.CutscenePlayback), "MAP_Filters_AC6_Cutscene_Playback" },
        { typeof(MSB_AC6.Region.FallPreventionWallRemoval), "MAP_Filters_AC6_Fall_Prevention_Wall_Removal" },
        { typeof(MSB_AC6.Region.BigJump), "MAP_Filters_AC6_Big_Jump" },
        { typeof(MSB_AC6.Region.Other), "MAP_Filters_AC6_Other" }
    };

    private Dictionary<Type, string> RegionTypes_NR = new Dictionary<Type, string>()
    {
        { typeof(MSB_NR.Region.EntryPoint), "MAP_Filters_NR_Entry_Point" },
        { typeof(MSB_NR.Region.EnvMapPoint), "MAP_Filters_NR_Env_Map_Point" },
        { typeof(MSB_NR.Region.RespawnPoint), "MAP_Filters_NR_Respawn_Point" },
        { typeof(MSB_NR.Region.Sound), "MAP_Filters_NR_Sound" },
        { typeof(MSB_NR.Region.SFX), "MAP_Filters_NR_SFX" },
        { typeof(MSB_NR.Region.WindSFX), "MAP_Filters_NR_Wind_SFX" },
        { typeof(MSB_NR.Region.ReturnPoint), "MAP_Filters_NR_Return_Point" },
        { typeof(MSB_NR.Region.Message), "MAP_Filters_NR_Message" },
        { typeof(MSB_NR.Region.EnvMapEffectBox), "MAP_Filters_NR_Env_Map_Effect_Box" },
        { typeof(MSB_NR.Region.WindPlacement), "MAP_Filters_NR_Wind_Placement" },
        { typeof(MSB_NR.Region.MapConnection), "MAP_Filters_NR_Map_Connection" },
        { typeof(MSB_NR.Region.SourceWaypoint), "MAP_Filters_NR_Source_Waypoint" },
        { typeof(MSB_NR.Region.StaticWaypoint), "MAP_Filters_NR_Static_Waypoint" },
        { typeof(MSB_NR.Region.MapGridLayerConnection), "MAP_Filters_NR_Map_Grid_Layer_Connection" },
        { typeof(MSB_NR.Region.EnemySpawnPoint), "MAP_Filters_NR_Enemy_Spawn_Point" },
        { typeof(MSB_NR.Region.BuddySummonPoint), "MAP_Filters_NR_Buddy_Summon_Point" },
        { typeof(MSB_NR.Region.RollingObjectOverride), "MAP_Filters_NR_Rolling_Object_Override" },
        { typeof(MSB_NR.Region.MufflingBox), "MAP_Filters_NR_Muffling_Box" },
        { typeof(MSB_NR.Region.MufflingPortal), "MAP_Filters_NR_Muffling_Portal" },
        { typeof(MSB_NR.Region.SoundOverride), "MAP_Filters_NR_Sound_Override" },
        { typeof(MSB_NR.Region.MufflingPlane), "MAP_Filters_NR_Muffling_Plane" },
        { typeof(MSB_NR.Region.PatrolPoint), "MAP_Filters_NR_Patrol_Point" },
        { typeof(MSB_NR.Region.MapPoint), "MAP_Filters_NR_Map_Point" },
        { typeof(MSB_NR.Region.SoundState), "MAP_Filters_NR_Sound_State" },
        { typeof(MSB_NR.Region.MapInfoOverride), "MAP_Filters_NR_Map_Info_Override" },
        { typeof(MSB_NR.Region.AutoDrawGroupSample), "MAP_Filters_NR_Auto_Draw_Group_Sample" },
        { typeof(MSB_NR.Region.MassPlacement), "MAP_Filters_NR_Mass_Placement" },
        { typeof(MSB_NR.Region.MapPointDiscoveryOverride), "MAP_Filters_NR_Map_Point_Discovery_Override" },
        { typeof(MSB_NR.Region.MapPointParticipationOverride), "MAP_Filters_NR_Map_Point_Participation_Override" },
        { typeof(MSB_NR.Region.HitSetting), "MAP_Filters_NR_Hit_Setting" },
        { typeof(MSB_NR.Region.FastTravelOverride), "MAP_Filters_NR_Fast_Travel_Override" },
        { typeof(MSB_NR.Region.WeatherAssetGeneration), "MAP_Filters_NR_Weather_Asset_Generation" },
        { typeof(MSB_NR.Region.PlayArea), "MAP_Filters_NR_Play_Area" },
        { typeof(MSB_NR.Region.MidRangeEnvMapOutput), "MAP_Filters_NR_Mid_Range_Env_Map_Output" },
        { typeof(MSB_NR.Region.MapVisibilityOverride), "MAP_Filters_NR_Map_Visibility_Override" },
        { typeof(MSB_NR.Region.BigJump), "MAP_Filters_NR_Big_Jump" },
        { typeof(MSB_NR.Region.OpenCharacterActivateLimit), "MAP_Filters_NR_Open_Character_Activate_Limit" },
        { typeof(MSB_NR.Region.SoundDummy), "MAP_Filters_NR_Sound_Dummy" },
        { typeof(MSB_NR.Region.FallPreventionOverride), "MAP_Filters_NR_Fall_Prevention_Override" },
        { typeof(MSB_NR.Region.NavmeshCutting), "MAP_Filters_NR_Navmesh_Cutting" },
        { typeof(MSB_NR.Region.MapNameOverride), "MAP_Filters_NR_Map_Name_Override" },
        { typeof(MSB_NR.Region.BigJumpExit), "MAP_Filters_NR_Big_Jump_Exit" },
        { typeof(MSB_NR.Region.MountOverride), "MAP_Filters_NR_Mount_Override" },
        { typeof(MSB_NR.Region.SmallBaseAttach), "MAP_Filters_NR_Small_Base_Attach" },
        { typeof(MSB_NR.Region.BirdRoute), "MAP_Filters_NR_Bird_Route" },
        { typeof(MSB_NR.Region.ClearInfo), "MAP_Filters_NR_Clear_Info" },
        { typeof(MSB_NR.Region.RespawnOverride), "MAP_Filters_NR_Respawn_Override" },
        { typeof(MSB_NR.Region.UserEdgeRemovalInner), "MAP_Filters_NR_User_Edge_Removal_Inner" },
        { typeof(MSB_NR.Region.UserEdgeRemovalOuter), "MAP_Filters_NR_User_Edge_Removal_Outer" },
        { typeof(MSB_NR.Region.BigJumpSealable), "MAP_Filters_NR_Big_Jump_Sealable" },
        { typeof(MSB_NR.Region.Other), "MAP_Filters_NR_Other" },
    };

    public RegionFilters(MapEditorView view)
    {
        View = view;

        SetupTruthList(true);
    }

    public void OnProjectChanged()
    {
        SetupTruthList(true);
    }

    public void SetupTruthList(bool defaultValue)
    {
        switch (View.Project.Descriptor.ProjectType)
        {
            // Supported Project Types
            case ProjectType.DS2:
            case ProjectType.DS2S:
                RegionVisibilityTruth = Enumerable.Repeat(defaultValue, RegionTypes_DS2.Count).ToList();
                break;
            case ProjectType.DS3:
                RegionVisibilityTruth = Enumerable.Repeat(defaultValue, RegionTypes_DS3.Count).ToList();
                break;
            case ProjectType.SDT:
                RegionVisibilityTruth = Enumerable.Repeat(defaultValue, RegionTypes_SDT.Count).ToList();
                break;
            case ProjectType.ER:
                RegionVisibilityTruth = Enumerable.Repeat(defaultValue, RegionTypes_ER.Count).ToList();
                break;
            case ProjectType.AC6:
                RegionVisibilityTruth = Enumerable.Repeat(defaultValue, RegionTypes_AC6.Count).ToList();
                break;
            case ProjectType.NR:
                RegionVisibilityTruth = Enumerable.Repeat(defaultValue, RegionTypes_NR.Count).ToList();
                break;
            default: break;
        }
    }

    public void DisplayOptions()
    {
        switch (View.Project.Descriptor.ProjectType)
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
            case ProjectType.NR:
                HandleGranularRegionToggles_NR();
                break;
            default: break;
        }
    }

    private void HandleGranularRegionToggles_DS2()
    {
        DisplayCommonToggles();

        for(int i = 0; i < RegionTypes_DS2.Count; i++)
        {
            var type = RegionTypes_DS2.ElementAt(i).Key;
            var locKey = RegionTypes_DS2.ElementAt(i).Value;

            DisplayIndividualToggle(type, LOC.Get(locKey), i);
        }
    }
    private void HandleGranularRegionToggles_DS3()
    {
        DisplayCommonToggles();

        for (int i = 0; i < RegionTypes_DS3.Count; i++)
        {
            var type = RegionTypes_DS3.ElementAt(i).Key;
            var locKey = RegionTypes_DS3.ElementAt(i).Value;

            DisplayIndividualToggle(type, LOC.Get(locKey), i);
        }
    }

    private void HandleGranularRegionToggles_SDT()
    {
        DisplayCommonToggles();

        for (int i = 0; i < RegionTypes_SDT.Count; i++)
        {
            var type = RegionTypes_SDT.ElementAt(i).Key;
            var locKey = RegionTypes_SDT.ElementAt(i).Value;

            DisplayIndividualToggle(type, LOC.Get(locKey), i);
        }
    }

    private void HandleGranularRegionToggles_ER()
    {
        DisplayCommonToggles();

        for (int i = 0; i < RegionTypes_ER.Count; i++)
        {
            var type = RegionTypes_ER.ElementAt(i).Key;
            var locKey = RegionTypes_ER.ElementAt(i).Value;

            DisplayIndividualToggle(type, LOC.Get(locKey), i);
        }
    }

    private void HandleGranularRegionToggles_AC6()
    {
        DisplayCommonToggles();

        for (int i = 0; i < RegionTypes_AC6.Count; i++)
        {
            var type = RegionTypes_AC6.ElementAt(i).Key;
            var locKey = RegionTypes_AC6.ElementAt(i).Value;

            DisplayIndividualToggle(type, LOC.Get(locKey), i);
        }
    }

    private void HandleGranularRegionToggles_NR()
    {
        DisplayCommonToggles();

        for (int i = 0; i < RegionTypes_NR.Count; i++)
        {
            var type = RegionTypes_NR.ElementAt(i).Key;
            var locKey = RegionTypes_NR.ElementAt(i).Value;

            DisplayIndividualToggle(type, LOC.Get(locKey), i);
        }
    }

    public void DisplayCommonToggles()
    {
        // Make All Regions Invisible
        if (ImGui.MenuItem($"{LOC.Get("MAP_Filters_Common_Toggle_Region_Visibility_OFF")}##toggleCommonOff"))
        {
            foreach (var entry in View.Project.Handler.MapData.PrimaryBank.Maps)
            {
                if (entry.Value.MapContainer != null)
                {
                    foreach (var child in entry.Value.MapContainer.RootObject.Children)
                    {
                        if (child.WrappedObject is MSB_AC6.Region or MSBE.Region or MSBS.Region or MSB3.Region or MSB2.Region)
                        {
                            child.EditorVisible = false;
                            SetupTruthList(false);
                        }
                    }
                }
            }

            View.DelayPicking();
        }
        GUI.Tooltip(LOC.Get("MAP_Filters_Common_Toggle_Region_Visibility_OFF_TT"));

        // Make All Regions Visible
        if (ImGui.MenuItem($"{LOC.Get("MAP_Filters_Common_Toggle_Region_Visibility_ON")}##toggleCommonOn"))
        {
            foreach (var entry in View.Project.Handler.MapData.PrimaryBank.Maps)
            {
                if (entry.Value.MapContainer != null)
                {
                    foreach (var child in entry.Value.MapContainer.RootObject.Children)
                    {
                        if (child.WrappedObject is MSB_AC6.Region or MSBE.Region or MSBS.Region or MSB3.Region or MSB2.Region or MSB_NR.Region)
                        {
                            child.EditorVisible = true;
                            SetupTruthList(true);
                        }
                    }
                }
            }

            View.DelayPicking();
        }
        GUI.Tooltip(LOC.Get("MAP_Filters_Common_Toggle_Region_Visibility_ON_TT"));

        ImGui.Separator();
    }

    public void DisplayIndividualToggle(Type regionType, string name, int truthIndex)
    {
        var show = false;

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
            // Toggle X
            if (ImGui.MenuItem(LOC.Get("MAP_Filters_Toggle_Action", name)))
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
            GUI.Tooltip(LOC.Get("MAP_Filters_Specific_Toggle_TT", name));
            GUI.ShowActiveStatus(RegionVisibilityTruth[truthIndex]);
        }
    }
}
