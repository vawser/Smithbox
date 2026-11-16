using SoulsFormats;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.TextEditor;

public class ChangeFmgEntryID : EditorAction, ITextActionTarget
{
    // ITextActionTarget: lets undo/redo select what this action changed
    public TextContainerWrapper TargetContainer => Info;
    public FMG TargetFmg => Entry.Parent;
    public FMG.Entry TargetEntry => Entry;

    private TextEditorView Parent;
    private FMG.Entry Entry;
    private int NewID;
    private int OldID;

    private TextContainerWrapper Info;

    public ChangeFmgEntryID(TextEditorView view, TextContainerWrapper info, FMG.Entry entry, int newId)
    {
        Parent = view;
        Info = info;
        Entry = entry;
        NewID = newId;
        OldID = entry.ID;
    }

    /// <summary>
    /// Where the entry was before Execute moved it; -1 if it wasn't in the list
    /// </summary>
    private int PreviousIndex = -1;

    public override ActionEvent Execute()
    {
        var fmg = Entry.Parent;

        // Move the entry to its place in ID order. It used to stay where it was, so the list
        // was out of order until a save sorted it (the row then jumped), and later adds,
        // which assume a sorted list, could land in the wrong place.
        PreviousIndex = fmg?.Entries.IndexOf(Entry) ?? -1;

        if (PreviousIndex != -1)
        {
            fmg.Entries.RemoveAt(PreviousIndex);
            Entry.ID = NewID;
            fmg.Entries.Insert(FmgEntryUtils.GetSortedInsertionIndex(fmg, NewID), Entry);
        }
        else
        {
            Entry.ID = NewID;
        }

        Info.MarkDirty(fmg);

        Parent.DifferenceManager.TrackFmgDifferences();

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        var fmg = Entry.Parent;

        // Put it back where it was (clamped in case the list shrank since)
        if (PreviousIndex != -1 && fmg != null && fmg.Entries.Remove(Entry))
        {
            Entry.ID = OldID;
            fmg.Entries.Insert(System.Math.Min(PreviousIndex, fmg.Entries.Count), Entry);
        }
        else
        {
            Entry.ID = OldID;
        }

        Info.MarkDirty(fmg);

        Parent.DifferenceManager.TrackFmgDifferences();

        return ActionEvent.NoEvent;
    }
}
