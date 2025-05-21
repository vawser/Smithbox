using StudioCore.Core;
using StudioCore.Editors.TimeActEditor;
using StudioCore.Editors.TimeActEditor.Bank;
using StudioCore.Formats.JSON;
using System.Linq;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextureViewer.Data;

public class TextureData
{
    public Smithbox BaseEditor;
    public ProjectEntry Project;

    public TextureBank PrimaryBank;

    public FileDictionary TextureFiles = new();
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
        // TPF
        var baseDict = new FileDictionary();
        baseDict.Entries = Project.FileDictionary.Entries.Where(e => e.Extension == "tpf").ToList();

        // Object Textures
        var objDict = new FileDictionary();
        objDict.Entries = Project.FileDictionary.Entries.Where(e => e.Extension == "objbnd").ToList();

        if (Project.ProjectType == ProjectType.DS2S || Project.ProjectType == ProjectType.DS2)
        {
            objDict.Entries = Project.FileDictionary.Entries.Where(e => e.Extension == "bnd" && e.Folder == "/model/obj").ToList();
        }

        TextureFiles = ProjectUtils.MergeFileDictionaries(baseDict, objDict);

        // Chr Textures
        var chrDict = new FileDictionary();
        chrDict.Entries = Project.FileDictionary.Entries.Where(e => e.Extension == "texbnd").ToList();

        TextureFiles = ProjectUtils.MergeFileDictionaries(TextureFiles, chrDict);

        // Part Textures
        var partDict = new FileDictionary();
        partDict.Entries = Project.FileDictionary.Entries.Where(e => e.Extension == "partsbnd").ToList();

        if (Project.ProjectType == ProjectType.DS2S || Project.ProjectType == ProjectType.DS2)
        {
            partDict.Entries = Project.FileDictionary.Entries.Where(e => e.Extension == "bnd" && e.Folder == "/model/obj").ToList();
        }

        TextureFiles = ProjectUtils.MergeFileDictionaries(TextureFiles, chrDict);
    }

    public void SetupShoeboxDictionaries()
    {

    }
}

