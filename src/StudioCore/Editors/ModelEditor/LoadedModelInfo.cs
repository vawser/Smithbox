using StudioCore.UserProject;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor
{
    public class LoadedModelInfo
    {
        public string ModelName { get; set; }
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

        public LoadedModelInfo(string modelName, ModelEditorModelType modelType, string mapId)
        {
            ModelName = modelName;
            Type = modelType;
            MapID = mapId;

            BinderDirectory = GetBinderDirectory();
            BinderExtension = GetBinderExtension();
            BinderPath = $"{BinderDirectory}{ModelName}{BinderExtension}";
            RootBinderPath = $"{Project.GameRootDirectory}{BinderPath}";
            ModBinderPath = $"{Project.GameModDirectory}{BinderPath}";
            ModBinderDirectory = $"{Project.GameModDirectory}{BinderDirectory}";

            FlverFileExtension = GetFlverExtension();
            FlverFileName = $"{ModelName}{FlverFileExtension}";
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

            if (Project.Type == ProjectType.DS2S)
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

                    if (Project.Type == ProjectType.DS2S)
                    {
                        chrDir = @"\model\chr\";
                    }

                    return chrDir;
                case ModelEditorModelType.Object:
                    string objDir = @"\obj\";

                    if (Project.Type == ProjectType.DS2S)
                    {
                        objDir = @"\model\obj\";
                    }
                    else if (Project.Type == ProjectType.ER)
                    {
                        var category = ModelName.Split("_")[0];
                        objDir = $@"\asset\aeg\{category}\";
                    }
                    else if (Project.Type == ProjectType.AC6)
                    {
                        objDir = @"\asset\environment\geometry\";
                    }

                    return objDir;
                case ModelEditorModelType.Parts:
                    string partDir = @"\parts\";

                    if (Project.Type == ProjectType.DS2S)
                    {
                        partDir = @"\model\parts\";
                    }

                    return partDir;
                case ModelEditorModelType.MapPiece:
                    string mapPieceDir = $@"\map\{MapID}\";

                    if (Project.Type == ProjectType.ER)
                    {
                        string shortMapId = MapID.Split("_")[0];
                        mapPieceDir = $@"\map\{shortMapId}\{MapID}\";
                    }

                    if (Project.Type == ProjectType.DS2S)
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

                    if (Project.Type == ProjectType.DS1)
                    {
                        chrExt = ".chrbnd";
                    }
                    if (Project.Type == ProjectType.DS2S)
                    {
                        chrExt = ".bnd";
                    }

                    return chrExt;
                case ModelEditorModelType.Object:
                    string objExt = @".objbnd.dcx";

                    if (Project.Type == ProjectType.DS1)
                    {
                        objExt = ".objbnd";
                    }
                    else if (Project.Type == ProjectType.DS2S)
                    {
                        objExt = ".bnd";
                    }
                    else if (Project.Type == ProjectType.ER)
                    {
                        objExt = ".geombnd.dcx";
                    }
                    else if (Project.Type == ProjectType.AC6)
                    {
                        objExt = ".geombnd.dcx";
                    }

                    return objExt;
                case ModelEditorModelType.Parts:
                    string partExt = @".partsbnd.dcx";

                    if (Project.Type == ProjectType.DS1)
                    {
                        partExt = ".partsbnd";
                    }
                    else if (Project.Type == ProjectType.DS2S)
                    {
                        partExt = ".bnd";
                    }

                    return partExt;
                case ModelEditorModelType.MapPiece:
                    string mapPieceExt = ".mapbnd.dcx";

                    if (Project.Type == ProjectType.DS2S)
                    {
                        mapPieceExt = ".mapbdt";
                    }
                    else if (Project.Type == ProjectType.DS1 || Project.Type == ProjectType.DS1R)
                    {
                        mapPieceExt = ".flver.dcx";
                    }

                    return mapPieceExt;
                default: break;
            }

            return "";
        }
    }
}
