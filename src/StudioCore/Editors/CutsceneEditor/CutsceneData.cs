using StudioCore.Core;
using StudioCore.Core.ProjectNS;
using StudioCore.Resources.JSON;
using StudioCore.Utilities;
using System.Linq;

namespace StudioCore.Editors.CutsceneEditorNS;

public class CutsceneData
{
    public BaseEditor BaseEditor;
    public Project Project;

    public CutsceneBank PrimaryBank;

    public FileDictionary CutsceneFiles;

    public bool IsSetup;

    public CutsceneData(BaseEditor editor, Project projectOwner)
    {
        BaseEditor = editor;
        Project = projectOwner;

        PrimaryBank = new(this, "Primary");

        CutsceneFiles = new();
        CutsceneFiles.Entries = new();

        Setup();
    }

    public async void Setup()
    {
        var rootFiles = Project.FileDictionary.Entries.Where(e => e.Extension == "cutscenebnd").ToList();
        var projectFiles = FileDictionaryUtils.GetUniqueFileEntries(Project, "cutscenebnd");
        CutsceneFiles.Entries = FileDictionaryUtils.GetFinalDictionaryEntries(rootFiles, projectFiles);

        // Async tasks here

        IsSetup = true;
    }
}
