using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace StudioCore.Editors.MapDataEditor;

/// <summary>
/// Undoable/redoable duplication of one or more MSB entries.
///
/// The target list and source entries are both resolved at construction time
/// so that subsequent selection changes do not affect Execute() or Undo().
/// Each selected entry is shallow-cloned via MemberwiseClone and inserted
/// immediately after its source, with " - Copy" appended to its Name.
/// </summary>
public class MsbEntryDuplicate : EditorAction
{
    private readonly MapDataEditorView _view;

    // The live backing list inside the MSB param — captured once at construction.
    private readonly IList _targetList;

    // Source entries snapshotted at construction time.
    private readonly List<(int Index, object Entry)> _sourceEntries;

    // Populated during Execute(); used by Undo() to remove by reference identity.
    private readonly List<(int InsertedIndex, object Clone)> _clonedEntries = new();

    public MsbEntryDuplicate(MapDataEditorView view, ProjectEntry project)
    {
        _view = view;

        // Resolve the list immediately while the selection is still valid.
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

        // Build clone list first, then insert from back to front so that
        // inserting at a higher index doesn't shift lower indices.
        var toInsert = _sourceEntries
            .Select(s => (InsertAfter: s.Index, Clone: CloneEntry(s.Entry)))
            .ToList();

        for (int i = toInsert.Count - 1; i >= 0; i--)
        {
            var (insertAfter, clone) = toInsert[i];
            int insertAt = Math.Clamp(insertAfter + 1, 0, _targetList.Count);
            _targetList.Insert(insertAt, clone);
            _clonedEntries.Insert(0, (insertAt, clone)); // keep ascending order
        }

        // Select the newly created clones.
        _view.Selection.ResetMsbEntrySelection();
        foreach (var (insertedIndex, clone) in _clonedEntries)
            _view.Selection.SelectedEntries.TryAdd(insertedIndex, clone);

        _view.MsbEditor.EntryView.RebuildEntryCache();

        return ActionEvent.ObjectAddedRemoved;
    }

    public override ActionEvent Undo()
    {
        if (_targetList is null)
            return ActionEvent.NoEvent;

        // Remove by reference identity from the back so earlier clones are
        // unaffected by each removal.
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

        // Restore selection to the original source entries.
        _view.Selection.ResetMsbEntrySelection();
        foreach (var (index, entry) in _sourceEntries)
            _view.Selection.SelectedEntries.TryAdd(index, entry);

        _view.MsbEditor.EntryView.RebuildEntryCache();

        return ActionEvent.ObjectAddedRemoved;
    }

    // ── Clone logic ───────────────────────────────────────────────────────────

    /// <summary>
    /// Clones <paramref name="source"/> by preferring its own <c>DeepCopy()</c>
    /// method, which every SoulsFormats entry type provides and which correctly
    /// deep-copies nested reference-type structs (gparam configs, shapes, etc.).
    /// Falls back to <c>MemberwiseClone</c> only for types that don't expose
    /// <c>DeepCopy()</c>, and logs a warning in that case so it can be caught
    /// during testing.
    ///
    /// After cloning, " - Copy" is appended to the <c>Name</c> property so the
    /// duplicate is visually distinct in the entry list.
    /// </summary>
    private static object CloneEntry(object source)
    {
        object clone;

        // Every SoulsFormats MSB entry (Part, Region, Event, Model, Route) exposes
        // a public DeepCopy() that handles nested structs correctly.  Use it.
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
            // Fallback for any future entry type that hasn't yet implemented DeepCopy().
            // MemberwiseClone is shallow, so reference-type fields will be shared —
            // flag this so it can be fixed in SoulsFormats.
            Smithbox.LogError<MsbEditor>($"[Map Data Editor] {source.GetType().FullName} has no DeepCopy() " +
                $"method; falling back to MemberwiseClone. Nested reference fields will be shared.");

            var memberwiseClone = typeof(object).GetMethod("MemberwiseClone",
                BindingFlags.Instance | BindingFlags.NonPublic);
            clone = memberwiseClone!.Invoke(source, null);
        }

        // Patch the Name so the duplicate is identifiable in the list.
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