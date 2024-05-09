
using SoulsFormats.KF4;
using StudioCore.UserProject;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Locators;
public static class ResourceModelLocator
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
        if (Project.Type == ProjectType.DS1 || Project.Type == ProjectType.DS1R)
            return $@"{modelname}A{mapid.Substring(1, 2)}";

        if (Project.Type == ProjectType.DES)
            return $@"{modelname}";

        if (Project.Type == ProjectType.DS2S || Project.Type == ProjectType.DS2)
            return modelname;

        return $@"{mapid}_{modelname.Substring(1)}";
    }

    public static ResourceDescriptor GetMapModel(string mapid, string model)
    {
        ResourceDescriptor ret = new();
        if (Project.Type == ProjectType.DS1 || Project.Type == ProjectType.BB || Project.Type == ProjectType.DES)
            ret.AssetPath = ResourceLocatorUtils.GetAssetPath($@"map\{mapid}\{model}.flver");
        else if (Project.Type == ProjectType.DS1R)
            ret.AssetPath = ResourceLocatorUtils.GetAssetPath($@"map\{mapid}\{model}.flver.dcx");
        else if (Project.Type == ProjectType.DS2S || Project.Type == ProjectType.DS2)
            ret.AssetPath = ResourceLocatorUtils.GetAssetPath($@"model\map\{mapid}.mapbhd");
        else if (Project.Type == ProjectType.ER)
            ret.AssetPath = ResourceLocatorUtils.GetAssetPath($@"map\{mapid[..3]}\{mapid}\{model}.mapbnd.dcx");
        else if (Project.Type == ProjectType.AC6)
            ret.AssetPath = ResourceLocatorUtils.GetAssetPath($@"map\{mapid[..3]}\{mapid}\{model}.mapbnd.dcx");
        else
            ret.AssetPath = ResourceLocatorUtils.GetAssetPath($@"map\{mapid}\{model}.mapbnd.dcx");

        ret.AssetName = model;
        if (Project.Type == ProjectType.DS2S || Project.Type == ProjectType.DS2)
        {
            ret.AssetArchiveVirtualPath = $@"map/{mapid}/model/";
            ret.AssetVirtualPath = $@"map/{mapid}/model/{model}.flv.dcx";
        }
        else
        {
            if (Project.Type is not ProjectType.DES
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
        if (Project.Type == ProjectType.DS1 || Project.Type == ProjectType.DES)
        {
            if (hi)
            {
                ret.AssetPath = ResourceLocatorUtils.GetAssetPath($@"map\{mapid}\{model}.hkx");
                ret.AssetName = model;
                ret.AssetVirtualPath = $@"map/{mapid}/hit/hi/{model}.hkx";
            }
            else
            {
                ret.AssetPath = ResourceLocatorUtils.GetAssetPath($@"map\{mapid}\l{model.Substring(1)}.hkx");
                ret.AssetName = model;
                ret.AssetVirtualPath = $@"map/{mapid}/hit/lo/l{model.Substring(1)}.hkx";
            }
        }
        else if (Project.Type == ProjectType.DS2S)
        {
            ret.AssetPath = ResourceLocatorUtils.GetAssetPath($@"model\map\h{mapid.Substring(1)}.hkxbhd");
            ret.AssetName = model;
            ret.AssetVirtualPath = $@"map/{mapid}/hit/hi/{model}.hkx.dcx";
            ret.AssetArchiveVirtualPath = $@"map/{mapid}/hit/hi";
        }
        else if (Project.Type == ProjectType.DS3 || Project.Type == ProjectType.BB)
        {
            if (hi)
            {
                ret.AssetPath = ResourceLocatorUtils.GetAssetPath($@"map\{mapid}\h{mapid.Substring(1)}.hkxbhd");
                ret.AssetName = model;
                ret.AssetVirtualPath = $@"map/{mapid}/hit/hi/h{model.Substring(1)}.hkx.dcx";
                ret.AssetArchiveVirtualPath = $@"map/{mapid}/hit/hi";
            }
            else
            {
                ret.AssetPath = ResourceLocatorUtils.GetAssetPath($@"map\{mapid}\l{mapid.Substring(1)}.hkxbhd");
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
        if (Project.Type == ProjectType.DS1 || Project.Type == ProjectType.DS1R || Project.Type == ProjectType.DES)
        {
            ret.AssetPath = ResourceLocatorUtils.GetAssetPath($@"map\{mapid}\{model}.nvm");
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
        ret.AssetPath = ResourceLocatorUtils.GetAssetPath($@"map\{mapid}\{mapid}.nvmhktbnd.dcx");
        ret.AssetName = mapid;
        ret.AssetArchiveVirtualPath = $@"map/{mapid}/nav";
        return ret;
    }

    public static ResourceDescriptor GetHavokNavmeshModel(string mapid, string model)
    {
        ResourceDescriptor ret = new();
        ret.AssetPath = ResourceLocatorUtils.GetAssetPath($@"map\{mapid}\{mapid}.nvmhktbnd.dcx");
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
        if (Project.Type == ProjectType.DS2S || Project.Type == ProjectType.DS2)
            ret.AssetVirtualPath = $@"chr/{chr}/model/{chr}.flv";
        else
            ret.AssetVirtualPath = $@"chr/{chr}/model/{chr}.flver";

        return ret;
    }

    public static ResourceDescriptor GetObjModel(string obj)
    {
        ResourceDescriptor ret = new();
        ret.AssetName = obj;
        ret.AssetArchiveVirtualPath = $@"obj/{obj}/model";

        if (Project.Type == ProjectType.DS2S || Project.Type == ProjectType.DS2)
            ret.AssetVirtualPath = $@"obj/{obj}/model/{obj}.flv";
        else if (Project.Type is ProjectType.ER or ProjectType.AC6)
            ret.AssetVirtualPath = $@"obj/{obj}/model/{obj.ToUpper()}.flver";
        else
            ret.AssetVirtualPath = $@"obj/{obj}/model/{obj}.flver";

        return ret;
    }

    public static ResourceDescriptor GetPartsModel(string part)
    {
        ResourceDescriptor ret = new();
        ret.AssetName = part;
        ret.AssetArchiveVirtualPath = $@"parts/{part}/model";

        if (Project.Type == ProjectType.DS2S || Project.Type == ProjectType.DS2)
        {
            ret.AssetVirtualPath = $@"parts/{part}/model/{part}.flv";
        }
        else if (Project.Type is ProjectType.DS1)
        {
            ret.AssetVirtualPath = $@"parts/{part}/model/{part.ToUpper()}.flver";
        }
        else
        {
            ret.AssetVirtualPath = $@"parts/{part}/model/{part}.flver";
        }
        return ret;
    }
}
