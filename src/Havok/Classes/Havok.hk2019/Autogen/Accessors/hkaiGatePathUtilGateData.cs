// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiGatePathUtilGateData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiGatePathUtil.Gate>
{
    public hkaiGatePathUtilGateData(Havok.hk2019.hkaiGatePathUtil.Gate instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_origin":
            case "origin":
            {
                value = instance.m_origin;
                return true;
            }
            case "m_uLen":
            case "uLen":
            {
                value = instance.m_uLen;
                return true;
            }
            case "m_vLen":
            case "vLen":
            {
                value = instance.m_vLen;
                return true;
            }
            case "m_type":
            case "type":
            {
                value = instance.m_type;
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
            case "m_origin":
            case "origin":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_origin = castValue;
                return true;
            }
            case "m_uLen":
            case "uLen":
            {
                if (value is not float castValue) return false;
                instance.m_uLen = castValue;
                return true;
            }
            case "m_vLen":
            case "vLen":
            {
                if (value is not float castValue) return false;
                instance.m_vLen = castValue;
                return true;
            }
            case "m_type":
            case "type":
            {
                if (value is Havok.hk2019.hkaiGatePathUtil.GateTypeValues castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_type = (Havok.hk2019.hkaiGatePathUtil.GateTypeValues)sbyteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
