using Microsoft.Extensions.Logging;
using StudioCore.Application;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextEditor;

public class TextData
{
    public Smithbox BaseEditor;
    public ProjectEntry Project;

    public TextBank PrimaryBank;
    public TextBank VanillaBank;
    public Dictionary<string, TextBank> AuxBanks = new();

    public FileDictionary FmgFiles = new();

    public LanguageDef LanguageDef;
    public ContainerDef ContainerDef;
    public AssociationDef AssociationDef;

    public TextData(Smithbox baseEditor, ProjectEntry project)
    {
        BaseEditor = baseEditor;
        Project = project;
    }

    public async Task<bool> Setup()
    {
        await Task.Yield();

        // TODO: for transitioning from the hardcoded stuff into JSON
        //LanguageDef = SetupLanguageDef();
        //ContainerDef = SetupContainerDef();
        //AssociationDef = SetupAssociationDef();

        FmgFiles = SetupFmgFilelist();

        PrimaryBank = new("Primary", BaseEditor, Project, Project.FS);
        VanillaBank = new("Vanilla", BaseEditor, Project, Project.VanillaFS);

        // Primary Bank
        Task<bool> primaryBankTask = PrimaryBank.Setup();
        bool primaryBankTaskResult = await primaryBankTask;

        if (!primaryBankTaskResult)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Text Editor] Failed to fully setup Primary Bank.", LogLevel.Error, LogPriority.High);
        }

        // Vanilla Bank
        Task<bool> vanillaBankTask = VanillaBank.Setup();
        bool vanillaBankTaskResult = await vanillaBankTask;

        if (!vanillaBankTaskResult)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Text Editor] Failed to fully setup Primary Bank.", LogLevel.Error, LogPriority.High);
        }

        return true;
    }

    public async Task<bool> LoadAuxBank(ProjectEntry targetProject, bool reloadProject)
    {
        await Task.Yield();

        if (reloadProject)
        {
            await targetProject.Init(silent: true, InitType.TextEditorOnly);
        }
        else
        {
            if (!targetProject.Initialized)
            {
                await targetProject.Init(silent: true, InitType.TextEditorOnly);
            }
        }

        var newAuxBank = new TextBank($"{targetProject.ProjectName}", BaseEditor, Project, targetProject.FS);

        // Aux Bank
        Task<bool> auxBankTask = newAuxBank.Setup();
        bool auxBankTaskResult = await auxBankTask;

        if (!auxBankTaskResult)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Text Editor] Failed to setup Aux FMG Bank.");
        }

        if (AuxBanks.ContainsKey(targetProject.ProjectName))
        {
            AuxBanks[targetProject.ProjectName] = newAuxBank;
        }
        else
        {
            AuxBanks.Add(targetProject.ProjectName, newAuxBank);
        }

        TaskLogs.AddLog($"[{Project.ProjectName}:Text Editor] Setup Aux FMG Bank.");

        return true;
    }
    public FileDictionary SetupFmgFilelist()
    {
        var msgbndDictionary = new FileDictionary();
        msgbndDictionary.Entries = Project.FileDictionary.Entries
            .Where(e => e.Archive != "sd")
            .Where(e => e.Folder.StartsWith("/msg"))
            .Where(e => e.Extension == "msgbnd")
            .ToList();


        if (Project.ProjectType == ProjectType.ER)
        {
            msgbndDictionary.Entries = msgbndDictionary.Entries.OrderBy(e => e.Folder).ThenBy(e => e.Filename.Contains("dlc02")).ThenBy(e => e.Filename.Contains("dlc01")).ThenBy(e => e.Filename).ToList();
        }

        var fmgDictionary = new FileDictionary();
        fmgDictionary.Entries = new List<FileDictionaryEntry>();

        if (Project.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
        {
            fmgDictionary.Entries = Project.FileDictionary.Entries
                .Where(e => e.Archive != "sd")
                .Where(e => e.Folder.StartsWith("/menu/text"))
                .Where(e => e.Extension == "fmg").ToList();
        }

        return ProjectUtils.MergeFileDictionaries(msgbndDictionary, fmgDictionary);
    }


    public LanguageDef SetupLanguageDef()
    {
        LanguageDef def = null;

        var folder = Path.Join(AppContext.BaseDirectory, "Assets", "FMG", ProjectUtils.GetGameDirectory(Project.ProjectType));
        var file = Path.Combine(folder, "Languages.json");

        if (File.Exists(file))
        {
            try
            {
                var filestring = File.ReadAllText(file);

                try
                {
                    def = JsonSerializer.Deserialize(filestring, FmgJsonSerializerContext.Default.LanguageDef);
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"[Smithbox] Failed to deserialize the FMG Language Def: {file}", LogLevel.Error, LogPriority.High, e);
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[Smithbox] Failed to read the FMG Language Def: {file}", LogLevel.Error, LogPriority.High, e);
            }
        }

        return def;
    }

    public ContainerDef SetupContainerDef()
    {
        ContainerDef def = null;

        var folder = Path.Join(AppContext.BaseDirectory, "Assets", "FMG", ProjectUtils.GetGameDirectory(Project.ProjectType));
        var file = Path.Combine(folder, "Containers.json");

        if (File.Exists(file))
        {
            try
            {
                var filestring = File.ReadAllText(file);

                try
                {
                    def = JsonSerializer.Deserialize(filestring, FmgJsonSerializerContext.Default.ContainerDef);
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"[Smithbox] Failed to deserialize the FMG Container Def: {file}", LogLevel.Error, LogPriority.High, e);
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[Smithbox] Failed to read the FMG Container Def: {file}", LogLevel.Error, LogPriority.High, e);
            }
        }

        return def;
    }

    public AssociationDef SetupAssociationDef()
    {
        AssociationDef def = null;

        var folder = Path.Join(AppContext.BaseDirectory, "Assets", "FMG", ProjectUtils.GetGameDirectory(Project.ProjectType));
        var file = Path.Combine(folder, "Associations.json");


        if (File.Exists(file))
        {
            try
            {
                var filestring = File.ReadAllText(file);

                try
                {
                    def = JsonSerializer.Deserialize(filestring, FmgJsonSerializerContext.Default.AssociationDef);
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"[Smithbox] Failed to deserialize the FMG Association Def: {file}", LogLevel.Error, LogPriority.High, e);
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[Smithbox] Failed to read the FMG Association Def: {file}", LogLevel.Error, LogPriority.High, e);
            }
        }

        return def;
    }
}
