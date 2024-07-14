
using SoulsFormats.KF4;
using StudioCore.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Locators;
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

    public static string MapModelNameToAssetName(string mapid, string modelname)
    {
        if (Smithbox.ProjectType == ProjectType.DS1 || Smithbox.ProjectType == ProjectType.DS1R)
            return $@"{modelname}A{mapid.Substring(1, 2)}";

        if (Smithbox.ProjectType == ProjectType.DES)
            return $@"{modelname}";

        if (Smithbox.ProjectType == ProjectType.DS2S || Smithbox.ProjectType == ProjectType.DS2)
            return modelname;

        return $@"{mapid}_{modelname.Substring(1)}";
    }



    public static ResourceDescriptor GetMapModel(string mapid, string model)
    {
        ResourceDescriptor ret = new();
        if (Smithbox.ProjectType == ProjectType.DS1 || Smithbox.ProjectType == ProjectType.BB || Smithbox.ProjectType == ProjectType.DES)
            ret.AssetPath = LocatorUtils.GetAssetPath($@"map\{mapid}\{model}.flver");
        else if (Smithbox.ProjectType == ProjectType.DS1R)
            ret.AssetPath = LocatorUtils.GetAssetPath($@"map\{mapid}\{model}.flver.dcx");
        else if (Smithbox.ProjectType == ProjectType.DS2S || Smithbox.ProjectType == ProjectType.DS2)
            ret.AssetPath = LocatorUtils.GetAssetPath($@"model\map\{mapid}.mapbhd");
        else if (Smithbox.ProjectType == ProjectType.ER)
            ret.AssetPath = LocatorUtils.GetAssetPath($@"map\{mapid[..3]}\{mapid}\{model}.mapbnd.dcx");
        else if (Smithbox.ProjectType == ProjectType.AC6)
            ret.AssetPath = LocatorUtils.GetAssetPath($@"map\{mapid[..3]}\{mapid}\{model}.mapbnd.dcx");
        else
            ret.AssetPath = LocatorUtils.GetAssetPath($@"map\{mapid}\{model}.mapbnd.dcx");

        ret.AssetName = model;
        if (Smithbox.ProjectType == ProjectType.DS2S || Smithbox.ProjectType == ProjectType.DS2)
        {
            ret.AssetArchiveVirtualPath = $@"map/{mapid}/model/";
            ret.AssetVirtualPath = $@"map/{mapid}/model/{model}.flv.dcx";
        }
        else
        {
            if (Smithbox.ProjectType is not ProjectType.DES
                and not ProjectType.DS1
                and not ProjectType.DS1R
                and not ProjectType.BB)
                ret.AssetArchiveVirtualPath = $@"map/{mapid}/model/{model}";

            ret.AssetVirtualPath = $@"map/{mapid}/model/{model}/{model}.flver";
        }

        return ret;
    }

    public static ResourceDescriptor GetMapCollisionModel(string mapid, string model, bool hi = true)
    {
        ResourceDescriptor ret = new();
        if (Smithbox.ProjectType == ProjectType.DS1 || Smithbox.ProjectType == ProjectType.DES)
        {
            if (hi)
            {
                ret.AssetPath = LocatorUtils.GetAssetPath($@"map\{mapid}\{model}.hkx");
                ret.AssetName = model;
                ret.AssetVirtualPath = $@"map/{mapid}/hit/hi/{model}.hkx";
            }
            else
            {
                ret.AssetPath = LocatorUtils.GetAssetPath($@"map\{mapid}\l{model.Substring(1)}.hkx");
                ret.AssetName = model;
                ret.AssetVirtualPath = $@"map/{mapid}/hit/lo/l{model.Substring(1)}.hkx";
            }
        }
        else if (Smithbox.ProjectType == ProjectType.DS2S)
        {
            ret.AssetPath = LocatorUtils.GetAssetPath($@"model\map\h{mapid.Substring(1)}.hkxbhd");
            ret.AssetName = model;
            ret.AssetVirtualPath = $@"map/{mapid}/hit/hi/{model}.hkx.dcx";
            ret.AssetArchiveVirtualPath = $@"map/{mapid}/hit/hi";
        }
        else if (Smithbox.ProjectType == ProjectType.DS3 || Smithbox.ProjectType == ProjectType.BB)
        {
            if (hi)
            {
                ret.AssetPath = LocatorUtils.GetAssetPath($@"map\{mapid}\h{mapid.Substring(1)}.hkxbhd");
                ret.AssetName = model;
                ret.AssetVirtualPath = $@"map/{mapid}/hit/hi/h{model.Substring(1)}.hkx.dcx";
                ret.AssetArchiveVirtualPath = $@"map/{mapid}/hit/hi";
            }
            else
            {
                ret.AssetPath = LocatorUtils.GetAssetPath($@"map\{mapid}\l{mapid.Substring(1)}.hkxbhd");
                ret.AssetName = model;
                ret.AssetVirtualPath = $@"map/{mapid}/hit/lo/l{model.Substring(1)}.hkx.dcx";
                ret.AssetArchiveVirtualPath = $@"map/{mapid}/hit/lo";
            }
        }
        else if (Smithbox.ProjectType == ProjectType.ER)
        {
            if (hi)
            {
                ret.AssetPath = LocatorUtils.GetAssetPath($@"map\{mapid.Substring(0, 3)}\{mapid}\h{mapid.Substring(1)}.hkxbhd");
                ret.AssetName = model;
                ret.AssetVirtualPath = $@"map/{mapid}/hit/hi/h{model.Substring(1)}.hkx.dcx";
                ret.AssetArchiveVirtualPath = $@"map/{mapid}/hit/hi";
            }
            else
            {
                ret.AssetPath = LocatorUtils.GetAssetPath($@"map\{mapid.Substring(0,3)}\{mapid}\l{mapid.Substring(1)}.hkxbhd");
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

    public static ResourceDescriptor GetMapNVMModel(string mapid, string model)
    {
        ResourceDescriptor ret = new();
        if (Smithbox.ProjectType == ProjectType.DS1 || Smithbox.ProjectType == ProjectType.DS1R || Smithbox.ProjectType == ProjectType.DES)
        {
            ret.AssetPath = LocatorUtils.GetAssetPath($@"map\{mapid}\{model}.nvm");
            ret.AssetName = model;
            ret.AssetArchiveVirtualPath = $@"map/{mapid}/nav";
            ret.AssetVirtualPath = $@"map/{mapid}/nav/{model}.nvm";
        }
        else
            return GetNullAsset();

        return ret;
    }

    public static ResourceDescriptor GetHavokNavmeshes(string mapid)
    {
        ResourceDescriptor ret = new();
        ret.AssetPath = LocatorUtils.GetAssetPath($@"map\{mapid}\{mapid}.nvmhktbnd.dcx");
        ret.AssetName = mapid;
        ret.AssetArchiveVirtualPath = $@"map/{mapid}/nav";
        return ret;
    }

    public static ResourceDescriptor GetHavokNavmeshModel(string mapid, string model)
    {
        ResourceDescriptor ret = new();
        ret.AssetPath = LocatorUtils.GetAssetPath($@"map\{mapid}\{mapid}.nvmhktbnd.dcx");
        ret.AssetName = model;
        ret.AssetArchiveVirtualPath = $@"map/{mapid}/nav";
        ret.AssetVirtualPath = $@"map/{mapid}/nav/{model}.hkx";

        return ret;
    }

    public static ResourceDescriptor GetChrModel(string chr)
    {
        ResourceDescriptor ret = new();
        ret.AssetName = chr;
        ret.AssetArchiveVirtualPath = $@"chr/{chr}/model";
        if (Smithbox.ProjectType == ProjectType.DS2S || Smithbox.ProjectType == ProjectType.DS2)
            ret.AssetVirtualPath = $@"chr/{chr}/model/{chr}.flv";
        else
            ret.AssetVirtualPath = $@"chr/{chr}/model/{chr}.flver";

        // Direct paths
        if (Smithbox.ProjectType == ProjectType.DS1)
            ret.AssetPath = LocatorUtils.GetOverridenFilePath($@"chr\{chr}.chrbnd");
        else if (Smithbox.ProjectType == ProjectType.DS2S || Smithbox.ProjectType == ProjectType.DS2)
            ret.AssetPath = LocatorUtils.GetOverridenFilePath($@"model\chr\{chr}.bnd");
        else if(Smithbox.ProjectType == ProjectType.DES)
            ret.AssetPath = LocatorUtils.GetOverridenFilePath($@"chr\{chr}\{chr}.chrbnd.dcx");
        else
            ret.AssetPath = LocatorUtils.GetOverridenFilePath($@"chr\{chr}.chrbnd.dcx");

        return ret;
    }

    public static ResourceDescriptor GetObjModel(string obj)
    {
        ResourceDescriptor ret = new();
        ret.AssetName = obj;
        ret.AssetArchiveVirtualPath = $@"obj/{obj}/model";

        if (Smithbox.ProjectType == ProjectType.DS2S || Smithbox.ProjectType == ProjectType.DS2)
            ret.AssetVirtualPath = $@"obj/{obj}/model/{obj}.flv";
        else if (Smithbox.ProjectType is ProjectType.ER or ProjectType.AC6)
            ret.AssetVirtualPath = $@"obj/{obj}/model/{obj.ToUpper()}.flver";
        else
            ret.AssetVirtualPath = $@"obj/{obj}/model/{obj}.flver";

        // Direct paths
        if (Smithbox.ProjectType == ProjectType.DS1)
            ret.AssetPath = LocatorUtils.GetOverridenFilePath($@"obj\{obj}.objbnd");
        else if (Smithbox.ProjectType == ProjectType.DS2S || Smithbox.ProjectType == ProjectType.DS2)
            ret.AssetPath = LocatorUtils.GetOverridenFilePath($@"model\obj\{obj}.bnd");
        else if(Smithbox.ProjectType == ProjectType.ER)
        {
            // Derive subfolder path from model name (all vanilla AEG are within subfolders)
            if (obj.Length >= 6)
            {
                ret.AssetPath = LocatorUtils.GetOverridenFilePath($@"asset\aeg\{obj.Substring(0, 6)}\{obj}.geombnd.dcx");
            }
        }
        else if(Smithbox.ProjectType == ProjectType.AC6)
        {
            if (obj.Length >= 6)
                ret.AssetPath = LocatorUtils.GetOverridenFilePath($@"asset\environment\geometry\{obj}.geombnd.dcx");
        }
        else
        {
            ret.AssetPath = LocatorUtils.GetOverridenFilePath($@"obj\{obj}.objbnd.dcx");
        }

        return ret;
    }

    public static ResourceDescriptor GetPartsModel(string part)
    {
        ResourceDescriptor ret = new();
        ret.AssetName = part;
        ret.AssetArchiveVirtualPath = $@"parts/{part}/model";

        if (Smithbox.ProjectType == ProjectType.DS2S || Smithbox.ProjectType == ProjectType.DS2)
        {
            ret.AssetVirtualPath = $@"parts/{part}/model/{part}.flv";
        }
        else if (Smithbox.ProjectType is ProjectType.DS1)
        {
            ret.AssetVirtualPath = $@"parts/{part}/model/{part.ToUpper()}.flver";
        }
        else
        {
            ret.AssetVirtualPath = $@"parts/{part}/model/{part}.flver";
        }

        // Direct paths
        if (Smithbox.ProjectType == ProjectType.DS1)
        {
            ret.AssetPath = LocatorUtils.GetOverridenFilePath($@"parts\{part}.partsbnd");
        }
        else if (Smithbox.ProjectType == ProjectType.DS2S || Smithbox.ProjectType == ProjectType.DS2)
        {
            var partType = "";
            switch (part.Substring(0, 2))
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

            ret.AssetPath = LocatorUtils.GetOverridenFilePath($@"model\parts\{partType}\{part}.bnd");
        }
        else
        {
            ret.AssetPath = LocatorUtils.GetOverridenFilePath($@"parts\{part}.partsbnd.dcx");
        }

        return ret;
    }

}
