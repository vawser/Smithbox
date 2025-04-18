using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using DotNext.IO.MemoryMappedFiles;

namespace SoulsFormats
{
    /// <summary>
    /// A general-purpose file container used before DS2. Extension: .*bnd
    /// </summary>
    public class BND3 : MountedSoulsFile<BND3>, IBinder, IBND3
    {
        /// <summary>
        /// The files contained within this BND3.
        /// </summary>
        public List<BinderFile> Files { get; set; }

        /// <summary>
        /// A timestamp or version number, 8 characters maximum.
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Indicates the format of the BND3.
        /// </summary>
        public Binder.Format Format { get; set; }

        /// <summary>
        /// Write bytes in big-endian order for PS3/X360.
        /// </summary>
        public bool BigEndian { get; set; }

        /// <summary>
        /// Unknown; usually false.
        /// </summary>
        public bool BitBigEndian { get; set; }

        /// <summary>
        /// Unknown; always 0 except in DeS where it's occasionally 0x80000000 (probably a byte).
        /// </summary>
        public int Unk18 { get; set; }

        private IMappedMemoryOwner _mappedMemory = null;

        /// <summary>
        /// Creates an empty BND3 formatted for DS1.
        /// </summary>
        public BND3()
        {
            Files = new List<BinderFile>();
            Version = SFUtil.DateToBinderTimestamp(DateTime.Now);
            Format = Binder.Format.IDs | Binder.Format.Names1 | Binder.Format.Names2 | Binder.Format.Compression;
        }

        /// <summary>
        /// Checks whether the data appears to be a file of this format.
        /// </summary>
        protected override bool Is(BinaryReaderEx br)
        {
            if (br.Length < 4)
                return false;

            string magic = br.GetASCII(0, 4);
            return magic == "BND3";
        }

        /// <summary>
        /// Deserializes file data from a stream.
        /// </summary>
        protected override void Read(BinaryReaderEx br, IMappedMemoryOwner owner)
        {
            _mappedMemory = owner;
            List<BinderFileHeader> fileHeaders = ReadHeader(this, br);
            Files = new List<BinderFile>(fileHeaders.Count);
            foreach (BinderFileHeader fileHeader in fileHeaders)
                Files.Add(fileHeader.ReadFileData(br));
        }

        internal static List<BinderFileHeader> ReadHeader(IBND3 bnd, BinaryReaderEx br)
        {
            br.AssertASCII("BND3");
            bnd.Version = br.ReadFixStr(8);

            bnd.BitBigEndian = br.GetBoolean(0xE);

            bnd.Format = Binder.ReadFormat(br, bnd.BitBigEndian);
            bnd.BigEndian = br.ReadBoolean();
            br.AssertBoolean(bnd.BitBigEndian);
            br.AssertByte(0);

            br.BigEndian = bnd.BigEndian || Binder.ForceBigEndian(bnd.Format);

            int fileCount = br.ReadInt32();
            br.ReadInt32(); // End of file headers, not including padding before data
            bnd.Unk18 = br.AssertInt32([0, unchecked((int)0x80000000)]);
            br.AssertInt32(0);

            var fileHeaders = new List<BinderFileHeader>(fileCount);
            for (int i = 0; i < fileCount; i++)
                fileHeaders.Add(BinderFileHeader.ReadBinder3FileHeader(br, bnd.Format, bnd.BitBigEndian));

            return fileHeaders;
        }

        /// <summary>
        /// Serializes file data to a stream.
        /// </summary>
        protected override void Write(BinaryWriterEx bw)
        {
            var fileHeaders = new List<BinderFileHeader>(Files.Count);
            foreach (BinderFile file in Files)
                fileHeaders.Add(new BinderFileHeader(file));

            WriteHeader(this, bw, fileHeaders);
            for (int i = 0; i < Files.Count; i++)
                fileHeaders[i].WriteBinder3FileData(bw, bw, Format, i, Files[i].Bytes);
        }

        private static void WriteHeader(IBND3 bnd, BinaryWriterEx bw, List<BinderFileHeader> fileHeaders)
        {
            bw.BigEndian = bnd.BigEndian || Binder.ForceBigEndian(bnd.Format);

            bw.WriteASCII("BND3");
            bw.WriteFixStr(bnd.Version, 8);

            Binder.WriteFormat(bw, bnd.BigEndian, bnd.Format);
            bw.WriteBoolean(bnd.BigEndian);
            bw.WriteBoolean(bnd.BitBigEndian);
            bw.WriteByte(0);

            bw.WriteInt32(fileHeaders.Count);
            bw.ReserveInt32("FileHeadersEnd");
            bw.WriteInt32(bnd.Unk18);
            bw.WriteInt32(0);

            for (int i = 0; i < fileHeaders.Count; i++)
                fileHeaders[i].WriteBinder3FileHeader(bw, bnd.Format, bnd.BitBigEndian, i);

            for (int i = 0; i < fileHeaders.Count; i++)
                fileHeaders[i].WriteFileName(bw, bnd.Format, false, i);

            bw.FillInt32($"FileHeadersEnd", (int)bw.Position);
        }

        protected override void Dispose(bool disposing)
        {
            _mappedMemory?.Dispose();
            _mappedMemory = null;
        }
    }
}
