using Hexa.NET.ImGui;
using System;
using System.Linq;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.TimeActEditor;

/// <summary>
/// What the viewport simulates from the events: a switch for each simulation, how much of the root motion is
/// applied, and the StateInfos that events can be gated on. Each is a plain CFG setting or simulation state read
/// every frame, so a change takes effect at once.
/// </summary>
public class TimeActSimulationTool
{
    private const float MaxRootMotionScale = 3f;

    private readonly TimeActEditorView View;
    private readonly ProjectEntry Project;

    public TimeActSimulationTool(TimeActEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void OnToolWindow()
    {
        if (!ToolSections.Header($"{LOC.Get("TAE_SimulationTool_Header")}##timeActSimulationToolHeader"))
            return;

        ImGui.BeginChild("TimeActSimulationToolSection", ImGuiChildFlags.Borders);

        Display();

        ImGui.EndChild();
    }

    private void Display()
    {
        var simulation = View.Playback.Simulation;

        GUI.WrappedText(LOC.Get("TAE_SimulationTool_Hint"));

        // Simulations
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("TAE_SimulationTool_Simulations_Header"), LOC.Get("TAE_SimulationTool_Simulations_Header_TT"));

        ImGui.Checkbox($"{LOC.Get("TAE_SimulationTool_Speed_Events")}##timeActSimSpeedEvents", ref CFG.Current.TimeActEditor_Sim_Speed_Events);
        GUI.Tooltip(LOC.Get("TAE_SimulationTool_Speed_Events_TT"));

        if (ImGui.Checkbox($"{LOC.Get("TAE_SimulationTool_State_Info")}##timeActSimStateInfo", ref CFG.Current.TimeActEditor_Sim_State_Info))
        {
            simulation.OnStateInfoToggled();
        }
        GUI.Tooltip(LOC.Get("TAE_SimulationTool_State_Info_TT"));

        ImGui.Checkbox($"{LOC.Get("TAE_SimulationTool_Model_Masks")}##timeActSimModelMasks", ref CFG.Current.TimeActEditor_Sim_Model_Masks);
        GUI.Tooltip(LOC.Get("TAE_SimulationTool_Model_Masks_TT"));

        ImGui.Checkbox($"{LOC.Get("TAE_SimulationTool_Weapons")}##timeActSimWeapons", ref CFG.Current.TimeActEditor_Sim_Weapons);
        GUI.Tooltip(LOC.Get("TAE_SimulationTool_Weapons_TT"));

        ImGui.Checkbox($"{LOC.Get("TAE_SimulationTool_Turning")}##timeActSimTurning", ref CFG.Current.TimeActEditor_Sim_Turning);
        GUI.Tooltip(LOC.Get("TAE_SimulationTool_Turning_TT"));

        if (ImGui.Checkbox($"{LOC.Get("TAE_SimulationTool_Rumble_Cam")}##timeActSimRumbleCam", ref CFG.Current.TimeActEditor_Sim_Rumble_Cam)
            && !CFG.Current.TimeActEditor_Sim_Rumble_Cam)
        {
            View.Playback.Rumble.StopAll();
        }
        GUI.Tooltip(LOC.Get("TAE_SimulationTool_Rumble_Cam_TT"));

        // Turning
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("TAE_SimulationTool_Turn_Header"), LOC.Get("TAE_SimulationTool_Turn_Header_TT"));

        ImGui.SetNextItemWidth(-1);
        ImGui.SliderFloat("##timeActSimTurnTarget", ref CFG.Current.TimeActEditor_Turn_Target_Angle, -180f, 180f,
            $"{LOC.Get("TAE_SimulationTool_Turn_Target")}: %.0f");
        GUI.Tooltip(LOC.Get("TAE_SimulationTool_Turn_Target_TT"));

        if (CFG.Current.TimeActEditor_Sim_Turning && View.Selection.SelectedAnimation != null)
        {
            GUI.WrappedText(simulation.HasTurnEvents
                ? LOC.Get("TAE_SimulationTool_Turn_Status", simulation.Turn.ToString("0"))
                : LOC.Get("TAE_SimulationTool_Turn_None"));
        }

        // Weapons
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("TAE_SimulationTool_Weapon_Style_Header"), LOC.Get("TAE_SimulationTool_Weapon_Style_Header_TT"));

        DisplayWeaponStyle(simulation);

        // Root Motion
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("TAE_SimulationTool_Root_Motion_Header"), LOC.Get("TAE_SimulationTool_Root_Motion_Header_TT"));

        DisplayScale("TAE_SimulationTool_Root_Motion_Ground", "timeActSimRootGround", ref CFG.Current.TimeActEditor_Root_Motion_Scale_Ground);
        DisplayScale("TAE_SimulationTool_Root_Motion_Up", "timeActSimRootUp", ref CFG.Current.TimeActEditor_Root_Motion_Scale_Up);
        DisplayScale("TAE_SimulationTool_Root_Motion_Turn", "timeActSimRootTurn", ref CFG.Current.TimeActEditor_Root_Motion_Scale_Turn);

        if (ImGui.Button($"{LOC.Get("TAE_SimulationTool_Root_Motion_Reset")}##timeActSimRootReset"))
        {
            CFG.Current.TimeActEditor_Root_Motion_Scale_Ground = 1f;
            CFG.Current.TimeActEditor_Root_Motion_Scale_Up = 1f;
            CFG.Current.TimeActEditor_Root_Motion_Scale_Turn = 1f;
        }
        GUI.Tooltip(LOC.Get("TAE_SimulationTool_Root_Motion_Reset_TT"));

        if (!CFG.Current.TimeActEditor_Apply_Root_Motion)
        {
            GUI.WrappedText(LOC.Get("TAE_SimulationTool_Root_Motion_Off"));
        }

        // States
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("TAE_SimulationTool_States_Header"), LOC.Get("TAE_SimulationTool_States_Header_TT"));

        DisplayStates(simulation);
    }

    // The styles that can be picked to start from, by their WeaponStyle value
    private static readonly (int Style, string Key)[] WeaponStyles =
    {
        (1, "TAE_SimulationTool_Weapon_Style_One_Hand"),
        (3, "TAE_SimulationTool_Weapon_Style_Right_Both"),
        (2, "TAE_SimulationTool_Weapon_Style_Left_Both")
    };

    private static void DisplayWeaponStyle(TimeActSimulation simulation)
    {
        var current = Array.FindIndex(WeaponStyles, s => s.Style == CFG.Current.TimeActEditor_Weapon_Style);
        var names = WeaponStyles.Select(s => LOC.Get(s.Key)).ToArray();

        if (current < 0)
            current = 0;

        ImGui.SetNextItemWidth(-1);
        if (ImGui.Combo("##timeActSimWeaponStyle", ref current, names, names.Length))
        {
            CFG.Current.TimeActEditor_Weapon_Style = WeaponStyles[current].Style;
        }
        GUI.Tooltip(LOC.Get("TAE_SimulationTool_Weapon_Style_TT"));

        // An event may have changed it since the start
        if (simulation.WeaponStyle != CFG.Current.TimeActEditor_Weapon_Style)
        {
            GUI.WrappedText(LOC.Get("TAE_SimulationTool_Weapon_Style_Now", simulation.WeaponStyle));
        }
    }

    private static void DisplayScale(string key, string id, ref float value)
    {
        ImGui.SetNextItemWidth(-1);
        ImGui.SliderFloat($"##{id}", ref value, 0f, MaxRootMotionScale, $"{LOC.Get(key)}: %.2f");
        GUI.Tooltip(LOC.Get($"{key}_TT"));
    }

    private void DisplayStates(TimeActSimulation simulation)
    {
        if (!CFG.Current.TimeActEditor_Sim_State_Info)
        {
            GUI.WrappedText(LOC.Get("TAE_SimulationTool_States_Off"));
            return;
        }

        if (View.Selection.SelectedAnimation == null)
            return;

        var states = simulation.GetStates();

        if (states.Count == 0)
        {
            GUI.WrappedText(LOC.Get("TAE_SimulationTool_States_None"));
            return;
        }

        foreach (var state in states)
        {
            var enabled = simulation.IsStateEnabled(state);

            if (ImGui.Checkbox($"{simulation.GetStateName(state)}##timeActSimState{state}", ref enabled))
            {
                simulation.SetStateEnabled(state, enabled);
            }
        }

        if (ImGui.Button($"{LOC.Get("TAE_SimulationTool_States_Enable_All")}##timeActSimStatesAll"))
        {
            simulation.EnableAllStates();
        }
        GUI.Tooltip(LOC.Get("TAE_SimulationTool_States_Enable_All_TT"));
    }
}
