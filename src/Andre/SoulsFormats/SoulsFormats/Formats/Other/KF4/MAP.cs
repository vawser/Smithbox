using System.Collections.Generic;

namespace SoulsFormats.KF4
{
    /// <summary>
    /// A map asset container used in King's Field IV. Extension: .map
    /// </summary>
    public class MAP : SoulsFile<MAP>
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public List<Struct4> Struct4s { get; set; }

        protected override void Read(BinaryReaderEx br)
        {
            br.ReadInt32(); // File size
            int offset1 = br.ReadInt32();
            int offset2 = br.ReadInt32();
            int offset3 = br.ReadInt32();
            int offset4 = br.ReadInt32();
            int offset5 = br.ReadInt32();
            int offset6 = br.ReadInt32();
            br.AssertInt32(0);
            int offset8 = br.ReadInt32();
            int offset9 = br.ReadInt32();
            int offset10 = br.ReadInt32();
            short count1 = br.ReadInt16();
            short count2 = br.ReadInt16();
            short count3 = br.ReadInt16();
            short count4 = br.ReadInt16();
            short count5 = br.ReadInt16();
            short count6 = br.ReadInt16();
            br.AssertInt16(0);
            short count8 = br.ReadInt16();
            short count9 = br.ReadInt16();
            short count10 = br.ReadInt16();

            br.Position = offset4;
            Struct4s = new List<Struct4>(count4);
            for (int i = 0; i < count4; i++)
                Struct4s.Add(new Struct4(br));
        }

        public class Struct4
        {
            public OM2 Om2 { get; set; }

            internal Struct4(BinaryReaderEx br)
            {
                byte[] om2Bytes = br.ReadBytes(br.GetInt32(br.Position));
                byte[] tx2Bytes = br.ReadBytes(br.GetInt32(br.Position));

                Om2 = OM2.Read(om2Bytes);
            }
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
