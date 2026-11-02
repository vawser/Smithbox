using System;
using System.Collections.Generic;
using System.Numerics;

namespace SoulsFormats
{
    /// <summary>
    /// A Sekiro file that marks which parts of a collision are walls. It holds no geometry of its own:
    /// each triangle is a triangle of the low collision with the same ID, found by its shape key, and is
    /// split into smaller cells so that only part of a triangle can be marked.
    /// Extension: .wall
    /// </summary>
    public class WALL : SoulsFile<WALL>
    {
        /// <summary>
        /// The number of the collision this file belongs to, e.g. 100 for h000100.
        /// </summary>
        public long ID { get; set; }

        /// <summary>
        /// Points within a triangle that cells are made from, as weights of its second and third corners.
        /// Cells refer to the corners of the triangle as 0, 1 and 2, and to these as 3 onwards.
        /// </summary>
        public List<Vector2> Points { get; set; }

        /// <summary>
        /// The collision triangles that are marked, in ascending order of shape key.
        /// </summary>
        public List<Triangle> Triangles { get; set; }

        /// <summary>
        /// Whether the list of triangles is written when there are none. Some files without triangles have it,
        /// others don't. It is always written when there are triangles.
        /// </summary>
        public bool HasTree { get; set; }

        /// <summary>
        /// Creates an empty WALL.
        /// </summary>
        public WALL()
        {
            Points = new List<Vector2>();
            Triangles = new List<Triangle>();
            HasTree = true;
        }

        /// <summary>
        /// Checks whether the data appears to be a file of this format.
        /// </summary>
        protected override bool Is(BinaryReaderEx br)
        {
            return br.Length >= 0x48 && br.GetInt32(0) == 3 && br.GetInt64(8) == 0x48;
        }

        /// <summary>
        /// Deserializes file data from a stream.
        /// </summary>
        protected override void Read(BinaryReaderEx br)
        {
            br.BigEndian = false;
            br.AssertInt32(3);
            HasTree = br.AssertInt32([0, 1]) == 1;
            long treeOffset = br.AssertInt64(0x48);
            ID = br.ReadInt64();
            long pointsOffset = br.ReadInt64();
            long trianglesOffset = br.ReadInt64();
            br.ReadInt64(); // Cells offset
            int pointCount = br.ReadInt32();
            int triangleCount = br.ReadInt32();
            br.ReadInt32(); // Cell count
            br.ReadInt32(); // Unsplit cell count
            br.AssertInt32(1);
            br.AssertInt32(0);

            if (HasTree)
            {
                br.Position = treeOffset;
                br.AssertInt64(0);
                br.AssertInt64(trianglesOffset);
                br.AssertInt64(triangleCount);
            }

            br.Position = pointsOffset;
            Points = new List<Vector2>(pointCount);
            for (int i = 0; i < pointCount; i++)
                Points.Add(br.ReadVector2());

            br.Position = trianglesOffset;
            Triangles = new List<Triangle>(triangleCount);
            for (int i = 0; i < triangleCount; i++)
                Triangles.Add(new Triangle(br, treeOffset));
        }

        /// <summary>
        /// Serializes file data to a stream.
        /// </summary>
        protected override void Write(BinaryWriterEx bw)
        {
            bw.BigEndian = false;

            // Cells are stored depth first, with the children of a cell next to each other
            var order = new List<Cell>();
            foreach (Triangle triangle in Triangles)
                AddChildren(triangle, order);

            int unsplitCount = 0;
            foreach (Triangle triangle in Triangles)
                unsplitCount += triangle.CountUnsplit();

            bool hasTree = HasTree || Triangles.Count > 0;

            bw.WriteInt32(3);
            bw.WriteInt32(hasTree ? 1 : 0);
            bw.WriteInt64(0x48);
            bw.WriteInt64(ID);
            bw.ReserveInt64("PointsOffset");
            bw.ReserveInt64("TrianglesOffset");
            bw.ReserveInt64("CellsOffset");
            bw.WriteInt32(Points.Count);
            bw.WriteInt32(Triangles.Count);
            bw.WriteInt32(order.Count);
            bw.WriteInt32(unsplitCount);
            bw.WriteInt32(1);
            bw.WriteInt32(0);

            if (hasTree)
            {
                bw.WriteInt64(0);
                bw.WriteInt64(0x60);
                bw.WriteInt64(Triangles.Count);
            }

            long trianglesOffset = bw.Position;
            long cellsOffset = trianglesOffset + Triangles.Count * Triangle.Size;

            // Offsets of each cell's children are known up front, as every cell has a fixed size
            var childrenOffsets = new Dictionary<Cell, long>();
            long nextOffset = cellsOffset;
            foreach (Triangle triangle in Triangles)
                AssignChildrenOffsets(triangle, childrenOffsets, ref nextOffset);

            bw.FillInt64("TrianglesOffset", trianglesOffset);
            foreach (Triangle triangle in Triangles)
                triangle.Write(bw, childrenOffsets);

            bw.FillInt64("CellsOffset", cellsOffset);
            foreach (Cell cell in order)
                cell.WriteCell(bw, childrenOffsets);

            bw.FillInt64("PointsOffset", bw.Position);
            foreach (Vector2 point in Points)
                bw.WriteVector2(point);
        }

        private static void AddChildren(Cell cell, List<Cell> order)
        {
            if (cell.Children.Count == 0)
                return;

            order.AddRange(cell.Children);
            foreach (Cell child in cell.Children)
                AddChildren(child, order);
        }

        private static void AssignChildrenOffsets(Cell cell, Dictionary<Cell, long> offsets, ref long nextOffset)
        {
            if (cell.Children.Count == 0)
                return;

            offsets[cell] = nextOffset;
            nextOffset += cell.Children.Count * Cell.Size;

            foreach (Cell child in cell.Children)
                AssignChildrenOffsets(child, offsets, ref nextOffset);
        }

        /// <summary>
        /// What a cell is.
        /// </summary>
        public enum CellType : byte
        {
            /// <summary>
            /// Not a wall.
            /// </summary>
            None = 0,

            /// <summary>
            /// A wall.
            /// </summary>
            Wall = 1,

            /// <summary>
            /// Split into the cells in its children.
            /// </summary>
            Split = 2,
        }

        /// <summary>
        /// A part of a triangle, made from three of the corners and points of the file.
        /// </summary>
        public class Cell
        {
            internal const int Size = 0x18;

            /// <summary>
            /// The first corner of the cell, as an index into the corners of the triangle and then the points of the file.
            /// </summary>
            public ushort Index1 { get; set; }

            /// <summary>
            /// The second corner of the cell.
            /// </summary>
            public ushort Index2 { get; set; }

            /// <summary>
            /// The third corner of the cell.
            /// </summary>
            public ushort Index3 { get; set; }

            /// <summary>
            /// What the cell is. A split cell has children, any other has none.
            /// </summary>
            public CellType Type { get; set; }

            /// <summary>
            /// The number of the collision a wall belongs to. Usually the ID of the file, but walls along
            /// the edge of a collision can belong to the one next to it. Usually 0 on cells that aren't walls.
            /// </summary>
            public long CollisionID { get; set; }

            /// <summary>
            /// The cells this one is split into.
            /// </summary>
            public List<Cell> Children { get; set; }

            /// <summary>
            /// Creates a Cell with default values.
            /// </summary>
            public Cell()
            {
                Index2 = 1;
                Index3 = 2;
                Children = new List<Cell>();
            }

            internal Cell(BinaryReaderEx br)
            {
                ReadCell(br);
            }

            private protected void ReadCell(BinaryReaderEx br)
            {
                long childrenOffset = br.ReadInt64();
                CollisionID = br.ReadInt64();
                Index1 = br.ReadUInt16();
                Index2 = br.ReadUInt16();
                Index3 = br.ReadUInt16();
                byte childCount = br.ReadByte();
                Type = br.ReadEnum8<CellType>();

                Children = new List<Cell>(childCount);
                if (childrenOffset != 0)
                {
                    br.StepIn(childrenOffset);
                    for (int i = 0; i < childCount; i++)
                        Children.Add(new Cell(br));
                    br.StepOut();
                }
            }

            internal void WriteCell(BinaryWriterEx bw, Dictionary<Cell, long> childrenOffsets)
            {
                bw.WriteInt64(Children.Count > 0 ? childrenOffsets[this] : 0);
                bw.WriteInt64(CollisionID);
                bw.WriteUInt16(Index1);
                bw.WriteUInt16(Index2);
                bw.WriteUInt16(Index3);
                bw.WriteByte(checked((byte)Children.Count));
                bw.WriteByte((byte)Type);
            }

            internal int CountUnsplit()
            {
                if (Children.Count == 0)
                    return Type == CellType.Split ? 0 : 1;

                int count = 0;
                foreach (Cell child in Children)
                    count += child.CountUnsplit();
                return count;
            }

            /// <summary>
            /// Creates a deep copy of the cell.
            /// </summary>
            public Cell Clone()
            {
                var clone = (Cell)MemberwiseClone();
                clone.Children = Children.ConvertAll(c => c.Clone());
                return clone;
            }

            /// <summary>
            /// Returns the type and corners of the cell.
            /// </summary>
            public override string ToString()
            {
                return $"{Type} [{Index1}, {Index2}, {Index3}] {CollisionID}";
            }
        }

        /// <summary>
        /// A triangle of the low collision, and the cell covering all of it.
        /// </summary>
        public class Triangle : Cell
        {
            internal new const int Size = 0x28;

            /// <summary>
            /// The shape key of the triangle in the low collision: its section, primitive, and which half
            /// of a quad it is, from the top bit down, with the bits that aren't used set.
            /// </summary>
            public uint ShapeKey { get; set; }

            /// <summary>
            /// Creates a Triangle with default values.
            /// </summary>
            public Triangle()
            {
                ShapeKey = uint.MaxValue;
            }

            internal Triangle(BinaryReaderEx br, long treeOffset)
            {
                ReadCell(br);
                br.AssertInt64(treeOffset);
                ShapeKey = br.ReadUInt32();
                br.AssertInt32(0);
            }

            internal void Write(BinaryWriterEx bw, Dictionary<Cell, long> childrenOffsets)
            {
                WriteCell(bw, childrenOffsets);
                bw.WriteInt64(0x48);
                bw.WriteUInt32(ShapeKey);
                bw.WriteInt32(0);
            }

            /// <summary>
            /// Creates a deep copy of the triangle.
            /// </summary>
            public new Triangle Clone()
            {
                return (Triangle)base.Clone();
            }

            /// <summary>
            /// Returns the shape key and type of the triangle.
            /// </summary>
            public override string ToString()
            {
                return $"0x{ShapeKey:X8} {base.ToString()}";
            }
        }
    }
}
