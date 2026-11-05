using SoulsFormats;
using StudioCore.Editors.Common;
using System.Collections.Generic;

namespace StudioCore.Editors.TextEditor;

public class SortFmgList : EditorAction
{
    private TextFmgWrapper Wrapper;

    /// <summary>
    /// The order before the sort, so Undo can restore it before earlier actions
    /// on the stack are undone.
    /// </summary>
    private List<FMG.Entry> PreviousOrder;
    private bool PreviousDirty;

    public SortFmgList(TextFmgWrapper wrapper)
    {
        Wrapper = wrapper;
    }

    public override ActionEvent Execute()
    {
        PreviousOrder = new List<FMG.Entry>(Wrapper.File.Entries);
        PreviousDirty = Wrapper.IsDirty;

        // List.Sort is unstable; a stable sort keeps duplicate-ID entries in their existing order.
        var sorted = new List<FMG.Entry>(Wrapper.File.Entries);
        sorted.Sort(new StableIdComparer(PreviousOrder));

        Wrapper.File.Entries.Clear();
        Wrapper.File.Entries.AddRange(sorted);
        Wrapper.IsDirty = true;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        if (PreviousOrder == null)
            return ActionEvent.NoEvent;

        Wrapper.File.Entries.Clear();
        Wrapper.File.Entries.AddRange(PreviousOrder);
        Wrapper.IsDirty = PreviousDirty;

        return ActionEvent.NoEvent;
    }

    private class StableIdComparer : IComparer<FMG.Entry>
    {
        private readonly Dictionary<FMG.Entry, int> OriginalIndex = new(ReferenceEqualityComparer.Instance);

        public StableIdComparer(List<FMG.Entry> originalOrder)
        {
            for (int i = 0; i < originalOrder.Count; i++)
                OriginalIndex[originalOrder[i]] = i;
        }

        public int Compare(FMG.Entry a, FMG.Entry b)
        {
            var result = a.ID.CompareTo(b.ID);

            if (result != 0)
                return result;

            return OriginalIndex[a].CompareTo(OriginalIndex[b]);
        }
    }
}
