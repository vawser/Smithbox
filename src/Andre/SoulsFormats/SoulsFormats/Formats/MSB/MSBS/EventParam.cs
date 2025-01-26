using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace SoulsFormats
{
    public partial class MSBS
    {
        internal enum EventType : uint
        {
            //Light = 0,
            //Sound = 1,
            //SFX = 2,
            //Wind = 3,
            Treasure = 4,
            Generator = 5,
            //Message = 6,
            ObjAct = 7,
            //SpawnPoint = 8,
            MapOffset = 9,
            //Navmesh = 10,
            //Environment = 11,
            //PseudoMultiplayer = 12,
            //WindSFX = 13,
            PatrolInfo = 14,
            PlatoonInfo = 15,
            //DarkSight = 16,
            ResourceItemInfo = 17,
            GrassLodParam = 18,
            //AutoDrawGroupSettings = 19,
            SkitInfo = 20,
            PlacementGroup = 21,
            PartsGroup = 22,
            Talk = 23,
            AutoDrawGroupCollision = 24,
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
            /// Indicates a shift of the entire map; already accounted for in MSB positions, but must be applied to other formats such as BTL. Should only be one per map, if any.
            /// </summary>
            public List<Event.MapOffset> MapOffsets { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Event.PatrolInfo> PatrolInfo { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Event.PlatoonInfo> PlatoonInfo { get; set; }

            /// <summary>
            /// Resource items such as spirit emblems placed in the map.
            /// </summary>
            public List<Event.ResourceItemInfo> ResourceItemInfo { get; set; }

            /// <summary>
            /// Sets the grass lod parameters for the map. Should only be one per map, if any.
            /// </summary>
            public List<Event.GrassLodParam> GrassLodParams { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Event.SkitInfo> SkitInfo { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Event.PlacementGroup> PlacementGroups { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Event.PartsGroup> PartsGroups { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Event.Talk> Talks { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Event.AutoDrawGroupCollision> AutoDrawGroupCollisions { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Event.Other> Others { get; set; }

            /// <summary>
            /// Creates an empty EventParam with the default version.
            /// </summary>
            public EventParam() : base(35, "EVENT_PARAM_ST")
            {
                Treasures = new List<Event.Treasure>();
                Generators = new List<Event.Generator>();
                ObjActs = new List<Event.ObjAct>();
                MapOffsets = new List<Event.MapOffset>();
                PatrolInfo = new List<Event.PatrolInfo>();
                PlatoonInfo = new List<Event.PlatoonInfo>();
                ResourceItemInfo = new List<Event.ResourceItemInfo>();
                GrassLodParams = new List<Event.GrassLodParam>();
                SkitInfo = new List<Event.SkitInfo>();
                PlacementGroups = new List<Event.PlacementGroup>();
                PartsGroups = new List<Event.PartsGroup>();
                Talks = new List<Event.Talk>();
                AutoDrawGroupCollisions = new List<Event.AutoDrawGroupCollision>();
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
                    case Event.MapOffset e: MapOffsets.Add(e); break;
                    case Event.PatrolInfo e: PatrolInfo.Add(e); break;
                    case Event.PlatoonInfo e: PlatoonInfo.Add(e); break;
                    case Event.ResourceItemInfo e: ResourceItemInfo.Add(e); break;
                    case Event.GrassLodParam e: GrassLodParams.Add(e); break;
                    case Event.SkitInfo e: SkitInfo.Add(e); break;
                    case Event.PlacementGroup e: PlacementGroups.Add(e); break;
                    case Event.PartsGroup e: PartsGroups.Add(e); break;
                    case Event.Talk e: Talks.Add(e); break;
                    case Event.AutoDrawGroupCollision e: AutoDrawGroupCollisions.Add(e); break;
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
                    Treasures, Generators, ObjActs, MapOffsets, PatrolInfo,
                    PlatoonInfo, ResourceItemInfo, GrassLodParams, SkitInfo, PlacementGroups,
                    PartsGroups, Talks, AutoDrawGroupCollisions, Others);
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

                    case EventType.MapOffset:
                        return MapOffsets.EchoAdd(new Event.MapOffset(br));

                    case EventType.PatrolInfo:
                        return PatrolInfo.EchoAdd(new Event.PatrolInfo(br));

                    case EventType.PlatoonInfo:
                        return PlatoonInfo.EchoAdd(new Event.PlatoonInfo(br));

                    case EventType.ResourceItemInfo:
                        return ResourceItemInfo.EchoAdd(new Event.ResourceItemInfo(br));

                    case EventType.GrassLodParam:
                        return GrassLodParams.EchoAdd(new Event.GrassLodParam(br));

                    case EventType.SkitInfo:
                        return SkitInfo.EchoAdd(new Event.SkitInfo(br));

                    case EventType.PlacementGroup:
                        return PlacementGroups.EchoAdd(new Event.PlacementGroup(br));

                    case EventType.PartsGroup:
                        return PartsGroups.EchoAdd(new Event.PartsGroup(br));

                    case EventType.Talk:
                        return Talks.EchoAdd(new Event.Talk(br));

                    case EventType.AutoDrawGroupCollision:
                        return AutoDrawGroupCollisions.EchoAdd(new Event.AutoDrawGroupCollision(br));

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
            /// Unknown.
            /// </summary>
            public int EventID { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            [MSBReference(ReferenceType = typeof(Part))]
            public string PartName { get; set; }
            public int PartIndex { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            [MSBReference(ReferenceType = typeof(Region))]
            public string RegionName { get; set; }
            public int RegionIndex { get; set; }

            /// <summary>
            /// Identifies the Event in event scripts.
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
                long nameOffset = br.ReadInt64();
                EventID = br.ReadInt32();
                br.AssertUInt32((uint)Type);
                br.ReadInt32(); // ID
                br.AssertInt32(0);
                long baseDataOffset = br.ReadInt64();
                long typeDataOffset = br.ReadInt64();

                if (nameOffset == 0)
                    throw new InvalidDataException($"{nameof(nameOffset)} must not be 0 in type {GetType()}.");
                if (baseDataOffset == 0)
                    throw new InvalidDataException($"{nameof(baseDataOffset)} must not be 0 in type {GetType()}.");
                if (HasTypeData ^ typeDataOffset != 0)
                    throw new InvalidDataException($"Unexpected {nameof(typeDataOffset)} 0x{typeDataOffset:X} in type {GetType()}.");

                br.Position = start + nameOffset;
                Name = br.ReadUTF16();

                br.Position = start + baseDataOffset;
                PartIndex = br.ReadInt32();
                RegionIndex = br.ReadInt32();
                EntityID = br.ReadInt32();
                br.AssertInt32(0);

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
                bw.ReserveInt64("BaseDataOffset");
                bw.ReserveInt64("TypeDataOffset");

                bw.FillInt64("NameOffset", bw.Position - start);
                bw.WriteUTF16(Name, true);
                bw.Pad(8);

                bw.FillInt64("BaseDataOffset", bw.Position - start);
                bw.WriteInt32(PartIndex);
                bw.WriteInt32(RegionIndex);
                bw.WriteInt32(EntityID);
                bw.WriteInt32(0);

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
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(ReadTypeData)}.");

            internal virtual void GetNames(MSBS msb, Entries entries)
            {
                PartName = MSB.FindName(entries.Parts, PartIndex);
                RegionName = MSB.FindName(entries.Regions, RegionIndex);
            }

            internal virtual void GetIndices(MSBS msb, Entries entries)
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
                [MSBReference(ReferenceType = typeof(Region))]
                public string TreasurePartName { get; set; }
                public int TreasurePartIndex { get; set; }

                /// <summary>
                /// The item lot to be given.
                /// </summary>
                public int ItemLotID { get; set; }

                /// <summary>
                /// If not -1, uses an entry from ActionButtonParam for the pickup prompt.
                /// </summary>
                [MSBParamReference(ParamName = "ActionButtonParam")]
                public int ActionButtonID { get; set; }

                /// <summary>
                /// Animation to play when taking this treasure.
                /// </summary>
                public int PickupAnimID { get; set; }

                /// <summary>
                /// Changes the text of the pickup prompt.
                /// </summary>
                public bool InChest { get; set; }

                /// <summary>
                /// Whether the treasure should be hidden by default.
                /// </summary>
                public bool StartDisabled { get; set; }

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
                    InChest = br.ReadBoolean();
                    StartDisabled = br.ReadBoolean();
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
                    bw.WriteBoolean(InChest);
                    bw.WriteBoolean(StartDisabled);
                    bw.WriteInt16(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }

                internal override void GetNames(MSBS msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    TreasurePartName = MSB.FindName(entries.Parts, TreasurePartIndex);
                }

                internal override void GetIndices(MSBS msb, Entries entries)
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
                public string[] SpawnRegionNames { get; private set; }
                public int[] SpawnRegionIndices;

                /// <summary>
                /// Parts that will be respawned.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Part))]
                public string[] SpawnPartNames { get; private set; }
                public int[] SpawnPartIndices;

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

                internal override void GetNames(MSBS msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    SpawnRegionNames = MSB.FindNames(entries.Regions, SpawnRegionIndices);
                    SpawnPartNames = MSB.FindNames(entries.Parts, SpawnPartIndices);
                }

                internal override void GetIndices(MSBS msb, Entries entries)
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
                public int ObjActEntityID { get; set; }

                /// <summary>
                /// The part to be interacted with.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Part))]
                public string ObjActPartName { get; set; }
                public int ObjActPartIndex { get; set; }

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
                public int EventFlagID { get; set; }

                /// <summary>
                /// Creates an ObjAct with default values.
                /// </summary>
                public ObjAct() : base($"{nameof(Event)}: {nameof(ObjAct)}")
                {
                    ObjActEntityID = -1;
                    ObjActID = -1;
                    EventFlagID = -1;
                }

                internal ObjAct(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    ObjActEntityID = br.ReadInt32();
                    ObjActPartIndex = br.ReadInt32();
                    ObjActID = br.ReadInt32();
                    StateType = br.ReadByte();
                    br.AssertByte(0);
                    br.AssertInt16(0);
                    EventFlagID = br.ReadInt32();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(ObjActEntityID);
                    bw.WriteInt32(ObjActPartIndex);
                    bw.WriteInt32(ObjActID);
                    bw.WriteByte(StateType);
                    bw.WriteByte(0);
                    bw.WriteInt16(0);
                    bw.WriteInt32(EventFlagID);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }

                internal override void GetNames(MSBS msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    ObjActPartName = MSB.FindName(entries.Parts, ObjActPartIndex);
                }

                internal override void GetIndices(MSBS msb, Entries entries)
                {
                    base.GetIndices(msb, entries);
                    ObjActPartIndex = MSB.FindIndex(this, entries.Parts, ObjActPartName);
                }
            }

            /// <summary>
            /// Shifts the entire map; already accounted for in MSB coordinates.
            /// </summary>
            public class MapOffset : Event
            {
                private protected override EventType Type => EventType.MapOffset;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// How much to shift by.
                /// </summary>
                public Vector3 Position { get; set; }

                /// <summary>
                /// Unknown, but looks like rotation.
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
            public class PatrolInfo : Event
            {
                private protected override EventType Type => EventType.PatrolInfo;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Determines patrol behavior. 0 = return to first region on loop, 1 = go through list backwards on loop, etc.
                /// </summary>
                public int PatrolType { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Region))]
                public string[] WalkRegionNames { get; private set; }
                public short[] WalkRegionIndices;

                /// <summary>
                /// Unknown.
                /// </summary>
                public WREntry[] WREntries { get; set; }

                /// <summary>
                /// Creates a PatrolInfo with default values.
                /// </summary>
                public PatrolInfo() : base($"{nameof(Event)}: {nameof(PatrolInfo)}")
                {
                    WalkRegionNames = new string[32];
                    WREntries = new WREntry[5];
                    for (int i = 0; i < 5; i++)
                        WREntries[i] = new WREntry();
                }

                private protected override void DeepCopyTo(Event evnt)
                {
                    var walkRoute = (PatrolInfo)evnt;
                    walkRoute.WalkRegionNames = (string[])WalkRegionNames.Clone();
                    walkRoute.WREntries = new WREntry[WREntries.Length];
                    for (int i = 0; i < WREntries.Length; i++)
                        walkRoute.WREntries[i] = WREntries[i].DeepCopy();
                }

                internal PatrolInfo(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    PatrolType = br.ReadInt32();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    WalkRegionIndices = br.ReadInt16s(32);
                    WREntries = new WREntry[5];
                    for (int i = 0; i < 5; i++)
                        WREntries[i] = new WREntry(br);
                    br.AssertPattern(0x14, 0x00);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(PatrolType);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt16s(WalkRegionIndices);
                    for (int i = 0; i < 5; i++)
                        WREntries[i].Write(bw);
                    bw.WritePattern(0x14, 0x00);
                }

                internal override void GetNames(MSBS msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    WalkRegionNames = new string[WalkRegionIndices.Length];
                    for (int i = 0; i < WalkRegionIndices.Length; i++)
                        WalkRegionNames[i] = MSB.FindName(entries.Regions, WalkRegionIndices[i]);

                    foreach (WREntry wrEntry in WREntries)
                        wrEntry.GetNames(entries);
                }

                internal override void GetIndices(MSBS msb, Entries entries)
                {
                    base.GetIndices(msb, entries);
                    WalkRegionIndices = new short[WalkRegionNames.Length];
                    for (int i = 0; i < WalkRegionNames.Length; i++)
                        WalkRegionIndices[i] = (short)MSB.FindIndex(this, entries.Regions, WalkRegionNames[i]);

                    foreach (WREntry wrEntry in WREntries)
                        wrEntry.GetIndices(this, entries);
                }

                /// <summary>
                /// Unknown.
                /// </summary>
                public class WREntry
                {
                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    [MSBReference(ReferenceType = typeof(Region))]
                    public string RegionName { get; set; }
                    public short RegionIndex;

                    /// <summary>
                    /// ID of animation to play when arriving at destination region.
                    /// </summary>
                    public int ArrivalAnimID { get; set; }

                    /// <summary>
                    /// Time in seconds to wait (after animation finishes) after arriving at destination region.
                    /// </summary>
                    public int ArrivalWaitTime { get; set; }

                    /// <summary>
                    /// Creates a WREntry with default values.
                    /// </summary>
                    public WREntry() { }

                    /// <summary>
                    /// Creates a deep copy of the entry.
                    /// </summary>
                    public WREntry DeepCopy()
                    {
                        return (WREntry)MemberwiseClone();
                    }

                    internal WREntry(BinaryReaderEx br)
                    {
                        RegionIndex = br.ReadInt16();
                        br.AssertInt16(0);
                        ArrivalAnimID = br.ReadInt32();
                        ArrivalWaitTime = br.ReadInt32();
                    }

                    internal void Write(BinaryWriterEx bw)
                    {
                        bw.WriteInt16(RegionIndex);
                        bw.WriteInt16(0);
                        bw.WriteInt32(ArrivalAnimID);
                        bw.WriteInt32(ArrivalWaitTime);
                    }

                    internal void GetNames(Entries entries)
                    {
                        RegionName = MSB.FindName(entries.Regions, RegionIndex);
                    }

                    internal void GetIndices(IMsbEntry entry, Entries entries)
                    {
                        RegionIndex = (short)MSB.FindIndex(entry, entries.Regions, RegionName);
                    }
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
                public int PlatoonIDScriptActive { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int State { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Part))]
                public string[] GroupPartNames { get; private set; }
                public int[] GroupPartIndices;

                /// <summary>
                /// Creates a PlatoonInfo with default values.
                /// </summary>
                public PlatoonInfo() : base($"{nameof(Event)}: {nameof(PlatoonInfo)}")
                {
                    GroupPartNames = new string[32];
                }

                private protected override void DeepCopyTo(Event evnt)
                {
                    var groupTour = (PlatoonInfo)evnt;
                    groupTour.GroupPartNames = (string[])GroupPartNames.Clone();
                }

                internal PlatoonInfo(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    PlatoonIDScriptActive = br.ReadInt32();
                    State = br.ReadInt32();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    GroupPartIndices = br.ReadInt32s(32);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(PlatoonIDScriptActive);
                    bw.WriteInt32(State);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32s(GroupPartIndices);
                }

                internal override void GetNames(MSBS msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    GroupPartNames = MSB.FindNames(entries.Parts, GroupPartIndices);
                }

                internal override void GetIndices(MSBS msb, Entries entries)
                {
                    base.GetIndices(msb, entries);
                    GroupPartIndices = MSB.FindIndices(this, entries.Parts, GroupPartNames);
                }
            }

            /// <summary>
            /// A resource item placed in the map; uses the base Event's region for positioning.
            /// </summary>
            public class ResourceItemInfo : Event
            {
                private protected override EventType Type => EventType.ResourceItemInfo;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// ID of a row in ResourceItemLotParam that determines the resource(s) to give.
                /// </summary>
                public int ResourceItemLotParamID { get; set; }

                /// <summary>
                /// Creates a ResourceItemInfo with default values.
                /// </summary>
                public ResourceItemInfo() : base($"{nameof(Event)}: {nameof(ResourceItemInfo)}") { }

                internal ResourceItemInfo(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    ResourceItemLotParamID = br.ReadInt32();
                    br.AssertPattern(0x1C, 0x00);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(ResourceItemLotParamID);
                    bw.WritePattern(0x1C, 0x00);
                }
            }

            /// <summary>
            /// Sets the grass lod parameters for the map.
            /// </summary>
            public class GrassLodParam : Event
            {
                private protected override EventType Type => EventType.GrassLodParam;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// ID of a row in GrassLodRangeParam.
                /// </summary>
                public int GrassLodRangeParamID { get; set; }

                /// <summary>
                /// Creates a GrassLodParam with default values.
                /// </summary>
                public GrassLodParam() : base($"{nameof(Event)}: {nameof(GrassLodParam)}") { }

                internal GrassLodParam(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    GrassLodRangeParamID = br.ReadInt32();
                    br.AssertPattern(0x1C, 0x00);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(GrassLodRangeParamID);
                    bw.WritePattern(0x1C, 0x00);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class SkitInfo : Event
            {
                private protected override EventType Type => EventType.SkitInfo;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT00 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT04 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT05 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT06 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT07 { get; set; }

                /// <summary>
                /// Creates a SkitInfo with default values.
                /// </summary>
                public SkitInfo() : base($"{nameof(Event)}: {nameof(SkitInfo)}") { }

                internal SkitInfo(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    UnkT00 = br.ReadInt32();
                    UnkT04 = br.ReadByte();
                    UnkT05 = br.ReadByte();
                    UnkT06 = br.ReadByte();
                    UnkT07 = br.ReadByte();
                    br.AssertPattern(0x18, 0x00);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(UnkT00);
                    bw.WriteByte(UnkT04);
                    bw.WriteByte(UnkT05);
                    bw.WriteByte(UnkT06);
                    bw.WriteByte(UnkT07);
                    bw.WritePattern(0x18, 0x00);
                }
            }

            /// <summary>
            /// Associates enemies.
            /// Note: bRestrictSamePlacementGroup in AISoundParam makes sounds spawned by enemies in
            /// a PlacementGroup only heard by other enemies in the same group.
            /// </summary>
            public class PlacementGroup : Event
            {
                private protected override EventType Type => EventType.PlacementGroup;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Unknown.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Part.EnemyBase))]
                public string[] PlacementGroupEnemyNames { get; private set; }
                public int[] PlacementGroupEnemyIndices;

                /// <summary>
                /// Creates a PlacementGroup with default values.
                /// </summary>
                public PlacementGroup() : base($"{nameof(Event)}: {nameof(PlacementGroup)}")
                {
                    PlacementGroupEnemyNames = new string[32];
                }

                private protected override void DeepCopyTo(Event evnt)
                {
                    var placementGroup = (PlacementGroup)evnt;
                    placementGroup.PlacementGroupEnemyNames = (string[])PlacementGroupEnemyNames.Clone();
                }

                internal PlacementGroup(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    PlacementGroupEnemyIndices = br.ReadInt32s(32);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32s(PlacementGroupEnemyIndices);
                }

                internal override void GetNames(MSBS msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    PlacementGroupEnemyNames = MSB.FindNames(entries.Parts, PlacementGroupEnemyIndices);
                }

                internal override void GetIndices(MSBS msb, Entries entries)
                {
                    base.GetIndices(msb, entries);
                    PlacementGroupEnemyIndices = MSB.FindIndices(this, entries.Parts, PlacementGroupEnemyNames);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class PartsGroup : Event
            {
                private protected override EventType Type => EventType.PartsGroup;
                private protected override bool HasTypeData => false;

                /// <summary>
                /// Creates a PartsGroup with default values.
                /// </summary>
                public PartsGroup() : base($"{nameof(Event)}: {nameof(PartsGroup)}") { }

                internal PartsGroup(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class Talk : Event
            {
                private protected override EventType Type => EventType.Talk;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT00 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Part.EnemyBase))]
                public string[] EnemyNames { get; private set; }
                public int[] EnemyIndices;

                /// <summary>
                /// IDs of talk ESDs.
                /// </summary>
                public int[] TalkIDs { get; private set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short UnkT44 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short UnkT46 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT48 { get; set; }

                /// <summary>
                /// Creates a Talk with default values.
                /// </summary>
                public Talk() : base($"{nameof(Event)}: {nameof(Talk)}")
                {
                    EnemyNames = new string[8];
                    TalkIDs = new int[8];
                }

                private protected override void DeepCopyTo(Event evnt)
                {
                    var talk = (Talk)evnt;
                    talk.EnemyNames = (string[])EnemyNames.Clone();
                    talk.TalkIDs = (int[])TalkIDs.Clone();
                }

                internal Talk(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    UnkT00 = br.ReadInt32();
                    EnemyIndices = br.ReadInt32s(8);
                    TalkIDs = br.ReadInt32s(8);
                    UnkT44 = br.ReadInt16();
                    UnkT46 = br.ReadInt16();
                    UnkT48 = br.ReadInt32();
                    br.AssertPattern(0x34, 0x00);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(UnkT00);
                    bw.WriteInt32s(EnemyIndices);
                    bw.WriteInt32s(TalkIDs);
                    bw.WriteInt16(UnkT44);
                    bw.WriteInt16(UnkT46);
                    bw.WriteInt32(UnkT48);
                    bw.WritePattern(0x34, 0x00);
                }

                internal override void GetNames(MSBS msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    EnemyNames = MSB.FindNames(msb.Parts.Enemies, EnemyIndices);
                }

                internal override void GetIndices(MSBS msb, Entries entries)
                {
                    base.GetIndices(msb, entries);
                    EnemyIndices = MSB.FindIndices(this, msb.Parts.Enemies, EnemyNames);
                }
            }

            /// <summary>
            /// Specifies a collision to which an autodrawgroup filming point belongs, whatever that means.
            /// </summary>
            public class AutoDrawGroupCollision : Event
            {
                private protected override EventType Type => EventType.AutoDrawGroupCollision;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Name of the filming point for the autodrawgroup capture, probably.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Region))]
                public string AutoDrawGroupPointName { get; set; }
                public int AutoDrawGroupPointIndex { get; set; }

                /// <summary>
                /// The collision that the filming point belongs to, presumably.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Part.Collision))]
                public string OwningCollisionName { get; set; }
                public int OwningCollisionIndex { get; set; }

                /// <summary>
                /// Creates an AutoDrawGroupCollision with default values.
                /// </summary>
                public AutoDrawGroupCollision() : base($"{nameof(Event)}: {nameof(AutoDrawGroupCollision)}") { }

                internal AutoDrawGroupCollision(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    AutoDrawGroupPointIndex = br.ReadInt32();
                    OwningCollisionIndex = br.ReadInt32();
                    br.AssertPattern(0x18, 0x00);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(AutoDrawGroupPointIndex);
                    bw.WriteInt32(OwningCollisionIndex);
                    bw.WritePattern(0x18, 0x00);
                }

                internal override void GetNames(MSBS msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    AutoDrawGroupPointName = MSB.FindName(msb.Regions.AutoDrawGroupPoints, AutoDrawGroupPointIndex);
                    OwningCollisionName = MSB.FindName(msb.Parts.Collisions, OwningCollisionIndex);
                }

                internal override void GetIndices(MSBS msb, Entries entries)
                {
                    base.GetIndices(msb, entries);
                    AutoDrawGroupPointIndex = MSB.FindIndex(this, msb.Regions.AutoDrawGroupPoints, AutoDrawGroupPointName);
                    OwningCollisionIndex = MSB.FindIndex(this, msb.Parts.Collisions, OwningCollisionName);
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
