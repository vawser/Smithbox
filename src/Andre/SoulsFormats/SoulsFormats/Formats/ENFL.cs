using System.Collections.Generic;

namespace SoulsFormats
{
    /// <summary>
    /// A mysterious file format used in BB and DS3. Speculation: determines assets to load based on location in a map. Extension: .entryfilelist
    /// </summary>
    public class ENFL : SoulsFile<ENFL>
    {
        /// <summary>
        /// Unknown.
        /// </summary>
        public List<Struct1> Struct1s;

        /// <summary>
        /// Uknown.
        /// </summary>
        public List<Struct2> Struct2s;

        /// <summary>
        /// A list of file paths.
        /// </summary>
        public List<string> Strings;

        /// <summary>
        /// Checks whether the data appears to be a file of this format.
        /// </summary>
        protected override bool Is(BinaryReaderEx br)
        {
            if (br.Length < 4)
                return false;

            string magic = br.GetASCII(0, 4);
            return magic == "ENFL";
        }

        /// <summary>
        /// Deserializes file data from a stream.
        /// </summary>
        protected override void Read(BinaryReaderEx br)
        {
            br.BigEndian = false;

            br.AssertASCII("ENFL");
            // Probably 4 bytes
            br.AssertInt32(0x10415);
            int compressedSize = br.ReadInt32();
            int uncompressedSize = br.ReadInt32();
            byte[] data = SFUtil.ReadZlib(br, compressedSize);

            br = new BinaryReaderEx(false, data);
            br.AssertInt32(0);
            int unkCount1 = br.ReadInt32();
            int unkCount2 = br.ReadInt32();
            br.AssertInt32(0);

            Struct1s = new List<Struct1>(unkCount1);
            for (int i = 0; i < unkCount1; i++)
                Struct1s.Add(new Struct1(br));
            br.Pad(0x10);

            Struct2s = new List<Struct2>(unkCount2);
            for (int i = 0; i < unkCount2; i++)
                Struct2s.Add(new Struct2(br));
            br.Pad(0x10);

            br.AssertInt16(0);
            Strings = new List<string>(unkCount2);
            for (int i = 0; i < unkCount2; i++)
                Strings.Add(br.ReadUTF16());
        }

        /// <summary>
        /// Serializes file data to a stream.
        /// </summary>
        protected override void Write(BinaryWriterEx bw)
        {
            BinaryWriterEx bwData = new BinaryWriterEx(false);

            bwData.WriteInt32(0);
            bwData.WriteInt32(Struct1s.Count);
            bwData.WriteInt32(Struct2s.Count);
            bwData.WriteInt32(0);

            foreach (Struct1 struct1 in Struct1s)
                struct1.Write(bwData);
            bwData.Pad(0x10);

            foreach (Struct2 struct2 in Struct2s)
                struct2.Write(bwData);
            bwData.Pad(0x10);

            bwData.WriteInt16(0);
            foreach (string str in Strings)
                bwData.WriteUTF16(str, true);
            bwData.Pad(0x10);

            byte[] data = bwData.FinishBytes();

            bw.WriteASCII("ENFL");
            bw.WriteInt32(0x10415);
            bw.ReserveInt32("CompressedSize");
            bw.WriteInt32(data.Length);
            int compressedSize = SFUtil.WriteZlib(bw, 0xDA, data);
            bw.FillInt32("CompressedSize", compressedSize);
        }

        /// <summary>
        /// Some kind of weird iteration through the strings.
        /// </summary>
        public class Struct1
        {
            /// <summary>
            /// Increase of index to next Struct1. For instance, if Step is 0, the next Struct1 will have the same index. If Step is 2, the next Struct will have this Index + 2.
            /// </summary>
            public short Step;

            /// <summary>
            /// Almost certainly an index into the strings.
            /// </summary>
            public short Index;

            internal Struct1(BinaryReaderEx br)
            {
                Step = br.ReadInt16();
                Index = br.ReadInt16();
            }

            internal void Write(BinaryWriterEx bw)
            {
                bw.WriteInt16(Step);
                bw.WriteInt16(Index);
            }

            /// <summary>
            /// Returns the step and index.
            /// </summary>
            public override string ToString()
            {
                return $"0x{Step:X4} 0x{Index:X4}";
            }
        }

        /// <summary>
        /// Some data corresponding to each string. Possibly a hash?
        /// </summary>
        public class Struct2
        {
            /// <summary>
            /// Almost definitely not a single field. Appears to be 6 bytes of hash and a short in range 0-2, but I don't know.
            /// </summary>
            public long Unk1;

            internal Struct2(BinaryReaderEx br)
            {
                Unk1 = br.ReadInt64();
            }

            internal void Write(BinaryWriterEx bw)
            {
                bw.WriteInt64(Unk1);
            }

            /// <summary>
            /// Returns the entire struct as a number.
            /// </summary>
            public override string ToString()
            {
                return $"0x{Unk1:X16}";
            }
        }
    }
}
