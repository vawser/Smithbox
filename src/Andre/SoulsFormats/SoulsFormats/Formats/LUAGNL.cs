using System;
using System.Collections.Generic;

namespace SoulsFormats
{
    /// <summary>
    /// A list of global variable names for Lua scripts.
    /// </summary>
    public class LUAGNL : SoulsFile<LUAGNL>
    {
        /// <summary>
        /// If true, write as big endian.
        /// </summary>
        public bool BigEndian { get; set; }

        /// <summary>
        /// If true, write with 64-bit offsets and UTF-16 strings.
        /// </summary>
        public bool LongFormat { get; set; }

        /// <summary>
        /// Global variable names.
        /// </summary>
        public List<string> Globals { get; set; }

        /// <summary>
        /// Create an empty LUAGNL formatted for PC DS1.
        /// </summary>
        public LUAGNL() : this(false, false) { }

        /// <summary>
        /// Create an empty LUAGNL with the specified format.
        /// </summary>
        public LUAGNL(bool bigEndian, bool longFormat)
        {
            BigEndian = bigEndian;
            LongFormat = longFormat;
            Globals = new List<string>();
        }

        /// <summary>
        /// Deserializes file data from a stream.
        /// </summary>
        protected override void Read(BinaryReaderEx br)
        {
            BigEndian = br.GetInt16(0) == 0;
            br.BigEndian = BigEndian;
            LongFormat = br.GetInt32(BigEndian ? 0 : 4) == 0;

            Globals = new List<string>();
            long offset;
            do
            {
                offset = LongFormat ? br.ReadInt64() : br.ReadUInt32();
                if (offset != 0)
                    Globals.Add(LongFormat ? br.GetUTF16(offset) : br.GetShiftJIS(offset));
            }
            while (offset != 0);
        }

        /// <summary>
        /// Serializes file data to a stream.
        /// </summary>
        protected override void Write(BinaryWriterEx bw)
        {
            bw.BigEndian = BigEndian;
            for (int i = 0; i < Globals.Count; i++)
            {
                if (LongFormat)
                    bw.ReserveInt64($"Offset{i}");
                else
                    bw.ReserveUInt32($"Offset{i}");
            }

            if (LongFormat)
                bw.WriteInt64(0);
            else
                bw.WriteUInt32(0);

            for (int i = 0; i < Globals.Count; i++)
            {
                if (LongFormat)
                {
                    bw.FillInt64($"Offset{i}", bw.Position);
                    bw.WriteUTF16(Globals[i], true);
                }
                else
                {
                    bw.FillUInt32($"Offset{i}", (uint)bw.Position);
                    bw.WriteShiftJIS(Globals[i], true);
                }
            }
            bw.Pad(0x10);
        }
    }
}
