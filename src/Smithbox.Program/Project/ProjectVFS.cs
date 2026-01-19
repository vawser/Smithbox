using Andre.IO.VFS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Application;

public class ProjectVFS : IDisposable
{
    public ProjectEntry Project;

    public VirtualFileSystem FS = EmptyVirtualFileSystem.Instance;

    public VirtualFileSystem ProjectFS = EmptyVirtualFileSystem.Instance;

    public VirtualFileSystem VanillaBinderFS = EmptyVirtualFileSystem.Instance;

    public VirtualFileSystem VanillaRealFS = EmptyVirtualFileSystem.Instance;

    public VirtualFileSystem VanillaFS = EmptyVirtualFileSystem.Instance;

    public VirtualFileSystem PTDE_FS = EmptyVirtualFileSystem.Instance;

    public ProjectVFS(ProjectEntry project)
    {
        Project = project;
    }

    public void Initialize()
    {
        DisposeInternal();

        List<VirtualFileSystem> fileSystems = [];

        // Order of addition to FS determines precendence when getting a file
        // e.g. ProjectFS is prioritised over VanillaFS

        // Project File System
        if (Directory.Exists(Project.Descriptor.ProjectPath))
        {
            ProjectFS = new RealVirtualFileSystem(Project.Descriptor.ProjectPath, false);
            fileSystems.Add(ProjectFS);
        }
        else
        {
            ProjectFS = EmptyVirtualFileSystem.Instance;
        }

        // Vanilla File System
        if (Directory.Exists(Project.Descriptor.DataPath))
        {
            VanillaRealFS = new RealVirtualFileSystem(Project.Descriptor.DataPath, false);
            fileSystems.Add(VanillaRealFS);

            var andreGame = Project.Descriptor.ProjectType.AsAndreGame();

            if (andreGame != null)
            {
                if (!Project.Descriptor.ProjectType.IsLooseGame())
                {
                    VanillaBinderFS = ArchiveBinderVirtualFileSystem.FromGameFolder(Project.Descriptor.DataPath, andreGame.Value);
                    fileSystems.Add(VanillaBinderFS);
                }

                VanillaFS = new CompundVirtualFileSystem([VanillaRealFS, VanillaBinderFS]);
            }
            else
            {
                VanillaRealFS = EmptyVirtualFileSystem.Instance;
                VanillaFS = EmptyVirtualFileSystem.Instance;
            }
        }
        else
        {
            VanillaRealFS = EmptyVirtualFileSystem.Instance;
            VanillaFS = EmptyVirtualFileSystem.Instance;
        }

        // PTDE File System
        if (Directory.Exists(CFG.Current.PTDE_Data_Path))
        {
            PTDE_FS = new RealVirtualFileSystem(CFG.Current.PTDE_Data_Path, false);
            fileSystems.Add(PTDE_FS);
        }
        else
        {
            PTDE_FS = EmptyVirtualFileSystem.Instance;
        }

        if (fileSystems.Count == 0)
            FS = EmptyVirtualFileSystem.Instance;
        else
            FS = new CompundVirtualFileSystem(fileSystems);
    }

    private bool _disposed;

    public void Dispose()
    {
        if (_disposed)
            return;

        DisposeInternal();

        _disposed = true;
    }

    private void DisposeInternal()
    {
        FS?.Dispose();
        ProjectFS?.Dispose();
        VanillaBinderFS?.Dispose();
        VanillaRealFS?.Dispose();
        VanillaFS?.Dispose();
        PTDE_FS?.Dispose();
        FS?.Dispose();

        FS = EmptyVirtualFileSystem.Instance;
        ProjectFS = EmptyVirtualFileSystem.Instance;
        VanillaBinderFS = EmptyVirtualFileSystem.Instance;
        VanillaRealFS = EmptyVirtualFileSystem.Instance;
        VanillaFS = EmptyVirtualFileSystem.Instance;
        PTDE_FS = EmptyVirtualFileSystem.Instance;
        FS = EmptyVirtualFileSystem.Instance;
    }
}
