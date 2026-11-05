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
            public string ModelName { get; set; }
            private int ModelIndex { get; set; }

            /// <summary>
            /// Involved with serialization.
            /// </summary>
            public int InstanceID { get; set; }

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
            /// 1 disables the part, 2 and 3 are unknown.
            /// </summary>
            public uint GameEditionDisable { get; set; } = 0;

            /// <summary>
            /// Very speculative
            /// </summary>
            public uint MapStudioLayer { get; set; }

            /// <summary>
            /// Identifies the part in event scripts.
            /// </summary>
            public uint EntityID { get; set; }

            /// <summary>
            /// Enables use of PartsDrawParamID. If false, asset param is used instead.
            /// </summary>
            public byte isUsePartsDrawParamID { get; set; }

            /// <summary>
            /// The PartsDrawParam row used when isUsePartsDrawParamID is set; -1 for none. The draw flags below fall back to it
            /// when their override is off.
            /// </summary>
            public int PartsDrawParamID { get; set; } = -1;

            // The draw flags come in pairs: when the override byte is set the part uses the value byte, otherwise the
            // PartsDrawParam row's field (CSMsbParts getters at 0x140CF0210..0x140CF08A0 in eldenring.exe).

            /// <summary>
            /// If set, IsPointLightShadowSrc is used instead of the draw param's isPointLightShadowSrc.
            /// </summary>
            public byte OverridePointLightShadowSrc { get; set; }

            /// <summary>
            /// Whether the part casts point light shadows, when OverridePointLightShadowSrc is set.
            /// </summary>
            public byte IsPointLightShadowSrc { get; set; }

            /// <summary>
            /// If set, IsDirLightShadowSrc is used instead of the draw param's isDirLightShadowSrc (default on).
            /// </summary>
            public byte OverrideDirLightShadowSrc { get; set; }

            /// <summary>
            /// Whether the part casts the sun's shadow, when OverrideDirLightShadowSrc is set.
            /// </summary>
            public byte IsDirLightShadowSrc { get; set; }

            /// <summary>
            /// If set, IsShadowDest is used instead of the draw param's isShadowDst (default on).
            /// </summary>
            public byte OverrideShadowDest { get; set; }

            /// <summary>
            /// Whether shadows fall on the part, when OverrideShadowDest is set.
            /// </summary>
            public byte IsShadowDest { get; set; }

            /// <summary>
            /// Unknown; not read by the game.
            /// </summary>
            public byte UnkE12 { get; set; }

            /// <summary>
            /// Unknown; not read by the game.
            /// </summary>
            public byte UnkE13 { get; set; }

            /// <summary>
            /// If set, IsShadowOnly is used instead of the draw param's isShadowOnly.
            /// </summary>
            public byte OverrideShadowOnly { get; set; }

            /// <summary>
            /// Whether the part is drawn only into shadows, when OverrideShadowOnly is set.
            /// </summary>
            public byte IsShadowOnly { get; set; }

            /// <summary>
            /// If set, DrawByReflectCam is used instead of the draw param's drawByReflectCam.
            /// </summary>
            public byte OverrideDrawByReflectCam { get; set; }

            /// <summary>
            /// Whether the part is drawn by the reflection camera, when OverrideDrawByReflectCam is set.
            /// </summary>
            public byte DrawByReflectCam { get; set; }

            /// <summary>
            /// If set, DrawOnlyReflectCam is used instead of the draw param's drawOnlyReflectCam.
            /// </summary>
            public byte OverrideDrawOnlyReflectCam { get; set; }

            /// <summary>
            /// Whether the part is drawn only by the reflection camera, when OverrideDrawOnlyReflectCam is set.
            /// </summary>
            public byte DrawOnlyReflectCam { get; set; }

            /// <summary>
            /// Unknown; not read by the game.
            /// </summary>
            public byte UnkE1A { get; set; }

            /// <summary>
            /// Unknown; not read by the game.
            /// </summary>
            public byte UnkE1B { get; set; }

            /// <summary>
            /// Allows multiple parts to be identified by the same entity ID.
            /// </summary>
            public uint[] EntityGroupIDs { get; set; }

            /// <summary>
            /// Unknown. -1 when unused.
            /// </summary>
            public short UnkE3C { get; set; }

            /// <summary>
            /// Unknown. Becomes a render flag of the part (byte 2 bit 0 of its draw flags).
            /// </summary>
            public byte UnkE3E { get; set; }

            /// <summary>
            /// Disables Ray Traced Ambient Occlusion on this part.
            /// </summary>
            public bool DisableRTAO { get; set; }

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
                GameEditionDisable = br.ReadUInt32();
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

                if (!BinaryReaderEx.IgnoreAsserts)
                {
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
                }

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
                PartsDrawParamID = br.ReadInt32();
                OverridePointLightShadowSrc = br.ReadByte();
                IsPointLightShadowSrc = br.ReadByte();
                OverrideDirLightShadowSrc = br.ReadByte();
                IsDirLightShadowSrc = br.ReadByte();
                OverrideShadowDest = br.ReadByte();
                IsShadowDest = br.ReadByte();
                UnkE12 = br.ReadByte();
                UnkE13 = br.ReadByte();
                OverrideShadowOnly = br.ReadByte();
                IsShadowOnly = br.ReadByte();
                OverrideDrawByReflectCam = br.ReadByte();
                DrawByReflectCam = br.ReadByte();
                OverrideDrawOnlyReflectCam = br.ReadByte();
                DrawOnlyReflectCam = br.ReadByte();
                UnkE1A = br.ReadByte();
                UnkE1B = br.ReadByte();
                EntityGroupIDs = br.ReadUInt32s(8);
                UnkE3C = br.ReadInt16();
                UnkE3E = br.ReadByte();
                DisableRTAO = br.ReadBoolean();
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
                bw.WriteInt32(PartsDrawParamID);
                bw.WriteByte(OverridePointLightShadowSrc);
                bw.WriteByte(IsPointLightShadowSrc);
                bw.WriteByte(OverrideDirLightShadowSrc);
                bw.WriteByte(IsDirLightShadowSrc);
                bw.WriteByte(OverrideShadowDest);
                bw.WriteByte(IsShadowDest);
                bw.WriteByte(UnkE12);
                bw.WriteByte(UnkE13);
                bw.WriteByte(OverrideShadowOnly);
                bw.WriteByte(IsShadowOnly);
                bw.WriteByte(OverrideDrawByReflectCam);
                bw.WriteByte(DrawByReflectCam);
                bw.WriteByte(OverrideDrawOnlyReflectCam);
                bw.WriteByte(DrawOnlyReflectCam);
                bw.WriteByte(UnkE1A);
                bw.WriteByte(UnkE1B);
                bw.WriteUInt32s(EntityGroupIDs);
                bw.WriteInt16(UnkE3C);
                bw.WriteByte(UnkE3E);
                bw.WriteBoolean(DisableRTAO);
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
            /// The part's display and draw groups, with an alternate set of each the loader can swap or merge in.
            /// Layout from MsbResCap loading in eldenring.exe (0x140CF9B62): for MapPiece, Enemy, Collision and Asset parts.
            /// </summary>
            public class DisplayDataStruct
            {
                /// <summary>
                /// Groups this part shows when it is the collision the player stands on (collisions), or the groups it
                /// belongs to for display.
                /// </summary>
                public uint[] DisplayGroups { get; set; }

                /// <summary>
                /// Display groups that replace or are merged into DisplayGroups when UseAltGroups is set.
                /// </summary>
                public uint[] AltDisplayGroups { get; set; }

                /// <summary>
                /// When merging, the DisplayGroups bits set here are kept; the rest come from AltDisplayGroups.
                /// </summary>
                public uint[] AltDisplayGroupsMask { get; set; }

                /// <summary>
                /// Draw groups the part belongs to: it is drawn while any of them is shown. A collision is always also
                /// in its own display groups.
                /// </summary>
                public uint[] DrawGroups { get; set; }

                /// <summary>
                /// Draw groups merged into DrawGroups when UseAltGroups is set.
                /// </summary>
                public uint[] AltDrawGroups { get; set; }

                /// <summary>
                /// When merging, the DrawGroups bits set here are kept; the rest come from AltDrawGroups.
                /// </summary>
                public uint[] AltDrawGroupsMask { get; set; }

                /// <summary>
                /// If set, the alternate groups are never merged in, only swapped in whole (see UseAltGroups).
                /// </summary>
                public byte NoAltGroupMerge { get; set; }

                /// <summary>
                /// Enables the alternate groups. On load, when a GameMan state byte (+B71h) is 1 the display groups are
                /// replaced by AltDisplayGroups; otherwise, unless that byte is -1 or NoAltGroupMerge is set, both sets
                /// are merged in through their masks.
                /// </summary>
                public byte UseAltGroups { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkC2 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkC3 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short UnkC4 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short UnkC6 { get; set; }

                /// <summary>
                /// Creates a DisplayDataStruct with default values.
                /// </summary>
                public DisplayDataStruct()
                {
                    DisplayGroups = new uint[8];
                    AltDisplayGroups = new uint[8];
                    AltDisplayGroupsMask = new uint[8];
                    DrawGroups = new uint[8];
                    AltDrawGroups = new uint[8];
                    AltDrawGroupsMask = new uint[8];
                }

                /// <summary>
                /// Creates a deep copy of the struct.
                /// </summary>
                public DisplayDataStruct DeepCopy()
                {
                    var copy = (DisplayDataStruct)MemberwiseClone();
                    copy.DisplayGroups = (uint[])DisplayGroups.Clone();
                    copy.AltDisplayGroups = (uint[])AltDisplayGroups.Clone();
                    copy.AltDisplayGroupsMask = (uint[])AltDisplayGroupsMask.Clone();
                    copy.DrawGroups = (uint[])DrawGroups.Clone();
                    copy.AltDrawGroups = (uint[])AltDrawGroups.Clone();
                    copy.AltDrawGroupsMask = (uint[])AltDrawGroupsMask.Clone();
                    return copy;
                }

                internal DisplayDataStruct(BinaryReaderEx br)
                {
                    DisplayGroups = br.ReadUInt32s(8);
                    AltDisplayGroups = br.ReadUInt32s(8);
                    AltDisplayGroupsMask = br.ReadUInt32s(8);
                    DrawGroups = br.ReadUInt32s(8);
                    AltDrawGroups = br.ReadUInt32s(8);
                    AltDrawGroupsMask = br.ReadUInt32s(8);
                    NoAltGroupMerge = br.ReadByte();
                    UseAltGroups = br.ReadByte();
                    UnkC2 = br.ReadByte();
                    UnkC3 = br.ReadByte();
                    UnkC4 = br.ReadInt16(); // Always -1 in retail
                    UnkC6 = br.ReadInt16(); // Always 0 or 1 in retail
                    br.AssertPattern(0xC0, 0x00);
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteUInt32s(DisplayGroups);
                    bw.WriteUInt32s(AltDisplayGroups);
                    bw.WriteUInt32s(AltDisplayGroupsMask);
                    bw.WriteUInt32s(DrawGroups);
                    bw.WriteUInt32s(AltDrawGroups);
                    bw.WriteUInt32s(AltDrawGroupsMask);
                    bw.WriteByte(NoAltGroupMerge);
                    bw.WriteByte(UseAltGroups);
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
            public class DisplayGroupStruct
            {
                /// <summary>
                /// Unknown. The game reads it both as an int and as separate bytes 0, 1 and 2.
                /// </summary>
                public int Condition { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public uint[] DispGroups { get; set; }

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
                    DispGroups = new uint[8];
                }

                /// <summary>
                /// Creates a deep copy of the struct.
                /// </summary>
                public DisplayGroupStruct DeepCopy()
                {
                    var unk2 = (DisplayGroupStruct)MemberwiseClone();
                    unk2.DispGroups = (uint[])DispGroups.Clone();
                    return unk2;
                }

                internal DisplayGroupStruct(BinaryReaderEx br)
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
            /// Unknown; Struct seems absolutely gutted compared to Sekiro
            /// </summary>
            public class SceneGparamConfigStruct
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
                public sbyte Unk1C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public sbyte Unk1D { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public sbyte Unk20 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public sbyte Unk21 { get; set; }

                /// <summary>
                /// Creates a SceneGparamConfig with default values.
                /// </summary>
                public SceneGparamConfigStruct()
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
                public SceneGparamConfigStruct DeepCopy()
                {
                    var config = (SceneGparamConfigStruct)MemberwiseClone();
                    return config;
                }

                internal SceneGparamConfigStruct(BinaryReaderEx br)
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
            public class GrassConfigStruct
            {
                /// <summary>
                /// Unknown.
                /// </summary>
                public int GrassParamId0 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int GrassParamId1 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int GrassParamId2 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int GrassParamId3 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int GrassParamId4 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int GrassParamId5 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short GrassConfigStruct_Unk18 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short GrassConfigStruct_Unk1a { get; set; }

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
                    GrassParamId0 = br.ReadInt32();
                    GrassParamId1 = br.ReadInt32();
                    GrassParamId2 = br.ReadInt32();
                    GrassParamId3 = br.ReadInt32();
                    GrassParamId4 = br.ReadInt32();
                    GrassParamId5 = br.ReadInt32();
                    GrassConfigStruct_Unk18 = br.ReadInt16();
                    GrassConfigStruct_Unk1a = br.ReadInt16();
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
                    bw.WriteInt16(GrassConfigStruct_Unk18);
                    bw.WriteInt16(GrassConfigStruct_Unk1a);
                    bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class UnkStruct8
            {
                /// <summary>
                /// Unknown. 0 or 1; the game reads it as a byte.
                /// </summary>
                public byte UnkStruct8_Unk00 { get; set; }

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
                    UnkStruct8_Unk00 = br.ReadByte();
                    br.AssertByte(0);
                    br.AssertByte(0);
                    br.AssertByte(0);
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
                    bw.WriteByte(UnkStruct8_Unk00);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
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
                /// Unknown. The game reads it as a byte and compares it to 1, 2 and 3.
                /// </summary>
                public byte UnkStruct9_Unk00 { get; set; }

                /// <summary>
                /// Unknown; not read by the game.
                /// </summary>
                public byte UnkStruct9_Unk01 { get; set; }

                /// <summary>
                /// Unknown; not read by the game.
                /// </summary>
                public byte UnkStruct9_Unk02 { get; set; }

                /// <summary>
                /// Unknown; not read by the game.
                /// </summary>
                public byte UnkStruct9_Unk03 { get; set; }

                /// <summary>
                /// Unknown. Read by the MSB loader.
                /// </summary>
                public int UnkStruct9_Unk04 { get; set; }

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
                    UnkStruct9_Unk00 = br.ReadByte();
                    UnkStruct9_Unk01 = br.ReadByte();
                    UnkStruct9_Unk02 = br.ReadByte();
                    UnkStruct9_Unk03 = br.ReadByte();
                    UnkStruct9_Unk04 = br.ReadInt32();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteByte(UnkStruct9_Unk00);
                    bw.WriteByte(UnkStruct9_Unk01);
                    bw.WriteByte(UnkStruct9_Unk02);
                    bw.WriteByte(UnkStruct9_Unk03);
                    bw.WriteInt32(UnkStruct9_Unk04);
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
                /// Unknown. The game reads it as a byte.
                /// </summary>
                public byte TileLoadConfig_Unk04 { get; set; }

                /// <summary>
                /// Unknown; not read by the game.
                /// </summary>
                public byte TileLoadConfig_Unk05 { get; set; }

                /// <summary>
                /// Unknown; not read by the game.
                /// </summary>
                public byte TileLoadConfig_Unk06 { get; set; }

                /// <summary>
                /// Unknown; not read by the game.
                /// </summary>
                public byte TileLoadConfig_Unk07 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int TileLoadConfig_Unk0C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int TileLoadConfig_Unk10 { get; set; }

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
                    TileLoadConfig_Unk04 = br.ReadByte();
                    TileLoadConfig_Unk05 = br.ReadByte();
                    TileLoadConfig_Unk06 = br.ReadByte();
                    TileLoadConfig_Unk07 = br.ReadByte();
                    br.AssertInt32(0);
                    TileLoadConfig_Unk0C = br.ReadInt32();
                    TileLoadConfig_Unk10 = br.AssertInt32([0, 1]);
                    CullingHeightBehavior = br.ReadInt32();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteBytes(MapID);
                    bw.WriteByte(TileLoadConfig_Unk04);
                    bw.WriteByte(TileLoadConfig_Unk05);
                    bw.WriteByte(TileLoadConfig_Unk06);
                    bw.WriteByte(TileLoadConfig_Unk07);
                    bw.WriteInt32(0);
                    bw.WriteInt32(TileLoadConfig_Unk0C);
                    bw.WriteInt32(TileLoadConfig_Unk10);
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
                public int UnkStruct11_Unk00 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkStruct11_Unk04 { get; set; }

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
                    UnkStruct11_Unk00 = br.ReadInt32();
                    UnkStruct11_Unk04 = br.ReadInt32();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteInt32(UnkStruct11_Unk00);
                    bw.WriteInt32(UnkStruct11_Unk04);
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
                public DisplayDataStruct DisplayDataStruct { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public GparamConfigStruct GparamConfigStruct { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public GrassConfigStruct GrassConfigStruct { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct8 UnkStruct8 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct9 UnkStruct9 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public TileLoadConfig TileLoadConfig { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct11 UnkStruct11 { get; set; }

                /// <summary>
                /// Creates a MapPiece with default values.
                /// </summary>
                public MapPiece() : base("mXXXXXX_XXXX")
                {
                    DisplayDataStruct = new DisplayDataStruct();
                    GparamConfigStruct = new GparamConfigStruct();
                    GrassConfigStruct = new GrassConfigStruct();
                    UnkStruct8 = new UnkStruct8();
                    UnkStruct9 = new UnkStruct9();
                    TileLoadConfig = new TileLoadConfig();
                    UnkStruct11 = new UnkStruct11();
                }

                private protected override void DeepCopyTo(Part part)
                {
                    var piece = (MapPiece)part;
                    piece.DisplayDataStruct = DisplayDataStruct.DeepCopy();
                    piece.GparamConfigStruct = GparamConfigStruct.DeepCopy();
                    piece.GrassConfigStruct = GrassConfigStruct.DeepCopy();
                    piece.UnkStruct8 = UnkStruct8.DeepCopy();
                    piece.UnkStruct9 = UnkStruct9.DeepCopy();
                    piece.TileLoadConfig = TileLoadConfig.DeepCopy();
                    piece.UnkStruct11 = UnkStruct11.DeepCopy();
                }

                internal MapPiece(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                private protected override void ReadUnk1(BinaryReaderEx br) => DisplayDataStruct = new DisplayDataStruct(br);
                private protected override void ReadGparamConfig(BinaryReaderEx br) => GparamConfigStruct = new GparamConfigStruct(br);
                private protected override void ReadGrassConfig(BinaryReaderEx br) => GrassConfigStruct = new GrassConfigStruct(br);
                private protected override void ReadUnk8(BinaryReaderEx br) => UnkStruct8 = new UnkStruct8(br);
                private protected override void ReadUnk9(BinaryReaderEx br) => UnkStruct9 = new UnkStruct9(br);
                private protected override void ReadTileLoad(BinaryReaderEx br) => TileLoadConfig = new TileLoadConfig(br);
                private protected override void ReadUnk11(BinaryReaderEx br) => UnkStruct11 = new UnkStruct11(br);

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }

                private protected override void WriteUnk1(BinaryWriterEx bw) => DisplayDataStruct.Write(bw);
                private protected override void WriteGparamConfig(BinaryWriterEx bw) => GparamConfigStruct.Write(bw);
                private protected override void WriteGrassConfig(BinaryWriterEx bw) => GrassConfigStruct.Write(bw);
                private protected override void WriteUnk8(BinaryWriterEx bw) => UnkStruct8.Write(bw);
                private protected override void WriteUnk9(BinaryWriterEx bw) => UnkStruct9.Write(bw);
                private protected override void WriteTileLoad(BinaryWriterEx bw) => TileLoadConfig.Write(bw);
                private protected override void WriteUnk11(BinaryWriterEx bw) => UnkStruct11.Write(bw);
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
                public DisplayDataStruct DisplayDataStruct { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public GparamConfigStruct GparamConfigStruct { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct8 UnkStruct8 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public TileLoadConfig TileLoadConfig { get; set; }

                /// <summary>
                /// An ID in NPCThinkParam that determines the enemy's AI characteristics.
                /// </summary>
                public int ThinkParamID { get; set; }

                /// <summary>
                /// An ID in NPCParam that determines a variety of enemy properties.
                /// </summary>
                public int NPCParamID { get; set; }

                /// <summary>
                /// Talk ID
                /// </summary>
                public int TalkID { get; set; }

                /// <summary>
                /// Unknown. Always 0 in retail; read by the game (getter 0x140CF17C0).
                /// </summary>
                public byte UnkT14 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public bool UnkT15 { get; set; }

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
                private int CollisionPartIndex { get; set; }

                /// <summary>
                /// Walk route followed by this enemy.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Event.PatrolInfo))]
                public string WalkRouteName { get; set; }
                private short WalkRouteIndex { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT24 { get; set; }

                /// <summary>
                /// Hour of the in-game clock the enemy starts appearing at. When the four ActivateTime values are all 0, the
                /// times of its ChrActivateConditionParam row are used instead (e.g. 20:00-05:00 for night-only enemies).
                /// </summary>
                public byte ActivateTimeStartHour { get; set; }

                /// <summary>
                /// Minute of the in-game clock the enemy starts appearing at.
                /// </summary>
                public byte ActivateTimeStartMinute { get; set; }

                /// <summary>
                /// Hour of the in-game clock the enemy stops appearing at.
                /// </summary>
                public byte ActivateTimeEndHour { get; set; }

                /// <summary>
                /// Minute of the in-game clock the enemy stops appearing at.
                /// </summary>
                public byte ActivateTimeEndMinute { get; set; }

                /// <summary>
                /// ID in ChrActivateConditionParam that affects enemy appearance conditions.
                /// </summary>
                public int ChrActivateCondParamID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT34 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int BackupEventAnimID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT3C { get; set; }

                /// <summary>
                /// Refers to SpEffectSetParam ID. Applies SpEffects to an enemy.
                /// </summary>
                public int[] SpEffectSetParamID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT84 { get; set; }

                private protected EnemyBase() : base("cXXXX_XXXX")
                {
                    DisplayDataStruct = new DisplayDataStruct();
                    GparamConfigStruct = new GparamConfigStruct();
                    UnkStruct8 = new UnkStruct8();
                    TileLoadConfig = new TileLoadConfig();
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
                    enemy.DisplayDataStruct = DisplayDataStruct.DeepCopy();
                    enemy.GparamConfigStruct = GparamConfigStruct.DeepCopy();
                    enemy.UnkStruct8 = UnkStruct8.DeepCopy();
                    enemy.TileLoadConfig = TileLoadConfig.DeepCopy();
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
                    UnkT14 = br.ReadByte();
                    UnkT15 = br.ReadBoolean();
                    PlatoonID = br.ReadInt16();
                    CharaInitID = br.ReadInt32();
                    CollisionPartIndex = br.ReadInt32();
                    WalkRouteIndex = br.ReadInt16();
                    br.AssertInt16(0);
                    UnkT24 = br.ReadInt32();
                    ActivateTimeStartHour = br.ReadByte();
                    ActivateTimeStartMinute = br.ReadByte();
                    ActivateTimeEndHour = br.ReadByte();
                    ActivateTimeEndMinute = br.ReadByte();
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

                private protected override void ReadUnk1(BinaryReaderEx br) => DisplayDataStruct = new DisplayDataStruct(br);

                private protected override void ReadGparamConfig(BinaryReaderEx br) => GparamConfigStruct = new GparamConfigStruct(br);

                private protected override void ReadUnk8(BinaryReaderEx br) => UnkStruct8 = new UnkStruct8(br);

                private protected override void ReadTileLoad(BinaryReaderEx br) => TileLoadConfig = new TileLoadConfig(br);

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(ThinkParamID);
                    bw.WriteInt32(NPCParamID);
                    bw.WriteInt32(TalkID);
                    bw.WriteByte(UnkT14);
                    bw.WriteBoolean(UnkT15);
                    bw.WriteInt16(PlatoonID);
                    bw.WriteInt32(CharaInitID);
                    bw.WriteInt32(CollisionPartIndex);
                    bw.WriteInt16(WalkRouteIndex);
                    bw.WriteInt16(0);
                    bw.WriteInt32(UnkT24);
                    bw.WriteByte(ActivateTimeStartHour);
                    bw.WriteByte(ActivateTimeStartMinute);
                    bw.WriteByte(ActivateTimeEndHour);
                    bw.WriteByte(ActivateTimeEndMinute);
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

                private protected override void WriteUnk1(BinaryWriterEx bw) => DisplayDataStruct.Write(bw);

                private protected override void WriteGparamConfig(BinaryWriterEx bw) => GparamConfigStruct.Write(bw);

                private protected override void WriteUnk8(BinaryWriterEx bw) => UnkStruct8.Write(bw);

                private protected override void WriteTileLoad(BinaryWriterEx bw) => TileLoadConfig.Write(bw);

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
                public DisplayDataStruct DisplayDataStruct { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct8 UnkStruct8 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public TileLoadConfig TileLoadConfig { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk00 { get; set; }

                /// <summary>
                /// Creates a Player with default values.
                /// </summary>
                public Player() : base("c0000_XXXX")
                {
                    DisplayDataStruct = new DisplayDataStruct();
                    UnkStruct8 = new UnkStruct8();
                    TileLoadConfig = new TileLoadConfig();
                }

                private protected override void DeepCopyTo(Part part)
                {
                    var player = (Player)part;
                    player.DisplayDataStruct = DisplayDataStruct.DeepCopy();
                    player.UnkStruct8 = UnkStruct8.DeepCopy();
                    player.TileLoadConfig = TileLoadConfig.DeepCopy();
                }

                internal Player(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    Unk00 = br.ReadInt32();
                    br.AssertPattern(0x0C, 0x00);
                }

                private protected override void ReadUnk1(BinaryReaderEx br) => DisplayDataStruct = new DisplayDataStruct(br);

                private protected override void ReadUnk8(BinaryReaderEx br) => UnkStruct8 = new UnkStruct8(br);

                private protected override void ReadTileLoad(BinaryReaderEx br) => TileLoadConfig = new TileLoadConfig(br);

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WritePattern(0x0C, 0x00);
                }

                private protected override void WriteUnk1(BinaryWriterEx bw) => DisplayDataStruct.Write(bw);

                private protected override void WriteUnk8(BinaryWriterEx bw) => UnkStruct8.Write(bw);

                private protected override void WriteTileLoad(BinaryWriterEx bw) => TileLoadConfig.Write(bw);
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
                private protected override bool HasGrassConfig => false;
                private protected override bool HasUnk8 => true;
                private protected override bool HasUnk9 => false;
                private protected override bool HasTileLoadConfig => true;
                private protected override bool HasUnk11 => true;

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
                public GparamConfigStruct GparamConfigStruct { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public SceneGparamConfigStruct SceneGparamConfigStruct { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct8 UnkStruct8 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public TileLoadConfig TileLoadConfig { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct11 UnkStruct11 { get; set; }

                /// <summary>
                /// Sets collision behavior. Fall collision, death collision, enemy-only collision, etc.
                /// </summary>
                public byte HitFilterID { get; set; } = 8;

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT01 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT02 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public bool UnkT03 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT04 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT06 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT14 { get; set; }

                /// <summary>
                /// ID of location text to display when stepping onto this collision.
                /// </summary>
                public int LocationTextID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT1C { get; set; }

                /// <summary>
                /// Used to determine invasion eligibility.
                /// </summary>
                public int PlayRegionID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short UnkT24 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short UnkT26 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT30 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT34 { get; set; }

                /// <summary>
                /// Overrides the gparam set by the collision with this sub-ID
                /// mXX_XX_AABB
                /// This value replaces BB in the gparam file lookup
                /// </summary>
                public byte GparamOverride { get; set; }    

                /// <summary>
                /// Disable being able to summon/ride Torrent.
                /// </summary>
                public bool DisableTorrent { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short UnkT3C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short UnkT3E { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT40 { get; set; }

                /// <summary>
                /// Disables Fast Travel if Event Flag is not set.
                /// </summary>
                public uint EnableFastTravelEventFlagID { get; set; }

                /// <summary>
                /// Unknown. 0, 1 or 2 in retail; read by its own getter.
                /// </summary>
                public byte UnkT4C { get; set; }

                /// <summary>
                /// Unknown. Always 0 in retail; read by its own getter.
                /// </summary>
                public byte UnkT4D { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short UnkT4E { get; set; }

                /// <summary>
                /// Creates a Collision with default values.
                /// </summary>
                public Collision() : base("hXXXXXX")
                {
                    DisplayDataStruct = new DisplayDataStruct();
                    DisplayGroupStruct = new DisplayGroupStruct();
                    GparamConfigStruct = new GparamConfigStruct();
                    SceneGparamConfigStruct = new SceneGparamConfigStruct();
                    UnkStruct8 = new UnkStruct8();
                    TileLoadConfig = new TileLoadConfig();
                    UnkStruct11 = new UnkStruct11();
                }

                private protected override void DeepCopyTo(Part part)
                {
                    var collision = (Collision)part;
                    collision.DisplayDataStruct = DisplayDataStruct.DeepCopy();
                    collision.DisplayGroupStruct = DisplayGroupStruct.DeepCopy();
                    collision.GparamConfigStruct = GparamConfigStruct.DeepCopy();
                    collision.SceneGparamConfigStruct = SceneGparamConfigStruct.DeepCopy();
                    collision.UnkStruct8 = UnkStruct8.DeepCopy();
                    collision.TileLoadConfig = TileLoadConfig.DeepCopy();
                    collision.UnkStruct11 = UnkStruct11.DeepCopy();
                }

                internal Collision(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    HitFilterID = br.ReadByte();
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
                    GparamOverride = br.ReadByte();
                    DisableTorrent = br.ReadBoolean();
                    br.AssertByte(0);
                    br.AssertInt32(-1);
                    UnkT3C = br.ReadInt16();
                    UnkT3E = br.ReadInt16();
                    UnkT40 = br.ReadSingle();
                    br.AssertInt32(0);
                    EnableFastTravelEventFlagID = br.ReadUInt32();
                    UnkT4C = br.ReadByte();
                    UnkT4D = br.ReadByte();
                    UnkT4E = br.ReadInt16();
                }

                private protected override void ReadUnk1(BinaryReaderEx br) => DisplayDataStruct = new DisplayDataStruct(br);
                private protected override void ReadUnk2(BinaryReaderEx br) => DisplayGroupStruct = new DisplayGroupStruct(br);
                private protected override void ReadGparamConfig(BinaryReaderEx br) => GparamConfigStruct = new GparamConfigStruct(br);
                private protected override void ReadSceneGparamConfig(BinaryReaderEx br) => SceneGparamConfigStruct = new SceneGparamConfigStruct(br);
                private protected override void ReadUnk8(BinaryReaderEx br) => UnkStruct8 = new UnkStruct8(br);
                private protected override void ReadTileLoad(BinaryReaderEx br) => TileLoadConfig = new TileLoadConfig(br);
                private protected override void ReadUnk11(BinaryReaderEx br) => UnkStruct11 = new UnkStruct11(br);

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
                    bw.WriteByte(GparamOverride);
                    bw.WriteBoolean(DisableTorrent);
                    bw.WriteByte(0);
                    bw.WriteInt32(-1);
                    bw.WriteInt16(UnkT3C);
                    bw.WriteInt16(UnkT3E);
                    bw.WriteSingle(UnkT40);
                    bw.WriteInt32(0);
                    bw.WriteUInt32(EnableFastTravelEventFlagID);
                    bw.WriteByte(UnkT4C);
                    bw.WriteByte(UnkT4D);
                    bw.WriteInt16(UnkT4E);
                }

                private protected override void WriteUnk1(BinaryWriterEx bw) => DisplayDataStruct.Write(bw);
                private protected override void WriteUnk2(BinaryWriterEx bw) => DisplayGroupStruct.Write(bw);
                private protected override void WriteGparamConfig(BinaryWriterEx bw) => GparamConfigStruct.Write(bw);
                private protected override void WriteSceneGparamConfig(BinaryWriterEx bw) => SceneGparamConfigStruct.Write(bw);
                private protected override void WriteUnk8(BinaryWriterEx bw) => UnkStruct8.Write(bw);
                private protected override void WriteTileLoad(BinaryWriterEx bw) => TileLoadConfig.Write(bw);
                private protected override void WriteUnk11(BinaryWriterEx bw) => UnkStruct11.Write(bw);
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
                public DisplayDataStruct DisplayDataStruct { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public GparamConfigStruct GparamConfigStruct { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct8 UnkStruct8 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public TileLoadConfig TileLoadConfig { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT18 { get; set; }

                /// <summary>
                /// Creates a MapPiece with default values.
                /// </summary>
                public DummyAsset() : base("AEGxxx_xxx_xxxx")
                {
                    DisplayDataStruct = new DisplayDataStruct();
                    GparamConfigStruct = new GparamConfigStruct();
                    UnkStruct8 = new UnkStruct8();
                    TileLoadConfig = new TileLoadConfig();
                }

                private protected override void DeepCopyTo(Part part)
                {
                    var asset = (DummyAsset)part;
                    asset.DisplayDataStruct = DisplayDataStruct.DeepCopy();
                    asset.GparamConfigStruct = GparamConfigStruct.DeepCopy();
                    asset.UnkStruct8 = UnkStruct8.DeepCopy();
                    asset.TileLoadConfig = TileLoadConfig.DeepCopy();
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

                private protected override void ReadUnk1(BinaryReaderEx br) => DisplayDataStruct = new DisplayDataStruct(br);
                private protected override void ReadGparamConfig(BinaryReaderEx br) => GparamConfigStruct = new GparamConfigStruct(br);
                private protected override void ReadUnk8(BinaryReaderEx br) => UnkStruct8 = new UnkStruct8(br);
                private protected override void ReadTileLoad(BinaryReaderEx br) => TileLoadConfig = new TileLoadConfig(br);

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

                private protected override void WriteUnk1(BinaryWriterEx bw) => DisplayDataStruct.Write(bw);
                private protected override void WriteGparamConfig(BinaryWriterEx bw) => GparamConfigStruct.Write(bw);
                private protected override void WriteUnk8(BinaryWriterEx bw) => UnkStruct8.Write(bw);
                private protected override void WriteTileLoad(BinaryWriterEx bw) => TileLoadConfig.Write(bw);
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
                public DisplayDataStruct DisplayDataStruct { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public DisplayGroupStruct DisplayGroupStruct { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct8 UnkStruct8 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public TileLoadConfig TileLoadConfig { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct11 UnkStruct11 { get; set; }

                /// <summary>
                /// The collision part to attach to.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Collision))]
                [NoRenderGroupInheritence()]
                public string CollisionName { get; set; }
                private int CollisionIndex { get; set; }

                /// <summary>
                /// The map to load when on this collision.
                /// </summary>
                public byte[] MapID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT08 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public bool UnkT09 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT0A { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public bool UnkT0B { get; set; }

                /// <summary>
                /// Creates a ConnectCollision with default values.
                /// </summary>
                public ConnectCollision() : base("hXXXXXX_XXXX")
                {
                    DisplayDataStruct = new DisplayDataStruct();
                    DisplayGroupStruct = new DisplayGroupStruct();
                    MapID = new byte[4];
                    UnkStruct8 = new UnkStruct8();
                    TileLoadConfig = new TileLoadConfig();
                    UnkStruct11 = new UnkStruct11();
                }

                private protected override void DeepCopyTo(Part part)
                {
                    var connect = (ConnectCollision)part;
                    connect.DisplayDataStruct = DisplayDataStruct.DeepCopy();
                    connect.DisplayGroupStruct = DisplayGroupStruct.DeepCopy();
                    connect.MapID = (byte[])MapID.Clone();
                    connect.UnkStruct8 = UnkStruct8.DeepCopy();
                    connect.TileLoadConfig = TileLoadConfig.DeepCopy();
                    connect.UnkStruct11 = UnkStruct11.DeepCopy();
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

                private protected override void ReadUnk1(BinaryReaderEx br) => DisplayDataStruct = new DisplayDataStruct(br);
                private protected override void ReadUnk2(BinaryReaderEx br) => DisplayGroupStruct = new DisplayGroupStruct(br);
                private protected override void ReadUnk8(BinaryReaderEx br) => UnkStruct8 = new UnkStruct8(br);
                private protected override void ReadTileLoad(BinaryReaderEx br) => TileLoadConfig = new TileLoadConfig(br);
                private protected override void ReadUnk11(BinaryReaderEx br) => UnkStruct11 = new UnkStruct11(br);

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

                private protected override void WriteUnk1(BinaryWriterEx bw) => DisplayDataStruct.Write(bw);
                private protected override void WriteUnk2(BinaryWriterEx bw) => DisplayGroupStruct.Write(bw);
                private protected override void WriteUnk8(BinaryWriterEx bw) => UnkStruct8.Write(bw);
                private protected override void WriteTileLoad(BinaryWriterEx bw) => TileLoadConfig.Write(bw);
                private protected override void WriteUnk11(BinaryWriterEx bw) => UnkStruct11.Write(bw);

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
                public DisplayDataStruct DisplayDataStruct { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public DisplayGroupStruct DisplayGroupStruct { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public GparamConfigStruct GparamConfigStruct { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public GrassConfigStruct GrassConfigStruct { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct8 UnkStruct8 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct9 UnkStruct9 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public TileLoadConfig TileLoadConfig { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct11 UnkStruct11 { get; set; }

                /// <summary>
                /// Unknown; not read by the game.
                /// </summary>
                public byte UnkT00 { get; set; }

                /// <summary>
                /// Unknown. Read by its own getter.
                /// </summary>
                public byte UnkT01 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short UnkT02 { get; set; }

                /// <summary>
                /// Unknown. Always 0 in retail; the game tests it.
                /// </summary>
                public long UnkT08 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT10 { get; set; }

                /// <summary>
                /// If enabled, any destructible objects touching this object will always be destroyed when it is destroyed.
                /// </summary>
                public bool ApplyDestructionToAdjacentAssets { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT12 { get; set; }

                /// <summary>
                /// Overrides the asset geometry param's excludeActivateRatio_PS4 (Grid or Legacy) when not 0. Only read when
                /// running as the PS4 build; 0 in retail.
                /// </summary>
                public byte ExcludeActivateRatioOverride_PS4 { get; set; }

                /// <summary>
                /// Value added onto model ID determining AssetModelSfxParam to use.
                /// </summary>
                public short AssetSfxParamRelativeID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short UnkT1E { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT24 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT25 { get; set; }

                /// <summary>
                /// Unknown. Read by the game.
                /// </summary>
                public byte UnkT26 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT27 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT28 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT30 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT34 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT35 { get; set; }

                /// <summary>
                /// Unknown. Read by the game.
                /// </summary>
                public byte UnkT36 { get; set; }

                /// <summary>
                /// Unknown. Read by the game.
                /// </summary>
                public byte UnkT37 { get; set; }

                /// <summary>
                /// Parts referenced by the asset. [4] and [5] are its Collision: the game uses [5] if set, else [4], and
                /// reads that collision's type data (e.g. DisableTorrent). On open-world tiles the loader clears both.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Part))]
                public string[] PartNames { get; set; }

                private int[] PartIndices { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public bool UnkT50 { get; set; }

                /// <summary>
                /// Unknown category, 0, 1 or 2 (0 for nearly all retail assets). 1 and 2 put the asset in two separate instance
                /// lists of the geometry manager, checked next to DisableRTAO.
                /// </summary>
                public byte UnkT51 { get; set; }

                /// <summary>
                /// Overrides the asset geometry param's excludeActivateRatio_Xboxone (Grid or Legacy) when not 0. Only read when
                /// running as the Xbox One build, for MSB version 0x45 and later; 0 in retail.
                /// </summary>
                public byte ExcludeActivateRatioOverride_XboxOne { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT53 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Part))]
                public string UnkT54PartName { get; set; }
                private int UnkT54PartIndex { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                [EldenRingAssetMask]
                public int UnkModelMaskAndAnimID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT5C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT60 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT64 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public class AssetUnkStruct1
                {
                    /// <summary>
                    /// Unknown. Read by the game.
                    /// </summary>
                    public byte AssetStruct1_Unk00 { get; set; }

                    /// <summary>
                    /// Unknown; not read by the game.
                    /// </summary>
                    public byte AssetStruct1_Unk01 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public bool AssetStruct1_Unk04 { get; set; }

                    /// <summary>
                    /// Disable being able to summon/ride Torrent, but only when asset isn't referencing collision DisableTorrent.
                    /// </summary>
                    public bool DisableTorrentAssetOnly { get; set; }

                    /// <summary>
                    /// Unknown. Always -1 in retail; read by the game.
                    /// </summary>
                    public short AssetStruct1_Unk06 { get; set; } = -1;

                    /// <summary>
                    /// Unknown. Always 0 in retail; read by the game as a byte.
                    /// </summary>
                    public byte AssetStruct1_Unk0C { get; set; }

                    /// <summary>
                    /// Unknown. Always -1 in retail; read by the game.
                    /// </summary>
                    public int AssetStruct1_Unk10 { get; set; } = -1;

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public int AssetStruct1_Unk1C { get; set; }

                    /// <summary>
                    /// Unknown. Always 0 in retail; read by the game.
                    /// </summary>
                    public int AssetStruct1_Unk20 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public short AssetStruct1_Unk24 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public short AssetStruct1_Unk26 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public int AssetStruct1_Unk28 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public int AssetStruct1_Unk2C { get; set; }

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
                        AssetStruct1_Unk00 = br.ReadByte();
                        AssetStruct1_Unk01 = br.ReadByte();
                        br.AssertInt16(-1);
                        AssetStruct1_Unk04 = br.ReadBoolean();
                        DisableTorrentAssetOnly = br.ReadBoolean();
                        AssetStruct1_Unk06 = br.ReadInt16();
                        br.AssertInt32(0);
                        AssetStruct1_Unk0C = br.ReadByte();
                        br.AssertByte(0);
                        br.AssertByte(0);
                        br.AssertByte(0);
                        AssetStruct1_Unk10 = br.ReadInt32();
                        br.AssertInt32(-1);
                        br.AssertInt32(-1);
                        AssetStruct1_Unk1C = br.ReadInt32();
                        AssetStruct1_Unk20 = br.ReadInt32();
                        AssetStruct1_Unk24 = br.ReadInt16();
                        AssetStruct1_Unk26 = br.ReadInt16();
                        AssetStruct1_Unk28 = br.ReadInt32();
                        AssetStruct1_Unk2C = br.ReadInt32();
                        br.AssertInt32(0);
                        br.AssertInt32(0);
                        br.AssertInt32(0);
                        br.AssertInt32(0);
                    }

                    internal void Write(BinaryWriterEx bw)
                    {
                        bw.WriteByte(AssetStruct1_Unk00);
                        bw.WriteByte(AssetStruct1_Unk01);
                        bw.WriteInt16(-1);
                        bw.WriteBoolean(AssetStruct1_Unk04);
                        bw.WriteBoolean(DisableTorrentAssetOnly);
                        bw.WriteInt16(AssetStruct1_Unk06);
                        bw.WriteInt32(0);
                        bw.WriteByte(AssetStruct1_Unk0C);
                        bw.WriteByte(0);
                        bw.WriteByte(0);
                        bw.WriteByte(0);
                        bw.WriteInt32(AssetStruct1_Unk10);
                        bw.WriteInt32(-1);
                        bw.WriteInt32(-1);
                        bw.WriteInt32(AssetStruct1_Unk1C);
                        bw.WriteInt32(AssetStruct1_Unk20);
                        bw.WriteInt16(AssetStruct1_Unk24);
                        bw.WriteInt16(AssetStruct1_Unk26);
                        bw.WriteInt32(AssetStruct1_Unk28);
                        bw.WriteInt32(AssetStruct1_Unk2C);
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
                    /// Unknown. Read by the game as a byte.
                    /// </summary>
                    public byte AssetUnkStruct2_Unk00 { get; set; }

                    /// <summary>
                    /// Unknown; not read by the game.
                    /// </summary>
                    public byte AssetUnkStruct2_Unk01 { get; set; }

                    /// <summary>
                    /// Unknown; not read by the game.
                    /// </summary>
                    public byte AssetUnkStruct2_Unk02 { get; set; }

                    /// <summary>
                    /// Unknown; not read by the game.
                    /// </summary>
                    public byte AssetUnkStruct2_Unk03 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public int AssetUnkStruct2_Unk04 { get; set; }

                    /// <summary>
                    /// Unknown. Always -1 in retail; read by the game (sign tested).
                    /// </summary>
                    public int AssetUnkStruct2_Unk08 { get; set; } = -1;

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public float AssetUnkStruct2_Unk14 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public byte AssetUnkStruct2_Unk1C { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public byte AssetUnkStruct2_Unk1D { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public byte AssetUnkStruct2_Unk1E { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public byte AssetUnkStruct2_Unk1F { get; set; }

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
                        AssetUnkStruct2_Unk00 = br.ReadByte();
                        AssetUnkStruct2_Unk01 = br.ReadByte();
                        AssetUnkStruct2_Unk02 = br.ReadByte();
                        AssetUnkStruct2_Unk03 = br.ReadByte();
                        AssetUnkStruct2_Unk04 = br.ReadInt32();
                        AssetUnkStruct2_Unk08 = br.ReadInt32();
                        br.AssertInt32(0);
                        br.AssertInt32(0);
                        AssetUnkStruct2_Unk14 = br.ReadSingle();
                        br.AssertInt32(0);
                        AssetUnkStruct2_Unk1C = br.ReadByte();
                        AssetUnkStruct2_Unk1D = br.ReadByte();
                        AssetUnkStruct2_Unk1E = br.ReadByte();
                        AssetUnkStruct2_Unk1F = br.ReadByte();
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
                        bw.WriteByte(AssetUnkStruct2_Unk00);
                        bw.WriteByte(AssetUnkStruct2_Unk01);
                        bw.WriteByte(AssetUnkStruct2_Unk02);
                        bw.WriteByte(AssetUnkStruct2_Unk03);
                        bw.WriteInt32(AssetUnkStruct2_Unk04);
                        bw.WriteInt32(AssetUnkStruct2_Unk08);
                        bw.WriteInt32(0);
                        bw.WriteInt32(0);
                        bw.WriteSingle(AssetUnkStruct2_Unk14);
                        bw.WriteInt32(0);
                        bw.WriteByte(AssetUnkStruct2_Unk1C);
                        bw.WriteByte(AssetUnkStruct2_Unk1D);
                        bw.WriteByte(AssetUnkStruct2_Unk1E);
                        bw.WriteByte(AssetUnkStruct2_Unk1F);
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
                    /// Unknown. Read by the game as a byte.
                    /// </summary>
                    public byte AssetUnkStruct3_Unk00 { get; set; }

                    /// <summary>
                    /// Unknown; not read by the game.
                    /// </summary>
                    public byte AssetUnkStruct3_Unk01 { get; set; }

                    /// <summary>
                    /// Unknown; not read by the game.
                    /// </summary>
                    public byte AssetUnkStruct3_Unk02 { get; set; }

                    /// <summary>
                    /// Unknown; not read by the game.
                    /// </summary>
                    public byte AssetUnkStruct3_Unk03 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public float AssetUnkStruct3_Unk04 { get; set; }

                    /// <summary>
                    /// Unknown. Always -1 in retail; read by the game.
                    /// </summary>
                    public sbyte AssetUnkStruct3_Unk08 { get; set; } = -1;

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public byte AssetUnkStruct3_Unk09 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public byte AssetUnkStruct3_Unk0A { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public byte AssetUnkStruct3_Unk0B { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public short AssetUnkStruct3_Unk0C { get; set; }

                    /// <summary>
                    /// Unknown. Read by the game as a byte.
                    /// </summary>
                    public byte AssetUnkStruct3_Unk0E { get; set; }

                    /// <summary>
                    /// Unknown. Read by the game as a byte.
                    /// </summary>
                    public byte AssetUnkStruct3_Unk0F { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public float AssetUnkStruct3_Unk10 { get; set; }

                    /// <summary>
                    /// Disables the asset when the specified map is loaded.
                    /// </summary>
                    public sbyte[] DisableWhenMapLoadedMapID { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public int AssetUnkStruct3_Unk18 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public int AssetUnkStruct3_Unk1C { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public int AssetUnkStruct3_Unk20 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public sbyte AssetUnkStruct3_Unk24 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public bool AssetUnkStruct3_Unk25 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public int AssetUnkStruct3_Unk28 { get; set; }

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
                        AssetUnkStruct3_Unk00 = br.ReadByte();
                        AssetUnkStruct3_Unk01 = br.ReadByte();
                        AssetUnkStruct3_Unk02 = br.ReadByte();
                        AssetUnkStruct3_Unk03 = br.ReadByte();
                        AssetUnkStruct3_Unk04 = br.ReadSingle();
                        AssetUnkStruct3_Unk08 = br.ReadSByte();
                        AssetUnkStruct3_Unk09 = br.ReadByte();
                        AssetUnkStruct3_Unk0A = br.ReadByte();
                        AssetUnkStruct3_Unk0B = br.ReadByte();
                        AssetUnkStruct3_Unk0C = br.ReadInt16();
                        AssetUnkStruct3_Unk0E = br.ReadByte();
                        AssetUnkStruct3_Unk0F = br.ReadByte();
                        AssetUnkStruct3_Unk10 = br.ReadSingle();
                        DisableWhenMapLoadedMapID = br.ReadSBytes(4);
                        AssetUnkStruct3_Unk18 = br.ReadInt32();
                        AssetUnkStruct3_Unk1C = br.ReadInt32();
                        AssetUnkStruct3_Unk20 = br.ReadInt32();
                        AssetUnkStruct3_Unk24 = br.ReadSByte();
                        AssetUnkStruct3_Unk25 = br.ReadBoolean();
                        br.AssertByte(0);
                        br.AssertByte(0);
                        AssetUnkStruct3_Unk28 = br.ReadInt32();
                        br.AssertInt32(0);
                        br.AssertInt32(0);
                        br.AssertInt32(0);
                        br.AssertInt32(0);
                        br.AssertInt32(0);
                    }

                    internal void Write(BinaryWriterEx bw)
                    {
                        bw.WriteByte(AssetUnkStruct3_Unk00);
                        bw.WriteByte(AssetUnkStruct3_Unk01);
                        bw.WriteByte(AssetUnkStruct3_Unk02);
                        bw.WriteByte(AssetUnkStruct3_Unk03);
                        bw.WriteSingle(AssetUnkStruct3_Unk04);
                        bw.WriteSByte(AssetUnkStruct3_Unk08);
                        bw.WriteByte(AssetUnkStruct3_Unk09);
                        bw.WriteByte(AssetUnkStruct3_Unk0A);
                        bw.WriteByte(AssetUnkStruct3_Unk0B);
                        bw.WriteInt16(AssetUnkStruct3_Unk0C);
                        bw.WriteByte(AssetUnkStruct3_Unk0E);
                        bw.WriteByte(AssetUnkStruct3_Unk0F);
                        bw.WriteSingle(AssetUnkStruct3_Unk10);
                        bw.WriteSBytes(DisableWhenMapLoadedMapID);
                        bw.WriteInt32(AssetUnkStruct3_Unk18);
                        bw.WriteInt32(AssetUnkStruct3_Unk1C);
                        bw.WriteInt32(AssetUnkStruct3_Unk20);
                        bw.WriteSByte(AssetUnkStruct3_Unk24);
                        bw.WriteBoolean(AssetUnkStruct3_Unk25);
                        bw.WriteByte(0);
                        bw.WriteByte(0);
                        bw.WriteInt32(AssetUnkStruct3_Unk28);
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
                    public bool AssetUnkStruct4_Unk00 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public byte AssetUnkStruct4_Unk01 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public byte AssetUnkStruct4_Unk02 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public bool AssetUnkStruct4_Unk03 { get; set; }

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
                        AssetUnkStruct4_Unk00 = br.ReadBoolean();
                        AssetUnkStruct4_Unk01 = br.ReadByte();
                        AssetUnkStruct4_Unk02 = br.ReadByte();
                        AssetUnkStruct4_Unk03 = br.ReadBoolean();
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
                        bw.WriteBoolean(AssetUnkStruct4_Unk00);
                        bw.WriteByte(AssetUnkStruct4_Unk01);
                        bw.WriteByte(AssetUnkStruct4_Unk02);
                        bw.WriteBoolean(AssetUnkStruct4_Unk03);
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
                    DisplayDataStruct = new DisplayDataStruct();
                    DisplayGroupStruct = new DisplayGroupStruct();
                    GparamConfigStruct = new GparamConfigStruct();
                    GrassConfigStruct = new GrassConfigStruct();
                    UnkStruct8 = new UnkStruct8();
                    UnkStruct9 = new UnkStruct9();
                    TileLoadConfig = new TileLoadConfig();
                    UnkStruct11 = new UnkStruct11();

                    AssetUnk1 = new AssetUnkStruct1();
                    AssetUnk2 = new AssetUnkStruct2();
                    AssetUnk3 = new AssetUnkStruct3();
                    AssetUnk4 = new AssetUnkStruct4();

                    PartNames = new string[6];
                }

                private protected override void DeepCopyTo(Part part)
                {
                    var asset = (Asset)part;
                    asset.DisplayDataStruct = DisplayDataStruct.DeepCopy();
                    asset.DisplayGroupStruct = DisplayGroupStruct.DeepCopy();
                    asset.GparamConfigStruct = GparamConfigStruct.DeepCopy();
                    asset.GrassConfigStruct = GrassConfigStruct.DeepCopy();
                    asset.UnkStruct8 = UnkStruct8.DeepCopy();
                    asset.UnkStruct9 = UnkStruct9.DeepCopy();
                    asset.TileLoadConfig = TileLoadConfig.DeepCopy();
                    asset.UnkStruct11 = UnkStruct11.DeepCopy();

                    asset.AssetUnk1 = AssetUnk1.DeepCopy();
                    asset.AssetUnk2 = AssetUnk2.DeepCopy();
                    asset.AssetUnk3 = AssetUnk3.DeepCopy();
                    asset.AssetUnk4 = AssetUnk4.DeepCopy();

                    PartNames = (string[])PartNames.Clone();
                }

                internal Asset(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    UnkT00 = br.ReadByte();
                    UnkT01 = br.ReadByte();
                    UnkT02 = br.AssertInt16([0, 1]);
                    br.AssertInt32(0);
                    UnkT08 = br.ReadInt64();
                    UnkT10 = br.ReadByte();
                    ApplyDestructionToAdjacentAssets = br.ReadBoolean();
                    UnkT12 = br.ReadByte();
                    ExcludeActivateRatioOverride_PS4 = br.ReadByte();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    AssetSfxParamRelativeID = br.ReadInt16();
                    UnkT1E = br.ReadInt16();
                    br.AssertInt32(-1);
                    UnkT24 = br.ReadByte();
                    UnkT25 = br.ReadByte();
                    UnkT26 = br.ReadByte();
                    UnkT27 = br.ReadByte();
                    UnkT28 = br.ReadInt32();
                    br.AssertInt32(0);
                    UnkT30 = br.ReadInt32();
                    UnkT34 = br.ReadByte();
                    UnkT35 = br.ReadByte();
                    UnkT36 = br.ReadByte();
                    UnkT37 = br.ReadByte();
                    PartIndices = br.ReadInt32s(6);
                    UnkT50 = br.ReadBoolean();
                    UnkT51 = br.ReadByte();
                    ExcludeActivateRatioOverride_XboxOne = br.ReadByte();
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

                private protected override void ReadUnk1(BinaryReaderEx br) => DisplayDataStruct = new DisplayDataStruct(br);
                private protected override void ReadUnk2(BinaryReaderEx br) => DisplayGroupStruct = new DisplayGroupStruct(br);
                private protected override void ReadGparamConfig(BinaryReaderEx br) => GparamConfigStruct = new GparamConfigStruct(br);
                private protected override void ReadGrassConfig(BinaryReaderEx br) => GrassConfigStruct = new GrassConfigStruct(br);
                private protected override void ReadUnk8(BinaryReaderEx br) => UnkStruct8 = new UnkStruct8(br);
                private protected override void ReadUnk9(BinaryReaderEx br) => UnkStruct9 = new UnkStruct9(br);
                private protected override void ReadTileLoad(BinaryReaderEx br) => TileLoadConfig = new TileLoadConfig(br);
                private protected override void ReadUnk11(BinaryReaderEx br) => UnkStruct11 = new UnkStruct11(br);

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteByte(UnkT00);
                    bw.WriteByte(UnkT01);
                    bw.WriteInt16(UnkT02);
                    bw.WriteInt32(0);
                    bw.WriteInt64(UnkT08);
                    bw.WriteByte(UnkT10);
                    bw.WriteBoolean(ApplyDestructionToAdjacentAssets);
                    bw.WriteByte(UnkT12);
                    bw.WriteByte(ExcludeActivateRatioOverride_PS4);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt16(AssetSfxParamRelativeID);
                    bw.WriteInt16(UnkT1E);
                    bw.WriteInt32(-1);
                    bw.WriteByte(UnkT24);
                    bw.WriteByte(UnkT25);
                    bw.WriteByte(UnkT26);
                    bw.WriteByte(UnkT27);
                    bw.WriteInt32(UnkT28);
                    bw.WriteInt32(0);
                    bw.WriteInt32(UnkT30);
                    bw.WriteByte(UnkT34);
                    bw.WriteByte(UnkT35);
                    bw.WriteByte(UnkT36);
                    bw.WriteByte(UnkT37);
                    bw.WriteInt32s(PartIndices);
                    bw.WriteBoolean(UnkT50);
                    bw.WriteByte(UnkT51);
                    bw.WriteByte(ExcludeActivateRatioOverride_XboxOne);
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

                private protected override void WriteUnk1(BinaryWriterEx bw) => DisplayDataStruct.Write(bw);
                private protected override void WriteUnk2(BinaryWriterEx bw) => DisplayGroupStruct.Write(bw);
                private protected override void WriteGparamConfig(BinaryWriterEx bw) => GparamConfigStruct.Write(bw);
                private protected override void WriteGrassConfig(BinaryWriterEx bw) => GrassConfigStruct.Write(bw);
                private protected override void WriteUnk8(BinaryWriterEx bw) => UnkStruct8.Write(bw);
                private protected override void WriteUnk9(BinaryWriterEx bw) => UnkStruct9.Write(bw);
                private protected override void WriteTileLoad(BinaryWriterEx bw) => TileLoadConfig.Write(bw);
                private protected override void WriteUnk11(BinaryWriterEx bw) => UnkStruct11.Write(bw);

                internal override void GetNames(MSBE msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    PartNames = MSB.FindNames(entries.Parts, PartIndices);
                    UnkT54PartName = MSB.FindName(entries.Parts, UnkT54PartIndex);
                }

                internal override void GetIndices(MSBE msb, Entries entries)
                {
                    base.GetIndices(msb, entries);
                    PartIndices = MSB.FindIndices(this, entries.Parts, PartNames);
                    UnkT54PartIndex = MSB.FindIndex(entries.Parts, UnkT54PartName);
                }
            }
        }
    }
}
