// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018.hkaiGatePathUtil;

namespace HKLib.Reflection.hk2018;

internal class hkaiGatePathUtilGateData : HavokData<Gate> 
{
    public hkaiGatePathUtilGateData(HavokType type, Gate instance) : base(type, instance) {}

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
                if ((byte)instance.m_type is TGet byteValue)
                {
                    value = byteValue;
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
                if (value is GateTypeValues castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_type = (GateTypeValues)byteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
