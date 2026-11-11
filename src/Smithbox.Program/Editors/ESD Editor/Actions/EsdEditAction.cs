using StudioCore.Editors.Common;

namespace StudioCore.Editors.EsdEditor;

/// <summary>
/// An undoable ESD edit: one of Framework/EsdEdits' ops (gated by EsdSurvey editcheck: done, written and read back as
/// edited; undone, the file writes byte-identical again), marking its container changed.
/// </summary>
public class EsdEditAction : EditorAction
{
    private readonly EsdContainer Container;
    private readonly IEsdOp Op;

    /// <summary>What the edit was, for the log</summary>
    public string Description { get; }

    public EsdEditAction(EsdContainer container, IEsdOp op, string description)
    {
        Container = container;
        Op = op;
        Description = description;
    }

    public override ActionEvent Execute()
    {
        Op.Do();
        Container.MarkChanged();
        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Op.Undo();
        Container.MarkUndone();
        return ActionEvent.NoEvent;
    }
}
