using SoulsFormats;
using StudioCore.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor.LightmapAtlasEditor;

public class LightmapAtlasChangeAtlasID : ViewportAction
{
    private LightmapAtlasInfo ParentEntry;
    private BTAB.Entry Entry;
    private int OldValue;
    private int NewValue;
    private bool ChangeModifiedState;

    public LightmapAtlasChangeAtlasID(LightmapAtlasInfo selectedParententry, BTAB.Entry selectedEntry, int newValue, int oldValue)
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

    public override ActionEvent Execute(bool isRedo = false)
    {
        if (ChangeModifiedState)
            ParentEntry.IsModified = true;

        Entry.AtlasID = NewValue;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        if (ChangeModifiedState)
            ParentEntry.IsModified = false;

        Entry.AtlasID = OldValue;

        return ActionEvent.NoEvent;
    }
}

public class LightmapAtlasChangePartName : ViewportAction
{
    private LightmapAtlasInfo ParentEntry;
    private BTAB.Entry Entry;
    private string OldValue;
    private string NewValue;
    private bool ChangeModifiedState;

    public LightmapAtlasChangePartName(LightmapAtlasInfo selectedParententry, BTAB.Entry selectedEntry, string newValue, string oldValue)
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

    public override ActionEvent Execute(bool isRedo = false)
    {
        if (ChangeModifiedState)
            ParentEntry.IsModified = true;

        Entry.PartName = NewValue;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        if (ChangeModifiedState)
            ParentEntry.IsModified = false;

        Entry.PartName = OldValue;

        return ActionEvent.NoEvent;
    }
}

public class LightmapAtlasChangeMaterialName : ViewportAction
{
    private LightmapAtlasInfo ParentEntry;
    private BTAB.Entry Entry;
    private string OldValue;
    private string NewValue;
    private bool ChangeModifiedState;

    public LightmapAtlasChangeMaterialName(LightmapAtlasInfo selectedParententry, BTAB.Entry selectedEntry, string newValue, string oldValue)
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

    public override ActionEvent Execute(bool isRedo = false)
    {
        if (ChangeModifiedState)
            ParentEntry.IsModified = true;

        Entry.MaterialName = NewValue;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        if (ChangeModifiedState)
            ParentEntry.IsModified = false;

        Entry.MaterialName = OldValue;

        return ActionEvent.NoEvent;
    }
}

public class LightmapAtlasChangeUVOffset : ViewportAction
{
    private LightmapAtlasInfo ParentEntry;
    private BTAB.Entry Entry;
    private Vector2 OldValue;
    private Vector2 NewValue;
    private bool ChangeModifiedState;

    public LightmapAtlasChangeUVOffset(LightmapAtlasInfo selectedParententry, BTAB.Entry selectedEntry, Vector2 newValue, Vector2 oldValue)
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

    public override ActionEvent Execute(bool isRedo = false)
    {
        if (ChangeModifiedState)
            ParentEntry.IsModified = true;

        Entry.UVOffset = NewValue;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        if (ChangeModifiedState)
            ParentEntry.IsModified = false;

        Entry.UVOffset = OldValue;

        return ActionEvent.NoEvent;
    }
}

public class LightmapAtlasChangeUVScale : ViewportAction
{
    private LightmapAtlasInfo ParentEntry;
    private BTAB.Entry Entry;
    private Vector2 OldValue;
    private Vector2 NewValue;
    private bool ChangeModifiedState;

    public LightmapAtlasChangeUVScale(LightmapAtlasInfo selectedParententry, BTAB.Entry selectedEntry, Vector2 newValue, Vector2 oldValue)
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

    public override ActionEvent Execute(bool isRedo = false)
    {
        if (ChangeModifiedState)
            ParentEntry.IsModified = true;

        Entry.UVScale = NewValue;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        if (ChangeModifiedState)
            ParentEntry.IsModified = false;

        Entry.UVScale = OldValue;

        return ActionEvent.NoEvent;
    }
}
