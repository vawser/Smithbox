using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.Common;

/// <summary>
///     Manages undo and redo for an editor context
/// </summary>
public class ActionManager
{
    private readonly UndoStack<EditorAction> _stack;

    public ActionManager()
    {
        _stack = new UndoStack<EditorAction>(a => a.Execute(), a => a.Undo(), a => a.Execute())
        {
            IsRefused = IsBlocked,
            // Every change can make what the editors derived from the data stale
            AfterChange = CacheBank.ClearCaches
        };
    }

    /// <summary>
    /// Raised whenever an action is executed, undone or redone, regardless of the event it returns
    /// </summary>
    public event Action ActionPerformed
    {
        add => _stack.ActionPerformed += value;
        remove => _stack.ActionPerformed -= value;
    }

    /// <summary>
    /// Optional check run before an action is executed, undone or redone. Returns a reason to refuse it (which is
    /// logged as a warning), or null to allow it.
    /// </summary>
    public Func<EditorAction, string> ExecutionGuard;

    /// <summary>Goes up whenever the history changes, so lists built from it know to rebuild</summary>
    public int Version => _stack.Version;

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
        _stack.AddEventHandler(handler);
    }

    /// <summary>
    /// Executes the action and records it for undo. Returns false if the <see cref="ExecutionGuard"/> refused it.
    /// </summary>
    public bool ExecuteAction(EditorAction a)
    {
        return _stack.Execute(a);
    }

    /// <summary>
    /// Records everything the child manager did as one undo step. A child with nothing to undo adds no step.
    /// </summary>
    public void PushSubManager(ActionManager child, string displayName = null, string details = null)
    {
        if (child._stack.UndoCount == 0)
        {
            return;
        }

        // Most recent first, so reversed this is the order they were done in
        List<EditorAction> childList = child._stack.UndoSteps().ToList();
        childList.Reverse();

        _stack.PushDone(new CompoundAction(childList) { DisplayName = displayName, Details = details });
    }

    public EditorAction PeekUndoAction() => _stack.PeekUndo();

    public EditorAction PeekRedoAction() => _stack.PeekRedo();

    public int UndoCount => _stack.UndoCount;

    public int RedoCount => _stack.RedoCount;

    /// <summary>
    /// The actions that can be undone, most recent first
    /// </summary>
    public IReadOnlyList<EditorAction> GetUndoHistory() => _stack.UndoSteps();

    /// <summary>
    /// The actions that can be redone, the next one to redo first
    /// </summary>
    public IReadOnlyList<EditorAction> GetRedoHistory() => _stack.RedoSteps();

    /// <summary>
    /// Every action that can still be undone or redone: the undo steps most recent first, then the redo steps
    /// next first
    /// </summary>
    public IEnumerable<EditorAction> GetHistory() => _stack.UndoSteps().Concat(_stack.RedoSteps());

    public void UndoAction() => _stack.Undo();

    /// <summary>Undoes every step, stopping at the first one the <see cref="ExecutionGuard"/> refuses</summary>
    public void UndoAllAction() => _stack.UndoAll();

    public void RedoAction() => _stack.Redo();

    /// <summary>Undoes the steps after the given one, and then it. Stops at a step the guard refuses.</summary>
    public void UndoTo(EditorAction target) => _stack.UndoTo(target);

    /// <summary>Redoes the steps up to and including the given one. Stops at a step the guard refuses.</summary>
    public void RedoTo(EditorAction target) => _stack.RedoTo(target);

    public bool CanUndo() => _stack.CanUndo;

    public bool CanRedo() => _stack.CanRedo;

    public void Clear() => _stack.Clear();
}
