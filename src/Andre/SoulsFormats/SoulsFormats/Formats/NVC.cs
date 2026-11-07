using System.Collections.Generic;
using System.IO;

namespace SoulsFormats
{
    /// <summary>
    /// The navmesh connections between two neighbouring Elden Ring maps, streamed in when both are loaded.
    /// Laid out like the connector sections of <see cref="NVA_ER"/>: each connector pairs a navmesh of the first map
    /// with one of the second, and lists the face edges that join them (Havok's hkaiStreamingSet::NavMeshConnection).
    /// Extension: .nvc, named mAA_BB_CC_DD_mAA_BB_CC_DD after the two maps, in /map/nvc.
    /// </summary>
    public class NVC : SoulsFile<NVC>
    {
        /// <summary>
        /// The format version; 3 in every Elden Ring file. The game accepts 1 to 3.
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// The UID of the first map of the file name: the Unk00 of that map's <see cref="NVA_ER.Entry11"/>.
        /// Not the packed map number, which the game takes from the file name.
        /// </summary>
        public uint MapUID { get; set; }

        /// <summary>
        /// The UID of the second map of the file name: the Unk00 of that map's <see cref="NVA_ER.Entry11"/>.
        /// </summary>
        public uint OppositeMapUID { get; set; }

        /// <summary>
        /// Pairs of navmeshes, one in each map, and the connections between them.
        /// </summary>
        public List<Connector> Connectors { get; set; }

        /// <summary>
        /// Creates an empty NVC formatted for Elden Ring.
        /// </summary>
        public NVC()
        {
            Version = 3;
            Connectors = new List<Connector>();
        }

        /// <summary>
        /// Checks whether the data appears to be a file of this format.
        /// </summary>
        protected override bool Is(BinaryReaderEx br)
        {
            return br.Length >= 0x10 && br.GetASCII(0, 4) == "NVMC";
        }

        // Section types, by what the game stores them as (0x140C90DC0)
        private const int ConnectorSection = 0;
        private const int NavMeshConnectionSection = 1;
        private const int GraphConnectionSection = 2;
        private const int MapSection = 3;
        private const int Section4 = 4;

        /// <summary>
        /// Deserializes file data from a stream.
        /// </summary>
        protected override void Read(BinaryReaderEx br)
        {
            br.BigEndian = false;
            br.AssertASCII("NVMC");
            Version = br.AssertInt32([1, 2, 3]);
            br.ReadInt32(); // File size
            int sectionCount = br.ReadInt32();

            var connectorData = new List<(Connector Connector, int NavCount, int GraphCount, int NavIndex, int GraphIndex)>();
            var navMeshConns = new List<NVA_ER.NavMeshConnection>();
            var graphConns = new List<NVA_ER.GraphConnection>();
            Connectors = new List<Connector>();

            for (int i = 0; i < sectionCount; i++)
            {
                int type = br.ReadInt32();
                br.AssertInt32(1); // Section version
                int length = br.ReadInt32();
                int count = br.ReadInt32();
                long start = br.Position;

                switch (type)
                {
                    case MapSection:
                        MapUID = br.ReadUInt32();
                        OppositeMapUID = br.ReadUInt32();
                        br.AssertInt32(0);
                        br.AssertInt32(0);
                        break;
                    case ConnectorSection:
                        for (int c = 0; c < count; c++)
                        {
                            var connector = new Connector();
                            connector.MainNameID = br.ReadInt32();
                            connector.TargetNameID = br.ReadInt32();
                            int navCount = br.ReadInt32();
                            int graphCount = br.ReadInt32();
                            int navIndex = br.ReadInt32();
                            connector.Unk14 = br.ReadInt32();
                            int graphIndex = br.ReadInt32();
                            connector.Unk1C = br.ReadInt32();
                            connectorData.Add((connector, navCount, graphCount, navIndex, graphIndex));
                        }
                        break;
                    case NavMeshConnectionSection:
                        for (int c = 0; c < count; c++)
                            navMeshConns.Add(new NVA_ER.NavMeshConnection(br));
                        break;
                    case GraphConnectionSection:
                        for (int c = 0; c < count; c++)
                            graphConns.Add(new NVA_ER.GraphConnection(br));
                        break;
                    case Section4:
                        // Empty in every Elden Ring file, so its records are unknown
                        if (count != 0 || length != 0)
                            throw new InvalidDataException($"NVC section 4 has {count} entries; its layout is unknown.");
                        break;
                    default:
                        throw new InvalidDataException($"Unknown NVC section type {type}.");
                }

                br.Position = start + length;
            }

            foreach (var (connector, navCount, graphCount, navIndex, graphIndex) in connectorData)
            {
                connector.NavMeshConnections = navMeshConns.GetRange(navIndex, navCount);
                connector.GraphConnections = graphConns.GetRange(graphIndex, graphCount);
                Connectors.Add(connector);
            }
        }

        /// <summary>
        /// Serializes file data to a stream.
        /// </summary>
        protected override void Write(BinaryWriterEx bw)
        {
            bw.BigEndian = false;
            bw.WriteASCII("NVMC");
            bw.WriteInt32(Version);
            bw.ReserveInt32("FileSize");
            bw.WriteInt32(5);

            // The game files keep this order
            WriteSection(bw, MapSection, 1, () =>
            {
                bw.WriteUInt32(MapUID);
                bw.WriteUInt32(OppositeMapUID);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
            });

            int navIndex = 0, graphIndex = 0;
            WriteSection(bw, ConnectorSection, Connectors.Count, () =>
            {
                foreach (var connector in Connectors)
                {
                    bw.WriteInt32(connector.MainNameID);
                    bw.WriteInt32(connector.TargetNameID);
                    bw.WriteInt32(connector.NavMeshConnections.Count);
                    bw.WriteInt32(connector.GraphConnections.Count);
                    bw.WriteInt32(navIndex);
                    bw.WriteInt32(connector.Unk14);
                    bw.WriteInt32(graphIndex);
                    bw.WriteInt32(connector.Unk1C);
                    navIndex += connector.NavMeshConnections.Count;
                    graphIndex += connector.GraphConnections.Count;
                }
            });

            WriteSection(bw, NavMeshConnectionSection, navIndex, () =>
            {
                foreach (var connector in Connectors)
                    foreach (var conn in connector.NavMeshConnections)
                        conn.Write(bw);
            });

            WriteSection(bw, GraphConnectionSection, graphIndex, () =>
            {
                foreach (var connector in Connectors)
                    foreach (var conn in connector.GraphConnections)
                        conn.Write(bw);
            });

            WriteSection(bw, Section4, 0, () => { });

            bw.FillInt32("FileSize", (int)bw.Position);
        }

        private static void WriteSection(BinaryWriterEx bw, int type, int count, System.Action writeEntries)
        {
            bw.WriteInt32(type);
            bw.WriteInt32(1);
            long lengthPos = bw.Position;
            bw.WriteInt32(0);
            bw.WriteInt32(count);

            long start = bw.Position;
            writeEntries();
            // Every record the game files use is a multiple of 0x10 long; pad the 8 byte graph connections like NVA does
            if ((bw.Position - start) % 0x10 != 0)
                bw.WritePattern(0x10 - (int)((bw.Position - start) % 0x10), 0xFF);

            long end = bw.Position;
            bw.Position = lengthPos;
            bw.WriteInt32((int)(end - start));
            bw.Position = end;
        }

        /// <summary>
        /// A navmesh of the first map and one of the second, and the connections between them.
        /// </summary>
        public class Connector
        {
            /// <summary>
            /// The <see cref="NVA_ER.Navmesh.NameID"/> of the navmesh in the first map.
            /// </summary>
            public int MainNameID { get; set; }

            /// <summary>
            /// The <see cref="NVA_ER.Navmesh.NameID"/> of the navmesh in the second map.
            /// </summary>
            public int TargetNameID { get; set; }

            /// <summary>
            /// Unknown; always 0. Kept apart like <see cref="NVA_ER.Connector.Unk14"/>.
            /// </summary>
            public int Unk14 { get; set; }

            /// <summary>
            /// Unknown; always 0. Kept apart like <see cref="NVA_ER.Connector.Unk1C"/>.
            /// </summary>
            public int Unk1C { get; set; }

            /// <summary>
            /// The face edges joining the two navmeshes: Face/EdgeIndex in the first, the Opposite ones in the second.
            /// </summary>
            public List<NVA_ER.NavMeshConnection> NavMeshConnections { get; set; }

            /// <summary>
            /// Graph node connections; empty in every Elden Ring file.
            /// </summary>
            public List<NVA_ER.GraphConnection> GraphConnections { get; set; }

            /// <summary>
            /// Creates an empty Connector.
            /// </summary>
            public Connector()
            {
                NavMeshConnections = new List<NVA_ER.NavMeshConnection>();
                GraphConnections = new List<NVA_ER.GraphConnection>();
            }

            /// <summary>
            /// Returns a string representation of the connector.
            /// </summary>
            public override string ToString()
            {
                return $"{MainNameID} -> {TargetNameID} [{NavMeshConnections.Count} Connections]";
            }
        }
    }
}
