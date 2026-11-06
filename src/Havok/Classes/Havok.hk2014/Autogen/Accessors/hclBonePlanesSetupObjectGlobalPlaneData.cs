// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hclBonePlanesSetupObjectGlobalPlaneData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hclBonePlanesSetupObject.GlobalPlane>
{
    public hclBonePlanesSetupObjectGlobalPlaneData(Havok.hk2014.hclBonePlanesSetupObject.GlobalPlane instance) : base(instance) { }

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
            case "m_planeEquationBoneSpace":
            case "planeEquationBoneSpace":
            {
                value = instance.m_planeEquationBoneSpace;
                return true;
            }
            case "m_allowedPenetration":
            case "allowedPenetration":
            {
                value = instance.m_allowedPenetration;
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
                if (value is not Havok.hk2014.hclVertexSelectionInput castValue) return false;
                instance.m_particles = castValue;
                return true;
            }
            case "m_planeEquationBoneSpace":
            case "planeEquationBoneSpace":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_planeEquationBoneSpace = castValue;
                return true;
            }
            case "m_allowedPenetration":
            case "allowedPenetration":
            {
                if (value is not Havok.hk2014.hclVertexFloatInput castValue) return false;
                instance.m_allowedPenetration = castValue;
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
