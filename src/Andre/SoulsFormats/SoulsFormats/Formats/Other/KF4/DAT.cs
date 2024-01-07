using System;
using System.Collections.Generic;

namespace SoulsFormats.KF4
{
    /// <summary>
    /// Specifically KF4.DAT, the main archive.
    /// </summary>
    public class DAT : SoulsFile<DAT>
    {
        /// <summary>
        /// Files in the archive.
        /// </summary>
        public List<File> Files;

        /// <summary>
        /// Deserializes file data from a stream.
        /// </summary>
        protected override void Read(BinaryReaderEx br)
        {
            br.BigEndian = false;

            br.AssertByte(0x00);
            br.AssertByte(0x80);
            br.AssertByte(0x04);
            br.AssertByte(0x1E);

            int fileCount = br.ReadInt32();

            for (int i = 0; i < 0x38; i++)
                br.AssertByte(0);

            Files = new List<File>(fileCount);
            for (int i = 0; i < fileCount; i++)
                Files.Add(new File(br));
        }

        /// <summary>
        /// A file in a DAT archive.
        /// </summary>
        public class File
        {
            /// <summary>
            /// The path of the file.
            /// </summary>
            public string Name;

            /// <summary>
            /// The file's data.
            /// </summary>
            public byte[] Bytes;

            internal File(BinaryReaderEx br)
            {
                Name = br.ReadFixStr(0x34);
                int size = br.ReadInt32();
                int paddedSize = br.ReadInt32();
                int offset = br.ReadInt32();

                Bytes = br.GetBytes(offset, size);
            }
        }
    }
}
