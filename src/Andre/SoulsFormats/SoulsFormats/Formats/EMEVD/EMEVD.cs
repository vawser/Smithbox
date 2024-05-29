using System;
using System.Collections.Generic;
using System.Linq;

namespace SoulsFormats
{
    /// <summary>
    /// A list of game area logic events, each with a script. Extension: *.evd, *.emevd
    /// </summary>
    public partial class EMEVD : SoulsFile<EMEVD>
    {
        /// <summary>
        /// Determines the format the EMEVD will be written in.
        /// </summary>
        public Game Format { get; set; }

        /// <summary>
        /// List of events in this EMEVD.
        /// </summary>
        public List<Event> Events { get; set; }

        /// <summary>
        /// Offsets in the string data to linked file names used in Bloodborne and Dark Souls III.
        /// </summary>
        public List<long> LinkedFileOffsets { get; set; }

        /// <summary>
        /// Raw string data referenced by linked files and some instructions.
        /// </summary>
        public byte[] StringData { get; set; }

        /// <summary>
        /// Creates an empty EMEVD formatted for DS1.
        /// </summary>
        public EMEVD() : this(Game.DarkSouls1) { }

        /// <summary>
        /// Creates an empty EMEVD with the given format.
        /// </summary>
        public EMEVD(Game format)
        {
            Format = format;
            Events = new List<Event>();
            LinkedFileOffsets = new List<long>();
            StringData = new byte[0];
        }

        /// <summary>
        /// Imports event names from an EMELD file, overwriting existing names if specified.
        /// </summary>
        public void ImportEMELD(EMELD eld, bool overwrite = false)
        {
            var names = new Dictionary<long, string>(eld.Events.Count);
            foreach (EMELD.Event evt in eld.Events)
                names[evt.ID] = evt.Name;

            foreach (Event evt in Events)
            {
                if ((overwrite || evt.Name == null) && names.ContainsKey(evt.ID))
                    evt.Name = names[evt.ID];
            }
        }

        /// <summary>
        /// Exports event names to an EMELD file.
        /// </summary>
        public EMELD ExportEMELD()
        {
            var eld = new EMELD(Format);
            foreach (Event evt in Events)
            {
                if (evt.Name != null)
                    eld.Events.Add(new EMELD.Event(evt.ID, evt.Name));
            }
            return eld;
        }

        /// <summary>
        /// Checks whether the data appears to be a file of this format.
        /// </summary>
        protected override bool Is(BinaryReaderEx br)
        {
            if (br.Length < 4)
                return false;

            string magic = br.GetASCII(0, 4);
            return magic == "EVD\0";
        }

        /// <summary>
        /// Deserializes file data from a stream.
        /// </summary>
        protected override void Read(BinaryReaderEx br)
        {
            br.AssertASCII("EVD\0");
            bool bigEndian = br.ReadBoolean();
            bool is64Bit = br.AssertSByte([0, -1]) == -1;
            bool unk06 = br.ReadBoolean();
            bool unk07 = br.AssertSByte([0, -1]) == -1;
            br.BigEndian = bigEndian;
            br.VarintLong = is64Bit;

            int version = br.AssertInt32([0xCC, 0xCD]);
            br.ReadInt32(); // File size

            if (!bigEndian && !is64Bit && !unk06 && !unk07 && version == 0xCC)
                Format = Game.DarkSouls1;
            else if (bigEndian && !is64Bit && !unk06 && !unk07 && version == 0xCC)
                Format = Game.DarkSouls1BE;
            else if (!bigEndian && is64Bit && !unk06 && !unk07 && version == 0xCC)
                Format = Game.Bloodborne;
            else if (!bigEndian && is64Bit && unk06 && !unk07 && version == 0xCD)
                Format = Game.DarkSouls3;
            else if (!bigEndian && is64Bit && unk06 && unk07 && version == 0xCD)
                Format = Game.Sekiro;
            else
                throw new NotSupportedException($"Unknown EMEVD format: BigEndian={bigEndian} Is64Bit={is64Bit} Unicode={unk06} Unk07={unk07} Version=0x{version:X}");

            Offsets offsets;
            long eventCount = br.ReadVarint();
            offsets.Events = br.ReadVarint();
            br.ReadVarint(); // Instruction count
            offsets.Instructions = br.ReadVarint();
            br.AssertVarint(0); // Unknown struct count
            br.ReadVarint(); // Unknown struct offset
            br.ReadVarint(); // Layer count
            offsets.Layers = br.ReadVarint();
            br.ReadVarint(); // Parameter count
            offsets.Parameters = br.ReadVarint();
            long linkedFileCount = br.ReadVarint();
            offsets.LinkedFiles = br.ReadVarint();
            br.ReadVarint(); // Argument data length
            offsets.Arguments = br.ReadVarint();
            long stringsLength = br.ReadVarint();
            offsets.Strings = br.ReadVarint();
            if (!is64Bit)
                br.AssertInt32(0);

            br.Position = offsets.Events;
            Events = new List<Event>((int)eventCount);
            for (int i = 0; i < eventCount; i++)
                Events.Add(new Event(br, Format, offsets));

            br.Position = offsets.LinkedFiles;
            LinkedFileOffsets = new List<long>(br.ReadVarints((int)linkedFileCount));

            br.Position = offsets.Strings;
            StringData = br.ReadBytes((int)stringsLength);
        }

        /// <summary>
        /// Serializes file data to a stream.
        /// </summary>
        protected override void Write(BinaryWriterEx bw)
        {
            bool bigEndian = Format == Game.DarkSouls1BE;
            bool is64Bit = Format >= Game.Bloodborne;
            bool unk06 = Format >= Game.DarkSouls3;
            bool unk07 = Format >= Game.Sekiro;
            int version = Format < Game.DarkSouls3 ? 0xCC : 0xCD;

            var layers = new List<uint>();
            foreach (Event evt in Events)
            {
                foreach (Instruction inst in evt.Instructions)
                {
                    if (inst.Layer.HasValue && !layers.Contains(inst.Layer.Value))
                        layers.Add(inst.Layer.Value);
                }
            }

            bw.WriteASCII("EVD\0");
            bw.WriteBoolean(bigEndian);
            bw.WriteSByte((sbyte)(is64Bit ? -1 : 0));
            bw.WriteBoolean(unk06);
            bw.WriteSByte((sbyte)(unk07 ? -1 : 0));
            bw.BigEndian = bigEndian;
            bw.VarintLong = is64Bit;

            bw.WriteInt32(version);
            bw.ReserveInt32("FileSize");

            Offsets offsets = default;
            bw.WriteVarint(Events.Count);
            bw.ReserveVarint("EventsOffset");
            bw.WriteVarint(Events.Sum(e => e.Instructions.Count));
            bw.ReserveVarint("InstructionsOffset");
            bw.WriteVarint(0);
            bw.ReserveVarint("Offset3");
            bw.WriteVarint(layers.Count);
            bw.ReserveVarint("LayersOffset");
            bw.WriteVarint(Events.Sum(e => e.Parameters.Count));
            bw.ReserveVarint("ParametersOffset");
            bw.WriteVarint(LinkedFileOffsets.Count);
            bw.ReserveVarint("LinkedFilesOffset");
            bw.ReserveVarint("ArgumentsLength");
            bw.ReserveVarint("ArgumentsOffset");
            bw.WriteVarint(StringData.Length);
            bw.ReserveVarint("StringsOffset");
            if (!is64Bit)
                bw.WriteInt32(0);

            offsets.Events = bw.Position;
            bw.FillVarint("EventsOffset", bw.Position);
            for (int i = 0; i < Events.Count; i++)
                Events[i].Write(bw, Format, i);

            offsets.Instructions = bw.Position;
            bw.FillVarint("InstructionsOffset", bw.Position);
            for (int i = 0; i < Events.Count; i++)
                Events[i].WriteInstructions(bw, Format, offsets, i);

            bw.FillVarint("Offset3", bw.Position);

            offsets.Layers = bw.Position;
            bw.FillVarint("LayersOffset", bw.Position);
            var layerOffsets = new Dictionary<uint, long>(layers.Count);
            foreach (uint layer in layers)
            {
                layerOffsets[layer] = bw.Position - offsets.Layers;
                Layer.Write(bw, layer);
            }
            for (int i = 0; i < Events.Count; i++)
            {
                Event evt = Events[i];
                for (int j = 0; j < evt.Instructions.Count; j++)
                    evt.Instructions[j].FillLayerOffset(bw, Format, i, j, layerOffsets);
            }

            offsets.Arguments = bw.Position;
            bw.FillVarint("ArgumentsOffset", bw.Position);
            for (int i = 0; i < Events.Count; i++)
            {
                Event evt = Events[i];
                for (int j = 0; j < evt.Instructions.Count; j++)
                    evt.Instructions[j].WriteArgs(bw, Format, offsets, i, j);
            }
            if ((bw.Position - offsets.Arguments) % 0x10 > 0)
            {
                bw.WritePattern(0x10 - (int)(bw.Position - offsets.Arguments) % 0x10, 0x00);
            }
            bw.FillVarint("ArgumentsLength", bw.Position - offsets.Arguments);

            offsets.Parameters = bw.Position;
            bw.FillVarint("ParametersOffset", bw.Position);
            for (int i = 0; i < Events.Count; i++)
                Events[i].WriteParameters(bw, Format, offsets, i);

            offsets.LinkedFiles = bw.Position;
            bw.FillVarint("LinkedFilesOffset", bw.Position);
            foreach (long offset in LinkedFileOffsets)
                bw.WriteVarint((int)offset);

            offsets.Strings = bw.Position;
            bw.FillVarint("StringsOffset", bw.Position);
            bw.WriteBytes(StringData);

            bw.FillInt32("FileSize", (int)bw.Position);
        }

        /// <summary>
        /// Possible configurations for EMEVD formatting.
        /// </summary>
        public enum Game
        {
            /// <summary>
            /// Dark Souls 1 and 2 on PC.
            /// </summary>
            DarkSouls1,

            /// <summary>
            /// Dark Souls 1 and 2 on PS3 and Xbox 360.
            /// </summary>
            DarkSouls1BE,

            /// <summary>
            /// Bloodborne and SotFS on all platforms.
            /// </summary>
            Bloodborne,

            /// <summary>
            /// Dark Souls 3 on all platforms.
            /// </summary>
            DarkSouls3,

            /// <summary>
            /// Sekiro on all platforms.
            /// </summary>
            Sekiro
        }

        internal struct Offsets
        {
            public long Events;
            public long Instructions;
            public long Layers;
            public long Parameters;
            public long LinkedFiles;
            public long Arguments;
            public long Strings;
        }
    }
}
