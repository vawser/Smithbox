using Andre.IO.VFS;
using Octokit;
using StudioCore.Core;
using StudioCore.EzStateEditorNS;
using StudioCore.Formats.JSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.FileBrowserNS;

public class FileData
{
    public Smithbox BaseEditor;
    public ProjectEntry Project;

    public List<VirtualFileSystemFsEntry> Roots = new();

    public FileData(Smithbox baseEditor, ProjectEntry project)
    {
        BaseEditor = baseEditor;
        Project = project;
    }

    public async Task<bool> Setup()
    {
        await Task.Yield();

        // VFS Roots
        Task<bool> vfsRootsTask = LoadVfsRoots();
        bool vfsRootsLoaded = await vfsRootsTask;

        return true;
    }
    private async Task<bool> LoadVfsRoots()
    {
        await Task.Yield();

        //Roots.Clear();
        //bool anyFs = false;
        //bool vanillaFs = false;

        if (Project.VanillaRealFS is not EmptyVirtualFileSystem)
        {
            Roots.Add(new VirtualFileSystemFsEntry(Project, Project.VanillaRealFS, "Game Directory"));
            //anyFs = true;
            //vanillaFs = true;
        }

        //if (Project.VanillaBinderFS is not EmptyVirtualFileSystem)
        //{
        //    Roots.Add(new VirtualFileSystemFsEntry(Project, Project.VanillaBinderFS, "Vanilla Dvdbnds"));
        //    anyFs = true;
        //    vanillaFs = true;
        //}

        //if (vanillaFs && Project.VanillaFS is not EmptyVirtualFileSystem)
        //{
        //    Roots.Add(new VirtualFileSystemFsEntry(Project, Project.VanillaFS, "Full Vanilla FS"));
        //}

        if (Project.ProjectFS is not EmptyVirtualFileSystem)
        {
            Roots.Add(new VirtualFileSystemFsEntry(Project, Project.ProjectFS, "Project Directory"));
            //anyFs = true;
        }

        //if (anyFs && Project.FS is not EmptyVirtualFileSystem)
        //{
        //    Roots.Add(new VirtualFileSystemFsEntry(Project, Project.FS, "Full Combined FS"));
        //}

        return true;
    }

}
