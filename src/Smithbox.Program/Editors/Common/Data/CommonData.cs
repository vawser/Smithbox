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

        await SetupCharacterReloaderOffsets();

        return true;
    }

    public CharacterReloaderOffsets CharacterReloaderOffsets = new();

    /// <summary>
    /// Read the Character Reloader offsets, one file per game executable version.
    /// </summary>
    public async Task<bool> SetupCharacterReloaderOffsets()
    {
        await Task.Yield();

        CharacterReloaderOffsets = new();

        var srcDir = Path.Combine(AppContext.BaseDirectory, "Assets", "Character Reloader", ProjectUtils.GetGameDirectory(Project));

        if (!Directory.Exists(srcDir))
            return true;

        foreach (var filepath in Directory.EnumerateFiles(srcDir, "*.json"))
        {
            try
            {
                var filestring = await File.ReadAllTextAsync(filepath);

                var item = JsonSerializer.Deserialize(filestring, CommonJsonSerializerContext.Default.CharacterReloaderOffsetEntry);

                if (item != null && !CharacterReloaderOffsets.Groups.Any(e => e.exeVersion == item.exeVersion))
                {
                    CharacterReloaderOffsets.Groups.Add(item);
                }
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("COMMON_CharacterReloader_Error_Failed_Read_Offsets", filepath), e);
            }
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
        GC.SuppressFinalize(this);
    }
    #endregion
}