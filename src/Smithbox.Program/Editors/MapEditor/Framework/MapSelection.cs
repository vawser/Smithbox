using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Editors.MapEditor.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor.Framework;

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
}
