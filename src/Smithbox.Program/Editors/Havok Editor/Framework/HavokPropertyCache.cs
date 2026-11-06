using System.Reflection;

namespace StudioCore.Editors.HavokEditor;

public class HavokPropertyCache
{
    public HavokPropertyCache() { }

    /// <summary>
    /// The game whose files are shown. An SDK's generated classes hold every game's fields (ER, NR and DB_NT share
    /// hk2018), so fields the game's registry doesn't give a class are left out. Null keeps every field.
    /// </summary>
    public Havok.HavokProfile Profile { get; set; }

    public readonly Dictionary<string, FieldInfo[]> FieldCache = new();

    public FieldInfo[] GetCachedHavokFields(Type type)
    {
        if (!FieldCache.TryGetValue(type.FullName, out FieldInfo[] fields))
        {
            fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public);
            fields = fields.OrderBy(f => f.MetadataToken).ToArray();

            if (Profile != null && type.Namespace?.StartsWith("Havok.hk", StringComparison.Ordinal) == true)
            {
                fields = fields.Where(f => Havok.Files.HavokFile.HasField(Profile, type, f.Name)).ToArray();
            }

            FieldCache.Add(type.FullName, fields);
        }

        return fields;
    }
}
