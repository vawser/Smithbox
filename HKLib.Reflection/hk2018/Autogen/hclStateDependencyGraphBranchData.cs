// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclStateDependencyGraphBranchData : HavokData<hclStateDependencyGraph.Branch> 
{
    public hclStateDependencyGraphBranchData(HavokType type, hclStateDependencyGraph.Branch instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_branchId":
            case "branchId":
            {
                if (instance.m_branchId is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_stateOperatorIndices":
            case "stateOperatorIndices":
            {
                if (instance.m_stateOperatorIndices is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_parentBranches":
            case "parentBranches":
            {
                if (instance.m_parentBranches is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_childBranches":
            case "childBranches":
            {
                if (instance.m_childBranches is not TGet castValue) return false;
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
            case "m_branchId":
            case "branchId":
            {
                if (value is not int castValue) return false;
                instance.m_branchId = castValue;
                return true;
            }
            case "m_stateOperatorIndices":
            case "stateOperatorIndices":
            {
                if (value is not List<int> castValue) return false;
                instance.m_stateOperatorIndices = castValue;
                return true;
            }
            case "m_parentBranches":
            case "parentBranches":
            {
                if (value is not List<int> castValue) return false;
                instance.m_parentBranches = castValue;
                return true;
            }
            case "m_childBranches":
            case "childBranches":
            {
                if (value is not List<int> castValue) return false;
                instance.m_childBranches = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
