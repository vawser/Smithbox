using StudioCore.Editors.Common;

namespace StudioCore.Editors.HavokEditor;

/// <summary>
/// Several Havok edits applied and undone as one step (undone in reverse order).
/// Counts as a structural edit, so the behavior views rebuild their lists, and names its file,
/// so undoing it from another file jumps back to it.
/// </summary>
public class HavokCompoundAction : EditorAction, IHavokFileAction
{
    private readonly List<EditorAction> Actions;

    public HavokFileWrapper FileWrapper { get; }

    public bool ChangesStructure => true;

    public HavokCompoundAction(HavokFileWrapper fileWrapper, List<EditorAction> actions)
    {
        FileWrapper = fileWrapper;
        Actions = actions;
    }

    public bool HasActions => Actions.Count > 0;

    public override ActionEvent Execute()
    {
        foreach (var action in Actions)
            action.Execute();

        if (FileWrapper != null)
            FileWrapper.IsDirty = true;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        for (var i = Actions.Count - 1; i >= 0; i--)
            Actions[i].Undo();

        if (FileWrapper != null)
            FileWrapper.IsDirty = true;

        return ActionEvent.NoEvent;
    }
}
