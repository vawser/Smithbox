using ImGuiNET;
using StudioCore.Editor;
using StudioCore.Interface;
using StudioCore.Locators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

public static class ParamMapReferenceUtils
{
    public static void BonfireWarpParam(string activeParam, ParamEditorSelectionState _selection)
    {
        var activeRow = _selection.GetActiveRow();

        if (activeParam == "BonfireWarpParam")
        {
            bool show = false;
            var mapId = "";

            uint entityID = 0;
            entityID = (uint)activeRow.Cells.Where(e => e.Def.InternalName == "bonfireEntityId").First().Value;
            entityID = entityID - 1000; // To get the enemy ID

            byte AA = (byte)activeRow.Cells.Where(e => e.Def.InternalName == "areaNo").First().Value;
            byte BB = (byte)activeRow.Cells.Where(e => e.Def.InternalName == "gridXNo").First().Value;
            byte CC = (byte)activeRow.Cells.Where(e => e.Def.InternalName == "gridZNo").First().Value;

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
                if (ImGui.Selectable($"View in Map"))
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

    public static void GameAreaParam(string activeParam, ParamEditorSelectionState _selection)
    {
        var activeRow = _selection.GetActiveRow();

        if (activeParam == "GameAreaParam")
        {
            bool show = false;
            var mapId = "";

            uint entityID = 0;
            entityID = (uint)activeRow.ID;

            byte AA = (byte)activeRow.Cells.Where(e => e.Def.InternalName == "bossMapAreaNo").First().Value;
            byte BB = (byte)activeRow.Cells.Where(e => e.Def.InternalName == "bossMapBlockNo").First().Value;
            byte CC = (byte)activeRow.Cells.Where(e => e.Def.InternalName == "bossMapMapNo").First().Value;

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
                if (ImGui.Selectable($"View in Map"))
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
}
