// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclBonePlanesSetupObjectPerParticlePlaneData : HavokData<hclBonePlanesSetupObject.PerParticlePlane> 
{
    public hclBonePlanesSetupObjectPerParticlePlaneData(HavokType type, hclBonePlanesSetupObject.PerParticlePlane instance) : base(type, instance) {}

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
            case "m_particles":
            case "particles":
            {
                if (instance.m_particles is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_directionBoneSpace":
            case "directionBoneSpace":
            {
                if (instance.m_directionBoneSpace is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_allowedDistance":
            case "allowedDistance":
            {
                if (instance.m_allowedDistance is not TGet castValue) return false;
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
            case "m_particles":
            case "particles":
            {
                if (value is not hclVertexSelectionInput castValue) return false;
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
                if (value is not hclVertexFloatInput castValue) return false;
                instance.m_allowedDistance = castValue;
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
