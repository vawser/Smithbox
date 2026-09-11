using System;
using System.Collections.Generic;
using System.Text;

namespace StudioCore.Editors.HavokEditor;

public static class HavokTypeUtils
{
    public static bool IsBND3(ProjectEntry project)
    {
        if (project.Descriptor.ProjectType is ProjectType.DS1 or ProjectType.DS1R)
            return true;

        return false;
    }
    public static bool IsBXF3(ProjectEntry project)
    {
        if (project.Descriptor.ProjectType is ProjectType.DS1 or ProjectType.DS1R)
            return true;

        return false;
    }

    public static bool IsHKX(ProjectEntry project)
    {
        if (project.Descriptor.ProjectType is ProjectType.DS1 or ProjectType.DS1R or ProjectType.BB)
            return true;

        return false;
    }

    public static bool IsHKX2(ProjectEntry project)
    {
        if (project.Descriptor.ProjectType is ProjectType.DS3 or ProjectType.DS1 or ProjectType.DS1R or ProjectType.BB)
            return true;

        return false;
    }

    public static bool IsHKX3(ProjectEntry project)
    {
        if (project.Descriptor.ProjectType is ProjectType.ER or ProjectType.AC6 or ProjectType.NR)
            return true;

        return false;
    }
}
