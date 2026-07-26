using System;
using System.Collections.Generic;
using System.Text;

namespace StudioCore.Developer;

/// <summary>
/// Data-only version of ProjectEntry
/// </summary>
public class DataProjectEntry
{
    public DataProjectDescriptor Descriptor;

    public DataProjectVFS VFS;
    public DataProjectFileLocator Locator;

    public bool Initialized = false;

    public DataProjectEntry() { }

    public async Task<bool> Init()
    {
        // Sanity checks
        if (Descriptor.ProjectType is ProjectType.Undefined)
        {
            Smithbox.LogError(this, LOC.Get("PROJECT_Setup_Failed_Init_Undefined_Type"));

            return false;
        }

        if (!Directory.Exists(Descriptor.DataPath))
        {
            Smithbox.LogError(this, LOC.Get("PROJECT_Setup_Failed_Init_Bad_Data_Path", Descriptor.DataPath));

            return false;
        }

        Initialized = false;

        VFS = new(this);
        Locator = new(this);

        try
        {
            SetupDLLs();
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("PROJECT_Setup_Failed_Init_Failed_DLL_Setup"), e);
        }

        try
        {
            VFS.Initialize();
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("PROJECT_Setup_Failed_Init_Failed_VFS_Setup"), e);
        }

        try
        {
            await Locator.Initialize();
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("PROJECT_Setup_Failed_Init_Failed_Locator_Setup"), e);
        }

        Initialized = true;

        return true;
    }

    #region Setup DLLS
    public void SetupDLLs()
    {
        if (Descriptor.ProjectType is ProjectType.SDT or ProjectType.ER)
        {
#if WINDOWS
            var rootDllPath = Path.Join(Descriptor.DataPath, "oo2core_6_win64.dll");
            var projectDllPath = Path.Join(AppContext.BaseDirectory, "oo2core_6_win64.dll");
#elif OSX
            var rootDllPath = Path.Join(DataPath, "liboo2coremac64.2.6.dylib");
            var projectDllPath = Path.Join(AppContext.BaseDirectory, "liboo2coremac64.2.6.dylib");
#elif LINUX
            var rootDllPath = Path.Join(DataPath, "liboo2corelinux64.so.6");
            var projectDllPath = Path.Join(AppContext.BaseDirectory, "liboo2corelinux64.so.6");
#endif

            if (File.Exists(rootDllPath))
            {
                if (!File.Exists(projectDllPath))
                {
                    File.Copy(rootDllPath, projectDllPath);
                }
            }
        }

        if (Descriptor.ProjectType is ProjectType.AC6)
        {
#if WINDOWS
            var rootDllPath = Path.Join(Descriptor.DataPath, "oo2core_8_win64.dll");
            var projectDllPath = Path.Join(AppContext.BaseDirectory, "oo2core_8_win64.dll");
#elif OSX
            var rootDllPath = Path.Join(DataPath, "liboo2coremac64.2.8.dylib");
            var projectDllPath = Path.Join(AppContext.BaseDirectory, "liboo2coremac64.2.8.dylib");
#elif LINUX
            var rootDllPath = Path.Join(DataPath, "liboo2corelinux64.so.8");
            var projectDllPath = Path.Join(AppContext.BaseDirectory, "liboo2corelinux64.so.8");
#endif

            if (File.Exists(rootDllPath))
            {
                if (!File.Exists(projectDllPath))
                {
                    File.Copy(rootDllPath, projectDllPath);
                }
            }
        }


        if (Descriptor.ProjectType is ProjectType.NR)
        {
#if WINDOWS
            var rootDllPath = Path.Join(Descriptor.DataPath, "oo2core_9_win64.dll");
            var projectDllPath = Path.Join(AppContext.BaseDirectory, "oo2core_9_win64.dll");
#elif OSX
            var rootDllPath = Path.Join(DataPath, "liboo2coremac64.2.9.dylib");
            var projectDllPath = Path.Join(AppContext.BaseDirectory, "liboo2coremac64.2.9.dylib");
#elif LINUX
            var rootDllPath = Path.Join(DataPath, "liboo2corelinux64.so.9");
            var projectDllPath = Path.Join(AppContext.BaseDirectory, "liboo2corelinux64.so.9");
#endif

            if (File.Exists(rootDllPath))
            {
                if (!File.Exists(projectDllPath))
                {
                    File.Copy(rootDllPath, projectDllPath);
                }
            }
        }
    }
    #endregion
}
