using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor.Defaults;

public class MapObjectDefaults
{
    /// <summary>
    /// Create blank ObjAct event with proper defaults.
    /// </summary>
    /// <returns></returns>
    public static MSBE.Event.ObjAct CreateBlankObjAct_ER()
    {
        var newObjAct = new MSBE.Event.ObjAct();

        newObjAct.EventID = 0;
        newObjAct.PartName = "";
        newObjAct.RegionName = "";
        newObjAct.EntityID = 0;
        newObjAct.Unk14 = 0;
        newObjAct.MapID = -1;
        newObjAct.UnkE0C = 255;
        newObjAct.UnkS04 = 0;
        newObjAct.UnkS08 = 0;
        newObjAct.UnkS0C = -1;
        newObjAct.ObjActEntityID = 0;
        newObjAct.ObjActPartName = "";
        newObjAct.ObjActID = -1;
        newObjAct.StateType = 0;
        newObjAct.EventFlagID = 0;

        return newObjAct;
    }

    /// <summary>
    /// Create blank Treasure event with proper defaults.
    /// </summary>
    /// <returns></returns>
    public static MSBE.Event.Treasure CreateBlankTreasure_ER()
    {
        var newTreasure = new MSBE.Event.Treasure();
        newTreasure.EventID = 0;
        newTreasure.PartName = "";
        newTreasure.RegionName = "";
        newTreasure.EntityID = 0;
        newTreasure.Unk14 = 0;
        newTreasure.MapID = -1;
        newTreasure.UnkE0C = 255;
        newTreasure.UnkS04 = 0;
        newTreasure.UnkS08 = 0;
        newTreasure.UnkS0C = -1;
        newTreasure.TreasurePartName = "";
        newTreasure.ItemLotID = -1;
        newTreasure.ActionButtonID = -1;
        newTreasure.PickupAnimID = -1;
        newTreasure.InChest = 0;
        newTreasure.StartDisabled = 0;

        return newTreasure;
    }

    /// <summary>
    /// Create blank Asset part with proper defaults.
    /// </summary>
    /// <returns></returns>
    public static MSBE.Part.Asset CreateBlankAsset_ER()
    {
        var newAsset = new MSBE.Part.Asset();

        newAsset.ModelName = "";
        newAsset.InstanceID = 0;
        newAsset.SibPath = "";
        newAsset.Position = new Vector3(0, 0, 0);
        newAsset.Rotation = new Vector3(0, 0, 0);
        newAsset.Scale = new Vector3(1, 1, 1);
        newAsset.GameEditionDisable = 0;
        newAsset.MapStudioLayer = 4294967295;
        newAsset.EntityID = 0;
        newAsset.isUsePartsDrawParamID = 0;
        newAsset.PartsDrawParamID = 0;
        newAsset.IsPointLightShadowSrc = 0;
        newAsset.UnkE0B = 0;
        newAsset.IsShadowSrc = false;
        newAsset.IsStaticShadowSrc = 0;
        newAsset.IsCascade3ShadowSrc = 0;
        newAsset.UnkE0F = 1;
        newAsset.UnkE10 = 0;
        newAsset.IsShadowDest = true;
        newAsset.IsShadowOnly = false;
        newAsset.DrawByReflectCam = false;
        newAsset.DrawOnlyReflectCam = false;
        newAsset.EnableOnAboveShadow = 0;
        newAsset.DisablePointLightEffect = false;
        newAsset.UnkE17 = 0;
        newAsset.UnkE18 = 0;
        newAsset.EntityGroupIDs[0] = 0;
        newAsset.EntityGroupIDs[1] = 0;
        newAsset.EntityGroupIDs[2] = 0;
        newAsset.EntityGroupIDs[3] = 0;
        newAsset.EntityGroupIDs[4] = 0;
        newAsset.EntityGroupIDs[5] = 0;
        newAsset.EntityGroupIDs[6] = 0;
        newAsset.EntityGroupIDs[7] = 0;
        newAsset.UnkE3C = -1;
        newAsset.UnkE3E = 0;

        // Display Data
        newAsset.DisplayDataStruct.DisplayGroups[0] = 0;
        newAsset.DisplayDataStruct.DisplayGroups[1] = 0;
        newAsset.DisplayDataStruct.DisplayGroups[2] = 0;
        newAsset.DisplayDataStruct.DisplayGroups[3] = 0;
        newAsset.DisplayDataStruct.DisplayGroups[4] = 0;
        newAsset.DisplayDataStruct.DisplayGroups[5] = 0;
        newAsset.DisplayDataStruct.DisplayGroups[6] = 0;
        newAsset.DisplayDataStruct.DisplayGroups[7] = 0;

        newAsset.DisplayDataStruct.DrawGroups[0] = 0;
        newAsset.DisplayDataStruct.DrawGroups[1] = 0;
        newAsset.DisplayDataStruct.DrawGroups[2] = 0;
        newAsset.DisplayDataStruct.DrawGroups[3] = 0;
        newAsset.DisplayDataStruct.DrawGroups[4] = 0;
        newAsset.DisplayDataStruct.DrawGroups[5] = 0;
        newAsset.DisplayDataStruct.DrawGroups[6] = 0;

        newAsset.DisplayDataStruct.CollisionMask[0] = 0;
        newAsset.DisplayDataStruct.CollisionMask[1] = 0;
        newAsset.DisplayDataStruct.CollisionMask[2] = 0;
        newAsset.DisplayDataStruct.CollisionMask[3] = 0;
        newAsset.DisplayDataStruct.CollisionMask[4] = 0;
        newAsset.DisplayDataStruct.CollisionMask[5] = 0;
        newAsset.DisplayDataStruct.CollisionMask[6] = 0;
        newAsset.DisplayDataStruct.CollisionMask[7] = 0;
        newAsset.DisplayDataStruct.CollisionMask[8] = 0;
        newAsset.DisplayDataStruct.CollisionMask[9] = 0;
        newAsset.DisplayDataStruct.CollisionMask[10] = 0;
        newAsset.DisplayDataStruct.CollisionMask[11] = 0;
        newAsset.DisplayDataStruct.CollisionMask[12] = 0;
        newAsset.DisplayDataStruct.CollisionMask[13] = 0;
        newAsset.DisplayDataStruct.CollisionMask[14] = 0;
        newAsset.DisplayDataStruct.CollisionMask[15] = 0;
        newAsset.DisplayDataStruct.CollisionMask[16] = 0;
        newAsset.DisplayDataStruct.CollisionMask[17] = 0;
        newAsset.DisplayDataStruct.CollisionMask[18] = 0;
        newAsset.DisplayDataStruct.CollisionMask[19] = 0;
        newAsset.DisplayDataStruct.CollisionMask[20] = 0;
        newAsset.DisplayDataStruct.CollisionMask[21] = 0;
        newAsset.DisplayDataStruct.CollisionMask[22] = 0;
        newAsset.DisplayDataStruct.CollisionMask[23] = 0;
        newAsset.DisplayDataStruct.CollisionMask[24] = 0;
        newAsset.DisplayDataStruct.CollisionMask[25] = 0;
        newAsset.DisplayDataStruct.CollisionMask[26] = 0;
        newAsset.DisplayDataStruct.CollisionMask[27] = 0;
        newAsset.DisplayDataStruct.CollisionMask[28] = 0;
        newAsset.DisplayDataStruct.CollisionMask[29] = 0;
        newAsset.DisplayDataStruct.CollisionMask[30] = 0;
        newAsset.DisplayDataStruct.CollisionMask[31] = 0;

        newAsset.DisplayDataStruct.Condition1 = 0;
        newAsset.DisplayDataStruct.Condition2 = 0;

        newAsset.DisplayDataStruct.UnkC2 = 0;
        newAsset.DisplayDataStruct.UnkC3 = 0;
        newAsset.DisplayDataStruct.UnkC4 = -1;
        newAsset.DisplayDataStruct.UnkC6 = 0;

        // Display Groups
        newAsset.DisplayGroupStruct.Condition = -1;

        newAsset.DisplayGroupStruct.DispGroups[0] = 0;
        newAsset.DisplayGroupStruct.DispGroups[1] = 0;
        newAsset.DisplayGroupStruct.DispGroups[2] = 0;
        newAsset.DisplayGroupStruct.DispGroups[3] = 0;
        newAsset.DisplayGroupStruct.DispGroups[4] = 0;
        newAsset.DisplayGroupStruct.DispGroups[5] = 0;
        newAsset.DisplayGroupStruct.DispGroups[6] = 0;
        newAsset.DisplayGroupStruct.DispGroups[7] = 0;

        newAsset.DisplayGroupStruct.Unk24 = 0;
        newAsset.DisplayGroupStruct.Unk26 = -1;

        // Gparam Config
        newAsset.GparamConfigStruct.LightSetID = -1;
        newAsset.GparamConfigStruct.FogParamID = -1;
        newAsset.GparamConfigStruct.LightScatteringID = 0;
        newAsset.GparamConfigStruct.EnvMapID = 0;

        // Grass Config
        newAsset.GrassConfigStruct.GrassParamId0 = 0;
        newAsset.GrassConfigStruct.GrassParamId1 = 0;
        newAsset.GrassConfigStruct.GrassParamId2 = 0;
        newAsset.GrassConfigStruct.GrassParamId3 = 0;
        newAsset.GrassConfigStruct.GrassParamId4 = 0;
        newAsset.GrassConfigStruct.GrassParamId5 = 0;
        newAsset.GrassConfigStruct.GrassConfigStruct_Unk18 = -1;

        // Unk 8
        newAsset.UnkStruct8.UnkStruct8_Unk00 = 0;

        // Unk 9
        newAsset.UnkStruct9.UnkStruct9_Unk00 = 0;

        // Tile Load Config
        newAsset.TileLoadConfig.MapID[0] = 255;
        newAsset.TileLoadConfig.MapID[1] = 255;
        newAsset.TileLoadConfig.MapID[2] = 255;
        newAsset.TileLoadConfig.MapID[3] = 255;

        newAsset.TileLoadConfig.TileLoadConfig_Unk04 = 0;
        newAsset.TileLoadConfig.TileLoadConfig_Unk0C = -1;
        newAsset.TileLoadConfig.TileLoadConfig_Unk10 = 0;
        newAsset.TileLoadConfig.CullingHeightBehavior = -1;

        // Unk 11
        newAsset.UnkStruct11.UnkStruct11_Unk00 = 0;
        newAsset.UnkStruct11.UnkStruct11_Unk04 = 0;

        // Asset
        newAsset.UnkT00 = 0;
        newAsset.UnkT02 = 0;
        newAsset.UnkT10 = 0;
        newAsset.UnkT11 = false;
        newAsset.UnkT12 = 255;
        newAsset.AssetSfxParamRelativeID = -1;
        newAsset.UnkT1E = -1;
        newAsset.UnkT24 = -1;
        newAsset.UnkT28 = 0;
        newAsset.UnkT30 = -1;
        newAsset.UnkT34 = -1;

        newAsset.PartNames[0] = "";
        newAsset.PartNames[1] = "";
        newAsset.PartNames[2] = "";
        newAsset.PartNames[3] = "";
        newAsset.PartNames[4] = "";
        newAsset.PartNames[5] = "";

        newAsset.UnkT50 = false;
        newAsset.UnkT51 = 0;
        newAsset.UnkT53 = 0;
        newAsset.UnkT54PartName = "";
        newAsset.UnkModelMaskAndAnimID = -1;
        newAsset.UnkT5C = -1;
        newAsset.UnkT60 = -1;
        newAsset.UnkT64 = -1;

        // Asset Unk 1
        newAsset.AssetUnk1.AssetStruct1_Unk00 = 0;
        newAsset.AssetUnk1.AssetStruct1_Unk04 = false;
        newAsset.AssetUnk1.DisableTorrentAssetOnly = false;
        newAsset.AssetUnk1.AssetStruct1_Unk1C = -1;
        newAsset.AssetUnk1.AssetStruct1_Unk24 = -1;
        newAsset.AssetUnk1.AssetStruct1_Unk26 = -1;
        newAsset.AssetUnk1.AssetStruct1_Unk28 = -1;
        newAsset.AssetUnk1.AssetStruct1_Unk2C = -1;

        // Asset Unk 2
        newAsset.AssetUnk2.AssetUnkStruct2_Unk00 = 0;
        newAsset.AssetUnk2.AssetUnkStruct2_Unk04 = -1;
        newAsset.AssetUnk2.AssetUnkStruct2_Unk14 = -1;
        newAsset.AssetUnk2.AssetUnkStruct2_Unk1C = 255;
        newAsset.AssetUnk2.AssetUnkStruct2_Unk1D = 255;
        newAsset.AssetUnk2.AssetUnkStruct2_Unk1E = 255;
        newAsset.AssetUnk2.AssetUnkStruct2_Unk1F = 255;

        // Asset Unk 3
        newAsset.AssetUnk3.AssetUnkStruct3_Unk00 = 0;
        newAsset.AssetUnk3.AssetUnkStruct3_Unk04 = 0;
        newAsset.AssetUnk3.AssetUnkStruct3_Unk09 = 255;
        newAsset.AssetUnk3.AssetUnkStruct3_Unk0A = 0;
        newAsset.AssetUnk3.AssetUnkStruct3_Unk0B = 255;
        newAsset.AssetUnk3.AssetUnkStruct3_Unk0C = -1;
        newAsset.AssetUnk3.AssetUnkStruct3_Unk0E = 0;
        newAsset.AssetUnk3.AssetUnkStruct3_Unk10 = -1;

        newAsset.AssetUnk3.DisableWhenMapLoadedMapID[0] = -1;
        newAsset.AssetUnk3.DisableWhenMapLoadedMapID[1] = -1;
        newAsset.AssetUnk3.DisableWhenMapLoadedMapID[2] = -1;
        newAsset.AssetUnk3.DisableWhenMapLoadedMapID[3] = -1;

        newAsset.AssetUnk3.AssetUnkStruct3_Unk18 = -1;
        newAsset.AssetUnk3.AssetUnkStruct3_Unk1C = -1;
        newAsset.AssetUnk3.AssetUnkStruct3_Unk20 = -1;
        newAsset.AssetUnk3.AssetUnkStruct3_Unk24 = -1;
        newAsset.AssetUnk3.AssetUnkStruct3_Unk25 = false;
        newAsset.AssetUnk3.AssetUnkStruct3_Unk28 = 0;

        // Asset Unk 4
        newAsset.AssetUnk4.AssetUnkStruct4_Unk00 = false;
        newAsset.AssetUnk4.AssetUnkStruct4_Unk01 = 255;
        newAsset.AssetUnk4.AssetUnkStruct4_Unk02 = 255;
        newAsset.AssetUnk4.AssetUnkStruct4_Unk03 = false;

        return newAsset;
    }
}
