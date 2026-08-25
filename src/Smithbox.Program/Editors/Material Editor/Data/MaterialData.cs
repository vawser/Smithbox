using System.Text.Json;

namespace StudioCore.Editors.MaterialEditor;

/// <summary>
/// Holds the data banks for Materials.
/// Data Flow: Full Load
/// </summary>
public class MaterialData : IDisposable
{
    public ProjectEntry Project;

    public MaterialBank PrimaryBank;

    public MaterialMeta MaterialMeta = new();

    public MaterialData(ProjectEntry project)
    {
        Project = project;
    }

    public async Task<bool> Setup()
    {
        await Task.Yield();

        // Material Meta
        Task<bool> materialMetaTask = SetupMaterialMeta();
        bool materialMetaTaskResult = await materialMetaTask;

        if (materialMetaTaskResult)
        {
            Smithbox.Log(this,
                LOC.Get("MAT_Data_Setup_Material_Meta_PASS", Project.Descriptor.ProjectName));
        }
        else
        {
            Smithbox.LogError(this,
                LOC.Get("MAT_Data_Setup_Material_Meta_FAIL", Project.Descriptor.ProjectName));
        }

        PrimaryBank = new("Primary", Project, Project.VFS.FS);
        //VanillaBank = new("Vanilla", Project, Project.VFS.VanillaFS);

        // Primary Bank
        Task<bool> primaryBankTask = PrimaryBank.Setup();
        bool primaryBankTaskResult = await primaryBankTask;

        if (!primaryBankTaskResult)
        {
            Smithbox.LogError(this, LOC.Get("MAT_Data_Setup_Primary_Bank_FAIL"));
        }
        else
        {
            Smithbox.Log(this, LOC.Get("MAT_Data_Setup_Primary_Bank_PASS"));
        }

        return true;
    }

    public async Task<bool> SetupMaterialMeta()
    {
        await Task.Yield();

        MaterialMeta = new();

        var baseDir = Path.Join(AppContext.BaseDirectory, "Assets", "MATERIAL");

        foreach (var folder in Directory.EnumerateDirectories(baseDir))
        {
            // i.e. MTD / MATBIN
            var folderName = Path.GetFileName(folder);

            var classes = new Dictionary<string, MaterialClass>();

            foreach (var file in Directory.EnumerateFiles(folder))
            {
                string text = await File.ReadAllTextAsync(file);

                try
                {
                    var materialClass = JsonSerializer.Deserialize(text, MaterialEditorJsonSerializerContext.Default.MaterialClass);

                    var key = materialClass.Type;

                    classes.TryAdd(key, materialClass);
                }
                catch (Exception e)
                {
                    Smithbox.LogError(this,
                        LOC.Get("MAT_Data_Deserialize_Material_Meta_FAIL", file), e);
                }
            }

            if (classes.Count > 0)
            {
                MaterialMeta.TryAdd(folderName, classes);
            }
        }

        return true;
    }

    #region Dispose
    public void Dispose()
    {
        PrimaryBank?.Dispose();
        //VanillaBank?.Dispose();

        PrimaryBank = null;
        //VanillaBank = null;

        MaterialMeta = null;
    }
    #endregion
}
