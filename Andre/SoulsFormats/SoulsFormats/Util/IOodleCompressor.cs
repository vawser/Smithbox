using System;
namespace SoulsFormats;

public interface IOodleCompressor
{
        public byte[] Compress(Span<byte> source, Oodle.OodleLZ_Compressor compressor, Oodle.OodleLZ_CompressionLevel level);
        public Memory<byte> Decompress(Span<byte> source, long uncompressedSize);
}
