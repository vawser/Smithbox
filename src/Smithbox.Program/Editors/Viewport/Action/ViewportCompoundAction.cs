using System;
using System.Collections.Generic;

namespace StudioCore.Editors.Common;

public class ViewportCompoundAction : ViewportAction
{
    private readonly List<ViewportAction> Actions;

    private Action<bool> PostExecutionAction;

    public ViewportCompoundAction(List<ViewportAction> actions)
    {
        Actions = actions;

        // Scopes the actions carry from the start (such as time act edits) belong to the whole compound,
        // as the history only sees the compound
        foreach (var act in actions)
        {
            if (act != null)
            {
                TouchedScopes.UnionWith(act.TouchedScopes);
            }
        }
    }

    public void SetPostExecutionAction(Action<bool> action)
    {
        PostExecutionAction = action;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        var evt = ActionEvent.NoEvent;
        foreach (ViewportAction act in Actions)
        {
            if (act != null)
            {
                evt |= act.Execute(isRedo);
            }
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
        foreach (ViewportAction act in Actions)
        {
            if (act != null)
            {
                evt |= act.Undo();
            }
        }

        if (PostExecutionAction != null)
        {
            PostExecutionAction.Invoke(true);
        }

        return evt;
    }
}

