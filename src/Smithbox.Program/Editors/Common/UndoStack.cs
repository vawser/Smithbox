using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.Common;

/// <summary>
///     The undo and redo history shared by the editors' action managers (<see cref="ActionManager"/> for
///     <see cref="EditorAction"/>, <see cref="ViewportActionManager"/> for <see cref="ViewportAction"/>). It holds
///     the two stacks and the order things happen in; how an action is run, and anything a manager does on top
///     (refusing actions, clearing caches), is given to it.
/// </summary>
/// <remarks>
///     Each change runs in this order, which handlers of <see cref="ActionPerformed"/> may rely on:
///     run the action → notify the <see cref="IActionEventHandler"/>s → <see cref="Version"/> + 1 →
///     <see cref="ActionPerformed"/> → move the action between the stacks → <see cref="AfterChange"/>.
///     So during <see cref="ActionPerformed"/>, the action isn't on its new stack yet.
/// </remarks>
public sealed class UndoStack<TAction> where TAction : class
{
    private readonly Stack<TAction> _undo = new();
    private readonly Stack<TAction> _redo = new();
    private readonly List<IActionEventHandler> _eventHandlers = new();

    private readonly Func<TAction, ActionEvent> _execute;
    private readonly Func<TAction, ActionEvent> _undoAction;
    private readonly Func<TAction, ActionEvent> _redoAction;

    /// <param name="execute">Runs an action for the first time</param>
    /// <param name="undo">Reverts an action</param>
    /// <param name="redo">Runs an undone action again</param>
    public UndoStack(Func<TAction, ActionEvent> execute, Func<TAction, ActionEvent> undo, Func<TAction, ActionEvent> redo)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _undoAction = undo ?? throw new ArgumentNullException(nameof(undo));
        _redoAction = redo ?? throw new ArgumentNullException(nameof(redo));
    }

    /// <summary>Raised whenever an action is executed, undone or redone, regardless of the event it returns</summary>
    public event Action ActionPerformed;

    /// <summary>
    ///     Checked before an action is executed, undone or redone: true refuses it, leaving the history as it was.
    /// </summary>
    public Func<TAction, bool> IsRefused;

    /// <summary>Run after every change to the history made by running an action, or by <see cref="PushDone"/></summary>
    public Action AfterChange;

    /// <summary>Goes up whenever the history changes, so lists built from it know to rebuild</summary>
    public int Version { get; private set; }

    public int UndoCount => _undo.Count;

    public int RedoCount => _redo.Count;

    public bool CanUndo => _undo.Count > 0;

    public bool CanRedo => _redo.Count > 0;

    public void AddEventHandler(IActionEventHandler handler)
    {
        _eventHandlers.Add(handler);
    }

    /// <summary>Executes the action and records it for undo, clearing the redo steps</summary>
    /// <returns>False if <see cref="IsRefused"/> refused it</returns>
    public bool Execute(TAction action)
    {
        if (IsRefused?.Invoke(action) == true)
            return false;

        Changed(_execute(action));
        _undo.Push(action);
        _redo.Clear();
        AfterChange?.Invoke();

        return true;
    }

    /// <summary>
    ///     Records an action that has already been done (e.g. a compound of steps run elsewhere) as an undo step,
    ///     without running it, clearing the redo steps
    /// </summary>
    public void PushDone(TAction action)
    {
        _undo.Push(action);
        Version++;
        ActionPerformed?.Invoke();
        _redo.Clear();
        AfterChange?.Invoke();
    }

    /// <returns>False if there was nothing to undo, or the step was refused</returns>
    public bool Undo()
    {
        if (_undo.Count == 0 || IsRefused?.Invoke(_undo.Peek()) == true)
            return false;

        var action = _undo.Pop();
        Changed(_undoAction(action));
        _redo.Push(action);
        AfterChange?.Invoke();

        return true;
    }

    /// <returns>False if there was nothing to redo, or the step was refused</returns>
    public bool Redo()
    {
        if (_redo.Count == 0 || IsRefused?.Invoke(_redo.Peek()) == true)
            return false;

        var action = _redo.Pop();
        Changed(_redoAction(action));
        _undo.Push(action);
        AfterChange?.Invoke();

        return true;
    }

    /// <summary>Undoes every step, stopping at the first one refused</summary>
    public void UndoAll()
    {
        while (Undo())
        {
        }
    }

    /// <summary>
    ///     Undoes the steps after the given one, and then it, so the history is back to before it was made. Stops early
    ///     if a step is refused. Does nothing if the action isn't an undo step.
    /// </summary>
    public void UndoTo(TAction target)
    {
        if (!_undo.Contains(target))
            return;

        while (_undo.Count > 0)
        {
            var action = _undo.Peek();

            if (!Undo() || ReferenceEquals(action, target))
                break;
        }
    }

    /// <summary>
    ///     Redoes the steps up to and including the given one. Stops early if a step is refused. Does nothing if the
    ///     action isn't a redo step.
    /// </summary>
    public void RedoTo(TAction target)
    {
        if (!_redo.Contains(target))
            return;

        while (_redo.Count > 0)
        {
            var action = _redo.Peek();

            if (!Redo() || ReferenceEquals(action, target))
                break;
        }
    }

    public TAction PeekUndo() => _undo.Count == 0 ? null : _undo.Peek();

    public TAction PeekRedo() => _redo.Count == 0 ? null : _redo.Peek();

    /// <summary>The actions that can be undone, the most recent first</summary>
    public IReadOnlyList<TAction> UndoSteps() => _undo.ToList();

    /// <summary>The actions that can be redone, the next to be redone first</summary>
    public IReadOnlyList<TAction> RedoSteps() => _redo.ToList();

    public void Clear()
    {
        _undo.Clear();
        _redo.Clear();
        Version++;
    }

    /// <summary>
    ///     Drops the steps that don't satisfy <paramref name="keep"/>, from both stacks, keeping the rest in order.
    ///     Nothing is run, so <see cref="ActionPerformed"/> isn't raised.
    /// </summary>
    public void Retain(Func<TAction, bool> keep)
    {
        Refill(_undo, _undo.Where(keep).Reverse().ToList());
        Refill(_redo, _redo.Where(keep).Reverse().ToList());
        Version++;
    }

    /// <summary>
    ///     Moves both stacks out, leaving the history empty. Each list is in push order: the first undo step is the
    ///     oldest, and the last redo step is the next to be redone.
    /// </summary>
    public (IReadOnlyList<TAction> Undo, IReadOnlyList<TAction> Redo) Take()
    {
        var taken = ((IReadOnlyList<TAction>)_undo.Reverse().ToList(), (IReadOnlyList<TAction>)_redo.Reverse().ToList());

        _undo.Clear();
        _redo.Clear();
        Version++;

        return taken;
    }

    /// <summary>Replaces both stacks with ones from <see cref="Take"/>. Nothing is run.</summary>
    public void Restore(IReadOnlyList<TAction> undo, IReadOnlyList<TAction> redo)
    {
        Refill(_undo, undo ?? Array.Empty<TAction>());
        Refill(_redo, redo ?? Array.Empty<TAction>());
        Version++;
    }

    private void Changed(ActionEvent evt)
    {
        if (evt != ActionEvent.NoEvent)
        {
            foreach (var handler in _eventHandlers)
                handler.OnActionEvent(evt);
        }

        Version++;
        ActionPerformed?.Invoke();
    }

    /// <param name="pushOrder">Bottom of the stack first</param>
    private static void Refill(Stack<TAction> stack, IReadOnlyList<TAction> pushOrder)
    {
        stack.Clear();

        foreach (var action in pushOrder)
            stack.Push(action);
    }
}
