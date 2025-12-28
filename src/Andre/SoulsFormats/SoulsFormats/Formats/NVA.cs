using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using static SoulsFormats.NVA;
using static SoulsFormats.NVA.NodeBank;

namespace SoulsFormats
{
    /// <summary>
    /// A file that defines the placement and properties of navmeshes in BB, DS3, and Sekiro. Extension: .nva
    /// </summary>
    public class NVA : SoulsFile<NVA>
    {
        /// <summary>
        /// Version of the overall format.
        /// </summary>
        public enum NVAVersion : uint
        {
            /// <summary>
            /// Used for a single BB test map, m29_03_10_00; has no Section8
            /// </summary>
            OldBloodborne = 3,

            /// <summary>
            /// Dark Souls 3 and Bloodborne
            /// </summary>
            DarkSouls3 = 4,

            /// <summary>
            /// Sekiro
            /// </summary>
            Sekiro = 5,

            EldenRing = 8,
        }

        /// <summary>
        /// The format version of this file.
        /// </summary>
        public NVAVersion Version { get; set; }

        public int SectionCount { get; set; }

        /// <summary>
        /// Navmesh instances in the map.
        /// </summary>
        public NavmeshInfoSection NavmeshInfoEntries { get; set; }

        /// <summary>
        /// Unknown.
        /// </summary>
        public FaceDataSection FaceDataEntries { get; set; }

        /// <summary>
        /// Unknown.
        /// </summary>
        public NodeBankSection NodeBankEntries { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Section3 Section3Entries { get; set; }

        /// <summary>
        /// Connections between different navmeshes.
        /// </summary>
        public ConnectorSection ConnectorEntries { get; set; }

        /// <summary>
        /// Unknown.
        /// </summary>
        public LevelConnectorSection LevelConnectorEntries { get; set; }

        public Section9 Section9Entries { get; set; }

        public Section10 Section10Entries { get; set; }

        public Section11 Section11Entries { get; set; }

        public Section12 Section12Entries { get; set; }

        public Section13 Section13Entries { get; set; }

        /// <summary>
        /// Creates an empty NVA formatted for DS3.
        /// </summary>
        public NVA()
        {
            Version = NVAVersion.DarkSouls3;
            NavmeshInfoEntries = new NavmeshInfoSection(2);
            FaceDataEntries = new FaceDataSection();
            NodeBankEntries = new NodeBankSection();
            Section3Entries = new Section3();
            ConnectorEntries = new ConnectorSection();
            LevelConnectorEntries = new LevelConnectorSection();
        }

        /// <summary>
        /// Checks whether the data appears to be a file of this format.
        /// </summary>
        protected override bool Is(BinaryReaderEx br)
        {
            if (br.Length < 4)
                return false;

            string magic = br.GetASCII(0, 4);
            return magic == "NVMA";
        }

        /// <summary>
        /// Deserializes file data from a stream.
        /// </summary>
        protected override void Read(BinaryReaderEx br)
        {
            br.BigEndian = false;
            br.AssertASCII("NVMA");
            Version = br.ReadEnum32<NVAVersion>();
            br.ReadUInt32(); // File size
            SectionCount = br.ReadInt32(); // Section count

            if (Version is NVAVersion.EldenRing)
            {
                Section11Entries = new Section11(br);
            }

            NavmeshInfoEntries = new NavmeshInfoSection(br);
            FaceDataEntries = new FaceDataSection(br);
            NodeBankEntries = new NodeBankSection(br);
            Section3Entries = new Section3(br);
            ConnectorEntries = new ConnectorSection(br);

            var navmeshConnections = new NavmeshConnectionSection(br);
            var graphConnections = new GraphConnectionSection(br);

            LevelConnectorEntries = new LevelConnectorSection(br);

            GateNodeSection gateNodes;
            if (Version == NVAVersion.OldBloodborne)
                gateNodes = new GateNodeSection(1);
            else
                gateNodes = new GateNodeSection(br);

            foreach (NavmeshInfo navmesh in NavmeshInfoEntries)
                navmesh.TakeGateNodes(gateNodes);

            foreach (Connector connector in ConnectorEntries)
                connector.TakePointsAndConds(navmeshConnections, graphConnections);

            if(Version is NVAVersion.EldenRing)
            {
                Section9Entries = new Section9(br);
                Section10Entries = new Section10(br);
                Section12Entries = new Section12(br);
                Section13Entries = new Section13(br);
            }
        }

        /// <summary>
        /// Serializes file data to a stream.
        /// </summary>
        protected override void Write(BinaryWriterEx bw)
        {
            var navmeshConnections = new NavmeshConnectionSection();
            var graphConnections = new GraphConnectionSection();

            foreach (Connector connector in ConnectorEntries)
                connector.GivePointsAndConds(navmeshConnections, graphConnections);

            var mapNodes = new GateNodeSection(Version == NVAVersion.Sekiro ? 2 : 1);
            foreach (NavmeshInfo navmesh in NavmeshInfoEntries)
                navmesh.GiveGateNodes(mapNodes);

            bw.BigEndian = false;
            bw.WriteASCII("NVMA");
            bw.WriteUInt32((uint)Version);
            bw.ReserveUInt32("FileSize");
            bw.WriteInt32(SectionCount);

            if (Version is NVAVersion.EldenRing)
            {
                Section11Entries.Write(bw, 11);
            }

            NavmeshInfoEntries.Write(bw, 0);
            FaceDataEntries.Write(bw, 1);
            NodeBankEntries.Write(bw, 2);
            Section3Entries.Write(bw, 3);
            ConnectorEntries.Write(bw, 4);

            navmeshConnections.Write(bw, 5);
            graphConnections.Write(bw, 6);

            LevelConnectorEntries.Write(bw, 7);

            if (Version != NVAVersion.OldBloodborne)
                mapNodes.Write(bw, 8);

            if (Version is NVAVersion.EldenRing)
            {
                Section9Entries.Write(bw, 9);
                Section10Entries.Write(bw, 10);
                Section12Entries.Write(bw, 12);
                Section13Entries.Write(bw, 13);
            }

            bw.FillUInt32("FileSize", (uint)bw.Position);
        }

        /// <summary>
        /// NVA is split up into 8 lists of different types.
        /// </summary>
        public abstract class Section<T> : List<T>
        {
            /// <summary>
            /// A version number indicating the format of the section. Don't change this unless you know what you're doing.
            /// </summary>
            public int Version { get; set; }

            internal Section(int version) : base()
            {
                Version = version;
            }

            internal Section(BinaryReaderEx br, int index, params int[] versions) : base()
            {
                br.AssertInt32(index);
                Version = br.AssertInt32(versions);
                int length = br.ReadInt32();
                int count = br.ReadInt32();
                Capacity = count;

                long start = br.Position;
                ReadEntries(br, count);
                br.Position = start + length;
            }

            internal abstract void ReadEntries(BinaryReaderEx br, int count);

            internal void Write(BinaryWriterEx bw, int index)
            {
                bw.WriteInt32(index);
                bw.WriteInt32(Version);
                bw.ReserveInt32("SectionLength");
                bw.WriteInt32(Count);

                long start = bw.Position;
                WriteEntries(bw);
                if (bw.Position % 0x10 != 0)
                    bw.WritePattern(0x10 - (int)bw.Position % 0x10, 0xFF);
                bw.FillInt32("SectionLength", (int)(bw.Position - start));
            }

            internal abstract void WriteEntries(BinaryWriterEx bw);
        }

        /// <summary>
        /// A list of navmesh instances. Version: 2 for DS3 and the BB test map, 3 for BB, 4 for Sekiro.
        /// </summary>
        public class NavmeshInfoSection : Section<NavmeshInfo>
        {
            /// <summary>
            /// Creates an empty NavmeshSection with the given version.
            /// </summary>
            public NavmeshInfoSection(int version) : base(version) { }

            internal NavmeshInfoSection(BinaryReaderEx br) : base(br, 0, 2, 3, 4) { }

            internal override void ReadEntries(BinaryReaderEx br, int count)
            {
                for (int i = 0; i < count; i++)
                    Add(new NavmeshInfo(br, Version));
            }

            internal override void WriteEntries(BinaryWriterEx bw)
            {
                for (int i = 0; i < Count; i++)
                    this[i].Write(bw, Version, i);

                for (int i = 0; i < Count; i++)
                    this[i].WriteNameRefs(bw, Version, i);
            }
        }

        /// <summary>
        /// An instance of a navmesh.
        /// </summary>
        public class NavmeshInfo
        {
            /// <summary>
            /// Position of the mesh.
            /// </summary>
            public Vector3 Position { get; set; }

            private float PositionW { get; set; }

            /// <summary>
            /// Rotation of the mesh, in radians.
            /// </summary>
            public Vector3 Rotation { get; set; }

            private float RotationW { get; set; }

            /// <summary>
            /// Scale of the mesh.
            /// </summary>
            public Vector3 Scale { get; set; }
            private float ScaleW { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int NameID { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int ModelID { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int FaceDataIndex { get; set; }

            /// <summary>
            /// Should equal number of vertices in the model file.
            /// </summary>
            public int FaceCount { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<int> ConnectedNavmeshIDs { get; set; }

            /// <summary>
            /// Adjacent nodes in an inter-navmesh graph.
            /// </summary>
            public List<GateNode> GateNodes { get; set; }

            /// <summary>
            /// Unknown
            /// </summary>
            public bool Unk4C { get; set; }

            private short GateNodeIndex;
            private short SubNodesCount;

            public int Unk3C { get; set; }


            private int UnkOffset;
            public float ER_Unk04 { get; set; }
            public float ER_Unk08 { get; set; }
            public int ER_Unk0C { get; set; }

            /// <summary>
            /// Creates a Navmesh with default values.
            /// </summary>
            public NavmeshInfo()
            {
                Scale = Vector3.One;
                ConnectedNavmeshIDs = new List<int>();
                GateNodes = new List<GateNode>();
            }

            internal NavmeshInfo(BinaryReaderEx br, int version)
            {
                Position = br.ReadVector3();
                PositionW = br.ReadSingle();
                Rotation = br.ReadVector3();
                RotationW = br.ReadSingle();
                Scale = br.ReadVector3();
                ScaleW = br.ReadSingle();

                NameID = br.ReadInt32();
                ModelID = br.ReadInt32();
                FaceDataIndex = br.ReadInt32();
                Unk3C = br.ReadInt32();
                FaceCount = br.ReadInt32();
                int connectedNavmeshesCount = br.ReadInt32();
                GateNodeIndex = br.ReadInt16();
                SubNodesCount = br.ReadInt16();
                Unk4C = br.AssertInt32([0, 1]) == 1;

                // Fallback
                ConnectedNavmeshIDs = new List<int>();

                if (version == 2 || version == 3)
                {
                    if (connectedNavmeshesCount > 16)
                        throw new InvalidDataException("Name reference count should not exceed 16 in DS3/BB.");

                    ConnectedNavmeshIDs = br.ReadInt32s(connectedNavmeshesCount).ToList();

                    for (int i = 0; i < 16 - connectedNavmeshesCount; i++)
                        br.AssertInt32(-1);
                }
                else if(version == 4)
                {
                    UnkOffset = br.ReadInt32();
                    ER_Unk04 = br.ReadSingle();
                    ER_Unk08 = br.ReadSingle();
                    ER_Unk0C = br.ReadInt32();

                    if (UnkOffset == 0xFF01)
                    {
                        ConnectedNavmeshIDs = br.ReadInt32s(12).ToList();
                    }
                    else if (connectedNavmeshesCount > 0)
                    {
                        long start = br.Position;
                        br.Position = start + UnkOffset;

                        ConnectedNavmeshIDs = br.GetInt32s(UnkOffset, connectedNavmeshesCount).ToList();

                        br.Position = start;
                    }
                }
            }

            internal void TakeGateNodes(GateNodeSection gateNodes)
            {
                GateNodes = new List<GateNode>(SubNodesCount);

                for (int i = 0; i < SubNodesCount; i++)
                    GateNodes.Add(gateNodes[GateNodeIndex + i]);

                SubNodesCount = -1;

                foreach (GateNode mapNode in GateNodes)
                {
                    if (mapNode.NeighbourGateNodeCosts.Count > GateNodes.Count)
                        mapNode.NeighbourGateNodeCosts.RemoveRange(GateNodes.Count, mapNode.NeighbourGateNodeCosts.Count - GateNodes.Count);
                }
            }

            internal void Write(BinaryWriterEx bw, int version, int index)
            {
                bw.WriteVector3(Position);
                bw.WriteSingle(PositionW);
                bw.WriteVector3(Rotation);
                bw.WriteSingle(RotationW);
                bw.WriteVector3(Scale);
                bw.WriteSingle(ScaleW);
                bw.WriteInt32(NameID);
                bw.WriteInt32(ModelID);
                bw.WriteInt32(FaceDataIndex);
                bw.WriteInt32(Unk3C);
                bw.WriteInt32(FaceCount);
                bw.WriteInt32(ConnectedNavmeshIDs.Count);
                bw.WriteInt16(GateNodeIndex);
                bw.WriteInt16((short)GateNodes.Count);
                bw.WriteInt32(Unk4C ? 1 : 0);

                if (version == 2 || version == 3)
                {
                    if (ConnectedNavmeshIDs.Count > 16)
                        throw new InvalidDataException("Name reference count should not exceed 16 in DS3/BB.");

                    bw.WriteInt32s(ConnectedNavmeshIDs);

                    for (int i = 0; i < 16 - ConnectedNavmeshIDs.Count; i++)
                        bw.WriteInt32(-1);
                }
                else
                {
                    bw.ReserveInt32($"NameRefOffset{index}");
                    bw.WriteSingle(ER_Unk04);
                    bw.WriteSingle(ER_Unk08);
                    bw.WriteInt32(ER_Unk0C);

                    if (UnkOffset == 0xFF01)
                    {
                        bw.WriteInt32s(new int[12]);
                    }
                    else if (ConnectedNavmeshIDs.Count > 0)
                    {
                        long start = bw.Position;
                        bw.Position = start + UnkOffset;

                        for (int i = 0; i < ConnectedNavmeshIDs.Count; i++)
                        {
                            bw.WriteInt32(ConnectedNavmeshIDs[i]);
                        }

                        bw.Position = start;
                    }
                }
            }

            internal void WriteNameRefs(BinaryWriterEx bw, int version, int index)
            {
                if (version >= 4)
                {
                    bw.FillInt32($"NameRefOffset{index}", (int)bw.Position);
                    bw.WriteInt32s(ConnectedNavmeshIDs);
                }
            }

            internal void GiveGateNodes(GateNodeSection mapNodes)
            {
                // Sometimes when the map node count is 0 the index is also 0,
                // but usually this is accurate.
                GateNodeIndex = (short)mapNodes.Count;
                mapNodes.AddRange(GateNodes);
            }

            /// <summary>
            /// Returns a string representation of the navmesh.
            /// </summary>
            public override string ToString()
            {
                return $"{NameID} {Position} {Rotation} [{ConnectedNavmeshIDs.Count} References] [{GateNodes.Count} MapNodes]";
            }
        }

        /// <summary>
        /// Unknown.
        /// </summary>
        public class FaceDataSection : Section<FaceData>
        {
            /// <summary>
            /// Creates an empty Section1.
            /// </summary>
            public FaceDataSection() : base(1) { }

            internal FaceDataSection(BinaryReaderEx br) : base(br, 1, 1) { }

            internal override void ReadEntries(BinaryReaderEx br, int count)
            {
                for (int i = 0; i < count; i++)
                    Add(new FaceData(br));
            }

            internal override void WriteEntries(BinaryWriterEx bw)
            {
                foreach (FaceData entry in this)
                    entry.Write(bw);
            }
        }

        /// <summary>
        /// Unknown.
        /// </summary>
        public class FaceData
        {
            /// <summary>
            /// Unknown; always 0 in DS3 and SDT, sometimes 1 in BB.
            /// </summary>
            public int Unk00 { get; set; }

            /// <summary>
            /// Creates an Entry1 with default values.
            /// </summary>
            public FaceData() { }

            internal FaceData(BinaryReaderEx br)
            {
                Unk00 = br.ReadInt32();
                br.AssertInt32(0);
            }

            internal void Write(BinaryWriterEx bw)
            {
                bw.WriteInt32(Unk00);
                bw.WriteInt32(0);
            }

            /// <summary>
            /// Returns a string representation of the entry.
            /// </summary>
            public override string ToString()
            {
                return $"{Unk00}";
            }
        }

        /// <summary>
        /// Unknown.
        /// </summary>
        public class NodeBankSection : Section<NodeBank>
        {
            /// <summary>
            /// Creates an empty Section2.
            /// </summary>
            public NodeBankSection() : base(1) { }

            internal NodeBankSection(BinaryReaderEx br) : base(br, 2, 1) { }

            internal override void ReadEntries(BinaryReaderEx br, int count)
            {
                for (int i = 0; i < count; i++)
                    Add(new NodeBank(br));
            }

            internal override void WriteEntries(BinaryWriterEx bw)
            {
                foreach (NodeBank entry in this)
                    entry.Write(bw);
            }
        }

        /// <summary>
        /// Unknown.
        /// </summary>
        public class NodeBank
        {
            /// <summary>
            /// Unknown; seems to just be the index of this entry.
            /// </summary>
            public int BankIndex { get; set; }

            /// <summary>
            /// References in this entry; maximum of 64.
            /// </summary>
            public List<NodeBankFace> Faces { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int EntityID { get; set; }

            /// <summary>
            /// Creates an Entry2 with default values.
            /// </summary>
            public NodeBank()
            {
                Faces = new List<NodeBankFace>();
                EntityID = -1;
            }

            internal NodeBank(BinaryReaderEx br)
            {
                BankIndex = br.ReadInt32();
                int faceNum = br.ReadInt32();
                EntityID = br.ReadInt32();
                br.AssertInt32(0);

                if (faceNum > 64)
                    throw new InvalidDataException("NodeBank faceNum count should not exceed 64.");

                Faces = new List<NodeBankFace>(faceNum);

                for (int i = 0; i < faceNum; i++)
                    Faces.Add(new NodeBankFace(br));

                for (int i = 0; i < 64 - faceNum; i++)
                    br.AssertInt64(0);
            }

            internal void Write(BinaryWriterEx bw)
            {
                bw.WriteInt32(BankIndex);
                bw.WriteInt32(Faces.Count);
                bw.WriteInt32(EntityID);
                bw.WriteInt32(0);
                if (Faces.Count > 64)
                    throw new InvalidDataException("Entry2 reference count should not exceed 64.");

                foreach (NodeBankFace reference in Faces)
                    reference.Write(bw);

                for (int i = 0; i < 64 - Faces.Count; i++)
                    bw.WriteInt64(0);
            }

            /// <summary>
            /// Returns a string representation of the entry.
            /// </summary>
            public override string ToString()
            {
                return $"{BankIndex} {EntityID} [{Faces.Count} References]";
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class NodeBankFace
            {
                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkIndex { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int NameID { get; set; }

                /// <summary>
                /// Creates a Reference with defalt values.
                /// </summary>
                public NodeBankFace() { }

                internal NodeBankFace(BinaryReaderEx br)
                {
                    UnkIndex = br.ReadInt32();
                    NameID = br.ReadInt32();
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteInt32(UnkIndex);
                    bw.WriteInt32(NameID);
                }

                /// <summary>
                /// Returns a string representation of the reference.
                /// </summary>
                public override string ToString()
                {
                    return $"{UnkIndex} {NameID}";
                }
            }
        }

        public class Section3 : Section<Entry3>
        {
            public Section3() : base(1) { }

            internal Section3(BinaryReaderEx br) : base(br, 3, 1) { }

            internal override void ReadEntries(BinaryReaderEx br, int count)
            {
                for (int i = 0; i < count; i++)
                    Add(new Entry3(br, Version));
            }

            internal override void WriteEntries(BinaryWriterEx bw)
            {
                foreach (Entry3 entry in this)
                    entry.Write(bw, Version);
            }
        }

        public class Entry3
        {
            internal Entry3(BinaryReaderEx br, int version)
            {
                if (version == 8)
                {
                    // Vector4 unk00[10];
                    br.ReadVector4();
                    br.ReadVector4();
                    br.ReadVector4();
                    br.ReadVector4();
                    br.ReadVector4();
                    br.ReadVector4();
                    br.ReadVector4();
                    br.ReadVector4();
                    br.ReadVector4();
                    br.ReadVector4();
                }
            }

            internal void Write(BinaryWriterEx bw, int version)
            {
                if (version == 8)
                {
                    bw.WriteVector4(new Vector4());
                    bw.WriteVector4(new Vector4());
                    bw.WriteVector4(new Vector4());
                    bw.WriteVector4(new Vector4());
                    bw.WriteVector4(new Vector4());
                    bw.WriteVector4(new Vector4());
                    bw.WriteVector4(new Vector4());
                    bw.WriteVector4(new Vector4());
                    bw.WriteVector4(new Vector4());
                    bw.WriteVector4(new Vector4());
                }
            }
        }

        /// <summary>
        /// A list of connections between navmeshes.
        /// </summary>
        public class ConnectorSection : Section<Connector>
        {
            /// <summary>
            /// Creates an empty ConnectorSection.
            /// </summary>
            public ConnectorSection() : base(1) { }

            internal ConnectorSection(BinaryReaderEx br) : base(br, 4, 1) { }

            internal override void ReadEntries(BinaryReaderEx br, int count)
            {
                for (int i = 0; i < count; i++)
                    Add(new Connector(br));
            }

            internal override void WriteEntries(BinaryWriterEx bw)
            {
                foreach (Connector entry in this)
                    entry.Write(bw);
            }
        }

        /// <summary>
        /// A connection between two navmeshes.
        /// </summary>
        public class Connector
        {
            /// <summary>
            /// Unknown.
            /// </summary>
            public int MainNameID { get; set; }

            /// <summary>
            /// The navmesh to be attached.
            /// </summary>
            public int TargetNameID { get; set; }

            /// <summary>
            /// Points used by this connection.
            /// </summary>
            public List<NavmeshConnection> NavmeshConnections { get; set; }

            /// <summary>
            /// Conditions used by this connection.
            /// </summary>
            public List<GraphConnection> GraphConnections { get; set; }

            private int NavmeshConnectionCount;
            private int GraphConnectionCount;
            private int NavmeshConnectionIndex;
            private int GraphConnectionIndex;

            /// <summary>
            /// Creates a Connector with default values.
            /// </summary>
            public Connector()
            {
                NavmeshConnections = new List<NavmeshConnection>();
                GraphConnections = new List<GraphConnection>();
            }

            internal Connector(BinaryReaderEx br)
            {
                MainNameID = br.ReadInt32();
                TargetNameID = br.ReadInt32();
                NavmeshConnectionCount = br.ReadInt32();
                GraphConnectionCount = br.ReadInt32();
                NavmeshConnectionIndex = br.ReadInt32();
                br.AssertInt32(0);
                GraphConnectionIndex = br.ReadInt32();
                br.AssertInt32(0);
            }

            internal void TakePointsAndConds(NavmeshConnectionSection points, GraphConnectionSection conds)
            {
                NavmeshConnections = new List<NavmeshConnection>(NavmeshConnectionCount);
                for (int i = 0; i < NavmeshConnectionCount; i++)
                    NavmeshConnections.Add(points[NavmeshConnectionIndex + i]);
                NavmeshConnectionCount = -1;

                GraphConnections = new List<GraphConnection>(GraphConnectionCount);
                for (int i = 0; i < GraphConnectionCount; i++)
                    GraphConnections.Add(conds[GraphConnectionIndex + i]);
                GraphConnectionCount = -1;
            }

            internal void GivePointsAndConds(NavmeshConnectionSection points, GraphConnectionSection conds)
            {
                NavmeshConnectionIndex = points.Count;
                points.AddRange(NavmeshConnections);

                GraphConnectionIndex = conds.Count;
                conds.AddRange(GraphConnections);
            }

            internal void Write(BinaryWriterEx bw)
            {
                bw.WriteInt32(MainNameID);
                bw.WriteInt32(TargetNameID);
                bw.WriteInt32(NavmeshConnections.Count);
                bw.WriteInt32(GraphConnections.Count);
                bw.WriteInt32(NavmeshConnectionIndex);
                bw.WriteInt32(0);
                bw.WriteInt32(GraphConnectionIndex);
                bw.WriteInt32(0);
            }

            /// <summary>
            /// Returns a string representation of the connector.
            /// </summary>
            public override string ToString()
            {
                return $"{MainNameID} -> {TargetNameID} [{NavmeshConnections.Count} Points][{GraphConnections.Count} Conditions]";
            }
        }

        /// <summary>
        /// A list of points used to connect navmeshes.
        /// </summary>
        internal class NavmeshConnectionSection : Section<NavmeshConnection>
        {
            /// <summary>
            /// Creates an empty ConnectorPointSection.
            /// </summary>
            public NavmeshConnectionSection() : base(1) { }

            internal NavmeshConnectionSection(BinaryReaderEx br) : base(br, 5, 1) { }

            internal override void ReadEntries(BinaryReaderEx br, int count)
            {
                for (int i = 0; i < count; i++)
                    Add(new NavmeshConnection(br));
            }

            internal override void WriteEntries(BinaryWriterEx bw)
            {
                foreach (NavmeshConnection entry in this)
                    entry.Write(bw);
            }
        }

        /// <summary>
        /// A point used to connect two navmeshes.
        /// </summary>
        public class NavmeshConnection
        {
            /// <summary>
            /// Unknown.
            /// </summary>
            public int FaceIndex { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int EdgeIndex { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int OppositeFaceIndex { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int OppositeEdgeIndex { get; set; }

            /// <summary>
            /// Creates a ConnectorPoint with default values.
            /// </summary>
            public NavmeshConnection() { }

            internal NavmeshConnection(BinaryReaderEx br)
            {
                FaceIndex = br.ReadInt32();
                EdgeIndex = br.ReadInt32();
                OppositeFaceIndex = br.ReadInt32();
                OppositeEdgeIndex = br.ReadInt32();
            }

            internal void Write(BinaryWriterEx bw)
            {
                bw.WriteInt32(FaceIndex);
                bw.WriteInt32(EdgeIndex);
                bw.WriteInt32(OppositeFaceIndex);
                bw.WriteInt32(OppositeEdgeIndex);
            }

            /// <summary>
            /// Returns a string representation of the point.
            /// </summary>
            public override string ToString()
            {
                return $"{FaceIndex} {EdgeIndex} {OppositeFaceIndex} {OppositeEdgeIndex}";
            }
        }

        /// <summary>
        /// A list of unknown conditions used by connectors.
        /// </summary>
        internal class GraphConnectionSection : Section<GraphConnection>
        {
            /// <summary>
            /// Creates an empty ConnectorConditionSection.
            /// </summary>
            public GraphConnectionSection() : base(1) { }

            internal GraphConnectionSection(BinaryReaderEx br) : base(br, 6, 1) { }

            internal override void ReadEntries(BinaryReaderEx br, int count)
            {
                for (int i = 0; i < count; i++)
                    Add(new GraphConnection(br));
            }

            internal override void WriteEntries(BinaryWriterEx bw)
            {
                foreach (GraphConnection entry in this)
                    entry.Write(bw);
            }
        }

        /// <summary>
        /// An unknown condition used by a connector.
        /// </summary>
        public class GraphConnection
        {
            /// <summary>
            /// Unknown.
            /// </summary>
            public int NodeIndex { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int OppositeNodeIndex { get; set; }

            /// <summary>
            /// Creates a ConnectorCondition with default values.
            /// </summary>
            public GraphConnection() { }

            internal GraphConnection(BinaryReaderEx br)
            {
                NodeIndex = br.ReadInt32();
                OppositeNodeIndex = br.ReadInt32();
            }

            internal void Write(BinaryWriterEx bw)
            {
                bw.WriteInt32(NodeIndex);
                bw.WriteInt32(OppositeNodeIndex);
            }

            /// <summary>
            /// Returns a string representation of the condition.
            /// </summary>
            public override string ToString()
            {
                return $"{NodeIndex} {OppositeNodeIndex}";
            }
        }

        /// <summary>
        /// Unknown.
        /// </summary>
        public class LevelConnectorSection : Section<LevelConnector>
        {
            /// <summary>
            /// Creates an empty Section7.
            /// </summary>
            public LevelConnectorSection() : base(1) { }

            internal LevelConnectorSection(BinaryReaderEx br) : base(br, 7, 1) { }

            internal override void ReadEntries(BinaryReaderEx br, int count)
            {
                for (int i = 0; i < count; i++)
                    Add(new LevelConnector(br));
            }

            internal override void WriteEntries(BinaryWriterEx bw)
            {
                foreach (LevelConnector entry in this)
                    entry.Write(bw);
            }
        }

        /// <summary>
        /// Unknown; believed to have something to do with connecting maps.
        /// </summary>
        public class LevelConnector
        {
            /// <summary>
            /// Unknown.
            /// </summary>
            public Vector3 Position { get; set; }
            public float PositionW { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int NameID { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int Unk18 { get; set; }

            /// <summary>
            /// Creates an Entry7 with default values.
            /// </summary>
            public LevelConnector() { }

            internal LevelConnector(BinaryReaderEx br)
            {
                Position = br.ReadVector3();
                PositionW = br.ReadSingle();
                NameID = br.ReadInt32();
                br.AssertInt32(0);
                Unk18 = br.ReadInt32();
                br.AssertInt32(0);
            }

            internal void Write(BinaryWriterEx bw)
            {
                bw.WriteVector3(Position);
                bw.WriteSingle(PositionW);
                bw.WriteInt32(NameID);
                bw.WriteInt32(0);
                bw.WriteInt32(Unk18);
                bw.WriteInt32(0);
            }

            /// <summary>
            /// Returns a string representation of the entry.
            /// </summary>
            public override string ToString()
            {
                return $"{Position} {NameID} {Unk18}";
            }
        }

        /// <summary>
        /// Unknown. Version: 1 for BB and DS3, 2 for Sekiro.
        /// </summary>
        internal class GateNodeSection : Section<GateNode>
        {
            /// <summary>
            /// Creates an empty Section8 with the given version.
            /// </summary>
            public GateNodeSection(int version) : base(version) { }

            internal GateNodeSection(BinaryReaderEx br) : base(br, 8, 1, 2) { }

            internal override void ReadEntries(BinaryReaderEx br, int count)
            {
                for (int i = 0; i < count; i++)
                    Add(new GateNode(br, Version));
            }

            internal override void WriteEntries(BinaryWriterEx bw)
            {
                for (int i = 0; i < Count; i++)
                    this[i].Write(bw, Version, i);

                for (int i = 0; i < Count; i++)
                    this[i].WriteNeighbourGateNodeCostsOffsets(bw, Version, i);
            }
        }

        /// <summary>
        /// Unknown.
        /// </summary>
        public class GateNode
        {
            /// <summary>
            /// Unknown.
            /// </summary>
            public Vector3 Position { get; set; }

            /// <summary>
            /// Index to a navmesh.
            /// </summary>
            public short ConnectedNavmeshIndex { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public short NodeSubId { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<float> NeighbourGateNodeCosts { get; set; }

            /// <summary>
            /// Unknown; only present in Sekiro.
            /// </summary>
            public int Unk14 { get; set; }

            /// <summary>
            /// Creates an Entry8 with default values.
            /// </summary>
            public GateNode()
            {
                NeighbourGateNodeCosts = new List<float>();
            }

            internal GateNode(BinaryReaderEx br, int version)
            {
                Position = br.ReadVector3();
                ConnectedNavmeshIndex = br.ReadInt16();
                NodeSubId = br.ReadInt16();

                if (version == 1)
                {
                    NeighbourGateNodeCosts = new List<float>(
                        br.ReadUInt16s(16).Select(s => s == 0xFFFF ? -1 : s * 0.01f));
                }
                else if(version == 2)
                {
                    int neighbourGateNodeCostsCount = br.ReadInt32();
                    Unk14 = br.ReadInt32();
                    int neighbourGateNodeCostsOffset = br.ReadInt32();
                    br.AssertInt32(0);

                    NeighbourGateNodeCosts = new List<float>(
                        br.GetUInt16s(neighbourGateNodeCostsOffset, neighbourGateNodeCostsCount).Select(s => s == 0xFFFF ? -1 : s * 0.01f));
                }
            }

            internal void Write(BinaryWriterEx bw, int version, int index)
            {
                bw.WriteVector3(Position);
                bw.WriteInt16(ConnectedNavmeshIndex);
                bw.WriteInt16(NodeSubId);

                if (version == 1)
                {
                    if (NeighbourGateNodeCosts.Count > 16)
                        throw new InvalidDataException("MapNode distance count must not exceed 16 in DS3/BB.");

                    foreach (float distance in NeighbourGateNodeCosts)
                        bw.WriteUInt16((ushort)(distance == -1 ? 0xFFFF : Math.Round(distance * 100)));

                    for (int i = 0; i < 16 - NeighbourGateNodeCosts.Count; i++)
                        bw.WriteUInt16(0xFFFF);
                }
                else if(version == 2)
                {
                    bw.WriteInt32(NeighbourGateNodeCosts.Count);
                    bw.WriteInt32(Unk14);
                    bw.ReserveInt32($"NeighbourGateNodeCostsOffset{index}");
                    bw.WriteInt32(0);
                }
            }

            internal void WriteNeighbourGateNodeCostsOffsets(BinaryWriterEx bw, int version, int index)
            {
                if (version >= 2)
                {
                    bw.FillInt32($"NeighbourGateNodeCostsOffset{index}", (int)bw.Position);
                    foreach (float distance in NeighbourGateNodeCosts)
                        bw.WriteUInt16((ushort)(distance == -1 ? 0xFFFF : Math.Round(distance * 100)));
                }
            }

            /// <summary>
            /// Returns a string representation of the entry.
            /// </summary>
            public override string ToString()
            {
                return $"{Position} {ConnectedNavmeshIndex} {NodeSubId} [{NeighbourGateNodeCosts.Count} SubIDs]";
            }
        }
    }

    public class Section9 : Section<Entry9>
    {
        public Section9() : base(1) { }

        internal Section9(BinaryReaderEx br) : base(br, 9, 1) { }

        internal override void ReadEntries(BinaryReaderEx br, int count)
        {
            for (int i = 0; i < count; i++)
                Add(new Entry9(br));
        }

        internal override void WriteEntries(BinaryWriterEx bw)
        {
            for (int i = 0; i < Count; i++)
                this[i].Write(bw, i);
        }
    }

    public class Entry9
    {
        public Entry9()
        {
        }

        internal Entry9(BinaryReaderEx br)
        {
            br.AssertInt32(0);
            br.AssertInt32(0);
            br.AssertInt32(0);
            br.AssertInt32(0);
        }

        internal void Write(BinaryWriterEx bw, int index)
        {
            bw.WriteInt32(0);
            bw.WriteInt32(0);
            bw.WriteInt32(0);
            bw.WriteInt32(0);
        }
    }

    public class Section10 : Section<Entry10>
    {
        public Section10() : base(1) { }

        internal Section10(BinaryReaderEx br) : base(br, 10, 1) { }

        internal override void ReadEntries(BinaryReaderEx br, int count)
        {
            for (int i = 0; i < count; i++)
                Add(new Entry10(br));
        }

        internal override void WriteEntries(BinaryWriterEx bw)
        {
            for (int i = 0; i < Count; i++)
                this[i].Write(bw, i);
        }
    }

    public class Entry10
    {
        public Vector4 Unk00 { get; set; }
        public int Unk04 { get; set; }
        public int Unk08 { get; set; }
        public int Unk0C { get; set; }
        public int Unk10 { get; set; }

        public Entry10()
        {
        }

        internal Entry10(BinaryReaderEx br)
        {
            Unk00 = br.ReadVector4();
            Unk04 = br.ReadInt32();
            Unk08 = br.ReadInt32();
            Unk0C = br.ReadInt32();
            Unk10 = br.ReadInt32();
        }

        internal void Write(BinaryWriterEx bw, int index)
        {
            bw.WriteVector4(Unk00);
            bw.WriteInt32(Unk04);
            bw.WriteInt32(Unk08);
            bw.WriteInt32(Unk0C);
            bw.WriteInt32(Unk10);
        }
    }

    public class Section11 : Section<Entry11>
    {
        public Section11() : base(1) { }

        internal Section11(BinaryReaderEx br) : base(br, 11, 1) { }

        internal override void ReadEntries(BinaryReaderEx br, int count)
        {
            for (int i = 0; i < count; i++)
                Add(new Entry11(br));
        }

        internal override void WriteEntries(BinaryWriterEx bw)
        {
            for (int i = 0; i < Count; i++)
                this[i].Write(bw, i);
        }
    }

    public class Entry11
    {
        public int Unk00 { get; set; }

        public Entry11()
        {
        }

        internal Entry11(BinaryReaderEx br)
        {
            Unk00 = br.ReadInt32();
            br.AssertInt32(0);
            br.AssertInt32(0);
            br.AssertInt32(0);
        }

        internal void Write(BinaryWriterEx bw, int index)
        {
            bw.WriteInt32(Unk00);
            bw.WriteInt32(0);
            bw.WriteInt32(0);
            bw.WriteInt32(0);
        }
    }

    public class Section12 : Section<Entry12>
    {
        public Section12() : base(1) { }

        internal Section12(BinaryReaderEx br) : base(br, 12, 1) { }

        internal override void ReadEntries(BinaryReaderEx br, int count = 0)
        {
            for (int i = 0; i < count; i++)
                Add(new Entry12(br));
        }

        internal override void WriteEntries(BinaryWriterEx bw)
        {
            for (int i = 0; i < Count; i++)
                this[i].Write(bw, i);
        }
    }

    public class Entry12
    {
        public Entry12()
        {
        }

        internal Entry12(BinaryReaderEx br)
        {
        }

        internal void Write(BinaryWriterEx bw, int index)
        {
        }
    }

    public class Section13 : Section<Entry13>
    {
        public Section13() : base(1) { }

        internal Section13(BinaryReaderEx br) : base(br, 13, 1) { }

        internal override void ReadEntries(BinaryReaderEx br, int count)
        {
            for (int i = 0; i < count; i++)
                Add(new Entry13(br));
        }

        internal override void WriteEntries(BinaryWriterEx bw)
        {
            for (int i = 0; i < Count; i++)
                this[i].Write(bw, i);
        }
    }

    public class Entry13
    {
        public Vector4 Unk00 { get; set; }
        public Vector4 Unk04 { get; set; }
        public Vector4 Unk08 { get; set; }
        public Vector4 Unk0C { get; set; }
        public Vector4 Unk10 { get; set; }
        public Vector4 Unk14 { get; set; }

        public int Unk18 { get; set; }
        public int Unk20 { get; set; }
        public int Unk24 { get; set; }
        public int Unk28 { get; set; }
        public int Unk2C { get; set; }
        public int Unk30 { get; set; }
        public int Unk34 { get; set; }

        public Entry13()
        {
        }

        internal Entry13(BinaryReaderEx br)
        {
            Unk00 = br.ReadVector4();
            Unk04 = br.ReadVector4();
            Unk08 = br.ReadVector4();
            Unk0C = br.ReadVector4();
            Unk10 = br.ReadVector4();
            Unk14 = br.ReadVector4();

            Unk18 = br.ReadInt32();
            Unk20 = br.ReadInt32();
            Unk24 = br.ReadInt32();
            Unk28 = br.ReadInt32();
            Unk2C = br.ReadInt32();
            Unk30 = br.ReadInt32();
            Unk34 = br.ReadInt32();
        }

        internal void Write(BinaryWriterEx bw, int index)
        {
            bw.WriteVector4(Unk00);
            bw.WriteVector4(Unk04);
            bw.WriteVector4(Unk08);
            bw.WriteVector4(Unk0C);
            bw.WriteVector4(Unk10);
            bw.WriteVector4(Unk14);

            bw.WriteInt32(Unk18);
            bw.WriteInt32(Unk20);
            bw.WriteInt32(Unk24);
            bw.WriteInt32(Unk28);
            bw.WriteInt32(Unk2C);
            bw.WriteInt32(Unk30);
            bw.WriteInt32(Unk34);
        }

        public override string ToString()
        {
            return $"";
        }
    }
}
