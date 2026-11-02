// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkgpEnhancedConvexHullSolverConfigData : HavokData<HKLib.hk2019.hkgpEnhancedConvexHull.SolverConfig> 
{
    public hkgpEnhancedConvexHullSolverConfigData(HavokType type, HKLib.hk2019.hkgpEnhancedConvexHull.SolverConfig instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_axisSampling":
            case "axisSampling":
            {
                if (instance.m_axisSampling is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_eventsSampling":
            case "eventsSampling":
            {
                if (instance.m_eventsSampling is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_bias":
            case "bias":
            {
                if (instance.m_bias is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_refineSearch":
            case "refineSearch":
            {
                if (instance.m_refineSearch is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_globalSearch":
            case "globalSearch":
            {
                if (instance.m_globalSearch is not TGet castValue) return false;
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
