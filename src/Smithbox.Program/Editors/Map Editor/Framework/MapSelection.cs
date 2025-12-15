using StudioCore.Application;
using System.Linq;

namespace StudioCore.Editors.MapEditor;

public class MapSelection
{
    public MapEditorScreen Editor;
    public ProjectEntry Project;

    public string SelectedMapID;
    public MapContainer SelectedMapContainer;

    public MapSelection(MapEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public bool IsAnyMapLoaded()
    {
        var check = false;

        foreach (var entry in Project.MapData.PrimaryBank.Maps)
        {
            if (entry.Value.MapContainer != null)
            {
                check = true;
            }
        }

        return check;
    }

    public MapContainer GetMapContainerFromMapID(string mapID)
    {
        var targetMap = Project.MapData.PrimaryBank.Maps.FirstOrDefault(e => e.Key.Filename == mapID);

        if (targetMap.Value != null && targetMap.Value.MapContainer != null)
        {
            return targetMap.Value.MapContainer;
        }

        return null;
    }

    public FileDictionaryEntry GetFileEntryFromMapID(string mapID)
    {
        var targetMap = Project.MapData.PrimaryBank.Maps.FirstOrDefault(e => e.Key.Filename == mapID);

        if (targetMap.Value != null)
        {
            return targetMap.Key;
        }

        return null;
    }
}
