using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.MapDataEditor;

/// <summary>
/// Undoable/redoable deletion of one or more MSB entries.
///
/// The target list and the entries to remove are both resolved at construction
/// time so that subsequent selection changes do not affect Execute() or Undo().
/// </summary>
public class MsbEntryDelete : EditorAction
{
    private readonly MapDataEditorView _view;

    // The live backing list inside the MSB param — captured once at construction.
    private readonly IList _targetList;

    // Snapshot of what will be deleted: (listIndex, entryObject), ascending order.
    private readonly List<(int Index, object Entry)> _deletedEntries;

    public MsbEntryDelete(MapDataEditorView view, ProjectEntry project)
    {
        _view = view;

        // Resolve the list immediately while the selection is still valid.
        var entryView = view.MsbEditor.EntryView;
        object paramObj = entryView.GetParamObject();
        _targetList = paramObj is not null ? entryView.FindMutableEntryList(paramObj) : null;

        // Snapshot the selected entries now, before Execute() is called.
        _deletedEntries = view.Selection.SelectedEntries
            .OrderBy(kv => kv.Key)
            .Select(kv => (kv.Key, kv.Value))
            .ToList();
    }

    public override ActionEvent Execute()
    {
        if (_targetList is null || _deletedEntries.Count == 0)
            return ActionEvent.NoEvent;

        // Remove in reverse-index order so earlier indices stay valid as we go.
        for (int i = _deletedEntries.Count - 1; i >= 0; i--)
        {
            int idx = _deletedEntries[i].Index;
            if (idx >= 0 && idx < _targetList.Count)
                _targetList.RemoveAt(idx);
        }

        _view.Selection.ResetMsbEntrySelection();
        _view.MsbEditor.EntryView.RebuildEntryCache();

        return ActionEvent.ObjectAddedRemoved;
    }

    public override ActionEvent Undo()
    {
        if (_targetList is null)
            return ActionEvent.NoEvent;

        // Reinsert in ascending index order so each Insert lands at the right spot.
        foreach (var (index, entry) in _deletedEntries)
        {
            int clampedIndex = Math.Clamp(index, 0, _targetList.Count);
            _targetList.Insert(clampedIndex, entry);
        }

        // Restore the selection to the re-added entries.
        _view.Selection.ResetMsbEntrySelection();

        _view.MsbEditor.EntryView.RebuildEntryCache();

        return ActionEvent.ObjectAddedRemoved;
    }
}