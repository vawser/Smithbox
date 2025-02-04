using StudioCore.Editors.MapEditor.Actions.Viewport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

