using StudioCore.Application;
using StudioCore.Editors.Common;
using System.Collections.Generic;

namespace StudioCore.Editors.MapEditor;

public static class MapEditorUtils
{
    /// <summary>
    /// Get the list of entities from all maps that have been loaded.
    /// </summary>
    /// <param name="baseEditor"></param>
    /// <param name="projectEntry"></param>
    /// <returns></returns>
    public static List<Entity> GetAllEntities(ProjectEntry projectEntry)
    {
        var entities = new List<Entity>();

        foreach (var entry in projectEntry.Handler.MapData.PrimaryBank.Maps)
        {
            var wrapper = entry.Value;

            if (wrapper.MapContainer == null)
                continue;

            foreach (var ent in wrapper.MapContainer.Objects)
            {
                entities.Add(ent);
            }
        }

        return entities;
    }

}
