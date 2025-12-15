using StudioCore.Editors.Common;
using System.Collections.Generic;

namespace StudioCore.Editors.GparamEditor;

public class GparamBatchChangeAction : EditorAction
{
    private readonly List<GparamValueChangeAction> ChangeActions = new();

    public GparamBatchChangeAction(List<GparamValueChangeAction> actions)
    {
        ChangeActions = actions;
    }

    public override ActionEvent Execute()
    {
        foreach (GparamValueChangeAction action in ChangeActions)
        {
            action.Execute();
        }

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        foreach (GparamValueChangeAction action in ChangeActions)
        {
            action.Undo();
        }

        return ActionEvent.NoEvent;
    }
}