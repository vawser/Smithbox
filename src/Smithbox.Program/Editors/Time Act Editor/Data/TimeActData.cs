using StudioCore.Editors.ModelEditor;
using System;
using System.Collections.Generic;
using System.Text;

namespace StudioCore.Editors.TimeActEditor;

public class TimeActData : IDisposable
{
    public ProjectEntry Project;

    public TimeActBank PrimaryBank;

    // TODO: time act meta

    public TimeActData(ProjectEntry project)
    {
        Project = project;
    }
    public async Task<bool> Setup()
    {
        await Task.Yield();

        PrimaryBank = new("Primary", Project, Project.VFS.FS);

        // Primary Bank
        Task<bool> primaryBankTask = PrimaryBank.Setup();
        bool primaryBankTaskResult = await primaryBankTask;

        if (!primaryBankTaskResult)
        {
            Smithbox.LogError(this, LOC.Get("TAE_Data_Setup_Primary_Bank_FAIL"));
        }
        else
        {
            Smithbox.Log(this, LOC.Get("TAE_Data_Setup_Primary_Bank_PASS"));
        }

        return primaryBankTaskResult;
    }

    #region Dispose
    public void Dispose()
    {
        PrimaryBank?.Dispose();

        PrimaryBank = null;
    }
    #endregion
}
