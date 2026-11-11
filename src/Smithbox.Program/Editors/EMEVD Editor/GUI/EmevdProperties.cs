using Hexa.NET.ImGui;
using SoulsFormats;
using System.Numerics;

namespace StudioCore.Editors.EmevdEditor;

/// <summary>
/// The selected instruction's layout and args, or the selected event's header and parameters. Arg values, their
/// parameter slots, the rest behaviour and undecoded instructions' bytes can be edited (P2).
/// </summary>
public class EmevdProperties
{
    private readonly EmevdEditorView View;

    public EmevdProperties(EmevdEditorView view)
    {
        View = view;
    }

    private EmevdCatalog Catalog => View.Project.Handler.EmevdData?.Catalog;

    public void Draw()
    {
        GUI.TitleHeader(LOC.Get("EMEVD_Properties_Header"), LOC.Get("EMEVD_Properties_Header_TT"));

        ImGui.BeginChild("emevdProperties", Vector2.Zero, ImGuiChildFlags.Borders);

        var selection = View.Selection;
        var script = View.BlockView.CurrentScript;

        if (selection.SelectedEvent == null || script == null || script.Event != selection.SelectedEvent)
        {
            ImGui.TextDisabled(LOC.Get("EMEVD_Properties_Nothing"));
        }
        else if (selection.InstructionIndex < 0)
        {
            DrawEvent(script);
        }
        else
        {
            DrawInstruction(script, script.Lines[selection.InstructionIndex]);
        }

        ImGui.EndChild();
    }

    private void DrawEvent(EmevdScript script)
    {
        var ev = script.Event;

        if (ImGui.BeginTable("emevdEventProps", 2, ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.RowBg))
        {
            ImGui.TableSetupColumn("name", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("value", ImGuiTableColumnFlags.WidthStretch);

            Row(LOC.Get("EMEVD_Properties_Event_ID"), ev.ID.ToString());
            Row(LOC.Get("EMEVD_Properties_Event_Name"), string.IsNullOrEmpty(ev.Name) ? "-" : ev.Name);
            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.TextDisabled(LOC.Get("EMEVD_Properties_Event_Rest"));
            ImGui.TableNextColumn();
            DrawRest(ev);
            Row(LOC.Get("EMEVD_Properties_Event_Instructions"), ev.Instructions.Count.ToString());
            Row(LOC.Get("EMEVD_Properties_Event_Parameters"), ev.Parameters.Count.ToString());

            ImGui.EndTable();
        }

        if (ev.Parameters.Count == 0)
            return;

        GUI.Spacer();
        ImGui.Text(LOC.Get("EMEVD_Properties_Parameter_Table"));
        GUI.Tooltip(LOC.Get("EMEVD_Properties_Parameter_Table_TT"));

        if (ImGui.BeginTable("emevdEventParams", 4, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg))
        {
            ImGui.TableSetupColumn(LOC.Get("EMEVD_Properties_Col_Slot"));
            ImGui.TableSetupColumn(LOC.Get("EMEVD_Properties_Col_Line"));
            ImGui.TableSetupColumn(LOC.Get("EMEVD_Properties_Col_Target"));
            ImGui.TableSetupColumn(LOC.Get("EMEVD_Properties_Col_Arg"));
            ImGui.TableHeadersRow();

            for (var i = 0; i < ev.Parameters.Count; i++)
            {
                var p = ev.Parameters[i];
                var line = p.InstructionIndex >= 0 && p.InstructionIndex < script.Lines.Count ? script.Lines[(int)p.InstructionIndex] : null;
                var arg = line?.Decoded.Args.FirstOrDefault(a => a.Offset == p.TargetStartByte);

                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.TextColored(EmevdFormat.ParamChipColor, EmevdScript.SlotName(p.SourceStartByte, p.ByteCount));

                ImGui.TableNextColumn();
                if (line != null)
                {
                    if (ImGui.Selectable($"{p.InstructionIndex} {line.Name}##emevdParamLine{i}"))
                    {
                        View.Selection.SelectInstruction(line.Index);
                        View.Selection.ScrollToInstruction = true;
                    }
                }
                else
                {
                    ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, p.InstructionIndex.ToString());
                }

                ImGui.TableNextColumn();
                ImGui.Text($"+{p.TargetStartByte:X2}");

                ImGui.TableNextColumn();
                if (arg?.Def != null)
                    ImGui.Text(EmevdFormat.ArgName(arg.Value));
                else
                    ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("EMEVD_Properties_Param_No_Arg"));
            }

            ImGui.EndTable();
        }
    }

    private void DrawRest(EMEVD.Event ev)
    {
        ImGui.SetNextItemWidth(-1);
        if (!ImGui.BeginCombo("##emevdRest", EmevdFormat.RestBehavior(ev.RestBehavior)))
        {
            GUI.Tooltip(LOC.Get("EMEVD_Edit_Rest_TT"));
            return;
        }

        foreach (var rest in Enum.GetValues<EMEVD.Event.RestBehaviorType>())
        {
            if (ImGui.Selectable(EmevdFormat.RestBehavior(rest), rest == ev.RestBehavior) && rest != ev.RestBehavior)
                View.Edit(new EmevdValueChange<EMEVD.Event.RestBehaviorType>(View.Selection.SelectedFile, () => ev.RestBehavior, v => ev.RestBehavior = v, rest));
        }

        ImGui.EndCombo();
    }

    private void DrawInstruction(EmevdScript script, EmevdLine line)
    {
        var def = line.Decoded.Def;

        if (ImGui.BeginTable("emevdInstrProps", 2, ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.RowBg))
        {
            ImGui.TableSetupColumn("name", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("value", ImGuiTableColumnFlags.WidthStretch);

            Row(LOC.Get("EMEVD_Properties_Line"), line.Index.ToString());
            Row(LOC.Get("EMEVD_Properties_Key"), line.Key);

            if (def != null)
            {
                Row(LOC.Get("EMEVD_Properties_Name"), def.HasName ? def.Name : LOC.Get("EMEVD_Properties_Unnamed"));
                if (!string.IsNullOrEmpty(def.JapaneseName))
                    Row(LOC.Get("EMEVD_Properties_Japanese_Name"), def.JapaneseName);
                Row(LOC.Get("EMEVD_Properties_Class"), def.Class.DisplayName);
                Row(LOC.Get("EMEVD_Properties_Confidence"), def.Confidence, LOC.Get($"EMEVD_Confidence_{def.Confidence}_TT"));
                if (!string.IsNullOrEmpty(def.Source))
                    Row(LOC.Get("EMEVD_Properties_Source"), def.Source);
                if (!string.IsNullOrEmpty(def.Exe))
                    Row(LOC.Get("EMEVD_Properties_Exe"), def.Exe);
                if (!string.IsNullOrEmpty(def.NameSource))
                    Row(LOC.Get("EMEVD_Properties_Name_Source"), def.NameSource);
                Row(LOC.Get("EMEVD_Properties_Uses"), def.Uses.ToString());
            }
            else
            {
                Row(LOC.Get("EMEVD_Properties_Name"), LOC.Get("EMEVD_Properties_No_Layout"));
            }

            if (line.Instr.Layer != null)
                Row(LOC.Get("EMEVD_Properties_Layer"), $"0x{line.Instr.Layer.Value:X8}");

            ImGui.EndTable();
        }

        if (!string.IsNullOrEmpty(def?.Warnings))
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, def.Warnings);

        if (!line.Decoded.Fits && def != null)
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, line.Decoded.Problem);

        if (line.Kind == EmevdBlockKind.Call && !line.CallTargetIsParam && line.CallTarget >= 0)
            DrawCall(line);

        if (line.Decoded.Fits && line.Decoded.Args.Count > 0)
        {
            GUI.Spacer();

            if (line.Kind is EmevdBlockKind.Skip or EmevdBlockKind.Goto)
                GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("EMEVD_Edit_Flow_Arg_Warning"));

            if (ImGui.BeginTable("emevdArgs", 5, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.Resizable))
            {
                ImGui.TableSetupColumn(LOC.Get("EMEVD_Properties_Col_Arg"));
                ImGui.TableSetupColumn(LOC.Get("EMEVD_Properties_Col_Type"), ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn(LOC.Get("EMEVD_Properties_Col_Value"), ImGuiTableColumnFlags.WidthStretch);
                ImGui.TableSetupColumn(LOC.Get("EMEVD_Properties_Col_Meaning"));
                ImGui.TableSetupColumn(LOC.Get("EMEVD_Properties_Col_Slot"));
                ImGui.TableHeadersRow();

                for (var a = 0; a < line.Decoded.Args.Count; a++)
                    DrawArg(script, line, line.Decoded.Args[a], a);

                ImGui.EndTable();
            }
        }

        GUI.Spacer();
        ImGui.TextDisabled(LOC.Get("EMEVD_Properties_Raw"));

        if (line.Decoded.Fits)
        {
            ImGui.TextWrapped(EmevdFormat.Hex(line.Instr.ArgData));
        }
        else if (line.Instr.ArgData?.Length > 0)
        {
            // Undecoded instructions can still be edited byte by byte, at their length
            GUI.Tooltip(LOC.Get("EMEVD_Edit_Raw_TT"));
            if (View.Input.HexBytes("##emevdRawBytes", EmevdFormat.Hex(line.Instr.ArgData), out var bytes, -1))
            {
                if (bytes.Length != line.Instr.ArgData.Length)
                    Smithbox.Log(this, LOC.Get("EMEVD_Edit_Raw_Length", line.Instr.ArgData.Length), Microsoft.Extensions.Logging.LogLevel.Warning);
                else
                    View.Edit(new EmevdArgChange(View.Selection.SelectedFile, line.Instr, 0, bytes));
            }
        }
    }

    private void DrawArg(EmevdScript script, EmevdLine line, EmevdArg arg, int index)
    {
        ImGui.TableNextRow();

        ImGui.TableNextColumn();
        ImGui.Text(EmevdFormat.ArgName(arg));
        var tt = $"+{arg.Offset:X2}";
        if (!string.IsNullOrEmpty(arg.Def.JapaneseName))
            tt += $"\n{arg.Def.JapaneseName}";
        if (arg.Def.Optional)
            tt += "\n" + LOC.Get("EMEVD_Properties_Arg_Optional_TT");
        if (arg.Def.Inferred)
            tt += "\n" + LOC.Get("EMEVD_Properties_Arg_Inferred_TT");
        GUI.Tooltip(tt);

        ImGui.TableNextColumn();
        ImGui.TextDisabled(EmevdDecoded.TypeName(arg.Type));

        // The value box: the raw number, so enum args can hold values the enum doesn't name; the combo is in Meaning
        ImGui.TableNextColumn();
        var e = Catalog?.FindEnum(arg.Def.EnumKey);
        var hasEnum = e != null && e.Values.Count > 0;
        var linked = line.Params.ContainsKey(arg.Offset);

        // Linked: the slot pill; enum: the raw number (the combo is in Meaning); otherwise the typed box
        if (hasEnum && !linked)
            View.ArgEditor.RawInt($"##argRaw{index}", line, arg, -1);
        else
            View.ArgEditor.Value($"##argValue{index}", line, arg, -1);

        ImGui.TableNextColumn();
        if (hasEnum && !linked)
        {
            View.ArgEditor.Value($"##argEnum{index}", line, arg, -1);
            GUI.Tooltip(e.DisplayName);
        }
        else if (e != null)
        {
            var name = e.ValueName(arg.Int);
            if (name != null)
                ImGui.Text(name);
            else
                ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("EMEVD_Properties_Enum_Unknown"));

            GUI.Tooltip(e.DisplayName);
        }

        ImGui.TableNextColumn();
        View.ArgEditor.Slot($"##argSlot{index}", script, line, arg, -1);
    }

    private void DrawCall(EmevdLine line)
    {
        var (file, ev) = View.BlockView.ResolveCall(line);

        GUI.Spacer();

        if (ev == null)
        {
            ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("EMEVD_Properties_Call_Missing", line.CallTarget));
            return;
        }

        if (ImGui.Button($"{Icons.LongArrowRight} {LOC.Get("EMEVD_Properties_Call_Go", file.Name, EmevdFormat.EventLabel(ev))}##emevdCallGo"))
            View.Selection.JumpTo(file, ev);
    }

    private static void Row(string name, string value, string tooltip = null)
    {
        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.TextDisabled(name);
        ImGui.TableNextColumn();
        ImGui.TextWrapped(value ?? "");

        if (tooltip != null)
            GUI.Tooltip(tooltip);
    }
}
