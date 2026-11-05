using SoulsFormats;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.TextEditor;

public class DuplicateFmgEntry : EditorAction, ITextActionTarget
{
    // ITextActionTarget: lets undo/redo select what this action changed
    public TextContainerWrapper TargetContainer => Info;
    public FMG TargetFmg => Fmg;
    public FMG.Entry TargetEntry => NewEntry;

    private TextEditorView Parent;
    private FMG Fmg;

    /// <summary>
    /// Created once, so redo re-inserts the same object that later actions reference.
    /// </summary>
    private FMG.Entry NewEntry;

    private TextContainerWrapper Info;

    public DuplicateFmgEntry(TextEditorView view, TextContainerWrapper info, FMG currentFmg, FMG.Entry entry, int newID)
    {
        Parent = view;
        Info = info;
        Fmg = currentFmg;
        NewEntry = entry.Clone();
        NewEntry.ID = newID;
    }

    public override ActionEvent Execute()
    {
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
