using StudioCore.Application;

namespace StudioCore.Editors.AnimEditor;

public static class BehaviorUtils
{
    public static bool SupportsHKX1(ProjectEntry project)
    {
        if (project.Descriptor.ProjectType is ProjectType.DS1 or ProjectType.DS1R or ProjectType.DS2 or ProjectType.DS2S or ProjectType.BB)
        {
            return true;
        }

        return false;
    }

    public static bool SupportsHKX2(ProjectEntry project)
    {
        if (project.Descriptor.ProjectType is ProjectType.DS3)
        {
            return true;
        }

        return false;
    }

    public static bool SupportsHKX3(ProjectEntry project)
    {
        if (project.Descriptor.ProjectType is ProjectType.ER or ProjectType.NR)
        {
            return true;
        }

        return false;
    }
}
