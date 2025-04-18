using System;
using System.Collections.Generic;

namespace SoulsFormats.Kuon
{
    /// <summary>
    /// Kuon's main archive ALL/ELL. Extension: .bnd
    /// </summary>
    public class DVDBND0 : SoulsFile<DVDBND0>
    {
        /// <summary>
        /// Files in this BND.
        /// </summary>
        public List<File> Files;

        /// <summary>
        /// Deserializes file data from a stream.
        /// </summary>
        protected override void Read(BinaryReaderEx br)
        {
            br.BigEndian = false;

            br.AssertASCII("BND\0");
            br.AssertInt32(0xCA);
            int fileSize = br.ReadInt32();
            int fileCount = br.ReadInt32();

            Files = new List<File>(fileCount);
            for (int i = 0; i < fileCount; i++)
                Files.Add(new File(br));
        }

        /// <summary>
        /// A file in a DVDBND0.
        /// </summary>
        public class File
        {
            /// <summary>
            /// ID of this file.
            /// </summary>
            public int ID;

            /// <summary>
            /// Name of this file.
            /// </summary>
            public string Name;

            /// <summary>
            /// File data.
            /// </summary>
            public byte[] Bytes;

            internal File(BinaryReaderEx br)
            {
                ID = br.ReadInt32();
                int dataOffset = br.ReadInt32();
                int dataSize = br.ReadInt32();
                int nameOffset = br.ReadInt32();

                Name = br.GetShiftJIS(nameOffset);
                Bytes = br.GetBytes(dataOffset, dataSize);
            }
        }
    }
}
