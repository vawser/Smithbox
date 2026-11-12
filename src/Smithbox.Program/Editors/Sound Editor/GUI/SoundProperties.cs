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
            if (e.MediaId != 0) Row("SOUND_Prop_Media_Id", e.MediaId.ToString());
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
