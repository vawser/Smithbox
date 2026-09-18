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
    public static bool SupportsMapDataEditor(ProjectType curType)
    {
        return true;
    }

    public static bool SupportsModelEditor(ProjectType curType)
    {
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

    public static bool SupportsHavokEditor(ProjectType curType)
    {
        if (curType
            is ProjectType.ER
            or ProjectType.AC6
            or ProjectType.NR
            or ProjectType.DS3)
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
