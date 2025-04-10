using StudioCore.Editors.MapEditor.Actions.Viewport;
using System.Collections.Generic;

namespace StudioCore.Editors.MapEditor.Framework.MassEdit;

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

