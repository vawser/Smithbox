using Octokit;
using StudioCore.Application;
using StudioCore.Editors.MapEditor;
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

    public MsbMeta MsbMeta;

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
        bool primaryBankMsbTask = PrimaryBank_MSB.Setup();

        if (!primaryBankMsbTask)
        {
            Smithbox.LogError(this, $"[Map Data Editor] Failed to setup the Primary MSB Bank.");
        }
        else
        {
            Smithbox.Log(this, $"[Map Data Editor] Setup the Primary MSB Bank.");
        }

        // Primary Bank (ENFL)
        bool primaryBankEnflTask = PrimaryBank_ENFL.Setup();

        if (!primaryBankEnflTask)
        {
            Smithbox.LogError(this, $"[Map Data Editor] Failed to setup the Primary ENFL Bank.");
        }
        else
        {
            Smithbox.Log(this, $"[Map Data Editor] Setup the Primary ENFL Bank.");
        }

        // Msb META
        MsbMeta = new MsbMeta(Project);

        Task<bool> metaTask = MsbMeta.Setup();
        bool metaTaskResult = await metaTask;

        if (!metaTaskResult)
        {
            Smithbox.LogError(this, $"[Map Data Editor] Failed to setup the MSB meta.");
        }
        else
        {
            Smithbox.Log(this, $"[Map Data Editor] Setup the MSB meta.");
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
