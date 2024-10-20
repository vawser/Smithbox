using SoulsFormats;
using StudioCore.Core.Project;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StudioCore.Resource.Locators;
public static class TextureLocator
{
    public static List<ResourceDescriptor> GetMapTextures(string mapid)
    {
        List<ResourceDescriptor> ads = new();

        if (Smithbox.ProjectType == ProjectType.DS2S || Smithbox.ProjectType == ProjectType.DS2)
        {
            ResourceDescriptor t = new();
            t.AssetPath = LocatorUtils.GetAssetPath($@"model\map\t{mapid.Substring(1)}.tpfbhd");
            t.AssetArchiveVirtualPath = $@"map/tex/{mapid}/tex";
            ads.Add(t);
        }
        else if (Smithbox.ProjectType == ProjectType.DES)
        {
            var mid = mapid.Substring(0, 3);
            var paths = Directory.GetFileSystemEntries($@"{Smithbox.GameRoot}\map\{mid}\", "*.tpf.dcx");
            foreach (var path in paths)
            {
                ResourceDescriptor ad = new();
                ad.AssetPath = path;
                var tid = Path.GetFileNameWithoutExtension(path).Substring(4, 4);
                ad.AssetVirtualPath = $@"map/tex/{mid}/{tid}";
                ads.Add(ad);
            }
        }
        else if (Smithbox.ProjectType == ProjectType.ACFA)
        {
            ResourceDescriptor ad = new();

            ad.AssetPath = LocatorUtils.GetAssetPath($@"model\map\{mapid}\{mapid}_t.bnd");
            ad.AssetArchiveVirtualPath = $@"map/tex/{mapid}/tex";
            ads.Add(ad);
        }
        else if (Smithbox.ProjectType == ProjectType.ACV)
        {
            var paths = Directory.EnumerateFiles($@"{Smithbox.GameRoot}\model\map\{mapid}\", "*.tpf.dcx");
            foreach (var path in paths)
            {
                ResourceDescriptor ad = new();
                ad.AssetPath = path;
                var tid = Path.GetFileNameWithoutExtension(path);
                ad.AssetVirtualPath = $@"map/tex/{mapid}/{tid}";
                ads.Add(ad);
            }
        }
        else if (Smithbox.ProjectType == ProjectType.ACVD)
        {
            ResourceDescriptor ad = new();

            ad.AssetPath = LocatorUtils.GetAssetPath($@"model\map\{mapid}\{mapid}_htdcx.bnd");
            ad.AssetArchiveVirtualPath = $@"map/tex/{mapid}/tex";
            ads.Add(ad);
        }
        else
        {
            // Clean this up. Even if it's common code having something like "!=Sekiro" can lead to future issues
            var mid = mapid.Substring(0, 3);

            if (!(Smithbox.ProjectType is ProjectType.ER or ProjectType.AC6))
            {
                ResourceDescriptor t0000 = new();
                t0000.AssetPath = LocatorUtils.GetAssetPath($@"map\{mid}\{mid}_0000.tpfbhd");
                t0000.AssetArchiveVirtualPath = $@"map/tex/{mid}/0000";
                ads.Add(t0000);

                ResourceDescriptor t0001 = new();
                t0001.AssetPath = LocatorUtils.GetAssetPath($@"map\{mid}\{mid}_0001.tpfbhd");
                t0001.AssetArchiveVirtualPath = $@"map/tex/{mid}/0001";
                ads.Add(t0001);

                ResourceDescriptor t0002 = new();
                t0002.AssetPath = LocatorUtils.GetAssetPath($@"map\{mid}\{mid}_0002.tpfbhd");
                t0002.AssetArchiveVirtualPath = $@"map/tex/{mid}/0002";
                ads.Add(t0002);

                ResourceDescriptor t0003 = new();
                t0003.AssetPath = LocatorUtils.GetAssetPath($@"map\{mid}\{mid}_0003.tpfbhd");
                t0003.AssetArchiveVirtualPath = $@"map/tex/{mid}/0003";
                ads.Add(t0003);
            }

            if (Smithbox.ProjectType == ProjectType.DS1R)
            {
                ResourceDescriptor env = new();
                env.AssetPath = LocatorUtils.GetAssetPath($@"map\{mid}\GI_EnvM_{mid}.tpfbhd");
                env.AssetArchiveVirtualPath = $@"map/tex/{mid}/env";
                ads.Add(env);
            }
            else if (Smithbox.ProjectType == ProjectType.BB || Smithbox.ProjectType == ProjectType.DS3)
            {
                ResourceDescriptor env = new();
                env.AssetPath = LocatorUtils.GetAssetPath($@"map\{mid}\{mid}_envmap.tpf.dcx");
                env.AssetVirtualPath = $@"map/tex/{mid}/env";
                ads.Add(env);
            }
        }

        return ads;
    }

    public static List<string> GetEnvMapTextureNames(string mapid)
    {
        List<string> l = new();
        if (Smithbox.ProjectType == ProjectType.DS3)
        {
            var mid = mapid.Substring(0, 3);
            var path = LocatorUtils.GetAssetPath($@"map\{mid}\{mid}_envmap.tpf.dcx");
            if (File.Exists(path))
            {
                var t = TPF.Read(path);
                foreach (TPF.Texture tex in t.Textures)
                    l.Add(tex.Name);
            }
        }

        return l;
    }

    public static string GetChrTexturePath(string chrid, bool isLowDetail = false)
    {
        var overrideFilePath = "";

        if (Smithbox.ProjectType is ProjectType.DES)
        {
            overrideFilePath = LocatorUtils.GetOverridenFilePath($@"chr\{chrid}\{chrid}.tpf");
        }

        if (Smithbox.ProjectType is ProjectType.DS1)
        {
            var path = LocatorUtils.GetOverridenFilePath($@"chr\{chrid}\{chrid}.tpf");
            if (path != null)
                return path;

            overrideFilePath = LocatorUtils.GetOverridenFilePath($@"chr\{chrid}.chrbnd");
        }

        if (Smithbox.ProjectType is ProjectType.DS2S or ProjectType.DS2)
        {
            overrideFilePath = LocatorUtils.GetOverridenFilePath($@"model\chr\{chrid}.texbnd");
        }

        if (Smithbox.ProjectType is ProjectType.DS1R)
        {
            // TODO: Some textures require getting chrtpfbhd from chrbnd, then using it with chrtpfbdt in chr folder.
            overrideFilePath = LocatorUtils.GetOverridenFilePath($@"chr\{chrid}.chrbnd.dcx");
        }

        if (Smithbox.ProjectType is ProjectType.BB)
        {
            overrideFilePath = LocatorUtils.GetOverridenFilePath($@"chr\{chrid}.chrbnd.dcx");
        }

        if (Smithbox.ProjectType is ProjectType.DS3 or ProjectType.SDT)
        {
            overrideFilePath = LocatorUtils.GetOverridenFilePath($@"chr\{chrid}.texbnd.dcx");
        }

        if (Smithbox.ProjectType is ProjectType.ER)
        {
            overrideFilePath = LocatorUtils.GetOverridenFilePath($@"chr\{chrid}_h.texbnd.dcx");

            if (isLowDetail)
            {
                overrideFilePath = LocatorUtils.GetOverridenFilePath($@"chr\{chrid}_l.texbnd.dcx");
            }
        }

        if (Smithbox.ProjectType is ProjectType.AC6)
        {
            overrideFilePath = LocatorUtils.GetOverridenFilePath($@"chr\{chrid}.texbnd.dcx");

            if (isLowDetail)
            {
                overrideFilePath = LocatorUtils.GetOverridenFilePath($@"chr\{chrid}_l.texbnd.dcx");
            }
        }

        if (overrideFilePath != null)
        {
            return overrideFilePath;
        }

        return null;
    }

    public static ResourceDescriptor GetChrTextures(string chrid, bool isLowDetail = false)
    {
        var path = "";
        ResourceDescriptor ad = new();
        ad.AssetArchiveVirtualPath = null;
        ad.AssetPath = null;

        if (Smithbox.ProjectType is ProjectType.DES)
        {
            path = GetChrTexturePath(chrid);
            if (path != null)
            {
                ad.AssetPath = path;
                ad.AssetVirtualPath = $@"chr/{chrid}/tex";
            }
        }
        else if (Smithbox.ProjectType is ProjectType.DS1)
        {
            path = GetChrTexturePath(chrid);
            if (path != null)
            {
                ad.AssetPath = path;
                if (path.EndsWith(".chrbnd"))
                    ad.AssetArchiveVirtualPath = $@"chr/{chrid}/tex";
                else
                    ad.AssetVirtualPath = $@"chr/{chrid}/tex";
            }
        }
        else if (Smithbox.ProjectType is ProjectType.DS1R)
        {
            // TODO: Some textures require getting chrtpfbhd from chrbnd, then using it with chrtpfbdt in chr folder.
            path = GetChrTexturePath(chrid);
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
        else if (Smithbox.ProjectType is ProjectType.DS2S or ProjectType.DS2)
        {
            path = GetChrTexturePath(chrid);
            if (path != null)
            {
                ad = new ResourceDescriptor();
                ad.AssetPath = path;
                ad.AssetArchiveVirtualPath = $@"chr/{chrid}/tex";
            }
        }
        else if (Smithbox.ProjectType is ProjectType.BB)
        {
            path = GetChrTexturePath(chrid);

            if (path != null)
            {
                ad.AssetPath = path;

                if (path.EndsWith(".chrbnd.dcx"))
                    ad.AssetArchiveVirtualPath = $@"chr/{chrid}/tex";
                else
                    ad.AssetVirtualPath = $@"chr/{chrid}/tex";
            }
        }
        else if (Smithbox.ProjectType is ProjectType.DS3 or ProjectType.SDT)
        {
            path = GetChrTexturePath(chrid);
            if (path != null)
            {
                ad.AssetPath = path;
                ad.AssetArchiveVirtualPath = $@"chr/{chrid}/tex";
            }
        }
        else if (Smithbox.ProjectType is ProjectType.ER)
        {
            path = GetChrTexturePath(chrid);

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
        else if (Smithbox.ProjectType is ProjectType.AC6)
        {
            path = GetChrTexturePath(chrid);

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

    public static ResourceDescriptor GetEneTextureContainer(string ene)
    {
        ResourceDescriptor ad = new();
        ad.AssetPath = null;
        ad.AssetArchiveVirtualPath = null;
        string path = null;

        if (Smithbox.ProjectType == ProjectType.ACFA)
        {
            path = LocatorUtils.GetOverridenFilePath($@"model\ene\{ene}\{ene}_t.bnd");
        }
        else if (Smithbox.ProjectType == ProjectType.ACV || Smithbox.ProjectType == ProjectType.ACVD)
        {
            ad.AssetPath = LocatorUtils.GetOverridenFilePath($@"model\ene\{ene}\{ene}.tpf.dcx");
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

    public static ResourceDescriptor GetObjTextureContainer(string obj)
    {
        ResourceDescriptor ad = new();
        ad.AssetPath = null;
        ad.AssetArchiveVirtualPath = null;
        string path = null;

        if (Smithbox.ProjectType == ProjectType.DS1)
        {
            path = LocatorUtils.GetOverridenFilePath($@"obj\{obj}.objbnd");
        }
        else if (Smithbox.ProjectType == ProjectType.DS2S || Smithbox.ProjectType == ProjectType.DS2)
        {
            path = LocatorUtils.GetOverridenFilePath($@"model\obj\{obj}.bnd");
        }
        else if (Smithbox.ProjectType is ProjectType.DES or ProjectType.DS1R or ProjectType.BB or ProjectType.DS3 or ProjectType.SDT)
        {
            path = LocatorUtils.GetOverridenFilePath($@"obj\{obj}.objbnd.dcx");
        }
        else if (Smithbox.ProjectType is ProjectType.ACFA)
        {
            path = LocatorUtils.GetOverridenFilePath($@"model\obj\{obj}\{obj}_t.bnd");
        }
        else if (Smithbox.ProjectType is ProjectType.ACV or ProjectType.ACVD)
        {
            ad.AssetPath = LocatorUtils.GetOverridenFilePath($@"model\obj\{obj}\{obj}.tpf.dcx");
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

    public static ResourceDescriptor GetAetTexture(string aetid)
    {
        ResourceDescriptor ad = new();
        ad.AssetPath = null;
        ad.AssetArchiveVirtualPath = null;
        string path;

        if (Smithbox.ProjectType == ProjectType.ER)
        {
            path = LocatorUtils.GetOverridenFilePath($@"asset\aet\{aetid.Substring(0, 6)}\{aetid}.tpf.dcx");
        }
        else if (Smithbox.ProjectType is ProjectType.AC6)
        {
            path = LocatorUtils.GetOverridenFilePath($@"\asset\environment\texture\{aetid}.tpf.dcx");
        }
        else
        {
            throw new NotSupportedException();
        }

        if (path != null)
        {
            ad.AssetPath = path;
            ad.AssetArchiveVirtualPath = $@"aet/{aetid}/tex";
        }

        return ad;
    }

    public static ResourceDescriptor GetAatTexture(string aatname)
    {
        ResourceDescriptor ad = new();
        ad.AssetPath = null;
        ad.AssetArchiveVirtualPath = null;
        string path;

        if (Smithbox.ProjectType is ProjectType.ER or ProjectType.AC6)
            path = LocatorUtils.GetOverridenFilePath($@"parts\common_body.tpf.dcx");
        else
            throw new NotSupportedException();

        if (path != null)
        {
            ad.AssetPath = path;
            ad.AssetArchiveVirtualPath = $@"aat/tex";
        }

        return ad;
    }

    public static ResourceDescriptor GetSystexTexture(string aatname)
    {
        ResourceDescriptor ad = new();
        ad.AssetPath = null;
        ad.AssetArchiveVirtualPath = null;
        string path;

        if (Smithbox.ProjectType is ProjectType.AC6 or ProjectType.ER or ProjectType.SDT or ProjectType.DS3 or ProjectType.BB)
        {
            path = LocatorUtils.GetOverridenFilePath($@"other\systex.tpf.dcx");
        }
        else
        {
            throw new NotSupportedException();
        }

        if (path != null)
        {
            ad.AssetPath = path;
            ad.AssetArchiveVirtualPath = $@"systex/tex";
        }

        return ad;
    }

    public static ResourceDescriptor GetPartTextureContainer(string partsId, bool isLowDetail = false)
    {
        ResourceDescriptor ad = new();
        ad.AssetArchiveVirtualPath = null;
        ad.AssetPath = null;

        if (Smithbox.ProjectType == ProjectType.AC6)
        {
            var path = LocatorUtils.GetOverridenFilePath($@"parts\{partsId}.partsbnd.dcx");

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
        else if (Smithbox.ProjectType == ProjectType.ER)
        {
            var path = LocatorUtils.GetOverridenFilePath($@"parts\{partsId}.partsbnd.dcx");

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
                path = LocatorUtils.GetOverridenFilePath($@"parts\{partsId}.tpf.dcx");

                if (path != null)
                {
                    ad.AssetPath = path;
                    ad.AssetVirtualPath = $@"parts/{partsId}/tex";
                    ad.AssetArchiveVirtualPath = null;
                }
            }
        }
        else if (Smithbox.ProjectType == ProjectType.DS3 || Smithbox.ProjectType == ProjectType.SDT)
        {
            var path = LocatorUtils.GetOverridenFilePath($@"parts\{partsId}.partsbnd.dcx");
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
        else if (Smithbox.ProjectType == ProjectType.BB)
        {
            var path = LocatorUtils.GetOverridenFilePath($@"parts\{partsId}.partsbnd.dcx");
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
        else if (Smithbox.ProjectType == ProjectType.DS2S || Smithbox.ProjectType == ProjectType.DS2)
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

            var path = LocatorUtils.GetOverridenFilePath($@"model\parts\{partType}\{partsId}.bnd");

            if (path != null)
            {
                ad.AssetPath = path;
                ad.AssetArchiveVirtualPath = $@"parts/{partsId}/tex";
            }
        }
        else if (Smithbox.ProjectType == ProjectType.DS1R)
        {
            var path = LocatorUtils.GetOverridenFilePath($@"parts\{partsId}.partsbnd.dcx");
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
        else if (Smithbox.ProjectType == ProjectType.DS1)
        {
            var path = LocatorUtils.GetOverridenFilePath($@"parts\{partsId}.partsbnd");
            if (path != null)
            {
                ad.AssetPath = path;
                ad.AssetArchiveVirtualPath = $@"parts/{partsId}/tex";
            }
        }
        else if (Smithbox.ProjectType == ProjectType.DES)
        {
            var path = LocatorUtils.GetOverridenFilePath($@"parts\{partsId}.partsbnd.dcx");
            if (path != null)
            {
                ad.AssetPath = path;
                ad.AssetArchiveVirtualPath = $@"parts/{partsId}/tex";
            }
        }

        return ad;
    }

    // Special case for AC6 where the parts use both the partsbnd and a loose tpf for textures
    public static ResourceDescriptor GetPartTpf_Ac6(string partsId)
    {
        ResourceDescriptor ad = new();
        ad.AssetArchiveVirtualPath = null;
        ad.AssetPath = null;

        if (Smithbox.ProjectType == ProjectType.AC6)
        {
            string path;
            if (partsId.Substring(0, 2) == "wp")
            {
                string id;
                if (partsId.EndsWith("_l"))
                {
                    id = partsId[..^2].Split("_").Last();
                    path = LocatorUtils.GetOverridenFilePath($@"parts\wp_{id}_l.tpf.dcx");
                }
                else
                {
                    id = partsId.Split("_").Last();
                    path = LocatorUtils.GetOverridenFilePath($@"parts\wp_{id}.tpf.dcx");
                }
            }
            else
            {
                path = LocatorUtils.GetOverridenFilePath($@"parts\{partsId}_u.tpf.dcx");
            }

            if (path != null)
            {
                ad.AssetPath = path;
                ad.AssetVirtualPath = $@"parts/{partsId}/tex/tpf";
            }
        }

        return ad;
    }

    public static ResourceDescriptor GetAssetTextureContainer(string resourceName)
    {
        ResourceDescriptor ad = new();

        ad.AssetPath = null;
        ad.AssetVirtualPath = null;

        string path = null;

        if (Smithbox.ProjectType == ProjectType.ER)
        {
            path = LocatorUtils.GetOverridenFilePath($@"asset\aet\{resourceName.Substring(0, 6)}\{resourceName}.tpf.dcx");
        }
        else if (Smithbox.ProjectType is ProjectType.AC6)
        {
            path = LocatorUtils.GetOverridenFilePath($@"\asset\environment\texture\{resourceName}.tpf.dcx");
        }

        if (path != null)
        {
            ad.AssetPath = path;
            ad.AssetVirtualPath = $@"aet/{resourceName}/tex";
        }

        return ad;
    }

    // TPF
    public static string GetAssetTextureContainerPath(string resourceName)
    {
        var overrideFilePath = "";

        if (Smithbox.ProjectType is ProjectType.AC6)
        {
            overrideFilePath = LocatorUtils.GetOverridenFilePath($@"asset\environment\texture\{resourceName}.tpf.dcx");
        }

        if (Smithbox.ProjectType is ProjectType.ER)
        {
            overrideFilePath = LocatorUtils.GetOverridenFilePath($@"asset\aet\{resourceName.Substring(0, 6)}\{resourceName}.tpf.dcx");
        }

        if (overrideFilePath != null)
        {
            return overrideFilePath;
        }

        return null;
    }

    public static ResourceDescriptor GetMenuTextureContainer(string resourceName)
    {
        var path = "";
        ResourceDescriptor ad = new();
        ad.AssetVirtualPath = null;
        ad.AssetPath = null;

        path = GetMenuTextureContainerPath(resourceName);

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

    public static string GetMenuTextureContainerPath(string resourceName)
    {
        var overrideFilePath = "";

        // TPF
        if (Smithbox.ProjectType is ProjectType.AC6 or ProjectType.ER or ProjectType.SDT)
        {
            overrideFilePath = LocatorUtils.GetOverridenFilePath($@"menu\hi\{resourceName}.tpf.dcx");
        }

        if (Smithbox.ProjectType is ProjectType.DS3)
        {
            overrideFilePath = LocatorUtils.GetOverridenFilePath($@"menu\{resourceName}.tpf.dcx");
        }

        if (Smithbox.ProjectType is ProjectType.DS1R)
        {
            overrideFilePath = LocatorUtils.GetOverridenFilePath($@"menu\{resourceName}.tpf.dcx");
        }

        if (overrideFilePath != null)
        {
            return overrideFilePath;
        }
        // TPFBHD
        else
        {
            if (Smithbox.ProjectType is ProjectType.AC6 or ProjectType.ER or ProjectType.SDT)
            {
                overrideFilePath = LocatorUtils.GetOverridenFilePath($@"menu\hi\{resourceName}.tpfbhd");
            }

            if (Smithbox.ProjectType is ProjectType.DS3)
            {
                overrideFilePath = LocatorUtils.GetOverridenFilePath($@"menu\{resourceName}.tpfbhd");
            }

            if (overrideFilePath != null)
            {
                return overrideFilePath;
            }
        }

        return null;
    }

    public static ResourceDescriptor GetOtherTextureContainer(string resourceName)
    {
        var path = "";
        ResourceDescriptor ad = new();
        ad.AssetVirtualPath = null;
        ad.AssetPath = null;

        path = GetOtherTextureContainerPath(resourceName);

        if (path != null)
        {
            ad.AssetPath = path;
            ad.AssetVirtualPath = $@"other/{resourceName}/tex";
        }

        return ad;
    }

    public static string GetOtherTextureContainerPath(string resourceName)
    {
        var overrideFilePath = "";

        overrideFilePath = LocatorUtils.GetOverridenFilePath($@"other\{resourceName}.tpf.dcx");

        if (overrideFilePath != null)
        {
            return overrideFilePath;
        }

        return null;
    }

    public static ResourceDescriptor GetParticleTextureContainer(string resourceName)
    {
        var path = "";
        ResourceDescriptor ad = new();
        ad.AssetVirtualPath = null;
        ad.AssetPath = null;

        path = GetParticleTextureContainerPath(resourceName);

        if (path != null)
        {
            ad.AssetPath = path;
            ad.AssetArchiveVirtualPath = $@"sfx/{resourceName}/tex";
        }

        return ad;
    }

    public static string GetParticleTextureContainerPath(string resourceName)
    {
        var overrideFilePath = "";
        var fileExt = @".ffxbnd.dcx";

        if (Smithbox.ProjectType is ProjectType.DS2S or ProjectType.DS2)
        {
            fileExt = @".ffxbnd";
        }

        overrideFilePath = LocatorUtils.GetOverridenFilePath($@"sfx\{resourceName}{fileExt}");

        if (overrideFilePath != null)
        {
            return overrideFilePath;
        }

        return null;
    }
}
