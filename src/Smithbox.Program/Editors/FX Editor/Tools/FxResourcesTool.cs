using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Editors.Common;
using System.Numerics;

namespace StudioCore.Editors.FxEditor;

/// <summary>
/// Resources (FX Editor Plan §22, phase E5): the textures and models the selected effect uses, with the actions using
/// each, the file the game loads for it and the binders holding it, flagged when no binder holds it or none of the
/// effect's own binders does. Each row selects its action, or lists every effect using the resource.
/// </summary>
public class FxResourcesTool
{
    private readonly FxEditorScreen Editor;

    // Rebuilt when the effect or an edit changes it
    private (FxEntry, object, int) BuiltFor;
    private List<Row> Rows = new();
    private int Edits;

    private sealed class Row
    {
        public string Kind;
        public int Id;
        public string Type;
        public string File;
        public List<FxBinderRef> Binders;
        public bool InOwnBinder;
        public List<FxrAction> Actions = new();
        public List<string> ActionNames = new();
    }

    public FxResourcesTool(FxEditorScreen editor)
    {
        Editor = editor;
        editor.ActionManager.ActionPerformed += () => Edits++;
    }

    public void OnToolWindow()
    {
        if (!ToolSections.Header($"{LOC.Get("FX_ResourcesTool_Header")}##fxResourcesToolHeader"))
            return;

        ImGui.BeginChild("FxResourcesToolSection", ImGuiChildFlags.Borders);
        Display();
        ImGui.EndChild();
    }

    private void Display()
    {
        GUI.WrappedText(LOC.Get("FX_ResourcesTool_Hint"));

        var entry = Editor.Selection.Entry;
        if (entry?.Fxr == null)
        {
            GUI.WrappedText(LOC.Get("FX_ReferencesTool_No_Effect"));
            return;
        }

        Rebuild(entry);

        // Textures and models
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("FX_ResourcesTool_List_Header"), LOC.Get("FX_ResourcesTool_List_Header_TT"));

        if (Rows.Count == 0)
        {
            ImGui.TextDisabled(LOC.Get("FX_EffectList_None"));
            return;
        }

        int missing = Rows.Count(r => r.File == null);
        int elsewhere = Rows.Count(r => r.File != null && !r.InOwnBinder);
        GUI.WrappedText(LOC.Get("FX_ResourcesTool_Summary", Rows.Count, missing, elsewhere));

        for (int i = 0; i < Rows.Count; i++)
        {
            var r = Rows[i];
            ImGui.PushID($"fxResource{i}");

            var label = $"{LOC.Get($"FX_ResourcesTool_Kind_{r.Kind}")}  s{r.Id:D5}{(r.Type != null ? $"_{r.Type}" : "")}";
            if (r.File == null)
                GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, $"{label}  {LOC.Get("FX_ResourcesTool_Missing")}");
            else if (!r.InOwnBinder)
                GUI.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, $"{label}  {LOC.Get("FX_ResourcesTool_Elsewhere")}");
            else
                GUI.WrappedText(label);
            if (ImGui.IsItemHovered() && r.Binders is { Count: > 0 })
                ImGui.SetTooltip(string.Join("\n", r.Binders.Select(b => b.Binder.Path)));

            ImGui.Indent();
            ImGui.TextDisabled(string.Join(", ", r.ActionNames.Distinct()));
            if (r.File != null)
                ImGui.TextDisabled(LOC.Get("FX_ResourcesTool_File", r.File, r.Binders.Count));

            if (ImGui.Button($"{LOC.Get("FX_ResourcesTool_Select_Action")}##select"))
                Editor.Selection.SelectItem(r.Actions[0]);
            GUI.Tooltip(LOC.Get("FX_ResourcesTool_Select_Action_TT"));
            ImGui.SameLine();
            if (ImGui.Button($"{LOC.Get("FX_ResourcesTool_Users")}##users"))
                Editor.EffectList.ShowUsesResource(r.Id.ToString());
            GUI.Tooltip(LOC.Get("FX_ResourcesTool_Users_TT"));
            ImGui.Unindent();

            ImGui.PopID();
            ImGui.Spacing();
        }
    }

    private void Rebuild(FxEntry entry)
    {
        var key = (entry, (object)entry.Fxr, Edits);
        if (key.Equals(BuiltFor))
            return;
        BuiltFor = key;

        var bank = Editor.Data.Bank;
        var rows = new Dictionary<(string, int, string), Row>();
        List<FxrResource> resources;
        try
        {
            resources = FxrResources.Collect(entry.Fxr, Editor.Data.Schema, Editor.Data.Game);
        }
        catch
        {
            resources = new();
        }

        foreach (var r in resources)
        {
            if (r.Id <= 0 || r.Kind is not ("texture" or "model"))
                continue;
            var type = r.Kind == "texture" ? r.TextureType : null;
            if (!rows.TryGetValue((r.Kind, r.Id, type), out var row))
            {
                row = new Row { Kind = r.Kind, Id = r.Id, Type = type };
                // The file the game loads: s<id>_<type> when a binder has it, else s<id> (DS3 and SDT have both)
                var plain = $"s{r.Id:D5}";
                var typed = type != null ? $"{plain}_{type}" : null;
                var table = r.Kind == "texture" ? bank.Textures : bank.Models;
                if (typed != null && table.TryGetValue(typed, out var t))
                    (row.File, row.Binders) = (typed, t);
                else if (table.TryGetValue(plain, out var p))
                    (row.File, row.Binders) = (plain, p);
                else
                    row.Binders = new();
                row.InOwnBinder = row.Binders.Any(b => entry.InBinder(b.Binder.Path));
                rows[(r.Kind, r.Id, type)] = row;
            }
            row.Actions.Add(r.Action);
            row.ActionNames.Add(Editor.Data.Schema.Actions.TryGetValue(r.Action.Type, out var d) ? d.Name : r.Action.Type.ToString());
        }

        Rows = rows.Values.OrderBy(r => r.File == null ? 0 : r.InOwnBinder ? 2 : 1).ThenBy(r => r.Kind).ThenBy(r => r.Id).ToList();
    }
}
