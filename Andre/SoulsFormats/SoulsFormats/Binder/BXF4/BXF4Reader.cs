using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using DotNext.IO.MemoryMappedFiles;

namespace SoulsFormats
{
    /// <summary>
    /// An on-demand reader for BXF4 containers.
    /// </summary>
    public class BXF4Reader : BinderReader, IBXF4
    {
        /// <summary>
        /// Unknown.
        /// </summary>
        public bool Unk04 { get; set; }

        /// <summary>
        /// Unknown.
        /// </summary>
        public bool Unk05 { get; set; }

        /// <summary>
        /// Whether to write strings in UTF-16.
        /// </summary>
        public bool Unicode { get; set; }

        /// <summary>
        /// Indicates the presence of a filename hash table.
        /// </summary>
        public byte Extended { get; set; }

        /// <summary>
        /// Reads a BXF4 from the given BHD and BDT paths.
        /// </summary>
        public BXF4Reader(string bhdPath, string bdtPath)
        {
            using MemoryMappedFile headerFile = MemoryMappedFile.CreateFromFile(bhdPath, FileMode.Open, null, 0, MemoryMappedFileAccess.Read);
            _mappedFile = MemoryMappedFile.CreateFromFile(bdtPath, FileMode.Open, null, 0, MemoryMappedFileAccess.Read);
            using IMappedMemoryOwner fsHeader = headerFile.CreateMemoryAccessor(0, 0, MemoryMappedFileAccess.Read);
            _mappedAccessor = _mappedFile.CreateMemoryAccessor(0, 0, MemoryMappedFileAccess.Read);
            var brHeader = new BinaryReaderEx(false, fsHeader.Memory);
            var brData = new BinaryReaderEx(false, _mappedAccessor.Memory);
            Read(brHeader, brData);
        }

        /// <summary>
        /// Reads a BXF4 from the given BHD path and BDT bytes.
        /// </summary>
        public BXF4Reader(string bhdPath, Memory<byte> bdtBytes)
        {
            using MemoryMappedFile headerFile = MemoryMappedFile.CreateFromFile(bhdPath, FileMode.Open, null, 0, MemoryMappedFileAccess.Read);
            using IMappedMemoryOwner fsHeader = headerFile.CreateMemoryAccessor(0, 0, MemoryMappedFileAccess.Read);
            var brHeader = new BinaryReaderEx(false, fsHeader.Memory);
            var brData = new BinaryReaderEx(false, bdtBytes);
            Read(brHeader, brData);
        }

        /// <summary>
        /// Reads a BXF4 from the given BHD bytes and BDT path.
        /// </summary>
        public BXF4Reader(Memory<byte> bhdBytes, string bdtPath)
        {
            _mappedFile = MemoryMappedFile.CreateFromFile(bdtPath, FileMode.Open, null, 0, MemoryMappedFileAccess.Read);
            _mappedAccessor = _mappedFile.CreateMemoryAccessor(0, 0, MemoryMappedFileAccess.Read);
            var brHeader = new BinaryReaderEx(false, bhdBytes);
            var brData = new BinaryReaderEx(false, _mappedAccessor.Memory);
            Read(brHeader, brData);
        }

        /// <summary>
        /// Reads a BXF4 from the given BHD and BDT bytes.
        /// </summary>
        public BXF4Reader(Memory<byte> bhdBytes, Memory<byte> bdtBytes)
        {
            var brHeader = new BinaryReaderEx(false, bhdBytes);
            var brData = new BinaryReaderEx(false, bdtBytes);
            Read(brHeader, brData);
        }

        private void Read(BinaryReaderEx brHeader, BinaryReaderEx brData)
        {
            BXF4.ReadBDFHeader(brData);
            Files = BXF4.ReadBHFHeader(this, brHeader);
            DataBR = brData;
        }
    }
}
