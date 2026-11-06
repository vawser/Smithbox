// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hclBonePlanesSetupObjectPerParticlePlaneData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hclBonePlanesSetupObject.PerParticlePlane>
{
    public hclBonePlanesSetupObjectPerParticlePlaneData(Havok.hk2018.hclBonePlanesSetupObject.PerParticlePlane instance) : base(instance) { }

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
            case "m_particles":
            case "particles":
            {
                value = instance.m_particles;
                return true;
            }
            case "m_directionBoneSpace":
            case "directionBoneSpace":
            {
                value = instance.m_directionBoneSpace;
                return true;
            }
            case "m_allowedDistance":
            case "allowedDistance":
            {
                value = instance.m_allowedDistance;
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
            case "m_particles":
            case "particles":
            {
                if (value is not Havok.hk2018.hclVertexSelectionInput castValue) return false;
                instance.m_particles = castValue;
                return true;
            }
            case "m_directionBoneSpace":
            case "directionBoneSpace":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_directionBoneSpace = castValue;
                return true;
            }
            case "m_allowedDistance":
            case "allowedDistance":
            {
                if (value is not Havok.hk2018.hclVertexFloatInput castValue) return false;
                instance.m_allowedDistance = castValue;
                return true;
            }
            case "m_stiffness":
            case "stiffness":
            {
                if (value is not Havok.hk2018.hclVertexFloatInput castValue) return false;
                instance.m_stiffness = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
