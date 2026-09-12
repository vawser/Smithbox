using Andre.Formats;
using Hexa.NET.ImGui;
using StudioCore.Application;
using System;
using System.Collections.Generic;
using System.Linq;

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
                Tag = $"Auto Merge ({sources.Count})"
            }
        };

        if (sources.Count < 2)
        {
            result.Errors.Add("Select at least two delta patches to auto merge.");
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
                    $"{source.Filename}: project type {source.Delta.ProjectType} does not match loaded project {projectType}.");
            }

            if (source.Delta.ParamVersion != paramVersion)
            {
                result.Errors.Add(
                    $"{source.Filename}: param version {ParamUtils.ParseRegulationVersion(source.Delta.ParamVersion)} " +
                    $"does not match loaded project {ParamUtils.ParseRegulationVersion(paramVersion)}.");
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

    public ParamDeltaAutoMergeTool(ParamDeltaPatcher patcher)
    {
        Patcher = patcher;
        Engine = new ParamDeltaAutoMergeEngine(patcher);
        RegulationMerge = new ParamRegulationAutoMerge(patcher, Engine);
        FullModMerge = new ParamFullModAutoMerge(RegulationMerge);
    }

    public void Display()
    {
        GUI.WrappedText(
            "Auto Merge can merge complete mod folders, regulation files, or Smithbox delta patches. " +
            "Folder merge copies unique files, deduplicates identical files, performs field-level regulation merge, " +
            "and can merge BND3/BND4 containers by their internal entries instead of byte-merging them.");

        GUI.Spacer();
        DisplayConflictPolicy();

        GUI.Spacer();
        ImGui.Separator();
        GUI.Spacer();
        DisplayFullModMerge();

        GUI.Spacer();
        ImGui.Separator();
        GUI.Spacer();
        DisplayDirectRegulationMerge();

        GUI.Spacer();
        ImGui.Separator();
        GUI.Spacer();
        DisplayDeltaPatchMerge();
    }

    private void DisplayConflictPolicy()
    {
        GUI.SimpleHeader("Conflict policy", "Choose what Auto Merge should do when two mods change the same field differently.");

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

    private void DisplayFullModMerge()
    {
        GUI.SimpleHeader(
            "Full mod folder merge",
            "Merge two or more complete mod folders. Unique files are copied automatically, identical files are deduplicated, regulation.bin uses the field-level merger, and supported binders are merged by internal entry.");

        GUI.WrappedText(
            "Unsupported binary collisions are never byte-merged. They remain conflicts and can either stop the build " +
            "or be resolved using the earlier/later source policy above. Build into a new or empty output folder.");

        GUI.Spacer();

        for (var i = 0; i < FullModFolderPaths.Count; i++)
        {
            var value = FullModFolderPaths[i];
            if (ImGui.InputText($"Mod folder {i + 1}##autoMergeFullSource{i}", ref value, 1024))
            {
                FullModFolderPaths[i] = value;
                LastFullModAnalysis = null;
                FullModBuildStatus = "";
            }
        }

        if (ImGui.Button("Add mod folder##autoMergeFullAdd"))
        {
            FullModFolderPaths.Add("");
            LastFullModAnalysis = null;
        }

        ImGui.SameLine();
        if (ImGui.Button("Remove last##autoMergeFullRemove") && FullModFolderPaths.Count > 2)
        {
            FullModFolderPaths.RemoveAt(FullModFolderPaths.Count - 1);
            LastFullModAnalysis = null;
        }

        ImGui.SameLine();
        if (ImGui.Button("Clear folders##autoMergeFullClear"))
        {
            for (var i = 0; i < FullModFolderPaths.Count; i++)
                FullModFolderPaths[i] = "";

            LastFullModAnalysis = null;
            FullModOutputPath = "";
            FullModBuildStatus = "";
        }

        GUI.Spacer();
        ImGui.InputText("Output folder##autoMergeFullOutput", ref FullModOutputPath, 1024);

        if (ImGui.Checkbox("Merge BND3/BND4 containers by internal entry##autoMergeFullBinder", ref FullModEnableBinderMerge))
            LastFullModAnalysis = null;

        var autoUpgradeRegulation = RegulationMerge.AutoUpgradeMismatchedVersions;
        if (ImGui.Checkbox("Auto-upgrade older regulation.bin versions##autoMergeFullUpgradeReg", ref autoUpgradeRegulation))
        {
            RegulationMerge.AutoUpgradeMismatchedVersions = autoUpgradeRegulation;
            LastFullModAnalysis = null;
        }
        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip(
                "When an input regulation is older than the loaded project, Smithbox compares it against the matching " +
                "historical vanilla regulation and carries its mod changes forward to the loaded version. " +
                "Automatic downgrade from a newer regulation is not supported.");
        }

        if (ImGui.Checkbox("Ignore .smithbox, project.json and regulation.bin.prev##autoMergeFullMetadata", ref FullModIgnoreMetadata))
            LastFullModAnalysis = null;

        ImGui.Checkbox("Write SMITHBOX_MERGE_REPORT.txt##autoMergeFullReport", ref FullModWriteReport);

        GUI.Spacer();
        if (ImGui.Button("Analyze mod folders##autoMergeFullAnalyze"))
            AnalyzeFullModFolders();

        if (LastFullModAnalysis == null)
            return;

        GUI.Spacer();
        GUI.SimpleHeader("Folder merge result", "Review what will be copied, structurally merged, ignored, or treated as a conflict.");

        ImGui.Text($"Source folders: {LastFullModAnalysis.SourceFolders.Count}");
        ImGui.Text($"Files scanned: {LastFullModAnalysis.ScannedFiles}");
        ImGui.Text($"Unique files to copy: {LastFullModAnalysis.UniqueFiles}");
        ImGui.Text($"Identical duplicate files: {LastFullModAnalysis.IdenticalFiles}");
        ImGui.Text($"Binder files to merge: {LastFullModAnalysis.BinderFiles}");
        ImGui.Text($"Regulation merges: {LastFullModAnalysis.RegulationFiles}");
        ImGui.Text($"Ignored metadata files: {LastFullModAnalysis.IgnoredFiles}");
        ImGui.Text($"Conflicts: {LastFullModAnalysis.Conflicts.Count}");

        if (LastFullModAnalysis.Files.Any(e => e.Action == FullModMergeAction.BinderMerge))
        {
            GUI.Spacer();
            if (ImGui.CollapsingHeader("Binder merge details##autoMergeFullBinderDetails"))
            {
                foreach (var plan in LastFullModAnalysis.Files.Where(e => e.Action == FullModMergeAction.BinderMerge).Take(100))
                {
                    var summary = plan.BinderSummary;
                    if (summary == null)
                        continue;

                    GUI.WrappedText(
                        $"• {plan.RelativePath} | added entries: {summary.AddedEntries} | " +
                        $"identical entries: {summary.IdenticalEntries} | nested binders: {summary.NestedBinderMerges} | " +
                        $"MATBIN semantic merges: {summary.MatbinSemanticMerges} | conflicts: {summary.Conflicts}");
                }
            }
        }

        if (LastFullModAnalysis.Warnings.Count > 0)
        {
            GUI.Spacer();
            if (ImGui.CollapsingHeader($"Warnings ({LastFullModAnalysis.Warnings.Count})##autoMergeFullWarnings"))
            {
                foreach (var warning in LastFullModAnalysis.Warnings.Take(200))
                    GUI.WrappedText($"• {warning}");
            }
        }

        if (LastFullModAnalysis.Errors.Count > 0)
        {
            GUI.Spacer();
            GUI.SimpleHeader("Folder merge errors", "The merged mod cannot be built until these issues are fixed.");
            foreach (var error in LastFullModAnalysis.Errors)
                GUI.WrappedText($"• {error}");
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
            GUI.SimpleHeader("regulation.bin field conflicts", "Resolve regulation conflicts at row/field level. Manual values are supported for field conflicts.");
            DisplayMergeSummary(LastFullModAnalysis.RegulationAnalysis.MergeResult, "fullreg");
        }

        if (!LastFullModAnalysis.CanBuild)
        {
            GUI.Spacer();
            GUI.WrappedText(
                "No output will be written while unresolved conflicts remain. Resolve them in the tables above. " +
                "MATBIN values can be resolved per property/index with Earlier, Later, or Manual; unsupported binary files can choose Earlier or Later.");
            return;
        }

        GUI.Spacer();
        if (ImGui.Button("Build merged mod folder##autoMergeFullBuild"))
            BuildFullModFolder();

        if (!string.IsNullOrWhiteSpace(FullModBuildStatus))
        {
            GUI.Spacer();
            GUI.WrappedText(FullModBuildStatus);
        }
    }

    private void DisplayFullModConflictResolver(FullModMergeAnalysis analysis)
    {
        var editableConflicts = analysis.Conflicts.Where(e => e.Type != FullModMergeConflictType.Regulation).ToList();
        var unresolved = editableConflicts.Count(e => !e.IsResolved);
        GUI.SimpleHeader(
            $"File / binder conflict resolver ({editableConflicts.Count})",
            "Resolve each file/binder collision independently. Binary entries can choose Earlier or Later; direct byte editing is intentionally not offered.");
        ImGui.Text($"Resolved: {editableConflicts.Count - unresolved} | Unresolved: {unresolved}");

        ImGui.InputText("Filter##autoMergeFullConflictFilter", ref FullModConflictFilter, 512);

        if (ImGui.Button("Resolve visible as earlier##autoMergeFullResolveEarlier"))
        {
            foreach (var conflict in FilterFullModConflicts(analysis))
                conflict.Resolution = FullModConflictResolution.UseEarlier;
        }
        ImGui.SameLine();
        if (ImGui.Button("Resolve visible as later##autoMergeFullResolveLater"))
        {
            foreach (var conflict in FilterFullModConflicts(analysis))
                conflict.Resolution = FullModConflictResolution.UseLater;
        }
        ImGui.SameLine();
        if (ImGui.Button("Reset visible##autoMergeFullResolveReset"))
        {
            foreach (var conflict in FilterFullModConflicts(analysis))
                conflict.Resolution = FullModConflictResolution.Unresolved;
        }

        var filtered = FilterFullModConflicts(analysis).ToList();
        var pageCount = Math.Max(1, (filtered.Count + ConflictRowsPerPage - 1) / ConflictRowsPerPage);
        FullModConflictPage = Math.Clamp(FullModConflictPage, 0, pageCount - 1);

        if (ImGui.Button("<##autoMergeFullConflictPrev") && FullModConflictPage > 0)
            FullModConflictPage--;
        ImGui.SameLine();
        ImGui.Text($"Page {FullModConflictPage + 1}/{pageCount} ({filtered.Count} shown by filter)");
        ImGui.SameLine();
        if (ImGui.Button(">##autoMergeFullConflictNext") && FullModConflictPage + 1 < pageCount)
            FullModConflictPage++;

        var page = filtered.Skip(FullModConflictPage * ConflictRowsPerPage).Take(ConflictRowsPerPage);
        if (ImGui.BeginTable(
                "autoMergeFullConflictTable",
                6,
                ImGuiTableFlags.Resizable | ImGuiTableFlags.BordersOuterH | ImGuiTableFlags.BordersOuterV |
                ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingStretchSame))
        {
            ImGui.TableSetupColumn("Path");
            ImGui.TableSetupColumn("Earlier");
            ImGui.TableSetupColumn("Later");
            ImGui.TableSetupColumn("Resolution");
            ImGui.TableSetupColumn("Manual value");
            ImGui.TableSetupColumn("Reason");
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
                        conflict.ManualValue = manual;
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
            FullModConflictResolution.UseEarlier => "Use earlier",
            FullModConflictResolution.UseLater => "Use later",
            FullModConflictResolution.Manual => "Manual",
            _ => "Unresolved"
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
            FullModBuildStatus = $"Merged mod written to: {System.IO.Path.GetFullPath(cleanOutputPath)}";
        }
        catch (Exception ex)
        {
            FullModBuildStatus = $"Build failed: {ex.Message}";
        }
    }

    private void DisplayDirectRegulationMerge()
    {
        GUI.SimpleHeader(
            "Direct regulation merge",
            "Paste two or more regulation file paths. Smithbox will extract each file's vanilla-relative changes, auto merge them, and optionally build a new encrypted regulation file.");

        if (!RegulationMerge.IsSupportedProject)
        {
            GUI.WrappedText($"Direct regulation merge currently supports {RegulationMerge.SupportedProjectText}.");
            return;
        }

        GUI.WrappedText(
            "Regulation files that match the loaded project are compared directly against VanillaBank. " +
            "When automatic upgrade is enabled, older ER/AC6/NR regulations are compared against Smithbox's matching historical vanilla data " +
            "and their mod changes are carried forward to the loaded version. Newer inputs are never downgraded automatically.");

        var autoUpgradeRegulation = RegulationMerge.AutoUpgradeMismatchedVersions;
        if (ImGui.Checkbox("Auto-upgrade older regulation versions##autoMergeRegUpgrade", ref autoUpgradeRegulation))
        {
            RegulationMerge.AutoUpgradeMismatchedVersions = autoUpgradeRegulation;
            LastRegulationAnalysis = null;
        }

        GUI.Spacer();

        for (var i = 0; i < RegulationPaths.Count; i++)
        {
            var value = RegulationPaths[i];
            if (ImGui.InputText($"Source {i + 1}##autoMergeRegSource{i}", ref value, 1024))
            {
                RegulationPaths[i] = value;
                LastRegulationAnalysis = null;
                RegulationBuildStatus = "";
            }
        }

        if (ImGui.Button("Add source##autoMergeRegAdd"))
        {
            RegulationPaths.Add("");
            LastRegulationAnalysis = null;
        }

        ImGui.SameLine();
        if (ImGui.Button("Remove last##autoMergeRegRemove") && RegulationPaths.Count > 2)
        {
            RegulationPaths.RemoveAt(RegulationPaths.Count - 1);
            LastRegulationAnalysis = null;
        }

        ImGui.SameLine();
        if (ImGui.Button("Clear paths##autoMergeRegClear"))
        {
            for (var i = 0; i < RegulationPaths.Count; i++)
                RegulationPaths[i] = "";

            LastRegulationAnalysis = null;
            RegulationOutputPath = "";
            RegulationBuildStatus = "";
        }

        GUI.Spacer();
        if (ImGui.Button("Analyze regulation files##autoMergeRegAnalyze"))
            AnalyzeRegulations();

        if (LastRegulationAnalysis == null)
            return;

        GUI.Spacer();

        if (LastRegulationAnalysis.Sources.Count > 0)
        {
            GUI.SimpleHeader("Loaded regulation sources", "Each source has been converted to a vanilla-relative delta in memory.");
            foreach (var source in LastRegulationAnalysis.Sources)
            {
                var versionText = source.AutoUpgraded
                    ? $"{ParamUtils.ParseRegulationVersion(source.OriginalParamVersion)} -> {ParamUtils.ParseRegulationVersion(source.ParamVersion)} (auto-upgraded)"
                    : ParamUtils.ParseRegulationVersion(source.ParamVersion);
                GUI.WrappedText(
                    $"• {source.Filename} | {versionText} | " +
                    $"{source.ParsedParamCount} params | {source.Delta.Params.Sum(e => e.Rows.Count)} changed rows");
            }
        }

        if (LastRegulationAnalysis.Warnings.Count > 0)
        {
            GUI.Spacer();
            if (ImGui.CollapsingHeader($"Warnings ({LastRegulationAnalysis.Warnings.Count})##autoMergeRegWarnings"))
            {
                foreach (var warning in LastRegulationAnalysis.Warnings.Take(100))
                    GUI.WrappedText($"• {warning}");
            }
        }

        if (LastRegulationAnalysis.Errors.Count > 0)
        {
            GUI.Spacer();
            GUI.SimpleHeader("Regulation errors", "The direct merge cannot continue until these issues are fixed.");
            foreach (var error in LastRegulationAnalysis.Errors)
                GUI.WrappedText($"• {error}");
            return;
        }

        if (LastRegulationAnalysis.MergeResult == null)
            return;

        GUI.Spacer();
        DisplayMergeSummary(LastRegulationAnalysis.MergeResult, "reg");

        if (!LastRegulationAnalysis.MergeResult.CanApply)
            return;

        GUI.Spacer();
        GUI.SimpleHeader("Build regulation.bin", "The output is created as a new file; input files are never overwritten.");
        ImGui.InputText("Output path##autoMergeRegOutput", ref RegulationOutputPath, 1024);

        if (ImGui.Button("Build merged regulation.bin##autoMergeRegBuild"))
            BuildRegulation();

        ImGui.SameLine();
        if (ImGui.Button("Import merged changes into project##autoMergeRegImport"))
        {
            Engine.ApplyConflictResolutions(LastRegulationAnalysis.MergeResult);
            Patcher.Importer.ImportDelta("direct_regulation_auto_merge", LastRegulationAnalysis.MergeResult.Patch);
            RegulationBuildStatus = "Merged changes were sent to the loaded Param Editor project.";
        }

        ImGui.SameLine();
        if (ImGui.Button("Save merged delta##autoMergeRegSaveDelta"))
        {
            Engine.ApplyConflictResolutions(LastRegulationAnalysis.MergeResult);
            var name = string.IsNullOrWhiteSpace(OutputName) ? "auto_merged" : OutputName.Trim();
            Patcher.WriteDeltaPatch(LastRegulationAnalysis.MergeResult.Patch, name);
            Patcher.Selection.RefreshImportList();
            RegulationBuildStatus = $"Saved merged delta as {name}.";
        }

        if (!string.IsNullOrWhiteSpace(RegulationBuildStatus))
        {
            GUI.Spacer();
            GUI.WrappedText(RegulationBuildStatus);
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
            RegulationBuildStatus = $"Merged regulation written to: {System.IO.Path.GetFullPath(cleanOutputPath)}";
        }
        catch (Exception ex)
        {
            RegulationBuildStatus = $"Build failed: {ex.Message}";
        }
    }

    private void DisplayDeltaPatchMerge()
    {
        GUI.SimpleHeader("Delta patch merge", "Merge existing Smithbox delta JSON files using the same field-level conflict engine.");

        if (ImGui.Button("Select all compatible##autoMergeSelectAll"))
        {
            foreach (var entry in Patcher.Selection.ImportList)
            {
                if (entry.Delta.ProjectType == Patcher.Project.Descriptor.ProjectType &&
                    entry.Delta.ParamVersion == Patcher.Project.Handler.ParamData.PrimaryBank.ParamVersion)
                    SelectedFiles.Add(entry.Filename);
            }
        }

        ImGui.SameLine();
        if (ImGui.Button("Clear##autoMergeClear"))
        {
            SelectedFiles.Clear();
            LastResult = null;
        }

        ImGui.SameLine();
        if (ImGui.Button("Refresh##autoMergeRefresh"))
        {
            Patcher.Selection.RefreshImportList();
            LastResult = null;
        }

        GUI.Spacer();
        ImGui.BeginChild("autoMergeSourceList", new System.Numerics.Vector2(0, 180), ImGuiChildFlags.Borders);

        foreach (var entry in Patcher.Selection.ImportList)
        {
            var isSelected = SelectedFiles.Contains(entry.Filename);
            var version = ParamUtils.ParseRegulationVersion(entry.Delta.ParamVersion);
            var sameGame = entry.Delta.ProjectType == Patcher.Project.Descriptor.ProjectType;
            var sameVersion = entry.Delta.ParamVersion == Patcher.Project.Handler.ParamData.PrimaryBank.ParamVersion;
            var compatibility = !sameGame ? " (different game)" : !sameVersion ? " (different param version)" : "";
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

        GUI.Spacer();
        GUI.SimpleHeader("Delta output", "Name used if the merged result is saved as a delta patch.");
        ImGui.InputText("##autoMergeOutputName", ref OutputName, 255);

        GUI.Spacer();
        if (ImGui.Button("Analyze delta merge##autoMergeAnalyze"))
            AnalyzeDeltaPatches();

        if (LastResult == null)
            return;

        GUI.Spacer();
        DisplayMergeSummary(LastResult, "delta");

        if (!LastResult.CanApply)
            return;

        GUI.Spacer();
        if (ImGui.Button("Save merged delta##autoMergeSave"))
        {
            Engine.ApplyConflictResolutions(LastResult);
            var name = string.IsNullOrWhiteSpace(OutputName) ? "auto_merged" : OutputName.Trim();
            Patcher.WriteDeltaPatch(LastResult.Patch, name);
            Patcher.Selection.RefreshImportList();
        }

        ImGui.SameLine();
        if (ImGui.Button("Import merged delta##autoMergeImport"))
        {
            Engine.ApplyConflictResolutions(LastResult);
            var name = string.IsNullOrWhiteSpace(OutputName) ? "auto_merged" : OutputName.Trim();
            Patcher.Importer.ImportDelta(name, LastResult.Patch);
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
        GUI.SimpleHeader("Merge result", "Review and resolve field-level conflicts before saving, importing, or building a regulation file.");

        ImGui.Text($"Sources: {result.SourceCount}");
        ImGui.Text($"Rows merged: {result.Patch.Params.Sum(e => e.Rows.Count)}");
        ImGui.Text($"Safe field changes: {result.SafeFields}");
        ImGui.Text($"Identical overlapping fields: {result.IdenticalFields}");
        ImGui.Text($"Conflicts: {result.Conflicts.Count} | Resolved: {result.ResolvedConflicts} | Unresolved: {result.UnresolvedConflicts}");

        if (result.Errors.Count > 0)
        {
            GUI.Spacer();
            GUI.SimpleHeader("Errors", "The merge cannot be applied until these issues are fixed.");
            foreach (var error in result.Errors)
                GUI.WrappedText($"• {error}");
        }

        if (result.Conflicts.Count > 0)
        {
            GUI.Spacer();
            if (ImGui.CollapsingHeader($"Conflict resolution editor ({result.Conflicts.Count})##autoMergeConflicts_{idSuffix}", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.InputText($"Filter##paramConflictFilter_{idSuffix}", ref ParamConflictFilter, 512);

                var filtered = result.Conflicts.Where(conflict =>
                    string.IsNullOrWhiteSpace(ParamConflictFilter) ||
                    conflict.ParamName.Contains(ParamConflictFilter, StringComparison.OrdinalIgnoreCase) ||
                    conflict.Field.Contains(ParamConflictFilter, StringComparison.OrdinalIgnoreCase) ||
                    conflict.ExistingSource.Contains(ParamConflictFilter, StringComparison.OrdinalIgnoreCase) ||
                    conflict.IncomingSource.Contains(ParamConflictFilter, StringComparison.OrdinalIgnoreCase)).ToList();

                if (ImGui.Button($"Visible -> earlier##paramResolveEarlier_{idSuffix}"))
                {
                    foreach (var conflict in filtered)
                        conflict.Resolution = ParamDeltaConflictResolution.UseEarlier;
                    Engine.ApplyConflictResolutions(result);
                }
                ImGui.SameLine();
                if (ImGui.Button($"Visible -> later##paramResolveLater_{idSuffix}"))
                {
                    foreach (var conflict in filtered)
                        conflict.Resolution = ParamDeltaConflictResolution.UseLater;
                    Engine.ApplyConflictResolutions(result);
                }
                ImGui.SameLine();
                if (ImGui.Button($"Reset visible##paramResolveReset_{idSuffix}"))
                {
                    foreach (var conflict in filtered)
                        conflict.Resolution = ParamDeltaConflictResolution.Unresolved;
                }

                var pageCount = Math.Max(1, (filtered.Count + ConflictRowsPerPage - 1) / ConflictRowsPerPage);
                ParamConflictPage = Math.Clamp(ParamConflictPage, 0, pageCount - 1);
                if (ImGui.Button($"<##paramConflictPrev_{idSuffix}") && ParamConflictPage > 0)
                    ParamConflictPage--;
                ImGui.SameLine();
                ImGui.Text($"Page {ParamConflictPage + 1}/{pageCount} ({filtered.Count} shown by filter)");
                ImGui.SameLine();
                if (ImGui.Button($">##paramConflictNext_{idSuffix}") && ParamConflictPage + 1 < pageCount)
                    ParamConflictPage++;

                var page = filtered.Skip(ParamConflictPage * ConflictRowsPerPage).Take(ConflictRowsPerPage);
                if (ImGui.BeginTable(
                        $"paramConflictTable_{idSuffix}",
                        6,
                        ImGuiTableFlags.Resizable | ImGuiTableFlags.BordersOuterH | ImGuiTableFlags.BordersOuterV |
                        ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingStretchSame))
                {
                    ImGui.TableSetupColumn("Param / row / field");
                    ImGui.TableSetupColumn("Earlier");
                    ImGui.TableSetupColumn("Later");
                    ImGui.TableSetupColumn("Resolution");
                    ImGui.TableSetupColumn("Manual value");
                    ImGui.TableSetupColumn("Type");
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
                        ImGui.Text(conflict.Type == ParamDeltaMergeConflictType.FieldValue ? "Field" : "Row state");
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
            GUI.WrappedText(
                "Resolve every conflict in the table before building/importing. Field conflicts support Earlier, Later, or a manually edited value. " +
                "Row-state conflicts support Earlier or Later.");
        }
    }

    private static string GetParamResolutionName(ParamDeltaConflictResolution resolution)
    {
        return resolution switch
        {
            ParamDeltaConflictResolution.UseEarlier => "Use earlier",
            ParamDeltaConflictResolution.UseLater => "Use later",
            ParamDeltaConflictResolution.Manual => "Manual",
            _ => "Unresolved"
        };
    }

    private static string GetStrategyName(ParamDeltaConflictStrategy strategy)
    {
        return strategy switch
        {
            ParamDeltaConflictStrategy.StopOnConflict => "Stop on conflict (recommended)",
            ParamDeltaConflictStrategy.PreferFirst => "Prefer earlier source in list",
            ParamDeltaConflictStrategy.PreferLast => "Prefer later source in list",
            _ => strategy.ToString()
        };
    }
}
