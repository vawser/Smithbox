namespace StudioCore.Editors.BehaviorEditor;

/// <summary>
/// Holds the data for the Behavior Editor: the project's AC6 behbnds and the enums their fields use.
/// Data Flow: Full Load (98 behbnds, small)
/// </summary>
public class BhvData : IDisposable
{
    public ProjectEntry Project;

    public BhvBank PrimaryBank;

    public BhvEnums Enums;

    public BhvData(ProjectEntry project)
    {
        Project = project;
    }

    public async Task<bool> Setup()
    {
        await Task.Yield();

        Enums = BhvEnums.Load(Project);

        PrimaryBank = new BhvBank(Project, Project.VFS.FS);

        var result = await PrimaryBank.Setup();

        if (result)
            Smithbox.Log(this, LOC.Get("BHV_Data_Setup_Primary_Bank_PASS", PrimaryBank.Binders.Count));
        else
            Smithbox.LogError(this, LOC.Get("BHV_Data_Setup_Primary_Bank_FAIL"));

        return result;
    }

    public void Dispose()
    {
        PrimaryBank = null;
        Enums = null;
    }
}
