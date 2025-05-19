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


        return newAsset;
    }
}
