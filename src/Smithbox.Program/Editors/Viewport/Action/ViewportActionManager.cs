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

    // The top-level action being executed, undone or redone, so the data it changes can record itself
    // on it (actions nested in a compound action record on the compound one)
    [ThreadStatic]
    private static ViewportAction RunningAction;

    /// <summary>
    /// Records that the action currently running changes the scope (e.g. a map ID). Does nothing outside an action.
    /// </summary>
    public static void RecordScope(string scope)
    {
        if (scope != null)
        {
            RunningAction?.TouchedScopes.Add(scope);
        }
    }

    private static ActionEvent Run(ViewportAction a, Func<ActionEvent> step)
    {
        var previous = RunningAction;
        RunningAction ??= a;

        try
        {
            return step();
        }
        finally
        {
            RunningAction = previous;
        }
    }

    public void ExecuteAction(ViewportAction a)
    {
        NotifyHandlers(Run(a, () => a.Execute()));
        Version++;
        ActionPerformed?.Invoke();
        UndoStack.Push(a);
        RedoStack.Clear();
    }

    /// <summary>
    /// Drops the actions that changed the scope from the undo and redo history, keeping the rest in order.
    /// Actions that recorded no scope are dropped too, as nothing says what they changed.
    /// </summary>
    public void RemoveActionsTouching(string scope)
    {
        static void Filter(Stack<ViewportAction> stack, string scope)
        {
            // A stack enumerates from the top, so reversed this is bottom to top, the order to push them back in
            var kept = stack
                .Where(a => a.TouchedScopes.Count > 0 && !a.TouchedScopes.Contains(scope))
                .Reverse()
                .ToList();

            stack.Clear();

            foreach (var a in kept)
            {
                stack.Push(a);
            }
        }

        // ActionPerformed isn't raised, as no data changes; its handlers treat it as an edit
        Filter(UndoStack, scope);
        Filter(RedoStack, scope);
        Version++;
    }

    /// <summary>
    /// How many undo and redo steps RemoveActionsTouching would drop for the scope
    /// </summary>
    public int CountActionsTouching(string scope)
    {
        static bool Dropped(ViewportAction a, string scope) => a.TouchedScopes.Count == 0 || a.TouchedScopes.Contains(scope);

        return UndoStack.Count(a => Dropped(a, scope)) + RedoStack.Count(a => Dropped(a, scope));
    }

    /// <summary>
    /// Drops every action that didn't change the scope from the undo and redo history, keeping the rest in order
    /// </summary>
    public void KeepOnlyActionsTouching(string scope)
    {
        static void Filter(Stack<ViewportAction> stack, string scope)
        {
            var kept = stack
                .Where(a => a.TouchedScopes.Contains(scope))
                .Reverse()
                .ToList();

            stack.Clear();

            foreach (var a in kept)
            {
                stack.Push(a);
            }
        }

        Filter(UndoStack, scope);
        Filter(RedoStack, scope);
        Version++;
    }

    public ViewportAction PeekRedoAction()
    {
        return RedoStack.Count == 0 ? null : RedoStack.Peek();
    }

    /// <summary>
    /// The actions that can be undone, the most recent first
    /// </summary>
    public IReadOnlyList<ViewportAction> GetUndoActions()
    {
        return UndoStack.ToList();
    }

    /// <summary>
    /// The actions that can be redone, the next to be redone first
    /// </summary>
    public IReadOnlyList<ViewportAction> GetRedoActions()
    {
        return RedoStack.ToList();
    }

    /// <summary>
    /// Raised whenever the history changes, so lists built from it know to rebuild
    /// </summary>
    public int Version { get; private set; }

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
        NotifyHandlers(Run(a, a.Undo));
        Version++;
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
            NotifyHandlers(Run(a, a.Undo));
            Version++;
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
        NotifyHandlers(Run(a, () => a.Execute(true)));
        Version++;
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
        Version++;
    }

    /// <summary>
    /// Moves the undo and redo history out, leaving the manager empty, so it can be given back later with
    /// <see cref="RestoreHistory"/>. The manager object stays the same, so everything holding it keeps working.
    /// </summary>
    public ViewportActionHistory TakeHistory()
    {
        var history = new ViewportActionHistory(UndoStack.Reverse().ToList(), RedoStack.Reverse().ToList());

        UndoStack.Clear();
        RedoStack.Clear();
        Version++;

        return history;
    }

    /// <summary>
    /// Replaces the history with one taken by <see cref="TakeHistory"/>. Nothing is executed or undone.
    /// </summary>
    public void RestoreHistory(ViewportActionHistory history)
    {
        UndoStack.Clear();
        RedoStack.Clear();

        if (history != null)
        {
            foreach (var a in history.Undo)
            {
                UndoStack.Push(a);
            }

            foreach (var a in history.Redo)
            {
                RedoStack.Push(a);
            }
        }

        Version++;
    }
}

/// <summary>
/// An undo and redo history taken out of a <see cref="ViewportActionManager"/>. Each list is in push order: the
/// first undo step is the oldest, and the last redo step is the next to be redone.
/// </summary>
public sealed class ViewportActionHistory
{
    public IReadOnlyList<ViewportAction> Undo { get; }
    public IReadOnlyList<ViewportAction> Redo { get; }

    public ViewportActionHistory(IReadOnlyList<ViewportAction> undo, IReadOnlyList<ViewportAction> redo)
    {
        Undo = undo;
        Redo = redo;
    }

    public int Count => Undo.Count + Redo.Count;
}

