// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hknpShapeMassConfigData : HavokData<HKLib.hk2019.hknpShape.MassConfig> 
{
    public hknpShapeMassConfigData(HavokType type, HKLib.hk2019.hknpShape.MassConfig instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
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
            case "m_quality":
            case "quality":
            {
                if (value is HKLib.hk2019.hknpShape.MassConfig.Quality castValue)
                {
                    instance.m_quality = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_quality = (HKLib.hk2019.hknpShape.MassConfig.Quality)intValue;
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
