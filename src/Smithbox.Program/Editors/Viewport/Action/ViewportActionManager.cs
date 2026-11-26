using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.Common;

/// <summary>
///     Manages undo and redo for an editor context
/// </summary>
public class ViewportActionManager
{
    private readonly UndoStack<ViewportAction> _stack;

    public ViewportActionManager()
    {
        // Each run records, on the top-level action, the scopes the data it changes reports
        _stack = new UndoStack<ViewportAction>(
            a => Run(LastPerformed = a, () => a.Execute()),
            a => Run(LastPerformed = a, a.Undo),
            a => Run(LastPerformed = a, () => a.Execute(true)));
    }

    /// <summary>
    /// The top-level action last executed, undone or redone, for <see cref="ActionPerformed" /> handlers that react to
    /// some kinds only (the Model Editor doesn't mark its model unsaved for a variant's tint or colour shift).
    /// </summary>
    public ViewportAction LastPerformed { get; private set; }

    /// <summary>
    /// Raised whenever an action is executed, undone or redone, regardless of the event it returns
    /// </summary>
    public event Action ActionPerformed
    {
        add => _stack.ActionPerformed += value;
        remove => _stack.ActionPerformed -= value;
    }

    public void AddEventHandler(IActionEventHandler handler)
    {
        _stack.AddEventHandler(handler);
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
        _stack.Execute(a);
    }

    /// <summary>
    /// Drops the actions that changed the scope from the undo and redo history, keeping the rest in order.
    /// Actions that recorded no scope are dropped too, as nothing says what they changed.
    /// </summary>
    public void RemoveActionsTouching(string scope)
    {
        // ActionPerformed isn't raised, as no data changes; its handlers treat it as an edit
        _stack.Retain(a => !Touches(a, scope));
    }

    /// <summary>
    /// How many undo and redo steps RemoveActionsTouching would drop for the scope
    /// </summary>
    public int CountActionsTouching(string scope)
    {
        return _stack.UndoSteps().Count(a => Touches(a, scope)) + _stack.RedoSteps().Count(a => Touches(a, scope));
    }

    /// <summary>
    /// Drops every action that didn't change the scope from the undo and redo history, keeping the rest in order
    /// </summary>
    public void KeepOnlyActionsTouching(string scope)
    {
        _stack.Retain(a => a.TouchedScopes.Contains(scope));
    }

    /// <summary>Whether the action changed the scope, or recorded no scope so may have</summary>
    private static bool Touches(ViewportAction a, string scope)
    {
        return a.TouchedScopes.Count == 0 || a.TouchedScopes.Contains(scope);
    }

    public ViewportAction PeekRedoAction() => _stack.PeekRedo();

    /// <summary>
    /// The actions that can be undone, the most recent first
    /// </summary>
    public IReadOnlyList<ViewportAction> GetUndoActions() => _stack.UndoSteps();

    /// <summary>
    /// The actions that can be redone, the next to be redone first
    /// </summary>
    public IReadOnlyList<ViewportAction> GetRedoActions() => _stack.RedoSteps();

    /// <summary>
    /// Goes up whenever the history changes, so lists built from it know to rebuild
    /// </summary>
    public int Version => _stack.Version;

    public ViewportAction PeekUndoAction() => _stack.PeekUndo();

    public void UndoAction() => _stack.Undo();

    public void UndoAllAction() => _stack.UndoAll();

    public void RedoAction() => _stack.Redo();

    /// <summary>
    /// Undoes the steps after the given one, and then it, so the history is back to before it was made
    /// </summary>
    public void UndoTo(ViewportAction target) => _stack.UndoTo(target);

    /// <summary>
    /// Redoes the steps up to and including the given one
    /// </summary>
    public void RedoTo(ViewportAction target) => _stack.RedoTo(target);

    public bool CanUndo() => _stack.CanUndo;

    public bool CanRedo() => _stack.CanRedo;

    public void Clear() => _stack.Clear();

    /// <summary>
    /// Moves the undo and redo history out, leaving the manager empty, so it can be given back later with
    /// <see cref="RestoreHistory"/>. The manager object stays the same, so everything holding it keeps working.
    /// </summary>
    public ViewportActionHistory TakeHistory()
    {
        var (undo, redo) = _stack.Take();

        return new ViewportActionHistory(undo, redo);
    }

    /// <summary>
    /// Replaces the history with one taken by <see cref="TakeHistory"/>. Nothing is executed or undone.
    /// </summary>
    public void RestoreHistory(ViewportActionHistory history)
    {
        _stack.Restore(history?.Undo, history?.Redo);
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
