using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace SoulsFormats
{
    public partial class MSBB
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
            Other = 0xFFFFFFFF,
        }

        /// <summary>
        /// All instances of concrete things in the map.
        /// </summary>
        public class PartsParam : Param<Part>, IMsbParam<IMsbPart>
        {
            internal override int Version => 3;
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
            /// Unknown.
            /// </summary>
            public List<Part.Other> Others { get; set; }

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
                Others = new List<Part.Other>();
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
                    case Part.Other p: Others.Add(p); break;

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
                    Navmeshes, DummyObjects, DummyEnemies, ConnectCollisions, Others);
            }
            IReadOnlyList<IMsbPart> IMsbParam<IMsbPart>.GetEntries() => GetEntries();

            internal override Part ReadEntry(BinaryReaderEx br)
            {
                PartType type = br.GetEnum32<PartType>(br.Position + 0x14);
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

                    case PartType.Other:
                        return Others.EchoAdd(new Part.Other(br));

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
            private protected abstract bool HasTypeData { get; }
            private protected abstract bool HasGparamConfig { get; }
            private protected abstract bool HasSceneGparamConfig { get; }

            /// <summary>
            /// A description of the part, usually left blank.
            /// </summary>
            public string Description { get; set; }

            /// <summary>
            /// The name of the part.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Unknown; appears to count up with each instance of a model that was added.
            /// </summary>
            public int InstanceID { get; set; }

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
            /// Unknown.
            /// </summary>
            public uint[] BackreadGroups { get; private set; }

            /// <summary>
            /// Identifies the part in external files.
            /// </summary>
            public int EntityID { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int ResidentFXParamID { get; set; }

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
            /// Is a BitFlag.
            /// </summary>
            public bool PointLightShadowSource { get; set; }

            /// <summary>
            /// Unknown.
            /// Is a BitFlag.
            /// </summary>
            public bool ShadowSource { get; set; }

            /// <summary>
            /// Unknown.
            /// Is a BitFlag.
            /// </summary>
            public bool ShadowDest { get; set; }

            /// <summary>
            /// Unknown.
            /// Is a BitFlag.
            /// </summary>
            public bool IsShadowOnly { get; set; }

            /// <summary>
            /// Unknown.
            /// Is a BitFlag.
            /// </summary>
            public bool DrawByReflectCam { get; set; }

            /// <summary>
            /// Unknown.
            /// Is a BitFlag.
            /// </summary>
            public bool DrawOnlyReflectCam { get; set; }

            /// <summary>
            /// Unknown.
            /// Is a BitFlag.
            /// </summary>
            public bool UseDepthBiasFloat { get; set; }

            /// <summary>
            /// Unknown.
            /// Is a BitFlag.
            /// </summary>
            public bool DisablePointLightEffect { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte UnkE0F { get; set; }

            private protected Part(string name)
            {
                Description = "";
                Name = name;
                SibPath = "";
                Scale = Vector3.One;
                DrawGroups = new uint[8] {
                    0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF };
                DispGroups = new uint[8] {
                    0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF };
                BackreadGroups = new uint[8] {
                    0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF };
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
                part.BackreadGroups = (uint[])BackreadGroups.Clone();
                DeepCopyTo(part);
                return part;
            }
            IMsbPart IMsbPart.DeepCopy() => DeepCopy();

            private protected virtual void DeepCopyTo(Part part) { }

            private protected Part(BinaryReaderEx br)
            {
                long start = br.Position;
                long descOffset = br.ReadInt64();
                long nameOffset = br.ReadInt64();
                InstanceID = br.ReadInt32();
                br.AssertUInt32((uint)Type);
                br.ReadInt32(); // ID
                ModelIndex = br.ReadInt32();
                long sibOffset = br.ReadInt64();
                Position = br.ReadVector3();
                Rotation = br.ReadVector3();
                Scale = br.ReadVector3();
                DrawGroups = br.ReadUInt32s(8);
                DispGroups = br.ReadUInt32s(8);
                BackreadGroups = br.ReadUInt32s(8);
                br.AssertInt32(0);
                long entityDataOffset = br.ReadInt64();
                long typeDataOffset = br.ReadInt64();
                long gparamOffset = br.ReadInt64();
                long sceneGparamOffset = br.ReadInt64();

                if (descOffset == 0)
                    throw new InvalidDataException($"{nameof(descOffset)} must not be 0 in type {GetType()}.");
                if (nameOffset == 0)
                    throw new InvalidDataException($"{nameof(nameOffset)} must not be 0 in type {GetType()}.");
                if (sibOffset == 0)
                    throw new InvalidDataException($"{nameof(sibOffset)} must not be 0 in type {GetType()}.");
                if (entityDataOffset == 0)
                    throw new InvalidDataException($"{nameof(entityDataOffset)} must not be 0 in type {GetType()}.");
                if (HasTypeData ^ typeDataOffset != 0)
                    throw new InvalidDataException($"Unexpected {nameof(typeDataOffset)} 0x{typeDataOffset:X} in type {GetType()}.");
                if (HasGparamConfig ^ gparamOffset != 0)
                    throw new InvalidDataException($"Unexpected {nameof(gparamOffset)} 0x{gparamOffset:X} in type {GetType()}.");
                if (HasSceneGparamConfig ^ sceneGparamOffset != 0)
                    throw new InvalidDataException($"Unexpected {nameof(sceneGparamOffset)} 0x{sceneGparamOffset:X} in type {GetType()}.");

                br.Position = start + descOffset;
                Description = br.ReadUTF16();

                br.Position = start + nameOffset;
                Name = br.ReadUTF16();

                br.Position = start + sibOffset;
                SibPath = br.ReadUTF16();

                br.Position = start + entityDataOffset;
                ReadEntityData(br);

                if (HasTypeData)
                {
                    br.Position = start + typeDataOffset;
                    ReadTypeData(br);
                }

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
                ResidentFXParamID = br.ReadInt32();
                br.AssertInt32(0);
                LanternID = br.ReadByte();
                LodParamID = br.ReadByte();

                byte VisualConfig = br.ReadByte();
                PointLightShadowSource = (VisualConfig & (1 << 0)) != 0;
                ShadowSource = (VisualConfig & (1 << 1)) != 0;
                ShadowDest = (VisualConfig & (1 << 2)) != 0;
                IsShadowOnly = (VisualConfig & (1 << 3)) != 0;
                DrawByReflectCam = (VisualConfig & (1 << 4)) != 0;
                DrawOnlyReflectCam = (VisualConfig & (1 << 5)) != 0;
                UseDepthBiasFloat = (VisualConfig & (1 << 6)) != 0;
                DisablePointLightEffect = (VisualConfig & (1 << 7)) != 0;

                UnkE0F = br.ReadByte();
            }

            private protected virtual void ReadTypeData(BinaryReaderEx br)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(ReadTypeData)}.");

            private protected virtual void ReadGparamConfig(BinaryReaderEx br)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(ReadGparamConfig)}.");

            private protected virtual void ReadSceneGparamConfig(BinaryReaderEx br)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(ReadSceneGparamConfig)}.");

            internal override void Write(BinaryWriterEx bw, int id)
            {
                long start = bw.Position;
                bw.ReserveInt64("DescOffset");
                bw.ReserveInt64("NameOffset");
                bw.WriteInt32(InstanceID);
                bw.WriteUInt32((uint)Type);
                bw.WriteInt32(Type == PartType.Other ? 0 : id);
                bw.WriteInt32(ModelIndex);
                bw.ReserveInt64("SibOffset");
                bw.WriteVector3(Position);
                bw.WriteVector3(Rotation);
                bw.WriteVector3(Scale);
                bw.WriteUInt32s(DrawGroups);
                bw.WriteUInt32s(DispGroups);
                bw.WriteUInt32s(BackreadGroups);
                bw.WriteInt32(0);
                bw.ReserveInt64("EntityDataOffset");
                bw.ReserveInt64("TypeDataOffset");
                bw.ReserveInt64("GparamOffset");
                bw.ReserveInt64("SceneGparamOffset");

                long stringsStart = bw.Position;
                bw.FillInt64("DescOffset", bw.Position - start);
                bw.WriteUTF16(Description, true);

                bw.FillInt64("NameOffset", bw.Position - start);
                bw.WriteUTF16(MSB.ReambiguateName(Name), true);

                bw.FillInt64("SibOffset", bw.Position - start);
                bw.WriteUTF16(SibPath, true);
                if (bw.Position - stringsStart <= 0x38)
                    bw.WritePattern(0x3C - (int)(bw.Position - stringsStart), 0x00);
                else
                    bw.Pad(8);

                bw.FillInt64("EntityDataOffset", bw.Position - start);
                WriteEntityData(bw);
                if (Type != PartType.MapPiece)
                    bw.Pad(8);

                if (HasTypeData)
                {
                    bw.FillInt64("TypeDataOffset", bw.Position - start);
                    WriteTypeData(bw);
                }
                else
                {
                    bw.FillInt64("TypeDataOffset", 0);
                }
                bw.Pad(8);

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
                bw.WriteInt32(ResidentFXParamID);

                bw.WriteInt32(0);

                bw.WriteByte(LanternID);
                bw.WriteByte(LodParamID);

                byte VisualConfig = 0;
                if (PointLightShadowSource)
                    VisualConfig |= 1 << 0;
                if (ShadowSource)
                    VisualConfig |= 1 << 1;
                if (ShadowDest)
                    VisualConfig |= 1 << 2;
                if (IsShadowOnly)
                    VisualConfig |= 1 << 3;
                if (DrawByReflectCam)
                    VisualConfig |= 1 << 4;
                if (DrawOnlyReflectCam)
                    VisualConfig |= 1 << 5;
                if (UseDepthBiasFloat)
                    VisualConfig |= 1 << 6;
                if (DisablePointLightEffect)
                    VisualConfig |= 1 << 7;
                bw.WriteByte(VisualConfig);

                bw.WriteByte(UnkE0F);
            }

            private protected virtual void WriteTypeData(BinaryWriterEx bw)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(WriteTypeData)}.");

            private protected virtual void WriteGparamConfig(BinaryWriterEx bw)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(WriteGparamConfig)}.");

            private protected virtual void WriteSceneGparamConfig(BinaryWriterEx bw)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(WriteSceneGparamConfig)}.");

            internal virtual void GetNames(MSBB msb, Entries entries)
            {
                ModelName = MSB.FindName(entries.Models, ModelIndex);
            }

            internal virtual void GetIndices(MSBB msb, Entries entries)
            {
                ModelIndex = MSB.FindIndex(this, entries.Models, ModelName);
            }

            /// <summary>
            /// Returns a string representation of the part.
            /// </summary>
            public override string ToString()
            {
                if (Description == "")
                    return $"{Type} {Name}";
                else
                    return $"{Type} {Name} - {Description}";
            }

            /// <summary>
            /// Gparam value IDs for various part types.
            /// </summary>
            public class GparamConfigStruct
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
                public GparamConfigStruct() { }

                /// <summary>
                /// Creates a deep copy of the gparam config.
                /// </summary>
                public GparamConfigStruct DeepCopy()
                {
                    return (GparamConfigStruct)MemberwiseClone();
                }

                internal GparamConfigStruct(BinaryReaderEx br)
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
            public class SceneGparamConfigStruct
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
                public SceneGparamConfigStruct()
                {
                    EventIDs = new sbyte[4];
                }

                /// <summary>
                /// Creates a deep copy of the scene gparam config.
                /// </summary>
                public SceneGparamConfigStruct DeepCopy()
                {
                    var config = (SceneGparamConfigStruct)MemberwiseClone();
                    config.EventIDs = (sbyte[])EventIDs.Clone();
                    return config;
                }

                internal SceneGparamConfigStruct(BinaryReaderEx br)
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
            /// A visible but not physical model making up the map.
            /// </summary>
            public class MapPiece : Part
            {
                private protected override PartType Type => PartType.MapPiece;
                private protected override bool HasTypeData => true;
                private protected override bool HasGparamConfig => true;
                private protected override bool HasSceneGparamConfig => false;

                /// <summary>
                /// Gparam IDs for this map piece.
                /// </summary>
                public GparamConfigStruct GparamConfig { get; set; }

                /// <summary>
                /// Creates a MapPiece with default values.
                /// </summary>
                public MapPiece() : base("mXXXXXX_XXXX")
                {
                    GparamConfig = new GparamConfigStruct();
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

                private protected override void ReadGparamConfig(BinaryReaderEx br) => GparamConfig = new GparamConfigStruct(br);

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
                private protected override bool HasTypeData => true;
                private protected override bool HasGparamConfig => true;
                private protected override bool HasSceneGparamConfig => false;

                /// <summary>
                /// Gparam IDs for this object.
                /// </summary>
                public GparamConfigStruct GparamConfigStruct { get; set; }

                /// <summary>
                /// Collision that controls loading of the object.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Collision))]
                [NoRenderGroupInheritence()]
                public string CollisionName { get; set; }
                private int CollisionIndex;

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
                /// Something to do with breakobj functionality. Possibly decides which part of a shared-map (chalice) .breakobj file to use.
                /// </summary>
                public int[] BreakobjID { get; private set; }

                private protected ObjectBase() : base("oXXXXXX_XXXX")
                {
                    GparamConfigStruct = new GparamConfigStruct();
                    AnimIDs = new short[4] { -1, -1, -1, -1 };
                    BreakobjID = new int[2];
                }

                private protected override void DeepCopyTo(Part part)
                {
                    var obj = (ObjectBase)part;
                    obj.GparamConfigStruct = GparamConfigStruct.DeepCopy();
                    obj.AnimIDs = (short[])AnimIDs.Clone();
                    obj.BreakobjID = (int[])BreakobjID.Clone();
                }

                private protected ObjectBase(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    CollisionIndex = br.ReadInt32();
                    BreakTerm = br.ReadSByte();
                    NetSyncType = br.ReadSByte();
                    CollisionFilter = br.ReadBoolean();
                    SetMainObjStructureBooleans = br.ReadBoolean();
                    AnimIDs = br.ReadInt16s(4);
                    BreakobjID = br.ReadInt32s(2);
                }

                private protected override void ReadGparamConfig(BinaryReaderEx br) => GparamConfigStruct = new GparamConfigStruct(br);

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(CollisionIndex);
                    bw.WriteSByte(BreakTerm);
                    bw.WriteSByte(NetSyncType);
                    bw.WriteBoolean(CollisionFilter);
                    bw.WriteBoolean(SetMainObjStructureBooleans);
                    bw.WriteInt16s(AnimIDs);
                    bw.WriteInt32s(BreakobjID);
                }

                private protected override void WriteGparamConfig(BinaryWriterEx bw) => GparamConfigStruct.Write(bw);

                internal override void GetNames(MSBB msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    CollisionName = MSB.FindName(entries.Parts, CollisionIndex);
                }

                internal override void GetIndices(MSBB msb, Entries entries)
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
                private protected override bool HasTypeData => true;
                private protected override bool HasGparamConfig => true;
                private protected override bool HasSceneGparamConfig => false;

                /// <summary>
                /// Gparam IDs for this enemy.
                /// </summary>
                public GparamConfigStruct GparamConfigStruct { get; set; }

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
                public short PatrolType { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short FallbackPlatoonID { get; set; }

                /// <summary>
                /// ID in CharaInitParam determining equipment and stats for humans.
                /// </summary>
                public int CharaInitID { get; set; }

                /// <summary>
                /// Collision that controls loading of the enemy.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Collision))]
                public string CollisionName { get; set; }
                private int CollisionIndex;

                /// <summary>
                /// Unknown.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Event.PatrolInfo))]
                public string PatrolInfoName { get; set; }
                private short PatrolInfoIndex;

                /// <summary>
                /// Regions for the enemy to patrol.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Region))]
                public string[] MovePointNames { get; private set; }
                private short[] MovePointIndices;

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
                    GparamConfigStruct = new GparamConfigStruct();
                    ThinkParamID = -1;
                    NPCParamID = -1;
                    TalkID = -1;
                    CharaInitID = -1;
                    MovePointNames = new string[8];
                }

                private protected override void DeepCopyTo(Part part)
                {
                    var enemy = (EnemyBase)part;
                    enemy.GparamConfigStruct = GparamConfigStruct.DeepCopy();
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
                    PatrolType = br.ReadInt16();
                    FallbackPlatoonID = br.ReadInt16();
                    CharaInitID = br.ReadInt32();
                    CollisionIndex = br.ReadInt32();
                    PatrolInfoIndex = br.ReadInt16();
                    br.AssertInt16(0);
                    br.AssertInt32(0);
                    MovePointIndices = br.ReadInt16s(8);
                    InitAnimID = br.ReadInt32();
                    DamageAnimID = br.ReadInt32();
                }

                private protected override void ReadGparamConfig(BinaryReaderEx br) => GparamConfigStruct = new GparamConfigStruct(br);

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(ThinkParamID);
                    bw.WriteInt32(NPCParamID);
                    bw.WriteInt32(TalkID);
                    bw.WriteInt16(PatrolType);
                    bw.WriteInt16(FallbackPlatoonID);
                    bw.WriteInt32(CharaInitID);
                    bw.WriteInt32(CollisionIndex);
                    bw.WriteInt16(PatrolInfoIndex);
                    bw.WriteInt16(0);
                    bw.WriteInt32(0);
                    bw.WriteInt16s(MovePointIndices);
                    bw.WriteInt32(InitAnimID);
                    bw.WriteInt32(DamageAnimID);
                }

                private protected override void WriteGparamConfig(BinaryWriterEx bw) => GparamConfigStruct.Write(bw);

                internal override void GetNames(MSBB msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    CollisionName = MSB.FindName(entries.Parts, CollisionIndex);

                    PatrolInfoName = MSB.FindNameInSubType(entries.Events, typeof(Event.PatrolInfo), PatrolInfoIndex);

                    MovePointNames = new string[MovePointIndices.Length];
                    for (int i = 0; i < MovePointIndices.Length; i++)
                        MovePointNames[i] = MSB.FindName(entries.Regions, MovePointIndices[i]);
                }

                internal override void GetIndices(MSBB msb, Entries entries)
                {
                    base.GetIndices(msb, entries);
                    CollisionIndex = MSB.FindIndex(this, entries.Parts, CollisionName);

                    PatrolInfoIndex = (short)MSB.FindIndexOfSubType(this, entries.Events, typeof(Event.PatrolInfo), PatrolInfoName);

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
                private protected override bool HasTypeData => true;
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
            /// Invisible but physical geometry.
            /// </summary>
            public class Collision : Part
            {

                private protected override PartType Type => PartType.Collision;
                private protected override bool HasTypeData => true;
                private protected override bool HasGparamConfig => true;
                private protected override bool HasSceneGparamConfig => true;

                /// <summary>
                /// Gparam IDs for this collision.
                /// </summary>
                public GparamConfigStruct GparamConfigStruct { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public SceneGparamConfigStruct SceneGparamConfigStruct { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte HitFilterID { get; set; }

                /// <summary>
                /// Causes sounds to be modulated when standing on the collision.
                /// </summary>
                public byte SoundSpaceType { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short EnvLightMapSpotIndex { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float ReflectPlaneHeight { get; set; }

                /// <summary>
                /// Controls displays of the map name on screen or the loading menu.
                /// </summary>
                public short MapNameID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public bool DisableStart { get; set; }

                /// <summary>
                /// Determines if enemy think will use dark and pitch dark eye distances.
                /// </summary>
                public byte MapVisibilityType { get; set; } = 0;

                /// <summary>
                /// If set, disables a bonfire when any enemy is on the collision.
                /// </summary>
                public int DisableBonfireEntityID { get; set; }

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
                public Collision() : base("hXXXXXX_XXXX")
                {
                    GparamConfigStruct = new GparamConfigStruct();
                    SceneGparamConfigStruct = new SceneGparamConfigStruct();
                    MapNameID = -1;
                    DisableBonfireEntityID = -1;
                    LockCamParamID1 = -1;
                    LockCamParamID2 = -1;
                }

                private protected override void DeepCopyTo(Part part)
                {
                    var collision = (Collision)part;
                    collision.GparamConfigStruct = GparamConfigStruct.DeepCopy();
                    collision.SceneGparamConfigStruct = SceneGparamConfigStruct.DeepCopy();
                }

                internal Collision(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    HitFilterID = br.ReadByte();
                    SoundSpaceType = br.ReadByte();
                    EnvLightMapSpotIndex = br.ReadInt16();
                    ReflectPlaneHeight = br.ReadSingle();
                    MapNameID = br.ReadInt16();
                    DisableStart = br.ReadBoolean();
                    MapVisibilityType = br.ReadByte();
                    DisableBonfireEntityID = br.ReadInt32();
                    PlayRegionID = br.ReadInt32();
                    LockCamParamID1 = br.ReadInt16();
                    LockCamParamID2 = br.ReadInt16();
                }

                private protected override void ReadGparamConfig(BinaryReaderEx br) => GparamConfigStruct = new GparamConfigStruct(br);
                private protected override void ReadSceneGparamConfig(BinaryReaderEx br) => SceneGparamConfigStruct = new SceneGparamConfigStruct(br);

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteByte(HitFilterID);
                    bw.WriteByte(SoundSpaceType);
                    bw.WriteInt16(EnvLightMapSpotIndex);
                    bw.WriteSingle(ReflectPlaneHeight);
                    bw.WriteInt16(MapNameID);
                    bw.WriteBoolean(DisableStart);
                    bw.WriteByte((byte)MapVisibilityType);
                    bw.WriteInt32(DisableBonfireEntityID);
                    bw.WriteInt32(PlayRegionID);
                    bw.WriteInt16(LockCamParamID1);
                    bw.WriteInt16(LockCamParamID2);
                }

                private protected override void WriteGparamConfig(BinaryWriterEx bw) => GparamConfigStruct.Write(bw);
                private protected override void WriteSceneGparamConfig(BinaryWriterEx bw) => SceneGparamConfigStruct.Write(bw);
            }

            /// <summary>
            /// An AI navigation mesh.
            /// </summary>
            public class Navmesh : Part
            {
                private protected override PartType Type => PartType.Navmesh;
                private protected override bool HasTypeData => true;
                private protected override bool HasGparamConfig => false;
                private protected override bool HasSceneGparamConfig => false;

                /// <summary>
                /// Creates a Navmesh with default values.
                /// </summary>
                public Navmesh() : base("nXXXXBX") { }

                internal Navmesh(BinaryReaderEx br) : base(br) { }

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
                private protected override bool HasTypeData => true;
                private protected override bool HasGparamConfig => false;
                private protected override bool HasSceneGparamConfig => false;

                /// <summary>
                /// The collision which will load another map.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Collision))]
                public string CollisionName { get; set; }
                private int CollisionIndex;

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

                internal override void GetNames(MSBB msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    CollisionName = MSB.FindName(msb.Parts.Collisions, CollisionIndex);
                }

                internal override void GetIndices(MSBB msb, Entries entries)
                {
                    base.GetIndices(msb, entries);
                    CollisionIndex = MSB.FindIndex(this, msb.Parts.Collisions, CollisionName);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class Other : Part
            {
                private protected override PartType Type => PartType.Other;
                private protected override bool HasTypeData => false;
                private protected override bool HasGparamConfig => false;
                private protected override bool HasSceneGparamConfig => false;

                /// <summary>
                /// Creates an Other with default values.
                /// </summary>
                // TODO verify this
                public Other() : base("hXXXXBX_XXXX") { }

                internal Other(BinaryReaderEx br) : base(br) { }
            }
        }
    }
}
