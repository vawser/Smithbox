using Andre.IO.VFS;
using HKLib.hk2018.hkaiCollisionAvoidance;
using StudioCore.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.AnimEditor;

public class BehaviorBank : IDisposable
{
    public ProjectEntry Project;

    public VirtualFileSystem TargetFS = EmptyVirtualFileSystem.Instance;

    public string Name;

    public BehaviorBank(string name, ProjectEntry project, VirtualFileSystem targetFs)
    {
        Project = project;
        Name = name;
        TargetFS = targetFs;
    }

    public async Task<bool> Setup()
    {
        await Task.Yield();

        return true;
    }

    #region Dispose
    public void Dispose()
    {

    }
    #endregion
}
