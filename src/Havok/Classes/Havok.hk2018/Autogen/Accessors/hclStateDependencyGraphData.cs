// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hclStateDependencyGraphData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hclStateDependencyGraph>
{
    public hclStateDependencyGraphData(Havok.hk2018.hclStateDependencyGraph instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                value = instance.m_propertyBag;
                return true;
            }
            case "m_branches":
            case "branches":
            {
                value = instance.m_branches;
                return true;
            }
            case "m_rootBranchIds":
            case "rootBranchIds":
            {
                value = instance.m_rootBranchIds;
                return true;
            }
            case "m_children":
            case "children":
            {
                value = instance.m_children;
                return true;
            }
            case "m_parents":
            case "parents":
            {
                value = instance.m_parents;
                return true;
            }
            case "m_multiThreadable":
            case "multiThreadable":
            {
                value = instance.m_multiThreadable;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not Havok.hk2018.hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_branches":
            case "branches":
            {
                if (value is not List<Havok.hk2018.hclStateDependencyGraph.Branch> castValue) return false;
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
