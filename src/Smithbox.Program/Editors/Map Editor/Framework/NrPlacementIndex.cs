using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// One part with per-session placement fields (<see cref="NrPlacementFilter"/>), anywhere in the project
/// </summary>
public sealed record PlacementRecord(string MapID, string Name, string PartType, long EntityID, PlacementFields Fields,
    int ScenarioKey0 = -1, int ScenarioKey1 = -1, int ScenarioStep = -1)
{
    /// <summary>Whether the part has a whole ScenarioPlacementParam key triple (the game ignores a partial one)</summary>
    public bool HasScenario => ScenarioKey0 != -1 && ScenarioKey1 != -1 && ScenarioStep != -1;
}

/// <summary>
/// The placement fields of every map in the project, for the tools that ask "which parts use X" across maps
/// (RandomAppearParam's Used By, Claude/Documentation/Map Variations (NR) Analysis.md §4). Scanned on request in the
/// background: loaded maps as they are in the editor, the rest read from the project's files. Not kept up to date
/// with edits; the result says when it was made, and Rescan reads it again.
/// </summary>
public class NrPlacementIndex
{
    private static readonly ConditionalWeakTable<ProjectEntry, NrPlacementIndex> Indices = new();

    /// <summary>The index of a project, made the first time it is asked for (empty until scanned)</summary>
    public static NrPlacementIndex For(ProjectEntry project) => Indices.GetValue(project, p => new NrPlacementIndex(p));

    private readonly ProjectEntry Project;

    private volatile IReadOnlyList<PlacementRecord> _records;
    private volatile bool _scanning;
    private int _progress;
    private int _total;

    public DateTime ScannedAt { get; private set; }

    public bool IsScanned => _records != null;
    public bool IsScanning => _scanning;
    public int Progress => _progress;
    public int Total => _total;

    /// <summary>Every part with placement fields, or an empty list before the first scan</summary>
    public IReadOnlyList<PlacementRecord> Records => _records ?? Array.Empty<PlacementRecord>();

    private NrPlacementIndex(ProjectEntry project)
    {
        Project = project;
    }

    /// <summary>
    /// The parts whose RandomAppearParamID is this row
    /// </summary>
    public List<PlacementRecord> UsingRandomAppear(int rowId) =>
        Records.Where(r => r.Fields.RandomAppearParamID == rowId).ToList();

    /// <summary>
    /// Starts a scan of every map. Must be called on the main thread: the loaded maps are read there.
    /// </summary>
    public void Scan()
    {
        if (_scanning)
            return;

        var mapData = Project.Handler.MapData;
        var adapter = MsbGameAdapter.For(Project.Descriptor.ProjectType);

        if (mapData?.PrimaryBank == null || adapter == null)
        {
            _records = Array.Empty<PlacementRecord>();
            ScannedAt = DateTime.Now;
            return;
        }

        // Loaded maps are taken now, as their objects belong to the main thread
        var records = new List<PlacementRecord>();
        var toRead = new List<(string MapID, FileDictionaryEntry File)>();
        var scannedMaps = new HashSet<string>(mapData.PrimaryBank.Maps.Keys.Select(k => k.Filename), StringComparer.OrdinalIgnoreCase);

        foreach (var (fileEntry, wrapper) in mapData.PrimaryBank.Maps.OrderBy(e => e.Key.Filename))
        {
            var map = wrapper?.MapContainer;

            if (map is { LoadState: MapContentLoadState.Loaded })
            {
                foreach (var ent in map.Objects)
                {
                    if (ent.WrappedObject != null)
                        AddRecord(records, map.Name, ent.WrappedObject);
                }
            }
            else
            {
                toRead.Add((fileEntry.Filename, fileEntry));
            }
        }

        var fs = mapData.PrimaryBank.TargetFS;

        _scanning = true;
        _progress = 0;
        _total = toRead.Count;

        Task.Run(() =>
        {
            try
            {
                foreach (var (mapId, fileEntry) in toRead)
                {
                    try
                    {
                        var msb = MsbQueryRunner.ReadMsb(adapter, fs, fileEntry);

                        if (msb != null)
                        {
                            foreach (var part in msb.Parts.GetEntries())
                            {
                                AddRecord(records, mapId, part);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Smithbox.LogError(this, $"Placement index: {mapId} could not be read.", e);
                    }

                    Interlocked.Increment(ref _progress);
                }

                ScannedMaps = scannedMaps;
                _records = records;
                ScannedAt = DateTime.Now;
            }
            finally
            {
                _scanning = false;
            }
        });
    }

    private static void AddRecord(List<PlacementRecord> records, string mapId, object part)
    {
        if (part is not IMsbPart msbPart || !NrPlacementFilter.TryGetFields(part, out var fields))
            return;

        NrPlacementFilter.TryGetScenarioKeys(part, out var k0, out var k1, out var step);
        var hasScenario = k0 != -1 && k1 != -1 && step != -1;

        // Only parts a session can change: the rest are placed the same way every session
        if (fields.Variation < 0 && fields.RandomAppearParamID == 0 && !hasScenario)
            return;

        // NR parts keep it in EntityData, which a lookup on the part itself misses
        var entityId = MsbEntityIds.Get(part);

        records.Add(new PlacementRecord(mapId, msbPart.Name, part.GetType().Name, entityId, fields, k0, k1, step));
    }

    /// <summary>
    /// The parts whose scenario key triple matches a ScenarioPlacementParam row's keys (placeScenarioId,
    /// conditionScenarioId, conditionScenarioStage), as CSScenarioPlacementManagerImp matches them
    /// </summary>
    public List<PlacementRecord> UsingScenario(int key0, int key1, int step) =>
        Records.Where(r => r.HasScenario && r.ScenarioKey0 == key0 && r.ScenarioKey1 == key1 && r.ScenarioStep == step).ToList();

    /// <summary>
    /// How many parts of a map a map variation places (Variation, or Variation + 10 for assets that also match it)
    /// </summary>
    public int CountAtVariation(string mapId, int variation) =>
        Records.Count(r => string.Equals(r.MapID, mapId, StringComparison.OrdinalIgnoreCase) &&
            NrPlacementFilter.MatchingVariations(r.Fields, r.MapID).Contains(variation));

    /// <summary>Whether the scan read a map (a map the project doesn't have isn't in it)</summary>
    public bool HasMap(string mapId) => ScannedMaps.Contains(mapId);

    private HashSet<string> ScannedMaps = new(StringComparer.OrdinalIgnoreCase);
}
