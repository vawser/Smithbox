using SoulsFormats;
using StudioCore.UserProject;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.AssetLocator;
public static class TextureAssetLocator
{
    public static List<AssetDescription> GetMapTextures(string mapid)
    {
        List<AssetDescription> ads = new();

        if (Project.Type == ProjectType.DS2S)
        {
            AssetDescription t = new();
            t.AssetPath = LocatorUtils.GetAssetPath($@"model\map\t{mapid.Substring(1)}.tpfbhd");
            t.AssetArchiveVirtualPath = $@"map/tex/{mapid}/tex";
            ads.Add(t);
        }
        else if (Project.Type == ProjectType.DS1)
        {
            // TODO
        }
        else if (Project.Type == ProjectType.ER)
        {
            // TODO ER
        }
        else if (Project.Type == ProjectType.AC6)
        {
            // TODO AC6
        }
        else if (Project.Type == ProjectType.DES)
        {
            var mid = mapid.Substring(0, 3);
            var paths = Directory.GetFileSystemEntries($@"{Project.GameRootDirectory}\map\{mid}\", "*.tpf.dcx");
            foreach (var path in paths)
            {
                AssetDescription ad = new();
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

            AssetDescription t0000 = new();
            t0000.AssetPath = LocatorUtils.GetAssetPath($@"map\{mid}\{mid}_0000.tpfbhd");
            t0000.AssetArchiveVirtualPath = $@"map/tex/{mid}/0000";
            ads.Add(t0000);

            AssetDescription t0001 = new();
            t0001.AssetPath = LocatorUtils.GetAssetPath($@"map\{mid}\{mid}_0001.tpfbhd");
            t0001.AssetArchiveVirtualPath = $@"map/tex/{mid}/0001";
            ads.Add(t0001);

            AssetDescription t0002 = new();
            t0002.AssetPath = LocatorUtils.GetAssetPath($@"map\{mid}\{mid}_0002.tpfbhd");
            t0002.AssetArchiveVirtualPath = $@"map/tex/{mid}/0002";
            ads.Add(t0002);

            AssetDescription t0003 = new();
            t0003.AssetPath = LocatorUtils.GetAssetPath($@"map\{mid}\{mid}_0003.tpfbhd");
            t0003.AssetArchiveVirtualPath = $@"map/tex/{mid}/0003";
            ads.Add(t0003);

            if (Project.Type == ProjectType.DS1R)
            {
                AssetDescription env = new();
                env.AssetPath = LocatorUtils.GetAssetPath($@"map\{mid}\GI_EnvM_{mid}.tpfbhd");
                env.AssetArchiveVirtualPath = $@"map/tex/{mid}/env";
                ads.Add(env);
            }
            else if (Project.Type == ProjectType.BB || Project.Type == ProjectType.DS3)
            {
                AssetDescription env = new();
                env.AssetPath = LocatorUtils.GetAssetPath($@"map\{mid}\{mid}_envmap.tpf.dcx");
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
            if (File.Exists(LocatorUtils.GetAssetPath($@"map\{mid}\{mid}_envmap.tpf.dcx")))
            {
                var t = TPF.Read(LocatorUtils.GetAssetPath($@"map\{mid}\{mid}_envmap.tpf.dcx"));
                foreach (TPF.Texture tex in t.Textures)
                    l.Add(tex.Name);
            }
        }

        return l;
    }

    public static string GetChrTexturePath(string chrid)
    {
        if (Project.Type is ProjectType.DES)
            return LocatorUtils.GetOverridenFilePath($@"chr\{chrid}\{chrid}.tpf");

        if (Project.Type is ProjectType.DS1)
        {
            var path = LocatorUtils.GetOverridenFilePath($@"chr\{chrid}\{chrid}.tpf");
            if (path != null)
                return path;

            return LocatorUtils.GetOverridenFilePath($@"chr\{chrid}.chrbnd");
        }

        if (Project.Type is ProjectType.DS2S)
            return LocatorUtils.GetOverridenFilePath($@"model\chr\{chrid}.texbnd");

        if (Project.Type is ProjectType.DS1R)
            // TODO: Some textures require getting chrtpfbhd from chrbnd, then using it with chrtpfbdt in chr folder.
            return LocatorUtils.GetOverridenFilePath($@"chr\{chrid}.chrbnd");

        if (Project.Type is ProjectType.BB)
            return LocatorUtils.GetOverridenFilePath($@"chr\{chrid}_2.tpf.dcx");

        if (Project.Type is ProjectType.DS3 or ProjectType.SDT)
            return LocatorUtils.GetOverridenFilePath($@"chr\{chrid}.texbnd.dcx");

        if (Project.Type is ProjectType.ER)
            // TODO: Maybe add an option down the line to load lower quality
            return LocatorUtils.GetOverridenFilePath($@"chr\{chrid}_h.texbnd.dcx");

        if (Project.Type is ProjectType.AC6)
            return LocatorUtils.GetOverridenFilePath($@"chr\{chrid}.texbnd.dcx");

        return null;
    }

    public static AssetDescription GetChrTextures(string chrid)
    {
        AssetDescription ad = new();
        ad.AssetArchiveVirtualPath = null;
        ad.AssetPath = null;
        if (Project.Type is ProjectType.DES)
        {
            var path = GetChrTexturePath(chrid);
            if (path != null)
            {
                ad.AssetPath = path;
                ad.AssetVirtualPath = $@"chr/{chrid}/tex";
            }
        }
        else if (Project.Type is ProjectType.DS1)
        {
            var path = GetChrTexturePath(chrid);
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
            var path = GetChrTexturePath(chrid);
            if (path != null)
            {
                ad = new AssetDescription();
                ad.AssetPath = path;
                ad.AssetVirtualPath = $@"chr/{chrid}/tex";
            }
        }
        else if (Project.Type is ProjectType.DS2S)
        {
            var path = GetChrTexturePath(chrid);
            if (path != null)
            {
                ad = new AssetDescription();
                ad.AssetPath = path;
                ad.AssetVirtualPath = $@"chr/{chrid}/tex";
            }
        }
        else if (Project.Type is ProjectType.BB)
        {
            var path = GetChrTexturePath(chrid);
            if (path != null)
            {
                ad.AssetPath = path;
                ad.AssetVirtualPath = $@"chr/{chrid}/tex";
            }
        }
        else if (Project.Type is ProjectType.DS3 or ProjectType.SDT)
        {
            var path = GetChrTexturePath(chrid);
            if (path != null)
            {
                ad.AssetPath = path;
                ad.AssetArchiveVirtualPath = $@"chr/{chrid}/tex";
            }
        }
        else if (Project.Type is ProjectType.ER or ProjectType.AC6)
        {
            var path = GetChrTexturePath(chrid);
            if (path != null)
            {
                ad.AssetPath = path;
                ad.AssetArchiveVirtualPath = $@"chr/{chrid}/tex";
            }
        }

        return ad;
    }

    public static AssetDescription GetObjTexture(string obj)
    {
        AssetDescription ad = new();
        ad.AssetPath = null;
        ad.AssetArchiveVirtualPath = null;
        string path = null;
        if (Project.Type == ProjectType.DS1)
            path = LocatorUtils.GetOverridenFilePath($@"obj\{obj}.objbnd");
        else if (Project.Type is ProjectType.DES or ProjectType.DS1R or ProjectType.BB
                 or ProjectType.DS3 or ProjectType.SDT)
            path = LocatorUtils.GetOverridenFilePath($@"obj\{obj}.objbnd.dcx");

        if (path != null)
        {
            ad.AssetPath = path;
            ad.AssetArchiveVirtualPath = $@"obj/{obj}/tex";
        }

        return ad;
    }

    public static AssetDescription GetAetTexture(string aetid)
    {
        AssetDescription ad = new();
        ad.AssetPath = null;
        ad.AssetArchiveVirtualPath = null;
        string path;
        if (Project.Type == ProjectType.ER)
            path = LocatorUtils.GetOverridenFilePath($@"asset\aet\{aetid.Substring(0, 6)}\{aetid}.tpf.dcx");
        else if (Project.Type is ProjectType.AC6)
            path = LocatorUtils.GetOverridenFilePath($@"\asset\environment\texture\{aetid}.tpf.dcx");
        else
            throw new NotSupportedException();

        if (path != null)
        {
            ad.AssetPath = path;
            ad.AssetArchiveVirtualPath = $@"aet/{aetid}/tex";
        }

        return ad;
    }

    public static AssetDescription GetPartTextures(string partsId)
    {
        AssetDescription ad = new();
        ad.AssetArchiveVirtualPath = null;
        ad.AssetPath = null;
        if (Project.Type == ProjectType.AC6)
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
                path = LocatorUtils.GetOverridenFilePath($@"parts\{partsId}_u.tpf.dcx");

            if (path != null)
            {
                ad.AssetPath = path;
                ad.AssetVirtualPath = $@"parts/{partsId}/tex";
            }
        }
        else if (Project.Type == ProjectType.ER)
        {
            // Maybe add an option down the line to load lower quality
            var path = LocatorUtils.GetOverridenFilePath($@"parts\{partsId}.partsbnd.dcx");
            if (path != null)
            {
                ad.AssetPath = path;
                ad.AssetArchiveVirtualPath = $@"parts/{partsId}/tex";
            }
        }
        else if (Project.Type == ProjectType.DS3 || Project.Type == ProjectType.SDT)
        {
            var path = LocatorUtils.GetOverridenFilePath($@"parts\{partsId}.partsbnd.dcx");
            if (path != null)
            {
                ad.AssetPath = path;
                ad.AssetArchiveVirtualPath = $@"parts/{partsId}/tex";
            }
        }
        else if (Project.Type == ProjectType.BB)
        {
            var path = LocatorUtils.GetOverridenFilePath($@"parts\{partsId}.partsbnd.dcx");
            if (path != null)
            {
                ad.AssetPath = path;
                ad.AssetVirtualPath = $@"parts/{partsId}/tex";
            }
        }
        else if (Project.Type == ProjectType.DS1)
        {
            var path = LocatorUtils.GetOverridenFilePath($@"parts\{partsId}.partsbnd");
            if (path != null)
            {
                ad.AssetPath = path;
                ad.AssetArchiveVirtualPath = $@"parts/{partsId}/tex";
            }
        }
        else if (Project.Type == ProjectType.DES)
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
}
