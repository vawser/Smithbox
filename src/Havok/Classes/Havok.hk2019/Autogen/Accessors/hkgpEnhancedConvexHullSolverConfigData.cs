// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkgpEnhancedConvexHullSolverConfigData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkgpEnhancedConvexHull.SolverConfig>
{
    public hkgpEnhancedConvexHullSolverConfigData(Havok.hk2019.hkgpEnhancedConvexHull.SolverConfig instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_axisSampling":
            case "axisSampling":
            {
                value = instance.m_axisSampling;
                return true;
            }
            case "m_eventsSampling":
            case "eventsSampling":
            {
                value = instance.m_eventsSampling;
                return true;
            }
            case "m_bias":
            case "bias":
            {
                value = instance.m_bias;
                return true;
            }
            case "m_refineSearch":
            case "refineSearch":
            {
                value = instance.m_refineSearch;
                return true;
            }
            case "m_globalSearch":
            case "globalSearch":
            {
                value = instance.m_globalSearch;
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
            case "m_axisSampling":
            case "axisSampling":
            {
                if (value is not int castValue) return false;
                instance.m_axisSampling = castValue;
                return true;
            }
            case "m_eventsSampling":
            case "eventsSampling":
            {
                if (value is not int castValue) return false;
                instance.m_eventsSampling = castValue;
                return true;
            }
            case "m_bias":
            case "bias":
            {
                if (value is not float castValue) return false;
                instance.m_bias = castValue;
                return true;
            }
            case "m_refineSearch":
            case "refineSearch":
            {
                if (value is not bool castValue) return false;
                instance.m_refineSearch = castValue;
                return true;
            }
            case "m_globalSearch":
            case "globalSearch":
            {
                if (value is not bool castValue) return false;
                instance.m_globalSearch = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
