// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclStateDependencyGraphData : HavokData<hclStateDependencyGraph> 
{
    public hclStateDependencyGraphData(HavokType type, hclStateDependencyGraph instance) : base(type, instance) {}

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
            case "m_branches":
            case "branches":
            {
                if (instance.m_branches is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_rootBranchIds":
            case "rootBranchIds":
            {
                if (instance.m_rootBranchIds is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_children":
            case "children":
            {
                if (instance.m_children is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_parents":
            case "parents":
            {
                if (instance.m_parents is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_multiThreadable":
            case "multiThreadable":
            {
                if (instance.m_multiThreadable is not TGet castValue) return false;
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
            case "m_branches":
            case "branches":
            {
                if (value is not List<hclStateDependencyGraph.Branch> castValue) return false;
                instance.m_branches = castValue;
                return true;
            }
            case "m_rootBranchIds":
            case "rootBranchIds":
            {
                if (value is not List<int> castValue) return false;
                instance.m_rootBranchIds = castValue;
                return true;
            }
            case "m_children":
            case "children":
            {
                if (value is not List<List<int>> castValue) return false;
                instance.m_children = castValue;
                return true;
            }
            case "m_parents":
            case "parents":
            {
                if (value is not List<List<int>> castValue) return false;
                instance.m_parents = castValue;
                return true;
            }
            case "m_multiThreadable":
            case "multiThreadable":
            {
                if (value is not bool castValue) return false;
                instance.m_multiThreadable = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
