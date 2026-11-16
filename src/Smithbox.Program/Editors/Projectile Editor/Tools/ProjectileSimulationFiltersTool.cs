#nullable enable
using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.ProjectileEditor;

/// <summary>
/// What the viewport and the Properties window show of the simulation: the bullets' effects, the lines drawn over
/// it, how the camera follows a new row, and how fields are named. None of it changes the simulation itself.
/// </summary>
public class ProjectileSimulationFiltersTool
{
    private readonly ProjectileEditorView View;

    public ProjectileSimulationFiltersTool(ProjectileEditorView view)
    {
        View = view;
    }

    public void OnToolWindow()
    {
        if (!ToolSections.Header($"{LOC.Get("PROJECTILE_Tool_Simulation_Filters")}##projectileSimulationFiltersToolHeader"))
            return;

        ImGui.BeginChild("ProjectileSimulationFiltersToolSection", ImGuiChildFlags.Borders);

        Display();

        ImGui.EndChild();
    }

    private static void Display()
    {
        var cfg = CFG.Current;

        GUI.WrappedText(LOC.Get("PROJECTILE_SimulationFilters_Hint"));

        // Effects
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("PROJECTILE_SimulationFilters_Effects_Header"), LOC.Get("PROJECTILE_SimulationFilters_Effects_Header_TT"));

        // FX turns every effect on or off; the two below pick which kinds play while it is on
        Toggle("PROJECTILE_Display_Fx", ref cfg.ProjectileEditor_Display_Fx);

        ImGui.BeginDisabled(!cfg.ProjectileEditor_Display_Fx);
        Toggle("PROJECTILE_Display_Effects", ref cfg.ProjectileEditor_Display_Effects);
        Toggle("PROJECTILE_Display_Hit_Effects", ref cfg.ProjectileEditor_Display_Hit_Effects);
        ImGui.EndDisabled();

        // Overlays
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("PROJECTILE_SimulationFilters_Overlays_Header"), LOC.Get("PROJECTILE_SimulationFilters_Overlays_Header_TT"));

        Toggle("PROJECTILE_Display_Bullets", ref cfg.ProjectileEditor_Display_Bullets);
        Toggle("PROJECTILE_Display_Impacts", ref cfg.ProjectileEditor_Display_Impacts);
        Toggle("PROJECTILE_Display_Trails", ref cfg.ProjectileEditor_Display_Trails);
        Toggle("PROJECTILE_Display_Reach", ref cfg.ProjectileEditor_Display_Reach);
        Toggle("PROJECTILE_Display_Damage", ref cfg.ProjectileEditor_Display_Damage);
        Toggle("PROJECTILE_Display_Target", ref cfg.ProjectileEditor_Display_Target);
        Toggle("PROJECTILE_Display_Target_Path", ref cfg.ProjectileEditor_Display_Target_Path);
        Toggle("PROJECTILE_Display_Wall", ref cfg.ProjectileEditor_Display_Wall);
        Toggle("PROJECTILE_Display_Ground_Grid", ref cfg.ProjectileEditor_Display_Ground_Grid);

        // Camera
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("PROJECTILE_SimulationFilters_Camera_Header"), "");

        Toggle("PROJECTILE_Frame_On_Select", ref cfg.ProjectileEditor_Frame_On_Select);

        // Properties
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("PROJECTILE_SimulationFilters_Properties_Header"), "");

        Toggle("PROJECTILE_Show_Community_Names", ref cfg.ProjectileEditor_Show_Community_Names);
    }

    private static void Toggle(string locKey, ref bool value)
    {
        ImGui.Checkbox($"{LOC.Get(locKey)}##{locKey}", ref value);
        GUI.Tooltip(LOC.Get($"{locKey}_TT"));
    }
}
