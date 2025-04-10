using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace StudioCore.Editors.HavokEditor.Framework;

public class HavokFieldCache
{
    public readonly Dictionary<string, FieldInfo[]> FieldCache = new();

    public HavokFieldCache()
    { }

    public FieldInfo[] GetCachedFields(Type type)
    {
        if (!FieldCache.TryGetValue(type.FullName, out FieldInfo[] fields))
        {
            fields = type.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
            fields = fields.OrderBy(p => p.MetadataToken).ToArray();
            FieldCache.Add(type.FullName, fields);
        }

        return fields;
    }
}
