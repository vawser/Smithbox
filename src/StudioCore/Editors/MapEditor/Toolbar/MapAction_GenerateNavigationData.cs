using ImGuiNET;
using StudioCore.Editor;
using StudioCore.MsbEditor;
using StudioCore.UserProject;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor.Toolbar
{
    public static class MapAction_GenerateNavigationData
    {
        private static bool NavigationDataProcessed = false;
        private static int FrameCount = 0;

        public static void Select(ViewportSelection _selection)
        {
            if (Project.Type is ProjectType.DES || Project.Type is ProjectType.DS1 || Project.Type is ProjectType.DS1R)
            {
                if (ImGui.Selectable("Navigation Data##tool_Selection_Generate_Navigation_Data", false, ImGuiSelectableFlags.AllowDoubleClick))
                {
                    MapEditorState.CurrentTool = SelectedTool.Selection_Generate_Navigation_Data;

                    if (ImGui.IsMouseDoubleClicked(0) && _selection.IsSelection())
                    {
                        Act(_selection);
                    }
                }
            }
        }

        public static void Act(ViewportSelection _selection)
        {
            Dictionary<string, ObjectContainer> orderedMaps = MapEditorState.Universe.LoadedObjectContainers;

            HashSet<string> idCache = new();
            foreach (var map in orderedMaps)
            {
                string mapid = map.Key;

                if (Project.Type is ProjectType.DES)
                {
                    if (mapid != "m03_01_00_99" && !mapid.StartsWith("m99"))
                    {
                        var areaId = mapid.Substring(0, 3);
                        if (idCache.Contains(areaId))
                            continue;
                        idCache.Add(areaId);

                        var areaDirectories = new List<string>();
                        foreach (var orderMap in orderedMaps)
                        {
                            if (orderMap.Key.StartsWith(areaId) && orderMap.Key != "m03_01_00_99")
                            {
                                areaDirectories.Add(Path.Combine(Project.GameRootDirectory, "map", orderMap.Key));
                            }
                        }
                        SoulsMapMetadataGenerator.GenerateMCGMCP(areaDirectories, toBigEndian: true);
                    }
                    else
                    {
                        var areaDirectories = new List<string> { Path.Combine(Project.GameRootDirectory, "map", mapid) };
                        SoulsMapMetadataGenerator.GenerateMCGMCP(areaDirectories, toBigEndian: true);
                    }
                }
                else if (Project.Type is ProjectType.DS1 or ProjectType.DS1R)
                {
                    var areaDirectories = new List<string> { Path.Combine(Project.GameRootDirectory, "map", mapid) };

                    SoulsMapMetadataGenerator.GenerateMCGMCP(areaDirectories, toBigEndian: false);
                }
            }

            NavigationDataProcessed = true;
        }

        public static void Configure(ViewportSelection _selection)
        {
            if (MapEditorState.CurrentTool == SelectedTool.Selection_Generate_Navigation_Data)
            {
                ImGui.Text("Regenerate the navigation data files used for pathfinding.");
                ImGui.Separator();

                if (NavigationDataProcessed)
                {
                    ImGui.Text("Navigation data has been regenerated for all maps.");
                }
            }
        }

        public static void OnTextReset()
        {
            // This is to reset temporary Text elements. Only used by Generate Navigation Data currently.
            if (FrameCount > 1000)
            {
                FrameCount = 0;
                NavigationDataProcessed = false;
            }
            FrameCount++;
        }
    }
}
