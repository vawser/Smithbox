// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiGraphPathSearchParametersData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiGraphPathSearchParameters>
{
    public hkaiGraphPathSearchParametersData(Havok.hk2019.hkaiGraphPathSearchParameters instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_heuristicWeight":
            case "heuristicWeight":
            {
                value = instance.m_heuristicWeight;
                return true;
            }
            case "m_useHierarchicalHeuristic":
            case "useHierarchicalHeuristic":
            {
                value = instance.m_useHierarchicalHeuristic;
                return true;
            }
            case "m_bufferSizes":
            case "bufferSizes":
            {
                value = instance.m_bufferSizes;
                return true;
            }
            case "m_hierarchyBufferSizes":
            case "hierarchyBufferSizes":
            {
                value = instance.m_hierarchyBufferSizes;
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
                if (value is not Havok.hk2019.hkaiSearchParameters.BufferSizes castValue) return false;
                instance.m_bufferSizes = castValue;
                return true;
            }
            case "m_hierarchyBufferSizes":
            case "hierarchyBufferSizes":
            {
                if (value is not Havok.hk2019.hkaiSearchParameters.BufferSizes castValue) return false;
                instance.m_hierarchyBufferSizes = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
