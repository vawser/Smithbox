using StudioCore.Core;
using StudioCore.Editors.TimeActEditor.Bank;
using StudioCore.EzStateEditorNS;
using StudioCore.Formats.JSON;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static SoulsFormats.TAE;

namespace StudioCore.Editors.TimeActEditor;

public class TimeActData
{
    public Smithbox BaseEditor;
    public ProjectEntry Project;

    public TimeActBank PrimaryBank;
    public TimeActBank VanillaBank;

    public Dictionary<string, Template> TimeActTemplates = new Dictionary<string, Template>();

    public FileDictionary TimeActFiles = new();

    public TimeActData(Smithbox baseEditor, ProjectEntry project)
    {
        BaseEditor = baseEditor;
        Project = project;
    }


    public async Task<bool> Setup()
    {
        await Task.Yield();

        var chrBndDictionary = new FileDictionary();
        chrBndDictionary.Entries = Project.FileDictionary.Entries
            .Where(e => e.Archive != "sd")
            .Where(e => e.Extension == "anibnd")
            .ToList();

        var looseDictionary = new FileDictionary();
        looseDictionary.Entries = Project.FileDictionary.Entries
            .Where(e => e.Archive != "sd")
            .Where(e => e.Extension == "tae")
            .ToList();

        TimeActFiles = ProjectUtils.MergeFileDictionaries(chrBndDictionary, looseDictionary);

        PrimaryBank = new("Primary", BaseEditor, Project, Project.FS);
        VanillaBank = new("Vanilla", BaseEditor, Project, Project.VanillaFS);

        // TAE Templates
        Task<bool> templateTask = LoadTimeActTemplates();
        bool templateTaskResult = await templateTask;

        if (!templateTaskResult)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Time Act Editor] Failed to setup Time Act templates.");
        }

        // Primary Bank (Chr)
        Task<bool> primaryChrBankTask = PrimaryBank.Setup();
        bool primaryChrBankTaskResult = await primaryChrBankTask;

        if (!primaryChrBankTaskResult)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Time Act Editor] Failed to setup Primary Character Time Act bank.");
        }

        // Vanilla Bank (Chr)
        Task<bool> vanillaChrBankTask = VanillaBank.Setup();
        bool vanillaChrBankTaskResult = await vanillaChrBankTask;

        if (!vanillaChrBankTaskResult)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Time Act Editor] Failed to setup Vanilla Character Time Act bank.");
        }


        return true;
    }

    public async Task<bool> LoadTimeActTemplates()
    {
        await Task.Yield();

        string templateDir = $"{AppContext.BaseDirectory}Assets\\TAE\\";
        foreach (string file in Directory.EnumerateFiles(templateDir, "*.xml"))
        {
            string name = Path.GetFileNameWithoutExtension(file);
            Template template = Template.ReadXMLFile(file);

            TimeActTemplates.Add(name, template);
        }

        return true;
    }
}
