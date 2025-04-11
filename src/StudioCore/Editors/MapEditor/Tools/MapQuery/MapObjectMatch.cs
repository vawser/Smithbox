using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor.Tools.MapQuery;

public class MapObjectMatch
{
    public string MapName = "";
    public string EntityName = "";

    public string PropertyName = "";
    public string PropertyValue = "";

    public object MapObject = null;
    public MapObjectMatch(object obj, string mapname, string entityName, string propName, string propValue)
    {
        MapObject = obj;
        MapName = mapname;
        EntityName = entityName;
        PropertyName = propName;
        PropertyValue = propValue;
    }
}
