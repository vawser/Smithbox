// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiPathData : HavokData<hkaiPath> 
{
    public hkaiPathData(HavokType type, hkaiPath instance) : base(type, instance) {}

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
            case "m_points":
            case "points":
            {
                if (instance.m_points is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_referenceFrame":
            case "referenceFrame":
            {
                if (instance.m_referenceFrame is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_referenceFrame is TGet byteValue)
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_points":
            case "points":
            {
                if (value is not List<hkaiPath.PathPoint> castValue) return false;
                instance.m_points = castValue;
                return true;
            }
            case "m_referenceFrame":
            case "referenceFrame":
            {
                if (value is hkaiPath.ReferenceFrame castValue)
                {
                    instance.m_referenceFrame = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_referenceFrame = (hkaiPath.ReferenceFrame)byteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
