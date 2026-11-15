using Hexa.NET.ImGui;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.CutsceneEditor;

/// <summary>
/// What the cut shows in the viewport, and what it plays while it plays: the map and its assets, the actors' models,
/// the camera's path and frustum, markers and subtitles; subtitle voices and sound events. The toggles that load or
/// play something take effect at once (through the screen's Set* methods).
/// </summary>
public class CutsceneFiltersTool
{
    private readonly CutsceneEditorScreen Editor;

    public CutsceneFiltersTool(CutsceneEditorScreen editor)
    {
        Editor = editor;
    }

    public void OnToolWindow()
    {
        if (!ToolSections.Header($"{LOC.Get("CUTSCENE_FiltersTool_Header")}##cutsceneFiltersToolHeader"))
            return;

        ImGui.BeginChild("CutsceneFiltersToolSection", ImGuiChildFlags.Borders);

        Display();

        ImGui.EndChild();
    }

    private void Display()
    {
        GUI.WrappedText(LOC.Get("CUTSCENE_FiltersTool_Hint"));

        // Scene
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("CUTSCENE_FiltersTool_Scene_Header"), LOC.Get("CUTSCENE_FiltersTool_Scene_Header_TT"));

        var loadMap = CFG.Current.CutsceneEditor_Load_Map;
        if (ImGui.Checkbox($"{LOC.Get("CUTSCENE_FiltersTool_Map")}##cutsceneFilterMap", ref loadMap))
            Editor.SetLoadMap(loadMap);
        GUI.Tooltip(LOC.Get("CUTSCENE_FiltersTool_Map_TT"));

        ImGui.BeginDisabled(!CFG.Current.CutsceneEditor_Load_Map);
        var mapAssets = CFG.Current.CutsceneEditor_Map_Load_Assets;
        if (ImGui.Checkbox($"{LOC.Get("CUTSCENE_FiltersTool_Map_Assets")}##cutsceneFilterMapAssets", ref mapAssets))
            Editor.SetMapAssets(mapAssets);
        GUI.Tooltip(LOC.Get("CUTSCENE_FiltersTool_Map_Assets_TT"));

        var mapCharacters = CFG.Current.CutsceneEditor_Map_Load_Characters;
        if (ImGui.Checkbox($"{LOC.Get("CUTSCENE_FiltersTool_Map_Characters")}##cutsceneFilterMapCharacters", ref mapCharacters))
            Editor.SetMapCharacters(mapCharacters);
        GUI.Tooltip(LOC.Get("CUTSCENE_FiltersTool_Map_Characters_TT"));
        ImGui.EndDisabled();

        var models = CFG.Current.CutsceneEditor_Load_Models;
        if (ImGui.Checkbox($"{LOC.Get("CUTSCENE_FiltersTool_Models")}##cutsceneFilterModels", ref models))
            Editor.SetLoadModels(models);
        GUI.Tooltip(LOC.Get("CUTSCENE_FiltersTool_Models_TT"));

        // Draw Groups
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("CUTSCENE_FiltersTool_Draw_Groups_Header"), LOC.Get("CUTSCENE_FiltersTool_Draw_Groups_Header_TT"));
        DisplayDrawGroups();

        // Overlays
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("CUTSCENE_FiltersTool_Overlays_Header"), LOC.Get("CUTSCENE_FiltersTool_Overlays_Header_TT"));

        ImGui.Checkbox($"{LOC.Get("CUTSCENE_FiltersTool_Camera_Path")}##cutsceneFilterCameraPath", ref CFG.Current.CutsceneEditor_Display_Camera_Path);
        GUI.Tooltip(LOC.Get("CUTSCENE_FiltersTool_Camera_Path_TT"));

        ImGui.Checkbox($"{LOC.Get("CUTSCENE_FiltersTool_Camera")}##cutsceneFilterCamera", ref CFG.Current.CutsceneEditor_Display_Camera);
        GUI.Tooltip(LOC.Get("CUTSCENE_FiltersTool_Camera_TT"));

        ImGui.Checkbox($"{LOC.Get("CUTSCENE_FiltersTool_Markers")}##cutsceneFilterMarkers", ref CFG.Current.CutsceneEditor_Display_Markers);
        GUI.Tooltip(LOC.Get("CUTSCENE_Menu_Display_Markers_TT"));

        ImGui.Checkbox($"{LOC.Get("CUTSCENE_FiltersTool_Subtitles")}##cutsceneFilterSubtitles", ref CFG.Current.CutsceneEditor_Display_Subtitles);
        GUI.Tooltip(LOC.Get("CUTSCENE_Menu_Display_Subtitles_TT"));

        ImGui.Checkbox($"{LOC.Get("CUTSCENE_FiltersTool_Fades")}##cutsceneFilterFades", ref CFG.Current.CutsceneEditor_Display_Fades);
        GUI.Tooltip(LOC.Get("CUTSCENE_FiltersTool_Fades_TT"));

        var effects = CFG.Current.CutsceneEditor_Display_Effects;
        if (ImGui.Checkbox($"{LOC.Get("CUTSCENE_FiltersTool_Effects")}##cutsceneFilterEffects", ref effects))
            Editor.SetDisplayEffects(effects);
        GUI.Tooltip(LOC.Get("CUTSCENE_FiltersTool_Effects_TT"));
        DisplayEffectStatus();

        // Playback
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("CUTSCENE_FiltersTool_Playback_Header"), LOC.Get("CUTSCENE_FiltersTool_Playback_Header_TT"));

        var voices = CFG.Current.CutsceneEditor_Play_Voices;
        if (ImGui.Checkbox($"{LOC.Get("CUTSCENE_FiltersTool_Voices")}##cutsceneFilterVoices", ref voices))
            Editor.SetPlayVoices(voices);
        GUI.Tooltip(LOC.Get("CUTSCENE_Menu_Play_Voices_TT"));

        var sounds = CFG.Current.CutsceneEditor_Play_Sounds;
        if (ImGui.Checkbox($"{LOC.Get("CUTSCENE_FiltersTool_Sounds")}##cutsceneFilterSounds", ref sounds))
            Editor.SetPlaySounds(sounds);
        GUI.Tooltip(LOC.Get("CUTSCENE_Menu_Play_Sounds_TT"));

        // Audio needs the Sound Editor: say so rather than leave the toggles doing nothing
        if (!SoundReferenceHelper.IsAvailable(Editor.Project))
            GUI.WrappedText(LOC.Get("CUTSCENE_FiltersTool_No_Sound"));
    }

    /// <summary>Why effects may not show, or what the cut's effect events are doing</summary>
    private void DisplayEffectStatus()
    {
        if (!CFG.Current.CutsceneEditor_Display_Effects)
            return;

        var effects = Editor.Effects;
        if (!effects.IsAvailable)
        {
            GUI.WrappedText(LOC.Get("CUTSCENE_FiltersTool_Effects_No_Fx"));
            return;
        }

        if (Editor.Scene == null)
            return;

        if (effects.Bank is { IsIndexing: true } bank)
            GUI.WrappedText(LOC.Get("CUTSCENE_FiltersTool_Effects_Indexing", bank.IndexedBinders, bank.TotalBinders));
        else if (effects.EventCount == 0)
            GUI.WrappedText(LOC.Get("CUTSCENE_FiltersTool_Effects_None"));
        else
            GUI.WrappedText(LOC.Get("CUTSCENE_FiltersTool_Effects_Status", effects.EventCount, effects.RunningCount,
                effects.ParticleCount, effects.MissingCount));
    }

    private string CollisionFilter = "";
    private bool CollisionFilterExact;

    /// <summary>
    /// The draw groups the map is drawn in: on or off, what they came from, and the map's collisions to stand on
    /// instead of the one under the camera's start (for a cut whose camera starts somewhere the player can't be).
    /// </summary>
    private void DisplayDrawGroups()
    {
        var backdrop = Editor.Backdrop;

        if (!CFG.Current.CutsceneEditor_Load_Map || backdrop.LoadedMapId == null)
        {
            GUI.WrappedText(LOC.Get("CUTSCENE_FiltersTool_Groups_No_Map"));
            return;
        }

        ImGui.Checkbox($"{LOC.Get("CUTSCENE_FiltersTool_Draw_Groups")}##cutsceneFilterDrawGroups", ref CFG.Current.CutsceneEditor_Map_Use_Draw_Groups);
        GUI.Tooltip(LOC.Get("CUTSCENE_FiltersTool_Draw_Groups_TT"));

        // What the draw groups came from, so a wrong guess shows
        if (!CFG.Current.CutsceneEditor_Map_Use_Draw_Groups)
            GUI.WrappedText(LOC.Get("CUTSCENE_FiltersTool_Groups_Off", backdrop.ShownCount, backdrop.PartCount));
        else if (!backdrop.HasCollision)
            GUI.WrappedText(LOC.Get("CUTSCENE_FiltersTool_Groups_No_Collision", backdrop.PartCount));
        else if (backdrop.StandingOn == null)
            GUI.WrappedText(LOC.Get("CUTSCENE_FiltersTool_Groups_No_Hit", backdrop.PartCount));
        else
            GUI.WrappedText(LOC.Get(backdrop.StandingOnChosen ? "CUTSCENE_FiltersTool_Groups_Chosen" : "CUTSCENE_FiltersTool_Groups_Standing",
                backdrop.StandingOn, backdrop.ActiveDisplay?.Sum(w => System.Numerics.BitOperations.PopCount(w)) ?? 0,
                backdrop.ShownCount, backdrop.PartCount));

        if (!CFG.Current.CutsceneEditor_Map_Use_Draw_Groups || backdrop.Collisions.Count == 0)
            return;

        // The collision to stand on, for this cutscene (kept across its cuts)
        GUI.Spacer();
        EditorFilters.DisplayFramedListFilter("cutsceneCollisionFilter", ref CollisionFilter, ref CollisionFilterExact);

        var chosen = Editor.ChosenCollision;
        var height = Math.Min(10, backdrop.Collisions.Count + 1) * ImGui.GetTextLineHeightWithSpacing() + ImGui.GetStyle().FramePadding.Y * 2;
        if (ImGui.BeginChild("cutsceneCollisionList", new System.Numerics.Vector2(0, height), ImGuiChildFlags.Borders))
        {
            // Auto: the collision under the camera's start, as it was before a choice
            var auto = backdrop.CollisionUnderPoint != null
                ? LOC.Get("CUTSCENE_FiltersTool_Collision_Auto_Named", backdrop.CollisionUnderPoint)
                : LOC.Get("CUTSCENE_FiltersTool_Collision_Auto_None");
            if (ImGui.Selectable($"{auto}##cutsceneCollisionAuto", chosen == null))
                Editor.SetChosenCollision(null);
            GUI.Tooltip(LOC.Get("CUTSCENE_FiltersTool_Collision_Auto_TT"));

            foreach (var (name, groups) in backdrop.Collisions)
            {
                if (!string.IsNullOrEmpty(CollisionFilter) && !EditorFilters.IsMatch(CollisionFilter, name, CollisionFilterExact))
                    continue;

                var label = name == backdrop.CollisionUnderPoint
                    ? LOC.Get("CUTSCENE_FiltersTool_Collision_Row_Under", name, groups)
                    : LOC.Get("CUTSCENE_FiltersTool_Collision_Row", name, groups);

                if (ImGui.Selectable($"{label}##cutsceneCollision_{name}", string.Equals(chosen, name, StringComparison.OrdinalIgnoreCase)))
                    Editor.SetChosenCollision(name);
            }
        }

        ImGui.EndChild();
        GUI.Tooltip(LOC.Get("CUTSCENE_FiltersTool_Collision_List_TT"));
    }
}
