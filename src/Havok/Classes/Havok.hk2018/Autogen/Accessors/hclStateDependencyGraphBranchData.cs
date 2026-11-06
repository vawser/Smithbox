// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hclStateDependencyGraphBranchData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hclStateDependencyGraph.Branch>
{
    public hclStateDependencyGraphBranchData(Havok.hk2018.hclStateDependencyGraph.Branch instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_branchId":
            case "branchId":
            {
                value = instance.m_branchId;
                return true;
            }
            case "m_stateOperatorIndices":
            case "stateOperatorIndices":
            {
                value = instance.m_stateOperatorIndices;
                return true;
            }
            case "m_parentBranches":
            case "parentBranches":
            {
                value = instance.m_parentBranches;
                return true;
            }
            case "m_childBranches":
            case "childBranches":
            {
                value = instance.m_childBranches;
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
