using StudioCore.Core;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextureViewer;

public class TextureData
{
    public Smithbox BaseEditor;
    public ProjectEntry Project;

    public TextureFolderBank PrimaryBank;

    public ShoeboxLayoutContainer Shoebox;

    public TextureData(Smithbox baseEditor, ProjectEntry project)
    {
        BaseEditor = baseEditor;
        Project = project;
    }

    public async Task<bool> Setup()
    {
        await Task.Yield();

        PrimaryBank = new(BaseEditor, Project, Project.ProjectPath, Project.DataPath);
        Shoebox = new(BaseEditor, Project, Project.ProjectPath, Project.DataPath);

        // Primary Bank
        Task<bool> primaryBankTask = PrimaryBank.Setup();
        bool primaryBankTaskResult = await primaryBankTask;

        if (primaryBankTaskResult)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Texture Viewer] Setup Primary texture bank.");
        }
        else
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Texture Viewer] Failed to setup Primary texture bank.");
        }

        // Shoebox
        Task<bool> shoeboxTask = Shoebox.Setup();
        bool shoeboxTaskResult = await shoeboxTask;

        if (shoeboxTaskResult)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Texture Viewer] Setup Shoebox Layouts.");
        }
        else
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Texture Viewer] Failed to setup Shoebox Layouts.");
        }

        return true;
    }
}
