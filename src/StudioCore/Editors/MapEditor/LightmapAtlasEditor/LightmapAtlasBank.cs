using SoulsFormats;
using StudioCore.Core.Project;
using StudioCore.Locators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor.LightmapAtlasEditor;

public class LightmapAtlasBank
{
    public Dictionary<string, List<LightmapAtlasInfo>> LightmapAtlases = new Dictionary<string, List<LightmapAtlasInfo>>();

    public bool IsSaving = false;

    public bool UsesLightmapAtlases()
    {
        if (Smithbox.ProjectType is ProjectType.DS2 or ProjectType.DS2S or ProjectType.DS3)
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
            List<LightmapAtlasInfo> mapBTABs = new List<LightmapAtlasInfo>();

            List<ResourceDescriptor> resources = MapLocator.GetMapBTABs(map);
            foreach(var entry in resources)
            {
                var info = new LightmapAtlasInfo(entry.AssetPath);

                try
                {
                    BTAB lightmapAtlas = BTAB.Read(entry.AssetPath);
                    info.LightmapAtlas = lightmapAtlas;
                    mapBTABs.Add(info);
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"Failed to load BTAB file: {entry.AssetPath} - {e.Message}");
                }
            }

            LightmapAtlases.Add(map, mapBTABs);
        }
    }

    // Need to implement this (apply on map save, target specific mapid)
    public void SaveBank(string mapid)
    {

    }
}

public class LightmapAtlasInfo
{
    public string Filename { get; set; }
    public BTAB LightmapAtlas { get; set; }

    public bool IsModified { get; set; }

    public LightmapAtlasInfo()
    {
        IsModified = false;
    }

    public LightmapAtlasInfo(string path)
    {
        Filename = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(path));
    }
}