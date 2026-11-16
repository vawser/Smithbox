#nullable enable
using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.ProjectileEditor;

/// <summary>
/// The simulation and its clock, in one tool: what is simulated, then the time (play and pause, stop, framing the
/// camera on it, looping and speed, with how many effects are playing), then the setup around the shooter, what the
/// simulation couldn't follow, and the bullets it fired (<see cref="ProjectileSimulationSettings"/>). The time comes
/// near the top, so it can be scrubbed while the setup is changed.
/// </summary>
public class ProjectilePlaybackTool
{
    private readonly ProjectileEditorView View;
    private readonly ProjectileSimulationSettings Simulation;

    public ProjectilePlaybackTool(ProjectileEditorView view)
    {
        View = view;
        Simulation = new ProjectileSimulationSettings(view);
    }

    public void OnToolWindow()
    {
        if (!ToolSections.Header($"{LOC.Get("PROJECTILE_Tool_Playback")}##projectilePlaybackToolHeader"))
            return;

        ImGui.BeginChild("ProjectilePlaybackToolSection", ImGuiChildFlags.Borders);

        Display();

        ImGui.EndChild();
    }

    private void Display()
    {
        var simulator = View.Simulator;
        var projectType = View.Project.Descriptor.ProjectType;

        // Bullet: what is simulated, or why nothing is (no row selected, or a layout that can't be read)
        Simulation.DisplayTarget();

        if (simulator.Row == null)
            return;

        // The time and effects only once there is something to play; the setup can still be changed without it
        if (simulator.Result is { } result)
            DisplayClock(simulator, result.Duration);

        // Setup, Warnings, Projectiles
        Simulation.DisplaySetup(projectType);
        Simulation.DisplayWarnings();
        Simulation.DisplayProjectiles(projectType);
    }

    private void DisplayClock(ProjectileSimulator simulator, float duration)
    {

        // Time
        GUI.SimpleHeader(LOC.Get("PROJECTILE_Playback_Time_Header"), LOC.Get("PROJECTILE_Playback_Time_Header_TT"));

        ImGui.SetNextItemWidth(-1);

        if (ImGui.SliderFloat("##projectileTime", ref simulator.Time, 0, duration, $"{simulator.Time:F2} / {duration:F2} s"))
            simulator.Playing = false;

        if (ImGui.Button($"{(simulator.Playing ? LOC.Get("PROJECTILE_Pause") : LOC.Get("PROJECTILE_Play"))}##projectilePlay"))
            simulator.TogglePlay();

        ImGui.SameLine();

        if (ImGui.Button($"{LOC.Get("PROJECTILE_Stop")}##projectileStop"))
            simulator.Stop();

        ImGui.SameLine();

        if (ImGui.Button($"{LOC.Get("PROJECTILE_Frame")}##projectileFrame"))
            View.ViewportWindow.FrameSimulation();
        GUI.Tooltip(LOC.Get("PROJECTILE_Frame_TT"));

        // Options
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("PROJECTILE_Playback_Options_Header"), "");

        ImGui.Checkbox($"{LOC.Get("PROJECTILE_Loop")}##projectileLoop", ref CFG.Current.ProjectileEditor_Loop);

        ImGui.SetNextItemWidth(-1);
        ImGui.SliderFloat("##projectileSpeed", ref CFG.Current.ProjectileEditor_Speed, 0.05f, 2f, $"{LOC.Get("PROJECTILE_Speed")}: %.2f");

        // Effects
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("PROJECTILE_Playback_Effects_Header"), LOC.Get("PROJECTILE_Playback_Effects_Header_TT"));

        DisplayEffectStatus();

        GUI.Spacer();
    }

    /// <summary>How many effects the bullets have and are playing, or why there are none</summary>
    private void DisplayEffectStatus()
    {
        var effects = View.Effects;

        if (!ProjectileEffects.IsShowing)
        {
            ImGui.TextDisabled(LOC.Get("PROJECTILE_Effects_Off"));
            return;
        }

        if (!EditorConditions.SupportsFxEditor(View.Project.Descriptor.ProjectType))
        {
            ImGui.TextDisabled(LOC.Get("PROJECTILE_Effects_Unsupported"));
            return;
        }

        if (!effects.IsAvailable)
        {
            ImGui.TextDisabled(LOC.Get("PROJECTILE_Effects_Need_Fx_Editor"));
            return;
        }

        if (effects.Bank is { IsIndexed: false } && effects.EffectCount > 0)
        {
            ImGui.TextDisabled(LOC.Get("PROJECTILE_Effects_Indexing"));
            return;
        }

        var status = LOC.Get("PROJECTILE_Effects_Status", effects.EffectCount, effects.RunningCount, effects.ParticleCount);

        if (effects.MissingCount > 0)
            status += $"  {LOC.Get("PROJECTILE_Effects_Missing", effects.MissingCount)}";

        ImGui.TextDisabled(status);
    }
}
