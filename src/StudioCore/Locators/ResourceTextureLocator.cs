using SoulsFormats;
using StudioCore.BanksMain;
using StudioCore.Editors.ModelEditor;
using StudioCore.UserProject;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Locators;
public static class ResourceTextureLocator
{
    public static List<ResourceDescriptor> GetMapTextures(string mapid)
    {
        List<ResourceDescriptor> ads = new();

        if (Project.Type == ProjectType.DS2S)
        {
            ResourceDescriptor t = new();
            t.AssetPath = ResourceLocatorUtils.GetAssetPath($@"model\map\t{mapid.Substring(1)}.tpfbhd");
            t.AssetArchiveVirtualPath = $@"map/tex/{mapid}/tex";
            ads.Add(t);
        }
        else if (Project.Type == ProjectType.DES)
        {
            var mid = mapid.Substring(0, 3);
            var paths = Directory.GetFileSystemEntries($@"{Project.GameRootDirectory}\map\{mid}\", "*.tpf.dcx");
            foreach (var path in paths)
            {
                ResourceDescriptor ad = new();
                ad.AssetPath = path;
                var tid = Path.GetFileNameWithoutExtension(path).Substring(4, 4);
                ad.AssetVirtualPath = $@"map/tex/{mid}/{tid}";
                ads.Add(ad);
            }
        }
        else
        {
            // Clean this up. Even if it's common code having something like "!=Sekiro" can lead to future issues
            var mid = mapid.Substring(0, 3);

            ResourceDescriptor t0000 = new();
            t0000.AssetPath = ResourceLocatorUtils.GetAssetPath($@"map\{mid}\{mid}_0000.tpfbhd");
            t0000.AssetArchiveVirtualPath = $@"map/tex/{mid}/0000";
            ads.Add(t0000);

            ResourceDescriptor t0001 = new();
            t0001.AssetPath = ResourceLocatorUtils.GetAssetPath($@"map\{mid}\{mid}_0001.tpfbhd");
            t0001.AssetArchiveVirtualPath = $@"map/tex/{mid}/0001";
            ads.Add(t0001);

            ResourceDescriptor t0002 = new();
            t0002.AssetPath = ResourceLocatorUtils.GetAssetPath($@"map\{mid}\{mid}_0002.tpfbhd");
            t0002.AssetArchiveVirtualPath = $@"map/tex/{mid}/0002";
            ads.Add(t0002);

            ResourceDescriptor t0003 = new();
            t0003.AssetPath = ResourceLocatorUtils.GetAssetPath($@"map\{mid}\{mid}_0003.tpfbhd");
            t0003.AssetArchiveVirtualPath = $@"map/tex/{mid}/0003";
            ads.Add(t0003);

            if (Project.Type == ProjectType.DS1R)
            {
                ResourceDescriptor env = new();
                env.AssetPath = ResourceLocatorUtils.GetAssetPath($@"map\{mid}\GI_EnvM_{mid}.tpfbhd");
                env.AssetArchiveVirtualPath = $@"map/tex/{mid}/env";
                ads.Add(env);
            }
            else if (Project.Type == ProjectType.BB || Project.Type == ProjectType.DS3)
            {
                ResourceDescriptor env = new();
                env.AssetPath = ResourceLocatorUtils.GetAssetPath($@"map\{mid}\{mid}_envmap.tpf.dcx");
                env.AssetVirtualPath = $@"map/tex/{mid}/env";
                ads.Add(env);
            }
            else if (Project.Type == ProjectType.SDT)
            {
                //TODO SDT
            }
        }

        return ads;
    }

    public static List<string> GetEnvMapTextureNames(string mapid)
    {
        List<string> l = new();
        if (Project.Type == ProjectType.DS3)
        {
            var mid = mapid.Substring(0, 3);
            if (File.Exists(ResourceLocatorUtils.GetAssetPath($@"map\{mid}\{mid}_envmap.tpf.dcx")))
            {
                var t = TPF.Read(ResourceLocatorUtils.GetAssetPath($@"map\{mid}\{mid}_envmap.tpf.dcx"));
                foreach (TPF.Texture tex in t.Textures)
                    l.Add(tex.Name);
            }
        }

        return l;
    }

    public static string GetChrTexturePath(string chrid, bool isLowDetail = false)
    {
        var overrideFilePath = "";

        if (Project.Type is ProjectType.DES)
        {
            overrideFilePath = ResourceLocatorUtils.GetOverridenFilePath($@"chr\{chrid}\{chrid}.tpf");
        }

        if (Project.Type is ProjectType.DS1)
        {
            var path = ResourceLocatorUtils.GetOverridenFilePath($@"chr\{chrid}\{chrid}.tpf");
            if (path != null)
                return path;

            overrideFilePath = ResourceLocatorUtils.GetOverridenFilePath($@"chr\{chrid}.chrbnd");
        }

        if (Project.Type is ProjectType.DS2S)
        {
            overrideFilePath = ResourceLocatorUtils.GetOverridenFilePath($@"model\chr\{chrid}.texbnd");
        }

        if (Project.Type is ProjectType.DS1R)
        {
            // TODO: Some textures require getting chrtpfbhd from chrbnd, then using it with chrtpfbdt in chr folder.
            overrideFilePath = ResourceLocatorUtils.GetOverridenFilePath($@"chr\{chrid}.chrbnd");
        }

        if (Project.Type is ProjectType.BB)
        {
            overrideFilePath = ResourceLocatorUtils.GetOverridenFilePath($@"chr\{chrid}_2.tpf.dcx");
        }

        if (Project.Type is ProjectType.DS3 or ProjectType.SDT)
        {
            overrideFilePath = ResourceLocatorUtils.GetOverridenFilePath($@"chr\{chrid}.texbnd.dcx");
        }

        if (Project.Type is ProjectType.ER)
        {
            overrideFilePath = ResourceLocatorUtils.GetOverridenFilePath($@"chr\{chrid}_h.texbnd.dcx");

            if(isLowDetail)
            {
                overrideFilePath = ResourceLocatorUtils.GetOverridenFilePath($@"chr\{chrid}_l.texbnd.dcx");
            }
        }

        if (Project.Type is ProjectType.AC6)
        {
            overrideFilePath = ResourceLocatorUtils.GetOverridenFilePath($@"chr\{chrid}.texbnd.dcx");
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

        if (Project.Type is ProjectType.DES)
        {
            path = GetChrTexturePath(chrid);
            if (path != null)
            {
                ad.AssetPath = path;
                ad.AssetVirtualPath = $@"chr/{chrid}/tex";
            }
        }
        else if (Project.Type is ProjectType.DS1)
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
        else if (Project.Type is ProjectType.DS1R)
        {
            // TODO: Some textures require getting chrtpfbhd from chrbnd, then using it with chrtpfbdt in chr folder.
            path = GetChrTexturePath(chrid);
            if (path != null)
            {
                ad = new ResourceDescriptor();
                ad.AssetPath = path;
                ad.AssetVirtualPath = $@"chr/{chrid}/tex";
            }
        }
        else if (Project.Type is ProjectType.DS2S)
        {
            path = GetChrTexturePath(chrid);
            if (path != null)
            {
                ad = new ResourceDescriptor();
                ad.AssetPath = path;
                ad.AssetVirtualPath = $@"chr/{chrid}/tex";
            }
        }
        else if (Project.Type is ProjectType.BB)
        {
            path = GetChrTexturePath(chrid);

            if (path != null)
            {
                ad.AssetPath = path;
                ad.AssetVirtualPath = $@"chr/{chrid}/tex";
            }
        }
        else if (Project.Type is ProjectType.DS3 or ProjectType.SDT)
        {
            path = GetChrTexturePath(chrid);
            if (path != null)
            {
                ad.AssetPath = path;
                ad.AssetArchiveVirtualPath = $@"chr/{chrid}/tex";
            }
        }
        else if (Project.Type is ProjectType.ER)
        {
            path = GetChrTexturePath(chrid);

            if (path != null)
            {
                ad.AssetPath = path;
                ad.AssetArchiveVirtualPath = $@"chr/{chrid}/tex";

                if(isLowDetail)
                {
                    ad.AssetArchiveVirtualPath = $@"chr/{chrid}/tex/low";
                }
            }
        }
        else if (Project.Type is ProjectType.AC6)
        {
            path = GetChrTexturePath(chrid);

            if (path != null)
            {
                ad.AssetPath = path;
                ad.AssetArchiveVirtualPath = $@"chr/{chrid}/tex";
            }
        }

        return ad;
    }

    public static ResourceDescriptor GetObjTextureContainer(string obj)
    {
        ResourceDescriptor ad = new();
        ad.AssetPath = null;
        ad.AssetArchiveVirtualPath = null;
        string path = null;

        if (Project.Type == ProjectType.DS1)
        {
            path = ResourceLocatorUtils.GetOverridenFilePath($@"obj\{obj}.objbnd");
        }
        else if (Project.Type is ProjectType.DES or ProjectType.DS1R or ProjectType.BB or ProjectType.DS3 or ProjectType.SDT)
        {
            path = ResourceLocatorUtils.GetOverridenFilePath($@"obj\{obj}.objbnd.dcx");
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

        if (Project.Type == ProjectType.ER)
        {
            path = ResourceLocatorUtils.GetOverridenFilePath($@"asset\aet\{aetid.Substring(0, 6)}\{aetid}.tpf.dcx");
        }
        else if (Project.Type is ProjectType.AC6)
        {
            path = ResourceLocatorUtils.GetOverridenFilePath($@"\asset\environment\texture\{aetid}.tpf.dcx");
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

        if (Project.Type is ProjectType.ER or ProjectType.AC6)
            path = ResourceLocatorUtils.GetOverridenFilePath($@"parts\common_body.tpf.dcx");
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

        if (Project.Type == ProjectType.AC6 || Project.Type == ProjectType.ER || Project.Type == ProjectType.SDT || Project.Type == ProjectType.DS3)
        {
            path = ResourceLocatorUtils.GetOverridenFilePath($@"other\systex.tpf.dcx");
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

        if (Project.Type == ProjectType.AC6)
        {
            /*
            string path;
            if (partsId.Substring(0, 2) == "wp")
            {
                string id;
                if (partsId.EndsWith("_l"))
                {
                    id = partsId[..^2].Split("_").Last();
                    path = ResourceLocatorUtils.GetOverridenFilePath($@"parts\wp_{id}_l.tpf.dcx");
                }
                else
                {
                    id = partsId.Split("_").Last();
                    path = ResourceLocatorUtils.GetOverridenFilePath($@"parts\wp_{id}.tpf.dcx");
                }
            }
            else
            {
                path = ResourceLocatorUtils.GetOverridenFilePath($@"parts\{partsId}_u.tpf.dcx");
            }

            if (path != null)
            {
                ad.AssetPath = path;
                ad.AssetVirtualPath = $@"parts/{partsId}/tex";
            }
            */

            var path = ResourceLocatorUtils.GetOverridenFilePath($@"parts\{partsId}.partsbnd.dcx");

            if (path != null)
            {
                ad.AssetPath = path;
                ad.AssetArchiveVirtualPath = $@"parts/{partsId}/tex";
            }
        }
        else if (Project.Type == ProjectType.ER)
        {
            var path = ResourceLocatorUtils.GetOverridenFilePath($@"parts\{partsId}.partsbnd.dcx");

            if (path != null)
            {
                ad.AssetPath = path;
                ad.AssetArchiveVirtualPath = $@"parts/{partsId}/tex";

                if(isLowDetail)
                {
                    ad.AssetArchiveVirtualPath = $@"parts/{partsId}/tex/low";
                }
            }
        }
        else if (Project.Type == ProjectType.DS3 || Project.Type == ProjectType.SDT)
        {
            var path = ResourceLocatorUtils.GetOverridenFilePath($@"parts\{partsId}.partsbnd.dcx");
            if (path != null)
            {
                ad.AssetPath = path;
                ad.AssetArchiveVirtualPath = $@"parts/{partsId}/tex";
            }
        }
        else if (Project.Type == ProjectType.BB)
        {
            var path = ResourceLocatorUtils.GetOverridenFilePath($@"parts\{partsId}.partsbnd.dcx");
            if (path != null)
            {
                ad.AssetPath = path;
                ad.AssetVirtualPath = $@"parts/{partsId}/tex";
            }
        }
        else if (Project.Type == ProjectType.DS1R)
        {
            var path = ResourceLocatorUtils.GetOverridenFilePath($@"parts\{partsId}.partsbnd.dcx");
            if (path != null)
            {
                ad.AssetPath = path;
                ad.AssetArchiveVirtualPath = $@"parts/{partsId}/tex";
            }
        }
        else if (Project.Type == ProjectType.DS1)
        {
            var path = ResourceLocatorUtils.GetOverridenFilePath($@"parts\{partsId}.partsbnd");
            if (path != null)
            {
                ad.AssetPath = path;
                ad.AssetArchiveVirtualPath = $@"parts/{partsId}/tex";
            }
        }
        else if (Project.Type == ProjectType.DES)
        {
            var path = ResourceLocatorUtils.GetOverridenFilePath($@"parts\{partsId}.partsbnd.dcx");
            if (path != null)
            {
                ad.AssetPath = path;
                ad.AssetArchiveVirtualPath = $@"parts/{partsId}/tex";
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

        if (Project.Type == ProjectType.ER)
        {
            path = ResourceLocatorUtils.GetOverridenFilePath($@"asset\aet\{resourceName.Substring(0, 6)}\{resourceName}.tpf.dcx");
        }
        else if (Project.Type is ProjectType.AC6)
        {
            path = ResourceLocatorUtils.GetOverridenFilePath($@"\asset\environment\texture\{resourceName}.tpf.dcx");
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

        if (Project.Type is ProjectType.AC6)
        {
            overrideFilePath = ResourceLocatorUtils.GetOverridenFilePath($@"asset\environment\texture\{resourceName}.tpf.dcx");
        }

        if (Project.Type is ProjectType.ER)
        {
            overrideFilePath = ResourceLocatorUtils.GetOverridenFilePath($@"asset\aet\{resourceName.Substring(0, 6)}\{resourceName}.tpf.dcx");
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
        if (Project.Type is ProjectType.AC6 or ProjectType.ER or ProjectType.SDT)
        {
            overrideFilePath = ResourceLocatorUtils.GetOverridenFilePath($@"menu\hi\{resourceName}.tpf.dcx");
        }

        if (Project.Type is ProjectType.DS3)
        {
            overrideFilePath = ResourceLocatorUtils.GetOverridenFilePath($@"menu\{resourceName}.tpf.dcx");
        }

        if (Project.Type is ProjectType.DS1R)
        {
            overrideFilePath = ResourceLocatorUtils.GetOverridenFilePath($@"menu\{resourceName}.tpf.dcx");
        }

        if (overrideFilePath != null)
        {
            return overrideFilePath;
        }
        // TPFBHD
        else
        {
            if (Project.Type is ProjectType.AC6 or ProjectType.ER or ProjectType.SDT)
            {
                overrideFilePath = ResourceLocatorUtils.GetOverridenFilePath($@"menu\hi\{resourceName}.tpfbhd");
            }

            if (Project.Type is ProjectType.DS3)
            {
                overrideFilePath = ResourceLocatorUtils.GetOverridenFilePath($@"menu\{resourceName}.tpfbhd");
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

        overrideFilePath = ResourceLocatorUtils.GetOverridenFilePath($@"other\{resourceName}.tpf.dcx");

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

        overrideFilePath = ResourceLocatorUtils.GetOverridenFilePath($@"sfx\{resourceName}.ffxbnd.dcx");

        if (overrideFilePath != null)
        {
            return overrideFilePath;
        }

        return null;
    }
}
