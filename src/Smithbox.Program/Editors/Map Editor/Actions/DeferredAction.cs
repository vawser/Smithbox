using StudioCore.Editors.Common;
using System;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// An action built when it first executes, for a step of a compound action that depends on what an earlier step
/// made (e.g. moving the clones a duplicate created). Redo and undo reuse the action built the first time.
/// </summary>
public class DeferredAction : ViewportAction
{
    private readonly Func<ViewportAction> Factory;
    private ViewportAction Inner;
    private bool Built;

    public DeferredAction(Func<ViewportAction> factory)
    {
        Factory = factory;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        if (!Built)
        {
            Inner = Factory();
            Built = true;
        }

        return Inner?.Execute(isRedo) ?? ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        return Inner?.Undo() ?? ActionEvent.NoEvent;
    }
}
