using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;

namespace StudioCore.Editors.EsdEditor;

/// <summary>
/// The state view's edits: each builds one of Framework/EsdEdits' ops (gated by EsdSurvey editcheck) for the selected
/// block and runs it through the undo stack. Menu items for the block context menu and the Edit menu live here too.
/// </summary>
public class EsdEditCommands
{
    private readonly EsdEditorView View;

    public EsdEditCommands(EsdEditorView view)
    {
        View = view;
    }

    private EsdSelection Selection => View.Selection;
    private EsdContainer Container => Selection.SelectedFile?.Container;
    private Dictionary<long, ESD.State> Machine => Selection.Machine;

    /// <summary>The row the selection is on, from the state view's current rows</summary>
    public EsdRow SelectedRow => View.StateView.FindSelectedRow();

    // Edits wait for the end of the frame: one made from a popup mid-draw would change the lists the rest of the frame
    // is drawing
    private readonly List<EsdEditAction> Pending = new();

    private void Run(IEsdOp op, string what)
    {
        if (Container == null || op == null)
            return;
        Pending.Add(new EsdEditAction(Container, op, what));
    }

    /// <summary>Runs the edits queued this frame, once every window is drawn</summary>
    public void Flush()
    {
        if (Pending.Count == 0)
            return;
        var edits = Pending.ToList();
        Pending.Clear();
        foreach (var e in edits)
            View.ActionManager.ExecuteAction(e);

        if (PendingSelectContainer != null)
        {
            Selection.SelectContainer(PendingSelectContainer);
            View.FileList.ScrollToSelection = true;
            PendingSelectContainer = null;
        }

        if (PendingSelect != null)
        {
            Selection.SelectFile(PendingSelect);
            View.FileList.ScrollToSelection = true;
            PendingSelect = null;
        }
    }

    // --- values ---

    public void SetEvaluator(ESD.Condition c, byte[] bytes)
    {
        if (bytes == null || bytes.AsSpan().SequenceEqual(c.Evaluator))
            return;
        Run(new EsdSetOp<byte[]>(() => c.Evaluator, v => c.Evaluator = v, bytes), "test");
    }

    public void SetArgument(ESD.CommandCall cmd, int index, byte[] bytes)
    {
        if (bytes == null || index < 0 || index >= cmd.Arguments.Count || bytes.AsSpan().SequenceEqual(cmd.Arguments[index]))
            return;
        Run(new EsdSetOp<byte[]>(() => cmd.Arguments[index], v => cmd.Arguments[index] = v, bytes), "arg");
    }

    public void SetTarget(ESD.Condition c, long? target)
    {
        if (c.TargetState == target)
            return;
        Run(new EsdSetOp<long?>(() => c.TargetState, v => c.TargetState = v, target), "target");
    }

    // --- a command's identity and args (Properties) ---

    /// <summary>The kind of value an arg type holds: float, string, or a whole number (int, bool, enum, unknown)</summary>
    private static string ValueGroup(string type) => type is "float" or "string" ? type : "int";

    /// <summary>The kind of value an arg's bytes hold when they are one literal, else null (any expression)</summary>
    private static string ValueGroup(byte[] arg)
    {
        var n = EsdExpr.Decode(arg) is { } root ? EsdExpr.Strip(root) : null;
        return n?.Kind switch
        {
            EsdExprKind.Float or EsdExprKind.Double => "float",
            EsdExprKind.String => "string",
            EsdExprKind.Int => "int",
            _ => null
        };
    }

    /// <summary>
    /// Makes a command another game command, in place (the selection stays on it). Each arg of the new command keeps
    /// the old arg at its place when it holds the same kind of value; the others get the new arg's default, and
    /// optional args are kept only while the old command had them.
    /// </summary>
    public void ChangeCommand(ESD.CommandCall cmd, EsdCallDef oldDef, EsdCallDef def)
    {
        if (def == null || cmd.CommandBank == def.Bank && cmd.CommandID == def.Id)
            return;

        var args = new List<byte[]>();
        for (var i = 0; i < def.Args.Count; i++)
        {
            var a = def.Args[i];
            if (i < cmd.Arguments.Count)
            {
                var had = oldDef != null && i < oldDef.Args.Count ? ValueGroup(oldDef.Args[i].Type) : ValueGroup(cmd.Arguments[i]);
                args.Add(had == null || had == ValueGroup(a.Type) ? EsdEdits.Copy(cmd.Arguments[i]) : EsdEdits.DefaultArg(a.Type));
            }
            else if (!a.Optional)
                args.Add(EsdEdits.DefaultArg(a.Type));
            else
                break;
        }

        Run(new EsdCompoundOp(new List<IEsdOp>
        {
            new EsdSetOp<int>(() => cmd.CommandBank, v => cmd.CommandBank = v, def.Bank),
            new EsdSetOp<int>(() => cmd.CommandID, v => cmd.CommandID = v, def.Id),
            new EsdSetOp<List<byte[]>>(() => cmd.Arguments, v => cmd.Arguments = v, args),
        }), "change command");
    }

    /// <summary>Points a call block at another machine of the file</summary>
    public void SetCallMachine(ESD.CommandCall cmd, long machine)
    {
        if (EsdLayout.KindOf(cmd) != EsdCommandKind.CallMachine || cmd.CommandID == machine)
            return;
        Run(new EsdSetOp<int>(() => cmd.CommandID, v => cmd.CommandID = v, (int)machine), "call machine");
    }

    public void AddArgument(ESD.CommandCall cmd, byte[] bytes)
    {
        Run(new EsdInsertOp<byte[]>(cmd.Arguments, cmd.Arguments.Count, bytes), "add arg");
    }

    public void RemoveArgument(ESD.CommandCall cmd, int index)
    {
        if (index < 0 || index >= cmd.Arguments.Count)
            return;
        Run(new EsdRemoveOp<byte[]>(cmd.Arguments, index), "remove arg");
    }

    /// <summary>
    /// Brings a command's arg count into its documented range: the missing required args with their defaults, or the
    /// args past the documented ones removed
    /// </summary>
    public void FixArgumentCount(ESD.CommandCall cmd, EsdCallDef def)
    {
        if (def == null)
            return;

        // One swap of the whole list: EsdInsertOp clamps its index when it is made, so a chain of inserts can't be
        // built ahead of running them
        var args = cmd.Arguments.Take(def.Args.Count).ToList();
        for (var i = args.Count; i < def.RequiredArgs; i++)
            args.Add(EsdEdits.DefaultArg(def.Args[i].Type));

        if (args.Count != cmd.Arguments.Count)
            Run(new EsdSetOp<List<byte[]>>(() => cmd.Arguments, v => cmd.Arguments = v, args), "fix args");
    }

    // --- commands ---

    private static List<ESD.CommandCall> ListOf(EsdRow row) => row.List switch
    {
        EsdCommandList.Entry => row.State.EntryCommands,
        EsdCommandList.While => row.State.WhileCommands,
        EsdCommandList.Exit => row.State.ExitCommands,
        _ => row.Condition.PassCommands
    };

    /// <summary>Inserts a command into a list of a state or condition, and selects it</summary>
    public void InsertCommand(ESD.State state, long stateId, ESD.Condition cond, EsdCommandList list, int index, ESD.CommandCall cmd)
    {
        var target = list switch
        {
            EsdCommandList.Entry => state.EntryCommands,
            EsdCommandList.While => state.WhileCommands,
            EsdCommandList.Exit => state.ExitCommands,
            _ => cond?.PassCommands
        };
        if (target == null)
            return;

        Run(new EsdInsertOp<ESD.CommandCall>(target, index < 0 ? target.Count : index, cmd), "insert command");
        Selection.SelectItem(cmd, stateId);
    }

    /// <summary>A new command with the catalogue's default arg for each documented arg</summary>
    public static ESD.CommandCall NewCommand(EsdCallDef def)
    {
        return new ESD.CommandCall(def.Bank, def.Id, def.Args.Where(a => !a.Optional).Select(a => EsdEdits.DefaultArg(a.Type)).ToArray());
    }

    public void DeleteCommand(EsdRow row)
    {
        var list = ListOf(row);
        if (row.Index < 0 || row.Index >= list.Count || !ReferenceEquals(list[row.Index], row.Command))
            return;
        Run(new EsdRemoveOp<ESD.CommandCall>(list, row.Index), "delete command");
        Selection.SelectItem(row.State, row.StateId);
    }

    public void DuplicateCommand(EsdRow row)
    {
        var list = ListOf(row);
        var copy = EsdEdits.CloneCommand(row.Command);
        Run(new EsdInsertOp<ESD.CommandCall>(list, row.Index + 1, copy), "duplicate command");
        Selection.SelectItem(copy, row.StateId);
    }

    public void MoveCommand(EsdRow row, int dir)
    {
        var list = ListOf(row);
        var to = row.Index + dir;
        if (to < 0 || to >= list.Count)
            return;
        Run(new EsdMoveOp<ESD.CommandCall>(list, row.Index, to), "move command");
        Selection.ScrollToItem = true;
    }

    // --- transitions ---

    /// <summary>Adds a transition that is never taken, to the state itself: a safe start to set the test and target of</summary>
    public void AddTransition(List<ESD.Condition> list, long stateId, int index = -1)
    {
        var c = new ESD.Condition(stateId, EsdEdits.Never);
        Run(new EsdInsertOp<ESD.Condition>(list, index < 0 ? list.Count : index, c), "add transition");
        Selection.SelectItem(c, stateId);
    }

    public void DeleteCondition(EsdRow row)
    {
        if (row.ParentList == null || !ReferenceEquals(row.ParentList[row.Index], row.Condition))
            return;
        Run(new EsdRemoveOp<ESD.Condition>(row.ParentList, row.Index), "delete transition");
        Selection.SelectItem(row.State, row.StateId);
    }

    public void DuplicateCondition(EsdRow row)
    {
        var copy = EsdEdits.CloneCondition(row.Condition);
        Run(new EsdInsertOp<ESD.Condition>(row.ParentList, row.Index + 1, copy), "duplicate transition");
        Selection.SelectItem(copy, row.StateId);
    }

    public void MoveCondition(EsdRow row, int dir)
    {
        var to = row.Index + dir;
        if (row.ParentList == null || to < 0 || to >= row.ParentList.Count)
            return;
        Run(new EsdMoveOp<ESD.Condition>(row.ParentList, row.Index, to), "move transition");
        Selection.ScrollToItem = true;
    }

    /// <summary>Gives this place its own copy of a condition other places share, so editing it changes only here</summary>
    public void MakeUnique(EsdRow row)
    {
        if (row.SharedCount < 2 || row.ParentList == null)
            return;
        var copy = EsdEdits.CloneCondition(row.Condition);
        Run(new EsdReplaceOp<ESD.Condition>(row.ParentList, row.Index, copy), "make unique");
        Selection.SelectItem(copy, row.StateId);
    }

    // --- states ---

    public void AddState(long afterId)
    {
        if (Machine == null)
            return;
        var id = EsdEdits.NextStateId(Machine);
        var state = new ESD.State();
        Run(EsdStateOp.AddState(Machine, id, state, afterId), "add state");
        Selection.SelectItem(state, id);
    }

    public void DuplicateState(long id)
    {
        if (Machine == null || !Machine.TryGetValue(id, out var state))
            return;
        var newId = EsdEdits.NextStateId(Machine);
        var copy = EsdEdits.CloneState(state);
        Run(EsdStateOp.AddState(Machine, newId, copy, id), "duplicate state");
        Selection.SelectItem(copy, newId);
    }

    /// <summary>Why a state can't be deleted (null if it can): the start state, or one transitions still lead to</summary>
    public string DeleteStateRefusal(long id)
    {
        if (Machine == null || !Machine.ContainsKey(id))
            return "";
        if (id == 0)
            return LOC.Get("ESD_Edit_Delete_Start_Refused");
        var n = EsdChecks.TransitionsTo(Machine, id);
        return n > 0 ? LOC.Get("ESD_Edit_Delete_Targeted_Refused", id, n) : null;
    }

    public void DeleteState(long id)
    {
        var refusal = DeleteStateRefusal(id);
        if (refusal != null)
        {
            if (refusal.Length > 0)
                Smithbox.Log(this, refusal, Microsoft.Extensions.Logging.LogLevel.Warning);
            return;
        }
        Run(EsdStateOp.RemoveState(Machine, id), "delete state");
        Selection.ClearItem();
    }

    // --- structural edits (P4, Framework/EsdStructureEdit: gated by EsdSurvey structedit) ---

    /// <summary>Runs a plan, or logs why it was refused. Returns whether it ran.</summary>
    private bool RunPlan(EsdEditPlan plan, string what)
    {
        if (plan.Op == null)
        {
            if (!string.IsNullOrEmpty(plan.Refusal) && plan.Refusal != "ESD_Edit_No_Change")
                Smithbox.Log(this, LOC.Get(plan.Refusal), Microsoft.Extensions.Logging.LogLevel.Warning);
            return false;
        }
        Run(plan.Op, what);
        return true;
    }

    /// <summary>Runs a plan that adds a state, and selects the new state (it's in the machine once the edit has run)</summary>
    private void RunAndSelect(EsdEditPlan plan, string what)
    {
        if (RunPlan(plan, what) && plan.NewState != null)
            Selection.SelectItem(plan.NewState, plan.NewId);
    }

    public void InsertStateAfter(long id)
    {
        if (Machine != null)
            RunAndSelect(EsdStructureEdit.InsertAfter(Machine, id), "insert state");
    }

    public void AddBranch(long id)
    {
        if (Machine != null)
            RunAndSelect(EsdStructureEdit.AddBranch(Machine, id), "add branch");
    }

    public void InsertCall(long id, long callee)
    {
        if (Machine != null && Selection.SelectedFile?.Esd is { } esd)
            RunAndSelect(EsdStructureEdit.InsertCall(esd, Machine, id, callee), "insert call");
    }

    public void RemoveStep(long id)
    {
        if (Machine != null && RunPlan(EsdStructureEdit.RemoveStep(Machine, id), "remove step"))
            Selection.ClearItem();
    }

    /// <summary>A command dropped on another block: moved into that list, before the index</summary>
    public void MoveCommandTo(EsdRow dragged, List<ESD.CommandCall> to, int index, long toState)
    {
        if (dragged?.Kind != EsdRowKind.Command)
            return;
        var from = ListOf(dragged);
        if (dragged.Index < 0 || dragged.Index >= from.Count || !ReferenceEquals(from[dragged.Index], dragged.Command))
            return;
        if (RunPlan(EsdStructureEdit.MoveCommand(from, dragged.Index, to, index), "move command"))
            Selection.SelectItem(dragged.Command, toState);
    }

    /// <summary>The list a drop on a row puts a command into: before a command, at the end of a state's entry or a transition's pass list</summary>
    public static (List<ESD.CommandCall> List, int Index) DropTarget(EsdRow row) => row.Kind switch
    {
        EsdRowKind.Command => (ListOf(row), row.Index),
        EsdRowKind.State => (row.State.EntryCommands, row.State.EntryCommands.Count),
        EsdRowKind.Condition => (row.Condition.PassCommands, row.Condition.PassCommands.Count),
        _ => (null, -1)
    };

    // --- palette (P4) ---

    /// <summary>A palette command double-clicked: after the selected command, else at the end of the selected state's entry or transition's pass list</summary>
    public void InsertAtSelection(ESD.CommandCall cmd)
    {
        var row = SelectedRow;
        var (list, index) = row != null ? DropTarget(row) : (null, -1);
        if (list == null)
        {
            Smithbox.Log(this, LOC.Get("ESD_PaletteTool_Select_Block"), Microsoft.Extensions.Logging.LogLevel.Warning);
            return;
        }
        if (row.Kind == EsdRowKind.Command)
            index++;
        Run(new EsdInsertOp<ESD.CommandCall>(list, index, cmd), "insert command");
        Selection.SelectItem(cmd, row.StateId);
    }

    /// <summary>A palette command dropped on a block</summary>
    public void DropCommand(EsdRow row, ESD.CommandCall cmd)
    {
        var (list, index) = DropTarget(row);
        if (list == null || cmd == null)
            return;
        Run(new EsdInsertOp<ESD.CommandCall>(list, index, cmd), "insert command");
        Selection.SelectItem(cmd, row.StateId);
    }

    /// <summary>
    /// A palette expression dropped on a value chip (path set) or on a transition's block (path null). On a chip: the
    /// node replaces the value, or an operator wraps it. On a block: an operator wraps the whole test; a function or
    /// value replaces a constant test, or joins any other test with &amp;&amp;.
    /// </summary>
    public void DropExpression(ESD.Condition cond, ESD.CommandCall cmd, int arg, List<int> path, Func<EsdExprNode> make, byte? wrapOp)
    {
        var bytes = cmd != null ? (arg >= 0 && arg < cmd.Arguments.Count ? cmd.Arguments[arg] : null) : cond?.Evaluator;
        var root = bytes != null ? EsdExpr.Decode(bytes) : null;
        if (root == null)
            return;

        byte[] updated;
        if (path != null)
        {
            var at = EsdEdits.At(root, path);
            if (at == null)
                return;
            var node = wrapOp is byte op ? EsdExprText.Wrap(at, op) : make?.Invoke();
            updated = node != null ? EsdExprText.ReplaceAt(bytes, path, node) : null;
        }
        else if (wrapOp is byte op)
            updated = EsdExpr.Encode(EsdExprText.Wrap(root, op));
        else
        {
            var node = make?.Invoke();
            if (node == null)
                return;
            updated = EsdExpr.Constant(root) != null
                ? EsdExpr.Encode(node)
                : EsdExpr.Encode(new EsdExprNode { Kind = EsdExprKind.Binary, Op = 0x98, Children = { root, node } });
        }

        if (updated == null)
            return;
        if (cmd != null)
            SetArgument(cmd, arg, updated);
        else
            SetEvaluator(cond, updated);
    }

    // --- ESD files of a binder ---

    /// <summary>The names of a binder's ESDs</summary>
    public static List<string> FileNames(EsdContainer container) => container.Files.Select(f => f.Name).ToList();

    /// <summary>Why a file can't be duplicated under a name, or null (LOC key)</summary>
    public static string DuplicateRefusal(EsdFileWrapper file, string name)
    {
        if (file?.Esd == null || !file.Container.IsBinder)
            return "ESD_File_Not_Binder_Entry";
        return EsdBinderEntries.NameRefusal(name, FileNames(file.Container), file.Kind == EsdScriptKind.Talk);
    }

    /// <summary>Other loaded binders that already have an ESD of this name (talk IDs are global)</summary>
    public List<string> NameUsedElsewhere(EsdContainer container, string name)
    {
        var bank = View.Project.Handler.EsdData?.PrimaryBank;
        return bank?.Containers.Where(c => c != container && c.IsLoaded && c.Files.Any(f => f.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            .Select(c => c.Name).ToList() ?? new();
    }

    /// <summary>A copy of an ESD in the same binder, after it, under a new name; selected once made</summary>
    public void DuplicateFile(EsdFileWrapper file, string name)
    {
        var refusal = DuplicateRefusal(file, name);
        if (refusal != null)
        {
            Smithbox.Log(this, LOC.Get(refusal, name), Microsoft.Extensions.Logging.LogLevel.Warning);
            return;
        }

        var container = file.Container;
        var copy = EsdFileWrapper.Create(container, name, EsdBinderEntries.Copy(file.Esd, file.Name, name));
        var index = container.Files.IndexOf(file) + 1;
        Pending.Add(new EsdEditAction(container, new EsdInsertOp<EsdFileWrapper>(container.Files, index, copy), "duplicate ESD"));
        PendingSelect = copy;
    }

    /// <summary>Why a file can't be deleted, or null (LOC key): only binder entries, and never a binder's last ESD</summary>
    public static string DeleteFileRefusal(EsdFileWrapper file)
    {
        if (file == null || !file.Container.IsBinder)
            return "ESD_File_Not_Binder_Entry";
        if (file.Container.Files.Count < 2)
            return "ESD_File_Delete_Last_Refused";
        return null;
    }

    public void DeleteFile(EsdFileWrapper file)
    {
        var refusal = DeleteFileRefusal(file);
        if (refusal != null)
        {
            Smithbox.Log(this, LOC.Get(refusal), Microsoft.Extensions.Logging.LogLevel.Warning);
            return;
        }

        var container = file.Container;
        var index = container.Files.IndexOf(file);
        if (index < 0)
            return;
        Pending.Add(new EsdEditAction(container, new EsdRemoveOp<EsdFileWrapper>(container.Files, index), "delete ESD"));
        if (Selection.SelectedFile == file)
            PendingSelect = container.Files[index > 0 ? index - 1 : 1];
    }

    // The file (or container) to select once the queued file edit has run
    private EsdFileWrapper PendingSelect;
    private EsdContainer PendingSelectContainer;

    private EsdBank Bank => View.Project.Handler.EsdData?.PrimaryBank;

    /// <summary>Why an ESD can't be renamed to a name, or null (LOC key)</summary>
    public static string RenameRefusal(EsdFileWrapper file, string name)
    {
        if (file?.Esd == null || !file.Container.IsBinder)
            return "ESD_File_Not_Binder_Entry";
        if (string.Equals(file.Name, name, StringComparison.Ordinal))
            return "ESD_Edit_No_Change";
        return EsdBinderEntries.NameRefusal(name, FileNames(file.Container).Where(n => !n.Equals(file.Name, StringComparison.OrdinalIgnoreCase)), file.Kind == EsdScriptKind.Talk);
    }

    /// <summary>Renames a binder's ESD (its entry on save); its internal name follows when it followed the old one</summary>
    public void RenameFile(EsdFileWrapper file, string name)
    {
        var refusal = RenameRefusal(file, name);
        if (refusal != null)
        {
            if (refusal != "ESD_Edit_No_Change")
                Smithbox.Log(this, LOC.Get(refusal, name), Microsoft.Extensions.Logging.LogLevel.Warning);
            return;
        }

        var ops = new List<IEsdOp>
        {
            new EsdSetOp<string>(() => file.Name, v => file.Name = v, name),
            new EsdSetOp<string>(() => file.EntryName, v => file.EntryName = v, name + ".esd"),
        };
        if (EsdBinderEntries.RenamedInternalName(file.Esd, file.Name, name) is { } inner)
            ops.Add(new EsdSetOp<string>(() => file.Esd.Name, v => file.Esd.Name = v, inner));

        Pending.Add(new EsdEditAction(file.Container, new EsdCompoundOp(ops), "rename ESD"));
    }

    /// <summary>A new empty ESD in a binder, in the format of the binder's other ESDs</summary>
    public void AddBlankFile(EsdContainer container, string name)
    {
        var refusal = container?.IsBinder == true && container.IsLoaded
            ? EsdBinderEntries.NameRefusal(name, FileNames(container), container.Kind == EsdScriptKind.Talk)
            : "ESD_File_Not_Binder_Entry";
        if (refusal != null)
        {
            Smithbox.Log(this, LOC.Get(refusal, name), Microsoft.Extensions.Logging.LogLevel.Warning);
            return;
        }

        var sibling = container.Files.FirstOrDefault(f => f.Esd != null)?.Esd;
        var file = EsdFileWrapper.Create(container, name, EsdBinderEntries.Blank(sibling, name));
        Pending.Add(new EsdEditAction(container, new EsdInsertOp<EsdFileWrapper>(container.Files, container.Files.Count, file), "new ESD"));
        PendingSelect = file;
    }

    // --- whole files: binders and loose ESDs ---

    /// <summary>A container's name split for a new one: the stem (m10_00_00_00, c0000) and what follows (.talkesdbnd)</summary>
    public static string StemOf(EsdContainer c) => c.Name.Split('.')[0];

    /// <summary>The VFS path and list name a container would have under another stem, in the same folder</summary>
    private static (string Path, string Name) Renamed(EsdContainer c, string stem)
    {
        var slash = c.Path.LastIndexOf('/');
        var fileName = c.Path[(slash + 1)..];
        var oldStem = fileName.Split('.')[0];
        return (c.Path[..(slash + 1)] + stem + fileName[oldStem.Length..], stem + c.Name[StemOf(c).Length..]);
    }

    /// <summary>Why a new file can't be made from this one under a stem, or null (LOC key)</summary>
    public string NewContainerRefusal(EsdContainer source, string stem)
    {
        var bank = Bank;
        if (bank == null || source == null)
            return "ESD_File_Not_Binder_Entry";
        var refusal = EsdBinderEntries.ContainerNameRefusal(stem, source.Kind, source.IsBinder, bank.Containers.Where(c => c.IsBinder == source.IsBinder && c.Kind == source.Kind).Select(StemOf));
        if (refusal != null)
            return refusal;
        return bank.IsFreePath(Renamed(source, stem).Path) ? null : "ESD_File_Name_Taken";
    }

    /// <summary>A free stem for a copy of a container: the next map ID for a talk binder, else stem_copy</summary>
    public string SuggestContainerStem(EsdContainer source)
    {
        var taken = Bank?.Containers.Select(StemOf).ToList() ?? new();
        var stem = StemOf(source);
        var m = System.Text.RegularExpressions.Regex.Match(stem, @"^m(\d\d)_(\d\d)_(\d\d)_(\d\d)$");
        if (m.Success)
        {
            // The last part counting up: m10_00_00_00 → m10_00_00_01
            for (var i = int.Parse(m.Groups[4].Value) + 1; i < 100; i++)
            {
                var candidate = $"m{m.Groups[1].Value}_{m.Groups[2].Value}_{m.Groups[3].Value}_{i:D2}";
                if (NewContainerRefusal(source, candidate) == null)
                    return candidate;
            }
        }
        return EsdBinderEntries.SuggestName(stem, taken);
    }

    /// <summary>
    /// A copy of a whole binder or loose ESD under a new name, in the same folder: every ESD copied, a binder's entry
    /// folders renamed for it. Written when saved.
    /// </summary>
    public void DuplicateContainer(EsdContainer source, string stem)
    {
        var refusal = NewContainerRefusal(source, stem) ?? (source.EnsureLoaded() ? null : "ESD_File_Not_Loaded");
        if (refusal != null)
        {
            Smithbox.Log(this, LOC.Get(refusal, stem), Microsoft.Extensions.Logging.LogLevel.Warning);
            return;
        }

        var (path, name) = Renamed(source, stem);
        var oldStem = StemOf(source);
        var copy = EsdContainer.CreateNew(Bank, path, name, source.IsBinder, source.Kind, source.IsBinder ? source.Path : null,
            source.IsBinder ? (oldStem, stem) : null,
            c => source.Files.Where(f => f.Esd != null).Select(f => source.IsBinder
                ? EsdFileWrapper.Create(c, f.Name, EsdBinderEntries.Copy(f.Esd, f.Name, f.Name))
                : EsdFileWrapper.CreateLoose(c, stem, EsdBinderEntries.Copy(f.Esd, f.Name, stem))).ToList());

        Pending.Add(new EsdEditAction(copy, new EsdContainerAddOp(Bank, copy), "duplicate file"));
        PendingSelectContainer = copy;
    }

    /// <summary>A new binder like another (its header and layout), holding one blank ESD</summary>
    public void NewBinder(EsdContainer template, string stem, string firstName)
    {
        var refusal = template?.IsBinder != true ? "ESD_File_Not_Binder_Entry"
            : NewContainerRefusal(template, stem) ?? EsdBinderEntries.NameRefusal(firstName, Array.Empty<string>(), template.Kind == EsdScriptKind.Talk)
            ?? (template.EnsureLoaded() ? null : "ESD_File_Not_Loaded");
        if (refusal != null)
        {
            Smithbox.Log(this, LOC.Get(refusal, stem), Microsoft.Extensions.Logging.LogLevel.Warning);
            return;
        }

        var (path, name) = Renamed(template, stem);
        var sibling = template.Files.FirstOrDefault(f => f.Esd != null)?.Esd;
        var binder = EsdContainer.CreateNew(Bank, path, name, true, template.Kind, template.Path, (StemOf(template), stem),
            c => new List<EsdFileWrapper> { EsdFileWrapper.Create(c, firstName, EsdBinderEntries.Blank(sibling, firstName)) });

        Pending.Add(new EsdEditAction(binder, new EsdContainerAddOp(Bank, binder), "new binder"));
        PendingSelectContainer = binder;
    }

    /// <summary>
    /// Why a whole file can't be deleted, or null (LOC key). Only what the project adds or overrides: a file only the
    /// game has can't be removed (the game would still load it).
    /// </summary>
    public static string DeleteContainerRefusal(EsdContainer c)
    {
        if (c == null)
            return "ESD_File_Not_Binder_Entry";
        if (c.PendingRevert)
            return "ESD_File_Already_Reverting";
        if (!c.IsNew && !c.InProject)
            return "ESD_File_Delete_Vanilla_Refused";
        return null;
    }

    /// <summary>
    /// Deletes a whole binder or loose ESD: an unsaved new one goes at once; a project-only file is moved into the
    /// project's backup folder on save; a project copy of a game file is moved there too, and the game's comes back
    /// </summary>
    public void DeleteContainer(EsdContainer c)
    {
        var refusal = DeleteContainerRefusal(c);
        if (refusal != null)
        {
            Smithbox.Log(this, LOC.Get(refusal), Microsoft.Extensions.Logging.LogLevel.Warning);
            return;
        }

        IEsdOp op = !c.IsNew && c.InGame ? new EsdContainerRevertOp(c) : new EsdContainerRemoveOp(Bank, c);
        Pending.Add(new EsdEditAction(c, op, "delete file"));
    }

    // --- machines ---

    public void NewMachine()
    {
        if (Selection.SelectedFile?.Esd is not { } esd)
            return;
        var plan = EsdStructureEdit.NewMachine(esd);
        if (RunPlan(plan, "new machine"))
            Selection.SelectMachine(plan.NewId);
    }

    public void DuplicateMachine(long mid)
    {
        if (Selection.SelectedFile?.Esd is not { } esd)
            return;
        var plan = EsdStructureEdit.DuplicateMachine(esd, mid);
        if (RunPlan(plan, "duplicate machine"))
            Selection.SelectMachine(plan.NewId);
    }

    public string DeleteMachineRefusal(long mid) =>
        Selection.SelectedFile?.Esd is { } esd ? EsdStructureEdit.DeleteMachine(esd, mid).Refusal : "";

    public void DeleteMachine(long mid)
    {
        if (Selection.SelectedFile?.Esd is not { } esd)
            return;
        if (RunPlan(EsdStructureEdit.DeleteMachine(esd, mid), "delete machine") && Selection.SelectedMachine == mid)
        {
            var other = esd.StateGroups.Keys.Where(k => k != mid).OrderBy(k => EsdLayout.IsSubMachine(k) ? 1 : 0).ThenBy(k => k).FirstOrDefault();
            Selection.SelectMachine(other);
        }
    }

    /// <summary>The machine list's right-click menu (a machine, or null for empty space)</summary>
    public void MachineMenu(long? mid)
    {
        if (ImGui.MenuItem(LOC.Get("ESD_Edit_New_Machine")))
            NewMachine();
        GUI.Tooltip(LOC.Get("ESD_Edit_New_Machine_TT"));

        if (mid is not long id)
            return;

        if (ImGui.MenuItem(LOC.Get("ESD_Edit_Duplicate_Machine")))
            DuplicateMachine(id);
        GUI.Tooltip(LOC.Get("ESD_Edit_Duplicate_Machine_TT"));

        var refusal = DeleteMachineRefusal(id);
        if (ImGui.MenuItem(LOC.Get("ESD_Edit_Delete_Machine"), "", false, refusal == null))
            DeleteMachine(id);
        if (!string.IsNullOrEmpty(refusal))
            GUI.Tooltip(LOC.Get(refusal));
        else if (!EsdLayout.IsSubMachine(id))
            GUI.Tooltip(LOC.Get("ESD_Edit_Delete_Entry_Machine_TT"));
    }

    /// <summary>The flow part of a state's menu: insert after, add branch, call and wait, remove step</summary>
    private void FlowMenu(EsdRow row)
    {
        var machine = Machine;
        if (machine == null)
            return;

        if (ImGui.MenuItem(LOC.Get("ESD_Edit_Insert_State_After")))
            InsertStateAfter(row.StateId);
        GUI.Tooltip(LOC.Get("ESD_Edit_Insert_State_After_TT"));

        var branch = EsdStructureEdit.AddBranch(machine, row.StateId).Refusal;
        if (ImGui.MenuItem(LOC.Get("ESD_Edit_Add_Branch"), "", false, branch == null))
            AddBranch(row.StateId);
        GUI.Tooltip(LOC.Get(branch ?? "ESD_Edit_Add_Branch_TT"));

        if (ImGui.BeginMenu(LOC.Get("ESD_Edit_Insert_Call")))
        {
            var file = Selection.SelectedFile;
            foreach (var mid in file?.Esd?.StateGroups.Keys.OrderBy(k => k) ?? Enumerable.Empty<long>())
                if (ImGui.MenuItem($"{EsdFormat.MachineLabel(file, mid)}##esdCallWait{mid}"))
                    InsertCall(row.StateId, mid);
            ImGui.EndMenu();
        }
        GUI.Tooltip(LOC.Get("ESD_Edit_Insert_Call_TT"));

        var remove = EsdStructureEdit.RemoveStep(machine, row.StateId).Refusal;
        if (ImGui.MenuItem(LOC.Get("ESD_Edit_Remove_Step"), "", false, remove == null))
            RemoveStep(row.StateId);
        GUI.Tooltip(LOC.Get(remove ?? "ESD_Edit_Remove_Step_TT"));
    }

    // --- keys and menus ---

    public void Delete()
    {
        var row = SelectedRow;
        switch (row?.Kind)
        {
            case EsdRowKind.State: DeleteState(row.StateId); break;
            case EsdRowKind.Command: DeleteCommand(row); break;
            case EsdRowKind.Condition: DeleteCondition(row); break;
        }
    }

    public void Duplicate()
    {
        var row = SelectedRow;
        switch (row?.Kind)
        {
            case EsdRowKind.State: DuplicateState(row.StateId); break;
            case EsdRowKind.Command: DuplicateCommand(row); break;
            case EsdRowKind.Condition: DuplicateCondition(row); break;
        }
    }

    public void Move(int dir)
    {
        var row = SelectedRow;
        switch (row?.Kind)
        {
            case EsdRowKind.Command: MoveCommand(row, dir); break;
            case EsdRowKind.Condition: MoveCondition(row, dir); break;
        }
    }

    /// <summary>The right-click menu of a block</summary>
    public void ContextMenu(EsdRow row)
    {
        switch (row.Kind)
        {
            case EsdRowKind.State:
                if (ImGui.MenuItem(LOC.Get("ESD_Edit_Add_State")))
                    AddState(row.StateId);
                GUI.Tooltip(LOC.Get("ESD_Edit_Add_State_TT"));
                if (ImGui.MenuItem(LOC.Get("ESD_Edit_Duplicate_State"), InputManager.GetHint(KeybindID.Duplicate)))
                    DuplicateState(row.StateId);
                var refusal = DeleteStateRefusal(row.StateId);
                if (ImGui.MenuItem(LOC.Get("ESD_Edit_Delete_State"), InputManager.GetHint(KeybindID.Delete), false, refusal == null))
                    DeleteState(row.StateId);
                if (!string.IsNullOrEmpty(refusal))
                    GUI.Tooltip(refusal);
                ImGui.Separator();
                FlowMenu(row);
                ImGui.Separator();
                if (ImGui.MenuItem(LOC.Get("ESD_Edit_Add_Transition")))
                    AddTransition(row.State.Conditions, row.StateId);
                GUI.Tooltip(LOC.Get("ESD_Edit_Add_Transition_TT"));
                CommandMenu(LOC.Get("ESD_Edit_Add_Entry"), row.State, row.StateId, null, EsdCommandList.Entry, -1);
                CommandMenu(LOC.Get("ESD_Edit_Add_While"), row.State, row.StateId, null, EsdCommandList.While, -1);
                CommandMenu(LOC.Get("ESD_Edit_Add_Exit"), row.State, row.StateId, null, EsdCommandList.Exit, -1);
                break;

            case EsdRowKind.Command:
                if (ImGui.MenuItem(LOC.Get("ESD_Edit_Duplicate"), InputManager.GetHint(KeybindID.Duplicate)))
                    DuplicateCommand(row);
                if (ImGui.MenuItem(LOC.Get("ESD_Edit_Delete"), InputManager.GetHint(KeybindID.Delete)))
                    DeleteCommand(row);
                if (ImGui.MenuItem(LOC.Get("ESD_Edit_Move_Up"), InputManager.GetHint(KeybindID.Reorder_Up), false, row.Index > 0))
                    MoveCommand(row, -1);
                if (ImGui.MenuItem(LOC.Get("ESD_Edit_Move_Down"), InputManager.GetHint(KeybindID.Reorder_Down), false, row.Index < ListOf(row).Count - 1))
                    MoveCommand(row, 1);
                if (row.Command.Arguments.Count > 0 && ImGui.BeginMenu(LOC.Get("ESD_Text_Edit_Arg")))
                {
                    var file = Selection.SelectedFile;
                    var cat = file != null ? View.Project.Handler.EsdData?.Catalog?.For(file.Kind) : null;
                    var def = EsdLayout.KindOf(row.Command) == EsdCommandKind.Game ? cat?.Command(row.Command.CommandBank, row.Command.CommandID) : null;
                    for (var i = 0; i < row.Command.Arguments.Count; i++)
                    {
                        var argName = def != null && i < def.Args.Count ? def.Args[i].Name : $"arg{i}";
                        var node = EsdExpr.Decode(row.Command.Arguments[i]);
                        var preview = node != null ? EsdExprText.Format(node, cat) : "?";
                        if (preview.Length > 40) preview = preview[..40] + "…";
                        if (ImGui.MenuItem($"{argName}: {preview}##esdArgText{i}", i == 0 ? "F2" : ""))
                            View.StateView.OpenTextEditor(null, row.Command, i);
                    }
                    ImGui.EndMenu();
                }
                GUI.Tooltip(LOC.Get("ESD_Text_Edit_Arg_TT"));
                ImGui.Separator();
                CommandMenu(LOC.Get("ESD_Edit_Insert_After"), row.State, row.StateId, row.Condition, row.List, row.Index + 1);
                break;

            case EsdRowKind.Condition:
                if (ImGui.MenuItem(LOC.Get("ESD_Edit_Duplicate"), InputManager.GetHint(KeybindID.Duplicate)))
                    DuplicateCondition(row);
                if (ImGui.MenuItem(LOC.Get("ESD_Edit_Delete"), InputManager.GetHint(KeybindID.Delete)))
                    DeleteCondition(row);
                if (ImGui.MenuItem(LOC.Get("ESD_Edit_Move_Up"), InputManager.GetHint(KeybindID.Reorder_Up), false, row.Index > 0))
                    MoveCondition(row, -1);
                if (ImGui.MenuItem(LOC.Get("ESD_Edit_Move_Down"), InputManager.GetHint(KeybindID.Reorder_Down), false, row.ParentList != null && row.Index < row.ParentList.Count - 1))
                    MoveCondition(row, 1);
                if (row.SharedCount > 1)
                {
                    if (ImGui.MenuItem(LOC.Get("ESD_Edit_Make_Unique")))
                        MakeUnique(row);
                    GUI.Tooltip(LOC.Get("ESD_Edit_Make_Unique_TT", row.SharedCount));
                }
                ImGui.Separator();
                if (ImGui.MenuItem(LOC.Get("ESD_Text_Edit_Test"), "F2"))
                    View.StateView.OpenTextEditor(row.Condition, null, -1);
                GUI.Tooltip(LOC.Get("ESD_Text_Edit_Test_TT"));
                if (ImGui.MenuItem(LOC.Get("ESD_Edit_Set_Always")))
                    SetEvaluator(row.Condition, EsdEdits.Always);
                if (ImGui.MenuItem(LOC.Get("ESD_Edit_Set_Never")))
                    SetEvaluator(row.Condition, EsdEdits.Never);
                GUI.Tooltip(LOC.Get("ESD_Edit_Set_Never_TT"));
                if (row.Condition.TargetState != null && ImGui.MenuItem(LOC.Get("ESD_Edit_Clear_Target")))
                    SetTarget(row.Condition, null);
                ImGui.Separator();
                if (ImGui.MenuItem(LOC.Get("ESD_Edit_Add_Subcondition")))
                    AddTransition(row.Condition.Subconditions, row.StateId);
                GUI.Tooltip(LOC.Get("ESD_Edit_Add_Subcondition_TT"));
                CommandMenu(LOC.Get("ESD_Edit_Add_Pass"), row.State, row.StateId, row.Condition, EsdCommandList.Pass, -1);
                break;
        }
    }

    private string CommandFilter = "";

    /// <summary>A submenu of the commands to insert: the catalogue's for this kind of ESD, a call to a machine of the file, return</summary>
    private void CommandMenu(string label, ESD.State state, long stateId, ESD.Condition cond, EsdCommandList list, int index)
    {
        if (!ImGui.BeginMenu(label))
            return;

        ImGui.SetNextItemWidth(220f * DPI.UIScale());
        ImGui.InputTextWithHint("##esdCmdFilter", LOC.Get("ESD_Edit_Filter_Hint"), ref CommandFilter, 128);

        var file = Selection.SelectedFile;
        var cat = file != null ? View.Project.Handler.EsdData?.Catalog?.For(file.Kind) : null;

        if (ImGui.BeginMenu(LOC.Get("ESD_Edit_Call_Machine")))
        {
            foreach (var mid in file?.Esd?.StateGroups.Keys.OrderBy(k => k) ?? Enumerable.Empty<long>())
            {
                if (ImGui.MenuItem($"{EsdFormat.MachineLabel(file, mid)}##esdCallTo{mid}"))
                    InsertCommand(state, stateId, cond, list, index, new ESD.CommandCall(6, (int)mid));
            }
            ImGui.EndMenu();
        }
        GUI.Tooltip(LOC.Get("ESD_Edit_Call_Machine_TT"));

        if (ImGui.MenuItem(LOC.Get("ESD_Command_Return")))
            InsertCommand(state, stateId, cond, list, index, new ESD.CommandCall(7, -1, EsdEdits.DefaultArg("int")));
        GUI.Tooltip(LOC.Get("ESD_States_Return_TT"));

        ImGui.Separator();

        if (cat == null)
        {
            ImGui.TextDisabled(LOC.Get("ESD_States_No_Catalogue"));
        }
        else
        {
            ImGui.BeginChild("##esdCmdList", new System.Numerics.Vector2(320f * DPI.UIScale(), 300f * DPI.UIScale()));
            foreach (var def in cat.Commands.Values.Where(d => d.Name != null).OrderBy(d => d.Name))
            {
                if (!string.IsNullOrEmpty(CommandFilter) && !def.Name.Contains(CommandFilter, StringComparison.OrdinalIgnoreCase))
                    continue;
                if (ImGui.Selectable($"{def.Name}##esdNewCmd{def.Bank}_{def.Id}"))
                {
                    InsertCommand(state, stateId, cond, list, index, NewCommand(def));
                    ImGui.CloseCurrentPopup();
                }
                if (ImGui.IsItemHovered())
                    GUI.Tooltip($"{def.Bank}:{def.Id}  ({string.Join(", ", def.Args.Select(a => $"{a.Name}: {a.Type}"))})" + (def.Comment != null ? $"\n{def.Comment}" : "")
                        + (def.Seen ? "" : $"\n{LOC.Get("ESD_Edit_Unseen_TT")}"));
            }
            ImGui.EndChild();
        }

        ImGui.EndMenu();
    }
}
