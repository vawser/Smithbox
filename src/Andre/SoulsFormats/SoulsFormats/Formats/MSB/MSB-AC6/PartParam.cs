using System;
using System.Collections.Generic;
using System.Numerics;

namespace SoulsFormats
{
    public partial class MSB_AC6
    {
        public enum PartType : uint
        {
            MapPiece = 0,
            Enemy = 2,
            Player = 4,
            Collision = 5,
            DummyAsset = 9,
            DummyEnemy = 10, 
            ConnectCollision = 11, 
            Asset = 13 
        }

        /// <summary>
        /// Instances of actual things in the map.
        /// </summary>
        public class PartsParam : Param<Part>, IMsbParam<IMsbPart>
        {
            private int ParamVersion;

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
            public PartsParam() : base(52, "PARTS_PARAM_ST")
            {
                ParamVersion = base.Version;

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
                    case Part.MapPiece p:
                        MapPieces.Add(p);
                        break;
                    case Part.Enemy p:
                        Enemies.Add(p);
                        break;
                    case Part.Player p:
                        Players.Add(p);
                        break;
                    case Part.Collision p:
                        Collisions.Add(p);
                        break;
                    case Part.DummyAsset p:
                        DummyAssets.Add(p);
                        break;
                    case Part.DummyEnemy p:
                        DummyEnemies.Add(p);
                        break;
                    case Part.ConnectCollision p:
                        ConnectCollisions.Add(p);
                        break;
                    case Part.Asset p:
                        Assets.Add(p);
                        break;
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
                    MapPieces, 
                    Enemies,
                    Players, 
                    Collisions,
                    DummyAssets, 
                    DummyEnemies, 
                    ConnectCollisions, 
                    Assets);
            }
            IReadOnlyList<IMsbPart> IMsbParam<IMsbPart>.GetEntries() => GetEntries();

            internal override Part ReadEntry(BinaryReaderEx br, long offsetLength)
            {
                PartType type = br.GetEnum32<PartType>(br.Position + 8);

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
                        return Assets.EchoAdd(new Part.Asset(br, ParamVersion));

                    default:
                        throw new NotImplementedException($"Unimplemented part type: {type} {(int)type}");
                }
            }
        }

        /// <summary>
        /// Common data for all types of part.
        /// </summary>
        public abstract class Part : Entry, IMsbPart
        {
            private int version;

            // MAIN
            private protected abstract PartType Type { get; }
            private protected abstract bool HasUnkOffsetT50 { get; }
            private protected abstract bool HasUnkOffsetT58 { get; }
            private protected abstract bool HasOffsetGparam { get; } // Gparam
            private protected abstract bool HasOffsetSceneGparam { get; } // SceneGparam
            private protected abstract bool HasOffsetGrass { get; } // Grass
            private protected abstract bool HasUnkOffsetT88 { get; }
            private protected abstract bool HasUnkOffsetT90 { get; }
            private protected abstract bool HasUnkOffsetT98 { get; } // Tile Load
            private protected abstract bool HasUnkOffsetTA0 { get; }

            // Index among parts of the same type
            public int TypeIndex { get; set; }

            public string ModelName { get; set; }
            private int ModelIndex;

            /// <summary>
            /// A path to a .sib file, presumably some kind of editor placeholder.
            /// </summary>
            public string LayoutPath { get; set; }

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
            /// Unknown integer.
            /// </summary>
            public int UnkT44 { get; set; }

            // COMMON

            /// <summary>
            /// Identifies the part in event scripts.
            /// </summary>
            public uint EntityID { get; set; }

            /// <summary>
            /// Unknown.
            /// 0 or 1. Boolean.
            /// </summary>
            public byte UsePartsDrawParamID { get; set; }

            /// <summary>
            /// Unknown. 
            /// Always 0.
            /// </summary>
            public byte UnkE05 { get; set; }

            /// <summary>
            /// Unknown. 
            /// Always 0.
            /// </summary>
            public byte UnkE06 { get; set; }

            /// <summary>
            /// Unknown. 
            /// Always 0.
            /// </summary>
            public byte UnkE07 { get; set; }

            /// <summary>
            /// Parts Draw parameter.
            /// </summary>
            public ushort PartsDrawParamID { get; set; }

            /// <summary>
            /// Unknown. 
            /// Always 0.
            /// </summary>
            public sbyte IsPointLightShadowSrc { get; set; }

            /// <summary>
            /// Unknown. 
            /// Always 0.
            /// </summary>
            public byte UnkE0B { get; set; }

            /// <summary>
            /// Unknown. 
            /// Always 0.
            /// </summary>
            public bool IsShadowSrc { get; set; }

            /// <summary>
            /// Unknown. 
            /// 0 or 1. Boolean?
            /// </summary>
            public byte IsStaticShadowSrc { get; set; }

            /// <summary>
            /// Unknown. 
            /// 0 or 1. Boolean?
            /// </summary>
            public byte IsCascade3ShadowSrc { get; set; }

            /// <summary>
            /// Unknown. 
            /// 0 or 1. Boolean?
            /// </summary>
            public byte UnkE0F { get; set; }

            /// <summary>
            /// Unknown. 
            /// 0 or 1. Boolean?
            /// </summary>
            public byte UnkE10 { get; set; }

            /// <summary>
            /// Unknown. 
            /// Always 0.
            /// </summary>
            public bool IsShadowDest { get; set; }

            /// <summary>
            /// Unknown. 
            /// Always 0.
            /// </summary>
            public bool IsShadowOnly { get; set; }

            /// <summary>
            /// Unknown. 
            /// Always 0.
            /// </summary>
            public bool DrawByReflectCam { get; set; }

            /// <summary>
            /// Unknown. 
            /// Always 0.
            /// </summary>
            public bool DrawOnlyReflectCam { get; set; }

            /// <summary>
            /// Unknown. 
            /// 0 or 1. Boolean?
            /// </summary>
            public byte EnableOnAboveShadow { get; set; }

            /// <summary>
            /// Unknown. 
            /// Always 0.
            /// </summary>
            public bool DisablePointLightEffect { get; set; }

            /// <summary>
            /// Unknown. 
            /// 0 or 1.
            /// </summary>
            public byte UnkE17 { get; set; }

            /// <summary>
            /// Unknown. 
            /// 0 or 1. Boolean? 
            /// Only 1 within the Garage on dummy asset.
            /// </summary>
            public int UnkE18 { get; set; }

            /// <summary>
            /// Allows multiple parts to be identified by the same entity ID.
            /// </summary>
            public uint[] EntityGroupIDs { get; set; }

            /// <summary>
            /// Unknown. 
            /// Integer values.
            /// </summary>
            public short UnkE3C { get; set; }

            /// <summary>
            /// Unknown. 
            /// 0 or 1.  Boolean?
            /// Only 1 in m50_30_00_00 (The Wall) on a culling asset.
            /// Only 1 in m50_50_00_00 (BAWS Arsenal) on giant jigsaw asset.
            /// </summary>
            public byte UnkE3F { get; set; }

            private protected Part(string name)
            {
                Name = name;
                LayoutPath = "";
                Scale = Vector3.One;

                IsShadowDest = true;
                EntityGroupIDs = new uint[8];
                UnkE3C = (short)-1;
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

                // MAIN
                long nameOffset = br.ReadInt64();
                br.AssertUInt32((uint)Type);
                TypeIndex = br.ReadInt32();
                ModelIndex = br.ReadInt32();
                br.AssertInt32(new int[1]);
                long sourceOffset = br.ReadInt64();
                Position = br.ReadVector3();
                Rotation = br.ReadVector3();
                Scale = br.ReadVector3();
                UnkT44 = br.ReadInt32();
                br.AssertInt32(-1);
                br.AssertInt32(1);
                long unkOffsetT50 = br.ReadInt64();
                long unkOffsetT58 = br.ReadInt64();
                long commonOffset = br.ReadInt64();
                long typeDataOffset = br.ReadInt64();
                long gparamOffset = br.ReadInt64();
                long sceneGparamOffset = br.ReadInt64();
                long grassOffset = br.ReadInt64();
                long unkOffsetT88 = br.ReadInt64();
                long unkOffsetT90 = br.ReadInt64();
                long unkOffsetT98 = br.ReadInt64();
                long unkOffsetTA0 = br.ReadInt64();
                br.AssertInt64(new long[1]);
                br.AssertInt64(new long[1]);
                br.AssertInt64(new long[1]);

                Name = br.GetUTF16(start + nameOffset);
                LayoutPath = br.GetUTF16(start + sourceOffset);

                if (HasUnkOffsetT50)
                {
                    br.Position = start + unkOffsetT50;
                    ReadUnkOffsetT50(br);
                }

                if (HasUnkOffsetT58 && unkOffsetT58 != 0L)
                {
                    br.Position = start + unkOffsetT58;
                    ReadUnkOffsetT58(br);
                }

                br.Position = start + commonOffset;
                ReadEntityData(br);

                br.Position = start + typeDataOffset;
                ReadTypeData(br);

                if (HasOffsetGparam && gparamOffset != 0L)
                {
                    br.Position = start + gparamOffset;
                    ReadGparamStruct(br);
                }

                if (HasOffsetSceneGparam && sceneGparamOffset != 0L)
                {
                    br.Position = start + sceneGparamOffset;
                    ReadSceneGparamStruct(br);
                }

                if (HasOffsetGrass && grassOffset != 0L)
                {
                    br.Position = start + grassOffset;
                    ReadGrassStruct(br);
                }

                if (HasUnkOffsetT88)
                {
                    br.Position = start + unkOffsetT88;
                    ReadUnkOffsetT88(br);
                }

                if (HasUnkOffsetT90 && unkOffsetT90 != 0L)
                {
                    br.Position = start + unkOffsetT90;
                    ReadUnkOffsetT90(br);
                }

                if (HasUnkOffsetT98)
                {
                    br.Position = start + unkOffsetT98;
                    ReadUnkOffsetT98(br);
                }

                if (HasUnkOffsetTA0 && unkOffsetTA0 != 0L)
                {
                    br.Position = start + unkOffsetTA0;
                    ReadUnkOffsetTA0(br);
                }
            }

            private void ReadEntityData(BinaryReaderEx br)
            {
                EntityID = br.ReadUInt32();
                UsePartsDrawParamID = br.ReadByte();
                UnkE05 = br.ReadByte();
                UnkE06 = br.ReadByte();
                UnkE07 = br.ReadByte();
                PartsDrawParamID = br.ReadUInt16();
                IsPointLightShadowSrc = br.ReadSByte();
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
                EntityGroupIDs = br.ReadUInt32s(8);
                UnkE3C = br.ReadInt16();
                br.AssertByte(new byte[1]);
                UnkE3F = br.ReadByte();
            }

            private protected abstract void ReadTypeData(BinaryReaderEx br);

            private protected virtual void ReadUnkOffsetT50(BinaryReaderEx br)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(ReadUnkOffsetT50)}.");

            private protected virtual void ReadUnkOffsetT58(BinaryReaderEx br)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(ReadUnkOffsetT58)}.");

            private protected virtual void ReadGparamStruct(BinaryReaderEx br)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(ReadGparamStruct)}.");

            private protected virtual void ReadSceneGparamStruct(BinaryReaderEx br)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(ReadSceneGparamStruct)}.");

            private protected virtual void ReadGrassStruct(BinaryReaderEx br)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(ReadGrassStruct)}.");

            private protected virtual void ReadUnkOffsetT88(BinaryReaderEx br)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(ReadUnkOffsetT88)}.");

            private protected virtual void ReadUnkOffsetT90(BinaryReaderEx br)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(ReadUnkOffsetT90)}.");

            private protected virtual void ReadUnkOffsetT98(BinaryReaderEx br)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(ReadUnkOffsetT98)}.");

            private protected virtual void ReadUnkOffsetTA0(BinaryReaderEx br)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(ReadUnkOffsetTA0)}.");

            internal override void Write(BinaryWriterEx bw, int id)
            {
                long start = bw.Position;

                bw.ReserveInt64("NameOffset");

                bw.WriteUInt32((uint)Type);
                bw.WriteInt32(TypeIndex);
                bw.WriteInt32(ModelIndex);
                bw.WriteInt32(0);
                bw.ReserveInt64("SourceOffset");
                bw.WriteVector3(Position);
                bw.WriteVector3(Rotation);
                bw.WriteVector3(Scale);
                bw.WriteInt32(UnkT44);
                bw.WriteInt32(-1);
                bw.WriteInt32(1);
                bw.ReserveInt64("UnkOffsetT50");
                bw.ReserveInt64("UnkOffsetT58");
                bw.ReserveInt64("CommonOffset");
                bw.ReserveInt64("TypeDataOffset");
                bw.ReserveInt64("GparamOffset");
                bw.ReserveInt64("SceneGparamOffset");
                bw.ReserveInt64("GrassOffset");
                bw.ReserveInt64("UnkOffsetT88");
                bw.ReserveInt64("UnkOffsetT90");
                bw.ReserveInt64("UnkOffsetT98");
                bw.ReserveInt64("UnkOffsetTA0");
                bw.WriteInt64(0L);
                bw.WriteInt64(0L);
                bw.WriteInt64(0L);

                // Name
                bw.FillInt64("NameOffset", bw.Position - start);
                bw.WriteUTF16(MSB.ReambiguateName(Name), true);

                // Layout
                bw.FillInt64("SourceOffset", bw.Position - start);
                bw.WriteUTF16(LayoutPath, true);
                bw.Pad(8);

                // Struct50
                if (HasUnkOffsetT50)
                {
                    bw.FillInt64("UnkOffsetT50", bw.Position - start);
                    WriteUnkOffsetT50(bw);
                }
                else
                {
                    bw.FillInt64("UnkOffsetT50", 0);
                }

                // Struct58
                if (HasUnkOffsetT58)
                {
                    bw.FillInt64("UnkOffsetT58", bw.Position - start);
                    WriteUnkOffsetT58(bw);
                }
                else
                {
                    bw.FillInt64("UnkOffsetT58", 0);
                }

                // Entity
                bw.FillInt64("CommonOffset", bw.Position - start);
                WriteEntityData(bw);

                // Type
                bw.FillInt64("TypeDataOffset", bw.Position - start);
                WriteTypeData(bw);

                if (HasOffsetGparam)
                {
                    bw.FillInt64("GparamOffset", bw.Position - start);
                    WriteGparamStruct(bw);
                }
                else
                {
                    bw.FillInt64("GparamOffset", 0);
                }

                if (HasOffsetSceneGparam)
                {
                    bw.FillInt64("SceneGparamOffset", bw.Position - start);
                    WriteSceneGparamStruct(bw);
                }
                else
                {
                    bw.FillInt64("SceneGparamOffset", 0);
                }

                if (HasOffsetGrass)
                {
                    bw.FillInt64("GrassOffset", bw.Position - start);
                    WriteGrassStruct(bw);
                }
                else
                {
                    bw.FillInt64("GrassOffset", 0);
                }

                // Struct88
                if (HasUnkOffsetT88)
                {
                    bw.FillInt64("UnkOffsetT88", bw.Position - start);
                    WriteUnkOffsetT88(bw);
                }
                else
                {
                    bw.FillInt64("UnkOffsetT88", 0);
                }

                // Struct90
                if (HasUnkOffsetT90)
                {
                    bw.FillInt64("UnkOffsetT90", bw.Position - start);
                    WriteUnkOffsetT90(bw);
                }
                else
                {
                    bw.FillInt64("UnkOffsetT90", 0);
                }

                // Struct98
                if (HasUnkOffsetT98)
                {
                    bw.FillInt64("UnkOffsetT98", bw.Position - start);
                    WriteUnkOffsetT98(bw);
                }
                else
                {
                    bw.FillInt64("UnkOffsetT98", 0);
                }

                // StructA0
                if (HasUnkOffsetTA0)
                {
                    bw.FillInt64("UnkOffsetTA0", bw.Position - start);
                    WriteUnkOffsetTA0(bw);
                }
                else
                {
                    bw.FillInt64("UnkOffsetTA0", 0);
                }
            }

            private void WriteEntityData(BinaryWriterEx bw)
            {
                bw.WriteUInt32(EntityID);
                bw.WriteByte(UsePartsDrawParamID);
                bw.WriteByte(UnkE05);
                bw.WriteByte(UnkE06);
                bw.WriteByte(UnkE07);
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
                bw.WriteByte((byte)0);
                bw.WriteByte(UnkE3F);
            }

            private protected abstract void WriteTypeData(BinaryWriterEx bw);

            private protected virtual void WriteUnkOffsetT50(BinaryWriterEx bw)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(WriteUnkOffsetT50)}.");

            private protected virtual void WriteUnkOffsetT58(BinaryWriterEx bw)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(WriteUnkOffsetT58)}.");

            private protected virtual void WriteGparamStruct(BinaryWriterEx bw)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(WriteGparamStruct)}.");

            private protected virtual void WriteSceneGparamStruct(BinaryWriterEx bw)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(WriteSceneGparamStruct)}.");

            private protected virtual void WriteGrassStruct(BinaryWriterEx bw)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(WriteGrassStruct)}.");

            private protected virtual void WriteUnkOffsetT88(BinaryWriterEx bw)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(WriteUnkOffsetT88)}.");

            private protected virtual void WriteUnkOffsetT90(BinaryWriterEx bw)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(WriteUnkOffsetT90)}.");

            private protected virtual void WriteUnkOffsetT98(BinaryWriterEx bw)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(WriteUnkOffsetT98)}.");

            private protected virtual void WriteUnkOffsetTA0(BinaryWriterEx bw)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(WriteUnkOffsetTA0)}.");

            internal virtual void GetNames(MSB_AC6 msb, Entries entries)
            {
                ModelName = MSB.FindName(entries.Models, ModelIndex);
            }

            internal virtual void GetIndices(MSB_AC6 msb, Entries entries)
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
                /// 0 or 1. Boolean?
                /// </summary>
                public byte Struct50_UnkC0 { get; set; }

                /// <summary>
                /// Creates an UnkStruct1 with default values.
                /// </summary>
                public DisplayDataStruct()
                {
                    DisplayGroups = new uint[8];
                    DrawGroups = new uint[8];
                    CollisionMask = new uint[32];
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
                    Struct50_UnkC0 = br.ReadByte();
                    br.AssertByte(new byte[1]);
                    br.AssertByte(new byte[1]);
                    br.AssertByte(new byte[1]);
                    br.AssertInt16((short)-1);
                    br.AssertInt16(new short[1]);

                    for (int index = 0; index < 48; ++index)
                        br.AssertInt32(new int[1]);
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteUInt32s(DisplayGroups);
                    bw.WriteUInt32s(DrawGroups);
                    bw.WriteUInt32s(CollisionMask);
                    bw.WriteByte(Struct50_UnkC0);
                    bw.WriteByte((byte)0);
                    bw.WriteByte((byte)0);
                    bw.WriteByte((byte)0);
                    bw.WriteInt16((short)-1);
                    bw.WriteInt16((short)0);
                    for (int index = 0; index < 48; ++index)
                        bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class DisplayStruct
            {
                /// <summary>
                /// Unknown.
                /// 10, 11, 12, 13, 15, 20, 21, 22, 23, 30, 31, 32, 33, 40
                /// Only used on collision within m10_20_00_00 (Grid 012)
                /// </summary>
                public int Struct58_Unk00 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public uint[] Struct58_DispGroups { get; private set; }

                /// <summary>
                /// Creates an UnkStruct2 with default values.
                /// </summary>
                public DisplayStruct()
                {
                    Struct58_Unk00 = -1;
                    Struct58_DispGroups = new uint[8];
                }

                /// <summary>
                /// Creates a deep copy of the struct.
                /// </summary>
                public DisplayStruct DeepCopy()
                {
                    var unk2 = (DisplayStruct)MemberwiseClone();
                    unk2.Struct58_DispGroups = (uint[])Struct58_DispGroups.Clone();
                    return unk2;
                }

                internal DisplayStruct(BinaryReaderEx br)
                {
                    Struct58_Unk00 = br.ReadInt32();
                    Struct58_DispGroups = br.ReadUInt32s(8);
                    br.AssertInt16(new short[1]);
                    br.AssertInt16((short)-1);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Struct58_Unk00);
                    bw.WriteUInt32s(Struct58_DispGroups);
                    bw.WriteInt16((short)0);
                    bw.WriteInt16((short)-1);
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
            /// Unknown. Is Gparam struct in Elden Ring.
            /// </summary>
            public class GparamConfigStruct
            {
                /// <summary>
                /// Corresponds to GPARAM LightSet.
                /// Corresponds to the ID used in the GPARAM Value Rows.
                /// </summary>
                public int LightId { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int FogId { get; set; }

                /// <summary>
                /// Creates a UnkStruct70 with default values.
                /// </summary>
                public GparamConfigStruct()
                {
                    LightId = -1;
                    FogId = -1;
                }

                /// <summary>
                /// Creates a deep copy of UnkStruct70.
                /// </summary>
                public GparamConfigStruct DeepCopy()
                {
                    return (GparamConfigStruct)MemberwiseClone();
                }

                internal GparamConfigStruct(BinaryReaderEx br)
                {
                    LightId = br.ReadInt32();
                    FogId = br.ReadInt32();
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteInt32(LightId);
                    bw.WriteInt32(FogId);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }

                /// <summary>
                /// Returns the struct values as a string.
                /// </summary>
                public override string ToString()
                {
                    return $"{LightId}, {FogId}";
                }
            }

            /// <summary>
            /// Unknown; Is SceneGparam in Elden Ring.
            /// </summary>
            public class SceneGparamConfigStruct
            {
                /// <summary>
                /// Unknown.
                /// </summary>
                public float TransitionTime { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public sbyte GparamSubID_Base { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public sbyte GparamSubID_Override1 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public sbyte GparamSubID_Override2 { get; set; }

                /// <summary>
                /// Creates a UnkStruct78 with default values.
                /// </summary>
                public SceneGparamConfigStruct()
                {
                    TransitionTime = -1f;
                    GparamSubID_Base = (sbyte)-1;
                    GparamSubID_Override1 = (sbyte)-1;
                    GparamSubID_Override2 = (sbyte)-1;
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
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    TransitionTime = br.ReadSingle();
                    br.AssertInt32(-1);
                    GparamSubID_Base = br.ReadSByte();
                    GparamSubID_Override1 = br.ReadSByte();
                    GparamSubID_Override2 = br.ReadSByte();
                    br.AssertSByte((sbyte)-1);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteSingle(TransitionTime);
                    bw.WriteInt32(-1);
                    bw.WriteSByte(GparamSubID_Base);
                    bw.WriteSByte(GparamSubID_Override1);
                    bw.WriteSByte(GparamSubID_Override2);
                    bw.WriteSByte((sbyte)-1);
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
            public class GrassConfigStruct
            {
                /// <summary>
                /// Unknown.
                /// </summary>
                public int[] GrassTypeParamIds { get; set; }

                /// <summary>
                /// Creates an StructGrass with default values.
                /// </summary>
                public GrassConfigStruct()
                {
                    GrassTypeParamIds = new int[6];
                }

                /// <summary>
                /// Creates a deep copy of the struct.
                /// </summary>
                public GrassConfigStruct DeepCopy()
                {
                    var grass = (GrassConfigStruct)MemberwiseClone();
                    grass.GrassTypeParamIds = (int[])GrassTypeParamIds.Clone();
                    return grass;
                }

                internal GrassConfigStruct(BinaryReaderEx br)
                {
                    GrassTypeParamIds = br.ReadInt32s(6);
                    br.AssertInt32(-1);
                    br.AssertInt32(new int[1]);
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteInt32s(GrassTypeParamIds);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class UnkStruct88
            {
                /// <summary>
                /// Unknown. 
                /// 0 or 1. Boolean?
                /// </summary>
                public byte Struct88_Unk00 { get; set; }

                /// <summary>
                /// Unknown. 
                /// 0 or 1. Boolean?
                /// </summary>
                public byte Struct88_Unk01 { get; set; }

                /// <summary>
                /// Creates an UnkStruct88 with default values.
                /// </summary>
                public UnkStruct88() { }

                /// <summary>
                /// Creates a deep copy of the struct.
                /// </summary>
                public UnkStruct88 DeepCopy()
                {
                    return (UnkStruct88)MemberwiseClone();
                }

                internal UnkStruct88(BinaryReaderEx br)
                {
                    Struct88_Unk00 = br.ReadByte();
                    Struct88_Unk01 = br.ReadByte();
                    br.AssertInt16(new short[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteByte(Struct88_Unk00);
                    bw.WriteByte(Struct88_Unk01);
                    bw.WriteInt16((short)0);
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
            public class UnkStruct90
            {
                /// <summary>
                /// Unknown.
                /// 0, 1, 2
                /// </summary>
                public int Struct90_Unk00 { get; set; }

                /// <summary>
                /// Creates an UnkStruct90 with default values.
                /// </summary>
                public UnkStruct90() { }

                /// <summary>
                /// Creates a deep copy of the struct.
                /// </summary>
                public UnkStruct90 DeepCopy()
                {
                    return (UnkStruct90)MemberwiseClone();
                }

                internal UnkStruct90(BinaryReaderEx br)
                {
                    Struct90_Unk00 = br.ReadInt32();
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Struct90_Unk00);
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
            public class UnkStruct98
            {

                /// <summary>
                /// Creates an UnkStruct7 with default values.
                /// </summary>
                public UnkStruct98() { }

                /// <summary>
                /// Creates a deep copy of the struct.
                /// </summary>
                public UnkStruct98 DeepCopy()
                {
                    var unks10 = (UnkStruct98)MemberwiseClone();
                    return unks10;
                }

                internal UnkStruct98(BinaryReaderEx br)
                {
                    br.AssertInt32(-1);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(-1);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(-1);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteInt32(-1);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(0);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class UnkStructA0
            {
                /// <summary>
                /// Creates an UnkStruct7 with default values.
                /// </summary>
                public UnkStructA0() { }

                /// <summary>
                /// Creates a deep copy of the struct.
                /// </summary>
                public UnkStructA0 DeepCopy()
                {
                    return (UnkStructA0)MemberwiseClone();
                }

                internal UnkStructA0(BinaryReaderEx br)
                {
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                }

                internal void Write(BinaryWriterEx bw)
                {
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
            /// Fixed visual geometry. Doesn't seem used much in ER?
            /// </summary>
            public class MapPiece : Part
            {
                private protected override PartType Type => PartType.MapPiece;
                private protected override bool HasUnkOffsetT50 => true;
                private protected override bool HasUnkOffsetT58 => false;
                private protected override bool HasOffsetGparam => true;
                private protected override bool HasOffsetSceneGparam => false;
                private protected override bool HasOffsetGrass => true;
                private protected override bool HasUnkOffsetT88 => true;
                private protected override bool HasUnkOffsetT90 => true;
                private protected override bool HasUnkOffsetT98 => true;
                private protected override bool HasUnkOffsetTA0 => true;

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
                public UnkStruct88 UnkStruct88 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct90 UnkStruct90 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct98 UnkStruct98 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStructA0 UnkStructA0 { get; set; }

                /// <summary>
                /// Creates a MapPiece with default values.
                /// </summary>
                public MapPiece() : base("mXXXXXX_XXXX")
                {
                    DisplayDataStruct = new DisplayDataStruct();
                    GparamConfigStruct = new GparamConfigStruct();
                    GrassConfigStruct = new GrassConfigStruct();
                    UnkStruct88 = new UnkStruct88();
                    UnkStruct90 = new UnkStruct90();
                    UnkStruct98 = new UnkStruct98();
                    UnkStructA0 = new UnkStructA0();
                }

                private protected override void DeepCopyTo(Part part)
                {
                    var piece = (MapPiece)part;
                    piece.DisplayDataStruct = DisplayDataStruct.DeepCopy();
                    piece.GparamConfigStruct = GparamConfigStruct.DeepCopy();
                    piece.GrassConfigStruct = GrassConfigStruct.DeepCopy();
                    piece.UnkStruct88 = UnkStruct88.DeepCopy();
                    piece.UnkStruct90 = UnkStruct90.DeepCopy();
                    piece.UnkStruct98 = UnkStruct98.DeepCopy();
                    piece.UnkStructA0 = UnkStructA0.DeepCopy();
                }

                internal MapPiece(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                }

                private protected override void ReadUnkOffsetT50(BinaryReaderEx br) => DisplayDataStruct = new DisplayDataStruct(br);
                private protected override void ReadGparamStruct(BinaryReaderEx br) => GparamConfigStruct = new GparamConfigStruct(br);
                private protected override void ReadGrassStruct(BinaryReaderEx br) => GrassConfigStruct = new GrassConfigStruct(br);
                private protected override void ReadUnkOffsetT88(BinaryReaderEx br) => UnkStruct88 = new UnkStruct88(br);
                private protected override void ReadUnkOffsetT90(BinaryReaderEx br) => UnkStruct90 = new UnkStruct90(br);
                private protected override void ReadUnkOffsetT98(BinaryReaderEx br) => UnkStruct98 = new UnkStruct98(br);
                private protected override void ReadUnkOffsetTA0(BinaryReaderEx br) => UnkStructA0 = new UnkStructA0(br);

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }

                private protected override void WriteUnkOffsetT50(BinaryWriterEx bw) => DisplayDataStruct.Write(bw);
                private protected override void WriteGparamStruct(BinaryWriterEx bw) => GparamConfigStruct.Write(bw);
                private protected override void WriteGrassStruct(BinaryWriterEx bw) => GrassConfigStruct.Write(bw);
                private protected override void WriteUnkOffsetT88(BinaryWriterEx bw) => UnkStruct88.Write(bw);
                private protected override void WriteUnkOffsetT90(BinaryWriterEx bw) => UnkStruct90.Write(bw);
                private protected override void WriteUnkOffsetT98(BinaryWriterEx bw) => UnkStruct98.Write(bw);
                private protected override void WriteUnkOffsetTA0(BinaryWriterEx bw) => UnkStructA0.Write(bw);
            }

            /// <summary>
            /// Common base data for enemies and dummy enemies.
            /// </summary>
            public abstract class EnemyBase : Part
            {
                private protected override bool HasUnkOffsetT50 => true;
                private protected override bool HasUnkOffsetT58 => false;
                private protected override bool HasOffsetGparam => true;
                private protected override bool HasOffsetSceneGparam => false;
                private protected override bool HasOffsetGrass => false;
                private protected override bool HasUnkOffsetT88 => true;
                private protected override bool HasUnkOffsetT90 => false;
                private protected override bool HasUnkOffsetT98 => true;
                private protected override bool HasUnkOffsetTA0 => false;

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
                public UnkStruct88 UnkStruct88 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct98 UnkStruct98 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Enemy_ActionButtonParamID { get; set; }

                /// <summary>
                /// An ID in NPCParam that determines a variety of enemy properties.
                /// </summary>
                public int NPCParamID { get; set; }

                /// <summary>
                /// An ID in NPCThinkParam that determines the enemy's AI characteristics.
                /// </summary>
                public int ThinkParamID { get; set; }

                /// <summary>
                /// Talk ID
                /// </summary>
                public int TalkID { get; set; }

                /// <summary>
                /// Unknown.
                /// 10, 30, 100, 200, 210, 300, 600
                /// </summary>
                public short Enemy_Unk16 { get; set; }

                /// <summary>
                /// An ID in CharaInitParam that determines a human's inventory and stats.
                /// </summary>
                public int CharaInitID { get; set; }

                /// <summary>
                /// Should reference the collision the enemy starts on.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Collision))]
                public string CollisionPartName { get; set; }
                public int CollisionPartIndex;

                /// <summary>
                /// Walk route followed by this enemy.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Event.PatrolRoute))]
                public string WalkRouteName { get; set; }
                public short WalkRouteIndex;

                /// <summary>
                /// Unknown. Anim ID?
                /// 1, 11, 12, 13, 14, 20, 30, 31, 70, 100, 103, 110, 120, 122, 131, 150, 200, 300, 330, 400, 451, 480, 500, 600, 900, 6010
                /// </summary>
                public short Enemy_Unk22 { get; set; }

                /// <summary>
                /// Default idle anim ID.
                /// </summary>
                public int BackupEventAnimID { get; set; }

                /// <summary>
                /// Unknown.
                /// Always 0.
                /// </summary>
                public int Enemy_Unk3C { get; set; }

                /// <summary>
                /// Unknown. Appears to be a secondary Entity ID?
                /// </summary>
                public int Enemy_LinkedEntityID { get; set; }

                /// <summary>
                /// Unknown.
                /// Integer.
                /// </summary>
                public int Enemy_Unk44 { get; set; }

                /// <summary>
                /// Unknown.
                /// 0 or 60.
                /// Only 60 in m01_04_80_00 (Xylem)
                /// </summary>
                public int Enemy_Unk48 { get; set; }

                /// <summary>
                /// Unknown.
                /// Integer.
                /// </summary>
                public int Enemy_Unk50 { get; set; }

                /// <summary>
                /// Unknown.
                /// 0 or 1. Boolean?
                /// Only used in Arenas for the Dummy entity.
                /// </summary>
                public byte Enemy_Unk54 { get; set; }

                /// <summary>
                /// Unknown.
                /// 0 or 2.
                /// Only used in m01_03_70_00 for AC enemy Nosaac.
                /// </summary>
                public byte Enemy_Unk55 { get; set; }

                /// <summary>
                /// Determines the kill payout classification when this enemy is killed during destruction-pay missions.
                /// Differs in each mission.
                /// </summary>
                public sbyte KillPayoutClassification { get; set; }

                /// <summary>
                /// PartsTokenParam
                /// </summary>
                public int PartsTokenParamID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                private long UnkEnemyOffset70 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                private long UnkEnemyOffset78 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public class EnemyUnkStruct70
                {
                    /// <summary>
                    /// Unknown.
                    /// Integer.
                    /// </summary>
                    public short EnemyStruct70_Unk00 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// Integer.
                    /// </summary>
                    public short EnemyStruct70_Unk02 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// Integer.
                    /// </summary>
                    public short EnemyStruct70_Unk04 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// Integer.
                    /// </summary>
                    public short EnemyStruct70_Unk06 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// Always 0.
                    /// </summary>
                    public short EnemyStruct70_Unk08 { get; set; }

                    /// <summary>
                    /// Creates an EnemyUnkStruct70 with default values.
                    /// </summary>
                    public EnemyUnkStruct70()
                    {
                        EnemyStruct70_Unk00 = (short)-1;
                        EnemyStruct70_Unk02 = (short)-1;
                        EnemyStruct70_Unk04 = (short)-1;
                        EnemyStruct70_Unk06 = (short)-1;
                        EnemyStruct70_Unk08 = (short)-1;
                    }

                    /// <summary>
                    /// Creates a deep copy of the struct.
                    /// </summary>
                    public EnemyUnkStruct70 DeepCopy()
                    {
                        return (EnemyUnkStruct70)MemberwiseClone();
                    }

                    internal EnemyUnkStruct70(BinaryReaderEx br)
                    {
                        EnemyStruct70_Unk00 = br.ReadInt16();
                        EnemyStruct70_Unk02 = br.ReadInt16();
                        EnemyStruct70_Unk04 = br.ReadInt16();
                        EnemyStruct70_Unk06 = br.ReadInt16();
                        EnemyStruct70_Unk08 = br.ReadInt16();
                        br.AssertInt16(new short[1]);
                        br.AssertInt32(new int[1]);
                    }

                    internal void Write(BinaryWriterEx bw)
                    {
                        bw.WriteInt16(EnemyStruct70_Unk00);
                        bw.WriteInt16(EnemyStruct70_Unk02);
                        bw.WriteInt16(EnemyStruct70_Unk04);
                        bw.WriteInt16(EnemyStruct70_Unk06);
                        bw.WriteInt16(EnemyStruct70_Unk08);
                        bw.WriteInt16((short)0);
                        bw.WriteInt32(0);
                    }
                }

                /// <summary>
                /// Unknown.
                /// </summary>
                public class EnemyUnkStruct78
                {
                    /// <summary>
                    /// Creates an EnemyUnkStruct70 with default values.
                    /// </summary>
                    public EnemyUnkStruct78() { }

                    /// <summary>
                    /// Creates a deep copy of the struct.
                    /// </summary>
                    public EnemyUnkStruct78 DeepCopy()
                    {
                        return (EnemyUnkStruct78)MemberwiseClone();
                    }

                    internal EnemyUnkStruct78(BinaryReaderEx br)
                    {
                        br.AssertInt32(new int[1]);

                        double num1 = (double)br.AssertSingle(1f);
                        for (int index = 0; index < 5; ++index)
                        {
                            br.AssertInt32(-1);
                            int num2 = (int)br.AssertInt16((short)-1);
                            int num3 = (int)br.AssertInt16((short)10);
                        }

                        br.AssertInt32(new int[1]);
                        br.AssertInt32(new int[1]);
                        br.AssertInt32(new int[1]);
                        br.AssertInt32(new int[1]);
                    }

                    internal void Write(BinaryWriterEx bw)
                    {
                        bw.WriteInt32(0);
                        bw.WriteSingle(1f);
                        for (int index = 0; index < 5; ++index)
                        {
                            bw.WriteInt32(-1);
                            bw.WriteInt16((short)-1);
                            bw.WriteInt16((short)10);
                        }
                        bw.WriteInt32(0);
                        bw.WriteInt32(0);
                        bw.WriteInt32(0);
                        bw.WriteInt32(0);
                    }
                }

                /// <summary>
                /// Unknown.
                /// </summary>
                public EnemyUnkStruct70 UnkEnemyStruct70 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public EnemyUnkStruct78 UnkEnemyStruct78 { get; set; }

                private protected EnemyBase() : base("cXXXX_XXXX")
                {
                    DisplayDataStruct = new DisplayDataStruct();
                    GparamConfigStruct = new GparamConfigStruct();
                    UnkStruct88 = new UnkStruct88();
                    UnkStruct98 = new UnkStruct98();

                    Enemy_ActionButtonParamID = -1;
                    CharaInitID = -1;
                    CollisionPartIndex = -1;
                    WalkRouteIndex = (short)-1;
                    Enemy_Unk22 = (short)-1;
                    BackupEventAnimID = -1;
                    Enemy_Unk3C = -1;
                    Enemy_Unk44 = -1;
                    Enemy_Unk50 = -1;
                    KillPayoutClassification = (sbyte)-1;
                    PartsTokenParamID = -1;

                    UnkEnemyStruct70 = new EnemyUnkStruct70();
                    UnkEnemyStruct78 = new EnemyUnkStruct78();
                }

                private protected override void DeepCopyTo(Part part)
                {
                    var enemy = (EnemyBase)part;
                    enemy.DisplayDataStruct = DisplayDataStruct.DeepCopy();
                    enemy.GparamConfigStruct = GparamConfigStruct.DeepCopy();
                    enemy.UnkStruct88 = UnkStruct88.DeepCopy();
                    enemy.UnkStruct98 = UnkStruct98.DeepCopy();

                    enemy.UnkEnemyStruct70 = UnkEnemyStruct70.DeepCopy();
                    enemy.UnkEnemyStruct78 = UnkEnemyStruct78.DeepCopy();
                }

                private protected EnemyBase(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    long start = br.Position;

                    Enemy_ActionButtonParamID = br.ReadInt32();
                    br.AssertInt32(new int[1]);
                    ThinkParamID = br.ReadInt32();
                    NPCParamID = br.ReadInt32();
                    TalkID = br.ReadInt32();
                    br.AssertInt16(new short[1]);
                    Enemy_Unk16 = br.ReadInt16();
                    CharaInitID = br.ReadInt32();
                    CollisionPartIndex = br.ReadInt32();
                    WalkRouteIndex = br.ReadInt16();
                    Enemy_Unk22 = br.ReadInt16();
                    br.AssertInt32(-1);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    BackupEventAnimID = br.ReadInt32();
                    Enemy_Unk3C = br.ReadInt32();
                    Enemy_LinkedEntityID = br.ReadInt32(); // Entity id?
                    Enemy_Unk44 = br.ReadInt32();
                    Enemy_Unk48 = br.ReadInt32();
                    br.AssertInt32(new int[1]);
                    Enemy_Unk50 = br.ReadInt32();
                    Enemy_Unk54 = br.ReadByte();
                    Enemy_Unk55 = br.ReadByte();
                    KillPayoutClassification = br.ReadSByte();
                    br.AssertByte(new byte[1]);
                    PartsTokenParamID = br.ReadInt32();
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);

                    UnkEnemyOffset70 = br.ReadInt64();
                    UnkEnemyOffset78 = br.ReadInt64();

                    br.Position = start + UnkEnemyOffset70;
                    if (UnkEnemyOffset70 != 0L)
                        UnkEnemyStruct70 = new EnemyUnkStruct70(br);

                    br.Position = start + UnkEnemyOffset78;
                    UnkEnemyStruct78 = new EnemyUnkStruct78(br);
                }

                private protected override void ReadUnkOffsetT50(BinaryReaderEx br) => DisplayDataStruct = new DisplayDataStruct(br);
                private protected override void ReadGparamStruct(BinaryReaderEx br) => GparamConfigStruct = new GparamConfigStruct(br);
                private protected override void ReadUnkOffsetT88(BinaryReaderEx br) => UnkStruct88 = new UnkStruct88(br);
                private protected override void ReadUnkOffsetT98(BinaryReaderEx br) => UnkStruct98 = new UnkStruct98(br);

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    long start = bw.Position;

                    bw.WriteInt32(Enemy_ActionButtonParamID);
                    bw.WriteInt32(0);
                    bw.WriteInt32(ThinkParamID);
                    bw.WriteInt32(NPCParamID);
                    bw.WriteInt32(TalkID);
                    bw.WriteInt16((short)0);
                    bw.WriteInt16(Enemy_Unk16);
                    bw.WriteInt32(CharaInitID);
                    bw.WriteInt32(CollisionPartIndex);
                    bw.WriteInt16(WalkRouteIndex);
                    bw.WriteInt16(Enemy_Unk22);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(BackupEventAnimID);
                    bw.WriteInt32(Enemy_Unk3C);
                    bw.WriteInt32(Enemy_LinkedEntityID);
                    bw.WriteInt32(Enemy_Unk44);
                    bw.WriteInt32(Enemy_Unk48);
                    bw.WriteInt32(0);
                    bw.WriteInt32(Enemy_Unk50);
                    bw.WriteByte(Enemy_Unk54);
                    bw.WriteByte(Enemy_Unk55);
                    bw.WriteSByte(KillPayoutClassification);
                    bw.WriteByte((byte)0);
                    bw.WriteInt32(PartsTokenParamID);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);

                    bw.ReserveInt64("UnkEnemyOffset70");
                    bw.ReserveInt64("UnkEnemyOffset78");

                    if (UnkEnemyStruct70 == null)
                    {
                        bw.FillInt64("UnkEnemyOffset70", 0L);
                    }
                    else
                    {
                        bw.FillInt64("UnkEnemyOffset70", bw.Position - start);
                        UnkEnemyStruct70.Write(bw);
                    }

                    bw.FillInt64("UnkEnemyOffset78", bw.Position - start);
                    UnkEnemyStruct78.Write(bw);
                }

                private protected override void WriteUnkOffsetT50(BinaryWriterEx bw) => DisplayDataStruct.Write(bw);
                private protected override void WriteGparamStruct(BinaryWriterEx bw) => GparamConfigStruct.Write(bw);
                private protected override void WriteUnkOffsetT88(BinaryWriterEx bw) => UnkStruct88.Write(bw);
                private protected override void WriteUnkOffsetT98(BinaryWriterEx bw) => UnkStruct98.Write(bw);

                internal override void GetNames(MSB_AC6 msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    CollisionPartName = MSB.FindName(entries.Parts, CollisionPartIndex);
                    WalkRouteName = MSB.FindName(msb.Events.PatrolRoutes, WalkRouteIndex);
                }

                internal override void GetIndices(MSB_AC6 msb, Entries entries)
                {
                    base.GetIndices(msb, entries);
                    CollisionPartIndex = MSB.FindIndex(this, entries.Parts, CollisionPartName);
                    WalkRouteIndex = (short)MSB.FindIndex(this, msb.Events.PatrolRoutes, WalkRouteName);
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
                private protected override bool HasUnkOffsetT50 => true;
                private protected override bool HasUnkOffsetT58 => false;
                private protected override bool HasOffsetGparam => false;
                private protected override bool HasOffsetSceneGparam => false;
                private protected override bool HasOffsetGrass => false;
                private protected override bool HasUnkOffsetT88 => true;
                private protected override bool HasUnkOffsetT90 => false;
                private protected override bool HasUnkOffsetT98 => true;
                private protected override bool HasUnkOffsetTA0 => false;

                /// <summary>
                /// Unknown.
                /// </summary>
                public DisplayDataStruct DisplayDataStruct { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct88 UnkStruct88 { get; set; }

                /// Unknown.
                /// </summary>
                public UnkStruct98 UnkStruct98 { get; set; }

                /// <summary>
                /// Creates a Player with default values.
                /// </summary>
                public Player() : base("c0000_XXXX")
                {
                    DisplayDataStruct = new DisplayDataStruct();
                    UnkStruct88 = new UnkStruct88();
                    UnkStruct98 = new UnkStruct98();
                }

                private protected override void DeepCopyTo(Part part)
                {
                    var player = (Player)part;
                    player.DisplayDataStruct = DisplayDataStruct.DeepCopy();
                    player.UnkStruct88 = UnkStruct88.DeepCopy();
                    player.UnkStruct98 = UnkStruct98.DeepCopy();
                }

                internal Player(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                }
                private protected override void ReadUnkOffsetT50(BinaryReaderEx br) => DisplayDataStruct = new DisplayDataStruct(br);
                private protected override void ReadUnkOffsetT88(BinaryReaderEx br) => UnkStruct88 = new UnkStruct88(br);
                private protected override void ReadUnkOffsetT98(BinaryReaderEx br) => UnkStruct98 = new UnkStruct98(br);

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }

                private protected override void WriteUnkOffsetT50(BinaryWriterEx bw) => DisplayDataStruct.Write(bw);
                private protected override void WriteUnkOffsetT88(BinaryWriterEx bw) => UnkStruct88.Write(bw);
                private protected override void WriteUnkOffsetT98(BinaryWriterEx bw) => UnkStruct98.Write(bw);
            }

            /// <summary>
            /// Invisible but physical geometry.
            /// </summary>
            public class Collision : Part
            {
                private protected override PartType Type => PartType.Collision;
                private protected override bool HasUnkOffsetT50 => true;
                private protected override bool HasUnkOffsetT58 => true;
                private protected override bool HasOffsetGparam => true;
                private protected override bool HasOffsetSceneGparam => true;
                private protected override bool HasOffsetGrass => false;
                private protected override bool HasUnkOffsetT88 => true;
                private protected override bool HasUnkOffsetT90 => false;
                private protected override bool HasUnkOffsetT98 => true;
                private protected override bool HasUnkOffsetTA0 => true;

                /// <summary>
                /// Unknown.
                /// </summary>
                public DisplayDataStruct DisplayDataStruct { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public DisplayStruct DisplayStruct { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public GparamConfigStruct PartStructGparam { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public SceneGparamConfigStruct PartStructSceneGparam { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct88 UnkStruct88 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct98 UnkStruct98 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStructA0 UnkStructA0 { get; set; }

                /// <summary>
                /// Sets collision behavior. Fall collision, death collision, enemy-only collision, etc.
                /// </summary>
                public byte HitFilterID { get; set; } = 8;

                /// <summary>
                /// Unknown.
                /// -1, 1, 2, 3, 4, 5, 7
                /// </summary>
                public sbyte Collision_Unk01 { get; set; }

                /// <summary>
                /// Unknown.
                /// -1, 0, 1, 2
                /// </summary>
                public sbyte Collision_Unk02 { get; set; }

                /// <summary>
                /// Unknown. Always -1.
                /// </summary>
                public sbyte Collision_Unk03 { get; set; }

                /// <summary>
                /// Unknown.
                /// 3, 3000, 20000
                /// </summary>
                public float Collision_Unk04 { get; set; }

                /// <summary>
                /// Unknown. 
                /// 0 or 1. Boolean?
                /// </summary>
                public short Collision_Unk26 { get; set; }

                /// <summary>
                /// Unknown. 
                /// 0 or 1. Boolean?
                /// </summary>
                public short Collision_Unk36 { get; set; }

                /// <summary>
                /// Creates a Collision with default values.
                /// </summary>
                public Collision() : base("hXXXXXX")
                {
                    DisplayDataStruct = new DisplayDataStruct();
                    DisplayStruct = new DisplayStruct();
                    PartStructGparam = new GparamConfigStruct();
                    PartStructSceneGparam = new SceneGparamConfigStruct();
                    UnkStruct88 = new UnkStruct88();
                    UnkStruct98 = new UnkStruct98();
                    UnkStructA0 = new UnkStructA0();
                }

                private protected override void DeepCopyTo(Part part)
                {
                    var collision = (Collision)part;
                    collision.DisplayDataStruct = DisplayDataStruct.DeepCopy();
                    collision.DisplayStruct = DisplayStruct.DeepCopy();
                    collision.PartStructGparam = PartStructGparam.DeepCopy();
                    collision.PartStructSceneGparam = PartStructSceneGparam.DeepCopy();
                    collision.UnkStruct88 = UnkStruct88.DeepCopy();
                    collision.UnkStruct98 = UnkStruct98.DeepCopy();
                    collision.UnkStructA0 = UnkStructA0.DeepCopy();
                }

                internal Collision(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    HitFilterID = br.ReadByte();
                    Collision_Unk01 = br.ReadSByte();
                    Collision_Unk02 = br.ReadSByte();
                    Collision_Unk03 = br.ReadSByte();
                    Collision_Unk04 = br.ReadSingle();
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    int num1 = (int)br.AssertInt16((short)-1);
                    this.Collision_Unk26 = br.ReadInt16();
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(-1);
                    br.AssertInt32(new int[1]);
                    int num2 = (int)br.AssertSByte((sbyte)-1);
                    int num3 = (int)br.AssertByte(new byte[1]);
                    this.Collision_Unk36 = br.ReadInt16();
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(-1);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                }

                private protected override void ReadUnkOffsetT50(BinaryReaderEx br) => DisplayDataStruct = new DisplayDataStruct(br);
                private protected override void ReadUnkOffsetT58(BinaryReaderEx br) => DisplayStruct = new DisplayStruct(br);
                private protected override void ReadGparamStruct(BinaryReaderEx br) => PartStructGparam = new GparamConfigStruct(br);
                private protected override void ReadSceneGparamStruct(BinaryReaderEx br) => PartStructSceneGparam = new SceneGparamConfigStruct(br);
                private protected override void ReadUnkOffsetT88(BinaryReaderEx br) => UnkStruct88 = new UnkStruct88(br);
                private protected override void ReadUnkOffsetT98(BinaryReaderEx br) => UnkStruct98 = new UnkStruct98(br);
                private protected override void ReadUnkOffsetTA0(BinaryReaderEx br) => UnkStructA0 = new UnkStructA0(br);

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteByte((byte)HitFilterID);
                    bw.WriteSByte(Collision_Unk01);
                    bw.WriteSByte(Collision_Unk02);
                    bw.WriteSByte(Collision_Unk03);
                    bw.WriteSingle(Collision_Unk04);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt16((short)-1);
                    bw.WriteInt16(this.Collision_Unk26);
                    bw.WriteInt32(0);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(0);
                    bw.WriteSByte((sbyte)-1);
                    bw.WriteByte((byte)0);
                    bw.WriteInt16(this.Collision_Unk36);
                    bw.WriteInt32(0);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
                private protected override void WriteUnkOffsetT50(BinaryWriterEx bw) => DisplayDataStruct.Write(bw);
                private protected override void WriteUnkOffsetT58(BinaryWriterEx bw) => DisplayStruct.Write(bw);
                private protected override void WriteGparamStruct(BinaryWriterEx bw) => PartStructGparam.Write(bw);
                private protected override void WriteSceneGparamStruct(BinaryWriterEx bw) => PartStructSceneGparam.Write(bw);
                private protected override void WriteUnkOffsetT88(BinaryWriterEx bw) => UnkStruct88.Write(bw);
                private protected override void WriteUnkOffsetT98(BinaryWriterEx bw) => UnkStruct98.Write(bw);
                private protected override void WriteUnkOffsetTA0(BinaryWriterEx bw) => UnkStructA0.Write(bw);
            }

            /// <summary>
            /// This is in the same type of a legacy DummyObject, but struct is pretty gutted
            /// </summary>
            public class DummyAsset : Part
            {
                private protected override PartType Type => PartType.DummyAsset;
                private protected override bool HasUnkOffsetT50 => true;
                private protected override bool HasUnkOffsetT58 => false;
                private protected override bool HasOffsetGparam => true;
                private protected override bool HasOffsetSceneGparam => false;
                private protected override bool HasOffsetGrass => false;
                private protected override bool HasUnkOffsetT88 => true;
                private protected override bool HasUnkOffsetT90 => false;
                private protected override bool HasUnkOffsetT98 => true;
                private protected override bool HasUnkOffsetTA0 => false;

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
                public UnkStruct88 UnkStruct88 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct98 UnkStruct98 { get; set; }

                /// <summary>
                /// Creates a DummyAsset with default values.
                /// </summary>
                public DummyAsset() : base("AEGxxx_xxx_xxxx")
                {
                    DisplayDataStruct = new DisplayDataStruct();
                    GparamConfigStruct = new GparamConfigStruct();
                    UnkStruct88 = new UnkStruct88();
                    UnkStruct98 = new UnkStruct98();
                }

                private protected override void DeepCopyTo(Part part)
                {
                    var asset = (DummyAsset)part;
                    asset.DisplayDataStruct = DisplayDataStruct.DeepCopy();
                    asset.GparamConfigStruct = GparamConfigStruct.DeepCopy();
                    asset.UnkStruct88 = UnkStruct88.DeepCopy();
                    asset.UnkStruct98 = UnkStruct98.DeepCopy();
                }

                internal DummyAsset(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(-1);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(-1);
                    br.AssertInt32(-1);
                    br.AssertInt32(-1);
                    br.AssertInt32(-1);
                }

                private protected override void ReadUnkOffsetT50(BinaryReaderEx br) => DisplayDataStruct = new DisplayDataStruct(br);
                private protected override void ReadGparamStruct(BinaryReaderEx br) => GparamConfigStruct = new GparamConfigStruct(br);
                private protected override void ReadUnkOffsetT88(BinaryReaderEx br) => UnkStruct88 = new UnkStruct88(br);
                private protected override void ReadUnkOffsetT98(BinaryReaderEx br) => UnkStruct98 = new UnkStruct98(br);

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(0);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                }
                private protected override void WriteUnkOffsetT50(BinaryWriterEx bw) => DisplayDataStruct.Write(bw);
                private protected override void WriteGparamStruct(BinaryWriterEx bw) => GparamConfigStruct.Write(bw);
                private protected override void WriteUnkOffsetT88(BinaryWriterEx bw) => UnkStruct88.Write(bw);
                private protected override void WriteUnkOffsetT98(BinaryWriterEx bw) => UnkStruct98.Write(bw);
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
                private protected override bool HasUnkOffsetT50 => true;
                private protected override bool HasUnkOffsetT58 => true;
                private protected override bool HasOffsetGparam => false;
                private protected override bool HasOffsetSceneGparam => false;
                private protected override bool HasOffsetGrass => false;
                private protected override bool HasUnkOffsetT88 => true;
                private protected override bool HasUnkOffsetT90 => false;
                private protected override bool HasUnkOffsetT98 => true;
                private protected override bool HasUnkOffsetTA0 => true;

                /// <summary>
                /// Unknown.
                /// </summary>
                public DisplayDataStruct DisplayDataStruct { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public DisplayStruct DisplayStruct { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct88 UnkStruct88 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct98 UnkStruct98 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStructA0 UnkStructA0 { get; set; }

                /// <summary>
                /// The collision part to attach to.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Collision))]
                [NoRenderGroupInheritence()]
                public string CollisionName { get; set; }
                public int CollisionIndex;

                /// <summary>
                /// The map to load when on this collision.
                /// </summary>
                public sbyte[] MapID { get; private set; }

                /// <summary>
                /// Creates a ConnectCollision with default values.
                /// </summary>
                public ConnectCollision() : base("hXXXXXX_XXXX")
                {
                    CollisionIndex = -1;
                    MapID = new sbyte[4];
                    DisplayDataStruct = new DisplayDataStruct();
                    DisplayStruct = new DisplayStruct();
                    UnkStruct88 = new UnkStruct88();
                    UnkStruct98 = new UnkStruct98();
                    UnkStructA0 = new UnkStructA0();
                }

                private protected override void DeepCopyTo(Part part)
                {
                    var connect = (ConnectCollision)part;
                    connect.MapID = (sbyte[])MapID.Clone();
                    connect.DisplayDataStruct = DisplayDataStruct.DeepCopy();
                    connect.DisplayStruct = DisplayStruct.DeepCopy();
                    connect.UnkStruct88 = UnkStruct88.DeepCopy();
                    connect.UnkStruct98 = UnkStruct98.DeepCopy();
                    connect.UnkStructA0 = UnkStructA0.DeepCopy();
                }

                internal ConnectCollision(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    CollisionIndex = br.ReadInt32();
                    MapID = br.ReadSBytes(4);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                }
                private protected override void ReadUnkOffsetT50(BinaryReaderEx br) => DisplayDataStruct = new DisplayDataStruct(br);
                private protected override void ReadUnkOffsetT58(BinaryReaderEx br) => DisplayStruct = new DisplayStruct(br);
                private protected override void ReadUnkOffsetT88(BinaryReaderEx br) => UnkStruct88 = new UnkStruct88(br);
                private protected override void ReadUnkOffsetT98(BinaryReaderEx br) => UnkStruct98 = new UnkStruct98(br);
                private protected override void ReadUnkOffsetTA0(BinaryReaderEx br) => UnkStructA0 = new UnkStructA0(br);

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(CollisionIndex);
                    bw.WriteSBytes(MapID);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
                private protected override void WriteUnkOffsetT50(BinaryWriterEx bw) => DisplayDataStruct.Write(bw);
                private protected override void WriteUnkOffsetT58(BinaryWriterEx bw) => DisplayStruct.Write(bw);
                private protected override void WriteUnkOffsetT88(BinaryWriterEx bw) => UnkStruct88.Write(bw);
                private protected override void WriteUnkOffsetT98(BinaryWriterEx bw) => UnkStruct98.Write(bw);
                private protected override void WriteUnkOffsetTA0(BinaryWriterEx bw) => UnkStructA0.Write(bw);

                internal override void GetNames(MSB_AC6 msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    CollisionName = MSB.FindName(msb.Parts.Collisions, CollisionIndex);
                }

                internal override void GetIndices(MSB_AC6 msb, Entries entries)
                {
                    base.GetIndices(msb, entries);
                    CollisionIndex = MSB.FindIndex(this, msb.Parts.Collisions, CollisionName);
                }
            }

            /// <summary>
            /// An asset placement in AC6
            /// </summary>
            public class Asset : Part
            {
                private int version;

                private protected override PartType Type => PartType.Asset;
                private protected override bool HasUnkOffsetT50 => true;
                private protected override bool HasUnkOffsetT58 => true;
                private protected override bool HasOffsetGparam => true;
                private protected override bool HasOffsetSceneGparam => false;
                private protected override bool HasOffsetGrass => true;
                private protected override bool HasUnkOffsetT88 => true;
                private protected override bool HasUnkOffsetT90 => true;
                private protected override bool HasUnkOffsetT98 => true;
                private protected override bool HasUnkOffsetTA0 => true;

                /// <summary>
                /// Unknown.
                /// </summary>
                public DisplayDataStruct DisplayDataStruct { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public DisplayStruct DisplayStruct { get; set; }

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
                public UnkStruct88 UnkStruct88 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct90 UnkStruct90 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct98 UnkStruct98 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStructA0 UnkStructA0 { get; set; }

                /// <summary>
                /// Unknown.
                /// Boolean.
                /// </summary>
                public bool Asset_Unk00 { get; set; }

                /// <summary>
                /// Unknown.
                /// 0 or 25.
                /// </summary>
                public byte Asset_Unk01 { get; set; }

                /// <summary>
                /// Unknown.
                /// -1 or 1.
                /// </summary>
                public sbyte Asset_Unk03 { get; set; }

                /// <summary>
                /// Unknown.
                /// -1, 1, 2
                /// </summary>
                public int Asset_Unk04 { get; set; }

                /// <summary>
                /// Unknown.
                /// 0, 3
                /// </summary>
                public byte Asset_Unk10 { get; set; }

                /// <summary>
                /// Unknown.
                /// Boolean.
                /// </summary>
                public bool Asset_Unk11 { get; set; }

                /// <summary>
                /// Unknown.
                /// Boolean.
                /// </summary>
                public bool Asset_Unk12 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public bool Asset_Unk13 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short AssetSfxParamRelativeID { get; set; }

                /// <summary>
                /// Unknown.
                /// -1, 1, 2
                /// </summary>
                public short Asset_Unk1C { get; set; }

                /// <summary>
                /// Unknown.
                /// -1 or 1.
                /// Only 1 in m03_40_00_00.
                /// </summary>
                public short Asset_Unk1E { get; set; }

                /// <summary>
                /// Unknown.
                /// Always -1.
                /// </summary>
                public int Asset_Unk20 { get; set; }

                /// <summary>
                /// Unknown.
                /// -1, 25
                /// </summary>
                public int Asset_Unk24 { get; set; }

                /// <summary>
                /// Unknown.
                /// -1, 1
                /// </summary>
                public int Asset_Unk28 { get; set; }

                /// <summary>
                /// Unknown.
                /// -1, 7, 8, 11, 15, 20, 21
                /// </summary>
                public int Asset_Unk2C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Part))]
                public string[] PartNames { get; private set; }
                private int[] PartIndices { get; set; }

                /// <summary>
                /// Unknown.
                /// -1, 999
                /// </summary>
                public int Asset_Unk44 { get; set; }

                /// <summary>
                /// Unknown.
                /// 0, 560, 561, 562, 564
                /// </summary>
                public int Asset_TalkID { get; set; }

                /// <summary>
                /// Unknown.
                /// 0 or 1. Boolean?
                /// </summary>
                public byte Asset_Unk4C { get; set; }

                /// <summary>
                /// Unknown.
                /// 0 or 1. Boolean?
                /// </summary>
                public byte Asset_Unk4D { get; set; }

                /// <summary>
                /// Unknown.
                /// Always -1.
                /// </summary>
                public short Asset_Unk4E { get; set; }

                /// <summary>
                /// Unknown.
                /// Always -1.
                /// </summary>
                public int Asset_Unk50 { get; set; }

                /// <summary>
                /// Unknown.
                /// Power of 2 values?
                /// </summary>
                public int Asset_Unk54 { get; set; }

                /// <summary>
                /// Unknown.
                /// Integer.
                /// </summary>
                public int Asset_Unk5C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                private long UnkAssetOffset60 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                private long UnkAssetOffset68 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                private long UnkAssetOffset70 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                private long UnkAssetOffset78 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public class AssetUnkStruct60
                {
                    /// <summary>
                    /// Unknown.
                    /// 0 or 1. Boolean?
                    /// </summary>
                    public short AssetStruct60_Unk00 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// 0 or 1. Boolean?
                    /// </summary>
                    public int AssetStruct60_Unk04 { get; set; }

                    /// <summary>
                    /// Creates an AssetUnkStruct60 with default values.
                    /// </summary>
                    public AssetUnkStruct60() { }

                    /// <summary>
                    /// Creates a deep copy of the struct.
                    /// </summary>
                    public AssetUnkStruct60 DeepCopy()
                    {
                        return (AssetUnkStruct60)MemberwiseClone();
                    }

                    internal AssetUnkStruct60(BinaryReaderEx br)
                    {
                        AssetStruct60_Unk00 = br.ReadInt16();
                        br.AssertInt16((short)-1);
                        AssetStruct60_Unk04 = br.ReadInt32();
                        br.AssertInt32(new int[1]);
                        br.AssertInt32(new int[1]);
                        br.AssertInt32(new int[1]);
                        br.AssertInt32(-1);
                        br.AssertInt32(-1);
                        br.AssertInt32(new int[1]);
                        br.AssertInt32(new int[1]);
                        br.AssertInt32(new int[1]);
                        br.AssertInt32(new int[1]);
                        br.AssertInt32(new int[1]);
                        br.AssertInt32(new int[1]);
                        br.AssertInt32(new int[1]);
                        br.AssertInt32(new int[1]);
                        br.AssertInt32(new int[1]);
                    }

                    internal void Write(BinaryWriterEx bw)
                    {
                        bw.WriteInt16(AssetStruct60_Unk00);
                        bw.WriteInt16((short)-1);
                        bw.WriteInt32(AssetStruct60_Unk04);
                        bw.WriteInt32(0);
                        bw.WriteInt32(0);
                        bw.WriteInt32(0);
                        bw.WriteInt32(-1);
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
                    }
                }

                /// <summary>
                /// Unknown.
                /// </summary>
                public class AssetUnkStruct68
                {
                    /// <summary>
                    /// Unknown.
                    /// 0 or 1.
                    /// </summary>
                    public float AssetStruct68_Unk14 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// Always 0.
                    /// </summary>
                    public int AssetStruct68_Unk1C { get; set; }

                    /// <summary>
                    /// Creates an AssetUnkStruct2 with default values.
                    /// </summary>
                    public AssetUnkStruct68()
                    {
                        AssetStruct68_Unk14 = -1;
                        AssetStruct68_Unk1C = -1;
                    }

                    /// <summary>
                    /// Creates a deep copy of the struct.
                    /// </summary>
                    public AssetUnkStruct68 DeepCopy()
                    {
                        return (AssetUnkStruct68)MemberwiseClone();
                    }

                    internal AssetUnkStruct68(BinaryReaderEx br)
                    {
                        br.AssertInt32(new int[1]);
                        br.AssertInt32(-1);
                        br.AssertInt32(-1);
                        br.AssertInt32(new int[1]);
                        br.AssertInt32(new int[1]);
                        AssetStruct68_Unk14 = br.ReadSingle();
                        br.AssertInt32(-1);
                        AssetStruct68_Unk1C = br.ReadInt32();
                        br.AssertInt32(new int[1]);
                        br.AssertInt32(new int[1]);
                        br.AssertInt32(new int[1]);
                        br.AssertInt32(new int[1]);
                        br.AssertInt32(new int[1]);
                        br.AssertInt32(new int[1]);
                        br.AssertInt32(new int[1]);
                        br.AssertInt32(new int[1]);
                    }

                    internal void Write(BinaryWriterEx bw)
                    {
                        bw.WriteInt32(0);
                        bw.WriteInt32(-1);
                        bw.WriteInt32(-1);
                        bw.WriteInt32(0);
                        bw.WriteInt32(0);
                        bw.WriteSingle(AssetStruct68_Unk14);
                        bw.WriteInt32(-1);
                        bw.WriteInt32(AssetStruct68_Unk1C);
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
                public class AssetUnkStruct70
                {
                    /// <summary>
                    /// Unknown.
                    /// 0 or 1.
                    /// </summary>
                    public int AssetStruct70_Unk00 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// Integer.
                    /// </summary>
                    public float AssetStruct70_Unk04 { get; set; }

                    /// <summary>
                    /// Creates an AssetUnkStruct3 with default values.
                    /// </summary>
                    public AssetUnkStruct70() { }

                    /// <summary>
                    /// Creates a deep copy of the struct.
                    /// </summary>
                    public AssetUnkStruct70 DeepCopy()
                    {
                        return (AssetUnkStruct70)MemberwiseClone();
                    }

                    internal AssetUnkStruct70(BinaryReaderEx br)
                    {
                        AssetStruct70_Unk00 = br.ReadInt32();
                        AssetStruct70_Unk04 = br.ReadSingle();
                        br.AssertSByte((sbyte)-1);
                        br.AssertByte(new byte[1]);
                        br.AssertInt16(new short[1]);
                        br.AssertInt32(new int[1]);
                        br.AssertInt32(new int[1]);
                        br.AssertInt32(new int[1]);
                        br.AssertInt32(new int[1]);
                        br.AssertInt32(new int[1]);
                        br.AssertInt32(new int[1]);
                        br.AssertInt32(new int[1]);
                        br.AssertInt32(new int[1]);
                        br.AssertInt32(new int[1]);
                        br.AssertInt32(new int[1]);
                        br.AssertInt32(new int[1]);
                        br.AssertInt32(new int[1]);
                        br.AssertInt32(new int[1]);
                    }

                    internal void Write(BinaryWriterEx bw)
                    {
                        bw.WriteInt32(AssetStruct70_Unk00);
                        bw.WriteSingle(AssetStruct70_Unk04);
                        bw.WriteSByte((sbyte)-1);
                        bw.WriteByte((byte)0);
                        bw.WriteInt16((short)0);
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
                public class AssetUnkStruct78
                {
                    /// <summary>
                    /// Unknown.
                    /// 0 or 1. Boolean
                    /// </summary>
                    public byte AssetStruct78_Unk00 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// -1 or 2.
                    /// </summary>
                    public sbyte AssetStruct78_Unk01 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// -1, 1, 2
                    /// </summary>
                    public sbyte AssetStruct78_Unk02 { get; set; }

                    /// <summary>
                    /// Creates an AssetUnkStruct4 with default values.
                    /// </summary>
                    public AssetUnkStruct78()
                    {
                        AssetStruct78_Unk01 = (sbyte)-1;
                        AssetStruct78_Unk02 = (sbyte)-1;
                    }

                    /// <summary>
                    /// Creates a deep copy of the struct.
                    /// </summary>
                    public AssetUnkStruct78 DeepCopy()
                    {
                        return (AssetUnkStruct78)MemberwiseClone();
                    }

                    internal AssetUnkStruct78(BinaryReaderEx br)
                    {
                        AssetStruct78_Unk00 = br.ReadByte();
                        AssetStruct78_Unk01 = br.ReadSByte();
                        AssetStruct78_Unk02 = br.ReadSByte();
                        br.AssertSByte(new sbyte[1]);
                        br.AssertInt32(new int[1]);
                        br.AssertInt32(new int[1]);
                        br.AssertInt32(new int[1]);
                        br.AssertInt32(new int[1]);
                        br.AssertInt32(new int[1]);
                        br.AssertInt32(new int[1]);
                        br.AssertInt32(new int[1]);
                        br.AssertInt32(new int[1]);
                        br.AssertInt32(new int[1]);
                        br.AssertInt32(new int[1]);
                        br.AssertInt32(new int[1]);
                        br.AssertInt32(new int[1]);
                        br.AssertInt32(new int[1]);
                        br.AssertInt32(new int[1]);
                        br.AssertInt32(new int[1]);
                    }

                    internal void Write(BinaryWriterEx bw)
                    {
                        bw.WriteByte(AssetStruct78_Unk00);
                        bw.WriteSByte(AssetStruct78_Unk01);
                        bw.WriteSByte(AssetStruct78_Unk02);
                        bw.WriteByte((byte)0);
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
                public AssetUnkStruct60 UnkAssetStruct60 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public AssetUnkStruct68 UnkAssetStruct68 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public AssetUnkStruct70 UnkAssetStruct70 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public AssetUnkStruct78 UnkAssetStruct78 { get; set; }

                /// <summary>
                /// Creates an Asset with default values.
                /// </summary>
                public Asset() : base("AEGxxx_xxx_xxxx")
                {
                    DisplayDataStruct = new DisplayDataStruct();
                    DisplayStruct = new DisplayStruct();
                    GparamConfigStruct = new GparamConfigStruct();
                    GrassConfigStruct = new GrassConfigStruct();
                    UnkStruct88 = new UnkStruct88();
                    UnkStruct90 = new UnkStruct90();
                    UnkStruct98 = new UnkStruct98();
                    UnkStructA0 = new UnkStructA0();

                    Asset_Unk04 = -1;
                    AssetSfxParamRelativeID = (short)-1;
                    Asset_Unk1C = (short)-1;
                    Asset_Unk1E = (short)-1;
                    Asset_Unk20 = -1;
                    Asset_Unk24 = -1;
                    Asset_Unk28 = -1;
                    Asset_Unk2C = -1;
                    PartIndices = new int[4];
                    Array.Fill<int>(PartIndices, -1);
                    Asset_Unk44 = -1;
                    Asset_Unk4D = (byte)1;
                    Asset_Unk4E = (short)-1;
                    Asset_Unk5C = -1;

                    UnkAssetStruct60 = new AssetUnkStruct60();
                    UnkAssetStruct68 = new AssetUnkStruct68();
                    UnkAssetStruct70 = new AssetUnkStruct70();
                    UnkAssetStruct78 = new AssetUnkStruct78();

                    PartNames = new string[4];
                }

                private protected override void DeepCopyTo(Part part)
                {
                    var asset = (Asset)part;
                    asset.DisplayDataStruct = DisplayDataStruct.DeepCopy();
                    asset.DisplayStruct = DisplayStruct.DeepCopy();
                    asset.GparamConfigStruct = GparamConfigStruct.DeepCopy();
                    asset.GrassConfigStruct = GrassConfigStruct.DeepCopy();
                    asset.UnkStruct88 = UnkStruct88.DeepCopy();
                    asset.UnkStruct90 = UnkStruct90.DeepCopy();
                    asset.UnkStruct98 = UnkStruct98.DeepCopy();
                    asset.UnkStructA0 = UnkStructA0.DeepCopy();

                    asset.UnkAssetStruct60 = UnkAssetStruct60.DeepCopy();
                    asset.UnkAssetStruct68 = UnkAssetStruct68.DeepCopy();
                    asset.UnkAssetStruct70 = UnkAssetStruct70.DeepCopy();
                    asset.UnkAssetStruct78 = UnkAssetStruct78.DeepCopy();

                    PartNames = (string[])PartNames.Clone();
                }

                internal Asset(BinaryReaderEx br, int _version) : base(br)
                {
                    version = _version;
                }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    long start = br.Position;

                    Asset_Unk00 = br.ReadBoolean();
                    Asset_Unk01 = br.ReadByte();
                    br.AssertByte(new byte[1]);
                    Asset_Unk03 = br.ReadSByte();
                    Asset_Unk04 = br.ReadInt32();
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    Asset_Unk10 = br.ReadByte();
                    Asset_Unk11 = br.ReadBoolean();
                    Asset_Unk12 = br.ReadBoolean();
                    Asset_Unk13 = br.ReadBoolean();
                    AssetSfxParamRelativeID = br.ReadInt16();
                    br.AssertInt16((short)-1);
                    br.AssertInt32(-1);
                    Asset_Unk1C = br.ReadInt16();
                    Asset_Unk1E = br.ReadInt16();
                    Asset_Unk20 = br.ReadInt32();
                    Asset_Unk24 = br.ReadInt32();
                    Asset_Unk28 = br.ReadInt32();
                    Asset_Unk2C = br.ReadInt32();
                    PartIndices = br.ReadInt32s(4);
                    br.AssertInt32(-1);
                    Asset_Unk44 = br.ReadInt32();
                    Asset_TalkID = br.ReadInt32();
                    Asset_Unk4C = br.ReadByte();
                    Asset_Unk4D = br.ReadByte();
                    Asset_Unk4E = br.ReadInt16();
                    Asset_Unk50 = br.ReadInt32();
                    Asset_Unk54 = br.ReadInt32();
                    br.AssertInt32(-1);
                    Asset_Unk5C = br.ReadInt32();

                    UnkAssetOffset60 = br.ReadInt64();
                    UnkAssetOffset68 = br.ReadInt64();
                    UnkAssetOffset70 = br.ReadInt64();
                    UnkAssetOffset78 = br.ReadInt64();

                    if (MSB_AC6.CurrentVersion >= 52)
                    {
                        br.AssertInt32(new int[1]);
                        br.AssertInt32(new int[1]);
                        br.AssertInt32(new int[1]);
                        br.AssertInt32(new int[1]);
                        br.AssertInt32(new int[1]);
                        br.AssertInt32(new int[1]);
                        br.AssertInt32(new int[1]);
                        br.AssertInt32(new int[1]);
                        br.AssertInt32(new int[1]);
                        br.AssertInt32(new int[1]);
                        br.AssertInt32(new int[1]);
                        br.AssertInt32(new int[1]);
                        br.AssertInt32(new int[1]);
                        br.AssertInt32(new int[1]);
                        br.AssertInt32(new int[1]);
                        br.AssertInt32(new int[1]);
                    }

                    br.Position = start + UnkAssetOffset60;
                    UnkAssetStruct60 = new AssetUnkStruct60(br);

                    br.Position = start + UnkAssetOffset68;
                    UnkAssetStruct68 = new AssetUnkStruct68(br);

                    br.Position = start + UnkAssetOffset70;
                    UnkAssetStruct70 = new AssetUnkStruct70(br);

                    br.Position = start + UnkAssetOffset78;
                    UnkAssetStruct78 = new AssetUnkStruct78(br);
                }
                private protected override void ReadUnkOffsetT50(BinaryReaderEx br) => DisplayDataStruct = new DisplayDataStruct(br);
                private protected override void ReadUnkOffsetT58(BinaryReaderEx br) => DisplayStruct = new DisplayStruct(br);
                private protected override void ReadGparamStruct(BinaryReaderEx br) => GparamConfigStruct = new GparamConfigStruct(br);
                private protected override void ReadGrassStruct(BinaryReaderEx br) => GrassConfigStruct = new GrassConfigStruct(br);
                private protected override void ReadUnkOffsetT88(BinaryReaderEx br) => UnkStruct88 = new UnkStruct88(br);
                private protected override void ReadUnkOffsetT90(BinaryReaderEx br) => UnkStruct90 = new UnkStruct90(br);
                private protected override void ReadUnkOffsetT98(BinaryReaderEx br) => UnkStruct98 = new UnkStruct98(br);
                private protected override void ReadUnkOffsetTA0(BinaryReaderEx br) => UnkStructA0 = new UnkStructA0(br);

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    long start = bw.Position;

                    bw.WriteBoolean(Asset_Unk00);
                    bw.WriteByte(Asset_Unk01);
                    bw.WriteByte((byte)0);
                    bw.WriteSByte(Asset_Unk03);
                    bw.WriteInt32(Asset_Unk04);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteByte(Asset_Unk10);
                    bw.WriteBoolean(Asset_Unk11);
                    bw.WriteBoolean(Asset_Unk12);
                    bw.WriteBoolean(Asset_Unk13);
                    bw.WriteInt16(AssetSfxParamRelativeID);
                    bw.WriteInt16((short)-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt16(Asset_Unk1C);
                    bw.WriteInt16(Asset_Unk1E);
                    bw.WriteInt32(Asset_Unk20);
                    bw.WriteInt32(Asset_Unk24);
                    bw.WriteInt32(Asset_Unk28);
                    bw.WriteInt32(Asset_Unk2C);
                    bw.WriteInt32s(PartIndices);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(Asset_Unk44);
                    bw.WriteInt32(Asset_TalkID);
                    bw.WriteByte(Asset_Unk4C);
                    bw.WriteByte(Asset_Unk4D);
                    bw.WriteInt16(Asset_Unk4E);
                    bw.WriteInt32(Asset_Unk50);
                    bw.WriteInt32(Asset_Unk54);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(Asset_Unk5C);

                    bw.ReserveInt64("UnkAssetOffset60");
                    bw.ReserveInt64("UnkAssetOffset68");
                    bw.ReserveInt64("UnkAssetOffset70");
                    bw.ReserveInt64("UnkAssetOffset78");

                    if (MSB_AC6.CurrentVersion >= 52)
                    {
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
                        bw.WriteInt32(0);
                    }

                    bw.FillInt64("UnkAssetOffset60", bw.Position - start);
                    UnkAssetStruct60.Write(bw);

                    bw.FillInt64("UnkAssetOffset68", bw.Position - start);
                    UnkAssetStruct68.Write(bw);

                    bw.FillInt64("UnkAssetOffset70", bw.Position - start);
                    UnkAssetStruct70.Write(bw);

                    bw.FillInt64("UnkAssetOffset78", bw.Position - start);
                    UnkAssetStruct78.Write(bw);
                }
                private protected override void WriteUnkOffsetT50(BinaryWriterEx bw) => DisplayDataStruct.Write(bw);
                private protected override void WriteUnkOffsetT58(BinaryWriterEx bw) => DisplayStruct.Write(bw);
                private protected override void WriteGparamStruct(BinaryWriterEx bw) => GparamConfigStruct.Write(bw);
                private protected override void WriteGrassStruct(BinaryWriterEx bw) => GrassConfigStruct.Write(bw);
                private protected override void WriteUnkOffsetT88(BinaryWriterEx bw) => UnkStruct88.Write(bw);
                private protected override void WriteUnkOffsetT90(BinaryWriterEx bw) => UnkStruct90.Write(bw);
                private protected override void WriteUnkOffsetT98(BinaryWriterEx bw) => UnkStruct98.Write(bw);
                private protected override void WriteUnkOffsetTA0(BinaryWriterEx bw) => UnkStructA0.Write(bw);
                internal override void GetNames(MSB_AC6 msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    PartNames = MSB.FindNames(entries.Parts, PartIndices);
                }

                internal override void GetIndices(MSB_AC6 msb, Entries entries)
                {
                    base.GetIndices(msb, entries);
                    PartIndices = MSB.FindIndices(this, entries.Parts, PartNames);
                }
            }
        }
    }
}
