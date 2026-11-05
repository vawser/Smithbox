using SoulsFormats;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.TextEditor;

public class DeleteFmgEntry : EditorAction, ITextActionTarget
{
    // ITextActionTarget: lets undo/redo select what this action changed
    public TextContainerWrapper TargetContainer => Info;
    public FMG TargetFmg => Fmg;
    public FMG.Entry TargetEntry => Entry;

    private TextEditorView Parent;

    private FMG Fmg;

    /// <summary>
    /// The deleted entry itself (not a copy), so earlier actions on the undo stack
    /// that reference it still apply to the entry that undo puts back.
    /// </summary>
    private FMG.Entry Entry;

    /// <summary>
    /// Where the entry was when it was removed; -1 if it wasn't in the list.
    /// </summary>
    private int RemovedIndex = -1;

    private TextContainerWrapper Info;

    public DeleteFmgEntry(TextEditorView view, TextContainerWrapper info, FMG currentFmg, FMG.Entry entry)
    {
        Parent = view;
        Info = info;
        Fmg = currentFmg;
        Entry = entry;
    }

    public override ActionEvent Execute()
    {
        // Find the exact entry by reference, so duplicate IDs can't select the wrong one.
        RemovedIndex = Fmg.Entries.IndexOf(Entry);

        if (RemovedIndex != -1)
        {
            Fmg.Entries.RemoveAt(RemovedIndex);
            Info.MarkDirty(Fmg);
        }

        Parent.DifferenceManager.TrackFmgDifferences();

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        if (RemovedIndex != -1)
        {
            // Clamp in case the list shrank since; order is cosmetic as saves write by ID.
            var index = RemovedIndex > Fmg.Entries.Count ? Fmg.Entries.Count : RemovedIndex;

            Fmg.Entries.Insert(index, Entry);
            Info.MarkDirty(Fmg);
        }

        Parent.DifferenceManager.TrackFmgDifferences();

        return ActionEvent.NoEvent;
    }
}
