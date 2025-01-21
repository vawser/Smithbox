using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace SoulsFormats
{
    public partial class MSB1
    {
        internal enum PartType : uint
        {
            MapPiece = 0,
            Object = 1,
            Enemy = 2,
            Player = 4,
            Collision = 5,
            Navmesh = 8,
            DummyObject = 9,
            DummyEnemy = 10,
            ConnectCollision = 11,
        }

        /// <summary>
        /// All instances of concrete things in the map.
        /// </summary>
        public class PartsParam : Param<Part>, IMsbParam<IMsbPart>
        {
            internal override string Name => "PARTS_PARAM_ST";

            /// <summary>
            /// All of the fixed visual geometry of the map.
            /// </summary>
            public List<Part.MapPiece> MapPieces { get; set; }

            /// <summary>
            /// Dynamic props and interactive things.
            /// </summary>
            public List<Part.Object> Objects { get; set; }

            /// <summary>
            /// All non-player characters.
            /// </summary>
            public List<Part.Enemy> Enemies { get; set; }

            /// <summary>
            /// These have something to do with player spawn points.
            /// </summary>
            public List<Part.Player> Players { get; set; }

            /// <summary>
            /// Invisible physical geometry of the map.
            /// </summary>
            public List<Part.Collision> Collisions { get; set; }

            /// <summary>
            /// AI navigation meshes.
            /// </summary>
            public List<Part.Navmesh> Navmeshes { get; set; }

            /// <summary>
            /// Objects that don't appear normally; either unused, or used for cutscenes.
            /// </summary>
            public List<Part.DummyObject> DummyObjects { get; set; }

            /// <summary>
            /// Enemies that don't appear normally; either unused, or used for cutscenes.
            /// </summary>
            public List<Part.DummyEnemy> DummyEnemies { get; set; }

            /// <summary>
            /// Dummy parts that reference an actual collision and cause it to load another map.
            /// </summary>
            public List<Part.ConnectCollision> ConnectCollisions { get; set; }

            /// <summary>
            /// Creates an empty PartsParam.
            /// </summary>
            public PartsParam() : base()
            {
                MapPieces = new List<Part.MapPiece>();
                Objects = new List<Part.Object>();
                Enemies = new List<Part.Enemy>();
                Players = new List<Part.Player>();
                Collisions = new List<Part.Collision>();
                Navmeshes = new List<Part.Navmesh>();
                DummyObjects = new List<Part.DummyObject>();
                DummyEnemies = new List<Part.DummyEnemy>();
                ConnectCollisions = new List<Part.ConnectCollision>();
            }

            /// <summary>
            /// Adds a part to the appropriate list for its type; returns the part.
            /// </summary>
            public Part Add(Part part)
            {
                switch (part)
                {
                    case Part.MapPiece p: MapPieces.Add(p); break;
                    case Part.Object p: Objects.Add(p); break;
                    case Part.Enemy p: Enemies.Add(p); break;
                    case Part.Player p: Players.Add(p); break;
                    case Part.Collision p: Collisions.Add(p); break;
                    case Part.Navmesh p: Navmeshes.Add(p); break;
                    case Part.DummyObject p: DummyObjects.Add(p); break;
                    case Part.DummyEnemy p: DummyEnemies.Add(p); break;
                    case Part.ConnectCollision p: ConnectCollisions.Add(p); break;

                    default:
                        throw new ArgumentException($"Unrecognized type {part.GetType()}.", nameof(part));
                }
                return part;
            }
            IMsbPart IMsbParam<IMsbPart>.Add(IMsbPart item) => Add((Part)item);

            /// <summary>
            /// Returns every Part in the order they'll be written.
            /// </summary>
            public override List<Part> GetEntries()
            {
                return SFUtil.ConcatAll<Part>(
                    MapPieces, Objects, Enemies, Players, Collisions,
                    Navmeshes, DummyObjects, DummyEnemies, ConnectCollisions);
            }
            IReadOnlyList<IMsbPart> IMsbParam<IMsbPart>.GetEntries() => GetEntries();

            internal override Part ReadEntry(BinaryReaderEx br)
            {
                PartType type = br.GetEnum32<PartType>(br.Position + 4);
                switch (type)
                {
                    case PartType.MapPiece:
                        return MapPieces.EchoAdd(new Part.MapPiece(br));

                    case PartType.Object:
                        return Objects.EchoAdd(new Part.Object(br));

                    case PartType.Enemy:
                        return Enemies.EchoAdd(new Part.Enemy(br));

                    case PartType.Player:
                        return Players.EchoAdd(new Part.Player(br));

                    case PartType.Collision:
                        return Collisions.EchoAdd(new Part.Collision(br));

                    case PartType.Navmesh:
                        return Navmeshes.EchoAdd(new Part.Navmesh(br));

                    case PartType.DummyObject:
                        return DummyObjects.EchoAdd(new Part.DummyObject(br));

                    case PartType.DummyEnemy:
                        return DummyEnemies.EchoAdd(new Part.DummyEnemy(br));

                    case PartType.ConnectCollision:
                        return ConnectCollisions.EchoAdd(new Part.ConnectCollision(br));

                    default:
                        throw new NotImplementedException($"Unimplemented part type: {type}");
                }
            }
        }

        /// <summary>
        /// Common information for all concrete entities.
        /// </summary>
        public abstract class Part : Entry, IMsbPart
        {
            private protected abstract PartType Type { get; }

            /// <summary>
            /// The model of the Part, corresponding to an entry in the ModelParam.
            /// </summary>
            public string ModelName { get; set; }
            private int ModelIndex;

            /// <summary>
            /// A path to a .sib file, presumed to be some kind of editor placeholder.
            /// </summary>
            public string SibPath { get; set; }

            /// <summary>
            /// Location of the part.
            /// </summary>
            public Vector3 Position { get; set; }

            /// <summary>
            /// Rotation of the part, in degrees.
            /// </summary>
            public Vector3 Rotation { get; set; }

            /// <summary>
            /// Scale of the part, only meaningful for map pieces and objects.
            /// </summary>
            public Vector3 Scale { get; set; }

            /// <summary>
            /// Controls when the part is visible.
            /// </summary>
            public uint[] DrawGroups { get; private set; }

            /// <summary>
            /// Controls when the part is visible.
            /// </summary>
            public uint[] DispGroups { get; private set; }

            /// <summary>
            /// Identifies the part in external files.
            /// </summary>
            public int EntityID { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte LightID { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte FogID { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte ScatterID { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte LensFlareID { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte ShadowID { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte DofID { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte ToneMapID { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte ToneCorrectID { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte LanternID { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte LodParamID { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte IsShadowSrc { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte IsShadowDest { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte IsShadowOnly { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte DrawByReflectCam { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte DrawOnlyReflectCam { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte UseDepthBiasFloat { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte DisablePointLightEffect { get; set; }

            private protected Part(string name)
            {
                Name = name;
                SibPath = "";
                Scale = Vector3.One;
                DrawGroups = new uint[4] {
                    0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF };
                DispGroups = new uint[4] {
                    0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF };
                EntityID = -1;
            }

            /// <summary>
            /// Creates a deep copy of the part.
            /// </summary>
            public Part DeepCopy()
            {
                var part = (Part)MemberwiseClone();
                part.DrawGroups = (uint[])DrawGroups.Clone();
                part.DispGroups = (uint[])DispGroups.Clone();
                DeepCopyTo(part);
                return part;
            }
            IMsbPart IMsbPart.DeepCopy() => DeepCopy();

            private protected virtual void DeepCopyTo(Part part) { }

            private protected Part(BinaryReaderEx br)
            {
                long start = br.Position;
                int nameOffset = br.ReadInt32();
                br.AssertUInt32((uint)Type);
                br.ReadInt32(); // ID
                ModelIndex = br.ReadInt32();
                int sibOffset = br.ReadInt32();
                Position = br.ReadVector3();
                Rotation = br.ReadVector3();
                Scale = br.ReadVector3();
                DrawGroups = br.ReadUInt32s(4);
                DispGroups = br.ReadUInt32s(4);
                int entityDataOffset = br.ReadInt32();
                int typeDataOffset = br.ReadInt32();
                br.AssertInt32(0);

                if (nameOffset == 0)
                    throw new InvalidDataException($"{nameof(nameOffset)} must not be 0 in type {GetType()}.");
                if (sibOffset == 0)
                    throw new InvalidDataException($"{nameof(sibOffset)} must not be 0 in type {GetType()}.");
                if (entityDataOffset == 0)
                    throw new InvalidDataException($"{nameof(entityDataOffset)} must not be 0 in type {GetType()}.");
                if (typeDataOffset == 0)
                    throw new InvalidDataException($"{nameof(typeDataOffset)} must not be 0 in type {GetType()}.");

                br.Position = start + nameOffset;
                Name = br.ReadShiftJIS();

                br.Position = start + sibOffset;
                SibPath = br.ReadShiftJIS();

                br.Position = start + entityDataOffset;
                ReadEntityData(br);

                br.Position = start + typeDataOffset;
                ReadTypeData(br);
            }

            private void ReadEntityData(BinaryReaderEx br)
            {
                EntityID = br.ReadInt32();
                LightID = br.ReadByte();
                FogID = br.ReadByte();
                ScatterID = br.ReadByte();
                LensFlareID = br.ReadByte();
                ShadowID = br.ReadByte();
                DofID = br.ReadByte();
                ToneMapID = br.ReadByte();
                ToneCorrectID = br.ReadByte();
                LanternID = br.ReadByte();
                LodParamID = br.ReadByte();
                br.AssertByte(0);
                IsShadowSrc = br.ReadByte();
                IsShadowDest = br.ReadByte();
                IsShadowOnly = br.ReadByte();
                DrawByReflectCam = br.ReadByte();
                DrawOnlyReflectCam = br.ReadByte();
                UseDepthBiasFloat = br.ReadByte();
                DisablePointLightEffect = br.ReadByte();
                br.AssertByte(0);
                br.AssertByte(0);
            }

            private protected abstract void ReadTypeData(BinaryReaderEx br);

            internal override void Write(BinaryWriterEx bw, int id)
            {
                long start = bw.Position;
                bw.ReserveInt32("NameOffset");
                bw.WriteUInt32((uint)Type);
                bw.WriteInt32(id);
                bw.WriteInt32(ModelIndex);
                bw.ReserveInt32("SibOffset");
                bw.WriteVector3(Position);
                bw.WriteVector3(Rotation);
                bw.WriteVector3(Scale);
                bw.WriteUInt32s(DrawGroups);
                bw.WriteUInt32s(DispGroups);
                bw.ReserveInt32("EntityDataOffset");
                bw.ReserveInt32("TypeDataOffset");
                bw.WriteInt32(0);

                long stringsStart = bw.Position;
                bw.FillInt32("NameOffset", (int)(bw.Position - start));
                bw.WriteShiftJIS(MSB.ReambiguateName(Name), true);

                bw.FillInt32("SibOffset", (int)(bw.Position - start));
                bw.WriteShiftJIS(SibPath, true);
                bw.Pad(4);
                if (bw.Position - stringsStart < 0x14)
                    bw.WritePattern((int)(0x14 - (bw.Position - stringsStart)), 0x00);

                bw.FillInt32("EntityDataOffset", (int)(bw.Position - start));
                WriteEntityData(bw);

                bw.FillInt32("TypeDataOffset", (int)(bw.Position - start));
                WriteTypeData(bw);
            }

            private void WriteEntityData(BinaryWriterEx bw)
            {
                bw.WriteInt32(EntityID);
                bw.WriteByte(LightID);
                bw.WriteByte(FogID);
                bw.WriteByte(ScatterID);
                bw.WriteByte(LensFlareID);
                bw.WriteByte(ShadowID);
                bw.WriteByte(DofID);
                bw.WriteByte(ToneMapID);
                bw.WriteByte(ToneCorrectID);
                bw.WriteByte(LanternID);
                bw.WriteByte(LodParamID);
                bw.WriteByte(0);
                bw.WriteByte(IsShadowSrc);
                bw.WriteByte(IsShadowDest);
                bw.WriteByte(IsShadowOnly);
                bw.WriteByte(DrawByReflectCam);
                bw.WriteByte(DrawOnlyReflectCam);
                bw.WriteByte(UseDepthBiasFloat);
                bw.WriteByte(DisablePointLightEffect);
                bw.WriteByte(0);
                bw.WriteByte(0);
            }

            private protected abstract void WriteTypeData(BinaryWriterEx bw);

            internal virtual void GetNames(MSB1 msb, Entries entries)
            {
                ModelName = MSB.FindName(entries.Models, ModelIndex);
            }

            internal virtual void GetIndices(MSB1 msb, Entries entries)
            {
                ModelIndex = MSB.FindIndex(this, entries.Models, ModelName);
            }

            /// <summary>
            /// A visible but not physical model making up the map.
            /// </summary>
            public class MapPiece : Part
            {
                private protected override PartType Type => PartType.MapPiece;

                /// <summary>
                /// Creates a MapPiece with default values.
                /// </summary>
                public MapPiece() : base("mXXXXBX") { }

                internal MapPiece(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// Common base data for objects and dummy objects.
            /// </summary>
            public abstract class ObjectBase : Part
            {
                /// <summary>
                /// Collision that controls loading of the object.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Collision))]
                [NoRenderGroupInheritence()]
                public string CollisionName { get; set; }
                public int CollisionIndex { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public sbyte BreakTerm { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public sbyte NetSyncType { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short InitAnimID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short UnkT0E { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT10 { get; set; }

                private protected ObjectBase() : base("oXXXX_XXXX") { }

                private protected ObjectBase(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    br.AssertInt32(0);
                    CollisionIndex = br.ReadInt32();
                    BreakTerm = br.ReadSByte();
                    NetSyncType = br.ReadSByte();
                    br.AssertInt16(0);
                    InitAnimID = br.ReadInt16();
                    UnkT0E = br.ReadInt16();
                    UnkT10 = br.ReadInt32();
                    br.AssertInt32(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(0);
                    bw.WriteInt32(CollisionIndex);
                    bw.WriteSByte(BreakTerm);
                    bw.WriteSByte(NetSyncType);
                    bw.WriteInt16(0);
                    bw.WriteInt16(InitAnimID);
                    bw.WriteInt16(UnkT0E);
                    bw.WriteInt32(UnkT10);
                    bw.WriteInt32(0);
                }

                internal override void GetNames(MSB1 msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    CollisionName = MSB.FindName(entries.Parts, CollisionIndex);
                }

                internal override void GetIndices(MSB1 msb, Entries entries)
                {
                    base.GetIndices(msb, entries);
                    CollisionIndex = MSB.FindIndex(this, entries.Parts, CollisionName);
                }
            }

            /// <summary>
            /// A dynamic or interactible part of the map.
            /// </summary>
            public class Object : ObjectBase
            {
                private protected override PartType Type => PartType.Object;

                /// <summary>
                /// Creates an Object with default values.
                /// </summary>
                public Object() : base() { }

                internal Object(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// Common base data for enemies and dummy enemies.
            /// </summary>
            public abstract class EnemyBase : Part
            {
                /// <summary>
                /// ID in NPCThinkParam determining AI properties.
                /// </summary>
                public int ThinkParamID { get; set; }

                /// <summary>
                /// ID in NPCParam determining character properties.
                /// </summary>
                public int NPCParamID { get; set; }

                /// <summary>
                /// ID of a talk ESD used by the character.
                /// </summary>
                public int TalkID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte PointMoveType { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public ushort PlatoonID { get; set; }

                /// <summary>
                /// ID in CharaInitParam determining equipment and stats for humans.
                /// </summary>
                public int CharaInitID { get; set; }

                /// <summary>
                /// Collision that controls loading of the enemy.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Collision))]
                public string CollisionName { get; set; }
                public int CollisionIndex { get; set; }

                /// <summary>
                /// Regions for the enemy to patrol.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Region))]
                public string[] MovePointNames { get; set; }
                public short[] MovePointIndices { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int InitAnimID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int DamageAnimID { get; set; }

                private protected EnemyBase() : base("cXXXX_XXXX")
                {
                    ThinkParamID = -1;
                    NPCParamID = -1;
                    TalkID = -1;
                    CharaInitID = -1;
                    MovePointIndices = new short[8];
                    MovePointNames = new string[8];
                }

                private protected override void DeepCopyTo(Part part)
                {
                    var enemy = (EnemyBase)part;
                    enemy.MovePointIndices = (short[])MovePointIndices.Clone();
                    enemy.MovePointNames = (string[])MovePointNames.Clone();
                }

                private protected EnemyBase(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    ThinkParamID = br.ReadInt32();
                    NPCParamID = br.ReadInt32();
                    TalkID = br.ReadInt32();
                    PointMoveType = br.ReadByte();
                    br.AssertByte(0);
                    PlatoonID = br.ReadUInt16();
                    CharaInitID = br.ReadInt32();
                    CollisionIndex = br.ReadInt32();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    MovePointIndices = br.ReadInt16s(8);
                    InitAnimID = br.ReadInt32();
                    DamageAnimID = br.ReadInt32();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(ThinkParamID);
                    bw.WriteInt32(NPCParamID);
                    bw.WriteInt32(TalkID);
                    bw.WriteByte(PointMoveType);
                    bw.WriteByte(0);
                    bw.WriteUInt16(PlatoonID);
                    bw.WriteInt32(CharaInitID);
                    bw.WriteInt32(CollisionIndex);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt16s(MovePointIndices);
                    bw.WriteInt32(InitAnimID);
                    bw.WriteInt32(DamageAnimID);
                }

                internal override void GetNames(MSB1 msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    CollisionName = MSB.FindName(entries.Parts, CollisionIndex);

                    MovePointNames = new string[MovePointIndices.Length];
                    for (int i = 0; i < MovePointIndices.Length; i++)
                        MovePointNames[i] = MSB.FindName(entries.Regions, MovePointIndices[i]);
                }

                internal override void GetIndices(MSB1 msb, Entries entries)
                {
                    base.GetIndices(msb, entries);
                    CollisionIndex = MSB.FindIndex(this, entries.Parts, CollisionName);

                    MovePointIndices = new short[MovePointNames.Length];
                    for (int i = 0; i < MovePointNames.Length; i++)
                        MovePointIndices[i] = (short)MSB.FindIndex(this, entries.Regions, MovePointNames[i]);
                }
            }

            /// <summary>
            /// Any living entity besides the player character.
            /// </summary>
            public class Enemy : EnemyBase
            {
                private protected override PartType Type => PartType.Enemy;

                /// <summary>
                /// Creates an Enemy with default values.
                /// </summary>
                public Enemy() : base() { }

                internal Enemy(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// Unknown exactly what these do.
            /// </summary>
            public class Player : Part
            {
                private protected override PartType Type => PartType.Player;

                /// <summary>
                /// Creates a Player with default values.
                /// </summary>
                public Player() : base("c0000_XXXX") { }

                internal Player(BinaryReaderEx br) : base(br) { }

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
            /// Invisible but physical geometry.
            /// </summary>
            public class Collision : Part
            {
                private protected override PartType Type => PartType.Collision;

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte HitFilterID { get; set; }

                /// <summary>
                /// Causes sounds to be modulated when standing on the collision.
                /// </summary>
                public byte SoundSpaceType { get; set; }

                /// <summary>
                /// Environment lighting
                /// </summary>
                [MSBReference(ReferenceType = typeof(Event.Environment))]
                public string EnvLightMapSpotName { get; set; }
                public short EnvLightMapSpotIndex { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float ReflectPlaneHeight { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public uint[] NvmGroups { get; private set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int[] VagrantEntityIDs { get; private set; }

                /// <summary>
                /// Controls displays of the map name on screen or the loading menu.
                /// </summary>
                public short MapNameID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public bool DisableStart { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT27 { get; set; }

                /// <summary>
                /// If set, disables a bonfire when any enemy is on the collision.
                /// </summary>
                public int DisableBonfireEntityID { get; set; }

                /// <summary>
                /// Unknown value only used in m99 maps.
                /// </summary>
                public int M99Unk00 { get; set; }

                /// <summary>
                /// Unknown value only used in m99 maps.
                /// </summary>
                public int M99Unk04 { get; set; }

                /// <summary>
                /// Unknown value only used in m99 maps.
                /// </summary>
                public int M99Unk08 { get; set; }

                /// <summary>
                /// An ID used for multiplayer eligibility.
                /// </summary>
                public int PlayRegionID { get; set; }

                /// <summary>
                /// ID in LockCamParam determining camera properties.
                /// </summary>
                public short LockCamParamID1 { get; set; }

                /// <summary>
                /// ID in LockCamParam determining camera properties.
                /// </summary>
                public short LockCamParamID2 { get; set; }

                /// <summary>
                /// Creates a Collision with default values.
                /// </summary>
                public Collision() : base("hXXXXBX")
                {
                    NvmGroups = new uint[4]{
                        0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF };
                    VagrantEntityIDs = new int[3] { -1, -1, -1 };
                    MapNameID = -1;
                    DisableBonfireEntityID = -1;
                    LockCamParamID1 = -1;
                    LockCamParamID2 = -1;
                }

                private protected override void DeepCopyTo(Part part)
                {
                    var collision = (Collision)part;
                    collision.NvmGroups = (uint[])NvmGroups.Clone();
                    collision.VagrantEntityIDs = (int[])VagrantEntityIDs.Clone();
                }

                internal Collision(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    HitFilterID = br.ReadByte();
                    SoundSpaceType = br.ReadByte();
                    EnvLightMapSpotIndex = br.ReadInt16();
                    ReflectPlaneHeight = br.ReadSingle();
                    NvmGroups = br.ReadUInt32s(4);
                    VagrantEntityIDs = br.ReadInt32s(3);
                    MapNameID = br.ReadInt16();
                    DisableStart = br.ReadBoolean();
                    UnkT27 = br.ReadByte();
                    DisableBonfireEntityID = br.ReadInt32();
                    M99Unk00 = br.ReadInt32();
                    M99Unk04 = br.ReadInt32();
                    M99Unk08 = br.ReadInt32();
                    PlayRegionID = br.ReadInt32();
                    LockCamParamID1 = br.ReadInt16();
                    LockCamParamID2 = br.ReadInt16();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteByte(HitFilterID);
                    bw.WriteByte(SoundSpaceType);
                    bw.WriteInt16(EnvLightMapSpotIndex);
                    bw.WriteSingle(ReflectPlaneHeight);
                    bw.WriteUInt32s(NvmGroups);
                    bw.WriteInt32s(VagrantEntityIDs);
                    bw.WriteInt16(MapNameID);
                    bw.WriteBoolean(DisableStart);
                    bw.WriteByte(UnkT27);
                    bw.WriteInt32(DisableBonfireEntityID);
                    bw.WriteInt32(M99Unk00);
                    bw.WriteInt32(M99Unk04);
                    bw.WriteInt32(M99Unk08);
                    bw.WriteInt32(PlayRegionID);
                    bw.WriteInt16(LockCamParamID1);
                    bw.WriteInt16(LockCamParamID2);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }

                internal override void GetNames(MSB1 msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    EnvLightMapSpotName = MSB.FindName(entries.Events.FindAll(e => e.GetType() == typeof(MSB1.Event.Environment)), EnvLightMapSpotIndex);
                }

                internal override void GetIndices(MSB1 msb, Entries entries)
                {
                    base.GetIndices(msb, entries);
                    EnvLightMapSpotIndex = (short)MSB.FindIndex(this, entries.Events.FindAll(e => e.GetType() == typeof(MSB1.Event.Environment)), EnvLightMapSpotName);
                }
            }

            /// <summary>
            /// An AI navigation mesh.
            /// </summary>
            public class Navmesh : Part
            {
                private protected override PartType Type => PartType.Navmesh;

                /// <summary>
                /// Unknown.
                /// </summary>
                public uint[] NvmGroups { get; private set; }

                /// <summary>
                /// Creates a Navmesh with default values.
                /// </summary>
                public Navmesh() : base("nXXXXBX")
                {
                    NvmGroups = new uint[4] {
                        0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF };
                }

                private protected override void DeepCopyTo(Part part)
                {
                    var navmesh = (Navmesh)part;
                    navmesh.NvmGroups = (uint[])NvmGroups.Clone();
                }

                internal Navmesh(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    NvmGroups = br.ReadUInt32s(4);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteUInt32s(NvmGroups);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// A normally invisible object, either unused or for a cutscene.
            /// </summary>
            public class DummyObject : ObjectBase
            {
                private protected override PartType Type => PartType.DummyObject;

                /// <summary>
                /// Creates a DummyObject with default values.
                /// </summary>
                public DummyObject() : base() { }

                internal DummyObject(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// A normally invisible enemy, either unused or for a cutscene.
            /// </summary>
            public class DummyEnemy : EnemyBase
            {
                private protected override PartType Type => PartType.DummyEnemy;

                /// <summary>
                /// Creates a DummyEnemy with default values.
                /// </summary>
                public DummyEnemy() : base() { }

                internal DummyEnemy(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// Attaches to an actual Collision and causes another map to be loaded when standing on it.
            /// </summary>
            public class ConnectCollision : Part
            {
                private protected override PartType Type => PartType.ConnectCollision;

                /// <summary>
                /// The collision which will load another map.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Collision))]
                public string CollisionName { get; set; }
                public int CollisionIndex { get; set; }

                /// <summary>
                /// Four bytes specifying the map ID to load.
                /// </summary>
                public byte[] MapID { get; private set; }

                /// <summary>
                /// Creates a ConnectCollision with default values.
                /// </summary>
                public ConnectCollision() : base("hXXXXBX_XXXX")
                {
                    MapID = new byte[4] { 10, 2, 0, 0 };
                }

                private protected override void DeepCopyTo(Part part)
                {
                    var connect = (ConnectCollision)part;
                    connect.MapID = (byte[])MapID.Clone();
                }

                internal ConnectCollision(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    CollisionIndex = br.ReadInt32();
                    MapID = br.ReadBytes(4);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(CollisionIndex);
                    bw.WriteBytes(MapID);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }

                internal override void GetNames(MSB1 msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    CollisionName = MSB.FindName(msb.Parts.Collisions, CollisionIndex);
                }

                internal override void GetIndices(MSB1 msb, Entries entries)
                {
                    base.GetIndices(msb, entries);
                    CollisionIndex = MSB.FindIndex(this, msb.Parts.Collisions, CollisionName);
                }
            }
        }
    }
}
