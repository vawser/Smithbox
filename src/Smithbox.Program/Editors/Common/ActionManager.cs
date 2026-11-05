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

    /// <summary>
    /// Optional check run before an action is executed, undone or redone. Returns a reason to refuse it (which is
    /// logged as a warning), or null to allow it.
    /// </summary>
    public Func<EditorAction, string> ExecutionGuard;

    private bool IsBlocked(EditorAction a)
    {
        var reason = ExecutionGuard?.Invoke(a);

        if (reason == null)
        {
            return false;
        }

        Smithbox.Log(this, reason, Microsoft.Extensions.Logging.LogLevel.Warning);
        return true;
    }

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

    /// <summary>
    /// Executes the action and records it for undo. Returns false if the <see cref="ExecutionGuard"/> refused it.
    /// </summary>
    public bool ExecuteAction(EditorAction a)
    {
        if (IsBlocked(a))
        {
            return false;
        }

        NotifyHandlers(a.Execute());
        ActionPerformed?.Invoke();
        UndoStack.Push(a);
        RedoStack.Clear();
        CacheBank.ClearCaches();

        return true;
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

    public EditorAction PeekRedoAction()
    {
        if (RedoStack.Count == 0)
        {
            return null;
        }

        return RedoStack.Peek();
    }

    public int UndoCount => UndoStack.Count;

    /// <summary>
    /// Every action that can still be undone or redone
    /// </summary>
    public IEnumerable<EditorAction> GetHistory()
    {
        return UndoStack.Concat(RedoStack);
    }

    public void UndoAction()
    {
        if (UndoStack.Count() == 0)
        {
            return;
        }

        if (IsBlocked(UndoStack.Peek()))
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
            if (IsBlocked(UndoStack.Peek()))
            {
                return;
            }

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

        if (IsBlocked(RedoStack.Peek()))
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
