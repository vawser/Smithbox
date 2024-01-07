using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace HKX2
{
    public partial class hkcdStaticMeshTreeBaseSection : hkcdStaticTreeTreehkcdStaticTreeDynamicStorage4
    {
        // Recursively builds the BVH tree from the compressed packed array
        private BVHNode buildBVHTree(Vector3 parentBBMin, Vector3 parentBBMax, uint nodeIndex)
        {
            BVHNode node = new BVHNode();
            var cnode = m_nodes[(int)nodeIndex];
            node.Min = cnode.DecompressMin(parentBBMin, parentBBMax);
            node.Max = cnode.DecompressMax(parentBBMin, parentBBMax);

            if ((cnode.m_data & 0x01) > 0)
            {
                node.Left = buildBVHTree(node.Min, node.Max, nodeIndex + 1);
                node.Right = buildBVHTree(node.Min, node.Max, nodeIndex + ((uint)cnode.m_data & 0xFE));
            }
            else
            {
                node.IsTerminal = true;
                node.Index = (uint)cnode.m_data / 2;
            }
            return node;
        }

        // Extracts an easily processable BVH tree from the packed version in the mesh data
        public BVHNode getSectionBVH()
        {
            if (m_nodes == null || m_nodes.Count == 0)
            {
                return null;
            }

            BVHNode root = new BVHNode();
            root.Min = new Vector3(m_domain.m_min.X, m_domain.m_min.Y, m_domain.m_min.Z);
            root.Max = new Vector3(m_domain.m_max.X, m_domain.m_max.Y, m_domain.m_max.Z);

            var cnode = m_nodes[0];
            if ((cnode.m_data & 0x01) > 0)
            {
                root.Left = buildBVHTree(root.Min, root.Max, 1);
                root.Right = buildBVHTree(root.Min, root.Max, (uint)cnode.m_data & 0xFE);
            }
            else
            {
                root.IsTerminal = true;
                root.Index = (uint)cnode.m_data / 2;
            }

            return root;
        }
    }
}
