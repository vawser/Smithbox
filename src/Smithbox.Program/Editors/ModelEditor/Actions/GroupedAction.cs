using StudioCore.Editors.MapEditor.Actions.Viewport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor.Actions;

/// <summary>
/// Execute a group of actions
/// </summary>
public class GroupedAction : ViewportAction
{
    private readonly List<ViewportAction> Actions;

    private Action<bool> PostExecutionAction;

    public GroupedAction(List<ViewportAction> actions)
    {
        Actions = actions;
    }

    public bool HasActions => Actions.Any();

    public void SetPostExecutionAction(Action<bool> action)
    {
        PostExecutionAction = action;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        var evt = ActionEvent.NoEvent;
        for (var i = 0; i < Actions.Count; i++)
        {
            ViewportAction act = Actions[i];
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
            ViewportAction act = Actions[i];
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
    public override string GetEditMessage()
    {
        return "";
    }
}
