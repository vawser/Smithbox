using System.Reflection;

namespace StudioCore.Editors.HavokEditor;

public class HavokPropertyCache
{
    public HavokPropertyCache() { }

    /// <summary>
    /// The profile of the file being shown, as it was read (DeS 5.1 files in a DeS project read as DES_51, ER's 2016
    /// files as SDT). An SDK's generated classes hold every variant's fields (ER, NR and DB_NT share hk2018; DeS 5.5
    /// and 5.1 share hk55), so fields that profile's registry doesn't give a class are left out. Null keeps every field.
    /// </summary>
    public Func<Havok.HavokProfile> ProfileSource { get; set; }

    private readonly Dictionary<(Havok.HavokProfile Profile, Type Type), FieldInfo[]> FieldCache = new();

    public FieldInfo[] GetCachedHavokFields(Type type)
    {
        var profile = ProfileSource?.Invoke();

        if (!FieldCache.TryGetValue((profile, type), out FieldInfo[] fields))
        {
            fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public);
            fields = fields.OrderBy(f => f.MetadataToken).ToArray();

            if (profile != null && type.Namespace?.StartsWith("Havok.hk", StringComparison.Ordinal) == true)
            {
                fields = fields.Where(f => Havok.Files.HavokFile.HasField(profile, type, f.Name)).ToArray();
            }

            FieldCache.Add((profile, type), fields);
        }

        return fields;
    }
}
