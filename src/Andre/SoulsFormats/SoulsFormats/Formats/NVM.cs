using System;
using System.Collections.Generic;
using System.Numerics;

namespace SoulsFormats
{
    /// <summary>
    /// A navmesh format used in DeS and DS1.
    /// </summary>
    public class NVM : SoulsFile<NVM>
    {
        /// <summary>
        /// True for DeS format, false for DS1.
        /// </summary>
        public bool BigEndian;

        /// <summary>
        /// Positions of triangle vertices.
        /// </summary>
        public List<Vector3> Vertices;

        /// <summary>
        /// Triangles in this navmesh.
        /// </summary>
        public List<Triangle> Triangles;

        /// <summary>
        /// The topmost box in the hierarchy of boxes encompassing the navmesh.
        /// </summary>
        public Box RootBox;

        /// <summary>
        /// Enables disabling of specific triangles by event entity ID.
        /// </summary>
        public List<Entity> Entities;

        /// <summary>
        /// Deserializes file data from a stream.
        /// </summary>
        protected override void Read(BinaryReaderEx br)
        {
            br.BigEndian = false;
            BigEndian = br.AssertInt32([1, 0x1000000]) != 1;
            br.BigEndian = BigEndian;

            int vertexCount = br.ReadInt32();
            // Vertex offset
            br.AssertInt32(0x80);
            int triangleCount = br.ReadInt32();
            // Triangle offset
            br.AssertInt32(0x80 + vertexCount * 0xC);
            int rootBoxOffset = br.ReadInt32();
            br.AssertInt32(0);
            int entityCount = br.ReadInt32();
            int entityOffset = br.ReadInt32();
            for (int i = 0; i < 23; i++)
                br.AssertInt32(0);

            Vertices = new List<Vector3>(vertexCount);
            for (int i = 0; i < vertexCount; i++)
                Vertices.Add(br.ReadVector3());

            Triangles = new List<Triangle>(triangleCount);
            for (int i = 0; i < triangleCount; i++)
                Triangles.Add(new Triangle(br));

            br.Position = rootBoxOffset;
            RootBox = new Box(br);

            br.Position = entityOffset;
            Entities = new List<Entity>(entityCount);
            for (int i = 0; i < entityCount; i++)
                Entities.Add(new Entity(br));
        }

        /// <summary>
        /// Serializes file data to a stream.
        /// </summary>
        protected override void Write(BinaryWriterEx bw)
        {
            bw.BigEndian = BigEndian;

            bw.WriteInt32(1);
            bw.WriteInt32(Vertices.Count);
            bw.ReserveInt32("VertexOffset");
            bw.WriteInt32(Triangles.Count);
            bw.ReserveInt32("TriangleOffset");
            bw.ReserveInt32("RootBoxOffset");
            bw.WriteInt32(0);
            bw.WriteInt32(Entities.Count);
            bw.ReserveInt32("EntityOffset");
            for (int i = 0; i < 23; i++)
                bw.WriteInt32(0);

            bw.FillInt32("VertexOffset", (int)bw.Position);
            foreach (Vector3 vertex in Vertices)
                bw.WriteVector3(vertex);

            bw.FillInt32("TriangleOffset", (int)bw.Position);
            foreach (Triangle triangle in Triangles)
                triangle.Write(bw);

            var boxTriangleIndexOffsets = new Queue<int>();
            void WriteBoxTriangleIndices(Box box)
            {
                if (box == null)
                    return;

                WriteBoxTriangleIndices(box.ChildBox1);
                WriteBoxTriangleIndices(box.ChildBox2);
                WriteBoxTriangleIndices(box.ChildBox3);
                WriteBoxTriangleIndices(box.ChildBox4);

                if (box.TriangleIndices.Count == 0)
                {
                    boxTriangleIndexOffsets.Enqueue(0);
                }
                else
                {
                    boxTriangleIndexOffsets.Enqueue((int)bw.Position);
                    bw.WriteInt32s(box.TriangleIndices);
                }
            }
            WriteBoxTriangleIndices(RootBox);

            int rootBoxOffset = RootBox.Write(bw, boxTriangleIndexOffsets);
            bw.FillInt32("RootBoxOffset", rootBoxOffset);

            var entityTriangleIndexOffsets = new List<int>();
            foreach (Entity entity in Entities)
            {
                entityTriangleIndexOffsets.Add((int)bw.Position);
                bw.WriteInt32s(entity.TriangleIndices);
            }

            bw.FillInt32("EntityOffset", (int)bw.Position);
            for (int i = 0; i < Entities.Count; i++)
                Entities[i].Write(bw, entityTriangleIndexOffsets[i]);
        }

        /// <summary>
        /// A surface with flags indicating how AI should behave on it.
        /// </summary>
        public class Triangle
        {
            /// <summary>
            /// Indices of the vertices defining this triangle.
            /// </summary>
            public int VertexIndex1, VertexIndex2, VertexIndex3;

            /// <summary>
            /// Index of the triangle adjacent to the vertex 1-2 edge, if any.
            /// </summary>
            public int EdgeIndex1;

            /// <summary>
            /// Index of the triangle adjacent to the vertex 2-3 edge, if any.
            /// </summary>
            public int EdgeIndex2;

            /// <summary>
            /// Index of the triangle adjacent to the vertex 1-3 edge, if any.
            /// </summary>
            public int EdgeIndex3;

            /// <summary>
            /// Number of breakable objects on this triangle.
            /// </summary>
            public int ObstacleCount;

            /// <summary>
            /// Controls AI behavior on this triangle.
            /// </summary>
            public TriangleFlags Flags;

            internal Triangle(BinaryReaderEx br)
            {
                VertexIndex1 = br.ReadInt32();
                VertexIndex2 = br.ReadInt32();
                VertexIndex3 = br.ReadInt32();
                EdgeIndex1 = br.ReadInt32();
                EdgeIndex2 = br.ReadInt32();
                EdgeIndex3 = br.ReadInt32();

                // Seems super janky, but it works for DS1 and seems to work for DeS
                int obstaclesAndFlags = br.ReadInt32();
                ObstacleCount = (obstaclesAndFlags >> 2) & 0x3FFF;
                Flags = (TriangleFlags)(obstaclesAndFlags >> 16);

                if ((obstaclesAndFlags & 3) != 0)
                    throw new FormatException("Lower 2 bits of obstacle count are expected to be 0, but weren't.");
            }

            internal void Write(BinaryWriterEx bw)
            {
                bw.WriteInt32(VertexIndex1);
                bw.WriteInt32(VertexIndex2);
                bw.WriteInt32(VertexIndex3);
                bw.WriteInt32(EdgeIndex1);
                bw.WriteInt32(EdgeIndex2);
                bw.WriteInt32(EdgeIndex3);

                int obstaclesAndFlags = (ObstacleCount << 2) | ((int)Flags << 16);
                bw.WriteInt32(obstaclesAndFlags);
            }

            /// <summary>
            /// Returns the 3 vertex indices, 3 edge indices, flags and obstacle count in that order.
            /// </summary>
            public override string ToString()
            {
                return $"[{VertexIndex1}, {VertexIndex2}, {VertexIndex3}] [{EdgeIndex1}, {EdgeIndex2}, {EdgeIndex3}] {Flags} {ObstacleCount}";
            }
        }

        /// <summary>
        /// Available AI properties of a triangle.
        /// </summary>
        [Flags]
        public enum TriangleFlags
        {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
            NONE = 0x00000,
            INSIDE_WALL = 0x0001,
            BLOCK_GATE = 0x0002,
            CLOSED_DOOR = 0x0004,
            DOOR = 0x0008,
            HOLE = 0x0010,
            LADDER = 0x0020,
            LARGE_SPACE = 0x0040,
            EDGE = 0x0080,
            EVENT = 0x0100,
            LANDING_POINT = 0x0200,
            FLOOR_TO_WALL = 0x0400,
            DEGENERATE = 0x0800,
            WALL = 0x1000,
            BLOCK = 0x2000,
            GATE = 0x4000,
            DISABLE = 0x8000,
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        }

        /// <summary>
        /// A rectangular prism in a tree structure encompassing the navmesh.
        /// </summary>
        public class Box
        {
            /// <summary>
            /// One of two corners defining the extent of the box.
            /// </summary>
            public Vector3 MinValueCorner, MaxValueCorner;

            /// <summary>
            /// Indices of triangles within this box. Only used for leaf nodes.
            /// </summary>
            public List<int> TriangleIndices;

            /// <summary>
            /// The four boxes that subdivide this one.
            /// Boxes are always separated horizontally so Y/Vertical values remain constant.
            /// </summary>
            //                           MaxValueCorner
            // ---------------------------------------
            // |                  |                  |
            // |                  |                  |
            // |                  |                  |
            // |    ChildBox4     |    ChildBox3     |
            // |                  |                  |
            // |                  |                  |
            // |                  |                  |
            // | ------------------------------------| 
            // |                  |                  |
            // |                  |                  |
            // |                  |                  |
            // |     ChildBox1    |    ChildBox2     |
            // |                  |                  |
            // |                  |                  |
            // |                  |                  |
            // ---------------------------------------
            // MinValue Corner
            public Box ChildBox1, ChildBox2, ChildBox3, ChildBox4;

            internal Box(BinaryReaderEx br)
            {
                MinValueCorner = br.ReadVector3();
                int triangleCount = br.ReadInt32();
                MaxValueCorner = br.ReadVector3();
                int triangleOffset = br.ReadInt32();
                int boxOffset1 = br.ReadInt32();
                int boxOffset2 = br.ReadInt32();
                int boxOffset3 = br.ReadInt32();
                int boxOffset4 = br.ReadInt32();
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);

                if (triangleCount == 0)
                    TriangleIndices = new List<int>();
                else
                    TriangleIndices = new List<int>(br.GetInt32s(triangleOffset, triangleCount));

                Box ReadBox(int boxOffset)
                {
                    if (boxOffset == 0)
                        return null;

                    br.Position = boxOffset;
                    return new Box(br);
                }

                ChildBox1 = ReadBox(boxOffset1);
                ChildBox2 = ReadBox(boxOffset2);
                ChildBox3 = ReadBox(boxOffset3);
                ChildBox4 = ReadBox(boxOffset4);
            }

            internal int Write(BinaryWriterEx bw, Queue<int> triangleIndexOffsets)
            {
                int boxOffset1 = ChildBox1?.Write(bw, triangleIndexOffsets) ?? 0;
                int boxOffset2 = ChildBox2?.Write(bw, triangleIndexOffsets) ?? 0;
                int boxOffset3 = ChildBox3?.Write(bw, triangleIndexOffsets) ?? 0;
                int boxOffset4 = ChildBox4?.Write(bw, triangleIndexOffsets) ?? 0;

                int thisOffset = (int)bw.Position;
                bw.WriteVector3(MinValueCorner);
                bw.WriteInt32(TriangleIndices.Count);
                bw.WriteVector3(MaxValueCorner);
                bw.WriteInt32(triangleIndexOffsets.Dequeue());
                bw.WriteInt32(boxOffset1);
                bw.WriteInt32(boxOffset2);
                bw.WriteInt32(boxOffset3);
                bw.WriteInt32(boxOffset4);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                return thisOffset;
            }
        }

        /// <summary>
        /// A list of triangles that can be disabled via event entity ID.
        /// </summary>
        public class Entity
        {
            /// <summary>
            /// The event entity ID used to access these triangles.
            /// </summary>
            public int EntityID;

            /// <summary>
            /// The triangles to be disabled from an event script.
            /// </summary>
            public List<int> TriangleIndices;

            internal Entity(BinaryReaderEx br)
            {
                EntityID = br.ReadInt32();
                int indexOffset = br.ReadInt32();
                int indexCount = br.ReadInt32();
                br.AssertInt32(0);

                TriangleIndices = new List<int>(br.GetInt32s(indexOffset, indexCount));
            }

            internal void Write(BinaryWriterEx bw, int indexOffset)
            {
                bw.WriteInt32(EntityID);
                bw.WriteInt32(indexOffset);
                bw.WriteInt32(TriangleIndices.Count);
                bw.WriteInt32(0);
            }
        }
    }
}
