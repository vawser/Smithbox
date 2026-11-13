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
