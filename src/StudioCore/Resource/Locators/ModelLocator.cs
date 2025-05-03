
using Octokit;
using SoulsFormats.KF4;
using StudioCore.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Resource.Locators;
public static class ModelLocator
{
    public static ResourceDescriptor GetNullAsset()
    {
        ResourceDescriptor ret = new();
        ret.AssetPath = "null";
        ret.AssetName = "null";
        ret.AssetArchiveVirtualPath = "null";
        ret.AssetVirtualPath = "null";
        return ret;
    }

    public static string MapModelNameToAssetName(ProjectEntry project, string mapid, string modelname)
    {
        if (project.ProjectType == ProjectType.DS1 || project.ProjectType == ProjectType.DS1R)
            return $@"{modelname}A{mapid.Substring(1, 2)}";

        if (project.ProjectType == ProjectType.DES)
            return $@"{modelname}";

        if (project.ProjectType == ProjectType.DS2S || project.ProjectType == ProjectType.DS2 || project.ProjectType == ProjectType.ACFA || project.ProjectType == ProjectType.ACV || project.ProjectType == ProjectType.ACVD)
            return modelname;

        return $@"{mapid}_{modelname.Substring(1)}";
    }

    public static ResourceDescriptor GetMapModel(ProjectEntry project, string mapid, string mapContainerId, string modelId)
    {
        ResourceDescriptor ret = new();

        if (project.ProjectType is ProjectType.DS1 or ProjectType.DES)
            ret.AssetPath = LocatorUtils.GetAssetPath(project, $@"map\{mapid}\{modelId}.flver");
        else if (project.ProjectType is ProjectType.DS1R or ProjectType.BB)
            ret.AssetPath = LocatorUtils.GetAssetPath(project, $@"map\{mapid}\{modelId}.flver.dcx");
        else if (project.ProjectType is ProjectType.DS2S or ProjectType.DS2)
            ret.AssetPath = LocatorUtils.GetAssetPath(project, $@"model\map\{mapid}.mapbhd");
        else if (project.ProjectType is ProjectType.ER)
            ret.AssetPath = LocatorUtils.GetAssetPath(project, $@"map\{mapid[..3]}\{mapid}\{modelId}.mapbnd.dcx");
        else if (project.ProjectType is ProjectType.AC6)
            ret.AssetPath = LocatorUtils.GetAssetPath(project, $@"map\{mapid[..3]}\{mapid}\{modelId}.mapbnd.dcx");
        else if (project.ProjectType is ProjectType.ACFA)
            ret.AssetPath = LocatorUtils.GetAssetPath(project, $@"model\map\{mapid}\{mapid}_m.bnd");
        else if (project.ProjectType is ProjectType.ACV or ProjectType.ACVD)
            ret.AssetPath = LocatorUtils.GetAssetPath(project, $@"model\map\{mapid}\{mapid}_m.dcx.bnd");
        else
            ret.AssetPath = LocatorUtils.GetAssetPath(project, $@"map\{mapid}\{modelId}.mapbnd.dcx");

        ret.AssetName = modelId;
        if (project.ProjectType is ProjectType.DS2S or ProjectType.DS2)
        {
            ret.AssetArchiveVirtualPath = $@"map/{mapid}/model";
            ret.AssetVirtualPath = $@"map/{mapid}/model/{modelId}.flv.dcx";
        }
        else if (project.ProjectType is ProjectType.ACFA or ProjectType.ACV or ProjectType.ACVD)
        {
            ret.AssetArchiveVirtualPath = $@"map/{mapid}/model";
            ret.AssetVirtualPath = $@"map/{mapid}/model/{modelId}.flv";
        }
        else
        {
            if (project.ProjectType is not ProjectType.DES
                and not ProjectType.DS1
                and not ProjectType.DS1R
                and not ProjectType.BB)
                ret.AssetArchiveVirtualPath = $@"map/{mapid}/model/{modelId}";

            ret.AssetVirtualPath = $@"map/{mapid}/model/{mapContainerId}/{modelId}.flver";
        }

        return ret;
    }

    public static ResourceDescriptor GetMapCollisionModel(ProjectEntry project, string mapid, string model, bool hi = true)
    {
        ResourceDescriptor ret = new();

        if (project.ProjectType == ProjectType.DS1 || project.ProjectType == ProjectType.DES)
        {
            if (hi)
            {
                ret.AssetPath = LocatorUtils.GetAssetPath(project, $@"map\{mapid}\{model}.hkx");
                ret.AssetName = model;
                ret.AssetVirtualPath = $@"map/{mapid}/hit/hi/{model}.hkx";
            }
            else
            {
                ret.AssetPath = LocatorUtils.GetAssetPath(project, $@"map\{mapid}\l{model.Substring(1)}.hkx");
                ret.AssetName = model;
                ret.AssetVirtualPath = $@"map/{mapid}/hit/lo/l{model.Substring(1)}.hkx";
            }
        }
        else if (project.ProjectType is ProjectType.DS1R)
        {
            if (CFG.Current.PTDE_Collision_Root != "")
            {
                if (Directory.Exists(CFG.Current.PTDE_Collision_Root))
                {
                    if (hi)
                    {
                        ret.AssetPath = LocatorUtils.GetAssetPath_CollisionHack($@"map\{mapid}\{model}.hkx");
                        ret.AssetName = model;
                        ret.AssetVirtualPath = $@"map/{mapid}/hit/hi/{model}.hkx";
                    }
                    else
                    {
                        ret.AssetPath = LocatorUtils.GetAssetPath_CollisionHack($@"map\{mapid}\l{model.Substring(1)}.hkx");
                        ret.AssetName = model;
                        ret.AssetVirtualPath = $@"map/{mapid}/hit/lo/l{model.Substring(1)}.hkx";
                    }
                }
            }
        }
        else if (project.ProjectType == ProjectType.DS2S)
        {
            ret.AssetPath = LocatorUtils.GetAssetPath(project, $@"model\map\h{mapid.Substring(1)}.hkxbhd");
            ret.AssetName = model;
            ret.AssetVirtualPath = $@"map/{mapid}/hit/hi/{model}.hkx.dcx";
            ret.AssetArchiveVirtualPath = $@"map/{mapid}/hit/hi";
        }
        else if (project.ProjectType == ProjectType.DS3 || project.ProjectType == ProjectType.BB)
        {
            if (hi)
            {
                ret.AssetPath = LocatorUtils.GetAssetPath(project, $@"map\{mapid}\h{mapid.Substring(1)}.hkxbhd");
                ret.AssetName = model;
                ret.AssetVirtualPath = $@"map/{mapid}/hit/hi/h{model.Substring(1)}.hkx.dcx";
                ret.AssetArchiveVirtualPath = $@"map/{mapid}/hit/hi";
            }
            else
            {
                ret.AssetPath = LocatorUtils.GetAssetPath(project, $@"map\{mapid}\l{mapid.Substring(1)}.hkxbhd");
                ret.AssetName = model;
                ret.AssetVirtualPath = $@"map/{mapid}/hit/lo/l{model.Substring(1)}.hkx.dcx";
                ret.AssetArchiveVirtualPath = $@"map/{mapid}/hit/lo";
            }
        }
        else if (project.ProjectType == ProjectType.ER)
        {
            if (hi)
            {
                ret.AssetPath = LocatorUtils.GetAssetPath(project, $@"map\{mapid.Substring(0, 3)}\{mapid}\h{mapid.Substring(1)}.hkxbhd");
                ret.AssetName = model;
                ret.AssetVirtualPath = $@"map/{mapid}/hit/hi/h{model.Substring(1)}.hkx.dcx";
                ret.AssetArchiveVirtualPath = $@"map/{mapid}/hit/hi";
            }
            else
            {
                ret.AssetPath = LocatorUtils.GetAssetPath(project, $@"map\{mapid.Substring(0, 3)}\{mapid}\l{mapid.Substring(1)}.hkxbhd");
                ret.AssetName = model;
                ret.AssetVirtualPath = $@"map/{mapid}/hit/lo/l{model.Substring(1)}.hkx.dcx";
                ret.AssetArchiveVirtualPath = $@"map/{mapid}/hit/lo";
            }
        }
        else
        {
            return GetNullAsset();
        }

        return ret;
    }

    public static ResourceDescriptor GetMapNVMModel(ProjectEntry project, string mapid, string model)
    {
        ResourceDescriptor ret = new();

        if (project.ProjectType == ProjectType.DS1 || project.ProjectType == ProjectType.DS1R || project.ProjectType == ProjectType.DES)
        {
            ret.AssetPath = LocatorUtils.GetAssetPath(project, $@"map\{mapid}\{model}.nvm");
            ret.AssetName = model;
            ret.AssetArchiveVirtualPath = $@"map/{mapid}/nav";
            ret.AssetVirtualPath = $@"map/{mapid}/nav/{model}.nvm";
        }
        else
            return GetNullAsset();

        return ret;
    }

    public static ResourceDescriptor GetHavokNavmeshes(ProjectEntry project, string mapid)
    {
        ResourceDescriptor ret = new();
        ret.AssetPath = LocatorUtils.GetAssetPath(project, $@"map\{mapid}\{mapid}.nvmhktbnd.dcx");
        ret.AssetName = mapid;
        ret.AssetArchiveVirtualPath = $@"map/{mapid}/nav";
        return ret;
    }

    public static ResourceDescriptor GetHavokNavmeshModel(ProjectEntry project, string mapid, string model)
    {
        ResourceDescriptor ret = new();
        ret.AssetPath = LocatorUtils.GetAssetPath(project, $@"map\{mapid}\{mapid}.nvmhktbnd.dcx");
        ret.AssetName = model;
        ret.AssetArchiveVirtualPath = $@"map/{mapid}/nav";
        ret.AssetVirtualPath = $@"map/{mapid}/nav/{model}.hkx";

        return ret;
    }

    public static ResourceDescriptor GetChrModel(ProjectEntry project, string chrContainerId, string chrId)
    {
        ResourceDescriptor ret = new();
        ret.AssetName = chrId;
        ret.AssetArchiveVirtualPath = $@"chr/{chrContainerId}/model";

        if (project.ProjectType is ProjectType.DS2S or ProjectType.DS2)
            ret.AssetVirtualPath = $@"chr/{chrContainerId}/model/{chrId}.flv";
        else
            ret.AssetVirtualPath = $@"chr/{chrContainerId}/model/{chrId}.flver";

        // Direct paths
        if (project.ProjectType is ProjectType.DS1)
            ret.AssetPath = LocatorUtils.GetOverridenFilePath(project, $@"chr\{chrId}.chrbnd");
        else if (project.ProjectType is ProjectType.DS2S or ProjectType.DS2)
            ret.AssetPath = LocatorUtils.GetOverridenFilePath(project, $@"model\chr\{chrId}.bnd");
        else if (project.ProjectType is ProjectType.DES)
            ret.AssetPath = LocatorUtils.GetOverridenFilePath(project, $@"chr\{chrContainerId}\{chrId}.chrbnd.dcx");
        else
            ret.AssetPath = LocatorUtils.GetOverridenFilePath(project, $@"chr\{chrId}.chrbnd.dcx");

        return ret;
    }

    public static ResourceDescriptor GetEneModel(ProjectEntry project, string ene)
    {
        ResourceDescriptor ret = new();
        ret.AssetName = ene;
        ret.AssetArchiveVirtualPath = $@"ene/{ene}/model";
        ret.AssetVirtualPath = $@"ene/{ene}/model/{ene}.flv";

        return ret;
    }

    public static ResourceDescriptor GetObjModel(ProjectEntry project, string objContainerId, string objId)
    {
        ResourceDescriptor ret = new();
        ret.AssetName = objId;
        ret.AssetArchiveVirtualPath = $@"obj/{objContainerId}/model";

        if (project.ProjectType is ProjectType.DS2S or ProjectType.DS2 or ProjectType.ACFA or ProjectType.ACV or ProjectType.ACVD)
        {
            ret.AssetVirtualPath = $@"obj/{objContainerId}/model/{objId}.flv";
        }
        else if (project.ProjectType is ProjectType.ER or ProjectType.AC6)
        {
            ret.AssetVirtualPath = $@"obj/{objContainerId}/model/{objId.ToUpper()}.flver";
        }
        else
        {
            ret.AssetVirtualPath = $@"obj/{objContainerId}/model/{objId}.flver";
        }

        // Direct paths
        if (project.ProjectType is ProjectType.DS1)
        {
            ret.AssetPath = LocatorUtils.GetOverridenFilePath(project, $@"obj\{objId}.objbnd");
        }
        else if (project.ProjectType is ProjectType.DS2S or ProjectType.DS2)
        {
            ret.AssetPath = LocatorUtils.GetOverridenFilePath(project, $@"model\obj\{objId}.bnd");
        }
        else if (project.ProjectType is ProjectType.ACFA or ProjectType.ACV or ProjectType.ACVD)
        {
            ret.AssetPath = LocatorUtils.GetOverridenFilePath(project, $@"model\obj\{objId}\{objId}_m.bnd");
        }
        else if (project.ProjectType is ProjectType.ER)
        {
            // Derive subfolder path from model name (all vanilla AEG are within subfolders)
            if (objContainerId.Length >= 6)
            {
                ret.AssetPath = LocatorUtils.GetOverridenFilePath(project, $@"asset\aeg\{objContainerId.Substring(0, 6)}\{objId}.geombnd.dcx");
            }
        }
        else if (project.ProjectType is ProjectType.AC6)
        {
            if (objContainerId.Length >= 6)
                ret.AssetPath = LocatorUtils.GetOverridenFilePath(project, $@"asset\environment\geometry\{objId}.geombnd.dcx");
        }
        else
        {
            ret.AssetPath = LocatorUtils.GetOverridenFilePath(project, $@"obj\{objId}.objbnd.dcx");
        }

        return ret;
    }

    public static ResourceDescriptor GetPartsModel(ProjectEntry project, string partContainerId, string partId)
    {
        ResourceDescriptor ret = new();
        ret.AssetName = partId;
        ret.AssetArchiveVirtualPath = $@"parts/{partContainerId}/model";

        if (project.ProjectType == ProjectType.DS2S || project.ProjectType == ProjectType.DS2)
        {
            ret.AssetVirtualPath = $@"parts/{partContainerId}/model/{partId}.flv";
        }
        else if (project.ProjectType is ProjectType.DS1)
        {
            ret.AssetVirtualPath = $@"parts/{partContainerId}/model/{partId.ToUpper()}.flver";
        }
        else
        {
            ret.AssetVirtualPath = $@"parts/{partContainerId}/model/{partId}.flver";
        }

        // Direct paths
        if (project.ProjectType == ProjectType.DS1)
        {
            ret.AssetPath = LocatorUtils.GetOverridenFilePath(project, $@"parts\{partId}.partsbnd");
        }
        else if (project.ProjectType == ProjectType.DS2S || project.ProjectType == ProjectType.DS2)
        {
            var partType = "";
            switch (partId.Substring(0, 2))
            {
                case "as":
                    partType = "accessories";
                    break;
                case "am":
                    partType = "arm";
                    break;
                case "bd":
                    partType = "body";
                    break;
                case "fa":
                case "fc":
                case "fg":
                    partType = "face";
                    break;
                case "hd":
                    partType = "head";
                    break;
                case "leg":
                    partType = "leg";
                    break;
                case "sd":
                    partType = "shield";
                    break;
                case "wp":
                    partType = "weapon";
                    break;
            }

            ret.AssetPath = LocatorUtils.GetOverridenFilePath(project, $@"model\parts\{partType}\{partId}.bnd");
        }
        else
        {
            ret.AssetPath = LocatorUtils.GetOverridenFilePath(project, $@"parts\{partId}.partsbnd.dcx");
        }

        return ret;
    }

}
