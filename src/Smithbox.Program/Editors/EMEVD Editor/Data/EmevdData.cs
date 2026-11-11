namespace StudioCore.Editors.EmevdEditor;

/// <summary>
/// Holds the data for the EMEVD Editor: the game's instruction layouts and the project's event files.
/// Data Flow: the file list at setup, each file read on first use.
/// </summary>
public class EmevdData : IDisposable
{
    public ProjectEntry Project;

    public EmevdCatalog Catalog;

    public EmevdBank PrimaryBank;

    public EmevdData(ProjectEntry project)
    {
        Project = project;
    }

    public async Task<bool> Setup()
    {
        await Task.Yield();

        var layoutName = EmevdCatalog.LayoutName(Project.Descriptor.ProjectType);
        var path = Path.Join(AppContext.BaseDirectory, "Assets", "EMEVD", "Layouts", $"{layoutName}.json");

        try
        {
            Catalog = EmevdCatalog.Load(path);
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("EMEVD_Data_Failed_Load_Layouts", path), e);
            return false;
        }

        PrimaryBank = new EmevdBank(Project, Project.VFS.FS);

        var result = PrimaryBank.Setup();

        if (result)
            Smithbox.Log(this, LOC.Get("EMEVD_Data_Setup_Primary_Bank_PASS", PrimaryBank.Files.Count));
        else
            Smithbox.LogError(this, LOC.Get("EMEVD_Data_Setup_Primary_Bank_FAIL"));

        return result;
    }

    public void Dispose()
    {
        PrimaryBank = null;
        Catalog = null;
    }
}
