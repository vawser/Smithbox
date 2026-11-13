using Hexa.NET.ImGui;
using System.Diagnostics;
using System.Numerics;

namespace StudioCore.Editors.SoundEditor;

/// <summary>
/// Transport for the editor's player: play/pause, stop, loop, volume, position.
/// </summary>
public class SoundPlayerBar
{
    private readonly SoundEditorView View;

    // while the position slider is held, show the dragged value and seek on release
    private bool Dragging;
    private float DragValue;

    public SoundPlayerBar(SoundEditorView view)
    {
        View = view;
    }

    public void Draw()
    {
        var player = View.Player;
        // on the Events tab, Play plays the selected event's first sound
        var sel = View.ActiveTab == SoundListTab.Events
            ? View.SelectedEvent?.Entries.FirstOrDefault(e => e.CanPlay)
            : View.SelectedEntry;

        // Now playing
        var current = player.Current;
        var title = current != null ? current.Name : LOC.Get("SOUND_Player_Nothing");
        ImGui.TextDisabled(LOC.Get(StateKey(player.State)));
        ImGui.SameLine();
        ImGui.TextUnformatted(title);

        // Buttons
        var playLabel = player.State == SoundPlayerState.Playing ? LOC.Get("SOUND_Player_Pause") : LOC.Get("SOUND_Player_Play");
        bool canPlay = player.IsActive || sel?.CanPlay == true;
        if (!canPlay) ImGui.BeginDisabled();
        if (ImGui.Button($"{playLabel}##soundPlayPause"))
        {
            // Play starts the selected sound if it isn't the one the player has; otherwise pause/resume
            if (sel != null && !View.IsCurrent(sel) && sel.CanPlay)
                View.Play(sel);
            else
                player.TogglePause();
        }
        if (!canPlay) ImGui.EndDisabled();
        GUI.Tooltip(LOC.Get("SOUND_Player_Play_TT"));

        ImGui.SameLine();
        if (!player.IsActive) ImGui.BeginDisabled();
        if (ImGui.Button($"{LOC.Get("SOUND_Player_Stop")}##soundStop"))
            player.Stop();
        if (!player.IsActive) ImGui.EndDisabled();
        GUI.Tooltip(LOC.Get("SOUND_Player_Stop_TT"));

        ImGui.SameLine();
        var loop = player.Loop;
        if (ImGui.Checkbox($"{LOC.Get("SOUND_Player_Loop")}##soundLoop", ref loop))
            player.Loop = loop;
        GUI.Tooltip(LOC.Get("SOUND_Player_Loop_TT"));

        ImGui.SameLine();
        var volume = player.Volume * 100f;
        ImGui.SetNextItemWidth(120 * DPI.UIScale());
        if (ImGui.SliderFloat("##soundVolume", ref volume, 0f, 100f, $"{LOC.Get("SOUND_Player_Volume")} %.0f%%"))
            player.Volume = volume / 100f;
        GUI.Tooltip(LOC.Get("SOUND_Player_Volume_TT"));

        // Position
        var duration = (float)player.Duration;
        var pos = Dragging ? DragValue : (float)player.Position;
        ImGui.SetNextItemWidth(-1);
        if (duration <= 0) ImGui.BeginDisabled();
        var text = $"{SoundEditorView.FormatTime(pos)} / {SoundEditorView.FormatTime(duration)}";
        if (ImGui.SliderFloat("##soundPosition", ref pos, 0f, Math.Max(duration, 0.001f), text))
        {
            Dragging = true;
            DragValue = pos;
        }
        if (ImGui.IsItemDeactivatedAfterEdit() && Dragging)
        {
            player.Seek(DragValue);
            Dragging = false;
        }
        else if (!ImGui.IsItemActive())
            Dragging = false;
        if (duration <= 0) ImGui.EndDisabled();
        GUI.Tooltip(LOC.Get("SOUND_Player_Position_TT"));

        if (player.State == SoundPlayerState.Failed && player.Error != null)
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, player.Error);

        DrawExport();
        DrawEdits();
    }

    /// <summary>Unsaved replacements: how many, finding the banks they touch, saving.</summary>
    private void DrawEdits()
    {
        var data = View.Data;
        if (data == null || !View.CanEdit || (data.Edits.Count == 0 && !data.Edits.IsSaving))
            return;

        ImGui.Separator();
        var map = data.MediaMap;
        if (data.Edits.IsSaving)
            ImGui.TextUnformatted(LOC.Get("SOUND_Edits_Saving"));
        else if (!map.IsBuilt && map.Error == null)
            ImGui.ProgressBar(map.Total > 0 ? (float)map.Progress / map.Total : 0f, new Vector2(-1, 0),
                LOC.Get("SOUND_MediaMap_Progress", map.Progress, map.Total));
        else
        {
            var files = data.Edits.PendingFiles();
            ImGui.TextUnformatted(LOC.Get("SOUND_Edits_Pending", data.Edits.Count, files.Count));
            GUI.Tooltip(string.Join("\n", files.Take(40)) + (files.Count > 40 ? "\n…" : ""));
            ImGui.SameLine();
            if (ImGui.SmallButton($"{LOC.Get("EDITOR_Menubar_Action_Save")}##soundEditsSave"))
                View.SaveEdits();
        }
    }

    /// <summary>Progress of the running export, or the result of the last one until dismissed.</summary>
    private void DrawExport()
    {
        var job = View.ExportJob;
        if (job == null)
            return;

        ImGui.Separator();
        if (job.Running)
        {
            var frac = job.Total > 0 ? (float)(job.Done + job.Failed) / job.Total : 0f;
            var text = job.Total > 0 ? LOC.Get("SOUND_Export_Progress", job.Label, job.Done + job.Failed, job.Total) : LOC.Get("SOUND_Export_Preparing", job.Label);
            ImGui.ProgressBar(frac, new Vector2(-90 * DPI.UIScale(), 0), text);
            ImGui.SameLine();
            if (ImGui.Button($"{LOC.Get("SOUND_Export_Cancel")}##soundExportCancel", new Vector2(-1, 0)))
                job.Cancel();
        }
        else
        {
            var key = job.Cancelled ? "SOUND_Export_Result_Cancelled" : job.Failed > 0 || job.Total == 0 ? "SOUND_Export_Result_Failed" : "SOUND_Export_Result_Done";
            ImGui.TextUnformatted(LOC.Get(key, job.Label, job.Done, job.Total, job.Failed));
            ImGui.SameLine();
            if (ImGui.SmallButton($"{LOC.Get("SOUND_Export_Open_Folder")}##soundExportOpen") && Directory.Exists(job.Folder))
                Process.Start("explorer.exe", job.Folder);
            ImGui.SameLine();
            if (ImGui.SmallButton($"{LOC.Get("SOUND_Export_Dismiss")}##soundExportDismiss"))
                View.ExportJob = null;
        }

        if (job.FirstError != null)
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, job.FirstError);
    }

    private static string StateKey(SoundPlayerState s) => s switch
    {
        SoundPlayerState.Loading => "SOUND_Player_State_Loading",
        SoundPlayerState.Playing => "SOUND_Player_State_Playing",
        SoundPlayerState.Paused => "SOUND_Player_State_Paused",
        SoundPlayerState.Failed => "SOUND_Player_State_Failed",
        _ => "SOUND_Player_State_Stopped",
    };
}
