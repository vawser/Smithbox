using StudioCore.Editors.MetadataEditor;
using System.Diagnostics;
using System.Text.Json;

namespace StudioCore.Editors.Common;

public class CommonData : IDisposable
{
    public ProjectEntry Project;

    public HavokMeta HavokMeta = new();

    public CommonData(ProjectEntry project)
    {
        Project = project;
    }

    public async Task<bool> Setup()
    {
        await Task.Yield();

        // Havok Meta
        Task<bool> havokMetaTask = SetupHavokMeta();
        bool havokMetaTaskResult = await havokMetaTask;

        if (havokMetaTaskResult)
        {
            Smithbox.Log(this,
                LOC.Get("COMMON_Data_Setup_Havok_Meta_PASS", Project.Descriptor.ProjectName));
        }
        else
        {
            Smithbox.LogError(this,
                LOC.Get("COMMON_Data_Setup_Havok_Meta_FAIL", Project.Descriptor.ProjectName));
        }

        return true;
    }

    public async Task<bool> SetupHavokMeta()
    {
        await Task.Yield();

        HavokMeta = new();

        var baseDir = Path.Join(AppContext.BaseDirectory, "Assets", "HAVOK");

        foreach(var folder in Directory.EnumerateDirectories(baseDir))
        {
            // i.e. hk2018
            var folderName = Path.GetFileName(folder);

            var classes = new Dictionary<string, HavokClass>();

            foreach (var file in Directory.EnumerateFiles(folder))
            {
                string text = await File.ReadAllTextAsync(file);

                try
                {
                    var havokClassMeta = JsonSerializer.Deserialize(text, CommonJsonSerializerContext.Default.HavokClass);

                    // i.e. Aabb5BytesCodec
                    var key = havokClassMeta.Type;

                    classes.TryAdd(key, havokClassMeta);
                }
                catch (Exception e)
                {
                    Smithbox.LogError(this,
                        LOC.Get("COMMON_Data_Deserialize_Havok_Meta_FAIL", file), e);
                }
            }

            if(classes.Count > 0)
            {
                HavokMeta.TryAdd(folderName, classes);
            }
        }

        return true;
    }

    #region Dispose
    public void Dispose()
    {
        HavokMeta = null;
    }
    #endregion
}