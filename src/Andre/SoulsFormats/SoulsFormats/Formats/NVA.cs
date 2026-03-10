using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace SoulsFormats
{
    /// <summary>
    /// A file that defines the placement and properties of navmeshes in BB, DS3, Sekiro, and ER. Extension: .nva
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

            /// <summary>
            /// Elden Ring
            /// </summary>
            EldenRing = 8,
        }

        /// <summary>
        /// The format version of this file.
        /// </summary>
        public NVAVersion Version { get; set; }

        /// <summary>
        /// Navmesh instances in the map.
        /// </summary>
        public NavmeshSection Navmeshes { get; set; }

        public FaceDataSection FaceDatas { get; set; }

        public NodeBankSection NodeBanks { get; set; }

        public Section3 Entries3 { get; set; }

        /// <summary>
        /// Connections between different navmeshes.
        /// </summary>
        public ConnectorSection Connectors { get; set; }

        public LevelConnectorSection LevelConnectors { get; set; }

        public Section9 Entries9 { get; set; }
        public Section10 Entries10 { get; set; }
        public Section11 Entries11 { get; set; }
        public Section13 Entries13 { get; set; }

        /// <summary>
        /// Keeps track of whether the empty Section 12 was present in the file.
        /// </summary>
        public bool HasSection12 { get; set; } = false;

        /// <summary>
        /// Keeps track of the version used by the empty Section 12 for reserialization.
        /// </summary>
        public int Section12Version { get; set; } = 1;

        /// <summary>
        /// Creates an empty NVA formatted for Elden Ring.
        /// </summary>
        public NVA() // : this(NVAVersion.EldenRing) // for future compatability
        {
            Version = NVAVersion.EldenRing;
            Entries11 = new Section11();
            Navmeshes = new NavmeshSection(4);
            FaceDatas = new FaceDataSection();
            NodeBanks = new NodeBankSection();
            Entries3 = new Section3();
            Connectors = new ConnectorSection();
            LevelConnectors = new LevelConnectorSection();
            Entries9 = new Section9();
            Entries10 = new Section10();
            HasSection12 = true;
            Entries13 = new Section13();
        }

        // /// <summary>
        // /// Creates an empty NVA formatted for specified version.
        // /// </summary>
        // public NVA(NVAVersion version)
        // {
        //     // Need to go through this and only intialize the sections needed for that version.
        //     // Probably need a switch statement with fallthrough
        //     Version = version;
        //     Entries11 = new Section11();
        //     Navmeshes = new NavmeshSection(2);
        //     FaceDatas = new FaceDataSection();
        //     NodeBanks = new NodeBankSection();
        //     Entries3 = new Section3();
        //     Connectors = new ConnectorSection();
        //     LevelConnectors = new LevelConnectorSection();
        //     Entries9 = new Section9();
        //     Entries10 = new Section10();
        //     HasSection12 = true;
        //     Entries13 = new Section13();
        // }

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
            int sectionCount = br.ReadInt32();

            NavMeshConnectionSection navMeshConns = null;
            GraphConnectionSection graphConns = null;
            GateNodeSection gateNodes = null;

            for (int i = 0; i < sectionCount; i++)
            {
                // Peek at the index of the next section
                int index = br.GetInt32(br.Position);

                switch (index)
                {
                    case 0: Navmeshes = new NavmeshSection(br); break;
                    case 1: FaceDatas = new FaceDataSection(br); break;
                    case 2: NodeBanks = new NodeBankSection(br); break;
                    case 3: Entries3 = new Section3(br); break;
                    case 4: Connectors = new ConnectorSection(br); break;
                    case 5: navMeshConns = new NavMeshConnectionSection(br); break;
                    case 6: graphConns = new GraphConnectionSection(br); break;
                    case 7: LevelConnectors = new LevelConnectorSection(br); break;
                    case 8: gateNodes = new GateNodeSection(br); break;
                    case 9: Entries9 = new Section9(br); break;
                    case 10: Entries10 = new Section10(br); break;
                    case 11: Entries11 = new Section11(br); break;
                    case 12:
                        br.AssertInt32(12);
                        Section12Version = br.ReadInt32();
                        int length = br.ReadInt32();
                        br.ReadInt32(); // entryCount
                        br.Skip(length);
                        HasSection12 = true;
                        break;
                    case 13: Entries13 = new Section13(br); break;
                    default:
                        throw new NotImplementedException($"Unrecognized section index: {index}");
                }
            }

            // Link sub-sections back to their parent classes
            if (Navmeshes != null && gateNodes != null)
            {
                foreach (Navmesh navmesh in Navmeshes)
                    navmesh.TakeGateNodes(gateNodes);
            }

            if (Connectors != null && navMeshConns != null && graphConns != null)
            {
                foreach (Connector connector in Connectors)
                    connector.TakeConnections(navMeshConns, graphConns);
            }
        }

        /// <summary>
        /// Serializes file data to a stream.
        /// </summary>
        protected override void Write(BinaryWriterEx bw)
        {
            var navMeshConns = new NavMeshConnectionSection();
            var graphConns = new GraphConnectionSection();
            if (Connectors != null)
            {
                foreach (Connector connector in Connectors)
                    connector.GiveConnections(navMeshConns, graphConns);
            }

            var gateNodes = new GateNodeSection(Version == NVAVersion.Sekiro ? 2 : 1);
            if (Navmeshes != null)
            {
                foreach (Navmesh navmesh in Navmeshes)
                    navmesh.GiveGateNodes(gateNodes);
            }

            // Dynamically determine the section count based on present data
            int sectionCount = 0;
            if (Navmeshes != null) sectionCount++;
            if (FaceDatas != null) sectionCount++;
            if (NodeBanks != null) sectionCount++;
            if (Entries3 != null) sectionCount++;
            if (Connectors != null) sectionCount += 3; // Connectors (4), NavMeshConns (5), GraphConns (6)
            if (LevelConnectors != null) sectionCount++;

            // GateNodes (8) - Explicitly exclude for OldBloodborne
            if (Navmeshes != null && Version != NVAVersion.OldBloodborne) sectionCount++;

            if (Entries9 != null) sectionCount++;
            if (Entries10 != null) sectionCount++;
            if (Entries11 != null) sectionCount++;
            if (HasSection12) sectionCount++;
            if (Entries13 != null) sectionCount++;

            bw.BigEndian = false;
            bw.WriteASCII("NVMA");
            bw.WriteUInt32((uint)Version);
            bw.ReserveUInt32("FileSize");
            bw.WriteInt32(sectionCount);

            // Write sections only if they exist
            if (Entries11 != null) Entries11.Write(bw, 11);
            if (Navmeshes != null) Navmeshes.Write(bw, 0);
            if (FaceDatas != null) FaceDatas.Write(bw, 1);
            if (NodeBanks != null) NodeBanks.Write(bw, 2);
            if (Entries3 != null) Entries3.Write(bw, 3);

            if (Connectors != null)
            {
                Connectors.Write(bw, 4);
                navMeshConns.Write(bw, 5);
                graphConns.Write(bw, 6);
            }

            if (LevelConnectors != null) LevelConnectors.Write(bw, 7);
            if (Navmeshes != null) gateNodes.Write(bw, 8);
            if (Entries9 != null) Entries9.Write(bw, 9);
            if (Entries10 != null) Entries10.Write(bw, 10);

            if (HasSection12)
            {
                bw.WriteInt32(12);
                bw.WriteInt32(Section12Version);
                bw.ReserveInt32("Section12Length");
                bw.WriteInt32(0); // count

                long start = bw.Position;
                if (bw.Position % 0x10 != 0)
                    bw.WritePattern(0x10 - (int)bw.Position % 0x10, 0xFF);
                bw.FillInt32("Section12Length", (int)(bw.Position - start));
            }

            if (Entries13 != null) Entries13.Write(bw, 13);

            bw.FillUInt32("FileSize", (uint)bw.Position);
        }

        /// <summary>
        /// NVA is split up into lists of different types.
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
        public class NavmeshSection : Section<Navmesh>
        {
            /// <summary>
            /// Creates an empty NavmeshSection with the given version.
            /// </summary>
            public NavmeshSection(int version) : base(version) { }

            internal NavmeshSection(BinaryReaderEx br) : base(br, 0, 2, 3, 4) { }

            internal override void ReadEntries(BinaryReaderEx br, int count)
            {
                for (int i = 0; i < count; i++)
                    Add(new Navmesh(br, Version));
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
        public class Navmesh
        {
            // Split the Vector4s into Vector4 + float since Smithbox doesn't really support Vector4 for Position/Rotation/Scale and this is the easiest place to 'fix' the issue.

            /// <summary>
            /// Position of the mesh.
            /// </summary>
            public Vector3 Position { get; set; }
            public float PositionW { get; set; }

            /// <summary>
            /// Rotation of the mesh, in radians.
            /// </summary>
            public Vector3 Rotation { get; set; }
            public float RotationW { get; set; }

            /// <summary>
            /// Scale of the mesh.
            /// </summary>
            public Vector3 Scale { get; set; }
            public float ScaleW { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int NameID { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int ModelID { get; set; }
            public int FaceDataIndex { get; set; }
            public int Unk3C { get; set; }
            /// <summary>
            /// Should equal number of vertices in the model file.
            /// </summary>
            public int FaceCount { get; set; }
            public int Unk4C { get; set; }
            public bool IsConnectedNavmeshesInline { get; set; }

            public List<int> ConnectedNavmeshes { get; set; }
            /// <summary>
            /// Adjacent nodes in an inter-navmesh graph.
            /// </summary>
            public List<GateNode> GateNodes { get; set; }

            private short GateNodeIndex;
            private short GateNodeCount;
            private int ConnectedNavmeshesCount;

            /// <summary>
            /// Creates a Navmesh with default values.
            /// </summary>
            public Navmesh()
            {
                Scale = Vector3.One;
                ScaleW = 1;

                ConnectedNavmeshes = new List<int>();
                GateNodes = new List<GateNode>();
                // No clue. Maybe a sentinel value for when 
                // ConnectedNavmeshes is inline (even though unkOffset seems to also do that?)
                ConnectedNavmeshesCount = 1075419545;
            }

            internal Navmesh(BinaryReaderEx br, int version)
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
                ConnectedNavmeshesCount = br.ReadInt32();
                GateNodeIndex = br.ReadInt16();
                GateNodeCount = br.ReadInt16();
                Unk4C = br.ReadInt32();

                if (version < 4)
                {
                    if (ConnectedNavmeshesCount > 16)
                        throw new InvalidDataException("Connected navmeshes count should not exceed 16 in DS3/BB.");
                    ConnectedNavmeshes = new List<int>(br.ReadInt32s(ConnectedNavmeshesCount));
                    for (int i = 0; i < 16 - ConnectedNavmeshesCount; i++)
                        br.AssertInt32(-1);
                }
                else
                {
                    int unkOffset = br.ReadInt32();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);

                    if (unkOffset == 0xFF01)
                    {
                        IsConnectedNavmeshesInline = true;
                        ConnectedNavmeshes = new List<int>(br.ReadInt32s(12));
                    }
                    else if (ConnectedNavmeshesCount > 0)
                    {
                        ConnectedNavmeshes = new List<int>(br.GetInt32s(unkOffset, ConnectedNavmeshesCount));
                    }
                    else
                    {
                        ConnectedNavmeshes = new List<int>();
                    }
                }
            }

            internal void TakeGateNodes(GateNodeSection gateNodes)
            {
                GateNodes = new List<GateNode>(GateNodeCount);
                for (int i = 0; i < GateNodeCount; i++)
                    GateNodes.Add(gateNodes[GateNodeIndex + i]);
                GateNodeCount = -1;
            }

            internal void GiveGateNodes(GateNodeSection gateNodesList)
            {
                // Sometimes when the gate node count is 0 the index is also 0,
                // but usually this is accurate.
                GateNodeIndex = (short)gateNodesList.Count;
                gateNodesList.AddRange(GateNodes);
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
                bw.WriteInt32(IsConnectedNavmeshesInline ? ConnectedNavmeshesCount : ConnectedNavmeshes.Count);
                bw.WriteInt16(GateNodeIndex);
                bw.WriteInt16((short)GateNodes.Count);
                bw.WriteInt32(Unk4C);

                if (version < 4)
                {
                    if (ConnectedNavmeshes.Count > 16)
                        throw new InvalidDataException("Connected navmeshes count should not exceed 16 in DS3/BB.");
                    bw.WriteInt32s(ConnectedNavmeshes);
                    for (int i = 0; i < 16 - ConnectedNavmeshes.Count; i++)
                        bw.WriteInt32(-1);
                }
                else
                {
                    if (IsConnectedNavmeshesInline)
                    {
                        bw.WriteInt32(0xFF01);
                        bw.WriteInt32(0);
                        bw.WriteInt32(0);
                        bw.WriteInt32(0);
                        for (int i = 0; i < 12; i++)
                        {
                            bw.WriteInt32(i < ConnectedNavmeshes.Count ? ConnectedNavmeshes[i] : 0);
                        }
                    }
                    else
                    {
                        bw.ReserveInt32($"UnkOffset{index}");
                        bw.WriteInt32(0);
                        bw.WriteInt32(0);
                        bw.WriteInt32(0);
                    }
                }
            }

            internal void WriteNameRefs(BinaryWriterEx bw, int version, int index)
            {
                if (version >= 4 && !IsConnectedNavmeshesInline)
                {
                    if (ConnectedNavmeshes.Count > 0)
                    {
                        bw.FillInt32($"UnkOffset{index}", (int)bw.Position);
                        bw.WriteInt32s(ConnectedNavmeshes);
                    }
                    else
                    {
                        bw.FillInt32($"UnkOffset{index}", 0);
                    }
                }
            }
        }

        /// <summary>
        /// Face Data Section.
        /// </summary>
        public class FaceDataSection : Section<FaceData>
        {
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
        /// Face Data.
        /// </summary>
        public class FaceData
        {
            public int Unk00 { get; set; }
            public int Unk04 { get; set; }

            public FaceData() { }

            internal FaceData(BinaryReaderEx br)
            {
                Unk00 = br.ReadInt32();
                Unk04 = br.ReadInt32();
            }

            internal void Write(BinaryWriterEx bw)
            {
                bw.WriteInt32(Unk00);
                bw.WriteInt32(Unk04);
            }

            /// <summary>
            /// Returns a string representation of the entry.
            /// </summary>
            public override string ToString()
            {
                return $"Unk00: {Unk00} Unk04: {Unk04}";
            }
        }

        /// <summary>
        /// Node Bank Section.
        /// </summary>
        public class NodeBankSection : Section<NodeBank>
        {
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
        /// Node Bank.
        /// </summary>
        public class NodeBank
        {
            /// <summary>
            /// Bank ID.
            /// </summary>
            public int BankID { get; set; }

            /// <summary>
            /// Face references in this entry; maximum of 64.
            /// </summary>
            public List<NodeBankFace> Faces { get; set; }

            public int EntityID { get; set; }
            public int Unk0C { get; set; }

            public NodeBank()
            {
                Faces = new List<NodeBankFace>();
            }

            internal NodeBank(BinaryReaderEx br)
            {
                BankID = br.ReadInt32();
                int faceNum = br.ReadInt32();
                EntityID = br.ReadInt32();
                Unk0C = br.ReadInt32();

                if (faceNum > 64)
                    throw new InvalidDataException("NodeBank face count should not exceed 64.");

                Faces = new List<NodeBankFace>(faceNum);
                for (int i = 0; i < faceNum; i++)
                    Faces.Add(new NodeBankFace(br));

                for (int i = 0; i < 64 - faceNum; i++)
                    br.AssertInt64(0);
            }

            internal void Write(BinaryWriterEx bw)
            {
                bw.WriteInt32(BankID);
                bw.WriteInt32(Faces.Count);
                bw.WriteInt32(EntityID);
                bw.WriteInt32(Unk0C);

                if (Faces.Count > 64)
                    throw new InvalidDataException("NodeBank face count should not exceed 64.");

                foreach (NodeBankFace face in Faces)
                    face.Write(bw);

                for (int i = 0; i < 64 - Faces.Count; i++)
                    bw.WriteInt64(0);
            }

            /// <summary>
            /// Returns a string representation of the entry.
            /// </summary>
            public override string ToString()
            {
                return $"{BankID} {EntityID} [{Faces.Count} Faces]";
            }

        }

        public class NodeBankFace
        {
            public int FaceIndex { get; set; }
            public int CollisionID { get; set; }

            public NodeBankFace() { }

            internal NodeBankFace(BinaryReaderEx br)
            {
                FaceIndex = br.ReadInt32();
                CollisionID = br.ReadInt32();
            }

            internal void Write(BinaryWriterEx bw)
            {
                bw.WriteInt32(FaceIndex);
                bw.WriteInt32(CollisionID);
            }

            /// <summary>
            /// Returns a string representation of the reference.
            /// </summary>
            public override string ToString()
            {
                return $"{FaceIndex} {CollisionID}";
            }
        }

        public class Section3 : Section<Entry3>
        {
            public Section3() : base(1) { }

            internal Section3(BinaryReaderEx br) : base(br, 3, 1) { }

            internal override void ReadEntries(BinaryReaderEx br, int count)
            {
                for (int i = 0; i < count; i++)
                    Add(new Entry3(br));
            }

            internal override void WriteEntries(BinaryWriterEx bw)
            {
                foreach (Entry3 entry in this)
                    entry.Write(bw);
            }
        }

        public class Entry3
        {
            public Vector4[] Vectors { get; set; }

            const int _numElements = 8;

            public Entry3()
            {
                Vectors = new Vector4[_numElements];
            }

            internal Entry3(BinaryReaderEx br)
            {
                // Reminder that this should be empty prior to Elden Ring
                // throw new NotImplementedException("Section3 is empty in all known NVAs.");
                Vectors = new Vector4[_numElements];
                for (int i = 0; i < _numElements; i++)
                    Vectors[i] = br.ReadVector4();
            }

            internal void Write(BinaryWriterEx bw)
            {
                // Reminder that this should be empty prior to Elden Ring
                // throw new NotImplementedException("Section3 is empty in all known NVAs.");

                for (int i = 0; i < _numElements; i++)
                    bw.WriteVector4(Vectors[i]);
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
            public int Unk14 { get; set; }
            public int Unk1C { get; set; }

            /// <summary>
            /// NavMeshs used by this connection.
            /// </summary>
            public List<NavMeshConnection> NavMeshConnections { get; set; }
            public List<GraphConnection> GraphConnections { get; set; }

            private int NavMeshConnectionCount;
            private int GraphConnectionCount;
            private int NavMeshConnectionIndex;
            private int GraphConnectionIndex;

            /// <summary>
            /// Creates a Connector with default values.
            /// </summary>
            public Connector()
            {
                NavMeshConnections = new List<NavMeshConnection>();
                GraphConnections = new List<GraphConnection>();
            }

            internal Connector(BinaryReaderEx br)
            {
                MainNameID = br.ReadInt32();
                TargetNameID = br.ReadInt32();
                NavMeshConnectionCount = br.ReadInt32();
                GraphConnectionCount = br.ReadInt32();
                NavMeshConnectionIndex = br.ReadInt32();
                Unk14 = br.ReadInt32();
                GraphConnectionIndex = br.ReadInt32();
                Unk1C = br.ReadInt32();
            }

            internal void TakeConnections(NavMeshConnectionSection navMeshConns, GraphConnectionSection graphConns)
            {
                NavMeshConnections = new List<NavMeshConnection>(NavMeshConnectionCount);
                for (int i = 0; i < NavMeshConnectionCount; i++)
                    NavMeshConnections.Add(navMeshConns[NavMeshConnectionIndex + i]);
                NavMeshConnectionCount = -1;

                GraphConnections = new List<GraphConnection>(GraphConnectionCount);
                for (int i = 0; i < GraphConnectionCount; i++)
                    GraphConnections.Add(graphConns[GraphConnectionIndex + i]);
                GraphConnectionCount = -1;
            }

            internal void GiveConnections(NavMeshConnectionSection navMeshConns, GraphConnectionSection graphConns)
            {
                NavMeshConnectionIndex = navMeshConns.Count;
                navMeshConns.AddRange(NavMeshConnections);

                GraphConnectionIndex = graphConns.Count;
                graphConns.AddRange(GraphConnections);
            }

            internal void Write(BinaryWriterEx bw)
            {
                bw.WriteInt32(MainNameID);
                bw.WriteInt32(TargetNameID);
                bw.WriteInt32(NavMeshConnections.Count);
                bw.WriteInt32(GraphConnections.Count);
                bw.WriteInt32(NavMeshConnectionIndex);
                bw.WriteInt32(Unk14);
                bw.WriteInt32(GraphConnectionIndex);
                bw.WriteInt32(Unk1C);
            }

            /// <summary>
            /// Returns a string representation of the connector.
            /// </summary>
            public override string ToString()
            {
                return $"{MainNameID} -> {TargetNameID} [{NavMeshConnections.Count} Points][{GraphConnections.Count} Conditions]";
            }

        }

        /// <summary>
        /// A list of NavMeshes used to connect navmeshes.
        /// </summary>
        internal class NavMeshConnectionSection : Section<NavMeshConnection>
        {
            /// <summary>
            /// Creates an empty ConnectorPointSection.
            /// </summary>
            public NavMeshConnectionSection() : base(1) { }

            /// <summary>
            /// Creates an empty NavMeshConnectionSection
            /// </summary>
            internal NavMeshConnectionSection(BinaryReaderEx br) : base(br, 5, 1) { }

            internal override void ReadEntries(BinaryReaderEx br, int count)
            {
                for (int i = 0; i < count; i++)
                    Add(new NavMeshConnection(br));
            }

            internal override void WriteEntries(BinaryWriterEx bw)
            {
                foreach (NavMeshConnection entry in this)
                    entry.Write(bw);
            }
        }

        /// <summary>
        /// A point used to connect two navmeshes.
        /// </summary>
        public class NavMeshConnection
        {
            public int FaceIndex { get; set; }
            public int EdgeIndex { get; set; }
            public int OppositeFaceIndex { get; set; }
            public int OppositeEdgeIndex { get; set; }

            public NavMeshConnection() { }

            internal NavMeshConnection(BinaryReaderEx br)
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
        }

        /// <summary>
        /// A list of unknown conditions used by connectors.
        /// </summary>
        internal class GraphConnectionSection : Section<GraphConnection>
        {
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
            public int NodeIndex { get; set; }
            public int OppositeNodeIndex { get; set; }

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
        /// Returns a string representation of the condition.
        /// </summary>
        public class LevelConnectorSection : Section<LevelConnector>
        {
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
            public Vector4 Position { get; set; }
            public int NavmeshID { get; set; }
            public int Unk14 { get; set; }
            public int Unk18 { get; set; }
            public int Unk1C { get; set; }

            /// <summary>
            /// Creates a LevelConnector with default values.
            /// </summary>
            public LevelConnector() { }

            internal LevelConnector(BinaryReaderEx br)
            {
                Position = br.ReadVector4();
                NavmeshID = br.ReadInt32();
                Unk14 = br.ReadInt32();
                Unk18 = br.ReadInt32();
                Unk1C = br.ReadInt32();
            }

            internal void Write(BinaryWriterEx bw)
            {
                bw.WriteVector4(Position);
                bw.WriteInt32(NavmeshID);
                bw.WriteInt32(Unk14);
                bw.WriteInt32(Unk18);
                bw.WriteInt32(Unk1C);
            }

            /// <summary>
            /// Returns a string representation of the entry.
            /// </summary>
            public override string ToString()
            {
                return $"Position: {Position} NavmeshID: {NavmeshID} Unk14: {Unk14} Unk18: {Unk18} Unk1C: {Unk1C}";
            }
        }

        /// <summary>
        /// Unknown. Version: 1 for BB and DS3, 2 for Sekiro.
        /// </summary>
        internal class GateNodeSection : Section<GateNode>
        {
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
                    this[i].WriteCosts(bw, Version, i);
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
            public short NodeSubID { get; set; }
            /// <summary>
            /// Unknown.
            /// </summary>
            public List<short> NeighbourGateNodeCosts { get; set; }

            /// <summary>
            /// Unknown; only present in Sekiro.
            /// </summary>
            public int Unk14 { get; set; }

            /// <summary>
            /// Creates a GateNode with default values.
            /// </summary>
            public GateNode()
            {
                NeighbourGateNodeCosts = new List<short>();
            }

            internal GateNode(BinaryReaderEx br, int version)
            {
                Position = br.ReadVector3();
                ConnectedNavmeshIndex = br.ReadInt16();
                NodeSubID = br.ReadInt16();

                if (version < 2)
                {
                    NeighbourGateNodeCosts = new List<short>(br.ReadInt16s(16));
                }
                else
                {
                    int costsCount = br.ReadInt32();
                    Unk14 = br.ReadInt32();
                    int costsOffset = br.ReadInt32();
                    br.AssertInt32(0);

                    if (costsCount > 0)
                        NeighbourGateNodeCosts = new List<short>(br.GetInt16s(costsOffset, costsCount));
                    else
                        NeighbourGateNodeCosts = new List<short>();
                }
            }

            internal void Write(BinaryWriterEx bw, int version, int index)
            {
                bw.WriteVector3(Position);
                bw.WriteInt16(ConnectedNavmeshIndex);
                bw.WriteInt16(NodeSubID);

                if (version < 2)
                {
                    if (NeighbourGateNodeCosts.Count > 16)
                        throw new InvalidDataException("GateNode costs count must not exceed 16 in BB/DS3.");

                    // Stored as floats in previous nva implementation. Converted with:
                    // (ushort)(distance == -1 ? 0xFFFF : Math.Round(distance * 100)
                    foreach (short cost in NeighbourGateNodeCosts)
                        bw.WriteInt16(cost);

                    for (int i = 0; i < 16 - NeighbourGateNodeCosts.Count; i++)
                        bw.WriteInt16(-1);
                }
                else
                {
                    bw.WriteInt32(NeighbourGateNodeCosts.Count);
                    bw.WriteInt32(Unk14);
                    bw.ReserveInt32($"CostsOffset{index}");
                    bw.WriteInt32(0);
                }
            }

            internal void WriteCosts(BinaryWriterEx bw, int version, int index)
            {
                if (version >= 2)
                {
                    // This check not in original. Is this needed?
                    if (NeighbourGateNodeCosts.Count > 0)
                    {
                        bw.FillInt32($"CostsOffset{index}", (int)bw.Position);
                        foreach (short cost in NeighbourGateNodeCosts)
                            bw.WriteInt16(cost);
                    }
                    else
                    {
                        bw.FillInt32($"CostsOffset{index}", 0);
                    }
                }
            }

            /// <summary>
            /// Returns a string representation of the entry.
            /// </summary>
            public override string ToString()
            {
                return $"[Position: {Position} ConnectedNavmeshIndex: {ConnectedNavmeshIndex} NeighbourGateNodeCosts.Count: {NeighbourGateNodeCosts.Count} SubIDs Unk14: {Unk14}]";
            }
        }

        public class Section9 : Section<Entry9>
        {
            public Section9() : base(1) { }

            internal Section9(BinaryReaderEx br) : base(br, 9, 1) { }

            internal override void ReadEntries(BinaryReaderEx br, int count)
            {
                for (int i = 0; i < count; i++) Add(new Entry9(br));
            }

            internal override void WriteEntries(BinaryWriterEx bw)
            {
                foreach (Entry9 entry in this) entry.Write(bw);
            }
        }

        public class Entry9
        {
            public int Unk00 { get; set; }
            public int Unk04 { get; set; }
            public int Unk08 { get; set; }
            public int Unk0C { get; set; }

            public Entry9()
            {
            }

            internal Entry9(BinaryReaderEx br)
            {
                Unk00 = br.ReadInt32();
                Unk04 = br.ReadInt32();
                Unk08 = br.ReadInt32();
                Unk0C = br.ReadInt32();
            }

            internal void Write(BinaryWriterEx bw)
            {
                bw.WriteInt32(Unk00);
                bw.WriteInt32(Unk04);
                bw.WriteInt32(Unk08);
                bw.WriteInt32(Unk0C);
            }
        }

        public class Section10 : Section<Entry10>
        {
            public Section10() : base(1)
            {
            }

            internal Section10(BinaryReaderEx br) : base(br, 10, 1)
            {
            }

            internal override void ReadEntries(BinaryReaderEx br, int count)
            {
                for (int i = 0; i < count; i++) Add(new Entry10(br));
            }

            internal override void WriteEntries(BinaryWriterEx bw)
            {
                foreach (Entry10 entry in this) entry.Write(bw);
            }
        }

        public class Entry10
        {
            public Vector4 Unk00 { get; set; }
            public int Unk10 { get; set; }
            public int Unk14 { get; set; }
            public int Unk18 { get; set; }
            public int Unk1C { get; set; }

            public Entry10()
            {
            }

            internal Entry10(BinaryReaderEx br)
            {
                Unk00 = br.ReadVector4();
                Unk10 = br.ReadInt32();
                Unk14 = br.ReadInt32();
                Unk18 = br.ReadInt32();
                Unk1C = br.ReadInt32();
            }

            internal void Write(BinaryWriterEx bw)
            {
                bw.WriteVector4(Unk00);
                bw.WriteInt32(Unk10);
                bw.WriteInt32(Unk14);
                bw.WriteInt32(Unk18);
                bw.WriteInt32(Unk1C);
            }
        }

        public class Section11 : Section<Entry11>
        {
            public Section11() : base(1)
            {
            }

            internal Section11(BinaryReaderEx br) : base(br, 11, 1)
            {
            }

            internal override void ReadEntries(BinaryReaderEx br, int count)
            {
                for (int i = 0; i < count; i++) Add(new Entry11(br));
            }

            internal override void WriteEntries(BinaryWriterEx bw)
            {
                foreach (Entry11 entry in this) entry.Write(bw);
            }
        }

        public class Entry11
        {
            public int Unk00 { get; set; }
            public int Unk04 { get; set; }
            public int Unk08 { get; set; }
            public int Unk0C { get; set; }

            public Entry11()
            {
            }

            internal Entry11(BinaryReaderEx br)
            {
                Unk00 = br.ReadInt32();
                Unk04 = br.ReadInt32();
                Unk08 = br.ReadInt32();
                Unk0C = br.ReadInt32();
            }

            internal void Write(BinaryWriterEx bw)
            {
                bw.WriteInt32(Unk00);
                bw.WriteInt32(Unk04);
                bw.WriteInt32(Unk08);
                bw.WriteInt32(Unk0C);
            }
        }

        public class Section13 : Section<Entry13>
        {
            public Section13() : base(1)
            {
            }

            internal Section13(BinaryReaderEx br) : base(br, 13, 1)
            {
            }

            internal override void ReadEntries(BinaryReaderEx br, int count)
            {
                for (int i = 0; i < count; i++) Add(new Entry13(br));
            }

            internal override void WriteEntries(BinaryWriterEx bw)
            {
                foreach (Entry13 entry in this) entry.Write(bw);
            }
        }

        public class Entry13
        {
            public Vector4[] Vectors { get; set; }
            public int[] Ints { get; set; }

            public Entry13()
            {
                Vectors = new Vector4[6];
                Ints = new int[8];
            }

            internal Entry13(BinaryReaderEx br)
            {
                Vectors = new Vector4[6];
                for (int i = 0; i < 6; i++) Vectors[i] = br.ReadVector4();

                Ints = new int[8];
                for (int i = 0; i < 8; i++) Ints[i] = br.ReadInt32();
            }

            internal void Write(BinaryWriterEx bw)
            {
                for (int i = 0; i < 6; i++) bw.WriteVector4(Vectors[i]);
                for (int i = 0; i < 8; i++) bw.WriteInt32(Ints[i]);
            }
        }
    }
}