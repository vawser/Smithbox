// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiNavMeshGenerationSettingsData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiNavMeshGeneration.Settings>
{
    public hkaiNavMeshGenerationSettingsData(Havok.hk2019.hkaiNavMeshGeneration.Settings instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_quantum":
            case "quantum":
            {
                value = instance.m_quantum;
                return true;
            }
            case "m_up":
            case "up":
            {
                value = instance.m_up;
                return true;
            }
            case "m_minCharacterRadius":
            case "minCharacterRadius":
            {
                value = instance.m_minCharacterRadius;
                return true;
            }
            case "m_maxCharacterRadius":
            case "maxCharacterRadius":
            {
                value = instance.m_maxCharacterRadius;
                return true;
            }
            case "m_maxStepHeight":
            case "maxStepHeight":
            {
                value = instance.m_maxStepHeight;
                return true;
            }
            case "m_maxGapWidth":
            case "maxGapWidth":
            {
                value = instance.m_maxGapWidth;
                return true;
            }
            case "m_canonicalize":
            case "canonicalize":
            {
                value = instance.m_canonicalize;
                return true;
            }
            case "m_faceUserdataStriding":
            case "faceUserdataStriding":
            {
                value = instance.m_faceUserdataStriding;
                return true;
            }
            case "m_precalculateFaceIterator":
            case "precalculateFaceIterator":
            {
                value = instance.m_precalculateFaceIterator;
                return true;
            }
            case "m_simplify":
            case "simplify":
            {
                value = instance.m_simplify;
                return true;
            }
            case "m_pruneRegions":
            case "pruneRegions":
            {
                value = instance.m_pruneRegions;
                return true;
            }
            case "m_regionPruningSettings":
            case "regionPruningSettings":
            {
                value = instance.m_regionPruningSettings;
                return true;
            }
            case "m_simplificationSettings":
            case "simplificationSettings":
            {
                value = instance.m_simplificationSettings;
                return true;
            }
            case "m_simplificationOverrides":
            case "simplificationOverrides":
            {
                value = instance.m_simplificationOverrides;
                return true;
            }
            case "m_bounds":
            case "bounds":
            {
                value = instance.m_bounds;
                return true;
            }
            case "m_outputTypes":
            case "outputTypes":
            {
                value = instance.m_outputTypes;
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
                if (value is not Havok.hk2019.hkaiNavMeshGeneration.RegionPruningSettings castValue) return false;
                instance.m_regionPruningSettings = castValue;
                return true;
            }
            case "m_simplificationSettings":
            case "simplificationSettings":
            {
                if (value is not Havok.hk2019.hkaiNavMeshSimplificationUtils.Settings castValue) return false;
                instance.m_simplificationSettings = castValue;
                return true;
            }
            case "m_simplificationOverrides":
            case "simplificationOverrides":
            {
                if (value is not List<Havok.hk2019.hkaiNavMeshGeneration.Settings.SimplificationOverride> castValue) return false;
                instance.m_simplificationOverrides = castValue;
                return true;
            }
            case "m_bounds":
            case "bounds":
            {
                if (value is not Havok.hk2019.hkAabb castValue) return false;
                instance.m_bounds = castValue;
                return true;
            }
            case "m_outputTypes":
            case "outputTypes":
            {
                if (value is Havok.hk2019.hkaiNavMeshGeneration.Settings.OutputTypes castValue)
                {
                    instance.m_outputTypes = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_outputTypes = (Havok.hk2019.hkaiNavMeshGeneration.Settings.OutputTypes)intValue;
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
