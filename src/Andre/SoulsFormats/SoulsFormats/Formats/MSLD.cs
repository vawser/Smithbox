using System;
using System.Collections.Generic;
using System.Numerics;

namespace SoulsFormats
{
    /// <summary>
    /// An Armored Core VI file that holds the limits of a map's mission area. Each area is outlined by vertical
    /// walls and was built from a collision part of the map's MSB, usually one named h98xxxx; maps without
    /// limits have a file with no areas. The walls are in the space of the map.
    /// Extension: .msld
    /// </summary>
    public class MSLD : SoulsFile<MSLD>
    {
        private const int SectionVersion = 1;

        /// <summary>
        /// Unknown; always 1.
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// Unknown; differs for each map and is 0 in the files with no areas. Not a CRC32 or FNV hash of the
        /// file or its map's name, so it is kept as read.
        /// </summary>
        public uint Hash { get; set; }

        /// <summary>
        /// The areas of the map.
        /// </summary>
        public List<Area> Areas { get; set; }

        /// <summary>
        /// Creates an MSLD with no areas.
        /// </summary>
        public MSLD()
        {
            Version = 1;
            Areas = new List<Area>();
        }

        /// <summary>
        /// Checks whether the data appears to be a file of this format.
        /// </summary>
        protected override bool Is(BinaryReaderEx br)
        {
            return br.Length >= 0x10 && br.GetASCII(0, 4) == "MSLD";
        }

        /// <summary>
        /// Deserializes file data from a stream.
        /// </summary>
        protected override void Read(BinaryReaderEx br)
        {
            br.BigEndian = false;
            br.AssertASCII("MSLD");
            Version = br.ReadInt32();
            br.ReadInt32(); // File size
            br.AssertInt32(3);

            ReadSectionHeader(br, 0, out _, out _);
            Hash = br.ReadUInt32();
            br.AssertInt32(0);
            br.AssertInt32(0);
            br.AssertInt32(0);

            ReadSectionHeader(br, 1, out _, out int areaCount);
            Areas = new List<Area>(areaCount);
            var edgeCounts = new int[areaCount];
            for (int i = 0; i < areaCount; i++)
                Areas.Add(new Area(br, out edgeCounts[i]));

            // The edges of all areas are in one list, in the order of their areas
            ReadSectionHeader(br, 2, out _, out int edgeCount);
            int total = 0;
            for (int i = 0; i < areaCount; i++)
            {
                total += edgeCounts[i];
                for (int j = 0; j < edgeCounts[i]; j++)
                    Areas[i].Edges.Add(new Edge(br));
            }

            if (total != edgeCount)
                throw new System.IO.InvalidDataException($"The areas have {total} edges, but the edge section has {edgeCount}.");
        }

        private static void ReadSectionHeader(BinaryReaderEx br, int type, out int size, out int count)
        {
            br.AssertInt32(type);
            br.AssertInt32(SectionVersion);
            size = br.ReadInt32();
            count = br.ReadInt32();
        }

        /// <summary>
        /// Serializes file data to a stream.
        /// </summary>
        protected override void Write(BinaryWriterEx bw)
        {
            int edgeCount = 0;
            foreach (Area area in Areas)
                edgeCount += area.Edges.Count;

            int size = 0x10 + (0x10 + 0x10) + (0x10 + Areas.Count * Area.Size) + (0x10 + edgeCount * Edge.Size);

            bw.BigEndian = false;
            bw.WriteASCII("MSLD");
            bw.WriteInt32(Version);
            bw.WriteInt32(size);
            bw.WriteInt32(3);

            WriteSectionHeader(bw, 0, 0x10, 1);
            bw.WriteUInt32(Hash);
            bw.WriteInt32(0);
            bw.WriteInt32(0);
            bw.WriteInt32(0);

            WriteSectionHeader(bw, 1, Areas.Count * Area.Size, Areas.Count);
            foreach (Area area in Areas)
                area.Write(bw);

            WriteSectionHeader(bw, 2, edgeCount * Edge.Size, edgeCount);
            foreach (Area area in Areas)
            {
                foreach (Edge edge in area.Edges)
                    edge.Write(bw);
            }
        }

        private static void WriteSectionHeader(BinaryWriterEx bw, int type, int size, int count)
        {
            bw.WriteInt32(type);
            bw.WriteInt32(SectionVersion);
            bw.WriteInt32(size);
            bw.WriteInt32(count);
        }

        /// <summary>
        /// An area of the mission, outlined by its edges.
        /// </summary>
        public class Area
        {
            internal const int Size = 0x40;

            /// <summary>
            /// The entity ID of the collision part the area was built from. 0 for some areas that have no edges.
            /// </summary>
            public uint EntityID { get; set; }

            /// <summary>
            /// The number in the name of the collision part the area was built from, such as 980010 for h980010.
            /// </summary>
            public int CollisionNumber { get; set; }

            /// <summary>
            /// Unknown; always -1.
            /// </summary>
            public int Unk0C { get; set; }

            /// <summary>
            /// The lowest corner of the box around the edges. Areas with no edges have a flat box.
            /// </summary>
            public Vector3 BoundsMin { get; set; }

            /// <summary>
            /// The highest corner of the box around the edges, including their height.
            /// </summary>
            public Vector3 BoundsMax { get; set; }

            /// <summary>
            /// The position of the collision part the area was built from. The edges already include it.
            /// </summary>
            public Vector3 CollisionPosition { get; set; }

            /// <summary>
            /// The walls that outline the area. They are not stored in order around it.
            /// </summary>
            public List<Edge> Edges { get; }

            /// <summary>
            /// Creates an area with no edges.
            /// </summary>
            public Area()
            {
                Unk0C = -1;
                Edges = new List<Edge>();
            }

            /// <summary>
            /// Creates a copy of an area and its edges.
            /// </summary>
            public Area(Area area)
            {
                EntityID = area.EntityID;
                CollisionNumber = area.CollisionNumber;
                Unk0C = area.Unk0C;
                BoundsMin = area.BoundsMin;
                BoundsMax = area.BoundsMax;
                CollisionPosition = area.CollisionPosition;
                Edges = new List<Edge>(area.Edges.Count);
                foreach (Edge edge in area.Edges)
                    Edges.Add(new Edge(edge));
            }

            internal Area(BinaryReaderEx br, out int edgeCount)
            {
                EntityID = br.ReadUInt32();
                edgeCount = br.ReadInt32();
                CollisionNumber = br.ReadInt32();
                Unk0C = br.ReadInt32();
                BoundsMin = br.ReadVector3();
                br.AssertSingle(0);
                BoundsMax = br.ReadVector3();
                br.AssertSingle(0);
                CollisionPosition = br.ReadVector3();
                br.AssertSingle(1);
                Edges = new List<Edge>(edgeCount);
            }

            internal void Write(BinaryWriterEx bw)
            {
                bw.WriteUInt32(EntityID);
                bw.WriteInt32(Edges.Count);
                bw.WriteInt32(CollisionNumber);
                bw.WriteInt32(Unk0C);
                bw.WriteVector3(BoundsMin);
                bw.WriteSingle(0);
                bw.WriteVector3(BoundsMax);
                bw.WriteSingle(0);
                bw.WriteVector3(CollisionPosition);
                bw.WriteSingle(1);
            }

            /// <summary>
            /// Grows the bounds to hold any edge that is outside them by more than the tolerance. The vanilla
            /// bounds are often wider than the edges, so they are never shrunk, and the tolerance keeps edges
            /// that are outside them by a rounding error from changing them.
            /// </summary>
            public void ExpandBoundsToEdges(float tolerance = 0.01f)
            {
                var min = BoundsMin;
                var max = BoundsMax;

                foreach (Edge edge in Edges)
                {
                    var low = Vector3.Min(edge.Start, edge.End);
                    var high = Vector3.Max(edge.Start, edge.End) + new Vector3(0, edge.Height, 0);

                    min = new Vector3(Lower(min.X, low.X), Lower(min.Y, low.Y), Lower(min.Z, low.Z));
                    max = new Vector3(Higher(max.X, high.X), Higher(max.Y, high.Y), Higher(max.Z, high.Z));
                }

                BoundsMin = min;
                BoundsMax = max;

                float Lower(float bound, float value) => value < bound - tolerance ? value : bound;
                float Higher(float bound, float value) => value > bound + tolerance ? value : bound;
            }
        }

        /// <summary>
        /// A vertical wall along the boundary of an area, rising from the line between its ends.
        /// </summary>
        public class Edge
        {
            internal const int Size = 0x30;

            /// <summary>
            /// One end of the bottom of the wall.
            /// </summary>
            public Vector3 Start { get; set; }

            /// <summary>
            /// The other end of the bottom of the wall. Always at the same height as the start in the vanilla files.
            /// </summary>
            public Vector3 End { get; set; }

            /// <summary>
            /// The height of the wall above its ends.
            /// </summary>
            public float Height { get; set; }

            /// <summary>
            /// Unknown; (0, 1, 0), except for collisions rotated about Y, where Y is the cosine of the rotation.
            /// </summary>
            public Vector3 UnkVector { get; set; }

            /// <summary>
            /// Creates a 10 m wall along X with a height of 10 m.
            /// </summary>
            public Edge()
            {
                Start = new Vector3(-5, 0, 0);
                End = new Vector3(5, 0, 0);
                Height = 10;
                UnkVector = Vector3.UnitY;
            }

            /// <summary>
            /// Creates a copy of an edge.
            /// </summary>
            public Edge(Edge edge)
            {
                Start = edge.Start;
                End = edge.End;
                Height = edge.Height;
                UnkVector = edge.UnkVector;
            }

            internal Edge(BinaryReaderEx br)
            {
                Start = br.ReadVector3();
                br.AssertSingle(0);
                End = br.ReadVector3();
                br.AssertSingle(0);
                Height = br.ReadSingle();
                UnkVector = br.ReadVector3();
            }

            internal void Write(BinaryWriterEx bw)
            {
                bw.WriteVector3(Start);
                bw.WriteSingle(0);
                bw.WriteVector3(End);
                bw.WriteSingle(0);
                bw.WriteSingle(Height);
                bw.WriteVector3(UnkVector);
            }
        }
    }
}
