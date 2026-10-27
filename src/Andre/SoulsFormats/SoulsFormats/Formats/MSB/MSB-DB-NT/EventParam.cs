using System;
using System.Collections.Generic;
using System.IO;

namespace SoulsFormats;

public partial class MSB_DB_NT
{
    internal enum EventType : uint
    {
        Treasure = 4,
        Generator = 5,
        ObjAct = 7,
        MapOffset = 9,
        PatrolInfo = 14,
        PlatoonInfo = 15,
        PatrolRoute = 20,
        SignPuddle = 23,
        BirdRoute = 25,
        TeamFight = 27,
        Unknown28 = 28,
        Unknown30 = 30,
        Unknown31 = 31,
        Unknown32 = 32,
        Unknown33 = 33,
        Unknown34 = 34,
        Unknown35 = 35,
        Unknown36 = 36,
        Unknown37 = 37,
        Other = 0xFFFFFFFF
    }

    public class EventParam : Param<Event>, IMsbParam<IMsbEvent>
    {
        public List<Event.Treasure> Treasures { get; set; }

        public List<Event.Generator> Generators { get; set; }

        public List<Event.ObjAct> ObjActs { get; set; }

        public List<Event.MapOffset> MapOffsets { get; set; }

        public List<Event.PatrolInfo> PatrolInfos { get; set; }

        public List<Event.PlatoonInfo> PlatoonInfos { get; set; }

        public List<Event.PatrolRoute> PatrolRoutes { get; set; }

        public List<Event.SignPuddle> SignPuddles { get; set; }

        public List<Event.BirdRoute> BirdRoutes { get; set; }

        public List<Event.TeamFight> TeamFights { get; set; }

        public List<Event.Unknown28> Unknown28s { get; set; }

        public List<Event.Unknown30> Unknown30s { get; set; }

        public List<Event.Unknown31> Unknown31s { get; set; }

        public List<Event.Unknown32> Unknown32s { get; set; }

        public List<Event.Unknown33> Unknown33s { get; set; }

        public List<Event.Unknown34> Unknown34s { get; set; }

        public List<Event.Unknown35> Unknown35s { get; set; }

        public List<Event.Unknown36> Unknown36s { get; set; }

        public List<Event.Unknown37> Unknown37s { get; set; }

        public List<Event.Other> Others { get; set; }

        public EventParam() : base(66, "EVENT_PARAM_ST")
        {
            Treasures = new List<Event.Treasure>();
            Generators = new List<Event.Generator>();
            ObjActs = new List<Event.ObjAct>();
            MapOffsets = new List<Event.MapOffset>();
            PatrolInfos = new List<Event.PatrolInfo>();
            PlatoonInfos = new List<Event.PlatoonInfo>();
            PatrolRoutes = new List<Event.PatrolRoute>();
            SignPuddles = new List<Event.SignPuddle>();
            BirdRoutes = new List<Event.BirdRoute>();
            TeamFights = new List<Event.TeamFight>();
            Unknown28s = new List<Event.Unknown28>();
            Unknown30s = new List<Event.Unknown30>();
            Unknown31s = new List<Event.Unknown31>();
            Unknown32s = new List<Event.Unknown32>();
            Unknown33s = new List<Event.Unknown33>();
            Unknown34s = new List<Event.Unknown34>();
            Unknown35s = new List<Event.Unknown35>();
            Unknown36s = new List<Event.Unknown36>();
            Unknown37s = new List<Event.Unknown37>();
            Others = new List<Event.Other>();
        }
        public Event Add(Event evnt)
        {
            switch (evnt)
            {
                case Event.Treasure e: Treasures.Add(e); break;
                case Event.Generator e: Generators.Add(e); break;
                case Event.ObjAct e: ObjActs.Add(e); break;
                case Event.MapOffset e: MapOffsets.Add(e); break;
                case Event.PatrolInfo e: PatrolInfos.Add(e); break;
                case Event.PlatoonInfo e: PlatoonInfos.Add(e); break;
                case Event.PatrolRoute e: PatrolRoutes.Add(e); break;
                case Event.SignPuddle e: SignPuddles.Add(e); break;
                case Event.BirdRoute e: BirdRoutes.Add(e); break;
                case Event.TeamFight e: TeamFights.Add(e); break;
                case Event.Unknown28 e: Unknown28s.Add(e); break;
                case Event.Unknown30 e: Unknown30s.Add(e); break;
                case Event.Unknown31 e: Unknown31s.Add(e); break;
                case Event.Unknown32 e: Unknown32s.Add(e); break;
                case Event.Unknown33 e: Unknown33s.Add(e); break;
                case Event.Unknown34 e: Unknown34s.Add(e); break;
                case Event.Unknown35 e: Unknown35s.Add(e); break;
                case Event.Unknown36 e: Unknown36s.Add(e); break;
                case Event.Unknown37 e: Unknown37s.Add(e); break;
                case Event.Other e: Others.Add(e); break;

                default:
                    throw new ArgumentException($"Unrecognized type {evnt.GetType()}.", nameof(evnt));
            }
            return evnt;
        }
        IMsbEvent IMsbParam<IMsbEvent>.Add(IMsbEvent item) => Add((Event)item);

        public override List<Event> GetEntries()
        {
            return SFUtil.ConcatAll<Event>(
                Treasures, 
                Generators, 
                ObjActs,
                MapOffsets, 
                PatrolInfos,
                PlatoonInfos, 
                PatrolRoutes, 
                SignPuddles, 
                BirdRoutes,
                TeamFights,
                Unknown28s,
                Unknown30s,
                Unknown31s,
                Unknown32s,
                Unknown33s,
                Unknown34s,
                Unknown35s,
                Unknown36s,
                Unknown37s,
                Others);
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

                case EventType.PatrolInfo:
                    return PatrolInfos.EchoAdd(new Event.PatrolInfo(br, version));

                case EventType.PlatoonInfo:
                    return PlatoonInfos.EchoAdd(new Event.PlatoonInfo(br, version));

                case EventType.PatrolRoute:
                    return PatrolRoutes.EchoAdd(new Event.PatrolRoute(br, version));

                case EventType.SignPuddle:
                    return SignPuddles.EchoAdd(new Event.SignPuddle(br, version));

                case EventType.BirdRoute:
                    return BirdRoutes.EchoAdd(new Event.BirdRoute(br, version));

                case EventType.TeamFight:
                    return TeamFights.EchoAdd(new Event.TeamFight(br, version));

                case EventType.Unknown28:
                    return Unknown28s.EchoAdd(new Event.Unknown28(br, version));

                case EventType.Unknown30:
                    return Unknown30s.EchoAdd(new Event.Unknown30(br, version));

                case EventType.Unknown31:
                    return Unknown31s.EchoAdd(new Event.Unknown31(br, version));

                case EventType.Unknown32:
                    return Unknown32s.EchoAdd(new Event.Unknown32(br, version));

                case EventType.Unknown33:
                    return Unknown33s.EchoAdd(new Event.Unknown33(br, version));

                case EventType.Unknown34:
                    return Unknown34s.EchoAdd(new Event.Unknown34(br, version));

                case EventType.Unknown35:
                    return Unknown35s.EchoAdd(new Event.Unknown35(br, version));

                case EventType.Unknown36:
                    return Unknown36s.EchoAdd(new Event.Unknown36(br, version));

                case EventType.Unknown37:
                    return Unknown37s.EchoAdd(new Event.Unknown37(br, version));

                case EventType.Other:
                    return Others.EchoAdd(new Event.Other(br, version));

                default:
                    throw new NotImplementedException($"Unimplemented event type: {type}");
            }
        }
    }

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

        internal virtual void GetNames(MSB_DB_NT msb, Entries entries)
        {
            PartName = MSB.FindName(entries.Parts, PartIndex);
            RegionName = MSB.FindName(entries.Regions, RegionIndex);
        }

        internal virtual void GetIndices(MSB_DB_NT msb, Entries entries)
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

            if (!BinaryReaderEx.IgnoreAsserts)
            {
                if (NameOffset == 0)
                    throw new InvalidDataException($"{nameof(NameOffset)} must not be 0 in type {GetType()}.");

                if (CommonOffset == 0)
                    throw new InvalidDataException($"{nameof(CommonOffset)} must not be 0 in type {GetType()}.");

                if (HasTypeData ^ TypeOffset != 0)
                    throw new InvalidDataException($"Unexpected {nameof(TypeOffset)} 0x{TypeOffset:X} in type {GetType()}.");
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

            // Check if needed
            //bw.Pad(8);
        }

        // Layout
        public int EventID { get; set; } = -1;
        internal int TypeIndex { get; set; }
        public int Unk14_Base { get; set; } = 0; // Hidden

        // Offsets
        internal long NameOffset { get; set; }
        internal long CommonOffset { get; set; }
        internal long TypeOffset { get; set; }
        internal long Unk28Offset { get; set; }

        // Common - Not separated into discrete class for simplicity
        private int PartIndex { get; set; } = -1;
        private int RegionIndex { get; set; } = -1;
        public uint EntityID { get; set; } = 0;
        public sbyte Unk0C_Common { get; set; } = -1;
        public byte Unk0D_Common { get; set; } = 0; // Hidden
        public short Unk0E_Common { get; set; } = 0; // Hidden

        // Names
        [MSBReference(ReferenceType = typeof(Part))]
        public string PartName { get; set; }

        [MSBReference(ReferenceType = typeof(Region))]
        public string RegionName { get; set; }

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

            // Layout
            public int Unk00 { get; set; } = 0; // Hidden
            public int Unk04 { get; set; } = 0; // Hidden
            private int TreasurePartIndex { get; set; } = -1;
            public int Unk0C { get; set; } = 0; // Hidden
            public int ItemLotID { get; set; } = -1;
            public int Unk14 { get; set; } = 0; // Hidden
            public int Unk18 { get; set; } = 0; // Hidden
            public int Unk1C { get; set; } = 0; // Hidden
            public int Unk20 { get; set; } = 0; // Hidden
            public int Unk24 { get; set; } = 0; // Hidden
            public int Unk28 { get; set; } = 0; // Hidden
            public int Unk2C { get; set; } = 0; // Hidden
            public int Unk30 { get; set; } = 0; // Hidden
            public int Unk34 { get; set; } = 0; // Hidden
            public int UncommonItemLotID { get; set; } = -1;
            public int PickupAnimID { get; set; } = -1; // Hidden
            public byte InChest { get; set; } = 1;
            public byte StartDisabled { get; set; } = 0; // Boolean
            public byte Unk42 { get; set; } = 0; // Boolean
            public byte Unk43 { get; set; } = 1; // Boolean
            public int Unk44 { get; set; } = -1;
            public short Unk48 { get; set; } = -1;
            public short Unk4A { get; set; } = 0;
            public int Unk4C { get; set; } = 0; // Hidden

            // Names
            [MSBReference(ReferenceType = typeof(Part))]
            public string TreasurePartName { get; set; }
        }

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
            public byte Unk11 { get; set; } = 0; // Hidden
            public short Unk12 { get; set; } = 0; // Hidden
            public float Unk14 { get; set; } = 0;
            public float Unk18 { get; set; } = 0;
            public int Unk1C { get; set; } = 0; // Hidden
            public int Unk20 { get; set; } = 0; // Hidden
            public int Unk24 { get; set; } = 0; // Hidden
            public int Unk28 { get; set; } = 0; // Hidden
            public int Unk2C { get; set; } = 0; // Hidden
            private int[] SpawnRegionIndices { get; set; } = new int[8];
            public int Unk50 { get; set; } = 0; // Hidden
            public int Unk54 { get; set; } = 0; // Hidden
            public int Unk58 { get; set; } = 0; // Hidden
            public int Unk5C { get; set; } = 0; // Hidden
            private int[] SpawnPartIndices { get; set; } = new int[32];
            public int UnkE0 { get; set; } = 0; // Hidden
            public int UnkE4 { get; set; } = 0; // Hidden
            public int UnkE8 { get; set; } = 0; // Hidden
            public int UnkEC { get; set; } = 0; // Hidden
            public int UnkF0 { get; set; } = 0; // Hidden
            public int UnkF4 { get; set; } = 0; // Hidden
            public int UnkF8 { get; set; } = 0; // Hidden
            public int UnkFC { get; set; } = 0; // Hidden

            // Names
            [MSBReference(ReferenceType = typeof(Region))]
            public string[] SpawnRegionNames { get; set; } = new string[8];

            [MSBReference(ReferenceType = typeof(Part))]
            public string[] SpawnPartNames { get; set; } = new string[32];
        }

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
            public int Unk14 { get; set; } = -1;
            private int Unk18 { get; set; } = 0; // Hidden
            private int Unk1C { get; set; } = 0; // Hidden

            // Names

            [MSBReference(ReferenceType = typeof(Part))]
            public string ObjActPartName { get; set; }

        }

        public class MapOffset : Event
        {
            private protected override EventType Type => EventType.MapOffset;
            private protected override bool HasTypeData => true;

            public MapOffset() : base($"{nameof(Event)}: {nameof(MapOffset)}") { }

            internal MapOffset(BinaryReaderEx br, int version) : base(br, version) { }
            private protected override void ReadTypeData(BinaryReaderEx br, int version)
            {
                // TODO: check
            }

            private protected override void WriteTypeData(BinaryWriterEx bw)
            {
            }

            // Layout
        }

        public class PatrolInfo : Event
        {
            private protected override EventType Type => EventType.PatrolInfo;
            private protected override bool HasTypeData => true;

            public byte PatrolType { get; set; }

            [MSBReference(ReferenceType = typeof(Region))]
            public string[] WalkRegionNames { get; set; }

            private short[] WalkRegionIndices { get; set; }

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

            internal PatrolInfo(BinaryReaderEx br, int version) : base(br, version) { }

            private protected override void ReadTypeData(BinaryReaderEx br, int version)
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

            internal override void GetNames(MSB_DB_NT msb, Entries entries)
            {
                base.GetNames(msb, entries);
                WalkRegionNames = new string[WalkRegionIndices.Length];
                for (int i = 0; i < WalkRegionIndices.Length; i++)
                    WalkRegionNames[i] = MSB.FindName(entries.Regions, WalkRegionIndices[i]);
            }

            internal override void GetIndices(MSB_DB_NT msb, Entries entries)
            {
                base.GetIndices(msb, entries);
                WalkRegionIndices = new short[WalkRegionNames.Length];
                for (int i = 0; i < WalkRegionNames.Length; i++)
                    WalkRegionIndices[i] = (short)MSB.FindIndex(this, entries.Regions, WalkRegionNames[i]);
            }
        }

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
            }
            internal override void GetNames(MSB_DB_NT msb, Entries entries)
            {
                base.GetNames(msb, entries);
                GroupPartsNames = MSB.FindNames(entries.Parts, GroupPartsIndices);
            }
            internal override void GetIndices(MSB_DB_NT msb, Entries entries)
            {
                base.GetIndices(msb, entries);
                GroupPartsIndices = MSB.FindIndices(this, entries.Parts, GroupPartsNames);
            }
            private protected override void ReadTypeData(BinaryReaderEx br, int version)
            {
                PlatoonScriptId = br.ReadInt32();
                Unk04 = br.ReadByte();
                Unk05 = br.ReadByte();
                Unk06 = br.ReadInt16();
                Unk08 = br.ReadInt32();
                Unk0C = br.ReadInt32();
                GroupPartsIndices = br.ReadInt32s(32);
            }

            private protected override void WriteTypeData(BinaryWriterEx bw)
            {
                bw.WriteInt32(PlatoonScriptId);
                bw.WriteByte(Unk04);
                bw.WriteByte(Unk05);
                bw.WriteInt16(Unk06);
                bw.WriteInt32(Unk08);
                bw.WriteInt32(Unk0C);
                bw.WriteInt32s(GroupPartsIndices);
            }

            // Layout
            public int PlatoonScriptId { get; set; } = -1;
            public byte Unk04 { get; set; } = 0; // Boolean
            public byte Unk05 { get; set; } = 0; // Hidden
            public short Unk06 { get; set; } = 0; // Hidden
            public int Unk08 { get; set; } = 0; // Hidden
            public int Unk0C { get; set; } = 0; // Hidden
            private int[] GroupPartsIndices { get; set; } = new int[32];

            // Names
            [MSBReference(ReferenceType = typeof(Part))]
            public string[] GroupPartsNames { get; set; } = new string[32];

        }

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

            internal override void GetNames(MSB_DB_NT msb, Entries entries)
            {
                base.GetNames(msb, entries);
                RegionNames = MSB.FindNames(entries.Regions, RegionIndices);
            }

            internal override void GetIndices(MSB_DB_NT msb, Entries entries)
            {
                base.GetIndices(msb, entries);
                RegionIndices = MSB.FindShortIndices(entries.Regions, RegionNames);
            }

            private protected override void ReadTypeData(BinaryReaderEx br, int version)
            {
                PatrolType = br.ReadByte();
                Unk01 = br.ReadByte();
                Unk02 = br.ReadByte();
                Unk03 = br.ReadByte();
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
                bw.WriteByte(Unk03);
                bw.WriteInt32(Unk04);
                bw.WriteInt32(Unk08);
                bw.WriteInt32(Unk0C);
                bw.WriteInt16s(RegionIndices);
            }

            // Layout
            public byte PatrolType { get; set; } = 0;
            public byte Unk01 { get; set; } = 0; // Hidden
            public byte Unk02 { get; set; } = 0; // Hidden
            public byte Unk03 { get; set; } = 1; // Hidden
            public int Unk04 { get; set; } = -1; // Hidden
            public int Unk08 { get; set; } = 0; // Hidden
            public int Unk0C { get; set; } = 0; // Hidden

            private short[] RegionIndices { get; set; } = new short[64];

            // Names
            [MSBReference(ReferenceType = typeof(Region))]
            public string[] RegionNames { get; set; } = new string[64];
        }

        public class SignPuddle : Event
        {
            private protected override EventType Type => EventType.SignPuddle;
            private protected override bool HasTypeData => true;

            public SignPuddle() : base($"{nameof(Event)}: {nameof(SignPuddle)}") { }
            internal SignPuddle(BinaryReaderEx br, int version) : base(br, version) { }

            private protected override void ReadTypeData(BinaryReaderEx br, int version)
            {
                Unk00 = br.ReadInt32();
                Unk04 = br.ReadInt32();
                Unk08 = br.ReadInt32();
                Unk0C = br.ReadInt32();

                Unk10 = br.ReadInt32();
                Unk14 = br.ReadInt32();
                Unk18 = br.ReadInt32();
                Unk1C = br.ReadInt32();

                Unk20 = br.ReadInt32();
                Unk24 = br.ReadInt32();
                Unk28 = br.ReadInt32();
                Unk2C = br.ReadInt32();

                Unk30 = br.ReadInt32();
                Unk34 = br.ReadInt32();
                Unk38 = br.ReadInt32();
                Unk3C = br.ReadInt32();

                Unk40 = br.ReadInt32();
                Unk44 = br.ReadInt32();
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
                bw.WriteInt32(Unk00);
                bw.WriteInt32(Unk04);
                bw.WriteInt32(Unk08);
                bw.WriteInt32(Unk0C);

                bw.WriteInt32(Unk10);
                bw.WriteInt32(Unk14);
                bw.WriteInt32(Unk18);
                bw.WriteInt32(Unk1C);

                bw.WriteInt32(Unk20);
                bw.WriteInt32(Unk24);
                bw.WriteInt32(Unk28);
                bw.WriteInt32(Unk2C);

                bw.WriteInt32(Unk30);
                bw.WriteInt32(Unk34);
                bw.WriteInt32(Unk38);
                bw.WriteInt32(Unk3C);

                bw.WriteInt32(Unk40);
                bw.WriteInt32(Unk44);
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

            // Layout
            public int Unk00 { get; set; } = 0;
            public int Unk04 { get; set; } = 0;
            public int Unk08 { get; set; } = 0;
            public int Unk0C { get; set; } = 0;

            public int Unk10 { get; set; } = 0;
            public int Unk14 { get; set; } = 0;
            public int Unk18 { get; set; } = 0;
            public int Unk1C { get; set; } = 0;

            public int Unk20 { get; set; } = 0;
            public int Unk24 { get; set; } = 0;
            public int Unk28 { get; set; } = 0;
            public int Unk2C { get; set; } = 0;

            public int Unk30 { get; set; } = 0;
            public int Unk34 { get; set; } = 0;
            public int Unk38 { get; set; } = 0;
            public int Unk3C { get; set; } = 0;

            public int Unk40 { get; set; } = 0;
            public int Unk44 { get; set; } = 0;
            public int Unk48 { get; set; } = 0;
            public int Unk4C { get; set; } = 0;

            public int Unk50 { get; set; } = 0;
            public int Unk54 { get; set; } = 0;
            public int Unk58 { get; set; } = 0;
            public int Unk5C { get; set; } = 0;

            public int Unk60 { get; set; } = 0;
            public int Unk64 { get; set; } = 0;
            public int Unk68 { get; set; } = 0;
            public int Unk6C { get; set; } = 0;

            public int Unk70 { get; set; } = 0;
            public int Unk74 { get; set; } = 0;
            public int Unk78 { get; set; } = 0;
            public int Unk7C { get; set; } = 0;
        }

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

            internal override void GetNames(MSB_DB_NT msb, Entries entries)
            {
                base.GetNames(msb, entries);
                RegionNames = MSB.FindNames(entries.Regions, RegionIndices);
            }

            internal override void GetIndices(MSB_DB_NT msb, Entries entries)
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

                // Padding 184?
                //br.AssertPattern(184, 0x00);
            }

            private protected override void WriteTypeData(BinaryWriterEx bw)
            {
                bw.WriteInt32(Unk00);
                bw.WriteInt32(Unk04);
                bw.WriteInt32(Unk08);
                bw.WriteInt32(Unk0C);
                bw.WriteInt16s(RegionIndices);

                // Padding 184?
                //bw.WritePattern(184, 0x00);
            }

            // Layout
            public int Unk00 { get; set; } = 0; // Hidden
            public int Unk04 { get; set; } = 0;
            public int Unk08 { get; set; } = 0;
            public int Unk0C { get; set; } = 0;
            public short[] RegionIndices { get; set; } = new short[32];

            // Names
            [MSBReference(ReferenceType = typeof(Region))]
            public string[] RegionNames { get; set; } = new string[32];

        }

        public class TeamFight : Event
        {
            private protected override EventType Type => EventType.TeamFight;
            private protected override bool HasTypeData => true;

            public TeamFight() : base($"{nameof(Event)}: {nameof(TeamFight)}") { }

            internal TeamFight(BinaryReaderEx br, int version) : base(br, version) { }

            private protected override void ReadTypeData(BinaryReaderEx br, int version)
            {
                Unk00 = br.ReadInt32();
                Unk04 = br.ReadInt32();
                Unk08 = br.ReadInt32();
                Unk0C = br.ReadInt32();

                Unk10 = br.ReadInt32();
                Unk14 = br.ReadInt32();
                Unk18 = br.ReadInt32();
                Unk1C = br.ReadInt32();

                Unk20 = br.ReadInt32();
                Unk24 = br.ReadInt32();
                Unk28 = br.ReadInt32();
                Unk2C = br.ReadInt32();

                Unk30 = br.ReadInt32();
                Unk34 = br.ReadInt32();
                Unk38 = br.ReadInt32();
                Unk3C = br.ReadInt32();

                Unk40 = br.ReadInt32();
            }

            private protected override void WriteTypeData(BinaryWriterEx bw)
            {
                bw.WriteInt32(Unk00);
                bw.WriteInt32(Unk04);
                bw.WriteInt32(Unk08);
                bw.WriteInt32(Unk0C);

                bw.WriteInt32(Unk10);
                bw.WriteInt32(Unk14);
                bw.WriteInt32(Unk18);
                bw.WriteInt32(Unk1C);

                bw.WriteInt32(Unk20);
                bw.WriteInt32(Unk24);
                bw.WriteInt32(Unk28);
                bw.WriteInt32(Unk2C);

                bw.WriteInt32(Unk30);
                bw.WriteInt32(Unk34);
                bw.WriteInt32(Unk38);
                bw.WriteInt32(Unk3C);

                bw.WriteInt32(Unk40);
            }

            // Layout
            public int Unk00 { get; set; } = 0;
            public int Unk04 { get; set; } = 0;
            public int Unk08 { get; set; } = 0;
            public int Unk0C { get; set; } = 0;

            public int Unk10 { get; set; } = 0;
            public int Unk14 { get; set; } = 0;
            public int Unk18 { get; set; } = 0;
            public int Unk1C { get; set; } = 0;

            public int Unk20 { get; set; } = 0;
            public int Unk24 { get; set; } = 0;
            public int Unk28 { get; set; } = 0;
            public int Unk2C { get; set; } = 0;

            public int Unk30 { get; set; } = 0;
            public int Unk34 { get; set; } = 0;
            public int Unk38 { get; set; } = 0;
            public int Unk3C { get; set; } = 0;

            public int Unk40 { get; set; } = 0;
        }

        public class Unknown28 : Event
        {
            private protected override EventType Type => EventType.Unknown28;
            private protected override bool HasTypeData => true;

            public Unknown28() : base($"{nameof(Event)}: {nameof(Unknown28)}") { }

            internal Unknown28(BinaryReaderEx br, int version) : base(br, version) { }

            private protected override void ReadTypeData(BinaryReaderEx br, int version)
            {
                Unk00 = br.ReadInt32();
                Unk04 = br.ReadInt32();

                // Padding 128?
                //br.AssertPattern(128, 0x00);
            }

            private protected override void WriteTypeData(BinaryWriterEx bw)
            {
                bw.WriteInt32(Unk00);
                bw.WriteInt32(Unk04);

                // Padding 128?
                //bw.WritePattern(128, 0x00);
            }

            // Layout
            public int Unk00 { get; set; } = 0;
            public int Unk04 { get; set; } = 0;
        }

        public class Unknown30 : Event
        {
            private protected override EventType Type => EventType.Unknown30;
            private protected override bool HasTypeData => true;

            public Unknown30() : base($"{nameof(Event)}: {nameof(TeamFight)}") { }

            internal Unknown30(BinaryReaderEx br, int version) : base(br, version) { }

            private protected override void ReadTypeData(BinaryReaderEx br, int version)
            {
                Unk00 = br.ReadInt32();
                Unk04 = br.ReadInt32();
                Unk08 = br.ReadInt32();
                Unk0C = br.ReadInt32();

                Unk10 = br.ReadInt32();
                Unk14 = br.ReadInt32();
                Unk18 = br.ReadInt32();
                Unk1C = br.ReadInt32();

                Unk20 = br.ReadInt32();
                Unk24 = br.ReadInt32();
                Unk28 = br.ReadInt32();
                Unk2C = br.ReadInt32();

                Unk30 = br.ReadInt32();
                Unk34 = br.ReadInt32();
                Unk38 = br.ReadInt32();
                Unk3C = br.ReadInt32();
            }

            private protected override void WriteTypeData(BinaryWriterEx bw)
            {
                bw.WriteInt32(Unk00);
                bw.WriteInt32(Unk04);
                bw.WriteInt32(Unk08);
                bw.WriteInt32(Unk0C);

                bw.WriteInt32(Unk10);
                bw.WriteInt32(Unk14);
                bw.WriteInt32(Unk18);
                bw.WriteInt32(Unk1C);

                bw.WriteInt32(Unk20);
                bw.WriteInt32(Unk24);
                bw.WriteInt32(Unk28);
                bw.WriteInt32(Unk2C);

                bw.WriteInt32(Unk30);
                bw.WriteInt32(Unk34);
                bw.WriteInt32(Unk38);
                bw.WriteInt32(Unk3C);
            }

            // Layout
            public int Unk00 { get; set; } = 0;
            public int Unk04 { get; set; } = 0;
            public int Unk08 { get; set; } = 0;
            public int Unk0C { get; set; } = 0;

            public int Unk10 { get; set; } = 0;
            public int Unk14 { get; set; } = 0;
            public int Unk18 { get; set; } = 0;
            public int Unk1C { get; set; } = 0;

            public int Unk20 { get; set; } = 0;
            public int Unk24 { get; set; } = 0;
            public int Unk28 { get; set; } = 0;
            public int Unk2C { get; set; } = 0;

            public int Unk30 { get; set; } = 0;
            public int Unk34 { get; set; } = 0;
            public int Unk38 { get; set; } = 0;
            public int Unk3C { get; set; } = 0;
        }

        public class Unknown31 : Event
        {
            private protected override EventType Type => EventType.Unknown31;
            private protected override bool HasTypeData => true;

            public Unknown31() : base($"{nameof(Event)}: {nameof(TeamFight)}") { }

            internal Unknown31(BinaryReaderEx br, int version) : base(br, version) { }

            private protected override void ReadTypeData(BinaryReaderEx br, int version)
            {
                Unk00 = br.ReadInt32();
                Unk04 = br.ReadInt32();
                Unk08 = br.ReadInt32();
                Unk0C = br.ReadInt32();

                Unk10 = br.ReadInt32();
                Unk14 = br.ReadInt32();
                Unk18 = br.ReadInt32();
                Unk1C = br.ReadInt32();

                Unk20 = br.ReadInt32();
                Unk24 = br.ReadInt32();
                Unk28 = br.ReadInt32();
                Unk2C = br.ReadInt32();

                Unk30 = br.ReadInt32();
                Unk34 = br.ReadInt32();
                Unk38 = br.ReadInt32();
                Unk3C = br.ReadInt32();
            }

            private protected override void WriteTypeData(BinaryWriterEx bw)
            {
                bw.WriteInt32(Unk00);
                bw.WriteInt32(Unk04);
                bw.WriteInt32(Unk08);
                bw.WriteInt32(Unk0C);

                bw.WriteInt32(Unk10);
                bw.WriteInt32(Unk14);
                bw.WriteInt32(Unk18);
                bw.WriteInt32(Unk1C);

                bw.WriteInt32(Unk20);
                bw.WriteInt32(Unk24);
                bw.WriteInt32(Unk28);
                bw.WriteInt32(Unk2C);

                bw.WriteInt32(Unk30);
                bw.WriteInt32(Unk34);
                bw.WriteInt32(Unk38);
                bw.WriteInt32(Unk3C);
            }

            // Layout
            public int Unk00 { get; set; } = 0;
            public int Unk04 { get; set; } = 0;
            public int Unk08 { get; set; } = 0;
            public int Unk0C { get; set; } = 0;

            public int Unk10 { get; set; } = 0;
            public int Unk14 { get; set; } = 0;
            public int Unk18 { get; set; } = 0;
            public int Unk1C { get; set; } = 0;

            public int Unk20 { get; set; } = 0;
            public int Unk24 { get; set; } = 0;
            public int Unk28 { get; set; } = 0;
            public int Unk2C { get; set; } = 0;

            public int Unk30 { get; set; } = 0;
            public int Unk34 { get; set; } = 0;
            public int Unk38 { get; set; } = 0;
            public int Unk3C { get; set; } = 0;
        }

        public class Unknown32 : Event
        {
            private protected override EventType Type => EventType.Unknown32;
            private protected override bool HasTypeData => true;

            public Unknown32() : base($"{nameof(Event)}: {nameof(TeamFight)}") { }

            internal Unknown32(BinaryReaderEx br, int version) : base(br, version) { }

            private protected override void ReadTypeData(BinaryReaderEx br, int version)
            {
                Unk00 = br.ReadInt32();
                Unk04 = br.ReadInt32();
                Unk08 = br.ReadInt32();
                Unk0C = br.ReadInt32();

                Unk10 = br.ReadInt32();
                Unk14 = br.ReadInt32();
                Unk18 = br.ReadInt32();
                Unk1C = br.ReadInt32();

                Unk20 = br.ReadInt32();
                Unk24 = br.ReadInt32();
                Unk28 = br.ReadInt32();
                Unk2C = br.ReadInt32();

                Unk30 = br.ReadInt32();
                Unk34 = br.ReadInt32();
                Unk38 = br.ReadInt32();
                Unk3C = br.ReadInt32();
            }

            private protected override void WriteTypeData(BinaryWriterEx bw)
            {
                bw.WriteInt32(Unk00);
                bw.WriteInt32(Unk04);
                bw.WriteInt32(Unk08);
                bw.WriteInt32(Unk0C);

                bw.WriteInt32(Unk10);
                bw.WriteInt32(Unk14);
                bw.WriteInt32(Unk18);
                bw.WriteInt32(Unk1C);

                bw.WriteInt32(Unk20);
                bw.WriteInt32(Unk24);
                bw.WriteInt32(Unk28);
                bw.WriteInt32(Unk2C);

                bw.WriteInt32(Unk30);
                bw.WriteInt32(Unk34);
                bw.WriteInt32(Unk38);
                bw.WriteInt32(Unk3C);
            }

            // Layout
            public int Unk00 { get; set; } = 0;
            public int Unk04 { get; set; } = 0;
            public int Unk08 { get; set; } = 0;
            public int Unk0C { get; set; } = 0;

            public int Unk10 { get; set; } = 0;
            public int Unk14 { get; set; } = 0;
            public int Unk18 { get; set; } = 0;
            public int Unk1C { get; set; } = 0;

            public int Unk20 { get; set; } = 0;
            public int Unk24 { get; set; } = 0;
            public int Unk28 { get; set; } = 0;
            public int Unk2C { get; set; } = 0;

            public int Unk30 { get; set; } = 0;
            public int Unk34 { get; set; } = 0;
            public int Unk38 { get; set; } = 0;
            public int Unk3C { get; set; } = 0;
        }

        public class Unknown33 : Event
        {
            private protected override EventType Type => EventType.Unknown33;
            private protected override bool HasTypeData => true;

            public Unknown33() : base($"{nameof(Event)}: {nameof(TeamFight)}") { }

            internal Unknown33(BinaryReaderEx br, int version) : base(br, version) { }

            private protected override void ReadTypeData(BinaryReaderEx br, int version)
            {
                Unk00 = br.ReadInt32();
                Unk04 = br.ReadInt32();
                Unk08 = br.ReadInt32();
                Unk0C = br.ReadInt32();

                Unk10 = br.ReadInt32();
                Unk14 = br.ReadInt32();
                Unk18 = br.ReadInt32();
                Unk1C = br.ReadInt32();

                Unk20 = br.ReadInt32();
                Unk24 = br.ReadInt32();
                Unk28 = br.ReadInt32();
                Unk2C = br.ReadInt32();

                Unk30 = br.ReadInt32();
                Unk34 = br.ReadInt32();
                Unk38 = br.ReadInt32();
                Unk3C = br.ReadInt32();

                Unk40 = br.ReadInt32();
                Unk44 = br.ReadInt32();
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

                Unk80 = br.ReadInt32();
                Unk84 = br.ReadInt32();
                Unk88 = br.ReadInt32();
                Unk8C = br.ReadInt32();

                Unk90 = br.ReadInt32();
                Unk94 = br.ReadInt32();
                Unk98 = br.ReadInt32();
                Unk9C = br.ReadInt32();

                Unk100 = br.ReadInt32();
                Unk104 = br.ReadInt32();
                Unk108 = br.ReadInt32();
                Unk10C = br.ReadInt32();

                Unk110 = br.ReadInt32();
                Unk114 = br.ReadInt32();
                Unk118 = br.ReadInt32();
                Unk11C = br.ReadInt32();

                Unk120 = br.ReadInt32();
                Unk124 = br.ReadInt32();
                Unk128 = br.ReadInt32();
                Unk12C = br.ReadInt32();

                Unk130 = br.ReadInt32();
                Unk134 = br.ReadInt32();
                Unk138 = br.ReadInt32();
                Unk13C = br.ReadInt32();

                Unk140 = br.ReadInt32();
                Unk144 = br.ReadInt32();
                Unk148 = br.ReadInt32();
                Unk14C = br.ReadInt32();

                Unk150 = br.ReadInt32();
                Unk154 = br.ReadInt32();
                Unk158 = br.ReadInt32();
                Unk15C = br.ReadInt32();

                Unk160 = br.ReadInt32();
                Unk164 = br.ReadInt32();
                Unk168 = br.ReadInt32();
                Unk16C = br.ReadInt32();
            }

            private protected override void WriteTypeData(BinaryWriterEx bw)
            {
                bw.WriteInt32(Unk00);
                bw.WriteInt32(Unk04);
                bw.WriteInt32(Unk08);
                bw.WriteInt32(Unk0C);

                bw.WriteInt32(Unk10);
                bw.WriteInt32(Unk14);
                bw.WriteInt32(Unk18);
                bw.WriteInt32(Unk1C);

                bw.WriteInt32(Unk20);
                bw.WriteInt32(Unk24);
                bw.WriteInt32(Unk28);
                bw.WriteInt32(Unk2C);

                bw.WriteInt32(Unk30);
                bw.WriteInt32(Unk34);
                bw.WriteInt32(Unk38);
                bw.WriteInt32(Unk3C);

                bw.WriteInt32(Unk40);
                bw.WriteInt32(Unk44);
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

                bw.WriteInt32(Unk80);
                bw.WriteInt32(Unk84);
                bw.WriteInt32(Unk88);
                bw.WriteInt32(Unk8C);

                bw.WriteInt32(Unk90);
                bw.WriteInt32(Unk94);
                bw.WriteInt32(Unk98);
                bw.WriteInt32(Unk9C);

                bw.WriteInt32(Unk100);
                bw.WriteInt32(Unk104);
                bw.WriteInt32(Unk108);
                bw.WriteInt32(Unk10C);

                bw.WriteInt32(Unk110);
                bw.WriteInt32(Unk114);
                bw.WriteInt32(Unk118);
                bw.WriteInt32(Unk11C);

                bw.WriteInt32(Unk120);
                bw.WriteInt32(Unk124);
                bw.WriteInt32(Unk128);
                bw.WriteInt32(Unk12C);

                bw.WriteInt32(Unk130);
                bw.WriteInt32(Unk134);
                bw.WriteInt32(Unk138);
                bw.WriteInt32(Unk13C);

                bw.WriteInt32(Unk140);
                bw.WriteInt32(Unk144);
                bw.WriteInt32(Unk148);
                bw.WriteInt32(Unk14C);

                bw.WriteInt32(Unk150);
                bw.WriteInt32(Unk154);
                bw.WriteInt32(Unk158);
                bw.WriteInt32(Unk15C);

                bw.WriteInt32(Unk160);
                bw.WriteInt32(Unk164);
                bw.WriteInt32(Unk168);
                bw.WriteInt32(Unk16C);
            }

            // Layout
            public int Unk00 { get; set; } = 0;
            public int Unk04 { get; set; } = 0;
            public int Unk08 { get; set; } = 0;
            public int Unk0C { get; set; } = 0;

            public int Unk10 { get; set; } = 0;
            public int Unk14 { get; set; } = 0;
            public int Unk18 { get; set; } = 0;
            public int Unk1C { get; set; } = 0;

            public int Unk20 { get; set; } = 0;
            public int Unk24 { get; set; } = 0;
            public int Unk28 { get; set; } = 0;
            public int Unk2C { get; set; } = 0;

            public int Unk30 { get; set; } = 0;
            public int Unk34 { get; set; } = 0;
            public int Unk38 { get; set; } = 0;
            public int Unk3C { get; set; } = 0;

            public int Unk40 { get; set; } = 0;
            public int Unk44 { get; set; } = 0;
            public int Unk48 { get; set; } = 0;
            public int Unk4C { get; set; } = 0;

            public int Unk50 { get; set; } = 0;
            public int Unk54 { get; set; } = 0;
            public int Unk58 { get; set; } = 0;
            public int Unk5C { get; set; } = 0;

            public int Unk60 { get; set; } = 0;
            public int Unk64 { get; set; } = 0;
            public int Unk68 { get; set; } = 0;
            public int Unk6C { get; set; } = 0;

            public int Unk70 { get; set; } = 0;
            public int Unk74 { get; set; } = 0;
            public int Unk78 { get; set; } = 0;
            public int Unk7C { get; set; } = 0;

            public int Unk80 { get; set; } = 0;
            public int Unk84 { get; set; } = 0;
            public int Unk88 { get; set; } = 0;
            public int Unk8C { get; set; } = 0;

            public int Unk90 { get; set; } = 0;
            public int Unk94 { get; set; } = 0;
            public int Unk98 { get; set; } = 0;
            public int Unk9C { get; set; } = 0;

            public int Unk100 { get; set; } = 0;
            public int Unk104 { get; set; } = 0;
            public int Unk108 { get; set; } = 0;
            public int Unk10C { get; set; } = 0;

            public int Unk110 { get; set; } = 0;
            public int Unk114 { get; set; } = 0;
            public int Unk118 { get; set; } = 0;
            public int Unk11C { get; set; } = 0;

            public int Unk120 { get; set; } = 0;
            public int Unk124 { get; set; } = 0;
            public int Unk128 { get; set; } = 0;
            public int Unk12C { get; set; } = 0;

            public int Unk130 { get; set; } = 0;
            public int Unk134 { get; set; } = 0;
            public int Unk138 { get; set; } = 0;
            public int Unk13C { get; set; } = 0;

            public int Unk140 { get; set; } = 0;
            public int Unk144 { get; set; } = 0;
            public int Unk148 { get; set; } = 0;
            public int Unk14C { get; set; } = 0;

            public int Unk150 { get; set; } = 0;
            public int Unk154 { get; set; } = 0;
            public int Unk158 { get; set; } = 0;
            public int Unk15C { get; set; } = 0;

            public int Unk160 { get; set; } = 0;
            public int Unk164 { get; set; } = 0;
            public int Unk168 { get; set; } = 0;
            public int Unk16C { get; set; } = 0;
        }

        public class Unknown34 : Event
        {
            private protected override EventType Type => EventType.Unknown34;
            private protected override bool HasTypeData => true;

            public Unknown34() : base($"{nameof(Event)}: {nameof(TeamFight)}") { }

            internal Unknown34(BinaryReaderEx br, int version) : base(br, version) { }

            private protected override void ReadTypeData(BinaryReaderEx br, int version)
            {
                Unk00 = br.ReadInt32();
                Unk04 = br.ReadInt32();
                Unk08 = br.ReadInt32();
                Unk0C = br.ReadInt32();

                Unk10 = br.ReadInt32();
                Unk14 = br.ReadInt32();
                Unk18 = br.ReadInt32();
                Unk1C = br.ReadInt32();

                Unk20 = br.ReadInt32();
                Unk24 = br.ReadInt32();
                Unk28 = br.ReadInt32();
                Unk2C = br.ReadInt32();

                Unk30 = br.ReadInt32();
                Unk34 = br.ReadInt32();
                Unk38 = br.ReadInt32();
                Unk3C = br.ReadInt32();

                Unk40 = br.ReadInt32();
                Unk44 = br.ReadInt32();
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

                Unk80 = br.ReadInt32();
                Unk84 = br.ReadInt32();
                Unk88 = br.ReadInt32();
                Unk8C = br.ReadInt32();

                Unk90 = br.ReadInt32();
                Unk94 = br.ReadInt32();
                Unk98 = br.ReadInt32();
                Unk9C = br.ReadInt32();

                Unk100 = br.ReadInt32();
                Unk104 = br.ReadInt32();
                Unk108 = br.ReadInt32();
                Unk10C = br.ReadInt32();

                Unk110 = br.ReadInt32();
                Unk114 = br.ReadInt32();
                Unk118 = br.ReadInt32();
                Unk11C = br.ReadInt32();

                Unk120 = br.ReadInt32();
                Unk124 = br.ReadInt32();
                Unk128 = br.ReadInt32();
                Unk12C = br.ReadInt32();

                Unk130 = br.ReadInt32();
                Unk134 = br.ReadInt32();
                Unk138 = br.ReadInt32();
                Unk13C = br.ReadInt32();

                Unk140 = br.ReadInt32();
                Unk144 = br.ReadInt32();
                Unk148 = br.ReadInt32();
                Unk14C = br.ReadInt32();

                Unk150 = br.ReadInt32();
                Unk154 = br.ReadInt32();
                Unk158 = br.ReadInt32();
                Unk15C = br.ReadInt32();
            }

            private protected override void WriteTypeData(BinaryWriterEx bw)
            {
                bw.WriteInt32(Unk00);
                bw.WriteInt32(Unk04);
                bw.WriteInt32(Unk08);
                bw.WriteInt32(Unk0C);

                bw.WriteInt32(Unk10);
                bw.WriteInt32(Unk14);
                bw.WriteInt32(Unk18);
                bw.WriteInt32(Unk1C);

                bw.WriteInt32(Unk20);
                bw.WriteInt32(Unk24);
                bw.WriteInt32(Unk28);
                bw.WriteInt32(Unk2C);

                bw.WriteInt32(Unk30);
                bw.WriteInt32(Unk34);
                bw.WriteInt32(Unk38);
                bw.WriteInt32(Unk3C);

                bw.WriteInt32(Unk40);
                bw.WriteInt32(Unk44);
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

                bw.WriteInt32(Unk80);
                bw.WriteInt32(Unk84);
                bw.WriteInt32(Unk88);
                bw.WriteInt32(Unk8C);

                bw.WriteInt32(Unk90);
                bw.WriteInt32(Unk94);
                bw.WriteInt32(Unk98);
                bw.WriteInt32(Unk9C);

                bw.WriteInt32(Unk100);
                bw.WriteInt32(Unk104);
                bw.WriteInt32(Unk108);
                bw.WriteInt32(Unk10C);

                bw.WriteInt32(Unk110);
                bw.WriteInt32(Unk114);
                bw.WriteInt32(Unk118);
                bw.WriteInt32(Unk11C);

                bw.WriteInt32(Unk120);
                bw.WriteInt32(Unk124);
                bw.WriteInt32(Unk128);
                bw.WriteInt32(Unk12C);

                bw.WriteInt32(Unk130);
                bw.WriteInt32(Unk134);
                bw.WriteInt32(Unk138);
                bw.WriteInt32(Unk13C);

                bw.WriteInt32(Unk140);
                bw.WriteInt32(Unk144);
                bw.WriteInt32(Unk148);
                bw.WriteInt32(Unk14C);

                bw.WriteInt32(Unk150);
                bw.WriteInt32(Unk154);
                bw.WriteInt32(Unk158);
                bw.WriteInt32(Unk15C);
            }

            // Layout
            public int Unk00 { get; set; } = 0;
            public int Unk04 { get; set; } = 0;
            public int Unk08 { get; set; } = 0;
            public int Unk0C { get; set; } = 0;

            public int Unk10 { get; set; } = 0;
            public int Unk14 { get; set; } = 0;
            public int Unk18 { get; set; } = 0;
            public int Unk1C { get; set; } = 0;

            public int Unk20 { get; set; } = 0;
            public int Unk24 { get; set; } = 0;
            public int Unk28 { get; set; } = 0;
            public int Unk2C { get; set; } = 0;

            public int Unk30 { get; set; } = 0;
            public int Unk34 { get; set; } = 0;
            public int Unk38 { get; set; } = 0;
            public int Unk3C { get; set; } = 0;

            public int Unk40 { get; set; } = 0;
            public int Unk44 { get; set; } = 0;
            public int Unk48 { get; set; } = 0;
            public int Unk4C { get; set; } = 0;

            public int Unk50 { get; set; } = 0;
            public int Unk54 { get; set; } = 0;
            public int Unk58 { get; set; } = 0;
            public int Unk5C { get; set; } = 0;

            public int Unk60 { get; set; } = 0;
            public int Unk64 { get; set; } = 0;
            public int Unk68 { get; set; } = 0;
            public int Unk6C { get; set; } = 0;

            public int Unk70 { get; set; } = 0;
            public int Unk74 { get; set; } = 0;
            public int Unk78 { get; set; } = 0;
            public int Unk7C { get; set; } = 0;

            public int Unk80 { get; set; } = 0;
            public int Unk84 { get; set; } = 0;
            public int Unk88 { get; set; } = 0;
            public int Unk8C { get; set; } = 0;

            public int Unk90 { get; set; } = 0;
            public int Unk94 { get; set; } = 0;
            public int Unk98 { get; set; } = 0;
            public int Unk9C { get; set; } = 0;

            public int Unk100 { get; set; } = 0;
            public int Unk104 { get; set; } = 0;
            public int Unk108 { get; set; } = 0;
            public int Unk10C { get; set; } = 0;

            public int Unk110 { get; set; } = 0;
            public int Unk114 { get; set; } = 0;
            public int Unk118 { get; set; } = 0;
            public int Unk11C { get; set; } = 0;

            public int Unk120 { get; set; } = 0;
            public int Unk124 { get; set; } = 0;
            public int Unk128 { get; set; } = 0;
            public int Unk12C { get; set; } = 0;

            public int Unk130 { get; set; } = 0;
            public int Unk134 { get; set; } = 0;
            public int Unk138 { get; set; } = 0;
            public int Unk13C { get; set; } = 0;

            public int Unk140 { get; set; } = 0;
            public int Unk144 { get; set; } = 0;
            public int Unk148 { get; set; } = 0;
            public int Unk14C { get; set; } = 0;

            public int Unk150 { get; set; } = 0;
            public int Unk154 { get; set; } = 0;
            public int Unk158 { get; set; } = 0;
            public int Unk15C { get; set; } = 0;
        }

        public class Unknown35 : Event
        {
            private protected override EventType Type => EventType.Unknown35;
            private protected override bool HasTypeData => true;

            public Unknown35() : base($"{nameof(Event)}: {nameof(TeamFight)}") { }

            internal Unknown35(BinaryReaderEx br, int version) : base(br, version) { }

            private protected override void ReadTypeData(BinaryReaderEx br, int version)
            {
                Unk00 = br.ReadInt32();
                Unk04 = br.ReadInt32();
                Unk08 = br.ReadInt32();
                Unk0C = br.ReadInt32();

                Unk10 = br.ReadInt32();
                Unk14 = br.ReadInt32();
                Unk18 = br.ReadInt32();
                Unk1C = br.ReadInt32();

                Unk20 = br.ReadInt32();
                Unk24 = br.ReadInt32();
                Unk28 = br.ReadInt32();
                Unk2C = br.ReadInt32();

                Unk30 = br.ReadInt32();
                Unk34 = br.ReadInt32();
                Unk38 = br.ReadInt32();
                Unk3C = br.ReadInt32();

                Unk40 = br.ReadInt32();
                Unk44 = br.ReadInt32();
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

                Unk80 = br.ReadInt32();
                Unk84 = br.ReadInt32();
                Unk88 = br.ReadInt32();
                Unk8C = br.ReadInt32();

                Unk90 = br.ReadInt32();
                Unk94 = br.ReadInt32();
                Unk98 = br.ReadInt32();
                Unk9C = br.ReadInt32();

                Unk100 = br.ReadInt32();
                Unk104 = br.ReadInt32();
                Unk108 = br.ReadInt32();
                Unk10C = br.ReadInt32();

                Unk110 = br.ReadInt32();
                Unk114 = br.ReadInt32();
                Unk118 = br.ReadInt32();
                Unk11C = br.ReadInt32();

                Unk120 = br.ReadInt32();
                Unk124 = br.ReadInt32();
                Unk128 = br.ReadInt32();
                Unk12C = br.ReadInt32();

                Unk130 = br.ReadInt32();
                Unk134 = br.ReadInt32();
                Unk138 = br.ReadInt32();
                Unk13C = br.ReadInt32();

                Unk140 = br.ReadInt32();
                Unk144 = br.ReadInt32();
                Unk148 = br.ReadInt32();
                Unk14C = br.ReadInt32();

                Unk150 = br.ReadInt32();
                Unk154 = br.ReadInt32();
                Unk158 = br.ReadInt32();
                Unk15C = br.ReadInt32();
            }

            private protected override void WriteTypeData(BinaryWriterEx bw)
            {
                bw.WriteInt32(Unk00);
                bw.WriteInt32(Unk04);
                bw.WriteInt32(Unk08);
                bw.WriteInt32(Unk0C);

                bw.WriteInt32(Unk10);
                bw.WriteInt32(Unk14);
                bw.WriteInt32(Unk18);
                bw.WriteInt32(Unk1C);

                bw.WriteInt32(Unk20);
                bw.WriteInt32(Unk24);
                bw.WriteInt32(Unk28);
                bw.WriteInt32(Unk2C);

                bw.WriteInt32(Unk30);
                bw.WriteInt32(Unk34);
                bw.WriteInt32(Unk38);
                bw.WriteInt32(Unk3C);

                bw.WriteInt32(Unk40);
                bw.WriteInt32(Unk44);
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

                bw.WriteInt32(Unk80);
                bw.WriteInt32(Unk84);
                bw.WriteInt32(Unk88);
                bw.WriteInt32(Unk8C);

                bw.WriteInt32(Unk90);
                bw.WriteInt32(Unk94);
                bw.WriteInt32(Unk98);
                bw.WriteInt32(Unk9C);

                bw.WriteInt32(Unk100);
                bw.WriteInt32(Unk104);
                bw.WriteInt32(Unk108);
                bw.WriteInt32(Unk10C);

                bw.WriteInt32(Unk110);
                bw.WriteInt32(Unk114);
                bw.WriteInt32(Unk118);
                bw.WriteInt32(Unk11C);

                bw.WriteInt32(Unk120);
                bw.WriteInt32(Unk124);
                bw.WriteInt32(Unk128);
                bw.WriteInt32(Unk12C);

                bw.WriteInt32(Unk130);
                bw.WriteInt32(Unk134);
                bw.WriteInt32(Unk138);
                bw.WriteInt32(Unk13C);

                bw.WriteInt32(Unk140);
                bw.WriteInt32(Unk144);
                bw.WriteInt32(Unk148);
                bw.WriteInt32(Unk14C);

                bw.WriteInt32(Unk150);
                bw.WriteInt32(Unk154);
                bw.WriteInt32(Unk158);
                bw.WriteInt32(Unk15C);
            }

            // Layout
            public int Unk00 { get; set; } = 0;
            public int Unk04 { get; set; } = 0;
            public int Unk08 { get; set; } = 0;
            public int Unk0C { get; set; } = 0;

            public int Unk10 { get; set; } = 0;
            public int Unk14 { get; set; } = 0;
            public int Unk18 { get; set; } = 0;
            public int Unk1C { get; set; } = 0;

            public int Unk20 { get; set; } = 0;
            public int Unk24 { get; set; } = 0;
            public int Unk28 { get; set; } = 0;
            public int Unk2C { get; set; } = 0;

            public int Unk30 { get; set; } = 0;
            public int Unk34 { get; set; } = 0;
            public int Unk38 { get; set; } = 0;
            public int Unk3C { get; set; } = 0;

            public int Unk40 { get; set; } = 0;
            public int Unk44 { get; set; } = 0;
            public int Unk48 { get; set; } = 0;
            public int Unk4C { get; set; } = 0;

            public int Unk50 { get; set; } = 0;
            public int Unk54 { get; set; } = 0;
            public int Unk58 { get; set; } = 0;
            public int Unk5C { get; set; } = 0;

            public int Unk60 { get; set; } = 0;
            public int Unk64 { get; set; } = 0;
            public int Unk68 { get; set; } = 0;
            public int Unk6C { get; set; } = 0;

            public int Unk70 { get; set; } = 0;
            public int Unk74 { get; set; } = 0;
            public int Unk78 { get; set; } = 0;
            public int Unk7C { get; set; } = 0;

            public int Unk80 { get; set; } = 0;
            public int Unk84 { get; set; } = 0;
            public int Unk88 { get; set; } = 0;
            public int Unk8C { get; set; } = 0;

            public int Unk90 { get; set; } = 0;
            public int Unk94 { get; set; } = 0;
            public int Unk98 { get; set; } = 0;
            public int Unk9C { get; set; } = 0;

            public int Unk100 { get; set; } = 0;
            public int Unk104 { get; set; } = 0;
            public int Unk108 { get; set; } = 0;
            public int Unk10C { get; set; } = 0;

            public int Unk110 { get; set; } = 0;
            public int Unk114 { get; set; } = 0;
            public int Unk118 { get; set; } = 0;
            public int Unk11C { get; set; } = 0;

            public int Unk120 { get; set; } = 0;
            public int Unk124 { get; set; } = 0;
            public int Unk128 { get; set; } = 0;
            public int Unk12C { get; set; } = 0;

            public int Unk130 { get; set; } = 0;
            public int Unk134 { get; set; } = 0;
            public int Unk138 { get; set; } = 0;
            public int Unk13C { get; set; } = 0;

            public int Unk140 { get; set; } = 0;
            public int Unk144 { get; set; } = 0;
            public int Unk148 { get; set; } = 0;
            public int Unk14C { get; set; } = 0;

            public int Unk150 { get; set; } = 0;
            public int Unk154 { get; set; } = 0;
            public int Unk158 { get; set; } = 0;
            public int Unk15C { get; set; } = 0;
        }


        public class Unknown36 : Event
        {
            private protected override EventType Type => EventType.Unknown36;
            private protected override bool HasTypeData => true;

            public Unknown36() : base($"{nameof(Event)}: {nameof(TeamFight)}") { }

            internal Unknown36(BinaryReaderEx br, int version) : base(br, version) { }

            private protected override void ReadTypeData(BinaryReaderEx br, int version)
            {
                Unk00 = br.ReadInt32();
                Unk04 = br.ReadInt32();
                Unk08 = br.ReadInt32();
                Unk0C = br.ReadInt32();

                Unk10 = br.ReadInt32();
                Unk14 = br.ReadInt32();
                Unk18 = br.ReadInt32();
                Unk1C = br.ReadInt32();

                Unk20 = br.ReadInt32();
                Unk24 = br.ReadInt32();
                Unk28 = br.ReadInt32();
                Unk2C = br.ReadInt32();

                Unk30 = br.ReadInt32();
                Unk34 = br.ReadInt32();
                Unk38 = br.ReadInt32();
                Unk3C = br.ReadInt32();

                Unk40 = br.ReadInt32();
                Unk44 = br.ReadInt32();
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

                Unk80 = br.ReadInt32();
                Unk84 = br.ReadInt32();
                Unk88 = br.ReadInt32();
                Unk8C = br.ReadInt32();

                Unk90 = br.ReadInt32();
                Unk94 = br.ReadInt32();
                Unk98 = br.ReadInt32();
                Unk9C = br.ReadInt32();

                Unk100 = br.ReadInt32();
                Unk104 = br.ReadInt32();
                Unk108 = br.ReadInt32();
                Unk10C = br.ReadInt32();

                Unk110 = br.ReadInt32();
                Unk114 = br.ReadInt32();
                Unk118 = br.ReadInt32();
                Unk11C = br.ReadInt32();

                Unk120 = br.ReadInt32();
                Unk124 = br.ReadInt32();
                Unk128 = br.ReadInt32();
                Unk12C = br.ReadInt32();

                Unk130 = br.ReadInt32();
                Unk134 = br.ReadInt32();
                Unk138 = br.ReadInt32();
                Unk13C = br.ReadInt32();

                Unk140 = br.ReadInt32();
                Unk144 = br.ReadInt32();
                Unk148 = br.ReadInt32();
                Unk14C = br.ReadInt32();

                Unk150 = br.ReadInt32();
                Unk154 = br.ReadInt32();
                Unk158 = br.ReadInt32();
                Unk15C = br.ReadInt32();
            }

            private protected override void WriteTypeData(BinaryWriterEx bw)
            {
                bw.WriteInt32(Unk00);
                bw.WriteInt32(Unk04);
                bw.WriteInt32(Unk08);
                bw.WriteInt32(Unk0C);

                bw.WriteInt32(Unk10);
                bw.WriteInt32(Unk14);
                bw.WriteInt32(Unk18);
                bw.WriteInt32(Unk1C);

                bw.WriteInt32(Unk20);
                bw.WriteInt32(Unk24);
                bw.WriteInt32(Unk28);
                bw.WriteInt32(Unk2C);

                bw.WriteInt32(Unk30);
                bw.WriteInt32(Unk34);
                bw.WriteInt32(Unk38);
                bw.WriteInt32(Unk3C);

                bw.WriteInt32(Unk40);
                bw.WriteInt32(Unk44);
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

                bw.WriteInt32(Unk80);
                bw.WriteInt32(Unk84);
                bw.WriteInt32(Unk88);
                bw.WriteInt32(Unk8C);

                bw.WriteInt32(Unk90);
                bw.WriteInt32(Unk94);
                bw.WriteInt32(Unk98);
                bw.WriteInt32(Unk9C);

                bw.WriteInt32(Unk100);
                bw.WriteInt32(Unk104);
                bw.WriteInt32(Unk108);
                bw.WriteInt32(Unk10C);

                bw.WriteInt32(Unk110);
                bw.WriteInt32(Unk114);
                bw.WriteInt32(Unk118);
                bw.WriteInt32(Unk11C);

                bw.WriteInt32(Unk120);
                bw.WriteInt32(Unk124);
                bw.WriteInt32(Unk128);
                bw.WriteInt32(Unk12C);

                bw.WriteInt32(Unk130);
                bw.WriteInt32(Unk134);
                bw.WriteInt32(Unk138);
                bw.WriteInt32(Unk13C);

                bw.WriteInt32(Unk140);
                bw.WriteInt32(Unk144);
                bw.WriteInt32(Unk148);
                bw.WriteInt32(Unk14C);

                bw.WriteInt32(Unk150);
                bw.WriteInt32(Unk154);
                bw.WriteInt32(Unk158);
                bw.WriteInt32(Unk15C);
            }

            // Layout
            public int Unk00 { get; set; } = 0;
            public int Unk04 { get; set; } = 0;
            public int Unk08 { get; set; } = 0;
            public int Unk0C { get; set; } = 0;

            public int Unk10 { get; set; } = 0;
            public int Unk14 { get; set; } = 0;
            public int Unk18 { get; set; } = 0;
            public int Unk1C { get; set; } = 0;

            public int Unk20 { get; set; } = 0;
            public int Unk24 { get; set; } = 0;
            public int Unk28 { get; set; } = 0;
            public int Unk2C { get; set; } = 0;

            public int Unk30 { get; set; } = 0;
            public int Unk34 { get; set; } = 0;
            public int Unk38 { get; set; } = 0;
            public int Unk3C { get; set; } = 0;

            public int Unk40 { get; set; } = 0;
            public int Unk44 { get; set; } = 0;
            public int Unk48 { get; set; } = 0;
            public int Unk4C { get; set; } = 0;

            public int Unk50 { get; set; } = 0;
            public int Unk54 { get; set; } = 0;
            public int Unk58 { get; set; } = 0;
            public int Unk5C { get; set; } = 0;

            public int Unk60 { get; set; } = 0;
            public int Unk64 { get; set; } = 0;
            public int Unk68 { get; set; } = 0;
            public int Unk6C { get; set; } = 0;

            public int Unk70 { get; set; } = 0;
            public int Unk74 { get; set; } = 0;
            public int Unk78 { get; set; } = 0;
            public int Unk7C { get; set; } = 0;

            public int Unk80 { get; set; } = 0;
            public int Unk84 { get; set; } = 0;
            public int Unk88 { get; set; } = 0;
            public int Unk8C { get; set; } = 0;

            public int Unk90 { get; set; } = 0;
            public int Unk94 { get; set; } = 0;
            public int Unk98 { get; set; } = 0;
            public int Unk9C { get; set; } = 0;

            public int Unk100 { get; set; } = 0;
            public int Unk104 { get; set; } = 0;
            public int Unk108 { get; set; } = 0;
            public int Unk10C { get; set; } = 0;

            public int Unk110 { get; set; } = 0;
            public int Unk114 { get; set; } = 0;
            public int Unk118 { get; set; } = 0;
            public int Unk11C { get; set; } = 0;

            public int Unk120 { get; set; } = 0;
            public int Unk124 { get; set; } = 0;
            public int Unk128 { get; set; } = 0;
            public int Unk12C { get; set; } = 0;

            public int Unk130 { get; set; } = 0;
            public int Unk134 { get; set; } = 0;
            public int Unk138 { get; set; } = 0;
            public int Unk13C { get; set; } = 0;

            public int Unk140 { get; set; } = 0;
            public int Unk144 { get; set; } = 0;
            public int Unk148 { get; set; } = 0;
            public int Unk14C { get; set; } = 0;

            public int Unk150 { get; set; } = 0;
            public int Unk154 { get; set; } = 0;
            public int Unk158 { get; set; } = 0;
            public int Unk15C { get; set; } = 0;
        }

        public class Unknown37 : Event
        {
            private protected override EventType Type => EventType.Unknown37;
            private protected override bool HasTypeData => true;

            public Unknown37() : base($"{nameof(Event)}: {nameof(TeamFight)}") { }

            internal Unknown37(BinaryReaderEx br, int version) : base(br, version) { }

            private protected override void ReadTypeData(BinaryReaderEx br, int version)
            {
                Unk00 = br.ReadInt32();
                Unk04 = br.ReadInt32();
                Unk08 = br.ReadInt32();
                Unk0C = br.ReadInt32();

                Unk10 = br.ReadInt32();
                Unk14 = br.ReadInt32();
                Unk18 = br.ReadInt32();
                Unk1C = br.ReadInt32();

                Unk20 = br.ReadInt32();
                Unk24 = br.ReadInt32();
                Unk28 = br.ReadInt32();
                Unk2C = br.ReadInt32();

                Unk30 = br.ReadInt32();
                Unk34 = br.ReadInt32();
                Unk38 = br.ReadInt32();
                Unk3C = br.ReadInt32();

                Unk40 = br.ReadInt32();
                Unk44 = br.ReadInt32();
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
                bw.WriteInt32(Unk00);
                bw.WriteInt32(Unk04);
                bw.WriteInt32(Unk08);
                bw.WriteInt32(Unk0C);

                bw.WriteInt32(Unk10);
                bw.WriteInt32(Unk14);
                bw.WriteInt32(Unk18);
                bw.WriteInt32(Unk1C);

                bw.WriteInt32(Unk20);
                bw.WriteInt32(Unk24);
                bw.WriteInt32(Unk28);
                bw.WriteInt32(Unk2C);

                bw.WriteInt32(Unk30);
                bw.WriteInt32(Unk34);
                bw.WriteInt32(Unk38);
                bw.WriteInt32(Unk3C);

                bw.WriteInt32(Unk40);
                bw.WriteInt32(Unk44);
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

            // Layout
            public int Unk00 { get; set; } = 0;
            public int Unk04 { get; set; } = 0;
            public int Unk08 { get; set; } = 0;
            public int Unk0C { get; set; } = 0;

            public int Unk10 { get; set; } = 0;
            public int Unk14 { get; set; } = 0;
            public int Unk18 { get; set; } = 0;
            public int Unk1C { get; set; } = 0;

            public int Unk20 { get; set; } = 0;
            public int Unk24 { get; set; } = 0;
            public int Unk28 { get; set; } = 0;
            public int Unk2C { get; set; } = 0;

            public int Unk30 { get; set; } = 0;
            public int Unk34 { get; set; } = 0;
            public int Unk38 { get; set; } = 0;
            public int Unk3C { get; set; } = 0;

            public int Unk40 { get; set; } = 0;
            public int Unk44 { get; set; } = 0;
            public int Unk48 { get; set; } = 0;
            public int Unk4C { get; set; } = 0;

            public int Unk50 { get; set; } = 0;
            public int Unk54 { get; set; } = 0;
            public int Unk58 { get; set; } = 0;
            public int Unk5C { get; set; } = 0;

            public int Unk60 { get; set; } = 0;
            public int Unk64 { get; set; } = 0;
            public int Unk68 { get; set; } = 0;
            public int Unk6C { get; set; } = 0;

            public int Unk70 { get; set; } = 0;
            public int Unk74 { get; set; } = 0;
            public int Unk78 { get; set; } = 0;
            public int Unk7C { get; set; } = 0;
        }


        public class Other : Event
        {
            private protected override EventType Type => EventType.Other;
            private protected override bool HasTypeData => false;

            public Other() : base($"{nameof(Event)}: {nameof(Other)}") { }

            internal Other(BinaryReaderEx br, int version) : base(br, version) { }
        }
    }

}