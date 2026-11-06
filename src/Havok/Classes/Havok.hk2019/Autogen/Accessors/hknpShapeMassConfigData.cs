// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hknpShapeMassConfigData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hknpShape.MassConfig>
{
    public hknpShapeMassConfigData(Havok.hk2019.hknpShape.MassConfig instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_quality":
            case "quality":
            {
                value = instance.m_quality;
                return true;
            }
            case "m_inertiaFactor":
            case "inertiaFactor":
            {
                value = instance.m_inertiaFactor;
                return true;
            }
            case "m_massOrNegativeDensity":
            case "massOrNegativeDensity":
            {
                value = instance.m_massOrNegativeDensity;
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
            case "m_quality":
            case "quality":
            {
                if (value is Havok.hk2019.hknpShape.MassConfig.Quality castValue)
                {
                    instance.m_quality = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_quality = (Havok.hk2019.hknpShape.MassConfig.Quality)intValue;
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
