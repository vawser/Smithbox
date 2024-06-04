// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiNavVolumeGenerationSettingsData : HavokData<hkaiNavVolumeGenerationSettings> 
{
    public hkaiNavVolumeGenerationSettingsData(HavokType type, hkaiNavVolumeGenerationSettings instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (instance.m_propertyBag is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_volumeAabb":
            case "volumeAabb":
            {
                if (instance.m_volumeAabb is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxHorizontalRange":
            case "maxHorizontalRange":
            {
                if (instance.m_maxHorizontalRange is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxVerticalRange":
            case "maxVerticalRange":
            {
                if (instance.m_maxVerticalRange is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_up":
            case "up":
            {
                if (instance.m_up is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_characterHeight":
            case "characterHeight":
            {
                if (instance.m_characterHeight is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_characterDepth":
            case "characterDepth":
            {
                if (instance.m_characterDepth is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_characterWidth":
            case "characterWidth":
            {
                if (instance.m_characterWidth is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_cellWidth":
            case "cellWidth":
            {
                if (instance.m_cellWidth is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_resolutionRoundingMode":
            case "resolutionRoundingMode":
            {
                if (instance.m_resolutionRoundingMode is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_resolutionRoundingMode is TGet byteValue)
                {
                    value = byteValue;
                    return true;
                }
                return false;
            }
            case "m_chunkSettings":
            case "chunkSettings":
            {
                if (instance.m_chunkSettings is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_border":
            case "border":
            {
                if (instance.m_border is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_useBorderCells":
            case "useBorderCells":
            {
                if (instance.m_useBorderCells is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_mergingSettings":
            case "mergingSettings":
            {
                if (instance.m_mergingSettings is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_minRegionVolume":
            case "minRegionVolume":
            {
                if (instance.m_minRegionVolume is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_minDistanceToSeedPoints":
            case "minDistanceToSeedPoints":
            {
                if (instance.m_minDistanceToSeedPoints is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_regionSeedPoints":
            case "regionSeedPoints":
            {
                if (instance.m_regionSeedPoints is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_defaultConstructionInfo":
            case "defaultConstructionInfo":
            {
                if (instance.m_defaultConstructionInfo is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_materialMap":
            case "materialMap":
            {
                if (instance.m_materialMap is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_carvers":
            case "carvers":
            {
                if (instance.m_carvers is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_painters":
            case "painters":
            {
                if (instance.m_painters is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_saveInputSnapshot":
            case "saveInputSnapshot":
            {
                if (instance.m_saveInputSnapshot is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_snapshotFilename":
            case "snapshotFilename":
            {
                if (instance.m_snapshotFilename is null)
                {
                    return true;
                }
                if (instance.m_snapshotFilename is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
    {
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_volumeAabb":
            case "volumeAabb":
            {
                if (value is not hkAabb castValue) return false;
                instance.m_volumeAabb = castValue;
                return true;
            }
            case "m_maxHorizontalRange":
            case "maxHorizontalRange":
            {
                if (value is not float castValue) return false;
                instance.m_maxHorizontalRange = castValue;
                return true;
            }
            case "m_maxVerticalRange":
            case "maxVerticalRange":
            {
                if (value is not float castValue) return false;
                instance.m_maxVerticalRange = castValue;
                return true;
            }
            case "m_up":
            case "up":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_up = castValue;
                return true;
            }
            case "m_characterHeight":
            case "characterHeight":
            {
                if (value is not float castValue) return false;
                instance.m_characterHeight = castValue;
                return true;
            }
            case "m_characterDepth":
            case "characterDepth":
            {
                if (value is not float castValue) return false;
                instance.m_characterDepth = castValue;
                return true;
            }
            case "m_characterWidth":
            case "characterWidth":
            {
                if (value is not float castValue) return false;
                instance.m_characterWidth = castValue;
                return true;
            }
            case "m_cellWidth":
            case "cellWidth":
            {
                if (value is not float castValue) return false;
                instance.m_cellWidth = castValue;
                return true;
            }
            case "m_resolutionRoundingMode":
            case "resolutionRoundingMode":
            {
                if (value is hkaiNavVolumeGenerationSettings.CellWidthToResolutionRounding castValue)
                {
                    instance.m_resolutionRoundingMode = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_resolutionRoundingMode = (hkaiNavVolumeGenerationSettings.CellWidthToResolutionRounding)byteValue;
                    return true;
                }
                return false;
            }
            case "m_chunkSettings":
            case "chunkSettings":
            {
                if (value is not hkaiNavVolumeGenerationSettings.ChunkSettings castValue) return false;
                instance.m_chunkSettings = castValue;
                return true;
            }
            case "m_border":
            case "border":
            {
                if (value is not float castValue) return false;
                instance.m_border = castValue;
                return true;
            }
            case "m_useBorderCells":
            case "useBorderCells":
            {
                if (value is not bool castValue) return false;
                instance.m_useBorderCells = castValue;
                return true;
            }
            case "m_mergingSettings":
            case "mergingSettings":
            {
                if (value is not hkaiNavVolumeGenerationSettings.MergingSettings castValue) return false;
                instance.m_mergingSettings = castValue;
                return true;
            }
            case "m_minRegionVolume":
            case "minRegionVolume":
            {
                if (value is not float castValue) return false;
                instance.m_minRegionVolume = castValue;
                return true;
            }
            case "m_minDistanceToSeedPoints":
            case "minDistanceToSeedPoints":
            {
                if (value is not float castValue) return false;
                instance.m_minDistanceToSeedPoints = castValue;
                return true;
            }
            case "m_regionSeedPoints":
            case "regionSeedPoints":
            {
                if (value is not List<Vector4> castValue) return false;
                instance.m_regionSeedPoints = castValue;
                return true;
            }
            case "m_defaultConstructionInfo":
            case "defaultConstructionInfo":
            {
                if (value is not hkaiNavVolumeGenerationSettings.MaterialConstructionInfo castValue) return false;
                instance.m_defaultConstructionInfo = castValue;
                return true;
            }
            case "m_materialMap":
            case "materialMap":
            {
                if (value is not List<hkaiNavVolumeGenerationSettings.MaterialConstructionInfo> castValue) return false;
                instance.m_materialMap = castValue;
                return true;
            }
            case "m_carvers":
            case "carvers":
            {
                if (value is not List<hkaiCarver?> castValue) return false;
                instance.m_carvers = castValue;
                return true;
            }
            case "m_painters":
            case "painters":
            {
                if (value is not List<hkaiMaterialPainter?> castValue) return false;
                instance.m_painters = castValue;
                return true;
            }
            case "m_saveInputSnapshot":
            case "saveInputSnapshot":
            {
                if (value is not bool castValue) return false;
                instance.m_saveInputSnapshot = castValue;
                return true;
            }
            case "m_snapshotFilename":
            case "snapshotFilename":
            {
                if (value is null)
                {
                    instance.m_snapshotFilename = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_snapshotFilename = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
