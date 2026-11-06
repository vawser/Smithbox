// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkxNodeLimitsData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkxNodeLimits>
{
    public hkxNodeLimitsData(Havok.hk2019.hkxNodeLimits instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_translationLimit":
            case "translationLimit":
            {
                value = instance.m_translationLimit;
                return true;
            }
            case "m_rotationLimit":
            case "rotationLimit":
            {
                value = instance.m_rotationLimit;
                return true;
            }
            case "m_scaleLimit":
            case "scaleLimit":
            {
                value = instance.m_scaleLimit;
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
            case "m_translationLimit":
            case "translationLimit":
            {
                if (value is not Havok.hk2019.hkxNodeLimits.Limit_2 castValue) return false;
                instance.m_translationLimit = castValue;
                return true;
            }
            case "m_rotationLimit":
            case "rotationLimit":
            {
                if (value is not Havok.hk2019.hkxNodeLimits.Limit_2 castValue) return false;
                instance.m_rotationLimit = castValue;
                return true;
            }
            case "m_scaleLimit":
            case "scaleLimit":
            {
                if (value is not Havok.hk2019.hkxNodeLimits.Limit_2 castValue) return false;
                instance.m_scaleLimit = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
