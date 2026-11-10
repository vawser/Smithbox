using StudioCore.Application;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextureViewer;

public class TextureData : IDisposable
{
    public ProjectEntry Project;

    public TextureBank PrimaryBank;

    public TextureData(ProjectEntry project)
    {
        Project = project;
    }

    public async Task<bool> Setup()
    {
        await Task.Yield();

        PrimaryBank = new("Primary", Project, Project.VFS.FS);

        Task<bool> primaryChrBankTask = PrimaryBank.Setup();
        bool primaryChrBankTaskResult = await primaryChrBankTask;

        if (!primaryChrBankTaskResult)
        {
            Smithbox.LogError(this, LOC.Get("TEXVIEW_DataSetup_Load_Primary_Bank_FAIL"));
        }
        else
        {
            Smithbox.Log(this, LOC.Get("TEXVIEW_DataSetup_Load_Primary_Bank_PASS"));
        }

        // The Preview bank (a second copy of the same files) was never read by anything and parsed every layout binder
        // a second time, so it was removed: one bank owns the loaded containers and their edits (Texture Editor plan, T1)

        return true;
    }

    #region Dispose
    public void Dispose()
    {
        PrimaryBank?.Dispose();
    }
    #endregion
}

