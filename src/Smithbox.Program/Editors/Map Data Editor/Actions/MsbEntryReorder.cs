using StudioCore.Editors.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapDataEditor;

/// <summary>
/// Moves a contiguous or discontiguous block of selected entries to a new
/// position within their owning IList, preserving their relative order.
/// Supports full undo/redo.
/// </summary>
public class MsbEntryReorder : EditorAction
{
    private readonly IList _list;

    private readonly List<int> _originalIndices;

    private readonly int _toIndex;

    private List<int> _movedIndices;

    public IReadOnlyList<int> MovedIndices => _movedIndices;

    public MsbEntryReorder(IList list, IEnumerable<int> selectedIndices, int toIndex)
    {
        _list = list;
        _originalIndices = selectedIndices.OrderBy(x => x).ToList();
        _toIndex = toIndex;
    }

    public override ActionEvent Execute()
    {
        _movedIndices = MoveBlock(_originalIndices, _toIndex);
        return ActionEvent.NoEvent;
    }

    // TODO: doesn't work currently
    public override ActionEvent Undo()
    {
        if (_movedIndices is null) 
            return ActionEvent.NoEvent;

        MoveBlock(_movedIndices, _originalIndices[0]);
        _movedIndices = null;

        return ActionEvent.NoEvent;
    }

    private List<int> MoveBlock(List<int> sourceIndices, int targetIndex)
    {
        if (sourceIndices.Count == 0) return new List<int>();

        var items = sourceIndices.Select(i => _list[i]).ToList();

        for (int i = sourceIndices.Count - 1; i >= 0; i--)
            _list.RemoveAt(sourceIndices[i]);

        int shift = sourceIndices.Count(i => i < targetIndex);
        int adjustedTarget = targetIndex - shift;

        bool movingForward = sourceIndices[0] < targetIndex;
        int insertAt = movingForward
            ? Math.Min(adjustedTarget + 1, _list.Count)
            : Math.Clamp(adjustedTarget, 0, _list.Count);

        for (int i = 0; i < items.Count; i++)
            _list.Insert(insertAt + i, items[i]);

        return Enumerable.Range(insertAt, items.Count).ToList();
    }
}