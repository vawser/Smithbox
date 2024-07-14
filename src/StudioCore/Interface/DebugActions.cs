using ImGuiNET;
using SoulsFormats;
using StudioCore.Editors.MapEditor;
using StudioCore.Formats;
using StudioCore.Locators;
using StudioCore.Platform;
using StudioCore.Resource;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace StudioCore.Interface;

public static class DebugActions
{
    public static List<MSB_AC6> maps = new List<MSB_AC6>();
    public static List<ResourceDescriptor> resMaps = new List<ResourceDescriptor>();
    private static MapPropertyCache _propCache;

    public static void LoadMsbData()
    {
        var mapDir = $"{Smithbox.GameRoot}/map/mapstudio/";

        foreach(var entry in Directory.EnumerateFiles(mapDir))
        {
            if (entry.Contains(".msb.dcx"))
            {
                var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(entry));
                ResourceDescriptor ad = MapLocator.GetMapMSB(name);
                if (ad.AssetPath != null)
                {
                    resMaps.Add(ad);
                }
            }
        }

        foreach(var res in resMaps)
        {
            var msb = MSB_AC6.Read(res.AssetPath);
            maps.Add(msb);
        }
    }

    public static void SearchInMsbForValue(int searchValue)
    {
        foreach(var map in maps)
        {
            var enemies = map.Parts.Enemies;
            foreach(var ene in enemies)
            {
                var prop = ene.GetType();
                PropertyInfo[] properties = prop.GetProperties();
                foreach(var field in properties)
                {
                    TaskLogs.AddLog($"{field.Name}");
                }
            }
        }
    }

    public static void ForceCrash()
    {
        var badArray = new int[2];
        var crash = badArray[5];
    }

    public static void DumpFlverLayouts()
    {
        if (PlatformUtils.Instance.SaveFileDialog("Save Flver layout dump", new[] { FilterStrings.TxtFilter },
                 out var path))
        {
            using (StreamWriter file = new(path))
            {
                foreach (KeyValuePair<string, FLVER2.BufferLayout> mat in FlverResource.MaterialLayouts)
                {
                    file.WriteLine(mat.Key + ":");
                    foreach (FLVER.LayoutMember member in mat.Value)
                    {
                        file.WriteLine($@"{member.Index}: {member.Type.ToString()}: {member.Semantic.ToString()}");
                    }

                    file.WriteLine();
                }
            }
        }
    }

    public static void DecryptRegulation()
    {
        var path = "C:\\Users\\benja\\Modding\\Elden Ring\\Projects\\ER-SOTE\\Mod\\regulation.bin";
        var writePath = "C:\\Users\\benja\\Modding\\Elden Ring\\Projects\\ER-SOTE\\Mod\\decrypted_regulation.bin";

        byte[] bytes = File.ReadAllBytes(path);
        bytes = SFUtil.DecryptByteArray(SFUtil.erRegulationKey, bytes);
        File.WriteAllBytes(writePath, bytes);
    }
}
