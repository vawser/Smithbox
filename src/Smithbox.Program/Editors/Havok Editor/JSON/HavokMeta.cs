using StudioCore.Editors.HavokEditor;

namespace StudioCore.Application;

public class HavokMeta : Dictionary<string, Dictionary<string, HavokClass>>;

public class HavokClass
{
    public string Type { get; set; } = "";

    public string NameField { get; set; } = "";
    public List<HavokField> Fields { get; set; } = new();

    // Tags:
    public bool SupportVariableBindings { get; set; } = false;

}

public class HavokField
{
    public string Field { get; set; } = "";
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";

    // Tags: General
    public bool IsRawData { get; set; } = false;
    public string ParamRef { get; set; } = "";

    /// <summary>
    /// The project types whose files have this field (ProjectType names, e.g. "NR"). Empty for all.
    /// For fields one game added to a class shared with another, like NR's CustomManualSelectorGenerator.m_isBasePoseAnim
    /// in the hk2018 classes ER also uses: elsewhere the field isn't saved, so it's hidden.
    /// </summary>
    public List<string> ProjectTypes { get; set; } = new();

    // Tags: Clip Generator
    public bool ClipGeneratorFlags { get; set; } = false;
    public bool AnimationInternalID { get; set; } = false;

    // Tags: Behavior indices (resolved to names from the behavior graph string data)
    public bool EventID { get; set; } = false;
    public bool VariableIndex { get; set; } = false;
    public bool StateID { get; set; } = false;

    // Tags: Animations (checked against the character's time acts and animation files)
    public bool AnimationName { get; set; } = false;
    public bool AnimationID { get; set; } = false;
}

public static class HavokMetaHelper
{
    /// <summary>
    /// The descriptions of a class, from the meta folders of the SDK its class set belongs to (see GetMetaFolders): hk2014 for
    /// Havok.hk2014 (DS3), hk2018 for the tagfile SDKs (2016, 2018 and 2019 classes are named alike, so they share
    /// them; fields only one SDK has have none). Null if there are none.
    /// </summary>
    public static HavokClass GetMeta(ProjectEntry project, Type rootType)
    {
        var key = NormalizeName(GetRelativeName(rootType));

        // The first folder that describes the class
        foreach (var folder in GetMetaFolders(rootType))
        {
            if (!project.Handler.CommonData.HavokMeta.TryGetValue(folder, out var metaSet))
                continue;

            if (MetaIndexes.GetValue(metaSet, BuildIndex).TryGetValue(key, out var meta))
                return meta;
        }

        return null;
    }

    /// <summary>
    /// The meta folders for a class set, in order. The classes of every SDK are named alike, so SDKs without a folder
    /// of their own share one: the tagfile SDKs (2015, 2016, 2019) hk2018's, the older packfile SDKs (hk2010, Havok
    /// 5.5) hk2014's, each falling back to the other.
    /// </summary>
    private static string[] GetMetaFolders(Type type)
    {
        return type.Namespace?.Split('.') switch
        {
            ["Havok", "hk2014", ..] => ["hk2014"],
            ["Havok", "hk2016" or "hk2018" or "hk2019", ..] => ["hk2018"],
            ["Havok", "hk2015", ..] => ["hk2018", "hk2014"],
            ["Havok", "hk2010" or "hk55", ..] => ["hk2014", "hk2018"],
            _ => []
        };
    }

    /// <summary>
    /// The class's name within its class set: Havok.hk2018.hkaiNavMesh+Edge is hkaiNavMesh+Edge, and generic
    /// classes (hkBitFieldStorage`1) lose their arity
    /// </summary>
    private static string GetRelativeName(Type type)
    {
        var fullName = type.FullName ?? type.Name;

        if (fullName.Contains('`'))
            fullName = fullName.Split('`')[0];

        foreach (var prefix in new[] { "Havok.hk55.", "Havok.hk2010.", "Havok.hk2014.", "Havok.hk2015.", "Havok.hk2016.", "Havok.hk2018.", "Havok.hk2019." })
        {
            if (fullName.StartsWith(prefix, StringComparison.Ordinal))
                return fullName[prefix.Length..];
        }

        return fullName;
    }

    /// <summary>
    /// A meta key without the library it was written for (HKLib.hk2018., HKX2.) and without separators, so a nested
    /// class matches however the meta spells it: hkbStateMachine+TimeInterval, hkbStateMachine.TimeInterval and
    /// HKX2's flat hkbStateMachineTimeInterval are the same key
    /// </summary>
    private static string NormalizeName(string name)
    {
        foreach (var prefix in new[] { "HKLib.hk2018.", "HKLib.hk2019.", "HKX2." })
        {
            if (name.StartsWith(prefix, StringComparison.Ordinal))
            {
                name = name[prefix.Length..];
                break;
            }
        }

        return name.Replace("+", "").Replace(".", "");
    }

    private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<Dictionary<string, HavokClass>, Dictionary<string, HavokClass>> MetaIndexes = new();

    private static Dictionary<string, HavokClass> BuildIndex(Dictionary<string, HavokClass> metaSet)
    {
        var index = new Dictionary<string, HavokClass>();

        foreach (var (key, meta) in metaSet)
        {
            index.TryAdd(NormalizeName(key), meta);
        }

        return index;
    }

    public static string GetFieldName(HavokClass havokMeta, string internalName)
    {
        var name = internalName;
        
        var entry = havokMeta.Fields.FirstOrDefault(f => f.Field == internalName);
        if(entry != null)
        {
            name = entry.Name;
        }

        return name;
    }

    public static string GetFieldDescription(HavokClass havokMeta, string internalName)
    {
        var description = "";

        var entry = havokMeta.Fields.FirstOrDefault(f => f.Field == internalName);
        if (entry != null)
        {
            description = entry.Description;
        }

        return description;
    }

    /// <summary>
    /// What the field indexes into, from its meta tags
    /// </summary>
    public static HavokIndexRole GetIndexRole(HavokClass havokMeta, string internalName)
    {
        if (havokMeta == null)
            return HavokIndexRole.None;

        var entry = havokMeta.Fields.FirstOrDefault(f => f.Field == internalName);

        if (entry == null)
            return HavokIndexRole.None;

        if (entry.EventID)
            return HavokIndexRole.Event;

        if (entry.VariableIndex)
            return HavokIndexRole.Variable;

        if (entry.StateID)
            return HavokIndexRole.State;

        return HavokIndexRole.None;
    }

    /// <summary>
    /// Whether the field is in this project's files (see <see cref="HavokField.ProjectTypes"/>)
    /// </summary>
    public static bool IsFieldInProject(HavokClass havokMeta, string internalName, ProjectEntry project)
    {
        var entry = havokMeta?.Fields.FirstOrDefault(f => f.Field == internalName);

        if (entry == null || entry.ProjectTypes.Count == 0)
            return true;

        var projectType = project.Descriptor.ProjectType.ToString();

        return entry.ProjectTypes.Any(t => string.Equals(t, projectType, StringComparison.OrdinalIgnoreCase));
    }

    public static bool IsRawData(HavokClass havokMeta, string internalName)
    {
        var entry = havokMeta.Fields.FirstOrDefault(f => f.Field == internalName);
        if (entry != null)
        {
            return entry.IsRawData;
        }

        return false;
    }
}