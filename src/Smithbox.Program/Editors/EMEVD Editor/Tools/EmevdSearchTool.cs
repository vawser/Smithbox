using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using System.Globalization;

namespace StudioCore.Editors.EmevdEditor;

public enum EmevdSearchScope { Event, File, AllFiles }

/// <summary>
/// Search: instructions holding a value in any arg (an event flag, entity ID, param row…) and/or matching an
/// instruction (key like 2003[66], or part of its name), in the selected event, the selected file or every file. The
/// layouts don't say which args are flags or entities, so this is the "used by" for them: search the number.
/// </summary>
public class EmevdSearchTool
{
    private readonly EmevdEditorView View;
    private readonly EmevdScan Scan;

    private string ValueText = "";
    private string InstructionText = "";
    private bool MatchFloats = false;
    private EmevdSearchScope Scope = EmevdSearchScope.File;
    private bool ShowResults = true;

    public EmevdSearchTool(EmevdEditorView view)
    {
        View = view;
        Scan = new(view, "emevdSearch");
    }

    public void OnToolWindow()
    {
        if (!ToolSections.Header($"{LOC.Get("EMEVD_SearchTool_Header")}##emevdSearchToolHeader"))
            return;

        ImGui.BeginChild("EmevdSearchToolSection", ImGuiChildFlags.Borders);

        Display();

        ImGui.EndChild();
    }

    private void Display()
    {
        GUI.WrappedText(LOC.Get("EMEVD_SearchTool_Hint"));

        var search = false;

        // Value
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("EMEVD_SearchTool_Value_Header"), LOC.Get("EMEVD_SearchTool_Value_Header_TT"));
        ImGui.SetNextItemWidth(-1);
        ImGui.InputText("##emevdSearchValue", ref ValueText, 32);
        GUI.Tooltip(LOC.Get("EMEVD_SearchTool_Value_Header_TT"));
        search |= ImGui.IsItemDeactivatedAfterEdit() && ImGui.IsKeyPressed(ImGuiKey.Enter);

        ImGui.Checkbox($"{LOC.Get("EMEVD_SearchTool_Match_Floats")}##emevdSearchFloats", ref MatchFloats);
        GUI.Tooltip(LOC.Get("EMEVD_SearchTool_Match_Floats_TT"));

        // Instruction
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("EMEVD_SearchTool_Instruction_Header"), LOC.Get("EMEVD_SearchTool_Instruction_Header_TT"));
        ImGui.SetNextItemWidth(-1);
        ImGui.InputText("##emevdSearchInstruction", ref InstructionText, 128);
        GUI.Tooltip(LOC.Get("EMEVD_SearchTool_Instruction_Header_TT"));
        search |= ImGui.IsItemDeactivatedAfterEdit() && ImGui.IsKeyPressed(ImGuiKey.Enter);

        // Scope
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("EMEVD_SearchTool_Scope_Header"), LOC.Get("EMEVD_SearchTool_Scope_Header_TT"));
        ImGui.SetNextItemWidth(-1);
        if (ImGui.BeginCombo("##emevdSearchScope", ScopeLabel(Scope)))
        {
            foreach (var s in Enum.GetValues<EmevdSearchScope>())
            {
                if (ImGui.Selectable(ScopeLabel(s), s == Scope))
                    Scope = s;
            }
            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("EMEVD_SearchTool_Scope_Header_TT"));

        var valueOk = TryValue(out var value);
        var hasQuery = (value != null || InstructionText.Trim().Length > 0) && valueOk;
        var hasScope = Scope switch
        {
            EmevdSearchScope.Event => View.Selection.SelectedEvent != null,
            EmevdSearchScope.File => View.Selection.SelectedFile != null,
            _ => true
        };

        if (!valueOk)
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("EMEVD_SearchTool_Value_Invalid", ValueText));
        else if (!hasScope)
            GUI.WrappedText(LOC.Get("EMEVD_SearchTool_No_Scope"));

        // Actions
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("EMEVD_SearchTool_Actions_Header"), LOC.Get("EMEVD_SearchTool_Actions_Header_TT"));

        GUI.ConditionalMultiButtonInput("emevdSearchActions",
            "emevdSearchButton", LOC.Get("EMEVD_SearchTool_Search"), LOC.Get("EMEVD_SearchTool_Search_TT"),
            () => search = true, hasQuery && hasScope,

            "emevdSearchClear", LOC.Get("EMEVD_SearchTool_Clear"), LOC.Get("EMEVD_SearchTool_Clear_TT"),
            () => { ValueText = ""; InstructionText = ""; Scan.Clear(); }, true);

        if (search && hasQuery && hasScope)
            Run(new EmevdQuery(value, InstructionText, MatchFloats));

        Scan.DrawProgress();
        Scan.DrawResults("EMEVD_SearchTool_Results_Header", ref ShowResults);
    }

    private void Run(EmevdQuery query)
    {
        var bank = View.Project.Handler.EmevdData?.PrimaryBank;
        var catalog = View.Project.Handler.EmevdData?.Catalog;
        if (bank == null)
            return;

        var subject = query.Value != null && !string.IsNullOrWhiteSpace(query.Instruction)
            ? LOC.Get("EMEVD_SearchTool_Subject_Both", query.Value, query.Instruction.Trim())
            : query.Value != null
                ? LOC.Get("EMEVD_SearchTool_Subject_Value", query.Value)
                : LOC.Get("EMEVD_SearchTool_Subject_Instruction", query.Instruction.Trim());

        var selectedEvent = View.Selection.SelectedEvent;
        var files = Scope switch
        {
            EmevdSearchScope.AllFiles => bank.Files.ToList(),
            _ => View.Selection.SelectedFile != null ? new List<EmevdFileWrapper> { View.Selection.SelectedFile } : new()
        };

        Scan.Start(subject, files, f =>
        {
            var events = Scope == EmevdSearchScope.Event ? new[] { selectedEvent }.ToList() : f.Emevd.Events.ToList();
            return events.SelectMany(ev => EmevdEventOps.Search(catalog, f.Name, ev, query)).ToList();
        });
    }

    /// <summary>
    /// The value box: empty (no value), a whole number or 0x hex. With Match Floats, f32 args equal to the number match
    /// too (e.g. 10 finds a 10.0 duration).
    /// </summary>
    private bool TryValue(out long? value)
    {
        value = null;
        var text = ValueText.Trim();

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

    private static string ScopeLabel(EmevdSearchScope s) => LOC.Get($"EMEVD_SearchTool_Scope_{s}");
}
