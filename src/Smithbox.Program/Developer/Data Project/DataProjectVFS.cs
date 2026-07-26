using Andre.IO.VFS;
using System;
using System.Collections.Generic;
using System.Text;

namespace StudioCore.Developer;

public class DataProjectVFS
{
    public DataProjectEntry Project;

    public VirtualFileSystem FS = EmptyVirtualFileSystem.Instance;

    public VirtualFileSystem VanillaBinderFS = EmptyVirtualFileSystem.Instance;

    public VirtualFileSystem VanillaRealFS = EmptyVirtualFileSystem.Instance;

    public VirtualFileSystem VanillaFS = EmptyVirtualFileSystem.Instance;

    public DataProjectVFS(DataProjectEntry project)
    {
        Project = project;
    }

    public void Initialize()
    {
        DisposeInternal();

        List<VirtualFileSystem> fileSystems = [];

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
        VanillaBinderFS?.Dispose();
        VanillaRealFS?.Dispose();
        VanillaFS?.Dispose();
        FS?.Dispose();

        FS = EmptyVirtualFileSystem.Instance;
        VanillaBinderFS = EmptyVirtualFileSystem.Instance;
        VanillaRealFS = EmptyVirtualFileSystem.Instance;
        VanillaFS = EmptyVirtualFileSystem.Instance;
        FS = EmptyVirtualFileSystem.Instance;
    }
}
