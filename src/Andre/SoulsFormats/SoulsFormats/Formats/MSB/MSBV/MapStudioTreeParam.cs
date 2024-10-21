using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace SoulsFormats
{
    public partial class MSBV
    {
        /// <summary>
        /// A hierarchy used in calculations such as drawing and collision.
        /// </summary>
        public class MapStudioTreeParam : IMsbTreeParam<MapStudioTree>
        {
            /// <summary>
            /// Unknown; probably some kind of version number.
            /// </summary>
            public int Version { get; set; }

            /// <summary>
            /// The Name or Type of the Param.
            /// </summary>
            private protected string Name = "MAPSTUDIO_TREE_ST";

            /// <summary>
            /// The axis-aligned tree bounding volume hierarchy.<br/>
            /// Set to null when not calculated yet.
            /// </summary>
            public MapStudioTree Tree { get; set; }

            /// <summary>
            /// Create a new <see cref="MapStudioTreeParam"/>.
            /// </summary>
            public MapStudioTreeParam()
            {
                // Set node null as we need to calculate that later
                Tree = null;
                Version = 10001002;
            }

            internal MapStudioTree Read(BinaryReaderEx br)
            {
                Version = br.ReadInt32();
                int nameOffset = br.ReadInt32();
                int offsetCount = br.ReadInt32();
                int rootNodeOffset;
                if (offsetCount - 1 > 0)
                {
                    rootNodeOffset = br.ReadInt32();
                    br.Skip(Math.Max(0, (offsetCount - 2) * 4)); // Entry Offsets
                }
                else
                {
                    rootNodeOffset = -1;
                }

                int nextParamOffset = br.ReadInt32();
                string name = br.GetASCII(nameOffset);
                if (name != Name)
                    throw new InvalidDataException($"Expected param \"{Name}\", got param \"{name}\"");

                if (offsetCount - 1 != 0)
                {
                    br.Position = rootNodeOffset;
                    Tree = new MapStudioTree(br);
                }
                else
                {
                    Tree = null;
                }

                if (nextParamOffset > 0)
                {
                    br.Position = nextParamOffset;
                }
                return Tree;
            }

            internal void Write(BinaryWriterEx bw)
            {
                bw.WriteInt32(Version);
                bw.ReserveInt32("ParamNameOffset");
                int count = Tree.GetNodeCount();
                bw.WriteInt32(count + 1);
                for (int i = 0; i < count; i++)
                {
                    bw.ReserveInt32($"OffsetTreeNode_{i}");
                }
                bw.ReserveInt32("NextParamOffset");

                bw.FillInt32("ParamNameOffset", (int)bw.Position);
                bw.WriteASCII(Name, true);
                bw.Pad(4);

                int index = 0;
                Tree.Write(bw, ref index);
            }
        }

        /// <summary>
        /// A tree hierarchy of axis-aligned bounding boxes used in various calculations such as drawing, culling, and collision detection.
        /// </summary>
        public class MapStudioTree : IMsbTree
        {
            /// <summary>
            /// The bounding box for this node.
            /// </summary>
            public MsbBoundingBox Bounds { get; set; }

            /// <summary>
            /// The left child of this node.
            /// </summary>
            public MapStudioTree Left { get; set; }
            IMsbTree IMsbTree.Left { get => Left; set => Left = (MapStudioTree)value; }

            /// <summary>
            /// The right child of this node.
            /// </summary>
            public MapStudioTree Right { get; set; }
            IMsbTree IMsbTree.Right { get => Right; set => Right = (MapStudioTree)value; }

            /// <summary>
            /// Indices to the parts this node contains.
            /// </summary>
            public List<short> PartIndices { get; set; }

            /// <summary>
            /// Create a new <see cref="MapStudioTree"/>.
            /// </summary>
            public MapStudioTree()
            {
                PartIndices = new List<short>();
                Bounds = new MsbBoundingBox();
                Left = null;
                Right = null;
            }

            /// <summary>
            /// Create a new <see cref="MapStudioTree"/> with the given bounding information.
            /// </summary>
            /// <param name="min">The minimum extent of the bounding box.</param>
            /// <param name="max">The maximum extent of the bounding box.</param>
            public MapStudioTree(Vector3 min, Vector3 max)
            {
                PartIndices = new List<short>();
                Bounds = new MsbBoundingBox(min, max);
                Left = null;
                Right = null;
            }

            internal MapStudioTree(BinaryReaderEx br)
            {
                long start = br.Position;
                Vector3 minimum = br.ReadVector3();
                int leftChildOffset = br.ReadInt32();
                Vector3 maximum = br.ReadVector3();
                br.AssertInt32(0); // Unknown
                Vector3 origin = br.ReadVector3();
                int rightChildOffset = br.ReadInt32();
                float radius = br.ReadSingle();
                Bounds = new MsbBoundingBox(minimum, maximum, origin, radius);

                int partIndexCount = br.ReadInt32();

                PartIndices = new List<short>();
                for (int i = 0; i < partIndexCount; i++)
                {
                    PartIndices.Add(br.ReadInt16());
                }

                if (leftChildOffset > 0)
                {
                    br.Position = start + leftChildOffset;
                    Left = new MapStudioTree(br);
                }

                if (rightChildOffset > 0)
                {
                    br.Position = start + rightChildOffset;
                    Right = new MapStudioTree(br);
                }
            }

            internal void Write(BinaryWriterEx bw, ref int index)
            {
                long start = bw.Position;
                bw.FillInt32($"OffsetTreeNode_{index}", (int)start);
                string fillStr1 = $"OffsetTreeNodeLeftChild_{index}";
                string fillStr2 = $"OffsetTreeNodeRightChild_{index}";

                bw.WriteVector3(Bounds.Min);
                bw.ReserveInt32(fillStr1);
                bw.WriteVector3(Bounds.Max);
                bw.WriteInt32(0); // Unknown
                bw.WriteVector3(Bounds.Origin);
                bw.ReserveInt32(fillStr2);
                bw.WriteSingle(Bounds.Radius);
                bw.WriteInt32(PartIndices.Count);
                for (int i = 0; i < PartIndices.Count; i++)
                {
                    bw.WriteInt16(PartIndices[i]);
                }
                bw.Pad(0x10);
                index += 1;

                if (Left != null)
                {
                    bw.FillInt32(fillStr1, (int)(bw.Position - start));
                    Left.Write(bw, ref index);
                }
                else
                {
                    bw.FillInt32(fillStr1, 0);
                }

                if (Right != null)
                {
                    bw.FillInt32(fillStr2, (int)(bw.Position - start));
                    Right.Write(bw, ref index);
                }
                else
                {
                    bw.FillInt32(fillStr2, 0);
                }
            }

            /// <summary>
            /// Get the total node count starting from this node.
            /// </summary>
            /// <returns>The total node count.</returns>
            public int GetNodeCount()
            {
                int count = 1;
                if (Left != null)
                {
                    count += Left.GetNodeCount();
                }

                if (Right != null)
                {
                    count += Right.GetNodeCount();
                }

                return count;
            }
        }
    }
}
