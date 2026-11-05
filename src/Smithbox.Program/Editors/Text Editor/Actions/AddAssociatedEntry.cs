using SoulsFormats;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.TextEditor;

public class AddAssociatedEntry : EditorAction, ITextActionTarget
{
    // ITextActionTarget: lets undo/redo select what this action changed
    public TextContainerWrapper TargetContainer => Info;
    public FMG TargetFmg => Fmg;
    public FMG.Entry TargetEntry => NewEntry;

    private TextEditorView Parent;

    private FMG Fmg;
    private FMG.Entry NewEntry;

    private TextContainerWrapper Info;

    public AddAssociatedEntry(TextEditorView view, TextContainerWrapper info, FMG targetFmg, FMG.Entry newEntry)
    {
        Parent = view;
        Info = info;
        Fmg = targetFmg;
        NewEntry = newEntry;
    }

    public override ActionEvent Execute()
    {
        // Falls back to the end of the list when no entry has a higher ID
        // (this used to insert at index 0).
        var insertionIndex = FmgEntryUtils.GetSortedInsertionIndex(Fmg, NewEntry.ID);
        Fmg.Entries.Insert(insertionIndex, NewEntry);
        Info.MarkDirty(Fmg);

        Parent.DifferenceManager.TrackFmgDifferences();

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Fmg.Entries.Remove(NewEntry);
        Info.MarkDirty(Fmg);

        Parent.DifferenceManager.TrackFmgDifferences();

        return ActionEvent.NoEvent;
    }
}
