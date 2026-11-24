using StudioCore.Editors.Common;

namespace SmithboxTests;

/// <summary>
///     The undo history both action managers run on. The ordering tests pin behaviour the managers had before they
///     shared it, which ActionPerformed handlers may depend on.
/// </summary>
public class UndoStackTests
{
    private sealed class Step(string name)
    {
        public string Name { get; } = name;
        public override string ToString() => Name;
    }

    private sealed class Recorder
    {
        public readonly List<string> Log = new();
        public readonly UndoStack<Step> Stack;

        public Recorder()
        {
            Stack = new UndoStack<Step>(
                s => { Log.Add($"do {s}"); return ActionEvent.NoEvent; },
                s => { Log.Add($"undo {s}"); return ActionEvent.NoEvent; },
                s => { Log.Add($"redo {s}"); return ActionEvent.NoEvent; });
        }

        public Step Do(string name)
        {
            var step = new Step(name);
            Stack.Execute(step);
            return step;
        }
    }

    [Fact]
    public void ExecuteUndoRedo_RunTheGivenDelegates()
    {
        var r = new Recorder();
        r.Do("a");
        r.Stack.Undo();
        r.Stack.Redo();

        Assert.Equal(["do a", "undo a", "redo a"], r.Log);
        Assert.Equal(1, r.Stack.UndoCount);
        Assert.Equal(0, r.Stack.RedoCount);
    }

    [Fact]
    public void Execute_ClearsRedo()
    {
        var r = new Recorder();
        r.Do("a");
        r.Stack.Undo();
        r.Do("b");

        Assert.False(r.Stack.CanRedo);
        Assert.Equal(["b"], r.Stack.UndoSteps().Select(s => s.Name));
    }

    [Fact]
    public void ActionPerformed_IsRaisedBeforeTheActionMovesStack()
    {
        var r = new Recorder();
        var counts = new List<(int Undo, int Redo)>();
        r.Stack.ActionPerformed += () => counts.Add((r.Stack.UndoCount, r.Stack.RedoCount));

        r.Do("a");
        r.Stack.Undo();
        r.Stack.Redo();

        Assert.Equal([(0, 0), (0, 0), (0, 0)], counts);
    }

    [Fact]
    public void AfterChange_RunsOnceTheActionHasMoved()
    {
        var r = new Recorder();
        var counts = new List<int>();
        r.Stack.AfterChange = () => counts.Add(r.Stack.UndoCount);

        r.Do("a");
        r.Stack.Undo();
        r.Stack.Redo();

        Assert.Equal([1, 0, 1], counts);
    }

    [Fact]
    public void EventHandlers_AreOnlyToldOfRealEvents()
    {
        var events = new List<ActionEvent>();
        var stack = new UndoStack<Step>(_ => ActionEvent.ObjectAddedRemoved, _ => ActionEvent.NoEvent, _ => ActionEvent.NoEvent);
        stack.AddEventHandler(new Handler(events));

        stack.Execute(new Step("a"));
        stack.Undo();

        Assert.Equal([ActionEvent.ObjectAddedRemoved], events);
    }

    private sealed class Handler(List<ActionEvent> events) : IActionEventHandler
    {
        public void OnActionEvent(ActionEvent evt) => events.Add(evt);
    }

    [Fact]
    public void Refused_LeavesTheHistoryAsItWas()
    {
        var r = new Recorder();
        var a = r.Do("a");
        r.Stack.IsRefused = s => s == a;
        var versionBefore = r.Stack.Version;

        Assert.False(r.Stack.Undo());
        Assert.False(r.Stack.Execute(a));
        Assert.Equal(1, r.Stack.UndoCount);
        Assert.Equal(versionBefore, r.Stack.Version);
        Assert.Equal(["do a"], r.Log);
    }

    [Fact]
    public void UndoAll_StopsAtTheFirstRefusedStep()
    {
        var r = new Recorder();
        r.Do("a");
        var b = r.Do("b");
        r.Do("c");
        r.Stack.IsRefused = s => s == b;

        r.Stack.UndoAll();

        Assert.Equal(["b", "a"], r.Stack.UndoSteps().Select(s => s.Name));
    }

    [Fact]
    public void UndoTo_UndoesTheTargetAndEverythingAfter()
    {
        var r = new Recorder();
        r.Do("a");
        var b = r.Do("b");
        r.Do("c");

        r.Stack.UndoTo(b);

        Assert.Equal(["a"], r.Stack.UndoSteps().Select(s => s.Name));
        Assert.Equal(["b", "c"], r.Stack.RedoSteps().Select(s => s.Name));
    }

    [Fact]
    public void RedoTo_RedoesUpToAndIncludingTheTarget()
    {
        var r = new Recorder();
        r.Do("a");
        var b = r.Do("b");
        r.Do("c");
        r.Stack.UndoAll();

        r.Stack.RedoTo(b);

        Assert.Equal(["b", "a"], r.Stack.UndoSteps().Select(s => s.Name));
        Assert.Equal(["c"], r.Stack.RedoSteps().Select(s => s.Name));
    }

    [Fact]
    public void UndoTo_IgnoresAStepNotInTheHistory()
    {
        var r = new Recorder();
        r.Do("a");

        r.Stack.UndoTo(new Step("elsewhere"));

        Assert.Equal(1, r.Stack.UndoCount);
    }

    [Fact]
    public void UndoTo_StopsAtARefusedStep()
    {
        var r = new Recorder();
        var a = r.Do("a");
        var b = r.Do("b");
        r.Do("c");
        r.Stack.IsRefused = s => s == b;

        // Without the stop this would spin on the refused step
        r.Stack.UndoTo(a);

        Assert.Equal(["b", "a"], r.Stack.UndoSteps().Select(s => s.Name));
    }

    [Fact]
    public void PushDone_RecordsWithoutRunning_AndClearsRedo()
    {
        var r = new Recorder();
        r.Do("a");
        r.Stack.Undo();
        r.Log.Clear();

        r.Stack.PushDone(new Step("done"));

        Assert.Empty(r.Log);
        Assert.False(r.Stack.CanRedo);
        Assert.Equal("done", r.Stack.PeekUndo()!.Name);
    }

    [Fact]
    public void Retain_KeepsOrderInBothStacks()
    {
        var r = new Recorder();
        foreach (var n in new[] { "a1", "b1", "a2", "b2", "a3" })
            r.Do(n);
        r.Stack.Undo();

        r.Stack.Retain(s => s.Name.StartsWith('a'));

        Assert.Equal(["a2", "a1"], r.Stack.UndoSteps().Select(s => s.Name));
        Assert.Equal(["a3"], r.Stack.RedoSteps().Select(s => s.Name));
    }

    [Fact]
    public void TakeAndRestore_RoundTrip()
    {
        var r = new Recorder();
        r.Do("a");
        r.Do("b");
        r.Do("c");
        r.Stack.Undo();

        var (undo, redo) = r.Stack.Take();

        Assert.Equal(0, r.Stack.UndoCount + r.Stack.RedoCount);
        Assert.Equal(["a", "b"], undo.Select(s => s.Name));
        Assert.Equal(["c"], redo.Select(s => s.Name));

        r.Stack.Restore(undo, redo);

        Assert.Equal(["b", "a"], r.Stack.UndoSteps().Select(s => s.Name));
        Assert.Equal(["c"], r.Stack.RedoSteps().Select(s => s.Name));
    }

    [Fact]
    public void Version_GoesUpOnEveryChange()
    {
        var r = new Recorder();
        var v = r.Stack.Version;

        r.Do("a");
        Assert.True(r.Stack.Version > v);

        v = r.Stack.Version;
        r.Stack.Clear();
        Assert.True(r.Stack.Version > v);
    }
}

/// <summary>The scope tracking ViewportActionManager adds on top of the shared history</summary>
public class ViewportActionManagerTests
{
    private sealed class ScopedAction(string scope) : ViewportAction
    {
        public override ActionEvent Execute(bool isRedo = false)
        {
            ViewportActionManager.RecordScope(scope);
            return ActionEvent.NoEvent;
        }

        public override ActionEvent Undo() => ActionEvent.NoEvent;
    }

    private sealed class UnscopedAction : ViewportAction
    {
        public override ActionEvent Execute(bool isRedo = false) => ActionEvent.NoEvent;
        public override ActionEvent Undo() => ActionEvent.NoEvent;
    }

    [Fact]
    public void Execute_RecordsTheScopesTheActionReports()
    {
        var manager = new ViewportActionManager();
        var action = new ScopedAction("m10_00_00_00");

        manager.ExecuteAction(action);

        Assert.Contains("m10_00_00_00", action.TouchedScopes);
    }

    [Fact]
    public void RemoveActionsTouching_DropsThatScopeAndUnscopedSteps()
    {
        var manager = new ViewportActionManager();
        var keep = new ScopedAction("m11");
        manager.ExecuteAction(new ScopedAction("m10"));
        manager.ExecuteAction(keep);
        manager.ExecuteAction(new UnscopedAction());

        Assert.Equal(2, manager.CountActionsTouching("m10"));

        manager.RemoveActionsTouching("m10");

        Assert.Equal([keep], manager.GetUndoActions());
    }

    [Fact]
    public void KeepOnlyActionsTouching_KeepsJustThatScope()
    {
        var manager = new ViewportActionManager();
        var keep = new ScopedAction("m10");
        manager.ExecuteAction(keep);
        manager.ExecuteAction(new ScopedAction("m11"));
        manager.ExecuteAction(new UnscopedAction());

        manager.KeepOnlyActionsTouching("m10");

        Assert.Equal([keep], manager.GetUndoActions());
    }
}
