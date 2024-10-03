using HKLib.hk2018;
using SoulsFormats;
using StudioCore.Core.Project;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor
{
    /// <summary>
    /// This represents the internal FLVER file, attached to a FLVER Container.
    /// </summary>
    public class InternalFlver
    {
        public string Name;
        public string ModelID;
        public FLVER2 CurrentFLVER;
        public string VirtualResourcePath = "";

        // Hold the bytes of the FLVER before saving (for byte perfect test)
        public byte[] InitialFlverBytes { get; set; }
    }

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
        public ModelEditorModelType Type { get; set; }
        public string MapID { get; set; }

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
            Type = ModelEditorModelType.Loose;
            MapID = "";

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

        public FlverContainer(string modelName, ModelEditorModelType modelType, string mapId)
        {
            InternalFlvers = new List<InternalFlver>();

            ContainerName = modelName;
            Type = modelType;
            MapID = mapId;

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

            if (File.Exists(RootBinderPath))
            {
                if (!File.Exists(ModBinderPath))
                {
                    File.Copy(RootBinderPath, ModBinderPath);
                }
            }
            // Mod-only model, no need to copy to mod
            else if(File.Exists(ModBinderPath))
            {
                return true;
            }
            else
            {
                TaskLogs.AddLog($"Container path does not exist: {RootBinderPath}");
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
                case ModelEditorModelType.Character:
                    string chrDir = @"\chr\";

                    if (Smithbox.ProjectType is ProjectType.DS2S or ProjectType.DS2)
                    {
                        chrDir = @"\model\chr\";
                    }

                    return chrDir;
                case ModelEditorModelType.Object:
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
                case ModelEditorModelType.Parts:
                    string partDir = @"\parts\";

                    if (Smithbox.ProjectType is ProjectType.DS2S or ProjectType.DS2)
                    {
                        partDir = @"\model\parts\";
                    }

                    return partDir;
                case ModelEditorModelType.MapPiece:
                    string mapPieceDir = $@"\map\{MapID}\";

                    if (Smithbox.ProjectType is ProjectType.ER)
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
                case ModelEditorModelType.Character:
                    string chrExt = @".chrbnd.dcx";

                    if (Smithbox.ProjectType is ProjectType.DS1)
                    {
                        chrExt = ".chrbnd";
                    }
                    if (Smithbox.ProjectType is ProjectType.DS2S or ProjectType.DS2)
                    {
                        chrExt = ".bnd";
                    }

                    return chrExt;
                case ModelEditorModelType.Object:
                    string objExt = @".objbnd.dcx";

                    if (Smithbox.ProjectType is ProjectType.DS1)
                    {
                        objExt = ".objbnd";
                    }
                    else if (Smithbox.ProjectType is ProjectType.DS2S or ProjectType.DS2)
                    {
                        objExt = ".bnd";
                    }
                    else if (Smithbox.ProjectType is ProjectType.ER)
                    {
                        objExt = ".geombnd.dcx";
                    }
                    else if (Smithbox.ProjectType is ProjectType.AC6)
                    {
                        objExt = ".geombnd.dcx";
                    }

                    return objExt;
                case ModelEditorModelType.Parts:
                    string partExt = @".partsbnd.dcx";

                    if (Smithbox.ProjectType is ProjectType.DS1)
                    {
                        partExt = ".partsbnd";
                    }
                    else if (Smithbox.ProjectType is ProjectType.DS2S or ProjectType.DS2)
                    {
                        partExt = ".bnd";
                    }

                    return partExt;
                case ModelEditorModelType.MapPiece:
                    string mapPieceExt = ".mapbnd.dcx";

                    if (Smithbox.ProjectType is ProjectType.DS2S or ProjectType.DS2)
                    {
                        mapPieceExt = ".mapbdt";
                    }
                    else if (Smithbox.ProjectType is ProjectType.DS1R or ProjectType.BB)
                    {
                        mapPieceExt = ".flver.dcx";
                    }
                    else if (Smithbox.ProjectType is ProjectType.DS1)
                    {
                        mapPieceExt = ".flver";
                    }

                    return mapPieceExt;
                default: break;
            }

            return "";
        }
    }
}
