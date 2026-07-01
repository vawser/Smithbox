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
    public TextureBank PreviewBank;

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

        PreviewBank = new("Preview", Project, Project.VFS.FS);

        Task<bool> previewBankTask = PreviewBank.Setup();
        bool previewBankTaskResult = await previewBankTask;

        if (!previewBankTaskResult)
        {
            Smithbox.LogError(this, LOC.Get("TEXVIEW_DataSetup_Load_Preview_Bank_FAIL"));
        }
        else
        {
            Smithbox.Log(this, LOC.Get("TEXVIEW_DataSetup_Load_Preview_Bank_PASS"));
        }

        return true;
    }

    #region Dispose
    public void Dispose()
    {
        PrimaryBank?.Dispose();
        PreviewBank?.Dispose();
    }
    #endregion
}

