// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkpMoppCodeData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkpMoppCode>
{
    public hkpMoppCodeData(Havok.hk2014.hkpMoppCode instance) : base(instance) { }

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
            case "m_buildType":
            case "buildType":
            {
                value = instance.m_buildType;
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
                if (value is not Havok.hk2014.hkpMoppCodeCodeInfo castValue) return false;
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
            case "m_buildType":
            case "buildType":
            {
                if (value is Havok.hk2014.hkpMoppCode.BuildType castValue)
                {
                    instance.m_buildType = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_buildType = (Havok.hk2014.hkpMoppCode.BuildType)sbyteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
