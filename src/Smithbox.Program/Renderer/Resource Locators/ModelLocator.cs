using Octokit;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System.IO;

namespace StudioCore.Renderer;

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
        if (project.ProjectType is ProjectType.DES)
            return $@"{modelname}";

        if (project.ProjectType is ProjectType.DS1 or ProjectType.DS1R)
            return $@"{modelname}A{mapid.Substring(1, 2)}";

        if (project.ProjectType is ProjectType.DS2S or ProjectType.DS2)
            return modelname;

        return $@"{mapid}_{modelname.Substring(1)}";
    }

    public static string GetMapModelName(ProjectEntry project, string mapid, string modelname)
    {
        if (project.ProjectType is ProjectType.DES)
            return $@"{modelname}";

        if (project.ProjectType is ProjectType.DS1 or ProjectType.DS1R)
            return $@"{modelname}A{mapid.Substring(1, 2)}";

        if (project.ProjectType is ProjectType.DS2S or ProjectType.DS2)
            return modelname;

        return $@"m{modelname.Substring(1)}";
    }

    public static ResourceDescriptor GetMapModel(ProjectEntry project, string mapid, string mapContainerId, string modelId)
    {
        ResourceDescriptor ret = new();

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

    public static ResourceDescriptor GetMapCollisionModel(ProjectEntry project, string mapid, string model, bool isConnectCollision = false)
    {
        ResourceDescriptor ret = new();

        var colType = "hit";
        if (isConnectCollision)
            colType = "connect";

        var targetType = HavokCollisionType.Low;

        if (project.MapEditor != null)
        {
            targetType = CFG.Current.CurrentHavokCollisionType;
        }

        // Always use low for connect collisions
        if(isConnectCollision)
        {
            targetType = HavokCollisionType.Low;
        }

        if (project.ProjectType is ProjectType.DS1 or ProjectType.DS1R or ProjectType.DES)
        {
            if (targetType is HavokCollisionType.High)
            {
                ret.AssetName = model;
                ret.AssetVirtualPath = $@"map/{mapid}/{colType}/hi/h{model}.hkx";
            }
            else if (targetType is HavokCollisionType.Low)
            {
                ret.AssetName = model;
                ret.AssetVirtualPath = $@"map/{mapid}/{colType}/lo/l{model.Substring(1)}.hkx";
            }
        }
        else if (project.ProjectType == ProjectType.DS2S)
        {
            ret.AssetName = model;
            ret.AssetVirtualPath = $@"map/{mapid}/{colType}/hi/{model}.hkx.dcx";
            ret.AssetArchiveVirtualPath = $@"map/{mapid}/{colType}/hi";
        }
        else if (project.ProjectType == ProjectType.DS3 || project.ProjectType == ProjectType.BB)
        {
            if (targetType is HavokCollisionType.High)
            {
                ret.AssetName = model;
                ret.AssetVirtualPath = $@"map/{mapid}/{colType}/hi/h{model.Substring(1)}.hkx.dcx";
                ret.AssetArchiveVirtualPath = $@"map/{mapid}/{colType}/hi";
            }
            else if (targetType is HavokCollisionType.Low)
            {
                ret.AssetName = model;
                ret.AssetVirtualPath = $@"map/{mapid}/{colType}/lo/l{model.Substring(1)}.hkx.dcx";
                ret.AssetArchiveVirtualPath = $@"map/{mapid}/{colType}/lo";
            }
        }
        else if (project.ProjectType is ProjectType.ER or ProjectType.NR)
        {
            if (targetType is HavokCollisionType.High)
            {
                ret.AssetName = model;
                ret.AssetVirtualPath = $@"map/{mapid}/{colType}/hi/h{model.Substring(1)}.hkx.dcx";
                ret.AssetArchiveVirtualPath = $@"map/{mapid}/{colType}/hi";
            }
            else if (targetType is HavokCollisionType.Low)
            {
                ret.AssetName = model;
                ret.AssetVirtualPath = $@"map/{mapid}/{colType}/lo/l{model.Substring(1)}.hkx.dcx";
                ret.AssetArchiveVirtualPath = $@"map/{mapid}/{colType}/lo";
            }
            else if (targetType is HavokCollisionType.FallProtection)
            {
                ret.AssetName = model;
                ret.AssetVirtualPath = $@"map/{mapid}/{colType}/fa/f{model.Substring(1)}.hkx.dcx";
                ret.AssetArchiveVirtualPath = $@"map/{mapid}/{colType}/fa";
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

        if (project.ProjectType is ProjectType.DS1 or ProjectType.DS1R or ProjectType.DES)
        {
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

        ret.AssetName = mapid;
        ret.AssetArchiveVirtualPath = $@"map/{mapid}/nav";

        return ret;
    }

    public static ResourceDescriptor GetHavokNavmeshModel(ProjectEntry project, string mapid, string model)
    {
        ResourceDescriptor ret = new();

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
        else if (project.ProjectType is ProjectType.ER or ProjectType.AC6 or ProjectType.NR)
        {
            ret.AssetVirtualPath = $@"obj/{objContainerId}/model/{objId.ToUpper()}.flver";
        }
        else
        {
            ret.AssetVirtualPath = $@"obj/{objContainerId}/model/{objId}.flver";
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

        return ret;
    }

}
