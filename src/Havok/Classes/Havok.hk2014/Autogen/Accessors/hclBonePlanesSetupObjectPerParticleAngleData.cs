// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hclBonePlanesSetupObjectPerParticleAngleData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hclBonePlanesSetupObject.PerParticleAngle>
{
    public hclBonePlanesSetupObjectPerParticleAngleData(Havok.hk2014.hclBonePlanesSetupObject.PerParticleAngle instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_transformName":
            case "transformName":
            {
                value = instance.m_transformName;
                return true;
            }
            case "m_particlesMaxAngle":
            case "particlesMaxAngle":
            {
                value = instance.m_particlesMaxAngle;
                return true;
            }
            case "m_particlesMinAngle":
            case "particlesMinAngle":
            {
                value = instance.m_particlesMinAngle;
                return true;
            }
            case "m_originBoneSpace":
            case "originBoneSpace":
            {
                value = instance.m_originBoneSpace;
                return true;
            }
            case "m_axisBoneSpace":
            case "axisBoneSpace":
            {
                value = instance.m_axisBoneSpace;
                return true;
            }
            case "m_minAngle":
            case "minAngle":
            {
                value = instance.m_minAngle;
                return true;
            }
            case "m_maxAngle":
            case "maxAngle":
            {
                value = instance.m_maxAngle;
                return true;
            }
            case "m_stiffness":
            case "stiffness":
            {
                value = instance.m_stiffness;
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
                if (value is not Havok.hk2014.hclVertexSelectionInput castValue) return false;
                instance.m_particlesMaxAngle = castValue;
                return true;
            }
            case "m_particlesMinAngle":
            case "particlesMinAngle":
            {
                if (value is not Havok.hk2014.hclVertexSelectionInput castValue) return false;
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
                if (value is not Havok.hk2014.hclVertexFloatInput castValue) return false;
                instance.m_minAngle = castValue;
                return true;
            }
            case "m_maxAngle":
            case "maxAngle":
            {
                if (value is not Havok.hk2014.hclVertexFloatInput castValue) return false;
                instance.m_maxAngle = castValue;
                return true;
            }
            case "m_stiffness":
            case "stiffness":
            {
                if (value is not Havok.hk2014.hclVertexFloatInput castValue) return false;
                instance.m_stiffness = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
