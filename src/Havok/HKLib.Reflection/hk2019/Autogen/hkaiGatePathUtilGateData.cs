// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiGatePathUtilGateData : HavokData<HKLib.hk2019.hkaiGatePathUtil.Gate> 
{
    public hkaiGatePathUtilGateData(HavokType type, HKLib.hk2019.hkaiGatePathUtil.Gate instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_origin":
            case "origin":
            {
                if (instance.m_origin is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_uLen":
            case "uLen":
            {
                if (instance.m_uLen is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_vLen":
            case "vLen":
            {
                if (instance.m_vLen is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_type":
            case "type":
            {
                if (instance.m_type is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((sbyte)instance.m_type is TGet sbyteValue)
                {
                    value = sbyteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
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
                if (value is HKLib.hk2019.hkaiGatePathUtil.GateTypeValues castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_type = (HKLib.hk2019.hkaiGatePathUtil.GateTypeValues)sbyteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
