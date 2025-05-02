using StudioCore.Core.Project;
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
    public static ResourceDescriptor GetCharacterBinder(string chrId, string postfix = "")
    {
        ResourceDescriptor ret = new();

        if (Smithbox.ProjectType == ProjectType.DS1)
            ret.AssetPath = LocatorUtils.GetOverridenFilePath($@"chr\{chrId}{postfix}.chrbnd");
        else if (Smithbox.ProjectType == ProjectType.DS2S || Smithbox.ProjectType == ProjectType.DS2)
            ret.AssetPath = LocatorUtils.GetOverridenFilePath($@"model\chr\{chrId}{postfix}.bnd");
        else if (Smithbox.ProjectType == ProjectType.DES)
            ret.AssetPath = LocatorUtils.GetOverridenFilePath($@"chr\{chrId}\{chrId}{postfix}.chrbnd.dcx");
        else
            ret.AssetPath = LocatorUtils.GetOverridenFilePath($@"chr\{chrId}{postfix}.chrbnd.dcx");

        return ret;
    }

    public static ResourceDescriptor GetCharacterAnimationBinder(string chrId, string postfix = "")
    {
        ResourceDescriptor ret = new();

        if (Smithbox.ProjectType == ProjectType.DS1)
            ret.AssetPath = LocatorUtils.GetOverridenFilePath($@"chr\{chrId}{postfix}.anibnd");
        else if (Smithbox.ProjectType == ProjectType.DS2S || Smithbox.ProjectType == ProjectType.DS2)
            ret.AssetPath = LocatorUtils.GetOverridenFilePath($@"model\chr\{chrId}{postfix}.bnd");
        else if (Smithbox.ProjectType == ProjectType.DES)
            ret.AssetPath = LocatorUtils.GetOverridenFilePath($@"chr\{chrId}\{chrId}{postfix}.anibnd.dcx");
        else
            ret.AssetPath = LocatorUtils.GetOverridenFilePath($@"chr\{chrId}{postfix}.anibnd.dcx");

        return ret;
    }

    public static ResourceDescriptor GetCharacterBehaviorBinder(string chrId, string postfix = "")
    {
        ResourceDescriptor ret = new();

        if (Smithbox.ProjectType == ProjectType.DS1)
            ret.AssetPath = null;
        else if (Smithbox.ProjectType == ProjectType.DS2S || Smithbox.ProjectType == ProjectType.DS2)
            ret.AssetPath = null;
        else if (Smithbox.ProjectType == ProjectType.DES)
            ret.AssetPath = null;
        else
            ret.AssetPath = LocatorUtils.GetOverridenFilePath($@"chr\{chrId}{postfix}.behbnd.dcx");

        return ret;
    }

    public static ResourceDescriptor GetCharacterTextureBinder(string chrId, string postfix = "")
    {
        ResourceDescriptor ret = new();

        if (Smithbox.ProjectType == ProjectType.DS1)
            ret.AssetPath = null;
        else if (Smithbox.ProjectType == ProjectType.DS2S || Smithbox.ProjectType == ProjectType.DS2)
            ret.AssetPath = null;
        else if (Smithbox.ProjectType == ProjectType.DES)
            ret.AssetPath = null;
        else
            ret.AssetPath = LocatorUtils.GetOverridenFilePath($@"chr\{chrId}{postfix}.texbnd.dcx");

        return ret;
    }

    public static ResourceDescriptor GetAssetGeomBinder(string asset)
    {
        ResourceDescriptor ret = new();

        if (Smithbox.ProjectType == ProjectType.ER)
        {
            if (asset.Length >= 6)
            {
                ret.AssetPath = LocatorUtils.GetOverridenFilePath($@"asset\aeg\{asset.Substring(0, 6)}\{asset}.geombnd.dcx");
            }
        }
        else if (Smithbox.ProjectType == ProjectType.AC6)
        {
            if (asset.Length >= 6)
                ret.AssetPath = LocatorUtils.GetOverridenFilePath($@"asset\environment\geometry\{asset}.geombnd.dcx");
        }

        return ret;
    }

    public static ResourceDescriptor GetAssetGeomHKXBinder(string asset, string postfix = "")
    {
        ResourceDescriptor ret = new();

        ret.AssetVirtualPath = $"obj/{asset}/collision/{asset}_{postfix}.hkx";

        if (Smithbox.ProjectType == ProjectType.ER)
        {
            if (asset.Length >= 6)
            {
                ret.AssetPath = LocatorUtils.GetOverridenFilePath($@"asset\aeg\{asset.Substring(0, 6)}\{asset}_{postfix}.geomhkxbnd.dcx");
            }
        }
        else if (Smithbox.ProjectType == ProjectType.AC6)
        {
            if (asset.Length >= 6)
                ret.AssetPath = LocatorUtils.GetOverridenFilePath($@"asset\environment\geometry\{asset}_{postfix}.geomhkxbnd.dcx");
        }

        return ret;
    }

    public static ResourceDescriptor GetPartBinder(string part, string postfix = "")
    {
        ResourceDescriptor ret = new();

        ret.AssetPath = LocatorUtils.GetOverridenFilePath($@"parts\{part}{postfix}.partsbnd.dcx");

        return ret;
    }

    public static ResourceDescriptor GetPartTpf(string part, string postfix = "")
    {
        ResourceDescriptor ret = new();

        ret.AssetPath = LocatorUtils.GetOverridenFilePath($@"parts\{part}{postfix}.tpf.dcx");

        return ret;
    }

    public static ResourceDescriptor GetMapPiece(string dir, string name)
    {
        ResourceDescriptor ret = new();

        ret.AssetPath = LocatorUtils.GetOverridenFilePath($@"map\{dir}\{name}.partsbnd.dcx");

        return ret;
    }
}
