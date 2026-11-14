using SoulsFormats;
using StudioCore.Application;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor;

/// <summary>The files a model can have beside its FLVER, which the Containers list can filter by.</summary>
public enum ModelSidecarKind
{
    Edge,
    Grass
}

/// <summary>
/// Which model containers have an EDGE or GRASS file, for the Containers list's <c>edge:</c> and <c>grass:</c> filter
/// commands. Built in the background for one list (a tab's file dictionary) the first time it is asked about, from the
/// project's files as stored, and kept for the session:
/// <list type="bullet">
/// <item>a binder (objbnd, mapbnd, ...) has the kind if it holds a .edge / .grass file (only the binder's header is read
/// after decompression);</item>
/// <item>a Sekiro map piece also has an EDGE file when its map's edgebhd holds one with its name, which the Model
/// Editor shows with it (read-only, see ModelWrapper.LoadMapPieceEdge).</item>
/// </list>
/// Models loaded since are judged by what they hold now (<see cref="Has"/>), so a file made in the EDGE Editor counts
/// before it is saved.
/// </summary>
public sealed class ModelSidecarIndex
{
    private readonly ProjectEntry Project;

    public sealed class ListIndex
    {
        public readonly ConcurrentDictionary<string, byte> Edge = new(StringComparer.OrdinalIgnoreCase);
        public readonly ConcurrentDictionary<string, byte> Grass = new(StringComparer.OrdinalIgnoreCase);

        public int Total;
        public int Done;
        public volatile bool Finished;

        /// <summary>Bumped whenever a container is found to have a kind, so a list knows to filter again.</summary>
        public int Version;
    }

    private readonly ConcurrentDictionary<FileDictionary, ListIndex> Lists = new();

    // Each Sekiro map's edgebhd file names, read once
    private readonly ConcurrentDictionary<string, HashSet<string>> MapEdgeNames = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Bumped when a loaded model's EDGE or GRASS file comes or goes without its stored files changing (an EDGE file
    /// made or taken back in the EDGE Editor), so lists filtered by a command filter again.
    /// </summary>
    public int OverlayVersion => _overlayVersion;
    private int _overlayVersion;

    public void MarkOverlayChanged() => Interlocked.Increment(ref _overlayVersion);

    public ModelSidecarIndex(ProjectEntry project)
    {
        Project = project;
    }

    /// <summary>The index of a list, started in the background if this is the first time it is asked for.</summary>
    public ListIndex Get(FileDictionary dictionary, bool isMapPieceList)
    {
        return Lists.GetOrAdd(dictionary, d =>
        {
            var index = new ListIndex { Total = d.Entries.Count };

            Task.Run(() => Build(d, isMapPieceList, index));

            return index;
        });
    }

    /// <summary>
    /// Whether a container has the kind: what its loaded models hold now, or else what the index found in its files.
    /// </summary>
    public bool Has(ListIndex index, FileDictionaryEntry entry, ModelSidecarKind kind)
    {
        if (Project.Handler.ModelData?.PrimaryBank?.Models is { } models
            && models.TryGetValue(entry, out var container)
            && container.Models.Any(m => m.FLVER != null))
        {
            return container.Models.Any(m => kind == ModelSidecarKind.Edge ? m.EDGE != null : m.GRASS != null);
        }

        var set = kind == ModelSidecarKind.Edge ? index.Edge : index.Grass;

        return set.ContainsKey(entry.Filename);
    }

    private void Build(FileDictionary dictionary, bool isMapPieceList, ListIndex index)
    {
        var isSekiro = Project.Descriptor.ProjectType is ProjectType.SDT;

        try
        {
            Parallel.ForEach(dictionary.Entries.ToList(), new ParallelOptions { MaxDegreeOfParallelism = 4 }, entry =>
            {
                try
                {
                    ReadEntry(entry, isMapPieceList && isSekiro, index);
                }
                catch (Exception e)
                {
                    Smithbox.LogError(this, LOC.Get("MODEL_SourceList_Sidecar_Index_Failed", entry.Path), e);
                }
                finally
                {
                    Interlocked.Increment(ref index.Done);
                }
            });
        }
        finally
        {
            index.Finished = true;
            Interlocked.Increment(ref index.Version);
        }
    }

    private void ReadEntry(FileDictionaryEntry entry, bool mapEdges, ListIndex index)
    {
        var found = false;

        if (mapEdges && GetMapEdgeNames(entry).Contains(entry.Filename))
        {
            found |= index.Edge.TryAdd(entry.Filename, 0);
        }

        var path = entry.Path;

        // Only binders hold other files: loose FLVERs (DES, DS2 parts and so on) have none
        if (path.Contains("bnd", StringComparison.OrdinalIgnoreCase))
        {
            var data = Project.VFS.FS.ReadFile(path);

            if (data != null)
            {
                foreach (var name in ReadBinderNames(data.Value))
                {
                    if (name.EndsWith(".edge", StringComparison.OrdinalIgnoreCase) || name.Contains(".edge.", StringComparison.OrdinalIgnoreCase))
                        found |= index.Edge.TryAdd(entry.Filename, 0);

                    if (name.EndsWith(".grass", StringComparison.OrdinalIgnoreCase) || name.Contains(".grass.", StringComparison.OrdinalIgnoreCase))
                        found |= index.Grass.TryAdd(entry.Filename, 0);
                }
            }
        }

        if (found)
        {
            Interlocked.Increment(ref index.Version);
        }
    }

    /// <summary>The file names in a binder's header, BND4 or BND3; none if it is neither.</summary>
    private static List<string> ReadBinderNames(Memory<byte> data)
    {
        try
        {
            using var reader = new BND4Reader(data);
            return reader.Files.Select(f => f.Name ?? "").ToList();
        }
        catch
        {
        }

        try
        {
            using var reader = new BND3Reader(data);
            return reader.Files.Select(f => f.Name ?? "").ToList();
        }
        catch
        {
        }

        return new List<string>();
    }

    /// <summary>
    /// The names (without extensions) in the edgebhd of a Sekiro map piece's map, e.g. m11_00_00_00_000200 for
    /// m11_00_00_00_000200.edge.dcx. The map is the folder the mapbnd is in.
    /// </summary>
    private HashSet<string> GetMapEdgeNames(FileDictionaryEntry entry)
    {
        var parts = entry.Path.Split('/', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length < 2)
            return new HashSet<string>();

        var mapId = parts[^2];

        return MapEdgeNames.GetOrAdd(mapId, id =>
        {
            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            try
            {
                var bhd = Project.VFS.FS.ReadFile($"/map/{id}/{id}.edgebhd");
                var bdt = Project.VFS.FS.ReadFile($"/map/{id}/{id}.edgebdt");

                if (bhd == null || bdt == null)
                    return names;

                using var binder = BXF4.Read(bhd.Value, bdt.Value);

                foreach (var file in binder.Files)
                {
                    var fileName = file.Name.Substring(file.Name.LastIndexOf('\\') + 1);
                    names.Add(fileName.Split('.')[0]);
                }
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("MODEL_SourceList_Sidecar_Index_Failed", $"/map/{id}/{id}.edgebhd"), e);
            }

            return names;
        });
    }
}
