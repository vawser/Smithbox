using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Keybinds;
using StudioCore.Utilities;
using System.Numerics;

namespace StudioCore.Editors.TextEditor;

/// <summary>
/// Preview shown before a text import is applied: counts per text file (added / changed / kept /
/// identical), the entries themselves on expand, export text files that match nothing, and a
/// container mismatch warning. Apply rebuilds the plan and runs it as one undoable action.
/// Opened from Display() (called from the view window, via FmgImporter.OnGui) because ImGui
/// popup IDs depend on the window ID stack; Show() only requests it.
/// </summary>
public class TextImportPreviewPopup
{
    private TextEditorView View;
    private FmgImporter Importer;

    private TextImportPlan Plan;
    private bool OpenRequested;

    private const string PopupId = "##textImportPreviewModal";

    // Rows listed per text file when expanded; the rest is counted
    private const int MaxListedRows = 200;

    // Characters of entry text shown in a row; the tooltip has all of it
    private const int MaxShownChars = 80;

    public TextImportPreviewPopup(TextEditorView view, FmgImporter importer)
    {
        View = view;
        Importer = importer;
    }

    public void Show(TextImportPlan plan)
    {
        Plan = plan;
        OpenRequested = true;
    }

    public void Display()
    {
        var title = $"{LOC.Get("TEXT_ImportPreview_Title")}{PopupId}";

        if (OpenRequested)
        {
            OpenRequested = false;
            ImGui.OpenPopup(title);
        }

        // Set width, auto height: with AlwaysAutoResize alone, wrapped text never wraps
        ImGui.SetNextWindowSize(new Vector2(760, 0) * DPI.UIScale(), ImGuiCond.Always);

        if (!ImGui.BeginPopupModal(title, ImGuiWindowFlags.AlwaysAutoResize))
            return;

        if (Plan == null)
        {
            ImGui.CloseCurrentPopup();
            ImGui.EndPopup();
            return;
        }

        // What, where, how
        GUI.WrappedText(LOC.Get("TEXT_ImportPreview_Header",
            Plan.Source.Name, Plan.Target.FileEntry.Filename,
            LOC.Get(Plan.Behavior.GetDisplayName()), LOC.Get(Plan.Scope.GetDisplayName())));

        if (Plan.ContainerMismatch)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("TEXT_ImportPreview_Container_Mismatch",
                Plan.Source.SourceContainer, Plan.Target.FileEntry.Filename));
        }

        ImGui.Spacing();

        if (Plan.Fmgs.Count == 0)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("TEXT_ImportPreview_No_Match"));
        }
        else
        {
            GUI.WrappedText(LOC.Get("TEXT_ImportPreview_Summary", Plan.Added, Plan.Changed, Plan.Kept, Plan.Identical));

            if (Plan.Kept > 0)
                GUI.WrappedText(LOC.Get("TEXT_ImportPreview_Kept_Hint", Plan.Kept));

            DisplayFmgTable();
        }

        DisplayUnmatched();

        ImGui.Spacing();

        if (!Plan.HasChanges && Plan.Fmgs.Count > 0)
            GUI.WrappedText(LOC.Get("TEXT_ImportPreview_Nothing_To_Do"));
        else if (Plan.HasChanges)
            GUI.WrappedText(LOC.Get("TEXT_ImportPreview_Undo_Hint", InputManager.GetHint(KeybindID.Undo)));

        ImGui.Checkbox($"{LOC.Get("TEXT_ImportPreview_Option_Preview")}##importPreviewToggle",
            ref CFG.Current.TextEditor.TextEditor_DataTransfer_Preview_Imports);
        GUI.Tooltip(LOC.Get("TEXT_ImportPreview_Option_Preview_TT"));

        ImGui.Spacing();

        ImGui.BeginDisabled(!Plan.HasChanges);
        if (ImGui.Button($"{LOC.Get("TEXT_ImportPreview_Action_Apply")}##applyImport", DPI.StandardButtonSize))
        {
            Importer.ApplyPlan(Plan);
            Plan = null;
            ImGui.CloseCurrentPopup();
        }
        ImGui.EndDisabled();

        ImGui.SameLine();

        // Cancel is the default: Escape closes without importing
        if (ImGui.Button($"{LOC.Get("TEXT_ImportPreview_Action_Cancel")}##cancelImport", DPI.StandardButtonSize) ||
            ImGui.IsKeyPressed(ImGuiKey.Escape))
        {
            Plan = null;
            ImGui.CloseCurrentPopup();
        }

        ImGui.EndPopup();
    }

    private void DisplayFmgTable()
    {
        var tableFlags = ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders | ImGuiTableFlags.ScrollY |
            ImGuiTableFlags.SizingStretchProp;

        // Short imports don't need the full height
        var rows = Plan.Fmgs.Count + 1;
        var height = Math.Min(320f, rows * ImGui.GetFrameHeightWithSpacing() + 8f) * DPI.UIScale();

        if (!ImGui.BeginTable("importPreviewTable", 5, tableFlags, new Vector2(0, height)))
            return;

        ImGui.TableSetupScrollFreeze(0, 1);
        ImGui.TableSetupColumn(LOC.Get("TEXT_ImportPreview_Column_Text_File"), ImGuiTableColumnFlags.WidthStretch, 4f);
        ImGui.TableSetupColumn(LOC.Get("TEXT_ImportPreview_Column_Added"), ImGuiTableColumnFlags.WidthStretch, 1f);
        ImGui.TableSetupColumn(LOC.Get("TEXT_ImportPreview_Column_Changed"), ImGuiTableColumnFlags.WidthStretch, 1f);
        ImGui.TableSetupColumn(LOC.Get("TEXT_ImportPreview_Column_Kept"), ImGuiTableColumnFlags.WidthStretch, 1f);
        ImGui.TableSetupColumn(LOC.Get("TEXT_ImportPreview_Column_Identical"), ImGuiTableColumnFlags.WidthStretch, 1f);
        ImGui.TableHeadersRow();

        foreach (var fmgPlan in Plan.Fmgs)
        {
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            var hasRows = fmgPlan.Added.Count + fmgPlan.Changed.Count + fmgPlan.Kept.Count > 0;
            var nodeFlags = ImGuiTreeNodeFlags.SpanAllColumns | (hasRows ? ImGuiTreeNodeFlags.None : ImGuiTreeNodeFlags.Leaf);

            var open = ImGui.TreeNodeEx($"{fmgPlan.Target.Name} [{fmgPlan.Target.ID}]##importPreviewFmg_{fmgPlan.Target.ID}", nodeFlags);

            ImGui.TableSetColumnIndex(1);
            CountText(fmgPlan.Added.Count);
            ImGui.TableSetColumnIndex(2);
            CountText(fmgPlan.Changed.Count);
            ImGui.TableSetColumnIndex(3);
            CountText(fmgPlan.Kept.Count);
            ImGui.TableSetColumnIndex(4);
            CountText(fmgPlan.Identical);

            if (open)
            {
                DisplayEntryRows(fmgPlan);
                ImGui.TreePop();
            }
        }

        ImGui.EndTable();
    }

    private static void CountText(int count)
    {
        if (count == 0)
            ImGui.TextDisabled("0");
        else
            ImGui.TextUnformatted(count.ToString());
    }

    private void DisplayEntryRows(TextImportFmgPlan fmgPlan)
    {
        var listed = 0;

        foreach (var entry in fmgPlan.Added)
        {
            if (listed++ >= MaxListedRows)
                break;

            EntryRow(LOC.Get("TEXT_ImportPreview_Row_Added", entry.ID), null, entry.Text);
        }

        foreach (var (entry, newText) in fmgPlan.Changed)
        {
            if (listed++ >= MaxListedRows)
                break;

            EntryRow(LOC.Get("TEXT_ImportPreview_Row_Changed", entry.ID), entry.Text, newText);
        }

        foreach (var (entry, newText) in fmgPlan.Kept)
        {
            if (listed++ >= MaxListedRows)
                break;

            EntryRow(LOC.Get("TEXT_ImportPreview_Row_Kept", entry.ID), entry.Text, newText, disabled: true);
        }

        var total = fmgPlan.Added.Count + fmgPlan.Changed.Count + fmgPlan.Kept.Count;
        if (total > MaxListedRows)
        {
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);
            ImGui.TextDisabled(LOC.Get("TEXT_ImportPreview_More_Rows", total - MaxListedRows));
        }
    }

    /// <summary>A row under an expanded text file: label in the first column, the text across the rest.</summary>
    private static void EntryRow(string label, string oldText, string newText, bool disabled = false)
    {
        ImGui.TableNextRow();
        ImGui.TableSetColumnIndex(0);

        ImGui.Indent();
        if (disabled)
            ImGui.TextDisabled(label);
        else
            ImGui.TextUnformatted(label);
        ImGui.Unindent();

        ImGui.TableSetColumnIndex(1);

        var shown = oldText == null
            ? OneLine(newText)
            : $"{OneLine(oldText)}  ->  {OneLine(newText)}";

        if (disabled)
            ImGui.TextDisabled(shown);
        else
            ImGui.TextUnformatted(shown);

        GUI.Tooltip(oldText == null
            ? newText ?? ""
            : LOC.Get("TEXT_ImportPreview_Row_Change_TT", oldText ?? "", newText ?? ""));
    }

    private static string OneLine(string text)
    {
        if (string.IsNullOrEmpty(text))
            return LOC.Get("TEXT_ImportPreview_Empty_Text");

        var line = text.Replace("\r\n", " / ").Replace("\n", " / ");
        return line.Length > MaxShownChars ? line[..MaxShownChars] + "..." : line;
    }

    private void DisplayUnmatched()
    {
        if (Plan.Unmatched.Count == 0)
            return;

        ImGui.Spacing();

        GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color,
            LOC.Get("TEXT_ImportPreview_Unmatched", Plan.Unmatched.Count));

        if (ImGui.IsItemHovered())
        {
            var names = Plan.Unmatched.Take(30).Select(w => $"{w.Name} [{w.ID}]");
            var more = Plan.Unmatched.Count > 30 ? $"\n{LOC.Get("TEXT_ImportPreview_More_Rows", Plan.Unmatched.Count - 30)}" : "";
            GUI.Tooltip(string.Join("\n", names) + more);
        }
    }
}
