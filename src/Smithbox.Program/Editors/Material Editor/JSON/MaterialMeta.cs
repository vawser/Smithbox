using System;
using System.Collections.Generic;
using System.Text;

namespace StudioCore.Application;

public class MaterialMeta : Dictionary<string, Dictionary<string, MaterialClass>>;

public class MaterialClass
{
    public string Type { get; set; } = "";
    public List<MaterialField> Fields { get; set; } = new();
}

public class MaterialField
{
    public string Field { get; set; } = "";
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";

    // Attributes
    public string ParamRef { get; set; } = "";
}

public static class MaterialMetaHelper
{
    public static MaterialClass GetMeta(ProjectEntry project, Type rootType, string implType)
    {
        if (project.Handler.MaterialData.MaterialMeta.TryGetValue(implType, out var metaSet))
        {
            var fullName = rootType.FullName;

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

    public static string GetFieldName(MaterialClass classMeta, string internalName)
    {
        var name = internalName;

        var entry = classMeta.Fields.FirstOrDefault(f => f.Field == internalName);
        if (entry != null)
        {
            name = entry.Name;
        }

        return name;
    }

    public static string GetFieldDescription(MaterialClass classMeta, string internalName)
    {
        var description = "";

        var entry = classMeta.Fields.FirstOrDefault(f => f.Field == internalName);
        if (entry != null)
        {
            description = entry.Description;
        }

        return description;
    }
}