// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiNavigatorLengthLimitData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiNavigatorLengthLimit>
{
    public hkaiNavigatorLengthLimitData(Havok.hk2018.hkaiNavigatorLengthLimit instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_baseLength":
            case "baseLength":
            {
                value = instance.m_baseLength;
                return true;
            }
            case "m_lengthFactor":
            case "lengthFactor":
            {
                value = instance.m_lengthFactor;
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
            case "m_baseLength":
            case "baseLength":
            {
                if (value is not float castValue) return false;
                instance.m_baseLength = castValue;
                return true;
            }
            case "m_lengthFactor":
            case "lengthFactor":
            {
                if (value is not float castValue) return false;
                instance.m_lengthFactor = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
