using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Numerics;

namespace SoulsFormats
{
    public partial class MSB2
    {
        internal enum EventType : byte
        {
            Light = 1,
            Shadow = 2,
            Fog = 3,
            BGColor = 4,
            MapOffset = 5,
            Warp = 6,
            CheapMode = 7,
        }

        /// <summary>
        /// Abstract entities that control map properties or behaviors.
        /// </summary>
        public class EventParam : Param<Event>, IMsbParam<IMsbEvent>
        {
            internal override int Version => 5;
            internal override string Name => "EVENT_PARAM_ST";

            /// <summary>
            /// Unknown if these do anything.
            /// </summary>
            public List<Event.Light> Lights { get; set; }

            /// <summary>
            /// Unknown if these do anything.
            /// </summary>
            public List<Event.Shadow> Shadows { get; set; }

            /// <summary>
            /// Unknown if these do anything.
            /// </summary>
            public List<Event.Fog> Fogs { get; set; }

            /// <summary>
            /// Sets the background color when no models are in the way. Should only be one per map.
            /// </summary>
            public List<Event.BGColor> BGColors { get; set; }

            /// <summary>
            /// Sets the origin of the map; already factored into MSB positions, but affects BTL. Should only be one per map.
            /// </summary>
            public List<Event.MapOffset> MapOffsets { get; set; }

            /// <summary>
            /// Unknown exactly what this is for.
            /// </summary>
            public List<Event.Warp> Warps { get; set; }

            /// <summary>
            /// Unknown if these do anything.
            /// </summary>
            public List<Event.CheapMode> CheapModes { get; set; }

            /// <summary>
            /// Creates an empty EventParam.
            /// </summary>
            public EventParam()
            {
                Lights = new List<Event.Light>();
                Shadows = new List<Event.Shadow>();
                Fogs = new List<Event.Fog>();
                BGColors = new List<Event.BGColor>();
                MapOffsets = new List<Event.MapOffset>();
                Warps = new List<Event.Warp>();
                CheapModes = new List<Event.CheapMode>();
            }

            /// <summary>
            /// Adds an event to the appropriate list for its type; returns the event.
            /// </summary>
            public Event Add(Event evnt)
            {
                switch (evnt)
                {
                    case Event.Light e: Lights.Add(e); break;
                    case Event.Shadow e: Shadows.Add(e); break;
                    case Event.Fog e: Fogs.Add(e); break;
                    case Event.BGColor e: BGColors.Add(e); break;
                    case Event.MapOffset e: MapOffsets.Add(e); break;
                    case Event.Warp e: Warps.Add(e); break;
                    case Event.CheapMode e: CheapModes.Add(e); break;

                    default:
                        throw new ArgumentException($"Unrecognized type {evnt.GetType()}.", nameof(evnt));
                }
                return evnt;
            }
            IMsbEvent IMsbParam<IMsbEvent>.Add(IMsbEvent item) => Add((Event)item);

            /// <summary>
            /// Returns every Event in the order they'll be written.
            /// </summary>
            public override List<Event> GetEntries()
            {
                return SFUtil.ConcatAll<Event>(
                    Lights, Shadows, Fogs, BGColors, MapOffsets,
                    Warps, CheapModes);
            }
            IReadOnlyList<IMsbEvent> IMsbParam<IMsbEvent>.GetEntries() => GetEntries();

            internal override Event ReadEntry(BinaryReaderEx br)
            {
                EventType type = br.GetEnum8<EventType>(br.Position + br.VarintSize + 4);
                switch (type)
                {
                    case EventType.Light:
                        return Lights.EchoAdd(new Event.Light(br));

                    case EventType.Shadow:
                        return Shadows.EchoAdd(new Event.Shadow(br));

                    case EventType.Fog:
                        return Fogs.EchoAdd(new Event.Fog(br));

                    case EventType.BGColor:
                        return BGColors.EchoAdd(new Event.BGColor(br));

                    case EventType.MapOffset:
                        return MapOffsets.EchoAdd(new Event.MapOffset(br));

                    case EventType.Warp:
                        return Warps.EchoAdd(new Event.Warp(br));

                    case EventType.CheapMode:
                        return CheapModes.EchoAdd(new Event.CheapMode(br));

                    default:
                        throw new NotImplementedException($"Unimplemented event type: {type}");
                }
            }
        }

        /// <summary>
        /// An abstract entity that controls map properties or behaviors.
        /// </summary>
        public abstract class Event : NamedEntry, IMsbEvent
        {
            private protected abstract EventType Type { get; }

            /// <summary>
            /// Uniquely identifies the event in the map.
            /// </summary>
            public int EventID { get; set; }

            private protected Event(string name)
            {
                Name = name;
                EventID = -1;
            }

            /// <summary>
            /// Creates a deep copy of the event.
            /// </summary>
            public Event DeepCopy()
            {
                return (Event)MemberwiseClone();
            }
            IMsbEvent IMsbEvent.DeepCopy() => DeepCopy();

            private protected Event(BinaryReaderEx br)
            {
                long start = br.Position;
                long nameOffset = br.ReadVarint();
                EventID = br.ReadInt32();
                br.AssertByte((byte)Type);
                br.AssertByte(0);
                br.ReadInt16(); // ID
                long typeDataOffset = br.ReadVarint();
                if (!br.VarintLong)
                {
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                if (nameOffset == 0)
                    throw new InvalidDataException($"{nameof(nameOffset)} must not be 0 in type {GetType()}.");
                if (typeDataOffset == 0)
                    throw new InvalidDataException($"{nameof(typeDataOffset)} must not be 0 in type {GetType()}.");

                br.Position = start + nameOffset;
                Name = br.ReadUTF16();

                br.Position = start + typeDataOffset;
                ReadTypeData(br);
            }

            private protected abstract void ReadTypeData(BinaryReaderEx br);

            internal override void Write(BinaryWriterEx bw, int id)
            {
                long start = bw.Position;
                bw.ReserveVarint("NameOffset");
                bw.WriteInt32(EventID);
                bw.WriteByte((byte)Type);
                bw.WriteByte(0);
                bw.WriteInt16((short)id);
                bw.ReserveVarint("TypeDataOffset");
                if (!bw.VarintLong)
                {
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }

                bw.FillVarint("NameOffset", bw.Position - start);
                bw.WriteUTF16(Name, true);
                bw.Pad(bw.VarintSize);

                bw.FillVarint("TypeDataOffset", bw.Position - start);
                WriteTypeData(bw);
            }

            private protected abstract void WriteTypeData(BinaryWriterEx bw);

            /// <summary>
            /// Returns a string representation of the event.
            /// </summary>
            public override string ToString()
            {
                return $"[ID {EventID}] {Type} \"{Name}\"";
            }

            /// <summary>
            /// Unknown if this does anything.
            /// </summary>
            public class Light : Event
            {
                private protected override EventType Type => EventType.Light;

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT00 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT04 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT08 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public Color ColorT0C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public Color ColorT10 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT1C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT20 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public Color ColorT24 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public Color ColorT28 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public Color ColorT34 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public Color ColorT38 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public Color ColorT3C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT40 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT44 { get; set; }

                /// <summary>
                /// Creates a Light with default values.
                /// </summary>
                public Light() : base($"{nameof(Event)}: {nameof(Light)}") { }

                internal Light(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    UnkT00 = br.ReadByte();
                    br.AssertByte(0);
                    br.AssertInt16(-1);
                    UnkT04 = br.ReadSingle();
                    UnkT08 = br.ReadSingle();
                    ColorT0C = br.ReadRGBA();
                    ColorT10 = br.ReadRGBA();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    UnkT1C = br.ReadSingle();
                    UnkT20 = br.ReadSingle();
                    ColorT24 = br.ReadRGBA();
                    ColorT28 = br.ReadRGBA();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    ColorT34 = br.ReadRGBA();
                    ColorT38 = br.ReadRGBA();
                    ColorT3C = br.ReadRGBA();
                    UnkT40 = br.ReadSingle();
                    UnkT44 = br.ReadByte();
                    br.AssertPattern(0x3B, 0x00);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteByte(UnkT00);
                    bw.WriteByte(0);
                    bw.WriteInt16(-1);
                    bw.WriteSingle(UnkT04);
                    bw.WriteSingle(UnkT08);
                    bw.WriteRGBA(ColorT0C);
                    bw.WriteRGBA(ColorT10);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteSingle(UnkT1C);
                    bw.WriteSingle(UnkT20);
                    bw.WriteRGBA(ColorT24);
                    bw.WriteRGBA(ColorT28);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteRGBA(ColorT34);
                    bw.WriteRGBA(ColorT38);
                    bw.WriteRGBA(ColorT3C);
                    bw.WriteSingle(UnkT40);
                    bw.WriteByte(UnkT44);
                    bw.WritePattern(0x3B, 0x00);
                }
            }

            /// <summary>
            /// Unknown if this does anything.
            /// </summary>
            public class Shadow : Event
            {
                private protected override EventType Type => EventType.Shadow;

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT04 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT08 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT0C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT14 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT18 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT20 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public Color ColorT24 { get; set; }

                /// <summary>
                /// Creates a Shadow with default values.
                /// </summary>
                public Shadow() : base($"{nameof(Event)}: {nameof(Shadow)}") { }

                internal Shadow(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    br.AssertInt32(0);
                    UnkT04 = br.ReadSingle();
                    UnkT08 = br.ReadSingle();
                    UnkT0C = br.ReadSingle();
                    br.AssertInt32(0);
                    UnkT14 = br.ReadSingle();
                    UnkT18 = br.ReadSingle();
                    br.AssertInt32(0);
                    UnkT20 = br.ReadSingle();
                    ColorT24 = br.ReadRGBA();
                    br.AssertPattern(0x18, 0x00);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(0);
                    bw.WriteSingle(UnkT04);
                    bw.WriteSingle(UnkT08);
                    bw.WriteSingle(UnkT0C);
                    bw.WriteInt32(0);
                    bw.WriteSingle(UnkT14);
                    bw.WriteSingle(UnkT18);
                    bw.WriteInt32(0);
                    bw.WriteSingle(UnkT20);
                    bw.WriteRGBA(ColorT24);
                    bw.WritePattern(0x18, 0x00);
                }
            }

            /// <summary>
            /// Unknown if this does anything.
            /// </summary>
            public class Fog : Event
            {
                private protected override EventType Type => EventType.Fog;

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT00 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                [SupportsAlpha(true)]
                public Color ColorT04 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT08 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT0C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT10 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT14 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT15 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT16 { get; set; }

                /// <summary>
                /// Creates a Fog with default values.
                /// </summary>
                public Fog() : base($"{nameof(Event)}: {nameof(Fog)}") { }

                internal Fog(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    UnkT00 = br.ReadByte();
                    br.AssertByte(0);
                    br.AssertByte(0);
                    br.AssertByte(0);
                    ColorT04 = br.ReadRGBA();
                    UnkT08 = br.ReadSingle();
                    UnkT0C = br.ReadSingle();
                    UnkT10 = br.ReadSingle();
                    UnkT14 = br.ReadByte();
                    UnkT15 = br.ReadByte();
                    UnkT16 = br.ReadByte();
                    br.AssertPattern(0x11, 0x00);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteByte(UnkT00);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteRGBA(ColorT04);
                    bw.WriteSingle(UnkT08);
                    bw.WriteSingle(UnkT0C);
                    bw.WriteSingle(UnkT10);
                    bw.WriteByte(UnkT14);
                    bw.WriteByte(UnkT15);
                    bw.WriteByte(UnkT16);
                    bw.WritePattern(0x11, 0x00);
                }
            }

            /// <summary>
            /// Sets the background color of the map when no models are in the way.
            /// </summary>
            public class BGColor : Event
            {
                private protected override EventType Type => EventType.BGColor;

                /// <summary>
                /// The background color.
                /// </summary>
                public Color Color { get; set; }

                /// <summary>
                /// Creates a BGColor with default values.
                /// </summary>
                public BGColor() : base($"{nameof(Event)}: {nameof(BGColor)}") { }

                internal BGColor(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    Color = br.ReadRGBA();
                    br.AssertPattern(0x24, 0x00);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteRGBA(Color);
                    bw.WritePattern(0x24, 0x00);
                }
            }

            /// <summary>
            /// Sets the origin of the map; already factored into MSB positions but affects BTL.
            /// </summary>
            public class MapOffset : Event
            {
                private protected override EventType Type => EventType.MapOffset;

                /// <summary>
                /// The origin of the map.
                /// </summary>
                public Vector3 Translation { get; set; }

                /// <summary>
                /// Creates a MapOffset with default values.
                /// </summary>
                public MapOffset() : base($"{nameof(Event)}: {nameof(MapOffset)}") { }

                internal MapOffset(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    Translation = br.ReadVector3();
                    br.AssertInt32(0); // Degree
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteVector3(Translation);
                    bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// Unknown exactly what this is for.
            /// </summary>
            public class Warp : Event
            {
                private protected override EventType Type => EventType.Warp;

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT00 { get; set; }

                /// <summary>
                /// Presumably the position to be warped to.
                /// </summary>
                public Vector3 Position { get; set; }

                /// <summary>
                /// Creates a Warp with default values.
                /// </summary>
                public Warp() : base($"{nameof(Event)}: {nameof(Warp)}") { }

                internal Warp(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    UnkT00 = br.ReadByte();
                    br.AssertByte(0);
                    br.AssertByte(0);
                    br.AssertByte(0);
                    Position = br.ReadVector3();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteByte(UnkT00);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteVector3(Position);
                }
            }

            /// <summary>
            /// Unknown if this does anything.
            /// </summary>
            public class CheapMode : Event
            {
                private protected override EventType Type => EventType.CheapMode;

                /// <summary>
                /// Unknown.
                /// </summary>
                public short UnkT00 { get; set; }

                /// <summary>
                /// Creates a CheapMode with default values.
                /// </summary>
                public CheapMode() : base($"{nameof(Event)}: {nameof(CheapMode)}") { }

                internal CheapMode(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    UnkT00 = br.ReadInt16();
                    br.AssertPattern(0xE, 0x00);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt16(UnkT00);
                    bw.WritePattern(0xE, 0x00);
                }
            }
        }
    }
}
