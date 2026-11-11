using StudioCore.Editors.Common;

namespace StudioCore.Editors.EmevdEditor;

/// <summary>An EMEVD Editor action that can tell when it would change nothing, so it's left off the undo stack</summary>
public interface IEmevdEdit
{
    bool HasEffect { get; }
}

/// <summary>Sets a typed field (e.g. an event's rest behaviour) through a getter and setter</summary>
public class EmevdValueChange<T> : EditorAction, IEmevdEdit
{
    private readonly EmevdFileWrapper File;
    private readonly Action<T> Set;
    private readonly T OldValue;
    private readonly T NewValue;

    public EmevdValueChange(EmevdFileWrapper file, Func<T> get, Action<T> set, T value)
    {
        File = file;
        Set = set;
        OldValue = get();
        NewValue = value;
    }

    public bool HasEffect => !EqualityComparer<T>.Default.Equals(OldValue, NewValue);

    public override ActionEvent Execute()
    {
        Set(NewValue);
        File.MarkChanged();
        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Set(OldValue);
        File.MarkUndone();
        return ActionEvent.NoEvent;
    }
}
