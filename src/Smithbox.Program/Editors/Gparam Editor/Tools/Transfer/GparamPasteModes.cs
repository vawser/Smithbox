using StudioCore.Application;

namespace StudioCore.Editors.GparamEditor;

/// <summary>
/// How a whole-GPARAM payload is applied to a file.
/// </summary>
public enum GparamFilePasteMode
{
    /// <summary>
    /// Replace every group in the file with the payload's.
    /// </summary>
    ReplaceAll,

    /// <summary>
    /// Replace the groups whose key is in the payload, add the ones missing, keep the rest.
    /// </summary>
    Merge
}

/// <summary>
/// How a group or field payload is applied. Groups and fields are matched by key.
/// </summary>
public enum GparamKeyedPasteMode
{
    /// <summary>
    /// Replace the entry with the same key, or add it if there is none.
    /// </summary>
    MatchKey,

    /// <summary>
    /// Add only the entries whose key is missing; leave existing ones alone.
    /// </summary>
    AddMissing,

    /// <summary>
    /// Replace the selected entry with the pasted one, in its place. Takes one entry, whose key no other entry
    /// may already use.
    /// </summary>
    ReplaceSelected
}

/// <summary>
/// How pasted values are placed in the target field.
/// </summary>
public enum GparamValuePasteMode
{
    /// <summary>
    /// Insert after the last selected value (or at the end, with none selected). Colliding IDs are renumbered.
    /// </summary>
    InsertAfter,

    /// <summary>
    /// Replace the selected values, in order. Needs as many selected values as the payload holds.
    /// </summary>
    ReplaceSelection,

    /// <summary>
    /// Replace the value with the same ID, or insert it in ID order if there is none.
    /// </summary>
    ReplaceById
}

/// <summary>
/// The paste mode for each payload kind. Only the one for the payload's kind is used.
/// </summary>
public record GparamPasteModes(
    GparamFilePasteMode File = GparamFilePasteMode.ReplaceAll,
    GparamKeyedPasteMode Group = GparamKeyedPasteMode.MatchKey,
    GparamKeyedPasteMode Field = GparamKeyedPasteMode.MatchKey,
    GparamValuePasteMode Values = GparamValuePasteMode.InsertAfter)
{
    /// <summary>
    /// The modes saved in the settings: the defaults Paste uses.
    /// </summary>
    public static GparamPasteModes FromConfig()
    {
        return new GparamPasteModes(
            CFG.Current.GparamEditor_Transfer_File_Mode,
            CFG.Current.GparamEditor_Transfer_Group_Mode,
            CFG.Current.GparamEditor_Transfer_Field_Mode,
            CFG.Current.GparamEditor_Transfer_Values_Mode);
    }

    public string Describe(GparamTransferKind kind)
    {
        return kind switch
        {
            GparamTransferKind.Gparam => Name(File),
            GparamTransferKind.Group => Name(Group),
            GparamTransferKind.Field => Name(Field),
            GparamTransferKind.Values => Name(Values),
            _ => ""
        };
    }

    // The localization key of each mode's name; its tooltip is the same key with _TT
    public static string Key(GparamFilePasteMode mode) => $"GPARAM_Transfer_Mode_File_{mode}";
    public static string Key(GparamKeyedPasteMode mode) => $"GPARAM_Transfer_Mode_Keyed_{mode}";
    public static string Key(GparamValuePasteMode mode) => $"GPARAM_Transfer_Mode_Values_{mode}";

    public static string Name(GparamFilePasteMode mode) => LOC.Get(Key(mode));
    public static string Name(GparamKeyedPasteMode mode) => LOC.Get(Key(mode));
    public static string Name(GparamValuePasteMode mode) => LOC.Get(Key(mode));
}
