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
        ImGui.EndDisabled();
        GUI.Tooltip(LOC.Get("CUTSCENE_FiltersTool_Map_Assets_TT"));

        var models = CFG.Current.CutsceneEditor_Load_Models;
        if (ImGui.Checkbox($"{LOC.Get("CUTSCENE_FiltersTool_Models")}##cutsceneFilterModels", ref models))
            Editor.SetLoadModels(models);
        GUI.Tooltip(LOC.Get("CUTSCENE_FiltersTool_Models_TT"));

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
}
