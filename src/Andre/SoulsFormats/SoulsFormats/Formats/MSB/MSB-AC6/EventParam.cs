using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace SoulsFormats
{
    public partial class MSB_AC6
    {
        internal enum EventType : int
        {
            Light = 0,
            Sound = 1,
            Sfx = 2,
            MapWindSfx = 3,
            Treasure = 4,
            Generator = 5,
            Message = 6,
            ObjAct = 7,
            ReturnPoint = 8,
            MapOffset = 9,
            Navmesh = 10,
            Unknown_11 = 11,
            NpcEntryPoint = 12,
            WindSfx = 13,
            PatrolInfo = 14,
            PlatoonInfo = 15,
            Unknown_16 = 16,
            Unknown_17 = 17,
            Unknown_18 = 18,
            Unknown_19 = 19,
            PatrolRoute = 20,
            Riding = 21,
            StrategyRoute = 22,
            PatrolRoutePermanent = 23,
            MapGimmick = 24,
            Other = -1,
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
            public EventParam() : base(73, "EVENT_PARAM_ST")
            {
                Treasures = new List<Event.Treasure>();
                Generators = new List<Event.Generator>();
                MapOffsets = new List<Event.MapOffset>();
                PlatoonInfo = new List<Event.PlatoonInfo>();
                PatrolInfo = new List<Event.PatrolInfo>();
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
                PatrolRoutes = new List<Event.PatrolRoute>();
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

            internal override Event ReadEntry(BinaryReaderEx br, int Version)
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
                        return Lights.EchoAdd(new Event.Light(br));

                    case EventType.Sound:
                        return Sounds.EchoAdd(new Event.Sound(br));

                    case EventType.Sfx:
                        return Sfxs.EchoAdd(new Event.Sfx(br));

                    case EventType.MapWindSfx:
                        return MapWindSfxs.EchoAdd(new Event.MapWindSfx(br));

                    case EventType.Message:
                        return Messages.EchoAdd(new Event.Message(br));

                    case EventType.ObjAct:
                        return ObjActs.EchoAdd(new Event.ObjAct(br));

                    case EventType.ReturnPoint:
                        return ReturnPoints.EchoAdd(new Event.ReturnPoint(br));

                    case EventType.Navmesh:
                        return Navmeshes.EchoAdd(new Event.Navmesh(br));

                    case EventType.Unknown_11:
                        return Unknown_11s.EchoAdd(new Event.Unknown_11(br));

                    case EventType.NpcEntryPoint:
                        return NpcEntryPoints.EchoAdd(new Event.NpcEntryPoint(br));

                    case EventType.WindSfx:
                        return WindSfxs.EchoAdd(new Event.WindSfx(br));

                    case EventType.Unknown_16:
                        return Unknown_16s.EchoAdd(new Event.Unknown_16(br));

                    case EventType.Unknown_17:
                        return Unknown_17s.EchoAdd(new Event.Unknown_17(br));

                    case EventType.Unknown_18:
                        return Unknown_18s.EchoAdd(new Event.Unknown_18(br));

                    case EventType.Unknown_19:
                        return Unknown_19s.EchoAdd(new Event.Unknown_19(br));

                    case EventType.PatrolRoute:
                        return PatrolRoutes.EchoAdd(new Event.PatrolRoute(br));

                    case EventType.Riding:
                        return Ridings.EchoAdd(new Event.Riding(br));

                    case EventType.StrategyRoute:
                        return StrategyRoutes.EchoAdd(new Event.StrategyRoute(br));

                    case EventType.PatrolRoutePermanent:
                        return PatrolRoutePermanents.EchoAdd(new Event.PatrolRoutePermanent(br));

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
                br.AssertInt32((int)Type);
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
                return $"{Type} {Name}";
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
                /// Itemlot given by the treasure.
                /// </summary>
                [MSBParamReference(ParamName = "ItemLotParam")]
                public int ItemLotParamId { get; set; }

                /// <summary>
                /// Unknown. Potentially is 2nd Itemlot.
                /// </summary>

                public int UnkT14 { get; set; }

                /// <summary>
                /// Unknown. Potentially is 3rd Itemlot.
                /// </summary>
                public int UnkT18 { get; set; }

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
                /// Unknown.
                /// </summary>
                public short UnkT42 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT44 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT48 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT4C { get; set; }

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
                    ItemLotParamId = br.ReadInt32();
                    UnkT14 = br.ReadInt32();
                    UnkT18 = br.ReadInt32();
                    UnkT1C = br.ReadInt32();
                    UnkT20 = br.ReadInt32();
                    UnkT24 = br.ReadInt32();
                    UnkT28 = br.ReadInt32();
                    UnkT2C = br.ReadInt32();
                    UnkT30 = br.ReadInt32();
                    UnkT34 = br.ReadInt32();
                    ActionButtonParamId = br.ReadInt32();
                    PickupAnim = br.ReadInt32();
                    InChest = br.ReadBoolean();
                    StartDisabled = br.ReadBoolean();
                    UnkT42 = br.ReadInt16();
                    UnkT44 = br.ReadInt32();
                    UnkT48 = br.ReadInt32();
                    UnkT4C = br.ReadInt32();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(TreasurePartIndex);
                    bw.WriteInt32(0);
                    bw.WriteInt32(ItemLotParamId);
                    bw.WriteInt32(UnkT14);
                    bw.WriteInt32(UnkT18);
                    bw.WriteInt32(UnkT1C);
                    bw.WriteInt32(UnkT20);
                    bw.WriteInt32(UnkT24);
                    bw.WriteInt32(UnkT28);
                    bw.WriteInt32(UnkT2C);
                    bw.WriteInt32(UnkT30);
                    bw.WriteInt32(UnkT34);
                    bw.WriteInt32(ActionButtonParamId);
                    bw.WriteInt32(PickupAnim);
                    bw.WriteBoolean(InChest);
                    bw.WriteBoolean(StartDisabled);
                    bw.WriteInt16(UnkT42);
                    bw.WriteInt32(UnkT44);
                    bw.WriteInt32(UnkT48);
                    bw.WriteInt32(UnkT4C);
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
                public int UnkTE0 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkTE4 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkTE8 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkTEC { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkTF0 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkTF4 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkTF8 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkTFC { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT100 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT104 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT108 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT10C { get; set; }

                /// <summary>
                /// Points that enemies may be spawned at.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Region))]
                public string[] SpawnRegionNames { get; private set; }
                private int[] SpawnRegionIndices;

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
                    UnkT11 = br.ReadByte();
                    UnkT12 = br.ReadByte();
                    UnkT13 = br.ReadByte();
                    UnkT14 = br.ReadSingle();
                    UnkT18 = br.ReadSingle();
                    br.AssertPattern(0x14, 0x00); // 2C
                    SpawnRegionIndices = br.ReadInt32s(8); // 4C
                    br.AssertPattern(0x10, 0x00); // 5C
                    SpawnPartIndices = br.ReadInt32s(32); // DC

                    UnkTE0 = br.ReadInt32(); 
                    UnkTE4 = br.ReadInt32(); 
                    UnkTE8 = br.ReadInt32(); 
                    UnkTEC = br.ReadInt32(); 
                    UnkTF0 = br.ReadInt32();
                    UnkTF4 = br.ReadInt32();
                    UnkTF8 = br.ReadInt32();
                    UnkTFC = br.ReadInt32();
                    UnkT100 = br.ReadInt32();
                    UnkT104 = br.ReadInt32();
                    UnkT108 = br.ReadInt32();
                    UnkT10C = br.ReadInt32();
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
                    bw.WriteSingle(UnkT14);
                    bw.WriteSingle(UnkT18);
                    bw.WritePattern(0x14, 0x00);
                    bw.WriteInt32s(SpawnRegionIndices);
                    bw.WritePattern(0x10, 0x00);
                    bw.WriteInt32s(SpawnPartIndices);

                    bw.WriteInt32(UnkTE0);
                    bw.WriteInt32(UnkTE4);
                    bw.WriteInt32(UnkTE8);
                    bw.WriteInt32(UnkTEC);
                    bw.WriteInt32(UnkTF0);
                    bw.WriteInt32(UnkTF4);
                    bw.WriteInt32(UnkTF8);
                    bw.WriteInt32(UnkTFC);
                    bw.WriteInt32(UnkT100);
                    bw.WriteInt32(UnkT104);
                    bw.WriteInt32(UnkT108);
                    bw.WriteInt32(UnkT10C);
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
                public bool UnkT04 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public bool UnkT05 { get; set; }

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
                    UnkT04 = br.ReadBoolean();
                    UnkT05 = br.ReadBoolean();
                    br.AssertInt16(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    GroupPartsIndices = br.ReadInt32s(32);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(PlatoonIDScriptActivate);
                    bw.WriteBoolean(UnkT04);
                    bw.WriteBoolean(UnkT05);
                    bw.WriteInt16(0);
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
            /// A simple list of points defining a path for enemies to take.
            /// </summary>
            public class PatrolInfo : Event
            {
                private protected override EventType Type => EventType.PatrolInfo;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Unknown; probably some kind of route type.
                /// </summary>
                public int UnkT00 { get; set; }

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
                    UnkT00 = br.ReadInt32();
                    br.AssertInt32(-1);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    WalkPointIndices = br.ReadInt16s(24);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(UnkT00);
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
                public string[] PartNamesT0C { get; private set; }
                private short[] PartIndicesT0C;

                /// <summary>
                /// Unknown.
                /// </summary>
                
                [MSBReference(ReferenceType = typeof(Region))]
                public string[] PointNamesT28 { get; private set; }
                private short[] PointIndicesT28;

                /// <summary>
                /// Creates a MultiSummon with default values.
                /// </summary>
                public MapGimmick() : base($"{nameof(Event)}: {nameof(MapGimmick)}") { }

                internal MapGimmick(BinaryReaderEx br) : base(br) { }

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

                /// <summary>
                /// Creates a Light with default values.
                /// </summary>
                public Light() : base($"{nameof(Event)}: {nameof(Light)}") { }

                internal Light(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                }
            }

            /// <summary>
            /// Unknown
            /// </summary>
            public class Sound : Event
            {
                private protected override EventType Type => EventType.Sound;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Creates a Sound with default values.
                /// </summary>
                public Sound() : base($"{nameof(Event)}: {nameof(Sound)}") { }

                internal Sound(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                }
            }

            /// <summary>
            /// Unknown
            /// </summary>
            public class Sfx : Event
            {
                private protected override EventType Type => EventType.Sfx;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Creates a Sfx with default values.
                /// </summary>
                public Sfx() : base($"{nameof(Event)}: {nameof(Sfx)}") { }

                internal Sfx(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                }
            }

            /// <summary>
            /// Unknown
            /// </summary>
            public class MapWindSfx : Event
            {
                private protected override EventType Type => EventType.MapWindSfx;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Creates a MapWindSfx with default values.
                /// </summary>
                public MapWindSfx() : base($"{nameof(Event)}: {nameof(MapWindSfx)}") { }

                internal MapWindSfx(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                }
            }

            /// <summary>
            /// Unknown
            /// </summary>
            public class Message : Event
            {
                private protected override EventType Type => EventType.Message;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Creates a Message with default values.
                /// </summary>
                public Message() : base($"{nameof(Event)}: {nameof(Message)}") { }

                internal Message(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                }
            }

            /// <summary>
            /// Unknown
            /// </summary>
            public class ObjAct : Event
            {
                private protected override EventType Type => EventType.ObjAct;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Unknown why objacts need an extra entity ID.
                /// </summary>
                [MSBEntityReference]
                public uint ObjActEntityID { get; set; }

                /// <summary>
                /// The part to be interacted with.
                /// </summary>
                private int ObjActPartIndex;

                /// <summary>
                /// A row in ObjActParam.
                /// </summary>
                [MSBParamReference(ParamName = "ObjActParam")]
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
                /// Creates a ObjAct with default values.
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
            }

            /// <summary>
            /// Unknown
            /// </summary>
            public class ReturnPoint : Event
            {
                private protected override EventType Type => EventType.ReturnPoint;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Creates a ReturnPoint with default values.
                /// </summary>
                public ReturnPoint() : base($"{nameof(Event)}: {nameof(ReturnPoint)}") { }

                internal ReturnPoint(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                }
            }

            /// <summary>
            /// Unknown
            /// </summary>
            public class Navmesh : Event
            {
                private protected override EventType Type => EventType.Navmesh;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Creates a Unknown_10 with default values.
                /// </summary>
                public Navmesh() : base($"{nameof(Event)}: {nameof(Navmesh)}") { }

                internal Navmesh(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                }
            }

            /// <summary>
            /// Unknown
            /// </summary>
            public class Unknown_11 : Event
            {
                private protected override EventType Type => EventType.Unknown_11;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Creates a Light with default values.
                /// </summary>
                public Unknown_11() : base($"{nameof(Event)}: {nameof(Unknown_11)}") { }

                internal Unknown_11(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                }
            }

            /// <summary>
            /// Unknown
            /// </summary>
            public class NpcEntryPoint : Event
            {
                private protected override EventType Type => EventType.NpcEntryPoint;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Creates a NpcEntryPoint with default values.
                /// </summary>
                public NpcEntryPoint() : base($"{nameof(Event)}: {nameof(NpcEntryPoint)}") { }

                internal NpcEntryPoint(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                }
            }

            /// <summary>
            /// Unknown
            /// </summary>
            public class WindSfx : Event
            {
                private protected override EventType Type => EventType.WindSfx;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Creates a WindSfx with default values.
                /// </summary>
                public WindSfx() : base($"{nameof(Event)}: {nameof(WindSfx)}") { }

                internal WindSfx(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                }
            }

            /// <summary>
            /// Unknown
            /// </summary>
            public class Unknown_16 : Event
            {
                private protected override EventType Type => EventType.Light;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Creates a Unknown_16 with default values.
                /// </summary>
                public Unknown_16() : base($"{nameof(Event)}: {nameof(Unknown_16)}") { }

                internal Unknown_16(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                }
            }

            /// <summary>
            /// Unknown
            /// </summary>
            public class Unknown_17 : Event
            {
                private protected override EventType Type => EventType.Unknown_17;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Creates a Unknown_17 with default values.
                /// </summary>
                public Unknown_17() : base($"{nameof(Event)}: {nameof(Unknown_17)}") { }

                internal Unknown_17(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                }
            }

            /// <summary>
            /// Unknown
            /// </summary>
            public class Unknown_18 : Event
            {
                private protected override EventType Type => EventType.Unknown_18;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Creates a Unknown_18 with default values.
                /// </summary>
                public Unknown_18() : base($"{nameof(Event)}: {nameof(Unknown_18)}") { }

                internal Unknown_18(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                }
            }

            /// <summary>
            /// Unknown
            /// </summary>
            public class Unknown_19 : Event
            {
                private protected override EventType Type => EventType.Unknown_19;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Creates a Unknown_19 with default values.
                /// </summary>
                public Unknown_19() : base($"{nameof(Event)}: {nameof(Unknown_19)}") { }

                internal Unknown_19(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
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
                /// Creates a PatrolRoute with default values.
                /// </summary>
                public PatrolRoute() : base($"{nameof(Event)}: {nameof(PatrolRoute)}") { }

                internal PatrolRoute(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                }
            }

            /// <summary>
            /// Unknown
            /// </summary>
            public class Riding : Event
            {
                private protected override EventType Type => EventType.Riding;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Creates a Riding with default values.
                /// </summary>
                public Riding() : base($"{nameof(Event)}: {nameof(Riding)}") { }

                internal Riding(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                }
            }

            /// <summary>
            /// Unknown
            /// </summary>
            public class StrategyRoute : Event
            {
                private protected override EventType Type => EventType.StrategyRoute;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Creates a StrategyRoute with default values.
                /// </summary>
                public StrategyRoute() : base($"{nameof(Event)}: {nameof(StrategyRoute)}") { }

                internal StrategyRoute(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                }
            }

            /// <summary>
            /// Unknown
            /// </summary>
            public class PatrolRoutePermanent : Event
            {
                private protected override EventType Type => EventType.PatrolRoutePermanent;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Creates a PatrolRoutePermanent with default values.
                /// </summary>
                public PatrolRoutePermanent() : base($"{nameof(Event)}: {nameof(PatrolRoutePermanent)}") { }

                internal PatrolRoutePermanent(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
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
