using Microsoft.AspNetCore.Routing;
using StudioCore.Core;

namespace SmithboxTests;

public class ProjectManagerTests : SmithboxTestBase
{
    [Fact]
    public void Project_Is_Initialized()
    {
        var project = ProjectManager.SelectedProject;
        Assert.NotNull(project);
    }

    [Fact]
    public void SetupFolders_CreatesRequiredDirectories()
    {
        var baseFolder = ProjectUtils.GetBaseFolder();

        if (Directory.Exists(baseFolder))
            Directory.Delete(baseFolder, true);

        ProjectManager.SetupFolders();

        Assert.True(Directory.Exists(ProjectUtils.GetBaseFolder()));
        Assert.True(Directory.Exists(ProjectUtils.GetProjectsFolder()));
        Assert.True(Directory.Exists(ProjectUtils.GetConfigurationFolder()));
    }
}