using Andre.Formats;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace StudioCore.Editors.ParamEditor;

/// <summary>
/// A source to merge: a regulation file (<see cref="Path"/>) or another Smithbox project (<see cref="Project"/>)
/// </summary>
public sealed record MergeSourceSpec(string Path, ProjectEntry Project)
{
    public bool IsProject => Project != null;

    public static MergeSourceSpec File(string path) => new(path ?? "", null);
    public static MergeSourceSpec FromProject(ProjectEntry project) => new("", project);
}

public sealed class RegulationMergeSource
{
    public string Path { get; init; } = "";

    /// <summary>
    /// A project's changes (this project or another one) rather than a regulation file: it has no path
    /// </summary>
    public bool IsProject { get; init; }

    public string DisplayName { get; init; }
    public string Filename => DisplayName ?? System.IO.Path.GetFileName(Path);
    public ulong OriginalParamVersion { get; init; }
    public ulong ParamVersion { get; init; }
    public bool AutoUpgraded { get; init; }
    public int ParsedParamCount { get; init; }
    public ParamDeltaPatch Delta { get; init; } = new();
}

public sealed class RegulationMergeAnalysis
{
    public List<RegulationMergeSource> Sources { get; } = new();
    public List<string> Errors { get; } = new();
    public List<string> Warnings { get; } = new();
    public ParamDeltaAutoMergeResult MergeResult { get; set; }

    public bool CanBuild => Errors.Count == 0 && MergeResult != null && MergeResult.CanApply;
}

/// <summary>
/// Converts complete regulation files into vanilla-relative Param Delta patches,
/// feeds them through the normal auto-merge engine, and can materialize the merged
/// patch back into a new encrypted regulation file.
/// </summary>
public sealed class ParamRegulationAutoMerge
{
    private readonly ParamDeltaPatcher Patcher;
    private readonly ParamDeltaAutoMergeEngine Engine;

    public bool AutoUpgradeMismatchedVersions { get; set; } = true;

    /// <summary>
    /// The current project's changes are the first (earliest) source, so clashes with them are conflicts
    /// instead of being overwritten when the merge is imported
    /// </summary>
    public bool IncludeProject { get; set; } = true;

    private readonly record struct RowKey(int ID, int Index);

    public ParamRegulationAutoMerge(ParamDeltaPatcher patcher, ParamDeltaAutoMergeEngine engine)
    {
        Patcher = patcher;
        Engine = engine;
    }

    /// <summary>
    /// Regulation files can be read and built (encrypted regulation games). Project sources, import and saved
    /// deltas work for every game.
    /// </summary>
    public bool CanUseRegulationFiles => Patcher.Project.Descriptor.ProjectType is
        ProjectType.ER or ProjectType.AC6 or ProjectType.NR or ProjectType.DS3;

    public string SupportedProjectText => LOC.Get("PARAM_AutoMerge_RegulationMerge_Supported_Hint");

    /// <summary>
    /// Regulation files only (the Full Mod merge)
    /// </summary>
    public RegulationMergeAnalysis Analyze(IReadOnlyList<string> sourcePaths, ParamDeltaConflictStrategy strategy)
    {
        return Analyze(sourcePaths.Select(MergeSourceSpec.File).ToList(), strategy);
    }

    /// <param name="sources">Regulation files and other projects, earliest first</param>
    /// <param name="includeProject">Add the current project's changes as the first source (see <see cref="IncludeProject"/>)</param>
    /// <param name="progress">Called with (step label, fraction done); may be called from a worker thread</param>
    public RegulationMergeAnalysis Analyze(
        IReadOnlyList<MergeSourceSpec> sources,
        ParamDeltaConflictStrategy strategy,
        bool includeProject = false,
        Action<string, float> progress = null)
    {
        var analysis = new RegulationMergeAnalysis();

        var specs = sources
            .Where(e => e.IsProject || !string.IsNullOrWhiteSpace(e.Path))
            .Select(e => e.IsProject ? e : MergeSourceSpec.File(NormalizeUserPath(e.Path)))
            .Where(e => e.Project != Patcher.Project)
            .DistinctBy(e => e.IsProject
                ? $"project:{e.Project.Descriptor.ProjectName}"
                : $"file:{e.Path.ToLowerInvariant()}")
            .ToList();

        // With the project as a source, one other source is enough (merging a mod into the project)
        if (specs.Count < (includeProject ? 1 : 2))
        {
            analysis.Errors.Add(LOC.Get(includeProject
                ? "PARAM_AutoMerge_Error_Select_One_Regulation"
                : "PARAM_AutoMerge_Error_Select_Two_Regulations"));
            return analysis;
        }

        // Files that share a name (every mod's regulation.bin) are labelled with their folder
        var duplicateNames = specs
            .Where(e => !e.IsProject)
            .GroupBy(e => System.IO.Path.GetFileName(e.Path), StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var stepCount = specs.Count + (includeProject ? 2 : 1);
        var step = 0;

        if (includeProject)
        {
            progress?.Invoke(LOC.Get("PARAM_DirectMerge_Progress_Project"), step++ / (float)stepCount);

            try
            {
                analysis.Sources.Add(BuildProjectSource(analysis.Warnings));
            }
            catch (Exception ex)
            {
                analysis.Errors.Add($"{LOC.Get("PARAM_DirectMerge_Project_Source")}: {ex.Message}");
            }
        }

        foreach (var spec in specs)
        {
            if (spec.IsProject)
            {
                var projectName = spec.Project.Descriptor.ProjectName;
                progress?.Invoke(projectName, step++ / (float)stepCount);

                try
                {
                    analysis.Sources.Add(BuildOtherProjectSource(spec.Project, analysis.Warnings));
                }
                catch (Exception ex)
                {
                    analysis.Errors.Add($"{projectName}: {ex.Message}");
                }

                continue;
            }

            var path = spec.Path;
            var fileName = System.IO.Path.GetFileName(path);
            progress?.Invoke(fileName, step++ / (float)stepCount);

            if (!CanUseRegulationFiles)
            {
                analysis.Errors.Add(LOC.Get("PARAM_DirectMerge_Supported_Project_Type", SupportedProjectText));
                continue;
            }

            if (!File.Exists(path))
            {
                analysis.Errors.Add(LOC.Get("PARAM_AutoMerge_Error_File_Not_Found", path));
                continue;
            }

            var label = duplicateNames.Contains(fileName)
                ? $"{System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(path))}/{fileName}"
                : fileName;

            try
            {
                var source = BuildSource(path, label, analysis.Warnings);
                analysis.Sources.Add(source);
            }
            catch (Exception ex)
            {
                analysis.Errors.Add($"{label}: {ex.Message}");
            }
        }

        if (analysis.Errors.Count > 0 || analysis.Sources.Count < 2)
            return analysis;

        progress?.Invoke(LOC.Get("PARAM_DirectMerge_Progress_Merge"), step / (float)stepCount);

        var entries = analysis.Sources
            .Select(source => new DeltaImportEntry
            {
                Filename = source.Filename,
                Delta = source.Delta
            })
            .ToList();

        analysis.MergeResult = Engine.Merge(entries, strategy);
        analysis.Errors.AddRange(analysis.MergeResult.Errors);
        return analysis;
    }

    public void BuildMergedRegulation(
        RegulationMergeAnalysis analysis,
        string outputPath)
    {
        if (analysis == null || !analysis.CanBuild)
            throw new InvalidOperationException(LOC.Get("PARAM_AutoMerge_Error_Invalid_Regulation_Merge"));

        if (analysis.Sources.Count == 0)
            throw new InvalidOperationException(LOC.Get("PARAM_AutoMerge_Error_Missing_Regulation_Source"));

        outputPath = NormalizeUserPath(outputPath);
        if (string.IsNullOrWhiteSpace(outputPath))
            throw new InvalidOperationException(LOC.Get("PARAM_AutoMerge_Error_Missing_Regulation_Output_Path"));

        var outputFullPath = System.IO.Path.GetFullPath(outputPath);
        foreach (var source in analysis.Sources)
        {
            if (source.IsProject)
                continue;

            if (string.Equals(System.IO.Path.GetFullPath(source.Path), outputFullPath, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException(LOC.Get("PARAM_AutoMerge_Error_Regulation_Output_Collision"));
        }

        // Always build on the currently loaded game's vanilla regulation. This is important
        // when one or more input regulations were automatically upgraded from an older version.
        using var binder = ReadCurrentVanillaRegulation();

        if (!ulong.TryParse(binder.Version, out var version))
            throw new InvalidDataException(LOC.Get("PARAM_AutoMerge_Error_Invalid_Vanilla_Param_Version"));

        var loadedVersion = Patcher.Project.Handler.ParamData.PrimaryBank.ParamVersion;

        var displayVersion = ParamUtils.ParseRegulationVersion(version);
        var displayProjectVersion = ParamUtils.ParseRegulationVersion(loadedVersion);

        if (version != loadedVersion)
        {
            throw new InvalidDataException(
                LOC.Get("PARAM_AutoMerge_Error_Mismatched_Vanilla_Param_Version",
                displayVersion,
                displayProjectVersion));
        }

        var originalParamTypes = new Dictionary<string, string>(StringComparer.Ordinal);
        var parsedParams = ParseBinderParams(binder, version, null, strict: false, originalParamTypes);

        // Conflict selections/manual values are edited after analysis in the Auto Merge UI.
        // Materialize those decisions into the delta immediately before writing the regulation.
        Engine.ApplyConflictResolutions(analysis.MergeResult);
        ApplyMergedPatch(parsedParams, analysis.MergeResult.Patch);

        foreach (var file in binder.Files)
        {
            if (!file.Name.EndsWith(".PARAM", StringComparison.OrdinalIgnoreCase))
                continue;

            var paramName = System.IO.Path.GetFileNameWithoutExtension(
                file.Name.Replace('\\', System.IO.Path.DirectorySeparatorChar));

            if (parsedParams.TryGetValue(paramName, out var param))
            {
                var mappedType = param.ParamType;
                if (originalParamTypes.TryGetValue(paramName, out var originalType))
                    param.ParamType = string.IsNullOrEmpty(originalType) ? null : originalType;

                file.Bytes = param.Write();
                param.ParamType = mappedType;
            }
        }

        var outputDirectory = System.IO.Path.GetDirectoryName(outputFullPath);
        if (!string.IsNullOrWhiteSpace(outputDirectory))
            Directory.CreateDirectory(outputDirectory);

        WriteRegulation(outputFullPath, binder);
    }

    private RegulationMergeSource BuildSource(string path, string label, List<string> warnings)
    {
        using var binder = ReadRegulation(path);

        if (!ulong.TryParse(binder.Version, out var version))
            throw new InvalidDataException(LOC.Get("PARAM_AutoMerge_Error_Invalid_Param_Version"));

        var sourceParams = ParseBinderParams(binder, version, warnings, strict: false);
        var delta = BuildDeltaAtProjectVersion(label, sourceParams, version, warnings, out var autoUpgraded);

        return new RegulationMergeSource
        {
            Path = path,
            DisplayName = label,
            OriginalParamVersion = version,
            ParamVersion = Patcher.Project.Handler.ParamData.PrimaryBank.ParamVersion,
            AutoUpgraded = autoUpgraded,
            ParsedParamCount = sourceParams.Count,
            Delta = delta
        };
    }

    /// <summary>
    /// The source's changes against vanilla at the project's version. A source of an older version is upgraded
    /// first (one version step at a time, field by field, as the Param Upgrader does), when auto-upgrade is on.
    /// </summary>
    private ParamDeltaPatch BuildDeltaAtProjectVersion(string label, Dictionary<string, Param> sourceParams,
        ulong version, List<string> warnings, out bool autoUpgraded)
    {
        var loadedVersion = Patcher.Project.Handler.ParamData.PrimaryBank.ParamVersion;
        autoUpgraded = false;

        if (version == loadedVersion)
        {
            return BuildDelta(label, sourceParams, loadedVersion, warnings,
                Patcher.Project.Handler.ParamData.VanillaBank.Params);
        }

        var displayVersion = ParamUtils.ParseRegulationVersion(version);
        var displayProjectVersion = ParamUtils.ParseRegulationVersion(loadedVersion);

        if (!AutoUpgradeMismatchedVersions)
        {
            throw new InvalidDataException(
                LOC.Get("PARAM_AutoMerge_Error_Mismatched_Param_Version", displayVersion, displayProjectVersion));
        }

        if (Patcher.Project.Descriptor.ProjectType is not (ProjectType.ER or ProjectType.AC6 or ProjectType.NR))
        {
            throw new InvalidDataException(
                LOC.Get("PARAM_AutoMerge_Error_Auto_Reg_Upgrade_Not_Available", displayVersion, displayProjectVersion));
        }

        if (version > loadedVersion)
        {
            throw new InvalidDataException(
                LOC.Get("PARAM_AutoMerge_Error_Auto_Reg_Upgrade_Not_Supported", displayVersion, displayProjectVersion));
        }

        var (upgradedParams, baseParams, conflictCount) = UpgradeToLoadedVersion(sourceParams, version, loadedVersion);
        var delta = BuildDelta(label, upgradedParams, loadedVersion, warnings, baseParams);
        autoUpgraded = true;

        if (conflictCount > 0)
        {
            warnings?.Add(LOC.Get("PARAM_AutoMerge_Warning_Auto_Upgrade_Conflicts", label, conflictCount));
        }

        warnings?.Add(LOC.Get("PARAM_AutoMerge_Warning_Auto_Upgrade_Complete", label, displayVersion, displayProjectVersion));

        return delta;
    }

    /// <summary>
    /// The current project's params as a source: its changes against vanilla, at the project's version
    /// </summary>
    private RegulationMergeSource BuildProjectSource(List<string> warnings)
    {
        var paramData = Patcher.Project.Handler.ParamData;
        var version = paramData.PrimaryBank.ParamVersion;
        var name = LOC.Get("PARAM_DirectMerge_Project_Source");

        var delta = BuildDelta(name, paramData.PrimaryBank.Params, version, warnings, paramData.VanillaBank.Params);

        return new RegulationMergeSource
        {
            IsProject = true,
            DisplayName = name,
            OriginalParamVersion = version,
            ParamVersion = version,
            ParsedParamCount = paramData.PrimaryBank.Params.Count,
            Delta = delta
        };
    }

    /// <summary>
    /// Another Smithbox project's saved params as a source. They're read from its files into a bank of their own
    /// (with this project's paramdefs), which isn't added to the aux banks: the analysis runs on a worker thread,
    /// and the UI reads the aux banks every frame. Edits open in that project but not saved aren't included.
    /// </summary>
    private RegulationMergeSource BuildOtherProjectSource(ProjectEntry project, List<string> warnings)
    {
        var name = project.Descriptor.ProjectName;

        // Sets up the project's file system if it hasn't been loaded this session
        Smithbox.Orchestrator.LoadAuxiliaryProject(project, ProjectInitType.ParamEditorOnly, false)
            .GetAwaiter().GetResult();

        if (project.VFS?.FS == null)
            throw new InvalidDataException(LOC.Get("PARAM_DirectMerge_Error_Project_Not_Loaded"));

        var bank = new ParamBank(name, Patcher.Project, project.VFS.FS);

        if (!bank.Load().GetAwaiter().GetResult() || bank.Params == null || bank.Params.Count == 0)
            throw new InvalidDataException(LOC.Get("PARAM_DirectMerge_Error_Project_Params_Failed"));

        var sourceParams = bank.Params.ToDictionary(e => e.Key, e => e.Value, StringComparer.Ordinal);
        var delta = BuildDeltaAtProjectVersion(name, sourceParams, bank.ParamVersion, warnings, out var autoUpgraded);

        return new RegulationMergeSource
        {
            IsProject = true,
            DisplayName = name,
            OriginalParamVersion = bank.ParamVersion,
            ParamVersion = Patcher.Project.Handler.ParamData.PrimaryBank.ParamVersion,
            AutoUpgraded = autoUpgraded,
            ParsedParamCount = sourceParams.Count,
            Delta = delta
        };
    }

    private BND4 ReadRegulation(string path)
    {
        return Patcher.Project.Descriptor.ProjectType switch
        {
            ProjectType.ER => SFUtil.DecryptERRegulation(path),
            ProjectType.AC6 => SFUtil.DecryptAC6Regulation(path),
            ProjectType.NR => SFUtil.DecryptNightreignRegulation(path),
            ProjectType.DS3 => SFUtil.DecryptDS3Regulation(path),
            _ => throw new NotSupportedException(LOC.Get("PARAM_AutoMerge_Error_Direct_Merge_Not_Supported", Patcher.Project.Descriptor.ProjectType))
        };
    }

    private BND4 ReadRegulation(byte[] data)
    {
        return Patcher.Project.Descriptor.ProjectType switch
        {
            ProjectType.ER => SFUtil.DecryptERRegulation(data),
            ProjectType.AC6 => SFUtil.DecryptAC6Regulation(data),
            ProjectType.NR => SFUtil.DecryptNightreignRegulation(data),
            ProjectType.DS3 => SFUtil.DecryptDS3Regulation(data),
            _ => throw new NotSupportedException(LOC.Get("PARAM_AutoMerge_Error_Direct_Merge_Not_Supported", Patcher.Project.Descriptor.ProjectType))
        };
    }

    private BND4 ReadCurrentVanillaRegulation()
    {
        var relativePath = Patcher.Project.Descriptor.ProjectType == ProjectType.DS3
            ? "Data0.bdt"
            : "regulation.bin";

        var fs = Patcher.Project.VFS.VanillaRealFS;
        if (!fs.FileExists(relativePath))
            throw new FileNotFoundException(
                LOC.Get("PARAM_AutoMerge_Error_Current_Vanilla_Reg_Not_Found", relativePath));

        return ReadRegulation(fs.GetFile(relativePath).GetData().ToArray());
    }

    private void WriteRegulation(string path, BND4 binder)
    {
        switch (Patcher.Project.Descriptor.ProjectType)
        {
            case ProjectType.ER:
                SFUtil.EncryptERRegulation(path, binder);
                break;
            case ProjectType.AC6:
                SFUtil.EncryptAC6Regulation(path, binder);
                break;
            case ProjectType.NR:
                SFUtil.EncryptNightreignRegulation(path, binder);
                break;
            case ProjectType.DS3:
                SFUtil.EncryptDS3Regulation(path, binder);
                break;
            default:
                throw new NotSupportedException(LOC.Get("PARAM_AutoMerge_Error_Direct_Merge_Not_Supported", Patcher.Project.Descriptor.ProjectType));
        }
    }

    private Dictionary<string, Param> ParseBinderParams(
        BND4 binder,
        ulong version,
        List<string> warnings,
        bool strict,
        Dictionary<string, string> originalParamTypes = null)
    {
        var result = new Dictionary<string, Param>(StringComparer.Ordinal);

        foreach (var file in binder.Files)
        {
            if (!file.Name.EndsWith(".PARAM", StringComparison.OrdinalIgnoreCase))
                continue;

            var paramName = System.IO.Path.GetFileNameWithoutExtension(
                file.Name.Replace('\\', System.IO.Path.DirectorySeparatorChar));

            if (result.ContainsKey(paramName))
                continue;

            try
            {
                var param = Param.ReadIgnoreCompression(file.Bytes);
                if (!PrepareParamType(paramName, param, originalParamTypes))
                {
                    var message = LOC.Get("PARAM_AutoMerge_Error_ParamParse_No_Compatible_ParamDefType", paramName);
                    if (strict)
                        throw new InvalidDataException(message);

                    warnings?.Add(message);
                    continue;
                }

                ApplyParamFixups(param, version);

                var def = Patcher.Project.Handler.ParamData.ParamDefs[param.ParamType];
                param.ApplyParamdef(def, version, paramName);
                result.Add(paramName, param);
            }
            catch (Exception ex)
            {
                if (strict)
                    throw new InvalidDataException(
                        LOC.Get("PARAM_AutoMerge_Error_ParamParse_Failed", paramName, ex.Message), ex);

                warnings?.Add(LOC.Get("PARAM_AutoMerge_Warning_ParamParse_Failed", paramName, ex.Message));
            }
        }

        return result;
    }

    private bool PrepareParamType(
        string paramName,
        Param param,
        Dictionary<string, string> originalParamTypes)
    {
        var projectType = Patcher.Project.Descriptor.ProjectType;
        var defs = Patcher.Project.Handler.ParamData.ParamDefs;

        if (projectType is ProjectType.AC6 or ProjectType.DS3)
        {
            var mustMap = string.IsNullOrEmpty(param.ParamType) ||
                !defs.ContainsKey(param.ParamType) ||
                Patcher.Project.Handler.ParamData.ParamTypeInfo.Exceptions.Contains(paramName);

            if (mustMap)
            {
                if (!Patcher.Project.Handler.ParamData.ParamTypeInfo.Mapping.TryGetValue(paramName, out var mappedType))
                    return false;

                if (originalParamTypes != null)
                    originalParamTypes[paramName] = param.ParamType ?? "";

                param.ParamType = mappedType;
            }
        }

        return !string.IsNullOrEmpty(param.ParamType) && defs.ContainsKey(param.ParamType);
    }

    private void ApplyParamFixups(Param param, ulong version)
    {
        // The same fixups the Param Upgrader applies (ER only)
        var game = Upgrader.RegulationHistory.GameFor(Patcher.Project.Descriptor.ProjectType);
        if (game != null)
            Upgrader.RegulationParamLoader.ApplyFixups(game.Value, param, version);
    }

    /// <summary>
    /// Upgrades params of an older regulation to the project's version through the Param Upgrader's version
    /// diffs. Returns the upgraded params, the vanilla params of the project's version (to take the delta
    /// against) and the number of conflicts the upgrade found (the regulation's values are kept in those).
    /// </summary>
    private (Dictionary<string, Param> Upgraded, IReadOnlyDictionary<string, Param> Vanilla, int Conflicts)
        UpgradeToLoadedVersion(Dictionary<string, Param> sourceParams, ulong sourceVersion, ulong loadedVersion)
    {
        var history = Upgrader.RegulationHistory.For(Patcher.Project)
            ?? throw new NotSupportedException(LOC.Get("PARAM_AutoMerge_Error_ParamUpgrader_Not_Loaded"));

        var (chain, versions) = history.RebuildFrom(sourceVersion, history.CreateContext(), out var missing);
        if (chain == null)
        {
            throw new InvalidDataException(LOC.Get("PARAM_AutoMerge_Error_ParamUpgrader_No_Data",
                ParamUtils.ParseRegulationVersion(sourceVersion), ParamUtils.ParseRegulationVersion(missing)));
        }

        // The chain runs to the game's version; stop at the project's
        var steps = chain.FindIndex(d => d.ToVersion == loadedVersion) + 1;
        if (steps == 0)
        {
            throw new InvalidDataException(LOC.Get("PARAM_AutoMerge_Error_ParamUpgrader_No_Data",
                ParamUtils.ParseRegulationVersion(sourceVersion), ParamUtils.ParseRegulationVersion(loadedVersion)));
        }

        var vanilla = Patcher.Project.Handler.ParamData.VanillaBank;
        var usedVersions = versions.Take(steps + 1).ToList();

        // At the game's version, the Vanilla bank is the base, as for a regulation of the project's version
        IReadOnlyDictionary<string, Param> baseParams = usedVersions[^1];
        if (vanilla.ParamVersion == loadedVersion)
        {
            usedVersions[^1] = usedVersions[^1].ToDictionary(kv => kv.Key,
                kv => vanilla.Params.TryGetValue(kv.Key, out var v) ? v : kv.Value);
            baseParams = vanilla.Params;
        }

        var conflicts = new List<Upgrader.UpgradeConflict>();
        var upgraded = Upgrader.RegulationUpgradeChain.Upgrade(
            sourceParams, chain.Take(steps).ToList(), usedVersions, conflicts);

        return (upgraded, baseParams, conflicts.Sum(c => c.RowConflict ? 1 : c.Fields.Count));
    }

    private ParamDeltaPatch BuildDelta(
        string sourcePath,
        IReadOnlyDictionary<string, Param> sourceParams,
        ulong targetVersion,
        List<string> warnings,
        IReadOnlyDictionary<string, Param> baselineParams)
    {
        var patch = new ParamDeltaPatch
        {
            ProjectType = Patcher.Project.Descriptor.ProjectType,
            ParamVersion = targetVersion,
            Tag = $"Regulation: {System.IO.Path.GetFileName(sourcePath)}"
        };

        foreach (var pair in sourceParams.OrderBy(e => e.Key, StringComparer.Ordinal))
        {
            if (!baselineParams.TryGetValue(pair.Key, out var vanillaParam))
            {
                warnings?.Add(
                    LOC.Get("PARAM_AutoMerge_Error_Build_Delta_Skipped_Param", pair.Key));

                continue;
            }

            // Row table params hold their changes in Groups only
            var paramDelta = CompareParam(pair.Key, pair.Value, vanillaParam);
            if (paramDelta.Rows.Count > 0 || paramDelta.Groups.Count > 0)
                patch.Params.Add(paramDelta);
        }

        return patch;
    }

    private ParamDelta CompareParam(string paramName, Param source, Param vanilla)
    {
        var result = new ParamDelta { Name = paramName };

        // Row table params: whole groups, so an insert or delete inside a group stays one change
        if (ParamRowGroups.IsTableParam(Patcher.Project, paramName))
        {
            result.Groups = ParamRowGroups.DiffGroups(source, vanilla);
            return result;
        }

        var sourceRows = BuildRowMap(source);
        var vanillaRows = BuildRowMap(vanilla);

        foreach (var sourcePair in sourceRows)
        {
            if (!vanillaRows.TryGetValue(sourcePair.Key, out var vanillaRow))
            {
                result.Rows.Add(CreateAddedRow(sourcePair.Key, sourcePair.Value));
                continue;
            }

            // A regulation usually holds no row names while the vanilla bank holds Smithbox's, so a blank name
            // isn't a rename: without this, every row with a name was listed as a changed row with no fields
            var renamed = !string.IsNullOrEmpty(sourcePair.Value.Name) &&
                !string.Equals(sourcePair.Value.Name, vanillaRow.Name, StringComparison.Ordinal);

            // Byte-equal rows have no field changes: skips the field by field compare for most rows
            if (!renamed && sourcePair.Value.DataEquals(vanillaRow))
                continue;

            var modified = CreateModifiedRow(sourcePair.Key, sourcePair.Value, vanillaRow);

            if (modified.Fields.Count > 0 || renamed)
                result.Rows.Add(modified);
        }

        foreach (var vanillaPair in vanillaRows)
        {
            if (sourceRows.ContainsKey(vanillaPair.Key))
                continue;

            result.Rows.Add(new RowDelta
            {
                ID = vanillaPair.Key.ID,
                Index = vanillaPair.Key.Index,
                Name = vanillaPair.Value.Name,
                State = RowDeltaState.Deleted
            });
        }

        return result;
    }

    private Dictionary<RowKey, Param.Row> BuildRowMap(Param param)
    {
        return ParamRowGroups.BuildPositionMap(param)
            .ToDictionary(e => new RowKey(e.Key.ID, e.Key.Index), e => e.Value);
    }

    private RowDelta CreateAddedRow(RowKey key, Param.Row row)
    {
        var delta = new RowDelta
        {
            ID = key.ID,
            Index = key.Index,
            Name = row.Name,
            State = RowDeltaState.Added
        };

        foreach (var column in row.Columns)
        {
            var value = column.GetValue(row);
            var serialized = column.Def.InternalType == "dummy8" && column.Def.ArrayLength > 1
                ? ParamUtils.Dummy8Write((byte[])value)
                : value?.ToString() ?? "";

            delta.Fields.Add(new FieldDelta
            {
                Field = column.Def.InternalName,
                Value = serialized
            });
        }

        return delta;
    }

    private RowDelta CreateModifiedRow(RowKey key, Param.Row row, Param.Row vanillaRow)
    {
        var delta = new RowDelta
        {
            ID = key.ID,
            Index = key.Index,
            Name = row.Name,
            State = RowDeltaState.Modified
        };

        foreach (var column in row.Columns)
        {
            var vanillaColumn = vanillaRow.Columns.FirstOrDefault(e => e.Def.InternalName == column.Def.InternalName);
            if (vanillaColumn == null)
                continue;

            var value = column.GetValue(row);
            var vanillaValue = vanillaColumn.GetValue(vanillaRow);
            if (FieldValuesEqual(value, vanillaValue))
                continue;

            var serialized = column.Def.InternalType == "dummy8" && column.Def.ArrayLength > 1
                ? ParamUtils.Dummy8Write((byte[])value)
                : value?.ToString() ?? "";

            delta.Fields.Add(new FieldDelta
            {
                Field = column.Def.InternalName,
                Value = serialized
            });
        }

        return delta;
    }

    private static bool FieldValuesEqual(object left, object right)
    {
        if (left is byte[] leftBytes && right is byte[] rightBytes)
            return leftBytes.SequenceEqual(rightBytes);

        return Equals(left, right);
    }


    private void ApplyMergedPatch(Dictionary<string, Param> paramsByName, ParamDeltaPatch patch)
    {
        var vanillaParams = Patcher.Project.Handler.ParamData.VanillaBank.Params;

        foreach (var paramDelta in patch.Params)
        {
            if (!paramsByName.TryGetValue(paramDelta.Name, out var param))
            {
                throw new InvalidDataException(
                    LOC.Get("PARAM_AutoMerge_Error_Base_Reg_Missing_Param", paramDelta.Name));
            }

            vanillaParams.TryGetValue(paramDelta.Name, out var vanillaParam);

            ApplyGroupDeltas(paramDelta.Name, param, vanillaParam, paramDelta.Groups);
            ApplyRowDeltas(paramDelta.Name, param, vanillaParam, paramDelta.Rows);
        }
    }

    /// <summary>
    /// Each row table group replaces the base regulation's group of the same ID in one step
    /// </summary>
    private static void ApplyGroupDeltas(string paramName, Param param, Param vanillaParam, List<RowGroupDelta> groups)
    {
        var fallbackTemplate = param.Rows.FirstOrDefault() ?? vanillaParam?.Rows.FirstOrDefault();

        foreach (var group in groups)
        {
            var oldRows = ParamRowGroups.GetGroup(param, group.ID);
            var templates = oldRows.Count > 0 ? oldRows : ParamRowGroups.GetGroup(vanillaParam, group.ID);
            var newRows = ParamRowGroups.CreateGroupRows(param, group, templates, fallbackTemplate);

            ParamRowGroups.ReplaceGroup(param, group.ID, newRows);
        }
    }

    /// <summary>
    /// Every (ID, Index) is matched to its row before anything changes, so deleting or adding a row can't move a
    /// later delta onto another row with the same ID
    /// </summary>
    private void ApplyRowDeltas(string paramName, Param param, Param vanillaParam, List<RowDelta> rowDeltas)
    {
        if (rowDeltas.Count == 0)
            return;

        var positions = ParamRowGroups.BuildPositionMap(param);
        var targets = rowDeltas
            .Select(rd => (Delta: rd, Existing: positions.GetValueOrDefault((rd.ID, rd.Index))))
            .ToList();

        var toDelete = new List<Param.Row>();
        var toAdd = new List<(RowDelta Delta, Param.Row Row)>();

        foreach (var (rowDelta, existing) in targets)
        {
            if (rowDelta.State == RowDeltaState.Deleted)
            {
                if (existing != null)
                    toDelete.Add(existing);

                continue;
            }

            if (existing != null)
            {
                if (!string.IsNullOrWhiteSpace(rowDelta.Name))
                    existing.Name = rowDelta.Name;

                Patcher.Importer.HandleFieldImport(existing, rowDelta);
                continue;
            }

            Param.Row template;

            if (rowDelta.State == RowDeltaState.Modified)
            {
                template = ParamRowGroups.FindRowAtPosition(vanillaParam, rowDelta.ID, rowDelta.Index);
                if (template == null)
                {
                    throw new InvalidDataException(
                        LOC.Get("PARAM_AutoMerge_Error_Row_Delta_Failed_Reconstruction",
                        paramName, rowDelta.ID, rowDelta.Index));
                }
            }
            else
            {
                template = param.Rows.FirstOrDefault() ?? vanillaParam?.Rows.FirstOrDefault();

                if (template == null)
                {
                    throw new InvalidDataException(
                        LOC.Get("PARAM_AutoMerge_Error_Row_Delta_Failed_Add", paramName));
                }
            }

            var newRow = new Param.Row(template, param)
            {
                ID = rowDelta.ID,
                Name = rowDelta.Name
            };

            Patcher.Importer.HandleFieldImport(newRow, rowDelta);
            toAdd.Add((rowDelta, newRow));
        }

        foreach (var row in toDelete)
        {
            param.RemoveRow(row);
        }

        foreach (var (rowDelta, row) in toAdd)
        {
            ParamRowGroups.InsertRowAtPosition(param, row, rowDelta.ID, rowDelta.Index);
        }
    }

    private static string NormalizeUserPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return "";

        return path.Trim().Trim('"');
    }
}
