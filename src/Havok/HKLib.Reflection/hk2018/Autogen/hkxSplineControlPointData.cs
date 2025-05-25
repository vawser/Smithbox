// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkxSplineControlPointData : HavokData<hkxSpline.ControlPoint> 
{
    public hkxSplineControlPointData(HavokType type, hkxSpline.ControlPoint instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_position":
            case "position":
            {
                if (instance.m_position is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_tangentIn":
            case "tangentIn":
            {
                if (instance.m_tangentIn is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_tangentOut":
            case "tangentOut":
            {
                if (instance.m_tangentOut is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_inType":
            case "inType":
            {
                if (instance.m_inType is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_inType is TGet byteValue)
                {
                    value = byteValue;
                    return true;
                }
                return false;
            }
            case "m_outType":
            case "outType":
            {
                if (instance.m_outType is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_outType is TGet byteValue)
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
            case "m_position":
            case "position":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_position = castValue;
                return true;
            }
            case "m_tangentIn":
            case "tangentIn":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_tangentIn = castValue;
                return true;
            }
            case "m_tangentOut":
            case "tangentOut":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_tangentOut = castValue;
                return true;
            }
            case "m_inType":
            case "inType":
            {
                if (value is hkxSpline.ControlType castValue)
                {
                    instance.m_inType = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_inType = (hkxSpline.ControlType)byteValue;
                    return true;
                }
                return false;
            }
            case "m_outType":
            case "outType":
            {
                if (value is hkxSpline.ControlType castValue)
                {
                    instance.m_outType = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_outType = (hkxSpline.ControlType)byteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
