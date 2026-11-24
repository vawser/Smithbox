using Andre.IO.VFS;
using StudioCore.Application;

namespace SmithboxTests;

/// <summary>
///     Builds in-memory projects for tests. Nothing here touches the disk, the game data or the graphics device:
///     a test that needs those belongs elsewhere.
/// </summary>
internal static class TestProjects
{
    public static ProjectEntry Of(ProjectType type)
    {
        var project = new ProjectEntry
        {
            Descriptor = new ProjectDescriptor
            {
                ProjectName = "UnitTestProject",
                ProjectType = type
            }
        };

        // PathBuilder checks the built path against the VFS in DEBUG builds and logs when it's missing. The logger
        // only exists once the app has started, so report every file as present.
        project.VFS = new ProjectVFS(project) { FS = AllFilesExistFileSystem.Instance };

        return project;
    }

    /// <summary>An empty, read-only file system that reports every file as present</summary>
    private sealed class AllFilesExistFileSystem : EmptyVirtualFileSystem
    {
        public static new readonly AllFilesExistFileSystem Instance = new();

        public override bool FileExists(VFSPath path) => true;
    }
}
