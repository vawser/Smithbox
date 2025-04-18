using System.Collections.Generic;

namespace SoulsFormats
{
    /// <summary>
    /// Companion file to a FLVER that has something to do with cloth, probably.
    /// </summary>
    public class CLM2 : SoulsFile<CLM2>
    {
        /// <summary>
        /// Each of these corresponds to a mesh in the FLVER.
        /// </summary>
        public List<Mesh> Meshes { get; set; }

        /// <summary>
        /// Creates a new CLM2 with no meshes.
        /// </summary>
        public CLM2()
        {
            Meshes = new List<Mesh>();
        }

        /// <summary>
        /// Checks whether the data appears to be a file of this format.
        /// </summary>
        protected override bool Is(BinaryReaderEx br)
        {
            if (br.Length < 4)
                return false;

            string magic = br.GetASCII(0, 4);
            return magic == "CLM2";
        }

        /// <summary>
        /// Deserializes file data from a stream.
        /// </summary>
        protected override void Read(BinaryReaderEx br)
        {
            br.AssertASCII("CLM2");
            br.AssertInt32(0);
            br.AssertInt16(1);
            br.AssertInt16(1);
            br.AssertInt32(0);
            br.AssertInt32(0);
            int meshCount = br.ReadInt32();
            br.AssertInt32(0x28);
            br.AssertInt32(0);
            br.AssertInt32(0);
            br.AssertInt32(0x28);

            Meshes = new List<Mesh>(meshCount);
            for (int i = 0; i < meshCount; i++)
                Meshes.Add(new Mesh(br));
        }

        /// <summary>
        /// Serializes file data to a stream.
        /// </summary>
        protected override void Write(BinaryWriterEx bw)
        {
            bw.WriteASCII("CLM2");
            bw.WriteInt32(0);
            bw.WriteInt16(1);
            bw.WriteInt16(1);
            bw.WriteInt32(0);
            bw.WriteInt32(0);
            bw.WriteInt32(Meshes.Count);
            bw.WriteInt32(0x28);
            bw.WriteInt32(0);
            bw.WriteInt32(0);
            bw.WriteInt32(0x28);

            for (int i = 0; i < Meshes.Count; i++)
                Meshes[i].WriteHeader(bw, i);

            for (int i = 0; i < Meshes.Count; i++)
                Meshes[i].WriteEntries(bw, i);
        }

        /// <summary>
        /// A list of entries that control something or other in a corresponding FLVER mesh.
        /// </summary>
        public class Mesh : List<Mesh.Entry>
        {
            internal Mesh(BinaryReaderEx br) : base()
            {
                br.AssertInt32(0);
                int entryCount = br.ReadInt32();
                uint entriesOffset = br.ReadUInt32();
                br.AssertInt32(0);

                br.StepIn(entriesOffset);
                {
                    for (int i = 0; i < entryCount; i++)
                        Add(new Entry(br));
                }
                br.StepOut();
            }

            internal void WriteHeader(BinaryWriterEx bw, int index)
            {
                bw.WriteInt32(0);
                bw.WriteInt32(Count);
                bw.ReserveUInt32($"EntriesOffset{index}");
                bw.WriteInt32(0);
            }

            internal void WriteEntries(BinaryWriterEx bw, int index)
            {
                if (Count == 0)
                {
                    bw.FillUInt32($"EntriesOffset{index}", 0);
                }
                else
                {
                    bw.FillUInt32($"EntriesOffset{index}", (uint)bw.Position);
                    foreach (Entry entry in this)
                        entry.Write(bw);
                    bw.Pad(8);
                }
            }

            /// <summary>
            /// Unknown what this does.
            /// </summary>
            public class Entry
            {
                /// <summary>
                /// Unknown.
                /// </summary>
                public short Unk00 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short Unk02 { get; set; }

                /// <summary>
                /// Creates a new Entry with the given values.
                /// </summary>
                public Entry(short unk00, short unk02)
                {
                    Unk00 = unk00;
                    Unk02 = unk02;
                }

                internal Entry(BinaryReaderEx br)
                {
                    Unk00 = br.ReadInt16();
                    Unk02 = br.ReadInt16();
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteInt16(Unk00);
                    bw.WriteInt16(Unk02);
                }

                /// <summary>
                /// Returns the two member values as a string.
                /// </summary>
                public override string ToString()
                {
                    return $"{Unk00} - {Unk02}";
                }
            }
        }
    }
}
