// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hknpShapeMassConfigData : HavokData<hknpShape.MassConfig> 
{
    public hknpShapeMassConfigData(HavokType type, hknpShape.MassConfig instance) : base(type, instance) {}

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
            case "m_quality":
            case "quality":
            {
                if (instance.m_quality is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((int)instance.m_quality is TGet intValue)
                {
                    value = intValue;
                    return true;
                }
                return false;
            }
            case "m_inertiaFactor":
            case "inertiaFactor":
            {
                if (instance.m_inertiaFactor is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_massOrNegativeDensity":
            case "massOrNegativeDensity":
            {
                if (instance.m_massOrNegativeDensity is not TGet castValue) return false;
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
            case "m_quality":
            case "quality":
            {
                if (value is hknpShape.MassConfig.Quality castValue)
                {
                    instance.m_quality = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_quality = (hknpShape.MassConfig.Quality)intValue;
                    return true;
                }
                return false;
            }
            case "m_inertiaFactor":
            case "inertiaFactor":
            {
                if (value is not float castValue) return false;
                instance.m_inertiaFactor = castValue;
                return true;
            }
            case "m_massOrNegativeDensity":
            case "massOrNegativeDensity":
            {
                if (value is not float castValue) return false;
                instance.m_massOrNegativeDensity = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
