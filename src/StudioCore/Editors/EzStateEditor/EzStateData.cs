using HKLib.hk2018.castTest;
using StudioCore.Core;
using StudioCore.Core.ProjectNS;
using StudioCore.Editors.EventScriptEditorNS;
using StudioCore.JSON;
using StudioCore.Resources.JSON;
using StudioCore.Utilities;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace StudioCore.Editors.EzStateEditorNS;

public class EzStateData
{
    public BaseEditor BaseEditor;
    public Project Project;

    public EzStateMeta Meta;

    public EzStateBank PrimaryBank;

    public FileDictionary EzStateFiles;

    public bool IsSetup;

    public EzStateData(BaseEditor editor, Project projectOwner)
    {
        BaseEditor = editor;
        Project = projectOwner;

        PrimaryBank = new(this, "Primary");

        EzStateFiles = new();
        EzStateFiles.Entries = new();

        Setup();
    }

    public async void Setup()
    {
        var rootFiles = Project.FileDictionary.Entries.Where(e => e.Extension == "talkesdbnd").ToList();
        var projectFiles = FileDictionaryUtils.GetUniqueFileEntries(Project, "talkesdbnd");
        EzStateFiles.Entries = FileDictionaryUtils.GetFinalDictionaryEntries(rootFiles, projectFiles);

        Meta = new(BaseEditor, Project);

        // Meta
        Meta.Root = new EsdMeta_Root();

        // Meta
        Task<bool> metaTask = LoadMeta();
        bool metaTaskResult = await metaTask;

        if (metaTaskResult)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:EzState Editor] Setup Meta.");
        }
        else
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:EzState Editor] Failed to load Meta.");
        }

        IsSetup = true;
    }

    public async Task<bool> LoadMeta()
    {
        await Task.Delay(1000);

        var folder = @$"{AppContext.BaseDirectory}\Assets\ESD\{ProjectUtils.GetGameDirectory(Project)}";
        var file = "";

        switch (Project.ProjectType)
        {
            case ProjectType.DS1:
            case ProjectType.DS1R:
            case ProjectType.DS2:
            case ProjectType.DS2S:
            case ProjectType.BB:
            case ProjectType.DS3:
            case ProjectType.SDT:
            case ProjectType.ER:
            case ProjectType.AC6:
                file = Path.Combine(folder, "Talk.json");
                break;
            default: break;
        }

        if (File.Exists(file))
        {
            try
            {
                var filestring = File.ReadAllText(file);
                var options = new JsonSerializerOptions();
                Meta.Root = JsonSerializer.Deserialize(filestring, SmithboxSerializerContext.Default.EsdMeta_Root);

                if (Meta.Root == null)
                {
                    throw new Exception("JsonConvert returned null");
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddLog("[{Project.ProjectName}:EzState Editor] Failed to read Meta.");
            }
        }
        else
        {
            return false;
        }

        return true;
    }
}
