using StudioCore.Renderer;
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
        PrimaryBank.ShaderTable = await LoadShaderTable();
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

    /// <summary>
    /// Loads what is known about this game's shaders. Kept apart from Assets/MATERIAL, where every
    /// file is read as a MaterialClass. Games without a table are fine, their shaders are just unknown.
    /// </summary>
    private async Task<MaterialShaderTable> LoadShaderTable()
    {
        var folder = Path.Join(AppContext.BaseDirectory, "Assets", "Material Definitions");
        var game = Project.Descriptor.ProjectType;

        var path = Path.Join(folder, $"{game}.json");
        var groupsPath = Path.Join(folder, $"{game}_TextureGroups.json");
        var inputsPath = Path.Join(folder, $"{game}_TextureInputs.json");
        var blendsPath = Path.Join(folder, $"{game}_Blends.json");
        var detailsPath = Path.Join(folder, $"{game}_Details.json");
        var magmaPath = Path.Join(folder, $"{game}_Magma.json");
        var waterPath = Path.Join(folder, $"{game}_Water.json");
        var furPath = Path.Join(folder, $"{game}_Fur.json");
        var metallicPath = Path.Join(folder, $"{game}_Metallic.json");
        var uvSetsPath = Path.Join(folder, $"{game}_UvSets.json");
        var overlaysPath = Path.Join(folder, $"{game}_Overlays.json");
        var detailStacksPath = Path.Join(folder, $"{game}_DetailStacks.json");
        var colorAdjustPath = Path.Join(folder, $"{game}_ColorAdjust.json");
        var emissiveMasksPath = Path.Join(folder, $"{game}_EmissiveMasks.json");
        var generatedBlendsPath = Path.Join(folder, $"{game}_GeneratedBlends.json");

        MaterialShaderTable table = new();

        try
        {
            if (File.Exists(path))
            {
                string text = await File.ReadAllTextAsync(path);

                table = JsonSerializer.Deserialize(text, MaterialEditorJsonSerializerContext.Default.MaterialShaderTable)
                    ?? table;
            }

            if (File.Exists(groupsPath))
            {
                string text = await File.ReadAllTextAsync(groupsPath);

                table.TextureGroups = JsonSerializer.Deserialize(text,
                    MaterialEditorJsonSerializerContext.Default.DictionaryStringDictionaryInt32String) ?? new();
            }

            if (File.Exists(inputsPath))
            {
                string text = await File.ReadAllTextAsync(inputsPath);

                table.TextureInputs = JsonSerializer.Deserialize(text,
                    MaterialEditorJsonSerializerContext.Default.DictionaryStringDictionaryInt32MaterialTextureInput) ?? new();
            }

            if (File.Exists(blendsPath))
            {
                string text = await File.ReadAllTextAsync(blendsPath);

                table.Blends = JsonSerializer.Deserialize(text,
                    MaterialEditorJsonSerializerContext.Default.DictionaryStringMaterialBlendRecipe) ?? new();
            }

            if (File.Exists(detailsPath))
            {
                string text = await File.ReadAllTextAsync(detailsPath);

                table.Details = JsonSerializer.Deserialize(text,
                    MaterialEditorJsonSerializerContext.Default.DictionaryStringMaterialDetailRecipe) ?? new();
            }

            if (File.Exists(magmaPath))
            {
                string text = await File.ReadAllTextAsync(magmaPath);

                table.Magma = JsonSerializer.Deserialize(text,
                    MaterialEditorJsonSerializerContext.Default.DictionaryStringMaterialMagmaRecipe) ?? new();
            }

            if (File.Exists(waterPath))
            {
                string text = await File.ReadAllTextAsync(waterPath);

                table.Water = JsonSerializer.Deserialize(text,
                    MaterialEditorJsonSerializerContext.Default.DictionaryStringMaterialWaterRecipe) ?? new();
            }

            if (File.Exists(furPath))
            {
                string text = await File.ReadAllTextAsync(furPath);

                table.Fur = JsonSerializer.Deserialize(text,
                    MaterialEditorJsonSerializerContext.Default.DictionaryStringMaterialFurRecipe) ?? new();
            }

            if (File.Exists(metallicPath))
            {
                string text = await File.ReadAllTextAsync(metallicPath);

                table.Metallic = JsonSerializer.Deserialize(text,
                    MaterialEditorJsonSerializerContext.Default.DictionaryStringDictionaryInt32String) ?? new();
            }

            if (File.Exists(generatedBlendsPath))
            {
                string text = await File.ReadAllTextAsync(generatedBlendsPath);

                table.MergeBlends(JsonSerializer.Deserialize(text,
                    MaterialEditorJsonSerializerContext.Default.DictionaryStringMaterialBlendRecipe) ?? new());
            }

            if (File.Exists(overlaysPath))
            {
                string text = await File.ReadAllTextAsync(overlaysPath);

                table.MergeOverlays(JsonSerializer.Deserialize(text,
                    MaterialEditorJsonSerializerContext.Default.DictionaryStringMaterialDetailRecipe) ?? new());
            }

            // After the generated blends, so the generated recipes get it too
            if (File.Exists(colorAdjustPath))
            {
                string text = await File.ReadAllTextAsync(colorAdjustPath);

                table.MergeColorAdjust(JsonSerializer.Deserialize(text,
                    MaterialEditorJsonSerializerContext.Default.DictionaryStringMaterialColorAdjustRecipe) ?? new());
            }

            if (File.Exists(emissiveMasksPath))
            {
                string text = await File.ReadAllTextAsync(emissiveMasksPath);

                table.EmissiveMasks = JsonSerializer.Deserialize(text,
                    MaterialEditorJsonSerializerContext.Default.DictionaryStringMaterialEmissiveMask) ?? new();
            }

            if (File.Exists(detailStacksPath))
            {
                string text = await File.ReadAllTextAsync(detailStacksPath);

                table.MergeDetailStacks(JsonSerializer.Deserialize(text,
                    MaterialEditorJsonSerializerContext.Default.DictionaryStringMaterialDetailRecipe) ?? new());
            }

            if (File.Exists(uvSetsPath))
            {
                string text = await File.ReadAllTextAsync(uvSetsPath);

                table.UvSets = JsonSerializer.Deserialize(text,
                    MaterialEditorJsonSerializerContext.Default.DictionaryStringDictionaryInt32String) ?? new();
            }
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("MAT_Data_Deserialize_Shader_Table_FAIL", path), e);
        }

        return table;
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
