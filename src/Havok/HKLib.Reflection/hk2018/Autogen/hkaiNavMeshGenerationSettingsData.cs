// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;
using HKLib.hk2018.hkaiNavMeshGeneration;

namespace HKLib.Reflection.hk2018;

internal class hkaiNavMeshGenerationSettingsData : HavokData<Settings> 
{
    public hkaiNavMeshGenerationSettingsData(HavokType type, Settings instance) : base(type, instance) {}

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
            case "m_characterHeight":
            case "characterHeight":
            {
                if (instance.m_characterHeight is not TGet castValue) return false;
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
            case "m_maxWalkableSlope":
            case "maxWalkableSlope":
            {
                if (instance.m_maxWalkableSlope is not TGet castValue) return false;
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
            case "m_walkable":
            case "walkable":
            {
                if (instance.m_walkable is not TGet castValue) return false;
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
            case "m_simplify":
            case "simplify":
            {
                if (instance.m_simplify is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_overlappingMaterialCombiner":
            case "overlappingMaterialCombiner":
            {
                if (instance.m_overlappingMaterialCombiner is null)
                {
                    return true;
                }
                if (instance.m_overlappingMaterialCombiner is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_simplificationSettings":
            case "simplificationSettings":
            {
                if (instance.m_simplificationSettings is not TGet castValue) return false;
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
                if (value is OverlappingMaterialCombiner castValue)
                {
                    instance.m_overlappingMaterialCombiner = castValue;
                    return true;
                }
                return false;
            }
            case "m_simplificationSettings":
            case "simplificationSettings":
            {
                if (value is not HKLib.hk2018.hkaiNavMeshSimplificationUtils.Settings castValue) return false;
                instance.m_simplificationSettings = castValue;
                return true;
            }
            case "m_bounds":
            case "bounds":
            {
                if (value is not hkAabb castValue) return false;
                instance.m_bounds = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
