using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.Common;

/// <summary>
///     Manages undo and redo for an editor context
/// </summary>
public class ActionManager
{
    private readonly List<IActionEventHandler> _eventHandlers = new();
    private readonly Stack<EditorAction> RedoStack = new();

    private readonly Stack<EditorAction> UndoStack = new();

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

    public void ExecuteAction(EditorAction a)
    {
        NotifyHandlers(a.Execute());
        ActionPerformed?.Invoke();
        UndoStack.Push(a);
        RedoStack.Clear();
        CacheBank.ClearCaches();
    }

    public void PushSubManager(ActionManager child)
    {
        List<EditorAction> childList = child.UndoStack.ToList();
        childList.Reverse();
        UndoStack.Push(new CompoundAction(childList));
        ActionPerformed?.Invoke();
        RedoStack.Clear();
        CacheBank.ClearCaches();
    }

    public EditorAction PeekUndoAction()
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

        EditorAction a = UndoStack.Pop();
        NotifyHandlers(a.Undo());
        ActionPerformed?.Invoke();
        RedoStack.Push(a);
        CacheBank.ClearCaches();
    }

    public void UndoAllAction()
    {
        if (UndoStack.Count() == 0)
        {
            return;
        }

        while (UndoStack.Count() > 0)
        {
            EditorAction a = UndoStack.Pop();
            NotifyHandlers(a.Undo());
            ActionPerformed?.Invoke();
            RedoStack.Push(a);
            CacheBank.ClearCaches();
        }
    }

    public void RedoAction()
    {
        if (RedoStack.Count() == 0)
        {
            return;
        }

        EditorAction a = RedoStack.Pop();
        NotifyHandlers(a.Execute());
        ActionPerformed?.Invoke();
        UndoStack.Push(a);
        CacheBank.ClearCaches();
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
