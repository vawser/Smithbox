using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace SoulsFormats
{
    public partial class MSB3
    {
        internal enum PartsType : uint
        {
            MapPiece = 0,
            Object = 1,
            Enemy = 2,
            Item = 3,
            Player = 4,
            Collision = 5,
            NPCWander = 6,
            Protoboss = 7,
            Navmesh = 8,
            DummyObject = 9,
            DummyEnemy = 10,
            ConnectCollision = 11,
        }

        /// <summary>
        /// Instances of various "things" in this MSB.
        /// </summary>
        public class PartsParam : Param<Part>, IMsbParam<IMsbPart>
        {
            internal override int Version => 3;
            internal override string Type => "PARTS_PARAM_ST";

            /// <summary>
            /// Map pieces in the MSB.
            /// </summary>
            public List<Part.MapPiece> MapPieces { get; set; }

            /// <summary>
            /// Objects in the MSB.
            /// </summary>
            public List<Part.Object> Objects { get; set; }

            /// <summary>
            /// Enemies in the MSB.
            /// </summary>
            public List<Part.Enemy> Enemies { get; set; }

            /// <summary>
            /// Players in the MSB.
            /// </summary>
            public List<Part.Player> Players { get; set; }

            /// <summary>
            /// Collisions in the MSB.
            /// </summary>
            public List<Part.Collision> Collisions { get; set; }

            /// <summary>
            /// Dummy objects in the MSB.
            /// </summary>
            public List<Part.DummyObject> DummyObjects { get; set; }

            /// <summary>
            /// Dummy enemies in the MSB.
            /// </summary>
            public List<Part.DummyEnemy> DummyEnemies { get; set; }

            /// <summary>
            /// Connect collisions in the MSB.
            /// </summary>
            public List<Part.ConnectCollision> ConnectCollisions { get; set; }

            /// <summary>
            /// Creates a new PartsParam with no parts.
            /// </summary>
            public PartsParam()
            {
                MapPieces = new List<Part.MapPiece>();
                Objects = new List<Part.Object>();
                Enemies = new List<Part.Enemy>();
                Players = new List<Part.Player>();
                Collisions = new List<Part.Collision>();
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
            /// Returns every part in the order they'll be written.
            /// </summary>
            public override List<Part> GetEntries()
            {
                return SFUtil.ConcatAll<Part>(
                    MapPieces, Objects, Enemies, Players, Collisions,
                    DummyObjects, DummyEnemies, ConnectCollisions);
            }
            IReadOnlyList<IMsbPart> IMsbParam<IMsbPart>.GetEntries() => GetEntries();

            internal override Part ReadEntry(BinaryReaderEx br)
            {
                PartsType type = br.GetEnum32<PartsType>(br.Position + 8);
                switch (type)
                {
                    case PartsType.MapPiece:
                        return MapPieces.EchoAdd(new Part.MapPiece(br));

                    case PartsType.Object:
                        return Objects.EchoAdd(new Part.Object(br));

                    case PartsType.Enemy:
                        return Enemies.EchoAdd(new Part.Enemy(br));

                    case PartsType.Player:
                        return Players.EchoAdd(new Part.Player(br));

                    case PartsType.Collision:
                        return Collisions.EchoAdd(new Part.Collision(br));

                    case PartsType.DummyObject:
                        return DummyObjects.EchoAdd(new Part.DummyObject(br));

                    case PartsType.DummyEnemy:
                        return DummyEnemies.EchoAdd(new Part.DummyEnemy(br));

                    case PartsType.ConnectCollision:
                        return ConnectCollisions.EchoAdd(new Part.ConnectCollision(br));

                    default:
                        throw new NotImplementedException($"Unsupported part type: {type}");
                }
            }
        }

        /// <summary>
        /// Any instance of some "thing" in a map.
        /// </summary>
        public abstract class Part : NamedEntry, IMsbPart
        {
            private protected abstract PartsType Type { get; }
            private protected abstract bool HasGparamConfig { get; }
            private protected abstract bool HasSceneGparamConfig { get; }

            /// <summary>
            /// The name of this part.
            /// </summary>
            public override string Name { get; set; }

            /// <summary>
            /// Unknown network path to a .sib file.
            /// </summary>
            public string SibPath { get; set; }

            /// <summary>
            /// The name of this part's model.
            /// </summary>
            public string ModelName { get; set; }
            public int ModelIndex { get; set; }

            /// <summary>
            /// The center of the part.
            /// </summary>
            public Vector3 Position { get; set; }

            /// <summary>
            /// The rotation of the part.
            /// </summary>
            public Vector3 Rotation { get; set; }

            /// <summary>
            /// The scale of the part, which only really works right for map pieces.
            /// </summary>
            public Vector3 Scale { get; set; }

            /// <summary>
            /// A bitmask that determines which ceremonies the part appears in.
            /// </summary>
            public uint MapStudioLayer { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public uint[] DrawGroups { get; private set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public uint[] DispGroups { get; private set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public uint[] BackreadGroups { get; private set; }

            /// <summary>
            /// Used to identify the part in event scripts.
            /// </summary>
            public int EntityID { get; set; }

            /// <summary>
            /// Used to identify multiple parts with the same ID in event scripts.
            /// </summary>
            public int[] EntityGroups { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public sbyte UnkE04 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public sbyte UnkE05 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public sbyte LanternID { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public sbyte LodParamID { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public sbyte UnkE0E { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public bool PointLightShadowSource { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public bool ShadowSource { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public bool ShadowDest { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public bool IsShadowOnly { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public bool DrawByReflectCam { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public bool DrawOnlyReflectCam { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public bool UseDepthBiasFloat { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public bool DisablePointLightEffect { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int UnkE18 { get; set; }

            private protected Part(string name)
            {
                Name = name;
                Scale = Vector3.One;
                DrawGroups = new uint[8] { 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF,
                    0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF };
                DispGroups = new uint[8] { 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF,
                    0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF };
                BackreadGroups = new uint[8] { 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF,
                    0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF };
                EntityID = -1;
                EntityGroups = new int[8] { -1, -1, -1, -1, -1, -1, -1, -1 };
            }

            /// <summary>
            /// Creates a deep copy of the part.
            /// </summary>
            public Part DeepCopy()
            {
                var part = (Part)MemberwiseClone();
                part.DrawGroups = (uint[])DrawGroups.Clone();
                part.DispGroups = (uint[])DispGroups.Clone();
                part.BackreadGroups = (uint[])BackreadGroups.Clone();
                part.EntityGroups = (int[])EntityGroups.Clone();
                DeepCopyTo(part);
                return part;
            }
            IMsbPart IMsbPart.DeepCopy() => DeepCopy();

            private protected virtual void DeepCopyTo(Part part) { }

            private protected Part(BinaryReaderEx br)
            {
                long start = br.Position;

                long nameOffset = br.ReadInt64();
                br.AssertUInt32((uint)Type);
                br.ReadInt32(); // ID
                ModelIndex = br.ReadInt32();
                br.AssertInt32(0);
                long sibOffset = br.ReadInt64();
                Position = br.ReadVector3();
                Rotation = br.ReadVector3();
                Scale = br.ReadVector3();

                br.AssertInt32(-1);
                MapStudioLayer = br.ReadUInt32();
                DrawGroups = br.ReadUInt32s(8);
                DispGroups = br.ReadUInt32s(8);
                BackreadGroups = br.ReadUInt32s(8);
                br.AssertInt32(0);

                long entityDataOffset = br.ReadInt64();
                long typeDataOffset = br.ReadInt64();
                long gparamOffset = br.ReadInt64();
                long sceneGparamOffset = br.ReadInt64();

                if (nameOffset == 0)
                    throw new InvalidDataException($"{nameof(nameOffset)} must not be 0 in type {GetType()}.");
                if (sibOffset == 0)
                    throw new InvalidDataException($"{nameof(sibOffset)} must not be 0 in type {GetType()}.");
                if (entityDataOffset == 0)
                    throw new InvalidDataException($"{nameof(entityDataOffset)} must not be 0 in type {GetType()}.");
                if (typeDataOffset == 0)
                    throw new InvalidDataException($"{nameof(typeDataOffset)} must not be 0 in type {GetType()}.");
                if (HasGparamConfig ^ gparamOffset != 0)
                    throw new InvalidDataException($"Unexpected {nameof(gparamOffset)} 0x{gparamOffset:X} in type {GetType()}.");
                if (HasSceneGparamConfig ^ sceneGparamOffset != 0)
                    throw new InvalidDataException($"Unexpected {nameof(sceneGparamOffset)} 0x{sceneGparamOffset:X} in type {GetType()}.");

                br.Position = start + nameOffset;
                Name = br.ReadUTF16();

                br.Position = start + sibOffset;
                SibPath = br.ReadUTF16();

                br.Position = start + entityDataOffset;
                ReadEntityData(br);

                br.Position = start + typeDataOffset;
                ReadTypeData(br);

                if (HasGparamConfig)
                {
                    br.Position = start + gparamOffset;
                    ReadGparamConfig(br);
                }

                if (HasSceneGparamConfig)
                {
                    br.Position = start + sceneGparamOffset;
                    ReadSceneGparamConfig(br);
                }
            }

            private void ReadEntityData(BinaryReaderEx br)
            {
                EntityID = br.ReadInt32();
                UnkE04 = br.ReadSByte();
                UnkE05 = br.ReadSByte();
                br.AssertInt16(0);
                br.AssertInt32(0);
                LanternID = br.ReadSByte();
                LodParamID = br.ReadSByte();
                UnkE0E = br.ReadSByte();
                PointLightShadowSource = br.ReadBoolean();
                ShadowSource = br.ReadBoolean();
                ShadowDest = br.ReadBoolean();
                IsShadowOnly = br.ReadBoolean();
                DrawByReflectCam = br.ReadBoolean();
                DrawOnlyReflectCam = br.ReadBoolean();
                UseDepthBiasFloat = br.ReadBoolean();
                DisablePointLightEffect = br.ReadBoolean();
                br.AssertByte(0);
                UnkE18 = br.ReadInt32();
                EntityGroups = br.ReadInt32s(8);
                br.AssertInt32(0);
            }

            private protected abstract void ReadTypeData(BinaryReaderEx br);

            private protected virtual void ReadGparamConfig(BinaryReaderEx br)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(ReadGparamConfig)}.");

            private protected virtual void ReadSceneGparamConfig(BinaryReaderEx br)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(ReadSceneGparamConfig)}.");

            internal override void Write(BinaryWriterEx bw, int id)
            {
                long start = bw.Position;

                bw.ReserveInt64("NameOffset");
                bw.WriteUInt32((uint)Type);
                bw.WriteInt32(id);
                bw.WriteInt32(ModelIndex);
                bw.WriteInt32(0);
                bw.ReserveInt64("SibOffset");
                bw.WriteVector3(Position);
                bw.WriteVector3(Rotation);
                bw.WriteVector3(Scale);

                bw.WriteInt32(-1);
                bw.WriteUInt32(MapStudioLayer);
                bw.WriteUInt32s(DrawGroups);
                bw.WriteUInt32s(DispGroups);
                bw.WriteUInt32s(BackreadGroups);
                bw.WriteInt32(0);

                bw.ReserveInt64("EntityDataOffset");
                bw.ReserveInt64("TypeDataOffset");
                bw.ReserveInt64("GparamOffset");
                bw.ReserveInt64("SceneGparamOffset");

                bw.FillInt64("NameOffset", bw.Position - start);
                bw.WriteUTF16(MSB.ReambiguateName(Name), true);

                bw.FillInt64("SibOffset", bw.Position - start);
                bw.WriteUTF16(SibPath, true);
                // This is purely here for byte-perfect writes because From is nasty
                if (SibPath == "")
                    bw.WritePattern(0x24, 0x00);
                bw.Pad(8);

                bw.FillInt64("EntityDataOffset", bw.Position - start);
                WriteEntityData(bw);

                bw.FillInt64("TypeDataOffset", bw.Position - start);
                WriteTypeData(bw);

                if (HasGparamConfig)
                {
                    bw.FillInt64("GparamOffset", bw.Position - start);
                    WriteGparamConfig(bw);
                }
                else
                {
                    bw.FillInt64("GparamOffset", 0);
                }

                if (HasSceneGparamConfig)
                {
                    bw.FillInt64("SceneGparamOffset", bw.Position - start);
                    WriteSceneGparamConfig(bw);
                }
                else
                {
                    bw.FillInt64("SceneGparamOffset", 0);
                }
            }

            private void WriteEntityData(BinaryWriterEx bw)
            {
                bw.WriteInt32(EntityID);

                bw.WriteSByte(UnkE04);
                bw.WriteSByte(UnkE05);
                bw.WriteInt16(0);

                bw.WriteInt32(0);

                bw.WriteSByte(LanternID);
                bw.WriteSByte(LodParamID);
                bw.WriteSByte(UnkE0E);
                bw.WriteBoolean(PointLightShadowSource);

                bw.WriteBoolean(ShadowSource);
                bw.WriteBoolean(ShadowDest);
                bw.WriteBoolean(IsShadowOnly);
                bw.WriteBoolean(DrawByReflectCam);

                bw.WriteBoolean(DrawOnlyReflectCam);
                bw.WriteBoolean(UseDepthBiasFloat);
                bw.WriteBoolean(DisablePointLightEffect);
                bw.WriteByte(0);

                bw.WriteInt32(UnkE18);
                bw.WriteInt32s(EntityGroups);
                bw.WriteInt32(0);
                bw.Pad(8);
            }

            private protected abstract void WriteTypeData(BinaryWriterEx bw);

            private protected virtual void WriteGparamConfig(BinaryWriterEx bw)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(WriteGparamConfig)}.");

            private protected virtual void WriteSceneGparamConfig(BinaryWriterEx bw)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(WriteSceneGparamConfig)}.");

            internal virtual void GetNames(MSB3 msb, Entries entries)
            {
                ModelName = MSB.FindName(entries.Models, ModelIndex);
            }

            internal virtual void GetIndices(MSB3 msb, Entries entries)
            {
                ModelIndex = MSB.FindIndex(this, entries.Models, ModelName);
            }

            /// <summary>
            /// Returns the type and name of this part.
            /// </summary>
            public override string ToString()
            {
                return $"{Type} : {Name}";
            }

            /// <summary>
            /// Gparam value IDs for various part types.
            /// </summary>
            public class GparamConfig
            {
                /// <summary>
                /// ID of the value set from LightSet ParamEditor to use.
                /// </summary>
                public int LightSetID { get; set; }

                /// <summary>
                /// ID of the value set from FogParamEditor to use.
                /// </summary>
                public int FogParamID { get; set; }

                /// <summary>
                /// ID of the value set from LightScattering : ParamEditor to use.
                /// </summary>
                public int LightScatteringID { get; set; }

                /// <summary>
                /// ID of the value set from Env Map:Editor to use.
                /// </summary>
                public int EnvMapID { get; set; }

                /// <summary>
                /// Creates a GparamConfig with default values.
                /// </summary>
                public GparamConfig() { }

                /// <summary>
                /// Creates a deep copy of the gparam config.
                /// </summary>
                public GparamConfig DeepCopy()
                {
                    return (GparamConfig)MemberwiseClone();
                }

                internal GparamConfig(BinaryReaderEx br)
                {
                    LightSetID = br.ReadInt32();
                    FogParamID = br.ReadInt32();
                    LightScatteringID = br.ReadInt32();
                    EnvMapID = br.ReadInt32();
                    br.AssertPattern(0x10, 0x00);
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteInt32(LightSetID);
                    bw.WriteInt32(FogParamID);
                    bw.WriteInt32(LightScatteringID);
                    bw.WriteInt32(EnvMapID);
                    bw.WritePattern(0x10, 0x00);
                }

                /// <summary>
                /// Returns the four gparam values as a string.
                /// </summary>
                public override string ToString()
                {
                    return $"{LightSetID}, {FogParamID}, {LightScatteringID}, {EnvMapID}";
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class SceneGparamConfig
            {
                /// <summary>
                /// Unknown.
                /// </summary>
                public int ShadowParamID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int DofGlareQualityID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int ToneMapID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int DofID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int BloomID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int ColorGradingID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int FeedbackBlurID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int MotionBlurID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int AntiAliasID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int SSAO_ID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int LightShaftID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public sbyte[] EventIDs { get; private set; }

                /// <summary>
                /// Amount of time it takes for GParam to transition (in seconds). -1 = Some default time.
                /// </summary>
                public float TransitionTime { get; set; }

                /// <summary>
                /// Creates a SceneGparamConfig with default values.
                /// </summary>
                public SceneGparamConfig()
                {
                    EventIDs = new sbyte[4];
                }

                /// <summary>
                /// Creates a deep copy of the scene gparam config.
                /// </summary>
                public SceneGparamConfig DeepCopy()
                {
                    var config = (SceneGparamConfig)MemberwiseClone();
                    config.EventIDs = (sbyte[])EventIDs.Clone();
                    return config;
                }

                internal SceneGparamConfig(BinaryReaderEx br)
                {
                    ShadowParamID = br.ReadInt32();
                    DofGlareQualityID = br.ReadInt32();
                    ToneMapID = br.ReadInt32();
                    DofID = br.ReadInt32();
                    BloomID = br.ReadInt32();
                    ColorGradingID = br.ReadInt32();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    FeedbackBlurID = br.ReadInt32();
                    MotionBlurID = br.ReadInt32();
                    AntiAliasID = br.ReadInt32();
                    SSAO_ID = br.ReadInt32();
                    LightShaftID = br.ReadInt32();
                    EventIDs = br.ReadSBytes(4);
                    TransitionTime = br.ReadSingle();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteInt32(ShadowParamID);
                    bw.WriteInt32(DofGlareQualityID);
                    bw.WriteInt32(ToneMapID);
                    bw.WriteInt32(DofID);
                    bw.WriteInt32(BloomID);
                    bw.WriteInt32(ColorGradingID);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(FeedbackBlurID);
                    bw.WriteInt32(MotionBlurID);
                    bw.WriteInt32(AntiAliasID);
                    bw.WriteInt32(SSAO_ID);
                    bw.WriteInt32(LightShaftID);
                    bw.WriteSBytes(EventIDs);
                    bw.WriteSingle(TransitionTime);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// A static model making up the map.
            /// </summary>
            public class MapPiece : Part
            {
                private protected override PartsType Type => PartsType.MapPiece;
                private protected override bool HasGparamConfig => true;
                private protected override bool HasSceneGparamConfig => false;

                /// <summary>
                /// Gparam IDs for this map piece.
                /// </summary>
                public GparamConfig GparamConfig { get; set; }

                /// <summary>
                /// Creates a MapPiece with default values.
                /// </summary>
                public MapPiece() : base("mXXXXXX_XXXX")
                {
                    GparamConfig = new GparamConfig();
                }

                private protected override void DeepCopyTo(Part part)
                {
                    var piece = (MapPiece)part;
                    piece.GparamConfig = GparamConfig.DeepCopy();
                }

                internal MapPiece(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                private protected override void ReadGparamConfig(BinaryReaderEx br) => GparamConfig = new GparamConfig(br);

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }

                private protected override void WriteGparamConfig(BinaryWriterEx bw) => GparamConfig.Write(bw);
            }

            /// <summary>
            /// Common base data for objects and dummy objects.
            /// </summary>
            public abstract class ObjectBase : Part
            {
                private protected override bool HasGparamConfig => true;
                private protected override bool HasSceneGparamConfig => false;

                /// <summary>
                /// Gparam IDs for this object.
                /// </summary>
                public GparamConfig GparamConfig { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Collision))]
                [NoRenderGroupInheritence()]
                public string CollisionName { get; set; }
                public int CollisionPartIndex { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte BreakTerm { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public bool NetSyncType { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public bool CollisionFilter { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public bool SetMainObjStructureBooleans { get; set; }

                /// <summary>
                /// Automatically playing animations; only the first is actually used, according to Pav.
                /// </summary>
                public short[] AnimIDs { get; private set; }

                /// <summary>
                /// Value added to the base ModelSfxParam ID; only the first is actually used, according to Pav.
                /// </summary>
                public short[] ModelSfxParamRelativeIDs { get; private set; }

                private protected ObjectBase() : base("oXXXXXX_XXXX")
                {
                    GparamConfig = new GparamConfig();
                    AnimIDs = new short[4] { -1, -1, -1, -1 };
                    ModelSfxParamRelativeIDs = new short[4] { -1, -1, -1, -1 };
                }

                private protected override void DeepCopyTo(Part part)
                {
                    var obj = (ObjectBase)part;
                    obj.GparamConfig = GparamConfig.DeepCopy();
                    obj.AnimIDs = (short[])AnimIDs.Clone();
                    obj.ModelSfxParamRelativeIDs = (short[])ModelSfxParamRelativeIDs.Clone();
                }

                private protected ObjectBase(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    CollisionPartIndex = br.ReadInt32();
                    BreakTerm = br.ReadByte();
                    NetSyncType = br.ReadBoolean();
                    CollisionFilter = br.ReadBoolean();
                    SetMainObjStructureBooleans = br.ReadBoolean();
                    AnimIDs = br.ReadInt16s(4);
                    ModelSfxParamRelativeIDs = br.ReadInt16s(4);
                }

                private protected override void ReadGparamConfig(BinaryReaderEx br) => GparamConfig = new GparamConfig(br);

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(CollisionPartIndex);
                    bw.WriteByte(BreakTerm);
                    bw.WriteBoolean(NetSyncType);
                    bw.WriteBoolean(CollisionFilter);
                    bw.WriteBoolean(SetMainObjStructureBooleans);
                    bw.WriteInt16s(AnimIDs);
                    bw.WriteInt16s(ModelSfxParamRelativeIDs);
                }

                private protected override void WriteGparamConfig(BinaryWriterEx bw) => GparamConfig.Write(bw);

                internal override void GetNames(MSB3 msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    CollisionName = MSB.FindName(entries.Parts, CollisionPartIndex);
                }

                internal override void GetIndices(MSB3 msb, Entries entries)
                {
                    base.GetIndices(msb, entries);
                    CollisionPartIndex = MSB.FindIndex(this, entries.Parts, CollisionName);
                }
            }

            /// <summary>
            /// Any dynamic object such as elevators, crates, ladders, etc.
            /// </summary>
            public class Object : ObjectBase
            {
                private protected override PartsType Type => PartsType.Object;

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
                private protected override bool HasGparamConfig => true;
                private protected override bool HasSceneGparamConfig => false;

                /// <summary>
                /// Gparam IDs for this enemy.
                /// </summary>
                public GparamConfig GparamConfig { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Collision))]
                public string CollisionName { get; set; }
                public int CollisionPartIndex { get; set; }

                /// <summary>
                /// Controls enemy AI.
                /// </summary>
                public int ThinkParamID { get; set; }

                /// <summary>
                /// Controls enemy stats.
                /// </summary>
                public int NPCParamID { get; set; }

                /// <summary>
                /// Controls enemy speech.
                /// </summary>
                public int TalkID { get; set; }

                /// <summary>
                /// Controls enemy equipment.
                /// </summary>
                public int CharaInitID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte PointMoveType { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short PlatoonID { get; set; }

                /// <summary>
                /// Walk route followed by this enemy.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Event.PatrolInfo))]
                public string WalkRouteName { get; set; }
                public short WalkRouteIndex { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int BackupEventAnimID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT78 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT84 { get; set; }

                private protected EnemyBase() : base("cXXXX_XXXX")
                {
                    GparamConfig = new GparamConfig();
                }

                private protected override void DeepCopyTo(Part part)
                {
                    var enemy = (EnemyBase)part;
                    enemy.GparamConfig = GparamConfig.DeepCopy();
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
                    PlatoonID = br.ReadInt16();
                    CharaInitID = br.ReadInt32();
                    CollisionPartIndex = br.ReadInt32();
                    WalkRouteIndex = br.ReadInt16();
                    br.AssertInt16(0);
                    br.AssertInt32(0);
                    br.AssertInt32(-1);
                    br.AssertInt32(-1);
                    br.AssertInt32(-1);
                    br.AssertInt32(-1);
                    BackupEventAnimID = br.ReadInt32();
                    br.AssertInt32(-1); // BackupThrowAnimID
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    UnkT78 = br.ReadInt32();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    UnkT84 = br.ReadSingle();
                    for (int i = 0; i < 5; i++)
                    {
                        br.AssertInt32(-1);
                        br.AssertInt16(-1);
                        br.AssertInt16(0xA);
                    }
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                private protected override void ReadGparamConfig(BinaryReaderEx br) => GparamConfig = new GparamConfig(br);

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(ThinkParamID);
                    bw.WriteInt32(NPCParamID);
                    bw.WriteInt32(TalkID);
                    bw.WriteByte(PointMoveType);
                    bw.WriteByte(0);
                    bw.WriteInt16(PlatoonID);
                    bw.WriteInt32(CharaInitID);
                    bw.WriteInt32(CollisionPartIndex);
                    bw.WriteInt16(WalkRouteIndex);
                    bw.WriteInt16(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(BackupEventAnimID);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(UnkT78);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteSingle(UnkT84);
                    for (int i = 0; i < 5; i++)
                    {
                        bw.WriteInt32(-1);
                        bw.WriteInt16(-1);
                        bw.WriteInt16(0xA);
                    }
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }

                private protected override void WriteGparamConfig(BinaryWriterEx bw) => GparamConfig.Write(bw);

                internal override void GetNames(MSB3 msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    CollisionName = MSB.FindName(entries.Parts, CollisionPartIndex);
                    WalkRouteName = MSB.FindName(msb.Events.PatrolInfo, WalkRouteIndex);
                }

                internal override void GetIndices(MSB3 msb, Entries entries)
                {
                    base.GetIndices(msb, entries);
                    CollisionPartIndex = MSB.FindIndex(this, entries.Parts, CollisionName);
                    WalkRouteIndex = (short)MSB.FindIndex(this, msb.Events.PatrolInfo, WalkRouteName);
                }
            }

            /// <summary>
            /// Any non-player character, not necessarily hostile.
            /// </summary>
            public class Enemy : EnemyBase
            {
                private protected override PartsType Type => PartsType.Enemy;

                /// <summary>
                /// Creates an Enemy with default values.
                /// </summary>
                public Enemy() : base() { }

                internal Enemy(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// A player spawn point.
            /// </summary>
            public class Player : Part
            {
                private protected override PartsType Type => PartsType.Player;
                private protected override bool HasGparamConfig => false;
                private protected override bool HasSceneGparamConfig => false;

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
            /// An invisible collision mesh, also used for death planes.
            /// </summary>
            public class Collision : Part
            {
                private protected override PartsType Type => PartsType.Collision;
                private protected override bool HasGparamConfig => true;
                private protected override bool HasSceneGparamConfig => true;

                /// <summary>
                /// Gparam IDs for this collision.
                /// </summary>
                public GparamConfig GparamConfig { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public SceneGparamConfig SceneGparamConfig { get; set; }

                /// <summary>
                /// Sets collision behavior. Fall collision, death collision, enemy-only collision, etc.
                /// </summary>
                public byte HitFilterID { get; set; } = 8;

                /// <summary>
                /// Modifies sounds while the player is touching this collision.
                /// </summary>
                public byte SoundSpaceType { get; set; } = 0;

                /// <summary>
                /// Unknown.
                /// </summary>
                public short EnvLightMapSpotIndex { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float ReflectPlaneHeight { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short MapNameID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public bool DisableStart { get; set; }

                /// <summary>
                /// Disables a bonfire with this entity ID when an enemy is touching this collision.
                /// </summary>
                public int DisableBonfireEntityID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int PlayRegionID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short LockCamID1 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short LockCamID2 { get; set; }

                /// <summary>
                /// Unknown. Always refers to another collision part.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Collision))]
                public string UnkHitName { get; set; }
                public int UnkHitIndex { get; set; }

                /// <summary>
                /// ID in MapMimicryEstablishmentParam.
                /// </summary>
                public int ChameleonParamID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT34 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT35 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT36 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte MapVisType { get; set; } = 0;

                /// <summary>
                /// Creates a Collision with default values.
                /// </summary>
                public Collision() : base("hXXXXXX")
                {
                    GparamConfig = new GparamConfig();
                    SceneGparamConfig = new SceneGparamConfig();
                    SoundSpaceType = 0;
                    MapNameID = -1;
                    DisableStart = false;
                    DisableBonfireEntityID = -1;
                    MapVisType = 0;
                    PlayRegionID = -1;
                }

                private protected override void DeepCopyTo(Part part)
                {
                    var collision = (Collision)part;
                    collision.GparamConfig = GparamConfig.DeepCopy();
                    collision.SceneGparamConfig = SceneGparamConfig.DeepCopy();
                }

                internal Collision(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    HitFilterID = br.ReadByte();
                    SoundSpaceType = br.ReadByte();
                    EnvLightMapSpotIndex = br.ReadInt16();
                    ReflectPlaneHeight = br.ReadSingle();
                    br.AssertInt32(0); // Navmesh Group (4)
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(-1); // Vagrant Entity ID (3)
                    br.AssertInt32(-1);
                    br.AssertInt32(-1);
                    MapNameID = br.ReadInt16();
                    DisableStart = br.ReadBoolean();
                    br.AssertByte(0);
                    DisableBonfireEntityID = br.ReadInt32();
                    ChameleonParamID = br.ReadInt32();
                    UnkHitIndex = br.ReadInt32();
                    UnkT34 = br.ReadByte();
                    UnkT35 = br.ReadByte();
                    UnkT36 = br.ReadByte();
                    MapVisType = br.ReadByte();
                    PlayRegionID = br.ReadInt32();
                    LockCamID1 = br.ReadInt16();
                    LockCamID2 = br.ReadInt16();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                private protected override void ReadGparamConfig(BinaryReaderEx br) => GparamConfig = new GparamConfig(br);
                private protected override void ReadSceneGparamConfig(BinaryReaderEx br) => SceneGparamConfig = new SceneGparamConfig(br);

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteByte((byte)HitFilterID);
                    bw.WriteByte((byte)SoundSpaceType);
                    bw.WriteInt16(EnvLightMapSpotIndex);
                    bw.WriteSingle(ReflectPlaneHeight);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt16(MapNameID);
                    bw.WriteBoolean(DisableStart);
                    bw.WriteByte(0);
                    bw.WriteInt32(DisableBonfireEntityID);
                    bw.WriteInt32(ChameleonParamID);
                    bw.WriteInt32(UnkHitIndex);
                    bw.WriteByte(UnkT34);
                    bw.WriteByte(UnkT35);
                    bw.WriteByte(UnkT36);
                    bw.WriteByte((byte)MapVisType);
                    bw.WriteInt32(PlayRegionID);
                    bw.WriteInt16(LockCamID1);
                    bw.WriteInt16(LockCamID2);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }

                private protected override void WriteGparamConfig(BinaryWriterEx bw) => GparamConfig.Write(bw);
                private protected override void WriteSceneGparamConfig(BinaryWriterEx bw) => SceneGparamConfig.Write(bw);

                internal override void GetNames(MSB3 msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    UnkHitName = MSB.FindName(entries.Parts, UnkHitIndex);
                }

                internal override void GetIndices(MSB3 msb, Entries entries)
                {
                    base.GetIndices(msb, entries);
                    UnkHitIndex = MSB.FindIndex(this, entries.Parts, UnkHitName);
                }
            }

            /// <summary>
            /// An object that is either unused, or used for a cutscene.
            /// </summary>
            public class DummyObject : ObjectBase
            {
                private protected override PartsType Type => PartsType.DummyObject;

                /// <summary>
                /// Creates a DummyObject with default values.
                /// </summary>
                public DummyObject() : base() { }

                internal DummyObject(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// An enemy that is either unused, or used for a cutscene.
            /// </summary>
            public class DummyEnemy : EnemyBase
            {
                private protected override PartsType Type => PartsType.DummyEnemy;

                /// <summary>
                /// Creates a DummyEnemy with default values.
                /// </summary>
                public DummyEnemy() : base() { }

                internal DummyEnemy(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// Determines which collision parts load other maps.
            /// </summary>
            public class ConnectCollision : Part
            {
                private protected override PartsType Type => PartsType.ConnectCollision;
                private protected override bool HasGparamConfig => false;
                private protected override bool HasSceneGparamConfig => false;

                /// <summary>
                /// The name of the associated collision part.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Collision))]
                public string CollisionName { get; set; }
                public int CollisionIndex { get; set; }

                /// <summary>
                /// The map to load when on this collision.
                /// </summary>
                public byte[] MapID { get; private set; }

                /// <summary>
                /// Creates a new ConnectCollision with default values.
                /// </summary>
                public ConnectCollision() : base("hXXXXXX_XXXX")
                {
                    MapID = new byte[4];
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

                internal override void GetNames(MSB3 msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    CollisionName = MSB.FindName(msb.Parts.Collisions, CollisionIndex);
                }

                internal override void GetIndices(MSB3 msb, Entries entries)
                {
                    base.GetIndices(msb, entries);
                    CollisionIndex = MSB.FindIndex(this, msb.Parts.Collisions, CollisionName);
                }
            }
        }
    }
}
