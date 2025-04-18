using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using DotNext.IO.MemoryMappedFiles;

namespace SoulsFormats
{
    /// <summary>
    /// An on-demand reader for BND3 containers.
    /// </summary>
    public class BND3Reader : BinderReader, IBND3
    {
        /// <summary>
        /// Unknown; always 0 except in DeS where it's occasionally 0x80000000 (probably a byte).
        /// </summary>
        public int Unk18 { get; set; }

        /// <summary>
        /// Type of compression used, if any.
        /// </summary>
        public DCX.Type Compression { get; set; }

        /// <summary>
        /// Reads a BND3 from the given path, decompressing if necessary.
        /// </summary>
        public BND3Reader(string path)
        {
            _mappedFile = MemoryMappedFile.CreateFromFile(path, FileMode.Open, null, 0, MemoryMappedFileAccess.Read);
            _mappedAccessor = _mappedFile.CreateMemoryAccessor(0, 0, MemoryMappedFileAccess.Read);
            var br = new BinaryReaderEx(false, _mappedAccessor.Memory);
            Read(br);
        }

        /// <summary>
        /// Reads a BND3 from the given bytes, decompressing if necessary.
        /// </summary>
        public BND3Reader(Memory<byte> bytes)
        {
            var br = new BinaryReaderEx(false, bytes);
            Read(br);
        }

        private void Read(BinaryReaderEx br)
        {
            br = SFUtil.GetDecompressedBR(br, out DCX.Type compression);
            Compression = compression;
            Files = BND3.ReadHeader(this, br);
            DataBR = br;
        }
    }
}
