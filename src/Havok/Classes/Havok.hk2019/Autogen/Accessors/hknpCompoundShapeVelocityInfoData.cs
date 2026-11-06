// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hknpCompoundShapeVelocityInfoData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hknpCompoundShape.VelocityInfo>
{
    public hknpCompoundShapeVelocityInfoData(Havok.hk2019.hknpCompoundShape.VelocityInfo instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_linearVelocity":
            case "linearVelocity":
            {
                value = instance.m_linearVelocity;
                return true;
            }
            case "m_angularVelocity":
            case "angularVelocity":
            {
                value = instance.m_angularVelocity;
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
            case "m_linearVelocity":
            case "linearVelocity":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_linearVelocity = castValue;
                return true;
            }
            case "m_angularVelocity":
            case "angularVelocity":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_angularVelocity = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
