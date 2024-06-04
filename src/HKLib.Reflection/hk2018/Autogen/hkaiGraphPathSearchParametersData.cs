// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiGraphPathSearchParametersData : HavokData<hkaiGraphPathSearchParameters> 
{
    public hkaiGraphPathSearchParametersData(HavokType type, hkaiGraphPathSearchParameters instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_heuristicWeight":
            case "heuristicWeight":
            {
                if (instance.m_heuristicWeight is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_useHierarchicalHeuristic":
            case "useHierarchicalHeuristic":
            {
                if (instance.m_useHierarchicalHeuristic is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_bufferSizes":
            case "bufferSizes":
            {
                if (instance.m_bufferSizes is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_hierarchyBufferSizes":
            case "hierarchyBufferSizes":
            {
                if (instance.m_hierarchyBufferSizes is not TGet castValue) return false;
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
            case "m_heuristicWeight":
            case "heuristicWeight":
            {
                if (value is not float castValue) return false;
                instance.m_heuristicWeight = castValue;
                return true;
            }
            case "m_useHierarchicalHeuristic":
            case "useHierarchicalHeuristic":
            {
                if (value is not bool castValue) return false;
                instance.m_useHierarchicalHeuristic = castValue;
                return true;
            }
            case "m_bufferSizes":
            case "bufferSizes":
            {
                if (value is not hkaiSearchParameters.BufferSizes castValue) return false;
                instance.m_bufferSizes = castValue;
                return true;
            }
            case "m_hierarchyBufferSizes":
            case "hierarchyBufferSizes":
            {
                if (value is not hkaiSearchParameters.BufferSizes castValue) return false;
                instance.m_hierarchyBufferSizes = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
