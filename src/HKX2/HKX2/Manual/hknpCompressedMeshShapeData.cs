using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace HKX2
{
    // Represents a tree node for a mesh's BVH tree when it's expanded from its packed format
    [System.Serializable]
    public class BVHNode
    {
        // Bounding box AABB that contains all the children as well
        public Vector3 Min;
        public Vector3 Max;

        // Left and right children nodes
        public BVHNode Left;
        public BVHNode Right;

        // Terminal leaf in the node whihc means it points directly to a chunk or a triangle
        public bool IsTerminal;

        // If a terminal, this is the index of the chunk/triangle for this terminal
        public uint Index;
    }

    public partial class hknpCompressedMeshShapeData : hkReferencedObject
    {
        private BVHNode buildBVHTree(Vector3 parentBBMin, Vector3 parentBBMax, uint nodeIndex)
        {
            BVHNode node = new BVHNode();
            var cnode = m_meshTree.m_nodes[(int)nodeIndex];
            node.Min = cnode.DecompressMin(parentBBMin, parentBBMax);
            node.Max = cnode.DecompressMax(parentBBMin, parentBBMax);

            if ((cnode.m_hiData & 0x80) > 0)
            {
                node.Left = buildBVHTree(node.Min, node.Max, nodeIndex + 1);
                node.Right = buildBVHTree(node.Min, node.Max, nodeIndex + ((((uint)cnode.m_hiData & 0x7F) << 8) | (uint)cnode.m_loData) * 2);
            }
            else
            {
                node.IsTerminal = true;
                node.Index = (((uint)cnode.m_hiData & 0x7F) << 8) | (uint)cnode.m_loData;
            }
            return node;
        }

        // Extracts an easily processable BVH tree from the packed version in the mesh data
        public BVHNode getMeshBVH()
        {
            if (m_meshTree.m_nodes == null || m_meshTree.m_nodes.Count == 0)
            {
                return null;
            }

            BVHNode root = new BVHNode();
            root.Min = new Vector3(m_meshTree.m_domain.m_min.X, m_meshTree.m_domain.m_min.Y, m_meshTree.m_domain.m_min.Z);
            root.Max = new Vector3(m_meshTree.m_domain.m_max.X, m_meshTree.m_domain.m_max.Y, m_meshTree.m_domain.m_max.Z);

            var cnode = m_meshTree.m_nodes[0];
            if ((cnode.m_hiData & 0x80) > 0)
            {
                root.Left = buildBVHTree(root.Min, root.Max, 1);
                root.Right = buildBVHTree(root.Min, root.Max, ((((uint)cnode.m_hiData & 0x7F) << 8) | (uint)cnode.m_loData) * 2);
            }
            else
            {
                root.IsTerminal = true;
                root.Index = (((uint)cnode.m_hiData & 0x7F) << 8) | (uint)cnode.m_loData;
            }

            return root;
        }

        public Vector3 DecompressSharedVertex(ulong vertex, Vector4 bbMin, Vector4 bbMax)
        {
            float scaleX = (bbMax.X - bbMin.X) / (float)((1 << 21) - 1);
            float scaleY = (bbMax.Y - bbMin.Y) / (float)((1 << 21) - 1);
            float scaleZ = (bbMax.Z - bbMin.Z) / (float)((1 << 22) - 1);
            float x = ((float)(vertex & 0x1FFFFF)) * scaleX + bbMin.X;
            float y = ((float)((vertex >> 21) & 0x1FFFFF)) * scaleY + bbMin.Y;
            float z = ((float)((vertex >> 42) & 0x3FFFFF)) * scaleZ + bbMin.Z;
            return new Vector3(x, y, z);
        }

        public Vector3 DecompressPackedVertex(uint vertex, Vector3 scale, Vector3 offset)
        {
            float x = ((float)(vertex & 0x7FF)) * scale.X + offset.X;
            float y = ((float)((vertex >> 11) & 0x7FF)) * scale.Y + offset.Y;
            float z = ((float)((vertex >> 22) & 0x3FF)) * scale.Z + offset.Z;
            return new Vector3(x, y, z);
        }
    }
}
