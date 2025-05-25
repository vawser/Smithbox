// Automatically Generated

namespace HKLib.hk2018;

public class hclStateDependencyGraph : hkReferencedObject
{
    public List<hclStateDependencyGraph.Branch> m_branches = new();

    public List<int> m_rootBranchIds = new();

    public List<List<int>> m_children = new();

    public List<List<int>> m_parents = new();

    public bool m_multiThreadable;


    public class Branch : IHavokObject
    {
        public int m_branchId;

        public List<int> m_stateOperatorIndices = new();

        public List<int> m_parentBranches = new();

        public List<int> m_childBranches = new();

    }


}

