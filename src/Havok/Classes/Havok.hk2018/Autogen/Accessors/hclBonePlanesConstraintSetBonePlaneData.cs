// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hclBonePlanesConstraintSetBonePlaneData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hclBonePlanesConstraintSet.BonePlane>
{
    public hclBonePlanesConstraintSetBonePlaneData(Havok.hk2018.hclBonePlanesConstraintSet.BonePlane instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_planeEquationBone":
            case "planeEquationBone":
            {
                value = instance.m_planeEquationBone;
                return true;
            }
            case "m_particleIndex":
            case "particleIndex":
            {
                value = instance.m_particleIndex;
                return true;
            }
            case "m_transformIndex":
            case "transformIndex":
            {
                value = instance.m_transformIndex;
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
            case "m_planeEquationBone":
            case "planeEquationBone":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_planeEquationBone = castValue;
                return true;
            }
            case "m_particleIndex":
            case "particleIndex":
            {
                if (value is not ushort castValue) return false;
                instance.m_particleIndex = castValue;
                return true;
            }
            case "m_transformIndex":
            case "transformIndex":
            {
                if (value is not ushort castValue) return false;
                instance.m_transformIndex = castValue;
                return true;
            }
            case "m_stiffness":
            case "stiffness":
            {
                if (value is not float castValue) return false;
                instance.m_stiffness = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
