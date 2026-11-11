using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using System.Diagnostics;
using System.Globalization;
using System.Numerics;

namespace StudioCore.Editors.EsdEditor;

/// <summary>
/// Search (P5): every use of a value in this ESD or every ESD of the project (Framework/EsdSearch), optionally only as
/// an event flag. Files are read a few per frame, so a search of every file doesn't stall the editor. A result selects
/// the block that uses the value. Flags can also be looked up in the EMEVD Editor (emevd/search).
/// </summary>
public class EsdSearchTool
{
    private readonly EsdEditorView View;

    private string ValueText = "";
    private bool FlagsOnly = true;
    private bool AllFiles = true;
    private bool FocusNext;

    private class Result
    {
        public EsdFileWrapper File;
        public EsdSearchHit Hit;
        public string Label;
    }

    private readonly List<Result> Results = new();
    private long Value;
    private Queue<EsdContainer> Pending;
    private int Scanned, Total;
    private string Status;

    public EsdSearchTool(EsdEditorView view)
    {
        View = view;
    }

    private EsdBank Bank => View.Project.Handler.EsdData?.PrimaryBank;

    /// <summary>Searches every file for a value (a flag link, or the esd/search command)</summary>
    public void SearchValue(long value, bool flagsOnly)
    {
        ValueText = value.ToString(CultureInfo.InvariantCulture);
        FlagsOnly = flagsOnly;
        AllFiles = true;
        CFG.Current.Interface_EsdEditor_ToolWindow = true;
        CFG.Current.Interface_EsdEditor_Tool_Search = true;
        FocusNext = true;
        Start(value);
    }

    private void Start(long value)
    {
        Value = value;
        Results.Clear();
        Status = null;

        var bank = Bank;
        if (bank == null)
            return;

        var containers = AllFiles ? bank.Containers : new List<EsdContainer> { View.Selection.SelectedContainer }.Where(c => c != null).ToList();
        Pending = new Queue<EsdContainer>(containers);
        Scanned = 0;
        Total = containers.Count;
    }

    /// <summary>Reads and searches files for at most a few milliseconds a frame</summary>
    private void Step()
    {
        if (Pending == null)
            return;

        var watch = Stopwatch.StartNew();
        var catalog = View.Project.Handler.EsdData?.Catalog;
        var game = View.Links.Game;

        while (Pending.Count > 0 && watch.ElapsedMilliseconds < 8)
        {
            var container = Pending.Dequeue();
            Scanned++;
            if (!container.EnsureLoaded())
                continue;

            foreach (var file in container.Files.Where(f => f.Esd != null))
            {
                var cat = catalog?.For(file.Kind);
                foreach (var hit in EsdSearch.Find(file.Esd, Value, game, cat, FlagsOnly))
                    Results.Add(new Result { File = file, Hit = hit, Label = Describe(hit, cat) });
            }
        }

        if (Pending.Count == 0)
        {
            Pending = null;
            Status = LOC.Get("ESD_SearchTool_Done", Results.Count, Total);
        }
    }

    private static string Describe(EsdSearchHit hit, EsdKindCatalog cat)
    {
        if (hit.Role == EsdRoleKind.Machine && hit.Command != null && hit.CallArg < 0)
            return LOC.Get("ESD_SearchTool_Hit_Call");

        if (hit.CallArg >= 0)
        {
            var def = hit.CallBank < 0 ? cat?.Function(hit.CallId) : cat?.Command(hit.CallBank, (int)hit.CallId);
            var call = def?.Name ?? (hit.CallBank < 0 ? $"f{hit.CallId}" : $"{hit.CallBank}:{hit.CallId}");
            var arg = def != null && hit.CallArg < def.Args.Count ? def.Args[hit.CallArg].Name : $"arg{hit.CallArg}";
            return $"{call} · {arg}";
        }

        return LOC.Get("ESD_SearchTool_Hit_Test");
    }

    public void OnToolWindow()
    {
        if (FocusNext)
        {
            ToolSections.Focus();
            FocusNext = false;
        }

        if (!ToolSections.Header($"{LOC.Get("ESD_SearchTool_Header")}##esdSearchToolHeader"))
            return;

        ImGui.BeginChild("EsdSearchToolSection", ImGuiChildFlags.Borders);
        Display();
        ImGui.EndChild();
    }

    private void Display()
    {
        Step();

        GUI.WrappedText(LOC.Get("ESD_SearchTool_Hint"));

        if (Bank == null)
        {
            GUI.WrappedText(LOC.Get("ESD_PaletteTool_No_File"));
            return;
        }

        // Value
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("ESD_SearchTool_Value_Header"), LOC.Get("ESD_SearchTool_Value_Header_TT"));

        ImGui.SetNextItemWidth(-1);
        var enter = ImGui.InputText("##esdSearchValue", ref ValueText, 32, ImGuiInputTextFlags.EnterReturnsTrue);

        ImGui.Checkbox($"{LOC.Get("ESD_SearchTool_Flags_Only")}##esdSearchFlags", ref FlagsOnly);
        GUI.Tooltip(LOC.Get("ESD_SearchTool_Flags_Only_TT"));
        ImGui.SameLine();
        ImGui.Checkbox($"{LOC.Get("ESD_SearchTool_All_Files")}##esdSearchAll", ref AllFiles);
        GUI.Tooltip(LOC.Get("ESD_SearchTool_All_Files_TT"));

        var parsed = long.TryParse(ValueText.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value);
        ImGui.BeginDisabled(!parsed || Pending != null);
        if (ImGui.Button($"{LOC.Get("ESD_SearchTool_Search")}##esdSearchGo", new Vector2(-1, 0)) || enter && parsed)
            Start(value);
        ImGui.EndDisabled();

        if (parsed && View.Project.Descriptor.EnableEmevdEditor)
        {
            if (ImGui.Button($"{LOC.Get("ESD_SearchTool_Search_Emevd")}##esdSearchEmevd", new Vector2(-1, 0)))
                EditorCommandQueue.AddCommand($"emevd/search/{value}");
            GUI.Tooltip(LOC.Get("ESD_SearchTool_Search_Emevd_TT"));
        }

        // Results
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("ESD_SearchTool_Results_Header"), LOC.Get("ESD_SearchTool_Results_Header_TT"));

        if (Pending != null)
            ImGui.ProgressBar(Total == 0 ? 1f : (float)Scanned / Total, new Vector2(-1, 0), LOC.Get("ESD_SearchTool_Progress", Scanned, Total));
        else if (Status != null)
            ImGui.TextDisabled(Status);

        ImGui.BeginChild("esdSearchResults", Vector2.Zero, ImGuiChildFlags.Borders);
        var clipper = new ImGuiListClipper();
        clipper.Begin(Results.Count);
        while (clipper.Step())
        {
            for (var i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
            {
                var r = Results[i];
                var text = $"{r.File.Name} · {EsdLayout.MachineLabel(r.Hit.Machine)} · {LOC.Get("ESD_States_State_Title", r.Hit.State)}";
                if (ImGui.Selectable($"{text}##esdSearchHit{i}"))
                    Go(r.File, r.Hit);
                ImGui.SameLine();
                ImGui.TextDisabled(r.Label);
            }
        }
        clipper.End();
        ImGui.EndChild();
    }

    /// <summary>Selects a hit's block: its file, machine, and the condition or command</summary>
    public void Go(EsdFileWrapper file, EsdSearchHit hit)
    {
        var selection = View.Selection;
        selection.SelectFile(file);
        selection.SelectMachine(hit.Machine);
        object item = hit.Command != null ? hit.Command : hit.Condition;
        if (item != null)
            selection.SelectItem(item, hit.State);
        else
            selection.GoToState(hit.State);
        View.FileList.ScrollToSelection = true;
    }
}
