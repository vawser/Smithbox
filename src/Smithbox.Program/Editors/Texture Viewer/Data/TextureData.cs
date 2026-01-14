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

    public FileDictionary TextureFiles = new();

    public FileDictionary TexturePackedFiles = new();

    public FileDictionary ShoeboxFiles = new();

    public TextureData(ProjectEntry project)
    {
        Project = project;
    }

    public async Task<bool> Setup()
    {
        await Task.Yield();

        SetupTextureDictionaries();
        SetupPackedTextureDictionaries();
        SetupShoeboxDictionaries();

        PrimaryBank = new("Primary", Project, Project.VFS.FS);

        Task<bool> primaryChrBankTask = PrimaryBank.Setup();
        bool primaryChrBankTaskResult = await primaryChrBankTask;

        if (!primaryChrBankTaskResult)
        {
            TaskLogs.AddError($"[Texture Viewer] Failed to setup Primary Texture bank.");
        }

        PreviewBank = new("Preview", Project, Project.VFS.FS);

        Task<bool> previewBankTask = PreviewBank.Setup();
        bool previewBankTaskResult = await previewBankTask;

        if (!previewBankTaskResult)
        {
            TaskLogs.AddError($"[Texture Viewer] Failed to setup Preview Texture bank.");
        }

        return true;
    }

    public void SetupTextureDictionaries()
    {
        var secondaryDicts = new List<FileDictionary>();

        // TPF
        var baseDict = new FileDictionary();
        baseDict.Entries = Project.Locator.FileDictionary.Entries
            .Where(e => e.Archive != "sd")
            .Where(e => e.Extension == "tpf")
            .ToList();

        // Object Textures
        var objDict = new FileDictionary();
        objDict.Entries = Project.Locator.FileDictionary.Entries
            .Where(e => e.Archive != "sd")
            .Where(e => e.Extension == "objbnd")
            .ToList();

        if (Project.Descriptor.ProjectType is ProjectType.DS2S or ProjectType.DS2)
        {
            objDict.Entries = Project.Locator.FileDictionary.Entries
                .Where(e => e.Archive != "sd")
                .Where(e => e.Extension == "bnd" && e.Folder == "/model/obj")
                .ToList();
        }

        secondaryDicts.Add(objDict);

        // Chr Textures
        var chrDict = new FileDictionary();
        chrDict.Entries = Project.Locator.FileDictionary.Entries
            .Where(e => e.Archive != "sd")
            .Where(e => e.Extension == "texbnd")
            .ToList();

        secondaryDicts.Add(chrDict);

        // Part Textures
        var partDict = new FileDictionary();
        partDict.Entries = Project.Locator.FileDictionary.Entries
            .Where(e => e.Archive != "sd")
            .Where(e => e.Extension == "partsbnd")
            .ToList();

        if (Project.Descriptor.ProjectType is ProjectType.DS2S or ProjectType.DS2)
        {
            var commonPartDict = new FileDictionary();
            commonPartDict.Entries = Project.Locator.FileDictionary.Entries
                .Where(e => e.Archive != "sd")
                .Where(e => e.Extension == "commonbnd")
                .ToList();

            secondaryDicts.Add(commonPartDict);

            partDict.Entries = Project.Locator.FileDictionary.Entries
                .Where(e => e.Archive != "sd")
                .Where(e => e.Extension == "bnd" && e.Folder.Contains("/model/parts"))
                .ToList();

            secondaryDicts.Add(partDict);
        }
        else
        {
            secondaryDicts.Add(partDict);
        }

        // SFX Textures
        var sfxDict = new FileDictionary();
        sfxDict.Entries = Project.Locator.FileDictionary.Entries
            .Where(e => e.Archive != "sd")
            .Where(e => e.Extension == "ffxbnd")
            .ToList();

        secondaryDicts.Add(sfxDict);

        // Merge all unique entries from the secondary dicts into the base dict to form the final dictionary
        TextureFiles = ProjectUtils.MergeFileDictionaries(baseDict, secondaryDicts);
    }

    public void SetupPackedTextureDictionaries()
    {
        TexturePackedFiles.Entries = Project.Locator.FileDictionary.Entries
            .Where(e => e.Archive != "sd")
            .Where(e => e.Extension == "tpfbhd")
            .ToList();
    }

    public void SetupShoeboxDictionaries()
    {
        ShoeboxFiles.Entries = Project.Locator.FileDictionary.Entries
            .Where(e => e.Archive != "sd")
            .Where(e => e.Extension == "sblytbnd")
            .ToList();
    }

    #region Dispose
    public void Dispose()
    {
        PrimaryBank?.Dispose();
        PreviewBank?.Dispose();

        TextureFiles = null;
        TexturePackedFiles = null;
        ShoeboxFiles = null;
    }
    #endregion
}

