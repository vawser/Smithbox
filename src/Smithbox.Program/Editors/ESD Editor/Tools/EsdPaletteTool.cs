using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Editors.Common;
using System.Numerics;

namespace StudioCore.Editors.EsdEditor;

/// <summary>
/// Palette (P4): commands, functions, operators and values to drag into the States window.
/// - A command dropped on a command goes before it; on a state, at the end of its entry list; on a transition, at the
///   end of its pass list. Double-click inserts after the selected block.
/// - A function, operator or value dropped on a value chip replaces it (an operator keeps the value on its left). On
///   a transition's block: a constant test is replaced; any other test gets "&amp;&amp; function" or is wrapped in the operator.
/// </summary>
public class EsdPaletteTool
{
    public const string CommandPayload = "ESD_PALETTE_CMD";
    public const string ExprPayload = "ESD_PALETTE_EXPR";

    private readonly EsdEditorView View;

    private string Filter = "";
    private bool ExactFilter = false;

    /// <summary>The command being dragged (made fresh for each drop)</summary>
    public Func<ESD.CommandCall> DragCommand;

    /// <summary>The expression being dragged: a node to put in place, or an operator to wrap with</summary>
    public Func<EsdExprNode> DragExpr;
    public byte? DragWrapOp;
    public string DragLabel;
    private int DragFrame = -10;

    public EsdPaletteTool(EsdEditorView view)
    {
        View = view;
    }

    /// <summary>An expression is being dragged from the palette (this frame or the last: the drop frame)</summary>
    public bool DraggingExpr => (DragExpr != null || DragWrapOp != null) && ImGui.GetFrameCount() - DragFrame <= 1;

    public void EndExprDrag()
    {
        DragExpr = null;
        DragWrapOp = null;
        DragFrame = -10;
    }

    private EsdKindCatalog Catalog => View.Selection.SelectedFile is { } f ? View.Project.Handler.EsdData?.Catalog?.For(f.Kind) : null;

    public void OnToolWindow()
    {
        if (!ToolSections.Header($"{LOC.Get("ESD_PaletteTool_Header")}##esdPaletteToolHeader"))
            return;

        ImGui.BeginChild("EsdPaletteToolSection", ImGuiChildFlags.Borders);

        Display();

        ImGui.EndChild();
    }

    private void Display()
    {
        GUI.WrappedText(LOC.Get("ESD_PaletteTool_Hint"));

        var file = View.Selection.SelectedFile;
        if (file?.Esd == null)
        {
            GUI.WrappedText(LOC.Get("ESD_PaletteTool_No_File"));
            return;
        }

        var cat = Catalog;

        // Filter
        GUI.Spacer();
        EditorFilters.DisplayFramedListFilter("esdPaletteFilter", ref Filter, ref ExactFilter);

        ImGui.BeginChild("esdPaletteList", Vector2.Zero, ImGuiChildFlags.Borders);

        // Commands
        if (Group("ESD_PaletteTool_Commands"))
        {
            CommandItem(LOC.Get("ESD_Command_Return"), "7:-1", () => new ESD.CommandCall(7, -1, EsdEdits.DefaultArg("int")), LOC.Get("ESD_States_Return_TT"));

            if (ImGui.TreeNodeEx($"{LOC.Get("ESD_Edit_Call_Machine")}##esdPaletteCalls", Filter.Length > 0 ? ImGuiTreeNodeFlags.DefaultOpen : 0))
            {
                foreach (var mid in file.Esd.StateGroups.Keys.OrderBy(k => k))
                {
                    var label = LOC.Get("ESD_Command_Call", EsdFormat.MachineLabel(file, mid));
                    var callee = file.Esd.StateGroups[mid];
                    CommandItem(label, $"6:{mid}", () => new ESD.CommandCall(6, (int)mid,
                        Enumerable.Range(0, EsdStructure.ArgCount(callee)).Select(_ => EsdEdits.DefaultArg("int")).ToArray()), LOC.Get("ESD_States_Call_TT"));
                }
                ImGui.TreePop();
            }

            if (cat == null)
                ImGui.TextDisabled(LOC.Get("ESD_States_No_Catalogue"));
            else
                foreach (var def in cat.Commands.Values.Where(d => d.Name != null).OrderBy(d => d.Name))
                {
                    var d = def;
                    CommandItem(d.Name, $"{d.Bank}:{d.Id}", () => EsdEditCommands.NewCommand(d),
                        $"{d.Bank}:{d.Id}  ({string.Join(", ", d.Args.Select(a => $"{a.Name}: {a.Type}"))})" + (d.Comment != null ? $"\n{d.Comment}" : ""));
                }

            ImGui.TreePop();
        }

        // Functions
        if (Group("ESD_PaletteTool_Functions"))
        {
            if (cat == null)
                ImGui.TextDisabled(LOC.Get("ESD_States_No_Catalogue"));
            else
                foreach (var def in cat.Functions.Values.Where(d => d.Name != null).OrderBy(d => d.Name))
                {
                    var d = def;
                    var text = EsdExprText.FunctionName(cat, d.Id);
                    ExprItem(d.Name, $"f{d.Id}", () => EsdExprText.NewCall(d, d.Id), null,
                        $"{text}({string.Join(", ", d.Args.Select(a => $"{a.Name}: {a.Type}"))})" + (d.ReturnType != null ? $" → {d.ReturnType}" : "") + (d.Comment != null ? $"\n{d.Comment}" : ""));
                }
            ImGui.TreePop();
        }

        // Operators
        if (Group("ESD_PaletteTool_Operators"))
        {
            foreach (byte op in new byte[] { 0x94, 0x95, 0x92, 0x90, 0x93, 0x91, 0x98, 0x99, 0x8C, 0x8D, 0x8E, 0x8F })
                ExprItem($"x {EsdExpr.BinaryName(op)} {EsdExprText.WrapDefault(op)}", "", null, op, LOC.Get("ESD_PaletteTool_Operator_TT"));
            ImGui.TreePop();
        }

        // Values
        if (Group("ESD_PaletteTool_Values"))
        {
            ExprItem("0", "int", () => EsdExprNode.Integer(0), null, LOC.Get("ESD_PaletteTool_Value_TT"));
            ExprItem("0.0f", "float", () => EsdExpr.Decode(EsdEdits.DefaultArg("float")), null, LOC.Get("ESD_PaletteTool_Value_TT"));
            ExprItem("\"\"", "string", () => EsdExpr.Decode(EsdEdits.DefaultArg("string")), null, LOC.Get("ESD_PaletteTool_Value_TT"));
            var args = View.Selection.Machine is { } m ? Math.Max(1, EsdStructure.ArgCount(m)) : 1;
            for (var i = 0; i < args; i++)
            {
                var a = i;
                ExprItem($"arg{a}", "B8", () => new EsdExprNode { Kind = EsdExprKind.Unary, Op = 0xB8, Children = { EsdExprNode.Integer(a) } }, null, LOC.Get("ESD_PaletteTool_Arg_TT"));
            }
            ExprItem("CallStatus", "B9", () => new EsdExprNode { Kind = EsdExprKind.Leaf, Op = 0xB9 }, null, LOC.Get("ESD_PaletteTool_Status_TT"));
            ImGui.TreePop();
        }

        ImGui.EndChild();
    }

    private bool Group(string key)
    {
        if (Filter.Length > 0)
            ImGui.SetNextItemOpen(true, ImGuiCond.Always);
        return ImGui.TreeNodeEx($"{LOC.Get(key)}##{key}", ImGuiTreeNodeFlags.SpanAvailWidth);
    }

    private bool Visible(string label, string key) => Filter.Length == 0 || EditorFilters.IsMatch(Filter, label, ExactFilter, key);

    private unsafe void CommandItem(string label, string key, Func<ESD.CommandCall> make, string tooltip)
    {
        if (!Visible(label, key))
            return;

        ImGui.Selectable($"{label}##esdPaletteCmd{key}", false, ImGuiSelectableFlags.AllowDoubleClick);

        if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
            View.Commands.InsertAtSelection(make());

        if (ImGui.BeginDragDropSource())
        {
            DragCommand = make;
            byte dummy = 0;
            ImGui.SetDragDropPayload(CommandPayload, &dummy, 1);
            ImGui.Text(label);
            ImGui.EndDragDropSource();
        }

        GUI.Tooltip(tooltip);
        if (key.Length > 0)
        {
            ImGui.SameLine();
            ImGui.TextDisabled(key);
        }
    }

    private unsafe void ExprItem(string label, string key, Func<EsdExprNode> make, byte? wrapOp, string tooltip)
    {
        if (!Visible(label, key))
            return;

        ImGui.Selectable($"{label}##esdPaletteExpr{label}{key}");

        if (ImGui.BeginDragDropSource())
        {
            DragExpr = make;
            DragWrapOp = wrapOp;
            DragLabel = label;
            DragFrame = ImGui.GetFrameCount();
            byte dummy = 0;
            ImGui.SetDragDropPayload(ExprPayload, &dummy, 1);
            ImGui.Text(label);
            ImGui.EndDragDropSource();
        }

        GUI.Tooltip(tooltip);
        if (key.Length > 0)
        {
            ImGui.SameLine();
            ImGui.TextDisabled(key);
        }
    }
}
