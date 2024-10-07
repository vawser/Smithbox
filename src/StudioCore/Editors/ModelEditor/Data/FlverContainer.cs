using HKLib.hk2018;
using SoulsFormats;
using StudioCore.Core.Project;
using StudioCore.Editors.ModelEditor.Enums;
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

    public hkRootLevelContainer ER_LowCollision { get; set; }
    public hkRootLevelContainer ER_HighCollision { get; set; }


    public FlverContainer(string name, string loosePath)
    {
        InternalFlvers = new List<InternalFlver>();

        ContainerName = name;
        MapID = "";
        Type = FlverContainerType.Loose;
        BinderType = FlverBinderType.None;
        CompressionType = DCX.Type.None;

        BinderDirectory = GetBinderDirectory();
        BinderExtension = GetBinderExtension();
        BinderPath = $"{BinderDirectory}{ContainerName}{BinderExtension}";
        RootBinderPath = $"{Smithbox.GameRoot}{BinderPath}";
        ModBinderPath = $"{Smithbox.ProjectRoot}{BinderPath}";
        ModBinderDirectory = $"{Smithbox.ProjectRoot}{BinderDirectory}";

        FlverFileExtension = GetFlverExtension();
        FlverFileName = $"{ContainerName}{FlverFileExtension}";

        LoosePath = loosePath;
    }

    public FlverContainer(string modelName, FlverContainerType modelType, string mapId)
    {
        InternalFlvers = new List<InternalFlver>();

        ContainerName = modelName;
        MapID = mapId;
        Type = modelType;
        BinderType = FlverBinderType.None;
        CompressionType = DCX.Type.None;

        BinderDirectory = GetBinderDirectory();
        BinderExtension = GetBinderExtension();
        BinderPath = $"{BinderDirectory}{ContainerName}{BinderExtension}";
        RootBinderPath = $"{Smithbox.GameRoot}{BinderPath}";
        ModBinderPath = $"{Smithbox.ProjectRoot}{BinderPath}";
        ModBinderDirectory = $"{Smithbox.ProjectRoot}{BinderDirectory}";

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
            TaskLogs.AddLog($"Container path does not exist:\nRoot: {rootPath}\nProject: {modPath}");
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

        var rootPath = $"{Smithbox.GameRoot}\\{BinderDirectory}\\{name}";
        var modPath = $"{Smithbox.ProjectRoot}\\{BinderDirectory}\\{name}";

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
            TaskLogs.AddLog($"Container path does not exist:\nRoot: {rootPath}\nProject: {modPath}");
            return false;
        }

        return true;
    }

    public string GetFlverExtension()
    {
        string ext = ".flver";

        if (Smithbox.ProjectType is ProjectType.DS2S or ProjectType.DS2)
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

                if (Smithbox.ProjectType is ProjectType.DS2S or ProjectType.DS2)
                {
                    chrDir = @"\model\chr\";
                }

                return chrDir;
            case FlverContainerType.Object:
                string objDir = @"\obj\";

                if (Smithbox.ProjectType is ProjectType.DS2S or ProjectType.DS2)
                {
                    objDir = @"\model\obj\";
                }
                else if (Smithbox.ProjectType is ProjectType.ER)
                {
                    var category = ContainerName.Split("_")[0];
                    objDir = $@"\asset\aeg\{category}\";
                }
                else if (Smithbox.ProjectType is ProjectType.AC6)
                {
                    objDir = @"\asset\environment\geometry\";
                }

                return objDir;
            case FlverContainerType.Parts:
                string partDir = @"\parts\";

                if (Smithbox.ProjectType is ProjectType.DS2S or ProjectType.DS2)
                {
                    partDir = @"\model\parts\";

                    var partType = "";
                    switch (ContainerName.Substring(0, 2))
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

                return partDir;
            case FlverContainerType.MapPiece:
                string mapPieceDir = $@"\map\{MapID}\";

                if (Smithbox.ProjectType is ProjectType.ER or ProjectType.AC6)
                {
                    string shortMapId = MapID.Split("_")[0];
                    mapPieceDir = $@"\map\{shortMapId}\{MapID}\";
                }

                if (Smithbox.ProjectType is ProjectType.DS2S or ProjectType.DS2)
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
                string chrExt = @".chrbnd.dcx";
                BinderType = FlverBinderType.BND;

                if (Smithbox.ProjectType is ProjectType.DS1)
                {
                    chrExt = ".chrbnd";
                    BinderType = FlverBinderType.BND;
                }
                if (Smithbox.ProjectType is ProjectType.DS2S or ProjectType.DS2)
                {
                    chrExt = ".bnd";
                    BinderType = FlverBinderType.BND;
                }

                return chrExt;
            case FlverContainerType.Object:
                string objExt = @".objbnd.dcx";
                BinderType = FlverBinderType.BND;

                if (Smithbox.ProjectType is ProjectType.DS1)
                {
                    objExt = ".objbnd";
                    BinderType = FlverBinderType.BND;
                }
                else if (Smithbox.ProjectType is ProjectType.DS2S or ProjectType.DS2)
                {
                    objExt = ".bnd";
                    BinderType = FlverBinderType.BND;
                }
                else if (Smithbox.ProjectType is ProjectType.ER)
                {
                    objExt = ".geombnd.dcx";
                    BinderType = FlverBinderType.BND;
                }
                else if (Smithbox.ProjectType is ProjectType.AC6)
                {
                    objExt = ".geombnd.dcx";
                    BinderType = FlverBinderType.BND;
                }

                return objExt;
            case FlverContainerType.Parts:
                string partExt = @".partsbnd.dcx";
                BinderType = FlverBinderType.BND;

                if (Smithbox.ProjectType is ProjectType.DS1)
                {
                    partExt = ".partsbnd";
                    BinderType = FlverBinderType.BND;
                }
                else if (Smithbox.ProjectType is ProjectType.DS2S or ProjectType.DS2)
                {
                    partExt = ".bnd";
                    BinderType = FlverBinderType.BND;
                }

                return partExt;
            case FlverContainerType.MapPiece:
                string mapPieceExt = ".mapbnd.dcx";
                BinderType = FlverBinderType.BND;

                if (Smithbox.ProjectType is ProjectType.DS2S or ProjectType.DS2)
                {
                    mapPieceExt = ".mapbhd";
                    BinderType = FlverBinderType.BXF;
                }
                else if (Smithbox.ProjectType is ProjectType.DS1R or ProjectType.BB)
                {
                    mapPieceExt = ".flver.dcx";
                    BinderType = FlverBinderType.None;
                }
                else if (Smithbox.ProjectType is ProjectType.DS1)
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
