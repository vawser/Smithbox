using System;
using System.Collections.Generic;
using System.Text;
using StudioCore.Utilities;

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
        OodleSetup.CopyFromGameDirectory(Descriptor.DataPath);
    }
    #endregion
}
