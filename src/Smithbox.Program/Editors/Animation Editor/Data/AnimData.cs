using StudioCore.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.AnimEditor;

public class AnimData : IDisposable
{
    public ProjectEntry Project;

    public BehaviorBank BehaviorBank;

    public AnimData(ProjectEntry project)
    {
        Project = project;
    }

    public async Task<bool> Setup()
    {
        await Task.Yield();

        BehaviorBank = new("Primary", Project, Project.VFS.FS);

        // Primary Bank
        Task<bool> primaryBankTask = BehaviorBank.Setup();
        bool primaryBankTaskResult = await primaryBankTask;

        if (!primaryBankTaskResult)
        {
            Smithbox.LogError(this, $"[Animation Editor] Failed to fully setup Primary Behavior Bank.");
        }

        return primaryBankTaskResult;
    }

    #region Dispose
    public void Dispose()
    {
        BehaviorBank?.Dispose();

        BehaviorBank = null;
    }
    #endregion
}
