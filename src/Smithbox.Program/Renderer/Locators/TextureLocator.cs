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

        if (project.Descriptor.ProjectType is ProjectType.DS2S or ProjectType.DS2)
        {
            ResourceDescriptor t = new();
            t.AssetArchiveVirtualPath = $@"map/tex/{mapID}/tex";
            ads.Add(t);
        }
        else if (project.Descriptor.ProjectType is ProjectType.DES)
        {
            var mid = mapID.Substring(0, 3);

            foreach(var entry in project.Locator.FileDictionary.Entries)
            {
                if(entry.Folder == $"/map/{mid}" && entry.Extension == "tpf")
                {
                    ResourceDescriptor res = new();
                    res.AssetVirtualPath = $@"map/tex/{mid}/{entry.Filename.Substring(4, 4)}";
                    ads.Add(res);
                }
            }

            // The map's own TPF, map/mAA_BB_CC_DD/mAA_BB_CC_DD.tpf, holds textures the area TPFs don't (m03 mostly)
            if (mapID.Length > 3 && project.VFS.FS.FileExists(PathBuilder.SanitiseRelativePath(Path.Combine("map", mapID, $"{mapID}.tpf"))))
            {
                ResourceDescriptor own = new();
                own.AssetVirtualPath = $@"map/tex/{mapID}/own";
                ads.Add(own);
            }
        }
        else
        {
            var mid = mapID.Substring(0, 3);

            if (project.Descriptor.ProjectType is ProjectType.ER 
                or ProjectType.AC6 
                or ProjectType.NR
                or ProjectType.DB_NT
                or ProjectType.DB)
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

            if (project.Descriptor.ProjectType is ProjectType.DS1R)
            {
                ResourceDescriptor env = new();
                env.AssetArchiveVirtualPath = $@"map/tex/{mid}/env";
                ads.Add(env);
            }
            else if (project.Descriptor.ProjectType is ProjectType.BB or ProjectType.DS3)
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

        if (project.Descriptor.ProjectType is ProjectType.DES)
        {
            ad.AssetVirtualPath = $@"chr/{id}/tex";
        }
        else if (project.Descriptor.ProjectType is ProjectType.DS1 or ProjectType.DS1R or ProjectType.BB)
        {
            if (binder)
                ad.AssetArchiveVirtualPath = $@"chr/{id}/tex";
            else
                ad.AssetVirtualPath = $@"chr/{id}/tex";
        }
        else if (project.Descriptor.ProjectType is ProjectType.DS2S or ProjectType.DS2 or ProjectType.DS3 or ProjectType.SDT)
        {
            ad.AssetArchiveVirtualPath = $@"chr/{id}/tex";
        }
        else if (project.Descriptor.ProjectType is ProjectType.ER 
            or ProjectType.AC6 
            or ProjectType.NR
            or ProjectType.DB_NT
            or ProjectType.DB)
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

    /// <summary>
    /// The shared TPFs of Demon's Souls, parts/common_body.tpf and parts/common_ghost.tpf: the body and ghost textures
    /// that player parts and several characters point at through ...\parts\tex\
    /// </summary>
    public static List<ResourceDescriptor> GetDesCommonTextureVirtualPaths(ProjectEntry project)
    {
        return new List<ResourceDescriptor>
        {
            GetCharacterCommonTextureVirtualPath(project, "common_body"),
            GetCharacterCommonTextureVirtualPath(project, "common_ghost")
        };
    }

    /// <summary>
    /// The texture containers a Demon's Souls texture path names. DES models often use textures of another model, and
    /// their paths still name where it came from: ...\chr\c2000\tex\, ...\obj\o0000\tex\, ...\map\m02\tex\,
    /// ...\parts\Weapon\WP_A_1509\tex\, or ...\parts\tex\ for the common TPFs. Only containers that exist are returned.
    /// </summary>
    public static List<ResourceDescriptor> GetDesReferencedTextureContainers(ProjectEntry project, string texturePath)
    {
        List<ResourceDescriptor> result = new();

        if (string.IsNullOrWhiteSpace(texturePath))
            return result;

        var segs = texturePath.Replace('\\', '/').ToLowerInvariant().Split('/', StringSplitOptions.RemoveEmptyEntries);
        var texIndex = Array.LastIndexOf(segs, "tex");

        if (texIndex < 1)
            return result;

        // The nearest kind folder before the tex folder
        var kindIndex = -1;
        for (var i = texIndex - 1; i >= 0 && kindIndex < 0; i--)
        {
            if (segs[i] is "chr" or "obj" or "map" or "parts")
                kindIndex = i;
        }

        if (kindIndex < 0)
            return result;

        var kind = segs[kindIndex];
        var id = kindIndex + 1 < texIndex ? segs[kindIndex + 1] : null;

        // Thousands of map piece textures name the same few containers, and the map lookup walks the file list
        var cache = _desReferenceCache.GetValue(project, _ => new());
        var cacheKey = kind == "parts" ? $"parts/{(texIndex - 1 > kindIndex ? segs[texIndex - 1] : "")}" : $"{kind}/{id}";

        return cache.GetOrAdd(cacheKey, _ => ResolveDesReferencedTextureContainers(project, segs, kind, id, kindIndex, texIndex));
    }

    private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<ProjectEntry,
        System.Collections.Concurrent.ConcurrentDictionary<string, List<ResourceDescriptor>>> _desReferenceCache = new();

    private static List<ResourceDescriptor> ResolveDesReferencedTextureContainers(ProjectEntry project, string[] segs,
        string kind, string id, int kindIndex, int texIndex)
    {
        List<ResourceDescriptor> result = new();

        switch (kind)
        {
            case "chr" when id != null:
                result.Add(GetCharacterTextureVirtualPath(project, id));
                break;
            case "obj" when id != null:
                result.Add(GetObjectTextureVirtualPath(project, id));
                break;
            case "map" when id != null && id.Length >= 3:
                result.AddRange(GetMapTextureVirtualPaths(project, id));
                break;
            case "parts":
                // parts\<Kind>\<ID>\tex names a part; parts\tex the common TPFs
                if (texIndex - 1 > kindIndex)
                    result.Add(GetPartTextureVirtualPath(project, segs[texIndex - 1]));
                else
                    result.AddRange(GetDesCommonTextureVirtualPaths(project));
                break;
        }

        return result.Where(d =>
        {
            var path = d.IsValid() ? d.GetContainerPath(project) : null;
            return path != null && project.VFS.FS.FileExists(path);
        }).ToList();
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

    /// <summary>
    /// An envmap file of a map (AC6, ER, NR), which holds its irradiance volumes, e.g. m10_00_00_00_envmap_00_high_00
    /// </summary>
    /// <summary>
    /// DS3's binder of the lightmaps of an area's map pieces, map/mAA/gi_env_mAA.tpfbhd, whose textures are named
    /// AABBCCDD_mXXXXXX_gi_NNNN_00_dol_0K, see DolLightmapBank
    /// </summary>
    public static ResourceDescriptor GetDolLightmapVirtualPath(string mapId)
    {
        ResourceDescriptor ad = new();

        ad.AssetArchiveVirtualPath = $@"map/tex/{mapId.Substring(0, 3)}/gienv";

        return ad;
    }

    public static ResourceDescriptor GetEnvMapTextureVirtualPath(string mapId, string fileName)
    {
        ResourceDescriptor ad = new();

        ad.AssetArchiveVirtualPath = null;
        ad.AssetVirtualPath = $@"map/{mapId}/envmap/{fileName}";
        ad.AssetName = fileName;

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
        var curProject = Smithbox.Orchestrator.SelectedProject;
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
