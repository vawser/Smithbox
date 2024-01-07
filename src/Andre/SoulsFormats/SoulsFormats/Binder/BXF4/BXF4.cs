using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using DotNext.IO.MemoryMappedFiles;

namespace SoulsFormats
{
    /// <summary>
    /// A general-purpose headered file container used in DS2, DS3, and BB. Extensions: .*bhd (header) and .*bdt (data)
    /// </summary>
    public class BXF4 : MountedSoulsFile<BXF4>, IBinder, IBXF4
    {
        #region Public Is
        /// <summary>
        /// Returns true if the bytes appear to be a BXF3 header file.
        /// </summary>
        public static bool IsBHD(Memory<byte> bytes)
        {
            BinaryReaderEx br = new BinaryReaderEx(false, bytes);
            return IsBHD(SFUtil.GetDecompressedBR(br, out _));
        }

        /// <summary>
        /// Returns true if the file appears to be a BXF3 header file.
        /// </summary>
        public static bool IsBHD(string path)
        {
            using var file = MemoryMappedFile.CreateFromFile(path, FileMode.Open, null, 0, MemoryMappedFileAccess.Read);
            using var accessor = file.CreateMemoryAccessor(0, 0, MemoryMappedFileAccess.Read);
            BinaryReaderEx br = new BinaryReaderEx(false, accessor.Memory);
            return IsBHD(SFUtil.GetDecompressedBR(br, out _));
        }

        /// <summary>
        /// Returns true if the file appears to be a BXF3 data file.
        /// </summary>
        public static bool IsBDT(Memory<byte> bytes)
        {
            BinaryReaderEx br = new BinaryReaderEx(false, bytes);
            return IsBDT(SFUtil.GetDecompressedBR(br, out _));
        }

        /// <summary>
        /// Returns true if the file appears to be a BXF3 data file.
        /// </summary>
        public static bool IsBDT(string path)
        {
            using var file = MemoryMappedFile.CreateFromFile(path, FileMode.Open, null, 0, MemoryMappedFileAccess.Read);
            using var accessor = file.CreateMemoryAccessor(0, 0, MemoryMappedFileAccess.Read);
            BinaryReaderEx br = new BinaryReaderEx(false, accessor.Memory);
            return IsBDT(SFUtil.GetDecompressedBR(br, out _));
        }
        #endregion

        #region Public Read
        /// <summary>
        /// Reads two arrays of bytes as the BHD and BDT.
        /// </summary>
        public static BXF4 Read(Memory<byte> bhdBytes, Memory<byte> bdtBytes)
        {
            BinaryReaderEx bhdReader = new BinaryReaderEx(false, bhdBytes);
            BinaryReaderEx bdtReader = new BinaryReaderEx(false, bdtBytes);
            return new BXF4(bhdReader, bdtReader);
        }

        /// <summary>
        /// Reads an array of bytes as the BHD and a file as the BDT.
        /// </summary>
        public static BXF4 Read(Memory<byte> bhdBytes, string bdtPath)
        {
            using MemoryMappedFile dataFile = MemoryMappedFile.CreateFromFile(bdtPath, FileMode.Open, null, 0, MemoryMappedFileAccess.Read);
            IMappedMemoryOwner fsData = dataFile.CreateMemoryAccessor(0, 0, MemoryMappedFileAccess.Read);
            BinaryReaderEx bhdReader = new BinaryReaderEx(false, bhdBytes);
            BinaryReaderEx bdtReader = new BinaryReaderEx(false, fsData.Memory);
            return new BXF4(bhdReader, bdtReader) { _mappedMemory2 = fsData };
        }

        /// <summary>
        /// Reads a file as the BHD and an array of bytes as the BDT.
        /// </summary>
        public static BXF4 Read(string bhdPath, Memory<byte> bdtBytes)
        {
            using MemoryMappedFile headerFile = MemoryMappedFile.CreateFromFile(bhdPath, FileMode.Open, null, 0, MemoryMappedFileAccess.Read);
            IMappedMemoryOwner fsHeader = headerFile.CreateMemoryAccessor(0, 0, MemoryMappedFileAccess.Read);
            BinaryReaderEx bhdReader = new BinaryReaderEx(false, fsHeader.Memory);
            BinaryReaderEx bdtReader = new BinaryReaderEx(false, bdtBytes);
            return new BXF4(bhdReader, bdtReader) { _mappedMemory1 = fsHeader };
        }

        /// <summary>
        /// Reads two files as the BHD and BDT.
        /// </summary>
        public static BXF4 Read(string bhdPath, string bdtPath)
        {
            using MemoryMappedFile headerFile = MemoryMappedFile.CreateFromFile(bhdPath, FileMode.Open, null, 0, MemoryMappedFileAccess.Read),
                dataFile = MemoryMappedFile.CreateFromFile(bdtPath, FileMode.Open, null, 0, MemoryMappedFileAccess.Read);
            IMappedMemoryOwner fsHeader = headerFile.CreateMemoryAccessor(0, 0, MemoryMappedFileAccess.Read),
                fsData = dataFile.CreateMemoryAccessor(0, 0, MemoryMappedFileAccess.Read);
            BinaryReaderEx bhdReader = new BinaryReaderEx(false, fsHeader.Memory);
            BinaryReaderEx bdtReader = new BinaryReaderEx(false, fsData.Memory);
            return new BXF4(bhdReader, bdtReader) { _mappedMemory1 = fsHeader, _mappedMemory2 = fsData };
        }
        #endregion

        #region Public Write
        /// <summary>
        /// Writes the BHD and BDT as two arrays of bytes.
        /// </summary>
        public void Write(out byte[] bhdBytes, out byte[] bdtBytes)
        {
            BinaryWriterEx bhdWriter = new BinaryWriterEx(false);
            BinaryWriterEx bdtWriter = new BinaryWriterEx(false);
            Write(bhdWriter, bdtWriter);
            bhdBytes = bhdWriter.FinishBytes();
            bdtBytes = bdtWriter.FinishBytes();
        }

        /// <summary>
        /// Writes the BHD as an array of bytes and the BDT as a file.
        /// </summary>
        public void Write(out byte[] bhdBytes, string bdtPath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(bdtPath));
            using (FileStream bdtStream = System.IO.File.Create(bdtPath))
            {
                BinaryWriterEx bhdWriter = new BinaryWriterEx(false);
                BinaryWriterEx bdtWriter = new BinaryWriterEx(false, bdtStream);
                Write(bhdWriter, bdtWriter);
                bdtWriter.Finish();
                bhdBytes = bhdWriter.FinishBytes();
            }
        }

        /// <summary>
        /// Writes the BHD as a file and the BDT as an array of bytes.
        /// </summary>
        public void Write(string bhdPath, out byte[] bdtBytes)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(bhdPath));
            using (FileStream bhdStream = System.IO.File.Create(bhdPath))
            {
                BinaryWriterEx bhdWriter = new BinaryWriterEx(false, bhdStream);
                BinaryWriterEx bdtWriter = new BinaryWriterEx(false);
                Write(bhdWriter, bdtWriter);
                bhdWriter.Finish();
                bdtBytes = bdtWriter.FinishBytes();
            }
        }

        /// <summary>
        /// Writes the BHD and BDT as two files.
        /// </summary>
        public void Write(string bhdPath, string bdtPath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(bhdPath));
            Directory.CreateDirectory(Path.GetDirectoryName(bdtPath));
            using (FileStream bhdStream = System.IO.File.Create(bhdPath))
            using (FileStream bdtStream = System.IO.File.Create(bdtPath))
            {
                BinaryWriterEx bhdWriter = new BinaryWriterEx(false, bhdStream);
                BinaryWriterEx bdtWriter = new BinaryWriterEx(false, bdtStream);
                Write(bhdWriter, bdtWriter);
                bhdWriter.Finish();
                bdtWriter.Finish();
            }
        }
        #endregion

        /// <summary>
        /// The files contained within this BXF4.
        /// </summary>
        public List<BinderFile> Files { get; set; }

        /// <summary>
        /// A timestamp or version number, 8 characters maximum.
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Indicates the format of the BXF4.
        /// </summary>
        public Binder.Format Format { get; set; }

        /// <summary>
        /// Unknown.
        /// </summary>
        public bool Unk04 { get; set; }

        /// <summary>
        /// Unknown.
        /// </summary>
        public bool Unk05 { get; set; }

        /// <summary>
        /// Whether to use big-endian byte ordering.
        /// </summary>
        public bool BigEndian { get; set; }

        /// <summary>
        /// Controls ordering of flag bits.
        /// </summary>
        public bool BitBigEndian { get; set; }

        /// <summary>
        /// Whether to write strings in UTF-16.
        /// </summary>
        public bool Unicode { get; set; }

        /// <summary>
        /// Indicates the presence of a filename hash table.
        /// </summary>
        public byte Extended { get; set; }

        /// <summary>
        /// Creates an empty BXF4 formatted for DS3.
        /// </summary>

        private IMappedMemoryOwner _mappedMemory => throw new NotSupportedException();
        private IMappedMemoryOwner _mappedMemory1 = null;
        private IMappedMemoryOwner _mappedMemory2 = null;

        public BXF4()
        {
            Files = new List<BinderFile>();
            Version = SFUtil.DateToBinderTimestamp(DateTime.Now);
            Unicode = true;
            Format = Binder.Format.IDs | Binder.Format.Names1 | Binder.Format.Names2 | Binder.Format.Compression;
            Extended = 4;
        }

        private static bool IsBHD(BinaryReaderEx br)
        {
            if (br.Length < 4)
                return false;

            string magic = br.GetASCII(0, 4);
            return magic == "BHF4";
        }

        private static bool IsBDT(BinaryReaderEx br)
        {
            if (br.Length < 4)
                return false;

            string magic = br.GetASCII(0, 4);
            return magic == "BDF4";
        }

        private BXF4(BinaryReaderEx bhdReader, BinaryReaderEx bdtReader)
        {
            ReadBDFHeader(bdtReader);
            List<BinderFileHeader> fileHeaders = ReadBHFHeader(this, bhdReader);
            Files = new List<BinderFile>(fileHeaders.Count);
            foreach (BinderFileHeader fileHeader in fileHeaders)
                Files.Add(fileHeader.ReadFileData(bdtReader));
        }

        // I am very tempted to preserve these since they don't always match the BHF,
        // but it makes the API messy and they don't actually do anything.
        internal static void ReadBDFHeader(BinaryReaderEx br)
        {
            br.AssertASCII("BDF4");
            br.ReadBoolean(); // Unk04
            br.ReadBoolean(); // Unk05
            br.AssertByte(0);
            br.AssertByte(0);
            br.AssertByte(0);
            br.BigEndian = br.ReadBoolean();
            br.ReadBoolean(); // BitBigEndian
            br.AssertByte(0);
            br.AssertInt32(0);
            br.AssertInt64([0x30, 0x40]); // Header size, pretty sure 0x40 is just a mistake
            br.ReadFixStr(8); // Version
            br.AssertInt64(0);
            br.AssertInt64(0);
        }

        internal static List<BinderFileHeader> ReadBHFHeader(IBXF4 bxf, BinaryReaderEx br)
        {
            br.AssertASCII("BHF4");

            bxf.Unk04 = br.ReadBoolean();
            bxf.Unk05 = br.ReadBoolean();
            br.AssertByte(0);
            br.AssertByte(0);

            br.AssertByte(0);
            bxf.BigEndian = br.ReadBoolean();
            bxf.BitBigEndian = !br.ReadBoolean();
            br.AssertByte(0);

            br.BigEndian = bxf.BigEndian;

            int fileCount = br.ReadInt32();
            br.AssertInt64(0x40); // Header size
            bxf.Version = br.ReadFixStr(8);
            long fileHeaderSize = br.ReadInt64();
            br.AssertInt64(0);

            bxf.Unicode = br.ReadBoolean();
            bxf.Format = Binder.ReadFormat(br, bxf.BitBigEndian);
            bxf.Extended = br.AssertByte([0, 4]);
            br.AssertByte(0);

            if (fileHeaderSize != Binder.GetBND4FileHeaderSize(bxf.Format))
                throw new FormatException($"File header size for format {bxf.Format} is expected to be 0x{Binder.GetBND4FileHeaderSize(bxf.Format):X}, but was 0x{fileHeaderSize:X}");

            br.AssertInt32(0);

            if (bxf.Extended == 4)
            {
                long hashGroupsOffset = br.ReadInt64();
                br.StepIn(hashGroupsOffset);
                BinderHashTable.Assert(br);
                br.StepOut();
            }
            else
            {
                br.AssertInt64(0);
            }

            var fileHeaders = new List<BinderFileHeader>(fileCount);
            for (int i = 0; i < fileCount; i++)
                fileHeaders.Add(BinderFileHeader.ReadBinder4FileHeader(br, bxf.Format, bxf.BitBigEndian, bxf.Unicode));

            return fileHeaders;
        }

        private void Write(BinaryWriterEx bhdWriter, BinaryWriterEx bdtWriter)
        {
            var fileHeaders = new List<BinderFileHeader>(Files.Count);
            foreach (BinderFile file in Files)
                fileHeaders.Add(new BinderFileHeader(file));

            WriteBDFHeader(this, bdtWriter);
            WriteBHFHeader(this, bhdWriter, fileHeaders);
            for (int i = 0; i < Files.Count; i++)
                fileHeaders[i].WriteBinder4FileData(bhdWriter, bdtWriter, Format, i, Files[i].Bytes);
        }

        internal static void WriteBDFHeader(IBXF4 bxf, BinaryWriterEx bw)
        {
            bw.BigEndian = bxf.BigEndian;
            bw.WriteASCII("BDF4");
            bw.WriteBoolean(bxf.Unk04);
            bw.WriteBoolean(bxf.Unk05);
            bw.WriteByte(0);
            bw.WriteByte(0);
            bw.WriteByte(0);
            bw.WriteBoolean(bxf.BigEndian);
            bw.WriteBoolean(!bxf.BitBigEndian);
            bw.WriteByte(0);
            bw.WriteInt32(0);
            bw.WriteInt64(0x30);
            bw.WriteFixStr(bxf.Version, 8);
            bw.WriteInt64(0);
            bw.WriteInt64(0);
        }

        internal static void WriteBHFHeader(IBXF4 bxf, BinaryWriterEx bw, List<BinderFileHeader> fileHeaders)
        {
            bw.BigEndian = bxf.BigEndian;

            bw.WriteASCII("BHF4");

            bw.WriteBoolean(bxf.Unk04);
            bw.WriteBoolean(bxf.Unk05);
            bw.WriteByte(0);
            bw.WriteByte(0);

            bw.WriteByte(0);
            bw.WriteBoolean(bxf.BigEndian);
            bw.WriteBoolean(!bxf.BitBigEndian);
            bw.WriteByte(0);

            bw.WriteInt32(fileHeaders.Count);
            bw.WriteInt64(0x40);
            bw.WriteFixStr(bxf.Version, 8);
            bw.WriteInt64(Binder.GetBND4FileHeaderSize(bxf.Format));
            bw.WriteInt64(0);

            bw.WriteBoolean(bxf.Unicode);
            Binder.WriteFormat(bw, bxf.BitBigEndian, bxf.Format);
            bw.WriteByte(bxf.Extended);
            bw.WriteByte(0);

            bw.WriteInt32(0);
            bw.ReserveInt64("HashTableOffset");

            for (int i = 0; i < fileHeaders.Count; i++)
                fileHeaders[i].WriteBinder4FileHeader(bw, bxf.Format, bxf.BitBigEndian, i);

            for (int i = 0; i < fileHeaders.Count; i++)
                fileHeaders[i].WriteFileName(bw, bxf.Format, bxf.Unicode, i);

            if (bxf.Extended == 4)
            {
                bw.Pad(0x8);
                bw.FillInt64("HashTableOffset", bw.Position);
                BinderHashTable.Write(bw, fileHeaders);
            }
            else
            {
                bw.FillInt64("HashTableOffset", 0);
            }
        }

        protected override void Dispose(bool disposing)
        {
            _mappedMemory1?.Dispose();
            _mappedMemory1 = null;
            _mappedMemory2?.Dispose();
            _mappedMemory2 = null;
        }
    }
}
