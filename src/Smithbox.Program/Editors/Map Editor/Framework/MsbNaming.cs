using SoulsFormats;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// Unique names for map objects. An MSB resolves a name reference within one list (parts, regions or events),
/// so names only have to be unique within their list: a part and a region may share a name, as in vanilla maps.
/// </summary>
public static class MsbNaming
{
    private static readonly Regex TrailIDRegex = new(@"_(?<id>\d+)$");

    /// <summary>
    /// The list a name has to be unique in: parts, regions or events, or for anything else (lights, param rows)
    /// objects of the same type
    /// </summary>
    public static Type GetNameScope(object wrapped)
    {
        return wrapped switch
        {
            null => null,
            IMsbPart => typeof(IMsbPart),
            IMsbRegion => typeof(IMsbRegion),
            IMsbEvent => typeof(IMsbEvent),
            _ => wrapped.GetType()
        };
    }

    /// <summary>
    /// The MSB list (parts, regions or events) a type belongs to, or null for anything else. A reference attribute's
    /// type names a subtype, but SoulsFormats resolves the name against the whole list (MSB.FindIndex over
    /// entries.Parts, entries.Regions), and vanilla maps rely on that: DS1 objects whose CollisionName names a map
    /// piece, DS3 WindAreaName naming other regions (Claude/Tools/SaveCheckSurvey).
    /// </summary>
    public static Type GetListOfType(Type type)
    {
        if (type == null)
            return null;

        if (typeof(IMsbPart).IsAssignableFrom(type))
            return typeof(IMsbPart);

        if (typeof(IMsbRegion).IsAssignableFrom(type))
            return typeof(IMsbRegion);

        if (typeof(IMsbEvent).IsAssignableFrom(type))
            return typeof(IMsbEvent);

        return null;
    }

    /// <summary>
    /// Whether a reference field whose attribute names <paramref name="referenceType" /> can refer to
    /// <paramref name="target" />: same list (or the exact type, for anything that isn't in one)
    /// </summary>
    public static bool CanReferTo(Type referenceType, object target)
    {
        if (target == null)
            return false;

        if (referenceType == null)
            return true;

        var list = GetListOfType(referenceType);

        return list != null ? list.IsInstanceOfType(target) : referenceType.IsInstanceOfType(target);
    }

    private static bool InScope(Entity ent, Type scope)
    {
        return scope != null && ent?.WrappedObject != null && scope.IsInstanceOfType(ent.WrappedObject);
    }

    /// <summary>
    /// The names in use in the map's list that <paramref name="wrapped" /> belongs to, leaving out the given objects
    /// </summary>
    public static HashSet<string> GetNames(MapContainer map, object wrapped, IEnumerable<Entity> exclude = null)
    {
        var names = new HashSet<string>();

        if (map == null)
            return names;

        var scope = GetNameScope(wrapped);
        var excluded = exclude != null ? new HashSet<Entity>(exclude) : null;

        foreach (var ent in map.Objects)
        {
            if (excluded != null && excluded.Contains(ent))
                continue;

            if (InScope(ent, scope) && ent.Name != null)
            {
                names.Add(ent.Name);
            }
        }

        return names;
    }

    /// <summary>
    /// The objects of the map's list for <paramref name="ent" /> that already use <paramref name="name" />,
    /// other than the object itself
    /// </summary>
    public static List<Entity> FindClashes(MapContainer map, Entity ent, string name)
    {
        if (map == null || string.IsNullOrEmpty(name))
            return new List<Entity>();

        var scope = GetNameScope(ent.WrappedObject);

        return map.Objects.Where(o => o != ent && o.Name == name && InScope(o, scope)).ToList();
    }

    /// <summary>
    /// Renames the incoming entries (a prefab, say) whose names are already used in the target map's list, and
    /// rewrites the references between the entries to match, so the set keeps pointing at itself. References to
    /// objects outside the set are left as they are. Returns how many entries were renamed.
    /// </summary>
    public static int MakeUniqueWithReferences(IReadOnlyList<IMsbEntry> entries, MapContainer target)
    {
        // The names in use in each of the target map's lists
        var namesByScope = new Dictionary<Type, HashSet<string>>();

        HashSet<string> NamesFor(IMsbEntry entry)
        {
            var scope = GetNameScope(entry);

            if (!namesByScope.TryGetValue(scope, out var set))
            {
                set = GetNames(target, entry);
                namesByScope[scope] = set;
            }

            return set;
        }

        var renames = new Dictionary<IMsbEntry, string>();

        foreach (var entry in entries)
        {
            if (entry == null || string.IsNullOrEmpty(entry.Name))
                continue;

            var newName = MakeUnique(entry.Name, NamesFor(entry));

            if (newName != entry.Name)
            {
                renames[entry] = newName;
            }
        }

        if (renames.Count == 0)
            return 0;

        // References first, matched against the old names
        foreach (var entry in entries)
        {
            foreach (var (current, type, set) in MsbUtils.GetMsbReferences(entry))
            {
                if (string.IsNullOrEmpty(current))
                    continue;

                var renamed = renames.FirstOrDefault(r => MsbUtils.Matches(current, type, r.Key));

                if (renamed.Key != null)
                {
                    set(renamed.Value);
                }
            }
        }

        foreach (var (entry, newName) in renames)
        {
            entry.Name = newName;
        }

        return renames.Count;
    }

    /// <summary>
    /// <paramref name="name" /> made unique among <paramref name="names" />, which it is added to. A trailing
    /// number is counted up, keeping its width (c1000_9000 → c1000_9001); a name without one gets _0001.
    /// With <paramref name="alwaysNew" /> a new name is made even if the name itself is free, as for a copy.
    /// Empty names are left as they are.
    /// </summary>
    public static string MakeUnique(string name, ISet<string> names, bool alwaysNew = false)
    {
        if (string.IsNullOrEmpty(name))
            return name ?? "";

        if (!alwaysNew && !names.Contains(name))
        {
            names.Add(name);
            return name;
        }

        string result;
        Match idmatch = TrailIDRegex.Match(name);

        if (idmatch.Success)
        {
            var idstring = idmatch.Result("${id}");
            var baseName = name.Substring(0, name.Length - idstring.Length);

            // Too long to count up as an int, so treated as a name without a number
            if (idstring.Length <= 9 && int.TryParse(idstring, out var id))
            {
                var newid = idstring;

                while (names.Contains(baseName + newid))
                {
                    id++;
                    newid = id.ToString("D" + idstring.Length);
                }

                result = baseName + newid;
                names.Add(result);
                return result;
            }
        }

        var n = 1;
        result = $"{name}_{n:D4}";

        while (names.Contains(result))
        {
            n++;
            result = $"{name}_{n:D4}";
        }

        names.Add(result);
        return result;
    }
}
