// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkRangeInt32AttributeData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkRangeInt32Attribute>
{
    public hkRangeInt32AttributeData(Havok.hk2014.hkRangeInt32Attribute instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_absmin":
            case "absmin":
            {
                value = instance.m_absmin;
                return true;
            }
            case "m_absmax":
            case "absmax":
            {
                value = instance.m_absmax;
                return true;
            }
            case "m_softmin":
            case "softmin":
            {
                value = instance.m_softmin;
                return true;
            }
            case "m_softmax":
            case "softmax":
            {
                value = instance.m_softmax;
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
            case "m_absmin":
            case "absmin":
            {
                if (value is not int castValue) return false;
                instance.m_absmin = castValue;
                return true;
            }
            case "m_absmax":
            case "absmax":
            {
                if (value is not int castValue) return false;
                instance.m_absmax = castValue;
                return true;
            }
            case "m_softmin":
            case "softmin":
            {
                if (value is not int castValue) return false;
                instance.m_softmin = castValue;
                return true;
            }
            case "m_softmax":
            case "softmax":
            {
                if (value is not int castValue) return false;
                instance.m_softmax = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
