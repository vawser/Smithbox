using ImGuiNET;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Interface;
using StudioCore.MsbEditor;
using StudioCore.Platform;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
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
            if (Smithbox.ProjectType is ProjectType.DES || Smithbox.ProjectType is ProjectType.DS1 || Smithbox.ProjectType is ProjectType.DS1R)
            {
                if (ImGui.RadioButton("Navigation Data##tool_Selection_Generate_Navigation_Data", MapEditorState.SelectedAction == MapEditorAction.Selection_Generate_Navigation_Data))
                {
                    MapEditorState.SelectedAction = MapEditorAction.Selection_Generate_Navigation_Data;
                }

                if (!CFG.Current.Interface_MapEditor_Toolbar_ActionList_TopToBottom)
                {
                    ImGui.SameLine();
                }
            }
        }

        public static void Configure(ViewportSelection _selection)
        {
            if (MapEditorState.SelectedAction == MapEditorAction.Selection_Generate_Navigation_Data)
            {
                ImguiUtils.WrappedText("Regenerate the navigation data files used for pathfinding.");
                ImguiUtils.WrappedText("");

                if (NavigationDataProcessed)
                {
                    ImguiUtils.WrappedText("Navigation data has been regenerated for all maps.");
                }
            }
        }

        public static void Act(ViewportSelection _selection)
        {
            if (MapEditorState.SelectedAction == MapEditorAction.Selection_Generate_Navigation_Data)
            {
                if (ImGui.Button("Apply##action_Selection_Generate_Navigation_Data", new Vector2(200, 32)))
                {
                    if (_selection.IsSelection())
                    {
                        ApplyNavigationGeneration(_selection);
                    }
                    else
                    {
                        PlatformUtils.Instance.MessageBox("No object selected.", "Smithbox", MessageBoxButtons.OK);
                    }
                }
            }
        }
        public static void Shortcuts()
        {
            if (MapEditorState.SelectedAction == MapEditorAction.Selection_Generate_Navigation_Data)
            {

            }
        }

        public static void ApplyNavigationGeneration(ViewportSelection _selection)
        {
            Dictionary<string, ObjectContainer> orderedMaps = MapEditorState.Universe.LoadedObjectContainers;

            HashSet<string> idCache = new();
            foreach (var map in orderedMaps)
            {
                string mapid = map.Key;

                if (Smithbox.ProjectType is ProjectType.DES)
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
                                areaDirectories.Add(Path.Combine(Smithbox.GameRoot, "map", orderMap.Key));
                            }
                        }
                        SoulsMapMetadataGenerator.GenerateMCGMCP(areaDirectories, toBigEndian: true);
                    }
                    else
                    {
                        var areaDirectories = new List<string> { Path.Combine(Smithbox.GameRoot, "map", mapid) };
                        SoulsMapMetadataGenerator.GenerateMCGMCP(areaDirectories, toBigEndian: true);
                    }
                }
                else if (Smithbox.ProjectType is ProjectType.DS1 or ProjectType.DS1R)
                {
                    var areaDirectories = new List<string> { Path.Combine(Smithbox.GameRoot, "map", mapid) };

                    SoulsMapMetadataGenerator.GenerateMCGMCP(areaDirectories, toBigEndian: false);
                }
            }

            NavigationDataProcessed = true;
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
