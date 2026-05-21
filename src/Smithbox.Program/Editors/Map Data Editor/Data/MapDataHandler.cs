using StudioCore.Application;
using StudioCore.Editors.MaterialEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapDataEditor;

public class MapDataHandler : IDisposable
{
    public ProjectEntry Project;

    public MsbBank PrimaryBank_MSB;
    public EnflBank PrimaryBank_ENFL;

    public MapDataHandler(ProjectEntry project)
    {
        Project = project;
    }

    public async Task<bool> Setup()
    {
        await Task.Yield();

        PrimaryBank_MSB = new("Primary", Project, Project.VFS.FS);
        PrimaryBank_ENFL = new("Primary", Project, Project.VFS.FS);

        // Primary Bank (MSB)
        Task<bool> primaryBankMsbTask = PrimaryBank_MSB.Setup();
        bool primaryBankMsbTaskResult = await primaryBankMsbTask;

        if (!primaryBankMsbTaskResult)
        {
            Smithbox.LogError(this, $"[Map Data Editor] Failed to setup the Primary MSB Bank.");
        }
        else
        {
            Smithbox.Log(this, $"[Map Data Editor] Setup the Primary MSB Bank.");
        }

        // Primary Bank (ENFL)
        Task<bool> primaryBankEnflTask = PrimaryBank_ENFL.Setup();
        bool primaryBankEnflTaskResult = await primaryBankEnflTask;

        if (!primaryBankMsbTaskResult)
        {
            Smithbox.LogError(this, $"[Map Data Editor] Failed to setup the Primary ENFL Bank.");
        }
        else
        {
            Smithbox.Log(this, $"[Map Data Editor] Setup the Primary ENFL Bank.");
        }

        return true;
    }

    #region Dispose
    public void Dispose()
    {
        PrimaryBank_MSB?.Dispose();
        PrimaryBank_ENFL?.Dispose();

        PrimaryBank_MSB = null;
        PrimaryBank_ENFL = null;
    }
    #endregion
}
