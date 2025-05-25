using StudioCore.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    public void SetPostExecutionAction(Action<bool> action)
    {
        PostExecutionAction = action;
    }

    public override ActionEvent Execute()
    {
        var evt = ActionEvent.NoEvent;
        for (var i = 0; i < Actions.Count; i++)
        {
            EditorAction act = Actions[i];
            if (act != null)
            {
                evt |= act.Execute();
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
        for (var i = Actions.Count - 1; i >= 0; i--)
        {
            EditorAction act = Actions[i];
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

