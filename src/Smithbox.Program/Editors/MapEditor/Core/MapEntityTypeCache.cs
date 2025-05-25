using SoulsFormats.KF4;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Editors.MapEditor.Enums;
using StudioCore.Editors.MapEditor.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor.Core;

public class MapEntityTypeCache
{
    private MapEditorScreen Editor;

    public Dictionary<string, Dictionary<MsbEntityType, Dictionary<Type, List<MsbEntity>>>> _cachedTypeView;

    public MapEntityTypeCache(MapEditorScreen editor)
    {
        Editor = editor;
    }

    public void InvalidateCache()
    {
        _cachedTypeView = null;
    }

    public void RemoveMapFromCache(MapContentView curView)
    {
        var container = Editor.GetMapContainerFromMapID(curView.MapID);

        if (_cachedTypeView != null &&
            container == null && 
            _cachedTypeView.ContainsKey(curView.MapID))
        {
            _cachedTypeView.Remove(curView.MapID);
        }
    }

    public void AddMapToCache(MapContainer map)
    {
        if (_cachedTypeView == null || !_cachedTypeView.ContainsKey(map.Name))
        {
            RebuildCache(map);
        }
    }

    public void RebuildCache(MapContainer map)
    {
        if (_cachedTypeView == null)
        {
            _cachedTypeView =
                new Dictionary<string, Dictionary<MsbEntityType, Dictionary<Type, List<MsbEntity>>>>();
        }

        // Build the groupings from each top type
        Dictionary<MsbEntityType, Dictionary<Type, List<MsbEntity>>> mapcache = new();

        mapcache.Add(MsbEntityType.Part, new Dictionary<Type, List<MsbEntity>>());

        mapcache.Add(MsbEntityType.Region, new Dictionary<Type, List<MsbEntity>>());

        mapcache.Add(MsbEntityType.Event, new Dictionary<Type, List<MsbEntity>>());

        /*
        // Routes
        if (Smithbox.ProjectType is ProjectType.DS2 
            or ProjectType.DS2S
            or ProjectType.DS3
            or ProjectType.SDT
            or ProjectType.ER
            or ProjectType.AC6)
        {
            mapcache.Add(MsbEntityType.Routes, new Dictionary<Type, List<MsbEntity>>());
        }

        // Layers
        if (Smithbox.ProjectType is ProjectType.DS2
            or ProjectType.DS2S
            or ProjectType.DS3
            or ProjectType.AC6)
        {
            mapcache.Add(MsbEntityType.Layers, new Dictionary<Type, List<MsbEntity>>());
        }
        */

        if (Editor.Project.ProjectType is ProjectType.BB
            or ProjectType.DS3
            or ProjectType.SDT
            or ProjectType.ER
            or ProjectType.AC6)
        {
            mapcache.Add(MsbEntityType.Light, new Dictionary<Type, List<MsbEntity>>());
        }

        else if (Editor.Project.ProjectType is ProjectType.DS2S
            or ProjectType.DS2)
        {
            mapcache.Add(MsbEntityType.Light, new Dictionary<Type, List<MsbEntity>>());

            mapcache.Add(MsbEntityType.DS2Event, new Dictionary<Type, List<MsbEntity>>());

            mapcache.Add(MsbEntityType.DS2EventLocation, new Dictionary<Type, List<MsbEntity>>());

            mapcache.Add(MsbEntityType.DS2Generator, new Dictionary<Type, List<MsbEntity>>());

            mapcache.Add(MsbEntityType.DS2GeneratorRegist, new Dictionary<Type, List<MsbEntity>>());
        }

        // Fill the map cache
        foreach (Entity obj in map.Objects)
        {
            if (obj is MsbEntity e && mapcache.ContainsKey(e.Type))
            {
                Type typ = e.WrappedObject.GetType();
                if (!mapcache[e.Type].ContainsKey(typ))
                {
                    mapcache[e.Type].Add(typ, new List<MsbEntity>());
                }

                mapcache[e.Type][typ].Add(e);
            }
        }

        // Fill the type cache for this map
        if (!_cachedTypeView.ContainsKey(map.Name))
        {
            _cachedTypeView.Add(map.Name, mapcache);
        }
        else
        {
            _cachedTypeView[map.Name] = mapcache;
        }
    }
}
