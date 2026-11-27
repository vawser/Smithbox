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

        ImGui.Checkbox($"{LOC.Get("TAE_SimulationTool_Speed_Events")}##timeActSimSpeedEvents", ref CFG.Current.TimeActEditor.TimeActEditor_Sim_Speed_Events);
        GUI.Tooltip(LOC.Get("TAE_SimulationTool_Speed_Events_TT"));

        if (ImGui.Checkbox($"{LOC.Get("TAE_SimulationTool_State_Info")}##timeActSimStateInfo", ref CFG.Current.TimeActEditor.TimeActEditor_Sim_State_Info))
        {
            simulation.OnStateInfoToggled();
        }
        GUI.Tooltip(LOC.Get("TAE_SimulationTool_State_Info_TT"));

        ImGui.Checkbox($"{LOC.Get("TAE_SimulationTool_Model_Masks")}##timeActSimModelMasks", ref CFG.Current.TimeActEditor.TimeActEditor_Sim_Model_Masks);
        GUI.Tooltip(LOC.Get("TAE_SimulationTool_Model_Masks_TT"));

        ImGui.Checkbox($"{LOC.Get("TAE_SimulationTool_Weapons")}##timeActSimWeapons", ref CFG.Current.TimeActEditor.TimeActEditor_Sim_Weapons);
        GUI.Tooltip(LOC.Get("TAE_SimulationTool_Weapons_TT"));

        ImGui.Checkbox($"{LOC.Get("TAE_SimulationTool_Turning")}##timeActSimTurning", ref CFG.Current.TimeActEditor.TimeActEditor_Sim_Turning);
        GUI.Tooltip(LOC.Get("TAE_SimulationTool_Turning_TT"));

        if (ImGui.Checkbox($"{LOC.Get("TAE_SimulationTool_Rumble_Cam")}##timeActSimRumbleCam", ref CFG.Current.TimeActEditor.TimeActEditor_Sim_Rumble_Cam)
            && !CFG.Current.TimeActEditor.TimeActEditor_Sim_Rumble_Cam)
        {
            View.Playback.Rumble.StopAll();
        }
        GUI.Tooltip(LOC.Get("TAE_SimulationTool_Rumble_Cam_TT"));

        ImGui.Checkbox($"{LOC.Get("TAE_SimulationTool_Opacity")}##timeActSimOpacity", ref CFG.Current.TimeActEditor.TimeActEditor_Sim_Opacity);
        GUI.Tooltip(LOC.Get("TAE_SimulationTool_Opacity_TT"));

        ImGui.Checkbox($"{LOC.Get("TAE_SimulationTool_Magic")}##timeActSimMagic", ref CFG.Current.TimeActEditor.TimeActEditor_Sim_Magic);
        GUI.Tooltip(LOC.Get("TAE_SimulationTool_Magic_TT"));

        if (simulation.Opacity < 1)
        {
            GUI.WrappedText(LOC.Get("TAE_SimulationTool_Opacity_Status", (simulation.Opacity * 100).ToString("0")));
        }

        ImGui.Checkbox($"{LOC.Get("TAE_SimulationTool_Additive_Anims")}##timeActSimAdditive", ref CFG.Current.TimeActEditor.TimeActEditor_Sim_Additive_Anims);
        GUI.Tooltip(LOC.Get("TAE_SimulationTool_Additive_Anims_TT"));

        if (CFG.Current.TimeActEditor.TimeActEditor_Sim_Additive_Anims)
        {
            ImGui.Indent();
            ImGui.Checkbox($"{LOC.Get("TAE_SimulationTool_Assume_SpEffects")}##timeActSimAssumeSpEffects", ref CFG.Current.TimeActEditor.TimeActEditor_Assume_SpEffects);
            GUI.Tooltip(LOC.Get("TAE_SimulationTool_Assume_SpEffects_TT"));
            ImGui.Unindent();
        }

        ImGui.Checkbox($"{LOC.Get("TAE_SimulationTool_Blending")}##timeActSimBlending", ref CFG.Current.TimeActEditor.TimeActEditor_Sim_Blending);
        GUI.Tooltip(LOC.Get("TAE_SimulationTool_Blending_TT"));

        if (CFG.Current.TimeActEditor.TimeActEditor_Sim_Blending)
        {
            ImGui.Indent();
            ImGui.Checkbox($"{LOC.Get("TAE_SimulationTool_Blend_On_Change")}##timeActSimBlendOnChange", ref CFG.Current.TimeActEditor.TimeActEditor_Blend_On_Animation_Change);
            GUI.Tooltip(LOC.Get("TAE_SimulationTool_Blend_On_Change_TT"));
            ImGui.Unindent();
        }

        if (View.Playback.BlendWeight > 0)
        {
            GUI.WrappedText(LOC.Get("TAE_SimulationTool_Blend_Status", (View.Playback.BlendWeight * 100).ToString("0")));
        }

        if (simulation.AdditiveLayers.Count > 0)
        {
            GUI.WrappedText(LOC.Get("TAE_SimulationTool_Additive_Status",
                string.Join(", ", simulation.AdditiveLayers.Select(l => $"{l.AnimationId} ({l.Weight:0.00})"))));
        }

        DisplayBehaviorLayers();

        // Turning
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("TAE_SimulationTool_Turn_Header"), LOC.Get("TAE_SimulationTool_Turn_Header_TT"));

        ImGui.SetNextItemWidth(-1);
        ImGui.SliderFloat("##timeActSimTurnTarget", ref CFG.Current.TimeActEditor.TimeActEditor_Turn_Target_Angle, -180f, 180f,
            $"{LOC.Get("TAE_SimulationTool_Turn_Target")}: %.0f");
        GUI.Tooltip(LOC.Get("TAE_SimulationTool_Turn_Target_TT"));

        if (CFG.Current.TimeActEditor.TimeActEditor_Sim_Turning && View.Selection.SelectedAnimation != null)
        {
            GUI.WrappedText(simulation.HasTurnEvents
                ? LOC.Get("TAE_SimulationTool_Turn_Status", simulation.Turn.ToString("0"))
                : LOC.Get("TAE_SimulationTool_Turn_None"));
        }

        DisplayFloorMaterial();

        // Weapons
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("TAE_SimulationTool_Weapon_Style_Header"), LOC.Get("TAE_SimulationTool_Weapon_Style_Header_TT"));

        DisplayWeaponStyle(simulation);

        DisplayMagic();

        // Root Motion
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("TAE_SimulationTool_Root_Motion_Header"), LOC.Get("TAE_SimulationTool_Root_Motion_Header_TT"));

        DisplayScale("TAE_SimulationTool_Root_Motion_Ground", "timeActSimRootGround", ref CFG.Current.TimeActEditor.TimeActEditor_Root_Motion_Scale_Ground);
        DisplayScale("TAE_SimulationTool_Root_Motion_Up", "timeActSimRootUp", ref CFG.Current.TimeActEditor.TimeActEditor_Root_Motion_Scale_Up);
        DisplayScale("TAE_SimulationTool_Root_Motion_Turn", "timeActSimRootTurn", ref CFG.Current.TimeActEditor.TimeActEditor_Root_Motion_Scale_Turn);

        if (ImGui.Button($"{LOC.Get("TAE_SimulationTool_Root_Motion_Reset")}##timeActSimRootReset"))
        {
            CFG.Current.TimeActEditor.TimeActEditor_Root_Motion_Scale_Ground = 1f;
            CFG.Current.TimeActEditor.TimeActEditor_Root_Motion_Scale_Up = 1f;
            CFG.Current.TimeActEditor.TimeActEditor_Root_Motion_Scale_Turn = 1f;
        }
        GUI.Tooltip(LOC.Get("TAE_SimulationTool_Root_Motion_Reset_TT"));

        if (!CFG.Current.TimeActEditor.TimeActEditor_Apply_Root_Motion)
        {
            GUI.WrappedText(LOC.Get("TAE_SimulationTool_Root_Motion_Off"));
        }

        // States
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("TAE_SimulationTool_States_Header"), LOC.Get("TAE_SimulationTool_States_Header_TT"));

        DisplayStates(simulation);
    }

    // The value typed into each variable's field, kept until the field is left
    private readonly System.Collections.Generic.Dictionary<string, float> _typedVariables = new();

    /// <summary>
    /// The layers the character's behavior graph plays with the animation: where the clip sits in the graph, the clips
    /// laid with it, and the variables that set their weights and selections, which can be changed for the session.
    /// </summary>
    private void DisplayBehaviorLayers()
    {
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("TAE_SimulationTool_Behavior_Header"), LOC.Get("TAE_SimulationTool_Behavior_Header_TT"));

        ImGui.Checkbox($"{LOC.Get("TAE_SimulationTool_Behavior_Layers")}##timeActSimBehaviorLayers", ref CFG.Current.TimeActEditor.TimeActEditor_Sim_Behavior_Layers);
        GUI.Tooltip(LOC.Get("TAE_SimulationTool_Behavior_Layers_TT"));

        if (!CFG.Current.TimeActEditor.TimeActEditor_Sim_Behavior_Layers || View.Selection.SelectedAnimation == null)
            return;

        var behavior = View.Playback.BehaviorLayers;

        if (behavior.IsLoading)
        {
            GUI.WrappedText(LOC.Get("TAE_SimulationTool_Behavior_Loading"));
            return;
        }

        if (behavior.Plan is not { } plan)
        {
            GUI.WrappedText(LOC.Get("TAE_SimulationTool_Behavior_None"));
            return;
        }

        GUI.WrappedText(LOC.Get("TAE_SimulationTool_Behavior_Path", behavior.SourceFile ?? "", plan.Path));

        var package = View.Playback.Package;
        var shown = 0;

        foreach (var layer in plan.Layers)
        {
            var notes = "";
            if (layer.Underlay)
                notes += $" {LOC.Get("TAE_SimulationTool_Behavior_Under")}";
            if (layer.BoneWeights != null)
                notes += $" {LOC.Get("TAE_SimulationTool_Behavior_Masked")}";
            if (!behavior.HasClip(package, layer.AnimationName))
                notes += $" {LOC.Get("TAE_SimulationTool_Behavior_No_Clip")}";

            GUI.WrappedText($"{layer.AnimationName} ({layer.Weight * 100:0}%){notes}: {layer.Label}");
            shown++;
        }

        if (shown == 0)
        {
            GUI.WrappedText(LOC.Get("TAE_SimulationTool_Behavior_No_Layers"));
        }

        if (plan.Variables.Count == 0)
            return;

        if (!ImGui.TreeNode($"{LOC.Get("TAE_SimulationTool_Behavior_Variables")}##timeActSimBehaviorVariables"))
            return;

        GUI.Tooltip(LOC.Get("TAE_SimulationTool_Behavior_Variables_TT"));

        foreach (var (name, value) in plan.Variables)
        {
            var typed = _typedVariables.TryGetValue(name, out var t) ? t : value;

            ImGui.SetNextItemWidth(120);
            if (ImGui.InputFloat($"{name}##timeActSimBehaviorVar{name}", ref typed, 0, 0, "%.3g"))
            {
                _typedVariables[name] = typed;
            }

            if (GUI.InputCommitted())
            {
                behavior.SetOverride(name, typed);
                _typedVariables.Remove(name);
            }
            else if (!ImGui.IsItemActive())
            {
                _typedVariables.Remove(name);
            }
        }

        if (behavior.Overrides.Count > 0 &&
            ImGui.Button($"{LOC.Get("TAE_SimulationTool_Behavior_Reset")}##timeActSimBehaviorReset"))
        {
            behavior.ClearOverrides();
        }

        ImGui.TreePop();
    }

    /// <summary>
    /// The floor the character stands on, for the floor-determined effects. Only shown where the game has floor
    /// materials (HitMtrlParam).
    /// </summary>
    private void DisplayFloorMaterial()
    {
        var materials = TimeActFloorEffects.GetFloorMaterials(Project);

        if (materials.Count == 0)
            return;

        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("TAE_SimulationTool_Floor_Header"), LOC.Get("TAE_SimulationTool_Floor_Header_TT"));

        string Label((int Id, string Name) material) =>
            string.IsNullOrEmpty(material.Name) ? $"{material.Id}" : $"{material.Id}: {material.Name}";

        var current = materials.FindIndex(m => m.Id == CFG.Current.TimeActEditor.TimeActEditor_Floor_Material);
        var preview = current >= 0 ? Label(materials[current]) : $"{CFG.Current.TimeActEditor.TimeActEditor_Floor_Material}";

        ImGui.SetNextItemWidth(-1);
        if (ImGui.BeginCombo("##timeActSimFloorMaterial", preview))
        {
            foreach (var material in materials)
            {
                if (ImGui.Selectable($"{Label(material)}##timeActSimFloor{material.Id}", material.Id == CFG.Current.TimeActEditor.TimeActEditor_Floor_Material))
                {
                    CFG.Current.TimeActEditor.TimeActEditor_Floor_Material = material.Id;

                    // The effects are worked out once for the animation
                    View.Playback.Effects.Invalidate();
                }
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("TAE_SimulationTool_Floor_TT"));
    }

    // The rows shown in the magic list at most, as a game can have thousands of them
    private const int MaxMagicRows = 500;

    private string _magicFilter = "";

    /// <summary>
    /// The magic the cast events (CastHighlightedMagic) cast, from the game's spell param. Only shown where the game
    /// has one.
    /// </summary>
    private void DisplayMagic()
    {
        if (ProjectileEditor.ProjectileParams.GetSpellParamName(Project) is not { } paramName ||
            ProjectileEditor.ProjectileParams.GetParam(Project, paramName) is not { } param)
        {
            return;
        }

        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("TAE_SimulationTool_Magic_Header"), LOC.Get("TAE_SimulationTool_Magic_Header_TT"));

        static string Label(Andre.Formats.Param.Row row) =>
            string.IsNullOrEmpty(row.Name) ? $"{row.ID}" : $"{row.ID}: {row.Name}";

        var currentId = CFG.Current.TimeActEditor.TimeActEditor_Cast_Magic_ID;
        var current = View.Playback.MagicDisplay.GetMagic();
        var preview = current != null ? Label(current)
            : currentId >= 0 ? $"{currentId}"
            : LOC.Get("TAE_SimulationTool_Magic_None");

        ImGui.SetNextItemWidth(-1);
        if (ImGui.BeginCombo("##timeActSimMagic", preview, ImGuiComboFlags.HeightLarge))
        {
            if (ImGui.IsWindowAppearing())
            {
                ImGui.SetKeyboardFocusHere();
            }

            // Filters as it is typed, so there is nothing to commit
            ImGui.SetNextItemWidth(-1);
            ImGui.InputTextWithHint("##timeActSimMagicFilter", LOC.Get("TAE_SimulationTool_Magic_Filter"), ref _magicFilter, 128);

            if (ImGui.Selectable($"{LOC.Get("TAE_SimulationTool_Magic_None")}##timeActSimMagicNone", currentId < 0))
            {
                SetMagic(-1);
            }

            var shown = 0;

            foreach (var row in param.Rows)
            {
                var label = Label(row);

                if (_magicFilter.Length > 0 && !label.Contains(_magicFilter, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (shown++ >= MaxMagicRows)
                {
                    GUI.WrappedText(LOC.Get("TAE_SimulationTool_Magic_More"));
                    break;
                }

                if (ImGui.Selectable($"{label}##timeActSimMagic{row.ID}", row.ID == currentId))
                {
                    SetMagic(row.ID);
                }
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("TAE_SimulationTool_Magic_Row_TT", paramName));

        if (ImGui.Checkbox($"{LOC.Get("TAE_SimulationTool_Magic_Charged")}##timeActSimMagicCharged", ref CFG.Current.TimeActEditor.TimeActEditor_Cast_Magic_Charged))
        {
            View.Playback.EventDisplay.Invalidate();
        }
        GUI.Tooltip(LOC.Get("TAE_SimulationTool_Magic_Charged_TT"));

        GUI.WrappedText(TimeActEventResolver.GetMagicStatus(Project, currentId, CFG.Current.TimeActEditor.TimeActEditor_Cast_Magic_Charged));

        if (View.Playback.MagicDisplay.LatestResult is { } result)
        {
            GUI.WrappedText(LOC.Get("TAE_SimulationTool_Magic_Flight", result.Tracks.Count, result.Duration.ToString("0.00")));

            var magicDisplay = View.Playback.MagicDisplay;

            if (magicDisplay.EffectCount > 0)
            {
                GUI.WrappedText(LOC.Get("TAE_SimulationTool_Magic_Effects", magicDisplay.RunningEffectCount,
                    magicDisplay.EffectCount, magicDisplay.MissingEffectCount));
            }

            if (result.Warnings != ProjectileEditor.SimulationWarning.None)
            {
                GUI.WrappedText(LOC.Get("TAE_SimulationTool_Magic_Warnings", result.Warnings));
            }
        }
    }

    private void SetMagic(int id)
    {
        CFG.Current.TimeActEditor.TimeActEditor_Cast_Magic_ID = id;

        // The cast events say what they cast
        View.Playback.EventDisplay.Invalidate();
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
        var current = Array.FindIndex(WeaponStyles, s => s.Style == CFG.Current.TimeActEditor.TimeActEditor_Weapon_Style);
        var names = WeaponStyles.Select(s => LOC.Get(s.Key)).ToArray();

        if (current < 0)
            current = 0;

        ImGui.SetNextItemWidth(-1);
        if (ImGui.Combo("##timeActSimWeaponStyle", ref current, names, names.Length))
        {
            CFG.Current.TimeActEditor.TimeActEditor_Weapon_Style = WeaponStyles[current].Style;
        }
        GUI.Tooltip(LOC.Get("TAE_SimulationTool_Weapon_Style_TT"));

        // An event may have changed it since the start
        if (simulation.WeaponStyle != CFG.Current.TimeActEditor.TimeActEditor_Weapon_Style)
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
        if (!CFG.Current.TimeActEditor.TimeActEditor_Sim_State_Info)
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
