#nullable enable
using StudioCore.Editors.ParamEditor;
using StudioCore.Editors.TextEditor;
using System;
using System.Linq;

namespace StudioCore.Editors.CutsceneEditor;

/// <summary>
/// A subtitle event's talkParamId, resolved through the project's TalkParam: the row, the subtitle text and the
/// voice it plays. Shared by the parameter links (Properties) and the subtitle playback (viewport). Everything comes
/// from the Param and Text Editors' data, so it is null when those aren't loaded.
/// </summary>
public class CutsceneTalk
{
    private readonly ProjectEntry Project;

    public CutsceneTalk(ProjectEntry project)
    {
        Project = project;
    }

    /// <summary>The voice fields of TalkParam, in the order tried: ER/AC6/DS3 voiceId; SDT one per speaker slot
    /// (soundId_pc for the player, soundId0-7 for the others, as its lookAtTarget_pc/0-7). NR's has no known one.</summary>
    private static readonly string[] VoiceFields =
    [
        "voiceId", "soundId_pc", "soundId0", "soundId1", "soundId2", "soundId3", "soundId4", "soundId5", "soundId6", "soundId7"
    ];

    /// <summary>The sound type letter TalkParam voices play as: (v) Voice in every game's SoundType</summary>
    public const string VoiceLetter = "v";

    public ParamBank? ParamBank => Project.Handler.ParamEditor?.ViewHandler?.ActiveView?.GetPrimaryBank();

    /// <summary>The TalkParam row of an ID, or null (no row, or no params loaded)</summary>
    public Andre.Formats.Param.Row? Row(int id)
    {
        var bank = ParamBank;
        if (id <= 0 || bank?.Params == null || !bank.Params.TryGetValue("TalkParam", out var param))
            return null;

        return param[id];
    }

    /// <summary>
    /// The subtitle a TalkParam row names: its first field whose metadata gives an FMG (msgId in ER/AC6, textId in
    /// SDT, dialogueTextId_Male in NR), else msgId in Talk_Messages (DS3's meta names none).
    /// </summary>
    public TextResult? Text(Andre.Formats.Param.Row row)
    {
        var view = Project.Handler.TextEditor?.ViewHandler?.ActiveView;
        if (view == null)
            return null;

        var paramData = Project.Handler.ParamData;
        var param = ParamBank?.Params?.GetValueOrDefault("TalkParam");
        var meta = paramData != null && param?.AppliedParamdef != null ? paramData.GetParamMeta(param.AppliedParamdef) : null;

        if (meta != null)
        {
            foreach (var column in row.Columns)
            {
                if (paramData!.GetParamFieldMeta(meta, column.Def)?.FmgRef?.FirstOrDefault()?.fmg is not { } fmg)
                    continue;

                return ToInt(row[column.Def.InternalName]?.Value) is { } textId and >= 0
                    ? TextFinder.GetTextResult(view, fmg, textId)
                    : null;
            }
        }

        // No field names an FMG: the DS3 layout
        return ToInt(row["msgId"]?.Value) is { } msgId and >= 0 ? TextFinder.GetTextResult(view, "Talk_Messages", msgId) : null;
    }

    /// <summary>The subtitle text of a TalkParam ID, or null</summary>
    public string? TextOf(int talkParamId) =>
        Row(talkParamId) is { } row && Text(row)?.Entry?.Text is { Length: > 0 } text ? text : null;

    /// <summary>The voice sound ID a TalkParam row plays, and the field it came from; null if it has none set</summary>
    public (int Id, string Field)? Voice(Andre.Formats.Param.Row row)
    {
        foreach (var field in VoiceFields)
        {
            if (ToInt(row[field]?.Value) is { } id and > 0)
                return (id, field);
        }

        return null;
    }

    public static int? ToInt(object? value) => value switch
    {
        sbyte v => v,
        byte v => v,
        short v => v,
        ushort v => v,
        int v => v,
        uint v => v <= int.MaxValue ? (int)v : null,
        long v => v is >= int.MinValue and <= int.MaxValue ? (int)v : null,
        _ => null
    };
}
