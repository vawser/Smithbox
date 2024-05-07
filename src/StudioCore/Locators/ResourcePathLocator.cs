using StudioCore.UserProject;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static SoulsFormats.MSB_AC6;

namespace StudioCore.Locators;

public static class ResourcePathLocator
{
    public static string TexturePathToVirtual(string texpath)
    {
        // MAP Texture
        if (texpath.Contains(@"\map\"))
        {
            var splits = texpath.Split('\\');
            var mapid = splits[splits.Length - 3];
            return $@"map/tex/{mapid}/{Path.GetFileNameWithoutExtension(texpath)}";
        }

        // CHR Texture
        if (texpath.Contains(@"\chr\"))
        {
            var splits = texpath.Split('\\');
            var chrid = splits[splits.Length - 3];
            return $@"chr/{chrid}/tex/{Path.GetFileNameWithoutExtension(texpath)}";
        }

        // OBJ Texture
        if (texpath.Contains(@"\obj\"))
        {
            var splits = texpath.Split('\\');
            var objid = splits[splits.Length - 3];
            return $@"obj/{objid}/tex/{Path.GetFileNameWithoutExtension(texpath)}";
        }

        // AET Texture
        if (texpath.Contains(@"\aet") || texpath.StartsWith("aet"))
        {
            var splits = texpath.Split('\\');
            var aetid = splits[splits.Length - 1].Substring(0, 6);
            return $@"aet/{aetid}/{Path.GetFileNameWithoutExtension(texpath)}";
        }

        // AAT Texture
        if (texpath.Contains(@"\aat") || texpath.StartsWith("aat"))
        {
            var name = Path.GetFileName(texpath);
            return $@"aat/{Path.GetFileNameWithoutExtension(texpath)}";
        }

        // SYSTEX Texture
        if (texpath.Contains(@"\systex") || texpath.StartsWith("systex"))
        {
            var name = Path.GetFileName(texpath);
            return $@"systex/{Path.GetFileNameWithoutExtension(texpath)}";
        }

        // PARTS Texture
        if (texpath.Contains(@"\parts\"))
        {
            var splits = texpath.Split('\\');
            var partsId = splits[splits.Length - 3];
            return $@"parts/{partsId}/tex/{Path.GetFileNameWithoutExtension(texpath)}";
        }

        return texpath;
    }

    public static string VirtualToRealPath(string virtualPath, out string bndpath)
    {
        var pathElements = virtualPath.Split('/');
        Regex mapRegex = new(@"^m\d{2}_\d{2}_\d{2}_\d{2}$");
        var ret = "";

        // Parse the virtual path with a DFA and convert it to a game path
        var i = 0;

        // MAP
        if (pathElements[i].Equals("map"))
        {
            i++;
            if (pathElements[i].Equals("tex"))
            {
                i++;
                if (Project.Type == ProjectType.DS2S)
                {
                    var mid = pathElements[i];
                    i++;
                    var id = pathElements[i];
                    if (id == "tex")
                    {
                        bndpath = "";
                        return ResourceLocatorUtils.GetAssetPath($@"model\map\t{mid.Substring(1)}.tpfbhd");
                    }
                }
                else if (Project.Type == ProjectType.DES)
                {
                    var mid = pathElements[i];
                    i++;
                    bndpath = "";
                    return ResourceLocatorUtils.GetAssetPath($@"map\{mid}\{mid}_{pathElements[i]}.tpf.dcx");
                }
                else
                {
                    var mid = pathElements[i];
                    i++;
                    bndpath = "";
                    if (pathElements[i] == "env")
                    {
                        if (Project.Type == ProjectType.DS1R)
                            return ResourceLocatorUtils.GetAssetPath($@"map\{mid}\GI_EnvM_{mid}.tpf.dcx");

                        return ResourceLocatorUtils.GetAssetPath($@"map\{mid}\{mid}_envmap.tpf.dcx");
                    }

                    return ResourceLocatorUtils.GetAssetPath($@"map\{mid}\{mid}_{pathElements[i]}.tpfbhd");
                }
            }
            else if (mapRegex.IsMatch(pathElements[i]))
            {
                var mapid = pathElements[i];
                i++;
                if (pathElements[i].Equals("model"))
                {
                    i++;
                    bndpath = "";
                    if (Project.Type == ProjectType.DS1)
                        return ResourceLocatorUtils.GetAssetPath($@"map\{mapid}\{pathElements[i]}.flver");

                    if (Project.Type == ProjectType.DS1R)
                        return ResourceLocatorUtils.GetAssetPath($@"map\{mapid}\{pathElements[i]}.flver.dcx");

                    if (Project.Type == ProjectType.DS2S)
                        return ResourceLocatorUtils.GetAssetPath($@"model\map\{mapid}.mapbhd");

                    if (Project.Type == ProjectType.BB || Project.Type == ProjectType.DES)
                        return ResourceLocatorUtils.GetAssetPath($@"map\{mapid}\{pathElements[i]}.flver.dcx");

                    if (Project.Type is ProjectType.ER or ProjectType.AC6)
                        return ResourceLocatorUtils.GetAssetPath($@"map\{mapid.Substring(0, 3)}\{mapid}\{pathElements[i]}.mapbnd.dcx");

                    return ResourceLocatorUtils.GetAssetPath($@"map\{mapid}\{pathElements[i]}.mapbnd.dcx");
                }

                if (pathElements[i].Equals("hit"))
                {
                    i++;
                    var hittype = pathElements[i];
                    i++;
                    if (Project.Type == ProjectType.DS1 || Project.Type == ProjectType.DES)
                    {
                        bndpath = "";
                        return ResourceLocatorUtils.GetAssetPath($@"map\{mapid}\{pathElements[i]}");
                    }

                    if (Project.Type == ProjectType.DS2S)
                    {
                        bndpath = "";
                        return ResourceLocatorUtils.GetAssetPath($@"model\map\h{mapid.Substring(1)}.hkxbhd");
                    }

                    if (Project.Type == ProjectType.DS3 || Project.Type == ProjectType.BB)
                    {
                        bndpath = "";
                        if (hittype == "lo")
                            return ResourceLocatorUtils.GetAssetPath($@"map\{mapid}\l{mapid.Substring(1)}.hkxbhd");

                        return ResourceLocatorUtils.GetAssetPath($@"map\{mapid}\h{mapid.Substring(1)}.hkxbhd");
                    }

                    bndpath = "";
                    return null;
                }

                if (pathElements[i].Equals("nav"))
                {
                    i++;
                    if (Project.Type == ProjectType.DS1 || Project.Type == ProjectType.DES ||
                        Project.Type == ProjectType.DS1R)
                    {
                        if (i < pathElements.Length)
                            bndpath = $@"{pathElements[i]}";
                        else
                            bndpath = "";

                        if (Project.Type == ProjectType.DS1R)
                            return ResourceLocatorUtils.GetAssetPath($@"map\{mapid}\{mapid}.nvmbnd.dcx");

                        return ResourceLocatorUtils.GetAssetPath($@"map\{mapid}\{mapid}.nvmbnd");
                    }

                    if (Project.Type == ProjectType.DS3)
                    {
                        bndpath = "";
                        return ResourceLocatorUtils.GetAssetPath($@"map\{mapid}\{mapid}.nvmhktbnd.dcx");
                    }

                    bndpath = "";
                    return null;
                }
            }
        }
        // CHARACTERS
        else if (pathElements[i].Equals("chr"))
        {
            i++;
            var chrid = pathElements[i];
            i++;
            if (pathElements[i].Equals("model"))
            {
                bndpath = "";
                if (Project.Type == ProjectType.DS1)
                    return ResourceLocatorUtils.GetOverridenFilePath($@"chr\{chrid}.chrbnd");

                if (Project.Type == ProjectType.DS2S)
                    return ResourceLocatorUtils.GetOverridenFilePath($@"model\chr\{chrid}.bnd");

                if (Project.Type == ProjectType.DES)
                    return ResourceLocatorUtils.GetOverridenFilePath($@"chr\{chrid}\{chrid}.chrbnd.dcx");

                return ResourceLocatorUtils.GetOverridenFilePath($@"chr\{chrid}.chrbnd.dcx");
            }

            if (pathElements[i].Equals("tex"))
            {
                bndpath = "";

                var isLowDetail = false;

                // This is so the ER chr textures _l will display in the Texture Viewer
                if (pathElements.Length == 4)
                {
                    i++;
                    if (pathElements[i].Equals("low"))
                    {
                        isLowDetail = true;
                    }
                }

                return ResourceTextureLocator.GetChrTexturePath(chrid, isLowDetail);
            }
        }
        // OBJECTS
        else if (pathElements[i].Equals("obj"))
        {
            i++;
            var objid = pathElements[i];
            i++;
            if (pathElements[i].Equals("model") || pathElements[i].Equals("tex"))
            {
                bndpath = "";
                if (Project.Type == ProjectType.DS1)
                    return ResourceLocatorUtils.GetOverridenFilePath($@"obj\{objid}.objbnd");

                if (Project.Type == ProjectType.DS2S)
                    return ResourceLocatorUtils.GetOverridenFilePath($@"model\obj\{objid}.bnd");

                if (Project.Type == ProjectType.ER)
                {
                    // Derive subfolder path from model name (all vanilla AEG are within subfolders)
                    if (objid.Length >= 6)
                        return ResourceLocatorUtils.GetOverridenFilePath($@"asset\aeg\{objid.Substring(0, 6)}\{objid}.geombnd.dcx");

                    return null;
                }

                if (Project.Type == ProjectType.AC6)
                {
                    if (objid.Length >= 6)
                        return ResourceLocatorUtils.GetOverridenFilePath($@"asset\environment\geometry\{objid}.geombnd.dcx");

                    return null;
                }

                return ResourceLocatorUtils.GetOverridenFilePath($@"obj\{objid}.objbnd.dcx");
            }
        }
        // PARTS
        else if (pathElements[i].Equals("parts"))
        {
            i++;
            var partsId = pathElements[i];
            i++;

            if (pathElements[i].Equals("model") || pathElements[i].Equals("tex"))
            {
                bndpath = "";

                if (Project.Type == ProjectType.DS1)
                    return ResourceLocatorUtils.GetOverridenFilePath($@"parts\{partsId}.partsbnd");

                if (Project.Type == ProjectType.DS2S)
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

                    return ResourceLocatorUtils.GetOverridenFilePath($@"model\parts\{partType}\{partsId}.bnd");
                }

                if (Project.Type is ProjectType.ER)
                {
                    // This is so the ER parts _l will display in the Texture Viewer
                    if (pathElements.Length == 4)
                    {
                        i++;
                        if (pathElements[i].Equals("low"))
                        {
                            ResourceLocatorUtils.GetOverridenFilePath($@"parts\{partsId}_l.partsbnd.dcx");
                        }
                    }
                }

                if (Project.Type == ProjectType.AC6 && pathElements[i].Equals("tex"))
                {
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
                        path = ResourceLocatorUtils.GetOverridenFilePath($@"parts\{partsId}_u.tpf.dcx");

                    return path;
                }

                return ResourceLocatorUtils.GetOverridenFilePath($@"parts\{partsId}.partsbnd.dcx");
            }
        }
        // MENU
        else if (pathElements[i].Equals("menu"))
        {
            i++;

            var containerName = pathElements[i];
            i++;

            if (pathElements[i].Equals("tex"))
            {
                bndpath = "";

                if (Project.Type == ProjectType.DS2S)
                {
                    var path = $@"menu\tex\icon\{containerName}.tpf";

                    if (containerName.Contains("ic_area_"))
                    {
                        path = $@"menu\tex\icon\bonfire_area\{containerName}.tpf";
                    }
                    else if (containerName.Contains("ic_list_"))
                    {
                        path = $@"menu\tex\icon\bonfire_list\{containerName}.tpf";
                    }
                    else if (containerName.Contains("ic_cm_"))
                    {
                        path = $@"menu\tex\icon\charamaking\{containerName}.tpf";
                    }
                    else if (containerName.Contains("ei_"))
                    {
                        path = $@"menu\tex\icon\effect\{containerName}.tpf";
                    }
                    else if (containerName.Contains("ic_ca"))
                    {
                        path = $@"menu\tex\icon\item_category\{containerName}.tpf";
                    }
                    else if (containerName.Contains("map_name_"))
                    {
                        // TODO: support all the languages
                        path = $@"menu\tex\icon\mapname\english\{containerName}.tpf";
                    }
                    else if (containerName.Contains("vi_"))
                    {
                        path = $@"menu\tex\icon\vow\{containerName}.tpf";
                    }

                    return ResourceLocatorUtils.GetOverridenFilePath(path);
                }

                return ResourceTextureLocator.GetMenuTextureContainerPath(containerName);
            }
        }
        // ASSET
        else if (pathElements[i].Equals("aet"))
        {
            i++;

            var containerName = pathElements[i];
            i++;

            if (pathElements[i].Equals("tex"))
            {
                bndpath = "";
                return ResourceTextureLocator.GetAssetTextureContainerPath(containerName);
            }
        }
        // Particle
        else if (pathElements[i].Equals("sfx"))
        {
            i++;

            var containerName = pathElements[i];
            i++;

            if (pathElements[i].Equals("tex"))
            {
                bndpath = "";
                return ResourceTextureLocator.GetParticleTextureContainerPath(containerName);
            }
        }
        // OTHER
        else if (pathElements[i].Equals("other"))
        {
            i++;

            var containerName = pathElements[i];
            i++;

            if (pathElements[i].Equals("tex"))
            {
                bndpath = "";
                return ResourceTextureLocator.GetOtherTextureContainerPath(containerName);
            }
        }

        bndpath = virtualPath;
        return null;
    }
}
