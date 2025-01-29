using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulsFormats
{
    public enum BHV_TYPE
    {
        BHV_TYPE_BASENORMAL,
        BHV_TYPE_WEAPON,
        BHV_TYPE_W
    }

    /// <summary>
    /// A behavior configuration file introduced in AC6. Extension: .bhv
    /// </summary>
    public class BHV : SoulsFile<BHV>
    {
        /// <summary>
        /// The version of this BHV file.
        /// </summary>
        public short Version { get; set; }

        private BHV_TYPE FileType { get; set; }

        private short[] mystery { get; set; }

        private int MysterySize { get; set; }

        /// <summary>
        /// Deserializes file data from a stream.
        /// </summary>
        protected override void Read(BinaryReaderEx br, string filename)
        {
            // Get file type based on filename
            switch (filename)
            {
                case "basenormal.bhv":
                    FileType = BHV_TYPE.BHV_TYPE_BASENORMAL;
                    break;
                case "weapon.bhv":
                    FileType = BHV_TYPE.BHV_TYPE_WEAPON;
                    break;
                default:
                    FileType = BHV_TYPE.BHV_TYPE_W;
                    break;
            }

            // Get the mystery size based on file type
            switch (FileType)
            {
                case BHV_TYPE.BHV_TYPE_BASENORMAL:
                    MysterySize = 0xE0;
                    break;
                case BHV_TYPE.BHV_TYPE_WEAPON:
                    MysterySize = 0x0;
                    break;
                case BHV_TYPE.BHV_TYPE_W:
                    MysterySize = 0x40;
                    break;
                default:
                    MysterySize = -1;
                    break;
            }

            // Header
            Version = br.ReadInt16(); // version (5, 6, 10, 12)
            br.AssertInt16(3); // unk02
            int filesize = br.ReadInt32(); // fileSize
            br.AssertInt32(0); // unk08
            br.AssertInt32(0); // unk0c
            br.AssertInt32(0); // unk10
            br.AssertInt32(0); // unk14
            br.AssertInt32(0); // unk18
            br.AssertInt32(0); // unk1c

            int statesOffset = br.ReadInt32(); // statesOffset
            int stateCount = br.ReadInt32(); // stateCount
            int offsetB = br.ReadInt32(); // offsetB
            int countB = br.ReadInt32(); // countB
            int offsetC = br.ReadInt32(); // offsetC
            short countC = br.ReadInt16(); // countC
            short sizeC = br.ReadInt16(); // sizeC
            int offsetD = br.ReadInt32(); // offsetD
            int countD = br.ReadInt32(); // countD

            int sizeB = -1;

            switch (Version)
            {
                case 5:
                    sizeB = 0x40;
                    break;
                case 6:
                    sizeB = 0x50;
                    break;
                case 10:
                    sizeB = 0x60;
                    break;
                case 12:
                    sizeB = 0x60;
                    break;
                default:
                    throw new Exception("BHV: Invalid version during read.");
            }

            if (offsetC != offsetB + countB * sizeB)
            {
                throw new Exception("BHV: offsetC != offsetB + countB * sizeB");
            }
            if (offsetD != 0)
            {
                throw new Exception("BHV: offsetD != 0");
            }
            if (offsetD != offsetC + countC * sizeC)
            {
                throw new Exception("BHV: offsetD != offsetC + countC * sizeC");
            }

            // Mystery Size
            if (MysterySize != -1)
            {
                for(int i = 0; i < MysterySize / 2; i++)
                {
                    mystery[i] = br.ReadInt16();
                }
            }

            int DATA_START = 0x20;

            // States
            if(stateCount > 0)
            {

            }

            // TODO: finish

            //-----
            /*
            br.BigEndian = br.GetBoolean(0x10);

            br.AssertInt32(1);
            br.AssertInt32(0);
            int entryCount = br.ReadInt32();
            int stringsLength = br.ReadInt32();
            BigEndian = br.ReadBoolean();
            br.AssertByte(0);
            br.AssertByte(0);
            br.AssertByte(0);
            br.VarintLong = LongFormat = br.AssertInt32([0x1C, 0x28]) == 0x28; // Entry size
            br.AssertPattern(0x24, 0x00);

            long stringsStart = br.Position;
            br.Skip(stringsLength);
            Entries = new List<Entry>(entryCount);
            for (int i = 0; i < entryCount; i++)
                Entries.Add(new Entry(br, stringsStart));
            */
        }

        /// <summary>
        /// Serializes file data to a stream.
        /// </summary>
        protected override void Write(BinaryWriterEx bw)
        {
            //-----
            /*
            bw.BigEndian = BigEndian;
            bw.VarintLong = LongFormat;

            bw.WriteInt32(1);
            bw.WriteInt32(0);
            bw.WriteInt32(Entries.Count);
            bw.ReserveInt32("StringsLength");
            bw.WriteBoolean(BigEndian);
            bw.WriteByte(0);
            bw.WriteByte(0);
            bw.WriteByte(0);
            bw.WriteInt32(LongFormat ? 0x28 : 0x1C);
            bw.WritePattern(0x24, 0x00);

            long stringsStart = bw.Position;
            var stringOffsets = new List<long>(Entries.Count * 2);
            foreach (Entry entry in Entries)
            {
                long partNameOffset = bw.Position - stringsStart;
                stringOffsets.Add(partNameOffset);
                bw.WriteUTF16(entry.PartName, true);
                bw.PadRelative(stringsStart, 8); // This padding is not consistent, but it's the best I can do

                long materialNameOffset = bw.Position - stringsStart;
                stringOffsets.Add(materialNameOffset);
                bw.WriteUTF16(entry.MaterialName, true);
                bw.PadRelative(stringsStart, 8);
            }

            bw.FillInt32("StringsLength", (int)(bw.Position - stringsStart));
            for (int i = 0; i < Entries.Count; i++)
                Entries[i].Write(bw, stringOffsets[i * 2], stringOffsets[i * 2 + 1]);
            */
        }

        public class State
        {

        }

        public class Transition
        {

        }

        public class Condition
        {

        }

        public class StructABB
        {

        }

        public class StructB
        {

        }

        public class StructC
        {

        }

        public class StructD
        {

        }

        public class StructDA
        {

        }

        public class Strings
        {

        }
    }
}
