using HKLib.hk2018;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Editors.ModelEditor.Enums;
using StudioCore.Resource.Locators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor;

/// <summary>
/// This represents the loaded container, which may contain multiple FLVEr files.
/// </summary>
public class FlverContainer
{
    /// <summary>
    /// This is the current internal FLVER being viewed in the Model Editor
    /// </summary>
    public InternalFlver CurrentInternalFlver { get; set; }

    /// <summary>
    /// This is a list of all the internal FLVERs for the current loaded model container
    /// </summary>
    public List<InternalFlver> InternalFlvers { get; set; }

    public string ContainerName { get; set; }
    public string MapID { get; set; }
    public FlverContainerType Type { get; set; }
    public FlverBinderType BinderType { get; set; }

    public DCX.Type CompressionType { get; set; }

    public string RootBinderPath { get; set; }
    public string ModBinderPath { get; set; }
    public string ModBinderDirectory { get; set; }

    public string FlverFileName { get; set; }
    public string FlverFileExtension { get; set; }

    public string BinderDirectory { get; set; }
    public string BinderPath { get; set; }
    public string BinderExtension { get; set; }

    public string LoosePath { get; set; }

    public ResourceDescriptor ModelAssetDescriptor { get; set; }

    public hkRootLevelContainer ER_LowCollision { get; set; }
    public hkRootLevelContainer ER_HighCollision { get; set; }

    private ModelEditorScreen Editor;

    public FlverContainer(ModelEditorScreen editor, string name, string loosePath)
    {
        Editor = editor;

        InternalFlvers = new List<InternalFlver>();

        ContainerName = name;
        MapID = "";
        Type = FlverContainerType.Loose;
        BinderType = FlverBinderType.None;
        CompressionType = DCX.Type.None;

        BinderDirectory = GetBinderDirectory();
        BinderExtension = GetBinderExtension();
        BinderPath = $"{BinderDirectory}{ContainerName}{BinderExtension}";
        RootBinderPath = $"{Editor.Project.DataPath}{BinderPath}";
        ModBinderPath = $"{Editor.Project.ProjectPath}{BinderPath}";
        ModBinderDirectory = $"{Editor.Project.ProjectPath}{BinderDirectory}";

        FlverFileExtension = GetFlverExtension();
        FlverFileName = $"{ContainerName}{FlverFileExtension}";

        LoosePath = loosePath;
    }

    public FlverContainer(ModelEditorScreen editor, string modelName, FlverContainerType modelType, string mapId)
    {
        Editor = editor;

        InternalFlvers = new List<InternalFlver>();

        ContainerName = modelName;
        MapID = mapId;
        Type = modelType;
        BinderType = FlverBinderType.None;
        CompressionType = DCX.Type.None;

        BinderDirectory = GetBinderDirectory();
        BinderExtension = GetBinderExtension();
        BinderPath = $"{BinderDirectory}{ContainerName}{BinderExtension}";
        RootBinderPath = $"{Editor.Project.DataPath}{BinderPath}";
        ModBinderPath = $"{Editor.Project.ProjectPath}{BinderPath}";
        ModBinderDirectory = $"{Editor.Project.ProjectPath}{BinderDirectory}";

        FlverFileExtension = GetFlverExtension();
        FlverFileName = $"{ContainerName}{FlverFileExtension}";
    }

    public bool CopyBinderToMod()
    {
        if (!Directory.Exists(ModBinderDirectory))
        {
            Directory.CreateDirectory(ModBinderDirectory);
        }

        var rootPath = RootBinderPath;
        var modPath = ModBinderPath;

        if (File.Exists(rootPath))
        {
            if (!File.Exists(modPath))
            {
                File.Copy(rootPath, modPath);
            }
        }
        // Mod-only model, no need to copy to mod
        else if (File.Exists(modPath))
        {
            return true;
        }
        else
        {
            TaskLogs.AddLog($"FLVER container file path does not exist:\nRoot: {rootPath}\nProject: {modPath}", LogLevel.Warning);
            return false;
        }

        return true;
    }

    public bool CopyBXFtoMod(string name)
    {
        if (!Directory.Exists(ModBinderDirectory))
        {
            Directory.CreateDirectory(ModBinderDirectory);
        }

        var rootPath = $"{Editor.Project.DataPath}\\{BinderDirectory}\\{name}";
        var modPath = $"{Editor.Project.ProjectPath}\\{BinderDirectory}\\{name}";

        if (File.Exists(rootPath))
        {
            if (!File.Exists(modPath))
            {
                File.Copy(rootPath, modPath);
            }
        }
        // Mod-only model, no need to copy to mod
        else if (File.Exists(modPath))
        {
            return true;
        }
        else
        {
            TaskLogs.AddLog($"FLVER container file path does not exist:\nRoot: {rootPath}\nProject: {modPath}", LogLevel.Warning);
            return false;
        }

        return true;
    }

    public string GetFlverExtension()
    {
        string ext = ".flver";

        if (Editor.Project.ProjectType is ProjectType.DS2S or ProjectType.DS2 or ProjectType.ACFA or ProjectType.ACV or ProjectType.ACVD)
        {
            ext = ".flv";
        }

        return ext;
    }

    public string GetBinderDirectory()
    {
        switch (Type)
        {
            case FlverContainerType.Character:
                string chrDir = @"\chr\";

                if (Editor.Project.ProjectType is ProjectType.DS2S or ProjectType.DS2)
                {
                    chrDir = @"\model\chr\";
                }

                return chrDir;
            case FlverContainerType.Enemy:
                string eneDir = @"\model\ene\";
                return eneDir;
            case FlverContainerType.Object:
                string objDir = @"\obj\";

                if (Editor.Project.ProjectType is ProjectType.DS2S or ProjectType.DS2 or ProjectType.ACFA or ProjectType.ACV or ProjectType.ACVD)
                {
                    objDir = @"\model\obj\";
                }
                else if (Editor.Project.ProjectType is ProjectType.ER)
                {
                    var category = ContainerName.Split("_")[0];
                    objDir = $@"\asset\aeg\{category}\";
                }
                else if (Editor.Project.ProjectType is ProjectType.AC6)
                {
                    objDir = @"\asset\environment\geometry\";
                }

                return objDir;
            case FlverContainerType.Parts:
                string partDir = @"\parts\";

                if (Editor.Project.ProjectType is ProjectType.DS2S or ProjectType.DS2)
                {
                    partDir = @"\model\parts\";

                    var partType = "";
                    switch (ContainerName[..2])
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

                    partDir = $"{partDir}\\{partType}\\";
                }
                else if (Editor.Project.ProjectType is ProjectType.AC4 or ProjectType.ACFA or ProjectType.ACV or ProjectType.ACVD)
                {
                    partDir = @"\model\ac";

                    string partCat = @"parts";
                    string partType = string.Empty;
                    switch (ContainerName[..2])
                    {
                        case "hd":
                            partType = "head";
                            break;
                        case "cr":
                            partType = "core";
                            break;
                        case "am":
                            partType = "arm";
                            break;
                        case "lg":
                            partType = "leg";
                            break;
                        case "fs":
                            partType = "fcs";
                            break;
                        case "gn":
                            partType = "gene";
                            break;
                        case "bs":
                            partType = "boost";
                            break;
                        case "gb": // Actually gbs but we only read two chars here
                            partType = "g_boost";
                            break;
                        case "hr":
                        case "hl":
                            partType = "hand";
                            break;
                        case "br":
                        case "bl":
                            partType = "back";
                            break;
                        case "hg": // Actually hgr and hgl but we only read two chars here
                            partType = "hanger";
                            break;
                        case "sh":
                            partType = "shoul";
                            break;
                        case "ow":
                            partType = "ow";
                            break;
                        case "rc":
                            partType = "recon";
                            break;
                        case "ir":
                        case "il":
                            partCat = "sub";
                            partType = "arm";
                            break;
                        case "bb":
                            partCat = "sub";
                            partType = "bb";
                            break;
                        case "cl":
                            // What to do about cr here...
                            partCat = "sub";
                            partType = "core";
                            break;
                        case "dr":
                        case "dl":
                        case "dx":
                        case "dy":
                        case "er":
                        case "el":
                        case "ex":
                        case "ey":
                        case "fr":
                        case "fl":
                        case "fx":
                        case "fy":
                        case "gg":
                            partCat = "sub";
                            partType = "leg";
                            break;
                        case "ob":
                            partCat = "sub";
                            partType = "ob";
                            break;
                        case "sb":
                            partCat = "sub";
                            partType = "sb";
                            break;
                        case "aa":
                            // What to do about br here...
                            // What to do about bl here...
                            partCat = "sub";
                            partType = "head";
                            break;
                    }

                    partDir = $@"{partDir}\{partCat}\{partType}\";
                }

                return partDir;
            case FlverContainerType.MapPiece:
                string mapPieceDir = $@"\map\{MapID}\";

                if (Editor.Project.ProjectType is ProjectType.ER or ProjectType.AC6)
                {
                    string shortMapId = MapID.Split("_")[0];
                    mapPieceDir = $@"\map\{shortMapId}\{MapID}\";
                }

                if (Editor.Project.ProjectType is ProjectType.DS2S or ProjectType.DS2 or ProjectType.ACFA or ProjectType.ACV or ProjectType.ACVD)
                {
                    mapPieceDir = $@"\model\map\";
                }

                return mapPieceDir;
            default: break;
        }

        return "";
    }

    public string GetBinderExtension()
    {
        switch (Type)
        {
            case FlverContainerType.Character:
                string chrExt = ".chrbnd.dcx";
                BinderType = FlverBinderType.BND;

                if (Editor.Project.ProjectType is ProjectType.DS1)
                {
                    chrExt = ".chrbnd";
                    BinderType = FlverBinderType.BND;
                }
                else if (Editor.Project.ProjectType is ProjectType.DS2S or ProjectType.DS2 or ProjectType.ACFA)
                {
                    chrExt = ".bnd";
                    BinderType = FlverBinderType.BND;
                }
                else if (Editor.Project.ProjectType is ProjectType.ACV or ProjectType.ACVD)
                {
                    chrExt = ".bnd.dcx";
                    BinderType = FlverBinderType.BND;
                }

                return chrExt;
            case FlverContainerType.Enemy:
                string eneExt = ".bnd";
                BinderType = FlverBinderType.BND;

                if (Editor.Project.ProjectType is ProjectType.ACV or ProjectType.ACVD)
                {
                    eneExt = ".bnd.dcx";
                    BinderType = FlverBinderType.BND;
                }

                return eneExt;
            case FlverContainerType.Object:
                string objExt = ".objbnd.dcx";
                BinderType = FlverBinderType.BND;

                if (Editor.Project.ProjectType is ProjectType.DS1)
                {
                    objExt = ".objbnd";
                    BinderType = FlverBinderType.BND;
                }
                else if (Editor.Project.ProjectType is ProjectType.DS2S or ProjectType.DS2 or ProjectType.ACFA or ProjectType.ACV or ProjectType.ACVD)
                {
                    objExt = ".bnd";
                    BinderType = FlverBinderType.BND;
                }
                else if (Editor.Project.ProjectType is ProjectType.ER)
                {
                    objExt = ".geombnd.dcx";
                    BinderType = FlverBinderType.BND;
                }
                else if (Editor.Project.ProjectType is ProjectType.AC6)
                {
                    objExt = ".geombnd.dcx";
                    BinderType = FlverBinderType.BND;
                }

                return objExt;
            case FlverContainerType.Parts:
                string partExt = ".partsbnd.dcx";
                BinderType = FlverBinderType.BND;

                if (Editor.Project.ProjectType is ProjectType.DS1)
                {
                    partExt = ".partsbnd";
                    BinderType = FlverBinderType.BND;
                }
                else if (Editor.Project.ProjectType is ProjectType.DS2S or ProjectType.DS2 or ProjectType.ACFA)
                {
                    partExt = ".bnd";
                    BinderType = FlverBinderType.BND;
                }
                else if (Editor.Project.ProjectType is ProjectType.ACV or ProjectType.ACVD)
                {
                    partExt = ".bnd.dcx";
                    BinderType = FlverBinderType.BND;
                }

                return partExt;
            case FlverContainerType.MapPiece:
                string mapPieceExt = ".mapbnd.dcx";
                BinderType = FlverBinderType.BND;

                if (Editor.Project.ProjectType is ProjectType.DS2S or ProjectType.DS2)
                {
                    mapPieceExt = ".mapbhd";
                    BinderType = FlverBinderType.BXF;
                }
                else if (Editor.Project.ProjectType is ProjectType.ACFA)
                {
                    mapPieceExt = ".bnd";
                    BinderType = FlverBinderType.BND;
                }
                else if (Editor.Project.ProjectType is ProjectType.ACV or ProjectType.ACVD)
                {
                    mapPieceExt = ".dcx.bnd";
                    BinderType = FlverBinderType.BND;
                }
                else if (Editor.Project.ProjectType is ProjectType.DS1R or ProjectType.BB)
                {
                    mapPieceExt = ".flver.dcx";
                    BinderType = FlverBinderType.None;
                }
                else if (Editor.Project.ProjectType is ProjectType.DS1)
                {
                    mapPieceExt = ".flver";
                    BinderType = FlverBinderType.None;
                }

                return mapPieceExt;
            default: break;
        }

        return "";
    }
}
