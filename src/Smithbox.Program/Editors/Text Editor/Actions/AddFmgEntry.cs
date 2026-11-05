using SoulsFormats;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.TextEditor;

public class AddFmgEntry : EditorAction, ITextActionTarget
{
    // ITextActionTarget: lets undo/redo select what this action changed
    public TextContainerWrapper TargetContainer => Info;
    public FMG TargetFmg => Fmg;
    public FMG.Entry TargetEntry => NewEntry;

    private TextEditorView Parent;

    private FMG Fmg;
    private FMG.Entry NewEntry;

    private TextContainerWrapper Info;

    private bool IgnoreDiffCheck = false;

    public AddFmgEntry(TextEditorView view, TextContainerWrapper info, FMG.Entry sourceEntry, FMG.Entry newEntry, int newID, bool ignoreDiffCheck = false)
    {
        Parent = view;
        Info = info;
        Fmg = sourceEntry.Parent;
        NewEntry = newEntry;
        NewEntry.ID = newID;

        IgnoreDiffCheck = ignoreDiffCheck;
    }

    public override ActionEvent Execute()
    {
        // Position is resolved now rather than at construction, so batches of adds
        // and lists re-sorted by a save still land in ID order.
        var insertionIndex = FmgEntryUtils.GetSortedInsertionIndex(Fmg, NewEntry.ID);
        Fmg.Entries.Insert(insertionIndex, NewEntry);

        Info.MarkDirty(Fmg);

        if (!IgnoreDiffCheck)
        {
            Parent.DifferenceManager.TrackFmgDifferences();
        }

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        // Remove by reference: the entry may have moved since Execute.
        Fmg.Entries.Remove(NewEntry);

        Info.MarkDirty(Fmg);

        if (!IgnoreDiffCheck)
        {
            Parent.DifferenceManager.TrackFmgDifferences();
        }

        return ActionEvent.NoEvent;
    }
}
