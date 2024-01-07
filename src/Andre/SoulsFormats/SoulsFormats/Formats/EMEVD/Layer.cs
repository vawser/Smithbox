namespace SoulsFormats
{
    public partial class EMEVD
    {
        private static class Layer
        {
            public static uint Read(BinaryReaderEx br)
            {
                br.AssertInt32(2);
                uint layer = br.ReadUInt32();
                br.AssertVarint(0);
                br.AssertVarint(-1);
                br.AssertVarint(1);
                return layer;
            }

            public static void Write(BinaryWriterEx bw, uint layer)
            {
                bw.WriteInt32(2);
                bw.WriteUInt32(layer);
                bw.WriteVarint(0);
                bw.WriteVarint(-1);
                bw.WriteVarint(1);
            }
        }
    }
}
