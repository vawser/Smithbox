namespace StudioCore.Editors.MapEditor;

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
