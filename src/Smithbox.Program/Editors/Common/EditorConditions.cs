using System;
using System.Collections.Generic;
using System.Text;

namespace StudioCore.Editors.Common;

public static class EditorConditions
{
    public static bool SupportsMapEditor(ProjectType curType)
    {
        return true;
    }

    public static bool SupportsModelEditor(ProjectType curType)
    {
        return true;
    }

    public static bool SupportsTimeActEditor(ProjectType curType)
    {
        if (curType
            is ProjectType.DS2
            or ProjectType.DS2S)
        {
            return false;
        }

        return true;
    }

    public static bool SupportsTextEditor(ProjectType curType)
    {
        return true;
    }

    public static bool SupportsParamEditor(ProjectType curType)
    {
        return true;
    }

    /// <summary>The FX Editor: FXR effects of the games the FXR schema covers</summary>
    public static bool SupportsFxEditor(ProjectType curType)
    {
        return curType is ProjectType.DS3 or ProjectType.SDT or ProjectType.ER or ProjectType.AC6 or ProjectType.NR;
    }

    /// <summary>The Behavior Editor: AC6 .bhv state machines in chr behbnds</summary>
    public static bool SupportsBehaviorEditor(ProjectType curType)
    {
        return curType is ProjectType.AC6;
    }

    public static bool SupportsHavokEditor(ProjectType curType)
    {
        if (curType
            is ProjectType.ER
            or ProjectType.AC6
            or ProjectType.NR
            or ProjectType.DS3
            or ProjectType.SDT
            or ProjectType.BB
            or ProjectType.DS2S
            or ProjectType.DS1
            or ProjectType.DS1R
            or ProjectType.DES
            or ProjectType.DB_NT
            or ProjectType.DB)
        {
            return true;
        }

        return false;
    }

    public static bool SupportsGraphicsParamEditor(ProjectType curType)
    {
        if (curType
            is ProjectType.DES
            or ProjectType.DS1
            or ProjectType.DS1R)
        {
            return false;
        }

        return true;
    }

    public static bool SupportsMaterialEditor(ProjectType curType)
    {
        return true;
    }

    public static bool SupportsTextureViewer(ProjectType curType)
    {
        return true;
    }

    public static bool SupportsFileBrowser(ProjectType curType)
    {
        return true;
    }


}
