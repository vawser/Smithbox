using System;
using System.Collections.Generic;
using System.IO;

namespace SoulsFormats;

public partial class MSB_NR
{
    internal enum EventType : uint
    {
        // Unused = 0,
        // Unused = 1,
        // Unused = 2,
        // Unused = 3,
        Treasure = 4,
        Generator = 5,
        // Unused = 6,
        ObjAct = 7,
        // Unused = 8,
        MapOffset = 9,
        // Unused = 10,
        // Unused = 11,
        PseudoMultiplayer = 12,
        // Unused = 13,
        PatrolInfo = 14,
        PlatoonInfo = 15,
        // Unused = 16,
        // Unused = 17,
        // Unused = 18,
        // Unused = 19,
        PatrolRoute = 20,
        Riding = 21,
        AutoDrawGroup = 22,
        SignPuddle = 23,
        RetryPoint = 24,
        BirdRoute = 25,
        TalkInfo = 26,
        TeamFight = 27,
        Other = 0xFFFFFFFF,
    }

    /// <summary>
    /// Dynamic or interactive systems such as item pickups, levers, enemy spawners, etc.
    /// </summary>
    public class EventParam : Param<Event>, IMsbParam<IMsbEvent>
    {
        public List<Event.Treasure> Treasures { get; set; }
        public List<Event.Generator> Generators { get; set; }
        public List<Event.ObjAct> ObjActs { get; set; }
        public List<Event.MapOffset> MapOffsets { get; set; }
        public List<Event.PseudoMultiplayer> PseudoMultiplayers { get; set; }
        public List<Event.PatrolInfo> PatrolInfos { get; set; }
        public List<Event.PlatoonInfo> PlatoonInfos { get; set; }
        public List<Event.PatrolRoute> PatrolRoutes { get; set; }
        public List<Event.Riding> Ridings { get; set; }
        public List<Event.AutoDrawGroup> AutoDrawGroups { get; set; }
        public List<Event.SignPuddle> SignPuddles { get; set; }
        public List<Event.RetryPoint> RetryPoints { get; set; }
        public List<Event.BirdRoute> BirdRoutes { get; set; }
        public List<Event.TalkInfo> TalkInfos { get; set; }
        public List<Event.TeamFight> TeamFights { get; set; }
        public List<Event.Other> Others { get; set; }

        public EventParam() : base(78, "EVENT_PARAM_ST")
        {
            Treasures = new List<Event.Treasure>();
            Generators = new List<Event.Generator>();
            ObjActs = new List<Event.ObjAct>();
            MapOffsets = new List<Event.MapOffset>();
            PseudoMultiplayers = new List<Event.PseudoMultiplayer>();
            PatrolInfos = new List<Event.PatrolInfo>();
            PlatoonInfos = new List<Event.PlatoonInfo>();
            PatrolRoutes = new List<Event.PatrolRoute>();
            Ridings = new List<Event.Riding>();
            AutoDrawGroups = new List<Event.AutoDrawGroup>();
            SignPuddles = new List<Event.SignPuddle>();
            RetryPoints = new List<Event.RetryPoint>();
            BirdRoutes = new List<Event.BirdRoute>();
            TalkInfos = new List<Event.TalkInfo>();
            TeamFights = new List<Event.TeamFight>();
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
                case Event.PatrolInfo e: PatrolInfos.Add(e); break;
                case Event.PlatoonInfo e: PlatoonInfos.Add(e); break;
                case Event.PatrolRoute e: PatrolRoutes.Add(e); break;
                case Event.Riding e: Ridings.Add(e); break;
                case Event.AutoDrawGroup e: AutoDrawGroups.Add(e); break;
                case Event.SignPuddle e: SignPuddles.Add(e); break;
                case Event.RetryPoint e: RetryPoints.Add(e); break;
                case Event.BirdRoute e: BirdRoutes.Add(e); break;
                case Event.TalkInfo e: TalkInfos.Add(e); break;
                case Event.TeamFight e: TeamFights.Add(e); break;
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
                Treasures, Generators, ObjActs,
                MapOffsets, PseudoMultiplayers, PatrolInfos,
                PlatoonInfos, PatrolRoutes, Ridings,
                SignPuddles, RetryPoints, BirdRoutes,
                TalkInfos, TeamFights, Others);
        }
        IReadOnlyList<IMsbEvent> IMsbParam<IMsbEvent>.GetEntries() => GetEntries();

        internal override Event ReadEntry(BinaryReaderEx br, int version)
        {
            EventType type = br.GetEnum32<EventType>(br.Position + 0xC);
            switch (type)
            {
                case EventType.Treasure:
                    return Treasures.EchoAdd(new Event.Treasure(br, version));

                case EventType.Generator:
                    return Generators.EchoAdd(new Event.Generator(br, version));

                case EventType.ObjAct:
                    return ObjActs.EchoAdd(new Event.ObjAct(br, version));

                case EventType.MapOffset:
                    return MapOffsets.EchoAdd(new Event.MapOffset(br, version));

                case EventType.PseudoMultiplayer:
                    return PseudoMultiplayers.EchoAdd(new Event.PseudoMultiplayer(br, version));

                case EventType.PatrolInfo:
                    return PatrolInfos.EchoAdd(new Event.PatrolInfo(br, version));

                case EventType.PlatoonInfo:
                    return PlatoonInfos.EchoAdd(new Event.PlatoonInfo(br, version));

                case EventType.PatrolRoute:
                    return PatrolRoutes.EchoAdd(new Event.PatrolRoute(br, version));

                case EventType.Riding:
                    return Ridings.EchoAdd(new Event.Riding(br, version));

                case EventType.SignPuddle:
                    return SignPuddles.EchoAdd(new Event.SignPuddle(br, version));

                case EventType.RetryPoint:
                    return RetryPoints.EchoAdd(new Event.RetryPoint(br, version));

                case EventType.BirdRoute:
                    return BirdRoutes.EchoAdd(new Event.BirdRoute(br, version));

                case EventType.TalkInfo:
                    return TalkInfos.EchoAdd(new Event.TalkInfo(br, version));

                case EventType.TeamFight:
                    return TeamFights.EchoAdd(new Event.TeamFight(br, version));

                case EventType.Other:
                    return Others.EchoAdd(new Event.Other(br, version));

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
        private protected virtual void ReadTypeData(BinaryReaderEx br, int version)
            => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(ReadTypeData)}.");

        private protected virtual void WriteTypeData(BinaryWriterEx bw)
            => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(ReadTypeData)}.");

        private protected Event(string name)
        {
            Name = name;
        }

        public Event DeepCopy()
        {
            var evnt = (Event)MemberwiseClone();
            DeepCopyTo(evnt);
            return evnt;
        }
        IMsbEvent IMsbEvent.DeepCopy() => DeepCopy();

        private protected virtual void DeepCopyTo(Event evnt) { }

        internal virtual void GetNames(MSB_NR msb, Entries entries)
        {
            PartName = MSB.FindName(entries.Parts, PartIndex);
            RegionName = MSB.FindName(entries.Regions, RegionIndex);
        }

        internal virtual void GetIndices(MSB_NR msb, Entries entries)
        {
            PartIndex = MSB.FindIndex(this, entries.Parts, PartName);
            RegionIndex = MSB.FindIndex(this, entries.Regions, RegionName);
        }

        public override string ToString()
        {
            return $"{Type} {Name}";
        }

        private protected Event(BinaryReaderEx br, int version)
        {
            long start = br.Position;

            NameOffset = br.ReadInt64();
            EventID = br.ReadInt32();
            br.AssertUInt32((uint)Type);
            TypeIndex = br.ReadInt32();
            Unk14_Base = br.ReadInt32();
            CommonOffset = br.ReadInt64();
            TypeOffset = br.ReadInt64();
            Unk28Offset = br.ReadInt64();


            if (!BinaryReaderEx.IgnoreAsserts)
            {
                if (NameOffset == 0)
                    throw new InvalidDataException($"{nameof(NameOffset)} must not be 0 in type {GetType()}.");

                if (CommonOffset == 0)
                    throw new InvalidDataException($"{nameof(CommonOffset)} must not be 0 in type {GetType()}.");

                if (HasTypeData ^ TypeOffset != 0)
                    throw new InvalidDataException($"Unexpected {nameof(TypeOffset)} 0x{TypeOffset:X} in type {GetType()}.");

                if (Unk28Offset == 0)
                    throw new InvalidDataException($"{nameof(Unk28Offset)} must not be 0 in type {GetType()}.");
            }

            br.Position = start + NameOffset;
            Name = br.ReadUTF16();

            // Common
            br.Position = start + CommonOffset;
            PartIndex = br.ReadInt32();
            RegionIndex = br.ReadInt32();
            EntityID = br.ReadUInt32();
            Unk0C_Common = br.ReadSByte();
            Unk0D_Common = br.ReadByte();
            Unk0E_Common = br.ReadInt16();

            // Type
            if (HasTypeData)
            {
                br.Position = start + TypeOffset;
                ReadTypeData(br, version);
            }

            // Unk 28
            br.Position = start + Unk28Offset;
            MapID = br.ReadMapIDBytes(4);
            Unk04_Map = br.ReadInt32();
            Unk08_Map = br.ReadInt32();
            SourceEventID = br.ReadInt32();
            Unk10_Map = br.ReadInt32();
            Unk14_Map = br.ReadInt32();
            Unk18_Map = br.ReadInt32();
            Unk1C_Map = br.ReadInt32();
        }

        internal override void Write(BinaryWriterEx bw, int id)
        {
            long start = bw.Position;

            bw.ReserveInt64("NameOffset");
            bw.WriteInt32(EventID);
            bw.WriteUInt32((uint)Type);

            if (TypeIndex != -1)
                bw.WriteInt32(TypeIndex);
            else
                bw.WriteInt32(id);

            bw.WriteInt32(Unk14_Base);

            bw.ReserveInt64("CommonOffset");
            bw.ReserveInt64("TypeOffset");
            bw.ReserveInt64("Unk28Offset");

            bw.FillInt64("NameOffset", bw.Position - start);
            bw.WriteUTF16(MSB.ReambiguateName(Name), true);
            bw.Pad(8);

            // Common
            bw.FillInt64("CommonOffset", bw.Position - start);
            bw.WriteInt32(PartIndex);
            bw.WriteInt32(RegionIndex);
            bw.WriteUInt32(EntityID);
            bw.WriteSByte(Unk0C_Common);
            bw.WriteByte(Unk0D_Common);
            bw.WriteInt16(Unk0E_Common);

            // Type
            if (HasTypeData)
            {
                bw.FillInt64("TypeOffset", bw.Position - start);
                WriteTypeData(bw);
            }
            else
            {
                bw.FillInt64("TypeOffset", 0);
            }

            switch (Type)
            {
                case EventType.PseudoMultiplayer:
                    bw.Pad(4);
                    break;
                case EventType.TeamFight:
                    bw.Pad(4);
                    break;
                default:
                    bw.Pad(8);
                    break;
            }

            // Unk 28
            bw.FillInt64("Unk28Offset", bw.Position - start);
            bw.WriteMapIDBytes(MapID);
            bw.WriteInt32(Unk04_Map);
            bw.WriteInt32(Unk08_Map);
            bw.WriteInt32(SourceEventID);
            bw.WriteInt32(Unk10_Map);
            bw.WriteInt32(Unk14_Map);
            bw.WriteInt32(Unk18_Map);
            bw.WriteInt32(Unk1C_Map);

            // ???
            if (Type == EventType.TeamFight)
            {
                bw.Pad(8);
            }
        }

        // Layout
        public int EventID { get; set; } = -1;
        internal int TypeIndex { get; set; }
        private int Unk14_Base { get; set; } = 0; // Hidden: not read, 0 in retail

        // Offsets
        internal long NameOffset { get; set; }
        internal long CommonOffset { get; set; }
        internal long TypeOffset { get; set; }
        internal long Unk28Offset { get; set; }

        // Common - Not separated into discrete class for simplicity
        private int PartIndex { get; set; } = -1;
        private int RegionIndex { get; set; } = -1;
        public uint EntityID { get; set; } = 0;
        private sbyte Unk0C_Common { get; set; } = -1; // Hidden: not read; 2 (or 1) on events copied from another map, else -1
        private byte Unk0D_Common { get; set; } = 0; // Hidden
        private short Unk0E_Common { get; set; } = 0; // Hidden

        // Unk 28: the source of an event copied from another map (open-world LOD tiles), like the parts' TileStruct.
        // Not separated into discrete class for simplicity.
        private sbyte[] MapID { get; set; } = new sbyte[4]; // Hidden: not read for events; the source map of a copied event
        private int Unk04_Map { get; set; } = 0; // Hidden: not read for events; 13 (the "mAA_BB_CC_DD-" name prefix length) on copies
        private int Unk08_Map { get; set; } = 0; // Hidden

        /// <summary>
        /// The event ID the game uses when 0 or more; -1 uses EventID (nightreign.exe 140E3A0F0, used for platoons and
        /// points). Copies on LOD tiles carry the original event's ID. Was Unk0C_Map.
        /// </summary>
        public int SourceEventID { get; set; } = -1;
        private int Unk10_Map { get; set; } = 0; // Hidden
        private int Unk14_Map { get; set; } = 0; // Hidden
        private int Unk18_Map { get; set; } = 0; // Hidden
        private int Unk1C_Map { get; set; } = 0; // Hidden

        // Names
        [MSBReference(ReferenceType = typeof(Part))]
        public string PartName { get; set; }

        [MSBReference(ReferenceType = typeof(Region))]
        public string RegionName { get; set; }

        /// <summary>
        /// An item pickup in the open or inside a container.
        /// </summary>
        public class Treasure : Event
        {
            private protected override EventType Type => EventType.Treasure;
            private protected override bool HasTypeData => true;

            public Treasure() : base($"{nameof(Event)}: {nameof(Treasure)}") { }

            internal Treasure(BinaryReaderEx br, int version) : base(br, version) { }

            internal override void GetNames(MSB_NR msb, Entries entries)
            {
                base.GetNames(msb, entries);
                TreasurePartName = MSB.FindName(entries.Parts, TreasurePartIndex);
            }

            internal override void GetIndices(MSB_NR msb, Entries entries)
            {
                base.GetIndices(msb, entries);
                TreasurePartIndex = MSB.FindIndex(this, entries.Parts, TreasurePartName);
            }

            private protected override void ReadTypeData(BinaryReaderEx br, int version)
            {
                Unk00 = br.ReadInt32();
                Unk04 = br.ReadInt32();
                TreasurePartIndex = br.ReadInt32();
                Unk0C = br.ReadInt32();
                ItemLotID = br.ReadInt32();
                Unk14 = br.ReadInt32();
                Unk18 = br.ReadInt32();
                Unk1C = br.ReadInt32();
                Unk20 = br.ReadInt32();
                Unk24 = br.ReadInt32();
                Unk28 = br.ReadInt32();
                Unk2C = br.ReadInt32();
                Unk30 = br.ReadInt32();
                Unk34 = br.ReadInt32();
                UncommonItemLotID = br.ReadInt32();
                PickupAnimID = br.ReadInt32();
                InChest = br.ReadByte();
                StartDisabled = br.ReadByte();
                Unk42 = br.ReadByte();
                Unk43 = br.ReadByte();
                Unk44 = br.ReadInt32();
                Unk48 = br.ReadInt16();
                Unk4A = br.ReadInt16();
                Unk4C = br.ReadInt32();
            }

            private protected override void WriteTypeData(BinaryWriterEx bw)
            {
                bw.WriteInt32(Unk00);
                bw.WriteInt32(Unk04);
                bw.WriteInt32(TreasurePartIndex);
                bw.WriteInt32(Unk0C);
                bw.WriteInt32(ItemLotID);
                bw.WriteInt32(Unk14);
                bw.WriteInt32(Unk18);
                bw.WriteInt32(Unk1C);
                bw.WriteInt32(Unk20);
                bw.WriteInt32(Unk24);
                bw.WriteInt32(Unk28);
                bw.WriteInt32(Unk2C);
                bw.WriteInt32(Unk30);
                bw.WriteInt32(Unk34);
                bw.WriteInt32(UncommonItemLotID);
                bw.WriteInt32(PickupAnimID);
                bw.WriteByte(InChest);
                bw.WriteByte(StartDisabled);
                bw.WriteByte(Unk42);
                bw.WriteByte(Unk43);
                bw.WriteInt32(Unk44);
                bw.WriteInt16(Unk48);
                bw.WriteInt16(Unk4A);
                bw.WriteInt32(Unk4C);
            }

            // Layout. Read by the treasure setup 14056DE40 (+10, +38, +40, +42, +43, +44, +48, +4A).
            private int Unk00 { get; set; } = 0; // Hidden: not read, 0 in retail
            private int Unk04 { get; set; } = 0; // Hidden: not read, 0 in retail
            private int TreasurePartIndex { get; set; } = -1;
            private int Unk0C { get; set; } = 0; // Hidden: not read, 0 in retail
            public int ItemLotID { get; set; } = -1;
            private int Unk14 { get; set; } = -1; // Hidden: not read, -1 in retail
            private int Unk18 { get; set; } = -1; // Hidden: not read, -1 in retail
            private int Unk1C { get; set; } = -1; // Hidden: not read, -1 in retail
            private int Unk20 { get; set; } = -1; // Hidden: not read, -1 in retail
            private int Unk24 { get; set; } = -1; // Hidden: not read, -1 in retail
            private int Unk28 { get; set; } = -1; // Hidden: not read, -1 in retail
            private int Unk2C { get; set; } = -1; // Hidden: not read, -1 in retail
            private int Unk30 { get; set; } = -1; // Hidden: not read, -1 in retail
            private int Unk34 { get; set; } = -1; // Hidden: not read, -1 in retail

            /// <summary>
            /// A second item lot, copied with ItemLotID into the treasure's setup. -1 for none (543 of 1,010 retail treasures).
            /// </summary>
            public int UncommonItemLotID { get; set; } = -1;
            public int PickupAnimID { get; set; } = -1;

            /// <summary>
            /// The treasure's kind, copied into its setup and compared with 1 (see Unk42). 1 on 1,002 retail treasures,
            /// else 0, 3 or 4. Not a bool despite the name.
            /// </summary>
            public byte InChest { get; set; } = 1;
            public byte StartDisabled { get; set; } = 0; // Boolean

            /// <summary>
            /// Enables a check with Unk48: only when InChest is 1, Unk4A is 4 and a session query returns 2 (14056F0F7).
            /// ON for 448 retail treasures.
            /// </summary>
            public byte Unk42 { get; set; } = 0; // Boolean

            /// <summary>
            /// Copied into the treasure's setup as a bool. 1 on every retail treasure.
            /// </summary>
            public byte Unk43 { get; set; } = 1; // Boolean

            /// <summary>
            /// A third item lot-like ID copied into the treasure's setup (often ItemLotID's family + 500). -1 for none.
            /// </summary>
            public int Unk44 { get; set; } = -1;

            /// <summary>
            /// Read as an unsigned short for the Unk42 check (0xFFFF = none). -1, or 10 to 170 in steps of 10.
            /// </summary>
            public short Unk48 { get; set; } = -1;

            /// <summary>
            /// Read as a byte and copied into the treasure's setup; 4 is required by the Unk42 check. 0 or 4 in retail.
            /// </summary>
            public short Unk4A { get; set; } = 0;
            private int Unk4C { get; set; } = 0; // Hidden: not read, 0 in retail

            // Names
            [MSBReference(ReferenceType = typeof(Part))]
            public string TreasurePartName { get; set; }
        }

        /// <summary>
        /// An enemy spawner.
        /// </summary>
        public class Generator : Event
        {
            private protected override EventType Type => EventType.Generator;
            private protected override bool HasTypeData => true;

            public Generator() : base($"{nameof(Event)}: {nameof(Generator)}") { }
            internal Generator(BinaryReaderEx br, int version) : base(br, version) { }

            private protected override void DeepCopyTo(Event evnt)
            {
                var generator = (Generator)evnt;
                generator.SpawnRegionNames = (string[])SpawnRegionNames.Clone();
                generator.SpawnPartNames = (string[])SpawnPartNames.Clone();
            }

            internal override void GetNames(MSB_NR msb, Entries entries)
            {
                base.GetNames(msb, entries);
                SpawnRegionNames = MSB.FindNames(entries.Regions, SpawnRegionIndices);
                SpawnPartNames = MSB.FindNames(entries.Parts, SpawnPartIndices);
            }

            internal override void GetIndices(MSB_NR msb, Entries entries)
            {
                base.GetIndices(msb, entries);
                SpawnRegionIndices = MSB.FindIndices(this, entries.Regions, SpawnRegionNames);
                SpawnPartIndices = MSB.FindIndices(this, entries.Parts, SpawnPartNames);
            }

            private protected override void ReadTypeData(BinaryReaderEx br, int version)
            {
                MaxNum = br.ReadByte();
                GenType = br.ReadSByte();
                LimitNum = br.ReadInt16();
                MinGenNum = br.ReadInt16();
                MaxGenNum = br.ReadInt16();
                MinInterval = br.ReadSingle();
                MaxInterval = br.ReadSingle();
                InitialSpawnCount = br.ReadSByte();
                Unk11 = br.ReadByte();
                Unk12 = br.ReadInt16();
                Unk14 = br.ReadSingle();
                Unk18 = br.ReadSingle();
                Unk1C = br.ReadInt32();
                Unk20 = br.ReadInt32();
                Unk24 = br.ReadInt32();
                Unk28 = br.ReadInt32();
                Unk2C = br.ReadInt32();
                SpawnRegionIndices = br.ReadInt32s(8);
                Unk50 = br.ReadInt32();
                Unk54 = br.ReadInt32();
                Unk58 = br.ReadInt32();
                Unk5C = br.ReadInt32();
                SpawnPartIndices = br.ReadInt32s(32);
                UnkE0 = br.ReadInt32();
                UnkE4 = br.ReadInt32();
                UnkE8 = br.ReadInt32();
                UnkEC = br.ReadInt32();
                UnkF0 = br.ReadInt32();
                UnkF4 = br.ReadInt32();
                UnkF8 = br.ReadInt32();
                UnkFC = br.ReadInt32();
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
                bw.WriteSByte(InitialSpawnCount);
                bw.WriteByte(Unk11);
                bw.WriteInt16(Unk12);
                bw.WriteSingle(Unk14);
                bw.WriteSingle(Unk18);
                bw.WriteInt32(Unk1C);
                bw.WriteInt32(Unk20);
                bw.WriteInt32(Unk24);
                bw.WriteInt32(Unk28);
                bw.WriteInt32(Unk2C);
                bw.WriteInt32s(SpawnRegionIndices);
                bw.WriteInt32(Unk50);
                bw.WriteInt32(Unk54);
                bw.WriteInt32(Unk58);
                bw.WriteInt32(Unk5C);
                bw.WriteInt32s(SpawnPartIndices);
                bw.WriteInt32(UnkE0);
                bw.WriteInt32(UnkE4);
                bw.WriteInt32(UnkE8);
                bw.WriteInt32(UnkEC);
                bw.WriteInt32(UnkF0);
                bw.WriteInt32(UnkF4);
                bw.WriteInt32(UnkF8);
                bw.WriteInt32(UnkFC);
            }

            // Layout
            public byte MaxNum { get; set; } = 1;
            public sbyte GenType { get; set; }
            public short LimitNum { get; set; } = -1;
            public short MinGenNum { get; set; } = 1;
            public short MaxGenNum { get; set; } = 1;
            public float MinInterval { get; set; } = 0;
            public float MaxInterval { get; set; } = 0;
            public sbyte InitialSpawnCount { get; set; } = -1;
            private byte Unk11 { get; set; } = 0; // Hidden: not read, 0 in retail
            private short Unk12 { get; set; } = 0; // Hidden: not read, 0 in retail
            public float Unk14 { get; set; } = 0;
            public float Unk18 { get; set; } = 0;
            private int Unk1C { get; set; } = 0; // Hidden: not read, 0 in retail
            private int Unk20 { get; set; } = 0; // Hidden: not read, 0 in retail
            private int Unk24 { get; set; } = 0; // Hidden: not read, 0 in retail
            private int Unk28 { get; set; } = 0; // Hidden: not read, 0 in retail
            private int Unk2C { get; set; } = 0; // Hidden: not read, 0 in retail
            private int[] SpawnRegionIndices { get; set; } = new int[8];
            private int Unk50 { get; set; } = 0; // Hidden: not read, 0 in retail
            private int Unk54 { get; set; } = 0; // Hidden: not read, 0 in retail
            private int Unk58 { get; set; } = 0; // Hidden: not read, 0 in retail
            private int Unk5C { get; set; } = 0; // Hidden: not read, 0 in retail
            private int[] SpawnPartIndices { get; set; } = new int[32];
            private int UnkE0 { get; set; } = 0; // Hidden: not read, 0 in retail
            private int UnkE4 { get; set; } = 0; // Hidden: not read, 0 in retail
            private int UnkE8 { get; set; } = 0; // Hidden: not read, 0 in retail
            private int UnkEC { get; set; } = 0; // Hidden: not read, 0 in retail
            private int UnkF0 { get; set; } = 0; // Hidden: not read, 0 in retail
            private int UnkF4 { get; set; } = 0; // Hidden: not read, 0 in retail
            private int UnkF8 { get; set; } = 0; // Hidden: not read, 0 in retail
            private int UnkFC { get; set; } = 0; // Hidden: not read, 0 in retail

            // Names
            [MSBReference(ReferenceType = typeof(Region))]
            public string[] SpawnRegionNames { get; set; } = new string[8];

            [MSBReference(ReferenceType = typeof(Part))]
            public string[] SpawnPartNames { get; set; } = new string[32];
        }

        /// <summary>
        /// An interactive object.
        /// </summary>
        public class ObjAct : Event
        {
            private protected override EventType Type => EventType.ObjAct;
            private protected override bool HasTypeData => true;

            public ObjAct() : base($"{nameof(Event)}: {nameof(ObjAct)}") { }
            internal ObjAct(BinaryReaderEx br, int version) : base(br, version) { }

            internal override void GetNames(MSB_NR msb, Entries entries)
            {
                base.GetNames(msb, entries);
                ObjActPartName = MSB.FindName(entries.Parts, ObjActPartIndex);
            }

            internal override void GetIndices(MSB_NR msb, Entries entries)
            {
                base.GetIndices(msb, entries);
                ObjActPartIndex = MSB.FindIndex(this, entries.Parts, ObjActPartName);
            }

            private protected override void ReadTypeData(BinaryReaderEx br, int version)
            {
                ObjActEntityId = br.ReadUInt32();
                ObjActPartIndex = br.ReadInt32();
                ObjActParamId = br.ReadInt32();
                StateType = br.ReadByte();
                unk0D = br.ReadByte();
                unk0E = br.ReadInt16();
                EventFlagID = br.ReadInt32();
                Unk14 = br.ReadInt32();
                Unk18 = br.ReadInt32();
                Unk1C = br.ReadInt32();
            }

            private protected override void WriteTypeData(BinaryWriterEx bw)
            {
                bw.WriteUInt32(ObjActEntityId);
                bw.WriteInt32(ObjActPartIndex);
                bw.WriteInt32(ObjActParamId);
                bw.WriteByte(StateType);
                bw.WriteByte(unk0D);
                bw.WriteInt16(unk0E);
                bw.WriteInt32(EventFlagID);
                bw.WriteInt32(Unk14);
                bw.WriteInt32(Unk18);
                bw.WriteInt32(Unk1C);
            }

            // Layout
            public uint ObjActEntityId { get; set; } = 0;
            private int ObjActPartIndex { get; set; } = -1;
            public int ObjActParamId { get; set; }
            public byte StateType { get; set; } = 5;
            private byte unk0D { get; set; } = 0;
            private short unk0E { get; set; } = -1;
            public int EventFlagID { get; set; } = 0;
            private int Unk14 { get; set; } = -1; // Hidden: not read (the ObjAct setup 14037F2F0 reads +4, +8, +10; +0C in 14037F620), -1 in retail
            private int Unk18 { get; set; } = 0; // Hidden
            private int Unk1C { get; set; } = 0; // Hidden

            // Names

            [MSBReference(ReferenceType = typeof(Part))]
            public string ObjActPartName { get; set; }

        }

        /// <summary>
        /// 
        /// </summary>
        public class MapOffset : Event
        {
            private protected override EventType Type => EventType.MapOffset;
            private protected override bool HasTypeData => true;

            public MapOffset() : base($"{nameof(Event)}: {nameof(MapOffset)}") { }

            internal MapOffset(BinaryReaderEx br, int version) : base(br, version) { }
            private protected override void ReadTypeData(BinaryReaderEx br, int version)
            {
            }

            private protected override void WriteTypeData(BinaryWriterEx bw)
            {
            }

            // Layout
        }

        /// <summary>
        /// A fake multiplayer interaction where the player goes to an NPC's world.
        /// </summary>
        public class PseudoMultiplayer : Event
        {
            private protected override EventType Type => EventType.PseudoMultiplayer;
            private protected override bool HasTypeData => true;

            public PseudoMultiplayer() : base($"{nameof(Event)}: {nameof(PseudoMultiplayer)}") { }

            internal PseudoMultiplayer(BinaryReaderEx br, int version) : base(br, version) { }

            private protected override void ReadTypeData(BinaryReaderEx br, int version)
            {
            }

            private protected override void WriteTypeData(BinaryWriterEx bw)
            {
            }

            // Layout
        }

        /// <summary>
        /// 
        /// </summary>
        public class PatrolInfo : Event
        {
            private protected override EventType Type => EventType.PatrolInfo;
            private protected override bool HasTypeData => true;

            public PatrolInfo() : base($"{nameof(Event)}: {nameof(PatrolInfo)}") { }

            internal PatrolInfo(BinaryReaderEx br, int version) : base(br, version) { }

            private protected override void ReadTypeData(BinaryReaderEx br, int version)
            {
            }

            private protected override void WriteTypeData(BinaryWriterEx bw)
            {
            }

            // Layout
        }

        /// <summary>
        /// 
        /// </summary>
        public class PlatoonInfo : Event
        {
            private protected override EventType Type => EventType.PlatoonInfo;
            private protected override bool HasTypeData => true;

            public PlatoonInfo() : base($"{nameof(Event)}: {nameof(PlatoonInfo)}") { }
            internal PlatoonInfo(BinaryReaderEx br, int version) : base(br, version) { }

            private protected override void DeepCopyTo(Event evnt)
            {
                var groupTour = (PlatoonInfo)evnt;
                groupTour.GroupPartsNames = (string[])GroupPartsNames.Clone();
                groupTour.MemberEntityIDs = (int[])MemberEntityIDs.Clone();
            }
            internal override void GetNames(MSB_NR msb, Entries entries)
            {
                base.GetNames(msb, entries);
                GroupPartsNames = MSB.FindNames(entries.Parts, GroupPartsIndices);
            }
            internal override void GetIndices(MSB_NR msb, Entries entries)
            {
                base.GetIndices(msb, entries);
                GroupPartsIndices = MSB.FindIndices(this, entries.Parts, GroupPartsNames);
            }
            private protected override void ReadTypeData(BinaryReaderEx br, int version)
            {
                PlatoonScriptId = br.ReadInt32();
                State = br.ReadByte();
                EnableDeactiveMove = br.ReadByte();
                Unk06 = br.ReadByte();
                UseMemberEntityIDs = br.ReadByte();
                MemberEntityIDs = br.ReadInt32s(2);
                GroupPartsIndices = br.ReadInt32s(32);
            }

            private protected override void WriteTypeData(BinaryWriterEx bw)
            {
                bw.WriteInt32(PlatoonScriptId);
                bw.WriteByte(State);
                bw.WriteByte(EnableDeactiveMove);
                bw.WriteByte(Unk06);
                bw.WriteByte(UseMemberEntityIDs);
                bw.WriteInt32s(MemberEntityIDs);
                bw.WriteInt32s(GroupPartsIndices);
            }

            // Layout: same as MSBE.PlatoonInfo; the platoon setup (nightreign.exe 14030C340) reads +0, +4, +5, +7, +8 and
            // the group parts as eldenring.exe does.
            public int PlatoonScriptId { get; set; } = -1;

            /// <summary>
            /// The platoon's initial reflect state (AI scripts change it with SetReflectState). Same as MSBE. Was Unk04.
            /// 1 on 28 of 78 retail platoons.
            /// </summary>
            public byte State { get; set; } = 0;

            /// <summary>
            /// The initial value of the platoon's enable-deactive-move flag (AI: SetEnableDeactiveMove). Same as MSBE.
            /// Was the hidden Unk05. 0 in retail.
            /// </summary>
            public byte EnableDeactiveMove { get; set; } = 0; // Bool
            private byte Unk06 { get; set; } = 0; // Hidden: not read (MSBE UnkT06), 0 in retail

            /// <summary>
            /// If set, the characters in MemberEntityIDs also join the platoon. Same as MSBE. Was the high byte of the
            /// hidden short Unk06. 0 in retail.
            /// </summary>
            public byte UseMemberEntityIDs { get; set; } = 0; // Bool

            /// <summary>
            /// Characters that join the platoon when UseMemberEntityIDs is set (ER: 10000 is the player). 0 for none. Was
            /// the hidden Unk08 / Unk0C. 0 in retail.
            /// </summary>
            public int[] MemberEntityIDs { get; set; } = new int[2];
            private int[] GroupPartsIndices { get; set; } = new int[32];

            // Names
            [MSBReference(ReferenceType = typeof(Part))]
            public string[] GroupPartsNames { get; set; } = new string[32];

        }

        /// <summary>
        /// 
        /// </summary>
        public class PatrolRoute : Event
        {
            private protected override EventType Type => EventType.PatrolRoute;
            private protected override bool HasTypeData => true;

            public PatrolRoute() : base($"{nameof(Event)}: {nameof(PatrolRoute)}") { }
            internal PatrolRoute(BinaryReaderEx br, int version) : base(br, version) { }

            private protected override void DeepCopyTo(Event evnt)
            {
                var walkRoute = (PatrolRoute)evnt;
                walkRoute.RegionNames = (string[])RegionNames.Clone();
            }

            internal override void GetNames(MSB_NR msb, Entries entries)
            {
                base.GetNames(msb, entries);
                RegionNames = MSB.FindNames(entries.Regions, RegionIndices);
            }

            internal override void GetIndices(MSB_NR msb, Entries entries)
            {
                base.GetIndices(msb, entries);
                RegionIndices = MSB.FindShortIndices(entries.Regions, RegionNames);
            }

            private protected override void ReadTypeData(BinaryReaderEx br, int version)
            {
                PatrolType = br.ReadByte();
                Unk01 = br.ReadByte();
                Unk02 = br.ReadByte();
                UseAllWalkRegions = br.ReadByte();
                Unk04 = br.ReadInt32();
                Unk08 = br.ReadInt32();
                Unk0C = br.ReadInt32();
                RegionIndices = br.ReadInt16s(64);
            }

            private protected override void WriteTypeData(BinaryWriterEx bw)
            {
                bw.WriteByte(PatrolType);
                bw.WriteByte(Unk01);
                bw.WriteByte(Unk02);
                bw.WriteByte(UseAllWalkRegions);
                bw.WriteInt32(Unk04);
                bw.WriteInt32(Unk08);
                bw.WriteInt32(Unk0C);
                bw.WriteInt16s(RegionIndices);
            }

            // Layout
            // Layout. The walk route setup (nightreign.exe 140322810, event type 20) reads EntityID, PatrolType and
            // UseAllWalkRegions, as MSBE.PatrolInfo.
            public byte PatrolType { get; set; } = 0;
            private byte Unk01 { get; set; } = 0; // Hidden: not read, 0 in retail
            private byte Unk02 { get; set; } = 0; // Hidden: not read, 0 in retail

            /// <summary>
            /// If set, all 64 walk regions are used; otherwise only the first 24 (140322981). Same as MSBE. Was Unk03.
            /// 1 on every retail walk route.
            /// </summary>
            public byte UseAllWalkRegions { get; set; } = 1; // Bool
            private int Unk04 { get; set; } = -1; // Hidden: not read, -1 in retail
            private int Unk08 { get; set; } = 0; // Hidden: not read, 0 in retail
            private int Unk0C { get; set; } = 0; // Hidden: not read, 0 in retail

            private short[] RegionIndices { get; set; } = new short[64];

            // Names
            [MSBReference(ReferenceType = typeof(Region))]
            public string[] RegionNames { get; set; } = new string[64];
        }

        /// <summary>
        /// 
        /// </summary>
        public class Riding : Event
        {
            private protected override EventType Type => EventType.Riding;
            private protected override bool HasTypeData => true;

            public Riding() : base($"{nameof(Event)}: {nameof(Riding)}") { }
            internal Riding(BinaryReaderEx br, int version) : base(br, version) { }

            internal override void GetNames(MSB_NR msb, Entries entries)
            {
                base.GetNames(msb, entries);
                RiderPartName = MSB.FindName(entries.Parts, RiderPartIndex);
                MountPartName = MSB.FindName(entries.Parts, MountPartIndex);
            }

            internal override void GetIndices(MSB_NR msb, Entries entries)
            {
                base.GetIndices(msb, entries);
                RiderPartIndex = MSB.FindIndex(this, entries.Parts, RiderPartName);
                MountPartIndex = MSB.FindIndex(this, entries.Parts, MountPartName);
            }

            private protected override void ReadTypeData(BinaryReaderEx br, int version)
            {
                RiderPartIndex = br.ReadInt32();
                MountPartIndex = br.ReadInt32();
            }

            private protected override void WriteTypeData(BinaryWriterEx bw)
            {
                bw.WriteInt32(RiderPartIndex);
                bw.WriteInt32(MountPartIndex);
            }

            // Layout
            private int RiderPartIndex { get; set; }
            private int MountPartIndex { get; set; }

            // Names
            [MSBReference(ReferenceType = typeof(Part))]
            public string RiderPartName { get; set; }

            [MSBReference(ReferenceType = typeof(Part))]
            public string MountPartName { get; set; }

        }

        /// <summary>
        /// 
        /// </summary>
        public class AutoDrawGroup : Event
        {
            private protected override EventType Type => EventType.AutoDrawGroup;
            private protected override bool HasTypeData => true;

            public AutoDrawGroup() : base($"{nameof(Event)}: {nameof(AutoDrawGroup)}") { }

            internal AutoDrawGroup(BinaryReaderEx br, int version) : base(br, version) { }
            private protected override void ReadTypeData(BinaryReaderEx br, int version)
            {
            }

            private protected override void WriteTypeData(BinaryWriterEx bw)
            {
            }

            // Layout
        }

        /// <summary>
        /// 
        /// </summary>
        public class SignPuddle : Event
        {
            private protected override EventType Type => EventType.SignPuddle;
            private protected override bool HasTypeData => true;

            public SignPuddle() : base($"{nameof(Event)}: {nameof(SignPuddle)}") { }
            internal SignPuddle(BinaryReaderEx br, int version) : base(br, version) { }

            private protected override void ReadTypeData(BinaryReaderEx br, int version)
            {
            }

            private protected override void WriteTypeData(BinaryWriterEx bw)
            {
            }

            // Layout
        }

        /// <summary>
        /// 
        /// </summary>
        public class RetryPoint : Event
        {
            private protected override EventType Type => EventType.RetryPoint;
            private protected override bool HasTypeData => true;

            public RetryPoint() : base($"{nameof(Event)}: {nameof(RetryPoint)}") { }
            internal RetryPoint(BinaryReaderEx br, int version) : base(br, version) { }

            private protected override void ReadTypeData(BinaryReaderEx br, int version)
            {
            }

            private protected override void WriteTypeData(BinaryWriterEx bw)
            {
            }

            // Layout
        }

        /// <summary>
        /// 
        /// </summary>
        public class BirdRoute : Event
        {
            private protected override EventType Type => EventType.BirdRoute;
            private protected override bool HasTypeData => true;

            public BirdRoute() : base($"{nameof(Event)}: {nameof(BirdRoute)}") { }

            internal BirdRoute(BinaryReaderEx br, int version) : base(br, version) { }

            private protected override void DeepCopyTo(Event evnt)
            {
                var birdRoute = (BirdRoute)evnt;
                birdRoute.RegionNames = (string[])RegionNames.Clone();
            }

            internal override void GetNames(MSB_NR msb, Entries entries)
            {
                base.GetNames(msb, entries);
                RegionNames = MSB.FindNames(entries.Regions, RegionIndices);
            }

            internal override void GetIndices(MSB_NR msb, Entries entries)
            {
                base.GetIndices(msb, entries);
                RegionIndices = MSB.FindShortIndices(entries.Regions, RegionNames);
            }

            private protected override void ReadTypeData(BinaryReaderEx br, int version)
            {
                Unk00 = br.ReadInt32();
                Unk04 = br.ReadInt32();
                Unk08 = br.ReadInt32();
                Unk0C = br.ReadInt32();
                RegionIndices = br.ReadInt16s(32);
            }

            private protected override void WriteTypeData(BinaryWriterEx bw)
            {
                bw.WriteInt32(Unk00);
                bw.WriteInt32(Unk04);
                bw.WriteInt32(Unk08);
                bw.WriteInt32(Unk0C);
                bw.WriteInt16s(RegionIndices);
            }

            // Layout. Not seen read by nightreign.exe (45 retail routes, all in m60_00_00_99, through bird move points).
            private int Unk00 { get; set; } = 0; // Hidden: 0 in retail

            /// <summary>
            /// Unknown; not seen read. 0, or 7601-7605 on 7 retail routes.
            /// </summary>
            public int Unk04 { get; set; } = 0;

            /// <summary>
            /// Unknown; not seen read. 6001 on 30 retail routes, else 0, 7601 or 7603.
            /// </summary>
            public int Unk08 { get; set; } = 0;

            /// <summary>
            /// Unknown; not seen read. 0, or 7602 / 7605 on 4 retail routes.
            /// </summary>
            public int Unk0C { get; set; } = 0;
            private short[] RegionIndices { get; set; } = new short[32]; // was public; the names are written back over it

            // Names
            [MSBReference(ReferenceType = typeof(Region))]
            public string[] RegionNames { get; set; } = new string[32];

        }

        /// <summary>
        /// 
        /// </summary>
        public class TalkInfo : Event
        {
            private protected override EventType Type => EventType.TalkInfo;
            private protected override bool HasTypeData => true;

            public TalkInfo() : base($"{nameof(Event)}: {nameof(TalkInfo)}") { }

            internal TalkInfo(BinaryReaderEx br, int version) : base(br, version) { }

            private protected override void DeepCopyTo(Event evnt)
            {
                var talk = (TalkInfo)evnt;
                talk.EnemyPartNames = (string[])EnemyPartNames.Clone();
                talk.TalkEntries = (int[])TalkEntries.Clone();
            }

            private protected override void ReadTypeData(BinaryReaderEx br, int version)
            {
                EnemyPartIndices = br.ReadInt32s(4);
                Unk10 = br.ReadInt32();
                Unk14 = br.ReadInt32();
                Unk18 = br.ReadInt32();
                Unk1C = br.ReadInt32();
                Unk20 = br.ReadInt32();
                TalkEntries = br.ReadInt32s(3);
                Unk30 = br.ReadInt32();
                Unk34 = br.ReadInt32();
                Unk38 = br.ReadInt32();
                Unk3C = br.ReadInt32();
                Unk40 = br.ReadInt32();
                Unk44 = br.ReadByte();
                Unk45 = br.ReadByte();
                Unk46 = br.ReadByte();
                Unk47 = br.ReadByte();
                Unk48 = br.ReadInt32();
                Unk4C = br.ReadInt32();
                Unk50 = br.ReadInt32();
                Unk54 = br.ReadInt32();
                Unk58 = br.ReadInt32();
                Unk5C = br.ReadInt32();
                Unk60 = br.ReadInt32();
                Unk64 = br.ReadInt32();
                Unk68 = br.ReadInt32();
                Unk6C = br.ReadInt32();
                Unk70 = br.ReadInt32();
                Unk74 = br.ReadInt32();
                Unk78 = br.ReadInt32();
                Unk7C = br.ReadInt32();
            }

            private protected override void WriteTypeData(BinaryWriterEx bw)
            {
                bw.WriteInt32s(EnemyPartIndices);
                bw.WriteInt32(Unk10);
                bw.WriteInt32(Unk14);
                bw.WriteInt32(Unk18);
                bw.WriteInt32(Unk1C);
                bw.WriteInt32(Unk20);
                bw.WriteInt32s(TalkEntries);
                bw.WriteInt32(Unk30);
                bw.WriteInt32(Unk34);
                bw.WriteInt32(Unk38);
                bw.WriteInt32(Unk3C);
                bw.WriteInt32(Unk40);
                bw.WriteByte(Unk44);
                bw.WriteByte(Unk45);
                bw.WriteByte(Unk46);
                bw.WriteByte(Unk47);
                bw.WriteInt32(Unk48);
                bw.WriteInt32(Unk4C);
                bw.WriteInt32(Unk50);
                bw.WriteInt32(Unk54);
                bw.WriteInt32(Unk58);
                bw.WriteInt32(Unk5C);
                bw.WriteInt32(Unk60);
                bw.WriteInt32(Unk64);
                bw.WriteInt32(Unk68);
                bw.WriteInt32(Unk6C);
                bw.WriteInt32(Unk70);
                bw.WriteInt32(Unk74);
                bw.WriteInt32(Unk78);
                bw.WriteInt32(Unk7C);
            }

            internal override void GetNames(MSB_NR msb, Entries entries)
            {
                base.GetNames(msb, entries);
                EnemyPartNames = MSB.FindNames(msb.Parts.Enemies, EnemyPartIndices);
            }

            internal override void GetIndices(MSB_NR msb, Entries entries)
            {
                base.GetIndices(msb, entries);
                EnemyPartIndices = MSB.FindIndices(this, msb.Parts.Enemies, EnemyPartNames);
            }

            // Layout. The talk setup (nightreign.exe 140FE6C80, event type 26) loops over the entries: entry i uses
            // TalkEntries[i], the character EnemyPartNames[i + 1] and the byte Unk44 + i.
            private int[] EnemyPartIndices { get; set; } = new int[4]; // was public; the names are written back over it
            private int Unk10 { get; set; } = -1; // Hidden: not read, -1 in retail
            private int Unk14 { get; set; } = -1; // Hidden: not read, -1 in retail
            private int Unk18 { get; set; } = -1; // Hidden: not read, -1 in retail
            private int Unk1C { get; set; } = -1; // Hidden: not read, -1 in retail
            private int Unk20 { get; set; } = -1; // Hidden: not read, -1 in retail

            /// <summary>
            /// Talk ESD IDs; entry i is spoken by EnemyPartNames[i + 1]. 0 or less for none.
            /// </summary>
            public int[] TalkEntries { get; set; } = new int[3];
            private int Unk30 { get; set; } = 0; // Hidden: not read, 0 in retail
            private int Unk34 { get; set; } = 0; // Hidden: not read, 0 in retail
            private int Unk38 { get; set; } = 0; // Hidden: not read, 0 in retail
            private int Unk3C { get; set; } = 0; // Hidden: not read, 0 in retail
            private int Unk40 { get; set; } = 0; // Hidden: not read, 0 in retail

            /// <summary>
            /// A per-entry value for TalkEntries[0], read with it. 1 on 18 of 24 retail events, else 0, 3 or 7.
            /// </summary>
            public byte Unk44 { get; set; } = 0;

            /// <summary>
            /// The same per-entry value for TalkEntries[1]. 0, 1 or 4 in retail.
            /// </summary>
            public byte Unk45 { get; set; } = 0;

            /// <summary>
            /// The same per-entry value for TalkEntries[2]. Was the low byte of the short Unk46. 0, or 3 once.
            /// </summary>
            public byte Unk46 { get; set; } = 0;
            private byte Unk47 { get; set; } = 0; // Hidden: not read, 0 in retail
            private int Unk48 { get; set; } = 0; // Hidden
            private int Unk4C { get; set; } = 0; // Hidden
            private int Unk50 { get; set; } = 0; // Hidden
            private int Unk54 { get; set; } = 0; // Hidden
            private int Unk58 { get; set; } = 0; // Hidden
            private int Unk5C { get; set; } = 0; // Hidden
            private int Unk60 { get; set; } = 0; // Hidden
            private int Unk64 { get; set; } = 0; // Hidden
            private int Unk68 { get; set; } = 0; // Hidden
            private int Unk6C { get; set; } = 0; // Hidden
            private int Unk70 { get; set; } = 0; // Hidden
            private int Unk74 { get; set; } = 0; // Hidden
            private int Unk78 { get; set; } = 0; // Hidden
            private int Unk7C { get; set; } = 0; // Hidden

            // Names

            /// <summary>
            /// [0]: the main character (read by the talk setup), [1]-[3]: the character of TalkEntries[0]-[2].
            /// Was get-only.
            /// </summary>
            [MSBReference(ReferenceType = typeof(Part.EnemyBase))]
            public string[] EnemyPartNames { get; set; } = new string[4];
        }

        /// <summary>
        /// 
        /// </summary>
        public class TeamFight : Event
        {
            private protected override EventType Type => EventType.TeamFight;
            private protected override bool HasTypeData => true;

            private int Version = -1;

            public TeamFight() : base($"{nameof(Event)}: {nameof(TeamFight)}") { }

            internal TeamFight(BinaryReaderEx br, int version) : base(br, version) { }

            private protected override void ReadTypeData(BinaryReaderEx br, int version)
            {
                Version = version;

                TargetEntityGroupID = br.ReadInt32();
                ItemLot_Standard = br.ReadInt32();
                ItemLot_Cataclysm = br.ReadInt32();
                LeaderEntityID = br.ReadInt32();

                if (Version >= 78)
                {
                    ItemLot_DeepNight = br.ReadInt32();
                }
                
                if (Version >= 80)
                {
                    OverrideItemLotEventFlagID0 = br.ReadInt32();
                    OverrideItemLotParamID0 = br.ReadInt32();
                    OverrideItemLotEventFlagID1 = br.ReadInt32();
                    OverrideItemLotParamID1 = br.ReadInt32();
                }
            }

            private protected override void WriteTypeData(BinaryWriterEx bw)
            {
                // The layout follows the version of the param being written (a new event has no read version), so it
                // always matches what ReadTypeData expects for the file. Outside a param write: the read version, else
                // the new EventParam default (78).
                int version = ParamVersion >= 0 ? ParamVersion : Version >= 0 ? Version : 78;

                bw.WriteInt32(TargetEntityGroupID);
                bw.WriteInt32(ItemLot_Standard);
                bw.WriteInt32(ItemLot_Cataclysm);
                bw.WriteInt32(LeaderEntityID);

                if (version >= 78)
                {
                    bw.WriteInt32(ItemLot_DeepNight);
                }

                if (version >= 80)
                {
                    bw.WriteInt32(OverrideItemLotEventFlagID0);
                    bw.WriteInt32(OverrideItemLotParamID0);
                    bw.WriteInt32(OverrideItemLotEventFlagID1);
                    bw.WriteInt32(OverrideItemLotParamID1);
                }
            }

            // Layout

            public int TargetEntityGroupID { get; set; }
            public int ItemLot_Standard { get; set; }
            public int ItemLot_Cataclysm { get; set; } = -1;
            public int LeaderEntityID { get; set; }
            public int ItemLot_DeepNight { get; set; } = -1;

            // Event param version >= 80. nightreign.exe CS::CSMsbEventBattleGroup::CheckGetOverridePrivateItemLotId: the first
            // pair whose event flag is on gives the item lot instead of the normal one. MSB Research.md §5.5.
            public int OverrideItemLotEventFlagID0 { get; set; } = 0;
            public int OverrideItemLotParamID0 { get; set; } = -1;
            public int OverrideItemLotEventFlagID1 { get; set; } = 0;
            public int OverrideItemLotParamID1 { get; set; } = -1;
        }

        /// <summary>
        /// 
        /// </summary>
        public class Other : Event
        {
            private protected override EventType Type => EventType.Other;
            private protected override bool HasTypeData => false;

            public Other() : base($"{nameof(Event)}: {nameof(Other)}") { }

            internal Other(BinaryReaderEx br, int version) : base(br, version) { }
        }
    }
}
