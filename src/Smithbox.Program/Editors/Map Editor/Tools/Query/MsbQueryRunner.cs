using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// A query in progress. The matches are passed over through the queue and picked up as the tool is drawn.
/// </summary>
public class MsbQueryRun
{
    public readonly CancellationTokenSource Cancellation = new();
    public readonly ConcurrentQueue<MsbQueryMatch> Found = new();

    public volatile int Progress;
    public volatile int Total;
    public volatile string Status = "";
    public volatile bool Done;
    public volatile bool Truncated;

    /// <summary>
    /// The most matches that are kept, or 0 for no limit
    /// </summary>
    public int MaxResults;

    public int Count;

    public void Cancel() => Cancellation.Cancel();
}

/// <summary>
/// Gathers the maps to query on the main thread, and runs the query over them in the background. A map that isn't
/// loaded is read from the project for the query, without being loaded, so a query of every map can take a while.
/// </summary>
public class MsbQueryRunner
{
    public MapEditorView View;
    public ProjectEntry Project;

    public MsbQueryRunner(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    /// <summary>
    /// Whether the project's maps can be read and written
    /// </summary>
    public bool CanUse() => MsbGameAdapter.For(Project.Descriptor.ProjectType) != null;

    #region Sources
    /// <summary>
    /// A loaded map, queried as it is in the editor
    /// </summary>
    public MsbQuerySource GetLoadedSource(MapContainer map)
    {
        var fileEntry = View.Selection.GetFileEntryFromMapID(map.Name);

        return new MsbQuerySource(map.Name, GetMapAlias(map.Name), fileEntry, GetEntities(map));
    }

    /// <summary>
    /// The maps loaded in this view, in name order
    /// </summary>
    public List<MsbQuerySource> GetLoadedSources()
    {
        return GetLoadedMaps().Select(GetLoadedSource).ToList();
    }

    public List<MapContainer> GetLoadedMaps()
    {
        return Project.Handler.MapData.PrimaryBank.Maps
            .Select(e => e.Value?.MapContainer)
            .Where(IsLoadedHere)
            .OrderBy(e => e.Name)
            .ToList();
    }

    /// <summary>
    /// Every map of the project, in name order. The ones loaded in this view are queried as they are in the editor,
    /// the rest are read from the project.
    /// </summary>
    public List<MsbQuerySource> GetGlobalSources()
    {
        var sources = new List<MsbQuerySource>();

        foreach (var (fileEntry, wrapper) in Project.Handler.MapData.PrimaryBank.Maps.OrderBy(e => e.Key.Filename))
        {
            var map = wrapper?.MapContainer;
            var entities = IsLoadedHere(map) ? GetEntities(map) : null;

            sources.Add(new MsbQuerySource(fileEntry.Filename, GetMapAlias(fileEntry.Filename), fileEntry, entities));
        }

        return sources;
    }

    /// <summary>
    /// Whether a map is loaded, and its objects belong to this view
    /// </summary>
    public bool IsLoadedHere(MapContainer map)
    {
        return map is { LoadState: MapContentLoadState.Loaded } && (map.OwnerView == null || map.OwnerView == View);
    }

    /// <summary>
    /// The parts, regions and events of a loaded map, taken on the main thread
    /// </summary>
    public static List<MsbEntity> GetEntities(MapContainer map)
    {
        return map.Objects.OfType<MsbEntity>()
            .Where(e => e.Type is MsbEntityType.Part or MsbEntityType.Region or MsbEntityType.Event && e.WrappedObject is IMsbEntry)
            .ToList();
    }

    public string GetMapAlias(string mapID)
    {
        try
        {
            return AliasHelper.GetMapNameAlias(Project, mapID) ?? "";
        }
        catch
        {
            return "";
        }
    }
    #endregion

    #region Run
    /// <summary>
    /// Starts the query on a background task. The progress counts the maps.
    /// </summary>
    public MsbQueryRun Start(IMsbQueryFilter query, List<MsbQuerySource> sources, int maxResults)
    {
        var run = new MsbQueryRun
        {
            MaxResults = maxResults,
            Total = sources.Count
        };

        var token = run.Cancellation.Token;
        var matcher = new MsbQueryMatcher(Project);
        var adapter = MsbGameAdapter.For(Project.Descriptor.ProjectType);
        var fs = Project.Handler.MapData.PrimaryBank.TargetFS;

        Task.Run(() =>
        {
            try
            {
                Run(run, matcher, adapter, fs, query, sources, token);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("MAP_SearchTool_Failed"), e);
            }
            finally
            {
                run.Done = true;
            }
        }, token);

        return run;
    }

    private void Run(MsbQueryRun run, MsbQueryMatcher matcher, MsbGameAdapter adapter, Andre.IO.VFS.VirtualFileSystem fs,
        IMsbQueryFilter query, List<MsbQuerySource> sources, CancellationToken token)
    {
        foreach (var source in sources)
        {
            token.ThrowIfCancellationRequested();

            run.Status = source.MapID;

            try
            {
                if (!query.MatchMap(matcher, source.MapID, source.Alias))
                    continue;

                List<MsbQueryEntry> entries;

                if (source.Entities != null)
                {
                    entries = matcher.GetEntries(source.MapID, source.Entities);
                }
                else
                {
                    var msb = ReadMsb(adapter, fs, source.FileEntry);

                    if (msb == null)
                        continue;

                    entries = matcher.GetEntries(source.MapID, msb);
                }

                if (!QueryMap(run, matcher, query, source, entries, token))
                    return;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                // A loaded map can be edited while it is queried, which can upset the lists being read
                Smithbox.LogError(this, LOC.Get("MAP_SearchTool_Map_Failed", source.MapID), e);
            }
            finally
            {
                run.Progress++;
            }
        }
    }

    /// <summary>
    /// Reads a map's MSB from the project, or null if it can't be read
    /// </summary>
    public static IMsb ReadMsb(MsbGameAdapter adapter, Andre.IO.VFS.VirtualFileSystem fs, FileDictionaryEntry fileEntry)
    {
        if (adapter == null || fileEntry == null || !fs.FileExists(fileEntry.Path))
            return null;

        var data = fs.ReadFile(fileEntry.Path);

        if (data == null)
            return null;

        return adapter.Read(data.Value);
    }

    /// <summary>
    /// Returns false once there are as many matches as are kept
    /// </summary>
    private static bool QueryMap(MsbQueryRun run, MsbQueryMatcher matcher, IMsbQueryFilter query, MsbQuerySource source,
        List<MsbQueryEntry> entries, CancellationToken token)
    {
        foreach (var entry in entries)
        {
            token.ThrowIfCancellationRequested();

            if (!query.MatchEntry(matcher, entry))
                continue;

            var entryLabel = GetEntryLabel(entry);

            if (query.Level is MsbQueryLevel.Entry)
            {
                string match = null;

                var fields = query.NeedsFields || query.KeepFields ? matcher.GetFields(entry.Object) : null;

                if (query.NeedsFields && !query.MatchFields(matcher, entry, fields, out match))
                    continue;

                var label = match != null ? $"{entryLabel}  {match}" : entryLabel;

                var found = new MsbQueryMatch(source, entry, null, label) { Fields = query.KeepFields ? fields : null };

                if (!AddMatch(run, found))
                    return false;

                continue;
            }

            foreach (var field in matcher.GetFields(entry.Object))
            {
                if (!query.MatchField(matcher, entry, field))
                    continue;

                if (!AddMatch(run, new MsbQueryMatch(source, entry, field, $"{entryLabel}  {GetFieldLabel(field)}")))
                    return false;
            }
        }

        return true;
    }

    /// <summary>
    /// An object as it is listed: its name, the alias of its model, and its type
    /// </summary>
    public static string GetEntryLabel(MsbQueryEntry entry)
    {
        var label = string.IsNullOrEmpty(entry.Alias) ? entry.Name : $"{entry.Name} <{entry.Alias}>";

        return $"{label}  [{MsbQueryMatcher.GetCategoryName(entry.Category)}: {entry.TypeName}]";
    }

    public static string GetFieldLabel(MsbQueryField field)
    {
        return $"{field.Path} = {field.Shown}";
    }

    private static bool AddMatch(MsbQueryRun run, MsbQueryMatch match)
    {
        if (run.MaxResults > 0 && run.Count >= run.MaxResults)
        {
            run.Truncated = true;
            return false;
        }

        run.Count++;
        run.Found.Enqueue(match);

        return true;
    }
    #endregion

    #region Navigation
    /// <summary>
    /// Selects and frames the object of a match. A map that isn't loaded is loaded first, and the object selected
    /// once it has loaded.
    /// </summary>
    public void Jump(MsbQueryMatch match)
    {
        var map = View.Selection.GetMapContainerFromMapID(match.MapID);

        if (map != null && IsLoadedHere(map) && TryResolve(map, match.Entry, out var entity))
        {
            View.Selection.SelectedMapID = map.Name;
            View.Selection.SelectedMapContainer = map;

            View.ViewportSelection.ClearSelection();
            View.ViewportSelection.AddSelection(entity);
            View.ViewportSelection.GotoTreeTarget = entity;
            View.FrameAction.ApplyViewportFrame();
            return;
        }

        // The command queue loads the map if needed, and selects the object once it is loaded
        EditorCommandQueue.AddCommand($"map/select/{match.MapID}/{match.Entry.Name}/{match.Entry.Category}");
    }

    /// <summary>
    /// Finds the entity of a matched object in a loaded map: the one it was found as, if the map still holds it, or
    /// else the one at its position in its list with its name and type, or else the first with its name and type.
    /// </summary>
    public static bool TryResolve(MapContainer map, MsbQueryEntry entry, out MsbEntity entity, HashSet<Entity> objects = null)
    {
        entity = null;

        if (entry.Entity != null && (objects?.Contains(entry.Entity) ?? map.Objects.Contains(entry.Entity)))
        {
            entity = entry.Entity;
            return true;
        }

        var list = entry.Category switch
        {
            MsbEntityType.Part => map.Parts,
            MsbEntityType.Region => map.Regions,
            MsbEntityType.Event => map.Events,
            _ => null
        };

        if (list == null)
            return false;

        bool IsSame(Entity e) => e is MsbEntity m && m.WrappedObject is IMsbEntry msbEntry &&
            msbEntry.Name == entry.Name && m.WrappedObject.GetType().Name == entry.TypeName &&
            (objects?.Contains(m) ?? true);

        if (entry.Index >= 0 && entry.Index < list.Count && IsSame(list[entry.Index]))
        {
            entity = (MsbEntity)list[entry.Index];
            return true;
        }

        entity = list.FirstOrDefault(IsSame) as MsbEntity;

        return entity != null;
    }

    /// <summary>
    /// Finds a matched object in an MSB read again: at its position in its list with its name and type, or else the
    /// first with its name and type
    /// </summary>
    public static object TryResolve(IMsb msb, MsbQueryEntry entry)
    {
        IReadOnlyList<IMsbEntry> list = entry.Category switch
        {
            MsbEntityType.Part => msb.Parts.GetEntries(),
            MsbEntityType.Region => msb.Regions.GetEntries(),
            MsbEntityType.Event => msb.Events.GetEntries(),
            _ => null
        };

        if (list == null)
            return null;

        bool IsSame(IMsbEntry e) => e != null && e.Name == entry.Name && e.GetType().Name == entry.TypeName;

        if (entry.Index >= 0 && entry.Index < list.Count && IsSame(list[entry.Index]))
            return list[entry.Index];

        return list.FirstOrDefault(IsSame);
    }
    #endregion
}
