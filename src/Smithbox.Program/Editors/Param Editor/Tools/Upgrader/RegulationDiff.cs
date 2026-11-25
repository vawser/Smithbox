using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StudioCore.Editors.ParamEditor.Upgrader;

/// <summary>
/// The changes of one game version step (FromVersion -> ToVersion), stored so that the older version can be
/// rebuilt from the newer one, plus the field migration for the step. See Claude/Documentation/Param Upgrader
/// Diffs Plan.md.
/// </summary>
public sealed class RegulationDiff
{
    public const int CurrentFormat = 1;

    public int Format { get; set; } = CurrentFormat;
    public string Game { get; set; }
    public ulong FromVersion { get; set; }
    public ulong ToVersion { get; set; }

    /// <summary>"Smithbox" for the bundled diffs, "User" for diffs made in the Param Upgrader</summary>
    public string Generator { get; set; }

    /// <summary>Params in both versions</summary>
    public List<ParamDiff> Params { get; set; } = new();

    /// <summary>Params only in the newer version</summary>
    public List<string> AddedParams { get; set; } = new();

    /// <summary>
    /// Params only in the older version, or that the newer version can't load (the paramdef doesn't fit), stored
    /// whole (.param file, base64)
    /// </summary>
    public List<RemovedParam> RemovedParams { get; set; } = new();

    /// <summary>Params in the regulation that couldn't be loaded in one of the versions; left out of upgrades</summary>
    public List<string> UnloadedParams { get; set; } = new();
}

public sealed class ParamDiff
{
    public string Name { get; set; }
    public string ParamType { get; set; }
    public int FromRowSize { get; set; }
    public int ToRowSize { get; set; }

    /// <summary>Hash of the newer param (IDs, names, order, row bytes); checked before the older param is rebuilt</summary>
    public string ToHash { get; set; }

    /// <summary>Rows only in the newer version: dropped when rebuilding</summary>
    public List<RowKey> AddedRows { get; set; } = new();

    /// <summary>Rows only in the older version: put back at Position when rebuilding</summary>
    public List<RowChange> RemovedRows { get; set; } = new();

    /// <summary>Rows in both versions that differ</summary>
    public List<RowChange> ChangedRows { get; set; } = new();

    /// <summary>The full older row order, only when it can't be derived from the newer order</summary>
    public List<RowKey> Order { get; set; }

    /// <summary>Fields that moved in this step: the value of From (older layout) is in To (newer layout)</summary>
    public List<FieldMove> FieldMoves { get; set; } = new();

    /// <summary>Values for fields new in this step (field -> value), used for rows the mod added</summary>
    public Dictionary<string, string> FieldDefaults { get; set; } = new();

    /// <summary>
    /// Values the update changed across a whole column (field -> older value -> newer value), for example a "none"
    /// reference going from 0 to -1. Applied to rows the mod added that still hold the older value.
    /// </summary>
    public Dictionary<string, Dictionary<string, string>> ValueRemaps { get; set; } = new();

    [JsonIgnore]
    public bool HasRowChanges => AddedRows.Count > 0 || RemovedRows.Count > 0 || ChangedRows.Count > 0 || Order != null;
}

/// <summary>A row, by ID and by its occurrence among rows with that ID (0 for the first)</summary>
public sealed class RowKey
{
    public int ID { get; set; }
    public int Index { get; set; }
}

public sealed class RowChange
{
    public int ID { get; set; }
    public int Index { get; set; }

    /// <summary>For removed rows: the row's index in the older param</summary>
    public int Position { get; set; }

    /// <summary>True when Name holds the older name (which may be null)</summary>
    public bool NameChanged { get; set; }
    public string Name { get; set; }

    /// <summary>Older values of the fields that differ (field -> value)</summary>
    public Dictionary<string, string> Fields { get; set; }

    /// <summary>The whole older row as hex, used instead of Fields when the fields can't reproduce its bytes</summary>
    public string Data { get; set; }
}

public sealed class FieldMove
{
    public string From { get; set; }
    public string To { get; set; }
}

public sealed class RemovedParam
{
    public string Name { get; set; }
    public string Data { get; set; }
}

/// <summary>
/// Reading and writing diff files. Empty lists are left out of the file and come back as empty lists.
/// </summary>
public static class RegulationDiffIO
{
    public static string Serialize(RegulationDiff diff)
    {
        foreach (var p in diff.Params)
        {
            if (p.AddedRows?.Count == 0) p.AddedRows = null;
            if (p.RemovedRows?.Count == 0) p.RemovedRows = null;
            if (p.ChangedRows?.Count == 0) p.ChangedRows = null;
            if (p.FieldMoves?.Count == 0) p.FieldMoves = null;
            if (p.FieldDefaults?.Count == 0) p.FieldDefaults = null;
            if (p.ValueRemaps?.Count == 0) p.ValueRemaps = null;
        }

        if (diff.AddedParams?.Count == 0) diff.AddedParams = null;
        if (diff.RemovedParams?.Count == 0) diff.RemovedParams = null;
        if (diff.UnloadedParams?.Count == 0) diff.UnloadedParams = null;

        try
        {
            return JsonSerializer.Serialize(diff, RegulationDiffJsonContext.Default.RegulationDiff);
        }
        finally
        {
            Normalize(diff);
        }
    }

    public static RegulationDiff Deserialize(string json)
    {
        var diff = JsonSerializer.Deserialize(json, RegulationDiffJsonContext.Default.RegulationDiff)
                   ?? throw new RegulationDiffException("The diff file is empty.");

        if (diff.Format > RegulationDiff.CurrentFormat)
            throw new RegulationDiffException(
                $"The diff file is format {diff.Format}; this version of Smithbox reads up to {RegulationDiff.CurrentFormat}.");

        Normalize(diff);
        return diff;
    }

    private static void Normalize(RegulationDiff diff)
    {
        diff.Params ??= new();
        diff.AddedParams ??= new();
        diff.RemovedParams ??= new();
        diff.UnloadedParams ??= new();

        foreach (var p in diff.Params)
        {
            p.AddedRows ??= new();
            p.RemovedRows ??= new();
            p.ChangedRows ??= new();
            p.FieldMoves ??= new();
            p.FieldDefaults ??= new();
            p.ValueRemaps ??= new();
        }
    }
}

// Not indented: a diff holds tens of thousands of values, and indentation more than doubles the size
[JsonSourceGenerationOptions(
    WriteIndented = false,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault)]
[JsonSerializable(typeof(RegulationDiff))]
public partial class RegulationDiffJsonContext : JsonSerializerContext
{
}
