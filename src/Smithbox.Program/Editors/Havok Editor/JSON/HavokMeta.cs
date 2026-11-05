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

    // Tags: Clip Generator
    public bool ClipGeneratorFlags { get; set; } = false;
    public bool AnimationInternalID { get; set; } = false;

    // Tags: Behavior indices (resolved to names from the behavior graph string data)
    public bool EventID { get; set; } = false;
    public bool VariableIndex { get; set; } = false;
    public bool StateID { get; set; } = false;
}

public static class HavokMetaHelper
{
    public static HavokClass GetMeta(ProjectEntry project, Type rootType)
    {
        if (project.Descriptor.ProjectType is ProjectType.ER
            or ProjectType.AC6
            or ProjectType.NR
            or ProjectType.DB_NT
            or ProjectType.DB)
        {
            if (project.Handler.CommonData.HavokMeta.TryGetValue("hk2018", out var metaSet))
            {
                var fullName = rootType.FullName;

                // Handle hkBitFieldStorage fullname
                if (fullName.Contains("`"))
                {
                    fullName = fullName.Split("`")[0];
                }

                // The hk2019 classes are named like the hk2018 ones, so they share their descriptions.
                // Fields only in 2019 have none.
                if (fullName.StartsWith("HKLib.hk2019."))
                {
                    fullName = "HKLib.hk2018." + fullName["HKLib.hk2019.".Length..];
                }

                if (metaSet.TryGetValue(fullName, out var fieldMeta))
                {
                    return fieldMeta;
                }
            }
        }
        else if (project.Descriptor.ProjectType is ProjectType.DS3)
        {
            if (project.Handler.CommonData.HavokMeta.TryGetValue("hk2014", out var metaSet))
            {
                var fullName = rootType.FullName;

                // Handle hkBitFieldStorage fullname
                if (fullName.Contains("`"))
                {
                    fullName = fullName.Split("`")[0];
                }

                if (metaSet.TryGetValue(fullName, out var fieldMeta))
                {
                    return fieldMeta;
                }
            }
        }

        return null;
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