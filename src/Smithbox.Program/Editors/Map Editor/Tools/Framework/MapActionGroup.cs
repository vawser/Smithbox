using StudioCore.Editors.Common;
using System.Collections.Generic;

namespace StudioCore.Editors.MapEditor;

public class MapActionGroup
{
    public string MapID { get; set; }

    public List<ViewportAction> Actions { get; set; }

    public MapActionGroup(string mapID, List<ViewportAction> actions)
    {
        MapID = mapID;
        Actions = actions;
    }
}

