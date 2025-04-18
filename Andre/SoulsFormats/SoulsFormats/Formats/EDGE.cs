using System;
using System.Collections.Generic;
using System.Numerics;

namespace SoulsFormats
{
    /// <summary>
    /// A Sekiro file that defines grapple points and hangable edges for a model.
    /// </summary>
    public class EDGE : SoulsFile<EDGE>
    {
        /// <summary>
        /// Unknown.
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Edges defined in this file.
        /// </summary>
        public List<Edge> Edges { get; set; }

        /// <summary>
        /// Creates an empty EDGE.
        /// </summary>
        public EDGE()
        {
            Edges = new List<Edge>();
        }

        /// <summary>
        /// Deserializes file data from a stream.
        /// </summary>
        protected override void Read(BinaryReaderEx br)
        {
            br.BigEndian = false;
            br.AssertInt32(4);
            int edgeCount = br.ReadInt32();
            ID = br.ReadInt32();
            br.AssertInt32(0);

            Edges = new List<Edge>(edgeCount);
            for (int i = 0; i < edgeCount; i++)
                Edges.Add(new Edge(br));
        }

        /// <summary>
        /// Serializes file data to a stream.
        /// </summary>
        protected override void Write(BinaryWriterEx bw)
        {
            bw.BigEndian = false;
            bw.WriteInt32(4);
            bw.WriteInt32(Edges.Count);
            bw.WriteInt32(ID);
            bw.WriteInt32(0);

            foreach (Edge edge in Edges)
                edge.Write(bw);
        }

        /// <summary>
        /// Which type of edge an edge is.
        /// </summary>
        public enum EdgeType : byte
        {
            /// <summary>
            /// A grapplable point.
            /// </summary>
            Grapple = 1,

            /// <summary>
            /// A hangable ledge.
            /// </summary>
            Hang = 2,

            /// <summary>
            /// A huggable wall.
            /// </summary>
            Hug = 3,
        }

        /// <summary>
        /// A grapple point, hangable ledge, or huggable wall.
        /// </summary>
        public class Edge
        {
            /// <summary>
            /// The starting point of the edge.
            /// </summary>
            public Vector3 V1 { get; set; }

            /// <summary>
            /// The ending point of the edge.
            /// </summary>
            public Vector3 V2 { get; set; }

            /// <summary>
            /// Only for wires, the point you're actually pulled towards.
            /// </summary>
            public Vector3 V3 { get; set; }

            /// <summary>
            /// Only for wires, unknown, always 1.
            /// </summary>
            public float Unk2C { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int Unk30 { get; set; }

            /// <summary>
            /// What type of edge this is.
            /// </summary>
            public EdgeType Type { get; set; }

            /// <summary>
            /// For wires, a relative ID in WireVariationParam; for walls, unknown.
            /// </summary>
            public byte VariationID { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte Unk36 { get; set; }

            /// <summary>
            /// Creates an Edge with default values.
            /// </summary>
            public Edge()
            {
                Type = EdgeType.Grapple;
            }

            /// <summary>
            /// Clones an existing Edge.
            /// </summary>
            public Edge Clone()
            {
                return (Edge)MemberwiseClone();
            }

            internal Edge(BinaryReaderEx br)
            {
                V1 = br.ReadVector3();
                br.AssertSingle(1);
                V2 = br.ReadVector3();
                br.AssertSingle(1);
                V3 = br.ReadVector3();
                Unk2C = br.ReadSingle();
                Unk30 = br.ReadInt32();
                Type = br.ReadEnum8<EdgeType>();
                VariationID = br.ReadByte();
                Unk36 = br.ReadByte();
                br.AssertByte(0);
                br.AssertInt32(0);
                br.AssertInt32(0);
            }

            internal void Write(BinaryWriterEx bw)
            {
                bw.WriteVector3(V1);
                bw.WriteSingle(1);
                bw.WriteVector3(V2);
                bw.WriteSingle(1);
                bw.WriteVector3(V3);
                bw.WriteSingle(Unk2C);
                bw.WriteInt32(Unk30);
                bw.WriteByte((byte)Type);
                bw.WriteByte(VariationID);
                bw.WriteByte(Unk36);
                bw.WriteByte(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
            }

            /// <summary>
            /// Returns relevant information about the edge as a string.
            /// </summary>
            public override string ToString()
            {
                if (Type == EdgeType.Grapple)
                    return $"{Type} Var:{VariationID} {Unk30} {Unk36} {V1} {V2} {V3}";
                else
                    return $"{Type} Var:{VariationID} {Unk30} {Unk36} {V1} {V2}";
            }
        }
    }
}
