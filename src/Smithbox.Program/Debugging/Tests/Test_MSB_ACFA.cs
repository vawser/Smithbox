﻿using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Resource.Locators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace StudioCore.DebugNS;

public static class Test_MSB_ACFA
{
    public static void Display(Smithbox baseEditor, ProjectEntry project)
    {
        var buttonSize = new Vector2(400, 32);

        if (ImGui.Button("Check all Maps for Byte-Perfect Match", buttonSize))
        {
            Run(baseEditor);
        }
    }

    public static bool Run(Smithbox baseEditor)
    {
        var curProject = baseEditor.ProjectManager.SelectedProject;

        List<string> msbs = MapLocator.GetFullMapList(curProject);
        foreach (var msb in msbs)
        {
            ResourceDescriptor path = MapLocator.GetMapMSB(curProject, msb);
#if MSB_READ_WRITE_TEST_LOG_ON_CRASH
            try
            {
#endif
            var bytes = File.ReadAllBytes(path.AssetPath);
            MSBFA m = MSBFA.Read(bytes);
            var written = m.Write(DCX.Type.None);
            MSBFA wm = MSBFA.Read(written);
            var basepath = Path.GetDirectoryName(path.AssetPath).Replace("map", "map_test_mismatches");
            if (!bytes.AsMemory().Span.SequenceEqual(written))
            {
                Directory.CreateDirectory(basepath);
                File.WriteAllBytes(Path.Join(basepath, Path.GetFileNameWithoutExtension(path.AssetPath)),
                    written);
            }
            else
            {
                if (Directory.Exists(basepath))
                {
                    if (File.Exists(Path.Join(basepath, Path.GetFileNameWithoutExtension(path.AssetPath))))
                    {
                        File.Delete(Path.Join(basepath, Path.GetFileNameWithoutExtension(path.AssetPath)));
                    }

                    if (Directory.GetFiles(basepath).Length == 0)
                    {
                        Directory.Delete(basepath);
                    }
                }
            }
#if MSB_READ_WRITE_TEST_LOG_ON_CRASH
        }
            catch (Exception e)
            {
                TaskLogs.AddLog($"Failed testing ACFA MSB: {path.AssetPath}\n{e}", Microsoft.Extensions.Logging.LogLevel.Debug);
            }
#endif
        }

        return true;
    }
}
