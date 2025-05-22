using DotNext.Collections.Generic;
using StudioCore.Core;
using StudioCore.Editors.TimeActEditor;
using StudioCore.Editors.TimeActEditor.Bank;
using StudioCore.Formats.JSON;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextureViewer.Data;

public class TextureData
{
    public Smithbox BaseEditor;
    public ProjectEntry Project;

    public TextureBank PrimaryBank;

    public FileDictionary TextureFiles = new();

    public FileDictionary TexturePackedFiles = new();

    public FileDictionary ShoeboxFiles = new();

    public TextureData(Smithbox baseEditor, ProjectEntry project)
    {
        BaseEditor = baseEditor;
        Project = project;
    }

    public async Task<bool> Setup()
    {
        await Task.Yield();

        SetupTextureDictionaries();
        SetupPackedTextureDictionaries();
        SetupShoeboxDictionaries();

        PrimaryBank = new("Primary", BaseEditor, Project, Project.FS);

        Task<bool> primaryChrBankTask = PrimaryBank.Setup();
        bool primaryChrBankTaskResult = await primaryChrBankTask;

        if (!primaryChrBankTaskResult)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Texture Viewer] Failed to setup Primary Texture bank.");
        }

        return true;
    }

    public void SetupTextureDictionaries()
    {
        var secondaryDicts = new List<FileDictionary>();

        // TPF
        var baseDict = new FileDictionary();
        baseDict.Entries = Project.FileDictionary.Entries
            .Where(e => e.Archive != "sd")
            .Where(e => e.Extension == "tpf")
            .ToList();

        // Object Textures
        var objDict = new FileDictionary();
        objDict.Entries = Project.FileDictionary.Entries
            .Where(e => e.Archive != "sd")
            .Where(e => e.Extension == "objbnd")
            .ToList();

        if (Project.ProjectType == ProjectType.DS2S || Project.ProjectType == ProjectType.DS2)
        {
            objDict.Entries = Project.FileDictionary.Entries
                .Where(e => e.Archive != "sd")
                .Where(e => e.Extension == "bnd" && e.Folder == "/model/obj")
                .ToList();
        }

        secondaryDicts.Add(objDict);

        // Chr Textures
        var chrDict = new FileDictionary();
        chrDict.Entries = Project.FileDictionary.Entries
            .Where(e => e.Archive != "sd")
            .Where(e => e.Extension == "texbnd")
            .ToList();

        secondaryDicts.Add(chrDict);

        // Part Textures
        var partDict = new FileDictionary();
        partDict.Entries = Project.FileDictionary.Entries
            .Where(e => e.Archive != "sd")
            .Where(e => e.Extension == "partsbnd")
            .ToList();

        if (Project.ProjectType == ProjectType.DS2S || Project.ProjectType == ProjectType.DS2)
        {
            var commonPartDict = new FileDictionary();
            commonPartDict.Entries = Project.FileDictionary.Entries
                .Where(e => e.Archive != "sd")
                .Where(e => e.Extension == "commonbnd")
                .ToList();

            secondaryDicts.Add(commonPartDict);

            partDict.Entries = Project.FileDictionary.Entries
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
        sfxDict.Entries = Project.FileDictionary.Entries
            .Where(e => e.Archive != "sd")
            .Where(e => e.Extension == "ffxbnd")
            .ToList();

        secondaryDicts.Add(sfxDict);

        // Merge all unique entries from the secondary dicts into the base dict to form the final dictionary
        TextureFiles = ProjectUtils.MergeFileDictionaries(baseDict, secondaryDicts);
    }

    public void SetupPackedTextureDictionaries()
    {
        TexturePackedFiles.Entries = Project.FileDictionary.Entries
            .Where(e => e.Archive != "sd")
            .Where(e => e.Extension == "tpfbhd")
            .ToList();
    }

    public void SetupShoeboxDictionaries()
    {
        ShoeboxFiles.Entries = Project.FileDictionary.Entries
            .Where(e => e.Archive != "sd")
            .Where(e => e.Extension == "sblytbnd")
            .ToList();
    }
}

