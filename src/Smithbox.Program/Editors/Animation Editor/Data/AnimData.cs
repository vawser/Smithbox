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

    public BehaviorBank PrimaryBehaviorBank;
    public AnimData(ProjectEntry project)
    {
        Project = project;
    }

    public async Task<bool> Setup()
    {
        await Task.Yield();

        PrimaryBehaviorBank = new("Primary", Project, Project.VFS.FS);

        // Primary Bank
        Task<bool> primaryBankTask = PrimaryBehaviorBank.Setup();
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
        PrimaryBehaviorBank?.Dispose();

        PrimaryBehaviorBank = null;
    }
    #endregion
}
