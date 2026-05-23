using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.MapDataEditor;

public class MsbEntryDelete : EditorAction
{
    private readonly MapDataEditorView _view;

    private readonly IList _targetList;

    private readonly List<(int Index, object Entry)> _deletedEntries;

    public MsbEntryDelete(MapDataEditorView view, ProjectEntry project)
    {
        _view = view;

        var entryView = view.MsbEditor.EntryView;
        object paramObj = entryView.GetParamObject();
        _targetList = paramObj is not null ? entryView.FindMutableEntryList(paramObj) : null;

        _deletedEntries = view.Selection.SelectedEntries
            .OrderBy(kv => kv.Key)
            .Select(kv => (kv.Key, kv.Value))
            .ToList();
    }

    public override ActionEvent Execute()
    {
        if (_targetList is null || _deletedEntries.Count == 0)
            return ActionEvent.NoEvent;

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

        foreach (var (index, entry) in _deletedEntries)
        {
            int clampedIndex = Math.Clamp(index, 0, _targetList.Count);
            _targetList.Insert(clampedIndex, entry);
        }

        _view.Selection.ResetMsbEntrySelection();

        _view.MsbEditor.EntryView.RebuildEntryCache();

        return ActionEvent.ObjectAddedRemoved;
    }
}