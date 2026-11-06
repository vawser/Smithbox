// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiNavMeshGenerationSettingsData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiNavMeshGeneration.Settings>
{
    public hkaiNavMeshGenerationSettingsData(Havok.hk2018.hkaiNavMeshGeneration.Settings instance) : base(instance) { }

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
            case "m_characterHeight":
            case "characterHeight":
            {
                value = instance.m_characterHeight;
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
            case "m_maxWalkableSlope":
            case "maxWalkableSlope":
            {
                value = instance.m_maxWalkableSlope;
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
            case "m_walkable":
            case "walkable":
            {
                value = instance.m_walkable;
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
            case "m_simplify":
            case "simplify":
            {
                value = instance.m_simplify;
                return true;
            }
            case "m_overlappingMaterialCombiner":
            case "overlappingMaterialCombiner":
            {
                value = instance.m_overlappingMaterialCombiner;
                return true;
            }
            case "m_simplificationSettings":
            case "simplificationSettings":
            {
                value = instance.m_simplificationSettings;
                return true;
            }
            case "m_bounds":
            case "bounds":
            {
                value = instance.m_bounds;
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
            case "m_characterHeight":
            case "characterHeight":
            {
                if (value is not float castValue) return false;
                instance.m_characterHeight = castValue;
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
            case "m_maxWalkableSlope":
            case "maxWalkableSlope":
            {
                if (value is not float castValue) return false;
                instance.m_maxWalkableSlope = castValue;
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
            case "m_walkable":
            case "walkable":
            {
                if (value is not bool castValue) return false;
                instance.m_walkable = castValue;
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
            case "m_simplify":
            case "simplify":
            {
                if (value is not bool castValue) return false;
                instance.m_simplify = castValue;
                return true;
            }
            case "m_overlappingMaterialCombiner":
            case "overlappingMaterialCombiner":
            {
                if (value is null)
                {
                    instance.m_overlappingMaterialCombiner = default;
                    return true;
                }
                if (value is Havok.hk2018.hkaiNavMeshGeneration.OverlappingMaterialCombiner castValue)
                {
                    instance.m_overlappingMaterialCombiner = castValue;
                    return true;
                }
                return false;
            }
            case "m_simplificationSettings":
            case "simplificationSettings":
            {
                if (value is not Havok.hk2018.hkaiNavMeshSimplificationUtils.Settings castValue) return false;
                instance.m_simplificationSettings = castValue;
                return true;
            }
            case "m_bounds":
            case "bounds":
            {
                if (value is not Havok.hk2018.hkAabb castValue) return false;
                instance.m_bounds = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
