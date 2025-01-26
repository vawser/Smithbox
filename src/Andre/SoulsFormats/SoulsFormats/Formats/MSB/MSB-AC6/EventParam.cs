using Org.BouncyCastle.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Xml.Serialization;
using static SoulsFormats.GPARAM;

namespace SoulsFormats
{
    public partial class MSB_AC6
    {
        public enum EventType : int
        {
            Treasure = 4, 
            Generator = 5,
            MapOffset = 9,
            PlatoonInfo = 15, 
            PatrolRoute = 20, 
            MapGimmick = 24, 
            Other = -1, 
        }

        /// <summary>
        /// Dynamic or interactive systems such as item pickups, levers, enemy spawners, etc.
        /// </summary>
        public class EventParam : Param<Event>, IMsbParam<IMsbEvent>
        {
            public int ParamVersion;

            /// <summary>
            /// Item pickups out in the open or inside containers.
            /// </summary>
            public List<Event.Treasure> Treasures { get; set; }

            /// <summary>
            /// Enemy spawners.
            /// </summary>
            public List<Event.Generator> Generators { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public List<Event.MapOffset> MapOffsets { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Event.PlatoonInfo> PlatoonInfo { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Event.PatrolRoute> PatrolRoutes { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Event.MapGimmick> MapGimmicks { get; set; }

            /// <summary>
            /// Other events in the MSB.
            /// </summary>
            public List<Event.Other> Others { get; set; }

            /// <summary>
            /// Creates an empty EventParam with the default version.
            /// </summary>
            public EventParam() : base(52, "EVENT_PARAM_ST")
            {
                ParamVersion = base.Version;

                Treasures = new List<Event.Treasure>();
                Generators = new List<Event.Generator>();
                MapOffsets = new List<Event.MapOffset>();
                PlatoonInfo = new List<Event.PlatoonInfo>();
                PatrolRoutes = new List<Event.PatrolRoute>();
                MapGimmicks = new List<Event.MapGimmick>();
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
                    case Event.MapOffset e: MapOffsets.Add(e); break;
                    case Event.PlatoonInfo e: PlatoonInfo.Add(e); break;
                    case Event.PatrolRoute e: PatrolRoutes.Add(e); break;
                    case Event.MapGimmick e: MapGimmicks.Add(e); break;
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
                    Treasures, 
                    Generators, 
                    MapOffsets, 
                    PlatoonInfo,
                    PatrolRoutes,
                    MapGimmicks,
                    Others);
            }
            IReadOnlyList<IMsbEvent> IMsbParam<IMsbEvent>.GetEntries() => GetEntries();

            internal override Event ReadEntry(BinaryReaderEx br, long offsetLength)
            {
                EventType type = br.GetEnum32<EventType>(br.Position + 0xC);
                switch (type)
                {
                    case EventType.Treasure:
                        return Treasures.EchoAdd(new Event.Treasure(br));

                    case EventType.Generator:
                        return Generators.EchoAdd(new Event.Generator(br));

                    case EventType.MapOffset:
                        return MapOffsets.EchoAdd(new Event.MapOffset(br));

                    case EventType.PlatoonInfo:
                        return PlatoonInfo.EchoAdd(new Event.PlatoonInfo(br));

                    case EventType.PatrolRoute:
                        return PatrolRoutes.EchoAdd(new Event.PatrolRoute(br));

                    case EventType.MapGimmick:
                        return MapGimmicks.EchoAdd(new Event.MapGimmick(br));

                    case EventType.Other:
                        return Others.EchoAdd(new Event.Other(br));

                    default:
                        throw new NotImplementedException($"Unimplemented event type: {type}");
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

            private long NameOffset { get; set; }
            private long CommonOffset { get; set; }
            private long TypeOffset { get; set; }

            public string Name { get; set; }

            public int EventID { get; set; }

            public int TypeIndex { get; set; }

            [MSBReference(ReferenceType = typeof(Part))]
            public string PartName { get; set; }

            private int PartIndex { get; set; }

            [MSBReference(ReferenceType = typeof(Region))]
            public string RegionName { get; set; }

            private int RegionIndex { get; set; }

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

                // Main
                NameOffset = br.ReadInt64();
                EventID = br.ReadInt32();
                br.AssertInt32((int)Type);
                TypeIndex = br.ReadInt32();
                br.AssertInt32(0);

                CommonOffset = br.ReadInt64();
                TypeOffset = br.ReadInt64();

                Name = br.GetUTF16(start + NameOffset);

                // Common
                br.Position = start + CommonOffset;
                PartIndex = br.ReadInt32();
                RegionIndex = br.ReadInt32();
                EntityID = br.ReadInt32();
                br.AssertSByte(-1);
                br.AssertByte(0);
                br.AssertByte(0);
                br.AssertByte(0);

                // TypeData
                if (HasTypeData && TypeOffset != 0L)
                {
                    br.Position = start + TypeOffset;
                    ReadTypeData(br);
                }
            }

            private protected virtual void ReadTypeData(BinaryReaderEx br)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(ReadTypeData)}.");

            internal override void Write(BinaryWriterEx bw, int id)
            {
                long start = bw.Position;

                // Main
                bw.ReserveInt64("NameOffset");
                bw.WriteInt32(EventID);
                bw.WriteInt32((int)Type);
                bw.WriteInt32(TypeIndex);
                bw.WriteInt32(0);

                bw.ReserveInt64("CommonOffset");
                bw.ReserveInt64("TypeDataOffset");

                bw.FillInt64("NameOffset", bw.Position - start);
                bw.WriteUTF16(MSB.ReambiguateName(Name), true);
                bw.Pad(8);

                // Common
                bw.FillInt64("CommonOffset", bw.Position - start);
                bw.WriteInt32(PartIndex);
                bw.WriteInt32(RegionIndex);
                bw.WriteInt32(EntityID);
                bw.WriteSByte(-1);
                bw.WriteByte(0);
                bw.WriteByte(0);
                bw.WriteByte(0);

                // TypeData
                if (HasTypeData && Type != EventType.Other)
                {
                    bw.FillInt64("TypeDataOffset", bw.Position - start);
                    WriteTypeData(bw);
                }
                else
                {
                    bw.FillInt64("TypeDataOffset", 0L);
                }
            }

            private protected virtual void WriteTypeData(BinaryWriterEx bw)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(WriteTypeData)}.");

            internal virtual void GetNames(MSB_AC6 msb, Entries entries)
            {
                PartName = MSB.FindName(entries.Parts, PartIndex);
                RegionName = MSB.FindName(entries.Regions, RegionIndex);
            }

            internal virtual void GetIndices(MSB_AC6 msb, Entries entries)
            {
                PartIndex = MSB.FindIndex(this, entries.Parts, PartName);
                RegionIndex = MSB.FindIndex(this, entries.Regions, RegionName);
            }

            /// <summary>
            /// Returns the type and name of the event.
            /// </summary>
            public override string ToString()
            {
                return $"EVENT: {Type} - {Name}";
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
                private int TreasurePartIndex { get; set; }

                /// <summary>
                /// Itemlot given by the treasure.
                /// </summary>
                public int ItemLotID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int ActionButtonID { get; set; }

                /// <summary>
                /// Unknown; possible the pickup anim.
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
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
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
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(ActionButtonID);
                    bw.WriteInt32(PickupAnimID);
                    bw.WriteBoolean(InChest);
                    bw.WriteBoolean(StartDisabled);
                    bw.WriteInt16(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
                internal override void GetNames(MSB_AC6 msb, Entries entries)
                {
                    base.GetNames(msb, entries);

                    TreasurePartName = MSB.FindName(entries.Parts, TreasurePartIndex);
                }

                internal override void GetIndices(MSB_AC6 msb, Entries entries)
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
                public byte GenType { get; set; }

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
                /// Points that enemies may be spawned at.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Region))]
                public string[] SpawnRegionNames { get; private set; }

                private int[] SpawnRegionIndices { get; set; }

                /// <summary>
                /// Enemies to be respawned.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Part))]
                public string[] SpawnPartNames { get; private set; }

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
                    GenType = br.ReadByte();
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
                    bw.WriteByte(GenType);
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

                internal override void GetNames(MSB_AC6 msb, Entries entries)
                {
                    base.GetNames(msb, entries);

                    SpawnRegionNames = MSB.FindNames(entries.Regions, SpawnRegionIndices);
                    SpawnPartNames = MSB.FindNames(entries.Parts, SpawnPartIndices);
                }

                internal override void GetIndices(MSB_AC6 msb, Entries entries)
                {
                    base.GetIndices(msb, entries);

                    SpawnRegionIndices = MSB.FindIndices(this, entries.Regions, SpawnRegionNames);
                    SpawnPartIndices = MSB.FindIndices(this, entries.Parts, SpawnPartNames);
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
                public Vector3 Translation { get; set; }

                /// <summary>
                /// Rotation of the map.
                /// </summary>
                public float Rotation { get; set; }

                /// <summary>
                /// Creates a MapOffset with default values.
                /// </summary>
                public MapOffset() : base($"{nameof(Event)}: {nameof(MapOffset)}") { }

                internal MapOffset(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    Translation = br.ReadVector3();
                    Rotation = br.ReadSingle();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteVector3(Translation);
                    bw.WriteSingle(Rotation);
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
                public int PlatoonScriptID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public bool PlatoonInfo_UnkT04 { get; set; }

                /// <summary>
                /// Unknown. 
                /// Only true in m01_05_30_00.
                /// </summary>
                public bool PlatoonInfo_UnkT05 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Part))]
                public string[] GroupPartsNames { get; set; }

                private int[] GroupPartsIndices { get; set; }

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
                    PlatoonScriptID = br.ReadInt32();
                    PlatoonInfo_UnkT04 = br.ReadBoolean();
                    PlatoonInfo_UnkT05 = br.ReadBoolean();
                    br.AssertInt16(new short[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    GroupPartsIndices = br.ReadInt32s(32);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(PlatoonScriptID);
                    bw.WriteBoolean(PlatoonInfo_UnkT04);
                    bw.WriteBoolean(PlatoonInfo_UnkT05);
                    bw.WriteInt16((short)0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32s(GroupPartsIndices);
                }

                internal override void GetNames(MSB_AC6 msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    GroupPartsNames = MSB.FindNames(entries.Parts, GroupPartsIndices);
                }

                internal override void GetIndices(MSB_AC6 msb, Entries entries)
                {
                    base.GetIndices(msb, entries);
                    GroupPartsIndices = MSB.FindIndices(this, entries.Parts, GroupPartsNames);
                }
            }

            /// <summary>
            /// Unknown
            /// </summary>
            public class PatrolRoute : Event
            {
                private protected override EventType Type => EventType.PatrolRoute;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Unknown.
                /// </summary>
                public int PatrolType { get; set; }

                /// <summary>
                /// Unknown.
                /// Always 0.
                /// </summary>
                public int PatrolRoute_Unk08 { get; set; }
                
                /// <summary>
                /// Unknown.
                /// Always 0.
                /// </summary>
                public int PatrolRoute_Unk0C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Region))]
                public string[] WalkRegionNames { get; set; }

                private short[] WalkRegionIndices { get; set; }

                /// <summary>
                /// Creates a PatrolRoute with default values.
                /// </summary>
                public PatrolRoute() : base($"{nameof(Event)}: {nameof(PatrolRoute)}")
                {
                    WalkRegionNames = new string[64];
                }

                private protected override void DeepCopyTo(Event evnt)
                {
                    var patrolRoute = (PatrolRoute)evnt;
                    patrolRoute.WalkRegionNames = (string[])WalkRegionNames.Clone();
                }

                internal PatrolRoute(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    PatrolType = br.ReadInt32();
                    br.AssertInt32(-1);
                    PatrolRoute_Unk08 = br.ReadInt32();
                    PatrolRoute_Unk0C = br.ReadInt32();
                    WalkRegionIndices = br.ReadInt16s(24);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(PatrolType);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(PatrolRoute_Unk08);
                    bw.WriteInt32(PatrolRoute_Unk0C);
                    bw.WriteInt16s(WalkRegionIndices);
                }

                internal override void GetNames(MSB_AC6 msb, Entries entries)
                {
                    base.GetNames(msb, entries);

                    WalkRegionNames = MSB.FindNames(entries.Regions, WalkRegionIndices);
                }

                internal override void GetIndices(MSB_AC6 msb, Entries entries)
                {
                    base.GetIndices(msb, entries);

                    WalkRegionIndices = MSB.FindShortIndices(this, entries.Regions, WalkRegionNames);
                }
            }


            /// <summary>
            /// Unknown.
            /// </summary>
            public class MapGimmick : Event
            {
                private protected override EventType Type => EventType.MapGimmick;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Unknown.
                /// 0: 
                /// 6: m03_60_00_00
                /// 8: The launcher in Illegal Entry (m10_00_00_00)
                /// </summary>
                public int MapGimmick_UnkT00 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>

                [MSBReference(ReferenceType = typeof(Region))]
                public string RegionNameT04 { get; set; }
                private short RegionIndexT04 { get; set; }

                /// <summary>
                /// Unknown.
                /// Always 0.
                /// </summary>
                public short MapGimmick_UnkT06 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Part))]
                public string PartNameT08 { get; set; }
                private int PartIndexT08 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                
                [MSBReference(ReferenceType = typeof(Part))]
                public string[] PartNamesT0C { get; set; }
                private short[] PartIndicesT0C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                
                [MSBReference(ReferenceType = typeof(Region))]
                public string[] RegionNamesT28 { get; set; }
                private short[] RegionIndicesT28 { get; set; }

                /// <summary>
                /// Creates a MultiSummon with default values.
                /// </summary>
                public MapGimmick() : base($"{nameof(Event)}: {nameof(MapGimmick)}")
                {
                    PartNamesT0C = new string[14];
                    RegionNamesT28 = new string[16];
                }

                internal MapGimmick(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    MapGimmick_UnkT00 = br.ReadInt32();
                    RegionIndexT04 = br.ReadInt16();
                    MapGimmick_UnkT06 = br.ReadInt16();
                    PartIndexT08 = br.ReadInt32();
                    PartIndicesT0C = br.ReadInt16s(14);
                    RegionIndicesT28 = br.ReadInt16s(16);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(MapGimmick_UnkT00);
                    bw.WriteInt16(RegionIndexT04);
                    bw.WriteInt16(MapGimmick_UnkT06);
                    bw.WriteInt32(PartIndexT08);
                    bw.WriteInt16s(PartIndicesT0C);
                    bw.WriteInt16s(RegionIndicesT28);
                }

                internal override void GetNames(MSB_AC6 msb, Entries entries)
                {
                    base.GetNames(msb, entries);

                    RegionNameT04 = MSB.FindName(entries.Regions, RegionIndexT04);
                    PartNameT08 = MSB.FindName(entries.Parts, PartIndexT08);

                    PartNamesT0C = new string[PartIndicesT0C.Length];
                    for (int i = 0; i < PartIndicesT0C.Length; i++)
                        PartNamesT0C[i] = MSB.FindName(entries.Parts, PartIndicesT0C[i]);

                    RegionNamesT28 = new string[RegionIndicesT28.Length];
                    for (int i = 0; i < RegionIndicesT28.Length; i++)
                        RegionNamesT28[i] = MSB.FindName(entries.Regions, RegionIndicesT28[i]);
                }

                internal override void GetIndices(MSB_AC6 msb, Entries entries)
                {
                    base.GetIndices(msb, entries);

                    RegionIndexT04 = (short)MSB.FindIndex(this, entries.Regions, RegionNameT04);
                    PartIndexT08 = (short)MSB.FindIndex(this, entries.Parts, PartNameT08);

                    PartIndicesT0C = new short[PartNamesT0C.Length];
                    for (int i = 0; i < PartNamesT0C.Length; i++)
                        PartIndicesT0C[i] = (short)MSB.FindIndex(this, entries.Parts, PartNamesT0C[i]);

                    RegionIndicesT28 = new short[RegionNamesT28.Length];
                    for (int i = 0; i < RegionNamesT28.Length; i++)
                        RegionIndicesT28[i] = (short)MSB.FindIndex(this, entries.Regions, RegionNamesT28[i]);
                }
            }
            /// <summary>
            /// Unknown.
            /// </summary>
            public class Other : Event
            {
                private protected override EventType Type => EventType.Other;
                private protected override bool HasTypeData => true;


                /// <summary>
                /// Creates an Other with default values.
                /// </summary>
                public Other() : base($"{nameof(Event)}: {nameof(Other)}") { }

                internal Other(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                }
            }
        }
    }
}
