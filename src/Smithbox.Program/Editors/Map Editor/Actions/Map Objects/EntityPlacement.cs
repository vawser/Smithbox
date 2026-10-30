using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// Where an entity sits in a map: its index in the map's object list (which decides MSB order on save)
/// and its index among its parent's children (which decides the tree and transform hierarchy).
/// The two indices are independent, so actions that take entities out of a map capture both to put them back.
/// </summary>
public readonly record struct EntityPlacement(MapContainer Map, int MapIndex, Entity Parent, int ParentIndex)
{
    public static EntityPlacement Capture(MapContainer map, Entity ent)
    {
        var mapIndex = map != null ? map.Objects.IndexOf(ent) : -1;
        var parentIndex = ent.Parent != null ? ent.Parent.ChildIndex(ent) : -1;

        return new EntityPlacement(map, mapIndex, ent.Parent, parentIndex);
    }

    /// <summary>
    /// Takes the entity out of the map's object list and its parent's children
    /// </summary>
    public static void Detach(MapContainer map, Entity ent)
    {
        map?.Objects.Remove(ent);
        ent.Parent?.RemoveChild(ent);
    }

    /// <summary>
    /// Puts entities back where they were captured. Map and parent insertions are each done in ascending
    /// index order, so every entity lands at its original index even when several shared a list.
    /// Indices past the end of a list (because it shrank since capture) are clamped.
    /// </summary>
    public static void RestoreAll(IEnumerable<(Entity Entity, EntityPlacement Placement)> entries)
    {
        var list = entries.ToList();

        foreach (var (ent, p) in list.Where(e => e.Placement.Map != null && e.Placement.MapIndex >= 0)
                     .OrderBy(e => e.Placement.MapIndex))
        {
            p.Map.Objects.Insert(Math.Min(p.MapIndex, p.Map.Objects.Count), ent);
        }

        foreach (var (ent, p) in list.Where(e => e.Placement.Parent != null)
                     .OrderBy(e => e.Placement.ParentIndex))
        {
            if (p.ParentIndex >= 0)
            {
                p.Parent.AddChild(ent, Math.Min(p.ParentIndex, p.Parent.Children.Count));
            }
            else
            {
                p.Parent.AddChild(ent);
            }
        }
    }
}
