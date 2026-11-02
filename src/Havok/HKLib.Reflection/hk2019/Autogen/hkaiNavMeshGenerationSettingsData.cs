// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiNavMeshGenerationSettingsData : HavokData<HKLib.hk2019.hkaiNavMeshGeneration.Settings> 
{
    public hkaiNavMeshGenerationSettingsData(HavokType type, HKLib.hk2019.hkaiNavMeshGeneration.Settings instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_quantum":
            case "quantum":
            {
                if (instance.m_quantum is not TGet castValue) return false;
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
            case "m_minCharacterRadius":
            case "minCharacterRadius":
            {
                if (instance.m_minCharacterRadius is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxCharacterRadius":
            case "maxCharacterRadius":
            {
                if (instance.m_maxCharacterRadius is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxStepHeight":
            case "maxStepHeight":
            {
                if (instance.m_maxStepHeight is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxGapWidth":
            case "maxGapWidth":
            {
                if (instance.m_maxGapWidth is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_canonicalize":
            case "canonicalize":
            {
                if (instance.m_canonicalize is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_faceUserdataStriding":
            case "faceUserdataStriding":
            {
                if (instance.m_faceUserdataStriding is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_precalculateFaceIterator":
            case "precalculateFaceIterator":
            {
                if (instance.m_precalculateFaceIterator is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_simplify":
            case "simplify":
            {
                if (instance.m_simplify is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_pruneRegions":
            case "pruneRegions":
            {
                if (instance.m_pruneRegions is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_regionPruningSettings":
            case "regionPruningSettings":
            {
                if (instance.m_regionPruningSettings is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_simplificationSettings":
            case "simplificationSettings":
            {
                if (instance.m_simplificationSettings is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_simplificationOverrides":
            case "simplificationOverrides":
            {
                if (instance.m_simplificationOverrides is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_bounds":
            case "bounds":
            {
                if (instance.m_bounds is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_outputTypes":
            case "outputTypes":
            {
                if (instance.m_outputTypes is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((int)instance.m_outputTypes is TGet intValue)
                {
                    value = intValue;
                    return true;
                }
                return false;
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
            case "m_quantum":
            case "quantum":
            {
                if (value is not float castValue) return false;
                instance.m_quantum = castValue;
                return true;
            }
            case "m_up":
            case "up":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_up = castValue;
                return true;
            }
            case "m_minCharacterRadius":
            case "minCharacterRadius":
            {
                if (value is not float castValue) return false;
                instance.m_minCharacterRadius = castValue;
                return true;
            }
            case "m_maxCharacterRadius":
            case "maxCharacterRadius":
            {
                if (value is not float castValue) return false;
                instance.m_maxCharacterRadius = castValue;
                return true;
            }
            case "m_maxStepHeight":
            case "maxStepHeight":
            {
                if (value is not float castValue) return false;
                instance.m_maxStepHeight = castValue;
                return true;
            }
            case "m_maxGapWidth":
            case "maxGapWidth":
            {
                if (value is not float castValue) return false;
                instance.m_maxGapWidth = castValue;
                return true;
            }
            case "m_canonicalize":
            case "canonicalize":
            {
                if (value is not bool castValue) return false;
                instance.m_canonicalize = castValue;
                return true;
            }
            case "m_faceUserdataStriding":
            case "faceUserdataStriding":
            {
                if (value is not int castValue) return false;
                instance.m_faceUserdataStriding = castValue;
                return true;
            }
            case "m_precalculateFaceIterator":
            case "precalculateFaceIterator":
            {
                if (value is not bool castValue) return false;
                instance.m_precalculateFaceIterator = castValue;
                return true;
            }
            case "m_simplify":
            case "simplify":
            {
                if (value is not bool castValue) return false;
                instance.m_simplify = castValue;
                return true;
            }
            case "m_pruneRegions":
            case "pruneRegions":
            {
                if (value is not bool castValue) return false;
                instance.m_pruneRegions = castValue;
                return true;
            }
            case "m_regionPruningSettings":
            case "regionPruningSettings":
            {
                if (value is not HKLib.hk2019.hkaiNavMeshGeneration.RegionPruningSettings castValue) return false;
                instance.m_regionPruningSettings = castValue;
                return true;
            }
            case "m_simplificationSettings":
            case "simplificationSettings":
            {
                if (value is not HKLib.hk2019.hkaiNavMeshSimplificationUtils.Settings castValue) return false;
                instance.m_simplificationSettings = castValue;
                return true;
            }
            case "m_simplificationOverrides":
            case "simplificationOverrides":
            {
                if (value is not List<HKLib.hk2019.hkaiNavMeshGeneration.Settings.SimplificationOverride> castValue) return false;
                instance.m_simplificationOverrides = castValue;
                return true;
            }
            case "m_bounds":
            case "bounds":
            {
                if (value is not HKLib.hk2019.hkAabb castValue) return false;
                instance.m_bounds = castValue;
                return true;
            }
            case "m_outputTypes":
            case "outputTypes":
            {
                if (value is HKLib.hk2019.hkaiNavMeshGeneration.Settings.OutputTypes castValue)
                {
                    instance.m_outputTypes = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_outputTypes = (HKLib.hk2019.hkaiNavMeshGeneration.Settings.OutputTypes)intValue;
                    return true;
                }
                return false;
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
