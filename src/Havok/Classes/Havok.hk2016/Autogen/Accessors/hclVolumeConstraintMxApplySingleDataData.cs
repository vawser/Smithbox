// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hclVolumeConstraintMxApplySingleDataData : Havok.Formats.HavokFieldAccess<Havok.hk2016.hclVolumeConstraintMx.ApplySingleData>
{
    public hclVolumeConstraintMxApplySingleDataData(Havok.hk2016.hclVolumeConstraintMx.ApplySingleData instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_frameVector":
            case "frameVector":
            {
                value = instance.m_frameVector;
                return true;
            }
            case "m_particleIndex":
            case "particleIndex":
            {
                value = instance.m_particleIndex;
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
            case "m_frameVector":
            case "frameVector":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_frameVector = castValue;
                return true;
            }
            case "m_particleIndex":
            case "particleIndex":
            {
                if (value is not ushort castValue) return false;
                instance.m_particleIndex = castValue;
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
