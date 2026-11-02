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

        /// <summary>
        /// Gets a vertex of a primitive, which is either packed in its section or shared between sections.
        /// </summary>
        public Vector3 GetPrimitiveVertex(hkcdStaticMeshTreeBaseSection section, byte index)
        {
            var sharedVerticesLength = section.m_sharedVertices.m_data & 0xFF;
            var sharedVerticesIndex = section.m_sharedVertices.m_data >> 8;

            if (index < sharedVerticesLength)
            {
                var offset = new Vector3(section.m_codecParms_0, section.m_codecParms_1, section.m_codecParms_2);
                var scale = new Vector3(section.m_codecParms_3, section.m_codecParms_4, section.m_codecParms_5);
                return DecompressPackedVertex(m_meshTree.m_packedVertices[(int)(index + section.m_firstPackedVertex)], scale, offset);
            }

            var shared = m_meshTree.m_sharedVerticesIndex[(int)(index + sharedVerticesIndex - sharedVerticesLength)];
            return DecompressSharedVertex(m_meshTree.m_sharedVertices[shared], m_meshTree.m_domain.m_min, m_meshTree.m_domain.m_max);
        }

        /// <summary>
        /// Builds the shape key of a triangle. Keys are filled from the top bit down with the section, the
        /// primitive within it, and which half of a quad the triangle is, with the bits that aren't used set.
        /// </summary>
        public uint GetShapeKey(int sectionIndex, int primitiveIndex, int quadHalf)
        {
            var bits = (int)m_meshTree.m_bitsPerKey;
            var key = (uint)((sectionIndex << 8) | (primitiveIndex << 1) | quadHalf);
            var unused = 32 - bits;

            return (key << unused) | (uint)((1L << unused) - 1);
        }

        /// <summary>
        /// Gets the corners of the triangle with the given shape key, in local space. The second half of a
        /// quad uses its first, third and fourth vertices. Returns false if the key isn't a triangle of this mesh.
        /// </summary>
        public bool TryGetShapeKeyTriangle(uint shapeKey, out Vector3 a, out Vector3 b, out Vector3 c)
        {
            a = b = c = default;

            var bits = (int)m_meshTree.m_bitsPerKey;
            if (bits <= 1 || bits > 32)
                return false;

            var value = shapeKey >> (32 - bits);
            var sectionIndex = (int)(value >> 8);
            var primitiveIndex = (int)((value >> 1) & 0x7F);
            var quadHalf = (int)(value & 1);

            if (sectionIndex >= m_meshTree.m_sections.Count)
                return false;

            var section = m_meshTree.m_sections[sectionIndex];
            if (primitiveIndex >= (section.m_primitives.m_data & 0xFF))
                return false;

            var primitive = m_meshTree.m_primitives[primitiveIndex + (int)(section.m_primitives.m_data >> 8)];

            // Marks a shape that isn't a mesh primitive
            if (primitive.m_indices_0 == 0xDE && primitive.m_indices_1 == 0xAD && primitive.m_indices_2 == 0xDE &&
                primitive.m_indices_3 == 0xAD)
                return false;

            var isQuad = primitive.m_indices_2 != primitive.m_indices_3;
            if (quadHalf == 1 && !isQuad)
                return false;

            a = GetPrimitiveVertex(section, primitive.m_indices_0);
            if (quadHalf == 0)
            {
                b = GetPrimitiveVertex(section, primitive.m_indices_1);
                c = GetPrimitiveVertex(section, primitive.m_indices_2);
            }
            else
            {
                b = GetPrimitiveVertex(section, primitive.m_indices_2);
                c = GetPrimitiveVertex(section, primitive.m_indices_3);
            }

            return true;
        }
    }
}
