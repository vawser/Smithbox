using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor;

/// <summary>What a marker of an EDGE file draws.</summary>
public enum EdgeMarkerKind
{
    /// <summary>A sphere at A: a grapple point, or a ledge or wall with no length.</summary>
    Point,

    /// <summary>A smaller sphere at A: the point a grapple pulls the player towards.</summary>
    Pull,

    /// <summary>A cylinder from A to B: a ledge or wall.</summary>
    Segment,

    /// <summary>A thin cylinder from A (the grapple point) to B (its pull point).</summary>
    PullLine
}

/// <summary>
/// One marker of an EDGE file, in the part's space, with the entry it was drawn for and whether that entry is the one
/// selected in the EDGE Editor.
/// </summary>
public record EdgeMarker(EdgeMarkerKind Kind, EDGE.EdgeType Type, int Entry, Vector3 A, Vector3 B, bool Selected);

/// <summary>
/// What is displayed for an EDGE file: a sphere per grapple point (V2 is only a few centimetres away, so it isn't
/// drawn), with a line to its pull point; a cylinder per ledge or wall segment.
/// </summary>
public record EdgeMarkers(List<EdgeMarker> Markers);

/// <summary>
/// The EDGE files of Sekiro parts, and the points displayed for them. These come from two places: a collision part uses
/// the file with its number in the edgebhd of its map (m10_00_00_00_000500.edge.dcx for h000500), and an object uses
/// the file in its objbnd. In both cases the points are relative to the part.
///
/// The map files are documents the EDGE Editor edits: an edit swaps a collision's whole file
/// (<see cref="SetEdge"/>, through <see cref="EdgeEditAction"/>), marks the map's <see cref="MapFileType.Edge"/> as
/// modified, and the map's save writes the edgebhd (<see cref="SaveEdges"/>). Object EDGE files are only displayed:
/// the Model Editor edits those. Nothing is read until EDGE points are displayed or the editor asks for a map.
/// </summary>
public class EdgeBank
{
    // Applied per frame, as each marker of each part creates a mesh
    private const int MaxAppliedPerFrame = 128;

    private const string EdgeExtension = ".edge.dcx";

    private record EdgeResult(MsbEntity Entity, string Key, int Stamp, EdgeMarkers Markers);

    // Counts the builds of each part's points, so a background build that finishes after a newer one (made straight
    // away by RebuildModel) doesn't put older points back. Only used on the main thread.
    private readonly System.Runtime.CompilerServices.ConditionalWeakTable<MsbEntity, System.Runtime.CompilerServices.StrongBox<int>> Stamps = new();

    private int NextStamp(MsbEntity entity) => ++Stamps.GetOrCreateValue(entity).Value;

    private int CurrentStamp(MsbEntity entity) => Stamps.TryGetValue(entity, out var box) ? box.Value : 0;

    /// <summary>
    /// The EDGE files of a map's edgebhd, keyed by file name without its extension (m10_00_00_00_000500). Every
    /// access is under a lock on the instance, as the display reads them on background tasks.
    /// </summary>
    private sealed class MapEdges
    {
        public readonly Dictionary<string, EDGE> Files = new(StringComparer.OrdinalIgnoreCase);
        public readonly Dictionary<string, DCX.Type> Compression = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>Files edited, added or removed since the binder was read or saved.</summary>
        public readonly HashSet<string> Modified = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// A number for each file's contents, new whenever it is read or changed, so callers can keep what they built
        /// from a file until it changes (see <see cref="GetVersion"/>).
        /// </summary>
        public readonly Dictionary<string, int> Versions = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Each file's contents as stored, decompressed. A file edited back to these is left as stored: compressing it
        /// again wouldn't give the game's bytes, so an undone edit would still change the binder.
        /// </summary>
        public readonly Dictionary<string, byte[]> Stored = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Each file as first read, decompressed and as stored, so a file put back to it after a save is written with
        /// the bytes it had rather than compressed again.
        /// </summary>
        public readonly Dictionary<string, (byte[] Data, byte[] Packed)> FirstRead = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>The folder part of the binder's file names, used for files the editor adds.</summary>
        public string NamePrefix = "";
    }

    private readonly ProjectEntry Project;

    private readonly ConcurrentDictionary<string, Task<MapEdges>> Maps = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, Task<EDGE>> ObjectFiles = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentQueue<EdgeResult> Completed = new();

    private Dictionary<string, string> ObjectPaths;

    public bool Visible { get; private set; }

    public EdgeBank(ProjectEntry project)
    {
        Project = project;
    }

    /// <summary>Whether the project's game has EDGE files.</summary>
    public bool CanUse() => Project.Descriptor.ProjectType is ProjectType.SDT;

    /// <summary>
    /// The edgebhd name of a collision's EDGE file: the map and the collision's number (m10_00_00_00_000500 for h000500)
    /// </summary>
    public static string GetFileKey(string mapId, string modelName)
    {
        return $"{mapId}_{modelName.Substring(1)}";
    }

    #region Display

    /// <summary>
    /// Picks up changes in what should be displayed and applies finished parts.
    /// Checked here as the filter can be changed from several places (e.g. the presets).
    /// </summary>
    public void Update(bool visible)
    {
        visible = visible && CanUse();

        var refresh = visible && !Visible;

        Visible = visible;

        if (refresh)
        {
            Refresh();
        }

        ExpireHighlight();
        ApplyCompleted();
    }

    /// <summary>
    /// Requests the EDGE points of a part, if they aren't already what is displayed.
    /// </summary>
    public void Request(MsbEntity entity)
    {
        if (!Visible)
            return;

        // A part that has left the map still gets here when it is deselected, and any points it
        // asked for would be treated as displayed when the part is put back
        if (entity.Parent == null)
            return;

        var isCollision = EntityHelper.IsPartCollision(entity);

        if (!isCollision && !EntityHelper.IsPartAsset(entity))
            return;

        var modelName = entity.GetPartModelName();

        if (modelName == entity.EdgeKey)
            return;

        entity.ClearEdgePoints();

        if (string.IsNullOrEmpty(modelName) || modelName.Length < 2)
            return;

        var mapId = entity.MapID;

        entity.EdgeKey = modelName;

        var stamp = NextStamp(entity);

        if (isCollision)
        {
            var key = GetFileKey(mapId, modelName);
            var map = GetMapEdges(mapId);

            Task.Run(async () =>
            {
                var files = await map;
                EdgeMarkers markers = null;

                lock (files)
                {
                    if (files.Files.TryGetValue(key, out var edge))
                    {
                        markers = GetMarkers(edge, GetHighlight(mapId, modelName));
                    }
                }

                if (markers != null)
                {
                    Completed.Enqueue(new EdgeResult(entity, modelName, stamp, markers));
                }
            });
        }
        else
        {
            var file = ObjectFiles.GetOrAdd(modelName, _ => Task.Run(() => ReadObjectFile(modelName)));

            Task.Run(async () =>
            {
                var edge = await file;

                if (edge != null)
                {
                    Completed.Enqueue(new EdgeResult(entity, modelName, stamp, GetMarkers(edge, -1)));
                }
            });
        }
    }

    /// <summary>
    /// Builds the points of every part of a collision model again, after its EDGE file or the selected entry has
    /// changed. Done straight away from the map's documents when they are read, so the old points don't flicker out.
    /// </summary>
    public void RebuildModel(string mapId, string modelName)
    {
        if (!Visible || modelName == null || modelName.Length < 2)
            return;

        var files = GetLoadedMapEdges(mapId);

        foreach (var entity in GetParts(mapId))
        {
            if (!EntityHelper.IsPartCollision(entity)
                || !string.Equals(entity.GetPartModelName(), modelName, StringComparison.OrdinalIgnoreCase))
                continue;

            if (files == null || entity.RenderSceneMesh == null || entity.Parent == null)
            {
                entity.ClearEdgePoints();
                Request(entity);
                continue;
            }

            EdgeMarkers markers = null;

            lock (files)
            {
                if (files.Files.TryGetValue(GetFileKey(mapId, modelName), out var edge))
                {
                    markers = GetMarkers(edge, GetHighlight(mapId, modelName));
                }
            }

            entity.ClearEdgePoints();
            entity.EdgeKey = entity.GetPartModelName();
            NextStamp(entity);

            if (markers != null)
            {
                entity.ApplyEdgePoints(markers);
            }
        }
    }

    /// <summary>
    /// Rebuilds everything from the current data, so changes to the files show when EDGE points are turned back on.
    /// Map files with unsaved edits are kept; the others, and the object files, are read again.
    /// </summary>
    private void Refresh()
    {
        ObjectFiles.Clear();

        foreach (var mapId in Maps.Keys.ToList())
        {
            if (!HasEdits(mapId))
            {
                Maps.TryRemove(mapId, out _);
            }
        }

        foreach (var entity in GetParts(null))
        {
            // Entities still being set up get their points along with the rest of their drawable
            if (entity.SetupRenderMesh)
            {
                entity.ClearEdgePoints();
                entity.RefreshEdgePoints();
            }
        }
    }

    private void ApplyCompleted()
    {
        for (var i = 0; i < MaxAppliedPerFrame && Completed.TryDequeue(out var result); i++)
        {
            // The part may have changed, or left the map, since this was requested
            if (result.Entity.EdgeKey == result.Key && result.Stamp == CurrentStamp(result.Entity) && result.Entity.RenderSceneMesh != null)
            {
                result.Entity.ApplyEdgePoints(result.Markers);
            }
        }
    }

    /// <summary>The parts of the loaded maps, or of one of them.</summary>
    private IEnumerable<MsbEntity> GetParts(string mapId)
    {
        foreach (var entry in Project.Handler.MapData.PrimaryBank.Maps)
        {
            var container = entry.Value.MapContainer;

            if (container == null || (mapId != null && !string.Equals(container.Name, mapId, StringComparison.OrdinalIgnoreCase)))
                continue;

            foreach (var ent in container.Objects)
            {
                if (ent is MsbEntity msbEnt)
                {
                    yield return msbEnt;
                }
            }
        }
    }

    /// <summary>
    /// The markers of a file. Each entry gets its own, so an entry can be drawn as selected (the game's files repeat
    /// a few entries exactly, which then draw twice). <paramref name="selected"/> is the selected entry, or -1.
    /// </summary>
    private static EdgeMarkers GetMarkers(EDGE edge, int selected)
    {
        List<EdgeMarker> markers = new(edge.Edges.Count + 8);

        for (var i = 0; i < edge.Edges.Count; i++)
        {
            var entry = edge.Edges[i];
            var isSelected = i == selected;

            if (entry.Type is EDGE.EdgeType.Grapple)
            {
                markers.Add(new EdgeMarker(EdgeMarkerKind.Point, entry.Type, i, entry.V1, entry.V1, isSelected));

                // The point the player is pulled towards, which is left empty on some grapple points
                if (entry.V3 != Vector3.Zero)
                {
                    markers.Add(new EdgeMarker(EdgeMarkerKind.Pull, entry.Type, i, entry.V3, entry.V3, isSelected));

                    if (entry.V3 != entry.V1)
                    {
                        markers.Add(new EdgeMarker(EdgeMarkerKind.PullLine, entry.Type, i, entry.V1, entry.V3, isSelected));
                    }
                }
            }
            // A segment with no length has no direction to draw it in
            else if (entry.V1 == entry.V2)
            {
                markers.Add(new EdgeMarker(EdgeMarkerKind.Point, entry.Type, i, entry.V1, entry.V1, isSelected));
            }
            else
            {
                markers.Add(new EdgeMarker(EdgeMarkerKind.Segment, entry.Type, i, entry.V1, entry.V2, isSelected));
            }
        }

        return new EdgeMarkers(markers);
    }

    #endregion

    #region Highlight

    // The EDGE Editor's selected entry, drawn in the selected colour while the tool keeps asking for it
    private string HighlightMap;
    private string HighlightModel;
    private volatile int HighlightEntry = -1;
    private int Frame;
    private int HighlightFrame;

    /// <summary>
    /// Draws an entry of a collision's file as selected. Call every frame the entry should stay so: once the EDGE
    /// Editor stops asking (closed, or the part deselected), the highlight goes after a couple of frames.
    /// </summary>
    public void SetHighlight(string mapId, string modelName, int entry)
    {
        HighlightFrame = Frame;

        if (string.Equals(mapId, HighlightMap, StringComparison.OrdinalIgnoreCase)
            && string.Equals(modelName, HighlightModel, StringComparison.OrdinalIgnoreCase)
            && entry == HighlightEntry)
        {
            return;
        }

        var (oldMap, oldModel) = (HighlightMap, HighlightModel);

        HighlightMap = mapId;
        HighlightModel = modelName;
        HighlightEntry = entry;

        if (oldMap != null && oldModel != null)
        {
            RebuildModel(oldMap, oldModel);
        }

        if (mapId != null && modelName != null
            && !(string.Equals(mapId, oldMap, StringComparison.OrdinalIgnoreCase)
                 && string.Equals(modelName, oldModel, StringComparison.OrdinalIgnoreCase)))
        {
            RebuildModel(mapId, modelName);
        }
    }

    private void ExpireHighlight()
    {
        Frame++;

        if (HighlightMap != null && Frame - HighlightFrame > 2)
        {
            SetHighlight(null, null, -1);
        }
    }

    /// <summary>The selected entry of a collision's file, or -1 if none of it is selected.</summary>
    private int GetHighlight(string mapId, string modelName)
    {
        return string.Equals(mapId, HighlightMap, StringComparison.OrdinalIgnoreCase)
               && string.Equals(modelName, HighlightModel, StringComparison.OrdinalIgnoreCase)
            ? HighlightEntry
            : -1;
    }

    /// <summary>
    /// Builds the points of every part that displays them again, e.g. after the colours or sizes have changed.
    /// </summary>
    public void RebuildAll()
    {
        if (!Visible)
            return;

        foreach (var entity in GetParts(null))
        {
            if (entity.EdgeKey != null)
            {
                entity.ClearEdgePoints();
                Request(entity);
            }
        }
    }

    #endregion

    #region Documents

    /// <summary>
    /// Reads the EDGE files of a map if they haven't been, waiting for them. False if the map has no edgebhd or it
    /// can't be read.
    /// </summary>
    public bool EnsureMapLoaded(string mapId)
    {
        if (!CanUse())
            return false;

        try
        {
            GetMapEdges(mapId).Wait();
        }
        catch (Exception)
        {
            return false;
        }

        return GetLoadedMapEdges(mapId) != null;
    }

    /// <summary>Whether the map's EDGE files have been read.</summary>
    public bool IsMapLoaded(string mapId) => GetLoadedMapEdges(mapId) != null;

    /// <summary>
    /// A copy of a collision's EDGE file as edited, to change and hand back with <see cref="SetEdge"/>.
    /// Null if it has none or the map's files haven't been read.
    /// </summary>
    public EDGE GetEdge(string mapId, string modelName)
    {
        var files = GetLoadedMapEdges(mapId);

        if (files == null || string.IsNullOrEmpty(modelName) || modelName.Length < 2)
            return null;

        lock (files)
        {
            return files.Files.TryGetValue(GetFileKey(mapId, modelName), out var edge) ? Copy(edge) : null;
        }
    }

    /// <summary>
    /// Puts a copy of an EDGE file in place of a collision's, for an edit or its undo. A collision without one gets
    /// it; null takes the collision's file out. Use <see cref="EdgeEditAction"/>, which marks the map as modified.
    /// </summary>
    public void SetEdge(string mapId, string modelName, EDGE edge)
    {
        var files = GetLoadedMapEdges(mapId);

        if (files == null || string.IsNullOrEmpty(modelName) || modelName.Length < 2)
            return;

        var key = GetFileKey(mapId, modelName);

        lock (files)
        {
            if (edge == null)
            {
                if (!files.Files.Remove(key))
                    return;
            }
            else
            {
                files.Files[key] = Copy(edge);
            }

            files.Modified.Add(key);
            files.Versions[key] = Interlocked.Increment(ref VersionCounter);
        }
    }

    private int VersionCounter;

    /// <summary>
    /// The version of a collision's EDGE file: a number that changes whenever the file is read again or changed.
    /// -1 if it has none or the map's files haven't been read.
    /// </summary>
    public int GetVersion(string mapId, string modelName)
    {
        var files = GetLoadedMapEdges(mapId);

        if (files == null || string.IsNullOrEmpty(modelName) || modelName.Length < 2)
            return -1;

        lock (files)
        {
            var key = GetFileKey(mapId, modelName);

            return files.Files.ContainsKey(key) ? files.Versions.GetValueOrDefault(key, 0) : -1;
        }
    }

    /// <summary>
    /// The collision parts whose EDGE points are drawn and can be edited: in a loaded map, shown, with a file read.
    /// </summary>
    public IEnumerable<MsbEntity> GetEditableParts()
    {
        if (!Visible)
            yield break;

        foreach (var entity in GetParts(null))
        {
            if (entity.EdgeKey == null || entity.EdgePointMeshes == null || !entity.EditorVisible
                || !EntityHelper.IsPartCollision(entity))
                continue;

            if (GetVersion(entity.MapID, entity.EdgeKey) < 0)
                continue;

            yield return entity;
        }
    }

    /// <summary>
    /// Every shown collision part of the loaded maps, with or without an EDGE file: where new points can be placed.
    /// </summary>
    public IEnumerable<MsbEntity> GetCollisionParts()
    {
        foreach (var entity in GetParts(null))
        {
            if (entity.Parent == null || !entity.EditorVisible || entity.RenderSceneMesh == null
                || !EntityHelper.IsPartCollision(entity))
                continue;

            yield return entity;
        }
    }

    /// <summary>Whether a collision's EDGE file has been changed since it was read or saved.</summary>
    public bool IsModified(string mapId, string modelName)
    {
        var files = GetLoadedMapEdges(mapId);

        if (files == null || string.IsNullOrEmpty(modelName) || modelName.Length < 2)
            return false;

        lock (files)
        {
            return files.Modified.Contains(GetFileKey(mapId, modelName));
        }
    }

    /// <summary>Whether any EDGE file of the map has been changed since it was read or saved.</summary>
    public bool HasEdits(string mapId)
    {
        var files = GetLoadedMapEdges(mapId);

        if (files == null)
            return false;

        lock (files)
        {
            return files.Modified.Count > 0;
        }
    }

    public static EDGE Copy(EDGE edge)
    {
        return new EDGE
        {
            ID = edge.ID,
            Edges = edge.Edges.Select(e => e.Clone()).ToList()
        };
    }

    /// <summary>
    /// Forgets the EDGE files of an unloaded map, which are read again when needed. Files with unsaved edits are kept
    /// until they are saved.
    /// </summary>
    public void OnUnloadMap(string mapId)
    {
        if (GetLoadedMapEdges(mapId) != null && !HasEdits(mapId))
        {
            Maps.TryRemove(mapId, out _);
        }
    }

    private Task<MapEdges> GetMapEdges(string mapId)
    {
        return Maps.GetOrAdd(mapId, _ => Task.Run(() => ReadMapFiles(mapId)));
    }

    private MapEdges GetLoadedMapEdges(string mapId)
    {
        return mapId != null && Maps.TryGetValue(mapId, out var task) && task.IsCompletedSuccessfully ? task.Result : null;
    }

    private static string GetBhdPath(string mapId) => Path.Join("map", mapId, $"{mapId}.edgebhd");
    private static string GetBdtPath(string mapId) => Path.Join("map", mapId, $"{mapId}.edgebdt");

    // e.g. m10_00_00_00_000500.edge.dcx, after any folders
    private static string GetFileName(string binderName) => binderName.Substring(binderName.LastIndexOf('\\') + 1);

    private MapEdges ReadMapFiles(string mapId)
    {
        var edges = new MapEdges();

        var bhdPath = GetBhdPath(mapId);
        var bdtPath = GetBdtPath(mapId);

        try
        {
            if (!Project.VFS.FS.FileExists(bhdPath) || !Project.VFS.FS.FileExists(bdtPath))
                return edges;

            var bhdData = (Memory<byte>)Project.VFS.FS.ReadFile(bhdPath);
            var bdtData = (Memory<byte>)Project.VFS.FS.ReadFile(bdtPath);

            using var binder = BXF4.Read(bhdData, bdtData);

            foreach (var file in binder.Files)
            {
                var fileName = GetFileName(file.Name);

                if (!fileName.EndsWith(EdgeExtension, StringComparison.OrdinalIgnoreCase))
                    continue;

                var name = fileName.Split('.')[0];
                var data = DCX.Decompress(file.Bytes, out var compression);

                edges.Files[name] = EDGE.Read(data);
                edges.Compression[name] = compression;
                edges.Versions[name] = Interlocked.Increment(ref VersionCounter);
                edges.Stored[name] = data.ToArray();
                edges.FirstRead[name] = (edges.Stored[name], file.Bytes.ToArray());
                edges.NamePrefix = file.Name.Substring(0, file.Name.Length - fileName.Length);
            }
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("MAP_Edge_Failed_To_Read", bhdPath), e);
        }

        return edges;
    }

    /// <summary>
    /// Writes the EDGE files of a map that have been changed into its edgebhd, leaving the other files' bytes as they
    /// are (the binder round trips byte for byte, EdgeSurvey). Returns false if it couldn't be written.
    /// </summary>
    public bool SaveEdges(string mapId)
    {
        var edges = GetLoadedMapEdges(mapId);

        if (edges == null)
            return true;

        var bhdPath = GetBhdPath(mapId);
        var bdtPath = GetBdtPath(mapId);

        lock (edges)
        {
            // Files edited back to what is stored (an undone edit) are left as they are
            foreach (var name in edges.Modified.ToList())
            {
                if (edges.Files.TryGetValue(name, out var current)
                    && edges.Stored.TryGetValue(name, out var stored)
                    && current.Write().AsSpan().SequenceEqual(stored))
                {
                    edges.Modified.Remove(name);
                }
            }

            if (edges.Modified.Count == 0)
                return true;

            try
            {
                BXF4 binder;

                if (Project.VFS.FS.FileExists(bhdPath) && Project.VFS.FS.FileExists(bdtPath))
                {
                    var bhdData = (Memory<byte>)Project.VFS.FS.ReadFile(bhdPath);
                    var bdtData = (Memory<byte>)Project.VFS.FS.ReadFile(bdtPath);

                    binder = BXF4.Read(bhdData, bdtData);
                }
                else
                {
                    binder = CreateBinder();
                }

                using (binder)
                {
                    foreach (var name in edges.Modified)
                    {
                        var fileName = name + EdgeExtension;

                        bool IsThis(BinderFile e) => GetFileName(e.Name).Equals(fileName, StringComparison.OrdinalIgnoreCase);

                        if (!edges.Files.TryGetValue(name, out var edge))
                        {
                            binder.Files.RemoveAll(IsThis);
                            continue;
                        }

                        var compression = edges.Compression.GetValueOrDefault(name, DCX.Type.DCX_KRAK);
                        var data = edge.Write();
                        var bytes = edges.FirstRead.TryGetValue(name, out var first) && data.AsSpan().SequenceEqual(first.Data)
                            ? first.Packed
                            : DCX.Compress(data, compression);

                        var file = binder.Files.FirstOrDefault(IsThis);

                        if (file != null)
                        {
                            file.Bytes = bytes;
                        }
                        else
                        {
                            var nextId = binder.Files.Count == 0 ? 0 : binder.Files.Max(e => e.ID) + 1;
                            binder.Files.Add(new BinderFile(Binder.FileFlags.Flag1, nextId, edges.NamePrefix + fileName, bytes));
                        }
                    }

                    binder.Write(out byte[] newBhdBytes, out byte[] newBdtBytes);

                    Project.VFS.ProjectFS.WriteFile(bhdPath, newBhdBytes);
                    Project.VFS.ProjectFS.WriteFile(bdtPath, newBdtBytes);
                }

                Smithbox.Log(this, LOC.Get("MAP_Edge_Saved", bhdPath, edges.Modified.Count));

                // What is stored now, which later edits are compared with
                foreach (var name in edges.Modified)
                {
                    if (edges.Files.TryGetValue(name, out var edge))
                        edges.Stored[name] = edge.Write();
                    else
                        edges.Stored.Remove(name);
                }

                edges.Modified.Clear();

                return true;
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("MAP_Edge_Failed_To_Write", bhdPath), e);
                return false;
            }
        }
    }

    /// <summary>
    /// An empty edgebhd, set up as the game's are (EdgeSurvey round trips them byte for byte).
    /// </summary>
    private static BXF4 CreateBinder()
    {
        return new BXF4
        {
            Version = "07D7R6",
            Format = Binder.Format.IDs | Binder.Format.Names1 | Binder.Format.Names2 | Binder.Format.Compression,
            Unicode = true,
            Extended = 4
        };
    }

    #endregion

    #region Objects

    private EDGE ReadObjectFile(string modelName)
    {
        var path = GetObjectPath(modelName);

        if (path == null)
            return null;

        try
        {
            var data = Project.VFS.FS.ReadFile(path);

            if (data == null)
                return null;

            var binder = BND4.Read(data.Value);
            var file = binder.Files.FirstOrDefault(e => e.Name.EndsWith(".edge", StringComparison.OrdinalIgnoreCase));

            return file != null ? EDGE.Read(file.Bytes) : null;
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("MAP_Edge_Failed_To_Read", path), e);
        }

        return null;
    }

    /// <summary>
    /// Gets the path of the binder that holds an object model.
    /// </summary>
    private string GetObjectPath(string modelName)
    {
        lock (ObjectFiles)
        {
            if (ObjectPaths == null)
            {
                ObjectPaths = new(StringComparer.OrdinalIgnoreCase);

                foreach (var entry in Project.Locator.AssetFiles.Entries)
                {
                    ObjectPaths.TryAdd(entry.Filename, entry.Path);
                }
            }

            return ObjectPaths.GetValueOrDefault(modelName);
        }
    }

    #endregion
}
