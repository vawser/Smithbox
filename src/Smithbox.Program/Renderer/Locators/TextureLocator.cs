using SoulsFormats;
using StudioCore.Application;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StudioCore.Renderer;

public static class TextureLocator
{
    public static readonly char sl = Path.DirectorySeparatorChar;

    // These are for getting the virtual pathes to feed into the resource manager.
    // Remember: AssetVirtualPath is for tpf.dcx, AssetArchiveVirtualPath is for texbnd.dcx
    public static List<ResourceDescriptor> GetMapTextureVirtualPaths(ProjectEntry project, string mapID)
    {
        List<ResourceDescriptor> ads = new();

        if (project.ProjectType is ProjectType.DS2S or ProjectType.DS2)
        {
            ResourceDescriptor t = new();
            t.AssetArchiveVirtualPath = $@"map/tex/{mapID}/tex";
            ads.Add(t);
        }
        else if (project.ProjectType is ProjectType.DES)
        {
            var mid = mapID.Substring(0, 3);

            foreach(var entry in project.FileDictionary.Entries)
            {
                if(entry.Folder == $"/map/{mid}" && entry.Extension == "tpf")
                {
                    ResourceDescriptor res = new();
                    res.AssetVirtualPath = $@"map/tex/{mid}/{entry.Filename.Substring(4, 4)}";
                    ads.Add(res);
                }
            }
        }
        else
        {
            var mid = mapID.Substring(0, 3);

            if (project.ProjectType is ProjectType.ER or ProjectType.AC6 or ProjectType.NR)
            {

            }
            else
            {
                ResourceDescriptor t0000 = new();
                t0000.AssetArchiveVirtualPath = $@"map/tex/{mid}/0000";
                ads.Add(t0000);

                ResourceDescriptor t0001 = new();
                t0001.AssetArchiveVirtualPath = $@"map/tex/{mid}/0001";
                ads.Add(t0001);

                ResourceDescriptor t0002 = new();
                t0002.AssetArchiveVirtualPath = $@"map/tex/{mid}/0002";
                ads.Add(t0002);

                ResourceDescriptor t0003 = new();
                t0003.AssetArchiveVirtualPath = $@"map/tex/{mid}/0003";
                ads.Add(t0003);
            }

            if (project.ProjectType is ProjectType.DS1R)
            {
                ResourceDescriptor env = new();
                env.AssetArchiveVirtualPath = $@"map/tex/{mid}/env";
                ads.Add(env);
            }
            else if (project.ProjectType is ProjectType.BB or ProjectType.DS3)
            {
                ResourceDescriptor env = new();
                env.AssetVirtualPath = $@"map/tex/{mid}/env";
                ads.Add(env);
            }
        }

        return ads;
    }


    public static ResourceDescriptor GetCharacterTextureVirtualPath(ProjectEntry project, string id, bool binder = false, bool lowDetail = false)
    {
        ResourceDescriptor ad = new();

        ad.AssetVirtualPath = null;
        ad.AssetArchiveVirtualPath = null;

        if (project.ProjectType is ProjectType.DES)
        {
            ad.AssetVirtualPath = $@"chr/{id}/tex";
        }
        else if (project.ProjectType is ProjectType.DS1 or ProjectType.DS1R or ProjectType.BB)
        {
            if (binder)
                ad.AssetArchiveVirtualPath = $@"chr/{id}/tex";
            else
                ad.AssetVirtualPath = $@"chr/{id}/tex";
        }
        else if (project.ProjectType is ProjectType.DS2S or ProjectType.DS2 or ProjectType.DS3 or ProjectType.SDT)
        {
            ad.AssetArchiveVirtualPath = $@"chr/{id}/tex";
        }
        else if (project.ProjectType is ProjectType.ER or ProjectType.AC6 or ProjectType.NR)
        {
            ad.AssetArchiveVirtualPath = $@"chr/{id}/tex";

            if (lowDetail)
            {
                ad.AssetArchiveVirtualPath = $@"chr/{id}/tex/low";
            }
        }

        return ad;
    }

    public static ResourceDescriptor GetCharacterCommonTextureVirtualPath(ProjectEntry project, string id, bool binder = false, bool lowDetail = false)
    {
        ResourceDescriptor ad = new();

        ad.AssetVirtualPath = null;
        ad.AssetArchiveVirtualPath = null;

        ad.AssetVirtualPath = $@"aat/{id}/tex";

        return ad;
    }

    public static ResourceDescriptor GetObjectTextureVirtualPath(ProjectEntry project, string id, bool binder = false, bool lowDetail = false)
    {
        ResourceDescriptor ad = new();

        ad.AssetVirtualPath = null;
        ad.AssetArchiveVirtualPath = null;

        ad.AssetArchiveVirtualPath = $@"obj/{id}/tex";

        return ad;
    }

    public static ResourceDescriptor GetPartTextureVirtualPath(ProjectEntry project, string id, bool binder = false, bool lowDetail = false)
    {
        ResourceDescriptor ad = new();

        ad.AssetVirtualPath = null;
        ad.AssetArchiveVirtualPath = null;

        ad.AssetArchiveVirtualPath = $@"parts/{id}/tex";

        return ad;
    }

    public static ResourceDescriptor GetAssetTextureVirtualPath(ProjectEntry project, string id, bool binder = false, bool lowDetail = false)
    {
        ResourceDescriptor ad = new();

        ad.AssetVirtualPath = null;
        ad.AssetArchiveVirtualPath = null;

        var name = id.Replace("AEG", "AET");
        if (name.Contains("aeg"))
        {
            name = id.Replace("aeg", "aet");
        }

        ad.AssetVirtualPath = $@"aet/{name}/tex";

        return ad;
    }

    public static ResourceDescriptor GetSystexTextureVirtualPath(ProjectEntry project, string id, bool binder = false, bool lowDetail = false)
    {
        ResourceDescriptor ad = new();

        ad.AssetVirtualPath = null;
        ad.AssetArchiveVirtualPath = null;

        ad.AssetVirtualPath = $@"systex/{id}/tex";

        return ad;
    }

    /// <summary>
    /// Used in FlverResource
    /// </summary>
    /// <param name="texpath"></param>
    /// <returns></returns>
    public static string GetFlverTextureVirtualPath(string virtPath, string texpath)
    {
        var type = "";

        if (virtPath.Contains("/"))
        {
            type = virtPath.Split("/")[0];
        }

        // Usage of the global BaseEditor here:
        var curProject = Smithbox.ProjectManager.SelectedProject;
        texpath = texpath.Replace('\\', sl);

        // MAP Texture
        if (texpath.Contains($"{sl}map{sl}"))
        {
            var splits = texpath.Split(sl);
            var mapid = splits[splits.Length - 3];
            return $@"map/tex/{mapid}/{Path.GetFileNameWithoutExtension(texpath)}";
        }

        // CHR Texture
        if (texpath.Contains($"{sl}chr{sl}"))
        {
            var splits = texpath.Split(sl);
            var chrid = splits[splits.Length - 3];
            return $@"chr/{chrid}/tex/{Path.GetFileNameWithoutExtension(texpath)}";
        }
        else if (type == "chr")
        {
            var splits = virtPath.Split("/");
            var chrid = splits[1];
            return $@"chr/{chrid}/tex/{Path.GetFileNameWithoutExtension(texpath)}";
        }

        // OBJ Texture
        if (texpath.Contains($"{sl}obj{sl}"))
        {
            var splits = texpath.Split(sl);
            var objid = splits[splits.Length - 3];
            return $@"obj/{objid}/tex/{Path.GetFileNameWithoutExtension(texpath)}";
        }

        // AET Texture
        if (texpath.Contains($"{sl}aet") || texpath.StartsWith("aet"))
        {
            var splits = texpath.Split(sl);

            var aetid = splits[splits.Length - 1].Substring(0, 10);

            return $@"aet/{aetid}/tex/{Path.GetFileNameWithoutExtension(texpath)}";
        }

        // AAT Texture
        if (texpath.Contains($"{sl}aat") || texpath.StartsWith("aat"))
        {
            var name = Path.GetFileName(texpath);
            return $@"aat/common_body/tex/{Path.GetFileNameWithoutExtension(texpath)}";
        }

        // SYSTEX Texture
        if (texpath.Contains($"{sl}systex") || texpath.StartsWith("systex"))
        {
            var name = Path.GetFileName(texpath);
            return $@"systex/system/tex/{Path.GetFileNameWithoutExtension(texpath)}";
        }

        // PARTS Texture
        if (texpath.Contains($"{sl}parts{sl}"))
        {
            var splits = texpath.Split(sl);
            var partsId = splits[splits.Length - 4]; //! FIXME is this wrong?
            return $@"parts/{partsId}/tex/{Path.GetFileNameWithoutExtension(texpath)}";
        }
        else if (virtPath.StartsWith("parts"))
        {
            var splits = virtPath.Split("/");
            var partsId = splits[1];
            return $@"parts/{partsId}/tex/{Path.GetFileNameWithoutExtension(texpath)}";
        }

        return texpath;
    }

}
