// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hknpTyremarksWheelData : HavokData<hknpTyremarksWheel> 
{
    public hknpTyremarksWheelData(HavokType type, hknpTyremarksWheel instance) : base(type, instance) {}

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
            case "m_currentPosition":
            case "currentPosition":
            {
                if (instance.m_currentPosition is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_numPoints":
            case "numPoints":
            {
                if (instance.m_numPoints is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_tyremarkPoints":
            case "tyremarkPoints":
            {
                if (instance.m_tyremarkPoints is not TGet castValue) return false;
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
            case "m_currentPosition":
            case "currentPosition":
            {
                if (value is not int castValue) return false;
                instance.m_currentPosition = castValue;
                return true;
            }
            case "m_numPoints":
            case "numPoints":
            {
                if (value is not int castValue) return false;
                instance.m_numPoints = castValue;
                return true;
            }
            case "m_tyremarkPoints":
            case "tyremarkPoints":
            {
                if (value is not List<hknpTyremarkPoint> castValue) return false;
                instance.m_tyremarkPoints = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
