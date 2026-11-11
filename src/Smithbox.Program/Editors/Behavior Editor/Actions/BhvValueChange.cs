using StudioCore.Editors.Common;

namespace StudioCore.Editors.BehaviorEditor;

/// <summary>
/// Sets one typed record field (state ID, transition target, condition byte...). The getter and setter close over
/// the record object, not its index, so the action stays valid when later structure edits move the record.
/// </summary>
public class BhvValueChange<T> : EditorAction
{
    private readonly BhvFileWrapper File;
    private readonly Action<T> Set;
    private readonly T OldValue;
    private readonly T NewValue;

    public BhvValueChange(BhvFileWrapper file, Func<T> get, Action<T> set, T newValue)
    {
        File = file;
        Set = set;
        OldValue = get();
        NewValue = newValue;
    }

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
