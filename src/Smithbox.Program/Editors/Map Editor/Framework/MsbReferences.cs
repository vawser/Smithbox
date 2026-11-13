using SoulsFormats;
using StudioCore.Editors.Common;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// One name held by an [MSBReference] field of a map object: the field itself, or an element of a string[] or
/// List&lt;string&gt; field. MSBs refer to other entries by name, resolved to indices when saved.
/// </summary>
public readonly record struct ReferenceSlot(Entity Referrer, PropertyInfo Property, int Index, string Value, Type ReferenceType)
{
    public string FieldName => Index < 0 ? Property.Name : $"{Property.Name}[{Index}]";

    public void Set(string value)
    {
        var target = Referrer.WrappedObject;

        if (Index < 0)
        {
            Property.SetValue(target, value);
        }
        else if (Property.GetValue(target) is IList list && Index < list.Count)
        {
            list[Index] = value;
        }

        Referrer.CachedAliasName = null;
    }

    /// <summary>
    /// Whether the field can refer to the entity. A part and a region may share a name, so the list matters,
    /// but not the subtype: SoulsFormats resolves most reference names against the whole list (MSB.FindIndex over
    /// entries.Parts or entries.Regions), whatever subtype the attribute names. Vanilla maps rely on it, as DS1
    /// objects whose CollisionName names a map piece, or DS3 WindAreaName naming other regions (SaveCheckSurvey).
    /// </summary>
    public bool CanReferTo(Entity entity)
    {
        return MsbNaming.CanReferTo(ReferenceType, entity?.WrappedObject);
    }

    /// <summary>
    /// The MSB list (parts, regions or events) the name is looked up in, or null if it isn't one of those (or the
    /// field names no type)
    /// </summary>
    public Type TargetList => MsbNaming.GetListOfType(ReferenceType);
}

/// <summary>
/// Finds the name references between the objects of a map
/// </summary>
public static class MsbReferences
{
    // Cache for types -> properties that are [MSBReference], with the type each refers to
    private static readonly ConcurrentDictionary<Type, (PropertyInfo Property, Type ReferenceType)[]> RefPropsCache = new();

    private static (PropertyInfo Property, Type ReferenceType)[] GetReferenceProperties(Type type)
    {
        return RefPropsCache.GetOrAdd(type, t =>
            t.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Select(p => (Property: p, Attribute: p.GetCustomAttribute<MSBReference>()))
                .Where(e => e.Attribute != null)
                .Select(e => (e.Property, e.Attribute.ReferenceType))
                .ToArray());
    }

    /// <summary>
    /// Every non-empty name held by the object's reference fields. Like CollectReferenceNames, only the
    /// object's own properties are read, not those of nested structs.
    /// </summary>
    public static IEnumerable<ReferenceSlot> GetSlots(Entity referrer)
    {
        if (referrer?.WrappedObject == null)
            yield break;

        foreach (var (prop, refType) in GetReferenceProperties(referrer.WrappedObject.GetType()))
        {
            var value = prop.GetValue(referrer.WrappedObject);

            if (value is string s)
            {
                if (!string.IsNullOrEmpty(s) && prop.CanWrite)
                {
                    yield return new ReferenceSlot(referrer, prop, -1, s, refType);
                }
            }
            else if (value is IList list)
            {
                for (var i = 0; i < list.Count; i++)
                {
                    if (list[i] is string element && !string.IsNullOrEmpty(element))
                    {
                        yield return new ReferenceSlot(referrer, prop, i, element, refType);
                    }
                }
            }
        }
    }

    /// <summary>
    /// The references held by objects of the container that point to one of the targets, by the targets' current names
    /// </summary>
    public static List<(ReferenceSlot Slot, Entity Target)> FindReferencesTo(ObjectContainer container, IEnumerable<Entity> targets)
    {
        var result = new List<(ReferenceSlot, Entity)>();

        if (container == null)
            return result;

        var byName = new Dictionary<string, List<Entity>>(StringComparer.Ordinal);

        foreach (var target in targets)
        {
            if (string.IsNullOrEmpty(target?.Name))
                continue;

            if (!byName.TryGetValue(target.Name, out var list))
            {
                list = new List<Entity>();
                byName[target.Name] = list;
            }

            list.Add(target);
        }

        if (byName.Count == 0)
            return result;

        foreach (var referrer in container.Objects)
        {
            foreach (var slot in GetSlots(referrer))
            {
                if (!byName.TryGetValue(slot.Value, out var candidates))
                    continue;

                var target = candidates.FirstOrDefault(slot.CanReferTo);

                if (target != null && target != referrer)
                {
                    result.Add((slot, target));
                }
            }
        }

        return result;
    }

    /// <summary>
    /// The references held by the referrers that point to objects of the container
    /// </summary>
    public static List<(ReferenceSlot Slot, Entity Target)> FindReferencesFrom(ObjectContainer container, IEnumerable<Entity> referrers)
    {
        var result = new List<(ReferenceSlot, Entity)>();

        if (container == null)
            return result;

        var byName = new Dictionary<string, List<Entity>>(StringComparer.Ordinal);

        foreach (var ent in container.Objects)
        {
            if (string.IsNullOrEmpty(ent.Name))
                continue;

            if (!byName.TryGetValue(ent.Name, out var list))
            {
                list = new List<Entity>();
                byName[ent.Name] = list;
            }

            list.Add(ent);
        }

        foreach (var referrer in referrers)
        {
            foreach (var slot in GetSlots(referrer))
            {
                if (byName.TryGetValue(slot.Value, out var candidates) &&
                    candidates.FirstOrDefault(slot.CanReferTo) is Entity target && target != referrer)
                {
                    result.Add((slot, target));
                }
            }
        }

        return result;
    }

    /// <summary>
    /// A short list of references for a dialog, e.g. "c1000_0000.EntityName -> m10_xx"
    /// </summary>
    public static string Describe(IEnumerable<(ReferenceSlot Slot, Entity Target)> references, int max = 10)
    {
        var list = references.ToList();

        var lines = list.Take(max).Select(r => $"{r.Slot.Referrer.Name}.{r.Slot.FieldName} → {r.Slot.Value}");
        var text = string.Join("\n", lines);

        if (list.Count > max)
        {
            text += $"\n… (+{list.Count - max})";
        }

        return text;
    }
}

/// <summary>
/// Clears reference fields (sets them to null, which an MSB writes as "no reference"), restoring them on undo
/// </summary>
public class ClearReferencesAction : ViewportAction
{
    private readonly List<ReferenceSlot> Slots;

    public ClearReferencesAction(IEnumerable<ReferenceSlot> slots)
    {
        Slots = slots.ToList();
    }

    public int Count => Slots.Count;

    public override ActionEvent Execute(bool isRedo = false)
    {
        foreach (var slot in Slots)
        {
            slot.Set(null);
            MapContainer.MarkEntityDirty(slot.Referrer);
        }

        RebuildReferenceMaps();

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        foreach (var slot in Slots)
        {
            slot.Set(slot.Value);
            MapContainer.MarkEntityDirty(slot.Referrer);
        }

        RebuildReferenceMaps();

        return ActionEvent.NoEvent;
    }

    private void RebuildReferenceMaps()
    {
        foreach (var container in Slots.Select(s => s.Referrer.Container).Where(c => c != null).Distinct())
        {
            Entity.BuildReferenceMaps(container.Objects);
        }
    }
}
