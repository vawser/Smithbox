using HKLib.Serialization.hk2018.Binary;
using SoulsFormats;
using StudioCore.Editors.TextureViewer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Tools.Development
{
    public class FlverTestTool
    {
        public static void TestRead()
        {
            foreach (var (name, info) in TextureFolderBank.FolderBank)
            {
                if (info.Path.Contains("bnd.dcx"))
                {
                    BND4Reader reader = new BND4Reader(info.Path);
                    foreach (var file in reader.Files)
                    {
                        if (file.Name.Contains(".flv"))
                        {
                            TaskLogs.AddLog(file.Name);
                            var flver = FLVER2.Read(reader.ReadFile(file));
                        }
                    }
                }
            }
        }
    }
}
