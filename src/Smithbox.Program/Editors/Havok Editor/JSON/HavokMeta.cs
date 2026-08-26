namespace StudioCore.Application;

public class HavokMeta : Dictionary<string, Dictionary<string, HavokClass>>;

public class HavokClass
{
    public string Type { get; set; } = "";
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
}

public static class HavokMetaHelper
{
    public static HavokClass GetMeta(ProjectEntry project, Type rootType)
    {
        if(project.Handler.CommonData.HavokMeta.TryGetValue("hk2018", out var metaSet))
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