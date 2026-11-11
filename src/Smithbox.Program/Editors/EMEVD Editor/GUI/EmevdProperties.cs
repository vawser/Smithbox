using Hexa.NET.ImGui;
using SoulsFormats;
using System.Numerics;

namespace StudioCore.Editors.EmevdEditor;

/// <summary>
/// The selected instruction's layout and args, or the selected event's header and parameters. Read-only in P1.
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
            Row(LOC.Get("EMEVD_Properties_Event_Rest"), EmevdFormat.RestBehavior(ev.RestBehavior));
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

            if (ImGui.BeginTable("emevdArgs", 4, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.Resizable))
            {
                ImGui.TableSetupColumn(LOC.Get("EMEVD_Properties_Col_Arg"));
                ImGui.TableSetupColumn(LOC.Get("EMEVD_Properties_Col_Type"));
                ImGui.TableSetupColumn(LOC.Get("EMEVD_Properties_Col_Value"));
                ImGui.TableSetupColumn(LOC.Get("EMEVD_Properties_Col_Meaning"));
                ImGui.TableHeadersRow();

                foreach (var arg in line.Decoded.Args)
                    DrawArg(line, arg);

                ImGui.EndTable();
            }
        }

        GUI.Spacer();
        ImGui.TextDisabled(LOC.Get("EMEVD_Properties_Raw"));
        ImGui.TextWrapped(EmevdFormat.Hex(line.Instr.ArgData));
    }

    private void DrawArg(EmevdLine line, EmevdArg arg)
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

        ImGui.TableNextColumn();
        if (line.Params.TryGetValue(arg.Offset, out var p))
        {
            ImGui.TextColored(EmevdFormat.ParamChipColor, EmevdScript.SlotName(p.SourceStartByte, p.ByteCount));
            GUI.Tooltip(LOC.Get("EMEVD_Properties_Arg_Param_TT", EmevdFormat.RawValue(arg)));
        }
        else
        {
            ImGui.Text(EmevdFormat.RawValue(arg));
        }

        ImGui.TableNextColumn();
        var e = Catalog?.FindEnum(arg.Def.EnumKey);
        if (e != null)
        {
            var name = e.ValueName(arg.Int);
            if (name != null)
                ImGui.Text(name);
            else
                ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("EMEVD_Properties_Enum_Unknown"));

            GUI.Tooltip(e.DisplayName);
        }
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
