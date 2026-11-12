using Hexa.NET.ImGui;
using System.Numerics;

namespace StudioCore.Editors.SoundEditor;

/// <summary>
/// Read-only facts about the selected bank and sound: our own header parse, then vgmstream's report.
/// </summary>
public class SoundProperties
{
    private readonly SoundEditorView View;

    public SoundProperties(SoundEditorView view)
    {
        View = view;
    }

    public void Draw()
    {
        GUI.TitleHeader(LOC.Get("SOUND_Properties_Header"), LOC.Get("SOUND_Properties_Header_TT"));

        ImGui.BeginChild("soundProperties", Vector2.Zero, ImGuiChildFlags.Borders);

        var c = View.SelectedContainer;
        if (c == null)
        {
            ImGui.TextDisabled(LOC.Get("SOUND_Sounds_Select_Bank"));
            ImGui.EndChild();
            return;
        }

        // Bank
        GUI.SimpleHeader(LOC.Get("SOUND_Properties_Bank_Header"), LOC.Get("SOUND_Properties_Bank_Header_TT"));
        if (BeginTable("soundBankProps"))
        {
            Row("SOUND_Prop_Path", c.Path);
            if (!string.IsNullOrEmpty(c.FileAlias)) Row("SOUND_Prop_File_Alias", c.FileAlias);
            if (!string.IsNullOrEmpty(c.Language)) Row("SOUND_Prop_Language", c.Language);
            if (c.State == SoundLoadState.Loaded)
            {
                Row("SOUND_Prop_Format", c.FormatInfo);
                Row("SOUND_Prop_Sounds", c.Entries.Count.ToString());
                if (c.Kind == SoundContainerKind.WwiseBank)
                {
                    Row("SOUND_Prop_Bank_Id", WwiseBank.Hash(c.Name).ToString());
                    Row("SOUND_Prop_Events", c.EventCount.ToString());
                    Row("SOUND_Prop_Streamed", c.Entries.Count(e => e.IsStreamed).ToString());
                    if (c.GeneratedCount > 0) Row("SOUND_Prop_Generated", c.GeneratedCount.ToString());
                }
            }
            ImGui.EndTable();
        }

        // Events tab: the selected event
        if (View.ActiveTab == SoundListTab.Events)
        {
            if (View.SelectedEvent != null)
                DrawEvent(View.SelectedEvent);
            ImGui.EndChild();
            return;
        }

        var e = View.SelectedEntry;
        if (e == null)
        {
            ImGui.EndChild();
            return;
        }

        // Sound
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("SOUND_Properties_Sound_Header"), LOC.Get("SOUND_Properties_Sound_Header_TT"));
        if (BeginTable("soundEntryProps"))
        {
            if (!string.IsNullOrEmpty(e.Name)) Row("SOUND_Prop_Name", e.Name);
            if (e.SoundId != null) Row("SOUND_Prop_Sound_Id", $"{e.SoundId} ({LOC.Get("SOUND_Prop_Sound_Type")} {e.SoundType})");
            if (e.MediaId != 0) Row("SOUND_Prop_Media_Id", e.MediaId.ToString());
            if (e.SourceFile != null) Row("SOUND_Prop_Source_File", e.SourceFile);
            if (e.InnerFile != null) Row("SOUND_Prop_Inner_File", e.InnerFile);
            Row("SOUND_Prop_Codec", e.Codec);
            if (e.SampleRate > 0) Row("SOUND_Prop_Rate", $"{e.SampleRate} Hz");
            if (e.Channels > 0) Row("SOUND_Prop_Channels", e.Channels.ToString());
            if (e.Seconds > 0) Row("SOUND_Prop_Length", (e.SecondsApprox ? "~" : "") + SoundEditorView.FormatTime(e.Seconds));
            if (e.LoopStart >= 0) Row("SOUND_Prop_Loop", $"{e.LoopStart} - {e.LoopEnd}");
            if (e.Container.Kind == SoundContainerKind.WwiseBank)
            {
                Row("SOUND_Prop_Source", LOC.Get(e.IsStreamed ? "SOUND_Source_Streamed" : "SOUND_Source_InBank"));
                if (e.StreamPath != null) Row("SOUND_Prop_Stream_Path", e.StreamPath);
                Row("SOUND_Prop_Sound_Objects", e.SoundObjectCount.ToString());
            }
            if (e.IsNotAudio) Row("SOUND_Prop_Note", LOC.Get("SOUND_Note_Not_Audio"));
            else if (e.IsEmpty) Row("SOUND_Prop_Note", LOC.Get(e.Source == null ? "SOUND_Note_Missing_Stream" : "SOUND_Note_Empty"));
            ImGui.EndTable();
        }

        // Played by (Wwise events of this bank that can play it)
        if (e.PlayedBy.Count > 0)
        {
            GUI.Spacer();
            GUI.SimpleHeader(LOC.Get("SOUND_Properties_Played_By_Header"), LOC.Get("SOUND_Properties_Played_By_Header_TT"));
            foreach (var ev in e.PlayedBy.Take(50))
            {
                var label = string.IsNullOrEmpty(ev.Alias) ? ev.DisplayName : $"{ev.DisplayName}  ({ev.Alias})";
                if (ImGui.Selectable($"{label}##playedBy_{ev.Id}"))
                    View.GoToEvent(ev);
            }
            if (e.PlayedBy.Count > 50)
                ImGui.TextDisabled(LOC.Get("SOUND_Properties_More", e.PlayedBy.Count - 50));
        }

        // Alias
        DrawAlias(e.SoundId);

        // Decoder
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("SOUND_Properties_Decoder_Header"), LOC.Get("SOUND_Properties_Decoder_Header_TT"));
        if (e.Info != null)
        {
            if (BeginTable("soundDecoderProps"))
            {
                var i = e.Info;
                Row("SOUND_Prop_Encoding", i.Encoding);
                Row("SOUND_Prop_Rate", $"{i.SampleRate} Hz");
                Row("SOUND_Prop_Channels", i.Channels.ToString());
                Row("SOUND_Prop_Length", $"{SoundEditorView.FormatTime(i.Seconds)} ({i.Samples} samples)");
                if (i.LoopStart >= 0) Row("SOUND_Prop_Loop", $"{i.LoopStart} - {i.LoopEnd}");
                if (!string.IsNullOrEmpty(i.StreamName)) Row("SOUND_Prop_Stream_Name", i.StreamName);
                if (i.StreamTotal > 1) Row("SOUND_Prop_Subsong", $"{i.StreamIndex} / {i.StreamTotal}");
                Row("SOUND_Prop_Metadata", i.Metadata);
                ImGui.EndTable();
            }
        }
        else if (e.InfoError != null)
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, e.InfoError);
        else if (e.InfoRequested)
            ImGui.TextDisabled(LOC.Get("SOUND_Properties_Decoder_Reading"));
        else if (!VgmstreamCli.IsAvailable)
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("SOUND_Log_No_Vgmstream", VgmstreamCli.ExePath));

        ImGui.EndChild();
    }

    // the alias being typed, and which sound ID it belongs to (reset when the selection moves to another ID)
    private string AliasInput = "";
    private string AliasInputFor;

    /// <summary>
    /// The project's Sounds alias of the selected sound's ID: shown and edited here, saved to the project's alias file,
    /// so the Param Editor's sound fields and the Time Act Editor's sound events show it too.
    /// </summary>
    private void DrawAlias(string soundId)
    {
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("SOUND_Properties_Alias_Header"), LOC.Get("SOUND_Properties_Alias_Header_TT"));

        if (soundId == null)
        {
            GUI.WrappedText(LOC.Get("SOUND_Alias_No_Sound_Id"));
            return;
        }

        var names = View.Data.Names;
        var current = names.SoundAlias(soundId)?.Name ?? "";
        if (AliasInputFor != soundId)
        {
            AliasInputFor = soundId;
            AliasInput = current;
        }

        ImGui.SetNextItemWidth(-1);
        ImGui.InputText("##soundAliasInput", ref AliasInput, 255);
        GUI.Tooltip(LOC.Get("SOUND_Alias_Input_TT", soundId));
        bool commit = ImGui.IsItemDeactivatedAfterEdit();

        bool changed = AliasInput.Trim() != current;
        GUI.ConditionalMultiButtonInput("soundAliasActions",
            "soundAliasSet", LOC.Get("SOUND_Alias_Set"), LOC.Get("SOUND_Alias_Set_TT", soundId), () => commit = true, changed && AliasInput.Trim().Length > 0,
            "soundAliasClear", LOC.Get("SOUND_Alias_Clear"), LOC.Get("SOUND_Alias_Clear_TT", soundId), () => { AliasInput = ""; commit = true; }, current.Length > 0);

        if (commit && AliasInput.Trim() != current)
        {
            names.SetSoundAlias(soundId, AliasInput);
            Smithbox.Log(this, LOC.Get(AliasInput.Trim().Length > 0 ? "SOUND_Alias_Log_Set" : "SOUND_Alias_Log_Cleared", soundId, AliasInput.Trim()));
        }
    }

    private void DrawEvent(SoundEvent ev)
    {
        // Event
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("SOUND_Properties_Event_Header"), LOC.Get("SOUND_Properties_Event_Header_TT"));
        if (BeginTable("soundEventProps"))
        {
            Row("SOUND_Prop_Name", ev.DisplayName);
            Row("SOUND_Prop_Event_Id", ev.Id.ToString());
            Row("SOUND_Prop_Name_Source", LOC.Get($"SOUND_Event_Name_Source_{ev.NameSource}"));
            if (ev.SoundId != null) Row("SOUND_Prop_Sound_Id", $"{ev.SoundId} ({LOC.Get("SOUND_Prop_Sound_Type")} {ev.SoundType})");
            Row("SOUND_Prop_Actions", ev.ActionSummary);
            if (ev.ExternalTargets > 0) Row("SOUND_Prop_Note", LOC.Get("SOUND_Events_External_TT"));
            ImGui.EndTable();
        }

        // Sounds it can play (click: show in the Sounds tab; double-click: play)
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("SOUND_Properties_Event_Sounds_Header"), LOC.Get("SOUND_Properties_Event_Sounds_Header_TT"));
        if (ev.Entries.Count == 0)
            ImGui.TextDisabled(LOC.Get("SOUND_Properties_Event_No_Sounds"));
        foreach (var e in ev.Entries.Take(100))
        {
            var label = $"{e.DisplayName}  {SoundEditorView.FormatTime(e.BestSeconds)}";
            if (ImGui.Selectable($"{(View.IsCurrent(e) ? "> " : "")}{label}##eventSound_{e.Index}", false, ImGuiSelectableFlags.AllowDoubleClick))
            {
                if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                    View.Play(e);
                else
                    View.GoToEntry(e, false);
            }
        }
        if (ev.Entries.Count > 100)
            ImGui.TextDisabled(LOC.Get("SOUND_Properties_More", ev.Entries.Count - 100));

        // Alias
        DrawAlias(ev.SoundId);
    }

    private static bool BeginTable(string id)
    {
        if (!ImGui.BeginTable(id, 2, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.RowBg))
            return false;
        ImGui.TableSetupColumn("##k");
        ImGui.TableSetupColumn("##v", ImGuiTableColumnFlags.WidthStretch);
        return true;
    }

    private static void Row(string key, string value)
    {
        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.TextDisabled(LOC.Get(key));
        ImGui.TableNextColumn();
        ImGui.TextWrapped(value ?? "");
        if (ImGui.IsItemHovered() && ImGui.IsMouseClicked(ImGuiMouseButton.Right))
            ImGui.SetClipboardText(value ?? "");
    }
}
