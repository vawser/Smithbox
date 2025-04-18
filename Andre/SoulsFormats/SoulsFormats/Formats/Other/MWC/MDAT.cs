using System;
using System.Collections.Generic;

namespace SoulsFormats.MWC
{
    /// <summary>
    /// Container for model-related files used in Metal Wolf Chaos. Extension: _m.dat
    /// </summary>
    public class MDAT : SoulsFile<MDAT>
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public int Unk1C;
        public byte[] Data1;
        public byte[] Data2;
        public byte[] Data3;
        public byte[] Data5;
        public byte[] Data6;

        protected override void Read(BinaryReaderEx br)
        {
            br.BigEndian = false;
            int fileSize = br.ReadInt32();
            int offset1 = br.ReadInt32();
            int offset2 = br.ReadInt32();
            int offset3 = br.ReadInt32();
            br.AssertInt32(0);
            int offset5 = br.ReadInt32();
            int offset6 = br.ReadInt32();
            Unk1C = br.ReadInt32();

            var offsets = new List<int> { fileSize, offset1, offset2, offset3, offset5, offset6 };
            offsets.Sort();

            if (offset1 != 0)
                Data1 = br.GetBytes(offset1, offsets[offsets.IndexOf(offset1) + 1] - offset1);
            if (offset2 != 0)
                Data2 = br.GetBytes(offset2, offsets[offsets.IndexOf(offset2) + 1] - offset2);
            if (offset3 != 0)
                Data3 = br.GetBytes(offset3, offsets[offsets.IndexOf(offset3) + 1] - offset3);
            if (offset5 != 0)
                Data5 = br.GetBytes(offset5, offsets[offsets.IndexOf(offset5) + 1] - offset5);
            if (offset6 != 0)
                Data6 = br.GetBytes(offset6, offsets[offsets.IndexOf(offset6) + 1] - offset6);
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
