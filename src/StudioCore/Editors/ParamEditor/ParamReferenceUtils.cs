using Andre.Formats;
using ImGuiNET;
using StudioCore.Editor;
using StudioCore.Interface;
using StudioCore.Locators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

public static class ParamReferenceUtils
{
    public static void BonfireWarpParam(string activeParam, Param.Row row, string currentField)
    {
        if (activeParam == "BonfireWarpParam" && currentField == "bonfireEntityId")
        {
            bool show = false;
            var mapId = "";

            uint entityID = 0;
            Param.Cell? c = row?["bonfireEntityId"];
            entityID = (uint)c.Value.Value;
            entityID = entityID - 1000; // To get the enemy ID

            c = row?["areaNo"];
            byte AA = (byte)c.Value.Value;
            c = row?["gridXNo"];
            byte BB = (byte)c.Value.Value;
            c = row?["gridZNo"];
            byte CC = (byte)c.Value.Value;

            string sAA = $"{AA}";
            string sBB = $"{BB}";
            string sCC = $"{CC}";

            if (AA < 10)
                sAA = $"0{AA}";

            if (BB < 10)
                sBB = $"0{BB}";

            if (CC < 10)
                sCC = $"0{CC}";

            var rowMapId = $"m{sAA}_{sBB}_{sCC}_00";

            var mapList = ResourceMapLocator.GetFullMapList();

            if (mapList.Contains(rowMapId))
            {
                show = true;
                mapId = rowMapId;
            }

            if (show)
            {
                var width = ImGui.GetColumnWidth();

                if (ImGui.Button($"View in Map", new Vector2(width, 20)))
                {
                    if (mapId != "")
                    {
                        EditorCommandQueue.AddCommand($"map/load/{mapId}");
                    }
                    if (entityID != 0)
                    {
                        EditorCommandQueue.AddCommand($"map/idselect/enemy/{mapId}/{entityID}");
                    }
                }
                ImguiUtils.ShowHoverTooltip("Loads the map this bonfire is located in and selects the bonfire Enemy map object automatically, allowing you to frame it immediately.");
            }
        }
    }

    public static void GameAreaParam(string activeParam, Param.Row row, string currentField)
    {
        if (activeParam == "GameAreaParam" && currentField == "defeatBossFlagId")
        {
            bool show = false;
            var mapId = "";

            uint entityID = 0;
            entityID = (uint)row.ID;

            Param.Cell?  c = row?["bossMapAreaNo"];
            byte AA = (byte)c.Value.Value;
            c = row?["bossMapBlockNo"];
            byte BB = (byte)c.Value.Value;
            c = row?["bossMapMapNo"];
            byte CC = (byte)c.Value.Value;

            string sAA = $"{AA}";
            string sBB = $"{BB}";
            string sCC = $"{CC}";

            if (AA < 10)
                sAA = $"0{AA}";

            if (BB < 10)
                sBB = $"0{BB}";

            if (CC < 10)
                sCC = $"0{CC}";

            var rowMapId = $"m{sAA}_{sBB}_{sCC}_00";

            var mapList = ResourceMapLocator.GetFullMapList();

            if (mapList.Contains(rowMapId))
            {
                show = true;
                mapId = rowMapId;
            }

            if (show)
            {
                var width = ImGui.GetColumnWidth();

                if (ImGui.Button($"View in Map", new Vector2(width, 20)))
                {
                    if (mapId != "")
                    {
                        EditorCommandQueue.AddCommand($"map/load/{mapId}");
                    }
                    if (entityID != 0)
                    {
                        EditorCommandQueue.AddCommand($"map/idselect/enemy/{mapId}/{entityID}");
                    }
                }
                ImguiUtils.ShowHoverTooltip("Loads the map this boss is located in and selects the boss Enemy map object automatically, allowing you to frame it immediately.");
            }
        }
    }

    public static void GrassTypeParam(string activeParam, Param.Row row, string currentField)
    {
        if (activeParam.Contains("GrassTypeParam") && (currentField == "model0Name" || currentField == "model1Name" ) )
        {
            Param.Cell? c = row?["model0Name"];
            string modelId1 = (string)c.Value.Value;
            c = row?["model1Name"];
            string modelId2 = (string)c.Value.Value;

            var width = ImGui.GetColumnWidth();

            if (currentField == "model0Name" && modelId1 != "")
            {
                if (ImGui.Button($"View Model", new Vector2(width, 20)))
                {
                    EditorCommandQueue.AddCommand($"model/load/{modelId1}/Asset");
                }
                ImguiUtils.ShowHoverTooltip("View this model in the Model Editor, loading it automatically.");
            }

            if (currentField == "model1Name" && modelId2 != "")
            {
                if (ImGui.Button($"View Model", new Vector2(width, 20)))
                {
                    EditorCommandQueue.AddCommand($"model/load/{modelId2}/Asset");
                }
                ImguiUtils.ShowHoverTooltip("View this model in the Model Editor, loading it automatically.");
            }
        }
    }
}
