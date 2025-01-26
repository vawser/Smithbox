using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace SoulsFormats
{
    public partial class MSBE
    {
        internal enum EventType : uint
        {
            Treasure = 4,
            Generator = 5,
            ObjAct = 7,
            Navmesh = 10,
            PseudoMultiplayer = 12,
            PlatoonInfo = 15,
            PatrolInfo = 20,
            Mount = 21,
            SignPool = 23,
            RetryPoint = 24,
            AreaTeam = 25,
            Other = 0xFFFFFFFF,
        }

        /// <summary>
        /// Dynamic or interactive systems such as item pickups, levers, enemy spawners, etc.
        /// </summary>
        public class EventParam : Param<Event>, IMsbParam<IMsbEvent>
        {
            /// <summary>
            /// Item pickups out in the open or inside containers.
            /// </summary>
            public List<Event.Treasure> Treasures { get; set; }

            /// <summary>
            /// Enemy spawners.
            /// </summary>
            public List<Event.Generator> Generators { get; set; }

            /// <summary>
            /// Interactive objects like levers and doors.
            /// </summary>
            public List<Event.ObjAct> ObjActs { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Event.Navmesh> Navmeshes { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Event.PseudoMultiplayer> PseudoMultiplayers { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Event.PlatoonInfo> PlatoonInfo { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Event.PatrolInfo> PatrolInfo { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Event.Mount> Mounts { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Event.SignPool> SignPools { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Event.RetryPoint> RetryPoints { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Event.AreaTeam> AreaTeams { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Event.Other> Others { get; set; }

            /// <summary>
            /// Creates an empty EventParam with the default version.
            /// </summary>
            public EventParam() : base(73, "EVENT_PARAM_ST")
            {
                Treasures = new List<Event.Treasure>();
                Generators = new List<Event.Generator>();
                ObjActs = new List<Event.ObjAct>();
                Navmeshes = new List<Event.Navmesh>();
                PseudoMultiplayers = new List<Event.PseudoMultiplayer>();
                PlatoonInfo = new List<Event.PlatoonInfo>();
                PatrolInfo = new List<Event.PatrolInfo>();
                Mounts = new List<Event.Mount>();
                SignPools = new List<Event.SignPool>();
                RetryPoints = new List<Event.RetryPoint>();
                AreaTeams = new List<Event.AreaTeam>();
                Others = new List<Event.Other>();
            }

            /// <summary>
            /// Adds an event to the appropriate list for its type; returns the event.
            /// </summary>
            public Event Add(Event evnt)
            {
                switch (evnt)
                {
                    case Event.Treasure e: Treasures.Add(e); break;
                    case Event.Generator e: Generators.Add(e); break;
                    case Event.ObjAct e: ObjActs.Add(e); break;
                    case Event.Navmesh e: Navmeshes.Add(e); break;
                    case Event.PseudoMultiplayer e: PseudoMultiplayers.Add(e); break;
                    case Event.PlatoonInfo e: PlatoonInfo.Add(e); break;
                    case Event.PatrolInfo e: PatrolInfo.Add(e); break;
                    case Event.Mount e: Mounts.Add(e); break;
                    case Event.SignPool e: SignPools.Add(e); break;
                    case Event.RetryPoint e: RetryPoints.Add(e); break;
                    case Event.AreaTeam e: AreaTeams.Add(e); break;
                    case Event.Other e: Others.Add(e); break;

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
                    Treasures, Generators, ObjActs, Navmeshes, PseudoMultiplayers, PlatoonInfo,
                    PatrolInfo, Mounts, SignPools, RetryPoints, AreaTeams, Others);
            }
            IReadOnlyList<IMsbEvent> IMsbParam<IMsbEvent>.GetEntries() => GetEntries();

            internal override Event ReadEntry(BinaryReaderEx br)
            {
                EventType type = br.GetEnum32<EventType>(br.Position + 0xC);
                switch (type)
                {
                    case EventType.Treasure:
                        return Treasures.EchoAdd(new Event.Treasure(br));

                    case EventType.Generator:
                        return Generators.EchoAdd(new Event.Generator(br));

                    case EventType.ObjAct:
                        return ObjActs.EchoAdd(new Event.ObjAct(br));

                    case EventType.Navmesh:
                        return Navmeshes.EchoAdd(new Event.Navmesh(br));

                    case EventType.PseudoMultiplayer:
                        return PseudoMultiplayers.EchoAdd(new Event.PseudoMultiplayer(br));

                    case EventType.PlatoonInfo:
                        return PlatoonInfo.EchoAdd(new Event.PlatoonInfo(br));

                    case EventType.PatrolInfo:
                        return PatrolInfo.EchoAdd(new Event.PatrolInfo(br));

                    case EventType.Mount:
                        return Mounts.EchoAdd(new Event.Mount(br));

                    case EventType.SignPool:
                        return SignPools.EchoAdd(new Event.SignPool(br));

                    case EventType.RetryPoint:
                        return RetryPoints.EchoAdd(new Event.RetryPoint(br));

                    case EventType.AreaTeam:
                        return AreaTeams.EchoAdd(new Event.AreaTeam(br));

                    case EventType.Other:
                        return Others.EchoAdd(new Event.Other(br));

                    default:
                        throw new NotImplementedException($"Unimplemented event type: {type}");
                }
            }
        }

        /// <summary>
        /// A dynamic or interactive system.
        /// </summary>
        public abstract class Event : Entry, IMsbEvent
        {
            private protected abstract EventType Type { get; }
            private protected abstract bool HasTypeData { get; }

            /// <summary>
            /// "Other" events, which are likely unused, seem to have no logic to their ID, so we store it
            /// </summary>
            internal int OtherID { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int EventID { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            [MSBReference(ReferenceType = typeof(Part))]
            public string PartName { get; set; }
            private int PartIndex { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            [MSBReference(ReferenceType = typeof(Region))]
            public string RegionName { get; set; }
            private int RegionIndex { get; set; }

            /// <summary>
            /// Identifies the Event in event scripts.
            /// </summary>
            public uint EntityID { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int Unk14 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int MapID { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte UnkE0C { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int UnkS04 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int UnkS08 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int UnkS0C { get; set; }

            private protected Event(string name)
            {
                Name = name;
                OtherID = -1;
                EventID = -1;
                EntityID = 0;
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
                OtherID = br.ReadInt32(); // ID
                Unk14 = br.AssertInt32([0, 1]);
                long baseDataOffset = br.ReadInt64();
                long typeDataOffset = br.ReadInt64();
                long unk3Offset = br.ReadInt64();

                if (nameOffset == 0)
                    throw new InvalidDataException($"{nameof(nameOffset)} must not be 0 in type {GetType()}.");
                if (baseDataOffset == 0)
                    throw new InvalidDataException($"{nameof(baseDataOffset)} must not be 0 in type {GetType()}.");
                if (HasTypeData ^ typeDataOffset != 0)
                    throw new InvalidDataException($"Unexpected {nameof(typeDataOffset)} 0x{typeDataOffset:X} in type {GetType()}.");
                if (unk3Offset == 0)
                    throw new InvalidDataException($"{nameof(unk3Offset)} must not be 0 in type {GetType()}.");

                br.Position = start + nameOffset;
                Name = br.ReadUTF16();

                br.Position = start + baseDataOffset;
                PartIndex = br.ReadInt32();
                RegionIndex = br.ReadInt32();
                EntityID = br.ReadUInt32();
                UnkE0C = br.ReadByte();
                br.AssertByte(0);
                br.AssertByte(0);
                br.AssertByte(0);

                if (HasTypeData)
                {
                    br.Position = start + typeDataOffset;
                    ReadTypeData(br);
                }

                // Unk3
                br.Position = start + unk3Offset;
                MapID = br.ReadInt32();
                UnkS04 = br.ReadInt32();
                UnkS08 = br.ReadInt32();
                UnkS0C = br.ReadInt32();
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);
            }

            private protected virtual void ReadTypeData(BinaryReaderEx br)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(ReadTypeData)}.");

            internal override void Write(BinaryWriterEx bw, int id)
            {
                long start = bw.Position;
                bw.ReserveInt64("NameOffset");
                bw.WriteInt32(EventID);
                bw.WriteUInt32((uint)Type);
                if (OtherID != -1)
                    bw.WriteInt32(OtherID);
                else
                    bw.WriteInt32(id);
                bw.WriteInt32(Unk14);
                bw.ReserveInt64("BaseDataOffset");
                bw.ReserveInt64("TypeDataOffset");
                bw.ReserveInt64("Unk3Offset");

                bw.FillInt64("NameOffset", bw.Position - start);
                bw.WriteUTF16(MSB.ReambiguateName(Name), true);
                bw.Pad(8);

                bw.FillInt64("BaseDataOffset", bw.Position - start);
                bw.WriteInt32(PartIndex);
                bw.WriteInt32(RegionIndex);
                bw.WriteUInt32(EntityID);
                bw.WriteByte(UnkE0C);
                bw.WriteByte(0);
                bw.WriteByte(0);
                bw.WriteByte(0);

                if (HasTypeData)
                {
                    bw.FillInt64("TypeDataOffset", bw.Position - start);
                    WriteTypeData(bw);
                }
                else
                {
                    bw.FillInt64("TypeDataOffset", 0);
                }

                if (Type == EventType.PseudoMultiplayer)
                    bw.Pad(4);
                else
                    bw.Pad(8);

                bw.FillInt64("Unk3Offset", bw.Position - start);
                bw.WriteInt32(MapID);
                bw.WriteInt32(UnkS04);
                bw.WriteInt32(UnkS08);
                bw.WriteInt32(UnkS0C);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.Pad(8);
            }

            private protected virtual void WriteTypeData(BinaryWriterEx bw)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(ReadTypeData)}.");

            internal virtual void GetNames(MSBE msb, Entries entries)
            {
                PartName = MSB.FindName(entries.Parts, PartIndex);
                RegionName = MSB.FindName(entries.Regions, RegionIndex);
            }

            internal virtual void GetIndices(MSBE msb, Entries entries)
            {
                PartIndex = MSB.FindIndex(this, entries.Parts, PartName);
                RegionIndex = MSB.FindIndex(this, entries.Regions, RegionName);
            }

            /// <summary>
            /// Returns the type and name of the event as a string.
            /// </summary>
            public override string ToString()
            {
                return $"{Type} {Name}";
            }

            /// <summary>
            /// An item pickup in the open or inside a container.
            /// </summary>
            public class Treasure : Event
            {
                private protected override EventType Type => EventType.Treasure;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// The part that the treasure is attached to.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Part))]
                public string TreasurePartName { get; set; }
                private int TreasurePartIndex { get; set; }

                /// <summary>
                /// The item lot to be given.
                /// </summary>
                public int ItemLotID { get; set; }

                /// <summary>
                /// If not -1, uses an entry from ActionButtonParam for the pickup prompt.
                /// </summary>
                public int ActionButtonID { get; set; }

                /// <summary>
                /// Animation to play when taking this treasure.
                /// </summary>
                public int PickupAnimID { get; set; }

                /// <summary>
                /// Changes the text of the pickup prompt.
                /// </summary>
                public byte InChest { get; set; }

                /// <summary>
                /// Whether the treasure should be hidden by default.
                /// </summary>
                public byte StartDisabled { get; set; }

                /// <summary>
                /// Creates a Treasure with default values.
                /// </summary>
                public Treasure() : base($"{nameof(Event)}: {nameof(Treasure)}")
                {
                    ItemLotID = -1;
                    ActionButtonID = -1;
                    PickupAnimID = -1;
                }

                internal Treasure(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    TreasurePartIndex = br.ReadInt32();
                    br.AssertInt32(0);
                    ItemLotID = br.ReadInt32();
                    br.AssertPattern(0x24, 0xFF);
                    ActionButtonID = br.ReadInt32();
                    PickupAnimID = br.ReadInt32();
                    InChest = br.ReadByte();
                    StartDisabled = br.ReadByte();
                    br.AssertInt16(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(TreasurePartIndex);
                    bw.WriteInt32(0);
                    bw.WriteInt32(ItemLotID);
                    bw.WritePattern(0x24, 0xFF);
                    bw.WriteInt32(ActionButtonID);
                    bw.WriteInt32(PickupAnimID);
                    bw.WriteByte(InChest);
                    bw.WriteByte(StartDisabled);
                    bw.WriteInt16(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }

                internal override void GetNames(MSBE msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    TreasurePartName = MSB.FindName(entries.Parts, TreasurePartIndex);
                }

                internal override void GetIndices(MSBE msb, Entries entries)
                {
                    base.GetIndices(msb, entries);
                    TreasurePartIndex = MSB.FindIndex(this, entries.Parts, TreasurePartName);
                }
            }

            /// <summary>
            /// An enemy spawner.
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
                public float UnkT14 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT18 { get; set; }

                /// <summary>
                /// Regions where parts will spawn from.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Region))]
                public string[] SpawnRegionNames { get; set; }

                private int[] SpawnRegionIndices { get; set; }

                /// <summary>
                /// Parts that will be respawned.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Part))]
                public string[] SpawnPartNames { get; set; }

                private int[] SpawnPartIndices { get; set; }

                /// <summary>
                /// Creates a Generator with default values.
                /// </summary>
                public Generator() : base($"{nameof(Event)}: {nameof(Generator)}")
                {
                    SpawnRegionNames = new string[8];
                    SpawnPartNames = new string[32];
                }

                private protected override void DeepCopyTo(Event evnt)
                {
                    var generator = (Generator)evnt;
                    generator.SpawnRegionNames = (string[])SpawnRegionNames.Clone();
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
                    br.AssertByte(0);
                    br.AssertByte(0);
                    br.AssertByte(0);
                    UnkT14 = br.ReadSingle();
                    UnkT18 = br.ReadSingle();
                    br.AssertPattern(0x14, 0x00);
                    SpawnRegionIndices = br.ReadInt32s(8);
                    br.AssertPattern(0x10, 0x00);
                    SpawnPartIndices = br.ReadInt32s(32);
                    br.AssertPattern(0x20, 0x00);
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
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteSingle(UnkT14);
                    bw.WriteSingle(UnkT18);
                    bw.WritePattern(0x14, 0x00);
                    bw.WriteInt32s(SpawnRegionIndices);
                    bw.WritePattern(0x10, 0x00);
                    bw.WriteInt32s(SpawnPartIndices);
                    bw.WritePattern(0x20, 0x00);
                }

                internal override void GetNames(MSBE msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    SpawnRegionNames = MSB.FindNames(entries.Regions, SpawnRegionIndices);
                    SpawnPartNames = MSB.FindNames(entries.Parts, SpawnPartIndices);
                }

                internal override void GetIndices(MSBE msb, Entries entries)
                {
                    base.GetIndices(msb, entries);
                    SpawnRegionIndices = MSB.FindIndices(this, entries.Regions, SpawnRegionNames);
                    SpawnPartIndices = MSB.FindIndices(this, entries.Parts, SpawnPartNames);
                }
            }

            /// <summary>
            /// An interactive object.
            /// </summary>
            public class ObjAct : Event
            {
                private protected override EventType Type => EventType.ObjAct;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Unknown why objacts need an extra entity ID.
                /// </summary>
                public uint ObjActEntityID { get; set; }

                /// <summary>
                /// The part to be interacted with.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Part))]
                public string ObjActPartName { get; set; }
                private int ObjActPartIndex { get; set; }

                /// <summary>
                /// A row in ObjActParam.
                /// </summary>
                public int ObjActID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte StateType { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public uint EventFlagID { get; set; }

                /// <summary>
                /// Creates an ObjAct with default values.
                /// </summary>
                public ObjAct() : base($"{nameof(Event)}: {nameof(ObjAct)}")
                {
                    ObjActEntityID = 0;
                    ObjActID = -1;
                    EventFlagID = 0;
                }

                internal ObjAct(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    ObjActEntityID = br.ReadUInt32();
                    ObjActPartIndex = br.ReadInt32();
                    ObjActID = br.ReadInt32();
                    StateType = br.ReadByte();
                    br.AssertByte(0);
                    br.AssertInt16(0);
                    EventFlagID = br.ReadUInt32();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteUInt32(ObjActEntityID);
                    bw.WriteInt32(ObjActPartIndex);
                    bw.WriteInt32(ObjActID);
                    bw.WriteByte(StateType);
                    bw.WriteByte(0);
                    bw.WriteInt16(0);
                    bw.WriteUInt32(EventFlagID);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }

                internal override void GetNames(MSBE msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    ObjActPartName = MSB.FindName(entries.Parts, ObjActPartIndex);
                }

                internal override void GetIndices(MSBE msb, Entries entries)
                {
                    base.GetIndices(msb, entries);
                    ObjActPartIndex = MSB.FindIndex(this, entries.Parts, ObjActPartName);
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
                private int NavmeshRegionIndex { get; set; }

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

                internal override void GetNames(MSBE msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    NavmeshRegionName = MSB.FindName(entries.Regions, NavmeshRegionIndex);
                }

                internal override void GetIndices(MSBE msb, Entries entries)
                {
                    base.GetIndices(msb, entries);
                    NavmeshRegionIndex = MSB.FindIndex(this, entries.Regions, NavmeshRegionName);
                }
            }

            /// <summary>
            /// A fake multiplayer interaction where the player goes to an NPC's world.
            /// </summary>
            public class PseudoMultiplayer : Event
            {
                private protected override EventType Type => EventType.PseudoMultiplayer;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// The NPC whose world you're entering.
                /// </summary>
                public uint HostEntityID { get; set; }

                /// <summary>
                /// Set when inside the event's region, unset when outside it.
                /// </summary>
                public uint EventFlagID { get; set; }

                /// <summary>
                /// ID of a goods item that is used to trigger the event.
                /// </summary>
                public int ActivateGoodsID { get; set; }

                /// <summary>
                /// Unknown; possibly a sound ID.
                /// </summary>
                public int UnkT0C { get; set; }

                /// <summary>
                /// Unknown; possibly a map event ID.
                /// </summary>
                public int UnkT10 { get; set; }

                /// <summary>
                /// Unknown; possibly flags.
                /// </summary>
                public int UnkT14 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT18 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT1C { get; set; }

                /// <summary>
                /// Creates a new PseudoMultiplayer with the given name.
                /// </summary>
                public PseudoMultiplayer() : base($"{nameof(Event)}: {nameof(PseudoMultiplayer)}")
                {
                    HostEntityID = 0;
                    EventFlagID = 0;
                    ActivateGoodsID = -1;
                    UnkT0C = -1;
                    UnkT10 = -1;
                    UnkT1C = 0;
                }

                internal PseudoMultiplayer(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    HostEntityID = br.ReadUInt32();
                    EventFlagID = br.ReadUInt32();
                    ActivateGoodsID = br.ReadInt32();
                    UnkT0C = br.ReadInt32();
                    UnkT10 = br.ReadInt32();
                    UnkT14 = br.ReadInt32();
                    UnkT18 = br.ReadInt32();
                    UnkT1C = br.ReadInt32();
                    br.AssertInt32(-1);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteUInt32(HostEntityID);
                    bw.WriteUInt32(EventFlagID);
                    bw.WriteInt32(ActivateGoodsID);
                    bw.WriteInt32(UnkT0C);
                    bw.WriteInt32(UnkT10);
                    bw.WriteInt32(UnkT14);
                    bw.WriteInt32(UnkT18);
                    bw.WriteInt32(UnkT1C);
                    bw.WriteInt32(-1);
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
                public string[] GroupPartsNames { get; set; }

                private int[] GroupPartsIndices { get; set; }

                /// <summary>
                /// Creates a PlatoonInfo with default values.
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

                internal override void GetNames(MSBE msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    GroupPartsNames = MSB.FindNames(entries.Parts, GroupPartsIndices);
                }

                internal override void GetIndices(MSBE msb, Entries entries)
                {
                    base.GetIndices(msb, entries);
                    GroupPartsIndices = MSB.FindIndices(this, entries.Parts, GroupPartsNames);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class PatrolInfo : Event
            {
                private protected override EventType Type => EventType.PatrolInfo;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Determines patrol behavior. 0 = return to first region on loop, 1 = go through list backwards on loop, etc.
                /// </summary>
                public byte PatrolType { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Region))]
                public string[] WalkRegionNames { get; set; }

                private short[] WalkRegionIndices { get; set; }

                /// <summary>
                /// Creates a PatrolInfo with default values.
                /// </summary>
                public PatrolInfo() : base($"{nameof(Event)}: {nameof(PatrolInfo)}")
                {
                    WalkRegionIndices = new short[64];
                    WalkRegionNames = new string[64];
                }

                private protected override void DeepCopyTo(Event evnt)
                {
                    var walkRoute = (PatrolInfo)evnt;
                    walkRoute.WalkRegionNames = (string[])WalkRegionNames.Clone();
                }

                internal PatrolInfo(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    PatrolType = br.ReadByte();
                    br.AssertByte(0);
                    br.AssertByte(0);
                    br.AssertByte(1);
                    br.AssertInt32(-1);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    WalkRegionIndices = br.ReadInt16s(64);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteByte(PatrolType);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteByte(1);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt16s(WalkRegionIndices);
                }

                internal override void GetNames(MSBE msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    WalkRegionNames = new string[WalkRegionIndices.Length];
                    for (int i = 0; i < WalkRegionIndices.Length; i++)
                        WalkRegionNames[i] = MSB.FindName(entries.Regions, WalkRegionIndices[i]);
                }

                internal override void GetIndices(MSBE msb, Entries entries)
                {
                    base.GetIndices(msb, entries);
                    WalkRegionIndices = new short[WalkRegionNames.Length];
                    for (int i = 0; i < WalkRegionNames.Length; i++)
                        WalkRegionIndices[i] = (short)MSB.FindIndex(this, entries.Regions, WalkRegionNames[i]);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class Mount : Event
            {
                private protected override EventType Type => EventType.Mount;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Unknown.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Part))]
                public string RiderPartName { get; set; }
                private int RiderPartIndex { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Part))]
                public string MountPartName { get; set; }
                private int MountPartIndex { get; set; }

                /// <summary>
                /// Creates a Mount with default values.
                /// </summary>
                public Mount() : base($"{nameof(Event)}: {nameof(Mount)}")
                {
                    RiderPartIndex = -1;
                    MountPartIndex = -1;
                }

                internal Mount(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    RiderPartIndex = br.ReadInt32();
                    MountPartIndex = br.ReadInt32();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(RiderPartIndex);
                    bw.WriteInt32(MountPartIndex);
                }

                internal override void GetNames(MSBE msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    RiderPartName = MSB.FindName(entries.Parts, RiderPartIndex);
                    MountPartName = MSB.FindName(entries.Parts, MountPartIndex);
                }

                internal override void GetIndices(MSBE msb, Entries entries)
                {
                    base.GetIndices(msb, entries);
                    RiderPartIndex = MSB.FindIndex(this, entries.Parts, RiderPartName);
                    MountPartIndex = MSB.FindIndex(this, entries.Parts, MountPartName);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class SignPool : Event
            {
                private protected override EventType Type => EventType.SignPool;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Unknown.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Part))]
                public string SignPartName { get; set; }
                private int SignPartIndex { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int SignPuddleParamID { get; set; }

                /// <summary>
                /// Creates a SignPool with default values.
                /// </summary>
                public SignPool() : base($"{nameof(Event)}: {nameof(SignPool)}")
                {
                    SignPartIndex = -1;
                }

                internal SignPool(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    SignPartIndex = br.ReadInt32();
                    SignPuddleParamID = br.ReadInt32();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(SignPartIndex);
                    bw.WriteInt32(SignPuddleParamID);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }

                internal override void GetNames(MSBE msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    SignPartName = MSB.FindName(entries.Parts, SignPartIndex);
                }

                internal override void GetIndices(MSBE msb, Entries entries)
                {
                    base.GetIndices(msb, entries);
                    SignPartIndex = MSB.FindIndex(this, entries.Parts, SignPartName);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class AreaTeam : Event
            {
                private protected override EventType Type => EventType.AreaTeam;
                private protected override bool HasTypeData => true;

                public int EntityID_Leader { get; set; }
                public int EUnk04 { get; set; }
                public int EUnk08 { get; set; }
                public int EUnk0c { get; set; }
                public int RegionID_Leader { get; set; }
                public int RegionID_Guest1 { get; set; }
                public int RegionID_Guest2 { get; set; }
                public int EUnk1c { get; set; }
                public int EUnk20 { get; set; }
                public int EUnk24 { get; set; }
                public int EUnk28 { get; set; }

                /// <summary>
                /// Creates a AreaTeam with default values.
                /// </summary>
                public AreaTeam() : base($"{nameof(Event)}: {nameof(AreaTeam)}")
                {
                }

                internal AreaTeam(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    EntityID_Leader = br.ReadInt32();
                    EUnk04 = br.ReadInt32();
                    EUnk08 = br.ReadInt32();
                    EUnk0c = br.ReadInt32();
                    RegionID_Leader = br.ReadInt32();
                    RegionID_Guest1 = br.ReadInt32();
                    RegionID_Guest2 = br.ReadInt32();
                    EUnk1c = br.ReadInt32();
                    EUnk20 = br.ReadInt32();
                    EUnk24 = br.ReadInt32();
                    EUnk28 = br.ReadInt32();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(EntityID_Leader);
                    bw.WriteInt32(EUnk04);
                    bw.WriteInt32(EUnk08);
                    bw.WriteInt32(EUnk0c);
                    bw.WriteInt32(RegionID_Leader);
                    bw.WriteInt32(RegionID_Guest1);
                    bw.WriteInt32(RegionID_Guest2);
                    bw.WriteInt32(EUnk1c);
                    bw.WriteInt32(EUnk20);
                    bw.WriteInt32(EUnk24);
                    bw.WriteInt32(EUnk28);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class RetryPoint : Event
            {
                private protected override EventType Type => EventType.RetryPoint;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// The asset name that the stake is represented by.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Part))]
                public string RetryPartName { get; set; }
                private int RetryPartIndex { get; set; }

                /// <summary>
                /// Flag that must be set for stake to be available.
                /// </summary>
                public uint TriggerEventFlagID { get; set; }

                /// <summary>
                /// The distance at which the retry point is active (only used if no retry region is set).
                /// </summary>
                public float RetryDistance { get; set; }

                /// <summary>
                /// The region in which the retry point is active.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Region))]
                public string RetryRegionName { get; set; }
                private short RetryRegionIndex { get; set; }

                /// <summary>
                /// Creates a RetryPoint with default values.
                /// </summary>
                public RetryPoint() : base($"{nameof(Event)}: {nameof(RetryPoint)}") { }

                internal RetryPoint(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    RetryPartIndex = br.ReadInt32();
                    TriggerEventFlagID = br.ReadUInt32();
                    RetryDistance = br.ReadSingle();
                    RetryRegionIndex = br.ReadInt16();
                    br.AssertInt16(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(RetryPartIndex);
                    bw.WriteUInt32(TriggerEventFlagID);
                    bw.WriteSingle(RetryDistance);
                    bw.WriteInt16(RetryRegionIndex);
                    bw.WriteInt16(0);
                }

                internal override void GetNames(MSBE msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    RetryPartName = MSB.FindName(entries.Parts, RetryPartIndex);
                    RetryRegionName = MSB.FindName(entries.Regions, RetryRegionIndex);
                }

                internal override void GetIndices(MSBE msb, Entries entries)
                {
                    base.GetIndices(msb, entries);
                    RetryPartIndex = MSB.FindIndex(this, entries.Parts, RetryPartName);
                    RetryRegionIndex = (short)MSB.FindIndex(this, entries.Regions, RetryRegionName);
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
