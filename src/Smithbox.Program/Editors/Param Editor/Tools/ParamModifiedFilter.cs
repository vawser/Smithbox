using Andre.Formats;
using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StudioCore.Editors.ParamEditor;

/// <summary>
/// A secondary filter on the modified (vanilla diff) checks. The user defines rules with the Mass Edit
/// param and row selectors; modified rows that match them, and the params that hold such rows, are drawn
/// with the Modified Filter colours instead of the normal modified colours.
/// <para>
/// Rules are only run against rows that are already modified, so the cost scales with the number of
/// modified rows, not the size of the params. Results are cached per param and rebuilt when the rules,
/// the param's diff set or the param's edit version change.
/// </para>
/// </summary>
public class ParamModifiedFilter
{
    private const int MaxListedRowsPerParam = 500;

    private ParamEditorView View;
    private ProjectEntry Project;

    private string FilterPath = "";

    public ModifiedFilterStore Store = new();

    /// <summary>
    /// Goes up whenever a rule or option changes, so every cached result is rebuilt
    /// </summary>
    private int RulesVersion = 0;

    /// <summary>
    /// Set while rules are being run, so the "modifiedfilter" row search command can't recurse into itself
    /// </summary>
    private bool Evaluating = false;

    private bool ShowResultList = true;

    private class CacheEntry
    {
        public Param Param;
        public HashSet<Param.Row> DiffSet;
        public int DiffCount;
        public long DirtyVersion;
        public int RulesVersion;

        public int[] RuleCounts;
        public HashSet<Param.Row> Matched;
    }

    private Dictionary<string, CacheEntry> Cache = new();

    public ParamModifiedFilter(ParamEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;

        FilterPath = Path.Join(Project.Descriptor.ProjectPath, ".smithbox", "Workflow", "Modified Filter", "Modified Filter.json");

        Load();
    }

    #region Evaluation

    /// <summary>
    /// True when the filter is on and has at least one enabled rule
    /// </summary>
    public bool IsActive => Store.Enabled && Store.Rules.Any(r => r.Enabled);

    /// <summary>
    /// True if the row is modified and matches the rules. The row must belong to the primary bank.
    /// </summary>
    public bool RowMatches(string paramKey, Param.Row row)
    {
        if (!IsActive || row == null || paramKey == null)
            return false;

        var entry = GetEntry(paramKey);

        return entry != null && entry.Matched.Contains(row);
    }

    /// <summary>
    /// True if the param holds at least one modified row that matches the rules
    /// </summary>
    public bool ParamMatches(string paramKey)
    {
        if (!IsActive || paramKey == null)
            return false;

        var entry = GetEntry(paramKey);

        return entry != null && entry.Matched.Count > 0;
    }

    /// <summary>
    /// The matched rows for a param, or null if there are none (or the filter is off)
    /// </summary>
    public HashSet<Param.Row> GetMatchedRows(string paramKey)
    {
        if (!IsActive || paramKey == null)
            return null;

        return GetEntry(paramKey)?.Matched;
    }

    private CacheEntry GetEntry(string paramKey)
    {
        // Rules that use the "modifiedfilter" row command would otherwise recurse
        if (Evaluating)
            return null;

        var bank = View.GetPrimaryBank();

        if (bank == null || bank.Params == null || !bank.Params.TryGetValue(paramKey, out var param))
            return null;

        var diff = bank.VanillaDiffCache?.GetValueOrDefault(paramKey);

        if (diff == null || diff.Count == 0)
        {
            Cache.Remove(paramKey);
            return null;
        }

        var dirtyVersion = ParamDirtyTracker.GetVersion(param);

        if (Cache.TryGetValue(paramKey, out var entry) &&
            entry.Param == param &&
            entry.DiffSet == diff &&
            entry.DiffCount == diff.Count &&
            entry.DirtyVersion == dirtyVersion &&
            entry.RulesVersion == RulesVersion)
        {
            return entry;
        }

        entry = Evaluate(bank, param, diff);
        entry.Param = param;
        entry.DiffSet = diff;
        entry.DiffCount = diff.Count;
        entry.DirtyVersion = dirtyVersion;
        entry.RulesVersion = RulesVersion;

        Cache[paramKey] = entry;

        return entry;
    }

    private CacheEntry Evaluate(ParamBank bank, Param param, HashSet<Param.Row> diff)
    {
        var entry = new CacheEntry
        {
            RuleCounts = new int[Store.Rules.Count],
            Matched = new HashSet<Param.Row>()
        };

        Evaluating = true;

        try
        {
            var modifiedRows = new List<Param.Row>(diff);
            var context = (bank, param);

            HashSet<Param.Row> combined = null;

            for (int i = 0; i < Store.Rules.Count; i++)
            {
                var rule = Store.Rules[i];

                if (!rule.Enabled)
                    continue;

                var ruleRows = MatchRule(rule, context, modifiedRows);
                entry.RuleCounts[i] = ruleRows.Count;

                if (combined == null)
                {
                    combined = new HashSet<Param.Row>(ruleRows);
                }
                else if (Store.MatchMode == ModifiedFilterMatchMode.All)
                {
                    combined.IntersectWith(ruleRows);
                }
                else
                {
                    combined.UnionWith(ruleRows);
                }
            }

            if (combined != null)
            {
                entry.Matched = combined;
            }
        }
        catch (Exception e)
        {
            // The diff set can be swapped by a background refresh mid-copy; the next frame rebuilds it
            Smithbox.LogError<ParamModifiedFilter>($"[Param Editor] Modified Filter evaluation failed: {e.Message}");
        }
        finally
        {
            Evaluating = false;
        }

        return entry;
    }

    /// <summary>
    /// The modified rows of one param that a single rule matches. A rule whose param selector doesn't
    /// match the param, or whose selectors fail to parse, matches nothing.
    /// </summary>
    private List<Param.Row> MatchRule(ModifiedFilterRule rule, (ParamBank, Param) context, List<Param.Row> modifiedRows)
    {
        var paramSelector = rule.ParamSelector?.Trim() ?? "";
        var rowSelector = rule.RowSelector?.Trim() ?? "";

        if (paramSelector != "")
        {
            var paramResult = View.MassEdit.PSE.Search(false,
                new List<(ParamBank, Param)> { context }, paramSelector, false, false);

            if (paramResult.Count == 0)
                return new List<Param.Row>();
        }

        if (rowSelector == "")
            return modifiedRows;

        return View.MassEdit.RSE.Search(context, modifiedRows, rowSelector, false, false);
    }

    private void RulesChanged()
    {
        RulesVersion++;
        Save();
    }

    #endregion

    #region Display

    public void Display()
    {
        if (!ToolSections.Header($"{LOC.Get("PARAM_ModifiedFilter_Header")}##modifiedFilterHeader"))
            return;

        ImGui.BeginChild("ModifiedFilterToolSection", ImGuiChildFlags.Borders);

        GUI.WrappedText(LOC.Get("PARAM_ModifiedFilter_Hint"));

        DisplayOptions();
        DisplayRules();
        DisplayResults();

        ImGui.EndChild();
    }

    private void DisplayOptions()
    {
        // Options
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("PARAM_ModifiedFilter_Header_Options"),
            LOC.Get("PARAM_ModifiedFilter_Header_Options_TT"));

        var enabled = Store.Enabled;

        if (ImGui.Checkbox($"{LOC.Get("PARAM_ModifiedFilter_Enabled")}##modifiedFilterEnabled", ref enabled))
        {
            Store.Enabled = enabled;
            RulesChanged();
        }
        GUI.Tooltip(LOC.Get("PARAM_ModifiedFilter_Enabled_TT"));

        // Match Mode
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("PARAM_ModifiedFilter_Header_Match_Mode"),
            LOC.Get("PARAM_ModifiedFilter_Header_Match_Mode_TT"));

        var modeLabel = GetMatchModeLabel(Store.MatchMode);

        ImGui.SetNextItemWidth(-1);

        if (ImGui.BeginCombo("##modifiedFilterMatchMode", modeLabel))
        {
            foreach (var mode in Enum.GetValues<ModifiedFilterMatchMode>())
            {
                if (ImGui.Selectable(GetMatchModeLabel(mode), mode == Store.MatchMode))
                {
                    Store.MatchMode = mode;
                    RulesChanged();
                }
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("PARAM_ModifiedFilter_Match_Mode_TT"));
    }

    private string GetMatchModeLabel(ModifiedFilterMatchMode mode)
    {
        return mode == ModifiedFilterMatchMode.All
            ? LOC.Get("PARAM_ModifiedFilter_Match_Mode_All")
            : LOC.Get("PARAM_ModifiedFilter_Match_Mode_Any");
    }

    private void DisplayRules()
    {
        // Rules
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("PARAM_ModifiedFilter_Header_Rules"),
            LOC.Get("PARAM_ModifiedFilter_Header_Rules_TT"));

        if (Store.Rules.Count == 0)
        {
            GUI.WrappedText(LOC.Get("PARAM_ModifiedFilter_No_Rules"));
        }

        var totals = GetRuleTotals();

        int removeIndex = -1;
        int moveFrom = -1;
        int moveTo = -1;

        for (int i = 0; i < Store.Rules.Count; i++)
        {
            var rule = Store.Rules[i];

            ImGui.PushID($"modifiedFilterRule_{i}");

            if (i > 0)
            {
                ImGui.Separator();
            }

            // Enabled + title + counts
            var ruleEnabled = rule.Enabled;

            if (ImGui.Checkbox($"##ruleEnabled", ref ruleEnabled))
            {
                rule.Enabled = ruleEnabled;
                RulesChanged();
            }
            GUI.Tooltip(LOC.Get("PARAM_ModifiedFilter_Rule_Enabled_TT"));

            ImGui.SameLine();
            ImGui.AlignTextToFramePadding();

            var count = totals != null && i < totals.Length ? totals[i] : 0;
            var title = LOC.Get("PARAM_ModifiedFilter_Rule_Title", i + 1);

            if (rule.Enabled && IsActive)
            {
                ImGui.Text(title);
                ImGui.SameLine();
                // Not wrapped, so the row buttons stay on this line
                ImGui.TextColored(UI.Current.ImGui_AliasName_Text, LOC.Get("PARAM_ModifiedFilter_Rule_Count", count));
            }
            else
            {
                ImGui.TextDisabled(title);
            }

            // Move up / move down / remove
            var buttonWidth = ImGui.GetFrameHeight();
            var spacing = ImGui.GetStyle().ItemSpacing.X;
            var buttonsWidth = buttonWidth * 3 + spacing * 2;
            ImGui.SameLine();
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + Math.Max(0, ImGui.GetContentRegionAvail().X - buttonsWidth));

            ImGui.BeginDisabled(i == 0);
            if (ImGui.Button($"{Icons.ArrowUp}##ruleUp", new Vector2(buttonWidth, 0)))
            {
                moveFrom = i;
                moveTo = i - 1;
            }
            ImGui.EndDisabled();
            GUI.Tooltip(LOC.Get("PARAM_ModifiedFilter_Rule_Move_Up_TT"));

            ImGui.SameLine();

            ImGui.BeginDisabled(i == Store.Rules.Count - 1);
            if (ImGui.Button($"{Icons.ArrowDown}##ruleDown", new Vector2(buttonWidth, 0)))
            {
                moveFrom = i;
                moveTo = i + 1;
            }
            ImGui.EndDisabled();
            GUI.Tooltip(LOC.Get("PARAM_ModifiedFilter_Rule_Move_Down_TT"));

            ImGui.SameLine();

            if (ImGui.Button($"{Icons.Trash}##ruleRemove", new Vector2(buttonWidth, 0)))
            {
                removeIndex = i;
            }
            GUI.Tooltip(LOC.Get("PARAM_ModifiedFilter_Rule_Remove_TT"));

            // Param selector
            GUI.WrappedText(LOC.Get("PARAM_ModifiedFilter_Param_Selector"));

            var autoParam = View.MassEdit.AutoFill?.ParamSearchBarAutoFill();
            if (autoParam != null)
            {
                rule.ParamSelector = autoParam;
                RulesChanged();
            }

            ImGui.SameLine();

            var paramSelector = rule.ParamSelector ?? "";
            ImGui.SetNextItemWidth(-1);
            ImGui.InputTextWithHint("##paramSelector", LOC.Get("PARAM_ModifiedFilter_Param_Selector_Hint"), ref paramSelector, 1024);
            rule.ParamSelector = paramSelector;

            if (ImGui.IsItemDeactivatedAfterEdit())
            {
                RulesChanged();
            }
            GUI.Tooltip(LOC.Get("PARAM_ModifiedFilter_Param_Selector_TT"));

            // Row selector
            GUI.WrappedText(LOC.Get("PARAM_ModifiedFilter_Row_Selector"));

            var autoRow = View.MassEdit.AutoFill?.RowSearchBarAutoFill();
            if (autoRow != null)
            {
                rule.RowSelector = autoRow;
                RulesChanged();
            }

            ImGui.SameLine();

            var rowSelector = rule.RowSelector ?? "";
            ImGui.SetNextItemWidth(-1);
            ImGui.InputTextWithHint("##rowSelector", LOC.Get("PARAM_ModifiedFilter_Row_Selector_Hint"), ref rowSelector, 1024);
            rule.RowSelector = rowSelector;

            if (ImGui.IsItemDeactivatedAfterEdit())
            {
                RulesChanged();
            }
            GUI.Tooltip(LOC.Get("PARAM_ModifiedFilter_Row_Selector_TT"));

            ImGui.PopID();
        }

        if (moveFrom >= 0 && moveTo >= 0 && moveTo < Store.Rules.Count)
        {
            (Store.Rules[moveFrom], Store.Rules[moveTo]) = (Store.Rules[moveTo], Store.Rules[moveFrom]);
            RulesChanged();
        }

        if (removeIndex >= 0)
        {
            Store.Rules.RemoveAt(removeIndex);
            RulesChanged();
        }

        // Rule Actions
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("PARAM_ModifiedFilter_Header_Actions"),
            LOC.Get("PARAM_ModifiedFilter_Header_Actions_TT"));

        GUI.ConditionalMultiButtonInput("modifiedFilterActions",
            "modifiedFilterAddRule",
            LOC.Get("PARAM_ModifiedFilter_Add_Rule"),
            LOC.Get("PARAM_ModifiedFilter_Add_Rule_TT"),
            AddRule, true,

            "modifiedFilterClearRules",
            LOC.Get("PARAM_ModifiedFilter_Clear_Rules"),
            LOC.Get("PARAM_ModifiedFilter_Clear_Rules_TT"),
            ClearRules, Store.Rules.Count > 0);
    }

    private void DisplayResults()
    {
        // Results
        GUI.Spacer();
        GUI.ConditionalHeader(
            LOC.Get("PARAM_ModifiedFilter_Header_Results"),
            LOC.Get("PARAM_ModifiedFilter_Header_Results_TT"),
            ref ShowResultList);

        if (!IsActive)
        {
            GUI.WrappedText(LOC.Get("PARAM_ModifiedFilter_Inactive"));
            return;
        }

        var results = GetParamResults();
        var rowTotal = results.Sum(r => r.Rows.Count);

        GUI.WrappedText(LOC.Get("PARAM_ModifiedFilter_Result_Count", rowTotal, results.Count));

        if (!ShowResultList || results.Count == 0)
            return;

        ImGui.BeginChild("##modifiedFilterResults", new Vector2(0, 0), ImGuiChildFlags.Borders);

        foreach (var (paramKey, rows) in results)
        {
            ImGui.PushID($"modifiedFilterResult_{paramKey}");

            ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_ModifiedFilter_Text);
            var open = ImGui.TreeNodeEx($"{paramKey} ({rows.Count})##paramNode", ImGuiTreeNodeFlags.SpanAvailWidth);
            ImGui.PopStyleColor(1);

            if (ImGui.IsItemClicked() && !ImGui.IsItemToggledOpen())
            {
                EditorCommandQueue.AddCommand($@"param/view/{View.ViewIndex}/{paramKey}");
            }
            GUI.Tooltip(LOC.Get("PARAM_ModifiedFilter_Result_Param_TT"));

            if (open)
            {
                var ordered = rows.OrderBy(r => r.ID).Take(MaxListedRowsPerParam);

                foreach (var row in ordered)
                {
                    var name = string.IsNullOrEmpty(row.Name) ? "" : $" {Utils.ImGuiEscape(row.Name)}";

                    if (ImGui.Selectable($"{row.ID}{name}##row_{row.ID}_{row.GetHashCode()}"))
                    {
                        EditorCommandQueue.AddCommand($@"param/select/{View.ViewIndex}/{paramKey}/{row.ID}");
                    }
                }

                if (rows.Count > MaxListedRowsPerParam)
                {
                    ImGui.TextDisabled(LOC.Get("PARAM_ModifiedFilter_Result_Truncated", rows.Count - MaxListedRowsPerParam));
                }

                ImGui.TreePop();
            }

            ImGui.PopID();
        }

        ImGui.EndChild();
    }

    /// <summary>
    /// Every param with matched rows, in param key order
    /// </summary>
    private List<(string Key, HashSet<Param.Row> Rows)> GetParamResults()
    {
        var results = new List<(string, HashSet<Param.Row>)>();
        var bank = View.GetPrimaryBank();

        if (bank?.VanillaDiffCache == null)
            return results;

        foreach (var paramKey in bank.VanillaDiffCache.Keys.OrderBy(k => k))
        {
            var rows = GetMatchedRows(paramKey);

            if (rows != null && rows.Count > 0)
            {
                results.Add((paramKey, rows));
            }
        }

        return results;
    }

    /// <summary>
    /// How many modified rows each rule matches on its own, summed over all params
    /// </summary>
    private int[] GetRuleTotals()
    {
        if (!IsActive)
            return null;

        var totals = new int[Store.Rules.Count];
        var bank = View.GetPrimaryBank();

        if (bank?.VanillaDiffCache == null)
            return totals;

        foreach (var paramKey in bank.VanillaDiffCache.Keys)
        {
            var entry = GetEntry(paramKey);

            if (entry?.RuleCounts == null)
                continue;

            for (int i = 0; i < totals.Length && i < entry.RuleCounts.Length; i++)
            {
                totals[i] += entry.RuleCounts[i];
            }
        }

        return totals;
    }

    private void AddRule()
    {
        Store.Rules.Add(new ModifiedFilterRule());
        RulesChanged();
    }

    private void ClearRules()
    {
        Store.Rules.Clear();
        RulesChanged();
    }

    #endregion

    #region Storage

    private void Load()
    {
        try
        {
            if (!File.Exists(FilterPath))
                return;

            var json = File.ReadAllText(FilterPath);
            var store = JsonSerializer.Deserialize(json, ModifiedFilterSerializationContext.Default.ModifiedFilterStore);

            if (store != null)
            {
                store.Rules ??= new();
                Store = store;
                RulesVersion++;
            }
        }
        catch (Exception e)
        {
            Smithbox.LogError<ParamModifiedFilter>($"[Param Editor] Failed to load the Modified Filter rules from {FilterPath}: {e.Message}");
        }
    }

    private void Save()
    {
        try
        {
            var dir = Path.GetDirectoryName(FilterPath);

            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var json = JsonSerializer.Serialize(Store, ModifiedFilterSerializationContext.Default.ModifiedFilterStore);
            File.WriteAllText(FilterPath, json);
        }
        catch (Exception e)
        {
            Smithbox.LogError<ParamModifiedFilter>($"[Param Editor] Failed to save the Modified Filter rules to {FilterPath}: {e.Message}");
        }
    }

    #endregion
}

public enum ModifiedFilterMatchMode
{
    Any = 0,
    All = 1
}

public class ModifiedFilterRule
{
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// A Mass Edit param selector (for example "EquipParamWeapon" or "param Equip.*"). Empty matches every param.
    /// </summary>
    public string ParamSelector { get; set; } = "";

    /// <summary>
    /// A Mass Edit row selector (for example "prop sortId 0 && name Dagger"). Empty matches every modified row.
    /// </summary>
    public string RowSelector { get; set; } = "";
}

public class ModifiedFilterStore
{
    public bool Enabled { get; set; } = true;

    public ModifiedFilterMatchMode MatchMode { get; set; } = ModifiedFilterMatchMode.Any;

    public List<ModifiedFilterRule> Rules { get; set; } = new();
}

[JsonSourceGenerationOptions(
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Metadata,
    IncludeFields = true)
]
[JsonSerializable(typeof(ModifiedFilterStore))]
public partial class ModifiedFilterSerializationContext
    : JsonSerializerContext
{ }
