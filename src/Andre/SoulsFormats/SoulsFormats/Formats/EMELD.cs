using System;
using System.Collections.Generic;

namespace SoulsFormats
{
    /// <summary>
    /// A companion file to EMEVD that assigns names to different events.
    /// </summary>
    public class EMELD : SoulsFile<EMELD>
    {
        /// <summary>
        /// Determines the format the EMELD will be written in.
        /// </summary>
        public EMEVD.Game Format { get; set; }

        /// <summary>
        /// Events corresponding to those in the EMEVD.
        /// </summary>
        public List<Event> Events { get; set; }

        /// <summary>
        /// Creates an empty EMELD formatted for DS1.
        /// </summary>
        public EMELD() : this(EMEVD.Game.DarkSouls1) { }

        /// <summary>
        /// Creates an empty EMELD with the given format.
        /// </summary>
        public EMELD(EMEVD.Game format)
        {
            Format = format;
            Events = new List<Event>();
        }

        /// <summary>
        /// Checks whether the data appears to be a file of this format.
        /// </summary>
        protected override bool Is(BinaryReaderEx br)
        {
            if (br.Length < 4)
                return false;

            string magic = br.GetASCII(0, 4);
            return magic == "ELD\0";
        }

        /// <summary>
        /// Deserializes file data from a stream.
        /// </summary>
        protected override void Read(BinaryReaderEx br)
        {
            br.AssertASCII("ELD\0");
            bool bigEndian = br.ReadBoolean();
            bool is64Bit = br.AssertSByte([0, -1]) == -1;
            br.AssertByte(0);
            br.AssertByte(0);
            br.BigEndian = bigEndian;
            br.VarintLong = is64Bit;

            br.AssertInt16(0x65);
            br.AssertInt16(0xCC);
            br.ReadInt32(); // File size

            if (!bigEndian && !is64Bit)
                Format = EMEVD.Game.DarkSouls1;
            else if (bigEndian && !is64Bit)
                Format = EMEVD.Game.DarkSouls1BE;
            else if (!bigEndian && is64Bit)
                Format = EMEVD.Game.Bloodborne;
            else
                throw new NotSupportedException($"Unknown EMELD format: BigEndian={bigEndian} Is64Bit={is64Bit}");

            long eventCount = br.ReadVarint();
            long eventsOffset = br.ReadVarint();
            br.AssertVarint(0); // Unused count 2
            br.ReadVarint(); // Unused offset 2
            br.AssertVarint(0); // Unused count 3
            br.ReadVarint(); // Unused offset 3
            br.ReadVarint(); // Strings length
            long stringsOffset = br.ReadVarint();
            if (!is64Bit)
            {
                br.AssertInt32(0);
                br.AssertInt32(0);
            }

            br.Position = eventsOffset;
            Events = new List<Event>((int)eventCount);
            for (int i = 0; i < eventCount; i++)
                Events.Add(new Event(br, Format, stringsOffset));
        }

        /// <summary>
        /// Serializes file data to a stream.
        /// </summary>
        protected override void Write(BinaryWriterEx bw)
        {
            bool bigEndian = Format == EMEVD.Game.DarkSouls1BE;
            bool is64Bit = Format >= EMEVD.Game.Bloodborne;

            bw.WriteASCII("ELD\0");
            bw.WriteBoolean(bigEndian);
            bw.WriteSByte((sbyte)(is64Bit ? -1 : 0));
            bw.WriteByte(0);
            bw.WriteByte(0);
            bw.BigEndian = bigEndian;
            bw.VarintLong = is64Bit;

            bw.WriteInt16(0x65);
            bw.WriteInt16(0xCC);
            bw.ReserveInt32("FileSize");

            bw.WriteVarint(Events.Count);
            bw.ReserveVarint("EventsOffset");
            bw.WriteVarint(0);
            bw.ReserveVarint("Offset2");
            bw.WriteVarint(0);
            bw.ReserveVarint("Offset3");
            bw.ReserveVarint("StringsLength");
            bw.ReserveVarint("StringsOffset");
            if (!is64Bit)
            {
                bw.WriteInt32(0);
                bw.WriteInt32(0);
            }

            bw.FillVarint("EventsOffset", bw.Position);
            for (int i = 0; i < Events.Count; i++)
                Events[i].Write(bw, Format, i);

            bw.FillVarint("Offset2", bw.Position);
            bw.FillVarint("Offset3", bw.Position);

            long stringsOffset = bw.Position;
            bw.FillVarint("StringsOffset", bw.Position);
            for (int i = 0; i < Events.Count; i++)
                Events[i].WriteName(bw, i, stringsOffset);
            if ((bw.Position - stringsOffset) % 0x10 > 0)
                bw.WritePattern(0x10 - (int)(bw.Position - stringsOffset) % 0x10, 0x00);
            bw.FillVarint("StringsLength", bw.Position - stringsOffset);

            bw.FillInt32("FileSize", (int)bw.Position);
        }

        /// <summary>
        /// Assigns a name to a certain event ID.
        /// </summary>
        public class Event
        {
            /// <summary>
            /// ID of the event.
            /// </summary>
            public long ID { get; set; }

            /// <summary>
            /// Name of the event.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Creates an Event with the given values.
            /// </summary>
            public Event(long id, string name)
            {
                ID = id;
                Name = name;
            }

            internal Event(BinaryReaderEx br, EMEVD.Game format, long stringsOffset)
            {
                ID = br.ReadVarint();
                long nameOffset = br.ReadVarint();
                if (format < EMEVD.Game.Bloodborne)
                    br.AssertInt32(0);

                Name = br.GetUTF16(stringsOffset + nameOffset);
            }

            internal void Write(BinaryWriterEx bw, EMEVD.Game format, int index)
            {
                bw.WriteVarint(ID);
                bw.ReserveVarint($"Event{index}NameOffset");
                if (format < EMEVD.Game.Bloodborne)
                    bw.WriteInt32(0);
            }

            internal void WriteName(BinaryWriterEx bw, int index, long stringsOffset)
            {
                bw.FillVarint($"Event{index}NameOffset", bw.Position - stringsOffset);
                bw.WriteUTF16(Name, true);
            }
        }
    }
}
