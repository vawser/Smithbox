namespace SoulsFormats.Formats.Morpheme.NSA
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class DequantizationInfo
    {
        public byte[] init; //3 bytes
        public byte[] factorIdx; //3 bytes

        public DequantizationInfo() { }

        public DequantizationInfo(BinaryReaderEx br)
        {
            init = br.ReadBytes(3);
            factorIdx = br.ReadBytes(3);
        }
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
