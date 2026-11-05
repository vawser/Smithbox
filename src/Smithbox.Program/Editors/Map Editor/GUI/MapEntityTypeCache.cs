using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;

namespace StudioCore.Editors.MapEditor;

public class MapEntityTypeCache
{
    public MapEditorView View;
    public ProjectEntry Project;

    public Dictionary<string, Dictionary<MsbEntityType, Dictionary<Type, List<MsbEntity>>>> _cachedTypeView;

    public MapEntityTypeCache(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    // The same groupings, in the order the content tree draws them (categories and types by name), so the
    // tree doesn't re-sort them every frame
    private Dictionary<string, List<(MsbEntityType Category, List<(Type Type, List<MsbEntity> Entities)> Types)>> _sortedTypeView;

    public void InvalidateCache()
    {
        _cachedTypeView = null;
        _sortedTypeView = null;
    }

    public void RemoveMapFromCache(MapContainer container)
    {
        if (_cachedTypeView != null &&
            container != null &&
            _cachedTypeView.ContainsKey(container.Name))
        {
            _cachedTypeView.Remove(container.Name);
        }

        if (container != null)
        {
            _sortedTypeView?.Remove(container.Name);
        }
    }

    /// <summary>
    /// The map's objects grouped by category then type, both sorted by name
    /// </summary>
    public List<(MsbEntityType Category, List<(Type Type, List<MsbEntity> Entities)> Types)> GetSortedTypeView(MapContainer map)
    {
        AddMapToCache(map);

        _sortedTypeView ??= new();

        if (!_sortedTypeView.TryGetValue(map.Name, out var sorted))
        {
            sorted = _cachedTypeView[map.Name]
                .OrderBy(q => q.Key.ToString())
                .Select(cat => (cat.Key, cat.Value
                    .OrderBy(t => t.Key.Name)
                    .Select(t => (t.Key, t.Value))
                    .ToList()))
                .ToList();

            _sortedTypeView[map.Name] = sorted;
        }

        return sorted;
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

        // Internal Types
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

        // Part Pose
        if (Project.Descriptor.ProjectType is ProjectType.DS2S
            or ProjectType.DS2 or ProjectType.DS3)
        {
            mapcache.Add(MsbEntityType.PartPose, new Dictionary<Type, List<MsbEntity>>());
        }

        // External: BTL
        if (Project.Descriptor.ProjectType is ProjectType.BB
            or ProjectType.DS3
            or ProjectType.SDT
            or ProjectType.ER
            or ProjectType.NR
            or ProjectType.AC6
            or ProjectType.DB_NT
            or ProjectType.DB)
        {
            mapcache.Add(MsbEntityType.Light, new Dictionary<Type, List<MsbEntity>>());
        }

        // External: AIP
        if (View.AutoInvadeBank.CanUse())
        {
            mapcache.Add(MsbEntityType.AutoInvadePoint, new Dictionary<Type, List<MsbEntity>>());
        }

        // External: MPW
        if (View.MultiPlayerWallBank.CanUse())
        {
            mapcache.Add(MsbEntityType.MultiPlayerWall, new Dictionary<Type, List<MsbEntity>>());
        }

        // External: FVB
        if (View.FogVolumeBank.CanUse())
        {
            mapcache.Add(MsbEntityType.FogVolume, new Dictionary<Type, List<MsbEntity>>());
        }

        // External: MSLD, listed by area with the edges under each
        if (View.MapLimitBank.CanUse())
        {
            mapcache.Add(MsbEntityType.MapLimitArea, new Dictionary<Type, List<MsbEntity>>());
        }

        // External: BTAB
        if (View.LightAtlasBank.CanUse())
        {
            mapcache.Add(MsbEntityType.LightAtlas, new Dictionary<Type, List<MsbEntity>>());
        }

        // External: BTPB
        if (View.LightProbeBank.CanUse())
        {
            mapcache.Add(MsbEntityType.LightProbeVolume, new Dictionary<Type, List<MsbEntity>>());
        }

        // External: ACB
        if (View.AssetConfigurationBank.CanUse())
        {
            mapcache.Add(MsbEntityType.AssetConfiguration, new Dictionary<Type, List<MsbEntity>>());
        }

        // External: NVA
        if (View.HavokNavmeshBank.CanUse())
        {
            mapcache.Add(MsbEntityType.Navmesh, new Dictionary<Type, List<MsbEntity>>());
        }

        // External: DS2 PARAM
        else if (Project.Descriptor.ProjectType is ProjectType.DS2S
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

        // The sorted view is derived from this one
        _sortedTypeView?.Remove(map.Name);

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
