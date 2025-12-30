using SoulsFormats;
using StudioCore.Application;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StudioCore.Renderer;

public static class TextureLocator
{


    public static string GetChrTexturePath(ProjectEntry project, string chrid, bool isLowDetail = false)
    {
        var overrideFilePath = "";

        if (project.ProjectType is ProjectType.DES)
        {
            overrideFilePath = LocatorUtils.GetOverridenFilePath(project, Path.Join("chr", chrid, $"{chrid}.tpf"));
        }

        if (project.ProjectType is ProjectType.DS1)
        {
            var path = LocatorUtils.GetOverridenFilePath(project, Path.Join("chr", chrid, $"{chrid}.tpf"));
            if (path != null)
                return path;

            overrideFilePath = LocatorUtils.GetOverridenFilePath(project, Path.Join("chr", $"{chrid}.chrbnd"));
        }

        if (project.ProjectType is ProjectType.DS2S or ProjectType.DS2)
        {
            overrideFilePath = LocatorUtils.GetOverridenFilePath(project, Path.Join("model", "chr", $"{chrid}.texbnd"));
        }

        if (project.ProjectType is ProjectType.DS1R)
        {
            // TODO: Some textures require getting chrtpfbhd from chrbnd, then using it with chrtpfbdt in chr folder.
            overrideFilePath = LocatorUtils.GetOverridenFilePath(project, Path.Join("chr", $"{chrid}.chrbnd.dcx"));
        }

        if (project.ProjectType is ProjectType.BB)
        {
            overrideFilePath = LocatorUtils.GetOverridenFilePath(project, Path.Join("chr", $"{chrid}.chrbnd.dcx"));
        }

        if (project.ProjectType is ProjectType.DS3 or ProjectType.SDT)
        {
            overrideFilePath = LocatorUtils.GetOverridenFilePath(project, Path.Join("chr", $"{chrid}.texbnd.dcx"));
        }

        if (project.ProjectType is ProjectType.ER or ProjectType.NR)
        {
            overrideFilePath = LocatorUtils.GetOverridenFilePath(project, Path.Join("chr", $"{chrid}_h.texbnd.dcx"));

            if (isLowDetail)
            {
                overrideFilePath = LocatorUtils.GetOverridenFilePath(project, Path.Join("chr", $"{chrid}_l.texbnd.dcx"));
            }
        }

        if (project.ProjectType is ProjectType.AC6)
        {
            overrideFilePath = LocatorUtils.GetOverridenFilePath(project, Path.Join("chr", $"{chrid}.texbnd.dcx"));

            if (isLowDetail)
            {
                overrideFilePath = LocatorUtils.GetOverridenFilePath(project, Path.Join("chr", $"{chrid}_l.texbnd.dcx"));
            }
        }

        if (overrideFilePath != null)
        {
            return overrideFilePath;
        }

        return null;
    }

    public static ResourceDescriptor GetChrTextures(ProjectEntry project, string chrid, bool isLowDetail = false)
    {
        var path = "";
        ResourceDescriptor ad = new();
        ad.AssetArchiveVirtualPath = null;
        ad.AssetPath = null;

        if (project.ProjectType is ProjectType.DES)
        {
            path = GetChrTexturePath(project, chrid);
            if (path != null)
            {
                ad.AssetPath = path;
                ad.AssetVirtualPath = $@"chr/{chrid}/tex";
            }
        }
        else if (project.ProjectType is ProjectType.DS1)
        {
            path = GetChrTexturePath(project, chrid);
            if (path != null)
            {
                ad.AssetPath = path;
                if (path.EndsWith(".chrbnd"))
                    ad.AssetArchiveVirtualPath = $@"chr/{chrid}/tex";
                else
                    ad.AssetVirtualPath = $@"chr/{chrid}/tex";
            }
        }
        else if (project.ProjectType is ProjectType.DS1R)
        {
            // TODO: Some textures require getting chrtpfbhd from chrbnd, then using it with chrtpfbdt in chr folder.
            path = GetChrTexturePath(project, chrid);
            if (path != null)
            {
                ad = new ResourceDescriptor();
                ad.AssetPath = path;

                if (path.EndsWith(".chrbnd.dcx"))
                    ad.AssetArchiveVirtualPath = $@"chr/{chrid}/tex";
                else
                    ad.AssetVirtualPath = $@"chr/{chrid}/tex";
            }
        }
        else if (project.ProjectType is ProjectType.DS2S or ProjectType.DS2)
        {
            path = GetChrTexturePath(project, chrid);
            if (path != null)
            {
                ad = new ResourceDescriptor();
                ad.AssetPath = path;
                ad.AssetArchiveVirtualPath = $@"chr/{chrid}/tex";
            }
        }
        else if (project.ProjectType is ProjectType.BB)
        {
            path = GetChrTexturePath(project, chrid);

            if (path != null)
            {
                ad.AssetPath = path;

                if (path.EndsWith(".chrbnd.dcx"))
                    ad.AssetArchiveVirtualPath = $@"chr/{chrid}/tex";
                else
                    ad.AssetVirtualPath = $@"chr/{chrid}/tex";
            }
        }
        else if (project.ProjectType is ProjectType.DS3 or ProjectType.SDT)
        {
            path = GetChrTexturePath(project, chrid);
            if (path != null)
            {
                ad.AssetPath = path;
                ad.AssetArchiveVirtualPath = $@"chr/{chrid}/tex";
            }
        }
        else if (project.ProjectType is ProjectType.ER or ProjectType.NR)
        {
            path = GetChrTexturePath(project, chrid);

            if (path != null)
            {
                ad.AssetPath = path;
                ad.AssetArchiveVirtualPath = $@"chr/{chrid}/tex";

                if (isLowDetail)
                {
                    ad.AssetArchiveVirtualPath = $@"chr/{chrid}/tex/low";
                }
            }
        }
        else if (project.ProjectType is ProjectType.AC6)
        {
            path = GetChrTexturePath(project, chrid);

            if (path != null)
            {
                ad.AssetPath = path;
                ad.AssetArchiveVirtualPath = $@"chr/{chrid}/tex";

                if (isLowDetail)
                {
                    ad.AssetArchiveVirtualPath = $@"chr/{chrid}/tex/low";
                }
            }
        }

        return ad;
    }

    public static ResourceDescriptor GetEneTextureContainer(ProjectEntry project, string ene)
    {
        ResourceDescriptor ad = new();
        ad.AssetPath = null;
        ad.AssetArchiveVirtualPath = null;
        string path = null;

        if (project.ProjectType == ProjectType.ACFA)
        {
            path = LocatorUtils.GetOverridenFilePath(project, Path.Join("model", "ene", ene, $"{ene}_t.bnd"));
        }
        else if (project.ProjectType == ProjectType.ACV || project.ProjectType == ProjectType.ACVD)
        {
            ad.AssetPath = LocatorUtils.GetOverridenFilePath(project, Path.Join("model", "ene", ene, $"{ene}.tpf.dcx"));
            ad.AssetVirtualPath = $@"ene/{ene}/tex";
            return ad;
        }

        if (path != null)
        {
            ad.AssetPath = path;
            ad.AssetArchiveVirtualPath = $@"ene/{ene}/tex";
        }

        return ad;
    }

    public static ResourceDescriptor GetObjTextureContainer(ProjectEntry project, string obj)
    {
        ResourceDescriptor ad = new();
        ad.AssetPath = null;
        ad.AssetArchiveVirtualPath = null;
        string path = null;

        if (project.ProjectType == ProjectType.DS1)
        {
            path = LocatorUtils.GetOverridenFilePath(project, Path.Join("obj", $"{obj}.objbnd"));
        }
        else if (project.ProjectType == ProjectType.DS2S || project.ProjectType == ProjectType.DS2)
        {
            path = LocatorUtils.GetOverridenFilePath(project, Path.Join("model", "obj", $"{obj}.bnd"));
        }
        else if (project.ProjectType is ProjectType.DES or ProjectType.DS1R or ProjectType.BB or ProjectType.DS3 or ProjectType.SDT)
        {
            path = LocatorUtils.GetOverridenFilePath(project, Path.Join("obj", $"{obj}.objbnd.dcx"));
        }
        else if (project.ProjectType is ProjectType.ACFA)
        {
            path = LocatorUtils.GetOverridenFilePath(project, Path.Join("model", "obj", obj, $"{obj}_t.bnd"));
        }
        else if (project.ProjectType is ProjectType.ACV or ProjectType.ACVD)
        {
            ad.AssetPath = LocatorUtils.GetOverridenFilePath(project, Path.Join("model", "obj", obj, $"{obj}.tpf.dcx"));
            ad.AssetVirtualPath = $@"obj/{obj}/tex";
            return ad;
        }

        if (path != null)
        {
            ad.AssetPath = path;
            ad.AssetArchiveVirtualPath = $@"obj/{obj}/tex";
        }

        return ad;
    }

    public static ResourceDescriptor GetAetTexture(ProjectEntry project, string aetid)
    {
        ResourceDescriptor ad = new();
        ad.AssetPath = null;
        ad.AssetArchiveVirtualPath = null;
        string path;

        if (project.ProjectType is ProjectType.ER or ProjectType.NR)
        {
            path = LocatorUtils.GetOverridenFilePath(project, Path.Join("asset", "aet", aetid.Substring(0, 6), $"{aetid}.tpf.dcx"));
        }
        else if (project.ProjectType is ProjectType.AC6)
        {
            path = LocatorUtils.GetOverridenFilePath(project, Path.Join("asset", "environment", "texture", $"{aetid}.tpf.dcx"));
        }
        else
        {
            throw new NotSupportedException();
        }

        ad.AssetPath = path;
        ad.AssetVirtualPath = $@"aet/{aetid}/tex";

        return ad;
    }

    public static ResourceDescriptor GetAatTexture(ProjectEntry project, string aatname)
    {
        ResourceDescriptor ad = new();
        ad.AssetPath = null;
        ad.AssetArchiveVirtualPath = null;
        string path;

        if (project.ProjectType is ProjectType.ER or ProjectType.AC6 or ProjectType.NR)
            path = LocatorUtils.GetOverridenFilePath(project, Path.Join("parts", $"{aatname}.tpf.dcx"));
        else
            throw new NotSupportedException();

        if (path != null)
        {
            ad.AssetPath = path;
            ad.AssetVirtualPath = $@"aat/{aatname}/tex";
        }

        return ad;
    }

    public static ResourceDescriptor GetSystexTexture(ProjectEntry project, string systexname)
    {
        ResourceDescriptor ad = new();
        ad.AssetPath = null;
        ad.AssetArchiveVirtualPath = null;
        string path;

        if (project.ProjectType is ProjectType.AC6 or ProjectType.ER or ProjectType.SDT or ProjectType.DS3 or ProjectType.BB or ProjectType.NR)
        {
            path = LocatorUtils.GetOverridenFilePath(project, Path.Join("other", $"{systexname}.tpf.dcx"));
        }
        else
        {
            throw new NotSupportedException();
        }

        if (path != null)
        {
            ad.AssetPath = path;
            ad.AssetVirtualPath = $@"systex/{systexname}/tex";
        }

        return ad;
    }

    public static ResourceDescriptor GetPartTextureContainer(ProjectEntry project, string partsId, bool isLowDetail = false)
    {
        ResourceDescriptor ad = new();
        ad.AssetArchiveVirtualPath = null;
        ad.AssetPath = null;

        if (project.ProjectType == ProjectType.AC6)
        {
            var path = LocatorUtils.GetOverridenFilePath(project, Path.Join("parts", $"{partsId}.partsbnd.dcx"));

            if (path != null)
            {
                ad.AssetPath = path;
                ad.AssetArchiveVirtualPath = $@"parts/{partsId}/tex";

                if (isLowDetail)
                {
                    ad.AssetArchiveVirtualPath = $@"parts/{partsId}/tex/low";
                }
            }
        }
        else if (project.ProjectType is ProjectType.ER  or ProjectType.NR)
        {
            var path = LocatorUtils.GetOverridenFilePath(project, Path.Join("parts", $"{partsId}.partsbnd.dcx"));

            if (path != null)
            {
                ad.AssetPath = path;
                ad.AssetArchiveVirtualPath = $@"parts/{partsId}/tex";

                if (isLowDetail)
                {
                    ad.AssetArchiveVirtualPath = $@"parts/{partsId}/tex/low";
                }
            }

            if (partsId == "common_body")
            {
                path = LocatorUtils.GetOverridenFilePath(project, Path.Join("parts", $"{partsId}.tpf.dcx"));

                if (path != null)
                {
                    ad.AssetPath = path;
                    ad.AssetVirtualPath = $@"parts/{partsId}/tex";
                    ad.AssetArchiveVirtualPath = null;
                }
            }
        }
        else if (project.ProjectType == ProjectType.DS3 || project.ProjectType == ProjectType.SDT)
        {
            var path = LocatorUtils.GetOverridenFilePath(project, Path.Join("parts", $"{partsId}.partsbnd.dcx"));
            if (path != null)
            {
                ad.AssetPath = path;
                ad.AssetArchiveVirtualPath = $@"parts/{partsId}/tex";

                if (isLowDetail)
                {
                    ad.AssetArchiveVirtualPath = $@"parts/{partsId}/tex/low";
                }
            }
        }
        else if (project.ProjectType == ProjectType.BB)
        {
            var path = LocatorUtils.GetOverridenFilePath(project, Path.Join("parts", $"{partsId}.partsbnd.dcx"));
            if (path != null)
            {
                ad.AssetPath = path;

                ad.AssetArchiveVirtualPath = $@"parts/{partsId}/tex";

                if (isLowDetail)
                {
                    ad.AssetArchiveVirtualPath = $@"parts/{partsId}/tex/low";
                }
            }
        }
        else if (project.ProjectType == ProjectType.DS2S || project.ProjectType == ProjectType.DS2)
        {
            var partType = "";
            switch (partsId.Substring(0, 2))
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

            var path = LocatorUtils.GetOverridenFilePath(project, Path.Join("model", "parts", partType, $"{partsId}.bnd"));

            if (path != null)
            {
                ad.AssetPath = path;
                ad.AssetArchiveVirtualPath = $@"parts/{partsId}/tex";
            }
        }
        else if (project.ProjectType == ProjectType.DS1R)
        {
            var path = LocatorUtils.GetOverridenFilePath(project, Path.Join("parts", $"{partsId}.partsbnd.dcx"));
            if (path != null)
            {
                ad.AssetPath = path;
                ad.AssetArchiveVirtualPath = $@"parts/{partsId}/tex";

                if (isLowDetail)
                {
                    ad.AssetArchiveVirtualPath = $@"parts/{partsId}/tex/low";
                }
            }
        }
        else if (project.ProjectType == ProjectType.DS1)
        {
            var path = LocatorUtils.GetOverridenFilePath(project, Path.Join("parts", $"{partsId}.partsbnd"));
            if (path != null)
            {
                ad.AssetPath = path;
                ad.AssetArchiveVirtualPath = $@"parts/{partsId}/tex";
            }
        }
        else if (project.ProjectType == ProjectType.DES)
        {
            var path = LocatorUtils.GetOverridenFilePath(project, Path.Join("parts", $"{partsId}.partsbnd.dcx"));
            if (path != null)
            {
                ad.AssetPath = path;
                ad.AssetArchiveVirtualPath = $@"parts/{partsId}/tex";
            }
        }

        return ad;
    }

    // Special case for AC6 where the parts use both the partsbnd and a loose tpf for textures
    public static ResourceDescriptor GetPartTpf_Ac6(ProjectEntry project, string partsId)
    {
        ResourceDescriptor ad = new();
        ad.AssetArchiveVirtualPath = null;
        ad.AssetPath = null;

        if (project.ProjectType == ProjectType.AC6)
        {
            string path;
            if (partsId.Length >= 2 && partsId.Substring(0, 2) == "wp")
            {
                string id;
                if (partsId.EndsWith("_l"))
                {
                    id = partsId[..^2].Split("_").Last();
                    path = LocatorUtils.GetOverridenFilePath(project, Path.Join("parts", $"wp_{id}_l.tpf.dcx"));
                }
                else
                {
                    id = partsId.Split("_").Last();
                    path = LocatorUtils.GetOverridenFilePath(project, Path.Join("parts", $"wp_{id}.tpf.dcx"));
                }
            }
            else
            {
                path = LocatorUtils.GetOverridenFilePath(project, Path.Join("parts", $"{partsId}_u.tpf.dcx"));
            }

            if (path != null)
            {
                ad.AssetPath = path;
                ad.AssetVirtualPath = $@"parts/{partsId}/tex/tpf";
            }
        }

        return ad;
    }

    // TPF
    public static string GetAssetTextureContainerPath(ProjectEntry project, string resourceName)
    {
        var overrideFilePath = "";

        if (project.ProjectType is ProjectType.AC6)
        {
            overrideFilePath = LocatorUtils.GetOverridenFilePath(project, Path.Join("asset", "environment", "texture", $"{resourceName}.tpf.dcx"));
        }

        if (project.ProjectType is ProjectType.ER or ProjectType.NR)
        {
            overrideFilePath = LocatorUtils.GetOverridenFilePath(project, Path.Join("asset", "aet", resourceName.Substring(0, 6), $"{resourceName}.tpf.dcx"));
        }

        if (overrideFilePath != null)
        {
            return overrideFilePath;
        }

        return null;
    }

    public static ResourceDescriptor GetMenuTextureContainer(ProjectEntry project, string resourceName)
    {
        var path = "";
        ResourceDescriptor ad = new();
        ad.AssetVirtualPath = null;
        ad.AssetPath = null;

        path = GetMenuTextureContainerPath(project, resourceName);

        if (path != null)
        {
            ad.AssetPath = path;

            if (path.Contains(".tpfbhd"))
            {
                ad.AssetArchiveVirtualPath = $@"menu/{resourceName}/tex";
            }
            else
            {
                ad.AssetVirtualPath = $@"menu/{resourceName}/tex";
            }
        }

        return ad;
    }

    public static string GetMenuTextureContainerPath(ProjectEntry project, string resourceName)
    {
        var overrideFilePath = "";

        // TPF
        if (project.ProjectType is ProjectType.AC6 or ProjectType.ER or ProjectType.SDT or ProjectType.NR)
        {
            overrideFilePath = LocatorUtils.GetOverridenFilePath(project, Path.Join("menu", "hi", $"{resourceName}.tpf.dcx"));
        }

        if (project.ProjectType is ProjectType.DS3)
        {
            overrideFilePath = LocatorUtils.GetOverridenFilePath(project, Path.Join("menu", $"{resourceName}.tpf.dcx"));
        }

        if (project.ProjectType is ProjectType.DS1R)
        {
            overrideFilePath = LocatorUtils.GetOverridenFilePath(project, Path.Join("menu", $"{resourceName}.tpf.dcx"));
        }

        if (overrideFilePath != null)
        {
            return overrideFilePath;
        }
        // TPFBHD
        else
        {
            if (project.ProjectType is ProjectType.AC6 or ProjectType.ER or ProjectType.SDT or ProjectType.NR)
            {
                overrideFilePath = LocatorUtils.GetOverridenFilePath(project, Path.Join("menu", "hi", $"{resourceName}.tpfbhd"));
            }

            if (project.ProjectType is ProjectType.DS3)
            {
                overrideFilePath = LocatorUtils.GetOverridenFilePath(project, Path.Join("menu", $"{resourceName}.tpfbhd"));
            }

            if (overrideFilePath != null)
            {
                return overrideFilePath;
            }
        }

        return null;
    }

    public static ResourceDescriptor GetOtherTextureContainer(ProjectEntry project, string resourceName)
    {
        var path = "";
        ResourceDescriptor ad = new();
        ad.AssetVirtualPath = null;
        ad.AssetPath = null;

        path = GetOtherTextureContainerPath(project, resourceName);

        if (path != null)
        {
            ad.AssetPath = path;
            ad.AssetVirtualPath = $@"other/{resourceName}/tex";
        }

        return ad;
    }

    public static string GetOtherTextureContainerPath(ProjectEntry project, string resourceName)
    {
        var overrideFilePath = "";

        overrideFilePath = LocatorUtils.GetOverridenFilePath(project, Path.Join("other", $"{resourceName}.tpf.dcx"));

        if (overrideFilePath != null)
        {
            return overrideFilePath;
        }

        return null;
    }

    public static ResourceDescriptor GetParticleTextureContainer(ProjectEntry project, string resourceName)
    {
        var path = "";
        ResourceDescriptor ad = new();
        ad.AssetVirtualPath = null;
        ad.AssetPath = null;

        path = GetParticleTextureContainerPath(project, resourceName);

        if (path != null)
        {
            ad.AssetPath = path;
            ad.AssetArchiveVirtualPath = $@"sfx/{resourceName}/tex";
        }

        return ad;
    }

    public static string GetParticleTextureContainerPath(ProjectEntry project, string resourceName)
    {
        var overrideFilePath = "";
        var fileExt = @".ffxbnd.dcx";

        if (project.ProjectType is ProjectType.DS2S or ProjectType.DS2)
        {
            fileExt = @".ffxbnd";
        }

        overrideFilePath = LocatorUtils.GetOverridenFilePath(project, Path.Join("sfx", $"{resourceName}{fileExt}"));

        if (overrideFilePath != null)
        {
            return overrideFilePath;
        }

        return null;
    }
}
