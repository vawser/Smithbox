using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace SoulsFormats
{
    public partial class MSBD
    {
        internal enum EventType : uint
        {
            Light = 0,
            Sound = 1,
            SFX = 2,
            Wind = 3,
            Treasure = 4,
            Generator = 5,
            Message = 6,
        }

        /// <summary>
        /// Contains abstract entities that control various dynamic elements in the map.
        /// </summary>
        public class EventParam : Param<Event>, IMsbParam<IMsbEvent>
        {
            internal override string Name => "EVENT_PARAM_ST";

            /// <summary>
            /// Fixed point light sources.
            /// </summary>
            public List<Event.Light> Lights { get; set; }

            /// <summary>
            /// Background music and area-based sounds.
            /// </summary>
            public List<Event.Sound> Sounds { get; set; }

            /// <summary>
            /// Particle effects.
            /// </summary>
            public List<Event.SFX> SFX { get; set; }

            /// <summary>
            /// Wind that affects SFX in the map; should only be one per map, if any.
            /// </summary>
            public List<Event.Wind> Wind { get; set; }

            /// <summary>
            /// Item pickups in the open or in chests.
            /// </summary>
            public List<Event.Treasure> Treasures { get; set; }

            /// <summary>
            /// Repeated enemy spawners.
            /// </summary>
            public List<Event.Generator> Generators { get; set; }

            /// <summary>
            /// Static soapstone messages.
            /// </summary>
            public List<Event.Message> Messages { get; set; }

            /// <summary>
            /// Creates an empty EventParam.
            /// </summary>
            public EventParam() : base()
            {
                Lights = new List<Event.Light>();
                Sounds = new List<Event.Sound>();
                SFX = new List<Event.SFX>();
                Wind = new List<Event.Wind>();
                Treasures = new List<Event.Treasure>();
                Generators = new List<Event.Generator>();
                Messages = new List<Event.Message>();
            }

            /// <summary>
            /// Adds an event to the appropriate list for its type; returns the event.
            /// </summary>
            public Event Add(Event evnt)
            {
                switch (evnt)
                {
                    case Event.Light e: Lights.Add(e); break;
                    case Event.Sound e: Sounds.Add(e); break;
                    case Event.SFX e: SFX.Add(e); break;
                    case Event.Wind e: Wind.Add(e); break;
                    case Event.Treasure e: Treasures.Add(e); break;
                    case Event.Generator e: Generators.Add(e); break;
                    case Event.Message e: Messages.Add(e); break;

                    default:
                        throw new ArgumentException($"Unrecognized type {evnt.GetType()}.", nameof(evnt));
                }
                return evnt;
            }
            IMsbEvent IMsbParam<IMsbEvent>.Add(IMsbEvent item) => Add((Event)item);

            /// <summary>
            /// Returns a list of every event in the order they'll be written.
            /// </summary>
            public override List<Event> GetEntries()
            {
                return SFUtil.ConcatAll<Event>(
                    Lights, Sounds, SFX, Wind, Treasures,
                    Generators, Messages);
            }
            IReadOnlyList<IMsbEvent> IMsbParam<IMsbEvent>.GetEntries() => GetEntries();

            internal override Event ReadEntry(BinaryReaderEx br)
            {
                EventType type = br.GetEnum32<EventType>(br.Position + 8);
                switch (type)
                {
                    case EventType.Light:
                        return Lights.EchoAdd(new Event.Light(br));

                    case EventType.Sound:
                        return Sounds.EchoAdd(new Event.Sound(br));

                    case EventType.SFX:
                        return SFX.EchoAdd(new Event.SFX(br));

                    case EventType.Wind:
                        return Wind.EchoAdd(new Event.Wind(br));

                    case EventType.Treasure:
                        return Treasures.EchoAdd(new Event.Treasure(br));

                    case EventType.Generator:
                        return Generators.EchoAdd(new Event.Generator(br));

                    case EventType.Message:
                        return Messages.EchoAdd(new Event.Message(br));

                    default:
                        throw new NotImplementedException($"Unsupported event type: {type}");
                }
            }
        }

        /// <summary>
        /// Common data for all dynamic events.
        /// </summary>
        public abstract class Event : Entry, IMsbEvent
        {
            private protected abstract EventType Type { get; }

            /// <summary>
            /// The name of the event.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Unknown, should be unique.
            /// </summary>
            public int EventID { get; set; }

            /// <summary>
            /// Part referenced by the event.
            /// </summary>
            [MSBReference(ReferenceType = typeof(Part))]
            public string PartName { get; set; }
            private int PartIndex;

            /// <summary>
            /// Region referenced by the event.
            /// </summary>
            [MSBReference(ReferenceType = typeof(Region))]
            public string RegionName { get; set; }
            private int RegionIndex;

            /// <summary>
            /// Identifies the event in external files.
            /// </summary>
            public int EntityID { get; set; }

            private protected Event(string name)
            {
                Name = name;
                EventID = -1;
                EntityID = -1;
            }

            /// <summary>
            /// Creates a deep copy of the event.
            /// </summary>
            public Event DeepCopy()
            {
                var evnt = (Event)MemberwiseClone();
                DeepCopyTo(evnt);
                return evnt;
            }
            IMsbEvent IMsbEvent.DeepCopy() => DeepCopy();

            private protected virtual void DeepCopyTo(Event evnt) { }

            private protected Event(BinaryReaderEx br)
            {
                long start = br.Position;
                int nameOffset = br.ReadInt32();
                EventID = br.ReadInt32();
                br.AssertUInt32((uint)Type);
                br.ReadInt32(); // ID
                int baseDataOffset = br.ReadInt32();
                int typeDataOffset = br.ReadInt32();
                br.AssertInt32(0);
                br.AssertInt32(0);

                if (nameOffset == 0)
                    throw new InvalidDataException($"{nameof(nameOffset)} must not be 0 in type {GetType()}.");
                if (baseDataOffset == 0)
                    throw new InvalidDataException($"{nameof(baseDataOffset)} must not be 0 in type {GetType()}.");
                if (typeDataOffset == 0)
                    throw new InvalidDataException($"{nameof(typeDataOffset)} must not be 0 in type {GetType()}.");

                br.Position = start + nameOffset;
                Name = br.ReadShiftJIS();

                br.Position = start + baseDataOffset;
                PartIndex = br.ReadInt32();
                RegionIndex = br.ReadInt32();
                EntityID = br.ReadInt32();
                br.AssertInt32(0);

                br.Position = start + typeDataOffset;
                ReadTypeData(br);
            }

            private protected abstract void ReadTypeData(BinaryReaderEx br);

            internal override void Write(BinaryWriterEx bw, int id)
            {
                long start = bw.Position;
                bw.ReserveInt32("NameOffset");
                bw.WriteInt32(EventID);
                bw.WriteUInt32((uint)Type);
                bw.WriteInt32(id);
                bw.ReserveInt32("BaseDataOffset");
                bw.ReserveInt32("TypeDataOffset");
                bw.WriteInt32(0);
                bw.WriteInt32(0);

                bw.FillInt32("NameOffset", (int)(bw.Position - start));
                bw.WriteShiftJIS(Name, true);
                bw.Pad(4);

                bw.FillInt32("BaseDataOffset", (int)(bw.Position - start));
                bw.WriteInt32(PartIndex);
                bw.WriteInt32(RegionIndex);
                bw.WriteInt32(EntityID);
                bw.WriteInt32(0);

                bw.FillInt32("TypeDataOffset", (int)(bw.Position - start));
                WriteTypeData(bw);
            }

            private protected abstract void WriteTypeData(BinaryWriterEx bw);

            internal virtual void GetNames(MSBD msb, Entries entries)
            {
                PartName = MSB.FindName(entries.Parts, PartIndex);
                RegionName = MSB.FindName(entries.Regions, RegionIndex);
            }

            internal virtual void GetIndices(MSBD msb, Entries entries)
            {
                PartIndex = MSB.FindIndex(this, entries.Parts, PartName);
                RegionIndex = MSB.FindIndex(this, entries.Regions, RegionName);
            }

            /// <summary>
            /// Returns a string representation of the event.
            /// </summary>
            public override string ToString()
            {
                return $"{Type} {Name}";
            }

            /// <summary>
            /// A fixed point light.
            /// </summary>
            public class Light : Event
            {
                private protected override EventType Type => EventType.Light;

                /// <summary>
                /// Unknown.
                /// </summary>
                public int PointLightID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT04 { get; set; }

                /// <summary>
                /// Creates a Light with default values.
                /// </summary>
                public Light() : base($"{nameof(Event)}: {nameof(Light)}") { }

                internal Light(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    PointLightID = br.ReadInt32();
                    UnkT04 = br.ReadInt32();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(PointLightID);
                    bw.WriteInt32(UnkT04);
                }
            }

            /// <summary>
            /// An area-based music or sound effect.
            /// </summary>
            public class Sound : Event
            {
                private protected override EventType Type => EventType.Sound;

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT00 { get; set; }

                /// <summary>
                /// Category of sound.
                /// </summary>
                public int SoundType { get; set; }

                /// <summary>
                /// ID of the sound file in the FSBs.
                /// </summary>
                public int SoundID { get; set; }

                /// <summary>
                /// Creates a Sound with default values.
                /// </summary>
                public Sound() : base($"{nameof(Event)}: {nameof(Sound)}") { }

                internal Sound(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    UnkT00 = br.ReadInt32();
                    SoundType = br.ReadInt32();
                    SoundID = br.ReadInt32();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(UnkT00);
                    bw.WriteInt32(SoundType);
                    bw.WriteInt32(SoundID);
                }
            }

            /// <summary>
            /// A fixed particle effect.
            /// </summary>
            public class SFX : Event
            {
                private protected override EventType Type => EventType.SFX;

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT00 { get; set; }

                /// <summary>
                /// ID of the effect in the ffxbnds.
                /// </summary>
                public int EffectID { get; set; }

                /// <summary>
                /// Creates an SFX with default values.
                /// </summary>
                public SFX() : base($"{nameof(Event)}: {nameof(SFX)}") { }

                internal SFX(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    UnkT00 = br.ReadInt32();
                    EffectID = br.ReadInt32();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(UnkT00);
                    bw.WriteInt32(EffectID);
                }
            }

            /// <summary>
            /// Wind that affects particle effects.
            /// </summary>
            public class Wind : Event
            {
                private protected override EventType Type => EventType.Wind;

                /// <summary>
                /// Unknown.
                /// </summary>
                public Vector3 WindVecMin { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT0C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public Vector3 WindVecMax { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT1C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float WindSwingCycle0 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float WindSwingCycle1 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float WindSwingCycle2 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float WindSwingCycle3 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float WindSwingPow0 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float WindSwingPow1 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float WindSwingPow2 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float WindSwingPow3 { get; set; }

                /// <summary>
                /// Creates a Wind with default values.
                /// </summary>
                public Wind() : base($"{nameof(Event)}: {nameof(Wind)}") { }

                internal Wind(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    WindVecMin = br.ReadVector3();
                    UnkT0C = br.ReadSingle();
                    WindVecMax = br.ReadVector3();
                    UnkT1C = br.ReadSingle();
                    WindSwingCycle0 = br.ReadSingle();
                    WindSwingCycle1 = br.ReadSingle();
                    WindSwingCycle2 = br.ReadSingle();
                    WindSwingCycle3 = br.ReadSingle();
                    WindSwingPow0 = br.ReadSingle();
                    WindSwingPow1 = br.ReadSingle();
                    WindSwingPow2 = br.ReadSingle();
                    WindSwingPow3 = br.ReadSingle();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteVector3(WindVecMin);
                    bw.WriteSingle(UnkT0C);
                    bw.WriteVector3(WindVecMax);
                    bw.WriteSingle(UnkT1C);
                    bw.WriteSingle(WindSwingCycle0);
                    bw.WriteSingle(WindSwingCycle1);
                    bw.WriteSingle(WindSwingCycle2);
                    bw.WriteSingle(WindSwingCycle3);
                    bw.WriteSingle(WindSwingPow0);
                    bw.WriteSingle(WindSwingPow1);
                    bw.WriteSingle(WindSwingPow2);
                    bw.WriteSingle(WindSwingPow3);
                }
            }

            /// <summary>
            /// A pick-uppable item.
            /// </summary>
            public class Treasure : Event
            {
                private protected override EventType Type => EventType.Treasure;

                /// <summary>
                /// The part that the treasure is attached to, such as an item corpse.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Part))]
                public string TreasurePartName { get; set; }
                private int TreasurePartIndex;

                /// <summary>
                /// Five ItemLotParam IDs.
                /// </summary>
                public int[] ItemLots { get; private set; }

                /// <summary>
                /// Creates a Treasure with default values.
                /// </summary>
                public Treasure() : base($"{nameof(Event)}: {nameof(Treasure)}")
                {
                    ItemLots = new int[5] { -1, -1, -1, -1, -1 };
                }

                private protected override void DeepCopyTo(Event evnt)
                {
                    var treasure = (Treasure)evnt;
                    treasure.ItemLots = (int[])ItemLots.Clone();
                }

                internal Treasure(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    br.AssertInt32(0);
                    TreasurePartIndex = br.ReadInt32();
                    ItemLots = new int[5];
                    for (int i = 0; i < 5; i++)
                    {
                        ItemLots[i] = br.ReadInt32();
                        br.AssertInt32(-1);
                    }
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(0);
                    bw.WriteInt32(TreasurePartIndex);
                    for (int i = 0; i < 5; i++)
                    {
                        bw.WriteInt32(ItemLots[i]);
                        bw.WriteInt32(-1);
                    }
                }

                internal override void GetNames(MSBD msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    TreasurePartName = MSB.FindName(entries.Parts, TreasurePartIndex);
                }

                internal override void GetIndices(MSBD msb, Entries entries)
                {
                    base.GetIndices(msb, entries);
                    TreasurePartIndex = MSB.FindIndex(this, entries.Parts, TreasurePartName);
                }
            }

            /// <summary>
            /// A repeating enemy spawner.
            /// </summary>
            public class Generator : Event
            {
                private protected override EventType Type => EventType.Generator;

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte MaxNum { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public sbyte GenType { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short LimitNum { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short MinGenNum { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short MaxGenNum { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float MinInterval { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float MaxInterval { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte InitialSpawnCount { get; set; }

                /// <summary>
                /// Points that enemies may be spawned at.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Region))]
                public string[] SpawnPointNames { get; private set; }
                private int[] SpawnPointIndices;

                /// <summary>
                /// Enemies to be respawned.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Part))]
                public string[] SpawnPartNames { get; private set; }
                private int[] SpawnPartIndices;

                /// <summary>
                /// Creates a Generator with default values.
                /// </summary>
                public Generator() : base($"{nameof(Event)}: {nameof(Generator)}")
                {
                    SpawnPointNames = new string[4];
                    SpawnPartNames = new string[32];
                }

                private protected override void DeepCopyTo(Event evnt)
                {
                    var generator = (Generator)evnt;
                    generator.SpawnPointNames = (string[])SpawnPointNames.Clone();
                    generator.SpawnPartNames = (string[])SpawnPartNames.Clone();
                }

                internal Generator(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    MaxNum = br.ReadByte();
                    GenType = br.ReadSByte();
                    LimitNum = br.ReadInt16();
                    MinGenNum = br.ReadInt16();
                    MaxGenNum = br.ReadInt16();
                    MinInterval = br.ReadSingle();
                    MaxInterval = br.ReadSingle();
                    InitialSpawnCount = br.ReadByte();
                    br.AssertPattern(0x1F, 0x00);
                    SpawnPointIndices = br.ReadInt32s(4);
                    SpawnPartIndices = br.ReadInt32s(32);
                    br.AssertPattern(0x40, 0x00);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteByte(MaxNum);
                    bw.WriteSByte(GenType);
                    bw.WriteInt16(LimitNum);
                    bw.WriteInt16(MinGenNum);
                    bw.WriteInt16(MaxGenNum);
                    bw.WriteSingle(MinInterval);
                    bw.WriteSingle(MaxInterval);
                    bw.WriteByte(InitialSpawnCount);
                    bw.WritePattern(0x1F, 0x00);
                    bw.WriteInt32s(SpawnPointIndices);
                    bw.WriteInt32s(SpawnPartIndices);
                    bw.WritePattern(0x40, 0x00);
                }

                internal override void GetNames(MSBD msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    SpawnPointNames = MSB.FindNames(entries.Regions, SpawnPointIndices);
                    SpawnPartNames = MSB.FindNames(entries.Parts, SpawnPartIndices);
                }

                internal override void GetIndices(MSBD msb, Entries entries)
                {
                    base.GetIndices(msb, entries);
                    SpawnPointIndices = MSB.FindIndices(this, entries.Regions, SpawnPointNames);
                    SpawnPartIndices = MSB.FindIndices(this, entries.Parts, SpawnPartNames);
                }
            }

            /// <summary>
            /// A fixed orange soapstone message.
            /// </summary>
            public class Message : Event
            {
                private protected override EventType Type => EventType.Message;

                /// <summary>
                /// Unknown.
                /// </summary>
                public short UnkT00 { get; set; }

                /// <summary>
                /// FMG text ID to display.
                /// </summary>
                public short MessageID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int MessageParam { get; set; }

                /// <summary>
                /// Creates a Message with default values.
                /// </summary>
                public Message() : base($"{nameof(Event)}: {nameof(Message)}") { }

                internal Message(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    UnkT00 = br.ReadInt16();
                    MessageID = br.ReadInt16();
                    MessageParam = br.ReadInt32();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt16(UnkT00);
                    bw.WriteInt16(MessageID);
                    bw.WriteInt32(MessageParam);
                }
            }
        }
    }
}
