using StudioCore.Core.ProjectNS;
using StudioCore.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StudioCore.Editors.EventScriptEditorNS;
using StudioCore.Resources.JSON;
using StudioCore.Utilities;
using System.IO;

namespace StudioCore.Editors.GparamEditorNS;

public class GparamData
{
    public BaseEditor BaseEditor;
    public Project Project;

    public GparamBank PrimaryBank;
    public GparamBank VanillaBank;

    public FileDictionary GparamFiles;

    public GparamMeta Meta;

    public bool IsSetup;

    public GparamData(BaseEditor editor, Project projectOwner)
    {
        BaseEditor = editor;
        Project = projectOwner;

        PrimaryBank = new(this, "Primary", Project.FS);
        VanillaBank = new(this, "Vanilla", Project.VanillaFS);

        GparamFiles = new();
        GparamFiles.Entries = new();

        Meta = new(editor, projectOwner);

        Setup();
    }

    public async void Setup()
    {
        var rootFiles = Project.FileDictionary.Entries.Where(e => e.Extension == "gparam").ToList();
        var projectFiles = FileDictionaryUtils.GetUniqueFileEntries(Project, "gparam");
        GparamFiles.Entries = FileDictionaryUtils.GetFinalDictionaryEntries(rootFiles, projectFiles);

        Task<bool> metaTask = Meta.Setup();
        bool metaTaskResult = await metaTask;

        if (metaTaskResult)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Graphics Param Editor] Setup graphics param meta.");
        }
        else
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Graphics Param Editor] Failed to load graphics param meta.");
        }

        IsSetup = true;
    }
}
