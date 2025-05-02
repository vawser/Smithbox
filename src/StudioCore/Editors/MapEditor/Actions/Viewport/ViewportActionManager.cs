using StudioCore.Editor;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.MapEditor.Actions.Viewport;

[Flags]
public enum ActionEvent
{
    NoEvent = 0,

    // An object was added or removed from a scene
    ObjectAddedRemoved = 1
}

/// <summary>
///     Interface for objects that may react to events caused by actions that
///     happen. Useful for invalidating caches that various editors may have.
/// </summary>
public interface IActionEventHandler
{
    public void OnActionEvent(ActionEvent evt);
}

/// <summary>
///     Manages undo and redo for an editor context
/// </summary>
public class ViewportActionManager
{
    private readonly List<IActionEventHandler> _eventHandlers = new();
    private readonly Stack<ViewportAction> RedoStack = new();

    private readonly Stack<ViewportAction> UndoStack = new();

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

