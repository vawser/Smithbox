// Automatically Generated

namespace HKLib.hk2018;

public class hkcdSimdTree : IHavokObject
{
    public List<hkcdSimdTree.Node> m_nodes = new();

    public bool m_isCompact;


    public class Dynamic : IHavokObject
    {
        public List<hkcdSimdTree.Dynamic.NodeData> m_nodeData = new();

        public List<hkTuple2<uint, uint>> m_leaves = new();

        public int m_firstFreeNode;


        public class NodeData : IHavokObject
        {
            public uint m_parent;

            public uint m_dirty;

        }


    }


    public class Node : hkcdFourAabb
    {
        public readonly uint[] m_data = new uint[4];

        public bool m_isLeaf;

    }


}

