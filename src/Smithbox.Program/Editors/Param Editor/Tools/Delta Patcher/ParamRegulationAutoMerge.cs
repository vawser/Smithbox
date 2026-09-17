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

public sealed class RegulationMergeSource
{
    public string Path { get; init; } = "";
    public string Filename => System.IO.Path.GetFileName(Path);
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

    private readonly record struct RowKey(int ID, int Index);

    public ParamRegulationAutoMerge(ParamDeltaPatcher patcher, ParamDeltaAutoMergeEngine engine)
    {
        Patcher = patcher;
        Engine = engine;
    }

    public bool IsSupportedProject => Patcher.Project.Descriptor.ProjectType is
        ProjectType.ER or ProjectType.AC6 or ProjectType.NR or ProjectType.DS3;

    public string SupportedProjectText => "Elden Ring, Armored Core VI, Nightreign and Dark Souls III";

    public RegulationMergeAnalysis Analyze(
        IReadOnlyList<string> sourcePaths,
        ParamDeltaConflictStrategy strategy)
    {
        var analysis = new RegulationMergeAnalysis();

        if (!IsSupportedProject)
        {
            analysis.Errors.Add($"Direct regulation merge currently supports {SupportedProjectText}.");
            return analysis;
        }

        var normalizedPaths = sourcePaths
            .Where(e => !string.IsNullOrWhiteSpace(e))
            .Select(NormalizeUserPath)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (normalizedPaths.Count < 2)
        {
            analysis.Errors.Add("Select at least two regulation files.");
            return analysis;
        }

        foreach (var path in normalizedPaths)
        {
            if (!File.Exists(path))
            {
                analysis.Errors.Add($"File not found: {path}");
                continue;
            }

            try
            {
                var source = BuildSource(path, analysis.Warnings);
                analysis.Sources.Add(source);
            }
            catch (Exception ex)
            {
                analysis.Errors.Add($"{System.IO.Path.GetFileName(path)}: {ex.Message}");
            }
        }

        if (analysis.Errors.Count > 0 || analysis.Sources.Count < 2)
            return analysis;

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
            throw new InvalidOperationException("Analyze the regulation files successfully before building an output file.");

        if (analysis.Sources.Count == 0)
            throw new InvalidOperationException("No regulation source is available.");

        outputPath = NormalizeUserPath(outputPath);
        if (string.IsNullOrWhiteSpace(outputPath))
            throw new InvalidOperationException("Choose an output path for the merged regulation file.");

        var outputFullPath = System.IO.Path.GetFullPath(outputPath);
        foreach (var source in analysis.Sources)
        {
            if (string.Equals(System.IO.Path.GetFullPath(source.Path), outputFullPath, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("The output path must not overwrite one of the input regulation files.");
        }

        // Always build on the currently loaded game's vanilla regulation. This is important
        // when one or more input regulations were automatically upgraded from an older version.
        using var binder = ReadCurrentVanillaRegulation();

        if (!ulong.TryParse(binder.Version, out var version))
            throw new InvalidDataException("The current vanilla regulation does not contain a valid parameter version.");

        var loadedVersion = Patcher.Project.Handler.ParamData.PrimaryBank.ParamVersion;
        if (version != loadedVersion)
        {
            throw new InvalidDataException(
                $"The vanilla regulation version {ParamUtils.ParseRegulationVersion(version)} does not match " +
                $"the loaded project {ParamUtils.ParseRegulationVersion(loadedVersion)}.");
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

    private RegulationMergeSource BuildSource(string path, List<string> warnings)
    {
        using var binder = ReadRegulation(path);

        if (!ulong.TryParse(binder.Version, out var version))
            throw new InvalidDataException("The regulation does not contain a valid parameter version.");

        var loadedVersion = Patcher.Project.Handler.ParamData.PrimaryBank.ParamVersion;
        var sourceParams = ParseBinderParams(binder, version, warnings, strict: false);

        ParamDeltaPatch delta;
        var autoUpgraded = false;

        if (version == loadedVersion)
        {
            delta = BuildDelta(
                path, sourceParams, loadedVersion, warnings,
                Patcher.Project.Handler.ParamData.VanillaBank.Params);
        }
        else
        {
            if (!AutoUpgradeMismatchedVersions)
            {
                throw new InvalidDataException(
                    $"param version {ParamUtils.ParseRegulationVersion(version)} does not match the loaded project " +
                    $"{ParamUtils.ParseRegulationVersion(loadedVersion)}.");
            }

            if (Patcher.Project.Descriptor.ProjectType is not (ProjectType.ER or ProjectType.AC6 or ProjectType.NR))
            {
                throw new InvalidDataException(
                    $"Automatic regulation upgrade is only available for Elden Ring, Armored Core VI and Nightreign. " +
                    $"Source is {ParamUtils.ParseRegulationVersion(version)}, loaded project is {ParamUtils.ParseRegulationVersion(loadedVersion)}.");
            }

            if (version > loadedVersion)
            {
                throw new InvalidDataException(
                    $"Automatic downgrade is not supported. Source {ParamUtils.ParseRegulationVersion(version)} is newer than " +
                    $"the loaded project {ParamUtils.ParseRegulationVersion(loadedVersion)}.");
            }

            var oldVanillaParams = LoadHistoricalVanillaParams(version, warnings);
            delta = BuildDelta(path, sourceParams, loadedVersion, warnings, oldVanillaParams);
            NormalizeDeltaForLoadedVersion(delta, warnings);
            autoUpgraded = true;

            warnings?.Add(
                $"{System.IO.Path.GetFileName(path)}: automatically upgraded param changes " +
                $"{ParamUtils.ParseRegulationVersion(version)} -> {ParamUtils.ParseRegulationVersion(loadedVersion)} " +
                $"using Smithbox Param Upgrader historical vanilla data.");
        }

        return new RegulationMergeSource
        {
            Path = path,
            OriginalParamVersion = version,
            ParamVersion = loadedVersion,
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
            _ => throw new NotSupportedException($"Direct regulation merge is not supported for {Patcher.Project.Descriptor.ProjectType}.")
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
            _ => throw new NotSupportedException($"Direct regulation merge is not supported for {Patcher.Project.Descriptor.ProjectType}.")
        };
    }

    private BND4 ReadCurrentVanillaRegulation()
    {
        var relativePath = Patcher.Project.Descriptor.ProjectType == ProjectType.DS3
            ? "Data0.bdt"
            : "regulation.bin";

        var fs = Patcher.Project.VFS.VanillaRealFS;
        if (!fs.FileExists(relativePath))
            throw new FileNotFoundException($"Current vanilla regulation was not found: {relativePath}");

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
                throw new NotSupportedException($"Direct regulation merge is not supported for {Patcher.Project.Descriptor.ProjectType}.");
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
                    var message = $"{paramName}: no compatible ParamDef/ParamType mapping was found; skipped.";
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
                    throw new InvalidDataException($"Failed to parse {paramName}: {ex.Message}", ex);

                warnings?.Add($"{paramName}: failed to parse and was skipped ({ex.Message}).");
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
        if (Patcher.Project.Descriptor.ProjectType != ProjectType.ER)
            return;

        if (version >= 10601000 && param.ParamType == "CHR_MODEL_PARAM_ST")
            param.ExpandParamSize(12, 16);

        if (version >= 11210015)
        {
            if (param.ParamType == "GAME_SYSTEM_COMMON_PARAM_ST")
                param.ExpandParamSize(880, 1024);
            if (param.ParamType == "POSTURE_CONTROL_PARAM_WEP_RIGHT_ST")
                param.ExpandParamSize(112, 144);
            if (param.ParamType == "SIGN_PUDDLE_PARAM_ST")
                param.ExpandParamSize(32, 48);
        }
    }

    private Dictionary<string, Param> LoadHistoricalVanillaParams(ulong sourceVersion, List<string> warnings)
    {
        var gameDirectory = ProjectUtils.GetGameDirectory(Patcher.Project);
        var assetRoot = Path.Join(AppContext.BaseDirectory, "Assets", "PARAM", gameDirectory);
        var infoPath = Path.Join(assetRoot, "Upgrader Information.json");

        if (!File.Exists(infoPath))
            throw new FileNotFoundException($"Param Upgrader information was not found: {infoPath}");

        var infoJson = File.ReadAllText(infoPath);
        var info = JsonSerializer.Deserialize(
            infoJson, ParamEditorJsonSerializerContext.Default.ParamUpgraderInfo);

        if (info == null)
            throw new InvalidDataException("Param Upgrader information could not be loaded.");

        var sourceVersionText = ParamUtils.ParseRegulationVersion(sourceVersion);
        var entry = info.RegulationEntries.FirstOrDefault(e => e.Version == sourceVersionText);
        if (entry == null)
        {
            throw new InvalidDataException(
                $"Smithbox does not contain Param Upgrader baseline data for {sourceVersionText}. " +
                "Upgrade this regulation manually first or use a Smithbox build that supports that version.");
        }

        var regulationPath = Path.Join(assetRoot, "Regulations", entry.Folder, "regulation.bin");
        if (!File.Exists(regulationPath))
            throw new FileNotFoundException($"Historical vanilla regulation was not found: {regulationPath}");

        using var oldVanillaBinder = ReadRegulation(regulationPath);
        if (!ulong.TryParse(oldVanillaBinder.Version, out var oldVersion))
            throw new InvalidDataException("Historical vanilla regulation does not contain a valid parameter version.");

        if (oldVersion != sourceVersion)
        {
            throw new InvalidDataException(
                $"Historical vanilla version {ParamUtils.ParseRegulationVersion(oldVersion)} does not match " +
                $"the source version {sourceVersionText}.");
        }

        return ParseBinderParams(oldVanillaBinder, oldVersion, warnings, strict: false);
    }

    private void NormalizeDeltaForLoadedVersion(ParamDeltaPatch patch, List<string> warnings)
    {
        var currentVanilla = Patcher.Project.Handler.ParamData.VanillaBank.Params;
        var removedParams = 0;
        var removedFields = 0;

        for (var paramIndex = patch.Params.Count - 1; paramIndex >= 0; paramIndex--)
        {
            var paramDelta = patch.Params[paramIndex];
            if (!currentVanilla.TryGetValue(paramDelta.Name, out var currentParam))
            {
                patch.Params.RemoveAt(paramIndex);
                removedParams++;
                continue;
            }

            var validFields = currentParam.Rows
                .SelectMany(row => row.Columns)
                .Select(column => column.Def.InternalName)
                .ToHashSet(StringComparer.Ordinal);

            foreach (var row in paramDelta.Rows)
            {
                for (var fieldIndex = row.Fields.Count - 1; fieldIndex >= 0; fieldIndex--)
                {
                    if (validFields.Contains(row.Fields[fieldIndex].Field))
                        continue;

                    row.Fields.RemoveAt(fieldIndex);
                    removedFields++;
                }
            }

            paramDelta.Rows.RemoveAll(row =>
                row.State == RowDeltaState.Modified &&
                row.Fields.Count == 0 &&
                string.IsNullOrWhiteSpace(row.Name));

            if (paramDelta.Rows.Count == 0)
                patch.Params.RemoveAt(paramIndex);
        }

        if (removedParams > 0 || removedFields > 0)
        {
            warnings?.Add(
                $"Automatic upgrade skipped {removedParams} obsolete params and {removedFields} fields " +
                "that do not exist in the loaded version.");
        }
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
                warnings?.Add($"{pair.Key}: not present in the comparison vanilla regulation; skipped.");
                continue;
            }

            var paramDelta = CompareParam(pair.Key, pair.Value, vanillaParam);
            if (paramDelta.Rows.Count > 0)
                patch.Params.Add(paramDelta);
        }

        return patch;
    }

    private ParamDelta CompareParam(string paramName, Param source, Param vanilla)
    {
        var result = new ParamDelta { Name = paramName };
        var sourceRows = BuildRowMap(source);
        var vanillaRows = BuildRowMap(vanilla);

        foreach (var sourcePair in sourceRows)
        {
            if (!vanillaRows.TryGetValue(sourcePair.Key, out var vanillaRow))
            {
                result.Rows.Add(CreateAddedRow(sourcePair.Key, sourcePair.Value));
                continue;
            }

            var modified = CreateModifiedRow(sourcePair.Key, sourcePair.Value, vanillaRow);
            if (modified.Fields.Count > 0 || !string.Equals(sourcePair.Value.Name, vanillaRow.Name, StringComparison.Ordinal))
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
        var result = new Dictionary<RowKey, Param.Row>();
        var currentRowID = 0;
        var internalIndex = 0;

        foreach (var row in param.Rows)
        {
            if (row.ID == currentRowID)
                internalIndex++;
            else
                internalIndex = 0;

            result[new RowKey(row.ID, internalIndex)] = row;
            currentRowID = row.ID;
        }

        return result;
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
        foreach (var paramDelta in patch.Params)
        {
            if (!paramsByName.TryGetValue(paramDelta.Name, out var param))
                throw new InvalidDataException($"Base regulation is missing parameter {paramDelta.Name}.");

            foreach (var rowDelta in paramDelta.Rows)
                ApplyRowDelta(paramDelta.Name, param, rowDelta);
        }
    }

    private void ApplyRowDelta(string paramName, Param param, RowDelta rowDelta)
    {
        var existing = FindRow(param, rowDelta.ID, rowDelta.Index);

        if (rowDelta.State == RowDeltaState.Deleted)
        {
            if (existing != null)
                param.RemoveRow(existing);
            return;
        }

        if (existing == null)
        {
            Param.Row template;

            if (rowDelta.State == RowDeltaState.Modified)
            {
                template = FindVanillaRow(paramName, rowDelta.ID, rowDelta.Index);
                if (template == null)
                    throw new InvalidDataException($"Cannot reconstruct {paramName} row {rowDelta.ID}:{rowDelta.Index} from vanilla.");
            }
            else
            {
                template = param.Rows.FirstOrDefault() ??
                    Patcher.Project.Handler.ParamData.VanillaBank.Params[paramName].Rows.FirstOrDefault();

                if (template == null)
                    throw new InvalidDataException($"Cannot add a row to empty parameter {paramName}; no template row exists.");
            }

            existing = new Param.Row(template, param)
            {
                ID = rowDelta.ID,
                Name = rowDelta.Name
            };

            InsertRowAtIdentity(param, existing, rowDelta.ID, rowDelta.Index);
        }
        else if (!string.IsNullOrWhiteSpace(rowDelta.Name))
        {
            existing.Name = rowDelta.Name;
        }

        Patcher.Importer.HandleFieldImport(existing, rowDelta);
    }

    private Param.Row FindVanillaRow(string paramName, int id, int index)
    {
        if (!Patcher.Project.Handler.ParamData.VanillaBank.Params.TryGetValue(paramName, out var vanillaParam))
            return null;

        return FindRow(vanillaParam, id, index);
    }

    private static Param.Row FindRow(Param param, int id, int index)
    {
        var currentRowID = 0;
        var internalIndex = 0;

        foreach (var row in param.Rows)
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

    private static void InsertRowAtIdentity(Param param, Param.Row newRow, int id, int index)
    {
        var rows = param.Rows.ToList();
        var matchingPositions = rows
            .Select((row, position) => new { row, position })
            .Where(e => e.row.ID == id)
            .Select(e => e.position)
            .ToList();

        if (matchingPositions.Count == 0)
        {
            param.AddRow(newRow);
            return;
        }

        if (index < matchingPositions.Count)
        {
            param.InsertRow(matchingPositions[index], newRow);
            return;
        }

        param.InsertRow(matchingPositions[^1] + 1, newRow);
    }

    private static string NormalizeUserPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return "";

        return path.Trim().Trim('"');
    }
}
