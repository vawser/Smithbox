using StudioCore;
using StudioCore.Application;
using StudioCore.Renderer;

namespace SmithboxTests;

public abstract class SmithboxTestBase
{
    protected Smithbox SmithboxInstance { get; private set; }

    protected ProjectManager ProjectManager => SmithboxInstance.ProjectManager;

    protected SmithboxTestBase()
    {
        SmithboxInstance = new Smithbox(new VulkanGraphicsContext(), "Test", false);
        //SmithboxInstance = new Smithbox(new OpenGLCompatGraphicsContext(), "Test", true);

        Smithbox.FirstFrame = false;
        Smithbox.FontRebuildRequest = false;

        var newProject = CreateERProject();

        SmithboxInstance.ProjectManager.Projects.Add(newProject);
        SmithboxInstance.ProjectManager.StartupProject(newProject);
    }

    private ProjectEntry CreateDESProject()
    {
        return CreateProject(
            ProjectType.DES,
            "G:\\Modding\\Demon's Souls\\DES-Test",
            "G:\\Game Data Repository\\Demon's Souls\\PS3_GAME\\USRDIR");
    }

    private ProjectEntry CreateDS1Project()
    {
        return CreateProject(
            ProjectType.DS1,
            "G:\\Modding\\Dark Souls PTDE\\DS1-Test",
            "F:\\SteamLibrary\\steamapps\\common\\Dark Souls Prepare to Die Edition\\DATA");
    }

    private ProjectEntry CreateDS1RProject()
    {
        return CreateProject(
            ProjectType.DS1R,
            "G:\\Modding\\Dark Souls Remastered\\DS1R-Test\\",
            "F:\\SteamLibrary\\steamapps\\common\\DARK SOULS REMASTERED");
    }

    private ProjectEntry CreateDS2Project()
    {
        return CreateProject(
            ProjectType.DS2,
            "G:\\Modding\\Dark Souls II\\Projects\\DS2-Vanilla-Test",
            "F:\\SteamLibrary\\steamapps\\common\\Dark Souls II\\Game");
    }

    private ProjectEntry CreateDS2SProject()
    {
        return CreateProject(
            ProjectType.DS2S,
            "G:\\Modding\\Dark Souls II\\Projects\\DS2-Test",
            "F:\\SteamLibrary\\steamapps\\common\\Dark Souls II Scholar of the First Sin\\Game");
    }
    private ProjectEntry CreateBBProject()
    {
        return CreateProject(
            ProjectType.BB,
            "G:\\Modding\\Bloodborne\\Test-Project",
            "G:\\Game Data Repository\\Bloodborne\\dvdroot_ps4");
    }

    private ProjectEntry CreateDS3Project()
    {
        return CreateProject(
            ProjectType.DS3,
            "G:\\Modding\\Dark Souls III\\Projects\\DS3-Test",
            "F:\\SteamLibrary\\steamapps\\common\\DARK SOULS III\\Game");
    }

    private ProjectEntry CreateSDTProject()
    {
        return CreateProject(
            ProjectType.SDT,
            "G:\\Modding\\Sekiro\\Projects\\Sekiro-Test",
            "F:\\SteamLibrary\\steamapps\\common\\Sekiro");
    }

    private ProjectEntry CreateERProject()
    {
        return CreateProject(
            ProjectType.ER,
            "G:\\Modding\\Elden Ring\\Projects\\ER-Test\\mod",
            "F:\\SteamLibrary\\steamapps\\common\\ELDEN RING\\Game");
    }

    private ProjectEntry CreateAC6Project()
    {
        return CreateProject(
            ProjectType.AC6,
            "G:\\Modding\\Armored Core VI\\Projects\\AC6-Test\\Mod",
            "F:\\SteamLibrary\\steamapps\\common\\ARMORED CORE VI FIRES OF RUBICON\\Game");
    }

    private ProjectEntry CreateProject(ProjectType projType, string projectPath, string dataPath)
    {
        var newProject = new ProjectEntry();

        newProject.ProjectGUID = Guid.NewGuid();
        newProject.ProjectName = "UnitTestProject";
        newProject.ProjectPath = projectPath;
        newProject.DataPath = dataPath;
        newProject.ProjectType = projType;
        newProject.AutoSelect = true;
        newProject.EnableMapEditor = true;
        newProject.EnableModelEditor = true;
        newProject.EnableTextEditor = true;
        newProject.EnableParamEditor = true;
        newProject.EnableGparamEditor = true;
        newProject.EnableMaterialEditor = true;
        newProject.EnableTextureViewer = true;
        newProject.EnableFileBrowser = true;
        newProject.EnableExternalMaterialData = true;

        return newProject;
    }
}