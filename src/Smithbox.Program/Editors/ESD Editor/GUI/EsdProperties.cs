using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Editors.Common;
using System.Numerics;

namespace StudioCore.Editors.EsdEditor;

/// <summary>
/// Details of the selection: the machine (with the machines calling it), a state (and the states leading to it), a
/// condition (its test as text, as a tree and as bytes) or a command (each arg with its documented name, type and role).
/// </summary>
public class EsdProperties
{
    private readonly EsdEditorView View;

    public EsdProperties(EsdEditorView view)
    {
        View = view;
    }

    private EsdKindCatalog Catalog => View.Selection.SelectedFile is { } f ? View.Project.Handler.EsdData?.Catalog?.For(f.Kind) : null;

    public void Draw()
    {
        GUI.TitleHeader(LOC.Get("ESD_Properties_Header"), LOC.Get("ESD_Properties_Header_TT"));

        ImGui.BeginChild("esdProperties", Vector2.Zero, ImGuiChildFlags.Borders);

        var selection = View.Selection;

        switch (selection.SelectedItem)
        {
            case ESD.State state:
                DrawState(state);
                break;
            case ESD.Condition cond:
                DrawCondition(cond);
                break;
            case ESD.CommandCall cmd:
                DrawCommand(cmd);
                break;
            default:
                DrawMachine();
                break;
        }

        ImGui.EndChild();
    }

    private void DrawMachine()
    {
        var selection = View.Selection;
        var file = selection.SelectedFile;

        if (file?.Esd == null || selection.SelectedMachine is not long id || selection.Machine is not { } machine)
        {
            ImGui.TextDisabled(LOC.Get("ESD_Properties_Nothing"));
            return;
        }

        Row(LOC.Get("ESD_Properties_File"), file.EntryName != null ? $"{file.Container.Name} / {file.EntryName}" : file.Container.Path);
        Row(LOC.Get("ESD_Properties_Kind"), EsdFileList.KindName(file.Kind));
        Row(LOC.Get("ESD_Properties_Format"), LOC.Get("ESD_Properties_Format_Value", file.Esd.LongFormat ? 64 : 32, file.Esd.DarkSoulsCount));
        if (!string.IsNullOrEmpty(file.Esd.Name))
            Row(LOC.Get("ESD_Properties_Esd_Name"), file.Esd.Name);

        ImGui.Separator();

        Row(LOC.Get("ESD_Properties_Machine"), $"{EsdFormat.MachineLabel(file, id)}  ({id}, 0x{id:X})");
        Row(LOC.Get("ESD_Properties_States"), machine.Count.ToString());

        var callers = EsdLayout.Callers(file.Esd, id);
        ImGui.TextDisabled(LOC.Get("ESD_Properties_Called_By"));
        GUI.Tooltip(LOC.Get("ESD_Properties_Called_By_TT"));

        if (callers.Count == 0)
            ImGui.TextDisabled(LOC.Get(EsdLayout.IsSubMachine(id) ? "ESD_Properties_Not_Called_Sub" : "ESD_Properties_Not_Called_Entry"));

        foreach (var (caller, calls) in callers)
        {
            if (ImGui.Selectable($"{EsdFormat.MachineLabel(file, caller)}  ×{calls}##esdCaller_{caller}"))
                selection.GoToMachine(caller);
        }

        ImGui.Separator();
        var cat = View.Project.Handler.EsdData?.Catalog;
        Row(LOC.Get("ESD_Properties_Catalogue"), Catalog != null ? $"{cat.Game} {Catalog.Kind}" : LOC.Get("ESD_Properties_Catalogue_None"));
        if (Catalog != null)
            GUI.Tooltip(cat.Source);
    }

    private void DrawState(ESD.State state)
    {
        var selection = View.Selection;
        var file = selection.SelectedFile;
        var id = selection.SelectedState;

        Row(LOC.Get("ESD_Properties_State"), id.ToString());
        if (file != null && selection.SelectedMachine is long m && file.StateNames.TryGetValue((m, id), out var name))
            Row(LOC.Get("ESD_Properties_Name"), name);

        Row(LOC.Get("ESD_Section_Entry"), state.EntryCommands.Count.ToString());
        Row(LOC.Get("ESD_Section_While"), state.WhileCommands.Count.ToString());
        Row(LOC.Get("ESD_Section_Exit"), state.ExitCommands.Count.ToString());
        Row(LOC.Get("ESD_Properties_Conditions"), state.Conditions.Count.ToString());

        ImGui.Separator();

        // The states with a transition (at any depth) to this one
        ImGui.TextDisabled(LOC.Get("ESD_Properties_Reached_From"));
        GUI.Tooltip(LOC.Get("ESD_Properties_Reached_From_TT"));

        var any = false;
        foreach (var (sid, s) in selection.Machine.OrderBy(k => k.Key))
        {
            if (!LeadsTo(s.Conditions, id, new HashSet<ESD.Condition>(ReferenceEqualityComparer.Instance)))
                continue;

            any = true;
            if (ImGui.Selectable(LOC.Get("ESD_States_State_Title", sid) + $"##esdFrom_{sid}"))
                selection.GoToState(sid);
        }

        if (!any)
            ImGui.TextDisabled(LOC.Get("ESD_Properties_Reached_From_None"));
    }

    private static bool LeadsTo(List<ESD.Condition> conds, long target, HashSet<ESD.Condition> seen)
    {
        foreach (var c in conds)
        {
            if (!seen.Add(c))
                continue;
            if (c.TargetState == target || LeadsTo(c.Subconditions, target, seen))
                return true;
        }
        return false;
    }

    // --- editing (conditions and commands) ---

    private readonly EsdInput Input = new();

    // The args shown as expression text although they are one value (the ƒ button), for the selected command
    private readonly HashSet<int> ArgsAsText = new();
    private object ArgsAsTextFor;

    private string CommandFilter = "";

    private static float Avail => ImGui.GetContentRegionAvail().X;

    /// <summary>A state's label for lists and combos: its id, with its DS2 .edd name when it has one</summary>
    private string StateLabel(long id)
    {
        var label = LOC.Get("ESD_States_State_Title", id);
        var file = View.Selection.SelectedFile;
        if (file != null && View.Selection.SelectedMachine is long m && file.StateNames.TryGetValue((m, id), out var name))
            label += $"  {name}";
        return label;
    }

    private void DrawCondition(ESD.Condition c)
    {
        var cat = Catalog;
        var selection = View.Selection;
        var row = View.Commands.SelectedRow;

        // Shared: an edit here changes every place using it
        if (row is { Kind: EsdRowKind.Condition, SharedCount: > 1 } && ReferenceEquals(row.Condition, c))
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("ESD_Properties_Shared", row.SharedCount));
            if (ImGui.Button($"{LOC.Get("ESD_Edit_Make_Unique")}##esdPropUnique"))
                View.Commands.MakeUnique(row);
            GUI.Tooltip(LOC.Get("ESD_Edit_Make_Unique_TT", row.SharedCount));
            ImGui.Separator();
        }

        // Goes to
        GUI.SimpleHeader(LOC.Get("ESD_Properties_Target"), LOC.Get("ESD_Properties_Target_TT"));
        TargetCombo(c, selection.Machine);
        if (c.TargetState is long target)
        {
            ImGui.SameLine();
            if (ImGui.SmallButton($"{Icons.ArrowRight}##esdPropGoTarget"))
                selection.GoToState(target);
            GUI.Tooltip(LOC.Get("ESD_Properties_Go_To_State_TT", target));
        }

        // Test
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("ESD_Properties_Test"), LOC.Get("ESD_Properties_Test_TT"));
        ExpressionBox("##esdPropTest", c.Evaluator, cat, b => View.Commands.SetEvaluator(c, b));

        if (ImGui.SmallButton($"{LOC.Get("ESD_Properties_Always")}##esdPropAlways"))
            View.Commands.SetEvaluator(c, EsdEdits.Always);
        GUI.Tooltip(LOC.Get("ESD_Properties_Always_TT"));
        ImGui.SameLine();
        if (ImGui.SmallButton($"{LOC.Get("ESD_Properties_Never")}##esdPropNever"))
            View.Commands.SetEvaluator(c, EsdEdits.Never);
        GUI.Tooltip(LOC.Get("ESD_Edit_Set_Never_TT"));

        // Each value of the test, typed
        if (EsdExpr.Decode(c.Evaluator) is { } root && EsdExpr.Constant(root) == null)
        {
            var lits = EsdEdits.Literals(root).ToList();
            if (lits.Count > 0)
            {
                GUI.Spacer();
                GUI.SimpleHeader(LOC.Get("ESD_Properties_Values"), LOC.Get("ESD_Properties_Values_TT"));
                Values("esdPropTestVal", c.Evaluator, root, lits, null, -1, cat, b => View.Commands.SetEvaluator(c, b));
            }
        }

        // What it runs and holds: click to select
        if (c.PassCommands.Count > 0)
        {
            GUI.Spacer();
            GUI.SimpleHeader(LOC.Get("ESD_Properties_Pass_Commands"), LOC.Get("ESD_Properties_Pass_Commands_TT"));
            for (var i = 0; i < c.PassCommands.Count; i++)
                if (ImGui.Selectable($"{EsdFormat.CommandName(cat, c.PassCommands[i])}##esdPropPass{i}"))
                    selection.SelectItem(c.PassCommands[i], selection.SelectedState);
        }

        if (c.Subconditions.Count > 0)
        {
            GUI.Spacer();
            GUI.SimpleHeader(LOC.Get("ESD_Properties_Subconditions"), LOC.Get("ESD_Properties_Subconditions_TT"));
            for (var i = 0; i < c.Subconditions.Count; i++)
            {
                var s = c.Subconditions[i];
                var text = Shorten(EsdExpr.IsAlways(s.Evaluator) ? LOC.Get("ESD_Properties_Always") : EsdFormat.ExprText(cat, s.Evaluator), 60);
                var to = s.TargetState is long t ? $"  → {t}" : "";
                if (ImGui.Selectable($"{text}{to}##esdPropSub{i}"))
                    selection.SelectItem(s, selection.SelectedState);
            }
        }

        GUI.Spacer();
        Expression(c.Evaluator, cat, null, "esdCondTree");
    }

    private void TargetCombo(ESD.Condition c, Dictionary<long, ESD.State> machine)
    {
        var current = c.TargetState is long t ? StateLabel(t) : LOC.Get("ESD_Properties_Target_None");
        ImGui.SetNextItemWidth(Avail - (c.TargetState != null ? 32f * DPI.UIScale() : 0));
        if (machine == null || !ImGui.BeginCombo("##esdPropTarget", current, ImGuiComboFlags.HeightLarge))
            return;

        // No target is only meaningful with conditions inside to test (the writer needs one or the other)
        if (c.Subconditions.Count > 0 && ImGui.Selectable(LOC.Get("ESD_Edit_Target_None"), c.TargetState == null))
            View.Commands.SetTarget(c, null);

        foreach (var id in machine.Keys.OrderBy(k => k))
        {
            if (ImGui.Selectable($"{StateLabel(id)}##esdPropTarget{id}", c.TargetState == id))
                View.Commands.SetTarget(c, id);
            if (c.TargetState == id)
                ImGui.SetItemDefaultFocus();
        }
        ImGui.EndCombo();
    }

    private void DrawCommand(ESD.CommandCall c)
    {
        var cat = Catalog;
        var kind = EsdLayout.KindOf(c);
        var def = kind == EsdCommandKind.Game ? cat?.Command(c.CommandBank, c.CommandID) : null;

        if (!ReferenceEquals(ArgsAsTextFor, c))
        {
            ArgsAsText.Clear();
            ArgsAsTextFor = c;
        }

        // What it is: a game command (change it from the catalogue), a call (change the machine) or a return
        GUI.SimpleHeader(LOC.Get("ESD_Properties_Command"), LOC.Get("ESD_Properties_Command_TT"));
        switch (kind)
        {
            case EsdCommandKind.CallMachine:
                MachineCombo(c);
                ImGui.SameLine();
                if (ImGui.SmallButton($"{Icons.ArrowRight}##esdGoCall"))
                    View.Selection.GoToMachine(c.CommandID);
                GUI.Tooltip(LOC.Get("ESD_Properties_Go_To_Machine_TT"));
                break;
            case EsdCommandKind.Return:
                ImGui.Text(EsdFormat.CommandName(cat, c));
                GUI.Tooltip(LOC.Get("ESD_States_Return_TT"));
                break;
            case EsdCommandKind.Game:
                CommandCombo(c, def, cat);
                break;
            default:
                ImGui.Text(EsdFormat.CommandName(cat, c));
                break;
        }
        ImGui.TextDisabled($"{c.CommandBank}:{c.CommandID}");

        if (!string.IsNullOrEmpty(def?.Comment))
            GUI.WrappedTextColored(UI.Current.ImGui_AliasName_Text, def.Comment);

        if (def != null && (c.Arguments.Count < def.RequiredArgs || c.Arguments.Count > def.Args.Count))
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("ESD_States_Arg_Count_TT", c.Arguments.Count, def.RequiredArgs, def.Args.Count));
            if (ImGui.SmallButton($"{LOC.Get("ESD_Properties_Fix_Args")}##esdFixArgs"))
                View.Commands.FixArgumentCount(c, def);
            GUI.Tooltip(LOC.Get("ESD_Properties_Fix_Args_TT"));
        }

        // The args
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("ESD_Properties_Args"), LOC.Get("ESD_Properties_Args_TT"));

        if (c.Arguments.Count == 0)
            ImGui.TextDisabled(LOC.Get("ESD_Properties_No_Args"));

        for (var i = 0; i < c.Arguments.Count; i++)
        {
            ImGui.PushID($"esdArg{i}");
            Argument(c, def, i, cat);
            ImGui.PopID();
        }

        // An optional arg the command doesn't pass yet, or another arg for a call (the callee reads arg0, arg1...) or
        // an undocumented command
        var next = def != null && c.Arguments.Count < def.Args.Count ? def.Args[c.Arguments.Count] : null;
        var canAdd = next != null || kind == EsdCommandKind.CallMachine || kind == EsdCommandKind.Game && def == null;
        if (canAdd)
        {
            var label = next != null ? LOC.Get("ESD_Properties_Add_Named_Arg", next.Name) : LOC.Get("ESD_Properties_Add_Arg");
            if (ImGui.SmallButton($"{Icons.Plus} {label}##esdAddArg"))
                View.Commands.AddArgument(c, EsdEdits.DefaultArg(next?.Type));
            GUI.Tooltip(LOC.Get(next != null ? "ESD_Properties_Add_Named_Arg_TT" : "ESD_Properties_Add_Arg_TT"));
        }
    }

    /// <summary>One arg: its name, type and role, then its value box (or its expression and the values in it)</summary>
    private void Argument(ESD.CommandCall c, EsdCallDef def, int i, EsdKindCatalog cat)
    {
        var argDef = def != null && i < def.Args.Count ? def.Args[i] : null;
        var bytes = c.Arguments[i];
        var root = EsdExpr.Decode(bytes);

        ImGui.Separator();
        ImGui.AlignTextToFramePadding();
        ImGui.Text(argDef?.Name ?? $"arg{i}");
        var tip = argDef != null ? (argDef.Role != null ? $"{argDef.Type} · {argDef.Role}" : argDef.Type) : LOC.Get("ESD_Properties_Arg_Undocumented");
        if (!string.IsNullOrEmpty(argDef?.Comment))
            tip += $"\n{argDef.Comment}";
        GUI.Tooltip(tip);

        if (argDef != null)
        {
            ImGui.SameLine();
            ImGui.TextDisabled(argDef.Role != null ? $"{argDef.Type} · {argDef.Role}" : argDef.Type);
            if (argDef.Optional)
            {
                ImGui.SameLine();
                ImGui.TextDisabled(LOC.Get("ESD_Properties_Optional"));
            }
        }

        // One value (or the bytes don't decode): a typed box, unless switched to text
        var lits = root != null ? EsdEdits.Literals(root).ToList() : new();
        var single = root != null && EsdExpr.Strip(root).IsLiteral && lits.Count == 1;
        var asText = !single || ArgsAsText.Contains(i);

        // Right side: text / value toggle, remove
        var buttons = 0;
        var removable = i == c.Arguments.Count - 1 && (argDef == null || argDef.Optional);
        if (single) buttons++;
        if (removable) buttons++;
        if (buttons > 0)
        {
            var w = ImGui.GetFrameHeight() + ImGui.GetStyle().ItemSpacing.X;
            ImGui.SameLine();
            ImGui.SetCursorPosX(Math.Max(ImGui.GetCursorPosX(), ImGui.GetCursorPosX() + Avail - w * buttons));
            if (single)
            {
                if (ImGui.SmallButton(asText ? $"{Icons.Pencil}##esdArgMode" : $"ƒ##esdArgMode"))
                {
                    if (asText) ArgsAsText.Remove(i);
                    else ArgsAsText.Add(i);
                }
                GUI.Tooltip(LOC.Get(asText ? "ESD_Properties_As_Value_TT" : "ESD_Properties_As_Expression_TT"));
                if (removable)
                    ImGui.SameLine();
            }
            if (removable)
            {
                if (ImGui.SmallButton($"{Icons.Times}##esdArgRemove"))
                    View.Commands.RemoveArgument(c, i);
                GUI.Tooltip(LOC.Get("ESD_Properties_Remove_Arg_TT"));
            }
        }

        void Commit(byte[] b) => View.Commands.SetArgument(c, i, b);

        if (root == null)
        {
            ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("ESD_Properties_Undecoded"));
            ImGui.TextDisabled(EsdFormat.Hex(bytes));
            return;
        }

        if (!asText)
        {
            var (lit, path) = lits[0];
            Literal("esdArgValue", bytes, lit, path, argDef, cat, Commit, Avail);
            Links("esdArgLink", bytes, lit, path, c, i, cat);
        }
        else
        {
            ExpressionBox("##esdArgText", bytes, cat, Commit);
            var inner = lits.Count > 0 && !single ? lits : null;
            if (inner != null)
            {
                ImGui.Indent();
                Values("esdArgVal", bytes, root, inner, c, i, cat, Commit);
                ImGui.Unindent();
            }
        }

        Expression(bytes, cat, argDef, "esdArgTree");
    }

    /// <summary>A call block's machine, chosen from the file's machines</summary>
    private void MachineCombo(ESD.CommandCall c)
    {
        var file = View.Selection.SelectedFile;
        ImGui.SetNextItemWidth(Avail - 32f * DPI.UIScale());
        if (file?.Esd == null)
            return;
        if (!ImGui.BeginCombo("##esdPropMachine", EsdFormat.MachineLabel(file, c.CommandID), ImGuiComboFlags.HeightLarge))
        {
            GUI.Tooltip(LOC.Get("ESD_Properties_Call_Machine_TT"));
            return;
        }

        foreach (var mid in file.Esd.StateGroups.Keys.OrderBy(k => k))
        {
            if (ImGui.Selectable($"{EsdFormat.MachineLabel(file, mid)}##esdPropMachine{mid}", mid == c.CommandID))
                View.Commands.SetCallMachine(c, mid);
            if (mid == c.CommandID)
                ImGui.SetItemDefaultFocus();
        }
        ImGui.EndCombo();
    }

    /// <summary>A game command, changed to another of the catalogue's (filtered by name)</summary>
    private void CommandCombo(ESD.CommandCall c, EsdCallDef def, EsdKindCatalog cat)
    {
        ImGui.SetNextItemWidth(Avail);
        var open = ImGui.BeginCombo("##esdPropCommand", EsdFormat.CommandName(cat, c), ImGuiComboFlags.HeightLargest);
        if (!open)
        {
            GUI.Tooltip(LOC.Get("ESD_Properties_Change_Command_TT"));
            return;
        }

        if (cat == null)
        {
            ImGui.TextDisabled(LOC.Get("ESD_States_No_Catalogue"));
            ImGui.EndCombo();
            return;
        }

        ImGui.SetNextItemWidth(-1);
        if (ImGui.IsWindowAppearing())
            ImGui.SetKeyboardFocusHere();
        ImGui.InputTextWithHint("##esdPropCmdFilter", LOC.Get("ESD_Edit_Filter_Hint"), ref CommandFilter, 128);

        foreach (var d in cat.Commands.Values.Where(d => d.Name != null).OrderBy(d => d.Name))
        {
            if (!string.IsNullOrEmpty(CommandFilter) && !d.Name.Contains(CommandFilter, StringComparison.OrdinalIgnoreCase))
                continue;

            var isCurrent = d.Bank == c.CommandBank && d.Id == c.CommandID;
            if (!d.Seen)
                ImGui.PushStyleColor(ImGuiCol.Text, ImGui.GetStyle().Colors[(int)ImGuiCol.TextDisabled]);
            var picked = ImGui.Selectable($"{d.Name}##esdPropCmd{d.Bank}_{d.Id}", isCurrent);
            if (!d.Seen)
                ImGui.PopStyleColor();

            if (ImGui.IsItemHovered())
                GUI.Tooltip($"{d.Bank}:{d.Id}  ({string.Join(", ", d.Args.Select(a => $"{a.Name}: {a.Type}"))})" + (d.Comment != null ? $"\n{d.Comment}" : "")
                    + (d.Seen ? "" : $"\n{LOC.Get("ESD_Edit_Unseen_TT")}"));
            if (isCurrent && ImGui.IsWindowAppearing())
                ImGui.SetScrollHereY();
            if (picked && !isCurrent)
                View.Commands.ChangeCommand(c, def, d);
        }

        ImGui.EndCombo();
    }

    /// <summary>
    /// An expression as text: committed when it parses (Enter or clicking away), with the parse error shown live while
    /// typing. Text that doesn't parse when the box is left is dropped, with a warning.
    /// </summary>
    private void ExpressionBox(string id, byte[] code, EsdKindCatalog cat, Action<byte[]> commit)
    {
        var node = EsdExpr.Decode(code);
        var current = node != null ? EsdExprText.Format(node, cat) : "";

        if (Input.Text(id, current, out var text, Avail))
        {
            var bytes = EsdExprText.Recompile(code, text, cat, out var error);
            if (bytes != null)
                commit(bytes);
            else
                EsdInput.Reject(LOC.Get("ESD_Properties_Text_Not_Applied", LOC.Get(error.Message)));
        }
        GUI.Tooltip(LOC.Get("ESD_Text_Hint"));

        // While typing: does it parse, and what's wrong where
        if (Input.Editing(id) is { } typing)
        {
            if (EsdExprText.Recompile(code, typing, cat, out var error) == null)
            {
                var at = Math.Clamp(error.Position, 0, typing.Length);
                GUI.WrappedTextColored(UI.Current.ImGui_Invalid_Text_Color, LOC.Get(error.Message) + "  " + LOC.Get("ESD_Text_At", at + 1));
                if (at < typing.Length)
                    ImGui.TextDisabled($"…{typing[at..Math.Min(typing.Length, at + 24)]}");
            }
            else
                ImGui.TextDisabled(LOC.Get("ESD_Properties_Text_Commit_Hint"));
        }
        // Text wider than the box: the whole of it below, wrapped
        else if (ImGui.CalcTextSize(current).X > Avail)
            GUI.WrappedText(current);
    }

    /// <summary>The literals of an expression, each with what it sits in and its typed box</summary>
    private void Values(string id, byte[] code, EsdExprNode root, List<(EsdExprNode Node, List<int> Path)> lits, ESD.CommandCall cmd, int arg,
        EsdKindCatalog cat, Action<byte[]> commit)
    {
        if (!ImGui.BeginTable($"##{id}", 2, ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingStretchProp))
            return;
        ImGui.TableSetupColumn("where", ImGuiTableColumnFlags.WidthStretch, 0.45f);
        ImGui.TableSetupColumn("value", ImGuiTableColumnFlags.WidthStretch, 0.55f);

        for (var k = 0; k < lits.Count; k++)
        {
            var (lit, path) = lits[k];
            var (label, argDef) = LiteralContext(root, path, cat);

            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);
            ImGui.AlignTextToFramePadding();
            ImGui.TextDisabled(Shorten(label, 48));
            GUI.Tooltip(label + (argDef?.Comment is { } note ? $"\n{note}" : ""));

            ImGui.TableSetColumnIndex(1);
            ImGui.PushID(k);
            Literal("lit", code, lit, path, argDef, cat, commit, -1);
            Links("link", code, lit, path, cmd, arg, cat);
            ImGui.PopID();
        }

        ImGui.EndTable();
    }

    /// <summary>
    /// Where a literal sits, for its label: the function arg it is (with that arg's def, for its enum), the comparison
    /// it is in (with the compared function's return enum), or a register store
    /// </summary>
    private static (string Label, EsdArgDef Def) LiteralContext(EsdExprNode root, List<int> path, EsdKindCatalog cat)
    {
        for (var at = path.Count; at > 0; at--)
        {
            var parent = EsdEdits.At(root, path.Take(at - 1).ToList());
            if (parent == null)
                break;
            if (parent.Kind == EsdExprKind.Unary && parent.Op is 0xA6 or 0xB7)
                continue;

            var child = path[at - 1];
            switch (parent.Kind)
            {
                case EsdExprKind.Call:
                    {
                        var def = parent.FunctionId is long f ? cat?.Function(f) : null;
                        var ai = child - 1;
                        var argDef = def != null && ai < def.Args.Count ? def.Args[ai] : null;
                        var fn = parent.FunctionId is long ff ? EsdFormat.FunctionName(cat, ff) : "call";
                        return ($"{fn} › {argDef?.Name ?? $"arg{ai}"}", argDef);
                    }
                case EsdExprKind.Binary:
                    {
                        var other = EsdExpr.Strip(parent.Children[1 - child]);
                        var op = EsdExpr.BinaryName(parent.Op);
                        var otherText = Shorten(EsdFormat.Text(cat, other), 32);
                        EsdArgDef argDef = null;
                        if (other.Kind == EsdExprKind.Call && other.FunctionId is long f && cat?.Function(f) is { ReturnEnum: not null } fd)
                            argDef = new EsdArgDef { Name = fd.ReturnName, Type = fd.ReturnType, Enum = fd.ReturnEnum };
                        return (child == 1 ? $"{otherText} {op} …" : $"… {op} {otherText}", argDef);
                    }
                case EsdExprKind.Unary when parent.Op is >= 0xA7 and <= 0xAE:
                    return ($"r{parent.Op - 0xA7} =", null);
                default:
                    return (Shorten(EsdFormat.Text(cat, parent), 32), null);
            }
        }
        return (LOC.Get("ESD_Edit_Value"), null);
    }

    /// <summary>
    /// The typed box for one literal: an enum's combo (with a number box for values it doesn't name), a bool's
    /// checkbox, or a number or text box
    /// </summary>
    private void Literal(string id, byte[] code, EsdExprNode lit, List<int> path, EsdArgDef argDef, EsdKindCatalog cat, Action<byte[]> commit, float width)
    {
        if (width < 0)
            width = Avail;

        void Set(long? i = null, double? f = null, string s = null)
        {
            var updated = EsdEdits.SetLiteral(code, path, i, f, s);
            if (updated != null)
                commit(updated);
        }

        switch (lit.Kind)
        {
            case EsdExprKind.Int:
                {
                    var e = argDef?.Enum != null ? cat?.FindEnum(argDef.Enum) : null;
                    if (e != null && e.Values.Count > 0)
                    {
                        var spacing = ImGui.GetStyle().ItemSpacing.X;
                        var comboW = width * 0.65f;
                        if (EsdInput.Enum($"##{id}Enum", e, lit.IntValue, out var ev, comboW))
                            Set(i: ev);
                        GUI.Tooltip(argDef.Enum);
                        ImGui.SameLine();
                        if (Input.Int($"##{id}Int", lit.IntValue, out var iv, width - comboW - spacing))
                            Set(i: iv);
                        GUI.Tooltip(LOC.Get("ESD_Edit_Int_Hint"));
                    }
                    else if (argDef?.Type == "bool" && lit.IntValue is 0 or 1)
                    {
                        var b = lit.IntValue != 0;
                        if (ImGui.Checkbox($"##{id}Bool", ref b))
                            Set(i: b ? 1 : 0);
                        ImGui.SameLine();
                        ImGui.TextDisabled(b ? "true (1)" : "false (0)");
                    }
                    else
                    {
                        if (Input.Int($"##{id}Int", lit.IntValue, out var iv, width))
                            Set(i: iv);
                        GUI.Tooltip(LOC.Get("ESD_Edit_Int_Hint"));
                    }
                    break;
                }
            case EsdExprKind.Float or EsdExprKind.Double:
                if (Input.Number($"##{id}Num", lit.FloatValue, out var d, width))
                    Set(f: d);
                GUI.Tooltip(LOC.Get(lit.Kind == EsdExprKind.Float ? "ESD_Properties_Float_TT" : "ESD_Properties_Double_TT"));
                break;
            case EsdExprKind.String:
                if (Input.Text($"##{id}Str", lit.StringValue ?? "", out var s, width))
                    Set(s: s);
                GUI.Tooltip(LOC.Get("ESD_Edit_String_Hint"));
                break;
        }
    }

    /// <summary>What an int literal refers to (a flag, param row, text, entity or machine) and its link buttons</summary>
    private void Links(string id, byte[] code, EsdExprNode lit, List<int> path, ESD.CommandCall cmd, int arg, EsdKindCatalog cat)
    {
        if (lit.Kind != EsdExprKind.Int)
            return;
        var links = View.Links;
        var (role, def, sel) = links.RoleAt(code, path, cmd, arg, cat);
        if (role.Kind == EsdRoleKind.None || links.Describe(role, lit.IntValue, def, sel, cat) is not { } about)
            return;
        ImGui.TextColored(UI.Current.ImGui_AliasName_Text, $"{Icons.Link} {about}");
        links.Buttons(id, role, lit.IntValue, def, sel, cat);
    }

    private static string Shorten(string text, int max)
    {
        var oneLine = text.Replace("\r", "").Replace('\n', ' ');
        return oneLine.Length > max ? oneLine[..(max - 1)] + "…" : oneLine;
    }

    /// <summary>An expression's tree (each node with its opcode) and its bytes, folded away by default</summary>
    private void Expression(byte[] code, EsdKindCatalog cat, EsdArgDef argDef, string id)
    {
        var node = EsdExpr.Decode(code);

        if (ImGui.TreeNodeEx($"{LOC.Get("ESD_Properties_Bytecode")}##{id}", ImGuiTreeNodeFlags.None))
        {
            ImGui.TextDisabled(EsdFormat.Hex(code));
            if (node == null)
                ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("ESD_Properties_Undecoded"));
            else
                Tree(node, cat, 0);
            ImGui.TreePop();
        }
    }

    private void Tree(EsdExprNode n, EsdKindCatalog cat, int depth)
    {
        var pad = new string(' ', depth * 2);
        var text = n.Kind switch
        {
            EsdExprKind.Call => n.FunctionId is long f ? $"{EsdFormat.FunctionName(cat, f)} (f{f}, {n.CallArgCount} args)" : $"call ({n.CallArgCount} args)",
            EsdExprKind.Binary => EsdExpr.BinaryName(n.Op),
            EsdExprKind.Unary => n.Op switch
            {
                0xA6 => LOC.Get("ESD_Properties_Op_A6"),
                0xB7 => LOC.Get("ESD_Properties_Op_B7"),
                >= 0xA7 and <= 0xAE => $"r{n.Op - 0xA7} =",
                0xB8 => "arg",
                _ => $"op{n.Op:X2}"
            },
            _ => EsdExpr.Text(n)
        };

        ImGui.TextDisabled($"{pad}{n.Op:X2}");
        ImGui.SameLine();
        ImGui.Text(text);

        var children = n.Kind == EsdExprKind.Call ? n.Children.Skip(1) : n.Children;
        foreach (var c in children)
            Tree(c, cat, depth + 1);
    }

    private static void Row(string label, string value)
    {
        ImGui.TextDisabled(label);
        ImGui.SameLine();
        ImGui.Text(value);
    }
}
