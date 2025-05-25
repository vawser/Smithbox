using Octokit;
using StudioCore.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Resource.Locators;

// Locators for assets used with direct operations (e.g. not in the Resource Manager)
public static class AssetLocator
{
    public static ResourceDescriptor GetCharacterBinder(ProjectEntry project, string chrId, string postfix = "")
    {
        ResourceDescriptor ret = new();

        if (project.ProjectType == ProjectType.DS1)
            ret.AssetPath = LocatorUtils.GetOverridenFilePath(project, $@"chr\{chrId}{postfix}.chrbnd");
        else if (project.ProjectType == ProjectType.DS2S || project.ProjectType == ProjectType.DS2)
            ret.AssetPath = LocatorUtils.GetOverridenFilePath(project, $@"model\chr\{chrId}{postfix}.bnd");
        else if (project.ProjectType == ProjectType.DES)
            ret.AssetPath = LocatorUtils.GetOverridenFilePath(project, $@"chr\{chrId}\{chrId}{postfix}.chrbnd.dcx");
        else
            ret.AssetPath = LocatorUtils.GetOverridenFilePath(project, $@"chr\{chrId}{postfix}.chrbnd.dcx");

        return ret;
    }

    public static ResourceDescriptor GetCharacterAnimationBinder(ProjectEntry project, string chrId, string postfix = "")
    {
        ResourceDescriptor ret = new();

        if (project.ProjectType == ProjectType.DS1)
            ret.AssetPath = LocatorUtils.GetOverridenFilePath(project, $@"chr\{chrId}{postfix}.anibnd");
        else if (project.ProjectType == ProjectType.DS2S || project.ProjectType == ProjectType.DS2)
            ret.AssetPath = LocatorUtils.GetOverridenFilePath(project, $@"model\chr\{chrId}{postfix}.bnd");
        else if (project.ProjectType == ProjectType.DES)
            ret.AssetPath = LocatorUtils.GetOverridenFilePath(project, $@"chr\{chrId}\{chrId}{postfix}.anibnd.dcx");
        else
            ret.AssetPath = LocatorUtils.GetOverridenFilePath(project, $@"chr\{chrId}{postfix}.anibnd.dcx");

        return ret;
    }

    public static ResourceDescriptor GetCharacterBehaviorBinder(ProjectEntry project, string chrId, string postfix = "")
    {
        ResourceDescriptor ret = new();

        if (project.ProjectType == ProjectType.DS1)
            ret.AssetPath = null;
        else if (project.ProjectType == ProjectType.DS2S || project.ProjectType == ProjectType.DS2)
            ret.AssetPath = null;
        else if (project.ProjectType == ProjectType.DES)
            ret.AssetPath = null;
        else
            ret.AssetPath = LocatorUtils.GetOverridenFilePath(project, $@"chr\{chrId}{postfix}.behbnd.dcx");

        return ret;
    }

    public static ResourceDescriptor GetCharacterTextureBinder(ProjectEntry project, string chrId, string postfix = "")
    {
        ResourceDescriptor ret = new();

        if (project.ProjectType == ProjectType.DS1)
            ret.AssetPath = null;
        else if (project.ProjectType == ProjectType.DS2S || project.ProjectType == ProjectType.DS2)
            ret.AssetPath = null;
        else if (project.ProjectType == ProjectType.DES)
            ret.AssetPath = null;
        else
            ret.AssetPath = LocatorUtils.GetOverridenFilePath(project, $@"chr\{chrId}{postfix}.texbnd.dcx");

        return ret;
    }

    public static ResourceDescriptor GetAssetGeomBinder(ProjectEntry project, string asset)
    {
        ResourceDescriptor ret = new();

        if (project.ProjectType == ProjectType.ER)
        {
            if (asset.Length >= 6)
            {
                ret.AssetPath = LocatorUtils.GetOverridenFilePath(project, $@"asset\aeg\{asset.Substring(0, 6)}\{asset}.geombnd.dcx");
            }
        }
        else if (project.ProjectType == ProjectType.AC6)
        {
            if (asset.Length >= 6)
                ret.AssetPath = LocatorUtils.GetOverridenFilePath(project, $@"asset\environment\geometry\{asset}.geombnd.dcx");
        }

        return ret;
    }

    public static ResourceDescriptor GetAssetGeomHKXBinder(ProjectEntry project, string asset, string postfix = "")
    {
        ResourceDescriptor ret = new();

        ret.AssetVirtualPath = $"obj/{asset}/collision/{asset}_{postfix}.hkx";

        if (project.ProjectType == ProjectType.ER)
        {
            if (asset.Length >= 6)
            {
                ret.AssetPath = LocatorUtils.GetOverridenFilePath(project, $@"asset\aeg\{asset.Substring(0, 6)}\{asset}_{postfix}.geomhkxbnd.dcx");
            }
        }
        else if (project.ProjectType == ProjectType.AC6)
        {
            if (asset.Length >= 6)
                ret.AssetPath = LocatorUtils.GetOverridenFilePath(project, $@"asset\environment\geometry\{asset}_{postfix}.geomhkxbnd.dcx");
        }

        return ret;
    }

    public static ResourceDescriptor GetPartBinder(ProjectEntry project, string part, string postfix = "")
    {
        ResourceDescriptor ret = new();

        ret.AssetPath = LocatorUtils.GetOverridenFilePath(project, $@"parts\{part}{postfix}.partsbnd.dcx");

        return ret;
    }

    public static ResourceDescriptor GetPartTpf(ProjectEntry project, string part, string postfix = "")
    {
        ResourceDescriptor ret = new();

        ret.AssetPath = LocatorUtils.GetOverridenFilePath(project, $@"parts\{part}{postfix}.tpf.dcx");

        return ret;
    }

    public static ResourceDescriptor GetMapPiece(ProjectEntry project, string dir, string name)
    {
        ResourceDescriptor ret = new();

        ret.AssetPath = LocatorUtils.GetOverridenFilePath(project, $@"map\{dir}\{name}.partsbnd.dcx");

        return ret;
    }
}
