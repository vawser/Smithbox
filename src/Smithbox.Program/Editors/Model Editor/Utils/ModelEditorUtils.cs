using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.MapEditor;
using StudioCore.Utilities;
using System.IO;

namespace StudioCore.Editors.ModelEditor;

public static class ModelEditorUtils
{
    public static string GetAliasForSourceListEntry(ProjectEntry project, string filename, ModelListType type)
    {
        var alias = "";

        if (type is ModelListType.MapPiece)
        {
            alias = AliasHelper.GetMapNameAlias(project, filename);
        }

        if (type is ModelListType.Asset)
        {
            alias = AliasHelper.GetAssetAlias(project, filename);
        }

        if (type is ModelListType.Character)
        {
            alias = AliasHelper.GetCharacterAlias(project, filename);
        }

        if (type is ModelListType.Part)
        {
            alias = AliasHelper.GetPartAlias(project, filename);
        }

        if (type is ModelListType.MapPiece)
        {
            alias = AliasHelper.GetMapPieceAlias(project, filename);
        }

        return alias;
    }

    public static ResourceContainerType GetContainerTypeFromRelativePath(ProjectEntry project, string relativePath)
    {
        var containerType = ResourceContainerType.None;

        var relPath = relativePath.Replace(".dcx", "");

        if (relPath.EndsWith("flver"))
        {
            containerType = ResourceContainerType.None;
        }
        else if (relPath.EndsWith("mapbhd"))
        {
            containerType = ResourceContainerType.BXF;
        }
        else if (relPath.EndsWith("mapbnd"))
        {
            containerType = ResourceContainerType.BND;
        }
        else if (relPath.EndsWith("bnd"))
        {
            containerType = ResourceContainerType.BND;
        }
        else if (relPath.EndsWith("flv"))
        {
            containerType = ResourceContainerType.None;
        }
        else if (relPath.EndsWith("hkx"))
        {
            containerType = ResourceContainerType.None;
        }
        else if (relPath.EndsWith("hkxbhd"))
        {
            containerType = ResourceContainerType.BXF;
        }
        else if (relPath.EndsWith("chrbnd"))
        {
            containerType = ResourceContainerType.BND;
        }
        else if (relPath.EndsWith("objbnd"))
        {
            containerType = ResourceContainerType.BND;
        }
        else if (relPath.EndsWith("geombnd"))
        {
            containerType = ResourceContainerType.BND;
        }
        else if (relPath.EndsWith("partsbnd"))
        {
            containerType = ResourceContainerType.BND;
        }
        else if (relPath.EndsWith("tpfbhd"))
        {
            containerType = ResourceContainerType.BXF;
        }

        return containerType;
    }

    public static ResourceContainerType GetContainerTypeFromVirtualPath(ProjectEntry project, string virtPath)
    {
        var containerType = ResourceContainerType.None;

        var p = virtPath.Split('/');
        var i = 0;

        // --- Map ---
        if (p[i].Equals("map"))
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
                        containerType = ResourceContainerType.BXF;
                    }
                }
                else if (project.ProjectType is ProjectType.DES)
                {
                    var mid = p[i];
                    i++;

                    containerType = ResourceContainerType.None;
                }
                else
                {
                    var mid = p[i];
                    i++;
                    var id = p[i];

                    containerType = ResourceContainerType.BXF;
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
                        containerType = ResourceContainerType.None;
                    }
                    else if (project.ProjectType is ProjectType.DS1R)
                    {
                        containerType = ResourceContainerType.None;
                    }
                    else if (project.ProjectType is ProjectType.DS2S or ProjectType.DS2)
                    {
                        containerType = ResourceContainerType.BXF;
                    }
                    else if (project.ProjectType is ProjectType.BB or ProjectType.DES)
                    {
                        containerType = ResourceContainerType.None;
                    }
                    else if (project.ProjectType is ProjectType.ER or ProjectType.AC6 or ProjectType.NR)
                    {
                        containerType = ResourceContainerType.BND;
                    }
                    else
                    {
                        containerType = ResourceContainerType.BND;
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
                    }
                    else if (project.ProjectType == ProjectType.DS1R)
                    {
                    }
                    else if (project.ProjectType is ProjectType.DS2S or ProjectType.DS2)
                    {
                        containerType = ResourceContainerType.BXF;
                    }
                    else if (project.ProjectType is ProjectType.DS3 or ProjectType.BB or ProjectType.SDT)
                    {
                        containerType = ResourceContainerType.BXF;
                    }
                    else if (project.ProjectType is ProjectType.ER or ProjectType.NR or ProjectType.AC6)
                    {
                        containerType = ResourceContainerType.BXF;
                    }
                }
                // Navmesh
                else if (p[i].Equals("nav"))
                {
                    i++;

                    if (project.ProjectType is ProjectType.DS1 or ProjectType.DES)
                    {
                        containerType = ResourceContainerType.BND;
                    }
                    else if (project.ProjectType is ProjectType.DS1R)
                    {
                        containerType = ResourceContainerType.BND;
                    }
                    else if (project.MapEditor != null && project.MapEditor.HavokNavmeshBank.CanUse())
                    {
                        containerType = ResourceContainerType.BND;
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
                    containerType = ResourceContainerType.BND;
                }
                else if (project.ProjectType is ProjectType.DS1)
                {
                    containerType = ResourceContainerType.BND;
                }
                else if (project.ProjectType is ProjectType.DS2S or ProjectType.DS2)
                {
                    containerType = ResourceContainerType.BND;
                }
                else if (project.ProjectType is ProjectType.DES)
                {
                    containerType = ResourceContainerType.BND;
                }
                else
                {
                    containerType = ResourceContainerType.BND;
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
                    containerType = ResourceContainerType.None;
                }
                else if (project.ProjectType is ProjectType.DS1)
                {
                    containerType = ResourceContainerType.None;
                }
                else if (project.ProjectType is ProjectType.DS2S or ProjectType.DS2)
                {
                    containerType = ResourceContainerType.BND;
                }
                else if (project.ProjectType is ProjectType.DS1R)
                {
                    containerType = ResourceContainerType.BND;
                }
                else if (project.ProjectType is ProjectType.BB)
                {
                    containerType = ResourceContainerType.BND;
                }
                else if (project.ProjectType is ProjectType.DS3 or ProjectType.SDT)
                {
                    containerType = ResourceContainerType.BND;
                }
                else if (project.ProjectType is ProjectType.ER or ProjectType.NR)
                {
                    containerType = ResourceContainerType.BND;

                    if (isLowDetail)
                    {
                        containerType = ResourceContainerType.BND;
                    }
                }
                else if (project.ProjectType is ProjectType.AC6)
                {
                    containerType = ResourceContainerType.BND;

                    if (isLowDetail)
                    {
                        containerType = ResourceContainerType.BND;
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
                containerType = ResourceContainerType.BND;
            }
            // Collisions
            else if (p[i].Equals("collision"))
            {
                i++;
                var colName = Path.GetFileNameWithoutExtension(p[i]);
                i++;

                containerType = ResourceContainerType.BND;
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
                if (project.ProjectType is ProjectType.DS1 or ProjectType.DS2S or ProjectType.DS2)
                {
                    containerType = ResourceContainerType.BND;
                }
                else if (project.ProjectType is ProjectType.ER or ProjectType.NR)
                {
                    if (p.Length == 4)
                    {
                        i++;

                        if (p[i].Equals("low"))
                        {
                            containerType = ResourceContainerType.BND;
                        }
                        else
                        {
                            containerType = ResourceContainerType.BND;
                        }
                    }

                    if (partsId == "common_body")
                    {
                        containerType = ResourceContainerType.None;
                    }
                }
                else if (project.ProjectType is ProjectType.AC6 && p[i].Equals("tex"))
                {
                    if (p.Length == 4)
                    {
                        i++;
                        if (p[i].Equals("low"))
                        {
                            containerType = ResourceContainerType.BND;
                        }
                        else if (p[i].Equals("tpf"))
                        {
                            containerType = ResourceContainerType.None;
                        }
                        else
                        {
                            containerType = ResourceContainerType.BND;
                        }
                    }
                }
                else
                {
                    containerType = ResourceContainerType.BND;
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
                containerType = ResourceContainerType.None;
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
                containerType = ResourceContainerType.None;
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
                containerType = ResourceContainerType.None;
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
                containerType = ResourceContainerType.None;
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
                containerType = ResourceContainerType.BND;
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
                containerType = ResourceContainerType.None;
            }
        }

        return containerType;
    }
}
