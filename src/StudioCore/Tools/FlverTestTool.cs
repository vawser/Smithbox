using HKLib.Serialization.hk2018.Binary;
using SoulsFormats;
using StudioCore.Editors.TextureViewer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Tools
{
    public class FlverTestTool
    {
        public static void TestRead()
        {
            foreach (var (name, info) in TextureFolderBank.FolderBank)
            {
                TaskLogs.AddLog(info.Path);
            }
        }
    }
}
