using Hexa.NET.ImGui;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Text;

namespace StudioCore.Developer;

public static class MapScratchpad
{
    public static void Display()
    {
        if (ImGui.BeginMenu("Map"))
        {
            if (ImGui.Selectable("Add Entity Group IDs"))
            {
                MapScratchpadUtil.Process();
            }

            ImGui.EndMenu();
        }
    }
}

public static class MapScratchpadUtil
{
    private static Dictionary<string, string> MapData = new()
    {
        { "m30_00_00_00", "3005750" },
        { "m30_01_00_00", "3015750" },
        { "m31_00_00_00", "3105750" },
        { "m32_00_00_00", "3205750" },
        { "m33_00_00_00", "3305750" },
        { "m34_01_00_00", "3415750" },
        { "m35_00_00_00", "3505750" },
        { "m37_00_00_00", "3705750" },
        { "m38_00_00_00", "3805750" },
        { "m39_00_00_00", "3905750" },
        { "m40_00_00_00", "4005750" },
        { "m41_00_00_00", "4105750" },
        { "m45_00_00_00", "4505750" },
        { "m46_00_00_00", "4605750" },
        { "m47_00_00_00", "4705750" },
        { "m50_00_00_00", "5005750" },
        { "m51_00_00_00", "5105750" },
        { "m51_01_00_00", "5115750" },
        { "m53_00_00_00", "5305750" },
        { "m54_00_00_00", "5405750" },
    };

    private static List<int> IgnoredNPCs = new()
    {
        10000,
        100000
    };

    public static void Process()
    {
        var mapDir = @"G:\Modding\Dark Souls III\Projects\Cinders-Reforged\map\mapstudio";

        foreach(var file in Directory.EnumerateFiles(mapDir))
        {
            var fileData = File.ReadAllBytes(file);
            var filename = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(file));

            if (!MapData.ContainsKey(filename))
                continue;

            var targetEntityGroup = MapData[filename];

            var map = MSB3.Read(fileData);

            ProcessMap(map, filename, targetEntityGroup);

            map.Write(file);
        }
    }

    private static void ProcessMap(MSB3 map, string mapName, string targetEntityGroup)
    {
        int entityGroupID = int.Parse(targetEntityGroup);

        foreach (var enemy in map.Parts.Enemies)
        {
            // Ignore these enemies when applying entity group ID assignement
            if (IgnoredNPCs.Contains(enemy.NPCParamID))
                continue;

            bool applied = false;

            for(int i = 0; i < enemy.EntityGroups.Length; i++)
            {
                var curEntry = enemy.EntityGroups[i];

                // Apply entity group ID to first unused slot
                if (curEntry == -1)
                {
                    enemy.EntityGroups[i] = entityGroupID;
                    applied = true;
                    break;
                }
            }

            if(!applied)
            {
                Smithbox.LogError(typeof(MapScratchpadUtil), $"{mapName}: {enemy.Name} has no unused entity group slots");
            }
        }
    }
}