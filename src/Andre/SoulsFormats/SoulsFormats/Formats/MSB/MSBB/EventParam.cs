using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace SoulsFormats
{
    public partial class MSBB
    {
        internal enum EventType : uint
        {
            //Light = 0,
            Sound = 1,
            SFX = 2,
            //Wind = 3,
            Treasure = 4,
            Generator = 5,
            Message = 6,
            ObjAct = 7,
            SpawnPoint = 8,
            MapOffset = 9,
            Navmesh = 10,
            Environment = 11,
            //PseudoMultiplayer = 12,
            WindSFX = 13,
            PatrolInfo = 14,
            DarkLock = 15,
            PlatoonInfo = 16,
            MultiSummon = 17,
            Other = 0xFFFFFFFF,
        }

        /// <summary>
        /// Contains abstract entities that control various dynamic elements in the map.
        /// </summary>
        public class EventParam : Param<Event>, IMsbParam<IMsbEvent>
        {
            internal override int Version => 3;
            internal override string Name => "EVENT_PARAM_ST";

            /// <summary>
            /// Background music and area-based sounds.
            /// </summary>
            public List<Event.Sound> Sounds { get; set; }

            /// <summary>
            /// Particle effects.
            /// </summary>
            public List<Event.SFX> SFX { get; set; }

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
            /// Controllers for object interactions.
            /// </summary>
            public List<Event.ObjAct> ObjActs { get; set; }

            /// <summary>
            /// Unknown exactly what this is for.
            /// </summary>
            public List<Event.SpawnPoint> SpawnPoints { get; set; }

            /// <summary>
            /// Represents the origin of the map; already accounted for in MSB positions.
            /// </summary>
            public List<Event.MapOffset> MapOffsets { get; set; }

            /// <summary>
            /// Unknown, interacts with navmeshes somehow.
            /// </summary>
            public List<Event.Navmesh> Navmeshes { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Event.Environment> Environments { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Event.WindSFX> WindSFX { get; set; }

            /// <summary>
            /// Patrol info in the MSB.
            /// </summary>
            public List<Event.PatrolInfo> PatrolInfo { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Event.DarkLock> DarkLocks { get; set; }

            /// <summary>
            /// Platoon info in the MSB.
            /// </summary>
            public List<Event.PlatoonInfo> PlatoonInfo { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Event.MultiSummon> MultiSummons { get; set; }

            /// <summary>
            /// Other events in the MSB.
            /// </summary>
            public List<Event.Other> Others { get; set; }

            /// <summary>
            /// Creates an empty EventParam.
            /// </summary>
            public EventParam() : base()
            {
                Sounds = new List<Event.Sound>();
                SFX = new List<Event.SFX>();
                Treasures = new List<Event.Treasure>();
                Generators = new List<Event.Generator>();
                Messages = new List<Event.Message>();
                ObjActs = new List<Event.ObjAct>();
                SpawnPoints = new List<Event.SpawnPoint>();
                MapOffsets = new List<Event.MapOffset>();
                Navmeshes = new List<Event.Navmesh>();
                Environments = new List<Event.Environment>();
                WindSFX = new List<Event.WindSFX>();
                PatrolInfo = new List<Event.PatrolInfo>();
                DarkLocks = new List<Event.DarkLock>();
                PlatoonInfo = new List<Event.PlatoonInfo>();
                MultiSummons = new List<Event.MultiSummon>();
                Others = new List<Event.Other>();
            }

            /// <summary>
            /// Adds an event to the appropriate list for its type; returns the event.
            /// </summary>
            public Event Add(Event evnt)
            {
                switch (evnt)
                {
                    case Event.Sound e: Sounds.Add(e); break;
                    case Event.SFX e: SFX.Add(e); break;
                    case Event.Treasure e: Treasures.Add(e); break;
                    case Event.Generator e: Generators.Add(e); break;
                    case Event.Message e: Messages.Add(e); break;
                    case Event.ObjAct e: ObjActs.Add(e); break;
                    case Event.SpawnPoint e: SpawnPoints.Add(e); break;
                    case Event.MapOffset e: MapOffsets.Add(e); break;
                    case Event.Navmesh e: Navmeshes.Add(e); break;
                    case Event.Environment e: Environments.Add(e); break;
                    case Event.WindSFX e: WindSFX.Add(e); break;
                    case Event.PatrolInfo e: PatrolInfo.Add(e); break;
                    case Event.DarkLock e: DarkLocks.Add(e); break;
                    case Event.PlatoonInfo e: PlatoonInfo.Add(e); break;
                    case Event.MultiSummon e: MultiSummons.Add(e); break;
                    case Event.Other e: Others.Add(e); break;

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
                    Sounds, SFX, Treasures, Generators, Messages,
                    ObjActs, SpawnPoints, MapOffsets, Navmeshes, Environments,
                    WindSFX, PatrolInfo, DarkLocks, PlatoonInfo, MultiSummons,
                    Others);
            }
            IReadOnlyList<IMsbEvent> IMsbParam<IMsbEvent>.GetEntries() => GetEntries();

            internal override Event ReadEntry(BinaryReaderEx br)
            {
                EventType type = br.GetEnum32<EventType>(br.Position + 0xC);
                switch (type)
                {
                    case EventType.Sound:
                        return Sounds.EchoAdd(new Event.Sound(br));

                    case EventType.SFX:
                        return SFX.EchoAdd(new Event.SFX(br));

                    case EventType.Treasure:
                        return Treasures.EchoAdd(new Event.Treasure(br));

                    case EventType.Generator:
                        return Generators.EchoAdd(new Event.Generator(br));

                    case EventType.Message:
                        return Messages.EchoAdd(new Event.Message(br));

                    case EventType.ObjAct:
                        return ObjActs.EchoAdd(new Event.ObjAct(br));

                    case EventType.SpawnPoint:
                        return SpawnPoints.EchoAdd(new Event.SpawnPoint(br));

                    case EventType.MapOffset:
                        return MapOffsets.EchoAdd(new Event.MapOffset(br));

                    case EventType.Navmesh:
                        return Navmeshes.EchoAdd(new Event.Navmesh(br));

                    case EventType.Environment:
                        return Environments.EchoAdd(new Event.Environment(br));

                    case EventType.WindSFX:
                        return WindSFX.EchoAdd(new Event.WindSFX(br));

                    case EventType.PatrolInfo:
                        return PatrolInfo.EchoAdd(new Event.PatrolInfo(br));

                    case EventType.DarkLock:
                        return DarkLocks.EchoAdd(new Event.DarkLock(br));

                    case EventType.PlatoonInfo:
                        return PlatoonInfo.EchoAdd(new Event.PlatoonInfo(br));

                    case EventType.MultiSummon:
                        return MultiSummons.EchoAdd(new Event.MultiSummon(br));

                    case EventType.Other:
                        return Others.EchoAdd(new Event.Other(br));

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
            private protected abstract bool HasTypeData { get; }

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

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte UnkE0C { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte UnkE0D { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte UnkE0E { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte UnkE0F { get; set; }

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
                long nameOffset = br.ReadInt64();
                EventID = br.ReadInt32();
                br.AssertUInt32((uint)Type);
                br.ReadInt32(); // ID
                br.AssertInt32(0);
                long entityDataOffset = br.ReadInt64();
                long typeDataOffset = br.ReadInt64();

                if (nameOffset == 0)
                    throw new InvalidDataException($"{nameof(nameOffset)} must not be 0 in type {GetType()}.");
                if (entityDataOffset == 0)
                    throw new InvalidDataException($"{nameof(entityDataOffset)} must not be 0 in type {GetType()}.");
                if (HasTypeData ^ typeDataOffset != 0)
                    throw new InvalidDataException($"Unexpected {nameof(typeDataOffset)} 0x{typeDataOffset:X} in type {GetType()}.");

                br.Position = start + nameOffset;
                Name = br.ReadUTF16();

                br.Position = start + entityDataOffset;
                PartIndex = br.ReadInt32();
                RegionIndex = br.ReadInt32();
                EntityID = br.ReadInt32();
                UnkE0C = br.ReadByte();
                UnkE0D = br.ReadByte();
                UnkE0E = br.ReadByte();
                UnkE0F = br.ReadByte();

                if (HasTypeData)
                {
                    br.Position = start + typeDataOffset;
                    ReadTypeData(br);
                }
            }

            private protected virtual void ReadTypeData(BinaryReaderEx br)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(ReadTypeData)}.");

            internal override void Write(BinaryWriterEx bw, int id)
            {
                long start = bw.Position;
                bw.ReserveInt64("NameOffset");
                bw.WriteInt32(EventID);
                bw.WriteUInt32((uint)Type);
                bw.WriteInt32(id);
                bw.WriteInt32(0);
                bw.ReserveInt64("EntityDataOffset");
                bw.ReserveInt64("TypeDataOffset");

                bw.FillInt64("NameOffset", bw.Position - start);
                bw.WriteUTF16(Name, true);
                bw.Pad(8);

                bw.FillInt64("EntityDataOffset", bw.Position - start);
                bw.WriteInt32(PartIndex);
                bw.WriteInt32(RegionIndex);
                bw.WriteInt32(EntityID);
                bw.WriteByte(UnkE0C);
                bw.WriteByte(UnkE0D);
                bw.WriteByte(UnkE0E);
                bw.WriteByte(UnkE0F);

                if (HasTypeData)
                {
                    bw.FillInt64("TypeDataOffset", bw.Position - start);
                    WriteTypeData(bw);
                }
                else
                {
                    bw.FillInt64("TypeDataOffset", 0);
                }
            }

            private protected virtual void WriteTypeData(BinaryWriterEx bw)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(WriteTypeData)}.");

            internal virtual void GetNames(MSBB msb, Entries entries)
            {
                PartName = MSB.FindName(entries.Parts, PartIndex);
                RegionName = MSB.FindName(entries.Regions, RegionIndex);
            }

            internal virtual void GetIndices(MSBB msb, Entries entries)
            {
                PartIndex = MSB.FindIndex(this, entries.Parts, PartName);
                RegionIndex = MSB.FindIndex(this, entries.Regions, RegionName);
            }

            /// <summary>
            /// Returns the type and name of the event.
            /// </summary>
            public override string ToString()
            {
                return $"{Type} {Name}";
            }

            /// <summary>
            /// An area-based music or sound effect.
            /// </summary>
            public class Sound : Event
            {
                private protected override EventType Type => EventType.Sound;
                private protected override bool HasTypeData => true;

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
                    SoundType = br.ReadInt32();
                    SoundID = br.ReadInt32();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
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
                private protected override bool HasTypeData => true;

                /// <summary>
                /// ID of the particle effect.
                /// </summary>
                public int EffectID { get; set; }

                /// <summary>
                /// Stops the effect from playing automatically.
                /// </summary>
                public bool StartDisabled { get; set; }

                /// <summary>
                /// Creates an SFX with default values.
                /// </summary>
                public SFX() : base($"{nameof(Event)}: {nameof(SFX)}") { }

                internal SFX(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    EffectID = br.ReadInt32();
                    StartDisabled = br.AssertInt32([0, 1]) == 1;
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(EffectID);
                    bw.WriteInt32(StartDisabled ? 1 : 0);
                }
            }

            /// <summary>
            /// A pick-uppable item.
            /// </summary>
            public class Treasure : Event
            {
                private protected override EventType Type => EventType.Treasure;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// The part that the treasure is attached to, such as an item corpse.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Part))]
                public string TreasurePartName { get; set; }
                private int TreasurePartIndex;

                /// <summary>
                /// First itemlot given by the treasure.
                /// </summary>
                public int ItemLot1 { get; set; }

                /// <summary>
                /// Second itemlot given by the treasure.
                /// </summary>
                public int ItemLot2 { get; set; }

                /// <summary>
                /// Third itemlot given by the treasure.
                /// </summary>
                public int ItemLot3 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT1C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT20 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT24 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT28 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT2C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT30 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT34 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int ActionButtonParamID { get; set; }

                /// <summary>
                /// Unknown; possible the pickup anim.
                /// </summary>
                public int UnkT3C { get; set; }

                /// <summary>
                /// Changes the text of the pickup prompt.
                /// </summary>
                public bool InChest { get; set; }

                /// <summary>
                /// Whether the treasure should be hidden by default.
                /// </summary>
                public bool StartDisabled { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short UnkT42 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int EnableSecondaryItemlot { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT48 { get; set; }

                /// <summary>
                /// Creates a Treasure with default values.
                /// </summary>
                public Treasure() : base($"{nameof(Event)}: {nameof(Treasure)}") { }

                internal Treasure(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    TreasurePartIndex = br.ReadInt32();
                    br.AssertInt32(0);
                    ItemLot1 = br.ReadInt32();
                    ItemLot2 = br.ReadInt32();
                    ItemLot3 = br.ReadInt32();
                    UnkT1C = br.ReadInt32();
                    UnkT20 = br.ReadInt32();
                    UnkT24 = br.ReadInt32();
                    UnkT28 = br.ReadInt32();
                    UnkT2C = br.ReadInt32();
                    UnkT30 = br.ReadInt32();
                    UnkT34 = br.ReadInt32();
                    ActionButtonParamID = br.ReadInt32();
                    UnkT3C = br.ReadInt32();
                    InChest = br.ReadBoolean();
                    StartDisabled = br.ReadBoolean();
                    UnkT42 = br.ReadInt16();
                    EnableSecondaryItemlot = br.ReadInt32();
                    UnkT48 = br.ReadInt32();
                    br.AssertInt32(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(TreasurePartIndex);
                    bw.WriteInt32(0);
                    bw.WriteInt32(ItemLot1);
                    bw.WriteInt32(ItemLot2);
                    bw.WriteInt32(ItemLot3);
                    bw.WriteInt32(UnkT1C);
                    bw.WriteInt32(UnkT20);
                    bw.WriteInt32(UnkT24);
                    bw.WriteInt32(UnkT28);
                    bw.WriteInt32(UnkT2C);
                    bw.WriteInt32(UnkT30);
                    bw.WriteInt32(UnkT34);
                    bw.WriteInt32(ActionButtonParamID);
                    bw.WriteInt32(UnkT3C);
                    bw.WriteBoolean(InChest);
                    bw.WriteBoolean(StartDisabled);
                    bw.WriteInt16(UnkT42);
                    bw.WriteInt32(EnableSecondaryItemlot);
                    bw.WriteInt32(UnkT48);
                    bw.WriteInt32(0);
                }

                internal override void GetNames(MSBB msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    TreasurePartName = MSB.FindName(entries.Parts, TreasurePartIndex);
                }

                internal override void GetIndices(MSBB msb, Entries entries)
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
                private protected override bool HasTypeData => true;

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
                /// Unknown.
                /// </summary>
                public byte UnkT11 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT12 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT13 { get; set; }

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
                    SpawnPointNames = new string[8];
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
                    UnkT11 = br.ReadByte();
                    UnkT12 = br.ReadByte();
                    UnkT13 = br.ReadByte();
                    br.AssertPattern(0x1C, 0x00);
                    SpawnPointIndices = br.ReadInt32s(8);
                    SpawnPartIndices = br.ReadInt32s(32);
                    br.AssertPattern(0x30, 0x00);
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
                    bw.WriteByte(UnkT11);
                    bw.WriteByte(UnkT12);
                    bw.WriteByte(UnkT13);
                    bw.WritePattern(0x1C, 0x00);
                    bw.WriteInt32s(SpawnPointIndices);
                    bw.WriteInt32s(SpawnPartIndices);
                    bw.WritePattern(0x30, 0x00);
                }

                internal override void GetNames(MSBB msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    SpawnPointNames = MSB.FindNames(entries.Regions, SpawnPointIndices);
                    SpawnPartNames = MSB.FindNames(entries.Parts, SpawnPartIndices);
                }

                internal override void GetIndices(MSBB msb, Entries entries)
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
                private protected override bool HasTypeData => true;

                /// <summary>
                /// FMG text ID to display.
                /// </summary>
                public short MessageID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short UnkT02 { get; set; }

                /// <summary>
                /// Whether the Message requires Seek Guidance to see.
                /// </summary>
                public bool Hidden { get; set; }

                /// <summary>
                /// Creates a Message with default values.
                /// </summary>
                public Message() : base($"{nameof(Event)}: {nameof(Message)}") { }

                internal Message(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    MessageID = br.ReadInt16();
                    UnkT02 = br.ReadInt16();
                    Hidden = br.ReadBoolean();
                    br.AssertByte(0);
                    br.AssertInt16(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt16(MessageID);
                    bw.WriteInt16(UnkT02);
                    bw.WriteBoolean(Hidden);
                    bw.WriteByte(0);
                    bw.WriteInt16(0);
                }
            }

            /// <summary>
            /// Represents an interaction with an object.
            /// </summary>
            public class ObjAct : Event
            {
                private protected override EventType Type => EventType.ObjAct;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Unknown how this differs from the Event EntityID.
                /// </summary>
                public int ObjActEntityID { get; set; }

                /// <summary>
                /// The object that the ObjAct controls.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Part))]
                public string ObjActPartName { get; set; }
                private int ObjActPartIndex;

                /// <summary>
                /// ID in ObjActParam that configures the ObjAct.
                /// </summary>
                public int ObjActParamID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte ObjActState { get; set; } = 0;

                /// <summary>
                /// Unknown, probably enables or disables the ObjAct.
                /// </summary>
                public int EventFlagID { get; set; }

                /// <summary>
                /// Creates an ObjAct with default values.
                /// </summary>
                public ObjAct() : base($"{nameof(Event)}: {nameof(ObjAct)}")
                {
                    ObjActEntityID = -1;
                    ObjActParamID = -1;
                    EventFlagID = -1;
                }

                internal ObjAct(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    ObjActEntityID = br.ReadInt32();
                    ObjActPartIndex = br.ReadInt32();
                    ObjActParamID = br.ReadInt32();
                    ObjActState = br.ReadByte();
                    br.AssertByte(0);
                    br.AssertInt16(0);
                    EventFlagID = br.ReadInt32();
                    br.AssertInt32(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(ObjActEntityID);
                    bw.WriteInt32(ObjActPartIndex);
                    bw.WriteInt32(ObjActParamID);
                    bw.WriteByte((byte)ObjActState);
                    bw.WriteByte(0);
                    bw.WriteInt16(0);
                    bw.WriteInt32(EventFlagID);
                    bw.WriteInt32(0);
                }

                internal override void GetNames(MSBB msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    ObjActPartName = MSB.FindName(entries.Parts, ObjActPartIndex);
                }

                internal override void GetIndices(MSBB msb, Entries entries)
                {
                    base.GetIndices(msb, entries);
                    ObjActPartIndex = MSB.FindIndex(this, entries.Parts, ObjActPartName);
                }
            }

            /// <summary>
            /// Unknown what this accomplishes beyond just having the region.
            /// </summary>
            public class SpawnPoint : Event
            {
                private protected override EventType Type => EventType.SpawnPoint;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Point for the SpawnPoint to spawn at.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Region))]
                public string SpawnPointName { get; set; }
                private int SpawnPointIndex;

                /// <summary>
                /// Creates a SpawnPoint with default values.
                /// </summary>
                public SpawnPoint() : base($"{nameof(Event)}: {nameof(SpawnPoint)}") { }

                internal SpawnPoint(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    SpawnPointIndex = br.ReadInt32();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(SpawnPointIndex);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }

                internal override void GetNames(MSBB msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    SpawnPointName = MSB.FindName(entries.Regions, SpawnPointIndex);
                }

                internal override void GetIndices(MSBB msb, Entries entries)
                {
                    base.GetIndices(msb, entries);
                    SpawnPointIndex = MSB.FindIndex(this, entries.Regions, SpawnPointName);
                }
            }

            /// <summary>
            /// The origin of the map, already accounted for in MSB positions.
            /// </summary>
            public class MapOffset : Event
            {
                private protected override EventType Type => EventType.MapOffset;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Position of the map.
                /// </summary>
                public Vector3 Position { get; set; }

                /// <summary>
                /// Rotation of the map.
                /// </summary>
                public float RotationY { get; set; }

                /// <summary>
                /// Creates a MapOffset with default values.
                /// </summary>
                public MapOffset() : base($"{nameof(Event)}: {nameof(MapOffset)}") { }

                internal MapOffset(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    Position = br.ReadVector3();
                    RotationY = br.ReadSingle();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteVector3(Position);
                    bw.WriteSingle(RotationY);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class Navmesh : Event
            {
                private protected override EventType Type => EventType.Navmesh;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Unknown.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Region))]
                public string NavmeshRegionName { get; set; }
                private int NavmeshRegionIndex;

                /// <summary>
                /// Creates a Navmesh with default values.
                /// </summary>
                public Navmesh() : base($"{nameof(Event)}: {nameof(Navmesh)}") { }

                internal Navmesh(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    NavmeshRegionIndex = br.ReadInt32();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(NavmeshRegionIndex);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }

                internal override void GetNames(MSBB msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    NavmeshRegionName = MSB.FindName(entries.Regions, NavmeshRegionIndex);
                }

                internal override void GetIndices(MSBB msb, Entries entries)
                {
                    base.GetIndices(msb, entries);
                    NavmeshRegionIndex = MSB.FindIndex(this, entries.Regions, NavmeshRegionName);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class Environment : Event
            {
                private protected override EventType Type => EventType.Environment;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT00 { get; set; }

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
                public float UnkT10 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT14 { get; set; }

                /// <summary>
                /// Creates an Environment with default values.
                /// </summary>
                public Environment() : base($"{nameof(Event)}: {nameof(Environment)}") { }

                internal Environment(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    UnkT00 = br.ReadInt32();
                    UnkT04 = br.ReadSingle();
                    UnkT08 = br.ReadSingle();
                    UnkT0C = br.ReadSingle();
                    UnkT10 = br.ReadSingle();
                    UnkT14 = br.ReadSingle();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(UnkT00);
                    bw.WriteSingle(UnkT04);
                    bw.WriteSingle(UnkT08);
                    bw.WriteSingle(UnkT0C);
                    bw.WriteSingle(UnkT10);
                    bw.WriteSingle(UnkT14);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class WindSFX : Event
            {
                private protected override EventType Type => EventType.WindSFX;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Particle effect ID to play.
                /// </summary>
                public int EffectID { get; set; }

                /// <summary>
                /// Presumably the area where the wind takes effect.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Region))]
                public string WindRegionName { get; set; }
                private int WindRegionIndex;

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT08 { get; set; }

                /// <summary>
                /// Creates a Wind with default values.
                /// </summary>
                public WindSFX() : base($"{nameof(Event)}: {nameof(WindSFX)}") { }

                internal WindSFX(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    EffectID = br.ReadInt32();
                    WindRegionIndex = br.ReadInt32();
                    UnkT08 = br.ReadSingle();
                    br.AssertInt32(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(EffectID);
                    bw.WriteInt32(WindRegionIndex);
                    bw.WriteSingle(UnkT08);
                    bw.WriteInt32(0);
                }

                internal override void GetNames(MSBB msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    WindRegionName = MSB.FindName(entries.Regions, WindRegionIndex);
                }

                internal override void GetIndices(MSBB msb, Entries entries)
                {
                    base.GetIndices(msb, entries);
                    WindRegionIndex = MSB.FindIndex(this, entries.Regions, WindRegionName);
                }
            }

            /// <summary>
            /// A simple list of points defining a path for enemies to take.
            /// </summary>
            public class PatrolInfo : Event
            {
                private protected override EventType Type => EventType.PatrolInfo;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Unknown; probably some kind of route type.
                /// </summary>
                public int PatrolType { get; set; }

                /// <summary>
                /// List of points in the route.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Region))]
                public string[] WalkPointNames { get; private set; }
                private short[] WalkPointIndices;

                /// <summary>
                /// Creates a WalkRoute with default values.
                /// </summary>
                public PatrolInfo() : base($"{nameof(Event)}: {nameof(PatrolInfo)}")
                {
                    WalkPointNames = new string[32];
                }

                private protected override void DeepCopyTo(Event evnt)
                {
                    var walkRoute = (PatrolInfo)evnt;
                    walkRoute.WalkPointNames = (string[])WalkPointNames.Clone();
                }

                internal PatrolInfo(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    PatrolType = br.ReadInt32();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    WalkPointIndices = br.ReadInt16s(32);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(PatrolType);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt16s(WalkPointIndices);
                }

                internal override void GetNames(MSBB msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    WalkPointNames = new string[WalkPointIndices.Length];
                    for (int i = 0; i < WalkPointIndices.Length; i++)
                        WalkPointNames[i] = MSB.FindName(entries.Regions, WalkPointIndices[i]);
                }

                internal override void GetIndices(MSBB msb, Entries entries)
                {
                    base.GetIndices(msb, entries);
                    WalkPointIndices = new short[WalkPointNames.Length];
                    for (int i = 0; i < WalkPointNames.Length; i++)
                        WalkPointIndices[i] = (short)MSB.FindIndex(this, entries.Regions, WalkPointNames[i]);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class DarkLock : Event
            {
                private protected override EventType Type => EventType.DarkLock;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Creates a DarkLock with default values.
                /// </summary>
                public DarkLock() : base($"{nameof(Event)}: {nameof(DarkLock)}") { }

                internal DarkLock(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class PlatoonInfo : Event
            {
                private protected override EventType Type => EventType.PlatoonInfo;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Unknown.
                /// </summary>
                public int PlatoonIDScriptActivate { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int State { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Part))]
                public string[] GroupPartsNames { get; private set; }
                private int[] GroupPartsIndices;

                /// <summary>
                /// Creates a GroupTour with default values.
                /// </summary>
                public PlatoonInfo() : base($"{nameof(Event)}: {nameof(PlatoonInfo)}")
                {
                    GroupPartsNames = new string[32];
                }

                private protected override void DeepCopyTo(Event evnt)
                {
                    var groupTour = (PlatoonInfo)evnt;
                    groupTour.GroupPartsNames = (string[])GroupPartsNames.Clone();
                }

                internal PlatoonInfo(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    PlatoonIDScriptActivate = br.ReadInt32();
                    State = br.ReadInt32();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    GroupPartsIndices = br.ReadInt32s(32);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(PlatoonIDScriptActivate);
                    bw.WriteInt32(State);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32s(GroupPartsIndices);
                }

                internal override void GetNames(MSBB msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    GroupPartsNames = MSB.FindNames(entries.Parts, GroupPartsIndices);
                }

                internal override void GetIndices(MSBB msb, Entries entries)
                {
                    base.GetIndices(msb, entries);
                    GroupPartsIndices = MSB.FindIndices(this, entries.Parts, GroupPartsNames);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class MultiSummon : Event
            {
                private protected override EventType Type => EventType.MultiSummon;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Unknown.
                /// </summary>
                public int PlayRegionID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Region))]
                public string SpawnRegionName_00 { get; set; }
                private short SpawnRegionID_00;

                /// <summary>
                /// Unknown.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Region))]
                public string SpawnRegionName_01 { get; set; }
                private short SpawnRegionID_01;

                /// <summary>
                /// Unknown.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Region))]
                public string SpawnRegionName_02 { get; set; }
                private short SpawnRegionID_02;

                /// <summary>
                /// Unknown.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Region))]
                public string SpawnRegionName_03 { get; set; }
                private short SpawnRegionID_03;

                /// <summary>
                /// Creates a MultiSummon with default values.
                /// </summary>
                public MultiSummon() : base($"{nameof(Event)}: {nameof(MultiSummon)}") { }

                internal MultiSummon(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    PlayRegionID = br.ReadInt32();
                    SpawnRegionID_00 = br.ReadInt16();
                    SpawnRegionID_01 = br.ReadInt16();
                    SpawnRegionID_02 = br.ReadInt16();
                    SpawnRegionID_03 = br.ReadInt16();
                    br.AssertInt32(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(PlayRegionID);
                    bw.WriteInt16(SpawnRegionID_00);
                    bw.WriteInt16(SpawnRegionID_01);
                    bw.WriteInt16(SpawnRegionID_02);
                    bw.WriteInt16(SpawnRegionID_03);
                    bw.WriteInt32(0);
                }

                internal override void GetNames(MSBB msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    SpawnRegionName_00 = MSB.FindName(entries.Regions, SpawnRegionID_00);
                    SpawnRegionName_01 = MSB.FindName(entries.Regions, SpawnRegionID_01);
                    SpawnRegionName_02 = MSB.FindName(entries.Regions, SpawnRegionID_02);
                    SpawnRegionName_03 = MSB.FindName(entries.Regions, SpawnRegionID_03);
                }

                internal override void GetIndices(MSBB msb, Entries entries)
                {
                    base.GetIndices(msb, entries);
                    SpawnRegionID_00 = (short)MSB.FindIndex(this, entries.Regions, SpawnRegionName_00);
                    SpawnRegionID_01 = (short)MSB.FindIndex(this, entries.Regions, SpawnRegionName_01);
                    SpawnRegionID_02 = (short)MSB.FindIndex(this, entries.Regions, SpawnRegionName_02);
                    SpawnRegionID_03 = (short)MSB.FindIndex(this, entries.Regions, SpawnRegionName_03);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class Other : Event
            {
                private protected override EventType Type => EventType.Other;
                private protected override bool HasTypeData => false;

                /// <summary>
                /// Creates an Other with default values.
                /// </summary>
                public Other() : base($"{nameof(Event)}: {nameof(Other)}") { }

                internal Other(BinaryReaderEx br) : base(br) { }
            }
        }
    }
}
