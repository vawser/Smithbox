using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using System.Numerics;

namespace StudioCore.Editors.FxEditor;

/// <summary>
/// References (FX Editor Plan §22, phase E5): what uses the selected effect, and what it starts. Effects starting it
/// through Proxy nodes and param fields come from the Effect List's Used By index; TAE events and map objects from a
/// scan of every time act and map, run on request and kept in the project (<see cref="FxExternalReferences"/>).
/// Each entry opens where it is.
/// </summary>
public class FxReferencesTool
{
    private readonly FxEditorScreen Editor;

    public FxReferencesTool(FxEditorScreen editor)
    {
        Editor = editor;
    }

    private FxUsage Usage => Editor.EffectList.Usage;
    private FxExternalReferences External => Editor.EffectList.External;

    public void OnToolWindow()
    {
        if (!ToolSections.Header($"{LOC.Get("FX_ReferencesTool_Header")}##fxReferencesToolHeader"))
            return;

        ImGui.BeginChild("FxReferencesToolSection", ImGuiChildFlags.Borders);
        Display();
        ImGui.EndChild();
    }

    private void Display()
    {
        GUI.WrappedText(LOC.Get("FX_ReferencesTool_Hint"));

        var entry = Editor.Selection.Entry;
        if (entry == null)
        {
            GUI.WrappedText(LOC.Get("FX_ReferencesTool_No_Effect"));
            return;
        }

        // Effects that start it
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("FX_ReferencesTool_Effects_Header"), LOC.Get("FX_ReferencesTool_Effects_Header_TT"));
        if (!Usage.HasScan)
        {
            if (Usage.IsScanning)
                ImGui.ProgressBar(Usage.ScanTotal == 0 ? 0 : (float)Usage.ScanDone / Usage.ScanTotal, new Vector2(-1, 0),
                    LOC.Get("FX_EffectList_Scanning", Usage.ScanDone, Usage.ScanTotal));
            else
                GUI.MultiButtonInput("fxRefEffectsScan", "fxRefEffectsScanButton", LOC.Get("FX_EffectList_Scan"),
                    LOC.Get("FX_ReferencesTool_Scan_Effects_TT"), Usage.StartScan);
        }
        else
        {
            var hosts = Usage.StartedBy(entry.Id);
            EffectRows("fxRefStartedBy", hosts.Select(h => h.Id).ToList());
        }

        // Effects it starts
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("FX_ReferencesTool_Starts_Header"), LOC.Get("FX_ReferencesTool_Starts_Header_TT"));
        EffectRows("fxRefStarts", Usage.Starts(entry));

        // Params
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("FX_ReferencesTool_Params_Header"), LOC.Get("FX_ReferencesTool_Params_Header_TT"));
        if (!Usage.ParamsAvailable)
        {
            GUI.WrappedText(LOC.Get("FX_EffectList_Params_Unavailable"));
        }
        else
        {
            var uses = Usage.ParamsUsing(entry.Id);
            Rows("fxRefParams", uses.Count, i =>
            {
                var u = uses[i];
                var name = string.IsNullOrEmpty(u.RowName) ? "" : $"  {u.RowName}";
                if (ImGui.Selectable($"{u.Param} {u.RowId}{name} : {u.Field}##fxRefParam{i}"))
                    FxUsage.JumpToParam(u);
            });
            GUI.MultiButtonInput("fxRefParamsRescan", "fxRefParamsRescanButton", LOC.Get("FX_EffectList_Rescan_Params"),
                LOC.Get("FX_EffectList_Rescan_Params_TT"), Usage.ScanParams);
        }

        // Time acts and maps
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("FX_ReferencesTool_External_Header"), LOC.Get("FX_ReferencesTool_External_Header_TT"));
        DisplayExternal(entry);
    }

    private void DisplayExternal(FxEntry entry)
    {
        var external = External;
        if (!external.TaeAvailable && !external.MsbAvailable)
        {
            GUI.WrappedText(LOC.Get("FX_ReferencesTool_External_Unavailable"));
            return;
        }

        if (external.IsScanning)
        {
            ImGui.ProgressBar(external.Total == 0 ? 0 : (float)external.Done / external.Total, new Vector2(-1, 0),
                LOC.Get("FX_ReferencesTool_Scanning", external.Done, external.Total, external.Status));
            GUI.MultiButtonInput("fxRefExternalCancel", "fxRefExternalCancelButton", LOC.Get("FX_ReferencesTool_Cancel"),
                LOC.Get("FX_ReferencesTool_Cancel_TT"), external.Cancel);
            return;
        }

        if (!external.HasScan)
        {
            GUI.WrappedText(LOC.Get("FX_ReferencesTool_External_Not_Scanned"));
        }
        else
        {
            GUI.WrappedText(LOC.Get("FX_ReferencesTool_External_Status", external.ScannedAt.Value.ToLocalTime().ToString("g"),
                external.UseCount, external.SourceCount));
            if (external.IsStale)
                GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("FX_ReferencesTool_External_Stale"));

            var uses = external.UsesOf(entry.Id);
            Rows("fxRefExternal", uses.Count, i =>
            {
                var u = uses[i];
                bool canOpen = u.Command != null;
                ImGui.BeginDisabled(!canOpen);
                if (ImGui.Selectable($"[{u.Kind}]  {u.Label}##fxRefExternal{i}"))
                    EditorCommandQueue.AddCommand(u.Command);
                ImGui.EndDisabled();
                if (!canOpen && ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
                    ImGui.SetTooltip(LOC.Get("FX_ReferencesTool_No_Command"));
            });
        }

        GUI.MultiButtonInput("fxRefExternalScan", "fxRefExternalScanButton",
            LOC.Get(external.HasScan ? "FX_ReferencesTool_Rescan" : "FX_ReferencesTool_Scan"),
            LOC.Get("FX_ReferencesTool_Scan_TT"), external.StartScan);
    }

    private void EffectRows(string id, List<int> ids)
    {
        var bank = Editor.Data.Bank;
        Rows(id, ids.Count, i =>
        {
            int effect = ids[i];
            bool exists = bank.Entries.TryGetValue(effect, out var e);
            var label = exists ? $"{e.Stem}{(string.IsNullOrEmpty(e.Alias) ? "" : $"  {e.Alias}")}" : LOC.Get("FX_EffectList_Missing_Effect", $"f{effect:D9}");
            ImGui.BeginDisabled(!exists);
            if (ImGui.Selectable($"{label}##{id}{i}"))
                Editor.JumpTo(effect);
            ImGui.EndDisabled();
        });
    }

    /// <summary>A bordered, clipped list, or a note when it's empty.</summary>
    private static void Rows(string id, int count, Action<int> row)
    {
        if (count == 0)
        {
            ImGui.TextDisabled(LOC.Get("FX_EffectList_None"));
            return;
        }

        float height = Math.Min(count, 10) * ImGui.GetTextLineHeightWithSpacing() + ImGui.GetStyle().FramePadding.Y * 2;
        ImGui.BeginChild($"##{id}List", new Vector2(-1, height), ImGuiChildFlags.Borders);
        var clipper = new ImGuiListClipper();
        clipper.Begin(count);
        while (clipper.Step())
            for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
                row(i);
        clipper.End();
        ImGui.EndChild();
    }
}
