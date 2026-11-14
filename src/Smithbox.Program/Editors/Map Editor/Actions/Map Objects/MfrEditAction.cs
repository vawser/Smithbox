using SoulsFormats;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// An edit of a map's .mfr (forbidden region boxes) by the Forbidden Region Editor: the whole document before and
/// after, so every change (adding a file too, from null) undoes the same way.
/// </summary>
public class MfrEditAction : ViewportAction, IMapHistoryItem
{
    private readonly MfrBank Bank;
    private readonly MapContainer Map;
    private readonly MFR Before;
    private readonly MFR After;
    private readonly string Description;

    public string Describe() => Description;

    /// <param name="description">What the edit did, for the History tool.</param>
    public MfrEditAction(MfrBank bank, MapContainer map, MFR before, MFR after, string description)
    {
        Bank = bank;
        Map = map;
        Before = before == null ? null : MfrBank.Copy(before);
        After = after == null ? null : MfrBank.Copy(after);
        Description = description;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Apply(After);
        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Apply(Before);
        return ActionEvent.NoEvent;
    }

    private void Apply(MFR document)
    {
        Bank.Set(Map, document);
        Map.MarkDirty(MapFileType.MFR);
    }
}
