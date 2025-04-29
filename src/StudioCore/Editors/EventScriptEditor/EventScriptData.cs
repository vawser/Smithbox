using StudioCore.Core;
using StudioCore.Core.ProjectNS;
using StudioCore.Resources.JSON;
using StudioCore.Utilities;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace StudioCore.Editors.EventScriptEditorNS;

public class EventScriptData
{
    public BaseEditor BaseEditor;
    public Project Project;

    public EventScriptBank PrimaryBank;

    public EMEDF EventInformation = new();

    public FileDictionary EventScriptFiles;

    public bool IsSetup;

    public EventScriptData(BaseEditor editor, Project projectOwner)
    {
        BaseEditor = editor;
        Project = projectOwner;

        PrimaryBank = new(this, "Primary");

        EventScriptFiles = new();
        EventScriptFiles.Entries = new();

        Setup();
    }

    public async void Setup()
    {
        var rootFiles = Project.FileDictionary.Entries.Where(e => e.Extension == "emevd").ToList();
        var projectFiles = FileDictionaryUtils.GetUniqueFileEntries(Project, "emevd");
        EventScriptFiles.Entries = FileDictionaryUtils.GetFinalDictionaryEntries(rootFiles, projectFiles);

        // EMEDF
        Task<bool> emedfTask = LoadEMEDF();
        bool emedfTaskResult = await emedfTask;

        if (emedfTaskResult)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Event Script Editor] Setup EMEDF.");
        }
        else
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Event Script Editor] Failed to load EMEDF.");
        }

        IsSetup = true;
    }

    public async Task<bool> LoadEMEDF()
    {
        await Task.Delay(1000);

        var folder = @$"{AppContext.BaseDirectory}\Assets\EMEVD\";
        var file = "";

        switch (Project.ProjectType)
        {
            case ProjectType.DS1:
            case ProjectType.DS1R:
                file = Path.Combine(folder, "ds1-common.emedf.json");
                break;
            case ProjectType.DS2:
                file = Path.Combine(folder, "ds2-common.emedf.json");
                break;
            case ProjectType.DS2S:
                file = Path.Combine(folder, "ds2scholar-common.emedf.json");
                break;
            case ProjectType.BB:
                file = Path.Combine(folder, "bb-common.emedf.json");
                break;
            case ProjectType.DS3:
                file = Path.Combine(folder, "ds3-common.emedf.json");
                break;
            case ProjectType.SDT:
                file = Path.Combine(folder, "sekiro-common.emedf.json");
                break;
            case ProjectType.ER:
                file = Path.Combine(folder, "er-common.emedf.json");
                break;
            case ProjectType.AC6:
                file = Path.Combine(folder, "ac6-common.emedf.json");
                break;
            default: break;
        }

        if (File.Exists(file))
        {
            EventInformation = EMEDF.ReadFile(file);
        }
        else
        {
            return false;
        }

        return true;
    }
}
