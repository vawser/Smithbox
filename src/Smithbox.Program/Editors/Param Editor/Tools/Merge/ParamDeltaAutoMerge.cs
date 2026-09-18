using Andre.Formats;
using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.ParamEditor;

public enum ParamDeltaConflictStrategy
{
    StopOnConflict = 0,
    PreferFirst = 1,
    PreferLast = 2
}

public enum ParamDeltaMergeConflictType
{
    FieldValue = 0,
    RowState = 1
}

public enum ParamDeltaConflictResolution
{
    Unresolved = 0,
    UseEarlier = 1,
    UseLater = 2,
    Manual = 3
}

public sealed class ParamDeltaMergeConflict
{
    public ParamDeltaMergeConflictType Type { get; init; }
    public string ParamName { get; init; } = "";
    public int RowID { get; init; }
    public int RowIndex { get; init; }
    public string Field { get; init; } = "";
    public string ExistingSource { get; init; } = "";
    public string IncomingSource { get; init; } = "";
    public string ExistingValue { get; init; } = "";
    public string IncomingValue { get; init; } = "";
    public RowDeltaState ExistingState { get; init; }
    public RowDeltaState IncomingState { get; init; }
    public ParamDeltaConflictResolution Resolution { get; set; } = ParamDeltaConflictResolution.Unresolved;
    public string ManualValue { get; set; } = "";

    internal RowDelta ExistingRowSnapshot { get; init; }
    internal RowDelta IncomingRowSnapshot { get; init; }

    public bool IsResolved => Resolution != ParamDeltaConflictResolution.Unresolved;
}

public sealed class ParamDeltaAutoMergeResult
{
    public ParamDeltaPatch Patch { get; init; } = new();
    public List<ParamDeltaMergeConflict> Conflicts { get; } = new();
    public List<string> Errors { get; } = new();
    public int SourceCount { get; set; }
    public int SafeRows { get; set; }
    public int SafeFields { get; set; }
    public int IdenticalFields { get; set; }
    public ParamDeltaConflictStrategy Strategy { get; set; }

    public int ResolvedConflicts => Conflicts.Count(e => e.IsResolved);
    public int UnresolvedConflicts => Conflicts.Count - ResolvedConflicts;

    public bool CanApply => Errors.Count == 0 && Conflicts.All(e => e.IsResolved);
}

public sealed class ParamDeltaAutoMergeEngine
{
    private readonly ParamDeltaPatcher Patcher;

    private sealed class MergedRow
    {
        public RowDelta Row { get; set; } = new();
        public string RowSource { get; set; } = "";
        public Dictionary<string, string> FieldSources { get; } = new(StringComparer.Ordinal);
    }

    private readonly record struct RowKey(string ParamName, int ID, int Index);

    public ParamDeltaAutoMergeEngine(ParamDeltaPatcher patcher)
    {
        Patcher = patcher;
    }

    public static ParamDeltaConflictResolution ResolutionFromStrategy(ParamDeltaConflictStrategy strategy)
    {
        return strategy switch
        {
            ParamDeltaConflictStrategy.PreferFirst => ParamDeltaConflictResolution.UseEarlier,
            ParamDeltaConflictStrategy.PreferLast => ParamDeltaConflictResolution.UseLater,
            _ => ParamDeltaConflictResolution.Unresolved
        };
    }

    public void ApplyConflictResolutions(ParamDeltaAutoMergeResult result)
    {
        if (result == null)
            return;

        foreach (var conflict in result.Conflicts)
        {
            if (!conflict.IsResolved)
                continue;

            var param = result.Patch.Params.FirstOrDefault(e => e.Name == conflict.ParamName);
            if (param == null)
                continue;

            var row = param.Rows.FirstOrDefault(e => e.ID == conflict.RowID && e.Index == conflict.RowIndex);

            if (conflict.Type == ParamDeltaMergeConflictType.FieldValue)
            {
                if (row == null)
                    continue;

                var field = row.Fields.FirstOrDefault(e => e.Field == conflict.Field);
                if (field == null)
                {
                    field = new FieldDelta { Field = conflict.Field };
                    row.Fields.Add(field);
                }

                field.Value = conflict.Resolution switch
                {
                    ParamDeltaConflictResolution.UseEarlier => conflict.ExistingValue,
                    ParamDeltaConflictResolution.UseLater => conflict.IncomingValue,
                    ParamDeltaConflictResolution.Manual => conflict.ManualValue ?? "",
                    _ => field.Value
                };
                continue;
            }

            var selected = conflict.Resolution == ParamDeltaConflictResolution.UseLater
                ? conflict.IncomingRowSnapshot
                : conflict.ExistingRowSnapshot;

            if (selected == null)
                continue;

            if (row != null)
                param.Rows.Remove(row);

            param.Rows.Add(CloneRow(selected));
        }
    }

    public ParamDeltaAutoMergeResult Merge(
        IReadOnlyList<DeltaImportEntry> sources,
        ParamDeltaConflictStrategy strategy)
    {
        var result = new ParamDeltaAutoMergeResult
        {
            SourceCount = sources.Count,
            Strategy = strategy,
            Patch = new ParamDeltaPatch
            {
                ProjectType = Patcher.Project.Descriptor.ProjectType,
                ParamVersion = Patcher.Project.Handler.ParamData.PrimaryBank.ParamVersion,
                Tag = LOC.Get("PARAM_AutoMerge_Patch_Tag", sources.Count)
            }
        };

        if (sources.Count < 2)
        {
            result.Errors.Add(LOC.Get("PARAM_AutoMerge_Missing_Two_Delta_Patches"));
            return result;
        }

        ValidateSources(sources, result);
        if (result.Errors.Count > 0)
            return result;

        var mergedRows = new Dictionary<RowKey, MergedRow>();

        foreach (var source in sources)
        {
            foreach (var paramDelta in source.Delta.Params)
            {
                foreach (var originalRow in paramDelta.Rows)
                {
                    var incoming = NormalizeRow(paramDelta.Name, originalRow);
                    var key = new RowKey(paramDelta.Name, incoming.ID, incoming.Index);

                    if (!mergedRows.TryGetValue(key, out var current))
                    {
                        var cloned = CloneRow(incoming);
                        current = new MergedRow
                        {
                            Row = cloned,
                            RowSource = source.Filename
                        };

                        foreach (var field in cloned.Fields)
                            current.FieldSources[field.Field] = source.Filename;

                        mergedRows.Add(key, current);
                        result.SafeRows++;
                        result.SafeFields += cloned.Fields.Count;
                        continue;
                    }

                    MergeRow(key, current, incoming, source.Filename, strategy, result);
                }
            }
        }

        foreach (var paramGroup in mergedRows
                     .OrderBy(e => e.Key.ParamName, StringComparer.Ordinal)
                     .ThenBy(e => e.Key.ID)
                     .ThenBy(e => e.Key.Index)
                     .GroupBy(e => e.Key.ParamName, StringComparer.Ordinal))
        {
            var paramDelta = new ParamDelta { Name = paramGroup.Key };

            foreach (var row in paramGroup)
            {
                if (row.Value.Row.State == RowDeltaState.Modified && row.Value.Row.Fields.Count == 0)
                    continue;

                paramDelta.Rows.Add(CloneRow(row.Value.Row));
            }

            if (paramDelta.Rows.Count > 0)
                result.Patch.Params.Add(paramDelta);
        }

        return result;
    }

    private void ValidateSources(IReadOnlyList<DeltaImportEntry> sources, ParamDeltaAutoMergeResult result)
    {
        var projectType = Patcher.Project.Descriptor.ProjectType;
        var paramVersion = Patcher.Project.Handler.ParamData.PrimaryBank.ParamVersion;

        foreach (var source in sources)
        {
            if (source.Delta.ProjectType != projectType)
            {
                result.Errors.Add(
                    LOC.Get("PARAM_AutoMerge_Invalid_Project_Type", source.Filename, source.Delta.ProjectType, projectType));
            }

            if (source.Delta.ParamVersion != paramVersion)
            {
                result.Errors.Add(
                    LOC.Get("PARAM_AutoMerge_Invalid_Param_Version", source.Filename,
                    ParamUtils.ParseRegulationVersion(source.Delta.ParamVersion),
                    ParamUtils.ParseRegulationVersion(paramVersion)));
            }
        }
    }

    private void MergeRow(
        RowKey key,
        MergedRow current,
        RowDelta incoming,
        string incomingSource,
        ParamDeltaConflictStrategy strategy,
        ParamDeltaAutoMergeResult result)
    {
        if (current.Row.State == RowDeltaState.Deleted || incoming.State == RowDeltaState.Deleted)
        {
            if (current.Row.State == RowDeltaState.Deleted && incoming.State == RowDeltaState.Deleted)
                return;

            result.Conflicts.Add(new ParamDeltaMergeConflict
            {
                Type = ParamDeltaMergeConflictType.RowState,
                ParamName = key.ParamName,
                RowID = key.ID,
                RowIndex = key.Index,
                ExistingSource = current.RowSource,
                IncomingSource = incomingSource,
                ExistingState = current.Row.State,
                IncomingState = incoming.State,
                Resolution = ResolutionFromStrategy(strategy),
                ExistingRowSnapshot = CloneRow(current.Row),
                IncomingRowSnapshot = CloneRow(incoming)
            });

            if (strategy == ParamDeltaConflictStrategy.PreferLast)
            {
                current.Row = CloneRow(incoming);
                current.RowSource = incomingSource;
                current.FieldSources.Clear();
                foreach (var field in current.Row.Fields)
                    current.FieldSources[field.Field] = incomingSource;
            }

            return;
        }

        // The actual row state is determined against vanilla, not by the source delta.
        // This is important because Selected/All delta exports represent full rows as Added.
        current.Row.State = VanillaRowExists(key.ParamName, key.ID, key.Index)
            ? RowDeltaState.Modified
            : RowDeltaState.Added;

        if (string.IsNullOrWhiteSpace(current.Row.Name) && !string.IsNullOrWhiteSpace(incoming.Name))
            current.Row.Name = incoming.Name;

        foreach (var incomingField in incoming.Fields)
        {
            var existingField = current.Row.Fields.FirstOrDefault(e => e.Field == incomingField.Field);
            if (existingField == null)
            {
                current.Row.Fields.Add(CloneField(incomingField));
                current.FieldSources[incomingField.Field] = incomingSource;
                result.SafeFields++;
                continue;
            }

            if (string.Equals(existingField.Value, incomingField.Value, StringComparison.Ordinal))
            {
                result.IdenticalFields++;
                continue;
            }

            var existingSource = current.FieldSources.TryGetValue(incomingField.Field, out var fieldSource)
                ? fieldSource
                : current.RowSource;

            result.Conflicts.Add(new ParamDeltaMergeConflict
            {
                Type = ParamDeltaMergeConflictType.FieldValue,
                ParamName = key.ParamName,
                RowID = key.ID,
                RowIndex = key.Index,
                Field = incomingField.Field,
                ExistingSource = existingSource,
                IncomingSource = incomingSource,
                ExistingValue = existingField.Value,
                IncomingValue = incomingField.Value,
                ExistingState = current.Row.State,
                IncomingState = incoming.State,
                Resolution = ResolutionFromStrategy(strategy)
            });

            if (strategy == ParamDeltaConflictStrategy.PreferLast)
            {
                existingField.Value = incomingField.Value;
                current.FieldSources[incomingField.Field] = incomingSource;
            }
        }
    }

    private RowDelta NormalizeRow(string paramName, RowDelta source)
    {
        var normalized = CloneRow(source);
        var vanillaRow = GetVanillaRow(paramName, source.ID, source.Index);

        if (source.State == RowDeltaState.Deleted)
        {
            normalized.Fields.Clear();
            return normalized;
        }

        if (vanillaRow == null)
        {
            normalized.State = RowDeltaState.Added;
            return normalized;
        }

        normalized.State = RowDeltaState.Modified;

        // A full-row delta (Selected/All export) may label an existing vanilla row as Added.
        // Remove values that are identical to vanilla so they do not create false conflicts.
        if (source.State == RowDeltaState.Added)
        {
            normalized.Fields = normalized.Fields
                .Where(field => !FieldEqualsVanilla(vanillaRow, field))
                .ToList();
        }

        return normalized;
    }

    private bool FieldEqualsVanilla(Param.Row vanillaRow, FieldDelta field)
    {
        var column = vanillaRow.Columns.FirstOrDefault(e => e.Def.InternalName == field.Field);
        if (column == null)
            return false;

        var value = column.GetValue(vanillaRow);
        string vanillaValue;

        if (column.Def.InternalType == "dummy8" && column.Def.ArrayLength > 1)
            vanillaValue = ParamUtils.Dummy8Write((byte[])value);
        else
            vanillaValue = value?.ToString() ?? "";

        return string.Equals(vanillaValue, field.Value, StringComparison.Ordinal);
    }

    private bool VanillaRowExists(string paramName, int id, int index)
    {
        return GetVanillaRow(paramName, id, index) != null;
    }

    private Param.Row GetVanillaRow(string paramName, int id, int index)
    {
        var vanillaBank = Patcher.Project.Handler.ParamData.VanillaBank;
        if (!vanillaBank.Params.TryGetValue(paramName, out var vanillaParam))
            return null;

        var currentRowID = 0;
        var internalIndex = 0;

        foreach (var row in vanillaParam.Rows)
        {
            if (row.ID == currentRowID)
                internalIndex++;
            else
                internalIndex = 0;

            if (row.ID == id && internalIndex == index)
                return row;

            currentRowID = row.ID;
        }

        return null;
    }

    private static RowDelta CloneRow(RowDelta row)
    {
        var clone = new RowDelta
        {
            ID = row.ID,
            Index = row.Index,
            Name = row.Name,
            State = row.State
        };

        foreach (var field in row.Fields)
            clone.Fields.Add(CloneField(field));

        return clone;
    }

    private static FieldDelta CloneField(FieldDelta field)
    {
        return new FieldDelta
        {
            Field = field.Field,
            Value = field.Value
        };
    }
}

public sealed class ParamDeltaAutoMergeTool
{
    private readonly ParamDeltaPatcher Patcher;
    private readonly ParamDeltaAutoMergeEngine Engine;
    private readonly ParamRegulationAutoMerge RegulationMerge;
    private readonly ParamFullModAutoMerge FullModMerge;
    private readonly HashSet<string> SelectedFiles = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<string> RegulationPaths = new() { "", "" };
    private readonly List<string> FullModFolderPaths = new() { "", "" };

    private ParamDeltaAutoMergeResult LastResult;
    private RegulationMergeAnalysis LastRegulationAnalysis;
    private FullModMergeAnalysis LastFullModAnalysis;
    private ParamDeltaConflictStrategy Strategy = ParamDeltaConflictStrategy.StopOnConflict;
    private string OutputName = "auto_merged";
    private string RegulationOutputPath = "";
    private string RegulationBuildStatus = "";
    private string FullModOutputPath = "";
    private string FullModBuildStatus = "";
    private bool FullModEnableBinderMerge = true;
    private bool FullModIgnoreMetadata = true;
    private bool FullModWriteReport = true;
    private string FullModConflictFilter = "";
    private string ParamConflictFilter = "";
    private const int ConflictRowsPerPage = 100;
    private int FullModConflictPage = 0;
    private int ParamConflictPage = 0;

    private bool DisplayMergeReportSummary = true;
    private bool DisplayBinderMergeSummary = true;
    private bool DisplayConflictResolver = true;
    private bool DisplayWarnings = true;
    private bool DisplayErrors = true;

    public ParamDeltaAutoMergeTool(ParamDeltaPatcher patcher)
    {
        Patcher = patcher;
        Engine = new ParamDeltaAutoMergeEngine(patcher);
        RegulationMerge = new ParamRegulationAutoMerge(patcher, Engine);
        FullModMerge = new ParamFullModAutoMerge(RegulationMerge);
    }
    public void DisplayConflictPolicy()
    {
        GUI.SimpleHeader(
            LOC.Get("PARAM_AutoMerge_Conflict_Policy_Header"),
            LOC.Get("PARAM_AutoMerge_Conflict_Policy_Header_TT"));

        var strategyName = GetStrategyName(Strategy);
        if (ImGui.BeginCombo("##autoMergeStrategy", strategyName))
        {
            foreach (ParamDeltaConflictStrategy value in Enum.GetValues(typeof(ParamDeltaConflictStrategy)))
            {
                if (ImGui.Selectable(GetStrategyName(value), value == Strategy))
                {
                    Strategy = value;
                    LastResult = null;
                    LastRegulationAnalysis = null;
                    LastFullModAnalysis = null;
                    RegulationBuildStatus = "";
                    FullModBuildStatus = "";
                }
            }
            ImGui.EndCombo();
        }
    }

    public void DisplayDeltaPatchMerge()
    {
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("PARAM_AutoMerge_Delta_Merge_Source_List_Header"),
            LOC.Get("PARAM_AutoMerge_Delta_Merge_Source_List_Header_TT"));

        // Select All
        if (ImGui.Button($"{Icons.Bars}##selectAllAction", DPI.IconButtonSize))
        {
            foreach (var entry in Patcher.Selection.ImportList)
            {
                if (entry.Delta.ProjectType == Patcher.Project.Descriptor.ProjectType &&
                    entry.Delta.ParamVersion == Patcher.Project.Handler.ParamData.PrimaryBank.ParamVersion)
                    SelectedFiles.Add(entry.Filename);
            }
        }
        GUI.Tooltip(LOC.Get("PARAM_AutoMerge_Delta_Merge_Select_All_TT"));

        ImGui.SameLine();

        // Clear Selection
        if (ImGui.Button($"{Icons.Minus}##clearSelectionAction", DPI.IconButtonSize))
        {
            SelectedFiles.Clear();
            LastResult = null;
        }
        GUI.Tooltip(LOC.Get("PARAM_AutoMerge_Delta_Merge_Clear_Selection_TT"));

        ImGui.SameLine();

        // Refresh List
        if (ImGui.Button($"{Icons.Refresh}##refreshListAction", DPI.IconButtonSize))
        {
            Patcher.Selection.RefreshImportList();
            LastResult = null;
        }
        GUI.Tooltip(LOC.Get("PARAM_AutoMerge_Delta_Merge_Refresh_List_TT"));

        // List
        ImGui.BeginChild("autoMergeSourceList", new System.Numerics.Vector2(0, 180), ImGuiChildFlags.Borders);

        foreach (var entry in Patcher.Selection.ImportList)
        {
            var isSelected = SelectedFiles.Contains(entry.Filename);
            var version = ParamUtils.ParseRegulationVersion(entry.Delta.ParamVersion);
            var sameGame = entry.Delta.ProjectType == Patcher.Project.Descriptor.ProjectType;
            var sameVersion = entry.Delta.ParamVersion == Patcher.Project.Handler.ParamData.PrimaryBank.ParamVersion;
            var compatibility = !sameGame ? LOC.Get("PARAM_AutoMerge_Delta_Merge_Diff_Game") : !sameVersion ? LOC.Get("PARAM_AutoMerge_Delta_Merge_Diff_Param_Ver") : "";
            var label = $"{entry.Filename} [{version}]{compatibility}##autoMerge_{entry.Filename.GetHashCode()}";

            if (ImGui.Checkbox(label, ref isSelected))
            {
                if (isSelected)
                    SelectedFiles.Add(entry.Filename);
                else
                    SelectedFiles.Remove(entry.Filename);

                LastResult = null;
            }
        }

        ImGui.EndChild();

        // Output Filename
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("PARAM_AutoMerge_Delta_Merge_Output_Filename_Header"),
            LOC.Get("PARAM_AutoMerge_Delta_Merge_Output_Filename_Header_TT"));

        ImGui.InputTextWithHint("##autoMergeOutputName", LOC.Get("PARAM_AutoMerge_Delta_Merge_Output_Filename_Hint"), 
            ref OutputName, 255);

        // Actions
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("PARAM_AutoMerge_Actions_Header"),
            LOC.Get("PARAM_AutoMerge_Actions_Header_TT"));

        GUI.MultiButtonInput("deltaMergeActions",
            "analyze",
            LOC.Get("PARAM_AutoMerge_Analyze_Delta_Merge_Action"),
            LOC.Get("PARAM_AutoMerge_Analyze_Delta_Merge_Action_TT"),
            AnalyzeDeltaPatches);

        if (LastResult == null)
            return;

        // Summary
        GUI.Spacer();
        DisplayMergeSummary(LastResult, "delta");

        if (!LastResult.CanApply)
            return;

        GUI.MultiButtonInput("summaryMergeActions",
            "saveMergedDelta",
            LOC.Get("PARAM_AutoMerge_Save_Merged_Delta"),
            LOC.Get("PARAM_AutoMerge_Save_Merged_Delta_TT"),
            SaveMergedDelta,

            "importMergedDelta",
            LOC.Get("PARAM_AutoMerge_Import_Merged_Delta"),
            LOC.Get("PARAM_AutoMerge_Import_Merged_Delta_TT"),
            ImportMergedDelta);
    }

    private void SaveMergedDelta()
    {
        Engine.ApplyConflictResolutions(LastResult);
        var name = string.IsNullOrWhiteSpace(OutputName) ? "auto_merged" : OutputName.Trim();
        Patcher.WriteDeltaPatch(LastResult.Patch, name);
        Patcher.Selection.RefreshImportList();
    }

    private void ImportMergedDelta()
    {
        Engine.ApplyConflictResolutions(LastResult);
        var name = string.IsNullOrWhiteSpace(OutputName) ? "auto_merged" : OutputName.Trim();
        Patcher.Importer.ImportDelta(name, LastResult.Patch);
    }

    public void DisplayDirectRegulationMerge()
    {
        if (!RegulationMerge.IsSupportedProject)
        {
            GUI.WrappedText(
                LOC.Get("PARAM_DirectMerge_Supported_Project_Type", RegulationMerge.SupportedProjectText));
            return;
        }

        // Options
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("PARAM_DirectMerge_Options_Header"),
            LOC.Get("PARAM_DirectMerge_Options_Header_TT"));

        var autoUpgradeRegulation = RegulationMerge.AutoUpgradeMismatchedVersions;

        // Upgrade Param Version for Older Regulations
        if (ImGui.Checkbox($"{LOC.Get("PARAM_DirectMerge_Apply_ParamVer_AutoUpgrade")}##autoMergeRegUpgrade", ref autoUpgradeRegulation))
        {
            RegulationMerge.AutoUpgradeMismatchedVersions = autoUpgradeRegulation;
            LastRegulationAnalysis = null;
        }

        // Sources
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("PARAM_DirectMerge_Sources_Header"),
            LOC.Get("PARAM_DirectMerge_Sources_Header_TT"));

        // Add
        if (ImGui.Button($"{Icons.Plus}##addEntryAction", DPI.IconButtonSize))
        {
            RegulationPaths.Add("");
            LastRegulationAnalysis = null;
        }
        GUI.Tooltip(LOC.Get("PARAM_DirectMerge_Add_Regulation_Source_TT"));

        ImGui.SameLine();

        // Remove
        if (RegulationPaths.Count < 2)
        {
            ImGui.BeginDisabled();

            if (ImGui.Button($"{Icons.Minus}##removeEntryAction", DPI.IconButtonSize))
            {
            }
            GUI.Tooltip(LOC.Get("PARAM_DirectMerge_Remove_Regulation_Source_TT"));

            ImGui.EndDisabled();
        }
        else
        {
            if (ImGui.Button($"{Icons.Minus}##mapSelectionRemove", DPI.IconButtonSize))
            {
                RegulationPaths.RemoveAt(RegulationPaths.Count - 1);
                LastRegulationAnalysis = null;
            }
            GUI.Tooltip(LOC.Get("PARAM_DirectMerge_Remove_Regulation_Source_TT"));
        }

        ImGui.SameLine();

        // Reset
        if (ImGui.Button($"{LOC.Get("PARAM_DirectMerge_Reset_Source_List")}##resetEntryList", DPI.SelectorButtonSize))
        {
            for (var i = 0; i < RegulationPaths.Count; i++)
                RegulationPaths[i] = "";

            LastRegulationAnalysis = null;
            RegulationOutputPath = "";
            RegulationBuildStatus = "";
        }
        GUI.Tooltip(LOC.Get("PARAM_DirectMerge_Reset_Source_List_TT"));

        // Sources
        for (var i = 0; i < RegulationPaths.Count; i++)
        {
            // Select
            if(ImGui.Button($"{LOC.Get("PARAM_DirectMerge_Select_Path")}##selectPath{i}", DPI.SelectorButtonSize))
            {
                var dialog = PlatformUtils.Instance.OpenFileDialog(LOC.Get("PARAM_DirectMerge_Select_Regulation"), out var path);

                if(dialog)
                {
                    RegulationPaths[i] = path;
                    LastRegulationAnalysis = null;
                    RegulationBuildStatus = "";
                }
            }

            ImGui.SameLine();

            var value = RegulationPaths[i];
            if (ImGui.InputText($"{LOC.Get("PARAM_DirectMerge_Source", i + 1)}##autoMergeRegSource{i}", ref value, 1024))
            {
                RegulationPaths[i] = value;
                LastRegulationAnalysis = null;
                RegulationBuildStatus = "";
            }
        }

        GUI.MultiButtonInput("directMergeActions",
            "analyze",
            LOC.Get("PARAM_DirectMerge_Analyze_Regulation_Files"),
            LOC.Get("PARAM_DirectMerge_Analyze_Regulation_Files_TT"),
            AnalyzeRegulations);

        if (LastRegulationAnalysis == null)
            return;

        if (LastRegulationAnalysis.Sources.Count > 0)
        {
            GUI.Spacer();
            GUI.SimpleHeader(
                LOC.Get("PARAM_DirectMerge_Loaded_Sources_Header"),
                LOC.Get("PARAM_DirectMerge_Loaded_Sources_Header_TT"));

            foreach (var source in LastRegulationAnalysis.Sources)
            {
                var versionText = source.AutoUpgraded
                    ? $"{ParamUtils.ParseRegulationVersion(source.OriginalParamVersion)} -> {ParamUtils.ParseRegulationVersion(source.ParamVersion)} {LOC.Get("PARAM_DirectMerge_Loaded_Autoupgraded")}"
                    : ParamUtils.ParseRegulationVersion(source.ParamVersion);

                GUI.WrappedText(
                    $"• {source.Filename} | {versionText} | " +
                    $"{source.ParsedParamCount} {LOC.Get("PARAM_DirectMerge_Loaded_Params")} | {source.Delta.Params.Sum(e => e.Rows.Count)} {LOC.Get("PARAM_DirectMerge_Loaded_Changed_Rows")}");
            }
        }

        if (LastRegulationAnalysis.Warnings.Count > 0)
        {
            GUI.Spacer();
            GUI.ConditionalHeader(
                LOC.Get("PARAM_DirectMerge_Warnings_Header"),
                LOC.Get("PARAM_DirectMerge_Warnings_Header_TT"),
                ref DisplayWarnings);

            if(DisplayWarnings)
            {
                ImGui.BeginChild("warningsSection", new Vector2(0, 100));

                foreach (var warning in LastRegulationAnalysis.Warnings.Take(100))
                {
                    GUI.WrappedText($"• {warning}");
                }

                ImGui.EndChild();
            }
        }

        if (LastRegulationAnalysis.Errors.Count > 0)
        {
            GUI.Spacer();
            GUI.ConditionalHeader(
                LOC.Get("PARAM_DirectMerge_Errors_Header"),
                LOC.Get("PARAM_DirectMerge_Errors_Header_TT"),
                ref DisplayErrors);

            if (DisplayErrors)
            {
                foreach (var error in LastRegulationAnalysis.Errors)
                {
                    GUI.WrappedText($"• {error}");
                }
            }

            return;
        }

        if (LastRegulationAnalysis.MergeResult == null)
            return;

        GUI.Spacer();
        DisplayMergeSummary(LastRegulationAnalysis.MergeResult, "reg");

        if (!LastRegulationAnalysis.MergeResult.CanApply)
            return;

        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("PARAM_DirectMerge_Output_Header"),
            LOC.Get("PARAM_DirectMerge_Output_Header_TT"));

        // Output Path
        if (ImGui.Button($"{LOC.Get("PARAM_DirectMerge_Select_Path")}##selectOutputPath", DPI.SelectorButtonSize))
        {
            var dialog = PlatformUtils.Instance.OpenFileDialog(LOC.Get("PARAM_DirectMerge_Select_Regulation"), out var path);

            if (dialog)
            {
                RegulationOutputPath = path;
            }
        }

        ImGui.SameLine();

        ImGui.InputTextWithHint(
            $"{LOC.Get("PARAM_DirectMerge_OutputPath")}##autoMergeRegOutput", 
            LOC.Get("PARAM_DirectMerge_OutputPath_Hint"),
            ref RegulationOutputPath, 1024);

        GUI.Spacer();

        GUI.MultiButtonInput("directMergeBuildActions",
            "buildRegulation",
            LOC.Get("PARAM_DirectMerge_Build_Regulation_Action"),
            LOC.Get("PARAM_DirectMerge_Build_Regulation_Action_TT"),
            BuildRegulation,

            "importMergedChanges",
            LOC.Get("PARAM_DirectMerge_Import_Merged_Changes"),
            LOC.Get("PARAM_DirectMerge_Import_Merged_Changes_TT"),
            ImportMergedRegulationChanges,

            "saveMergedChanges",
            LOC.Get("PARAM_DirectMerge_Save_Merged_Delta"),
            LOC.Get("PARAM_DirectMerge_Save_Merged_Delta_TT"),
            SaveMergedRegulationChanges);

        if (!string.IsNullOrWhiteSpace(RegulationBuildStatus))
        {
            GUI.Spacer();
            GUI.WrappedText(RegulationBuildStatus);
        }
    }

    private void ImportMergedRegulationChanges()
    {
        Engine.ApplyConflictResolutions(LastRegulationAnalysis.MergeResult);
        Patcher.Importer.ImportDelta("direct_regulation_auto_merge", LastRegulationAnalysis.MergeResult.Patch);
        RegulationBuildStatus = LOC.Get("PARAM_DirectMerge_Imported_Merged_Changes");
    }

    private void SaveMergedRegulationChanges()
    {
        Engine.ApplyConflictResolutions(LastRegulationAnalysis.MergeResult);
        var name = string.IsNullOrWhiteSpace(OutputName) ? "auto_merged" : OutputName.Trim();
        Patcher.WriteDeltaPatch(LastRegulationAnalysis.MergeResult.Patch, name);
        Patcher.Selection.RefreshImportList();
        RegulationBuildStatus = LOC.Get("PARAM_DirectMerge_Saved_Merged_Changes", name);
    }

    public void DisplayFullModMerge()
    {
        // Options
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("PARAM_ProjectMerge_Options_Header"),
            LOC.Get("PARAM_ProjectMerge_Options_Header_TT"));

        // Merge Binder Containers by Internal Entry
        if (ImGui.Checkbox($"{LOC.Get("PARAM_ProjectMerge_Merge_By_Internal_Entry")}##autoMergeFullBinder", ref FullModEnableBinderMerge))
        {
            LastFullModAnalysis = null;
        }
        GUI.Tooltip(LOC.Get("PARAM_ProjectMerge_Merge_By_Internal_Entry_TT"));

        // Auto-upgrade Regulation Versions
        var autoUpgradeRegulation = RegulationMerge.AutoUpgradeMismatchedVersions;
        if (ImGui.Checkbox($"{LOC.Get("PARAM_ProjectMerge_AutoUpgrade_Regulation")}##autoMergeFullUpgradeReg", ref autoUpgradeRegulation))
        {
            RegulationMerge.AutoUpgradeMismatchedVersions = autoUpgradeRegulation;
            LastFullModAnalysis = null;
        }
        GUI.Tooltip(LOC.Get("PARAM_ProjectMerge_AutoUpgrade_Regulation_TT"));

        // Ignore Metadata Files
        if (ImGui.Checkbox($"{LOC.Get("PARAM_ProjectMerge_Merge_Metadata")}##autoMergeFullMetadata", ref FullModIgnoreMetadata))
        {
            LastFullModAnalysis = null;
        }
        GUI.Tooltip(LOC.Get("PARAM_ProjectMerge_Merge_Metadata_TT"));

        // Write Merge Report
        ImGui.Checkbox($"{LOC.Get("PARAM_ProjectMerge_Write_Merge_Report")}##autoMergeFullReport", ref FullModWriteReport);
        GUI.Tooltip(LOC.Get("PARAM_ProjectMerge_Write_Merge_Report_TT"));

        // Sources
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("PARAM_ProjectMerge_Sources_Header"),
            LOC.Get("PARAM_ProjectMerge_Sources_Header_TT"));


        // Add
        if (ImGui.Button($"{Icons.Plus}##addEntryAction", DPI.IconButtonSize))
        {
            FullModFolderPaths.Add("");
            LastFullModAnalysis = null;
        }
        GUI.Tooltip(LOC.Get("PARAM_ProjectMerge_Add_Project_Source_TT"));

        ImGui.SameLine();

        // Remove
        if (FullModFolderPaths.Count < 2)
        {
            ImGui.BeginDisabled();

            if (ImGui.Button($"{Icons.Minus}##removeEntryAction", DPI.IconButtonSize))
            {
            }
            GUI.Tooltip(LOC.Get("PARAM_ProjectMerge_Remove_Project_Source_TT"));

            ImGui.EndDisabled();
        }
        else
        {
            if (ImGui.Button($"{Icons.Minus}##mapSelectionRemove", DPI.IconButtonSize))
            {
                FullModFolderPaths.RemoveAt(FullModFolderPaths.Count - 1);
                LastFullModAnalysis = null;
            }
            GUI.Tooltip(LOC.Get("PARAM_ProjectMerge_Remove_Project_Source_TT"));
        }

        ImGui.SameLine();

        // Reset
        if (ImGui.Button($"{LOC.Get("PARAM_ProjectMerge_Reset_Source_List")}##resetEntryList", DPI.SelectorButtonSize))
        {
            for (var i = 0; i < FullModFolderPaths.Count; i++)
            {
                FullModFolderPaths[i] = "";
            }

            LastFullModAnalysis = null;
            FullModOutputPath = "";
            FullModBuildStatus = "";
        }
        GUI.Tooltip(LOC.Get("PARAM_ProjectMerge_Reset_Source_List_TT"));

        // Sources
        for (var i = 0; i < FullModFolderPaths.Count; i++)
        {
            // Select
            if (ImGui.Button($"{LOC.Get("PARAM_ProjectMerge_Select_Path")}##selectPath{i}", DPI.SelectorButtonSize))
            {
                var dialog = PlatformUtils.Instance.OpenFolderDialog(LOC.Get("PARAM_ProjectMerge_Select_Project_Folder"), out var path);

                if (dialog)
                {
                    FullModFolderPaths[i] = path;
                    LastFullModAnalysis = null;
                    FullModBuildStatus = "";
                }
            }

            ImGui.SameLine();

            var value = FullModFolderPaths[i];
            if (ImGui.InputText($"{LOC.Get("PARAM_ProjectMerge_Source", i + 1)}##autoMergeProjectSource{i}", ref value, 1024))
            {
                FullModFolderPaths[i] = value;
                LastFullModAnalysis = null;
                FullModBuildStatus = "";
            }
        }

        GUI.MultiButtonInput("analyzeActions",
            "analyze",
            LOC.Get("PARAM_ProjectMerge_Analyze_Mod_Folders"),
            LOC.Get("PARAM_ProjectMerge_Analyze_Mod_Folders_TT"),
            AnalyzeFullModFolders);

        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("PARAM_ProjectMerge_Output_Header"),
            LOC.Get("PARAM_ProjectMerge_Output_Header_TT"));

        // Select
        if (ImGui.Button($"{LOC.Get("PARAM_ProjectMerge_Select_Path")}##selectPath_output", DPI.SelectorButtonSize))
        {
            var dialog = PlatformUtils.Instance.OpenFolderDialog(LOC.Get("PARAM_ProjectMerge_Select_Project_Folder"), out var path);

            if (dialog)
            {
                FullModOutputPath = path;
            }
        }

        ImGui.SameLine();

        ImGui.InputText($"{LOC.Get("PARAM_ProjectMerge_Output_Folder")}##autoMergeFullOutput", ref FullModOutputPath, 1024);

        if (LastFullModAnalysis == null)
            return;

        // Actions
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("PARAM_ProjectMerge_Actions_Header"),
            LOC.Get("PARAM_ProjectMerge_Actions_Header_TT"));

        GUI.ConditionalMultiButtonInput("buildActions",
            "build",
            LOC.Get("PARAM_ProjectMerge_Build_Merge"),
            LOC.Get("PARAM_ProjectMerge_Build_Merge_TT"),
            BuildFullModFolder,
            LastFullModAnalysis.CanBuild);

        if (!string.IsNullOrWhiteSpace(FullModBuildStatus))
        {
            GUI.Spacer();
            GUI.WrappedText(FullModBuildStatus);
        }

        // Summary
        GUI.Spacer();
        GUI.ConditionalHeader(
            LOC.Get("PARAM_ProjectMerge_Summary_Header"),
            LOC.Get("PARAM_ProjectMerge_Summary_Header_TT"),
            ref DisplayMergeReportSummary);

        if (DisplayMergeReportSummary)
        {
            ImGui.Text($"{LOC.Get("PARAM_ProjectMerge_Source_Folders", LastFullModAnalysis.SourceFolders.Count)}");
            ImGui.Text($"{LOC.Get("PARAM_ProjectMerge_Files_Scanned", LastFullModAnalysis.ScannedFiles)}");
            ImGui.Text($"{LOC.Get("PARAM_ProjectMerge_Unique_Files_to_Copy", LastFullModAnalysis.UniqueFiles)}");
            ImGui.Text($"{LOC.Get("PARAM_ProjectMerge_Identical_Duplicate_Files", LastFullModAnalysis.IdenticalFiles)}");
            ImGui.Text($"{LOC.Get("PARAM_ProjectMerge_Binder_Files_to_Merge", LastFullModAnalysis.BinderFiles)}");
            ImGui.Text($"{LOC.Get("PARAM_ProjectMerge_Regulation_Merges", LastFullModAnalysis.RegulationFiles)}");
            ImGui.Text($"{LOC.Get("PARAM_ProjectMerge_Ignored_Metadata_Files", LastFullModAnalysis.IgnoredFiles)}");
            ImGui.Text($"{LOC.Get("PARAM_ProjectMerge_Conflicts", LastFullModAnalysis.Conflicts.Count)}");
        }

        if (LastFullModAnalysis.Files.Any(e => e.Action == FullModMergeAction.BinderMerge))
        {
            GUI.Spacer();
            GUI.ConditionalHeader(
                LOC.Get("PARAM_ProjectMerge_Binder_Merge_Summary_Header"),
                LOC.Get("PARAM_ProjectMerge_Binder_Merge_Summary_Header_TT"),
                ref DisplayBinderMergeSummary);

            if(DisplayBinderMergeSummary)
            {
                foreach (var plan in LastFullModAnalysis.Files.Where(e => e.Action == FullModMergeAction.BinderMerge).Take(100))
                {
                    var summary = plan.BinderSummary;
                    if (summary == null)
                        continue;

                    GUI.WrappedText(
                        LOC.Get("PARAM_ProjectMerge_Binder_Merge_Log",
                        plan.RelativePath,
                        summary.AddedEntries,
                        summary.IdenticalEntries,
                        summary.NestedBinderMerges,
                        summary.MatbinSemanticMerges,
                        summary.Conflicts));
                }
            }
        }

        // Warnings
        if (LastFullModAnalysis.Warnings.Count > 0)
        {
            GUI.Spacer();
            GUI.ConditionalHeader(
                LOC.Get("PARAM_ProjectMerge_Warnings_Header"),
                LOC.Get("PARAM_ProjectMerge_Warnings_Header_TT", LastFullModAnalysis.Warnings.Count),
                ref DisplayWarnings);

            if (DisplayWarnings)
            {
                ImGui.BeginChild("warningsSection", new Vector2(0, 100));

                foreach (var warning in LastFullModAnalysis.Warnings.Take(200))
                {
                    GUI.WrappedText($"• {warning}");
                }

                ImGui.EndChild();
            }
        }

        if (LastFullModAnalysis.Errors.Count > 0)
        {
            GUI.Spacer();
            GUI.ConditionalHeader(
                LOC.Get("PARAM_ProjectMerge_Errors_Header"),
                LOC.Get("PARAM_ProjectMerge_Errors_Header_TT", LastFullModAnalysis.Errors.Count),
                ref DisplayErrors);

            if (DisplayErrors)
            {
                foreach (var error in LastFullModAnalysis.Errors)
                {
                    GUI.WrappedText($"• {error}");
                }
            }

            return;
        }

        if (LastFullModAnalysis.Conflicts.Any(e => e.Type != FullModMergeConflictType.Regulation))
        {
            GUI.Spacer();
            DisplayFullModConflictResolver(LastFullModAnalysis);
        }

        if (LastFullModAnalysis.RegulationAnalysis?.MergeResult?.Conflicts.Count > 0)
        {
            GUI.Spacer();
            GUI.SimpleHeader(
                LOC.Get("PARAM_ProjectMerge_RegulationConflicts_Header"),
                LOC.Get("PARAM_ProjectMerge_RegulationConflicts_Header_TT"));

            DisplayMergeSummary(LastFullModAnalysis.RegulationAnalysis.MergeResult, "fullreg");
        }
    }

    private void DisplayFullModConflictResolver(FullModMergeAnalysis analysis)
    {
        var editableConflicts = analysis.Conflicts.Where(e => e.Type != FullModMergeConflictType.Regulation).ToList();
        var unresolved = editableConflicts.Count(e => !e.IsResolved);

        GUI.ConditionalHeader(
            LOC.Get("PARAM_ProjectMerge_FileConflictResolve_Header", editableConflicts.Count),
            LOC.Get("PARAM_ProjectMerge_FileConflictResolve_Header_TT"),
            ref DisplayConflictResolver);

        if (DisplayConflictResolver)
        {
            ImGui.Text(LOC.Get("PARAM_ProjectMerge_Resolved_Unresolved_Hint", editableConflicts.Count - unresolved, unresolved));

            ImGui.InputText(
                $"{LOC.Get("PARAM_AutoMerge_ConflictEditor_Filter")}##autoMergeFullConflictFilter",
                ref FullModConflictFilter, 512);

            // Visible -> Earlier
            if (ImGui.Button($"{LOC.Get("PARAM_AutoMerge_ConflictEditor_Visible_Earlier")}##autoMergeFullResolveEarlier"))
            {
                foreach (var conflict in FilterFullModConflicts(analysis))
                {
                    conflict.Resolution = FullModConflictResolution.UseEarlier;
                }
            }

            ImGui.SameLine();

            // Visible -> Later
            if (ImGui.Button($"{LOC.Get("PARAM_AutoMerge_ConflictEditor_Visible_Later")}##autoMergeFullResolveLater"))
            {
                foreach (var conflict in FilterFullModConflicts(analysis))
                {
                    conflict.Resolution = FullModConflictResolution.UseLater;
                }
            }

            ImGui.SameLine();

            // Reset Visible
            if (ImGui.Button($"{LOC.Get("PARAM_AutoMerge_ConflictEditor_Visible_Reset")}##autoMergeFullResolveReset"))
            {
                foreach (var conflict in FilterFullModConflicts(analysis))
                {
                    conflict.Resolution = FullModConflictResolution.Unresolved;
                }
            }

            var filtered = FilterFullModConflicts(analysis).ToList();
            var pageCount = Math.Max(1, (filtered.Count + ConflictRowsPerPage - 1) / ConflictRowsPerPage);
            FullModConflictPage = Math.Clamp(FullModConflictPage, 0, pageCount - 1);

            if (ImGui.Button($"{LOC.Get("PARAM_AutoMerge_ConflictEditor_Page_Back")}##autoMergeFullConflictPrev") && FullModConflictPage > 0)
            {
                FullModConflictPage--;
            }

            ImGui.SameLine();

            ImGui.Text($"{LOC.Get("PARAM_AutoMerge_ConflictEditor_Page", FullModConflictPage + 1, pageCount, filtered.Count)}");

            ImGui.SameLine();

            if (ImGui.Button($"{LOC.Get("PARAM_AutoMerge_ConflictEditor_Page_Next")}##autoMergeFullConflictNext") && FullModConflictPage + 1 < pageCount)
            {
                FullModConflictPage++;
            }

            var page = filtered.Skip(FullModConflictPage * ConflictRowsPerPage).Take(ConflictRowsPerPage);

            if (ImGui.BeginTable(
                    "autoMergeFullConflictTable",
                    6,
                    ImGuiTableFlags.Resizable | ImGuiTableFlags.BordersOuterH | ImGuiTableFlags.BordersOuterV |
                    ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingStretchSame))
            {
                ImGui.TableSetupColumn(LOC.Get("PARAM_AutoMerge_ConflictTable_Path"));
                ImGui.TableSetupColumn(LOC.Get("PARAM_AutoMerge_ConflictTable_Earlier"));
                ImGui.TableSetupColumn(LOC.Get("PARAM_AutoMerge_ConflictTable_Later"));
                ImGui.TableSetupColumn(LOC.Get("PARAM_AutoMerge_ConflictTable_Resolution"));
                ImGui.TableSetupColumn(LOC.Get("PARAM_AutoMerge_ConflictTable_Manual_Value"));
                ImGui.TableSetupColumn(LOC.Get("PARAM_AutoMerge_ConflictEditor_Reason"));
                ImGui.TableHeadersRow();

                var index = FullModConflictPage * ConflictRowsPerPage;
                foreach (var conflict in page)
                {
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    GUI.WrappedText(string.IsNullOrWhiteSpace(conflict.InternalPath)
                        ? conflict.RelativePath
                        : $"{conflict.RelativePath} :: {conflict.InternalPath}");

                    ImGui.TableSetColumnIndex(1);
                    GUI.WrappedText(string.IsNullOrWhiteSpace(conflict.ExistingValue)
                        ? conflict.ExistingSource
                        : $"{conflict.ExistingValue}\n{conflict.ExistingSource}");

                    ImGui.TableSetColumnIndex(2);
                    GUI.WrappedText(string.IsNullOrWhiteSpace(conflict.IncomingValue)
                        ? conflict.IncomingSource
                        : $"{conflict.IncomingValue}\n{conflict.IncomingSource}");

                    ImGui.TableSetColumnIndex(3);
                    var label = GetFullModResolutionName(conflict.Resolution);
                    if (ImGui.BeginCombo($"##fullConflictResolution{index}", label))
                    {
                        foreach (FullModConflictResolution resolution in Enum.GetValues(typeof(FullModConflictResolution)))
                        {
                            if (resolution == FullModConflictResolution.Manual && !conflict.SupportsManual)
                                continue;

                            if (ImGui.Selectable(GetFullModResolutionName(resolution), conflict.Resolution == resolution))
                            {
                                conflict.Resolution = resolution;
                                if (resolution == FullModConflictResolution.Manual && string.IsNullOrEmpty(conflict.ManualValue))
                                    conflict.ManualValue = conflict.ExistingValue;
                            }
                        }
                        ImGui.EndCombo();
                    }

                    ImGui.TableSetColumnIndex(4);
                    if (conflict.SupportsManual && conflict.Resolution == FullModConflictResolution.Manual)
                    {
                        var manual = conflict.ManualValue ?? "";
                        if (ImGui.InputText($"##fullConflictManual{index}", ref manual, 1024))
                        {
                            conflict.ManualValue = manual;
                        }
                    }
                    else
                    {
                        ImGui.TextDisabled("-");
                    }

                    ImGui.TableSetColumnIndex(5);
                    GUI.WrappedText(conflict.Message);
                    index++;
                }

                ImGui.EndTable();
            }
        }
    }

    private IEnumerable<FullModMergeConflict> FilterFullModConflicts(FullModMergeAnalysis analysis)
    {
        var source = analysis.Conflicts.Where(e => e.Type != FullModMergeConflictType.Regulation);
        if (string.IsNullOrWhiteSpace(FullModConflictFilter))
            return source;

        var filter = FullModConflictFilter.Trim();
        return source.Where(e =>
            e.RelativePath.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
            e.InternalPath.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
            e.ExistingSource.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
            e.IncomingSource.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
            e.Message.Contains(filter, StringComparison.OrdinalIgnoreCase));
    }

    private static string GetFullModResolutionName(FullModConflictResolution resolution)
    {
        return resolution switch
        {
            FullModConflictResolution.UseEarlier => LOC.Get("PARAM_AutoMerge_ConflictResolution_UseEarlier"),
            FullModConflictResolution.UseLater => LOC.Get("PARAM_AutoMerge_ConflictResolution_UseLater"),
            FullModConflictResolution.Manual => LOC.Get("PARAM_AutoMerge_ConflictResolution_Manual"),
            _ => LOC.Get("PARAM_AutoMerge_ConflictResolution_Unresolved")
        };
    }

    private void AnalyzeFullModFolders()
    {
        FullModBuildStatus = "";
        FullModConflictPage = 0;
        LastFullModAnalysis = FullModMerge.Analyze(
            FullModFolderPaths,
            Strategy,
            FullModEnableBinderMerge,
            FullModIgnoreMetadata);

        if (LastFullModAnalysis.SourceFolders.Count > 0 && string.IsNullOrWhiteSpace(FullModOutputPath))
        {
            var first = LastFullModAnalysis.SourceFolders[0];
            var parent = System.IO.Directory.GetParent(first)?.FullName ?? first;
            FullModOutputPath = System.IO.Path.Combine(parent, "Merged_Mod");
        }
    }

    private void BuildFullModFolder()
    {
        try
        {
            FullModMerge.Build(LastFullModAnalysis, FullModOutputPath, FullModWriteReport);
            var cleanOutputPath = FullModOutputPath.Trim().Trim('"');
            FullModBuildStatus = LOC.Get("PARAM_ProjectMerge_Merged_Mod", Path.GetFullPath(cleanOutputPath));
        }
        catch (Exception ex)
        {
            FullModBuildStatus = LOC.Get("PARAM_ProjectMerge_Merged_Mod_Failed", ex.Message);
        }
    }

    private void AnalyzeRegulations()
    {
        RegulationBuildStatus = "";
        LastRegulationAnalysis = RegulationMerge.Analyze(RegulationPaths, Strategy);

        if (LastRegulationAnalysis.Sources.Count > 0 && string.IsNullOrWhiteSpace(RegulationOutputPath))
        {
            var firstPath = LastRegulationAnalysis.Sources[0].Path;
            var directory = System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(firstPath));
            RegulationOutputPath = System.IO.Path.Combine(directory ?? "", "merged_regulation.bin");
        }
    }

    private void BuildRegulation()
    {
        try
        {
            RegulationMerge.BuildMergedRegulation(LastRegulationAnalysis, RegulationOutputPath);
            var cleanOutputPath = RegulationOutputPath.Trim().Trim('"');
            RegulationBuildStatus = LOC.Get("PARAM_DirectMerge_Merged_Regulation", Path.GetFullPath(cleanOutputPath));
        }
        catch (Exception ex)
        {
            RegulationBuildStatus = LOC.Get("PARAM_ProjectMerge_Merged_Mod_Failed", ex.Message);
        }
    }

    private void AnalyzeDeltaPatches()
    {
        var sources = Patcher.Selection.ImportList
            .Where(e => SelectedFiles.Contains(e.Filename))
            .ToList();

        LastResult = Engine.Merge(sources, Strategy);
    }

    private void DisplayMergeSummary(ParamDeltaAutoMergeResult result, string idSuffix)
    {
        GUI.SimpleHeader(
            LOC.Get("PARAM_AutoMerge_Merge_Summary_Header"),
            LOC.Get("PARAM_AutoMerge_Merge_Summary_Header_TT"));

        ImGui.Text(LOC.Get("PARAM_AutoMerge_Summary_Sources", result.SourceCount));
        ImGui.Text(LOC.Get("PARAM_AutoMerge_Summary_Rows_Merged", result.Patch.Params.Sum(e => e.Rows.Count)));
        ImGui.Text(LOC.Get("PARAM_AutoMerge_Summary_Safe_Field_Changes", result.SafeFields));
        ImGui.Text(LOC.Get("PARAM_AutoMerge_Summary_Identical_Fields", result.IdenticalFields));
        ImGui.Text(LOC.Get("PARAM_AutoMerge_Summary_Conflicts_Resolved_Unresolved", result.Conflicts.Count, result.ResolvedConflicts, result.UnresolvedConflicts));

        if (result.Errors.Count > 0)
        {
            GUI.Spacer();
            GUI.SimpleHeader(
                LOC.Get("PARAM_AutoMerge_Merge_Errors_Header"),
                LOC.Get("PARAM_AutoMerge_Merge_Errors_Header_TT"));

            foreach (var error in result.Errors)
            {
                GUI.WrappedText($"• {error}");
            }
        }

        if (result.Conflicts.Count > 0)
        {
            GUI.Spacer();

            if (ImGui.CollapsingHeader($"{LOC.Get("PARAM_AutoMerge_ConflictEditor_Header", result.Conflicts.Count)}##autoMergeConflicts_{idSuffix}", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.InputText($"{LOC.Get("PARAM_AutoMerge_ConflictEditor_Filter")}##paramConflictFilter_{idSuffix}", 
                    ref ParamConflictFilter, 512);

                var filtered = result.Conflicts.Where(conflict =>
                    string.IsNullOrWhiteSpace(ParamConflictFilter) ||
                    conflict.ParamName.Contains(ParamConflictFilter, StringComparison.OrdinalIgnoreCase) ||
                    conflict.Field.Contains(ParamConflictFilter, StringComparison.OrdinalIgnoreCase) ||
                    conflict.ExistingSource.Contains(ParamConflictFilter, StringComparison.OrdinalIgnoreCase) ||
                    conflict.IncomingSource.Contains(ParamConflictFilter, StringComparison.OrdinalIgnoreCase)).ToList();

                if (ImGui.Button($"{LOC.Get("PARAM_AutoMerge_ConflictEditor_Visible_Earlier")}##paramResolveEarlier_{idSuffix}"))
                {
                    foreach (var conflict in filtered)
                    {
                        conflict.Resolution = ParamDeltaConflictResolution.UseEarlier;
                    }

                    Engine.ApplyConflictResolutions(result);
                }

                ImGui.SameLine();
                if (ImGui.Button($"{LOC.Get("PARAM_AutoMerge_ConflictEditor_Visible_Later")}##paramResolveLater_{idSuffix}"))
                {
                    foreach (var conflict in filtered)
                    {
                        conflict.Resolution = ParamDeltaConflictResolution.UseLater;
                    }

                    Engine.ApplyConflictResolutions(result);
                }

                ImGui.SameLine();
                if (ImGui.Button($"{LOC.Get("PARAM_AutoMerge_ConflictEditor_Visible_Reset")}##paramResolveReset_{idSuffix}"))
                {
                    foreach (var conflict in filtered)
                    {
                        conflict.Resolution = ParamDeltaConflictResolution.Unresolved;
                    }
                }

                var pageCount = Math.Max(1, (filtered.Count + ConflictRowsPerPage - 1) / ConflictRowsPerPage);
                ParamConflictPage = Math.Clamp(ParamConflictPage, 0, pageCount - 1);
                if (ImGui.Button($"{LOC.Get("PARAM_AutoMerge_ConflictEditor_Page_Back")}##paramConflictPrev_{idSuffix}") && ParamConflictPage > 0)
                {
                    ParamConflictPage--;
                }

                ImGui.SameLine();
                ImGui.Text(LOC.Get("PARAM_AutoMerge_ConflictEditor_Page", ParamConflictPage + 1, pageCount, filtered.Count));

                ImGui.SameLine();

                if (ImGui.Button($"{LOC.Get("PARAM_AutoMerge_ConflictEditor_Page_Next")}##paramConflictNext_{idSuffix}") && ParamConflictPage + 1 < pageCount)
                {
                    ParamConflictPage++;
                }

                var page = filtered.Skip(ParamConflictPage * ConflictRowsPerPage).Take(ConflictRowsPerPage);
                if (ImGui.BeginTable(
                        $"paramConflictTable_{idSuffix}",
                        6,
                        ImGuiTableFlags.Resizable | ImGuiTableFlags.BordersOuterH | ImGuiTableFlags.BordersOuterV |
                        ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingStretchSame))
                {
                    ImGui.TableSetupColumn(LOC.Get("PARAM_AutoMerge_ConflictTable_ParamRowField"));
                    ImGui.TableSetupColumn(LOC.Get("PARAM_AutoMerge_ConflictTable_Earlier"));
                    ImGui.TableSetupColumn(LOC.Get("PARAM_AutoMerge_ConflictTable_Later"));
                    ImGui.TableSetupColumn(LOC.Get("PARAM_AutoMerge_ConflictTable_Resolution"));
                    ImGui.TableSetupColumn(LOC.Get("PARAM_AutoMerge_ConflictTable_Manual_Value"));
                    ImGui.TableSetupColumn(LOC.Get("PARAM_AutoMerge_ConflictTable_Type"));
                    ImGui.TableHeadersRow();

                    var index = ParamConflictPage * ConflictRowsPerPage;
                    foreach (var conflict in page)
                    {
                        ImGui.TableNextRow();
                        ImGui.TableSetColumnIndex(0);
                        GUI.WrappedText(conflict.Type == ParamDeltaMergeConflictType.FieldValue
                            ? $"{conflict.ParamName} / {conflict.RowID}:{conflict.RowIndex} / {conflict.Field}"
                            : $"{conflict.ParamName} / {conflict.RowID}:{conflict.RowIndex}");

                        ImGui.TableSetColumnIndex(1);
                        GUI.WrappedText(conflict.Type == ParamDeltaMergeConflictType.FieldValue
                            ? $"{conflict.ExistingSource}: {conflict.ExistingValue}"
                            : $"{conflict.ExistingSource}: {conflict.ExistingState}");

                        ImGui.TableSetColumnIndex(2);
                        GUI.WrappedText(conflict.Type == ParamDeltaMergeConflictType.FieldValue
                            ? $"{conflict.IncomingSource}: {conflict.IncomingValue}"
                            : $"{conflict.IncomingSource}: {conflict.IncomingState}");

                        ImGui.TableSetColumnIndex(3);
                        var resolutionLabel = GetParamResolutionName(conflict.Resolution);
                        if (ImGui.BeginCombo($"##paramResolution_{idSuffix}_{index}", resolutionLabel))
                        {
                            foreach (ParamDeltaConflictResolution resolution in Enum.GetValues(typeof(ParamDeltaConflictResolution)))
                            {
                                if (conflict.Type == ParamDeltaMergeConflictType.RowState && resolution == ParamDeltaConflictResolution.Manual)
                                    continue;

                                if (ImGui.Selectable(GetParamResolutionName(resolution), conflict.Resolution == resolution))
                                {
                                    conflict.Resolution = resolution;
                                    if (resolution == ParamDeltaConflictResolution.Manual && string.IsNullOrEmpty(conflict.ManualValue))
                                        conflict.ManualValue = conflict.ExistingValue;
                                    Engine.ApplyConflictResolutions(result);
                                }
                            }
                            ImGui.EndCombo();
                        }

                        ImGui.TableSetColumnIndex(4);
                        if (conflict.Type == ParamDeltaMergeConflictType.FieldValue && conflict.Resolution == ParamDeltaConflictResolution.Manual)
                        {
                            var manual = conflict.ManualValue ?? "";
                            if (ImGui.InputText($"##manualConflict_{idSuffix}_{index}", ref manual, 1024))
                            {
                                conflict.ManualValue = manual;
                                Engine.ApplyConflictResolutions(result);
                            }
                        }
                        else
                        {
                            ImGui.TextDisabled("-");
                        }

                        ImGui.TableSetColumnIndex(5);
                        ImGui.Text(conflict.Type == ParamDeltaMergeConflictType.FieldValue ? LOC.Get("PARAM_AutoMerge_ConflictEditor_Field") : LOC.Get("PARAM_AutoMerge_ConflictEditor_RowState"));
                        index++;
                    }

                    ImGui.EndTable();
                }
            }
        }

        Engine.ApplyConflictResolutions(result);

        if (!result.CanApply)
        {
            GUI.Spacer();
            GUI.WrappedText(LOC.Get("PARAM_AutoMerge_ConflictEditor_Cannot_Apply_Hint"));
        }
    }

    private static string GetParamResolutionName(ParamDeltaConflictResolution resolution)
    {
        return resolution switch
        {
            ParamDeltaConflictResolution.UseEarlier => LOC.Get("PARAM_AutoMerge_ConflictResolution_UseEarlier"),
            ParamDeltaConflictResolution.UseLater => LOC.Get("PARAM_AutoMerge_ConflictResolution_UseLater"),
            ParamDeltaConflictResolution.Manual => LOC.Get("PARAM_AutoMerge_ConflictResolution_Manual"),
            _ => LOC.Get("PARAM_AutoMerge_ConflictResolution_Unresolved")
        };
    }

    private static string GetStrategyName(ParamDeltaConflictStrategy strategy)
    {
        return strategy switch
        {
            ParamDeltaConflictStrategy.StopOnConflict => LOC.Get("PARAM_AutoMerge_ConflictStrategy_StopOnConflict"),
            ParamDeltaConflictStrategy.PreferFirst => LOC.Get("PARAM_AutoMerge_ConflictStrategy_PreferEarlierSource"),
            ParamDeltaConflictStrategy.PreferLast => LOC.Get("PARAM_AutoMerge_ConflictStrategy_PreferLaterSource"),
            _ => strategy.ToString()
        };
    }
}
