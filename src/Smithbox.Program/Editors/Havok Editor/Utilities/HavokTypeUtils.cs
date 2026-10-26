using HKLib.hk2018.castTest;
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
        if (project.Descriptor.ProjectType is ProjectType.ER or ProjectType.AC6 or ProjectType.NR or ProjectType.DB)
            return true;

        return false;
    }

    public static bool IsHKX2_Animation(ProjectEntry project, HKX2.hkRootLevelContainer rootContainer)
    {
        var animationContainers = HavokTreeSearch.FindAll<HKX2.hkaAnimationContainer>(rootContainer);

        if (animationContainers.Count > 0)
            return true;

        return false;
    }

    public static bool IsHKX2_Collision(ProjectEntry project, HKX2.hkRootLevelContainer rootContainer)
    {
        var colData = HavokTreeSearch.FindAll<HKX2.hknpPhysicsSystemData>(rootContainer);

        if (colData.Count > 0)
            return true;

        return false;
    }
}
