using StudioCore.Editors.Common;
using System.Collections.Generic;

namespace StudioCore.Editors.GparamEditor;

public class BatchChangeAction : EditorAction
{
    private readonly List<EditValueAction> ChangeActions = new();

    public BatchChangeAction(List<EditValueAction> actions)
    {
        ChangeActions = actions;
    }

    public override ActionEvent Execute()
    {
        foreach (EditValueAction action in ChangeActions)
        {
            action.Execute();
        }

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        foreach (EditValueAction action in ChangeActions)
        {
            action.Undo();
        }

        return ActionEvent.NoEvent;
    }
}