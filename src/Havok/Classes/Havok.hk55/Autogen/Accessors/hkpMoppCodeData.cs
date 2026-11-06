// Automatically Generated

#nullable enable

namespace Havok.hk55.Accessors;

internal sealed class hkpMoppCodeData : Havok.Formats.HavokFieldAccess<Havok.hk55.hkpMoppCode>
{
    public hkpMoppCodeData(Havok.hk55.hkpMoppCode instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_info":
            case "info":
            {
                value = instance.m_info;
                return true;
            }
            case "m_data":
            case "data":
            {
                value = instance.m_data;
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
            case "m_info":
            case "info":
            {
                if (value is not Havok.hk55.hkpMoppCodeCodeInfo castValue) return false;
                instance.m_info = castValue;
                return true;
            }
            case "m_data":
            case "data":
            {
                if (value is not List<byte> castValue) return false;
                instance.m_data = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
