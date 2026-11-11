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

    private void DrawCondition(ESD.Condition c)
    {
        var cat = Catalog;

        Row(LOC.Get("ESD_Properties_Target"), c.TargetState is long t ? LOC.Get("ESD_States_State_Title", t) : LOC.Get("ESD_Properties_Target_None"));
        if (c.TargetState is long target && ImGui.IsItemClicked())
            View.Selection.GoToState(target);

        Row(LOC.Get("ESD_Section_Pass"), c.PassCommands.Count.ToString());
        Row(LOC.Get("ESD_Properties_Subconditions"), c.Subconditions.Count.ToString());

        ImGui.Separator();
        ImGui.TextDisabled(LOC.Get("ESD_Properties_Test"));
        GUI.WrappedText(EsdExpr.IsAlways(c.Evaluator) ? LOC.Get("ESD_States_Always_TT") : EsdFormat.ExprText(cat, c.Evaluator));

        Expression(c.Evaluator, cat, null, "esdCondTree");
    }

    private void DrawCommand(ESD.CommandCall c)
    {
        var cat = Catalog;
        var kind = EsdLayout.KindOf(c);
        var def = kind == EsdCommandKind.Game ? cat?.Command(c.CommandBank, c.CommandID) : null;

        Row(LOC.Get("ESD_Properties_Command"), $"{c.CommandBank}:{c.CommandID}");
        Row(LOC.Get("ESD_Properties_Name"), EsdFormat.CommandName(cat, c));

        if (kind == EsdCommandKind.CallMachine)
        {
            ImGui.SameLine();
            if (ImGui.SmallButton($"{Icons.ArrowRight}##esdGoCall"))
                View.Selection.GoToMachine(c.CommandID);
            GUI.Tooltip(LOC.Get("ESD_Properties_Go_To_Machine_TT"));
        }

        if (!string.IsNullOrEmpty(def?.Comment))
            GUI.WrappedTextColored(UI.Current.ImGui_AliasName_Text, def.Comment);

        if (def != null && (c.Arguments.Count < def.RequiredArgs || c.Arguments.Count > def.Args.Count))
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("ESD_States_Arg_Count_TT", c.Arguments.Count, def.RequiredArgs, def.Args.Count));

        ImGui.Separator();

        for (var i = 0; i < c.Arguments.Count; i++)
        {
            var argDef = def != null && i < def.Args.Count ? def.Args[i] : null;
            var node = EsdExpr.Decode(c.Arguments[i]);

            var label = argDef != null ? $"{argDef.Name}" : $"arg{i}";
            ImGui.Text(label);
            if (argDef != null)
            {
                ImGui.SameLine();
                ImGui.TextDisabled(argDef.Role != null ? $"{argDef.Type} · {argDef.Role}" : argDef.Type);
                if (!string.IsNullOrEmpty(argDef.Comment))
                    GUI.Tooltip(argDef.Comment);
            }

            ImGui.Indent();
            GUI.WrappedText(node != null ? EsdFormat.ArgText(cat, def, i, node) : $"<{EsdFormat.Hex(c.Arguments[i])}>");
            Expression(c.Arguments[i], cat, argDef, $"esdArgTree{i}");
            ImGui.Unindent();
        }
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
