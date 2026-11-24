using Hexa.NET.ImGui;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.TimeActEditor;

/// <summary>
/// The editor's display and playback toggles, formerly in the Options menu: how the animation plays, the overlays in
/// the viewport, what the event graph shows, and the vanilla markers in the lists. Each is a plain CFG setting read
/// every frame, so a change takes effect at once.
/// </summary>
public class TimeActFiltersTool
{
    private readonly TimeActEditorView View;
    private readonly ProjectEntry Project;

    public TimeActFiltersTool(TimeActEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void OnToolWindow()
    {
        if (!ToolSections.Header($"{LOC.Get("TAE_FiltersTool_Header")}##timeActFiltersToolHeader"))
            return;

        ImGui.BeginChild("TimeActFiltersToolSection", ImGuiChildFlags.Borders);

        Display();

        ImGui.EndChild();
    }

    private void Display()
    {
        GUI.WrappedText(LOC.Get("TAE_FiltersTool_Hint"));

        // Playback
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("TAE_FiltersTool_Playback_Header"), LOC.Get("TAE_FiltersTool_Playback_Header_TT"));

        ImGui.Checkbox($"{LOC.Get("TAE_Editor_Options_Auto_Play")}##timeActFilterAutoPlay", ref CFG.Current.TimeActEditor.TimeActEditor_Auto_Play_Animation);
        GUI.Tooltip(LOC.Get("TAE_Editor_Options_Auto_Play_TT"));

        ImGui.Checkbox($"{LOC.Get("TAE_Editor_Options_Apply_Root_Motion")}##timeActFilterApplyRootMotion", ref CFG.Current.TimeActEditor.TimeActEditor_Apply_Root_Motion);
        GUI.Tooltip(LOC.Get("TAE_Editor_Options_Apply_Root_Motion_TT"));

        ImGui.Checkbox($"{LOC.Get("TAE_Editor_Options_Play_Event_Sounds")}##timeActFilterPlayEventSounds", ref CFG.Current.TimeActEditor.TimeActEditor_Play_Event_Sounds);
        GUI.Tooltip(LOC.Get("TAE_Editor_Options_Play_Event_Sounds_TT"));

        // Viewport
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("TAE_FiltersTool_Viewport_Header"), LOC.Get("TAE_FiltersTool_Viewport_Header_TT"));

        ImGui.Checkbox($"{LOC.Get("TAE_Editor_Options_Event_Markers")}##timeActFilterEventMarkers", ref CFG.Current.TimeActEditor.TimeActEditor_Display_Event_Markers);
        GUI.Tooltip(LOC.Get("TAE_Editor_Options_Event_Markers_TT"));

        ImGui.Checkbox($"{LOC.Get("TAE_Editor_Options_Attack_Capsules")}##timeActFilterAttackCapsules", ref CFG.Current.TimeActEditor.TimeActEditor_Display_Attack_Capsules);
        GUI.Tooltip(LOC.Get("TAE_Editor_Options_Attack_Capsules_TT"));

        ImGui.Checkbox($"{LOC.Get("TAE_Editor_Options_Root_Motion_Path")}##timeActFilterRootMotionPath", ref CFG.Current.TimeActEditor.TimeActEditor_Display_Root_Motion);
        GUI.Tooltip(LOC.Get("TAE_Editor_Options_Root_Motion_Path_TT"));

        if (ImGui.Checkbox($"{LOC.Get("TAE_Editor_Options_Display_Effects")}##timeActFilterDisplayEffects", ref CFG.Current.TimeActEditor.TimeActEditor_Display_Effects)
            && !CFG.Current.TimeActEditor.TimeActEditor_Display_Effects)
        {
            View.Playback.Effects.Clear();
        }
        GUI.Tooltip(LOC.Get("TAE_Editor_Options_Display_Effects_TT"));
        DisplayEffectStatus();

        // Event Graph
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("TAE_FiltersTool_Graph_Header"), LOC.Get("TAE_FiltersTool_Graph_Header_TT"));

        ImGui.Checkbox($"{LOC.Get("TAE_Editor_Options_Outside_Playback")}##timeActFilterOutsidePlayback", ref CFG.Current.TimeActEditor.TimeActEditor_Graph_Display_Outside_Playback);
        GUI.Tooltip(LOC.Get("TAE_Editor_Options_Outside_Playback_TT"));

        ImGui.Checkbox($"{LOC.Get("TAE_Editor_Options_Event_Names")}##timeActFilterEventNames", ref CFG.Current.TimeActEditor.TimeActEditor_Graph_Display_Event_Names);
        GUI.Tooltip(LOC.Get("TAE_Editor_Options_Event_Names_TT"));

        // Lists
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("TAE_FiltersTool_Lists_Header"), LOC.Get("TAE_FiltersTool_Lists_Header_TT"));

        ImGui.Checkbox($"{LOC.Get("TAE_Editor_Options_Vanilla_Markers")}##timeActFilterVanillaMarkers", ref CFG.Current.TimeActEditor.TimeActEditor_Display_Vanilla_Markers);
        GUI.Tooltip(LOC.Get("TAE_Editor_Options_Vanilla_Markers_TT"));
    }

    /// <summary>Why effects may not show, or what the animation's effect events are doing</summary>
    private void DisplayEffectStatus()
    {
        if (!CFG.Current.TimeActEditor.TimeActEditor_Display_Effects)
            return;

        var effects = View.Playback.Effects;

        if (!effects.IsAvailable)
        {
            GUI.WrappedText(LOC.Get("TAE_FiltersTool_Effects_No_Fx"));
            return;
        }

        if (View.Selection.SelectedAnimation == null)
            return;

        if (effects.Bank is { IsIndexing: true } bank)
            GUI.WrappedText(LOC.Get("TAE_FiltersTool_Effects_Indexing", bank.IndexedBinders, bank.TotalBinders));
        else if (effects.EventCount == 0)
            GUI.WrappedText(LOC.Get("TAE_FiltersTool_Effects_None"));
        else if (effects.IsLoading)
            GUI.WrappedText(LOC.Get("FX_Display_Loading_Effects", effects.LoadedCount, effects.LoadingCount));
        else
            GUI.WrappedText(LOC.Get("TAE_FiltersTool_Effects_Status", effects.EventCount, effects.RunningCount,
                effects.ParticleCount, effects.MissingCount));
    }
}
