using SoulsFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Locators;

public static class MergeUtils
{

    public static List<MapInfo> GetMaps(string projectPath)
    {
        List<MapInfo> maps = new List<MapInfo>();

        if(projectPath == "")
        {
            return maps;
        }

        var path = $"{projectPath}\\map\\MapStudio\\";

        foreach(var mapPath in Directory.GetFiles($@"{path}", "*.msb.dcx"))
        {
            MapInfo info = new MapInfo();
            info.mapId = GeNormalFileName(mapPath);
            info.mapPath = mapPath;

            try
            {
                var map = MSBE.Read(mapPath);
                info.map = map;
            }
            catch (Exception ex)
            {
                TaskLogs.AddLog(ex.Message);
            }

            maps.Add(info);
        }

        return maps;
    }

    public class MapInfo
    {
        public string mapId;
        public string mapPath;
        public MSBE map;
        public MSBE diffMap;
    }

    public static string GeNormalFileName(string path)
    {
        return Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(path));
    }
}
