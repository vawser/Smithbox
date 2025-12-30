using StudioCore.Application;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StudioCore.Renderer;

public class PathBuilder
{
    /// <summary>
    /// This transforms the virtual path into the relative path required for the VFS
    /// </summary>
    /// <param name="virtPath"></param>
    /// <returns></returns>
    public static string GetRelativePath(ProjectEntry project, string virtPath)
    {
        var relPath = "";

        var p = virtPath.Split('/');
        var i = 0;

        // --- Loose ---
        if (p[i].Equals("loose"))
        {
            i++;

            if (p[i].Equals("flver"))
            {
                i++;
                var loosePath = p[i];
                return loosePath;
            }
        }
        // --- Map ---
        else if (p[i].Equals("map"))
        {
            i++;

            // Map Textures
            if (p[i].Equals("tex"))
            {
                i++;
                if (project.ProjectType is ProjectType.DS2S or ProjectType.DS2)
                {
                    var mid = p[i];

                    i++;

                    var id = p[i];

                    if (id == "tex")
                    {
                        relPath = Path.Combine("model", "map", $"t{mid.Substring(1)}.tpfbhd");
                    }
                }
                else if (project.ProjectType is ProjectType.DES)
                {
                    var mid = p[i];
                    i++;

                    relPath = Path.Combine("map", mid, $"{mid}_{p[i]}.tpf.dcx");
                }
                else
                {
                    var mid = p[i];
                    i++;
                    var id = p[i];

                    relPath = Path.Combine("map", mid, $"{mid}_{id}.tpfbhd");
                }
            }
            // Map
            else if (GeneratedRegexMethods.IsMapId(p[i]))
            {
                var mapid = p[i];
                i++;

                // Models
                if (p[i].Equals("model"))
                {
                    i++;

                    if (project.ProjectType is ProjectType.DS1)
                    {
                        relPath = Path.Combine("map", mapid, $"{p[i]}.flver");
                    }
                    else if (project.ProjectType is ProjectType.DS1R)
                    {
                        relPath = Path.Combine("map", mapid, $"{p[i]}.flver.dcx");
                    }
                    else if (project.ProjectType is ProjectType.DS2S or ProjectType.DS2)
                    {
                        relPath = Path.Combine("model", "map", $"{mapid}.mapbhd");
                    }
                    else if (project.ProjectType is ProjectType.BB or ProjectType.DES)
                    {
                        relPath = Path.Combine("map", mapid, $"{p[i]}.flver.dcx");
                    }
                    else if (project.ProjectType is ProjectType.ER or ProjectType.AC6 or ProjectType.NR)
                    {
                        relPath = Path.Combine("map", mapid.Substring(0, 3), mapid, $"{p[i]}.mapbnd.dcx");
                    }
                    else
                    {
                        relPath = Path.Combine("map", mapid, $"{p[i]}.mapbnd.dcx");
                    }
                }
                // Collisions
                else if (p[i].Equals("hit"))
                {
                    i++;
                    var hittype = p[i];
                    i++;

                    if (project.ProjectType is ProjectType.DS1 or ProjectType.DES)
                    {
                        relPath = Path.Combine("map", mapid, p[i]);
                    }
                    else if (project.ProjectType == ProjectType.DS1R)
                    {
                        if (CFG.Current.PTDE_Collision_Root != "")
                        {
                            if (Directory.Exists(CFG.Current.PTDE_Collision_Root))
                            {
                                relPath = Path.Join(
                                    CFG.Current.PTDE_Collision_Root, "map", mapid, p[i]);
                            }
                        }
                    }
                    else if (project.ProjectType is ProjectType.DS2S or ProjectType.DS2)
                    {
                        relPath = Path.Combine("model", "map", $"h{mapid.Substring(1)}.hkxbhd");
                    }
                    else if (project.ProjectType is ProjectType.DS3 or ProjectType.BB or ProjectType.SDT)
                    {
                        if (hittype == "lo")
                        {
                            relPath = Path.Combine("map", mapid, $"l{mapid.Substring(1)}.hkxbhd");
                        }
                        else
                        {
                            relPath = Path.Combine("map", mapid, $"h{mapid.Substring(1)}.hkxbhd");
                        }
                    }
                    else if (project.ProjectType is ProjectType.ER or ProjectType.NR or ProjectType.AC6)
                    {
                        if (hittype == "lo")
                        {
                            relPath = Path.Combine("map", mapid.Substring(0, 3), mapid, $"l{mapid.Substring(1)}.hkxbhd");
                        }
                        else if (hittype == "hi")
                        {
                            relPath = Path.Combine("map", mapid.Substring(0, 3), mapid, $"h{mapid.Substring(1)}.hkxbhd");
                        }
                        else if (hittype == "fa")
                        {
                            relPath = Path.Combine("map", mapid.Substring(0, 3), mapid, $"f{mapid.Substring(1)}.hkxbhd");
                        }
                    }
                }
                // ConnectCollisions
                else if (p[i].Equals("connect"))
                {
                    i++;
                    var hittype = p[i];
                    i++;

                    if (project.ProjectType is ProjectType.DS1 or ProjectType.DES)
                    {
                        relPath = Path.Combine("map", mapid, p[i]);
                    }
                    else if (project.ProjectType == ProjectType.DS1R)
                    {
                        if (CFG.Current.PTDE_Collision_Root != "")
                        {
                            if (Directory.Exists(CFG.Current.PTDE_Collision_Root))
                            {
                                relPath = Path.Join(
                                    CFG.Current.PTDE_Collision_Root, "map", mapid, p[i]);
                            }
                        }
                    }
                    else if (project.ProjectType is ProjectType.DS2S or ProjectType.DS2)
                    {
                        relPath = Path.Combine("model", "map", $"l{mapid.Substring(1)}.hkxbhd");
                    }
                    else if (project.ProjectType is ProjectType.DS3 or ProjectType.BB or ProjectType.SDT)
                    {
                        relPath = Path.Combine("map", mapid, $"l{mapid.Substring(1)}.hkxbhd");
                    }
                    else if (project.ProjectType is ProjectType.ER or ProjectType.NR or ProjectType.AC6)
                    {
                        relPath = Path.Combine("map", mapid.Substring(0, 3), mapid, $"l{mapid.Substring(1)}.hkxbhd");
                    }
                }
                // Navmesh
                else if (p[i].Equals("nav"))
                {
                    i++;

                    if (project.ProjectType is ProjectType.DS1 or ProjectType.DES)
                    {
                        relPath = Path.Combine("map", mapid, $"{mapid}.nvmbnd");
                    }
                    else if (project.ProjectType is ProjectType.DS1R)
                    {
                        relPath = Path.Combine("map", mapid, $"{mapid}.nvmbnd.dcx");
                    }
                    else if (project.ProjectType is ProjectType.DS3 or ProjectType.BB or ProjectType.SDT or ProjectType.ER)
                    {
                        relPath = Path.Combine("map", mapid, $"{mapid}.nvmhktbnd.dcx");

                        if (project.ProjectType is ProjectType.ER or ProjectType.NR)
                        {
                            var id = mapid.Substring(0, 3);
                            relPath = Path.Combine("map", id, mapid, $"{mapid}.nvmhktbnd.dcx");
                        }
                    }
                }
            }
        }
        // --- Characters ---
        else if (p[i].Equals("chr"))
        {
            i++;
            var chrid = p[i];
            i++;

            // Models
            if (p[i].Equals("model"))
            {
                if (project.ProjectType is ProjectType.DES)
                {
                    relPath = Path.Combine("chr", chrid, $"{chrid}.chrbnd");
                }
                else if (project.ProjectType is ProjectType.DS1)
                {
                    relPath = Path.Combine("chr", $"{chrid}.chrbnd");
                }
                else if (project.ProjectType is ProjectType.DS2S or ProjectType.DS2)
                {
                    relPath = Path.Combine("model", "chr", $"{chrid}.bnd");
                }
                else if (project.ProjectType is ProjectType.DES)
                {
                    relPath = Path.Combine("chr", chrid, $"{chrid}.chrbnd.dcx");
                }
                else
                {
                    relPath = Path.Combine("chr", $"{chrid}.chrbnd.dcx");
                }
            }
            // Textures
            else if (p[i].Equals("tex"))
            {
                var isLowDetail = false;

                // This is so the ER chr textures _l will display in the Texture Viewer
                if (p.Length == 4)
                {
                    i++;
                    if (p[i].Equals("low"))
                    {
                        isLowDetail = true;
                    }
                }

                if (project.ProjectType is ProjectType.DES)
                {
                    relPath = Path.Combine("chr", chrid, $"{chrid}.tpf");
                }
                else if (project.ProjectType is ProjectType.DS1)
                {
                    relPath = Path.Combine("chr", chrid, $"{chrid}.tpf");

                    if (!project.FS.FileExists(SanitiseRelativePath(relPath)))
                    {
                        relPath = Path.Combine("chr", $"{chrid}.chrbnd");
                    }
                }
                else if (project.ProjectType is ProjectType.DS2S or ProjectType.DS2)
                {
                    relPath = Path.Combine("model", "chr", $"{chrid}.texbnd");
                }
                else if (project.ProjectType is ProjectType.DS1R)
                {
                    relPath = Path.Combine("chr", $"{chrid}.chrbnd.dcx");
                }
                else if (project.ProjectType is ProjectType.BB)
                {
                    relPath = Path.Combine("chr", $"{chrid}.chrbnd.dcx");
                }
                else if (project.ProjectType is ProjectType.DS3 or ProjectType.SDT)
                {
                    relPath = Path.Combine("chr", $"{chrid}.texbnd.dcx");
                }
                else if (project.ProjectType is ProjectType.ER or ProjectType.NR)
                {
                    relPath = Path.Combine("chr", $"{chrid}_h.texbnd.dcx");

                    if (isLowDetail)
                    {
                        relPath = Path.Combine("chr", $"{chrid}_l.texbnd.dcx");
                    }
                }
                else if (project.ProjectType is ProjectType.AC6)
                {
                    relPath = Path.Combine("chr", $"{chrid}.texbnd.dcx");

                    if (isLowDetail)
                    {
                        relPath = Path.Combine("chr", $"{chrid}_l.texbnd.dcx");
                    }
                }
            }
        }
        // --- Objects ---
        else if (p[i].Equals("obj"))
        {
            i++;
            var objid = p[i];
            i++;

            // Models / Textures
            if (p[i].Equals("model") || p[i].Equals("tex"))
            {
                if (project.ProjectType is ProjectType.DS1)
                {
                    relPath = Path.Combine("obj", $"{objid}.objbnd");
                }
                else if (project.ProjectType is ProjectType.DS2S or ProjectType.DS2)
                {
                    relPath = Path.Combine("model", "obj", $"{objid}.bnd");
                }
                else if (project.ProjectType is ProjectType.ER or ProjectType.NR)
                {
                    // Derive subfolder path from model name (all vanilla AEG are within subfolders)
                    if (objid.Length >= 6)
                    {
                        relPath = Path.Combine("asset", "aeg", objid.Substring(0, 6), $"{objid}.geombnd.dcx");
                    }
                }
                else if (project.ProjectType is ProjectType.AC6)
                {
                    if (objid.Length >= 6)
                    {
                        relPath = Path.Combine("asset", "environment", "geometry", $"{objid}.geombnd.dcx");
                    }
                }
                else
                {
                    relPath = Path.Combine("obj", $"{objid}.objbnd.dcx");
                }
            }
            // Collisions
            else if (p[i].Equals("collision"))
            {
                i++;
                var colName = Path.GetFileNameWithoutExtension(p[i]);
                i++;

                if (project.ProjectType is ProjectType.ER or ProjectType.NR)
                {
                    // Derive subfolder path from model name (all vanilla AEG are within subfolders)
                    if (objid.Length >= 6)
                    {
                        relPath = Path.Combine("asset", "aeg", objid.Substring(0, 6), $"{colName}.geomhkxbnd.dcx");
                    }
                }
                else if (project.ProjectType is ProjectType.AC6)
                {
                    if (objid.Length >= 6)
                    {
                        relPath = Path.Combine("asset", "environment", "geometry", $"{colName}.geomhkxbnd.dcx");
                    }
                }
                else
                {
                    relPath = Path.Combine("obj", $"{objid}.objbnd.dcx");
                }
            }
        }
        // --- Parts ---
        else if (p[i].Equals("parts"))
        {
            i++;
            var partsId = p[i];
            i++;

            // Models / Textures
            if (p[i].Equals("model") || p[i].Equals("tex"))
            {
                if (project.ProjectType is ProjectType.DS1)
                {
                    relPath = Path.Combine("parts", $"{partsId}.partsbnd");
                }
                else if (project.ProjectType is ProjectType.DS2S or ProjectType.DS2)
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

                    relPath = Path.Combine("model", "parts", partType, $"{partsId}.bnd");
                }
                else if (project.ProjectType is ProjectType.ER or ProjectType.NR)
                {
                    if (p.Length == 4)
                    {
                        i++;

                        if (p[i].Equals("low"))
                        {
                            relPath = Path.Combine("parts", $"{partsId}_l.partsbnd.dcx");
                        }
                        else
                        {
                            relPath = Path.Combine("parts", $"{partsId}.partsbnd.dcx");
                        }
                    }
                    else
                    {
                        relPath = Path.Combine("parts", $"{partsId}.partsbnd.dcx");
                    }

                    if (partsId == "common_body")
                    {
                        relPath = Path.Combine("parts", $"{partsId}.tpf.dcx");
                    }
                }
                else if (project.ProjectType is ProjectType.AC6 && p[i].Equals("tex"))
                {
                    if (p.Length == 4)
                    {
                        i++;
                        if (p[i].Equals("low"))
                        {
                            relPath = Path.Combine("parts", $"{partsId}_l.partsbnd.dcx");
                        }
                        else if (p[i].Equals("tpf"))
                        {
                            if (partsId.Substring(0, 2) == "wp")
                            {
                                string id;
                                if (partsId.EndsWith("_l"))
                                {
                                    id = partsId[..^2].Split("_").Last();
                                    relPath = Path.Combine("parts", $"wp_{id}_l.tpf.dcx");
                                }
                                else
                                {
                                    id = partsId.Split("_").Last();
                                    relPath = Path.Combine("parts", $"wp_{id}.tpf.dcx");
                                }
                            }
                            else
                            {
                                relPath = Path.Combine("parts", $"{partsId}_u.tpf.dcx");
                            }
                        }
                        else
                        {
                            relPath = Path.Combine("parts", $"{partsId}.partsbnd.dcx");
                        }
                    }
                }
                else
                {
                    relPath = Path.Combine("parts", $"{partsId}.partsbnd.dcx");
                }
            }
        }
        // --- Menu ---
        else if (p[i].Equals("menu"))
        {
            i++;

            var containerName = p[i];
            i++;

            // Textures
            if (p[i].Equals("tex"))
            {
                if (project.ProjectType is ProjectType.DS2S or ProjectType.DS2)
                {
                    relPath = Path.Combine("menu", "tex", "icon", $"{containerName}.tpf");

                    if (containerName.Contains("ic_area_"))
                    {
                        relPath = Path.Combine("menu", "tex", "icon", "bonfire_area", $"{containerName}.tpf");
                    }
                    else if (containerName.Contains("ic_list_"))
                    {
                        relPath = Path.Combine("menu", "tex", "icon", "bonfire_list", $"{containerName}.tpf");
                    }
                    else if (containerName.Contains("ic_cm_"))
                    {
                        relPath = Path.Combine("menu", "tex", "icon", "charamaking", $"{containerName}.tpf");
                    }
                    else if (containerName.Contains("ei_"))
                    {
                        relPath = Path.Combine("menu", "tex", "icon", "effect", $"{containerName}.tpf");
                    }
                    else if (containerName.Contains("ic_ca"))
                    {
                        relPath = Path.Combine("menu", "tex", "icon", "item_category", $"{containerName}.tpf");
                    }
                    else if (containerName.Contains("map_name_"))
                    {
                        // TODO: support all the languages
                        relPath = Path.Combine("menu", "tex", "icon", "mapname", "english", $"{containerName}.tpf");
                    }
                    else if (containerName.Contains("vi_"))
                    {
                        relPath = Path.Combine("menu", "tex", "icon", "vow", $"{containerName}.tpf");
                    }
                    else
                    {
                        relPath = containerName;
                    }
                }
                else
                {
                    relPath = containerName;
                }
            }
        }
        // --- AET ---
        else if (p[i].Equals("aet"))
        {
            i++;

            var containerName = p[i];
            i++;

            if (p[i].Equals("tex"))
            {
                if (project.ProjectType is ProjectType.AC6)
                {
                    relPath = Path.Combine("asset", "environment", "texture", $"{containerName}.tpf.dcx");
                }

                if (project.ProjectType is ProjectType.ER or ProjectType.NR)
                {
                    if (containerName.Length > 5)
                    {
                        relPath = Path.Combine("asset", "aet", containerName.Substring(0, 6), $"{containerName}.tpf.dcx");
                    }
                }
            }
        }
        // --- AAT ---
        else if (p[i].Equals("aat"))
        {
            i++;

            var containerName = p[i];
            i++;

            if (p[i].Equals("tex"))
            {
                relPath = Path.Combine("parts", $"{containerName}.tpf.dcx");
            }
        }
        // --- SYSTEX ---
        else if (p[i].Equals("systex"))
        {
            i++;

            var containerName = p[i];
            i++;

            if (p[i].Equals("tex"))
            {
                relPath = Path.Combine("other", $"{containerName}.tpf.dcx");
            }
        }
        // --- SFX ---
        else if (p[i].Equals("sfx"))
        {
            i++;

            var containerName = p[i];
            i++;

            if (p[i].Equals("tex"))
            {
                var fileExt = @".ffxbnd.dcx";

                if (project.ProjectType is ProjectType.DS2S or ProjectType.DS2)
                {
                    fileExt = @".ffxbnd";
                }

                relPath = Path.Combine("sfx", $"{containerName}{fileExt}");
            }
        }
        // --- OTHER ---
        else if (p[i].Equals("other"))
        {
            i++;

            var containerName = p[i];
            i++;

            if (p[i].Equals("tex"))
            {
                relPath = Path.Combine("other", $"{containerName}.tpf.dcx");
            }
        }

        relPath = SanitiseRelativePath(relPath);

#if DEBUG
        if (relPath != "" && !project.FS.FileExists(relPath))
        {
            TaskLogs.AddLog($"[Smithbox:DEBUG] Failed to find file in VFS: {relPath}", Microsoft.Extensions.Logging.LogLevel.Error);
        }
#endif

        return relPath;
    }

    /// <summary>
    /// Used to the external assets Smithbox loads in (i.e. the world map TPFs)
    /// </summary>
    /// <param name="project"></param>
    /// <param name="virtPath"></param>
    /// <returns></returns>
    public static string GetAbsolutePath(ProjectEntry project, string virtPath)
    {
        var absPath = "";

        var p = virtPath.Split('/');
        var i = 0;

        // --- SMITHBOX ---
        // Used for designed TPF elements that are tend rendered as images within Smithbox
        // i.e. the world maps
        if (p[i].Equals("smithbox"))
        {
            i++;

            var containerName = p[i];

            absPath = Path.Combine(
                AppContext.BaseDirectory, "Assets", "MSB", ProjectUtils.GetGameDirectory(project.ProjectType),
                "Maps", $"{containerName}.tpf.dcx");
        }

        return absPath;
    }

    public static string SanitiseRelativePath(string relativePath)
    {
        return '/' + relativePath.Replace('\\', '/').ToLower();
    }

    /// <summary>
    /// Gets the adjusted map ID that contains all the map assets
    /// </summary>
    /// <param name="mapid">The msb map ID to adjust</param>
    /// <returns>The map ID for the purpose of asset storage</returns>
    public static string GetAssetMapID(ProjectEntry project, string mapid)
    {
        if (project.ProjectType is ProjectType.DES or ProjectType.ER or ProjectType.NR or ProjectType.AC6)
        {
            return mapid;
        }
        else if (project.ProjectType is ProjectType.DS1R)
        {
            if (mapid.StartsWith("m99"))
            {
                // DSR m99 maps contain their own assets
                return mapid;
            }
        }
        else if (project.ProjectType is ProjectType.BB)
        {
            if (mapid.StartsWith("m29"))
            {
                // Special case for chalice dungeon assets
                return "m29_00_00_00";
            }
        }
        else if (project.ProjectType is ProjectType.ACFA)
        {
            return mapid[..4];
        }
        else if (project.ProjectType is ProjectType.ACV)
        {
            return mapid[..5];
        }
        else if (project.ProjectType is ProjectType.ACVD)
        {
            if (mapid.Length == 12 && mapid.StartsWith("ch"))
            {
                return string.Concat(mapid.AsSpan(7, 4), "0");
            }

            return mapid[..4] + '0';
        }

        // Default
        return mapid.Substring(0, 6) + "_00_00";
    }
}