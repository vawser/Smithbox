using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace SoulsFormats
{
    public partial class MSBS
    {
        internal enum PartType : uint
        {
            MapPiece = 0,
            Object = 1,
            Enemy = 2,
            Player = 4,
            Collision = 5,
            DummyObject = 9,
            DummyEnemy = 10,
            ConnectCollision = 11,
        }

        /// <summary>
        /// Instances of actual things in the map.
        /// </summary>
        public class PartsParam : Param<Part>, IMsbParam<IMsbPart>
        {
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
            /// Creates an empty PartsParam with the default version.
            /// </summary>
            public PartsParam() : base(35, "PARTS_PARAM_ST")
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
            /// Returns every Part in the order they'll be written.
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
                PartType type = br.GetEnum32<PartType>(br.Position + 8);
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
        /// Common data for all types of part.
        /// </summary>
        public abstract class Part : Entry, IMsbPart
        {
            private protected abstract PartType Type { get; }
            private protected abstract bool HasUnk1 { get; }
            private protected abstract bool HasUnk2 { get; }
            private protected abstract bool HasGparamConfig { get; }
            private protected abstract bool HasSceneGparamConfig { get; }
            private protected abstract bool HasUnk7 { get; }

            /// <summary>
            /// The model used by this part; requires an entry in ModelParam.
            /// </summary>
            public string ModelName { get; set; }
            public int ModelIndex { get; set; }

            /// <summary>
            /// A path to a .sib file, presumably some kind of editor placeholder.
            /// </summary>
            public string SibPath { get; set; }

            /// <summary>
            /// Location of the part.
            /// </summary>
            public Vector3 Position { get; set; }

            /// <summary>
            /// Rotation of the part.
            /// </summary>
            public Vector3 Rotation { get; set; }

            /// <summary>
            /// Scale of the part; only works for map pieces and objects.
            /// </summary>
            public Vector3 Scale { get; set; }

            /// <summary>
            /// Identifies the part in event scripts.
            /// </summary>
            public int EntityID { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte UnkE04 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte UnkE05 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte UnkE06 { get; set; }

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
            public byte UnkE09 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public bool IsPointLightShadowSrc { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte UnkE0B { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public bool IsShadowSrc { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte IsStaticShadowSrc { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte IsCascade3ShadowSrc { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte UnkE0F { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte UnkE10 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public bool IsShadowDest { get; set; }

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
            public byte EnableOnAboveShadow { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public bool DisablePointLightEffect { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte UnkE17 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int UnkE18 { get; set; }

            /// <summary>
            /// Allows multiple parts to be identified by the same entity ID.
            /// </summary>
            public int[] EntityGroupIDs { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int UnkE3C { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int UnkE40 { get; set; }

            private protected Part(string name)
            {
                Name = name;
                SibPath = "";
                Scale = Vector3.One;
                EntityID = -1;
                EntityGroupIDs = new int[8];
                for (int i = 0; i < 8; i++)
                    EntityGroupIDs[i] = -1;
            }

            /// <summary>
            /// Creates a deep copy of the part.
            /// </summary>
            public Part DeepCopy()
            {
                var part = (Part)MemberwiseClone();
                part.EntityGroupIDs = (int[])EntityGroupIDs.Clone();
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
                br.AssertInt32(-1);
                br.AssertInt32(0);
                long unkOffset1 = br.ReadInt64();
                long unkOffset2 = br.ReadInt64();
                long entityDataOffset = br.ReadInt64();
                long typeDataOffset = br.ReadInt64();
                long gparamOffset = br.ReadInt64();
                long unkOffset6 = br.ReadInt64();
                long unkOffset7 = br.ReadInt64();
                br.AssertInt64(0);
                br.AssertInt64(0);
                br.AssertInt64(0);

                if (nameOffset == 0)
                    throw new InvalidDataException($"{nameof(nameOffset)} must not be 0 in type {GetType()}.");
                if (sibOffset == 0)
                    throw new InvalidDataException($"{nameof(sibOffset)} must not be 0 in type {GetType()}.");
                if (HasUnk1 ^ unkOffset1 != 0)
                    throw new InvalidDataException($"Unexpected {nameof(unkOffset1)} 0x{unkOffset1:X} in type {GetType()}.");
                if (HasUnk2 ^ unkOffset2 != 0)
                    throw new InvalidDataException($"Unexpected {nameof(unkOffset2)} 0x{unkOffset2:X} in type {GetType()}.");
                if (entityDataOffset == 0)
                    throw new InvalidDataException($"{nameof(entityDataOffset)} must not be 0 in type {GetType()}.");
                if (typeDataOffset == 0)
                    throw new InvalidDataException($"{nameof(typeDataOffset)} must not be 0 in type {GetType()}.");
                if (HasGparamConfig ^ gparamOffset != 0)
                    throw new InvalidDataException($"Unexpected {nameof(gparamOffset)} 0x{gparamOffset:X} in type {GetType()}.");
                if (HasSceneGparamConfig ^ unkOffset6 != 0)
                    throw new InvalidDataException($"Unexpected {nameof(unkOffset6)} 0x{unkOffset6:X} in type {GetType()}.");
                if (HasUnk7 ^ unkOffset7 != 0)
                    throw new InvalidDataException($"Unexpected {nameof(unkOffset7)} 0x{unkOffset7:X} in type {GetType()}.");

                br.Position = start + nameOffset;
                Name = br.ReadUTF16();

                br.Position = start + sibOffset;
                SibPath = br.ReadUTF16();

                if (HasUnk1)
                {
                    br.Position = start + unkOffset1;
                    ReadUnk1(br);
                }

                if (HasUnk2)
                {
                    br.Position = start + unkOffset2;
                    ReadUnk2(br);
                }

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
                    br.Position = start + unkOffset6;
                    ReadSceneGparamConfig(br);
                }

                if (HasUnk7)
                {
                    br.Position = start + unkOffset7;
                    ReadUnk7(br);
                }
            }

            private void ReadEntityData(BinaryReaderEx br)
            {
                EntityID = br.ReadInt32();
                UnkE04 = br.ReadByte();
                UnkE05 = br.ReadByte();
                UnkE06 = br.ReadByte();
                LanternID = br.ReadByte();
                LodParamID = br.ReadByte();
                UnkE09 = br.ReadByte();
                IsPointLightShadowSrc = br.ReadBoolean();
                UnkE0B = br.ReadByte();
                IsShadowSrc = br.ReadBoolean();
                IsStaticShadowSrc = br.ReadByte();
                IsCascade3ShadowSrc = br.ReadByte();
                UnkE0F = br.ReadByte();
                UnkE10 = br.ReadByte();
                IsShadowDest = br.ReadBoolean();
                IsShadowOnly = br.ReadBoolean();
                DrawByReflectCam = br.ReadBoolean();
                DrawOnlyReflectCam = br.ReadBoolean();
                EnableOnAboveShadow = br.ReadByte();
                DisablePointLightEffect = br.ReadBoolean();
                UnkE17 = br.ReadByte();
                UnkE18 = br.ReadInt32();
                EntityGroupIDs = br.ReadInt32s(8);
                UnkE3C = br.ReadInt32();
                UnkE40 = br.ReadInt32();
                br.AssertPattern(0x10, 0x00);
            }

            private protected abstract void ReadTypeData(BinaryReaderEx br);

            private protected virtual void ReadUnk1(BinaryReaderEx br)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(ReadUnk1)}.");

            private protected virtual void ReadUnk2(BinaryReaderEx br)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(ReadUnk2)}.");

            private protected virtual void ReadGparamConfig(BinaryReaderEx br)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(ReadGparamConfig)}.");

            private protected virtual void ReadSceneGparamConfig(BinaryReaderEx br)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(ReadSceneGparamConfig)}.");

            private protected virtual void ReadUnk7(BinaryReaderEx br)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(ReadUnk7)}.");

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
                bw.WriteInt32(-1);
                bw.WriteInt32(0);
                bw.ReserveInt64("UnkOffset1");
                bw.ReserveInt64("UnkOffset2");
                bw.ReserveInt64("EntityDataOffset");
                bw.ReserveInt64("TypeDataOffset");
                bw.ReserveInt64("GparamOffset");
                bw.ReserveInt64("SceneGparamOffset");
                bw.ReserveInt64("UnkOffset7");
                bw.WriteInt64(0);
                bw.WriteInt64(0);
                bw.WriteInt64(0);

                bw.FillInt64("NameOffset", bw.Position - start);
                bw.WriteUTF16(MSB.ReambiguateName(Name), true);

                bw.FillInt64("SibOffset", bw.Position - start);
                bw.WriteUTF16(SibPath, true);
                bw.Pad(8);

                if (HasUnk1)
                {
                    bw.FillInt64("UnkOffset1", bw.Position - start);
                    WriteUnk1(bw);
                }
                else
                {
                    bw.FillInt64("UnkOffset1", 0);
                }

                if (HasUnk2)
                {
                    bw.FillInt64("UnkOffset2", bw.Position - start);
                    WriteUnk2(bw);
                }
                else
                {
                    bw.FillInt64("UnkOffset2", 0);
                }

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

                if (HasUnk7)
                {
                    bw.FillInt64("UnkOffset7", bw.Position - start);
                    WriteUnk7(bw);
                }
                else
                {
                    bw.FillInt64("UnkOffset7", 0);
                }
            }

            private void WriteEntityData(BinaryWriterEx bw)
            {
                bw.WriteInt32(EntityID);
                bw.WriteByte(UnkE04);
                bw.WriteByte(UnkE05);
                bw.WriteByte(UnkE06);
                bw.WriteByte(LanternID);
                bw.WriteByte(LodParamID);
                bw.WriteByte(UnkE09);
                bw.WriteBoolean(IsPointLightShadowSrc);
                bw.WriteByte(UnkE0B);
                bw.WriteBoolean(IsShadowSrc);
                bw.WriteByte(IsStaticShadowSrc);
                bw.WriteByte(IsCascade3ShadowSrc);
                bw.WriteByte(UnkE0F);
                bw.WriteByte(UnkE10);
                bw.WriteBoolean(IsShadowDest);
                bw.WriteBoolean(IsShadowOnly);
                bw.WriteBoolean(DrawByReflectCam);
                bw.WriteBoolean(DrawOnlyReflectCam);
                bw.WriteByte(EnableOnAboveShadow);
                bw.WriteBoolean(DisablePointLightEffect);
                bw.WriteByte(UnkE17);
                bw.WriteInt32(UnkE18);
                bw.WriteInt32s(EntityGroupIDs);
                bw.WriteInt32(UnkE3C);
                bw.WriteInt32(UnkE40);
                bw.WritePattern(0x10, 0x00);
                bw.Pad(8);
            }

            private protected abstract void WriteTypeData(BinaryWriterEx bw);

            private protected virtual void WriteUnk1(BinaryWriterEx bw)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(WriteUnk1)}.");

            private protected virtual void WriteUnk2(BinaryWriterEx bw)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(WriteUnk2)}.");

            private protected virtual void WriteGparamConfig(BinaryWriterEx bw)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(WriteGparamConfig)}.");

            private protected virtual void WriteSceneGparamConfig(BinaryWriterEx bw)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(WriteSceneGparamConfig)}.");

            private protected virtual void WriteUnk7(BinaryWriterEx bw)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(WriteUnk7)}.");

            internal virtual void GetNames(MSBS msb, Entries entries)
            {
                ModelName = MSB.FindName(entries.Models, ModelIndex);
            }

            internal virtual void GetIndices(MSBS msb, Entries entries)
            {
                ModelIndex = MSB.FindIndex(this, entries.Models, ModelName);
            }

            /// <summary>
            /// Returns the type and name of the part as a string.
            /// </summary>
            public override string ToString()
            {
                return $"{Type} {Name}";
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class DisplayDataStruct
            {
                /// <summary>
                /// Unknown.
                /// </summary>
                public uint[] DisplayGroups { get; private set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public uint[] DrawGroups { get; private set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public uint[] CollisionMask { get; private set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte Condition1 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte Condition2 { get; set; }

                /// <summary>
                /// Creates an UnkStruct1 with default values.
                /// </summary>
                public DisplayDataStruct()
                {
                    DisplayGroups = new uint[8];
                    DrawGroups = new uint[8];
                    CollisionMask = new uint[32];
                    Condition1 = 0;
                    Condition2 = 0;
                }

                /// <summary>
                /// Creates a deep copy of the struct.
                /// </summary>
                public DisplayDataStruct DeepCopy()
                {
                    var unk1 = (DisplayDataStruct)MemberwiseClone();
                    unk1.DisplayGroups = (uint[])DisplayGroups.Clone();
                    unk1.DrawGroups = (uint[])DrawGroups.Clone();
                    unk1.CollisionMask = (uint[])CollisionMask.Clone();
                    return unk1;
                }

                internal DisplayDataStruct(BinaryReaderEx br)
                {
                    DisplayGroups = br.ReadUInt32s(8);
                    DrawGroups = br.ReadUInt32s(8);
                    CollisionMask = br.ReadUInt32s(32);
                    Condition1 = br.ReadByte();
                    Condition2 = br.ReadByte();
                    br.AssertInt16(0);
                    br.AssertPattern(0xC0, 0x00);
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteUInt32s(DisplayGroups);
                    bw.WriteUInt32s(DrawGroups);
                    bw.WriteUInt32s(CollisionMask);
                    bw.WriteByte(Condition1);
                    bw.WriteByte(Condition2);
                    bw.WriteInt16(0);
                    bw.WritePattern(0xC0, 0x00);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class DisplayGroupStruct
            {
                /// <summary>
                /// Unknown.
                /// </summary>
                public int Condition { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int[] DispGroups { get; private set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short Unk24 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short Unk26 { get; set; }

                /// <summary>
                /// Creates an UnkStruct2 with default values.
                /// </summary>
                public DisplayGroupStruct()
                {
                    DispGroups = new int[8];
                }

                /// <summary>
                /// Creates a deep copy of the struct.
                /// </summary>
                public DisplayGroupStruct DeepCopy()
                {
                    var unk2 = (DisplayGroupStruct)MemberwiseClone();
                    unk2.DispGroups = (int[])DispGroups.Clone();
                    return unk2;
                }

                internal DisplayGroupStruct(BinaryReaderEx br)
                {
                    Condition = br.ReadInt32();
                    DispGroups = br.ReadInt32s(8);
                    Unk24 = br.ReadInt16();
                    Unk26 = br.ReadInt16();
                    br.AssertPattern(0x20, 0x00);
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Condition);
                    bw.WriteInt32s(DispGroups);
                    bw.WriteInt16(Unk24);
                    bw.WriteInt16(Unk26);
                    bw.WritePattern(0x20, 0x00);
                }
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
            /// Unknown; sceneGParam Struct according to Pav.
            /// </summary>
            public class SceneGparamConfig
            {
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
                    br.AssertPattern(0x3C, 0x00);
                    EventIDs = br.ReadSBytes(4);
                    TransitionTime = br.ReadSingle();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WritePattern(0x3C, 0x00);
                    bw.WriteSBytes(EventIDs);
                    bw.WriteSingle(TransitionTime);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }

                /// <summary>
                /// Returns a string representation of the object.
                /// </summary>
                public override string ToString()
                {
                    return $"EventID[{EventIDs[0],2}][{EventIDs[1],2}][{EventIDs[2],2}][{EventIDs[3],2}] {TransitionTime:0.0}";
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class GrassConfigStruct
            {
                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk00 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk04 { get; set; }

                /// <summary>
                /// ID in GrassTypeParam determining properties of dynamic grass on a map piece.
                /// </summary>
                [MSBParamReference(ParamName = "GrassTypeParam")]
                public int GrassTypeParamID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk0C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk10 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk14 { get; set; }

                /// <summary>
                /// Creates an UnkStruct7 with default values.
                /// </summary>
                public GrassConfigStruct() { }

                /// <summary>
                /// Creates a deep copy of the struct.
                /// </summary>
                public GrassConfigStruct DeepCopy()
                {
                    return (GrassConfigStruct)MemberwiseClone();
                }

                internal GrassConfigStruct(BinaryReaderEx br)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadInt32();
                    GrassTypeParamID = br.ReadInt32();
                    Unk0C = br.ReadInt32();
                    Unk10 = br.ReadInt32();
                    Unk14 = br.ReadInt32();
                    br.AssertInt32(-1);
                    br.AssertInt32(0);
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(GrassTypeParamID);
                    bw.WriteInt32(Unk0C);
                    bw.WriteInt32(Unk10);
                    bw.WriteInt32(Unk14);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// Fixed visual geometry.
            /// </summary>
            public class MapPiece : Part
            {
                private protected override PartType Type => PartType.MapPiece;
                private protected override bool HasUnk1 => true;
                private protected override bool HasUnk2 => false;
                private protected override bool HasGparamConfig => true;
                private protected override bool HasSceneGparamConfig => false;
                private protected override bool HasUnk7 => true;

                /// <summary>
                /// Unknown.
                /// </summary>
                public DisplayDataStruct DisplayDataStruct { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public GparamConfig GparamConfig { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public GrassConfigStruct GrassConfigStruct { get; set; }

                /// <summary>
                /// Creates a MapPiece with default values.
                /// </summary>
                public MapPiece() : base("mXXXXXX_XXXX")
                {
                    DisplayDataStruct = new DisplayDataStruct();
                    GparamConfig = new GparamConfig();
                    GrassConfigStruct = new GrassConfigStruct();
                }

                private protected override void DeepCopyTo(Part part)
                {
                    var piece = (MapPiece)part;
                    piece.DisplayDataStruct = DisplayDataStruct.DeepCopy();
                    piece.GparamConfig = GparamConfig.DeepCopy();
                    piece.GrassConfigStruct = GrassConfigStruct.DeepCopy();
                }

                internal MapPiece(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                private protected override void ReadUnk1(BinaryReaderEx br) => DisplayDataStruct = new DisplayDataStruct(br);
                private protected override void ReadGparamConfig(BinaryReaderEx br) => GparamConfig = new GparamConfig(br);
                private protected override void ReadUnk7(BinaryReaderEx br) => GrassConfigStruct = new GrassConfigStruct(br);

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }

                private protected override void WriteUnk1(BinaryWriterEx bw) => DisplayDataStruct.Write(bw);
                private protected override void WriteGparamConfig(BinaryWriterEx bw) => GparamConfig.Write(bw);
                private protected override void WriteUnk7(BinaryWriterEx bw) => GrassConfigStruct.Write(bw);
            }

            /// <summary>
            /// Common base data for objects and dummy objects.
            /// </summary>
            public abstract class ObjectBase : Part
            {
                private protected override bool HasUnk2 => false;
                private protected override bool HasGparamConfig => true;
                private protected override bool HasSceneGparamConfig => false;
                private protected override bool HasUnk7 => false;

                /// <summary>
                /// Unknown.
                /// </summary>
                public GparamConfig GparamConfig { get; set; }

                /// <summary>
                /// Reference to a map piece or collision; believed to determine when the object is loaded.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Part))]
                public string ObjPartName1 { get; set; }
                public int ObjPartIndex1 { get; set; }

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
                public byte UnkT0E { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public bool SetMainObjStructureBooleans { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short AnimID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short ModelSfxParamRelativeID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short UnkT1A { get; set; }

                /// <summary>
                /// Reference to a collision; believed to be involved with loading when grappling to the object.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Collision))]
                public string ObjPartName2 { get; set; }
                public int ObjPartIndex2 { get; set; }

                /// <summary>
                /// Reference to a collision; believed to be involved with loading when grappling to the object.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Collision))]
                public string ObjPartName3 { get; set; }
                public int ObjPartIndex3 { get; set; }

                private protected ObjectBase() : base("oXXXXXX_XXXX")
                {
                    GparamConfig = new GparamConfig();
                }

                private protected override void DeepCopyTo(Part part)
                {
                    var obj = (ObjectBase)part;
                    obj.GparamConfig = GparamConfig.DeepCopy();
                }

                private protected ObjectBase(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    ObjPartIndex1 = br.ReadInt32();
                    BreakTerm = br.ReadByte();
                    NetSyncType = br.ReadBoolean();
                    UnkT0E = br.ReadByte();
                    SetMainObjStructureBooleans = br.ReadBoolean();
                    AnimID = br.ReadInt16();
                    br.AssertInt16(-1);
                    br.AssertInt32(-1);
                    ModelSfxParamRelativeID = br.ReadInt16();
                    UnkT1A = br.ReadInt16();
                    br.AssertInt32(-1);
                    ObjPartIndex2 = br.ReadInt32();
                    ObjPartIndex3 = br.ReadInt32();
                }

                private protected override void ReadGparamConfig(BinaryReaderEx br) => GparamConfig = new GparamConfig(br);

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(ObjPartIndex1);
                    bw.WriteByte(BreakTerm);
                    bw.WriteBoolean(NetSyncType);
                    bw.WriteByte(UnkT0E);
                    bw.WriteBoolean(SetMainObjStructureBooleans);
                    bw.WriteInt16(AnimID);
                    bw.WriteInt16(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt16(ModelSfxParamRelativeID);
                    bw.WriteInt16(UnkT1A);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(ObjPartIndex2);
                    bw.WriteInt32(ObjPartIndex3);
                }

                private protected override void WriteGparamConfig(BinaryWriterEx bw) => GparamConfig.Write(bw);

                internal override void GetNames(MSBS msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    ObjPartName1 = MSB.FindName(entries.Parts, ObjPartIndex1);
                    ObjPartName2 = MSB.FindName(entries.Parts, ObjPartIndex2);
                    ObjPartName3 = MSB.FindName(entries.Parts, ObjPartIndex3);
                }

                internal override void GetIndices(MSBS msb, Entries entries)
                {
                    base.GetIndices(msb, entries);
                    ObjPartIndex1 = MSB.FindIndex(this, entries.Parts, ObjPartName1);
                    ObjPartIndex2 = MSB.FindIndex(this, entries.Parts, ObjPartName2);
                    ObjPartIndex3 = MSB.FindIndex(this, entries.Parts, ObjPartName3);
                }
            }

            /// <summary>
            /// A dynamic or interactible element in the map.
            /// </summary>
            public class Object : ObjectBase
            {
                private protected override PartType Type => PartType.Object;
                private protected override bool HasUnk1 => true;

                /// <summary>
                /// Unknown.
                /// </summary>
                public DisplayDataStruct DisplayDataStruct { get; set; }

                /// <summary>
                /// Creates an Object with default values.
                /// </summary>
                public Object() : base()
                {
                    DisplayDataStruct = new DisplayDataStruct();
                }

                private protected override void DeepCopyTo(Part part)
                {
                    base.DeepCopyTo(part);
                    var obj = (Object)part;
                    obj.DisplayDataStruct = DisplayDataStruct.DeepCopy();
                }

                internal Object(BinaryReaderEx br) : base(br) { }

                private protected override void ReadUnk1(BinaryReaderEx br) => DisplayDataStruct = new DisplayDataStruct(br);

                private protected override void WriteUnk1(BinaryWriterEx bw) => DisplayDataStruct.Write(bw);
            }

            /// <summary>
            /// Common base data for enemies and dummy enemies.
            /// </summary>
            public abstract class EnemyBase : Part
            {
                private protected override bool HasUnk2 => false;
                private protected override bool HasGparamConfig => true;
                private protected override bool HasSceneGparamConfig => false;
                private protected override bool HasUnk7 => false;

                /// <summary>
                /// Unknown.
                /// </summary>
                public GparamConfig GparamConfig { get; set; }

                /// <summary>
                /// An ID in NPCThinkParam that determines the enemy's AI characteristics.
                /// </summary>
                public int ThinkParamID { get; set; }

                /// <summary>
                /// An ID in NPCParam that determines a variety of enemy properties.
                /// </summary>
                public int NPCParamID { get; set; }

                /// <summary>
                /// Unknown; previously talk ID, now always 0 or 1 except for the Memorial Mob in Senpou.
                /// </summary>
                public int UnkT10 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short PlatoonID { get; set; }

                /// <summary>
                /// An ID in CharaInitParam that determines a human's inventory and stats.
                /// </summary>
                public int CharaInitID { get; set; }

                /// <summary>
                /// Should reference the collision the enemy starts on.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Collision))]
                public string CollisionPartName { get; set; }
                public int CollisionPartIndex { get; set; }

                /// <summary>
                /// References which PatrolInfo index to use for patrol information.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Event.PatrolInfo))]
                public string WalkRouteName { get; set; }
                public short WalkRouteIndex { get; set; }

                /// <summary>
                /// Enum that refers to an animation ID to use.
                /// </summary>
                public short InitAnimIDType { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT24 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int BackupEventAnimID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int EventFlagID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int EventFlagCompareState { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT48 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT4C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT50 { get; set; }

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
                    ThinkParamID = -1;
                    NPCParamID = -1;
                    UnkT10 = -1;
                    CharaInitID = -1;
                    BackupEventAnimID = -1;
                    EventFlagID = -1;
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
                    UnkT10 = br.ReadInt32();
                    br.AssertInt16(0);
                    PlatoonID = br.ReadInt16();
                    CharaInitID = br.ReadInt32();
                    CollisionPartIndex = br.ReadInt32();
                    WalkRouteIndex = br.ReadInt16();
                    InitAnimIDType = br.ReadInt16();
                    UnkT24 = br.ReadInt32();
                    br.AssertPattern(0x10, 0xFF);
                    BackupEventAnimID = br.ReadInt32();
                    br.AssertInt32(-1);
                    EventFlagID = br.ReadInt32();
                    EventFlagCompareState = br.ReadInt32();
                    UnkT48 = br.ReadInt32();
                    UnkT4C = br.ReadInt32();
                    UnkT50 = br.ReadInt32();
                    br.AssertInt32(1);
                    br.AssertInt32(-1);
                    br.AssertInt32(1);
                    br.AssertPattern(0x18, 0x00);
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
                    br.AssertPattern(0x10, 0x00);
                }

                private protected override void ReadGparamConfig(BinaryReaderEx br) => GparamConfig = new GparamConfig(br);

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(ThinkParamID);
                    bw.WriteInt32(NPCParamID);
                    bw.WriteInt32(UnkT10);
                    bw.WriteInt16(0);
                    bw.WriteInt16(PlatoonID);
                    bw.WriteInt32(CharaInitID);
                    bw.WriteInt32(CollisionPartIndex);
                    bw.WriteInt16(WalkRouteIndex);
                    bw.WriteInt16(InitAnimIDType);
                    bw.WriteInt32(UnkT24);
                    bw.WritePattern(0x10, 0xFF);
                    bw.WriteInt32(BackupEventAnimID);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(EventFlagID);
                    bw.WriteInt32(EventFlagCompareState);
                    bw.WriteInt32(UnkT48);
                    bw.WriteInt32(UnkT4C);
                    bw.WriteInt32(UnkT50);
                    bw.WriteInt32(1);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(1);
                    bw.WritePattern(0x18, 0x00);
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
                    bw.WritePattern(0x10, 0x00);
                }

                private protected override void WriteGparamConfig(BinaryWriterEx bw) => GparamConfig.Write(bw);

                internal override void GetNames(MSBS msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    CollisionPartName = MSB.FindName(entries.Parts, CollisionPartIndex);
                    WalkRouteName = MSB.FindName(entries.Events.FindAll(e => e.GetType() == typeof(MSBS.Event.PatrolInfo)), WalkRouteIndex);
                }

                internal override void GetIndices(MSBS msb, Entries entries)
                {
                    base.GetIndices(msb, entries);
                    CollisionPartIndex = MSB.FindIndex(this, entries.Parts, CollisionPartName);
                    WalkRouteIndex = (short)MSB.FindIndex(this, entries.Events.FindAll(e => e.GetType() == typeof(MSBS.Event.PatrolInfo)), WalkRouteName);
                }
            }

            /// <summary>
            /// Any non-player character.
            /// </summary>
            public class Enemy : EnemyBase
            {
                private protected override PartType Type => PartType.Enemy;
                private protected override bool HasUnk1 => true;

                /// <summary>
                /// Unknown.
                /// </summary>
                public DisplayDataStruct DisplayDataStruct { get; set; }

                /// <summary>
                /// Creates an Enemy with default values.
                /// </summary>
                public Enemy() : base()
                {
                    DisplayDataStruct = new DisplayDataStruct();
                }

                private protected override void DeepCopyTo(Part part)
                {
                    base.DeepCopyTo(part);
                    var enemy = (Enemy)part;
                    enemy.DisplayDataStruct = DisplayDataStruct.DeepCopy();
                }

                internal Enemy(BinaryReaderEx br) : base(br) { }

                private protected override void ReadUnk1(BinaryReaderEx br) => DisplayDataStruct = new DisplayDataStruct(br);

                private protected override void WriteUnk1(BinaryWriterEx bw) => DisplayDataStruct.Write(bw);
            }

            /// <summary>
            /// A spawn point for the player, or something.
            /// </summary>
            public class Player : Part
            {
                private protected override PartType Type => PartType.Player;
                private protected override bool HasUnk1 => false;
                private protected override bool HasUnk2 => false;
                private protected override bool HasGparamConfig => false;
                private protected override bool HasSceneGparamConfig => false;
                private protected override bool HasUnk7 => false;

                /// <summary>
                /// Creates a Player with default values.
                /// </summary>
                public Player() : base("c0000_XXXX") { }

                internal Player(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    br.AssertPattern(0x10, 0x00);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WritePattern(0x10, 0x00);
                }
            }

            /// <summary>
            /// Invisible but physical geometry.
            /// </summary>
            public class Collision : Part
            {
                private protected override PartType Type => PartType.Collision;
                private protected override bool HasUnk1 => true;
                private protected override bool HasUnk2 => true;
                private protected override bool HasGparamConfig => true;
                private protected override bool HasSceneGparamConfig => true;
                private protected override bool HasUnk7 => false;

                /// <summary>
                /// Unknown.
                /// </summary>
                public DisplayDataStruct DisplayDataStruct { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public DisplayGroupStruct DisplayGroupStruct { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public GparamConfig GparamConfig { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public SceneGparamConfig SceneGparamConfig { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte HitFilterID { get; set; }

                /// <summary>
                /// Adds reverb to sounds while on this collision to simulate echoes.
                /// </summary>
                public byte SoundSpaceType { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float ReflectPlaneHeight { get; set; }

                /// <summary>
                /// Determines the text to display for map popups and save files.
                /// </summary>
                public short MapNameID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public bool DisableStart { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT17 { get; set; }

                /// <summary>
                /// If not -1, the bonfire with this ID will be disabled when enemies are on this collision.
                /// </summary>
                public int DisableBonfireEntityID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT24 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT25 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT26 { get; set; }

                /// <summary>
                /// Should alter visibility while on this collision, but doesn't seem to do much.
                /// </summary>
                public byte MapVisibility { get; set; }

                /// <summary>
                /// Used to determine invasion eligibility.
                /// </summary>
                public int PlayRegionID { get; set; }

                /// <summary>
                /// Alters camera properties while on this collision.
                /// </summary>
                /// 
                public short LockCamParamID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT3C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT40 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT44 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT48 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT4C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT50 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT54 { get; set; }

                /// <summary>
                /// Creates a Collision with default values.
                /// </summary>
                public Collision() : base("hXXXXXX")
                {
                    DisplayDataStruct = new DisplayDataStruct();
                    DisplayGroupStruct = new DisplayGroupStruct();
                    GparamConfig = new GparamConfig();
                    SceneGparamConfig = new SceneGparamConfig();
                    DisableBonfireEntityID = -1;
                }

                private protected override void DeepCopyTo(Part part)
                {
                    var collision = (Collision)part;
                    collision.DisplayDataStruct = DisplayDataStruct.DeepCopy();
                    collision.DisplayGroupStruct = DisplayGroupStruct.DeepCopy();
                    collision.GparamConfig = GparamConfig.DeepCopy();
                    collision.SceneGparamConfig = SceneGparamConfig.DeepCopy();
                }

                internal Collision(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    HitFilterID = br.ReadByte(); // Pav says Type, did it change?
                    SoundSpaceType = br.ReadByte();
                    br.AssertInt16(0);
                    ReflectPlaneHeight = br.ReadSingle();
                    br.AssertInt32(-1);
                    br.AssertInt32(-1);
                    br.AssertInt32(-1);
                    MapNameID = br.ReadInt16();
                    DisableStart = br.ReadBoolean();
                    UnkT17 = br.ReadByte();
                    DisableBonfireEntityID = br.ReadInt32();
                    br.AssertInt32(-1);
                    br.AssertInt32(-1);
                    UnkT24 = br.ReadByte();
                    UnkT25 = br.ReadByte();
                    UnkT26 = br.ReadByte();
                    MapVisibility = br.ReadByte();
                    PlayRegionID = br.ReadInt32();
                    LockCamParamID = br.ReadInt16();
                    br.AssertInt16(-1);
                    br.AssertInt32(-1);
                    br.AssertInt32(-1);
                    br.AssertInt32(-1);
                    UnkT3C = br.ReadInt32();
                    UnkT40 = br.ReadInt32();
                    UnkT44 = br.ReadSingle();
                    UnkT48 = br.ReadSingle();
                    UnkT4C = br.ReadInt32();
                    UnkT50 = br.ReadSingle();
                    UnkT54 = br.ReadSingle();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                private protected override void ReadUnk1(BinaryReaderEx br) => DisplayDataStruct = new DisplayDataStruct(br);
                private protected override void ReadUnk2(BinaryReaderEx br) => DisplayGroupStruct = new DisplayGroupStruct(br);
                private protected override void ReadGparamConfig(BinaryReaderEx br) => GparamConfig = new GparamConfig(br);
                private protected override void ReadSceneGparamConfig(BinaryReaderEx br) => SceneGparamConfig = new SceneGparamConfig(br);

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteByte(HitFilterID);
                    bw.WriteByte(SoundSpaceType);
                    bw.WriteInt16(0);
                    bw.WriteSingle(ReflectPlaneHeight);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt16(MapNameID);
                    bw.WriteBoolean(DisableStart);
                    bw.WriteByte(UnkT17);
                    bw.WriteInt32(DisableBonfireEntityID);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteByte(UnkT24);
                    bw.WriteByte(UnkT25);
                    bw.WriteByte(UnkT26);
                    bw.WriteByte(MapVisibility);
                    bw.WriteInt32(PlayRegionID);
                    bw.WriteInt16(LockCamParamID);
                    bw.WriteInt16(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(UnkT3C);
                    bw.WriteInt32(UnkT40);
                    bw.WriteSingle(UnkT44);
                    bw.WriteSingle(UnkT48);
                    bw.WriteInt32(UnkT4C);
                    bw.WriteSingle(UnkT50);
                    bw.WriteSingle(UnkT54);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }

                private protected override void WriteUnk1(BinaryWriterEx bw) => DisplayDataStruct.Write(bw);
                private protected override void WriteUnk2(BinaryWriterEx bw) => DisplayGroupStruct.Write(bw);
                private protected override void WriteGparamConfig(BinaryWriterEx bw) => GparamConfig.Write(bw);
                private protected override void WriteSceneGparamConfig(BinaryWriterEx bw) => SceneGparamConfig.Write(bw);
            }

            /// <summary>
            /// An object that either isn't used, or is used for a cutscene.
            /// </summary>
            public class DummyObject : ObjectBase
            {
                private protected override PartType Type => PartType.DummyObject;
                private protected override bool HasUnk1 => false;

                /// <summary>
                /// Creates a DummyObject with default values.
                /// </summary>
                public DummyObject() : base() { }

                internal DummyObject(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// An enemy that either isn't used, or is used for a cutscene.
            /// </summary>
            public class DummyEnemy : EnemyBase
            {
                private protected override PartType Type => PartType.DummyEnemy;
                private protected override bool HasUnk1 => false;

                /// <summary>
                /// Creates a DummyEnemy with default values.
                /// </summary>
                public DummyEnemy() : base() { }

                internal DummyEnemy(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// References an actual collision and causes another map to be loaded while on it.
            /// </summary>
            public class ConnectCollision : Part
            {
                private protected override PartType Type => PartType.ConnectCollision;
                private protected override bool HasUnk1 => false;
                private protected override bool HasUnk2 => true;
                private protected override bool HasGparamConfig => false;
                private protected override bool HasSceneGparamConfig => false;
                private protected override bool HasUnk7 => false;

                /// <summary>
                /// Unknown.
                /// </summary>
                public DisplayGroupStruct DisplayGroupStruct { get; set; }

                /// <summary>
                /// The collision part to attach to.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Collision))]
                [NoRenderGroupInheritence()]
                public string CollisionName { get; set; }
                public int CollisionIndex { get; set; }

                /// <summary>
                /// The map to load when on this collision.
                /// </summary>
                public byte[] MapID { get; private set; }

                /// <summary>
                /// Creates a ConnectCollision with default values.
                /// </summary>
                public ConnectCollision() : base("hXXXXXX_XXXX")
                {
                    DisplayGroupStruct = new DisplayGroupStruct();
                    MapID = new byte[4];
                }

                private protected override void DeepCopyTo(Part part)
                {
                    var connect = (ConnectCollision)part;
                    connect.DisplayGroupStruct = DisplayGroupStruct.DeepCopy();
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

                private protected override void ReadUnk2(BinaryReaderEx br) => DisplayGroupStruct = new DisplayGroupStruct(br);

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(CollisionIndex);
                    bw.WriteBytes(MapID);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }

                private protected override void WriteUnk2(BinaryWriterEx bw) => DisplayGroupStruct.Write(bw);

                internal override void GetNames(MSBS msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    CollisionName = MSB.FindName(msb.Parts.Collisions, CollisionIndex);
                }

                internal override void GetIndices(MSBS msb, Entries entries)
                {
                    base.GetIndices(msb, entries);
                    CollisionIndex = MSB.FindIndex(this, msb.Parts.Collisions, CollisionName);
                }
            }
        }
    }
}
