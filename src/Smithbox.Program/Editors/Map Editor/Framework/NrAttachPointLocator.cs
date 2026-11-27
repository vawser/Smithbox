using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// Finds the SmallBaseAttach region of a SmallBaseAndSpotAttachPoint row. The row names a small m60 tile (areaNo,
/// gridXNo, gridZNo, mapVariantNo), but the region is usually in the large tile that covers it: of 474 retail regions
/// found, 374 are in m60_{X/4}_{Z/4}_{V}2 and 100 in the small tile m60_X_Z_V0; 88 rows have none in any retail map
/// (`NrMsbSurvey attach`, Claude/Logs/nr_attach_survey.txt). So the candidates are searched in that order (plus the
/// medium tile), loaded maps as they are in the editor and the rest from the project's files, and the answer is kept.
/// </summary>
public static class NrAttachPointLocator
{
    public sealed record Location(string MapID, string RegionName);

    private sealed class Cache
    {
        public readonly Dictionary<uint, Location> Found = new();
        public readonly HashSet<uint> Missing = new();
    }

    private static readonly ConditionalWeakTable<ProjectEntry, Cache> Caches = new();

    /// <summary>
    /// The maps the region can be in, in search order: the small tile, the large tile covering it, the medium tile
    /// </summary>
    public static List<string> Candidates(int area, int x, int z, int variant) => new()
    {
        $"m{area:00}_{x:00}_{z:00}_{variant}0",
        $"m{area:00}_{x / 4:00}_{z / 4:00}_{variant}2",
        $"m{area:00}_{x / 2:00}_{z / 2:00}_{variant}1"
    };

    /// <summary>The entity ID of an MSB entry, wherever its format keeps it (<see cref="MsbEntityIds"/>), or 0</summary>
    public static uint GetEntityId(object entry) => MsbEntityIds.GetUInt(entry);

    /// <summary>The cached answer for an entity ID: true with the location, true with null (no region), or false</summary>
    public static bool TryGetCached(ProjectEntry project, uint entityId, out Location location)
    {
        var cache = Caches.GetValue(project, _ => new Cache());

        if (cache.Found.TryGetValue(entityId, out location))
            return true;

        location = null;
        return cache.Missing.Contains(entityId);
    }

    /// <summary>
    /// Searches the candidate maps for the region with the entity ID. Reads each candidate's MSB that isn't
    /// loaded, so call it on a click, not every frame. Null when no candidate has the region.
    /// </summary>
    public static Location Locate(ProjectEntry project, int area, int x, int z, int variant, uint entityId)
    {
        if (entityId == 0)
            return null;

        if (TryGetCached(project, entityId, out var cached))
            return cached;

        var cache = Caches.GetValue(project, _ => new Cache());
        var mapData = project.Handler.MapData;
        var adapter = MsbGameAdapter.For(project.Descriptor.ProjectType);

        if (mapData?.PrimaryBank == null)
            return null;

        foreach (var mapId in Candidates(area, x, z, variant))
        {
            var entry = mapData.PrimaryBank.Maps.FirstOrDefault(e => string.Equals(e.Key.Filename, mapId, StringComparison.OrdinalIgnoreCase));

            if (entry.Key == null)
                continue;

            string name = null;
            var container = entry.Value?.MapContainer;

            if (container is { LoadState: MapContentLoadState.Loaded })
            {
                name = container.Objects
                    .FirstOrDefault(e => e is MsbEntity { Type: MsbEntityType.Region } && GetEntityId(e.WrappedObject) == entityId)?.Name;
            }
            else
            {
                try
                {
                    var msb = MsbQueryRunner.ReadMsb(adapter, mapData.PrimaryBank.TargetFS, entry.Key);
                    name = msb?.Regions.GetEntries().FirstOrDefault(r => GetEntityId(r) == entityId)?.Name;
                }
                catch (Exception e)
                {
                    Smithbox.LogError(typeof(NrAttachPointLocator), $"Attach point search: {mapId} could not be read.", e);
                }
            }

            if (name != null)
            {
                var location = new Location(mapId, name);
                cache.Found[entityId] = location;
                return location;
            }
        }

        cache.Missing.Add(entityId);
        return null;
    }

    /// <summary>Forgets the answers, after maps were edited</summary>
    public static void Clear(ProjectEntry project) => Caches.Remove(project);
}
