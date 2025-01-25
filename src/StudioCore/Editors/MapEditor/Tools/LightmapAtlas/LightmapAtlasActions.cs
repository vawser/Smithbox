using SoulsFormats;
using StudioCore.Editor;
using StudioCore.Editors.MapEditor.Actions.Viewport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor.Tools.LightmapAtlasEditor;

public class LightmapAtlasChangeAtlasID : ViewportAction
{
    private AtlasContainerInfo ParentEntry;
    private BTAB.Entry Entry;
    private int OldValue;
    private int NewValue;
    private bool ChangeModifiedState;

    public LightmapAtlasChangeAtlasID(AtlasContainerInfo selectedParententry, BTAB.Entry selectedEntry, int newValue, int oldValue)
    {
        ParentEntry = selectedParententry;
        Entry = selectedEntry;
        OldValue = oldValue;
        NewValue = newValue;

        if (!ParentEntry.IsModified)
        {
            ChangeModifiedState = true;
        }
    }

    public override Actions.Viewport.ActionEvent Execute(bool isRedo = false)
    {
        if (ChangeModifiedState)
            ParentEntry.IsModified = true;

        Entry.AtlasID = NewValue;

        return Actions.Viewport.ActionEvent.NoEvent;
    }

    public override Actions.Viewport.ActionEvent Undo()
    {
        if (ChangeModifiedState)
            ParentEntry.IsModified = false;

        Entry.AtlasID = OldValue;

        return Actions.Viewport.ActionEvent.NoEvent;
    }
}

public class LightmapAtlasChangePartName : ViewportAction
{
    private AtlasContainerInfo ParentEntry;
    private BTAB.Entry Entry;
    private string OldValue;
    private string NewValue;
    private bool ChangeModifiedState;

    public LightmapAtlasChangePartName(AtlasContainerInfo selectedParententry, BTAB.Entry selectedEntry, string newValue, string oldValue)
    {
        ParentEntry = selectedParententry;
        Entry = selectedEntry;
        OldValue = oldValue;
        NewValue = newValue;

        if (!ParentEntry.IsModified)
        {
            ChangeModifiedState = true;
        }
    }

    public override Actions.Viewport.ActionEvent Execute(bool isRedo = false)
    {
        if (ChangeModifiedState)
            ParentEntry.IsModified = true;

        Entry.PartName = NewValue;

        return Actions.Viewport.ActionEvent.NoEvent;
    }

    public override Actions.Viewport.ActionEvent Undo()
    {
        if (ChangeModifiedState)
            ParentEntry.IsModified = false;

        Entry.PartName = OldValue;

        return Actions.Viewport.ActionEvent.NoEvent;
    }
}

public class LightmapAtlasChangeMaterialName : ViewportAction
{
    private AtlasContainerInfo ParentEntry;
    private BTAB.Entry Entry;
    private string OldValue;
    private string NewValue;
    private bool ChangeModifiedState;

    public LightmapAtlasChangeMaterialName(AtlasContainerInfo selectedParententry, BTAB.Entry selectedEntry, string newValue, string oldValue)
    {
        ParentEntry = selectedParententry;
        Entry = selectedEntry;
        OldValue = oldValue;
        NewValue = newValue;

        if (!ParentEntry.IsModified)
        {
            ChangeModifiedState = true;
        }
    }

    public override Actions.Viewport.ActionEvent Execute(bool isRedo = false)
    {
        if (ChangeModifiedState)
            ParentEntry.IsModified = true;

        Entry.MaterialName = NewValue;

        return Actions.Viewport.ActionEvent.NoEvent;
    }

    public override Actions.Viewport.ActionEvent Undo()
    {
        if (ChangeModifiedState)
            ParentEntry.IsModified = false;

        Entry.MaterialName = OldValue;

        return Actions.Viewport.ActionEvent.NoEvent;
    }
}

public class LightmapAtlasChangeUVOffset : ViewportAction
{
    private AtlasContainerInfo ParentEntry;
    private BTAB.Entry Entry;
    private Vector2 OldValue;
    private Vector2 NewValue;
    private bool ChangeModifiedState;

    public LightmapAtlasChangeUVOffset(AtlasContainerInfo selectedParententry, BTAB.Entry selectedEntry, Vector2 newValue, Vector2 oldValue)
    {
        ParentEntry = selectedParententry;
        Entry = selectedEntry;
        OldValue = oldValue;
        NewValue = newValue;

        if (!ParentEntry.IsModified)
        {
            ChangeModifiedState = true;
        }
    }

    public override Actions.Viewport.ActionEvent Execute(bool isRedo = false)
    {
        if (ChangeModifiedState)
            ParentEntry.IsModified = true;

        Entry.UVOffset = NewValue;

        return Actions.Viewport.ActionEvent.NoEvent;
    }

    public override Actions.Viewport.ActionEvent Undo()
    {
        if (ChangeModifiedState)
            ParentEntry.IsModified = false;

        Entry.UVOffset = OldValue;

        return Actions.Viewport.ActionEvent.NoEvent;
    }
}

public class LightmapAtlasChangeUVScale : ViewportAction
{
    private AtlasContainerInfo ParentEntry;
    private BTAB.Entry Entry;
    private Vector2 OldValue;
    private Vector2 NewValue;
    private bool ChangeModifiedState;

    public LightmapAtlasChangeUVScale(AtlasContainerInfo selectedParententry, BTAB.Entry selectedEntry, Vector2 newValue, Vector2 oldValue)
    {
        ParentEntry = selectedParententry;
        Entry = selectedEntry;
        OldValue = oldValue;
        NewValue = newValue;

        if (!ParentEntry.IsModified)
        {
            ChangeModifiedState = true;
        }
    }

    public override Actions.Viewport.ActionEvent Execute(bool isRedo = false)
    {
        if (ChangeModifiedState)
            ParentEntry.IsModified = true;

        Entry.UVScale = NewValue;

        return Actions.Viewport.ActionEvent.NoEvent;
    }

    public override Actions.Viewport.ActionEvent Undo()
    {
        if (ChangeModifiedState)
            ParentEntry.IsModified = false;

        Entry.UVScale = OldValue;

        return Actions.Viewport.ActionEvent.NoEvent;
    }
}

/// <summary>
/// Action: BTAB.Entry add
/// </summary>
public class BtabEntryAdd : ViewportAction
{
    private BTAB.Entry NewEntry;
    private List<BTAB.Entry> EntryList;
    private int InsertionIndex;

    public BtabEntryAdd(BTAB.Entry newEntry, List<BTAB.Entry> entryList, int index)
    {
        InsertionIndex = index;
        NewEntry = newEntry;
        EntryList = entryList;
    }

    public override Actions.Viewport.ActionEvent Execute(bool isRedo = false)
    {
        EntryList.Insert(InsertionIndex, NewEntry);

        return Actions.Viewport.ActionEvent.NoEvent;
    }

    public override Actions.Viewport.ActionEvent Undo()
    {
        EntryList.RemoveAt(InsertionIndex);

        return Actions.Viewport.ActionEvent.NoEvent;
    }
}

/// <summary>
/// Action: BTAB.Entry duplicate
/// </summary>
public class BtabEntryDuplicate : ViewportAction
{
    private BTAB.Entry NewEntry;
    private List<BTAB.Entry> EntryList;
    private int InsertionIndex;

    public BtabEntryDuplicate(BTAB.Entry newEntry, List<BTAB.Entry> entryList, int index)
    {
        InsertionIndex = index;
        NewEntry = newEntry;
        EntryList = entryList;
    }

    public override Actions.Viewport.ActionEvent Execute(bool isRedo = false)
    {
        EntryList.Insert(InsertionIndex, NewEntry);

        return Actions.Viewport.ActionEvent.NoEvent;
    }

    public override Actions.Viewport.ActionEvent Undo()
    {
        EntryList.RemoveAt(InsertionIndex);

        return Actions.Viewport.ActionEvent.NoEvent;
    }
}

/// <summary>
/// Action: BTAB.Entry duplicate (multiple)
/// </summary>
public class BtabEntryMultiDuplicate : ViewportAction
{
    private List<BTAB.Entry> NewEntries;
    private List<BTAB.Entry> EntryList;
    private List<int> InsertionIndexes;

    public BtabEntryMultiDuplicate(List<BTAB.Entry> newEntries, List<BTAB.Entry> entryList, List<int> indexList)
    {
        InsertionIndexes = indexList;
        NewEntries = newEntries;
        EntryList = entryList;
    }

    public override Actions.Viewport.ActionEvent Execute(bool isRedo = false)
    {
        for (int i = 0; i < InsertionIndexes.Count; i++)
        {
            BTAB.Entry curNewEntry = NewEntries[i];
            int curIndex = InsertionIndexes[i];

            EntryList.Insert(curIndex, curNewEntry);
        }

        return Actions.Viewport.ActionEvent.NoEvent;
    }

    public override Actions.Viewport.ActionEvent Undo()
    {
        foreach (BTAB.Entry entry in NewEntries)
        {
            EntryList.Remove(entry);
        }

        return Actions.Viewport.ActionEvent.NoEvent;
    }
}

/// <summary>
/// Action: BTAB.Entry delete
/// </summary>
public class BtabEntryDelete : ViewportAction
{
    private BTAB.Entry StoredEntry;
    private List<BTAB.Entry> EntryList;
    private int RemovalIndex;

    public BtabEntryDelete(BTAB.Entry oldEntry, List<BTAB.Entry> entryList, int index)
    {
        RemovalIndex = index;
        StoredEntry = oldEntry;
        EntryList = entryList;
    }

    public override Actions.Viewport.ActionEvent Execute(bool isRedo = false)
    {
        EntryList.RemoveAt(RemovalIndex);

        return Actions.Viewport.ActionEvent.NoEvent;
    }

    public override Actions.Viewport.ActionEvent Undo()
    {
        EntryList.Insert(RemovalIndex, StoredEntry);

        return Actions.Viewport.ActionEvent.NoEvent;
    }
}

/// <summary>
/// Action: BTAB.Entry delete (multiple)
/// </summary>
public class BtabEntryMultiDelete : ViewportAction
{
    private List<BTAB.Entry> StoredEntries;
    private List<BTAB.Entry> EntryList;
    private List<int> RemovalIndices;
    private List<int> InsertIndices;

    public BtabEntryMultiDelete(List<BTAB.Entry> storedEntries, List<BTAB.Entry> entryList, List<int> removalIndices)
    {
        RemovalIndices = removalIndices;
        StoredEntries = storedEntries;
        EntryList = entryList;
    }

    public override Actions.Viewport.ActionEvent Execute(bool isRedo = false)
    {
        for (int i = RemovalIndices.Count - 1; i >= 0; i--)
        {
            int curIndex = RemovalIndices[i];
            EntryList.RemoveAt(curIndex);
        }

        return Actions.Viewport.ActionEvent.NoEvent;
    }

    public override Actions.Viewport.ActionEvent Undo()
    {
        for (int i = 0; i < RemovalIndices.Count; i++)
        {
            BTAB.Entry storedEntry = StoredEntries[i];
            int curIndex = RemovalIndices[i];

            EntryList.Insert(curIndex, storedEntry);
        }

        return Actions.Viewport.ActionEvent.NoEvent;
    }
}