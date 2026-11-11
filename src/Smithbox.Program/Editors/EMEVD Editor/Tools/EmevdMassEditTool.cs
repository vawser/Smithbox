using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using System.Globalization;
using System.Numerics;

namespace StudioCore.Editors.EmevdEditor;

/// <summary>
/// Mass Edit (P6): replaces integer arg values across the selected event, the selected file or every file, e.g.
/// renumbering an event flag everywhere, or moving a range of entity IDs. Preview plans the changes on a background
/// scan (EmevdMassEdit, the same code EmevdEditorCheck's mass gate runs); each can be left out; Apply makes the rest
/// as one undo step. Skip counts, goto labels and args a parameter writes are never changed, Initialize Event
/// targets only when asked.
/// </summary>
public class EmevdMassEditTool
{
    private readonly EmevdEditorView View;
    private readonly EmevdScan Scan;

    // Find
    private string FromText = "";
    private string ToText = "";
    private int RoleIndex = 0; // 0: any; else Roles[RoleIndex - 1]
    private string ParamText = "";
    private string InstructionText = "";
    private bool IncludeCallTargets = false;

    // Change
    private EmevdMassOp Op = EmevdMassOp.Set;
    private string OperandText = "";

    private EmevdSearchScope Scope = EmevdSearchScope.File;
    private bool ShowChanges = true;

    /// <summary>Result indexes the user left out of the apply</summary>
    private readonly HashSet<int> Excluded = new();

    private static readonly EmevdArgRole[] Roles = Enum.GetValues<EmevdArgRole>();

    public EmevdMassEditTool(EmevdEditorView view)
    {
        View = view;
        Scan = new(view, "emevdMassEdit");
    }

    public void OnToolWindow()
    {
        if (!ToolSections.Header($"{LOC.Get("EMEVD_MassEditTool_Header")}##emevdMassEditToolHeader"))
            return;

        ImGui.BeginChild("EmevdMassEditToolSection", ImGuiChildFlags.Borders);

        Display();

        ImGui.EndChild();
    }

    private void Display()
    {
        GUI.WrappedText(LOC.Get("EMEVD_MassEditTool_Hint"));

        if (View.Project.Handler.EmevdData?.PrimaryBank == null)
        {
            GUI.WrappedText(LOC.Get("EMEVD_MassEditTool_No_Bank"));
            return;
        }

        // Find
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("EMEVD_MassEditTool_Find_Header"), LOC.Get("EMEVD_MassEditTool_Find_Header_TT"));

        ImGui.SetNextItemWidth(-1);
        ImGui.InputTextWithHint("##emevdMassFrom", LOC.Get("EMEVD_MassEditTool_From_Hint"), ref FromText, 32);
        GUI.Tooltip(LOC.Get("EMEVD_MassEditTool_From_TT"));

        ImGui.SetNextItemWidth(-1);
        ImGui.InputTextWithHint("##emevdMassTo", LOC.Get("EMEVD_MassEditTool_To_Hint"), ref ToText, 32);
        GUI.Tooltip(LOC.Get("EMEVD_MassEditTool_To_TT"));

        // Only
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("EMEVD_MassEditTool_Only_Header"), LOC.Get("EMEVD_MassEditTool_Only_Header_TT"));

        ImGui.SetNextItemWidth(-1);
        if (ImGui.BeginCombo("##emevdMassRole", RoleLabel(RoleIndex)))
        {
            for (var i = 0; i <= Roles.Length; i++)
            {
                if (ImGui.Selectable($"{RoleLabel(i)}##emevdMassRole{i}", i == RoleIndex))
                    RoleIndex = i;
            }
            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("EMEVD_MassEditTool_Role_TT"));

        if (SelectedRole == EmevdArgRole.Param)
        {
            ImGui.SetNextItemWidth(-1);
            ImGui.InputTextWithHint("##emevdMassParam", LOC.Get("EMEVD_MassEditTool_Param_Hint"), ref ParamText, 64);
            GUI.Tooltip(LOC.Get("EMEVD_MassEditTool_Param_TT"));
        }

        ImGui.SetNextItemWidth(-1);
        ImGui.InputTextWithHint("##emevdMassInstruction", LOC.Get("EMEVD_MassEditTool_Instruction_Hint"), ref InstructionText, 128);
        GUI.Tooltip(LOC.Get("EMEVD_MassEditTool_Instruction_TT"));

        ImGui.Checkbox($"{LOC.Get("EMEVD_MassEditTool_Call_Targets")}##emevdMassCalls", ref IncludeCallTargets);
        GUI.Tooltip(LOC.Get("EMEVD_MassEditTool_Call_Targets_TT"));

        // Change
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("EMEVD_MassEditTool_Change_Header"), LOC.Get("EMEVD_MassEditTool_Change_Header_TT"));

        ImGui.SetNextItemWidth(-1);
        if (ImGui.BeginCombo("##emevdMassOp", OpLabel(Op)))
        {
            foreach (var op in Enum.GetValues<EmevdMassOp>())
            {
                if (ImGui.Selectable($"{OpLabel(op)}##emevdMassOp{op}", op == Op))
                    Op = op;
            }
            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("EMEVD_MassEditTool_Op_TT"));

        ImGui.SetNextItemWidth(-1);
        ImGui.InputTextWithHint("##emevdMassOperand", LOC.Get(Op == EmevdMassOp.Set ? "EMEVD_MassEditTool_Set_Hint" : "EMEVD_MassEditTool_Add_Hint"), ref OperandText, 32);
        GUI.Tooltip(LOC.Get("EMEVD_MassEditTool_Operand_TT"));

        // Scope
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("EMEVD_SearchTool_Scope_Header"), LOC.Get("EMEVD_MassEditTool_Scope_Header_TT"));
        ImGui.SetNextItemWidth(-1);
        if (ImGui.BeginCombo("##emevdMassScope", LOC.Get($"EMEVD_SearchTool_Scope_{Scope}")))
        {
            foreach (var s in Enum.GetValues<EmevdSearchScope>())
            {
                if (ImGui.Selectable($"{LOC.Get($"EMEVD_SearchTool_Scope_{s}")}##emevdMassScope{s}", s == Scope))
                    Scope = s;
            }
            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("EMEVD_MassEditTool_Scope_Header_TT"));

        // Validate the inputs
        var fromOk = TryNumber(FromText, out var from);
        var toOk = TryNumber(ToText, out var to);
        var operandOk = TryNumber(OperandText, out var operand);
        var hasScope = Scope switch
        {
            EmevdSearchScope.Event => View.Selection.SelectedEvent != null,
            EmevdSearchScope.File => View.Selection.SelectedFile != null,
            _ => true
        };

        string problem = null;
        if (!fromOk || from == null)
            problem = FromText.Trim().Length == 0 ? LOC.Get("EMEVD_MassEditTool_Need_From") : LOC.Get("EMEVD_SearchTool_Value_Invalid", FromText);
        else if (!toOk)
            problem = LOC.Get("EMEVD_SearchTool_Value_Invalid", ToText);
        else if (to != null && to < from)
            problem = LOC.Get("EMEVD_MassEditTool_Bad_Range");
        else if (!operandOk || operand == null)
            problem = OperandText.Trim().Length == 0 ? LOC.Get("EMEVD_MassEditTool_Need_Operand") : LOC.Get("EMEVD_SearchTool_Value_Invalid", OperandText);
        else if (!hasScope)
            problem = LOC.Get("EMEVD_SearchTool_No_Scope");

        if (problem != null)
            GUI.WrappedText(problem);

        var changes = Changes();
        var applicable = changes.Count(c => !Excluded.Contains(c.index) && !c.change.OutOfRange);

        // Actions
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("EMEVD_MassEditTool_Actions_Header"), LOC.Get("EMEVD_MassEditTool_Actions_Header_TT"));

        GUI.ConditionalMultiButtonInput("emevdMassActions",
            "emevdMassPreview", LOC.Get("EMEVD_MassEditTool_Preview"), LOC.Get("EMEVD_MassEditTool_Preview_TT"),
            () => Preview(new EmevdMassQuery(from.Value, to ?? from.Value, SelectedRole, ParamText, InstructionText, IncludeCallTargets, Op, operand.Value)),
            problem == null,

            "emevdMassApply", LOC.Get("EMEVD_MassEditTool_Apply", applicable), LOC.Get("EMEVD_MassEditTool_Apply_TT"),
            Apply, applicable > 0 && !Scan.IsRunning,

            "emevdMassClear", LOC.Get("EMEVD_SearchTool_Clear"), LOC.Get("EMEVD_MassEditTool_Clear_TT"),
            () => { Scan.Clear(); Excluded.Clear(); }, true);

        Scan.DrawProgress();
        DrawChanges(changes);
    }

    private EmevdArgRole? SelectedRole => RoleIndex == 0 ? null : Roles[RoleIndex - 1];

    private static string RoleLabel(int index) => index == 0
        ? LOC.Get("EMEVD_MassEditTool_Role_Any")
        : LOC.Get($"EMEVD_MassEditTool_Role_{Roles[index - 1]}");

    private static string OpLabel(EmevdMassOp op) => LOC.Get($"EMEVD_MassEditTool_Op_{op}");

    /// <summary>The planned changes found so far, with their result index</summary>
    private List<(int index, EmevdMassChange change)> Changes()
    {
        Scan.Pump();

        // Results only grow during a run; a new run or a clear starts the list again
        if (Scan.Results.Count < CachedScanned || !ReferenceEquals(CachedSubject, Scan.Subject))
        {
            CachedChanges = new();
            CachedScanned = 0;
            CachedSubject = Scan.Subject;
        }

        for (; CachedScanned < Scan.Results.Count; CachedScanned++)
        {
            if (Scan.Results[CachedScanned].Tag is EmevdMassChange c)
                CachedChanges.Add((CachedScanned, c));
        }

        return CachedChanges;
    }

    private List<(int index, EmevdMassChange change)> CachedChanges = new();
    private int CachedScanned;
    private string CachedSubject;

    private void Preview(EmevdMassQuery query)
    {
        var bank = View.Project.Handler.EmevdData?.PrimaryBank;
        var catalog = View.Project.Handler.EmevdData?.Catalog;
        if (bank == null)
            return;

        Excluded.Clear();

        var selectedEvent = View.Selection.SelectedEvent;
        var files = Scope switch
        {
            EmevdSearchScope.AllFiles => bank.Files.ToList(),
            _ => View.Selection.SelectedFile != null ? new List<EmevdFileWrapper> { View.Selection.SelectedFile } : new()
        };

        var range = query.From == query.To ? $"{query.From}" : $"{query.From}–{query.To}";
        var subject = LOC.Get(query.Op == EmevdMassOp.Set ? "EMEVD_MassEditTool_Subject_Set" : "EMEVD_MassEditTool_Subject_Add", range, query.Operand);

        Scan.Start(subject, files, f =>
        {
            var events = Scope == EmevdSearchScope.Event ? new List<SoulsFormats.EMEVD.Event> { selectedEvent } : f.Emevd.Events.ToList();
            return events
                .SelectMany(ev => EmevdMassEdit.Plan(catalog, f.Name, ev, query))
                .Select(c => new EmevdHit(c.File, c.EventId, c.Line, c.What) { Tag = c })
                .ToList();
        });
    }

    /// <summary>Makes every included change that still applies, as one undo step</summary>
    private void Apply()
    {
        var bank = View.Project.Handler.EmevdData?.PrimaryBank;
        if (bank == null)
            return;

        var actions = new List<EditorAction>();
        var files = new HashSet<string>();
        var stale = 0;

        foreach (var (index, change) in Changes())
        {
            if (Excluded.Contains(index) || change.OutOfRange)
                continue;

            var file = bank.FindFile(change.File);

            // Edited since the preview: left alone rather than overwriting the newer value
            if (file == null || !EmevdMassEdit.StillCurrent(change))
            {
                stale++;
                continue;
            }

            var action = new EmevdArgChange(file, change.Instr, change.Offset, change.NewBytes);
            if (!action.HasEffect)
                continue;

            actions.Add(action);
            files.Add(change.File);
        }

        if (actions.Count > 0)
            View.ActionManager.ExecuteAction(new CompoundAction(actions));

        Smithbox.Log(this, LOC.Get("EMEVD_MassEditTool_Applied", actions.Count, files.Count));
        if (stale > 0)
            Smithbox.Log(this, LOC.Get("EMEVD_MassEditTool_Stale", stale), Microsoft.Extensions.Logging.LogLevel.Warning);

        // The plan's old values are now out of date
        Scan.Clear();
        Excluded.Clear();
    }

    private void DrawChanges(List<(int index, EmevdMassChange change)> changes)
    {
        if (!Scan.HasRun)
            return;

        GUI.Spacer();
        GUI.ConditionalHeader(LOC.Get("EMEVD_MassEditTool_Changes_Header"), LOC.Get("EMEVD_MassEditTool_Changes_Header_TT"), ref ShowChanges);

        var fileCount = changes.Select(c => c.change.File).Distinct().Count();
        var outOfRange = changes.Count(c => c.change.OutOfRange);
        GUI.WrappedText(LOC.Get(Scan.IsRunning ? "EMEVD_MassEditTool_Changes_Running" : "EMEVD_MassEditTool_Changes_Done", changes.Count, fileCount, Scan.Subject));

        if (outOfRange > 0)
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("EMEVD_MassEditTool_Out_Of_Range", outOfRange));

        if (changes.Count == 0)
            return;

        GUI.MultiButtonInput("emevdMassInclude",
            "emevdMassIncludeAll", LOC.Get("EMEVD_MassEditTool_Include_All"), LOC.Get("EMEVD_MassEditTool_Include_All_TT"), Excluded.Clear,
            "emevdMassIncludeNone", LOC.Get("EMEVD_MassEditTool_Include_None"), LOC.Get("EMEVD_MassEditTool_Include_None_TT"),
            () => { foreach (var (i, _) in changes) Excluded.Add(i); });

        if (!ShowChanges)
            return;

        ImGui.BeginChild("emevdMassChanges", Vector2.Zero, ImGuiChildFlags.Borders);

        var bank = View.Project.Handler.EmevdData?.PrimaryBank;
        var clipper = new ImGuiListClipper();
        clipper.Begin(changes.Count);

        while (clipper.Step())
        {
            for (var r = clipper.DisplayStart; r < clipper.DisplayEnd; r++)
            {
                var (index, c) = changes[r];

                var include = !Excluded.Contains(index) && !c.OutOfRange;
                ImGui.BeginDisabled(c.OutOfRange);
                if (ImGui.Checkbox($"##emevdMassInc{index}", ref include))
                {
                    if (include)
                        Excluded.Remove(index);
                    else
                        Excluded.Add(index);
                }
                ImGui.EndDisabled();
                GUI.Tooltip(LOC.Get(c.OutOfRange ? "EMEVD_MassEditTool_Row_Out_Of_Range_TT" : "EMEVD_MassEditTool_Row_Include_TT"));

                ImGui.SameLine();
                if (ImGui.Selectable($"{LOC.Get("EMEVD_Scan_Hit_Line", c.File, c.EventId, c.Line)}##emevdMassHit{index}"))
                {
                    var file = bank?.FindFile(c.File);
                    var ev = file?.FindEvent(c.EventId);
                    if (ev != null)
                        View.Selection.JumpTo(file, ev, c.Line);
                }
                GUI.Tooltip(LOC.Get("EMEVD_Scan_Hit_TT"));

                ImGui.SameLine();
                var newText = c.OutOfRange ? LOC.Get("EMEVD_MassEditTool_Doesnt_Fit", c.New, EmevdDecoded.TypeName(c.Type)) : c.New.ToString(CultureInfo.InvariantCulture);
                ImGui.TextDisabled($"{c.What}: {c.Old} {Icons.LongArrowRight} {newText}");
            }
        }

        clipper.End();
        ImGui.EndChild();
    }

    /// <summary>Empty (null), a whole number (negative allowed) or 0x hex</summary>
    private static bool TryNumber(string text, out long? value)
    {
        value = null;
        text = text.Trim();

        if (text.Length == 0)
            return true;

        if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase) && long.TryParse(text.AsSpan(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var hex))
        {
            value = hex;
            return true;
        }

        if (long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v))
        {
            value = v;
            return true;
        }

        return false;
    }
}
