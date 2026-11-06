// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkpMoppCodeCodeInfoData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkpMoppCodeCodeInfo>
{
    public hkpMoppCodeCodeInfoData(Havok.hk2014.hkpMoppCodeCodeInfo instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_offset":
            case "offset":
            {
                value = instance.m_offset;
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
            case "m_offset":
            case "offset":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_offset = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
