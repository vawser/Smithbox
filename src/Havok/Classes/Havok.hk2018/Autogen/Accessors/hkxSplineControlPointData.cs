// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkxSplineControlPointData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkxSpline.ControlPoint>
{
    public hkxSplineControlPointData(Havok.hk2018.hkxSpline.ControlPoint instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_position":
            case "position":
            {
                value = instance.m_position;
                return true;
            }
            case "m_tangentIn":
            case "tangentIn":
            {
                value = instance.m_tangentIn;
                return true;
            }
            case "m_tangentOut":
            case "tangentOut":
            {
                value = instance.m_tangentOut;
                return true;
            }
            case "m_inType":
            case "inType":
            {
                value = instance.m_inType;
                return true;
            }
            case "m_outType":
            case "outType":
            {
                value = instance.m_outType;
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
                if (value is Havok.hk2018.hkxSpline.ControlType castValue)
                {
                    instance.m_inType = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_inType = (Havok.hk2018.hkxSpline.ControlType)byteValue;
                    return true;
                }
                return false;
            }
            case "m_outType":
            case "outType":
            {
                if (value is Havok.hk2018.hkxSpline.ControlType castValue)
                {
                    instance.m_outType = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_outType = (Havok.hk2018.hkxSpline.ControlType)byteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
