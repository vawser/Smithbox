// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiNavVolumeGenerationSettingsData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiNavVolumeGenerationSettings>
{
    public hkaiNavVolumeGenerationSettingsData(Havok.hk2018.hkaiNavVolumeGenerationSettings instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                value = instance.m_propertyBag;
                return true;
            }
            case "m_volumeAabb":
            case "volumeAabb":
            {
                value = instance.m_volumeAabb;
                return true;
            }
            case "m_maxHorizontalRange":
            case "maxHorizontalRange":
            {
                value = instance.m_maxHorizontalRange;
                return true;
            }
            case "m_maxVerticalRange":
            case "maxVerticalRange":
            {
                value = instance.m_maxVerticalRange;
                return true;
            }
            case "m_up":
            case "up":
            {
                value = instance.m_up;
                return true;
            }
            case "m_characterHeight":
            case "characterHeight":
            {
                value = instance.m_characterHeight;
                return true;
            }
            case "m_characterDepth":
            case "characterDepth":
            {
                value = instance.m_characterDepth;
                return true;
            }
            case "m_characterWidth":
            case "characterWidth":
            {
                value = instance.m_characterWidth;
                return true;
            }
            case "m_cellWidth":
            case "cellWidth":
            {
                value = instance.m_cellWidth;
                return true;
            }
            case "m_resolutionRoundingMode":
            case "resolutionRoundingMode":
            {
                value = instance.m_resolutionRoundingMode;
                return true;
            }
            case "m_chunkSettings":
            case "chunkSettings":
            {
                value = instance.m_chunkSettings;
                return true;
            }
            case "m_border":
            case "border":
            {
                value = instance.m_border;
                return true;
            }
            case "m_useBorderCells":
            case "useBorderCells":
            {
                value = instance.m_useBorderCells;
                return true;
            }
            case "m_mergingSettings":
            case "mergingSettings":
            {
                value = instance.m_mergingSettings;
                return true;
            }
            case "m_minRegionVolume":
            case "minRegionVolume":
            {
                value = instance.m_minRegionVolume;
                return true;
            }
            case "m_minDistanceToSeedPoints":
            case "minDistanceToSeedPoints":
            {
                value = instance.m_minDistanceToSeedPoints;
                return true;
            }
            case "m_regionSeedPoints":
            case "regionSeedPoints":
            {
                value = instance.m_regionSeedPoints;
                return true;
            }
            case "m_defaultConstructionInfo":
            case "defaultConstructionInfo":
            {
                value = instance.m_defaultConstructionInfo;
                return true;
            }
            case "m_materialMap":
            case "materialMap":
            {
                value = instance.m_materialMap;
                return true;
            }
            case "m_carvers":
            case "carvers":
            {
                value = instance.m_carvers;
                return true;
            }
            case "m_painters":
            case "painters":
            {
                value = instance.m_painters;
                return true;
            }
            case "m_saveInputSnapshot":
            case "saveInputSnapshot":
            {
                value = instance.m_saveInputSnapshot;
                return true;
            }
            case "m_snapshotFilename":
            case "snapshotFilename":
            {
                value = instance.m_snapshotFilename;
                return true;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField(string fieldName, object? value)
    {
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not Havok.hk2018.hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_volumeAabb":
            case "volumeAabb":
            {
                if (value is not Havok.hk2018.hkAabb castValue) return false;
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
                if (value is Havok.hk2018.hkaiNavVolumeGenerationSettings.CellWidthToResolutionRounding castValue)
                {
                    instance.m_resolutionRoundingMode = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_resolutionRoundingMode = (Havok.hk2018.hkaiNavVolumeGenerationSettings.CellWidthToResolutionRounding)byteValue;
                    return true;
                }
                return false;
            }
            case "m_chunkSettings":
            case "chunkSettings":
            {
                if (value is not Havok.hk2018.hkaiNavVolumeGenerationSettings.ChunkSettings castValue) return false;
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
                if (value is not Havok.hk2018.hkaiNavVolumeGenerationSettings.MergingSettings castValue) return false;
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
                if (value is not Havok.hk2018.hkaiNavVolumeGenerationSettings.MaterialConstructionInfo castValue) return false;
                instance.m_defaultConstructionInfo = castValue;
                return true;
            }
            case "m_materialMap":
            case "materialMap":
            {
                if (value is not List<Havok.hk2018.hkaiNavVolumeGenerationSettings.MaterialConstructionInfo> castValue) return false;
                instance.m_materialMap = castValue;
                return true;
            }
            case "m_carvers":
            case "carvers":
            {
                if (value is not List<Havok.hk2018.hkaiCarver?> castValue) return false;
                instance.m_carvers = castValue;
                return true;
            }
            case "m_painters":
            case "painters":
            {
                if (value is not List<Havok.hk2018.hkaiMaterialPainter?> castValue) return false;
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
