using Octokit;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.EventScriptEditorNS;
using StudioCore.Formats.JSON;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.EzStateEditorNS;

/// <summary>
/// Holds the data banks for EzState Scripts
/// Data Flow: Lazy Load
/// </summary>
public class EsdData
{
    public Smithbox BaseEditor;
    public ProjectEntry Project;

    public FileDictionary EsdFiles = new();

    public EsdBank PrimaryBank;
    public EsdBank VanillaBank;

    public EsdMeta Meta;

    public EsdData(Smithbox baseEditor, ProjectEntry project)
    {
        BaseEditor = baseEditor;
        Project = project;
    }
    public async Task<bool> Setup()
    {
        await Task.Delay(1);

        Meta = new(BaseEditor, Project);

        var talkDictionary = new FileDictionary();
        talkDictionary.Entries = Project.FileDictionary.Entries.Where(e => e.Extension == "talkesdbnd").ToList();

        var looseDictionary = new FileDictionary();
        looseDictionary.Entries = Project.FileDictionary.Entries.Where(e => e.Extension == "esd").ToList();

        EsdFiles = ProjectUtils.MergeFileDictionaries(talkDictionary, looseDictionary);

        // Meta
        Task<bool> metaTask = Meta.Setup();
        bool metaTaskResult = await metaTask;

        // Banks
        PrimaryBank = new("Primary", BaseEditor, Project, Project.FS);
        VanillaBank = new("Vanilla", BaseEditor, Project, Project.VanillaFS);

        // Primary Bank
        Task<bool> primaryBankTask = PrimaryBank.Setup();
        bool primaryBankTaskResult = await primaryBankTask;

        // Vanilla Bank
        Task<bool> vanillaBankTask = VanillaBank.Setup();
        bool vanillaBankTaskResult = await vanillaBankTask;

        return true;
    }
}
