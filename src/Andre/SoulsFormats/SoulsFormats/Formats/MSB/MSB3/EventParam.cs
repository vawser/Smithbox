using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace SoulsFormats
{
    public partial class MSB3
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
            PseudoMultiplayer = 12,
            //WindSFX = 13,
            PatrolInfo = 14,
            PlatoonInfo = 15,
            //DarkSight = 16,
            Other = 0xFFFFFFFF,
        }

        /// <summary>
        /// Events controlling various interactive or dynamic features in the map.
        /// </summary>
        public class EventParam : Param<Event>, IMsbParam<IMsbEvent>
        {
            internal override int Version => 3;
            internal override string Type => "EVENT_PARAM_ST";

            /// <summary>
            /// Treasures in the MSB.
            /// </summary>
            public List<Event.Treasure> Treasures { get; set; }

            /// <summary>
            /// Generators in the MSB.
            /// </summary>
            public List<Event.Generator> Generators { get; set; }

            /// <summary>
            /// Object actions in the MSB.
            /// </summary>
            public List<Event.ObjAct> ObjActs { get; set; }

            /// <summary>
            /// Map offsets in the MSB.
            /// </summary>
            public List<Event.MapOffset> MapOffsets { get; set; }

            /// <summary>
            /// Pseudo multiplayer events in the MSB.
            /// </summary>
            public List<Event.PseudoMultiplayer> PseudoMultiplayers { get; set; }

            /// <summary>
            /// Patrol info in the MSB.
            /// </summary>
            public List<Event.PatrolInfo> PatrolInfo { get; set; }

            /// <summary>
            /// Platoon info in the MSB.
            /// </summary>
            public List<Event.PlatoonInfo> PlatoonInfo { get; set; }

            /// <summary>
            /// Other events in the MSB.
            /// </summary>
            public List<Event.Other> Others { get; set; }

            /// <summary>
            /// Creates a new EventParam with no events.
            /// </summary>
            public EventParam()
            {
                Treasures = new List<Event.Treasure>();
                Generators = new List<Event.Generator>();
                ObjActs = new List<Event.ObjAct>();
                MapOffsets = new List<Event.MapOffset>();
                PseudoMultiplayers = new List<Event.PseudoMultiplayer>();
                PatrolInfo = new List<Event.PatrolInfo>();
                PlatoonInfo = new List<Event.PlatoonInfo>();
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
                    case Event.PseudoMultiplayer e: PseudoMultiplayers.Add(e); break;
                    case Event.PatrolInfo e: PatrolInfo.Add(e); break;
                    case Event.PlatoonInfo e: PlatoonInfo.Add(e); break;
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
                    Treasures, Generators, ObjActs, MapOffsets, PseudoMultiplayers,
                    PatrolInfo, PlatoonInfo, Others);
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

                    case EventType.PseudoMultiplayer:
                        return PseudoMultiplayers.EchoAdd(new Event.PseudoMultiplayer(br));

                    case EventType.PatrolInfo:
                        return PatrolInfo.EchoAdd(new Event.PatrolInfo(br));

                    case EventType.PlatoonInfo:
                        return PlatoonInfo.EchoAdd(new Event.PlatoonInfo(br));

                    case EventType.Other:
                        return Others.EchoAdd(new Event.Other(br));

                    default:
                        throw new NotImplementedException($"Unsupported event type: {type}");
                }
            }
        }

        /// <summary>
        /// An interactive or dynamic feature of the map.
        /// </summary>
        public abstract class Event : NamedEntry, IMsbEvent
        {
            private protected abstract EventType Type { get; }

            /// <summary>
            /// The name of this event.
            /// </summary>
            public override string Name { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int EventID { get; set; }

            /// <summary>
            /// The name of a part the event is attached to.
            /// </summary>
            [MSBReference(ReferenceType = typeof(Part))]
            public string PartName { get; set; }
            public int PartIndex { get; set; }

            /// <summary>
            /// The name of a region the event is attached to.
            /// </summary>
            [MSBReference(ReferenceType = typeof(Region))]
            public string PointName { get; set; }
            public int PointIndex { get; set; }

            /// <summary>
            /// Used to identify the event in event scripts.
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
                if (typeDataOffset == 0)
                    throw new InvalidDataException($"{nameof(typeDataOffset)} must not be 0 in type {GetType()}.");

                br.Position = start + nameOffset;
                Name = br.ReadUTF16();

                br.Position = start + baseDataOffset;
                PartIndex = br.ReadInt32();
                PointIndex = br.ReadInt32();
                EntityID = br.ReadInt32();
                br.AssertInt32(0);

                br.Position = start + typeDataOffset;
                ReadTypeData(br);
            }

            private protected abstract void ReadTypeData(BinaryReaderEx br);

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
                bw.WriteInt32(PointIndex);
                bw.WriteInt32(EntityID);
                bw.WriteInt32(0);

                bw.FillInt64("TypeDataOffset", bw.Position - start);
                WriteTypeData(bw);
            }

            private protected abstract void WriteTypeData(BinaryWriterEx bw);

            internal virtual void GetNames(MSB3 msb, Entries entries)
            {
                PartName = MSB.FindName(entries.Parts, PartIndex);
                PointName = MSB.FindName(entries.Regions, PointIndex);
            }

            internal virtual void GetIndices(MSB3 msb, Entries entries)
            {
                PartIndex = MSB.FindIndex(this, entries.Parts, PartName);
                PointIndex = MSB.FindIndex(this, entries.Regions, PointName);
            }

            /// <summary>
            /// Returns the type and name of this event.
            /// </summary>
            public override string ToString()
            {
                return $"{Type} : {Name}";
            }

            /// <summary>
            /// A pickuppable item.
            /// </summary>
            public class Treasure : Event
            {
                private protected override EventType Type => EventType.Treasure;

                /// <summary>
                /// The part the treasure is attached to.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Part))]
                public string TreasurePartName { get; set; }
                public int TreasurePartIndex { get; set; }

                /// <summary>
                /// First item lot given by this treasure.
                /// </summary>
                public int ItemLot1 { get; set; }

                /// <summary>
                /// Second item lot given by this treasure; rarely used.
                /// </summary>
                public int ItemLot2 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT18 { get; set; }

                /// <summary>
                /// If not -1, uses an entry from ActionButtonParam for the pickup prompt.
                /// </summary>
                public int ActionButtonParamID { get; set; }

                /// <summary>
                /// Animation to play when taking this treasure.
                /// </summary>
                public int PickupAnimID { get; set; }

                /// <summary>
                /// Changes the text of the pickup prompt and causes the treasure to be uninteractible by default.
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
                    ItemLot1 = -1;
                    ItemLot2 = -1;
                    UnkT18 = -1;
                    ActionButtonParamID = -1;
                    PickupAnimID = 60070;
                }

                internal Treasure(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    TreasurePartIndex = br.ReadInt32();
                    br.AssertInt32(0);
                    ItemLot1 = br.ReadInt32();
                    ItemLot2 = br.ReadInt32();
                    UnkT18 = br.ReadInt32();
                    br.AssertInt32(-1);
                    br.AssertInt32(-1);
                    br.AssertInt32(-1);
                    br.AssertInt32(-1);
                    br.AssertInt32(-1);
                    br.AssertInt32(-1);
                    br.AssertInt32(-1);
                    ActionButtonParamID = br.ReadInt32();
                    PickupAnimID = br.ReadInt32();

                    InChest = br.ReadBoolean();
                    StartDisabled = br.ReadBoolean();
                    br.AssertByte(0);
                    br.AssertByte(0);

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
                    bw.WriteInt32(ItemLot1);
                    bw.WriteInt32(ItemLot2);
                    bw.WriteInt32(UnkT18);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(ActionButtonParamID);
                    bw.WriteInt32(PickupAnimID);

                    bw.WriteBoolean(InChest);
                    bw.WriteBoolean(StartDisabled);
                    bw.WriteByte(0);
                    bw.WriteByte(0);

                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }

                internal override void GetNames(MSB3 msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    TreasurePartName = MSB.FindName(entries.Parts, TreasurePartIndex);
                }

                internal override void GetIndices(MSB3 msb, Entries entries)
                {
                    base.GetIndices(msb, entries);
                    TreasurePartIndex = MSB.FindIndex(this, entries.Parts, TreasurePartName);
                }
            }

            /// <summary>
            /// A continuous enemy spawner.
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
                /// Regions that enemies can be spawned at.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Region))]
                public string[] SpawnPointNames { get; private set; }
                public int[] SpawnPointIndices;

                /// <summary>
                /// Enemies spawned by this generator.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Part))]
                public string[] SpawnPartNames { get; private set; }
                public int[] SpawnPartIndices;

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
                    br.AssertByte(0);
                    br.AssertByte(0);
                    br.AssertByte(0);
                    UnkT14 = br.ReadSingle();
                    UnkT18 = br.ReadSingle();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    SpawnPointIndices = br.ReadInt32s(8);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    SpawnPartIndices = br.ReadInt32s(32);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
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
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32s(SpawnPointIndices);
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

                internal override void GetNames(MSB3 msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    SpawnPointNames = MSB.FindNames(entries.Regions, SpawnPointIndices);
                    SpawnPartNames = MSB.FindNames(entries.Parts, SpawnPartIndices);
                }

                internal override void GetIndices(MSB3 msb, Entries entries)
                {
                    base.GetIndices(msb, entries);
                    SpawnPointIndices = MSB.FindIndices(this, entries.Regions, SpawnPointNames);
                    SpawnPartIndices = MSB.FindIndices(this, entries.Parts, SpawnPartNames);
                }
            }

            /// <summary>
            /// Controls usable objects like levers.
            /// </summary>
            public class ObjAct : Event
            {
                private protected override EventType Type => EventType.ObjAct;

                /// <summary>
                /// Unknown.
                /// </summary>
                [MSBEntityReference]
                public int ObjActEntityID { get; set; }

                /// <summary>
                /// The object which is being interacted with.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Part))]
                public string ObjActPartName { get; set; }
                public int ObjActPartIndex { get; set; }

                /// <summary>
                /// ID in ObjActParam that configures this ObjAct.
                /// </summary>
                public int ObjActParamID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte ObjActStateType { get; set; } = 0;

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
                    ObjActStateType = 0;
                }

                internal ObjAct(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    ObjActEntityID = br.ReadInt32();
                    ObjActPartIndex = br.ReadInt32();
                    ObjActParamID = br.ReadInt32();

                    ObjActStateType = br.ReadByte();
                    br.AssertByte(0);
                    br.AssertByte(0);
                    br.AssertByte(0);

                    EventFlagID = br.ReadInt32();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(ObjActEntityID);
                    bw.WriteInt32(ObjActPartIndex);
                    bw.WriteInt32(ObjActParamID);

                    bw.WriteByte((byte)ObjActStateType);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteByte(0);

                    bw.WriteInt32(EventFlagID);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }

                internal override void GetNames(MSB3 msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    ObjActPartName = MSB.FindName(entries.Parts, ObjActPartIndex);
                }

                internal override void GetIndices(MSB3 msb, Entries entries)
                {
                    base.GetIndices(msb, entries);
                    ObjActPartIndex = MSB.FindIndex(this, entries.Parts, ObjActPartName);
                }
            }

            /// <summary>
            /// Moves all of the map pieces when cutscenes are played.
            /// </summary>
            public class MapOffset : Event
            {
                private protected override EventType Type => EventType.MapOffset;

                /// <summary>
                /// Position of the map offset.
                /// </summary>
                public Vector3 Position { get; set; }

                /// <summary>
                /// Rotation of the map offset.
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
            /// A fake multiplayer interaction where the player goes to an NPC's world.
            /// </summary>
            public class PseudoMultiplayer : Event
            {
                private protected override EventType Type => EventType.PseudoMultiplayer;

                /// <summary>
                /// The NPC whose world you're entering.
                /// </summary>
                [MSBEntityReference]
                public int HostEntityID { get; set; }

                /// <summary>
                /// Event flag set while the player is within a pseudo world. Unset when the player leaves.
                /// </summary>
                public int EventFlagID { get; set; }

                /// <summary>
                /// ID of a goods item that is used to trigger the event.
                /// </summary>
                public int ActivateGoodsID { get; set; }

                /// <summary>
                /// Ceremony Param ID to use.
                /// </summary>
                public int CeremonyParamID { get; set; }

                /// <summary>
                /// Region player will spawn at in pseudo world.
                /// </summary>
                public int SpawnRegionEntityID { get; set; }

                /// <summary>
                /// Player type to use while in pseudo world. 
                /// </summary>
                public byte PlayerChrType { get; set; } = 0;

                /// <summary>
                /// Determines which set of FMG entries to use in pseudo world.
                /// </summary>
                public byte MessageSetType { get; set; } = 0;

                /// <summary>
                /// ID of FMG entry to display when trying to join pseudo world.
                /// </summary>
                public int JoinMessageTextID { get; set; }

                /// <summary>
                /// Creates a new PseudoMultiplayer with the given name.
                /// </summary>
                public PseudoMultiplayer() : base($"{nameof(Event)}: {nameof(PseudoMultiplayer)}")
                {
                    HostEntityID = -1;
                    EventFlagID = -1;
                    ActivateGoodsID = -1;
                    CeremonyParamID = -1;
                    SpawnRegionEntityID = -1;
                }

                internal PseudoMultiplayer(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    HostEntityID = br.ReadInt32();
                    EventFlagID = br.ReadInt32();
                    ActivateGoodsID = br.ReadInt32();
                    CeremonyParamID = br.ReadInt32();
                    SpawnRegionEntityID = br.ReadInt32();
                    PlayerChrType = br.ReadByte();
                    MessageSetType = br.ReadByte();
                    br.ReadByte(); // Not an assert to prevent making old modded maps inaccessible.
                    br.ReadByte(); // Not an assert to prevent making old modded maps inaccessible.
                    JoinMessageTextID = br.ReadInt32();
                    br.AssertInt32(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(HostEntityID);
                    bw.WriteInt32(EventFlagID);
                    bw.WriteInt32(ActivateGoodsID);
                    bw.WriteInt32(CeremonyParamID);
                    bw.WriteInt32(SpawnRegionEntityID);
                    bw.WriteByte((byte)PlayerChrType);
                    bw.WriteByte((byte)MessageSetType);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteInt32(JoinMessageTextID);
                    bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// A simple list of points defining a path for enemies to take.
            /// </summary>
            public class PatrolInfo : Event
            {
                private protected override EventType Type => EventType.PatrolInfo;

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
                /// Creates a PatrolInfo with default values.
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
                    PatrolType = br.AssertInt32([0, 1, 2, 5]);
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

                internal override void GetNames(MSB3 msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    WalkPointNames = new string[WalkPointIndices.Length];
                    for (int i = 0; i < WalkPointIndices.Length; i++)
                        WalkPointNames[i] = MSB.FindName(entries.Regions, WalkPointIndices[i]);
                }

                internal override void GetIndices(MSB3 msb, Entries entries)
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
            public class PlatoonInfo : Event
            {
                private protected override EventType Type => EventType.PlatoonInfo;

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
                public int[] GroupPartsIndices;

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

                internal override void GetNames(MSB3 msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    GroupPartsNames = MSB.FindNames(entries.Parts, GroupPartsIndices);
                }

                internal override void GetIndices(MSB3 msb, Entries entries)
                {
                    base.GetIndices(msb, entries);
                    GroupPartsIndices = MSB.FindIndices(this, entries.Parts, GroupPartsNames);
                }
            }

            /// <summary>
            /// Unknown. Only appears once in one unused MSB so it's hard to draw too many conclusions from it.
            /// </summary>
            public class Other : Event
            {
                private protected override EventType Type => EventType.Other;

                /// <summary>
                /// Unknown; possibly a sound type.
                /// </summary>
                public int UnkT00 { get; set; }

                /// <summary>
                /// Unknown; possibly a sound ID.
                /// </summary>
                public int UnkT04 { get; set; }

                /// <summary>
                /// Creates an Other with default values.
                /// </summary>
                public Other() : base($"{nameof(Event)}: {nameof(Other)}") { }

                internal Other(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    UnkT00 = br.ReadInt32();
                    UnkT04 = br.ReadInt32();
                    br.AssertPattern(0x40, 0xFF);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(UnkT00);
                    bw.WriteInt32(UnkT04);
                    bw.WritePattern(0x40, 0xFF);
                }
            }
        }
    }
}
