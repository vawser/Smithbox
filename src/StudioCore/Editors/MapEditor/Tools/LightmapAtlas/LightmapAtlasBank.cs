using HKLib.hk2018.hkAsyncThreadPool;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Core.Project;
using StudioCore.Resource.Locators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor.Tools.LightmapAtlasEditor;

public class LightmapAtlasBank
{
    public Dictionary<string, List<AtlasContainerInfo>> LightmapAtlases = new Dictionary<string, List<AtlasContainerInfo>>();

    public bool IsSaving = false;

    public bool UsesLightmapAtlases()
    {
        if (Smithbox.ProjectType is ProjectType.DS3)
        {
            return true;
        }

        return false;
    }
    public void LoadBank()
    {
        if (!UsesLightmapAtlases())
            return;

        var mapList = MapLocator.GetFullMapList();

        foreach (var map in mapList)
        {
            List<AtlasContainerInfo> mapBTABs = new List<AtlasContainerInfo>();

            List<ResourceDescriptor> resources = MapLocator.GetMapBTABs(map);
            foreach (var entry in resources)
            {
                var info = new AtlasContainerInfo(entry.AssetPath);

                try
                {
                    BTAB lightmapAtlas = BTAB.Read(entry.AssetPath);
                    info.LightmapAtlas = lightmapAtlas;
                    mapBTABs.Add(info);
                }
                catch (Exception e)
                {
                    var filename = Path.GetFileNameWithoutExtension(entry.AssetPath);

                    TaskLogs.AddLog($"Failed to load BTAB file: {filename} at {entry.AssetPath}\n{e.Message}", LogLevel.Error);
                }
            }

            LightmapAtlases.Add(map, mapBTABs);
        }
    }

    // Need to implement this (apply on map save, target specific mapid)
    public void SaveBank(string mapid)
    {
        if (!UsesLightmapAtlases())
            return;

        foreach (var entry in LightmapAtlases[mapid])
        {
            if (entry.IsModified)
            {
                var filePath = entry.AssetPath;
                var projectFilePath = entry.AssetPath.Replace(Smithbox.GameRoot, Smithbox.ProjectRoot);
                var fileName = Path.GetFileName(entry.AssetPath);
                var rootPath = Path.GetDirectoryName(entry.AssetPath);
                var projectPath = rootPath.Replace(Smithbox.GameRoot, Smithbox.ProjectRoot);

                var fileBytes = entry.LightmapAtlas.Write();

                if (fileBytes != null)
                {
                    // Add folder if it does not exist in GameModDirectory
                    if (!Directory.Exists(projectPath))
                    {
                        Directory.CreateDirectory(projectPath);
                    }

                    File.WriteAllBytes(projectFilePath, fileBytes);
                    TaskLogs.AddLog($"Saved BTAB file: {fileName} at {projectFilePath}");
                }
                else
                {
                    TaskLogs.AddLog($"Failed BTAB file: {fileName} at {projectFilePath}", LogLevel.Error);
                }
            }
        }
    }
}

