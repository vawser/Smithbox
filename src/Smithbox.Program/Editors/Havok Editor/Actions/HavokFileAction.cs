using StudioCore.Editors.Common;

namespace StudioCore.Editors.HavokEditor;

/// <summary>
/// An action that edits the contents of one Havok file.
/// The bank keeps the files these actions refer to loaded, so undo and redo always apply to the displayed objects.
/// </summary>
public interface IHavokFileAction
{
    HavokFileWrapper FileWrapper { get; }

    /// <summary>
    /// Whether the action can add or remove objects from the file's object graph,
    /// so views that list objects from the graph (the behavior tabs) need to rebuild their lists
    /// </summary>
    bool ChangesStructure { get; }
}

/// <summary>
/// An action made of an apply and a revert step, used for edits that change several objects at once
/// (such as replacing a collision shape).
/// </summary>
public class HavokDelegateAction : EditorAction, IHavokFileAction
{
    public HavokFileWrapper FileWrapper { get; }

    public bool ChangesStructure => true;

    private readonly Action ApplyStep;
    private readonly Action RevertStep;

    public HavokDelegateAction(HavokFileWrapper fileWrapper, Action apply, Action revert)
    {
        FileWrapper = fileWrapper;
        ApplyStep = apply;
        RevertStep = revert;
    }

    public override ActionEvent Execute()
    {
        ApplyStep();

        if (FileWrapper != null)
            FileWrapper.IsDirty = true;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        RevertStep();

        if (FileWrapper != null)
            FileWrapper.IsDirty = true;

        return ActionEvent.NoEvent;
    }
}
