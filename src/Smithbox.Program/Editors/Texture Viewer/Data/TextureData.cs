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
            Smithbox.LogError(this, $"[Texture Viewer] Failed to setup Primary Texture bank.");
        }

        PreviewBank = new("Preview", Project, Project.VFS.FS);

        Task<bool> previewBankTask = PreviewBank.Setup();
        bool previewBankTaskResult = await previewBankTask;

        if (!previewBankTaskResult)
        {
            Smithbox.LogError(this, $"[Texture Viewer] Failed to setup Preview Texture bank.");
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

