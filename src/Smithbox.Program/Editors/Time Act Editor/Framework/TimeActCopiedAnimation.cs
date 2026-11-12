using SoulsFormats;
using System;

namespace StudioCore.Editors.TimeActEditor;

/// <summary>
/// An animation on the editor's clipboard. It is a copy taken when it was copied, so later edits to the
/// animation don't change what is pasted. Its events hold only their bytes, so the time act it is pasted into
/// applies its own template.
/// </summary>
public class TimeActCopiedAnimation
{
    public TAE.Animation Animation { get; }

    /// <summary>
    /// The name of the animation in the time act it was copied from, such as a000_003000
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The source it was copied from, for the log
    /// </summary>
    public string SourceName { get; }

    /// <summary>
    /// The byte order of the event parameters. A copy can only go into a time act with the same order.
    /// </summary>
    public bool BigEndian { get; }

    public TimeActCopiedAnimation(TimeActContainerWrapper source, TimeActEntry entry, TAE.Animation animation)
    {
        BigEndian = entry.TimeAct.BigEndian;
        Name = source.GetAnimationName(entry, animation);
        SourceName = source.Name;

        Animation = TimeActEntry.CloneAnimation(animation, animation.ID, animation.AnimFileName, BigEndian, false);
    }

    /// <summary>
    /// The file name for a pasted copy. A file name that holds the animation's own name gets the new one,
    /// as duplicating does.
    /// </summary>
    public string GetFileName(string newName)
    {
        var fileName = Animation.AnimFileName;

        if (string.IsNullOrEmpty(fileName) || !fileName.Contains(Name, StringComparison.OrdinalIgnoreCase))
            return fileName ?? "";

        return fileName.Replace(Name, newName, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// The full ID of the animation whose motion this one plays, if its mini header points to another
    /// </summary>
    public long? GetImportedID() => Animation.MiniHeader switch
    {
        TAE.Animation.AnimMiniHeader.Standard { ImportsHKX: true } standard => standard.ImportHKXSourceAnimID,
        TAE.Animation.AnimMiniHeader.ImportOtherAnim import => import.ImportFromAnimID,
        _ => null
    };
}
