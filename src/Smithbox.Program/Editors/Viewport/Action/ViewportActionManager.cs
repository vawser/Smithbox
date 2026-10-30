using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.Common;

/// <summary>
///     Manages undo and redo for an editor context
/// </summary>
public class ViewportActionManager
{
    private readonly List<IActionEventHandler> _eventHandlers = new();
    private readonly Stack<ViewportAction> RedoStack = new();

    private readonly Stack<ViewportAction> UndoStack = new();

    /// <summary>
    /// Raised whenever an action is executed, undone or redone, regardless of the event it returns
    /// </summary>
    public event Action ActionPerformed;

    public void AddEventHandler(IActionEventHandler handler)
    {
        _eventHandlers.Add(handler);
    }

    private void NotifyHandlers(ActionEvent evt)
    {
        if (evt == ActionEvent.NoEvent)
        {
            return;
        }

        foreach (IActionEventHandler handler in _eventHandlers)
        {
            handler.OnActionEvent(evt);
        }
    }

    public void ExecuteAction(ViewportAction a)
    {
        NotifyHandlers(a.Execute());
        ActionPerformed?.Invoke();
        UndoStack.Push(a);
        RedoStack.Clear();
    }

    public ViewportAction PeekUndoAction()
    {
        if (UndoStack.Count() == 0)
        {
            return null;
        }

        return UndoStack.Peek();
    }

    public void UndoAction()
    {
        if (UndoStack.Count() == 0)
        {
            return;
        }

        ViewportAction a = UndoStack.Pop();
        NotifyHandlers(a.Undo());
        ActionPerformed?.Invoke();
        RedoStack.Push(a);
    }

    public void UndoAllAction()
    {
        if (UndoStack.Count() == 0)
        {
            return;
        }

        while (UndoStack.Count() > 0)
        {
            ViewportAction a = UndoStack.Pop();
            NotifyHandlers(a.Undo());
            ActionPerformed?.Invoke();
            RedoStack.Push(a);
        }
    }

    public void RedoAction()
    {
        if (RedoStack.Count() == 0)
        {
            return;
        }

        ViewportAction a = RedoStack.Pop();
        NotifyHandlers(a.Execute(true));
        ActionPerformed?.Invoke();
        UndoStack.Push(a);
    }

    public bool CanUndo()
    {
        return UndoStack.Count() > 0;
    }

    public bool CanRedo()
    {
        return RedoStack.Count() > 0;
    }

    public void Clear()
    {
        UndoStack.Clear();
        RedoStack.Clear();
    }
}

