using Andre.Formats;
using HKX2;
using ImGuiNET;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Editors.MapEditor.Toolbar;
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
    // Supports: ER, DS3, SDT
    public static void BonfireWarpParam(string activeParam, Param.Row row, string currentField)
    {
        if (!(Smithbox.ProjectType is ProjectType.ER or ProjectType.DS3 or ProjectType.SDT))
            return;

        if (activeParam == null)
            return;

        if (row == null)
            return;

        if (currentField == null)
            return;

        if (activeParam == "BonfireWarpParam")
        {
            bool show = false;
            var mapId = "";
            var rowMapId = "";
            uint entityID = 0;

            if (Smithbox.ProjectType is ProjectType.ER)
            {
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

                rowMapId = $"m{sAA}_{sBB}_{sCC}_00";
            }
            if (Smithbox.ProjectType is ProjectType.DS3)
            {
                Param.Cell? c = row?["WarpEventId"];
                var value = (int)c.Value.Value;

                entityID = (uint)(int)value;
                entityID = entityID - 1000; // To get the enemy ID

                var idStr = entityID.ToString();

                string sAA = $"{idStr.Substring(0, 2)}";
                string sBB = $"{idStr.Substring(2, 2)}";

                rowMapId = $"m{sAA}_{sBB}_00_00";
            }
            if (Smithbox.ProjectType is ProjectType.SDT)
            {
                Param.Cell? c = row?["bonfireEntityId"];
                var value = (int)c.Value.Value;

                entityID = (uint)(int)value;
                entityID = entityID - 1000; // To get the enemy ID

                var idStr = entityID.ToString();

                string sAA = $"{idStr.Substring(0, 2)}";
                string sBB = $"{idStr.Substring(2, 2)}";

                rowMapId = $"m{sAA}_{sBB}_00_00";
            }

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

    // Supports: ER, DS3, SDT, DS1, DS1R
    public static void GameAreaParam(string activeParam, Param.Row row, string currentField)
    {
        if (!(Smithbox.ProjectType is ProjectType.ER or ProjectType.DS1 or ProjectType.DS1R or ProjectType.DS3 or ProjectType.SDT))
            return;

        if (activeParam == null)
            return;

        if (row == null)
            return;

        if (currentField == null)
            return;

        if (activeParam == "GameAreaParam")
        {
            bool show = false;
            var mapId = "";
            var rowMapId = "";

            uint entityID = 0;
            entityID = (uint)row.ID;

            if (Smithbox.ProjectType is ProjectType.ER)
            {
                Param.Cell? c = row?["bossMapAreaNo"];
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

                rowMapId = $"m{sAA}_{sBB}_{sCC}_00";
            }
            if (Smithbox.ProjectType is ProjectType.DS1 or ProjectType.DS1R or ProjectType.DS3 or ProjectType.SDT)
            {
                var idStr = entityID.ToString();

                if (idStr.Length == 7)
                {
                    string sAA = $"{idStr.Substring(0, 2)}";
                    string sBB = $"{idStr.Substring(2, 2)}";

                    rowMapId = $"m{sAA}_{sBB}_00_00";
                }
            }

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

    // Supports: ER, AC6
    public static void GrassTypeParam(string activeParam, Param.Row row, string currentField)
    {
        if (!(Smithbox.ProjectType is ProjectType.ER or ProjectType.AC6))
            return;

        if (activeParam == null)
            return;

        if (row == null)
            return;

        if (currentField == null)
            return;

        if (activeParam.Contains("GrassTypeParam") && (currentField == "model0Name" || currentField == "model1Name" || currentField == "simpleModelName") )
        {
            Param.Cell? c = row?["model0Name"];
            string modelId1 = (string)c.Value.Value;

            string modelId2 = "";
            if (Smithbox.ProjectType is ProjectType.ER)
            {
                c = row?["model1Name"];
                modelId2 = (string)c.Value.Value;
            }
            if (Smithbox.ProjectType is ProjectType.AC6)
            {
                c = row?["simpleModelName"];
                modelId2 = (string)c.Value.Value;
            }

            var width = ImGui.GetColumnWidth();

            if (currentField == "model0Name" && modelId1 != "")
            {
                if (ImGui.Button($"View Model", new Vector2(width, 20)))
                {
                    EditorCommandQueue.AddCommand($"model/load/{modelId1}/Asset");
                }
                ImguiUtils.ShowHoverTooltip("View this model in the Model Editor, loading it automatically.");
            }

            if ((currentField == "model1Name" || currentField == "simpleModelName") && modelId2 != "")
            {
                if (ImGui.Button($"View Model", new Vector2(width, 20)))
                {
                    EditorCommandQueue.AddCommand($"model/load/{modelId2}/Asset");
                }
                ImguiUtils.ShowHoverTooltip("View this model in the Model Editor, loading it automatically.");
            }
        }
    }

    private static List<string> AssetList;

    // Supports: ER, AC6
    public static void AssetGeometryParam(string activeParam, Param.Row row, string currentField)
    {
        if (!(Smithbox.ProjectType is ProjectType.ER or ProjectType.AC6))
            return;

        if (activeParam == null)
            return;

        if (row == null)
            return;

        if (activeParam == "AssetEnvironmentGeometryParam")
        {
            int rowID = row.ID;
            string assetID = "";

            assetID = DeriveAssetID(rowID);

            var width = ImGui.GetColumnWidth();

            if(AssetList == null)
                AssetList = AssetListLocator.GetObjModels();

            if (AssetList.Contains(assetID.ToLower()) && assetID != "")
            {
                var aliasName = AliasUtils.GetAssetAlias(assetID.ToLower());

                if (ImGui.Button($"View Model: {assetID}", new Vector2(width, 20)))
                {
                    EditorCommandQueue.AddCommand($"model/load/{assetID}/Asset");
                }
                ImguiUtils.ShowWideHoverTooltip($"{assetID}: {aliasName}");
            }
        }
    }

    // Get the asset ID from the AssetGeometryParam row ID.
    private static string DeriveAssetID(int rowID)
    {
        string assetID = "";

        string id = rowID.ToString();
        if (id.Length > 3)
        {
            string assetNum = id.Substring(id.Length - 3, 3);

            if(id.Length == 4)
            {
                string assetCategoryNum = id.Substring(0, 1);
                assetID = $"AEG00{assetCategoryNum}_{assetNum}";
            }
            if (id.Length == 5)
            {
                string assetCategoryNum = id.Substring(0, 2);
                assetID = $"AEG0{assetCategoryNum}_{assetNum}";
            }
            if (id.Length == 6)
            {
                string assetCategoryNum = id.Substring(0, 3);
                assetID = $"AEG{assetCategoryNum}_{assetNum}";
            }
        }
        else
        {
            if (id.Length == 1)
                assetID = $"AEG000_00{id}";
            if (id.Length == 2)
                assetID = $"AEG000_0{id}";
            if (id.Length == 3)
                assetID = $"AEG000_{id}";
        }

        return assetID;
    }

    // Supports: ER
    public static void BuddyStoneParam(string activeParam, Param.Row row, string currentField)
    {
        if (!(Smithbox.ProjectType is ProjectType.ER))
            return;
        
        if (activeParam == null)
            return;

        if (row == null)
            return;

        if (currentField == null)
            return;

        if (activeParam == "BuddyStoneParam")
        {
            bool show = false;
            var mapId = "";

            uint entityID = (uint)row.ID;

            string rowID = row.ID.ToString();

            string AA = "";
            string BB = "";
            string CC = "";

            // Legacy Dungeon
            if (rowID.Length == 8)
            {
                AA = $"{rowID.Substring(0, 2)}";
                BB = $"{rowID.Substring(2, 2)}";
                CC = $"{rowID.Substring(4, 1)}0";
            }
            // Open-world Tile
            else if (rowID.Length == 10)
            {
                AA = $"{rowID.Substring(0, 2)}";

                if (AA == "10")
                    AA = "60";

                if (AA == "20")
                    AA = "61";

                BB = $"{rowID.Substring(2, 2)}";
                CC = $"{rowID.Substring(4, 2)}";
            }
            else
            {
                // Ignore other rows
                return;
            }

            if (AA == "" || BB == "" || CC == "")
                return;

            var rowMapId = $"m{AA}_{BB}_{CC}_00";

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
                ImguiUtils.ShowHoverTooltip("Loads the map and select the buddy stone Enemy map object.");
            }
        }
    }

    public static string CurrentMapID;
    public static MSB1 CurrentPeekMap_DS1;
    public static MSB3 CurrentPeekMap_DS3;
    public static MSBS CurrentPeekMap_SDT;
    public static MSBE CurrentPeekMap_ER;
    public static MSB_AC6 CurrentPeekMap_AC6;

    // Supports: ER
    public static void ItemLotParam(string activeParam, Param.Row row, string currentField)
    {
        if (Smithbox.ProjectType is ProjectType.DS1 or ProjectType.DS1R)
        {
            ItemLotParam_DS1(activeParam, row, currentField);
        }
        if (Smithbox.ProjectType is ProjectType.DS3)
        {
            ItemLotParam_DS3(activeParam, row, currentField);
        }
        if (Smithbox.ProjectType is ProjectType.SDT)
        {
            ItemLotParam_SDT(activeParam, row, currentField);
        }
        if (Smithbox.ProjectType is ProjectType.ER)
        {
            ItemLotParam_ER(activeParam, row, currentField);
        }
        if (Smithbox.ProjectType is ProjectType.AC6)
        {
            ItemLotParam_AC6(activeParam, row, currentField);
        }
    }

    private static void ItemLotParam_Button(string mapId, string AssetName)
    {
        var width = ImGui.GetColumnWidth();

        if (ImGui.Button($"View in Map", new Vector2(width, 20)))
        {
            if (mapId != "")
            {
                EditorCommandQueue.AddCommand($"map/load/{mapId}");
            }
            if (AssetName != "")
            {
                EditorCommandQueue.AddCommand($"map/select/{mapId}/{AssetName}");
            }
        }
        ImguiUtils.ShowHoverTooltip("Loads the map and selects the asset that holds this treasure.");
    }

    public static void ItemLotParam_DS1(string activeParam, Param.Row row, string currentField)
    {
        if (activeParam == null)
            return;

        if (row == null)
            return;

        if (currentField == null)
            return;

        if (activeParam == "ItemLotParam")
        {
            bool show = false;
            var mapId = "";

            string rowID = row.ID.ToString();

            string AA = "";
            string BB = "";

            if (rowID.Length >= 7)
            {
                AA = $"{rowID.Substring(0, 2)}";
                BB = $"{rowID.Substring(2, 2)}";
            }

            if (AA == "" || BB == "")
                return;

            // For some reason they XX_01 lots are setup like this
            if (BB == "10")
                BB = "01";

            var rowMapId = $"m{AA}_{BB}_00_00";

            var mapList = ResourceMapLocator.GetFullMapList();

            if (mapList.Contains(rowMapId))
            {
                show = true;
                mapId = rowMapId;
            }

            if (show)
            {
                if (CurrentMapID != rowMapId)
                {
                    CurrentMapID = rowMapId;
                    var mapPath = ResourceMapLocator.GetMapMSB(rowMapId);
                    CurrentPeekMap_DS1 = MSB1.Read(mapPath.AssetPath);
                }

                if (CurrentPeekMap_DS1 == null)
                    return;

                string AssetName = null;

                foreach (var entry in CurrentPeekMap_DS1.Events.Treasures)
                {
                    if (entry.ItemLots[0] == row.ID)
                    {
                        AssetName = entry.TreasurePartName;
                        break;
                    }
                }

                if (AssetName == null)
                    return;

                ItemLotParam_Button(mapId, AssetName);
            }
        }
    }

    public static void ItemLotParam_DS3(string activeParam, Param.Row row, string currentField)
    {
        if (activeParam == null)
            return;

        if (row == null)
            return;

        if (currentField == null)
            return;

        if (activeParam == "ItemLotParam")
        {
            bool show = false;
            bool isNGPlusLot = false;
            var mapId = "";

            string rowID = row.ID.ToString();

            string AA = "";
            string BB = "";

            if (rowID.Length == 7)
            {
                AA = $"{rowID.Substring(0, 2)}";
                BB = $"{rowID.Substring(2, 2)}";
            }

            // NG+ lots
            if (rowID.Length == 9)
            {
                AA = $"{rowID.Substring(2, 2)}";
                BB = $"{rowID.Substring(4, 2)}";
                isNGPlusLot = true;
            }

            if (AA == "" || BB == "")
                return;

            // For some reason they XX_01 lots are setup like this
            if (BB == "10")
                BB = "01";

            var rowMapId = $"m{AA}_{BB}_00_00";

            var mapList = ResourceMapLocator.GetFullMapList();

            if (mapList.Contains(rowMapId))
            {
                show = true;
                mapId = rowMapId;
            }

            if (show)
            {
                if (CurrentMapID != rowMapId)
                {
                    CurrentMapID = rowMapId;
                    var mapPath = ResourceMapLocator.GetMapMSB(rowMapId);
                    CurrentPeekMap_DS3 = MSB3.Read(mapPath.AssetPath);
                }

                if (CurrentPeekMap_DS3 == null)
                    return;

                string AssetName = null;

                foreach (var entry in CurrentPeekMap_DS3.Events.Treasures)
                {
                    var id = row.ID;

                    if(isNGPlusLot)
                    {
                        id = id - 200000000;
                    }

                    if (entry.ItemLot1 == id)
                    {
                        AssetName = entry.TreasurePartName;
                        break;
                    }
                }

                if (AssetName == null)
                    return;

                ItemLotParam_Button(mapId, AssetName);
            }
        }
    }

    public static void ItemLotParam_SDT(string activeParam, Param.Row row, string currentField)
    {
        if (activeParam == null)
            return;

        if (row == null)
            return;

        if (currentField == null)
            return;

        if (activeParam == "ItemLotParam")
        {
            bool show = false;
            var mapId = "";

            string rowID = row.ID.ToString();

            string AA = "";
            string BB = "";

            if (rowID.Length >= 7)
            {
                AA = $"{rowID.Substring(0, 2)}";
                BB = $"{rowID.Substring(2, 2)}";
            }

            if (AA == "" || BB == "")
                return;

            // For some reason they XX_01 lots are setup like this
            if (BB == "10")
                BB = "01";

            var rowMapId = $"m{AA}_{BB}_00_00";

            var mapList = ResourceMapLocator.GetFullMapList();

            if (mapList.Contains(rowMapId))
            {
                show = true;
                mapId = rowMapId;
            }

            if (show)
            {
                if (CurrentMapID != rowMapId)
                {
                    CurrentMapID = rowMapId;
                    var mapPath = ResourceMapLocator.GetMapMSB(rowMapId);
                    CurrentPeekMap_SDT = MSBS.Read(mapPath.AssetPath);
                }

                if (CurrentPeekMap_SDT == null)
                    return;

                string AssetName = null;

                foreach (var entry in CurrentPeekMap_SDT.Events.Treasures)
                {
                    if (entry.ItemLotID == row.ID)
                    {
                        AssetName = entry.TreasurePartName;
                        break;
                    }
                }

                if (AssetName == null)
                    return;

                ItemLotParam_Button(mapId, AssetName);
            }
        }
    }

    public static void ItemLotParam_ER(string activeParam, Param.Row row, string currentField)
    {
        if (activeParam == null)
            return;

        if (row == null)
            return;

        if (currentField == null)
            return;

        if (activeParam == "ItemLotParam_map")
        {
            bool show = false;
            var mapId = "";

            string rowID = row.ID.ToString();

            string AA = "";
            string BB = "";
            string CC = "";

            // Legacy Dungeon
            if (rowID.Length == 8)
            {
                AA = $"{rowID.Substring(0, 2)}";
                BB = $"{rowID.Substring(2, 2)}";
                CC = $"{rowID.Substring(4, 1)}0";
            }
            // Open-world Tile
            else if (rowID.Length >= 8)
            {
                AA = $"{rowID.Substring(0, 2)}";
                BB = $"{rowID.Substring(2, 2)}";
                CC = $"{rowID.Substring(4, 2)}";
            }

            if (AA == "" || BB == "" || CC == "")
                return;

            var rowMapId = $"m{AA}_{BB}_{CC}_00";

            var mapList = ResourceMapLocator.GetFullMapList();

            if (mapList.Contains(rowMapId))
            {
                show = true;
                mapId = rowMapId;
            }

            if (show)
            {
                if (CurrentMapID != rowMapId)
                {
                    CurrentMapID = rowMapId;
                    var mapPath = ResourceMapLocator.GetMapMSB(rowMapId);
                    CurrentPeekMap_ER = MSBE.Read(mapPath.AssetPath);
                }

                if (CurrentPeekMap_ER == null)
                    return;

                string AssetName = null;

                foreach (var entry in CurrentPeekMap_ER.Events.Treasures)
                {
                    if (entry.ItemLotID == row.ID)
                    {
                        AssetName = entry.TreasurePartName;
                        break;
                    }
                }

                if (AssetName == null)
                    return;

                ItemLotParam_Button(mapId, AssetName);
            }
        }
    }

    public static void ItemLotParam_AC6(string activeParam, Param.Row row, string currentField)
    {
        if (activeParam == null)
            return;

        if (row == null)
            return;

        if (currentField == null)
            return;

        if (activeParam == "ItemLotParam")
        {
            bool show = false;
            var mapId = "";

            string rowID = row.ID.ToString();

            string AA = "01";
            string BB = "";
            string CC = "";

            // 21 0460 3220
            if (rowID.Length >= 10)
            {
                BB = $"{rowID.Substring(2, 2)}";
                CC = $"{rowID.Substring(4, 2)}";
            }

            if (AA == "" || BB == "" || CC == "")
                return;

            var rowMapId = $"m{AA}_{BB}_{CC}_00";

            var mapList = ResourceMapLocator.GetFullMapList();

            if (mapList.Contains(rowMapId))
            {
                show = true;
                mapId = rowMapId;
            }

            if (show)
            {
                if (CurrentMapID != rowMapId)
                {
                    CurrentMapID = rowMapId;
                    var mapPath = ResourceMapLocator.GetMapMSB(rowMapId);
                    CurrentPeekMap_AC6 = MSB_AC6.Read(mapPath.AssetPath);
                }

                if (CurrentPeekMap_AC6 == null)
                    return;

                string AssetName = null;

                foreach (var entry in CurrentPeekMap_AC6.Events.Treasures)
                {
                    if (entry.ItemLotParamId == row.ID)
                    {
                        AssetName = entry.TreasurePartName;
                        break;
                    }
                }

                if (AssetName == null)
                    return;

                ItemLotParam_Button(mapId, AssetName);
            }
        }
    }
}
