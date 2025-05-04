using StudioCore.Core;
using StudioCore.Editors.TimeActEditor.Bank;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using static SoulsFormats.TAE;

namespace StudioCore.Editors.TimeActEditor;

public class TimeActData
{
    public Smithbox BaseEditor;
    public ProjectEntry Project;

    public TimeActBank PrimaryCharacterBank;
    public TimeActBank VanillaCharacterBank;
    public TimeActBank PrimaryObjectBank;
    public TimeActBank VanillaObjectBank;

    public Dictionary<string, Template> TimeActTemplates = new Dictionary<string, Template>();

    public TimeActData(Smithbox baseEditor, ProjectEntry project)
    {
        BaseEditor = baseEditor;
        Project = project;
    }

    public async Task<bool> Setup()
    {
        await Task.Delay(1);

        PrimaryCharacterBank = new(BaseEditor, Project, TimeActType.Character, Project.ProjectPath, Project.DataPath);
        VanillaCharacterBank = new(BaseEditor, Project, TimeActType.Character, Project.DataPath, Project.DataPath);

        PrimaryObjectBank = new(BaseEditor, Project, TimeActType.Object, Project.ProjectPath, Project.DataPath);
        VanillaObjectBank = new(BaseEditor, Project, TimeActType.Object, Project.DataPath, Project.DataPath);

        // TAE Templates
        Task<bool> templateTask = LoadTimeActTemplates();
        bool templateTaskResult = await templateTask;

        if (templateTaskResult)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Texture Viewer] Setup Time Act templates.");
        }
        else
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Texture Viewer] Failed to setup Time Act templates.");
        }

        // Primary Bank (Chr)
        Task<bool> primaryChrBankTask = PrimaryCharacterBank.Setup();
        bool primaryChrBankTaskResult = await primaryChrBankTask;

        if (primaryChrBankTaskResult)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Texture Viewer] Setup Primary Character Time Act bank.");
        }
        else
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Texture Viewer] Failed to setup Primary Character Time Act bank.");
        }

        // Vanilla Bank (Chr)
        Task<bool> vanillaChrBankTask = VanillaCharacterBank.Setup();
        bool vanillaChrBankTaskResult = await vanillaChrBankTask;

        if (vanillaChrBankTaskResult)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Texture Viewer] Setup Vanilla Character Time Act bank.");
        }
        else
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Texture Viewer] Failed to setup Vanilla Character Time Act bank.");
        }

        //// Primary Bank (Obj)
        //Task<bool> primaryObjBankTask = PrimaryObjectBank.Setup();
        //bool primaryObjBankTaskResult = await primaryObjBankTask;

        //if (primaryObjBankTaskResult)
        //{
        //    TaskLogs.AddLog($"[{Project.ProjectName}:Texture Viewer] Setup Primary Object Time Act bank.");
        //}
        //else
        //{
        //    TaskLogs.AddLog($"[{Project.ProjectName}:Texture Viewer] Failed to setup Primary Object Time Act bank.");
        //}

        //// Vanilla Bank (Obj)
        //Task<bool> vanillaObjectBankTask = VanillaObjectBank.Setup();
        //bool vanillaObjectBankTaskResult = await vanillaObjectBankTask;

        //if (vanillaObjectBankTaskResult)
        //{
        //    TaskLogs.AddLog($"[{Project.ProjectName}:Texture Viewer] Setup Vanilla Object Time Act bank.");
        //}
        //else
        //{
        //    TaskLogs.AddLog($"[{Project.ProjectName}:Texture Viewer] Failed to setup Vanilla Object Time Act bank.");
        //}

        return true;
    }

    public async Task<bool> LoadTimeActTemplates()
    {
        await Task.Delay(1);

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
