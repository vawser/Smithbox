using System.Text.Json;

namespace StudioCore.Editors.ModelEditor;

public class ModelData : IDisposable
{
    public ProjectEntry Project;

    public ModelBank PrimaryBank;

    public ModelMeta ModelMeta;

    public ModelData(ProjectEntry project)
    {
        Project = project;
    }

    public async Task<bool> Setup()
    {
        await Task.Yield();

        PrimaryBank = new("Primary", Project, Project.VFS.FS);

        // Model Meta
        Task<bool> modelMetaTask = SetupModelMeta();
        bool modelMetaTaskResult = await modelMetaTask;

        if (modelMetaTaskResult)
        {
            Smithbox.Log(this,
                LOC.Get("MODEL_Data_Setup_Model_Meta_PASS", Project.Descriptor.ProjectName));
        }
        else
        {
            Smithbox.LogError(this,
                LOC.Get("MODEL_Data_Setup_Model_Meta_FAIL", Project.Descriptor.ProjectName));
        }

        // Primary Bank
        Task<bool> primaryBankTask = PrimaryBank.Setup();
        bool primaryBankTaskResult = await primaryBankTask;

        if (!primaryBankTaskResult)
        {
            Smithbox.LogError(this, LOC.Get("MODEL_Data_Setup_Primary_Bank_FAIL"));
        }
        else
        {
            Smithbox.Log(this, LOC.Get("MODEL_Data_Setup_Primary_Bank_PASS"));
        }

        return primaryBankTaskResult;
    }

    public async Task<bool> SetupModelMeta()
    {
        await Task.Yield();

        ModelMeta = new();

        var baseDir = Path.Join(AppContext.BaseDirectory, "Assets", "MODEL");

        if (Directory.Exists(baseDir))
        {
            foreach (var folder in Directory.EnumerateDirectories(baseDir))
            {
                // i.e. FLVER, CLM2, EDGE, etc
                var folderName = Path.GetFileName(folder);

                var classes = new Dictionary<string, ModelClass>();

                foreach (var file in Directory.EnumerateFiles(folder))
                {
                    string text = await File.ReadAllTextAsync(file);

                    try
                    {
                        var classMeta = JsonSerializer.Deserialize(text, ModelEditorJsonSerializerContext.Default.ModelClass);

                        var key = classMeta.Type;

                        classes.TryAdd(key, classMeta);
                    }
                    catch (Exception e)
                    {
                        Smithbox.LogError(this,
                            LOC.Get("MODEL_Data_Log_Failed_Model_Meta", file), e);
                    }
                }

                if (classes.Count > 0)
                {
                    ModelMeta.TryAdd(folderName, classes);
                }
            }
        }

        return true;
    }

    #region Dispose
    public void Dispose()
    {
        PrimaryBank?.Dispose();

        PrimaryBank = null;

        ModelMeta = null;
    }
    #endregion
}
