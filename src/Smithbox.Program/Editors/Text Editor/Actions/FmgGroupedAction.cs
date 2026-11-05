using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.TextEditor;

public class FmgGroupedAction : EditorAction
{
    private readonly List<EditorAction> Actions;

    private Action<bool> PostExecutionAction;

    public FmgGroupedAction(List<EditorAction> actions)
    {
        Actions = actions;
    }

    public bool HasActions => Actions.Any();

    public IReadOnlyList<EditorAction> GetActions() => Actions;

    public void SetPostExecutionAction(Action<bool> action)
    {
        PostExecutionAction = action;
    }

    public override ActionEvent Execute()
    {
        var evt = ActionEvent.NoEvent;

        // One diff-cache rebuild for the whole group instead of one per sub-action
        TextDifferenceManager.BeginBatch();
        try
        {
            for (var i = 0; i < Actions.Count; i++)
            {
                EditorAction act = Actions[i];
                if (act != null)
                {
                    evt |= act.Execute();
                }
            }
        }
        finally
        {
            TextDifferenceManager.EndBatch();
        }

        if (PostExecutionAction != null)
        {
            PostExecutionAction.Invoke(false);
        }

        return evt;
    }

    public override ActionEvent Undo()
    {
        var evt = ActionEvent.NoEvent;

        TextDifferenceManager.BeginBatch();
        try
        {
            for (var i = Actions.Count - 1; i >= 0; i--)
            {
                EditorAction act = Actions[i];
                if (act != null)
                {
                    evt |= act.Undo();
                }
            }
        }
        finally
        {
            TextDifferenceManager.EndBatch();
        }

        if (PostExecutionAction != null)
        {
            PostExecutionAction.Invoke(true);
        }

        return evt;
    }
}

