using SoulsFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Globalization;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace StudioCore.Editors.ParamEditor;

public enum FullModMergeAction
{
    Copy = 0,
    Identical = 1,
    RegulationMerge = 2,
    BinderMerge = 3,
    Conflict = 4,
    Ignored = 5
}

public enum FullModMergeConflictType
{
    File = 0,
    BinderEntry = 1,
    Regulation = 2,
    MatbinValue = 3
}

public enum FullModConflictResolution
{
    Unresolved = 0,
    UseEarlier = 1,
    UseLater = 2,
    Manual = 3
}

public enum FullModConflictValueKind
{
    None = 0,
    String = 1,
    Bool = 2,
    Int = 3,
    UInt = 4,
    Float = 5
}

public sealed class FullModMergeConflict
{
    public FullModMergeConflictType Type { get; init; }
    public string RelativePath { get; init; } = "";
    public string InternalPath { get; init; } = "";
    public string ExistingSource { get; init; } = "";
    public string IncomingSource { get; init; } = "";
    public string Message { get; init; } = "";
    public string ExistingValue { get; init; } = "";
    public string IncomingValue { get; init; } = "";
    public string ManualValue { get; set; } = "";
    public FullModConflictValueKind ValueKind { get; init; } = FullModConflictValueKind.None;
    public FullModConflictResolution Resolution { get; set; } = FullModConflictResolution.Unresolved;

    public bool SupportsManual => Type == FullModMergeConflictType.MatbinValue && ValueKind != FullModConflictValueKind.None;
    public bool IsResolved => Resolution != FullModConflictResolution.Unresolved;
}

public sealed class FullModBinderSummary
{
    public int SourceCount { get; set; }
    public int AddedEntries { get; set; }
    public int IdenticalEntries { get; set; }
    public int NestedBinderMerges { get; set; }
    public int MatbinSemanticMerges { get; set; }
    public int Conflicts { get; set; }
}

public sealed class FullModFilePlan
{
    public string RelativePath { get; init; } = "";
    public FullModMergeAction Action { get; set; }
    public List<string> SourcePaths { get; } = new();
    public string SelectedSourcePath { get; set; } = "";
    public FullModBinderSummary BinderSummary { get; set; }
}

public sealed class FullModMergeAnalysis
{
    public List<string> SourceFolders { get; } = new();
    public List<FullModFilePlan> Files { get; } = new();
    public List<FullModMergeConflict> Conflicts { get; } = new();
    public List<string> Warnings { get; } = new();
    public List<string> Errors { get; } = new();
    public RegulationMergeAnalysis RegulationAnalysis { get; set; }
    public ParamDeltaConflictStrategy Strategy { get; set; }
    public bool BinderMergeEnabled { get; set; }
    public bool IgnoreSmithboxMetadata { get; set; }

    public int ScannedFiles { get; set; }
    public int UniqueFiles => Files.Count(e => e.Action == FullModMergeAction.Copy);
    public int IdenticalFiles => Files.Count(e => e.Action == FullModMergeAction.Identical);
    public int BinderFiles => Files.Count(e => e.Action == FullModMergeAction.BinderMerge);
    public int RegulationFiles => Files.Count(e => e.Action == FullModMergeAction.RegulationMerge);
    public int ConflictFiles => Files.Count(e => e.Action == FullModMergeAction.Conflict);
    public int IgnoredFiles => Files.Count(e => e.Action == FullModMergeAction.Ignored);

    public bool CanBuild
    {
        get
        {
            if (Errors.Count > 0)
                return false;

            if (RegulationAnalysis != null && !RegulationAnalysis.CanBuild)
                return false;

            // Regulation conflicts are resolved in the field-level regulation conflict editor.
            // File/binder conflicts are resolved individually below.
            return Conflicts.Where(e => e.Type != FullModMergeConflictType.Regulation).All(e => e.IsResolved);
        }
    }
}

/// <summary>
/// Phase-1 full mod folder merger.
///
/// - Copies files that exist in only one source.
/// - Deduplicates byte-identical files.
/// - Delegates regulation.bin to ParamRegulationAutoMerge.
/// - Merges BND3/BND4 containers at BinderFile granularity, including nested binders.
/// - Leaves unsupported binary collisions as explicit conflicts instead of byte-merging them.
/// </summary>
public sealed class ParamFullModAutoMerge
{
    private readonly ParamRegulationAutoMerge RegulationMerge;
    private readonly Dictionary<string, string> HashCache = new(StringComparer.OrdinalIgnoreCase);

    private const int MaxNestedBinderDepth = 4;

    public ParamFullModAutoMerge(ParamRegulationAutoMerge regulationMerge)
    {
        RegulationMerge = regulationMerge;
    }

    private static FullModConflictResolution ResolutionFromStrategy(ParamDeltaConflictStrategy strategy)
    {
        return strategy switch
        {
            ParamDeltaConflictStrategy.PreferFirst => FullModConflictResolution.UseEarlier,
            ParamDeltaConflictStrategy.PreferLast => FullModConflictResolution.UseLater,
            _ => FullModConflictResolution.Unresolved
        };
    }

    private static FullModConflictResolution EffectiveResolution(
        FullModConflictResolution resolution,
        ParamDeltaConflictStrategy strategy)
    {
        if (resolution != FullModConflictResolution.Unresolved)
            return resolution;

        return ResolutionFromStrategy(strategy);
    }

    private static string ConflictKey(FullModMergeConflict conflict)
    {
        return $"{conflict.Type}|{NormalizeRelativePath(conflict.RelativePath)}|{NormalizeInternalPath(conflict.InternalPath)}|{conflict.ExistingSource}|{conflict.IncomingSource}";
    }

    public FullModMergeAnalysis Analyze(
        IReadOnlyList<string> sourceFolders,
        ParamDeltaConflictStrategy strategy,
        bool enableBinderMerge,
        bool ignoreSmithboxMetadata)
    {
        HashCache.Clear();

        var analysis = new FullModMergeAnalysis
        {
            Strategy = strategy,
            BinderMergeEnabled = enableBinderMerge,
            IgnoreSmithboxMetadata = ignoreSmithboxMetadata
        };

        var normalizedSources = sourceFolders
            .Where(e => !string.IsNullOrWhiteSpace(e))
            .Select(NormalizeUserPath)
            .Select(Path.GetFullPath)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (normalizedSources.Count < 2)
        {
            analysis.Errors.Add("Select at least two mod folders.");
            return analysis;
        }

        foreach (var source in normalizedSources)
        {
            if (!Directory.Exists(source))
                analysis.Errors.Add($"Mod folder not found: {source}");
            else
                analysis.SourceFolders.Add(source);
        }

        if (analysis.Errors.Count > 0)
            return analysis;

        var byRelativePath = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        foreach (var source in analysis.SourceFolders)
        {
            foreach (var path in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories))
            {
                var relative = NormalizeRelativePath(Path.GetRelativePath(source, path));
                analysis.ScannedFiles++;

                if (ignoreSmithboxMetadata && ShouldIgnore(relative))
                {
                    var ignoredPlan = new FullModFilePlan
                    {
                        RelativePath = relative,
                        Action = FullModMergeAction.Ignored,
                        SelectedSourcePath = path
                    };
                    ignoredPlan.SourcePaths.Add(path);
                    analysis.Files.Add(ignoredPlan);
                    continue;
                }

                if (!byRelativePath.TryGetValue(relative, out var paths))
                {
                    paths = new List<string>();
                    byRelativePath.Add(relative, paths);
                }

                paths.Add(path);
            }
        }

        foreach (var entry in byRelativePath.OrderBy(e => e.Key, StringComparer.OrdinalIgnoreCase))
            AnalyzeFileGroup(entry.Key, entry.Value, analysis);

        return analysis;
    }

    public void Build(
        FullModMergeAnalysis analysis,
        string outputFolder,
        bool writeReport)
    {
        if (analysis == null || !analysis.CanBuild)
            throw new InvalidOperationException("Analyze the mod folders successfully before building the merged mod.");

        outputFolder = NormalizeUserPath(outputFolder);
        if (string.IsNullOrWhiteSpace(outputFolder))
            throw new InvalidOperationException("Choose an output folder.");

        var outputFullPath = Path.GetFullPath(outputFolder);
        ValidateOutputFolder(analysis.SourceFolders, outputFullPath);

        if (Directory.Exists(outputFullPath) && Directory.EnumerateFileSystemEntries(outputFullPath).Any())
        {
            throw new InvalidOperationException(
                "The output folder is not empty. Choose a new/empty folder so Auto Merge never deletes unrelated files.");
        }

        Directory.CreateDirectory(outputFullPath);

        foreach (var plan in analysis.Files.OrderBy(e => e.RelativePath, StringComparer.OrdinalIgnoreCase))
        {
            if (plan.Action == FullModMergeAction.Ignored)
                continue;

            var destination = Path.Combine(
                outputFullPath,
                plan.RelativePath.Replace('/', Path.DirectorySeparatorChar));

            var destinationDirectory = Path.GetDirectoryName(destination);
            if (!string.IsNullOrWhiteSpace(destinationDirectory))
                Directory.CreateDirectory(destinationDirectory);

            switch (plan.Action)
            {
                case FullModMergeAction.Copy:
                case FullModMergeAction.Identical:
                    CopyFile(plan.SelectedSourcePath, destination);
                    break;

                case FullModMergeAction.RegulationMerge:
                    RegulationMerge.BuildMergedRegulation(analysis.RegulationAnalysis, destination);
                    break;

                case FullModMergeAction.BinderMerge:
                    BuildBinder(plan.SourcePaths, analysis, plan.RelativePath, destination);
                    break;

                case FullModMergeAction.Conflict:
                {
                    var conflict = analysis.Conflicts.FirstOrDefault(e =>
                        e.Type == FullModMergeConflictType.File &&
                        string.Equals(e.RelativePath, plan.RelativePath, StringComparison.OrdinalIgnoreCase));
                    var resolution = conflict == null
                        ? ResolutionFromStrategy(analysis.Strategy)
                        : EffectiveResolution(conflict.Resolution, analysis.Strategy);

                    if (resolution == FullModConflictResolution.Unresolved)
                        throw new InvalidOperationException($"Unresolved conflict: {plan.RelativePath}");

                    var source = resolution == FullModConflictResolution.UseLater
                        ? plan.SourcePaths[^1]
                        : plan.SourcePaths[0];
                    CopyFile(source, destination);
                    break;
                }
            }
        }

        if (writeReport)
            WriteReport(analysis, Path.Combine(outputFullPath, "SMITHBOX_MERGE_REPORT.txt"));
    }

    private void AnalyzeFileGroup(
        string relativePath,
        List<string> sourcePaths,
        FullModMergeAnalysis analysis)
    {
        var plan = new FullModFilePlan
        {
            RelativePath = relativePath
        };
        plan.SourcePaths.AddRange(sourcePaths);

        if (sourcePaths.Count == 1)
        {
            plan.Action = FullModMergeAction.Copy;
            plan.SelectedSourcePath = sourcePaths[0];
            analysis.Files.Add(plan);
            return;
        }

        if (AllFilesEqual(sourcePaths))
        {
            plan.Action = FullModMergeAction.Identical;
            plan.SelectedSourcePath = sourcePaths[0];
            analysis.Files.Add(plan);
            return;
        }

        if (IsRootRegulation(relativePath))
        {
            AnalyzeRegulationGroup(plan, analysis);
            analysis.Files.Add(plan);
            return;
        }

        if (analysis.BinderMergeEnabled && TryAnalyzeBinderGroup(plan, analysis))
        {
            analysis.Files.Add(plan);
            return;
        }

        plan.Action = FullModMergeAction.Conflict;
        plan.SelectedSourcePath = SelectSource(sourcePaths, analysis.Strategy);
        analysis.Conflicts.Add(new FullModMergeConflict
        {
            Type = FullModMergeConflictType.File,
            RelativePath = relativePath,
            ExistingSource = SourceLabel(sourcePaths[0]),
            IncomingSource = SourceLabel(sourcePaths[^1]),
            Message = "The same file path has different binary data and no safe structured merger is available.",
            Resolution = ResolutionFromStrategy(analysis.Strategy)
        });
        analysis.Files.Add(plan);
    }

    private void AnalyzeRegulationGroup(FullModFilePlan plan, FullModMergeAnalysis analysis)
    {
        if (!RegulationMerge.IsSupportedProject)
        {
            plan.Action = FullModMergeAction.Conflict;
            plan.SelectedSourcePath = SelectSource(plan.SourcePaths, analysis.Strategy);
            analysis.Conflicts.Add(new FullModMergeConflict
            {
                Type = FullModMergeConflictType.Regulation,
                RelativePath = plan.RelativePath,
                ExistingSource = SourceLabel(plan.SourcePaths[0]),
                IncomingSource = SourceLabel(plan.SourcePaths[^1]),
                Message = $"Structured regulation merge is not supported for this project. Supported: {RegulationMerge.SupportedProjectText}.",
                Resolution = ResolutionFromStrategy(analysis.Strategy)
            });
            return;
        }

        var regulationAnalysis = RegulationMerge.Analyze(plan.SourcePaths, analysis.Strategy);
        analysis.RegulationAnalysis = regulationAnalysis;

        foreach (var warning in regulationAnalysis.Warnings)
            analysis.Warnings.Add($"regulation.bin: {warning}");

        foreach (var error in regulationAnalysis.Errors)
            analysis.Errors.Add($"regulation.bin: {error}");

        if (regulationAnalysis.MergeResult != null)
        {
            foreach (var conflict in regulationAnalysis.MergeResult.Conflicts)
            {
                analysis.Conflicts.Add(new FullModMergeConflict
                {
                    Type = FullModMergeConflictType.Regulation,
                    RelativePath = plan.RelativePath,
                    InternalPath = conflict.Type == ParamDeltaMergeConflictType.FieldValue
                        ? $"{conflict.ParamName}/{conflict.RowID}:{conflict.RowIndex}/{conflict.Field}"
                        : $"{conflict.ParamName}/{conflict.RowID}:{conflict.RowIndex}",
                    ExistingSource = conflict.ExistingSource,
                    IncomingSource = conflict.IncomingSource,
                    Message = conflict.Type == ParamDeltaMergeConflictType.FieldValue
                        ? $"Different field values: {conflict.ExistingValue} vs {conflict.IncomingValue}"
                        : $"Different row states: {conflict.ExistingState} vs {conflict.IncomingState}",
                    Resolution = conflict.Resolution switch
                    {
                        ParamDeltaConflictResolution.UseEarlier => FullModConflictResolution.UseEarlier,
                        ParamDeltaConflictResolution.UseLater => FullModConflictResolution.UseLater,
                        _ => FullModConflictResolution.Unresolved
                    }
                });
            }
        }

        plan.Action = FullModMergeAction.RegulationMerge;
        plan.SelectedSourcePath = SelectSource(plan.SourcePaths, analysis.Strategy);
    }

    private bool TryAnalyzeBinderGroup(FullModFilePlan plan, FullModMergeAnalysis analysis)
    {
        var conflicts = new List<FullModMergeConflict>();
        if (!TryMergeBinderFiles(
                plan.SourcePaths,
                analysis.Strategy,
                plan.RelativePath,
                outputPath: null,
                conflicts,
                resolutionMap: null,
                out var summary,
                out var error))
        {
            if (!string.IsNullOrWhiteSpace(error))
                analysis.Warnings.Add($"{plan.RelativePath}: binder merge skipped ({error})");
            return false;
        }

        plan.Action = FullModMergeAction.BinderMerge;
        plan.BinderSummary = summary;
        plan.SelectedSourcePath = SelectSource(plan.SourcePaths, analysis.Strategy);
        analysis.Conflicts.AddRange(conflicts);
        return true;
    }

    private void BuildBinder(
        IReadOnlyList<string> sourcePaths,
        FullModMergeAnalysis analysis,
        string relativePath,
        string outputPath)
    {
        var conflicts = new List<FullModMergeConflict>();
        var resolutionMap = analysis.Conflicts
            .Where(e => e.Type is FullModMergeConflictType.BinderEntry or FullModMergeConflictType.MatbinValue)
            .GroupBy(ConflictKey, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(e => e.Key, e => e.Last(), StringComparer.OrdinalIgnoreCase);

        if (!TryMergeBinderFiles(
                sourcePaths,
                analysis.Strategy,
                relativePath,
                outputPath,
                conflicts,
                resolutionMap,
                out _,
                out var error))
        {
            throw new InvalidOperationException(
                $"Could not rebuild binder {relativePath}: {error ?? "unsupported binder format"}");
        }

        if (conflicts.Any(e => EffectiveResolution(e.Resolution, analysis.Strategy) == FullModConflictResolution.Unresolved))
            throw new InvalidOperationException($"Binder {relativePath} still has unresolved conflicts.");
    }

    private bool TryMergeBinderFiles(
        IReadOnlyList<string> sourcePaths,
        ParamDeltaConflictStrategy strategy,
        string relativePath,
        string outputPath,
        List<FullModMergeConflict> conflicts,
        IReadOnlyDictionary<string, FullModMergeConflict> resolutionMap,
        out FullModBinderSummary summary,
        out string error)
    {
        summary = new FullModBinderSummary { SourceCount = sourcePaths.Count };
        error = null;

        if (sourcePaths.Count < 2)
            return false;

        BinderDocument baseDocument = null;
        try
        {
            if (!BinderDocument.TryRead(sourcePaths[0], out baseDocument))
                return false;

            for (var i = 1; i < sourcePaths.Count; i++)
            {
                if (!BinderDocument.TryRead(sourcePaths[i], out var incoming))
                    return false;

                using (incoming)
                {
                    if (!string.Equals(baseDocument.Signature, incoming.Signature, StringComparison.Ordinal))
                        return false;

                    MergeBinderDocuments(
                        baseDocument,
                        incoming,
                        strategy,
                        relativePath,
                        internalPrefix: "",
                        SourceLabel(sourcePaths[0]),
                        SourceLabel(sourcePaths[i]),
                        depth: 0,
                        conflicts,
                        summary,
                        resolutionMap);
                }
            }

            summary.Conflicts = conflicts.Count(e => e.RelativePath == relativePath);

            if (!string.IsNullOrWhiteSpace(outputPath))
            {
                var directory = Path.GetDirectoryName(outputPath);
                if (!string.IsNullOrWhiteSpace(directory))
                    Directory.CreateDirectory(directory);
                baseDocument.Write(outputPath);
            }

            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
        finally
        {
            baseDocument?.Dispose();
        }
    }

    private void MergeBinderDocuments(
        BinderDocument current,
        BinderDocument incoming,
        ParamDeltaConflictStrategy strategy,
        string relativePath,
        string internalPrefix,
        string existingSource,
        string incomingSource,
        int depth,
        List<FullModMergeConflict> conflicts,
        FullModBinderSummary summary,
        IReadOnlyDictionary<string, FullModMergeConflict> resolutionMap)
    {
        var currentMap = current.Files
            .GroupBy(GetBinderEntryKey, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(e => e.Key, e => e.First(), StringComparer.OrdinalIgnoreCase);

        foreach (var incomingFile in incoming.Files)
        {
            var key = GetBinderEntryKey(incomingFile);
            var displayName = GetBinderEntryDisplayName(incomingFile);
            var internalPath = string.IsNullOrWhiteSpace(internalPrefix)
                ? displayName
                : $"{internalPrefix} -> {displayName}";

            if (!currentMap.TryGetValue(key, out var existingFile))
            {
                var clone = CloneBinderFile(incomingFile);
                current.Files.Add(clone);
                currentMap[key] = clone;
                summary.AddedEntries++;
                continue;
            }

            if (BinderEntryEquivalent(existingFile, incomingFile))
            {
                summary.IdenticalEntries++;
                continue;
            }

            if (IsMatbinEntry(existingFile) && IsMatbinEntry(incomingFile) &&
                TryMergeMatbin(
                    existingFile,
                    incomingFile,
                    strategy,
                    relativePath,
                    internalPath,
                    existingSource,
                    incomingSource,
                    conflicts,
                    resolutionMap,
                    out var mergedMatbinBytes))
            {
                existingFile.Bytes = mergedMatbinBytes;
                summary.MatbinSemanticMerges++;
                continue;
            }

            if (depth < MaxNestedBinderDepth &&
                TryMergeNestedBinder(
                    existingFile,
                    incomingFile,
                    strategy,
                    relativePath,
                    internalPath,
                    existingSource,
                    incomingSource,
                    depth + 1,
                    conflicts,
                    summary,
                    resolutionMap,
                    out var mergedBytes))
            {
                existingFile.Bytes = mergedBytes;
                summary.NestedBinderMerges++;
                continue;
            }

            var conflict = new FullModMergeConflict
            {
                Type = FullModMergeConflictType.BinderEntry,
                RelativePath = relativePath,
                InternalPath = internalPath,
                ExistingSource = existingSource,
                IncomingSource = incomingSource,
                Message = "Both binders contain this entry with different data. No safe semantic merger is available for this entry type.",
                Resolution = ResolutionFromStrategy(strategy)
            };

            if (resolutionMap != null && resolutionMap.TryGetValue(ConflictKey(conflict), out var savedConflict))
            {
                conflict.Resolution = savedConflict.Resolution;
                conflict.ManualValue = savedConflict.ManualValue;
            }

            conflicts.Add(conflict);

            if (EffectiveResolution(conflict.Resolution, strategy) == FullModConflictResolution.UseLater)
            {
                existingFile.Bytes = incomingFile.Bytes.ToArray();
                existingFile.Flags = incomingFile.Flags;
                existingFile.CompressionType = incomingFile.CompressionType;
                existingFile.ID = incomingFile.ID;
                existingFile.Name = incomingFile.Name;
            }
        }
    }

    private bool TryMergeMatbin(
        BinderFile existingFile,
        BinderFile incomingFile,
        ParamDeltaConflictStrategy strategy,
        string relativePath,
        string internalPath,
        string existingSource,
        string incomingSource,
        List<FullModMergeConflict> conflicts,
        IReadOnlyDictionary<string, FullModMergeConflict> resolutionMap,
        out Memory<byte> mergedBytes)
    {
        mergedBytes = default;

        try
        {
            var existing = MATBIN.Read(existingFile.Bytes);
            var incoming = MATBIN.Read(incomingFile.Bytes);

            MergeMatbinString(
                existing.ShaderPath, incoming.ShaderPath,
                v => existing.ShaderPath = v,
                "ShaderPath", strategy, relativePath, internalPath, existingSource, incomingSource, conflicts, resolutionMap);
            MergeMatbinString(
                existing.SourcePath, incoming.SourcePath,
                v => existing.SourcePath = v,
                "SourcePath", strategy, relativePath, internalPath, existingSource, incomingSource, conflicts, resolutionMap);
            MergeMatbinUInt(
                existing.Key, incoming.Key,
                v => existing.Key = v,
                "Key", strategy, relativePath, internalPath, existingSource, incomingSource, conflicts, resolutionMap);

            var existingParams = existing.Params
                .GroupBy(e => e.Name ?? "", StringComparer.Ordinal)
                .ToDictionary(e => e.Key, e => e.First(), StringComparer.Ordinal);

            foreach (var incomingParam in incoming.Params)
            {
                var key = incomingParam.Name ?? "";
                if (!existingParams.TryGetValue(key, out var existingParam))
                {
                    var clone = CloneMatbinParam(incomingParam);
                    existing.Params.Add(clone);
                    existingParams[key] = clone;
                    continue;
                }

                MergeMatbinUInt(
                    existingParam.Key, incomingParam.Key,
                    v => existingParam.Key = v,
                    $"Param/{incomingParam.Name}/Key", strategy, relativePath, internalPath,
                    existingSource, incomingSource, conflicts, resolutionMap);

                if (existingParam.Type != incomingParam.Type)
                {
                    var conflict = CreateMatbinConflict(
                        strategy, relativePath,
                        $"{internalPath} :: MATBIN/Param/{incomingParam.Name}/Type",
                        existingSource, incomingSource,
                        existingParam.Type.ToString(), incomingParam.Type.ToString(),
                        FullModConflictValueKind.None,
                        "MATBIN parameter types differ; choose which parameter definition to keep.",
                        resolutionMap);
                    conflicts.Add(conflict);
                    if (EffectiveResolution(conflict.Resolution, strategy) == FullModConflictResolution.UseLater)
                    {
                        var index = existing.Params.IndexOf(existingParam);
                        var clone = CloneMatbinParam(incomingParam);
                        existing.Params[index] = clone;
                        existingParams[key] = clone;
                    }
                    continue;
                }

                MergeMatbinParamValue(
                    existingParam,
                    incomingParam,
                    strategy,
                    relativePath,
                    internalPath,
                    existingSource,
                    incomingSource,
                    conflicts,
                    resolutionMap);
            }

            var existingSamplers = existing.Samplers
                .GroupBy(e => e.Type ?? "", StringComparer.Ordinal)
                .ToDictionary(e => e.Key, e => e.First(), StringComparer.Ordinal);

            foreach (var incomingSampler in incoming.Samplers)
            {
                var samplerKey = incomingSampler.Type ?? "";
                if (!existingSamplers.TryGetValue(samplerKey, out var existingSampler))
                {
                    var clone = CloneMatbinSampler(incomingSampler);
                    existing.Samplers.Add(clone);
                    existingSamplers[samplerKey] = clone;
                    continue;
                }

                var samplerPrefix = $"Sampler/{samplerKey}";
                MergeMatbinString(
                    existingSampler.Path, incomingSampler.Path,
                    v => existingSampler.Path = v,
                    $"{samplerPrefix}/Path", strategy, relativePath, internalPath,
                    existingSource, incomingSource, conflicts, resolutionMap);
                MergeMatbinUInt(
                    existingSampler.Key, incomingSampler.Key,
                    v => existingSampler.Key = v,
                    $"{samplerPrefix}/Key", strategy, relativePath, internalPath,
                    existingSource, incomingSource, conflicts, resolutionMap);
                MergeMatbinFloat(
                    existingSampler.Unk14.X, incomingSampler.Unk14.X,
                    v => existingSampler.Unk14 = new Vector2(v, existingSampler.Unk14.Y),
                    $"{samplerPrefix}/Unk14[0]", strategy, relativePath, internalPath,
                    existingSource, incomingSource, conflicts, resolutionMap);
                MergeMatbinFloat(
                    existingSampler.Unk14.Y, incomingSampler.Unk14.Y,
                    v => existingSampler.Unk14 = new Vector2(existingSampler.Unk14.X, v),
                    $"{samplerPrefix}/Unk14[1]", strategy, relativePath, internalPath,
                    existingSource, incomingSource, conflicts, resolutionMap);
            }

            mergedBytes = existing.Write();
            return true;
        }
        catch
        {
            return false;
        }
    }

    private void MergeMatbinParamValue(
        MATBIN.Param existingParam,
        MATBIN.Param incomingParam,
        ParamDeltaConflictStrategy strategy,
        string relativePath,
        string internalPath,
        string existingSource,
        string incomingSource,
        List<FullModMergeConflict> conflicts,
        IReadOnlyDictionary<string, FullModMergeConflict> resolutionMap)
    {
        var prefix = $"Param/{existingParam.Name}";
        switch (existingParam.Type)
        {
            case MATBIN.ParamType.Bool:
                MergeMatbinBool(
                    (bool)existingParam.Value, (bool)incomingParam.Value,
                    v => existingParam.Value = v,
                    prefix, strategy, relativePath, internalPath, existingSource, incomingSource, conflicts, resolutionMap);
                break;
            case MATBIN.ParamType.Int:
                MergeMatbinInt(
                    (int)existingParam.Value, (int)incomingParam.Value,
                    v => existingParam.Value = v,
                    prefix, strategy, relativePath, internalPath, existingSource, incomingSource, conflicts, resolutionMap);
                break;
            case MATBIN.ParamType.Float:
                MergeMatbinFloat(
                    (float)existingParam.Value, (float)incomingParam.Value,
                    v => existingParam.Value = v,
                    prefix, strategy, relativePath, internalPath, existingSource, incomingSource, conflicts, resolutionMap);
                break;
            case MATBIN.ParamType.Int2:
            {
                var existingValues = (int[])existingParam.Value;
                var incomingValues = (int[])incomingParam.Value;
                if (existingValues.Length != incomingValues.Length)
                {
                    MergeWholeMatbinParam(existingParam, incomingParam, strategy, relativePath, internalPath, existingSource, incomingSource, conflicts, resolutionMap);
                    break;
                }
                for (var i = 0; i < existingValues.Length; i++)
                {
                    var index = i;
                    MergeMatbinInt(
                        existingValues[index], incomingValues[index],
                        v => existingValues[index] = v,
                        $"{prefix}[{index}]", strategy, relativePath, internalPath, existingSource, incomingSource, conflicts, resolutionMap);
                }
                break;
            }
            case MATBIN.ParamType.Float2:
            case MATBIN.ParamType.Float3:
            case MATBIN.ParamType.Float4:
            case MATBIN.ParamType.Float5:
            {
                var existingValues = (float[])existingParam.Value;
                var incomingValues = (float[])incomingParam.Value;
                if (existingValues.Length != incomingValues.Length)
                {
                    MergeWholeMatbinParam(existingParam, incomingParam, strategy, relativePath, internalPath, existingSource, incomingSource, conflicts, resolutionMap);
                    break;
                }
                for (var i = 0; i < existingValues.Length; i++)
                {
                    var index = i;
                    MergeMatbinFloat(
                        existingValues[index], incomingValues[index],
                        v => existingValues[index] = v,
                        $"{prefix}[{index}]", strategy, relativePath, internalPath, existingSource, incomingSource, conflicts, resolutionMap);
                }
                break;
            }
            default:
                MergeWholeMatbinParam(existingParam, incomingParam, strategy, relativePath, internalPath, existingSource, incomingSource, conflicts, resolutionMap);
                break;
        }
    }

    private void MergeWholeMatbinParam(
        MATBIN.Param existingParam,
        MATBIN.Param incomingParam,
        ParamDeltaConflictStrategy strategy,
        string relativePath,
        string internalPath,
        string existingSource,
        string incomingSource,
        List<FullModMergeConflict> conflicts,
        IReadOnlyDictionary<string, FullModMergeConflict> resolutionMap)
    {
        if (MatbinValueEquals(existingParam.Value, incomingParam.Value))
            return;

        var conflict = CreateMatbinConflict(
            strategy, relativePath,
            $"{internalPath} :: MATBIN/Param/{existingParam.Name}",
            existingSource, incomingSource,
            FormatMatbinValue(existingParam.Value), FormatMatbinValue(incomingParam.Value),
            FullModConflictValueKind.None,
            "MATBIN parameter values differ and cannot be edited safely as a scalar.",
            resolutionMap);
        conflicts.Add(conflict);
        if (EffectiveResolution(conflict.Resolution, strategy) == FullModConflictResolution.UseLater)
        {
            existingParam.Type = incomingParam.Type;
            existingParam.Key = incomingParam.Key;
            existingParam.Value = CloneMatbinValue(incomingParam.Value);
        }
    }

    private void MergeMatbinString(
        string existingValue, string incomingValue, Action<string> setter, string property,
        ParamDeltaConflictStrategy strategy, string relativePath, string internalPath,
        string existingSource, string incomingSource, List<FullModMergeConflict> conflicts,
        IReadOnlyDictionary<string, FullModMergeConflict> resolutionMap)
    {
        if (string.Equals(existingValue, incomingValue, StringComparison.Ordinal))
            return;
        var conflict = CreateMatbinConflict(
            strategy, relativePath, $"{internalPath} :: MATBIN/{property}", existingSource, incomingSource,
            existingValue ?? "", incomingValue ?? "", FullModConflictValueKind.String,
            "MATBIN string value differs.", resolutionMap);
        conflicts.Add(conflict);
        var resolution = EffectiveResolution(conflict.Resolution, strategy);
        if (resolution == FullModConflictResolution.UseLater)
            setter(incomingValue);
        else if (resolution == FullModConflictResolution.Manual)
            setter(conflict.ManualValue ?? "");
    }

    private void MergeMatbinBool(
        bool existingValue, bool incomingValue, Action<bool> setter, string property,
        ParamDeltaConflictStrategy strategy, string relativePath, string internalPath,
        string existingSource, string incomingSource, List<FullModMergeConflict> conflicts,
        IReadOnlyDictionary<string, FullModMergeConflict> resolutionMap)
    {
        if (existingValue == incomingValue)
            return;
        var conflict = CreateMatbinConflict(
            strategy, relativePath, $"{internalPath} :: MATBIN/{property}", existingSource, incomingSource,
            existingValue.ToString(), incomingValue.ToString(), FullModConflictValueKind.Bool,
            "MATBIN boolean value differs.", resolutionMap);
        conflicts.Add(conflict);
        var resolution = EffectiveResolution(conflict.Resolution, strategy);
        if (resolution == FullModConflictResolution.UseLater)
            setter(incomingValue);
        else if (resolution == FullModConflictResolution.Manual)
        {
            if (!bool.TryParse(conflict.ManualValue, out var value))
                throw new FormatException($"Invalid manual boolean value for {conflict.InternalPath}: {conflict.ManualValue}");
            setter(value);
        }
    }

    private void MergeMatbinInt(
        int existingValue, int incomingValue, Action<int> setter, string property,
        ParamDeltaConflictStrategy strategy, string relativePath, string internalPath,
        string existingSource, string incomingSource, List<FullModMergeConflict> conflicts,
        IReadOnlyDictionary<string, FullModMergeConflict> resolutionMap)
    {
        if (existingValue == incomingValue)
            return;
        var conflict = CreateMatbinConflict(
            strategy, relativePath, $"{internalPath} :: MATBIN/{property}", existingSource, incomingSource,
            existingValue.ToString(CultureInfo.InvariantCulture), incomingValue.ToString(CultureInfo.InvariantCulture), FullModConflictValueKind.Int,
            "MATBIN integer value differs.", resolutionMap);
        conflicts.Add(conflict);
        var resolution = EffectiveResolution(conflict.Resolution, strategy);
        if (resolution == FullModConflictResolution.UseLater)
            setter(incomingValue);
        else if (resolution == FullModConflictResolution.Manual)
        {
            if (!int.TryParse(conflict.ManualValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
                throw new FormatException($"Invalid manual integer value for {conflict.InternalPath}: {conflict.ManualValue}");
            setter(value);
        }
    }

    private void MergeMatbinUInt(
        uint existingValue, uint incomingValue, Action<uint> setter, string property,
        ParamDeltaConflictStrategy strategy, string relativePath, string internalPath,
        string existingSource, string incomingSource, List<FullModMergeConflict> conflicts,
        IReadOnlyDictionary<string, FullModMergeConflict> resolutionMap)
    {
        if (existingValue == incomingValue)
            return;
        var conflict = CreateMatbinConflict(
            strategy, relativePath, $"{internalPath} :: MATBIN/{property}", existingSource, incomingSource,
            existingValue.ToString(CultureInfo.InvariantCulture), incomingValue.ToString(CultureInfo.InvariantCulture), FullModConflictValueKind.UInt,
            "MATBIN unsigned integer value differs.", resolutionMap);
        conflicts.Add(conflict);
        var resolution = EffectiveResolution(conflict.Resolution, strategy);
        if (resolution == FullModConflictResolution.UseLater)
            setter(incomingValue);
        else if (resolution == FullModConflictResolution.Manual)
        {
            if (!uint.TryParse(conflict.ManualValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
                throw new FormatException($"Invalid manual unsigned integer value for {conflict.InternalPath}: {conflict.ManualValue}");
            setter(value);
        }
    }

    private void MergeMatbinFloat(
        float existingValue, float incomingValue, Action<float> setter, string property,
        ParamDeltaConflictStrategy strategy, string relativePath, string internalPath,
        string existingSource, string incomingSource, List<FullModMergeConflict> conflicts,
        IReadOnlyDictionary<string, FullModMergeConflict> resolutionMap)
    {
        if (existingValue.Equals(incomingValue))
            return;
        var conflict = CreateMatbinConflict(
            strategy, relativePath, $"{internalPath} :: MATBIN/{property}", existingSource, incomingSource,
            existingValue.ToString("R", CultureInfo.InvariantCulture), incomingValue.ToString("R", CultureInfo.InvariantCulture), FullModConflictValueKind.Float,
            "MATBIN floating-point value differs.", resolutionMap);
        conflicts.Add(conflict);
        var resolution = EffectiveResolution(conflict.Resolution, strategy);
        if (resolution == FullModConflictResolution.UseLater)
            setter(incomingValue);
        else if (resolution == FullModConflictResolution.Manual)
        {
            if (!float.TryParse(conflict.ManualValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
                throw new FormatException($"Invalid manual floating-point value for {conflict.InternalPath}: {conflict.ManualValue}");
            setter(value);
        }
    }

    private FullModMergeConflict CreateMatbinConflict(
        ParamDeltaConflictStrategy strategy,
        string relativePath,
        string internalPath,
        string existingSource,
        string incomingSource,
        string existingValue,
        string incomingValue,
        FullModConflictValueKind valueKind,
        string message,
        IReadOnlyDictionary<string, FullModMergeConflict> resolutionMap)
    {
        var conflict = new FullModMergeConflict
        {
            Type = FullModMergeConflictType.MatbinValue,
            RelativePath = relativePath,
            InternalPath = internalPath,
            ExistingSource = existingSource,
            IncomingSource = incomingSource,
            ExistingValue = existingValue ?? "",
            IncomingValue = incomingValue ?? "",
            ValueKind = valueKind,
            Message = message,
            Resolution = ResolutionFromStrategy(strategy)
        };

        if (resolutionMap != null && resolutionMap.TryGetValue(ConflictKey(conflict), out var savedConflict))
        {
            conflict.Resolution = savedConflict.Resolution;
            conflict.ManualValue = savedConflict.ManualValue;
        }

        return conflict;
    }

    private static bool IsMatbinEntry(BinderFile file)
    {
        return !string.IsNullOrWhiteSpace(file.Name) && file.Name.EndsWith(".matbin", StringComparison.OrdinalIgnoreCase);
    }

    private static MATBIN.Param CloneMatbinParam(MATBIN.Param source)
    {
        return new MATBIN.Param
        {
            Name = source.Name,
            Key = source.Key,
            Type = source.Type,
            Value = CloneMatbinValue(source.Value)
        };
    }

    private static MATBIN.Sampler CloneMatbinSampler(MATBIN.Sampler source)
    {
        return new MATBIN.Sampler
        {
            Type = source.Type,
            Path = source.Path,
            Key = source.Key,
            Unk14 = source.Unk14
        };
    }

    private static object CloneMatbinValue(object value)
    {
        return value switch
        {
            int[] ints => ints.ToArray(),
            float[] floats => floats.ToArray(),
            _ => value
        };
    }

    private static bool MatbinValueEquals(object a, object b)
    {
        if (ReferenceEquals(a, b))
            return true;
        if (a == null || b == null)
            return false;
        if (a is int[] ai && b is int[] bi)
            return ai.SequenceEqual(bi);
        if (a is float[] af && b is float[] bf)
            return af.SequenceEqual(bf);
        return a.Equals(b);
    }

    private static string FormatMatbinValue(object value)
    {
        return value switch
        {
            null => "",
            float f => f.ToString("R", CultureInfo.InvariantCulture),
            int i => i.ToString(CultureInfo.InvariantCulture),
            uint u => u.ToString(CultureInfo.InvariantCulture),
            bool b => b.ToString(),
            int[] ints => string.Join(", ", ints.Select(e => e.ToString(CultureInfo.InvariantCulture))),
            float[] floats => string.Join(", ", floats.Select(e => e.ToString("R", CultureInfo.InvariantCulture))),
            _ => value.ToString() ?? ""
        };
    }

    private bool TryMergeNestedBinder(
        BinderFile existingFile,
        BinderFile incomingFile,
        ParamDeltaConflictStrategy strategy,
        string relativePath,
        string internalPath,
        string existingSource,
        string incomingSource,
        int depth,
        List<FullModMergeConflict> conflicts,
        FullModBinderSummary summary,
        IReadOnlyDictionary<string, FullModMergeConflict> resolutionMap,
        out Memory<byte> mergedBytes)
    {
        mergedBytes = default;

        if (!BinderDocument.TryRead(existingFile.Bytes.ToArray(), out var existingBinder))
            return false;

        using (existingBinder)
        {
            if (!BinderDocument.TryRead(incomingFile.Bytes.ToArray(), out var incomingBinder))
                return false;

            using (incomingBinder)
            {
                if (!string.Equals(existingBinder.Signature, incomingBinder.Signature, StringComparison.Ordinal))
                    return false;

                MergeBinderDocuments(
                    existingBinder,
                    incomingBinder,
                    strategy,
                    relativePath,
                    internalPath,
                    existingSource,
                    incomingSource,
                    depth,
                    conflicts,
                    summary,
                    resolutionMap);

                mergedBytes = existingBinder.WriteBytes();
                return true;
            }
        }
    }

    private static BinderFile CloneBinderFile(BinderFile file)
    {
        return new BinderFile(file.Flags, file.ID, file.Name, file.Bytes.ToArray())
        {
            CompressionType = file.CompressionType
        };
    }

    private static string GetBinderEntryKey(BinderFile file)
    {
        if (!string.IsNullOrWhiteSpace(file.Name))
            return $"name:{NormalizeInternalPath(file.Name)}";

        return $"id:{file.ID}";
    }

    private static string GetBinderEntryDisplayName(BinderFile file)
    {
        return !string.IsNullOrWhiteSpace(file.Name)
            ? NormalizeInternalPath(file.Name)
            : $"[ID {file.ID}]";
    }

    private bool AllFilesEqual(IReadOnlyList<string> paths)
    {
        if (paths.Count < 2)
            return true;

        var firstInfo = new FileInfo(paths[0]);
        var firstHash = GetHash(paths[0]);

        for (var i = 1; i < paths.Count; i++)
        {
            var info = new FileInfo(paths[i]);
            if (info.Length != firstInfo.Length)
                return false;

            if (!string.Equals(firstHash, GetHash(paths[i]), StringComparison.Ordinal))
                return false;
        }

        return true;
    }

    private string GetHash(string path)
    {
        if (HashCache.TryGetValue(path, out var cached))
            return cached;

        using var stream = File.OpenRead(path);
        using var sha = SHA256.Create();
        var hash = Convert.ToHexString(sha.ComputeHash(stream));
        HashCache[path] = hash;
        return hash;
    }

    private static bool BinderEntryEquivalent(BinderFile a, BinderFile b)
    {
        return a.ID == b.ID &&
               a.Flags == b.Flags &&
               a.CompressionType == b.CompressionType &&
               a.Bytes.Span.SequenceEqual(b.Bytes.Span);
    }

    private static string SelectSource(IReadOnlyList<string> sourcePaths, ParamDeltaConflictStrategy strategy)
    {
        return strategy == ParamDeltaConflictStrategy.PreferLast
            ? sourcePaths[^1]
            : sourcePaths[0];
    }

    private static string SourceLabel(string path)
    {
        return Path.GetFullPath(path);
    }

    private static bool IsRootRegulation(string relativePath)
    {
        return string.Equals(
            NormalizeRelativePath(relativePath),
            "regulation.bin",
            StringComparison.OrdinalIgnoreCase);
    }

    private static bool ShouldIgnore(string relativePath)
    {
        var path = NormalizeRelativePath(relativePath);

        return path.StartsWith(".smithbox/", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(path, ".smithbox", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(path, "project.json", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(path, "regulation.bin.prev", StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeRelativePath(string path)
    {
        return path.Replace('\\', '/').TrimStart('/');
    }

    private static string NormalizeInternalPath(string path)
    {
        return (path ?? "").Replace('\\', '/').TrimStart('/');
    }

    private static string NormalizeUserPath(string path)
    {
        return (path ?? "").Trim().Trim('"');
    }

    private static void ValidateOutputFolder(IReadOnlyList<string> sources, string outputFullPath)
    {
        foreach (var source in sources)
        {
            var sourceFullPath = Path.GetFullPath(source)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var output = outputFullPath
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            if (string.Equals(sourceFullPath, output, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("The output folder must be different from every source mod folder.");

            if (IsSubPathOf(output, sourceFullPath))
                throw new InvalidOperationException("The output folder must not be inside a source mod folder.");

            if (IsSubPathOf(sourceFullPath, output))
                throw new InvalidOperationException("A source mod folder must not be inside the output folder.");
        }
    }

    private static bool IsSubPathOf(string candidate, string parent)
    {
        var normalizedParent = parent.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var normalizedCandidate = candidate.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        return normalizedCandidate.StartsWith(normalizedParent, StringComparison.OrdinalIgnoreCase);
    }

    private static void CopyFile(string source, string destination)
    {
        if (string.IsNullOrWhiteSpace(source) || !File.Exists(source))
            throw new FileNotFoundException("Merge source file was not found.", source);

        File.Copy(source, destination, overwrite: true);
    }

    private static void WriteReport(FullModMergeAnalysis analysis, string path)
    {
        var builder = new StringBuilder();
        builder.AppendLine("Smithbox Full Mod Auto Merge Report");
        builder.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        builder.AppendLine($"Conflict strategy: {analysis.Strategy}");
        builder.AppendLine();
        builder.AppendLine("Sources:");
        foreach (var source in analysis.SourceFolders)
            builder.AppendLine($"  - {source}");

        builder.AppendLine();
        builder.AppendLine($"Scanned files: {analysis.ScannedFiles}");
        builder.AppendLine($"Unique files copied: {analysis.UniqueFiles}");
        builder.AppendLine($"Identical duplicate files: {analysis.IdenticalFiles}");
        builder.AppendLine($"Binder files merged: {analysis.BinderFiles}");
        builder.AppendLine($"Regulation files merged: {analysis.RegulationFiles}");
        builder.AppendLine($"Ignored metadata files: {analysis.IgnoredFiles}");
        builder.AppendLine($"Conflicts: {analysis.Conflicts.Count}");
        builder.AppendLine($"Resolved conflicts: {analysis.Conflicts.Count(e => e.IsResolved)}");
        builder.AppendLine($"Unresolved conflicts: {analysis.Conflicts.Count(e => !e.IsResolved)}");

        if (analysis.Warnings.Count > 0)
        {
            builder.AppendLine();
            builder.AppendLine("Warnings:");
            foreach (var warning in analysis.Warnings)
                builder.AppendLine($"  - {warning}");
        }

        if (analysis.Conflicts.Count > 0)
        {
            builder.AppendLine();
            builder.AppendLine("Conflicts:");
            foreach (var conflict in analysis.Conflicts)
            {
                var internalPart = string.IsNullOrWhiteSpace(conflict.InternalPath)
                    ? ""
                    : $" :: {conflict.InternalPath}";
                builder.AppendLine($"  - [{conflict.Type}] {conflict.RelativePath}{internalPart}");
                builder.AppendLine($"      {conflict.ExistingSource} <-> {conflict.IncomingSource}");
                builder.AppendLine($"      {conflict.Message}");
                if (!string.IsNullOrWhiteSpace(conflict.ExistingValue) || !string.IsNullOrWhiteSpace(conflict.IncomingValue))
                    builder.AppendLine($"      Values: {conflict.ExistingValue} <-> {conflict.IncomingValue}");
                builder.AppendLine($"      Resolution: {conflict.Resolution}" +
                    (conflict.Resolution == FullModConflictResolution.Manual ? $" ({conflict.ManualValue})" : ""));
            }
        }

        File.WriteAllText(path, builder.ToString());
    }

    private sealed class BinderDocument : IDisposable
    {
        private readonly BND4 Bnd4;
        private readonly BND3 Bnd3;

        private BinderDocument(BND4 bnd4)
        {
            Bnd4 = bnd4;
        }

        private BinderDocument(BND3 bnd3)
        {
            Bnd3 = bnd3;
        }

        public List<BinderFile> Files => Bnd4 != null ? Bnd4.Files : Bnd3.Files;

        public string Signature
        {
            get
            {
                if (Bnd4 != null)
                {
                    return $"BND4|{Bnd4.Version}|{Bnd4.Format}|{Bnd4.BigEndian}|{Bnd4.BitBigEndian}|{Bnd4.Unicode}|{Bnd4.Extended}|{Bnd4.Unk04}|{Bnd4.Unk05}";
                }

                return $"BND3|{Bnd3.Version}|{Bnd3.Format}|{Bnd3.BigEndian}|{Bnd3.BitBigEndian}|{Bnd3.Unk18}";
            }
        }

        public static bool TryRead(string path, out BinderDocument document)
        {
            document = null;
            try
            {
                if (BND4.Is(path))
                {
                    document = new BinderDocument(BND4.Read(path));
                    return true;
                }

                if (BND3.Is(path))
                {
                    document = new BinderDocument(BND3.Read(path));
                    return true;
                }
            }
            catch
            {
                document?.Dispose();
                document = null;
            }

            return false;
        }

        public static bool TryRead(byte[] bytes, out BinderDocument document)
        {
            document = null;
            try
            {
                if (BND4.Is(bytes))
                {
                    document = new BinderDocument(BND4.Read(bytes.AsMemory()));
                    return true;
                }

                if (BND3.Is(bytes))
                {
                    document = new BinderDocument(BND3.Read(bytes.AsMemory()));
                    return true;
                }
            }
            catch
            {
                document?.Dispose();
                document = null;
            }

            return false;
        }

        public byte[] WriteBytes()
        {
            return Bnd4 != null ? Bnd4.Write() : Bnd3.Write();
        }

        public void Write(string path)
        {
            if (Bnd4 != null)
                Bnd4.Write(path);
            else
                Bnd3.Write(path);
        }

        public void Dispose()
        {
            if (Bnd4 is IDisposable b4)
                b4.Dispose();
            if (Bnd3 is IDisposable b3)
                b3.Dispose();
        }
    }
}
