using StudioCore.Core.Project;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static SoulsFormats.MSB_AC6;

namespace StudioCore.Resource.Locators;

public static class VirtualPathLocator
{
    public static string TexturePathToVirtual(string texpath)
    {
        // HACK: Only include texture name and not full virtual path
        return Path.GetFileNameWithoutExtension(texpath);

        /*
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
        */
    }

    public static string VirtualToRealPath(string virtualPath, out string bndpath)
    {
        var pathElements = virtualPath.Split('/');

        // Parse the virtual path with a DFA and convert it to a game path
        var i = 0;

        // LOOSE
        if (pathElements[i].Equals("loose"))
        {
            bndpath = "";
            i++;

            // Used to load loose FLVER files that may have any path
            if (pathElements[i].Equals("flver"))
            {
                i++;
                var loosePath = pathElements[i];
                return loosePath;
            }
        }

        // MAP
        if (pathElements[i].Equals("map"))
        {
            i++;
            if (pathElements[i].Equals("tex"))
            {
                i++;
                if (Smithbox.ProjectType == ProjectType.DS2S || Smithbox.ProjectType == ProjectType.DS2)
                {
                    var mid = pathElements[i];
                    i++;
                    var id = pathElements[i];
                    if (id == "tex")
                    {
                        bndpath = "";
                        return LocatorUtils.GetAssetPath($@"model\map\t{mid.Substring(1)}.tpfbhd");
                    }
                }
                else if (Smithbox.ProjectType == ProjectType.DES)
                {
                    var mid = pathElements[i];
                    i++;
                    bndpath = "";
                    return LocatorUtils.GetAssetPath($@"map\{mid}\{mid}_{pathElements[i]}.tpf.dcx");
                }
                else if (Smithbox.ProjectType == ProjectType.ACFA)
                {
                    var mid = pathElements[i];
                    bndpath = "";
                    return LocatorUtils.GetAssetPath($@"model\map\{mid}\{mid}_t.bnd");
                }
                else if (Smithbox.ProjectType == ProjectType.ACV)
                {
                    var mid = pathElements[i];
                    i++;
                    bndpath = "";
                    return LocatorUtils.GetAssetPath($@"model\map\{mid}\{pathElements[i]}.tpf.dcx");
                }
                else if (Smithbox.ProjectType == ProjectType.ACVD)
                {
                    var mid = pathElements[i];
                    bndpath = "";
                    return LocatorUtils.GetAssetPath($@"model\map\{mid}\{mid}_htdcx.bnd");
                }
                else
                {
                    var mid = pathElements[i];
                    i++;
                    var id = pathElements[i];
                    bndpath = "";

                    if (pathElements[i] == "env")
                    {
                        if (Smithbox.ProjectType == ProjectType.DS1R)
                            return LocatorUtils.GetAssetPath($@"map\{mid}\GI_EnvM_{mid}.tpf.dcx");

                        return LocatorUtils.GetAssetPath($@"map\{mid}\{mid}_envmap.tpf.dcx");
                    }

                    return LocatorUtils.GetAssetPath($@"map\{mid}\{mid}_{id}.tpfbhd");
                }
            }
            else if (GeneratedRegexMethods.IsMapId(pathElements[i]))
            {
                var mapid = pathElements[i];
                i++;
                if (pathElements[i].Equals("model"))
                {
                    i++;
                    bndpath = "";
                    if (Smithbox.ProjectType == ProjectType.DS1)
                        return LocatorUtils.GetAssetPath($@"map\{mapid}\{pathElements[i]}.flver");

                    if (Smithbox.ProjectType == ProjectType.DS1R)
                        return LocatorUtils.GetAssetPath($@"map\{mapid}\{pathElements[i]}.flver.dcx");

                    if (Smithbox.ProjectType == ProjectType.DS2S || Smithbox.ProjectType == ProjectType.DS2)
                        return LocatorUtils.GetAssetPath($@"model\map\{mapid}.mapbhd");

                    if (Smithbox.ProjectType == ProjectType.BB || Smithbox.ProjectType == ProjectType.DES)
                        return LocatorUtils.GetAssetPath($@"map\{mapid}\{pathElements[i]}.flver.dcx");

                    if (Smithbox.ProjectType is ProjectType.ER or ProjectType.AC6)
                        return LocatorUtils.GetAssetPath($@"map\{mapid.Substring(0, 3)}\{mapid}\{pathElements[i]}.mapbnd.dcx");

                    if (Smithbox.ProjectType is ProjectType.ACFA)
                        return LocatorUtils.GetAssetPath($@"model\map\{mapid}\{mapid}_m.bnd");

                    if (Smithbox.ProjectType is ProjectType.ACV or ProjectType.ACVD)
                        return LocatorUtils.GetAssetPath($@"model\map\{mapid}\{mapid}_m.dcx.bnd");

                    return LocatorUtils.GetAssetPath($@"map\{mapid}\{pathElements[i]}.mapbnd.dcx");
                }

                if (pathElements[i].Equals("hit"))
                {
                    i++;
                    var hittype = pathElements[i];
                    i++;
                    if (Smithbox.ProjectType == ProjectType.DS1 || Smithbox.ProjectType == ProjectType.DES)
                    {
                        bndpath = "";
                        return LocatorUtils.GetAssetPath($@"map\{mapid}\{pathElements[i]}");
                    }
                    else if (Smithbox.ProjectType == ProjectType.DS1R)
                    {
                        if (CFG.Current.PTDE_Collision_Root != "")
                        {
                            if (Directory.Exists(CFG.Current.PTDE_Collision_Root))
                            {
                                bndpath = "";
                                return LocatorUtils.GetAssetPath_CollisionHack($@"map\{mapid}\{pathElements[i]}");
                            }
                        }
                    }

                    if (Smithbox.ProjectType == ProjectType.DS2S || Smithbox.ProjectType == ProjectType.DS2)
                    {
                        bndpath = "";
                        return LocatorUtils.GetAssetPath($@"model\map\h{mapid.Substring(1)}.hkxbhd");
                    }

                    if (Smithbox.ProjectType == ProjectType.DS3 || Smithbox.ProjectType == ProjectType.BB)
                    {
                        bndpath = "";
                        if (hittype == "lo")
                            return LocatorUtils.GetAssetPath($@"map\{mapid}\l{mapid.Substring(1)}.hkxbhd");

                        return LocatorUtils.GetAssetPath($@"map\{mapid}\h{mapid.Substring(1)}.hkxbhd");
                    }

                    if (Smithbox.ProjectType == ProjectType.ER)
                    {
                        bndpath = "";
                        if (hittype == "lo")
                        {
                            return LocatorUtils.GetAssetPath($@"map\{mapid.Substring(0, 3)}\{mapid}\l{mapid.Substring(1)}.hkxbhd");
                        }
                        else if (hittype == "hi")
                        {
                            return LocatorUtils.GetAssetPath($@"map\{mapid.Substring(0, 3)}\{mapid}\h{mapid.Substring(1)}.hkxbhd");
                        }
                    }

                    bndpath = "";
                    return null;
                }

                if (pathElements[i].Equals("nav"))
                {
                    i++;
                    if (Smithbox.ProjectType == ProjectType.DS1 || Smithbox.ProjectType == ProjectType.DES ||
                        Smithbox.ProjectType == ProjectType.DS1R)
                    {
                        if (i < pathElements.Length)
                            bndpath = $@"{pathElements[i]}";
                        else
                            bndpath = "";

                        if (Smithbox.ProjectType == ProjectType.DS1R)
                            return LocatorUtils.GetAssetPath($@"map\{mapid}\{mapid}.nvmbnd.dcx");

                        return LocatorUtils.GetAssetPath($@"map\{mapid}\{mapid}.nvmbnd");
                    }

                    if (Smithbox.ProjectType == ProjectType.DS3)
                    {
                        bndpath = "";
                        return LocatorUtils.GetAssetPath($@"map\{mapid}\{mapid}.nvmhktbnd.dcx");
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
                if (Smithbox.ProjectType == ProjectType.DS1)
                    return LocatorUtils.GetOverridenFilePath($@"chr\{chrid}.chrbnd");

                if (Smithbox.ProjectType == ProjectType.DS2S || Smithbox.ProjectType == ProjectType.DS2)
                    return LocatorUtils.GetOverridenFilePath($@"model\chr\{chrid}.bnd");

                if (Smithbox.ProjectType == ProjectType.DES)
                    return LocatorUtils.GetOverridenFilePath($@"chr\{chrid}\{chrid}.chrbnd.dcx");

                return LocatorUtils.GetOverridenFilePath($@"chr\{chrid}.chrbnd.dcx");
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

                return TextureLocator.GetChrTexturePath(chrid, isLowDetail);
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
                if (Smithbox.ProjectType == ProjectType.DS1)
                    return LocatorUtils.GetOverridenFilePath($@"obj\{objid}.objbnd");

                if (Smithbox.ProjectType == ProjectType.DS2S || Smithbox.ProjectType == ProjectType.DS2)
                    return LocatorUtils.GetOverridenFilePath($@"model\obj\{objid}.bnd");

                if (Smithbox.ProjectType is ProjectType.ACFA)
                    if (pathElements[i].Equals("model"))
                        return LocatorUtils.GetOverridenFilePath($@"model\obj\{objid}\{objid}_m.bnd");
                    else if (pathElements[i].Equals("tex"))
                        return LocatorUtils.GetOverridenFilePath($@"model\obj\{objid}\{objid}_t.bnd");

                if (Smithbox.ProjectType is ProjectType.ACV or ProjectType.ACVD)
                    if (pathElements[i].Equals("model"))
                        return LocatorUtils.GetOverridenFilePath($@"model\obj\{objid}\{objid}_m.bnd.dcx");
                    else if (pathElements[i].Equals("tex"))
                        return LocatorUtils.GetOverridenFilePath($@"model\obj\{objid}\{objid}.tpf.dcx");

                if (Smithbox.ProjectType == ProjectType.ER)
                {
                    // Derive subfolder path from model name (all vanilla AEG are within subfolders)
                    if (objid.Length >= 6)
                    {
                        return LocatorUtils.GetOverridenFilePath($@"asset\aeg\{objid.Substring(0, 6)}\{objid}.geombnd.dcx");
                    }
                    return null;
                }

                if (Smithbox.ProjectType == ProjectType.AC6)
                {
                    if (objid.Length >= 6)
                        return LocatorUtils.GetOverridenFilePath($@"asset\environment\geometry\{objid}.geombnd.dcx");

                    return null;
                }

                return LocatorUtils.GetOverridenFilePath($@"obj\{objid}.objbnd.dcx");
            }
            if (pathElements[i].Equals("collision"))
            {
                i++;
                var colName = Path.GetFileNameWithoutExtension(pathElements[i]);
                i++;

                bndpath = "";

                if (Smithbox.ProjectType == ProjectType.ER)
                {
                    // Derive subfolder path from model name (all vanilla AEG are within subfolders)
                    if (objid.Length >= 6)
                    {
                        var path = LocatorUtils.GetOverridenFilePath($@"asset\aeg\{objid.Substring(0, 6)}\{colName}.geomhkxbnd.dcx");
                        return path;
                    }
                    return null;
                }

                if (Smithbox.ProjectType == ProjectType.AC6)
                {
                    if (objid.Length >= 6)
                        return LocatorUtils.GetOverridenFilePath($@"asset\environment\geometry\{colName}.geomhkxbnd.dcx");

                    return null;
                }

                return LocatorUtils.GetOverridenFilePath($@"obj\{objid}.objbnd.dcx");
            }
        }
        // ENEMIES
        else if (pathElements[i].Equals("ene"))
        {
            i++;
            var eneid = pathElements[i];
            i++;

            if (pathElements[i].Equals("model"))
            {
                if (Smithbox.ProjectType == ProjectType.ACFA)
                {
                    bndpath = "";
                    return LocatorUtils.GetOverridenFilePath($@"model\ene\{eneid}\{eneid}_m.bnd");
                }

                bndpath = "";
                return LocatorUtils.GetOverridenFilePath($@"model\ene\{eneid}\{eneid}_m.bnd.dcx");
            }

            if (pathElements[i].Equals("tex"))
            {
                if (Smithbox.ProjectType == ProjectType.ACFA)
                {
                    bndpath = "";
                    return LocatorUtils.GetOverridenFilePath($@"model\ene\{eneid}\{eneid}_t.bnd");
                }

                bndpath = "";
                return LocatorUtils.GetOverridenFilePath($@"model\ene\{eneid}\{eneid}.tpf.dcx");
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

                if (Smithbox.ProjectType == ProjectType.DS1)
                    return LocatorUtils.GetOverridenFilePath($@"parts\{partsId}.partsbnd");

                if (Smithbox.ProjectType == ProjectType.DS2S || Smithbox.ProjectType == ProjectType.DS2)
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

                    return LocatorUtils.GetOverridenFilePath($@"model\parts\{partType}\{partsId}.bnd");
                }

                if (Smithbox.ProjectType is ProjectType.ER)
                {
                    if (pathElements.Length == 4)
                    {
                        i++;
                        if (pathElements[i].Equals("low"))
                        {
                            return LocatorUtils.GetOverridenFilePath($@"parts\{partsId}_l.partsbnd.dcx");
                        }
                    }

                    if (partsId == "common_body")
                    {
                        return LocatorUtils.GetOverridenFilePath($@"parts\{partsId}.tpf.dcx");
                    }
                }

                if (Smithbox.ProjectType == ProjectType.AC6 && pathElements[i].Equals("tex"))
                {
                    if (pathElements.Length == 4)
                    {
                        i++;
                        if (pathElements[i].Equals("low"))
                        {
                            return LocatorUtils.GetOverridenFilePath($@"parts\{partsId}_l.partsbnd.dcx");
                        }
                        else if (pathElements[i].Equals("tpf"))
                        {
                            var path = "";

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

                            return path;
                        }
                    }
                }

                return LocatorUtils.GetOverridenFilePath($@"parts\{partsId}.partsbnd.dcx");
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

                if (Smithbox.ProjectType == ProjectType.DS2S || Smithbox.ProjectType == ProjectType.DS2)
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

                    return LocatorUtils.GetOverridenFilePath(path);
                }

                return TextureLocator.GetMenuTextureContainerPath(containerName);
            }
        }
        // SMITHBOX
        else if (pathElements[i].Equals("smithbox"))
        {
            bndpath = "";

            i++;

            if (pathElements[i].Equals("worldmap"))
            {
                return $"{AppContext.BaseDirectory}/Assets/WorldMap/world_map.tpf.dcx";
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
                return TextureLocator.GetAssetTextureContainerPath(containerName);
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
                return TextureLocator.GetParticleTextureContainerPath(containerName);
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
                return TextureLocator.GetOtherTextureContainerPath(containerName);
            }
        }
        // DIRECT
        else if (pathElements[i].Equals("direct"))
        {
            i++;
            var type = pathElements[i];

            bndpath = "";
            return $"direct/{type}";
        }

        bndpath = virtualPath;
        return null;
    }
}
