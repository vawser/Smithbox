using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Interface;
using StudioCore.Resource.Locators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.DebugNS;

public static class MapValidator
{
    public static bool HasFinished = false;

    public static bool TargetProjectFiles = false;

    public static List<ResourceDescriptor> resMaps = new List<ResourceDescriptor>();

    public static void Display(Smithbox baseEditor, ProjectEntry project)
    {
        if (project == null)
            return;

        if (project.MapEditor == null)
            return;

        var buttonSize = new Vector2(400, 32);

        ImGui.Text("This tool will validate the MSB for the current project by loading all MSB files.");
        ImGui.Text("");

        if (HasFinished)
        {
            ImGui.Text("Validation has finished.");
            ImGui.Text("");
        }

        ImGui.Checkbox("Check project files", ref TargetProjectFiles);
        UIHelper.Tooltip("The check will use the game root files by default, if you want to use your project's specific files, tick this.");

        if (ImGui.Button("Validate MSB", buttonSize))
        {
            ValidateMSB(baseEditor, project);
        }
    }

    public static void ValidateMSB(Smithbox baseEditor, ProjectEntry curProject)
    {
        // Disable this since it ignores asserts if on.
        CFG.Current.System_IgnoreAsserts = false;
        HasFinished = false;

        var mapDir = $"{curProject.DataPath}/map/mapstudio/";

        if (TargetProjectFiles)
        {
            mapDir = $"{curProject.ProjectPath}/map/mapstudio/";
        }

        foreach (var entry in Directory.EnumerateFiles(mapDir))
        {
            if (entry.Contains(".msb.dcx"))
            {
                var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(entry));
                ResourceDescriptor ad = MapLocator.GetMapMSB(curProject, name);
                if (ad.AssetPath != null)
                {
                    resMaps.Add(ad);
                }
            }
        }

        if (curProject.ProjectType == ProjectType.DES)
        {
            foreach (var res in resMaps)
            {
                try
                {
                    var msb = MSBD.Read(res.AssetPath);
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"[{res.AssetPath}", Microsoft.Extensions.Logging.LogLevel.Error, Tasks.LogPriority.High, e);
                }
            }
        }
        if (curProject.ProjectType == ProjectType.DS1 || curProject.ProjectType == ProjectType.DS1R)
        {
            foreach (var res in resMaps)
            {
                try
                {
                    var msb = MSB1.Read(res.AssetPath);
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"[{res.AssetPath}", Microsoft.Extensions.Logging.LogLevel.Error, Tasks.LogPriority.High, e);
                }
            }
        }
        if (curProject.ProjectType == ProjectType.DS2 || curProject.ProjectType == ProjectType.DS2S)
        {
            foreach (var res in resMaps)
            {
                try
                {
                    var msb = MSB2.Read(res.AssetPath);
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"[{res.AssetPath}", Microsoft.Extensions.Logging.LogLevel.Error, Tasks.LogPriority.High, e);
                }
            }
        }
        if (curProject.ProjectType == ProjectType.DS3)
        {
            foreach (var res in resMaps)
            {
                try
                {
                    var msb = MSB3.Read(res.AssetPath);
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"[{res.AssetPath}", Microsoft.Extensions.Logging.LogLevel.Error, Tasks.LogPriority.High, e);
                }
            }
        }
        if (curProject.ProjectType == ProjectType.BB)
        {
            foreach (var res in resMaps)
            {
                try
                {
                    var msb = MSBB.Read(res.AssetPath);
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"[{res.AssetPath}", Microsoft.Extensions.Logging.LogLevel.Error, Tasks.LogPriority.High, e);
                }
            }
        }
        if (curProject.ProjectType == ProjectType.SDT)
        {
            foreach (var res in resMaps)
            {
                try
                {
                    var msb = MSBS.Read(res.AssetPath);
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"[{res.AssetPath}", Microsoft.Extensions.Logging.LogLevel.Error, Tasks.LogPriority.High, e);
                }
            }
        }
        if (curProject.ProjectType == ProjectType.ER)
        {
            foreach (var res in resMaps)
            {
                try
                {
                    var msb = MSBE.Read(res.AssetPath);
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"[{res.AssetPath}", Microsoft.Extensions.Logging.LogLevel.Error, Tasks.LogPriority.High, e);
                }
            }
        }
        if (curProject.ProjectType == ProjectType.AC6)
        {
            foreach (var res in resMaps)
            {
                try
                {
                    var msb = MSB_AC6.Read(res.AssetPath);
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"[{res.AssetPath}", Microsoft.Extensions.Logging.LogLevel.Error, Tasks.LogPriority.High, e);
                }
            }
        }
        if (curProject.ProjectType == ProjectType.ERN)
        {
            foreach (var res in resMaps)
            {
                try
                {
                    var msb = MSB_ERN.Read(res.AssetPath);

                    if(msb.Regions.Unknown_0x3s.Count > 0)
                    {
                        TaskLogs.AddLog(res.AssetPath);
                    }
                    if (msb.Regions.Unknown_0x38s.Count > 0)
                    {
                        TaskLogs.AddLog(res.AssetPath);
                    }
                    if (msb.Regions.Unknown_0x39s.Count > 0)
                    {
                        TaskLogs.AddLog(res.AssetPath);
                    }
                    if (msb.Regions.Unknown_0x3As.Count > 0)
                    {
                        TaskLogs.AddLog(res.AssetPath);
                    }
                    if (msb.Regions.Unknown_0x3Bs.Count > 0)
                    {
                        TaskLogs.AddLog(res.AssetPath);
                    }
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"[{res.AssetPath}", Microsoft.Extensions.Logging.LogLevel.Error, Tasks.LogPriority.High, e);
                }
            }
        }

        HasFinished = true;
    }
}
