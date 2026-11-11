namespace StudioCore.Editors.EsdEditor;

/// <summary>
/// Holds the data for the ESD Editor: the game's catalogue and the project's ESD files.
/// Data Flow: the file list at setup, each file read on first use.
/// </summary>
public class EsdData : IDisposable
{
    public ProjectEntry Project;

    /// <summary>Names and arg types (null for a game the catalogue doesn't cover, e.g. DB_NT: raw IDs are shown)</summary>
    public EsdCatalog Catalog;

    public EsdBank PrimaryBank;

    public EsdData(ProjectEntry project)
    {
        Project = project;
    }

    public async Task<bool> Setup()
    {
        await Task.Yield();

        var name = EsdCatalog.FileName(Project.Descriptor.ProjectType);
        if (name != null)
        {
            var path = Path.Join(AppContext.BaseDirectory, "Assets", "ESD", "Catalogues", $"{name}.json");

            try
            {
                if (File.Exists(path))
                    Catalog = EsdCatalog.Load(path);
            }
            catch (Exception e)
            {
                // Names only: the editor still works with raw IDs
                Smithbox.LogError(this, LOC.Get("ESD_Data_Failed_Load_Catalogue", path), e);
            }
        }

        PrimaryBank = new EsdBank(Project, Project.VFS.FS);

        var result = PrimaryBank.Setup();

        if (result)
            Smithbox.Log(this, LOC.Get("ESD_Data_Setup_Primary_Bank_PASS", PrimaryBank.Containers.Count));
        else
            Smithbox.LogError(this, LOC.Get("ESD_Data_Setup_Primary_Bank_FAIL"));

        return result;
    }

    public void Dispose()
    {
        PrimaryBank = null;
        Catalog = null;
    }
}
