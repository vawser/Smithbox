using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using DotNext.IO.MemoryMappedFiles;

namespace SoulsFormats
{
    /// <summary>
    /// An on-demand reader for BXF3 containers.
    /// </summary>
    public class BXF3Reader : BinderReader, IBXF3
    {
        /// <summary>
        /// Reads a BXF3 from the given BHD and BDT paths.
        /// </summary>
        public BXF3Reader(string bhdPath, string bdtPath)
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
        /// Reads a BXF3 from the given BHD path and BDT bytes.
        /// </summary>
        public BXF3Reader(string bhdPath, Memory<byte> bdtBytes)
        {
            using MemoryMappedFile headerFile = MemoryMappedFile.CreateFromFile(bhdPath, FileMode.Open, null, 0, MemoryMappedFileAccess.Read);
            using IMappedMemoryOwner fsHeader = headerFile.CreateMemoryAccessor(0, 0, MemoryMappedFileAccess.Read);
            var brHeader = new BinaryReaderEx(false, fsHeader.Memory);
            var brData = new BinaryReaderEx(false, bdtBytes);
            Read(brHeader, brData);
        }

        /// <summary>
        /// Reads a BXF3 from the given BHD bytes and BDT path.
        /// </summary>
        public BXF3Reader(Memory<byte> bhdBytes, string bdtPath)
        {
            _mappedFile = MemoryMappedFile.CreateFromFile(bdtPath, FileMode.Open, null, 0, MemoryMappedFileAccess.Read);
            _mappedAccessor = _mappedFile.CreateMemoryAccessor(0, 0, MemoryMappedFileAccess.Read);
            var brHeader = new BinaryReaderEx(false, bhdBytes);
            var brData = new BinaryReaderEx(false, _mappedAccessor.Memory);
            Read(brHeader, brData);
        }

        /// <summary>
        /// Reads a BXF3 from the given BHD and BDT bytes.
        /// </summary>
        public BXF3Reader(Memory<byte> bhdBytes, Memory<byte> bdtBytes)
        {
            var brHeader = new BinaryReaderEx(false, bhdBytes);
            var brData = new BinaryReaderEx(false, bdtBytes);
            Read(brHeader, brData);
        }

        private void Read(BinaryReaderEx brHeader, BinaryReaderEx brData)
        {
            BXF3.ReadBDFHeader(brData);
            Files = BXF3.ReadBHFHeader(this, brHeader);
            DataBR = brData;
        }
    }
}
