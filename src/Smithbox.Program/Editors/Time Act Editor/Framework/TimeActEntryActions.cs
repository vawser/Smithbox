using SoulsFormats;
using StudioCore.Editors.Common;
using System;
using StudioCore.Editors.Viewport;

namespace StudioCore.Editors.TimeActEditor;

/// <summary>
/// Adds a time act to a source, or removes one when it is told to delete instead.
/// The binder files are only added or removed when the source is saved.
/// </summary>
public class TimeActAddEntryAction : TimeActAction
{
    private readonly TimeActEditorView View;
    private readonly TimeActContainerWrapper Source;
    private readonly TimeActEntry Entry;

    private readonly bool IsDelete;

    private int Index;

    public TimeActAddEntryAction(TimeActEditorView view, TimeActContainerWrapper source, TimeActEntry entry,
        bool isDelete = false)
    {
        View = view;
        Source = source;
        Entry = entry;
        IsDelete = isDelete;

        Index = isDelete ? source.TimeActs.IndexOf(entry) : -1;
    }

    public override TimeActContainerWrapper TargetSource => Source;
    public override TimeActEntry TargetEntry => Entry;

    public override string Describe() =>
        LOC.Get(IsDelete ? "TAE_History_Delete_TimeAct" : "TAE_History_Add_TimeAct", Entry.Name);

    public override ActionEvent Execute(bool isRedo = false)
    {
        if (IsDelete)
        {
            Remove();
        }
        else
        {
            Insert();
        }

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        if (IsDelete)
        {
            Insert();
        }
        else
        {
            Remove();
        }

        return ActionEvent.NoEvent;
    }

    private void Insert()
    {
        Index = Source.InsertTimeAct(Entry, Index);

        if (!IsDelete && View.Selection.SelectedTimeActContainerWrapper == Source)
        {
            View.SelectionWindow.SelectTimeAct(Entry);
        }
    }

    private void Remove()
    {
        Index = Source.TimeActs.IndexOf(Entry);

        Source.RemoveTimeAct(Entry);

        if (View.Selection.SelectedTimeAct == Entry)
        {
            View.SelectionWindow.ClearSelection();
        }
    }
}

/// <summary>
/// Renames a time act, which moves it to the category its name stands for
/// </summary>
public class TimeActRenameEntryAction : TimeActAction
{
    private readonly TimeActEditorView View;
    private readonly TimeActContainerWrapper Source;
    private readonly TimeActEntry Entry;

    private readonly string OldName;
    private readonly int OldBindIndex;
    private readonly string NewName;
    private readonly int NewBindIndex;

    public TimeActRenameEntryAction(TimeActEditorView view, TimeActContainerWrapper source, TimeActEntry entry,
        string newName, int newBindIndex)
    {
        View = view;
        Source = source;
        Entry = entry;

        OldName = entry.Name;
        OldBindIndex = entry.BindIndex;
        NewName = newName;
        NewBindIndex = newBindIndex;
    }

    public override TimeActContainerWrapper TargetSource => Source;
    public override TimeActEntry TargetEntry => Entry;

    public override string Describe() => LOC.Get("TAE_History_Rename_TimeAct", OldName, NewName);

    public override ActionEvent Execute(bool isRedo = false)
    {
        Apply(NewName, NewBindIndex);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Apply(OldName, OldBindIndex);

        return ActionEvent.NoEvent;
    }

    private void Apply(string name, int bindIndex)
    {
        Entry.Rename(name, bindIndex);

        Source.ResortTimeAct(Entry);

        // The names of the animations come from the category, so the graph and playback are refreshed
        if (View.Selection.SelectedTimeAct == Entry)
        {
            View.Playback.OnAnimationChanged();
        }
    }
}

/// <summary>
/// Replaces a time act with another, such as one imported from a file
/// </summary>
public class TimeActReplaceEntryAction : TimeActAction
{
    private readonly TimeActEditorView View;
    private readonly TimeActContainerWrapper Source;
    private readonly TimeActEntry OldEntry;
    private readonly TimeActEntry NewEntry;

    public TimeActReplaceEntryAction(TimeActEditorView view, TimeActContainerWrapper source,
        TimeActEntry oldEntry, TimeActEntry newEntry)
    {
        View = view;
        Source = source;
        OldEntry = oldEntry;
        NewEntry = newEntry;
    }

    public override TimeActContainerWrapper TargetSource => Source;

    // Whichever of the two is in the source now
    public override TimeActEntry TargetEntry => Source.TimeActs.Contains(NewEntry) ? NewEntry : OldEntry;

    public override string Describe() => LOC.Get("TAE_History_Replace_TimeAct", OldEntry.Name);

    public override ActionEvent Execute(bool isRedo = false)
    {
        Swap(OldEntry, NewEntry);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Swap(NewEntry, OldEntry);

        return ActionEvent.NoEvent;
    }

    private void Swap(TimeActEntry from, TimeActEntry to)
    {
        Source.ReplaceTimeAct(from, to);

        // Written again, as the file now holds the other time act
        to.IsDirty = true;

        if (View.Selection.SelectedTimeAct == from)
        {
            View.SelectionWindow.SelectTimeAct(to);
        }
    }
}

/// <summary>
/// Changes a property of the time act or one of its animations, such as an ID or skeleton name.
/// The callback runs after each change, for properties that other things depend on.
/// </summary>
public class TimeActSetPropertyAction<T> : TimeActAction
{
    private readonly TimeActEntry Entry;
    private readonly Action<T> Set;
    private readonly T OldValue;
    private readonly T NewValue;
    private readonly Action OnChanged;

    // The name of the property, and the animation it belongs to, if it isn't a property of the time act itself
    private readonly string PropertyName;
    private readonly TAE.Animation Animation;

    public TimeActSetPropertyAction(TimeActEntry entry, Action<T> set, T oldValue, T newValue, Action onChanged = null,
        string propertyName = null, TAE.Animation animation = null)
    {
        Entry = entry;
        Set = set;
        OldValue = oldValue;
        NewValue = newValue;
        OnChanged = onChanged;
        PropertyName = propertyName;
        Animation = animation;
    }

    public override TimeActEntry TargetEntry => Entry;
    public override TAE.Animation TargetAnimation => Animation;

    public override string Describe() => PropertyName != null
        ? LOC.Get("TAE_History_Set_Property", PropertyName, FormatValue(NewValue))
        : LOC.Get("TAE_History_Set_Property_Unnamed");

    public override ActionEvent Execute(bool isRedo = false)
    {
        Set(NewValue);
        Entry.IsDirty = true;
        OnChanged?.Invoke();

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Set(OldValue);
        Entry.IsDirty = true;
        OnChanged?.Invoke();

        return ActionEvent.NoEvent;
    }
}
