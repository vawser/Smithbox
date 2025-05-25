// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclBonePlanesSetupObjectPerParticleAngleData : HavokData<hclBonePlanesSetupObject.PerParticleAngle> 
{
    public hclBonePlanesSetupObjectPerParticleAngleData(HavokType type, hclBonePlanesSetupObject.PerParticleAngle instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_transformName":
            case "transformName":
            {
                if (instance.m_transformName is null)
                {
                    return true;
                }
                if (instance.m_transformName is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_particlesMaxAngle":
            case "particlesMaxAngle":
            {
                if (instance.m_particlesMaxAngle is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_particlesMinAngle":
            case "particlesMinAngle":
            {
                if (instance.m_particlesMinAngle is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_originBoneSpace":
            case "originBoneSpace":
            {
                if (instance.m_originBoneSpace is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_axisBoneSpace":
            case "axisBoneSpace":
            {
                if (instance.m_axisBoneSpace is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_minAngle":
            case "minAngle":
            {
                if (instance.m_minAngle is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxAngle":
            case "maxAngle":
            {
                if (instance.m_maxAngle is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_stiffness":
            case "stiffness":
            {
                if (instance.m_stiffness is not TGet castValue) return false;
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
            case "m_transformName":
            case "transformName":
            {
                if (value is null)
                {
                    instance.m_transformName = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_transformName = castValue;
                    return true;
                }
                return false;
            }
            case "m_particlesMaxAngle":
            case "particlesMaxAngle":
            {
                if (value is not hclVertexSelectionInput castValue) return false;
                instance.m_particlesMaxAngle = castValue;
                return true;
            }
            case "m_particlesMinAngle":
            case "particlesMinAngle":
            {
                if (value is not hclVertexSelectionInput castValue) return false;
                instance.m_particlesMinAngle = castValue;
                return true;
            }
            case "m_originBoneSpace":
            case "originBoneSpace":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_originBoneSpace = castValue;
                return true;
            }
            case "m_axisBoneSpace":
            case "axisBoneSpace":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_axisBoneSpace = castValue;
                return true;
            }
            case "m_minAngle":
            case "minAngle":
            {
                if (value is not hclVertexFloatInput castValue) return false;
                instance.m_minAngle = castValue;
                return true;
            }
            case "m_maxAngle":
            case "maxAngle":
            {
                if (value is not hclVertexFloatInput castValue) return false;
                instance.m_maxAngle = castValue;
                return true;
            }
            case "m_stiffness":
            case "stiffness":
            {
                if (value is not hclVertexFloatInput castValue) return false;
                instance.m_stiffness = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
