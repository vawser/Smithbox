using StudioCore.Core;
using StudioCore.Editors.MapEditor.Framework;
using StudioCore.MsbEditor;
using StudioCore.Resource.Types;
using StudioCore.Scene.Framework;
using StudioCore.Scene.Helpers;
using StudioCore.Scene.Meshes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SoulsFormats.TAE3.Event;

namespace StudioCore.Editors.MapEditor;

public class MapEditorUtils
{
    /// <summary>
    /// Update the model state for all loaded entities.
    /// </summary>
    /// <param name="baseEditor"></param>
    /// <param name="projectEntry"></param>
    public static void UpdateAllEntityModels(Smithbox baseEditor, ProjectEntry projectEntry)
    {
        var ents = GetAllEntities(baseEditor, projectEntry);

        var mapEditor = projectEntry.MapEditor;

        foreach (var ent in ents)
        {
            if(ent is MsbEntity entity)
            {
                entity.UpdateEntityModel();
            }
        }
    }

    /// <summary>
    /// Get the list of entities from all maps that have been loaded.
    /// </summary>
    /// <param name="baseEditor"></param>
    /// <param name="projectEntry"></param>
    /// <returns></returns>
    public static List<Entity> GetAllEntities(Smithbox baseEditor, ProjectEntry projectEntry)
    {
        var entities = new List<Entity>();

        foreach (var entry in projectEntry.MapData.PrimaryBank.Maps)
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

    /// <summary>
    /// Get the list of entities from the currently selected map (that has been loaded).
    /// </summary>
    /// <param name="baseEditor"></param>
    /// <param name="projectEntry"></param>
    /// <returns></returns>
    public static List<Entity> GetCurrentMapEntities(Smithbox baseEditor, ProjectEntry projectEntry)
    {
        var entities = new List<Entity>();

        foreach (var entry in projectEntry.MapData.PrimaryBank.Maps)
        {
            var wrapper = entry.Value;

            if (wrapper.Name != projectEntry.MapEditor.Selection.SelectedMapID)
                continue;

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
