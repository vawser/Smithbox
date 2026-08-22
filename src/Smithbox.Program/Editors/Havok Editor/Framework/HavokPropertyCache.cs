using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace StudioCore.Editors.HavokEditor;

public class HavokPropertyCache
{
    public HavokPropertyCache() { }

    public readonly Dictionary<string, FieldInfo[]> FieldCache = new();

    public FieldInfo[] GetCachedHavokFields(Type type)
    {
        if (!FieldCache.TryGetValue(type.FullName, out FieldInfo[] fields))
        {
            fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public);
            fields = fields.OrderBy(f => f.MetadataToken).ToArray();
            FieldCache.Add(type.FullName, fields);
        }

        return fields;
    }
}
