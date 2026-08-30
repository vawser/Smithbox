namespace StudioCore.Application;

public class ModelMeta : Dictionary<string, Dictionary<string, ModelClass>>;

public class ModelClass
{
    public string Type { get; set; } = "";
    public List<ModelField> Fields { get; set; } = new();
}

public class ModelField
{
    public string Field { get; set; } = "";
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";

    // Attributes
    public string ParamRef { get; set; } = "";

    public bool DummyRef { get; set; } = false;
    public bool NodeRef { get; set; } = false;
    public bool MeshRef { get; set; } = false;
    public bool MaterialRef { get; set; } = false;

    public bool IsMeshData { get; set; } = false;
}

public static class ModelMetaHelper
{
    public static ModelClass GetMeta(ProjectEntry project, Type rootType, string type)
    {
        if (project.Handler.ModelData.ModelMeta.TryGetValue(type, out var metaSet))
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

    public static string GetFieldName(ModelClass classMeta, string internalName)
    {
        var name = internalName;

        var entry = classMeta.Fields.FirstOrDefault(f => f.Field == internalName);
        if (entry != null)
        {
            name = entry.Name;
        }

        return name;
    }

    public static string GetFieldDescription(ModelClass classMeta, string internalName)
    {
        var description = "";

        var entry = classMeta.Fields.FirstOrDefault(f => f.Field == internalName);
        if (entry != null)
        {
            description = entry.Description;
        }

        return description;
    }

    public static ModelField GetFieldMeta(ModelClass classMeta, string internalName)
    {
        if(classMeta == null)
            return null;

        var entry = classMeta.Fields.FirstOrDefault(f => f.Field == internalName);
        if (entry != null)
        {
            return entry;
        }

        return null;
    }

    public static bool GetMeshDataBool(ModelClass classMeta, string internalName)
    {
        var isMeshData = false;

        var entry = classMeta.Fields.FirstOrDefault(f => f.Field == internalName);
        if (entry != null)
        {
            return entry.IsMeshData;
        }

        return isMeshData;
    }
}