using SoulsFormats;

namespace StudioCore.Editors.EmevdEditor;

/// <summary>Where a dragged or inserted unit goes: before a node of a list, or at the end of the list (Before null)</summary>
public record EmevdDropTarget(List<EmevdNode> List, EmevdNode Before);

/// <summary>
/// The structural edits of the selected event (P4): delete, duplicate, copy/cut/paste, move up/down, drag and drop,
/// insert from the palette. Each works on a copy of the structured tree, goes through EmevdStructureEdit.Apply, and
/// is checked before it's committed: an edit that would add a problem the event didn't have (a goto losing its label,
/// a skip running past the end, a parameter off its arg) is refused and logged, so the file stays as valid as it was.
/// Edits are deferred to the end of the frame, since the Script window draws from the tree they change.
/// </summary>
public class EmevdStructureEditor
{
    private readonly EmevdEditorView View;

    private readonly List<Action> Deferred = new();

    /// <summary>The copied lines; shared by every event and file of the project</summary>
    public List<EmevdClipLine> Clipboard { get; private set; }

    public EmevdStructureEditor(EmevdEditorView view)
    {
        View = view;
    }

    private EmevdCatalog Catalog => View.Project.Handler.EmevdData?.Catalog;

    private EmevdScript Script => View.BlockView.CurrentScript;

    private int Selected => View.Selection.InstructionIndex;

    /// <summary>The selected lines, as a snapshot (edits are deferred, the selection may change before they run)</summary>
    private List<int> SelectedLines => View.Selection.Lines.Where(l => l < (Script?.Lines.Count ?? 0)).OrderBy(l => l).ToList();

    public bool HasLine => Script != null && SelectedLines.Count > 0;

    /// <summary>The bottom selected line, where paste and insert go after</summary>
    private int LastSelected => SelectedLines.Count > 0 ? SelectedLines[^1] : -1;

    public bool CanPaste => Script != null && Clipboard is { Count: > 0 };

    /// <summary>Runs the edits queued this frame</summary>
    public void Flush()
    {
        foreach (var a in Deferred)
            a();

        Deferred.Clear();
    }

    private List<EmevdNode> Tree(EmevdScript script)
    {
        return EmevdStructureEdit.CopyTree(script.Nodes ?? EmevdStructure.Decompile(script));
    }

    // ---------------------------------------------------------------- commands

    // Every command acts on the units of the selected lines (EmevdStructureEdit.UnitLeads): a selected C-block takes
    // its body, so lines inside it that are selected too aren't acted on twice.

    public void Delete()
    {
        if (!HasLine)
            return;

        var lines = SelectedLines;
        Deferred.Add(() => Commit(LOC.Get("EMEVD_Edit_Op_Delete"), (script, tree) =>
            EmevdStructureEdit.DeleteMany(script, tree, EmevdStructureEdit.UnitLeads(script, tree, lines)) ? new() : null,
            selectAfter: lines[0]));
    }

    public void Duplicate()
    {
        if (!HasLine)
            return;

        var lines = SelectedLines;
        Deferred.Add(() => Commit(LOC.Get("EMEVD_Edit_Op_Duplicate"), (script, tree) =>
        {
            var copy = EmevdStructureEdit.DuplicateMany(script, tree, EmevdStructureEdit.UnitLeads(script, tree, lines));
            return copy.Count > 0 ? copy : null;
        }));
    }

    public void Copy()
    {
        if (!HasLine)
            return;

        var tree = Tree(Script);
        var copy = EmevdStructureEdit.CopyMany(Script, tree, EmevdStructureEdit.UnitLeads(Script, tree, SelectedLines));
        if (copy.Count == 0)
            return;

        Clipboard = copy;
        Smithbox.Log(this, LOC.Get("EMEVD_Edit_Copied", Clipboard.Count));
    }

    public void Cut()
    {
        Copy();
        Delete();
    }

    /// <summary>Pastes after the bottom selected block, or at the top of the event when only the header is selected</summary>
    public void Paste()
    {
        if (!CanPaste)
            return;

        var lines = Clipboard;
        var line = LastSelected;
        Deferred.Add(() => Commit(LOC.Get("EMEVD_Edit_Op_Paste"), (script, tree) => InsertAfter(script, tree, line, lines)));
    }

    /// <summary>
    /// Lines from elsewhere (a template) after the bottom selected block, or at the top of the event when only the
    /// header is selected, as a paste: their parameters come with them, and the new lines end up selected
    /// </summary>
    public void InsertLines(List<EmevdClipLine> lines, string what)
    {
        if (Script == null || lines == null || lines.Count == 0)
            return;

        var line = LastSelected;
        Deferred.Add(() => Commit(what, (script, tree) => InsertAfter(script, tree, line, lines)));
    }

    /// <summary>A new instruction from the palette, after the bottom selected block</summary>
    public void Insert(EmevdInstrDef def, EmevdDropTarget target = null)
    {
        if (Script == null || def == null)
            return;

        var line = LastSelected;
        var lines = new List<EmevdClipLine> { EmevdStructureEdit.NewInstruction(def) };

        Deferred.Add(() => Commit(LOC.Get("EMEVD_Edit_Op_Insert", def.DisplayName), (script, tree) =>
        {
            if (target == null)
                return InsertAfter(script, tree, line, lines);

            var at = Resolve(tree, target, script);
            if (at.List == null)
                return null;

            InsertAt(at, NewNodes(script, lines.Count));
            return lines;
        }));
    }

    /// <summary>The selected units can be wrapped: they sit next to each other in one block</summary>
    public bool CanWrap
    {
        get
        {
            if (!HasLine)
                return false;

            var tree = Tree(Script);
            var units = EmevdStructureEdit.UnitLeads(Script, tree, SelectedLines).Select(l => EmevdStructureEdit.UnitOf(Script, tree, l)).OrderBy(u => u.Index).ToList();
            if (units.Count == 0 || units.Any(u => u.List == null || !ReferenceEquals(u.List, units[0].List)))
                return false;

            for (var k = 0; k + 1 < units.Count; k++)
            {
                if (units[k].Index + units[k].Count != units[k + 1].Index)
                    return false;
            }

            return true;
        }
    }

    /// <summary>Wraps the selected units in a new skip or goto C-block (a goto gets a new label after them)</summary>
    public void Wrap(EmevdInstrDef def)
    {
        if (!HasLine || def == null)
            return;

        var lines = SelectedLines;
        Deferred.Add(() => Commit(LOC.Get("EMEVD_Edit_Op_Wrap", def.DisplayName), (script, tree) =>
        {
            var added = EmevdStructureEdit.Wrap(Catalog, script, tree, EmevdStructureEdit.UnitLeads(script, tree, lines), def);
            if (added == null)
                Smithbox.Log(this, LOC.Get("EMEVD_Edit_Wrap_Failed"), Microsoft.Extensions.Logging.LogLevel.Warning);
            return added;
        }));
    }

    /// <summary>The instructions that can head a wrap, by flow kind: every Skip or Goto of the layouts, by bank and ID</summary>
    public List<EmevdInstrDef> WrapDefs(EmevdFlowKind kind)
    {
        var catalog = Catalog;
        if (catalog == null)
            return new();

        if (WrapCache.TryGetValue(kind, out var cached) && ReferenceEquals(WrapCacheCatalog, catalog))
            return cached;

        if (!ReferenceEquals(WrapCacheCatalog, catalog))
            WrapCache.Clear();

        WrapCacheCatalog = catalog;

        var defs = catalog.Classes.Values.OrderBy(c => c.Bank)
            .SelectMany(c => c.Instructions.Values.OrderBy(i => i.ID))
            .Where(d => EmevdControlFlow.Find(catalog.Game, d.Bank, d.ID) is var f && f.Kind == kind
                && d.Args.Any(a => !a.Optional && a.Offset == f.ArgOffset))
            .ToList();

        WrapCache[kind] = defs;
        return defs;
    }

    private readonly Dictionary<EmevdFlowKind, List<EmevdInstrDef>> WrapCache = new();
    private EmevdCatalog WrapCacheCatalog;

    /// <summary>Steps every selected unit once; adjacent ones move together. Nothing moves if one can't.</summary>
    public void Step(bool down)
    {
        if (!HasLine)
            return;

        var lines = SelectedLines;
        Deferred.Add(() => Commit(LOC.Get(down ? "EMEVD_Edit_Op_Down" : "EMEVD_Edit_Op_Up"), (script, tree) =>
            EmevdStructureEdit.StepMany(script, tree, EmevdStructureEdit.UnitLeads(script, tree, lines), down) ? new List<EmevdClipLine>() : null,
            keepLines: lines));
    }

    /// <summary>
    /// Moves a dragged block to a drop target: the whole selection when the dragged line is in it (kept in file order,
    /// next to each other), else that line's block alone
    /// </summary>
    public void Move(int line, EmevdDropTarget target)
    {
        if (Script == null)
            return;

        var lines = View.Selection.IsSelected(line) ? SelectedLines : new List<int> { line };

        Deferred.Add(() => Commit(LOC.Get("EMEVD_Edit_Op_Move"), (script, tree) =>
        {
            var at = Resolve(tree, target, script);
            if (at.List == null)
                return null;

            var leads = EmevdStructureEdit.UnitLeads(script, tree, lines);
            return EmevdStructureEdit.MoveMany(script, tree, leads, at.List, at.Before) ? new List<EmevdClipLine>() : null;
        }, keepLines: lines));
    }

    // ---------------------------------------------------------------- drop targets

    /// <summary>
    /// The drop target for a row: before its block, or at the end of the block a foot or else row closes. Null in the
    /// flat view, which has no blocks to drop into.
    /// </summary>
    public EmevdDropTarget TargetOf(EmevdRow row)
    {
        var script = Script;
        var path = new List<(List<EmevdNode> list, int index)>();
        if (script?.Nodes == null || !EmevdStructureEdit.Find(script.Nodes, row.Line, out var list, out var index, path))
            return null;

        var node = list[index];

        // A case label: the end of the part before it (dropping before the label itself would leave the case)
        if (row.IsCase && path.Count > 0 && path[^1].list[path[^1].index] is { Kind: EmevdNodeKind.Switch } sw)
        {
            var c = sw.Cases.IndexOf(list);
            return new EmevdDropTarget(c <= 0 ? sw.Default : sw.Cases[c - 1], null);
        }

        // "Default": its start
        if (row.Kind == EmevdRowKind.Section)
            return new EmevdDropTarget(node.Default, node.Default.FirstOrDefault());

        if (row.Kind == EmevdRowKind.BlockEnd)
        {
            if (node.Kind == EmevdNodeKind.Switch)
                return new EmevdDropTarget(node.Cases.Count > 0 ? node.Cases[^1] : node.Default, null);

            return new EmevdDropTarget(node.ElseLine >= 0 ? node.Else : node.Body, null);
        }

        if (row.IsElse)
            return new EmevdDropTarget(node.Body, null);

        return new EmevdDropTarget(list, node);
    }

    /// <summary>The top of the event</summary>
    public EmevdDropTarget TopTarget()
    {
        var nodes = Script?.Nodes;
        return nodes == null ? null : new EmevdDropTarget(nodes, nodes.FirstOrDefault());
    }

    /// <summary>The same place in a copy of the tree (targets are found on the shown tree, edits run on a copy)</summary>
    private static EmevdDropTarget Resolve(List<EmevdNode> copy, EmevdDropTarget target, EmevdScript script)
    {
        var shown = script.Nodes ?? new();
        var list = Map(shown, copy, target.List);
        if (list == null)
            return new EmevdDropTarget(null, null);

        var before = target.Before == null ? null : list[target.List.IndexOf(target.Before)];
        return new EmevdDropTarget(list, before);
    }

    /// <summary>The list in the copy that sits where <paramref name="wanted"/> sits in the shown tree</summary>
    private static List<EmevdNode> Map(List<EmevdNode> shown, List<EmevdNode> copy, List<EmevdNode> wanted)
    {
        if (ReferenceEquals(shown, wanted))
            return copy;

        for (var i = 0; i < shown.Count && i < copy.Count; i++)
        {
            var shownChildren = shown[i].Children;
            var copyChildren = copy[i].Children;

            for (var c = 0; c < shownChildren.Count && c < copyChildren.Count; c++)
            {
                if (Map(shownChildren[c], copyChildren[c], wanted) is { } found)
                    return found;
            }
        }

        return null;
    }

    private static void InsertAt(EmevdDropTarget at, List<EmevdNode> nodes)
    {
        var index = at.Before == null ? at.List.Count : at.List.IndexOf(at.Before);
        at.List.InsertRange(index < 0 ? at.List.Count : index, nodes);
    }

    private static List<EmevdNode> NewNodes(EmevdScript script, int count)
    {
        return Enumerable.Range(0, count).Select(k => new EmevdNode { Kind = EmevdNodeKind.Line, Line = script.Lines.Count + k }).ToList();
    }

    private static List<EmevdClipLine> InsertAfter(EmevdScript script, List<EmevdNode> tree, int line, List<EmevdClipLine> lines)
    {
        var nodes = NewNodes(script, lines.Count);

        if (line < 0)
        {
            tree.InsertRange(0, nodes);
            return lines;
        }

        var unit = EmevdStructureEdit.UnitOf(script, tree, line);
        if (unit.List == null)
            return null;

        unit.List.InsertRange(unit.Index + unit.Count, nodes);
        return lines;
    }

    // ---------------------------------------------------------------- commit

    /// <summary>
    /// Applies an edit to a copy of the tree, checks it, and runs it through the undo stack. The edit returns the new
    /// lines it added (empty for none), or null when there's nothing to do.
    /// </summary>
    /// <param name="selectAfter">Select the line now at this index (after a delete)</param>
    /// <param name="keepLines">Keep these lines selected where they moved to</param>
    private void Commit(string what, Func<EmevdScript, List<EmevdNode>, List<EmevdClipLine>> edit, int selectAfter = -1, List<int> keepLines = null)
    {
        var script = Script;
        var file = View.Selection.SelectedFile;
        if (script == null || file == null)
            return;

        var tree = Tree(script);
        var added = edit(script, tree);
        if (added == null)
            return;

        var result = EmevdStructureEdit.Apply(Catalog, script, tree, added);

        // A skip that would have to jump further than its count arg can hold
        if (result.Overflows.Count > 0)
        {
            var (line, count) = result.Overflows[0];
            Smithbox.Log(this, LOC.Get("EMEVD_Edit_Refused", what, LOC.Get("EMEVD_Edit_Skip_Overflow", line, count)), Microsoft.Extensions.Logging.LogLevel.Warning);
            return;
        }

        // The guard: try the result on a copy and compare its problems with the event's own
        var bank = View.Project.Handler.EmevdData?.PrimaryBank;
        EMEVD.Event Find(long id) => bank?.ResolveEvent(file, id).ev;

        // New lines (inserted or pasted) are the user's work in progress: a new goto has no label yet. Only the lines
        // that were already there are protected; the Problems tool still lists the new ones.
        var old = script.Event.Instructions.ToHashSet(ReferenceEqualityComparer.Instance);
        var newLines = Enumerable.Range(0, result.Instructions.Count).Where(i => !old.Contains(result.Instructions[i])).ToHashSet();

        var before = EmevdChecks.Run(script, Find).GroupBy(p => p.Kind).ToDictionary(g => g.Key, g => g.Count());
        var preview = EmevdScript.Build(Catalog, Preview(script.Event, result));
        var worse = EmevdChecks.Run(preview, Find).Where(p => !newLines.Contains(p.Line)).GroupBy(p => p.Kind)
            .Where(g => g.Count() > before.GetValueOrDefault(g.Key))
            .Select(g => LOC.Get($"EMEVD_Problem_{g.Key}"))
            .ToList();

        if (worse.Count > 0)
        {
            Smithbox.Log(this, LOC.Get("EMEVD_Edit_Refused", what, string.Join("; ", worse)), Microsoft.Extensions.Logging.LogLevel.Warning);
            return;
        }

        var oldInstrs = script.Event.Instructions;
        var keep = (keepLines ?? new()).Where(l => l < oldInstrs.Count).Select(l => oldInstrs[l]).ToList();

        View.Edit(new EmevdStructureChange(file, script.Event, result));

        // Selection: the moved lines, every added line (duplicate, paste, insert), or the line now where a deleted one was
        var newInstrs = script.Event.Instructions;
        if (keep.Count > 0)
            View.Selection.SelectLines(keep.Select(i => newInstrs.IndexOf(i)));
        else if (added.Count > 0)
            View.Selection.SelectLines(newLines);
        else if (selectAfter >= 0)
            View.Selection.SelectInstruction(Math.Min(selectAfter, newInstrs.Count - 1));

        View.Selection.ScrollToInstruction = true;
    }

    /// <summary>The edited event on cloned instructions, for checking without changing anything</summary>
    private static EMEVD.Event Preview(EMEVD.Event ev, EmevdEditResult result)
    {
        var clones = new Dictionary<EMEVD.Instruction, EMEVD.Instruction>(ReferenceEqualityComparer.Instance);
        foreach (var i in result.Instructions)
            clones[i] = new EMEVD.Instruction(i.Bank, i.ID, (byte[])(i.ArgData ?? Array.Empty<byte>()).Clone()) { Layer = i.Layer };

        foreach (var (instr, offset, bytes) in result.Patches)
            bytes.CopyTo(clones[instr].ArgData, offset);

        return new EMEVD.Event(ev.ID, ev.RestBehavior)
        {
            Instructions = result.Instructions.Select(i => clones[i]).ToList(),
            Parameters = result.Parameters,
        };
    }
}
