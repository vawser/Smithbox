// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkxSplineData : HavokData<hkxSpline> 
{
    public hkxSplineData(HavokType type, hkxSpline instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (instance.m_propertyBag is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_controlPoints":
            case "controlPoints":
            {
                if (instance.m_controlPoints is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_isClosed":
            case "isClosed":
            {
                if (instance.m_isClosed is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
    {
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_controlPoints":
            case "controlPoints":
            {
                if (value is not List<hkxSpline.ControlPoint> castValue) return false;
                instance.m_controlPoints = castValue;
                return true;
            }
            case "m_isClosed":
            case "isClosed":
            {
                if (value is not bool castValue) return false;
                instance.m_isClosed = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
