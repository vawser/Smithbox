using System.Collections.Generic;

namespace SoulsFormats.ACE3
{
    /// <summary>
    /// A file container used in A.C.E. 3.
    /// </summary>
    public class BND0 : SoulsFile<BND0>
    {
        /// <summary>
        /// The files contained in this BND0.
        /// </summary>
        public List<File> Files;

        /// <summary>
        /// Whether to use the small header format or not.
        /// </summary>
        public bool Lite;

        /// <summary>
        /// Unknown, non-lite format.
        /// </summary>
        public byte Flag1;

        /// <summary>
        /// Unknown, non-lite format.
        /// </summary>
        public byte Flag2;

        /// <summary>
        /// Returns true if the data appears to be a BND0.
        /// </summary>
        protected override bool Is(BinaryReaderEx br)
        {
            if (br.Length < 4)
                return false;

            string magic = br.GetASCII(0, 4);
            return magic == "BND\0";
        }

        /// <summary>
        /// Reads BND0 data from a BinaryReaderEx.
        /// </summary>
        protected override void Read(BinaryReaderEx br)
        {
            br.BigEndian = false;
            br.AssertASCII("BND\0");
            // File size in non-lite format
            Lite = br.GetInt32(0xC) == 0;
            int fileSize, fileCount;

            if (Lite)
            {
                fileSize = br.ReadInt32();
                fileCount = br.ReadInt32();
                br.AssertInt32(0);
            }
            else
            {
                br.AssertInt32(0xF7FF);
                br.AssertInt32(0xD3);
                fileSize = br.ReadInt32();
                fileCount = br.ReadInt32();
                br.AssertInt32(0);

                Flag1 = br.AssertByte([0, ]);
                Flag2 = br.AssertByte([0, 0x08]);
                br.AssertByte(3);
                br.AssertByte(0);

                br.AssertInt32(0);
                br.AssertInt32(0);
            }

            Files = new List<File>(fileCount);
            for (int i = 0; i < fileCount; i++)
            {
                Files.Add(new File(br, Lite));
            }
        }

        /// <summary>
        /// Writes BND0 data to a BinaryWriterEx.
        /// </summary>
        protected override void Write(BinaryWriterEx bw)
        {
            bw.BigEndian = false;
            bw.WriteASCII("BND\0");

            if (Lite)
            {
                bw.ReserveInt32("FileSize");
                bw.WriteInt32(Files.Count);
                bw.WriteInt32(0);
            }
            else
            {
                bw.WriteInt32(0xF7FF);
                bw.WriteInt32(0xD3);
                bw.ReserveInt32("FileSize");
                bw.WriteInt32(Files.Count);
                bw.WriteInt32(0);

                bw.WriteByte(Flag1);
                bw.WriteByte(Flag2);
                bw.WriteByte(3);
                bw.WriteByte(0);

                bw.WriteInt32(0);
                bw.WriteInt32(0);
            }

            for (int i = 0; i < Files.Count; i++)
            {
                Files[i].Write(bw, Lite, i);
            }

            for (int i = 0; i < Files.Count; i++)
            {
                File file = Files[i];
                bw.Pad(0x20);

                bw.FillInt32($"FileOffset{i}", (int)bw.Position);
                if (Lite)
                {
                    bw.WriteInt32(file.Bytes.Length + 4);
                    bw.WriteBytes(file.Bytes);
                }
                else
                {
                    bw.WriteBytes(file.Bytes);
                }
            }

            bw.FillInt32("FileSize", (int)bw.Position);
        }

        /// <summary>
        /// A file in a BND0 container.
        /// </summary>
        public class File
        {
            /// <summary>
            /// The ID number of this file.
            /// </summary>
            public int ID;

            /// <summary>
            /// The raw data of this file.
            /// </summary>
            public byte[] Bytes;

            internal File(BinaryReaderEx br, bool lite)
            {
                ID = br.ReadInt32();
                int offset = br.ReadInt32();

                int size;
                if (lite)
                {
                    // Size int is included in size
                    size = br.GetInt32(offset) - 4;
                    offset += 4;
                }
                else
                {
                    size = br.ReadInt32();
                }

                Bytes = br.GetBytes(offset, size);
            }

            internal void Write(BinaryWriterEx bw, bool lite, int index)
            {
                bw.WriteInt32(ID);
                bw.ReserveInt32($"FileOffset{index}");
                if (!lite)
                    bw.WriteInt32(Bytes.Length);
            }
        }
    }
}
