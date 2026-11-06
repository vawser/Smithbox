// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkAabb24_16_24Data : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkAabb24_16_24>
{
    public hkAabb24_16_24Data(Havok.hk2018.hkAabb24_16_24 instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_min":
            case "min":
            {
                value = instance.m_min;
                return true;
            }
            case "m_max":
            case "max":
            {
                value = instance.m_max;
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
            case "m_min":
            case "min":
            {
                if (value is not ulong castValue) return false;
                instance.m_min = castValue;
                return true;
            }
            case "m_max":
            case "max":
            {
                if (value is not ulong castValue) return false;
                instance.m_max = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
