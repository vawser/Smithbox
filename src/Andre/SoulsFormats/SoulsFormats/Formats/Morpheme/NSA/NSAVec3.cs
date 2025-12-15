namespace SoulsFormats.Formats.Morpheme.NSA
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class NSAVec3
    {
        public short X;
        public short Y;
        public short Z;

        public NSAVec3() { }

        public NSAVec3(BinaryReaderEx br)
        {
            X = br.ReadInt16();
            Y = br.ReadInt16();
            Z = br.ReadInt16();
        }
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
