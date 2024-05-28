using Org.BouncyCastle.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using static SoulsFormats.GPARAM;

namespace SoulsFormats
{
    public partial class MSB_AC6
    {
        public enum EventType : int
        {
            Light = 0, // NOT IMPLEMENTED
            Sound = 1, // NOT IMPLEMENTED
            Sfx = 2, // NOT IMPLEMENTED
            MapWindSfx = 3, // NOT IMPLEMENTED
            Treasure = 4, 
            Generator = 5,
            Message = 6, // NOT IMPLEMENTED
            ObjAct = 7, // NOT IMPLEMENTED
            ReturnPoint = 8, // NOT IMPLEMENTED
            MapOffset = 9,
            Navmesh = 10, // NOT IMPLEMENTED
            Unknown_11 = 11, // NOT IMPLEMENTED
            NpcEntryPoint = 12, // NOT IMPLEMENTED
            WindSfx = 13, // NOT IMPLEMENTED
            PatrolInfo = 14, // NOT IMPLEMENTED
            PlatoonInfo = 15, 
            Unknown_16 = 16, // NOT IMPLEMENTED
            Unknown_17 = 17, // NOT IMPLEMENTED
            Unknown_18 = 18, // NOT IMPLEMENTED
            Unknown_19 = 19, // NOT IMPLEMENTED
            PatrolRoute = 20, 
            Riding = 21, // NOT IMPLEMENTED
            StrategyRoute = 22, // NOT IMPLEMENTED
            PatrolRoutePermanent = 23, // NOT IMPLEMENTED
            MapGimmick = 24, 
            Other = -1, 
        }

        /// <summary>
        /// Dynamic or interactive systems such as item pickups, levers, enemy spawners, etc.
        /// </summary>
        public class EventParam : Param<Event>, IMsbParam<IMsbEvent>
        {
            private int ParamVersion;

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
            public List<Event.PatrolInfo> PatrolInfo { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Event.MapGimmick> MapGimmicks { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Event.Light> Lights { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Event.Sound> Sounds { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Event.Sfx> Sfxs { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Event.MapWindSfx> MapWindSfxs { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Event.Message> Messages { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Event.ObjAct> ObjActs { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Event.ReturnPoint> ReturnPoints { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Event.Navmesh> Navmeshes { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Event.Unknown_11> Unknown_11s { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Event.NpcEntryPoint> NpcEntryPoints { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Event.WindSfx> WindSfxs { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Event.Unknown_16> Unknown_16s { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Event.Unknown_17> Unknown_17s { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Event.Unknown_18> Unknown_18s { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Event.Unknown_19> Unknown_19s { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Event.PatrolRoute> PatrolRoutes { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Event.Riding> Ridings { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Event.StrategyRoute> StrategyRoutes { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Event.PatrolRoutePermanent> PatrolRoutePermanents { get; set; }

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

                Lights = new List<Event.Light>();
                Sounds = new List<Event.Sound>();
                Sfxs = new List<Event.Sfx>();
                MapWindSfxs = new List<Event.MapWindSfx>();
                Messages = new List<Event.Message>();
                ObjActs = new List<Event.ObjAct>();
                ReturnPoints = new List<Event.ReturnPoint>();
                Navmeshes = new List<Event.Navmesh>();
                Unknown_11s = new List<Event.Unknown_11>();
                NpcEntryPoints = new List<Event.NpcEntryPoint>();
                WindSfxs = new List<Event.WindSfx>();
                Unknown_16s = new List<Event.Unknown_16>();
                Unknown_17s = new List<Event.Unknown_17>();
                Unknown_18s = new List<Event.Unknown_18>();
                Unknown_19s = new List<Event.Unknown_19>();
                PatrolInfo = new List<Event.PatrolInfo>();
                Ridings = new List<Event.Riding>();
                StrategyRoutes = new List<Event.StrategyRoute>();
                PatrolRoutePermanents = new List<Event.PatrolRoutePermanent>();
            }

            /// <summary>
            /// Adds an event to the appropriate list for its type; returns the event.
            /// </summary>
            public Event Add(Event evnt)
            {
                switch (evnt)
                {
                    case Event.Treasure e:
                        Treasures.Add(e);
                        break;
                    case Event.Generator e:
                        Generators.Add(e);
                        break;
                    case Event.MapOffset e:
                        MapOffsets.Add(e);
                        break;
                    case Event.PlatoonInfo e:
                        PlatoonInfo.Add(e);
                        break;
                    case Event.PatrolInfo e:
                        PatrolInfo.Add(e);
                        break;
                    case Event.MapGimmick e:
                        MapGimmicks.Add(e);
                        break;
                    case Event.Light e:
                        Lights.Add(e);
                        break;
                    case Event.Sound e:
                        Sounds.Add(e);
                        break;
                    case Event.Sfx e:
                        Sfxs.Add(e);
                        break;
                    case Event.MapWindSfx e:
                        MapWindSfxs.Add(e);
                        break;
                    case Event.Message e:
                        Messages.Add(e);
                        break;
                    case Event.ObjAct e:
                        ObjActs.Add(e);
                        break;
                    case Event.ReturnPoint e:
                        ReturnPoints.Add(e);
                        break;
                    case Event.Navmesh e:
                        Navmeshes.Add(e);
                        break;
                    case Event.Unknown_11 e:
                        Unknown_11s.Add(e);
                        break;
                    case Event.NpcEntryPoint e:
                        NpcEntryPoints.Add(e);
                        break;
                    case Event.WindSfx e:
                        WindSfxs.Add(e);
                        break;
                    case Event.Unknown_16 e:
                        Unknown_16s.Add(e);
                        break;
                    case Event.Unknown_17 e:
                        Unknown_17s.Add(e);
                        break;
                    case Event.Unknown_18 e:
                        Unknown_18s.Add(e);
                        break;
                    case Event.Unknown_19 e:
                        Unknown_19s.Add(e);
                        break;
                    case Event.PatrolRoute e:
                        PatrolRoutes.Add(e);
                        break;
                    case Event.Riding e:
                        Ridings.Add(e);
                        break;
                    case Event.StrategyRoute e:
                        StrategyRoutes.Add(e);
                        break;
                    case Event.PatrolRoutePermanent e:
                        PatrolRoutePermanents.Add(e);
                        break;
                    case Event.Other e:
                        Others.Add(e);
                        break;

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
                Treasures, Generators, MapOffsets, PlatoonInfo,
                    PatrolInfo, MapGimmicks,
                    Lights, Sounds, Sfxs, MapWindSfxs, Messages, ObjActs, ReturnPoints, Navmeshes, Unknown_11s, NpcEntryPoints,
                    WindSfxs, Unknown_16s, Unknown_17s, Unknown_18s, Unknown_19s, PatrolRoutes, Ridings, StrategyRoutes, PatrolRoutePermanents,
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

                    case EventType.PatrolInfo:
                        return PatrolInfo.EchoAdd(new Event.PatrolInfo(br));

                    case EventType.MapGimmick:
                        return MapGimmicks.EchoAdd(new Event.MapGimmick(br));

                    case EventType.Light:
                        return Lights.EchoAdd(new Event.Light(br, offsetLength));

                    case EventType.Sound:
                        return Sounds.EchoAdd(new Event.Sound(br, offsetLength));

                    case EventType.Sfx:
                        return Sfxs.EchoAdd(new Event.Sfx(br, offsetLength));

                    case EventType.MapWindSfx:
                        return MapWindSfxs.EchoAdd(new Event.MapWindSfx(br, offsetLength));

                    case EventType.Message:
                        return Messages.EchoAdd(new Event.Message(br, offsetLength));

                    case EventType.ObjAct:
                        return ObjActs.EchoAdd(new Event.ObjAct(br, offsetLength));

                    case EventType.ReturnPoint:
                        return ReturnPoints.EchoAdd(new Event.ReturnPoint(br, offsetLength));

                    case EventType.Navmesh:
                        return Navmeshes.EchoAdd(new Event.Navmesh(br, offsetLength));

                    case EventType.Unknown_11:
                        return Unknown_11s.EchoAdd(new Event.Unknown_11(br, offsetLength));

                    case EventType.NpcEntryPoint:
                        return NpcEntryPoints.EchoAdd(new Event.NpcEntryPoint(br, offsetLength));

                    case EventType.WindSfx:
                        return WindSfxs.EchoAdd(new Event.WindSfx(br, offsetLength));

                    case EventType.Unknown_16:
                        return Unknown_16s.EchoAdd(new Event.Unknown_16(br, offsetLength));

                    case EventType.Unknown_17:
                        return Unknown_17s.EchoAdd(new Event.Unknown_17(br, offsetLength));

                    case EventType.Unknown_18:
                        return Unknown_18s.EchoAdd(new Event.Unknown_18(br, offsetLength));

                    case EventType.Unknown_19:
                        return Unknown_19s.EchoAdd(new Event.Unknown_19(br, offsetLength));

                    case EventType.PatrolRoute:
                        return PatrolRoutes.EchoAdd(new Event.PatrolRoute(br));

                    case EventType.Riding:
                        return Ridings.EchoAdd(new Event.Riding(br, offsetLength));

                    case EventType.StrategyRoute:
                        return StrategyRoutes.EchoAdd(new Event.StrategyRoute(br, offsetLength));

                    case EventType.PatrolRoutePermanent:
                        return PatrolRoutePermanents.EchoAdd(new Event.PatrolRoutePermanent(br, offsetLength));

                    case EventType.Other:
                        return Others.EchoAdd(new Event.Other(br, offsetLength));

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
            /// Event: Main
            public string Name { get; set; }

            public int EventID { get; set; }

            private protected abstract EventType Type { get; }
            private protected abstract bool HasTypeData { get; }

            // Index among events of the same type
            public int TypeIndex { get; set; }

            /// Event: EventCommon
            [MSBReference(ReferenceType = typeof(Part))]
            public string PartName { get; set; }
            public int PartIndex;

            [MSBReference(ReferenceType = typeof(Region))]
            public string RegionName { get; set; }
            public int RegionIndex;

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
                long nameOffset = br.ReadInt64();
                EventID = br.ReadInt32();
                br.AssertInt32((int)Type);
                TypeIndex = br.ReadInt32();
                br.AssertInt32(new int[1]);

                long commonOffset = br.ReadInt64();
                long typeDataOffset = br.ReadInt64();

                Name = br.GetUTF16(start + nameOffset);

                // Common
                br.Position = start + commonOffset;
                PartIndex = br.ReadInt32();
                RegionIndex = br.ReadInt32();
                EntityID = br.ReadInt32();
                br.AssertSByte((sbyte)-1);
                br.AssertByte(new byte[1]);
                br.AssertByte(new byte[1]);
                br.AssertByte(new byte[1]);

                // TypeData
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

                // Main
                bw.ReserveInt64("NameOffset");
                bw.WriteInt32(EventID);
                bw.WriteInt32((int)Type);
                bw.WriteInt32(TypeIndex);
                bw.WriteInt32(0);

                bw.ReserveInt64("CommonOffset");
                bw.ReserveInt64("TypeDataOffset");

                bw.FillInt64("NameOffset", bw.Position - start);
                bw.WriteUTF16(Name, true);
                bw.Pad(8);

                // Common
                bw.FillInt64("CommonOffset", bw.Position - start);
                bw.WriteInt32(PartIndex);
                bw.WriteInt32(RegionIndex);
                bw.WriteInt32(EntityID);
                bw.WriteSByte((sbyte)-1);
                bw.WriteByte((byte)0);
                bw.WriteByte((byte)0);
                bw.WriteByte((byte)0);

                // TypeData
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
                public int TreasurePartIndex;

                /// <summary>
                /// Itemlot given by the treasure.
                /// </summary>
                [MSBParamReference(ParamName = "ItemLotParam")]
                public int ItemLotParamId { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                [MSBParamReference(ParamName = "ActionButtonParam")]
                public int ActionButtonParamId { get; set; }

                /// <summary>
                /// Unknown; possible the pickup anim.
                /// </summary>
                public int PickupAnim { get; set; }

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
                public Treasure() : base($"{nameof(Event)}: {nameof(Treasure)}") { }

                internal Treasure(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    TreasurePartIndex = br.ReadInt32();
                    br.AssertInt32(new int[1]);
                    ItemLotParamId = br.ReadInt32();
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    ActionButtonParamId = br.ReadInt32();
                    PickupAnim = br.ReadInt32();
                    InChest = br.ReadBoolean();
                    StartDisabled = br.ReadBoolean();
                    br.AssertInt16(new short[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(TreasurePartIndex);
                    bw.WriteInt32(0);
                    bw.WriteInt32(ItemLotParamId);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(ActionButtonParamId);
                    bw.WriteInt32(PickupAnim);
                    bw.WriteBoolean(InChest);
                    bw.WriteBoolean(StartDisabled);
                    bw.WriteInt16((short) 0);
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
                public int[] SpawnRegionIndices;

                /// <summary>
                /// Enemies to be respawned.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Part))]
                public string[] SpawnPartNames { get; private set; }
                public int[] SpawnPartIndices;

                /// <summary>
                /// Creates a Generator with default values.
                /// </summary>
                public Generator() : base($"{nameof(Event)}: {nameof(Generator)}")
                {
                    MaxNum = (byte)1;
                    GenType = (byte)3;
                    LimitNum = (short)-1;
                    MinGenNum = (short)1;
                    MaxGenNum = (short)1;
                    SpawnRegionIndices = new int[8];
                    Array.Fill<int>(SpawnRegionIndices, -1);
                    SpawnPartIndices = new int[32];
                    Array.Fill<int>(this.SpawnPartIndices, -1);

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
                    br.AssertByte(new byte[1]);
                    br.AssertByte(new byte[1]);
                    br.AssertByte(new byte[1]);
                    UnkT14 = br.ReadSingle();
                    UnkT18 = br.ReadSingle();
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    SpawnRegionIndices = br.ReadInt32s(8);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    SpawnPartIndices = br.ReadInt32s(32);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
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
                    bw.WriteByte((byte) 0);
                    bw.WriteByte((byte) 0);
                    bw.WriteByte((byte) 0);
                    bw.WriteSingle(UnkT14);
                    bw.WriteSingle(UnkT18);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32s(SpawnRegionIndices);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32s(SpawnPartIndices);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
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
                [PositionProperty]
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
                public bool UnkT04 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public bool UnkT05 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Part))]
                public string[] GroupPartsNames { get; set; }
                public int[] GroupPartsIndices;

                /// <summary>
                /// Creates a GroupTour with default values.
                /// </summary>
                public PlatoonInfo() : base($"{nameof(Event)}: {nameof(PlatoonInfo)}")
                {
                    PlatoonScriptID = -1;
                    GroupPartsIndices = new int[32];
                    Array.Fill<int>(GroupPartsIndices, -1);
                    GroupPartsNames = new string[32];
                    Array.Fill<string>(GroupPartsNames, "");
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
                    UnkT04 = br.ReadBoolean();
                    UnkT05 = br.ReadBoolean();
                    br.AssertInt16(new short[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    GroupPartsIndices = br.ReadInt32s(32);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(PlatoonScriptID);
                    bw.WriteBoolean(UnkT04);
                    bw.WriteBoolean(UnkT05);
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
            /// NOT USED IN AC6
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
                public short[] WalkPointIndices;

                /// <summary>
                /// Creates a WalkRoute with default values.
                /// </summary>
                public PatrolInfo() : base($"{nameof(Event)}: {nameof(PatrolInfo)}")
                {
                    WalkPointIndices = new short[24];
                    Array.Fill<short>(WalkPointIndices, (short)-1);
                    WalkPointNames = new string[24];
                    Array.Fill<string>(WalkPointNames, "");
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
                    br.AssertInt32(-1);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    WalkPointIndices = br.ReadInt16s(24);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(PatrolType);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt16s(WalkPointIndices);
                }

                internal override void GetNames(MSB_AC6 msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    WalkPointNames = new string[WalkPointIndices.Length];
                    for (int i = 0; i < WalkPointIndices.Length; i++)
                        WalkPointNames[i] = MSB.FindName(entries.Regions, WalkPointIndices[i]);
                }

                internal override void GetIndices(MSB_AC6 msb, Entries entries)
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
            public class MapGimmick : Event
            {
                private protected override EventType Type => EventType.MapGimmick;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT00 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short PointIndexT04 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short UnkT06 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int PartIndexT08 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                
                [MSBReference(ReferenceType = typeof(Part))]
                public string[] PartNamesT0C { get; set; }
                public short[] PartIndicesT0C;

                /// <summary>
                /// Unknown.
                /// </summary>
                
                [MSBReference(ReferenceType = typeof(Region))]
                public string[] PointNamesT28 { get; set; }
                public short[] PointIndicesT28;

                /// <summary>
                /// Creates a MultiSummon with default values.
                /// </summary>
                public MapGimmick() : base($"{nameof(Event)}: {nameof(MapGimmick)}") { }

                internal MapGimmick(BinaryReaderEx br) : base(br) 
                {
                    PointIndexT04 = (short)-1;
                    UnkT06 = (short)-1;
                    PartIndexT08 = -1;

                    PartIndicesT0C = new short[14];
                    Array.Fill<short>(PartIndicesT0C, (short)-1);
                    PartNamesT0C = new string[14];
                    Array.Fill<string>(PartNamesT0C, "");

                    PointIndicesT28 = new short[16];
                    Array.Fill<short>(PointIndicesT28, (short)-1);
                    PointNamesT28 = new string[16];
                    Array.Fill<string>(PointNamesT28, "");
                }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    UnkT00 = br.ReadInt32();
                    PointIndexT04 = br.ReadInt16();
                    UnkT06 = br.ReadInt16();
                    PartIndexT08 = br.ReadInt32();
                    PartIndicesT0C = br.ReadInt16s(14);
                    PointIndicesT28 = br.ReadInt16s(16);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(UnkT00);
                    bw.WriteInt16(PointIndexT04);
                    bw.WriteInt16(UnkT06);
                    bw.WriteInt32(PartIndexT08);
                    bw.WriteInt16s(PartIndicesT0C);
                    bw.WriteInt16s(PointIndicesT28);
                }

                internal override void GetNames(MSB_AC6 msb, Entries entries)
                {
                    base.GetNames(msb, entries);

                    // PartIndicesT0C
                    PartNamesT0C = new string[PartIndicesT0C.Length];
                    for (int i = 0; i < PartIndicesT0C.Length; i++)
                        PartNamesT0C[i] = MSB.FindName(entries.Parts, PartIndicesT0C[i]);

                    // PointIndicesT28
                    PointNamesT28 = new string[PointIndicesT28.Length];
                    for (int i = 0; i < PointIndicesT28.Length; i++)
                        PointNamesT28[i] = MSB.FindName(entries.Regions, PointIndicesT28[i]);
                }

                internal override void GetIndices(MSB_AC6 msb, Entries entries)
                {
                    base.GetIndices(msb, entries);

                    // PartIndicesT0C
                    PartIndicesT0C = new short[PartNamesT0C.Length];
                    for (int i = 0; i < PartNamesT0C.Length; i++)
                        PartIndicesT0C[i] = (short)MSB.FindIndex(this, entries.Parts, PartNamesT0C[i]);

                    // PointIndicesT28
                    PointIndicesT28 = new short[PointNamesT28.Length];
                    for (int i = 0; i < PointNamesT28.Length; i++)
                        PointIndicesT28[i] = (short)MSB.FindIndex(this, entries.Regions, PointNamesT28[i]);
                }
            }

            /// <summary>
            /// Unknown
            /// </summary>
            public class Light : Event
            {
                private protected override EventType Type => EventType.Light;
                private protected override bool HasTypeData => true;

                private long Length { get; set; }
                public byte[] Bytes { get; set; }

                public Light() : base($"{nameof(Event)}: {nameof(Light)}")
                {
                    Bytes = Array.Empty<byte>();
                }

                internal Light(BinaryReaderEx br, long _length) : base(br)
                {
                    Length = _length;
                }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    Bytes = br.ReadBytes((int)Length);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteBytes(Bytes);
                }
            }

            /// <summary>
            /// Unknown
            /// </summary>
            public class Sound : Event
            {
                private protected override EventType Type => EventType.Sound;
                private protected override bool HasTypeData => true;

                private long Length { get; set; }
                public byte[] Bytes { get; set; }

                public Sound() : base($"{nameof(Event)}: {nameof(Sound)}")
                {
                    Bytes = Array.Empty<byte>();
                }

                internal Sound(BinaryReaderEx br, long _length) : base(br)
                {
                    Length = _length;
                }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    Bytes = br.ReadBytes((int)Length);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteBytes(Bytes);
                }
            }

            /// <summary>
            /// Unknown
            /// </summary>
            public class Sfx : Event
            {
                private protected override EventType Type => EventType.Sfx;
                private protected override bool HasTypeData => true;

                private long Length { get; set; }
                public byte[] Bytes { get; set; }

                public Sfx() : base($"{nameof(Event)}: {nameof(Sfx)}")
                {
                    Bytes = Array.Empty<byte>();
                }

                internal Sfx(BinaryReaderEx br, long _length) : base(br)
                {
                    Length = _length;
                }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    Bytes = br.ReadBytes((int)Length);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteBytes(Bytes);
                }
            }

            /// <summary>
            /// Unknown
            /// </summary>
            public class MapWindSfx : Event
            {
                private protected override EventType Type => EventType.MapWindSfx;
                private protected override bool HasTypeData => true;

                private long Length { get; set; }
                public byte[] Bytes { get; set; }

                public MapWindSfx() : base($"{nameof(Event)}: {nameof(MapWindSfx)}")
                {
                    Bytes = Array.Empty<byte>();
                }

                internal MapWindSfx(BinaryReaderEx br, long _length) : base(br)
                {
                    Length = _length;
                }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    Bytes = br.ReadBytes((int)Length);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteBytes(Bytes);
                }
            }

            /// <summary>
            /// Unknown
            /// </summary>
            public class Message : Event
            {
                private protected override EventType Type => EventType.Message;
                private protected override bool HasTypeData => true;

                private long Length { get; set; }
                public byte[] Bytes { get; set; }

                public Message() : base($"{nameof(Event)}: {nameof(Message)}")
                {
                    Bytes = Array.Empty<byte>();
                }

                internal Message(BinaryReaderEx br, long _length) : base(br)
                {
                    Length = _length;
                }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    Bytes = br.ReadBytes((int)Length);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteBytes(Bytes);
                }
            }

            /// <summary>
            /// Unknown
            /// </summary>
            public class ObjAct : Event
            {
                private protected override EventType Type => EventType.ObjAct;
                private protected override bool HasTypeData => true;

                private long Length { get; set; }
                public byte[] Bytes { get; set; }

                public ObjAct() : base($"{nameof(Event)}: {nameof(ObjAct)}")
                {
                    Bytes = Array.Empty<byte>();
                }

                internal ObjAct(BinaryReaderEx br, long _length) : base(br)
                {
                    Length = _length;
                }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    Bytes = br.ReadBytes((int)Length);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteBytes(Bytes);
                }
            }

            /// <summary>
            /// Unknown
            /// </summary>
            public class ReturnPoint : Event
            {
                private protected override EventType Type => EventType.ReturnPoint;
                private protected override bool HasTypeData => true;

                private long Length { get; set; }
                public byte[] Bytes { get; set; }

                public ReturnPoint() : base($"{nameof(Event)}: {nameof(ReturnPoint)}")
                {
                    Bytes = Array.Empty<byte>();
                }

                internal ReturnPoint(BinaryReaderEx br, long _length) : base(br)
                {
                    Length = _length;
                }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    Bytes = br.ReadBytes((int)Length);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteBytes(Bytes);
                }
            }

            /// <summary>
            /// Unknown
            /// </summary>
            public class Navmesh : Event
            {
                private protected override EventType Type => EventType.Navmesh;
                private protected override bool HasTypeData => true;

                private long Length { get; set; }
                public byte[] Bytes { get; set; }

                public Navmesh() : base($"{nameof(Event)}: {nameof(Navmesh)}")
                {
                    Bytes = Array.Empty<byte>();
                }

                internal Navmesh(BinaryReaderEx br, long _length) : base(br)
                {
                    Length = _length;
                }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    Bytes = br.ReadBytes((int)Length);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteBytes(Bytes);
                }
            }

            /// <summary>
            /// Unknown
            /// </summary>
            public class Unknown_11 : Event
            {
                private protected override EventType Type => EventType.Unknown_11;
                private protected override bool HasTypeData => true;

                private long Length { get; set; }
                public byte[] Bytes { get; set; }

                public Unknown_11() : base($"{nameof(Event)}: {nameof(Unknown_11)}")
                {
                    Bytes = Array.Empty<byte>();
                }

                internal Unknown_11(BinaryReaderEx br, long _length) : base(br)
                {
                    Length = _length;
                }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    Bytes = br.ReadBytes((int)Length);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteBytes(Bytes);
                }
            }

            /// <summary>
            /// Unknown
            /// </summary>
            public class NpcEntryPoint : Event
            {
                private protected override EventType Type => EventType.NpcEntryPoint;
                private protected override bool HasTypeData => true;

                private long Length { get; set; }
                public byte[] Bytes { get; set; }

                public NpcEntryPoint() : base($"{nameof(Event)}: {nameof(NpcEntryPoint)}")
                {
                    Bytes = Array.Empty<byte>();
                }

                internal NpcEntryPoint(BinaryReaderEx br, long _length) : base(br)
                {
                    Length = _length;
                }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    Bytes = br.ReadBytes((int)Length);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteBytes(Bytes);
                }
            }

            /// <summary>
            /// Unknown
            /// </summary>
            public class WindSfx : Event
            {
                private protected override EventType Type => EventType.WindSfx;
                private protected override bool HasTypeData => true;

                private long Length { get; set; }
                public byte[] Bytes { get; set; }

                public WindSfx() : base($"{nameof(Event)}: {nameof(WindSfx)}")
                {
                    Bytes = Array.Empty<byte>();
                }

                internal WindSfx(BinaryReaderEx br, long _length) : base(br)
                {
                    Length = _length;
                }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    Bytes = br.ReadBytes((int)Length);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteBytes(Bytes);
                }
            }

            /// <summary>
            /// Unknown
            /// </summary>
            public class Unknown_16 : Event
            {
                private protected override EventType Type => EventType.Light;
                private protected override bool HasTypeData => true;

                private long Length { get; set; }
                public byte[] Bytes { get; set; }

                public Unknown_16() : base($"{nameof(Event)}: {nameof(Unknown_16)}")
                {
                    Bytes = Array.Empty<byte>();
                }

                internal Unknown_16(BinaryReaderEx br, long _length) : base(br)
                {
                    Length = _length;
                }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    Bytes = br.ReadBytes((int)Length);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteBytes(Bytes);
                }
            }

            /// <summary>
            /// Unknown
            /// </summary>
            public class Unknown_17 : Event
            {
                private protected override EventType Type => EventType.Unknown_17;
                private protected override bool HasTypeData => true;

                private long Length { get; set; }
                public byte[] Bytes { get; set; }

                public Unknown_17() : base($"{nameof(Event)}: {nameof(Unknown_17)}")
                {
                    Bytes = Array.Empty<byte>();
                }

                internal Unknown_17(BinaryReaderEx br, long _length) : base(br)
                {
                    Length = _length;
                }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    Bytes = br.ReadBytes((int)Length);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteBytes(Bytes);
                }
            }

            /// <summary>
            /// Unknown
            /// </summary>
            public class Unknown_18 : Event
            {
                private protected override EventType Type => EventType.Unknown_18;
                private protected override bool HasTypeData => true;

                private long Length { get; set; }
                public byte[] Bytes { get; set; }

                public Unknown_18() : base($"{nameof(Event)}: {nameof(Unknown_18)}")
                {
                    Bytes = Array.Empty<byte>();
                }

                internal Unknown_18(BinaryReaderEx br, long _length) : base(br)
                {
                    Length = _length;
                }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    Bytes = br.ReadBytes((int)Length);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteBytes(Bytes);
                }
            }

            /// <summary>
            /// Unknown
            /// </summary>
            public class Unknown_19 : Event
            {
                private protected override EventType Type => EventType.Unknown_19;
                private protected override bool HasTypeData => true;

                private long Length { get; set; }
                public byte[] Bytes { get; set; }

                public Unknown_19() : base($"{nameof(Event)}: {nameof(Unknown_19)}")
                {
                    Bytes = Array.Empty<byte>();
                }

                internal Unknown_19(BinaryReaderEx br, long _length) : base(br)
                {
                    Length = _length;
                }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    Bytes = br.ReadBytes((int)Length);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteBytes(Bytes);
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
                [MSBEnum(EnumType = "PATROL_TYPE")]
                public int PatrolType { get; set; }
                private int Unk08 { get; set; }
                private int Unk0C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Region))]
                public string[] WalkRegionNames { get; private set; }
                private short[] WalkRegionIndices;

                /// <summary>
                /// Creates a PatrolRoute with default values.
                /// </summary>
                public PatrolRoute() : base($"{nameof(Event)}: {nameof(PatrolRoute)}")
                {
                    WalkRegionIndices = new short[24];
                    Array.Fill<short>(WalkRegionIndices, -1);
                    WalkRegionNames = new string[24];
                    Array.Fill<string>(WalkRegionNames, "");
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
                    Unk08 = br.ReadInt32();
                    Unk0C = br.ReadInt32();
                    WalkRegionIndices = br.ReadInt16s(24);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(PatrolType);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
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
            /// Unknown
            /// </summary>
            public class Riding : Event
            {
                private protected override EventType Type => EventType.Riding;
                private protected override bool HasTypeData => true;

                private long Length { get; set; }
                public byte[] Bytes { get; set; }

                public Riding() : base($"{nameof(Event)}: {nameof(Riding)}")
                {
                    Bytes = Array.Empty<byte>();
                }

                internal Riding(BinaryReaderEx br, long _length) : base(br)
                {
                    Length = _length;
                }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    Bytes = br.ReadBytes((int)Length);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteBytes(Bytes);
                }
            }

            /// <summary>
            /// Unknown
            /// </summary>
            public class StrategyRoute : Event
            {
                private protected override EventType Type => EventType.StrategyRoute;
                private protected override bool HasTypeData => true;

                private long Length { get; set; }
                public byte[] Bytes { get; set; }

                public StrategyRoute() : base($"{nameof(Event)}: {nameof(StrategyRoute)}")
                {
                    Bytes = Array.Empty<byte>();
                }

                internal StrategyRoute(BinaryReaderEx br, long _length) : base(br)
                {
                    Length = _length;
                }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    Bytes = br.ReadBytes((int)Length);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteBytes(Bytes);
                }
            }

            /// <summary>
            /// Unknown
            /// </summary>
            public class PatrolRoutePermanent : Event
            {
                private protected override EventType Type => EventType.PatrolRoutePermanent;
                private protected override bool HasTypeData => true;

                private long Length { get; set; }
                public byte[] Bytes { get; set; }

                public PatrolRoutePermanent() : base($"{nameof(Event)}: {nameof(PatrolRoutePermanent)}")
                {
                    Bytes = Array.Empty<byte>();
                }

                internal PatrolRoutePermanent(BinaryReaderEx br, long _length) : base(br)
                {
                    Length = _length;
                }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    Bytes = br.ReadBytes((int)Length);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteBytes(Bytes);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class Other : Event
            {
                private protected override EventType Type => EventType.Other;
                private protected override bool HasTypeData => true;

                private long Length { get; set; }
                public byte[] Bytes { get; set; }

                /// <summary>
                /// Creates an Other with default values.
                /// </summary>
                public Other() : base($"{nameof(Event)}: {nameof(Other)}") 
                {
                    Bytes = Array.Empty<byte>();
                }

                internal Other(BinaryReaderEx br, long _length) : base(br)
                {
                    Length = _length;
                }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    Bytes = br.ReadBytes((int)Length);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteBytes(Bytes);
                }
            }
        }
    }
}
