using StudioCore.Application;
using StudioCore.Editors.Common;
using System.Linq;

namespace StudioCore.Editors.MapEditor;

public class MapSelection
{
    public MapEditorView View;
    public ProjectEntry Project;

    public string SelectedMapID;
    public MapContainer SelectedMapContainer;

    public MapSelection(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public bool IsAnyMapLoaded()
    {
        var check = false;

        foreach (var entry in Project.Handler.MapData.PrimaryBank.Maps)
        {
            if (entry.Value.MapContainer != null)
            {
                check = true;
            }
        }

        return check;
    }

    // Map ID -> map entry, as the bank is keyed by file entry. Rebuilt when the bank's size changes.
    private Dictionary<string, KeyValuePair<FileDictionaryEntry, MapWrapper>> _mapsByID;
    private int _mapsByIDCount = -1;
    private Dictionary<FileDictionaryEntry, MapWrapper> _mapsByIDSource;

    private bool TryGetMapEntry(string mapID, out KeyValuePair<FileDictionaryEntry, MapWrapper> entry)
    {
        entry = default;

        if (mapID == null)
            return false;

        var maps = Project.Handler.MapData.PrimaryBank.Maps;

        if (_mapsByID == null || _mapsByIDCount != maps.Count || !ReferenceEquals(_mapsByIDSource, maps))
        {
            _mapsByIDSource = maps;
            _mapsByID = new Dictionary<string, KeyValuePair<FileDictionaryEntry, MapWrapper>>();

            foreach (var e in maps)
            {
                _mapsByID.TryAdd(e.Key.Filename, e);
            }

            _mapsByIDCount = maps.Count;
        }

        return _mapsByID.TryGetValue(mapID, out entry);
    }

    public MapContainer GetMapContainerFromMapID(string mapID)
    {
        if (TryGetMapEntry(mapID, out var targetMap) && targetMap.Value?.MapContainer != null)
        {
            return targetMap.Value.MapContainer;
        }

        return null;
    }

    /// <summary>
    /// The loaded container of a tool's target map, looked up by its map ID. Null when no map has been
    /// picked (the "None" placeholder) or the map has been unloaded since, in which case the container
    /// the tool holds is stale and must not be used.
    /// </summary>
    public MapContainer ResolveTargetMap((string MapID, ObjectContainer Container) target)
    {
        // "Active Map" follows the map the content tree shows
        if (MapTargetPicker.IsActiveMap(target))
        {
            return SelectedMapContainer is { LoadState: MapContentLoadState.Loaded } active ? active : null;
        }

        if (target.Container == null || string.IsNullOrEmpty(target.MapID))
            return null;

        return GetMapContainerFromMapID(target.MapID);
    }

    public FileDictionaryEntry GetFileEntryFromMapID(string mapID)
    {
        if (TryGetMapEntry(mapID, out var targetMap) && targetMap.Value != null)
        {
            return targetMap.Key;
        }

        return null;
    }
}
