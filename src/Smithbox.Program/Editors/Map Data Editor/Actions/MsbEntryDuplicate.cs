using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace StudioCore.Editors.MapDataEditor;

public class MsbEntryDuplicate : EditorAction
{
    private readonly MapDataEditorView _view;

    private readonly IList _targetList;

    private readonly List<(int Index, object Entry)> _sourceEntries;

    private readonly List<(int InsertedIndex, object Clone)> _clonedEntries = new();

    public MsbEntryDuplicate(MapDataEditorView view, ProjectEntry project)
    {
        _view = view;

        var entryView = view.MsbEditor.EntryView;
        object paramObj = entryView.GetParamObject();
        _targetList = paramObj is not null ? entryView.FindMutableEntryList(paramObj) : null;

        _sourceEntries = view.Selection.SelectedEntries
            .OrderBy(kv => kv.Key)
            .Select(kv => (kv.Key, kv.Value))
            .ToList();
    }

    public override ActionEvent Execute()
    {
        if (_targetList is null || _sourceEntries.Count == 0)
            return ActionEvent.NoEvent;

        _clonedEntries.Clear();

        var toInsert = _sourceEntries
            .Select(s => (InsertAfter: s.Index, Clone: CloneEntry(s.Entry)))
            .ToList();

        for (int i = toInsert.Count - 1; i >= 0; i--)
        {
            var (insertAfter, clone) = toInsert[i];
            int insertAt = Math.Clamp(insertAfter + 1, 0, _targetList.Count);

            _targetList.Insert(insertAt, clone);
            _clonedEntries.Insert(0, (insertAt, clone));
        }

        _view.Selection.ResetMsbEntrySelection();
        foreach (var (insertedIndex, clone) in _clonedEntries)
        {
            _view.Selection.SelectedEntries.TryAdd(insertedIndex, clone);
        }

        _view.MsbEditor.EntryView.RebuildEntryCache();

        return ActionEvent.ObjectAddedRemoved;
    }

    public override ActionEvent Undo()
    {
        if (_targetList is null)
            return ActionEvent.NoEvent;

        foreach (var (_, clone) in Enumerable.Reverse(_clonedEntries))
        {
            for (int i = _targetList.Count - 1; i >= 0; i--)
            {
                if (ReferenceEquals(_targetList[i], clone))
                {
                    _targetList.RemoveAt(i);
                    break;
                }
            }
        }

        _view.Selection.ResetMsbEntrySelection();
        foreach (var (index, entry) in _sourceEntries)
            _view.Selection.SelectedEntries.TryAdd(index, entry);

        _view.MsbEditor.EntryView.RebuildEntryCache();

        return ActionEvent.ObjectAddedRemoved;
    }

    private static object CloneEntry(object source)
    {
        object clone;

        var deepCopyMethod = source.GetType().GetMethod("DeepCopy",
            BindingFlags.Public | BindingFlags.Instance,
            binder: null,
            types: Type.EmptyTypes,
            modifiers: null);

        if (deepCopyMethod is not null)
        {
            clone = deepCopyMethod.Invoke(source, null);
        }
        else
        {
            Smithbox.LogError<MsbEditor>($"[Map Data Editor] {source.GetType().FullName} has no DeepCopy() " +
                $"method; falling back to MemberwiseClone. Nested reference fields will be shared.");

            var memberwiseClone = typeof(object).GetMethod("MemberwiseClone",
                BindingFlags.Instance | BindingFlags.NonPublic);
            clone = memberwiseClone!.Invoke(source, null);
        }

        var nameProp = clone.GetType().GetProperty("Name",
            BindingFlags.Public | BindingFlags.Instance);

        if (nameProp is not null && nameProp.CanWrite)
        {
            string original = nameProp.GetValue(clone) as string ?? string.Empty;
            nameProp.SetValue(clone, $"{original} - Copy");
        }

        return clone;
    }
}