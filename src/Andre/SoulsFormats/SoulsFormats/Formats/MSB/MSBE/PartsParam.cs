using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Xml.Serialization;

namespace SoulsFormats
{
    public partial class MSBE
    {
        internal enum PartType : uint
        {
            MapPiece = 0,
            Enemy = 2,
            Player = 4,
            Collision = 5,
            DummyAsset = 9, // Speculative for now
            DummyEnemy = 10,
            ConnectCollision = 11,
            Asset = 13,
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
            public List<Part.DummyAsset> DummyAssets { get; set; }

            /// <summary>
            /// Enemies that don't appear normally; either unused, or used for cutscenes.
            /// </summary>
            public List<Part.DummyEnemy> DummyEnemies { get; set; }

            /// <summary>
            /// Dummy parts that reference an actual collision and cause it to load another map.
            /// </summary>
            public List<Part.ConnectCollision> ConnectCollisions { get; set; }

            /// <summary>
            /// Dynamic props and interactive things.
            /// </summary>
            public List<Part.Asset> Assets { get; set; }

            /// <summary>
            /// Creates an empty PartsParam with the default version.
            /// </summary>
            public PartsParam() : base(73, "PARTS_PARAM_ST")
            {
                MapPieces = new List<Part.MapPiece>();
                Enemies = new List<Part.Enemy>();
                Players = new List<Part.Player>();
                Collisions = new List<Part.Collision>();
                DummyAssets = new List<Part.DummyAsset>();
                DummyEnemies = new List<Part.DummyEnemy>();
                ConnectCollisions = new List<Part.ConnectCollision>();
                Assets = new List<Part.Asset>();
            }

            /// <summary>
            /// Adds a part to the appropriate list for its type; returns the part.
            /// </summary>
            public Part Add(Part part)
            {
                switch (part)
                {
                    case Part.MapPiece p: MapPieces.Add(p); break;
                    case Part.Enemy p: Enemies.Add(p); break;
                    case Part.Player p: Players.Add(p); break;
                    case Part.Collision p: Collisions.Add(p); break;
                    case Part.DummyAsset p: DummyAssets.Add(p); break;
                    case Part.DummyEnemy p: DummyEnemies.Add(p); break;
                    case Part.ConnectCollision p: ConnectCollisions.Add(p); break;
                    case Part.Asset p: Assets.Add(p); break;

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
                    MapPieces, Enemies, Players, Collisions,
                    DummyAssets, DummyEnemies, ConnectCollisions, Assets);
            }
            IReadOnlyList<IMsbPart> IMsbParam<IMsbPart>.GetEntries() => GetEntries();

            internal override Part ReadEntry(BinaryReaderEx br)
            {
                PartType type = br.GetEnum32<PartType>(br.Position + 12);
                switch (type)
                {
                    case PartType.MapPiece:
                        return MapPieces.EchoAdd(new Part.MapPiece(br));

                    case PartType.Enemy:
                        return Enemies.EchoAdd(new Part.Enemy(br));

                    case PartType.Player:
                        return Players.EchoAdd(new Part.Player(br));

                    case PartType.Collision:
                        return Collisions.EchoAdd(new Part.Collision(br));

                    case PartType.DummyAsset:
                        return DummyAssets.EchoAdd(new Part.DummyAsset(br));

                    case PartType.DummyEnemy:
                        return DummyEnemies.EchoAdd(new Part.DummyEnemy(br));

                    case PartType.ConnectCollision:
                        return ConnectCollisions.EchoAdd(new Part.ConnectCollision(br));

                    case PartType.Asset:
                        return Assets.EchoAdd(new Part.Asset(br));

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
            public enum GameEditionDisableType : uint
            {
                NeverDisable = 0,

                /// <summary>
                /// Disabled if Localize.PkgEdition byte ptr [rcx+0B73h] == 1.
                /// The value is 1 in all released versions of Elden Ring.
                /// </summary>
                DisableInRelease = 1,

                /// <summary>
                /// Disabled if Localize.PkgEdition byte ptr [rcx+0B73h] == 2.
                /// The value is 1 in all released versions of Elden Ring.
                /// </summary>
                DisableInPkgEditionFlag2 = 2,

                DisableInNetworkTest = 3,
            }

            private protected abstract PartType Type { get; }
            private protected abstract bool HasUnk1 { get; }
            private protected abstract bool HasUnk2 { get; }
            private protected abstract bool HasGparamConfig { get; }
            private protected abstract bool HasSceneGparamConfig { get; }
            private protected abstract bool HasGrassConfig { get; }
            private protected abstract bool HasUnk8 { get; }
            private protected abstract bool HasUnk9 { get; }
            private protected abstract bool HasTileLoadConfig { get; }
            private protected abstract bool HasUnk11 { get; }

            /// <summary>
            /// The model used by this part; requires an entry in ModelParam.
            /// </summary>
            [ModelNameLink]
            public string ModelName { get; set; }
            [IndexProperty]
            [XmlIgnore]
            private int ModelIndex { get; set; }

            /// <summary>
            /// Involved with serialization.
            /// </summary>
            public int InstanceID { get; set; }

            /// <summary>
            /// A path to a .sib file, presumably some kind of editor placeholder.
            /// </summary>
            [IgnoreProperty]
            public string SibPath { get; set; }

            /// <summary>
            /// Location of the part.
            /// </summary>
            [PositionProperty]
            public Vector3 Position { get; set; }

            /// <summary>
            /// Rotation of the part.
            /// </summary>
            [RotationProperty]
            public Vector3 Rotation { get; set; }

            /// <summary>
            /// Scale of the part; only works for map pieces and objects.
            /// </summary>
            [ScaleProperty]
            public Vector3 Scale { get; set; }

            /// <summary>
            /// 1 disables the part, 2 and 3 are unknown.
            /// </summary>
            [IgnoreProperty]
            public GameEditionDisableType GameEditionDisable { get; set; } = GameEditionDisableType.NeverDisable;

            /// <summary>
            /// Very speculative
            /// </summary>
            [IgnoreProperty]
            public uint MapStudioLayer { get; set; }

            /// <summary>
            /// Identifies the part in event scripts.
            /// </summary>
            [EnemyProperty]
            public uint EntityID { get; set; }

            /// <summary>
            /// Enables use of PartsDrawParamID. If false, asset param is used instead.
            /// </summary>
            [IgnoreProperty]
            public byte isUsePartsDrawParamID { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            [MSBParamReference(ParamName = "PartsDrawParam")]
            [IgnoreProperty]
            public ushort PartsDrawParamID { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            [IgnoreProperty]
            public sbyte IsPointLightShadowSrc { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            [IgnoreProperty]
            public byte UnkE0B { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            [IgnoreProperty]
            public bool IsShadowSrc { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            [IgnoreProperty]
            public byte IsStaticShadowSrc { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            [IgnoreProperty]
            public byte IsCascade3ShadowSrc { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            [IgnoreProperty]
            public byte UnkE0F { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            [IgnoreProperty]
            public byte UnkE10 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            [IgnoreProperty]
            public bool IsShadowDest { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            [IgnoreProperty]
            public bool IsShadowOnly { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            [IgnoreProperty]
            public bool DrawByReflectCam { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            [IgnoreProperty]
            public bool DrawOnlyReflectCam { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            [IgnoreProperty]
            public byte EnableOnAboveShadow { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            [IgnoreProperty]
            public bool DisablePointLightEffect { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            [IgnoreProperty]
            public byte UnkE17 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            [IgnoreProperty]
            public int UnkE18 { get; set; }

            /// <summary>
            /// Allows multiple parts to be identified by the same entity ID.
            /// </summary>
            [EnemyProperty]
            public uint[] EntityGroupIDs { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            [IgnoreProperty]
            public short UnkE3C { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            [IgnoreProperty]
            public short UnkE3E { get; set; }

            private protected Part(string name)
            {
                Name = name;
                SibPath = "";
                Scale = Vector3.One;
                EntityID = 0;
                EntityGroupIDs = new uint[8];
            }

            /// <summary>
            /// Creates a deep copy of the part.
            /// </summary>
            public Part DeepCopy()
            {
                var part = (Part)MemberwiseClone();
                part.EntityGroupIDs = (uint[])EntityGroupIDs.Clone();
                DeepCopyTo(part);
                return part;
            }
            IMsbPart IMsbPart.DeepCopy() => DeepCopy();

            private protected virtual void DeepCopyTo(Part part) { }

            private protected Part(BinaryReaderEx br)
            {
                long start = br.Position;
                long nameOffset = br.ReadInt64();
                InstanceID = br.ReadInt32();
                br.AssertUInt32((uint)Type);
                br.ReadInt32(); // ID
                ModelIndex = br.ReadInt32();
                long sibOffset = br.ReadInt64();
                Position = br.ReadVector3();
                Rotation = br.ReadVector3();
                Scale = br.ReadVector3();
                GameEditionDisable = br.ReadEnum32<GameEditionDisableType>();
                MapStudioLayer = br.ReadUInt32();
                br.AssertInt32(0);
                long unkOffset1 = br.ReadInt64();
                long unkOffset2 = br.ReadInt64();
                long entityDataOffset = br.ReadInt64();
                long typeDataOffset = br.ReadInt64();
                long gparamOffset = br.ReadInt64();
                long sceneGParamOffset = br.ReadInt64();
                long unkOffset7 = br.ReadInt64();
                long unkOffset8 = br.ReadInt64();
                long unkOffset9 = br.ReadInt64();
                long unkOffset10 = br.ReadInt64();
                long unkOffset11 = br.ReadInt64();
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
                if (HasSceneGparamConfig ^ sceneGParamOffset != 0)
                    throw new InvalidDataException($"Unexpected {nameof(sceneGParamOffset)} 0x{sceneGParamOffset:X} in type {GetType()}.");
                if (HasGrassConfig ^ unkOffset7 != 0)
                    throw new InvalidDataException($"Unexpected {nameof(unkOffset7)} 0x{unkOffset7:X} in type {GetType()}.");
                if (HasUnk8 ^ unkOffset8 != 0)
                    throw new InvalidDataException($"Unexpected {nameof(unkOffset8)} 0x{unkOffset8:X} in type {GetType()}.");
                if (HasUnk9 ^ unkOffset9 != 0)
                    throw new InvalidDataException($"Unexpected {nameof(unkOffset9)} 0x{unkOffset9:X} in type {GetType()}.");
                if (HasTileLoadConfig ^ unkOffset10 != 0)
                    throw new InvalidDataException($"Unexpected {nameof(unkOffset10)} 0x{unkOffset10:X} in type {GetType()}.");
                if (HasUnk11 ^ unkOffset11 != 0)
                    throw new InvalidDataException($"Unexpected {nameof(unkOffset11)} 0x{unkOffset11:X} in type {GetType()}.");

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
                    br.Position = start + sceneGParamOffset;
                    ReadSceneGparamConfig(br);
                }

                if (HasGrassConfig)
                {
                    br.Position = start + unkOffset7;
                    ReadGrassConfig(br);
                }

                if (HasUnk8)
                {
                    br.Position = start + unkOffset8;
                    ReadUnk8(br);
                }

                if (HasUnk9)
                {
                    br.Position = start + unkOffset9;
                    ReadUnk9(br);
                }

                if (HasTileLoadConfig)
                {
                    br.Position = start + unkOffset10;
                    ReadTileLoad(br);
                }

                if (HasUnk11)
                {
                    br.Position = start + unkOffset11;
                    ReadUnk11(br);
                }
            }

            private void ReadEntityData(BinaryReaderEx br)
            {
                EntityID = br.ReadUInt32();
                isUsePartsDrawParamID = br.ReadByte();
                br.AssertByte(0);
                br.AssertByte(0);
                br.AssertByte(0); // Former lantern ID
                PartsDrawParamID = br.ReadUInt16();
                IsPointLightShadowSrc = br.ReadSByte(); // Seems to be 0 or -1
                UnkE0B = br.ReadByte();
                IsShadowSrc = br.ReadBoolean();
                IsStaticShadowSrc = br.ReadByte();
                IsCascade3ShadowSrc = br.ReadByte();
                UnkE0F = br.ReadByte();
                UnkE10 = br.ReadByte();
                IsShadowDest = br.ReadBoolean();
                IsShadowOnly = br.ReadBoolean(); // Seems to always be 0
                DrawByReflectCam = br.ReadBoolean();
                DrawOnlyReflectCam = br.ReadBoolean();
                EnableOnAboveShadow = br.ReadByte();
                DisablePointLightEffect = br.ReadBoolean();
                UnkE17 = br.ReadByte();
                UnkE18 = br.ReadInt32();
                EntityGroupIDs = br.ReadUInt32s(8);
                UnkE3C = br.ReadInt16();
                UnkE3E = br.ReadInt16();
                //br.AssertPattern(0x10, 0x00);
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

            private protected virtual void ReadGrassConfig(BinaryReaderEx br)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(ReadGrassConfig)}.");

            private protected virtual void ReadUnk8(BinaryReaderEx br)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(ReadUnk8)}.");

            private protected virtual void ReadUnk9(BinaryReaderEx br)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(ReadUnk9)}.");

            private protected virtual void ReadTileLoad(BinaryReaderEx br)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(ReadTileLoad)}.");

            private protected virtual void ReadUnk11(BinaryReaderEx br)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(ReadUnk11)}.");

            internal override void Write(BinaryWriterEx bw, int id)
            {
                long start = bw.Position;
                bw.ReserveInt64("NameOffset");
                bw.WriteInt32(InstanceID);
                bw.WriteUInt32((uint)Type);
                bw.WriteInt32(id);
                bw.WriteInt32(ModelIndex);
                bw.ReserveInt64("SibOffset");
                bw.WriteVector3(Position);
                bw.WriteVector3(Rotation);
                bw.WriteVector3(Scale);
                bw.WriteInt32((int)GameEditionDisable);
                bw.WriteUInt32(MapStudioLayer);
                bw.WriteInt32(0);
                bw.ReserveInt64("UnkOffset1");
                bw.ReserveInt64("UnkOffset2");
                bw.ReserveInt64("EntityDataOffset");
                bw.ReserveInt64("TypeDataOffset");
                bw.ReserveInt64("GparamOffset");
                bw.ReserveInt64("SceneGparamOffset");
                bw.ReserveInt64("UnkOffset7");
                bw.ReserveInt64("UnkOffset8");
                bw.ReserveInt64("UnkOffset9");
                bw.ReserveInt64("UnkOffset10");
                bw.ReserveInt64("UnkOffset11");
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

                if (HasGrassConfig)
                {
                    bw.FillInt64("UnkOffset7", bw.Position - start);
                    WriteGrassConfig(bw);
                }
                else
                {
                    bw.FillInt64("UnkOffset7", 0);
                }

                if (HasUnk8)
                {
                    bw.FillInt64("UnkOffset8", bw.Position - start);
                    WriteUnk8(bw);
                }
                else
                {
                    bw.FillInt64("UnkOffset8", 0);
                }

                if (HasUnk9)
                {
                    bw.FillInt64("UnkOffset9", bw.Position - start);
                    WriteUnk9(bw);
                }
                else
                {
                    bw.FillInt64("UnkOffset9", 0);
                }

                if (HasTileLoadConfig)
                {
                    bw.FillInt64("UnkOffset10", bw.Position - start);
                    WriteTileLoad(bw);
                }
                else
                {
                    bw.FillInt64("UnkOffset10", 0);
                }

                if (HasUnk11)
                {
                    bw.FillInt64("UnkOffset11", bw.Position - start);
                    WriteUnk11(bw);
                }
                else
                {
                    bw.FillInt64("UnkOffset11", 0);
                }
            }

            private void WriteEntityData(BinaryWriterEx bw)
            {
                bw.WriteUInt32(EntityID);
                bw.WriteByte(isUsePartsDrawParamID);
                bw.WriteByte(0);
                bw.WriteByte(0);
                bw.WriteByte(0);
                bw.WriteUInt16(PartsDrawParamID);
                bw.WriteSByte(IsPointLightShadowSrc);
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
                bw.WriteUInt32s(EntityGroupIDs);
                bw.WriteInt16(UnkE3C);
                bw.WriteInt16(UnkE3E);
                //bw.WritePattern(0x10, 0x00);
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

            private protected virtual void WriteGrassConfig(BinaryWriterEx bw)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(WriteGrassConfig)}.");

            private protected virtual void WriteUnk8(BinaryWriterEx bw)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(WriteUnk8)}.");

            private protected virtual void WriteUnk9(BinaryWriterEx bw)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(WriteUnk9)}.");

            private protected virtual void WriteTileLoad(BinaryWriterEx bw)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(WriteTileLoad)}.");

            private protected virtual void WriteUnk11(BinaryWriterEx bw)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(WriteUnk11)}.");

            internal virtual void GetNames(MSBE msb, Entries entries)
            {
                ModelName = MSB.FindName(entries.Models, ModelIndex);
            }

            internal virtual void GetIndices(MSBE msb, Entries entries)
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
            public class UnkStruct1
            {
                /// <summary>
                /// Unknown.
                /// </summary>
                public uint[] DisplayGroups { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public uint[] DrawGroups { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public uint[] CollisionMask { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                [IgnoreProperty]
                public byte Condition1 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                [IgnoreProperty]
                public byte Condition2 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                [IgnoreProperty]
                public byte UnkC2 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                [IgnoreProperty]
                public byte UnkC3 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                [IgnoreProperty]
                public short UnkC4 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                [IgnoreProperty]
                public short UnkC6 { get; set; }

                /// <summary>
                /// Creates an UnkStruct1 with default values.
                /// </summary>
                public UnkStruct1()
                {
                    DisplayGroups = new uint[8];
                    DrawGroups = new uint[8];
                    CollisionMask = new uint[32];
                    Condition1 = 0;
                    Condition2 = 0;
                    UnkC2 = 0;
                    UnkC3 = 0;
                    UnkC4 = 0;
                    UnkC6 = 0;
                }

                /// <summary>
                /// Creates a deep copy of the struct.
                /// </summary>
                public UnkStruct1 DeepCopy()
                {
                    var unk1 = (UnkStruct1)MemberwiseClone();
                    unk1.DisplayGroups = (uint[])DisplayGroups.Clone();
                    unk1.DrawGroups = (uint[])DrawGroups.Clone();
                    unk1.CollisionMask = (uint[])CollisionMask.Clone();
                    return unk1;
                }

                internal UnkStruct1(BinaryReaderEx br)
                {
                    DisplayGroups = br.ReadUInt32s(8);
                    DrawGroups = br.ReadUInt32s(8);
                    CollisionMask = br.ReadUInt32s(32);
                    Condition1 = br.ReadByte();
                    Condition2 = br.ReadByte();
                    UnkC2 = br.ReadByte();
                    UnkC3 = br.ReadByte();
                    UnkC4 = br.ReadInt16(); // Always -1 in retail
                    UnkC6 = br.ReadInt16(); // Always 0 or 1 in retail
                    br.AssertPattern(0xC0, 0x00);
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteUInt32s(DisplayGroups);
                    bw.WriteUInt32s(DrawGroups);
                    bw.WriteUInt32s(CollisionMask);
                    bw.WriteByte(Condition1);
                    bw.WriteByte(Condition2);
                    bw.WriteByte(UnkC2);
                    bw.WriteByte(UnkC3);
                    bw.WriteInt16(UnkC4);
                    bw.WriteInt16(UnkC6);
                    bw.WritePattern(0xC0, 0x00);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class UnkStruct2
            {
                /// <summary>
                /// Unknown.
                /// </summary>
                [IgnoreProperty]
                public int Condition { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public uint[] DispGroups { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                [IgnoreProperty]
                public short Unk24 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                [IgnoreProperty]
                public short Unk26 { get; set; }

                /// <summary>
                /// Creates an UnkStruct2 with default values.
                /// </summary>
                public UnkStruct2()
                {
                    DispGroups = new uint[8];
                }

                /// <summary>
                /// Creates a deep copy of the struct.
                /// </summary>
                public UnkStruct2 DeepCopy()
                {
                    var unk2 = (UnkStruct2)MemberwiseClone();
                    unk2.DispGroups = (uint[])DispGroups.Clone();
                    return unk2;
                }

                internal UnkStruct2(BinaryReaderEx br)
                {
                    Condition = br.ReadInt32();
                    DispGroups = br.ReadUInt32s(8);
                    Unk24 = br.ReadInt16();
                    Unk26 = br.ReadInt16();
                    br.AssertPattern(0x20, 0x00);
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Condition);
                    bw.WriteUInt32s(DispGroups);
                    bw.WriteInt16(Unk24);
                    bw.WriteInt16(Unk26);
                    bw.WritePattern(0x20, 0x00);
                }
            }

            /// <summary>
            /// Gparam value IDs for various part types. Struct seems similar to Sekiro
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
            /// Unknown; Struct seems absolutely gutted compared to Sekiro
            /// </summary>
            public class SceneGparamConfig
            {
                /// <summary>
                /// Unknown.
                /// </summary>
                public float TransitionTime { get; set; }

                /// <summary>
                /// Value of the hundredths place of a Gparam to override use.
                /// </summary>
                public sbyte GparamSubID_Base { get; set; }

                /// <summary>
                /// Value of the hundredths place of a Gparam to override Base with.
                /// </summary>
                public sbyte GparamSubID_Override1 { get; set; }

                /// <summary>
                /// Value of the hundredths place of a Gparam to override Base and Override 1 with.
                /// </summary>
                public sbyte GparamSubID_Override2 { get; set; }

                /// <summary>
                /// Value of the hundredths place of a Gparam to override Base and Override 1 and Override 2 with.
                /// </summary>
                public sbyte GparamSubID_Override3 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                [IgnoreProperty]
                public sbyte Unk1C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                [IgnoreProperty]
                public sbyte Unk1D { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                [IgnoreProperty]
                public sbyte Unk20 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                [IgnoreProperty]
                public sbyte Unk21 { get; set; }

                /// <summary>
                /// Creates a SceneGparamConfig with default values.
                /// </summary>
                public SceneGparamConfig()
                {
                    TransitionTime = 0.0f;
                    GparamSubID_Base = -1;
                    GparamSubID_Override1 = -1;
                    GparamSubID_Override2 = -1;
                    GparamSubID_Override3 = -1;
                    Unk1C = -1;
                    Unk1D = -1;
                    Unk20 = -1;
                    Unk21 = -1;
                }

                /// <summary>
                /// Creates a deep copy of the struct.
                /// </summary>
                public SceneGparamConfig DeepCopy()
                {
                    var config = (SceneGparamConfig)MemberwiseClone();
                    return config;
                }

                internal SceneGparamConfig(BinaryReaderEx br)
                {
                    br.AssertPattern(16, 0x00);
                    TransitionTime = br.ReadSingle();
                    br.AssertInt32(0);
                    GparamSubID_Base = br.ReadSByte();
                    GparamSubID_Override1 = br.ReadSByte();
                    GparamSubID_Override2 = br.ReadSByte();
                    GparamSubID_Override3 = br.ReadSByte();
                    Unk1C = br.ReadSByte();
                    Unk1D = br.ReadSByte();
                    br.AssertSByte(0);
                    br.AssertSByte(0);
                    Unk20 = br.ReadSByte();
                    Unk21 = br.ReadSByte();
                    br.AssertSByte(0);
                    br.AssertSByte(0);
                    br.AssertPattern(44, 0x00);
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WritePattern(16, 0x00);
                    bw.WriteSingle(TransitionTime);
                    bw.WriteInt32(0);
                    bw.WriteSByte(GparamSubID_Base);
                    bw.WriteSByte(GparamSubID_Override1);
                    bw.WriteSByte(GparamSubID_Override2);
                    bw.WriteSByte(GparamSubID_Override3);
                    bw.WriteSByte(Unk1C);
                    bw.WriteSByte(Unk1D);
                    bw.WriteSByte(0);
                    bw.WriteSByte(0);
                    bw.WriteSByte(Unk20);
                    bw.WriteSByte(Unk21);
                    bw.WriteSByte(0);
                    bw.WriteSByte(0);
                    bw.WritePattern(44, 0x00);
                }
            }

            /// <summary>
            /// Unknown. Grass related?
            /// </summary>
            public class GrassConfig
            {
                /// <summary>
                /// Unknown.
                /// </summary>
                [MSBParamReference(ParamName = "GrassTypeParam")]
                public int GrassParamId0 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                [MSBParamReference(ParamName = "GrassTypeParam")]
                public int GrassParamId1 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                [MSBParamReference(ParamName = "GrassTypeParam")]
                public int GrassParamId2 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                [MSBParamReference(ParamName = "GrassTypeParam")]
                public int GrassParamId3 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                [MSBParamReference(ParamName = "GrassTypeParam")]
                public int GrassParamId4 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                [MSBParamReference(ParamName = "GrassTypeParam")]
                public int GrassParamId5 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk18 { get; set; }

                /// <summary>
                /// Creates an UnkStruct7 with default values.
                /// </summary>
                public GrassConfig() { }

                /// <summary>
                /// Creates a deep copy of the struct.
                /// </summary>
                public GrassConfig DeepCopy()
                {
                    return (GrassConfig)MemberwiseClone();
                }

                internal GrassConfig(BinaryReaderEx br)
                {
                    GrassParamId0 = br.ReadInt32();
                    GrassParamId1 = br.ReadInt32();
                    GrassParamId2 = br.ReadInt32();
                    GrassParamId3 = br.ReadInt32();
                    GrassParamId4 = br.ReadInt32();
                    GrassParamId5 = br.ReadInt32();
                    Unk18 = br.ReadInt32();
                    br.AssertInt32(0);
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteInt32(GrassParamId0);
                    bw.WriteInt32(GrassParamId1);
                    bw.WriteInt32(GrassParamId2);
                    bw.WriteInt32(GrassParamId3);
                    bw.WriteInt32(GrassParamId4);
                    bw.WriteInt32(GrassParamId5);
                    bw.WriteInt32(Unk18);
                    bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class UnkStruct8
            {
                /// <summary>
                /// Unknown.
                /// </summary>
                [IgnoreProperty]
                public int Unk00 { get; set; }

                /// <summary>
                /// Creates an UnkStruct7 with default values.
                /// </summary>
                public UnkStruct8() { }

                /// <summary>
                /// Creates a deep copy of the struct.
                /// </summary>
                public UnkStruct8 DeepCopy()
                {
                    return (UnkStruct8)MemberwiseClone();
                }

                internal UnkStruct8(BinaryReaderEx br)
                {
                    Unk00 = br.AssertInt32([0, 1]);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class UnkStruct9
            {
                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk00 { get; set; }

                /// <summary>
                /// Creates an UnkStruct7 with default values.
                /// </summary>
                public UnkStruct9() { }

                /// <summary>
                /// Creates a deep copy of the struct.
                /// </summary>
                public UnkStruct9 DeepCopy()
                {
                    return (UnkStruct9)MemberwiseClone();
                }

                internal UnkStruct9(BinaryReaderEx br)
                {
                    Unk00 = br.ReadInt32();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class TileLoadConfig
            {
                /// <summary>
                /// Unknown.
                /// </summary>
                public byte[] MapID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                [IgnoreProperty]
                public int Unk04 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                [IgnoreProperty]
                public int Unk0C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                [IgnoreProperty]
                public int Unk10 { get; set; }

                /// <summary>
                /// Unknown culling behaviour field for extreme height differences. Values 0000 - 10000.
                /// </summary>
                public int CullingHeightBehavior { get; set; }

                /// <summary>
                /// Creates an UnkStruct7 with default values.
                /// </summary>
                public TileLoadConfig()
                {
                    MapID = new byte[4];
                }

                /// <summary>
                /// Creates a deep copy of the struct.
                /// </summary>
                public TileLoadConfig DeepCopy()
                {
                    var unks10 = (TileLoadConfig)MemberwiseClone();
                    unks10.MapID = (byte[])MapID.Clone();
                    return unks10;
                }

                internal TileLoadConfig(BinaryReaderEx br)
                {
                    MapID = br.ReadBytes(4);
                    Unk04 = br.ReadInt32();
                    br.AssertInt32(0);
                    Unk0C = br.ReadInt32();
                    Unk10 = br.AssertInt32([0, 1]);
                    CullingHeightBehavior = br.ReadInt32();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteBytes(MapID);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(0);
                    bw.WriteInt32(Unk0C);
                    bw.WriteInt32(Unk10);
                    bw.WriteInt32(CullingHeightBehavior);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class UnkStruct11
            {
                /// <summary>
                /// Unknown.
                /// </summary>
                [IgnoreProperty]
                public int Unk00 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                [IgnoreProperty]
                public int Unk04 { get; set; }

                /// <summary>
                /// Creates an UnkStruct7 with default values.
                /// </summary>
                public UnkStruct11() { }

                /// <summary>
                /// Creates a deep copy of the struct.
                /// </summary>
                public UnkStruct11 DeepCopy()
                {
                    return (UnkStruct11)MemberwiseClone();
                }

                internal UnkStruct11(BinaryReaderEx br)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadInt32();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// Fixed visual geometry. Doesn't seem used much in ER?
            /// </summary>
            public class MapPiece : Part
            {
                private protected override PartType Type => PartType.MapPiece;
                private protected override bool HasUnk1 => true;
                private protected override bool HasUnk2 => false;
                private protected override bool HasGparamConfig => true;
                private protected override bool HasSceneGparamConfig => false;
                private protected override bool HasGrassConfig => true;
                private protected override bool HasUnk8 => true;
                private protected override bool HasUnk9 => true;
                private protected override bool HasTileLoadConfig => true;
                private protected override bool HasUnk11 => true;

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct1 Unk1 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public GparamConfig Gparam { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public GrassConfig Grass { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct8 Unk8 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct9 Unk9 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public TileLoadConfig TileLoad { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct11 Unk11 { get; set; }

                /// <summary>
                /// Creates a MapPiece with default values.
                /// </summary>
                public MapPiece() : base("mXXXXXX_XXXX")
                {
                    Unk1 = new UnkStruct1();
                    Gparam = new GparamConfig();
                    Grass = new GrassConfig();
                    Unk8 = new UnkStruct8();
                    Unk9 = new UnkStruct9();
                    TileLoad = new TileLoadConfig();
                    Unk11 = new UnkStruct11();
                }

                private protected override void DeepCopyTo(Part part)
                {
                    var piece = (MapPiece)part;
                    piece.Unk1 = Unk1.DeepCopy();
                    piece.Gparam = Gparam.DeepCopy();
                    piece.Grass = Grass.DeepCopy();
                    piece.Unk8 = Unk8.DeepCopy();
                    piece.Unk9 = Unk9.DeepCopy();
                    piece.TileLoad = TileLoad.DeepCopy();
                    piece.Unk11 = Unk11.DeepCopy();
                }

                internal MapPiece(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                private protected override void ReadUnk1(BinaryReaderEx br) => Unk1 = new UnkStruct1(br);
                private protected override void ReadGparamConfig(BinaryReaderEx br) => Gparam = new GparamConfig(br);
                private protected override void ReadGrassConfig(BinaryReaderEx br) => Grass = new GrassConfig(br);
                private protected override void ReadUnk8(BinaryReaderEx br) => Unk8 = new UnkStruct8(br);
                private protected override void ReadUnk9(BinaryReaderEx br) => Unk9 = new UnkStruct9(br);
                private protected override void ReadTileLoad(BinaryReaderEx br) => TileLoad = new TileLoadConfig(br);
                private protected override void ReadUnk11(BinaryReaderEx br) => Unk11 = new UnkStruct11(br);

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }

                private protected override void WriteUnk1(BinaryWriterEx bw) => Unk1.Write(bw);
                private protected override void WriteGparamConfig(BinaryWriterEx bw) => Gparam.Write(bw);
                private protected override void WriteGrassConfig(BinaryWriterEx bw) => Grass.Write(bw);
                private protected override void WriteUnk8(BinaryWriterEx bw) => Unk8.Write(bw);
                private protected override void WriteUnk9(BinaryWriterEx bw) => Unk9.Write(bw);
                private protected override void WriteTileLoad(BinaryWriterEx bw) => TileLoad.Write(bw);
                private protected override void WriteUnk11(BinaryWriterEx bw) => Unk11.Write(bw);
            }

            /// <summary>
            /// Common base data for enemies and dummy enemies.
            /// </summary>
            public abstract class EnemyBase : Part
            {
                private protected override bool HasUnk1 => true;
                private protected override bool HasUnk2 => false;
                private protected override bool HasGparamConfig => true;
                private protected override bool HasSceneGparamConfig => false;
                private protected override bool HasGrassConfig => false;
                private protected override bool HasUnk8 => true;
                private protected override bool HasUnk9 => false;
                private protected override bool HasTileLoadConfig => true;
                private protected override bool HasUnk11 => false;

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct1 Unk1 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public GparamConfig Gparam { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct8 Unk8 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public TileLoadConfig TileLoad { get; set; }

                /// <summary>
                /// An ID in NPCThinkParam that determines the enemy's AI characteristics.
                /// </summary>
                [EnemyProperty]
                [MSBParamReference(ParamName = "NpcThinkParam")]
                public int ThinkParamID { get; set; }

                /// <summary>
                /// An ID in NPCParam that determines a variety of enemy properties.
                /// </summary>
                [EnemyProperty]
                [MSBParamReference(ParamName = "NpcParam")]
                public int NPCParamID { get; set; }

                /// <summary>
                /// Talk ID
                /// </summary>
                [EnemyProperty]
                public int TalkID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                [IgnoreProperty]
                [EnemyProperty]
                public bool UnkT15 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                [EnemyProperty]
                public short PlatoonID { get; set; }

                /// <summary>
                /// An ID in CharaInitParam that determines a human's inventory and stats.
                /// </summary>
                [EnemyProperty]
                [MSBParamReference(ParamName = "CharaInitParam")]
                public int CharaInitID { get; set; }

                /// <summary>
                /// Should reference the collision the enemy starts on.
                /// </summary>
                [EnemyProperty]
                [MSBReference(ReferenceType = typeof(Collision))]
                public string CollisionPartName { get; set; }

                [IndexProperty]
                [XmlIgnore]
                private int CollisionPartIndex { get; set; }

                /// <summary>
                /// Walk route followed by this enemy.
                /// </summary>
                [EnemyProperty]
                [MSBReference(ReferenceType = typeof(Event.PatrolInfo))]
                public string WalkRouteName { get; set; }

                [IndexProperty]
                [XmlIgnore]
                private short WalkRouteIndex { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                [IgnoreProperty]
                public int UnkT24 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                [IgnoreProperty]
                public int UnkT28 { get; set; }

                /// <summary>
                /// ID in ChrActivateConditionParam that affects enemy appearance conditions.
                /// </summary>
                [EnemyProperty]
                [MSBParamReference(ParamName = "ChrActivateConditionParam")]
                public int ChrActivateCondParamID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                [IgnoreProperty]
                public int UnkT34 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                [EnemyProperty]
                public int BackupEventAnimID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                [IgnoreProperty]
                public int UnkT3C { get; set; }

                /// <summary>
                /// Refers to SpEffectSetParam ID. Applies SpEffects to an enemy.
                /// </summary>
                [EnemyProperty]
                [MSBParamReference(ParamName = "SpEffectSetParam")]
                public int[] SpEffectSetParamID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                [IgnoreProperty]
                public float UnkT84 { get; set; }

                private protected EnemyBase() : base("cXXXX_XXXX")
                {
                    Unk1 = new UnkStruct1();
                    Gparam = new GparamConfig();
                    Unk8 = new UnkStruct8();
                    TileLoad = new TileLoadConfig();
                    SpEffectSetParamID = new int[4];
                    ThinkParamID = -1;
                    NPCParamID = -1;
                    TalkID = -1;
                    UnkT15 = false;
                    PlatoonID = -1;
                    CharaInitID = -1;
                    BackupEventAnimID = -1;
                    UnkT24 = -1;
                }

                private protected override void DeepCopyTo(Part part)
                {
                    var enemy = (EnemyBase)part;
                    enemy.Unk1 = Unk1.DeepCopy();
                    enemy.Gparam = Gparam.DeepCopy();
                    enemy.Unk8 = Unk8.DeepCopy();
                    enemy.TileLoad = TileLoad.DeepCopy();
                    enemy.SpEffectSetParamID = (int[])SpEffectSetParamID.Clone();
                }

                private protected EnemyBase(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    ThinkParamID = br.ReadInt32();
                    NPCParamID = br.ReadInt32();
                    TalkID = br.ReadInt32();
                    br.AssertByte(0);
                    UnkT15 = br.ReadBoolean();
                    PlatoonID = br.ReadInt16();
                    CharaInitID = br.ReadInt32();
                    CollisionPartIndex = br.ReadInt32();
                    WalkRouteIndex = br.ReadInt16();
                    br.AssertInt16(0);
                    UnkT24 = br.ReadInt32();
                    UnkT28 = br.ReadInt32();
                    ChrActivateCondParamID = br.ReadInt32();
                    br.AssertInt32(0);
                    UnkT34 = br.ReadInt32();
                    BackupEventAnimID = br.ReadInt32();
                    UnkT3C = br.ReadInt32();
                    SpEffectSetParamID = br.ReadInt32s(4);
                    br.AssertPattern(40, 0);
                    br.AssertUInt64(0x80);
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

                private protected override void ReadUnk1(BinaryReaderEx br) => Unk1 = new UnkStruct1(br);

                private protected override void ReadGparamConfig(BinaryReaderEx br) => Gparam = new GparamConfig(br);

                private protected override void ReadUnk8(BinaryReaderEx br) => Unk8 = new UnkStruct8(br);

                private protected override void ReadTileLoad(BinaryReaderEx br) => TileLoad = new TileLoadConfig(br);

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(ThinkParamID);
                    bw.WriteInt32(NPCParamID);
                    bw.WriteInt32(TalkID);
                    bw.WriteByte(0);
                    bw.WriteBoolean(UnkT15);
                    bw.WriteInt16(PlatoonID);
                    bw.WriteInt32(CharaInitID);
                    bw.WriteInt32(CollisionPartIndex);
                    bw.WriteInt16(WalkRouteIndex);
                    bw.WriteInt16(0);
                    bw.WriteInt32(UnkT24);
                    bw.WriteInt32(UnkT28);
                    bw.WriteInt32(ChrActivateCondParamID);
                    bw.WriteInt32(0);
                    bw.WriteInt32(UnkT34);
                    bw.WriteInt32(BackupEventAnimID);
                    bw.WriteInt32(UnkT3C);
                    bw.WriteInt32s(SpEffectSetParamID);
                    bw.WritePattern(40, 0x00);
                    bw.WriteInt64(0x80);
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

                private protected override void WriteUnk1(BinaryWriterEx bw) => Unk1.Write(bw);

                private protected override void WriteGparamConfig(BinaryWriterEx bw) => Gparam.Write(bw);

                private protected override void WriteUnk8(BinaryWriterEx bw) => Unk8.Write(bw);

                private protected override void WriteTileLoad(BinaryWriterEx bw) => TileLoad.Write(bw);

                internal override void GetNames(MSBE msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    CollisionPartName = MSB.FindName(entries.Parts, CollisionPartIndex);
                    WalkRouteName = MSB.FindName(msb.Events.PatrolInfo, WalkRouteIndex);
                }

                internal override void GetIndices(MSBE msb, Entries entries)
                {
                    base.GetIndices(msb, entries);
                    CollisionPartIndex = MSB.FindIndex(this, entries.Parts, CollisionPartName);
                    WalkRouteIndex = (short)MSB.FindIndex(this, msb.Events.PatrolInfo, WalkRouteName);
                }
            }

            /// <summary>
            /// Any non-player character.
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
            /// A spawn point for the player, or something.
            /// </summary>
            public class Player : Part
            {
                private protected override PartType Type => PartType.Player;
                private protected override bool HasUnk1 => true;
                private protected override bool HasUnk2 => false;
                private protected override bool HasGparamConfig => false;
                private protected override bool HasSceneGparamConfig => false;
                private protected override bool HasGrassConfig => false;
                private protected override bool HasUnk8 => true;
                private protected override bool HasUnk9 => false;
                private protected override bool HasTileLoadConfig => true;
                private protected override bool HasUnk11 => false;

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct1 Unk1 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct8 Unk8 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public TileLoadConfig TileLoad { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                [IgnoreProperty]
                public int Unk00 { get; set; }

                /// <summary>
                /// Creates a Player with default values.
                /// </summary>
                public Player() : base("c0000_XXXX")
                {
                    Unk1 = new UnkStruct1();
                    Unk8 = new UnkStruct8();
                    TileLoad = new TileLoadConfig();
                }

                private protected override void DeepCopyTo(Part part)
                {
                    var player = (Player)part;
                    player.Unk1 = Unk1.DeepCopy();
                    player.Unk8 = Unk8.DeepCopy();
                    player.TileLoad = TileLoad.DeepCopy();
                }

                internal Player(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    Unk00 = br.ReadInt32();
                    br.AssertPattern(0x0C, 0x00);
                }

                private protected override void ReadUnk1(BinaryReaderEx br) => Unk1 = new UnkStruct1(br);

                private protected override void ReadUnk8(BinaryReaderEx br) => Unk8 = new UnkStruct8(br);

                private protected override void ReadTileLoad(BinaryReaderEx br) => TileLoad = new TileLoadConfig(br);

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WritePattern(0x0C, 0x00);
                }

                private protected override void WriteUnk1(BinaryWriterEx bw) => Unk1.Write(bw);

                private protected override void WriteUnk8(BinaryWriterEx bw) => Unk8.Write(bw);

                private protected override void WriteTileLoad(BinaryWriterEx bw) => TileLoad.Write(bw);
            }

            /// <summary>
            /// Invisible but physical geometry.
            /// </summary>
            public class Collision : Part
            {
                /// <summary>
                /// HitFilterType
                /// </summary>
                public enum HitFilterType : byte
                {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
                    Standard = 8,
                    CameraOnly = 9,
                    EnemyOnly = 11,
                    FallDeathCam = 13,
                    Kill = 15,
                    Unk16 = 16,
                    Unk17 = 17,
                    Unk19 = 19,
                    Unk20 = 20,
                    Unk21 = 21,
                    Unk22 = 22,
                    Unk23 = 23,
                    Unk24 = 24,
                    Unk29 = 29,
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
                }

                private protected override PartType Type => PartType.Collision;
                private protected override bool HasUnk1 => true;
                private protected override bool HasUnk2 => true;
                private protected override bool HasGparamConfig => true;
                private protected override bool HasSceneGparamConfig => true;
                private protected override bool HasGrassConfig => false;
                private protected override bool HasUnk8 => true;
                private protected override bool HasUnk9 => false;
                private protected override bool HasTileLoadConfig => true;
                private protected override bool HasUnk11 => true;

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct1 Unk1 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct2 Unk2 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public GparamConfig Gparam { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public SceneGparamConfig SceneGparam { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct8 Unk8 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public TileLoadConfig TileLoad { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct11 Unk11 { get; set; }

                /// <summary>
                /// Sets collision behavior. Fall collision, death collision, enemy-only collision, etc.
                /// </summary>
                public HitFilterType HitFilterID { get; set; } = HitFilterType.Standard;

                /// <summary>
                /// Unknown.
                /// </summary>
                [IgnoreProperty]
                public byte UnkT01 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                [IgnoreProperty]
                public byte UnkT02 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                [IgnoreProperty]
                public bool UnkT03 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                [IgnoreProperty]
                public float UnkT04 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                [IgnoreProperty]
                public int UnkT06 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                [IgnoreProperty]
                public float UnkT14 { get; set; }

                /// <summary>
                /// ID of location text to display when stepping onto this collision.
                /// </summary>
                public int LocationTextID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                [IgnoreProperty]
                public int UnkT1C { get; set; }

                /// <summary>
                /// Used to determine invasion eligibility.
                /// </summary>
                [MSBParamReference(ParamName = "PlayRegionParam")]
                public int PlayRegionID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                [IgnoreProperty]
                public short UnkT24 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                [IgnoreProperty]
                public short UnkT26 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                [IgnoreProperty]
                public int UnkT30 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                [IgnoreProperty]
                public byte UnkT34 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                [IgnoreProperty]
                public byte UnkT35 { get; set; }

                /// <summary>
                /// Disable being able to summon/ride Torrent.
                /// </summary>
                public bool DisableTorrent { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                [IgnoreProperty]
                public short UnkT3C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                [IgnoreProperty]
                public short UnkT3E { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                [IgnoreProperty]
                public float UnkT40 { get; set; }

                /// <summary>
                /// Disables Fast Travel if Event Flag is not set.
                /// </summary>
                public uint EnableFastTravelEventFlagID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                [IgnoreProperty]
                public short UnkT4C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                [IgnoreProperty]
                public short UnkT4E { get; set; }

                /// <summary>
                /// Creates a Collision with default values.
                /// </summary>
                public Collision() : base("hXXXXXX")
                {
                    Unk1 = new UnkStruct1();
                    Unk2 = new UnkStruct2();
                    Gparam = new GparamConfig();
                    SceneGparam = new SceneGparamConfig();
                    Unk8 = new UnkStruct8();
                    TileLoad = new TileLoadConfig();
                    Unk11 = new UnkStruct11();
                }

                private protected override void DeepCopyTo(Part part)
                {
                    var collision = (Collision)part;
                    collision.Unk1 = Unk1.DeepCopy();
                    collision.Unk2 = Unk2.DeepCopy();
                    collision.Gparam = Gparam.DeepCopy();
                    collision.SceneGparam = SceneGparam.DeepCopy();
                    collision.Unk8 = Unk8.DeepCopy();
                    collision.TileLoad = TileLoad.DeepCopy();
                    collision.Unk11 = Unk11.DeepCopy();
                }

                internal Collision(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    HitFilterID = br.ReadEnum8<HitFilterType>();
                    UnkT01 = br.ReadByte();
                    UnkT02 = br.ReadByte();
                    UnkT03 = br.ReadBoolean();
                    UnkT04 = br.ReadSingle();
                    UnkT06 = br.ReadInt32();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    UnkT14 = br.ReadSingle();
                    LocationTextID = br.ReadInt32();
                    UnkT1C = br.ReadInt32();
                    PlayRegionID = br.ReadInt32();
                    UnkT24 = br.ReadInt16();
                    UnkT26 = br.AssertInt16([0, 1]);
                    br.AssertInt32(0);
                    br.AssertInt32(-1);
                    UnkT30 = br.ReadInt32();
                    UnkT34 = br.ReadByte();
                    UnkT35 = br.ReadByte();
                    DisableTorrent = br.ReadBoolean();
                    br.AssertByte(0);
                    br.AssertInt32(-1);
                    UnkT3C = br.ReadInt16();
                    UnkT3E = br.ReadInt16();
                    UnkT40 = br.ReadSingle();
                    br.AssertInt32(0);
                    EnableFastTravelEventFlagID = br.ReadUInt32();
                    UnkT4C = br.AssertInt16([0, 1, 2]);
                    UnkT4E = br.ReadInt16();
                }

                private protected override void ReadUnk1(BinaryReaderEx br) => Unk1 = new UnkStruct1(br);
                private protected override void ReadUnk2(BinaryReaderEx br) => Unk2 = new UnkStruct2(br);
                private protected override void ReadGparamConfig(BinaryReaderEx br) => Gparam = new GparamConfig(br);
                private protected override void ReadSceneGparamConfig(BinaryReaderEx br) => SceneGparam = new SceneGparamConfig(br);
                private protected override void ReadUnk8(BinaryReaderEx br) => Unk8 = new UnkStruct8(br);
                private protected override void ReadTileLoad(BinaryReaderEx br) => TileLoad = new TileLoadConfig(br);
                private protected override void ReadUnk11(BinaryReaderEx br) => Unk11 = new UnkStruct11(br);

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteByte((byte)HitFilterID);
                    bw.WriteByte(UnkT01);
                    bw.WriteByte(UnkT02);
                    bw.WriteBoolean(UnkT03);
                    bw.WriteSingle(UnkT04);
                    bw.WriteInt32(UnkT06);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteSingle(UnkT14);
                    bw.WriteInt32(LocationTextID);
                    bw.WriteInt32(UnkT1C);
                    bw.WriteInt32(PlayRegionID);
                    bw.WriteInt16(UnkT24);
                    bw.WriteInt16(UnkT26);
                    bw.WriteInt32(0);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(UnkT30);
                    bw.WriteByte(UnkT34);
                    bw.WriteByte(UnkT35);
                    bw.WriteBoolean(DisableTorrent);
                    bw.WriteByte(0);
                    bw.WriteInt32(-1);
                    bw.WriteInt16(UnkT3C);
                    bw.WriteInt16(UnkT3E);
                    bw.WriteSingle(UnkT40);
                    bw.WriteInt32(0);
                    bw.WriteUInt32(EnableFastTravelEventFlagID);
                    bw.WriteInt16(UnkT4C);
                    bw.WriteInt16(UnkT4E);
                }

                private protected override void WriteUnk1(BinaryWriterEx bw) => Unk1.Write(bw);
                private protected override void WriteUnk2(BinaryWriterEx bw) => Unk2.Write(bw);
                private protected override void WriteGparamConfig(BinaryWriterEx bw) => Gparam.Write(bw);
                private protected override void WriteSceneGparamConfig(BinaryWriterEx bw) => SceneGparam.Write(bw);
                private protected override void WriteUnk8(BinaryWriterEx bw) => Unk8.Write(bw);
                private protected override void WriteTileLoad(BinaryWriterEx bw) => TileLoad.Write(bw);
                private protected override void WriteUnk11(BinaryWriterEx bw) => Unk11.Write(bw);
            }

            /// <summary>
            /// This is in the same type of a legacy DummyObject, but struct is pretty gutted
            /// </summary>
            public class DummyAsset : Part
            {
                private protected override PartType Type => PartType.DummyAsset;
                private protected override bool HasUnk1 => true;
                private protected override bool HasUnk2 => false;
                private protected override bool HasGparamConfig => true;
                private protected override bool HasSceneGparamConfig => false;
                private protected override bool HasGrassConfig => false;
                private protected override bool HasUnk8 => true;
                private protected override bool HasUnk9 => false;
                private protected override bool HasTileLoadConfig => true;
                private protected override bool HasUnk11 => false;

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct1 Unk1 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT18 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public GparamConfig Gparam { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct8 Unk8 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public TileLoadConfig TileLoad { get; set; }

                /// <summary>
                /// Creates a MapPiece with default values.
                /// </summary>
                public DummyAsset() : base("AEGxxx_xxx_xxxx")
                {
                    Unk1 = new UnkStruct1();
                    Gparam = new GparamConfig();
                    Unk8 = new UnkStruct8();
                    TileLoad = new TileLoadConfig();
                }

                private protected override void DeepCopyTo(Part part)
                {
                    var asset = (DummyAsset)part;
                    asset.Unk1 = Unk1.DeepCopy();
                    asset.Gparam = Gparam.DeepCopy();
                    asset.Unk8 = Unk8.DeepCopy();
                    asset.TileLoad = TileLoad.DeepCopy();
                }

                internal DummyAsset(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(-1);
                    br.AssertInt32(0);
                    br.AssertInt32(-1);
                    br.AssertInt32(-1);
                    UnkT18 = br.ReadInt32();
                    br.AssertInt32(-1);
                }

                private protected override void ReadUnk1(BinaryReaderEx br) => Unk1 = new UnkStruct1(br);
                private protected override void ReadGparamConfig(BinaryReaderEx br) => Gparam = new GparamConfig(br);
                private protected override void ReadUnk8(BinaryReaderEx br) => Unk8 = new UnkStruct8(br);
                private protected override void ReadTileLoad(BinaryReaderEx br) => TileLoad = new TileLoadConfig(br);

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(0);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(UnkT18);
                    bw.WriteInt32(-1);
                }

                private protected override void WriteUnk1(BinaryWriterEx bw) => Unk1.Write(bw);
                private protected override void WriteGparamConfig(BinaryWriterEx bw) => Gparam.Write(bw);
                private protected override void WriteUnk8(BinaryWriterEx bw) => Unk8.Write(bw);
                private protected override void WriteTileLoad(BinaryWriterEx bw) => TileLoad.Write(bw);
            }

            /// <summary>
            /// An enemy that either isn't used, or is used for a cutscene.
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
            /// References an actual collision and causes another map to be loaded while on it.
            /// </summary>
            public class ConnectCollision : Part
            {
                private protected override PartType Type => PartType.ConnectCollision;
                private protected override bool HasUnk1 => true;
                private protected override bool HasUnk2 => true;
                private protected override bool HasGparamConfig => false;
                private protected override bool HasSceneGparamConfig => false;
                private protected override bool HasGrassConfig => false;
                private protected override bool HasUnk8 => true;
                private protected override bool HasUnk9 => false;
                private protected override bool HasTileLoadConfig => true;
                private protected override bool HasUnk11 => true;

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct1 Unk1 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct2 Unk2 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct8 Unk8 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public TileLoadConfig TileLoad { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct11 Unk11 { get; set; }

                /// <summary>
                /// The collision part to attach to.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Collision))]
                [NoRenderGroupInheritence()]
                public string CollisionName { get; set; }

                [IndexProperty]
                [XmlIgnore]
                private int CollisionIndex { get; set; }

                /// <summary>
                /// The map to load when on this collision.
                /// </summary>
                public byte[] MapID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                [IgnoreProperty]
                public byte UnkT08 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                [IgnoreProperty]
                public bool UnkT09 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                [IgnoreProperty]
                public byte UnkT0A { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                [IgnoreProperty]
                public bool UnkT0B { get; set; }

                /// <summary>
                /// Creates a ConnectCollision with default values.
                /// </summary>
                public ConnectCollision() : base("hXXXXXX_XXXX")
                {
                    Unk1 = new UnkStruct1();
                    Unk2 = new UnkStruct2();
                    MapID = new byte[4];
                    Unk8 = new UnkStruct8();
                    TileLoad = new TileLoadConfig();
                    Unk11 = new UnkStruct11();
                }

                private protected override void DeepCopyTo(Part part)
                {
                    var connect = (ConnectCollision)part;
                    connect.Unk1 = Unk1.DeepCopy();
                    connect.Unk2 = Unk2.DeepCopy();
                    connect.MapID = (byte[])MapID.Clone();
                    connect.Unk8 = Unk8.DeepCopy();
                    connect.TileLoad = TileLoad.DeepCopy();
                    connect.Unk11 = Unk11.DeepCopy();
                }

                internal ConnectCollision(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    CollisionIndex = br.ReadInt32();
                    MapID = br.ReadBytes(4);
                    UnkT08 = br.ReadByte();
                    UnkT09 = br.ReadBoolean();
                    UnkT0A = br.ReadByte();
                    UnkT0B = br.ReadBoolean();
                    br.AssertInt32(0);
                }

                private protected override void ReadUnk1(BinaryReaderEx br) => Unk1 = new UnkStruct1(br);
                private protected override void ReadUnk2(BinaryReaderEx br) => Unk2 = new UnkStruct2(br);
                private protected override void ReadUnk8(BinaryReaderEx br) => Unk8 = new UnkStruct8(br);
                private protected override void ReadTileLoad(BinaryReaderEx br) => TileLoad = new TileLoadConfig(br);
                private protected override void ReadUnk11(BinaryReaderEx br) => Unk11 = new UnkStruct11(br);

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(CollisionIndex);
                    bw.WriteBytes(MapID);
                    bw.WriteByte(UnkT08);
                    bw.WriteBoolean(UnkT09);
                    bw.WriteByte(UnkT0A);
                    bw.WriteBoolean(UnkT0B);
                    bw.WriteInt32(0);
                }

                private protected override void WriteUnk1(BinaryWriterEx bw) => Unk1.Write(bw);
                private protected override void WriteUnk2(BinaryWriterEx bw) => Unk2.Write(bw);
                private protected override void WriteUnk8(BinaryWriterEx bw) => Unk8.Write(bw);
                private protected override void WriteTileLoad(BinaryWriterEx bw) => TileLoad.Write(bw);
                private protected override void WriteUnk11(BinaryWriterEx bw) => Unk11.Write(bw);

                internal override void GetNames(MSBE msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    CollisionName = MSB.FindName(msb.Parts.Collisions, CollisionIndex);
                }

                internal override void GetIndices(MSBE msb, Entries entries)
                {
                    base.GetIndices(msb, entries);
                    CollisionIndex = MSB.FindIndex(this, msb.Parts.Collisions, CollisionName);
                }
            }

            /// <summary>
            /// An asset placement in Elden Ring
            /// </summary>
            public class Asset : Part
            {
                private protected override PartType Type => PartType.Asset;
                private protected override bool HasUnk1 => true;
                private protected override bool HasUnk2 => true;
                private protected override bool HasGparamConfig => true;
                private protected override bool HasSceneGparamConfig => false;
                private protected override bool HasGrassConfig => true;
                private protected override bool HasUnk8 => true;
                private protected override bool HasUnk9 => true;
                private protected override bool HasTileLoadConfig => true;
                private protected override bool HasUnk11 => true;

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct1 Unk1 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct2 Unk2 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public GparamConfig Gparam { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public GrassConfig Grass { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct8 Unk8 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct9 Unk9 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public TileLoadConfig TileLoad { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct11 Unk11 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                [IgnoreProperty]
                public short UnkT00 { get; set; }
                /// <summary>
                /// Unknown.
                /// </summary>
                [IgnoreProperty]
                public short UnkT02 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                [IgnoreProperty]
                public byte UnkT10 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                [IgnoreProperty]
                public bool UnkT11 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                [IgnoreProperty]
                public byte UnkT12 { get; set; }

                /// <summary>
                /// Value added onto model ID determining AssetModelSfxParam to use.
                /// </summary>
                public short AssetSfxParamRelativeID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                [IgnoreProperty]
                public short UnkT1E { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                [IgnoreProperty]
                public int UnkT24 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                [IgnoreProperty]
                public int UnkT28 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                [IgnoreProperty]
                public int UnkT30 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                [IgnoreProperty]
                public int UnkT34 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Part))]
                public string[] UnkPartNames { get; set; }

                [IndexProperty]
                [XmlIgnore]
                private int[] UnkPartIndices { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                [IgnoreProperty]
                public bool UnkT50 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                [IgnoreProperty]
                public byte UnkT51 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                [IgnoreProperty]
                public byte UnkT53 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Part))]
                public string UnkT54PartName { get; set; }
                [XmlIgnore]
                [IndexProperty]
                private int UnkT54PartIndex { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                [EldenRingAssetMask]
                public int UnkModelMaskAndAnimID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                [IgnoreProperty]
                public int UnkT5C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                [IgnoreProperty]
                public int UnkT60 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                [IgnoreProperty]
                public int UnkT64 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public class AssetUnkStruct1
                {
                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    [IgnoreProperty]
                    public short Unk00 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    [IgnoreProperty]
                    public bool Unk04 { get; set; }

                    /// <summary>
                    /// Disable being able to summon/ride Torrent, but only when asset isn't referencing collision DisableTorrent.
                    /// </summary>
                    public bool DisableTorrentAssetOnly { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    [IgnoreProperty]
                    public int Unk1C { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    [IgnoreProperty]
                    public short Unk24 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    [IgnoreProperty]
                    public short Unk26 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    [IgnoreProperty]
                    public int Unk28 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    [IgnoreProperty]
                    public int Unk2C { get; set; }

                    /// <summary>
                    /// Creates an AssetUnkStruct1 with default values.
                    /// </summary>
                    public AssetUnkStruct1() { }

                    /// <summary>
                    /// Creates a deep copy of the struct.
                    /// </summary>
                    public AssetUnkStruct1 DeepCopy()
                    {
                        return (AssetUnkStruct1)MemberwiseClone();
                    }

                    internal AssetUnkStruct1(BinaryReaderEx br)
                    {
                        Unk00 = br.ReadInt16();
                        br.AssertInt16(-1);
                        Unk04 = br.ReadBoolean();
                        DisableTorrentAssetOnly = br.ReadBoolean();
                        br.AssertInt16(-1);
                        br.AssertInt32(0);
                        br.AssertInt32(0);
                        br.AssertInt32(-1);
                        br.AssertInt32(-1);
                        br.AssertInt32(-1);
                        Unk1C = br.ReadInt32();
                        br.AssertInt32(0);
                        Unk24 = br.ReadInt16();
                        Unk26 = br.ReadInt16();
                        Unk28 = br.ReadInt32();
                        Unk2C = br.ReadInt32();
                        br.AssertInt32(0);
                        br.AssertInt32(0);
                        br.AssertInt32(0);
                        br.AssertInt32(0);
                    }

                    internal void Write(BinaryWriterEx bw)
                    {
                        bw.WriteInt16(Unk00);
                        bw.WriteInt16(-1);
                        bw.WriteBoolean(Unk04);
                        bw.WriteBoolean(DisableTorrentAssetOnly);
                        bw.WriteInt16(-1);
                        bw.WriteInt32(0);
                        bw.WriteInt32(0);
                        bw.WriteInt32(-1);
                        bw.WriteInt32(-1);
                        bw.WriteInt32(-1);
                        bw.WriteInt32(Unk1C);
                        bw.WriteInt32(0);
                        bw.WriteInt16(Unk24);
                        bw.WriteInt16(Unk26);
                        bw.WriteInt32(Unk28);
                        bw.WriteInt32(Unk2C);
                        bw.WriteInt32(0);
                        bw.WriteInt32(0);
                        bw.WriteInt32(0);
                        bw.WriteInt32(0);
                    }
                }

                /// <summary>
                /// Unknown.
                /// </summary>
                public class AssetUnkStruct2
                {
                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    [IgnoreProperty]
                    public int Unk00 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    [IgnoreProperty]
                    public int Unk04 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    [IgnoreProperty]
                    public float Unk14 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    [IgnoreProperty]
                    public byte Unk1C { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    [IgnoreProperty]
                    public byte Unk1D { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    [IgnoreProperty]
                    public byte Unk1E { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    [IgnoreProperty]
                    public byte Unk1F { get; set; }

                    /// <summary>
                    /// Creates an AssetUnkStruct2 with default values.
                    /// </summary>
                    public AssetUnkStruct2() { }

                    /// <summary>
                    /// Creates a deep copy of the struct.
                    /// </summary>
                    public AssetUnkStruct2 DeepCopy()
                    {
                        return (AssetUnkStruct2)MemberwiseClone();
                    }

                    internal AssetUnkStruct2(BinaryReaderEx br)
                    {
                        Unk00 = br.ReadInt32();
                        Unk04 = br.ReadInt32();
                        br.AssertInt32(-1);
                        br.AssertInt32(0);
                        br.AssertInt32(0);
                        Unk14 = br.ReadSingle();
                        br.AssertInt32(0);
                        Unk1C = br.ReadByte();
                        Unk1D = br.ReadByte();
                        Unk1E = br.ReadByte();
                        Unk1F = br.ReadByte();
                        br.AssertInt32(0);
                        br.AssertInt32(0);
                        br.AssertInt32(0);
                        br.AssertInt32(0);
                        br.AssertInt32(0);
                        br.AssertInt32(0);
                        br.AssertInt32(0);
                        br.AssertInt32(0);
                    }

                    internal void Write(BinaryWriterEx bw)
                    {
                        bw.WriteInt32(Unk00);
                        bw.WriteInt32(Unk04);
                        bw.WriteInt32(-1);
                        bw.WriteInt32(0);
                        bw.WriteInt32(0);
                        bw.WriteSingle(Unk14);
                        bw.WriteInt32(0);
                        bw.WriteByte(Unk1C);
                        bw.WriteByte(Unk1D);
                        bw.WriteByte(Unk1E);
                        bw.WriteByte(Unk1F);
                        bw.WriteInt32(0);
                        bw.WriteInt32(0);
                        bw.WriteInt32(0);
                        bw.WriteInt32(0);
                        bw.WriteInt32(0);
                        bw.WriteInt32(0);
                        bw.WriteInt32(0);
                        bw.WriteInt32(0);
                    }
                }

                /// <summary>
                /// Unknown.
                /// </summary>
                public class AssetUnkStruct3
                {
                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    [IgnoreProperty]
                    public int Unk00 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    [IgnoreProperty]
                    public float Unk04 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    [IgnoreProperty]
                    public byte Unk09 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    [IgnoreProperty]
                    public byte Unk0A { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    [IgnoreProperty]
                    public byte Unk0B { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    [IgnoreProperty]
                    public short Unk0C { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    [IgnoreProperty]
                    public short Unk0E { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    [IgnoreProperty]
                    public float Unk10 { get; set; }

                    /// <summary>
                    /// Disables the asset when the specified map is loaded.
                    /// </summary>
                    public sbyte[] DisableWhenMapLoadedMapID { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    [IgnoreProperty]
                    public int Unk18 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    [IgnoreProperty]
                    public int Unk1C { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    [IgnoreProperty]
                    public int Unk20 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    [IgnoreProperty]
                    public byte Unk24 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    [IgnoreProperty]
                    public bool Unk25 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    [IgnoreProperty]
                    public int Unk28 { get; set; }

                    /// <summary>
                    /// Creates an AssetUnkStruct3 with default values.
                    /// </summary>
                    public AssetUnkStruct3()
                    {
                        DisableWhenMapLoadedMapID = new sbyte[4];
                    }

                    /// <summary>
                    /// Creates a deep copy of the struct.
                    /// </summary>
                    public AssetUnkStruct3 DeepCopy()
                    {
                        var unks3 = (AssetUnkStruct3)MemberwiseClone();
                        unks3.DisableWhenMapLoadedMapID = (sbyte[])DisableWhenMapLoadedMapID.Clone();
                        return unks3;
                    }

                    internal AssetUnkStruct3(BinaryReaderEx br)
                    {
                        Unk00 = br.ReadInt32();
                        Unk04 = br.ReadSingle();
                        br.AssertSByte(-1);
                        Unk09 = br.ReadByte();
                        Unk0A = br.ReadByte();
                        Unk0B = br.ReadByte();
                        Unk0C = br.ReadInt16();
                        Unk0E = br.ReadInt16();
                        Unk10 = br.ReadSingle();
                        DisableWhenMapLoadedMapID = br.ReadSBytes(4);
                        Unk18 = br.ReadInt32();
                        Unk1C = br.ReadInt32();
                        Unk20 = br.ReadInt32();
                        Unk24 = br.ReadByte();
                        Unk25 = br.ReadBoolean();
                        br.AssertByte(0);
                        br.AssertByte(0);
                        Unk28 = br.ReadInt32();
                        br.AssertInt32(0);
                        br.AssertInt32(0);
                        br.AssertInt32(0);
                        br.AssertInt32(0);
                        br.AssertInt32(0);
                    }

                    internal void Write(BinaryWriterEx bw)
                    {
                        bw.WriteInt32(Unk00);
                        bw.WriteSingle(Unk04);
                        bw.WriteSByte(-1);
                        bw.WriteByte(Unk09);
                        bw.WriteByte(Unk0A);
                        bw.WriteByte(Unk0B);
                        bw.WriteInt16(Unk0C);
                        bw.WriteInt16(Unk0E);
                        bw.WriteSingle(Unk10);
                        bw.WriteSBytes(DisableWhenMapLoadedMapID);
                        bw.WriteInt32(Unk18);
                        bw.WriteInt32(Unk1C);
                        bw.WriteInt32(Unk20);
                        bw.WriteByte(Unk24);
                        bw.WriteBoolean(Unk25);
                        bw.WriteByte(0);
                        bw.WriteByte(0);
                        bw.WriteInt32(Unk28);
                        bw.WriteInt32(0);
                        bw.WriteInt32(0);
                        bw.WriteInt32(0);
                        bw.WriteInt32(0);
                        bw.WriteInt32(0);
                    }
                }

                /// <summary>
                /// Unknown.
                /// </summary>
                public class AssetUnkStruct4
                {
                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    [IgnoreProperty]
                    public bool Unk00 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    [IgnoreProperty]
                    public byte Unk01 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    [IgnoreProperty]
                    public byte Unk02 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    [IgnoreProperty]
                    public bool Unk03 { get; set; }

                    /// <summary>
                    /// Creates an AssetUnkStruct4 with default values.
                    /// </summary>
                    public AssetUnkStruct4() { }

                    /// <summary>
                    /// Creates a deep copy of the struct.
                    /// </summary>
                    public AssetUnkStruct4 DeepCopy()
                    {
                        return (AssetUnkStruct4)MemberwiseClone();
                    }

                    internal AssetUnkStruct4(BinaryReaderEx br)
                    {
                        Unk00 = br.ReadBoolean();
                        Unk01 = br.ReadByte();
                        Unk02 = br.ReadByte();
                        Unk03 = br.ReadBoolean();
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
                        br.AssertInt32(0);
                    }

                    internal void Write(BinaryWriterEx bw)
                    {
                        bw.WriteBoolean(Unk00);
                        bw.WriteByte(Unk01);
                        bw.WriteByte(Unk02);
                        bw.WriteBoolean(Unk03);
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
                        bw.WriteInt32(0);
                    }
                }

                /// <summary>
                /// Unknown.
                /// </summary>
                public AssetUnkStruct1 AssetUnk1 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public AssetUnkStruct2 AssetUnk2 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public AssetUnkStruct3 AssetUnk3 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public AssetUnkStruct4 AssetUnk4 { get; set; }

                /// <summary>
                /// Creates an Asset with default values.
                /// </summary>
                public Asset() : base("AEGxxx_xxx_xxxx")
                {
                    Unk1 = new UnkStruct1();
                    Unk2 = new UnkStruct2();
                    Gparam = new GparamConfig();
                    Grass = new GrassConfig();
                    Unk8 = new UnkStruct8();
                    Unk9 = new UnkStruct9();
                    TileLoad = new TileLoadConfig();
                    Unk11 = new UnkStruct11();

                    AssetUnk1 = new AssetUnkStruct1();
                    AssetUnk2 = new AssetUnkStruct2();
                    AssetUnk3 = new AssetUnkStruct3();
                    AssetUnk4 = new AssetUnkStruct4();

                    UnkPartNames = new string[6];
                }

                private protected override void DeepCopyTo(Part part)
                {
                    var asset = (Asset)part;
                    asset.Unk1 = Unk1.DeepCopy();
                    asset.Unk2 = Unk2.DeepCopy();
                    asset.Gparam = Gparam.DeepCopy();
                    asset.Grass = Grass.DeepCopy();
                    asset.Unk8 = Unk8.DeepCopy();
                    asset.Unk9 = Unk9.DeepCopy();
                    asset.TileLoad = TileLoad.DeepCopy();
                    asset.Unk11 = Unk11.DeepCopy();

                    asset.AssetUnk1 = AssetUnk1.DeepCopy();
                    asset.AssetUnk2 = AssetUnk2.DeepCopy();
                    asset.AssetUnk3 = AssetUnk3.DeepCopy();
                    asset.AssetUnk4 = AssetUnk4.DeepCopy();

                    UnkPartNames = (string[])UnkPartNames.Clone();
                }

                internal Asset(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    UnkT00 = br.ReadInt16();
                    UnkT02 = br.AssertInt16([0, 1]);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    UnkT10 = br.ReadByte();
                    UnkT11 = br.ReadBoolean();
                    UnkT12 = br.ReadByte();
                    br.AssertByte(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    AssetSfxParamRelativeID = br.ReadInt16();
                    UnkT1E = br.ReadInt16();
                    br.AssertInt32(-1);
                    UnkT24 = br.ReadInt32();
                    UnkT28 = br.ReadInt32();
                    br.AssertInt32(0);
                    UnkT30 = br.ReadInt32();
                    UnkT34 = br.ReadInt32();
                    UnkPartIndices = br.ReadInt32s(6);
                    UnkT50 = br.ReadBoolean();
                    UnkT51 = br.ReadByte();
                    br.AssertByte(0);
                    UnkT53 = br.ReadByte();
                    UnkT54PartIndex = br.ReadInt32();
                    UnkModelMaskAndAnimID = br.ReadInt32();
                    UnkT5C = br.ReadInt32();
                    UnkT60 = br.ReadInt32();
                    UnkT64 = br.ReadInt32();

                    // Offsets for embedded structs that are fortunately always the same
                    br.AssertInt64(0x88);
                    br.AssertInt64(0xC8);
                    br.AssertInt64(0x108);
                    br.AssertInt64(0x148);

                    AssetUnk1 = new AssetUnkStruct1(br);
                    AssetUnk2 = new AssetUnkStruct2(br);
                    AssetUnk3 = new AssetUnkStruct3(br);
                    AssetUnk4 = new AssetUnkStruct4(br);
                }

                private protected override void ReadUnk1(BinaryReaderEx br) => Unk1 = new UnkStruct1(br);
                private protected override void ReadUnk2(BinaryReaderEx br) => Unk2 = new UnkStruct2(br);
                private protected override void ReadGparamConfig(BinaryReaderEx br) => Gparam = new GparamConfig(br);
                private protected override void ReadGrassConfig(BinaryReaderEx br) => Grass = new GrassConfig(br);
                private protected override void ReadUnk8(BinaryReaderEx br) => Unk8 = new UnkStruct8(br);
                private protected override void ReadUnk9(BinaryReaderEx br) => Unk9 = new UnkStruct9(br);
                private protected override void ReadTileLoad(BinaryReaderEx br) => TileLoad = new TileLoadConfig(br);
                private protected override void ReadUnk11(BinaryReaderEx br) => Unk11 = new UnkStruct11(br);

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt16(UnkT00);
                    bw.WriteInt16(UnkT02);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteByte(UnkT10);
                    bw.WriteBoolean(UnkT11);
                    bw.WriteByte(UnkT12);
                    bw.WriteByte(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt16(AssetSfxParamRelativeID);
                    bw.WriteInt16(UnkT1E);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(UnkT24);
                    bw.WriteInt32(UnkT28);
                    bw.WriteInt32(0);
                    bw.WriteInt32(UnkT30);
                    bw.WriteInt32(UnkT34);
                    bw.WriteInt32s(UnkPartIndices);
                    bw.WriteBoolean(UnkT50);
                    bw.WriteByte(UnkT51);
                    bw.WriteByte(0);
                    bw.WriteByte(UnkT53);
                    bw.WriteInt32(UnkT54PartIndex);
                    bw.WriteInt32(UnkModelMaskAndAnimID);
                    bw.WriteInt32(UnkT5C);
                    bw.WriteInt32(UnkT60);
                    bw.WriteInt32(UnkT64);

                    bw.WriteInt64(0x88);
                    bw.WriteInt64(0xC8);
                    bw.WriteInt64(0x108);
                    bw.WriteInt64(0x148);

                    AssetUnk1.Write(bw);
                    AssetUnk2.Write(bw);
                    AssetUnk3.Write(bw);
                    AssetUnk4.Write(bw);
                }

                private protected override void WriteUnk1(BinaryWriterEx bw) => Unk1.Write(bw);
                private protected override void WriteUnk2(BinaryWriterEx bw) => Unk2.Write(bw);
                private protected override void WriteGparamConfig(BinaryWriterEx bw) => Gparam.Write(bw);
                private protected override void WriteGrassConfig(BinaryWriterEx bw) => Grass.Write(bw);
                private protected override void WriteUnk8(BinaryWriterEx bw) => Unk8.Write(bw);
                private protected override void WriteUnk9(BinaryWriterEx bw) => Unk9.Write(bw);
                private protected override void WriteTileLoad(BinaryWriterEx bw) => TileLoad.Write(bw);
                private protected override void WriteUnk11(BinaryWriterEx bw) => Unk11.Write(bw);

                internal override void GetNames(MSBE msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    UnkPartNames = MSB.FindNames(entries.Parts, UnkPartIndices);
                    UnkT54PartName = MSB.FindName(entries.Parts, UnkT54PartIndex);
                }

                internal override void GetIndices(MSBE msb, Entries entries)
                {
                    base.GetIndices(msb, entries);
                    UnkPartIndices = MSB.FindIndices(this, entries.Parts, UnkPartNames);
                    UnkT54PartIndex = MSB.FindIndex(entries.Parts, UnkT54PartName);
                }
            }
        }
    }
}
