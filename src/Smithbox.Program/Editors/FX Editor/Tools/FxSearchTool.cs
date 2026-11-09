using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Editors.Common;
using System.Collections.Concurrent;
using System.Globalization;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace StudioCore.Editors.FxEditor;

/// <summary>A search hit: the effect, where the action is (node path, config index or -1 for the node's own, action index).</summary>
public record FxSearchMatch(int EffectId, string NodePath, int Config, int Action, string Label);

/// <summary>
/// Search (FX Editor Plan §22, phase E5): actions across every effect (or the changed ones) by type or name, values
/// by name with an optional range (fields, constants and every curve key), and textures or models by id. Runs on a
/// background task over the effects' packed copies (loaded effects as they are now); each match opens its action.
/// </summary>
public class FxSearchTool
{
    private readonly FxEditorScreen Editor;

    private string ActionText = "";
    private string ValueText = "";
    private bool UseRange;
    private float RangeMin, RangeMax = 1;
    private int ResourceId;
    private bool ChangedOnly;

    private const int MaxResults = 2000;

    private CancellationTokenSource Cancellation;
    private volatile bool Running;
    private volatile int Done, Total;
    private volatile bool Truncated;
    private readonly ConcurrentQueue<FxSearchMatch> Incoming = new();
    private readonly List<FxSearchMatch> Results = new();
    private string ResultFilter = "";
    private bool ResultExact;

    public FxSearchTool(FxEditorScreen editor)
    {
        Editor = editor;
    }

    public void OnToolWindow()
    {
        // Picked up even while another tool is shown, so a search keeps going
        while (Incoming.TryDequeue(out var m))
            Results.Add(m);

        if (!ToolSections.Header($"{LOC.Get("FX_SearchTool_Header")}##fxSearchToolHeader"))
            return;

        ImGui.BeginChild("FxSearchToolSection", ImGuiChildFlags.Borders);
        Display();
        ImGui.EndChild();
    }

    private void Display()
    {
        GUI.WrappedText(LOC.Get("FX_SearchTool_Hint"));

        if (!Editor.Data.Bank.IsIndexed)
        {
            GUI.WrappedText(LOC.Get("FX_EffectList_Indexing"));
            return;
        }

        // Action
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("FX_SearchTool_Action_Header"), LOC.Get("FX_SearchTool_Action_Header_TT"));
        ImGui.SetNextItemWidth(-1);
        ImGui.InputTextWithHint("##fxSearchAction", LOC.Get("FX_SearchTool_Action_Hint"), ref ActionText, 64);
        GUI.Tooltip(LOC.Get("FX_SearchTool_Action_TT"));

        // Value
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("FX_SearchTool_Value_Header"), LOC.Get("FX_SearchTool_Value_Header_TT"));
        ImGui.SetNextItemWidth(-1);
        ImGui.InputTextWithHint("##fxSearchValue", LOC.Get("FX_SearchTool_Value_Hint"), ref ValueText, 64);
        GUI.Tooltip(LOC.Get("FX_SearchTool_Value_TT"));
        ImGui.Checkbox($"{LOC.Get("FX_SearchTool_Range")}##fxSearchUseRange", ref UseRange);
        GUI.Tooltip(LOC.Get("FX_SearchTool_Range_TT"));
        if (UseRange)
        {
            var range = new Vector2(RangeMin, RangeMax);
            ImGui.SetNextItemWidth(-1);
            if (ImGui.InputFloat2("##fxSearchRange", ref range, "%.4g"))
                (RangeMin, RangeMax) = (range.X, range.Y);
            GUI.Tooltip(LOC.Get("FX_SearchTool_Range_Values_TT"));
        }

        // Resource
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("FX_SearchTool_Resource_Header"), LOC.Get("FX_SearchTool_Resource_Header_TT"));
        ImGui.SetNextItemWidth(-1);
        ImGui.InputInt("##fxSearchResource", ref ResourceId);
        ResourceId = Math.Max(0, ResourceId);
        GUI.Tooltip(LOC.Get("FX_SearchTool_Resource_TT"));

        // Scope
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("FX_SearchTool_Scope_Header"), LOC.Get("FX_SearchTool_Scope_Header_TT"));
        ImGui.Checkbox($"{LOC.Get("FX_SearchTool_Changed_Only")}##fxSearchChanged", ref ChangedOnly);
        GUI.Tooltip(LOC.Get("FX_SearchTool_Changed_Only_TT"));

        // Actions
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("FX_SearchTool_Actions_Header"), LOC.Get("FX_SearchTool_Actions_Header_TT"));
        bool hasQuery = ActionText.Trim() != "" || ValueText.Trim() != "" || ResourceId > 0;
        if (Running)
        {
            ImGui.ProgressBar(Total == 0 ? 0 : (float)Done / Total, new Vector2(-1, 0), LOC.Get("FX_SearchTool_Progress", Done, Total, Results.Count));
            GUI.MultiButtonInput("fxSearchCancel", "fxSearchCancelButton", LOC.Get("FX_ReferencesTool_Cancel"),
                LOC.Get("FX_SearchTool_Cancel_TT"), () => Cancellation?.Cancel());
        }
        else
        {
            GUI.ConditionalMultiButtonInput("fxSearchButtons",
                "fxSearchRun", LOC.Get("FX_SearchTool_Search"), LOC.Get("FX_SearchTool_Search_TT"), Start, hasQuery,
                "fxSearchClear", LOC.Get("FX_SearchTool_Clear"), LOC.Get("FX_SearchTool_Clear_TT"), Clear, true);
        }

        // Results
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("FX_SearchTool_Results_Header"), LOC.Get("FX_SearchTool_Results_Header_TT"));
        if (Results.Count == 0)
        {
            ImGui.TextDisabled(LOC.Get(Running ? "FX_SearchTool_Searching" : "FX_EffectList_None"));
            return;
        }
        GUI.WrappedText(LOC.Get(Truncated ? "FX_SearchTool_Count_Truncated" : "FX_SearchTool_Count", Results.Count, MaxResults));
        EditorFilters.DisplayFramedListFilter("fxSearchResults", ref ResultFilter, ref ResultExact);

        var shown = ResultFilter == "" ? Results : Results.Where(r => EditorFilters.IsMatch(ResultFilter, r.Label, ResultExact)).ToList();
        ImGui.BeginChild("##fxSearchResultList", new Vector2(-1, -1), ImGuiChildFlags.Borders);
        var clipper = new ImGuiListClipper();
        clipper.Begin(shown.Count);
        while (clipper.Step())
            for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
                if (ImGui.Selectable($"{shown[i].Label}##fxSearchResult{i}"))
                    Open(shown[i]);
        clipper.End();
        ImGui.EndChild();
    }

    private void Clear()
    {
        Cancellation?.Cancel();
        Results.Clear();
        while (Incoming.TryDequeue(out _)) { }
        ActionText = "";
        ValueText = "";
        UseRange = false;
        ResourceId = 0;
        Truncated = false;
    }

    /// <summary>Opens the effect and selects the matched action.</summary>
    private void Open(FxSearchMatch m)
    {
        if (!Editor.JumpTo(m.EffectId))
            return;
        var root = Editor.Selection.Entry?.Fxr?.Root;
        var node = root == null ? null : FxNodeOps.FromPath(root, m.NodePath);
        if (node == null)
            return;
        var list = m.Config < 0 ? node.Actions : m.Config < node.Configs.Count ? node.Configs[m.Config].Actions : null;
        if (list != null && m.Action < list.Count)
            Editor.Selection.SelectItem(list[m.Action]);
        else
            Editor.Selection.SelectItem(node);
    }

    // ---------------------------------------------------------------- search

    private void Start()
    {
        Cancellation?.Cancel();
        Results.Clear();
        while (Incoming.TryDequeue(out _)) { }
        Truncated = false;

        var bank = Editor.Data.Bank;
        var schema = Editor.Data.Schema;
        var game = Editor.Data.Game;
        // Loaded effects are searched as they are now; the rest from their packed copies
        var effects = bank.Entries.Values
            .Where(e => !ChangedOnly || FxProjectCheck.IsChanged(e))
            .Select(e => (e.Id, Loaded: e.IsLoaded ? e.Fxr : null, Entry: e))
            .ToList();

        var actionText = ActionText.Trim();
        int? actionType = int.TryParse(actionText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var t) ? t : null;
        var valueText = ValueText.Trim();
        bool useRange = UseRange;
        float min = Math.Min(RangeMin, RangeMax), max = Math.Max(RangeMin, RangeMax);
        int resource = ResourceId;

        Total = effects.Count;
        Done = 0;
        Running = true;
        Cancellation = new CancellationTokenSource();
        var token = Cancellation.Token;

        Task.Run(() =>
        {
            int count = 0;
            try
            {
                foreach (var (id, loaded, entry) in effects)
                {
                    token.ThrowIfCancellationRequested();
                    var fxr = loaded ?? bank.ReadCopy(entry);
                    Done++;
                    if (fxr == null)
                        continue;
                    try
                    {
                        foreach (var m in SearchEffect(id, fxr, schema, game, actionText, actionType, valueText, useRange, min, max, resource))
                        {
                            if (count++ >= MaxResults)
                            {
                                Truncated = true;
                                return;
                            }
                            Incoming.Enqueue(m);
                        }
                    }
                    catch (InvalidOperationException)
                    {
                        // A loaded effect edited mid-search
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                Running = false;
            }
        }, token);
    }

    private static IEnumerable<FxSearchMatch> SearchEffect(int id, FXR fxr, FxrSchema schema, FxrGame game,
        string actionText, int? actionType, string valueText, bool useRange, float min, float max, int resource)
    {
        HashSet<FxrAction> withResource = null;
        if (resource > 0)
        {
            withResource = new();
            foreach (var r in FxrResources.Collect(fxr, schema, game))
                if (r.Id == resource && r.Kind is "texture" or "model")
                    withResource.Add(r.Action);
            if (withResource.Count == 0)
                yield break;
        }

        foreach (var node in FXR.Walk(fxr.Root))
        {
            var path = FxNodeOps.PathOf(fxr.Root, node);
            for (int c = -1; c < node.Configs.Count; c++)
            {
                var actions = c < 0 ? node.Actions : node.Configs[c].Actions;
                for (int a = 0; a < actions.Count; a++)
                {
                    var action = actions[a];
                    if (withResource != null && !withResource.Contains(action))
                        continue;

                    var name = schema.Actions.TryGetValue(action.Type, out var def) ? def.Name : action.Type.ToString();
                    if (actionText != "" && (actionType is int type ? action.Type != type : !name.Contains(actionText, StringComparison.OrdinalIgnoreCase)))
                        continue;

                    string valueMatch = null;
                    if (valueText != "")
                    {
                        var view = new FxrActionView(schema, action, game, fxr);
                        if (!view.IsTyped || !MatchValues(view, valueText, useRange, min, max, out valueMatch))
                            continue;
                    }

                    var where = c < 0 ? $"node {(path == "" ? "root" : path)}" : $"node {(path == "" ? "root" : path)} config {c}";
                    var label = $"f{id:D9}  {where}  {name}" + (valueMatch != null ? $"  {valueMatch}" : "");
                    yield return new FxSearchMatch(id, path, c, a, label);
                }
            }
        }
    }

    /// <summary>
    /// Whether a value whose name contains the text is stored (and, with a range, has a component in it): fields,
    /// constants, and the values of every curve key.
    /// </summary>
    private static bool MatchValues(FxrActionView view, string text, bool useRange, float min, float max, out string match)
    {
        match = null;
        foreach (var name in view.Names)
        {
            if (!name.Contains(text, StringComparison.OrdinalIgnoreCase) || !view.IsStored(name))
                continue;

            IEnumerable<double> values;
            try
            {
                values = view.KindOf(name) switch
                {
                    FxrValueKind.Field => view.GetValue(name),
                    FxrValueKind.Property => CurveValues(view.GetCurve(name)),
                    FxrValueKind.Section10 => view.GetSection10(name)?.Select(v => (double)v) ?? Enumerable.Empty<double>(),
                    _ => Enumerable.Empty<double>(),
                };
            }
            catch
            {
                continue;
            }

            var list = values.ToList();
            if (useRange && !list.Any(v => v >= min && v <= max))
                continue;
            match = $"{FxNames.Pretty(name)} = {string.Join(", ", list.Take(4).Select(v => v.ToString("G4", CultureInfo.InvariantCulture)))}{(list.Count > 4 ? ", ..." : "")}";
            return true;
        }
        return false;
    }

    private static IEnumerable<double> CurveValues(FxrCurve curve)
    {
        if (curve.Constant != null)
            foreach (var v in curve.Constant)
                yield return v;
        foreach (var k in curve.Keyframes)
            foreach (var v in k.Value ?? Array.Empty<float>())
                yield return v;
        if (curve.ComponentKeyframes != null)
            foreach (var list in curve.ComponentKeyframes)
                foreach (var k in list)
                    foreach (var v in k.Value ?? Array.Empty<float>())
                        yield return v;
    }
}
