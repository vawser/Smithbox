using HKLib.hk2018;
using HKLib.Serialization.hk2018.Binary;
using Octokit;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace StudioCore.Editors.HavokEditor;

public class HavokData : IDisposable
{
    public ProjectEntry Project;

    public HavokData(ProjectEntry project)
    {
        Project = project;
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
