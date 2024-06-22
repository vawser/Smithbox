using ImGuiNET;
using SoulsFormats;
using SoulsFormats.KF4;
using StudioCore.Core;
using StudioCore.Editors.MapEditor;
using StudioCore.Formats;
using StudioCore.Locators;
using StudioCore.Platform;
using StudioCore.Resource;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace StudioCore.Tools
{
    public static class MapInformationTool
    {
        //public static string exportPath = $"{AppContext.BaseDirectory}";

        public static string exportPath = $"C:\\Users\\benja\\Modding\\FROM Software\\MSB\\";
        // C:\Users\benja\Modding\FROM Software\MSB

        public static bool TargetProject = false;

        public static List<ResourceDescriptor> resMaps = new List<ResourceDescriptor>();

        public static List<MSBE> ER_Maps = new List<MSBE>();
        public static List<MSB_AC6> AC6_Maps = new List<MSB_AC6>();

        public static void SelectExportDirectory()
        {
            if (PlatformUtils.Instance.OpenFolderDialog("Select report directory...", out var selectedPath))
            {
                exportPath = selectedPath;
            }
        }

        public static void GenerateReport()
        {
            var mapDir = $"{Smithbox.GameRoot}/map/mapstudio/";

            if (TargetProject)
            {
                mapDir = $"{Smithbox.ProjectRoot}/map/mapstudio/";
            }

            foreach (var entry in Directory.EnumerateFiles(mapDir))
            {
                if (entry.Contains(".msb.dcx"))
                {
                    var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(entry));
                    ResourceDescriptor ad = ResourceMapLocator.GetMapMSB(name);
                    if (ad.AssetPath != null)
                    {
                        resMaps.Add(ad);
                    }
                }
            }

            // ER
            if(Smithbox.ProjectType == ProjectType.ER)
            {
                foreach (var res in resMaps)
                {
                    var msb = MSBE.Read(res.AssetPath);
                    ER_Maps.Add(msb);
                }

                foreach(var map in ER_Maps)
                {
                    ProcessMap_ER(map);
                }
            }

            // AC6
            if (Smithbox.ProjectType == ProjectType.AC6)
            {
                foreach (var res in resMaps)
                {
                    var msb = MSB_AC6.Read(res.AssetPath);
                    AC6_Maps.Add(msb);
                }
            }
        }

        private static void ProcessMap_ER(MSBE map)
        {
            // Parts
            var enemies = map.Parts.Enemies;

            // Events

            // Regions
        }
    }
}
