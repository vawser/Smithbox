using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using static SoulsFormats.MSBE.Part;

namespace SoulsFormats
{
    public partial class MSB_NR
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

        public abstract class Part : Entry, IMsbPart
        {
            private protected abstract PartType Type { get; }

            private protected Part(string name)
            {
                Name = name;
                SibPath = "";
                Scale = Vector3.One;
            }

            public Part DeepCopy()
            {
                var part = (Part)MemberwiseClone();
                DeepCopyTo(part);
                return part;
            }
            IMsbPart IMsbPart.DeepCopy() => DeepCopy();

            private protected virtual void DeepCopyTo(Part part) { }

            private protected Part(BinaryReaderEx br)
            {
                long start = br.Position;

                NameOffset = br.ReadInt64();
                InstanceID = br.ReadInt32();
                br.AssertUInt32((uint)Type);
                TypeIndex = br.ReadInt32();
                ModelIndex = br.ReadInt32();
                FileOffset = br.ReadInt64();
                Position = br.ReadVector3();
                Rotation = br.ReadVector3();
                Scale = br.ReadVector3();
                Unk44 = br.ReadInt32();
                MapStudioLayer = br.ReadInt32();
                Unk4C = br.ReadInt32();

                Display_DataOffset = br.ReadInt64();
                DisplayGroup_DataOffset = br.ReadInt64();
                Entity_DataOffset = br.ReadInt64();
                Type_DataOffset = br.ReadInt64();
                Gparam_DataOffset = br.ReadInt64();
                SceneGparam_DataOffset = br.ReadInt64();
                Grass_DataOffset = br.ReadInt64();
                Unk88_DataOffset = br.ReadInt64();
                Unk90_DataOffset = br.ReadInt64();
                Tile_DataOffset = br.ReadInt64();
                UnkA0_DataOffset = br.ReadInt64();
                UnkA8_DataOffset = br.ReadInt64();

                if (NameOffset == 0)
                    throw new InvalidDataException($"{nameof(NameOffset)} must not be 0 in type {GetType()}.");

                if (FileOffset == 0)
                    throw new InvalidDataException($"{nameof(FileOffset)} must not be 0 in type {GetType()}.");

                if (HasDisplayData ^ Display_DataOffset != 0)
                    throw new InvalidDataException($"Unexpected {nameof(Display_DataOffset)} 0x{Display_DataOffset:X} in type {GetType()}.");

                if (HasDisplayGroupData ^ DisplayGroup_DataOffset != 0)
                    throw new InvalidDataException($"Unexpected {nameof(DisplayGroup_DataOffset)} 0x{DisplayGroup_DataOffset:X} in type {GetType()}.");

                if (Entity_DataOffset == 0)
                    throw new InvalidDataException($"{nameof(Entity_DataOffset)} must not be 0 in type {GetType()}.");

                if (Type_DataOffset == 0)
                    throw new InvalidDataException($"{nameof(Type_DataOffset)} must not be 0 in type {GetType()}.");

                if (HasGparamData ^ Gparam_DataOffset != 0)
                    throw new InvalidDataException($"Unexpected {nameof(Gparam_DataOffset)} 0x{Gparam_DataOffset:X} in type {GetType()}.");

                if (HasSceneGparamData ^ SceneGparam_DataOffset != 0)
                    throw new InvalidDataException($"Unexpected {nameof(SceneGparam_DataOffset)} 0x{SceneGparam_DataOffset:X} in type {GetType()}.");

                if (HasGrassData ^ Grass_DataOffset != 0)
                    throw new InvalidDataException($"Unexpected {nameof(Grass_DataOffset)} 0x{Grass_DataOffset:X} in type {GetType()}.");

                if (HasUnk88Data ^ Unk88_DataOffset != 0)
                    throw new InvalidDataException($"Unexpected {nameof(Unk88_DataOffset)} 0x{Unk88_DataOffset:X} in type {GetType()}.");

                if (HasUnk90Data ^ Unk88_DataOffset != 0)
                    throw new InvalidDataException($"Unexpected {nameof(Unk90_DataOffset)} 0x{Unk90_DataOffset:X} in type {GetType()}.");

                if (HasTileData ^ Tile_DataOffset != 0)
                    throw new InvalidDataException($"Unexpected {nameof(Tile_DataOffset)} 0x{Tile_DataOffset:X} in type {GetType()}.");

                if (HasUnkA0Data ^ UnkA0_DataOffset != 0)
                    throw new InvalidDataException($"Unexpected {nameof(UnkA0_DataOffset)} 0x{UnkA0_DataOffset:X} in type {GetType()}.");

                if (HasUnkA8Data ^ UnkA8_DataOffset != 0)
                    throw new InvalidDataException($"Unexpected {nameof(UnkA8_DataOffset)} 0x{UnkA8_DataOffset:X} in type {GetType()}.");

                UnkB0 = br.ReadInt64();
                UnkB8 = br.ReadInt64();

                // Name
                br.Position = start + NameOffset;
                Name = br.ReadUTF16();

                // File SIB
                br.Position = start + FileOffset;
                SibPath = br.ReadUTF16();

                if (HasDisplayData)
                {
                    br.Position = start + Display_DataOffset;
                    ReadDisplayData(br);
                }

                if (HasDisplayGroupData)
                {
                    br.Position = start + DisplayGroup_DataOffset;
                    ReadDisplayGroupData(br);
                }

                br.Position = start + Entity_DataOffset;
                ReadEntityData(br);

                br.Position = start + Type_DataOffset;
                ReadTypeData(br);

                if (HasGparamData)
                {
                    br.Position = start + Gparam_DataOffset;
                    ReadGparamData(br);
                }

                if (HasSceneGparamData)
                {
                    br.Position = start + SceneGparam_DataOffset;
                    ReadSceneGparamData(br);
                }

                if (HasGrassData)
                {
                    br.Position = start + Grass_DataOffset;
                    ReadGrassData(br);
                }

                if (HasUnk88Data)
                {
                    br.Position = start + Unk88_DataOffset;
                    ReadUnk88Data(br);
                }

                if (HasUnk90Data)
                {
                    br.Position = start + Unk90_DataOffset;
                    ReadUnk90Data(br);
                }

                if (HasTileData)
                {
                    br.Position = start + Tile_DataOffset;
                    ReadTileData(br);
                }

                if (HasUnkA0Data)
                {
                    br.Position = start + UnkA0_DataOffset;
                    ReadUnkA0Data(br);
                }

                if (HasUnkA8Data)
                {
                    br.Position = start + UnkA8_DataOffset;
                    ReadUnkA8Data(br);
                }
            }
            private protected abstract bool HasDisplayData { get; }
            private protected abstract bool HasDisplayGroupData { get; }
            private protected abstract bool HasGparamData { get; }
            private protected abstract bool HasSceneGparamData { get; }
            private protected abstract bool HasGrassData { get; }
            private protected abstract bool HasUnk88Data { get; }
            private protected abstract bool HasUnk90Data { get; }
            private protected abstract bool HasTileData { get; }
            private protected abstract bool HasUnkA0Data { get; }
            private protected abstract bool HasUnkA8Data { get; }

            // Part
            private long NameOffset { get; set; }

            public int InstanceID { get; set; } = -1;
            private int TypeIndex { get; set; }
            private int ModelIndex { get; set; } = -1;
            public string ModelName { get; set; }

            private long FileOffset { get; set; }

            public Vector3 Position { get; set; } = new Vector3();
            public Vector3 Rotation { get; set; } = new Vector3();
            public Vector3 Scale { get; set; } = new Vector3();
            public int Unk44 { get; set; } = 0;
            public int MapStudioLayer { get; set; } = -1;
            public int Unk4C { get; set; } = 0;

            private long Display_DataOffset { get; set; }
            private long DisplayGroup_DataOffset { get; set; }
            private long Entity_DataOffset { get; set; }
            private long Type_DataOffset { get; set; }
            private long Gparam_DataOffset { get; set; }
            private long SceneGparam_DataOffset { get; set; }
            private long Grass_DataOffset { get; set; }
            private long Unk88_DataOffset { get; set; }
            private long Unk90_DataOffset { get; set; }
            private long Tile_DataOffset { get; set; }
            private long UnkA0_DataOffset { get; set; }
            private long UnkA8_DataOffset { get; set; }
            private long UnkB0 { get; set; } = 0;
            private long UnkB8 { get; set; } = 0;

            public string SibPath { get; set; }

            private protected abstract void ReadTypeData(BinaryReaderEx br);

            private protected virtual void ReadDisplayData(BinaryReaderEx br)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(ReadDisplayData)}.");

            private protected virtual void ReadDisplayGroupData(BinaryReaderEx br)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(ReadDisplayGroupData)}.");

            private protected virtual void ReadEntityData(BinaryReaderEx br)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(ReadEntityData)}.");
            private protected virtual void ReadGparamData(BinaryReaderEx br)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(ReadGparamData)}.");

            private protected virtual void ReadSceneGparamData(BinaryReaderEx br)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(ReadSceneGparamData)}.");

            private protected virtual void ReadGrassData(BinaryReaderEx br)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(ReadGrassData)}.");

            private protected virtual void ReadUnk88Data(BinaryReaderEx br)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(ReadUnk88Data)}.");

            private protected virtual void ReadUnk90Data(BinaryReaderEx br)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(ReadUnk90Data)}.");

            private protected virtual void ReadTileData(BinaryReaderEx br)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(ReadTileData)}.");

            private protected virtual void ReadUnkA0Data(BinaryReaderEx br)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(ReadUnkA0Data)}.");

            private protected virtual void ReadUnkA8Data(BinaryReaderEx br)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(ReadUnkA8Data)}.");

            internal override void Write(BinaryWriterEx bw, int id)
            {
                long start = bw.Position;

                bw.ReserveInt64("NameOffset");

                bw.WriteInt32(InstanceID);
                bw.WriteUInt32((uint)Type);
                bw.WriteInt32(TypeIndex);
                bw.WriteInt32(ModelIndex);

                bw.ReserveInt64("FileOffset");

                bw.WriteVector3(Position);
                bw.WriteVector3(Rotation);
                bw.WriteVector3(Scale);

                bw.WriteInt32(Unk44);
                bw.WriteInt32(MapStudioLayer);
                bw.WriteInt32(Unk4C);

                bw.ReserveInt64("DisplayOffset");
                bw.ReserveInt64("DisplayGroupOffset");
                bw.ReserveInt64("EntityOffset");
                bw.ReserveInt64("TypeOffset");
                bw.ReserveInt64("GparamOffset");
                bw.ReserveInt64("SceneGparamtOffset");
                bw.ReserveInt64("GrassOffset");
                bw.ReserveInt64("Unk88Offset");
                bw.ReserveInt64("Unk90Offset");
                bw.ReserveInt64("TileOffset");
                bw.ReserveInt64("UnkA0Offset");
                bw.ReserveInt64("UnkA8Offset");
                bw.WriteInt64(UnkB0);
                bw.WriteInt64(UnkB8);

                bw.FillInt64("NameOffset", bw.Position - start);
                bw.WriteUTF16(MSB.ReambiguateName(Name), true);

                bw.FillInt64("FileOffset", bw.Position - start);
                bw.WriteUTF16(SibPath, true);
                bw.Pad(8);

                if (HasDisplayData)
                {
                    bw.FillInt64("DisplayOffset", bw.Position - start);
                    WriteDisplayData(bw);
                }
                else
                {
                    bw.FillInt64("DisplayOffset", 0);
                }

                if (HasDisplayGroupData)
                {
                    bw.FillInt64("DisplayGroupOffset", bw.Position - start);
                    WriteDisplayGroupData(bw);
                }
                else
                {
                    bw.FillInt64("DisplayGroupOffset", 0);
                }

                bw.FillInt64("EntityOffset", bw.Position - start);
                WriteEntityData(bw);

                bw.FillInt64("TypeOffset", bw.Position - start);
                WriteTypeData(bw);

                if (HasGparamData)
                {
                    bw.FillInt64("GparamOffset", bw.Position - start);
                    WriteGparamData(bw);
                }
                else
                {
                    bw.FillInt64("GparamOffset", 0);
                }

                if (HasSceneGparamData)
                {
                    bw.FillInt64("SceneGparamOffset", bw.Position - start);
                    WriteSceneGparamData(bw);
                }
                else
                {
                    bw.FillInt64("SceneGparamOffset", 0);
                }

                if (HasGrassData)
                {
                    bw.FillInt64("GrassOffset", bw.Position - start);
                    WriteGrassData(bw);
                }
                else
                {
                    bw.FillInt64("GrassOffset", 0);
                }

                if (HasUnk88Data)
                {
                    bw.FillInt64("Unk88Offset", bw.Position - start);
                    WriteUnk88Data(bw);
                }
                else
                {
                    bw.FillInt64("Unk88Offset", 0);
                }

                if (HasUnk90Data)
                {
                    bw.FillInt64("Unk90Offset", bw.Position - start);
                    WriteUnk90Data(bw);
                }
                else
                {
                    bw.FillInt64("Unk90Offset", 0);
                }

                if (HasTileData)
                {
                    bw.FillInt64("TileOffset", bw.Position - start);
                    WriteTileData(bw);
                }
                else
                {
                    bw.FillInt64("TileOffset", 0);
                }

                if (HasUnkA0Data)
                {
                    bw.FillInt64("UnkA0Offset", bw.Position - start);
                    WriteUnkA0Data(bw);
                }
                else
                {
                    bw.FillInt64("UnkA0Offset", 0);
                }

                if (HasUnkA8Data)
                {
                    bw.FillInt64("UnkA8Offset", bw.Position - start);
                    WriteUnkA8Data(bw);
                }
                else
                {
                    bw.FillInt64("UnkA8Offset", 0);
                }
            }

            private protected abstract void WriteTypeData(BinaryWriterEx bw);
            private protected virtual void WriteDisplayData(BinaryWriterEx bw)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(WriteDisplayData)}.");

            private protected virtual void WriteDisplayGroupData(BinaryWriterEx bw)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(WriteDisplayGroupData)}.");

            private protected virtual void WriteEntityData(BinaryWriterEx bw)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(WriteEntityData)}.");

            private protected virtual void WriteGparamData(BinaryWriterEx bw)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(WriteGparamData)}.");

            private protected virtual void WriteSceneGparamData(BinaryWriterEx bw)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(WriteSceneGparamData)}.");

            private protected virtual void WriteGrassData(BinaryWriterEx bw)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(WriteGrassData)}.");

            private protected virtual void WriteUnk88Data(BinaryWriterEx bw)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(WriteUnk88Data)}.");

            private protected virtual void WriteUnk90Data(BinaryWriterEx bw)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(WriteUnk90Data)}.");

            private protected virtual void WriteTileData(BinaryWriterEx bw)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(WriteTileData)}.");

            private protected virtual void WriteUnkA0Data(BinaryWriterEx bw)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(WriteUnkA0Data)}.");

            private protected virtual void WriteUnkA8Data(BinaryWriterEx bw)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(WriteUnkA8Data)}.");

            internal virtual void GetNames(MSB_NR msb, Entries entries)
            {
                ModelName = MSB.FindName(entries.Models, ModelIndex);
            }

            internal virtual void GetIndices(MSB_NR msb, Entries entries)
            {
                ModelIndex = MSB.FindIndex(this, entries.Models, ModelName);
            }
            public override string ToString()
            {
                return $"{Type} {Name}";
            }

            public class DisplayStruct
            {
                public DisplayStruct() { }

                public DisplayStruct DeepCopy()
                {
                    var unk1 = (DisplayStruct)MemberwiseClone();
                    unk1.DisplayGroups = (uint[])DisplayGroups.Clone();
                    unk1.DrawGroups = (uint[])DrawGroups.Clone();
                    unk1.CollisionMask = (uint[])CollisionMask.Clone();
                    return unk1;
                }

                internal DisplayStruct(BinaryReaderEx br)
                {
                    DisplayGroups = br.ReadUInt32s(8);
                    DrawGroups = br.ReadUInt32s(8);
                    CollisionMask = br.ReadUInt32s(32);
                    UnkC0 = br.ReadByte();
                    UnkC1 = br.ReadByte();
                    UnkC2 = br.ReadInt16();
                    UnkC4 = br.ReadInt16();
                    UnkC6 = br.ReadInt16(); 
                    br.AssertPattern(0xC0, 0x00);
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteUInt32s(DisplayGroups);
                    bw.WriteUInt32s(DrawGroups);
                    bw.WriteUInt32s(CollisionMask);
                    bw.WriteByte(UnkC0);
                    bw.WriteByte(UnkC1);
                    bw.WriteInt16(UnkC2);
                    bw.WriteInt16(UnkC4);
                    bw.WriteInt16(UnkC6);

                    bw.WritePattern(0xC0, 0x00); // 48 * 4
                }

                // Layout
                public uint[] DisplayGroups { get; set; } = new uint[8];
                public uint[] DrawGroups { get; set; } = new uint[8];
                public uint[] CollisionMask { get; set; } = new uint[32];
                public byte UnkC0 { get; set; } = 0; // Bool
                public byte UnkC1 { get; set; } = 0; // Bool
                public short UnkC2 { get; set; } = 0; // Hidden
                public short UnkC4 { get; set; } = -1; // Hidden
                public short UnkC6 { get; set; } = 0; // Hidden

            }

            public class DisplayGroupStruct
            {
                public DisplayGroupStruct() { }

                public DisplayGroupStruct DeepCopy()
                {
                    var unk2 = (DisplayGroupStruct)MemberwiseClone();
                    unk2.DispGroups = (uint[])DispGroups.Clone();
                    return unk2;
                }
                internal DisplayGroupStruct(BinaryReaderEx br)
                {
                    Unk00 = br.ReadInt32();
                    DispGroups = br.ReadUInt32s(8);
                    Unk24 = br.ReadInt16();
                    Unk26 = br.ReadInt16();
                    Unk28 = br.ReadInt32();
                    Unk2C = br.ReadInt32();
                    Unk30 = br.ReadInt32();
                    Unk34 = br.ReadInt32();
                    Unk38 = br.ReadInt32();
                    Unk3C = br.ReadInt32();
                    Unk40 = br.ReadInt32();
                    Unk44 = br.ReadInt32();
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteUInt32s(DispGroups);
                    bw.WriteInt16(Unk24);
                    bw.WriteInt16(Unk26);
                    bw.WriteInt32(Unk28);
                    bw.WriteInt32(Unk2C);
                    bw.WriteInt32(Unk30);
                    bw.WriteInt32(Unk34);
                    bw.WriteInt32(Unk38);
                    bw.WriteInt32(Unk3C);
                    bw.WriteInt32(Unk40);
                    bw.WriteInt32(Unk44);
                }

                // Layout
                public int Unk00 { get; set; } = -1;
                public uint[] DispGroups { get; set; } = new uint[8];
                public short Unk24 { get; set; } = 0; // Hidden
                public short Unk26 { get; set; } = -1; // Hidden
                public int Unk28 { get; set; } = 0; // Hidden
                public int Unk2C { get; set; } = 0; // Hidden
                public int Unk30 { get; set; } = 0; // Hidden
                public int Unk34 { get; set; } = 0; // Hidden
                public int Unk38 { get; set; } = 0; // Hidden
                public int Unk3C { get; set; } = 0; // Hidden
                public int Unk40 { get; set; } = 0; // Hidden
                public int Unk44 { get; set; } = 0; // Hidden

            }

            public class EntityStruct
            {
                public EntityStruct() { }

                public EntityStruct DeepCopy()
                {
                    return (EntityStruct)MemberwiseClone();
                }

                internal EntityStruct(BinaryReaderEx br)
                {
                    EntityID = br.ReadUInt32();
                    Unk04 = br.ReadByte();
                    Unk05 = br.ReadByte();
                    Unk06 = br.ReadInt16();
                    Unk08 = br.ReadInt32();

                    Unk0C = br.ReadByte();
                    Unk0D = br.ReadByte();
                    Unk0E = br.ReadByte();
                    Unk0F = br.ReadByte();
                    Unk10 = br.ReadByte();
                    Unk11 = br.ReadByte();
                    Unk12 = br.ReadInt16();

                    Unk14 = br.ReadByte();
                    Unk15 = br.ReadByte();
                    Unk16 = br.ReadByte();
                    Unk17 = br.ReadByte();
                    Unk18 = br.ReadInt16();

                    Unk1A = br.ReadByte();
                    Unk1B = br.ReadByte();

                    EntityGroupIDs = br.ReadUInt32s(8);

                    Unk3C = br.ReadInt16();
                    Unk3E = br.ReadInt16();
                    Unk40 = br.ReadInt32();
                    Variation = br.ReadInt32();

                    Unk48 = br.ReadInt32();
                    Unk4C = br.ReadInt32();
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteUInt32(EntityID);
                    bw.WriteByte(Unk04);
                    bw.WriteByte(Unk05);
                    bw.WriteInt16(Unk06);
                    bw.WriteInt32(Unk08);

                    bw.WriteByte(Unk0C);
                    bw.WriteByte(Unk0D);
                    bw.WriteByte(Unk0E);
                    bw.WriteByte(Unk0F);
                    bw.WriteByte(Unk10);
                    bw.WriteByte(Unk11);
                    bw.WriteInt16(Unk12);

                    bw.WriteByte(Unk14);
                    bw.WriteByte(Unk15);
                    bw.WriteByte(Unk16);
                    bw.WriteByte(Unk17);
                    bw.WriteInt16(Unk18);

                    bw.WriteByte(Unk1A);
                    bw.WriteByte(Unk1B);

                    bw.WriteUInt32s(EntityGroupIDs);

                    bw.WriteInt16(Unk3C);
                    bw.WriteInt16(Unk3E);
                    bw.WriteInt32(Unk40);
                    bw.WriteInt32(Variation);

                    bw.WriteInt32(Unk48);
                    bw.WriteInt32(Unk4C);
                }

                // Layout
                public uint EntityID { get; set; } = 0;
                public byte Unk04 { get; set; } = 0; // Bool
                public byte Unk05 { get; set; } = 0; // Bool
                public short Unk06 { get; set; } = 0; // Hidden
                public int Unk08 { get; set; } = 0; // Bool
                public byte Unk0C { get; set; } = 0; // Bool
                public byte Unk0D { get; set; } = 0; // Bool
                public byte Unk0E { get; set; } = 0; // Bool
                public byte Unk0F { get; set; } = 1; // Bool
                public byte Unk10 { get; set; } = 0; // Bool
                public byte Unk11 { get; set; } = 1; // Bool
                public short Unk12 { get; set; } = 0; // Hidden
                public byte Unk14 { get; set; } = 0; // Bool
                public byte Unk15 { get; set; } = 0; // Bool
                public byte Unk16 { get; set; } = 0; // Bool
                public byte Unk17 { get; set; } = 0; // Bool
                public short Unk18 { get; set; } = 0; // Hidden
                public byte Unk1A { get; set; } = 0;  // Bool
                public byte Unk1B { get; set; } = 0;
                public uint[] EntityGroupIDs { get; set; } = new uint[8];
                public short Unk3C { get; set; } = -1;
                public short Unk3E { get; set; } = 0;
                private int Unk40 { get; set; } = 0;
                private int Variation { get; set; } = -1;
                private int Unk48 { get; set; } // Hidden
                private int Unk4C { get; set; }

            }

            public class GparamStruct
            {
                public GparamStruct() { }

                public GparamStruct DeepCopy()
                {
                    return (GparamStruct)MemberwiseClone();
                }
                public override string ToString()
                {
                    return $"{LightID}, {FogID}";
                }

                internal GparamStruct(BinaryReaderEx br)
                {
                    LightID = br.ReadInt32();
                    FogID = br.ReadInt32();
                    Unk08 = br.ReadInt32();
                    Unk0C = br.ReadInt32();
                    Unk10 = br.ReadInt32();
                    Unk14 = br.ReadInt32();
                    Unk18 = br.ReadInt32();
                    Unk1C = br.ReadInt32();
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteInt32(LightID);
                    bw.WriteInt32(FogID);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                    bw.WriteInt32(Unk10);
                    bw.WriteInt32(Unk14);
                    bw.WriteInt32(Unk18);
                    bw.WriteInt32(Unk1C);
                }

                // Layout
                public int LightID { get; set; } = -1;
                public int FogID { get; set; } = -1;
                public int Unk08 { get; set; } = 0; // Hidden
                public int Unk0C { get; set; } = 0; // Hidden
                public int Unk10 { get; set; } = 0; // Hidden
                public int Unk14 { get; set; } = 0; // Hidden
                public int Unk18 { get; set; } = 0; // Hidden
                public int Unk1C { get; set; } = 0; // Hidden

            }
            public class SceneGparamStruct
            {
                public SceneGparamStruct() { }

                public SceneGparamStruct DeepCopy()
                {
                    var config = (SceneGparamStruct)MemberwiseClone();
                    return config;
                }

                internal SceneGparamStruct(BinaryReaderEx br)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadInt32();
                    Unk08 = br.ReadInt32();
                    Unk0C = br.ReadInt32();
                    TransitionTime = br.ReadSingle();
                    Unk14 = br.ReadInt32();
                    Unk18 = br.ReadSByte();
                    Unk19 = br.ReadSByte();
                    Unk1A = br.ReadInt16();
                    Unk1C = br.ReadSByte();
                    Unk1D = br.ReadSByte();
                    Unk1E = br.ReadInt16();
                    Unk20 = br.ReadInt16();
                    Unk22 = br.ReadInt16();
                    Unk24 = br.ReadInt32();
                    Unk28 = br.ReadInt32();
                    Unk30 = br.ReadInt32();
                    Unk34 = br.ReadInt32();
                    Unk38 = br.ReadInt32();
                    Unk3C = br.ReadInt32();
                    Unk40 = br.ReadInt32();
                    Unk44 = br.ReadInt32();
                    Unk48 = br.ReadInt32();
                    Unk4C = br.ReadInt32();
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                    bw.WriteSingle(TransitionTime);
                    bw.WriteInt32(Unk14);
                    bw.WriteSByte(Unk18);
                    bw.WriteSByte(Unk19);
                    bw.WriteInt16(Unk1A);
                    bw.WriteSByte(Unk1C);
                    bw.WriteSByte(Unk1D);
                    bw.WriteInt16(Unk1E);
                    bw.WriteInt16(Unk20);
                    bw.WriteInt16(Unk22);
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
                }

                // Layout
                public int Unk00 { get; set; } = 0; // Hidden
                public int Unk04 { get; set; } = 0; // Hidden
                public int Unk08 { get; set; } = 0; // Hidden
                public int Unk0C { get; set; } = 0; // Hidden
                public float TransitionTime { get; set; } = -1;
                public int Unk14 { get; set; } = 0; // Hidden
                public sbyte Unk18 { get; set; } = -1;
                public sbyte Unk19 { get; set; } = -1; // Hidden
                public short Unk1A { get; set; } = -1; // Hidden
                public sbyte Unk1C { get; set; } = -1; // Hidden
                public sbyte Unk1D { get; set; } = -1;
                public short Unk1E { get; set; } = 0; // Hidden
                public short Unk20 { get; set; } = -1;
                public short Unk22 { get; set; } = 0; // Hidden
                public int Unk24 { get; set; } = 0; // Hidden
                public int Unk28 { get; set; } = 0; // Hidden
                public int Unk2C { get; set; } = 0; // Hidden
                public int Unk30 { get; set; } = 0; // Hidden
                public int Unk34 { get; set; } = 0; // Hidden
                public int Unk38 { get; set; } = 0; // Hidden
                public int Unk3C { get; set; } = 0; // Hidden
                public int Unk40 { get; set; } = 0; // Hidden
                public int Unk44 { get; set; } = 0; // Hidden
                public int Unk48 { get; set; } = 0; // Hidden
                public int Unk4C { get; set; } = 0; // Hidden
            }

            public class GrassStruct
            {
                public GrassStruct() { }

                public GrassStruct DeepCopy()
                {
                    return (GrassStruct)MemberwiseClone();
                }
                internal GrassStruct(BinaryReaderEx br)
                {
                    GrassParamIds = br.ReadInt32s(6);
                    Unk18 = br.ReadInt32();
                    Unk1C = br.ReadInt32();
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteInt32s(GrassParamIds);
                    bw.WriteInt32(Unk18);
                    bw.WriteInt32(Unk1C);
                }

                // Layout
                public int[] GrassParamIds { get; set; } = new int[6];
                public int Unk18 { get; set; } = -1; // Hidden
                public int Unk1C { get; set; } = 0; // Hidden
            }

            public class Unk88Struct
            {
                public Unk88Struct() { }

                public Unk88Struct DeepCopy()
                {
                    return (Unk88Struct)MemberwiseClone();
                }

                internal Unk88Struct(BinaryReaderEx br)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadInt32();
                    Unk08 = br.ReadInt32();
                    Unk0C = br.ReadInt32();
                    Unk10 = br.ReadInt32();
                    Unk14 = br.ReadInt32();
                    Unk18 = br.ReadInt32();
                    Unk1C = br.ReadInt32();
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                    bw.WriteInt32(Unk10);
                    bw.WriteInt32(Unk14);
                    bw.WriteInt32(Unk18);
                    bw.WriteInt32(Unk1C);
                }

                // Layout
                public int Unk00 { get; set; } = 0; // Hidden
                public int Unk04 { get; set; } = 0; // Hidden
                public int Unk08 { get; set; } = 0; // Hidden
                public int Unk0C { get; set; } = 0; // Hidden
                public int Unk10 { get; set; } = 0; // Hidden
                public int Unk14 { get; set; } = 0; // Hidden
                public int Unk18 { get; set; } = 0; // Hidden
                public int Unk1C { get; set; } = 0; // Hidden
            }

            public class Unk90Struct
            {
                public Unk90Struct() { }

                public Unk90Struct DeepCopy()
                {
                    return (Unk90Struct)MemberwiseClone();
                }
                internal Unk90Struct(BinaryReaderEx br)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadInt32();
                    Unk08 = br.ReadInt32();
                    Unk0C = br.ReadInt32();
                    Unk10 = br.ReadInt32();
                    Unk14 = br.ReadInt32();
                    Unk18 = br.ReadInt32();
                    Unk1C = br.ReadInt32();
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                    bw.WriteInt32(Unk10);
                    bw.WriteInt32(Unk14);
                    bw.WriteInt32(Unk18);
                    bw.WriteInt32(Unk1C);
                }

                // Layout
                public int Unk00 { get; set; } = 0;
                public int Unk04 { get; set; } = 0; // Hidden
                public int Unk08 { get; set; } = 0; // Hidden
                public int Unk0C { get; set; } = 0; // Hidden
                public int Unk10 { get; set; } = 0; // Hidden
                public int Unk14 { get; set; } = 0; // Hidden
                public int Unk18 { get; set; } = 0; // Hidden
                public int Unk1C { get; set; } = 0; // Hidden

            }

            public class TileStruct
            {
                public TileStruct() { }

                public TileStruct DeepCopy()
                {
                    var part = (TileStruct)MemberwiseClone();
                    return part;
                }

                internal TileStruct(BinaryReaderEx br)
                {
                    MapID = br.ReadMapIDBytes(4);
                    Unk04 = br.ReadInt32();
                    Unk08 = br.ReadInt32();
                    Unk0C = br.ReadInt32();
                    Unk10 = br.ReadInt32();
                    Unk14 = br.ReadInt32();
                    Unk18 = br.ReadInt32();
                    Unk1C = br.ReadInt32();
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteMapIDBytes(MapID);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                    bw.WriteInt32(Unk10);
                    bw.WriteInt32(Unk14);
                    bw.WriteInt32(Unk18);
                    bw.WriteInt32(Unk1C);
                }

                // Layout
                public sbyte[] MapID { get; set; } = new sbyte[4];
                public int Unk04 { get; set; } = 0;
                public int Unk08 { get; set; } = 0; // Hidden
                public int Unk0C { get; set; } = -1;
                public int Unk10 { get; set; } = 0; // Hidden
                public int Unk14 { get; set; } = -1;
                public int Unk18 { get; set; } = 0; // Hidden
                public int Unk1C { get; set; } = 0; // Hidden

            }

            public class UnkA0Struct
            {
                public UnkA0Struct() { }

                public UnkA0Struct DeepCopy()
                {
                    return (UnkA0Struct)MemberwiseClone();
                }

                internal UnkA0Struct(BinaryReaderEx br)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadInt32();
                    Unk08 = br.ReadInt32();
                    Unk0C = br.ReadInt32();
                    Unk10 = br.ReadInt32();
                    Unk14 = br.ReadInt32();
                    Unk18 = br.ReadInt32();
                    Unk1C = br.ReadInt32();
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                    bw.WriteInt32(Unk10);
                    bw.WriteInt32(Unk14);
                    bw.WriteInt32(Unk18);
                    bw.WriteInt32(Unk1C);
                }

                // Layout
                public int Unk00 { get; set; } = 0; // Hidden
                public int Unk04 { get; set; } = 0; // Hidden
                public int Unk08 { get; set; } = 0; // Hidden
                public int Unk0C { get; set; } = 0; // Hidden
                public int Unk10 { get; set; } = 0; // Hidden
                public int Unk14 { get; set; } = 0; // Hidden
                public int Unk18 { get; set; } = 0; // Hidden
                public int Unk1C { get; set; } = 0; // Hidden
            }

            public class UnkA8Struct
            {
                public UnkA8Struct() { }

                public UnkA8Struct DeepCopy()
                {
                    return (UnkA8Struct)MemberwiseClone();
                }

                internal UnkA8Struct(BinaryReaderEx br)
                {
                    Unk00 = br.ReadInt16();
                    Unk02 = br.ReadInt16();
                    Unk04 = br.ReadInt16();
                    Unk06 = br.ReadInt16();
                    Unk08 = br.ReadInt32();
                    Unk0C = br.ReadInt32();
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteInt16(Unk00);
                    bw.WriteInt16(Unk02);
                    bw.WriteInt16(Unk04);
                    bw.WriteInt16(Unk06);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                }

                // Layout
                public short Unk00 { get; set; } = -1;
                public short Unk02 { get; set; } = -1;
                public short Unk04 { get; set; } = -1;
                public short Unk06 { get; set; } = 0; // Hidden
                public int Unk08 { get; set; } = 0; // Hidden
                public int Unk0C { get; set; } = 0; // Hidden
            }

            /// <summary>
            /// Fixed visual geometry. Doesn't seem used much in ER?
            /// </summary>
            public class MapPiece : Part
            {
                private protected override PartType Type => PartType.MapPiece;

                // --- Entity
                public EntityStruct EntityData { get; set; }
                private protected override void ReadEntityData(BinaryReaderEx br) => EntityData = new EntityStruct(br);
                private protected override void WriteEntityData(BinaryWriterEx bw) => EntityData.Write(bw);

                // --- Display
                private protected override bool HasDisplayData => true;
                public DisplayStruct DisplayData { get; set; }
                private protected override void ReadDisplayData(BinaryReaderEx br) => DisplayData = new DisplayStruct(br);
                private protected override void WriteDisplayData(BinaryWriterEx bw) => DisplayData.Write(bw);

                // --- Display Group
                private protected override bool HasDisplayGroupData => false;
                //public DisplayGroupStruct DisplayGroupData { get; set; }
                //private protected override void ReadDisplayGroupData(BinaryReaderEx br) => DisplayGroupData = new DisplayGroupStruct(br);
                //private protected override void WriteDisplayGroupData(BinaryWriterEx bw) => DisplayGroupData.Write(bw);

                // --- Gparam
                private protected override bool HasGparamData => true;
                public GparamStruct GparamData { get; set; }
                private protected override void ReadGparamData(BinaryReaderEx br) => GparamData = new GparamStruct(br);
                private protected override void WriteGparamData(BinaryWriterEx bw) => GparamData.Write(bw);

                // --- Scene Gparam
                private protected override bool HasSceneGparamData => false;
                //public SceneGparamStruct SceneGparamData { get; set; }
                //private protected override void ReadSceneGparamData(BinaryReaderEx br) => SceneGparamData = new SceneGparamStruct(br);
                //private protected override void WriteSceneGparamData(BinaryWriterEx bw) => SceneGparamData.Write(bw);

                // --- Grass
                private protected override bool HasGrassData => true;
                public GrassStruct GrassData { get; set; }
                private protected override void ReadGrassData(BinaryReaderEx br) => GrassData = new GrassStruct(br);
                private protected override void WriteGrassData(BinaryWriterEx bw) => GrassData.Write(bw);

                // --- Unk88
                private protected override bool HasUnk88Data => true;
                public Unk88Struct Unk88Data { get; set; }
                private protected override void ReadUnk88Data(BinaryReaderEx br) => Unk88Data = new Unk88Struct(br);
                private protected override void WriteUnk88Data(BinaryWriterEx bw) => Unk88Data.Write(bw);

                // --- Unk90
                private protected override bool HasUnk90Data => true;
                public Unk90Struct Unk90Data { get; set; }
                private protected override void ReadUnk90Data(BinaryReaderEx br) => Unk90Data = new Unk90Struct(br);
                private protected override void WriteUnk90Data(BinaryWriterEx bw) => Unk90Data.Write(bw);

                // --- Tile
                private protected override bool HasTileData => true;
                public TileStruct TileData { get; set; }
                private protected override void ReadTileData(BinaryReaderEx br) => TileData = new TileStruct(br);
                private protected override void WriteTileData(BinaryWriterEx bw) => TileData.Write(bw);

                // --- UnkA0
                private protected override bool HasUnkA0Data => true;
                public UnkA0Struct UnkA0Data { get; set; }
                private protected override void ReadUnkA0Data(BinaryReaderEx br) => UnkA0Data = new UnkA0Struct(br);
                private protected override void WriteUnkA0Data(BinaryWriterEx bw) => UnkA0Data.Write(bw);

                // --- UnkA8
                private protected override bool HasUnkA8Data => false;
                //public UnkA8Struct UnkA8Data { get; set; }
                //private protected override void ReadUnkA8Data(BinaryReaderEx br) => UnkA8Data = new UnkA8Struct(br);
                //private protected override void WriteUnkA8Data(BinaryWriterEx bw) => UnkA8Data.Write(bw);

                public MapPiece() : base("mXXXXXX_XXXX")
                {
                    EntityData = new EntityStruct();
                    DisplayData = new DisplayStruct();
                    GparamData = new GparamStruct();
                    GrassData = new GrassStruct();
                    Unk88Data = new Unk88Struct();
                    Unk90Data = new Unk90Struct();
                    TileData = new TileStruct();
                    UnkA0Data = new UnkA0Struct();
                }

                private protected override void DeepCopyTo(Part part)
                {
                    var piece = (MapPiece)part;
                    piece.EntityData = EntityData.DeepCopy();
                    piece.DisplayData = DisplayData.DeepCopy();
                    piece.GparamData = GparamData.DeepCopy();
                    piece.GrassData = GrassData.DeepCopy();
                    piece.Unk88Data = Unk88Data.DeepCopy();
                    piece.Unk90Data = Unk90Data.DeepCopy();
                    piece.TileData = TileData.DeepCopy();
                    piece.UnkA0Data = UnkA0Data.DeepCopy();
                }

                internal MapPiece(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadInt32();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                }

                // Layout
                public int Unk00 { get; set; } = 0; // Hidden
                public int Unk04 { get; set; } = 0; // Hidden

            }

            /// <summary>
            /// Common base data for enemies and dummy enemies.
            /// </summary>
            public abstract class EnemyBase : Part
            {
                // --- Entity
                public EntityStruct EntityData { get; set; }
                private protected override void ReadEntityData(BinaryReaderEx br) => EntityData = new EntityStruct(br);
                private protected override void WriteEntityData(BinaryWriterEx bw) => EntityData.Write(bw);

                // --- Display
                private protected override bool HasDisplayData => true;
                public DisplayStruct DisplayData { get; set; }
                private protected override void ReadDisplayData(BinaryReaderEx br) => DisplayData = new DisplayStruct(br);
                private protected override void WriteDisplayData(BinaryWriterEx bw) => DisplayData.Write(bw);

                // --- Display Group
                private protected override bool HasDisplayGroupData => false;
                //public DisplayGroupStruct DisplayGroupData { get; set; }
                //private protected override void ReadDisplayGroupData(BinaryReaderEx br) => DisplayGroupData = new DisplayGroupStruct(br);
                //private protected override void WriteDisplayGroupData(BinaryWriterEx bw) => DisplayGroupData.Write(bw);

                // --- Gparam
                private protected override bool HasGparamData => true;
                public GparamStruct GparamData { get; set; }
                private protected override void ReadGparamData(BinaryReaderEx br) => GparamData = new GparamStruct(br);
                private protected override void WriteGparamData(BinaryWriterEx bw) => GparamData.Write(bw);

                // --- Scene Gparam
                private protected override bool HasSceneGparamData => false;
                //public SceneGparamStruct SceneGparamData { get; set; }
                //private protected override void ReadSceneGparamData(BinaryReaderEx br) => SceneGparamData = new SceneGparamStruct(br);
                //private protected override void WriteSceneGparamData(BinaryWriterEx bw) => SceneGparamData.Write(bw);

                // --- Grass
                private protected override bool HasGrassData => false;
                //public GrassStruct GrassData { get; set; }
                //private protected override void ReadGrassData(BinaryReaderEx br) => GrassData = new GrassStruct(br);
                //private protected override void WriteGrassData(BinaryWriterEx bw) => GrassData.Write(bw);

                // --- Unk88
                private protected override bool HasUnk88Data => true;
                public Unk88Struct Unk88Data { get; set; }
                private protected override void ReadUnk88Data(BinaryReaderEx br) => Unk88Data = new Unk88Struct(br);
                private protected override void WriteUnk88Data(BinaryWriterEx bw) => Unk88Data.Write(bw);

                // --- Unk90
                private protected override bool HasUnk90Data => false;
                //public Unk90Struct Unk90Data { get; set; }
                //private protected override void ReadUnk90Data(BinaryReaderEx br) => Unk90Data = new Unk90Struct(br);
                //private protected override void WriteUnk90Data(BinaryWriterEx bw) => Unk90Data.Write(bw);

                // --- Tile
                private protected override bool HasTileData => true;
                public TileStruct TileData { get; set; }
                private protected override void ReadTileData(BinaryReaderEx br) => TileData = new TileStruct(br);
                private protected override void WriteTileData(BinaryWriterEx bw) => TileData.Write(bw);

                // --- UnkA0
                private protected override bool HasUnkA0Data => false;
                //public UnkA0Struct UnkA0Data { get; set; }
                //private protected override void ReadUnkA0Data(BinaryReaderEx br) => UnkA0Data = new UnkA0Struct(br);
                //private protected override void WriteUnkA0Data(BinaryWriterEx bw) => UnkA0Data.Write(bw);

                // --- UnkA8
                private protected override bool HasUnkA8Data => false;
                //public UnkA8Struct UnkA8Data { get; set; }
                //private protected override void ReadUnkA8Data(BinaryReaderEx br) => UnkA8Data = new UnkA8Struct(br);
                //private protected override void WriteUnkA8Data(BinaryWriterEx bw) => UnkA8Data.Write(bw);

                private protected EnemyBase() : base("cXXXX_XXXX")
                {
                    EntityData = new EntityStruct();
                    DisplayData = new DisplayStruct();
                    GparamData = new GparamStruct();
                    Unk88Data = new Unk88Struct();
                    TileData = new TileStruct();
                }
                private protected EnemyBase(BinaryReaderEx br) : base(br) { }
                private protected override void DeepCopyTo(Part part)
                {
                    var enemy = (EnemyBase)part;
                    enemy.EntityData = EntityData.DeepCopy();
                    enemy.DisplayData = DisplayData.DeepCopy();
                    enemy.GparamData = GparamData.DeepCopy();
                    enemy.Unk88Data = Unk88Data.DeepCopy();
                    enemy.TileData = TileData.DeepCopy();
                }

                internal override void GetNames(MSB_NR msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    HitPartName = MSB.FindName(entries.Parts, HitPartIndex);
                    PatrolRouteName = MSB.FindName(msb.Events.PatrolInfo, PatrolRouteIndex);
                }

                internal override void GetIndices(MSB_NR msb, Entries entries)
                {
                    base.GetIndices(msb, entries);
                    HitPartIndex = MSB.FindIndex(this, entries.Parts, HitPartName);
                    PatrolRouteIndex = (short)MSB.FindIndex(this, msb.Events.PatrolInfo, PatrolRouteName);
                }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    long start = br.Position;

                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadInt32();
                    NpcThinkParamId = br.ReadInt32();
                    NpcParamId = br.ReadInt32();
                    TalkID = br.ReadInt32();
                    Unk14 = br.ReadByte();
                    Unk15 = br.ReadByte();
                    PlatoonId = br.ReadInt16();
                    CharaInitParamId = br.ReadInt32();
                    HitPartIndex = br.ReadInt32();
                    PatrolRouteIndex = br.ReadInt16();
                    Unk22 = br.ReadInt16();
                    Unk24 = br.ReadInt32();
                    Unk28 = br.ReadInt32();
                    ChrActivateCondParamID = br.ReadInt32();
                    UnkSpEffectSetParamID = br.ReadInt32();
                    CondemnedSpEffectSetParamID = br.ReadInt32();
                    BackupEventAnimID = br.ReadInt32();
                    Unk3C = br.ReadSByte();
                    Unk3D = br.ReadByte();
                    Unk3E = br.ReadInt16();
                    SpEffectSetParamIds = br.ReadInt32s(4);
                    Unk50 = br.ReadInt32();
                    Unk54 = br.ReadInt32();
                    Unk58 = br.ReadInt32();
                    Unk5C = br.ReadInt32();
                    Unk60 = br.ReadInt32();
                    Unk64 = br.ReadInt32();
                    Unk68 = br.ReadInt32();
                    Unk6C = br.ReadInt32();

                    Offset70 = br.ReadInt64();
                    Offset78 = br.ReadInt64();

                    if (Offset70 != 0)
                        throw new InvalidDataException($"Unexpected {nameof(Offset70)} 0x{Offset70:X} in type {GetType()}.");

                    if (Offset78 == 0)
                        throw new InvalidDataException($"Unexpected {nameof(Offset78)} 0x{Offset78:X} in type {GetType()}.");

                    br.Position = start + Offset78;
                    Unk78Data = new EnemyUnk78Struct(br);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    var start = bw.Position;

                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(NpcThinkParamId);
                    bw.WriteInt32(NpcParamId);
                    bw.WriteInt32(TalkID);
                    bw.WriteByte(Unk14);
                    bw.WriteByte(Unk15);
                    bw.WriteInt16(PlatoonId);
                    bw.WriteInt32(CharaInitParamId);
                    bw.WriteInt32(HitPartIndex);
                    bw.WriteInt16(PatrolRouteIndex);
                    bw.WriteInt16(Unk22);
                    bw.WriteInt32(Unk24);
                    bw.WriteInt32(Unk28);
                    bw.WriteInt32(ChrActivateCondParamID);
                    bw.WriteInt32(UnkSpEffectSetParamID);
                    bw.WriteInt32(CondemnedSpEffectSetParamID);
                    bw.WriteInt32(BackupEventAnimID);
                    bw.WriteSByte(Unk3C);
                    bw.WriteByte(Unk3D);
                    bw.WriteInt16(Unk3E);
                    bw.WriteInt32s(SpEffectSetParamIds);

                    bw.WriteInt32(Unk50);
                    bw.WriteInt32(Unk54);
                    bw.WriteInt32(Unk58);
                    bw.WriteInt32(Unk5C);
                    bw.WriteInt32(Unk60);
                    bw.WriteInt32(Unk64);
                    bw.WriteInt32(Unk68);
                    bw.WriteInt32(Unk6C);

                    bw.ReserveInt64("Offset70");
                    bw.ReserveInt64("Offset78");

                    bw.FillInt64("Offset78", bw.Position - start);
                    Unk78Data.Write(bw);
                }

                // Internal
                public EnemyUnk78Struct Unk78Data { get; set; }

                // Layout
                public int Unk00 { get; set; } = -1; // Hidden
                public int Unk04 { get; set; } = -1; // Hidden
                public int NpcThinkParamId { get; set; } = 0;
                public int NpcParamId { get; set; } = 0;
                public int TalkID { get; set; } = 0;
                public byte Unk14 { get; set; } = 0; // Hidden
                public byte Unk15 { get; set; } = 0; // Boolean
                public short PlatoonId { get; set; } = 0;
                public int CharaInitParamId { get; set; } = -1;
                private int HitPartIndex { get; set; } = -1;
                private short PatrolRouteIndex { get; set; }
                public short Unk22 { get; set; } = -1;
                public int Unk24 { get; set; } = -1; // Hidden
                public int Unk28 { get; set; } = 0;
                public int ChrActivateCondParamID { get; set; } = 0;
                public int UnkSpEffectSetParamID { get; set; } = 0;
                public int CondemnedSpEffectSetParamID { get; set; } = 0;
                public int BackupEventAnimID { get; set; } = -1;
                public sbyte Unk3C { get; set; } = -1;
                public byte Unk3D { get; set; } = 0; // Hidden
                public short Unk3E { get; set; } = -1;
                public int[] SpEffectSetParamIds { get; set; } = new int[4];
                public int Unk50 { get; set; } = 0; // Hidden
                public int Unk54 { get; set; } = 0; // Hidden
                public int Unk58 { get; set; } = 0; // Hidden
                public int Unk5C { get; set; } = 0; // Hidden
                public int Unk60 { get; set; } = 0; // Hidden
                public int Unk64 { get; set; } = 0; // Hidden
                public int Unk68 { get; set; } = 0; // Hidden
                public int Unk6C { get; set; } = 0; // Hidden
                private long Offset70 { get; set; } = 0; // Hidden
                private long Offset78 { get; set; } = 0; // Hidden

                [MSBReference(ReferenceType = typeof(Collision))]
                public string HitPartName { get; set; }

                [MSBReference(ReferenceType = typeof(Event.PatrolInfo))]
                public string PatrolRouteName { get; set; }

                public class EnemyUnk78Struct
                {
                    public EnemyUnk78Struct() { }

                    public EnemyUnk78Struct DeepCopy()
                    {
                        return (EnemyUnk78Struct)MemberwiseClone();
                    }

                    internal EnemyUnk78Struct(BinaryReaderEx br)
                    {
                        Unk00 = br.ReadInt32();
                        Unk04 = br.ReadSingle();

                        InnerStruct1 = new EnemyInnerStruct(br);
                        InnerStruct2 = new EnemyInnerStruct(br);
                        InnerStruct3 = new EnemyInnerStruct(br);
                        InnerStruct4 = new EnemyInnerStruct(br);
                        InnerStruct5 = new EnemyInnerStruct(br);

                        Unk30 = br.ReadInt32();
                        Unk34 = br.ReadInt32();
                        Unk38 = br.ReadInt32();
                        Unk3C = br.ReadInt32();
                    }

                    internal void Write(BinaryWriterEx bw)
                    {
                        bw.WriteInt32(Unk00);
                        bw.WriteSingle(Unk04);

                        InnerStruct1.Write(bw);
                        InnerStruct2.Write(bw);
                        InnerStruct3.Write(bw);
                        InnerStruct4.Write(bw);
                        InnerStruct5.Write(bw);

                        bw.WriteInt32(Unk30);
                        bw.WriteInt32(Unk34);
                        bw.WriteInt32(Unk38);
                        bw.WriteInt32(Unk3C);
                    }

                    // Internal
                    public EnemyInnerStruct InnerStruct1 { get; set; }
                    public EnemyInnerStruct InnerStruct2 { get; set; }
                    public EnemyInnerStruct InnerStruct3 { get; set; }
                    public EnemyInnerStruct InnerStruct4 { get; set; }
                    public EnemyInnerStruct InnerStruct5 { get; set; }

                    // Layout
                    public int Unk00 { get; set; } = 0;
                    public float Unk04 { get; set; } = 1;
                    public int Unk30 { get; set; } = 0; // Hidden
                    public int Unk34 { get; set; } = 0; // Hidden
                    public int Unk38 { get; set; } = 0; // Hidden
                    public int Unk3C { get; set; } = 0; // Hidden

                    public class EnemyInnerStruct
                    {
                        public EnemyInnerStruct() { }

                        public EnemyInnerStruct DeepCopy()
                        {
                            return (EnemyInnerStruct)MemberwiseClone();
                        }

                        internal EnemyInnerStruct(BinaryReaderEx br)
                        {
                            Unk00 = br.ReadInt32();
                            Unk04 = br.ReadInt16();
                            Unk06 = br.ReadInt16();
                        }

                        internal void Write(BinaryWriterEx bw)
                        {
                            bw.WriteInt32(Unk00);
                            bw.WriteInt16(Unk04);
                            bw.WriteInt16(Unk06);
                        }

                        // Layout
                        public int Unk00 { get; set; } = -1;
                        public short Unk04 { get; set; } = -1;
                        public short Unk06 { get; set; } = 10;
                    }
                }
            }

            public class Enemy : EnemyBase
            {
                private protected override PartType Type => PartType.Enemy;
                public Enemy() : base() { }
                internal Enemy(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// A spawn point for the player, or something.
            /// </summary>
            public class Player : Part
            {
                private protected override PartType Type => PartType.Player;

                // --- Entity
                public EntityStruct EntityData { get; set; }
                private protected override void ReadEntityData(BinaryReaderEx br) => EntityData = new EntityStruct(br);
                private protected override void WriteEntityData(BinaryWriterEx bw) => EntityData.Write(bw);

                // --- Display
                private protected override bool HasDisplayData => true;
                public DisplayStruct DisplayData { get; set; }
                private protected override void ReadDisplayData(BinaryReaderEx br) => DisplayData = new DisplayStruct(br);
                private protected override void WriteDisplayData(BinaryWriterEx bw) => DisplayData.Write(bw);

                // --- Display Group
                private protected override bool HasDisplayGroupData => false;
                //public DisplayGroupStruct DisplayGroupData { get; set; }
                //private protected override void ReadDisplayGroupData(BinaryReaderEx br) => DisplayGroupData = new DisplayGroupStruct(br);
                //private protected override void WriteDisplayGroupData(BinaryWriterEx bw) => DisplayGroupData.Write(bw);

                // --- Gparam
                private protected override bool HasGparamData => false;
                //public GparamStruct GparamData { get; set; }
                //private protected override void ReadGparamData(BinaryReaderEx br) => GparamData = new GparamStruct(br);
                //private protected override void WriteGparamData(BinaryWriterEx bw) => GparamData.Write(bw);

                // --- Scene Gparam
                private protected override bool HasSceneGparamData => false;
                //public SceneGparamStruct SceneGparamData { get; set; }
                //private protected override void ReadSceneGparamData(BinaryReaderEx br) => SceneGparamData = new SceneGparamStruct(br);
                //private protected override void WriteSceneGparamData(BinaryWriterEx bw) => SceneGparamData.Write(bw);

                // --- Grass
                private protected override bool HasGrassData => false;
                //public GrassStruct GrassData { get; set; }
                //private protected override void ReadGrassData(BinaryReaderEx br) => GrassData = new GrassStruct(br);
                //private protected override void WriteGrassData(BinaryWriterEx bw) => GrassData.Write(bw);

                // --- Unk88
                private protected override bool HasUnk88Data => true;
                public Unk88Struct Unk88Data { get; set; }
                private protected override void ReadUnk88Data(BinaryReaderEx br) => Unk88Data = new Unk88Struct(br);
                private protected override void WriteUnk88Data(BinaryWriterEx bw) => Unk88Data.Write(bw);

                // --- Unk90
                private protected override bool HasUnk90Data => false;
                //public Unk90Struct Unk90Data { get; set; }
                //private protected override void ReadUnk90Data(BinaryReaderEx br) => Unk90Data = new Unk90Struct(br);
                //private protected override void WriteUnk90Data(BinaryWriterEx bw) => Unk90Data.Write(bw);

                // --- Tile
                private protected override bool HasTileData => true;
                public TileStruct TileData { get; set; }
                private protected override void ReadTileData(BinaryReaderEx br) => TileData = new TileStruct(br);
                private protected override void WriteTileData(BinaryWriterEx bw) => TileData.Write(bw);

                // --- UnkA0
                private protected override bool HasUnkA0Data => false;
                //public UnkA0Struct UnkA0Data { get; set; }
                //private protected override void ReadUnkA0Data(BinaryReaderEx br) => UnkA0Data = new UnkA0Struct(br);
                //private protected override void WriteUnkA0Data(BinaryWriterEx bw) => UnkA0Data.Write(bw);

                // --- UnkA8
                private protected override bool HasUnkA8Data => false;
                //public UnkA8Struct UnkA8Data { get; set; }
                //private protected override void ReadUnkA8Data(BinaryReaderEx br) => UnkA8Data = new UnkA8Struct(br);
                //private protected override void WriteUnkA8Data(BinaryWriterEx bw) => UnkA8Data.Write(bw);

                public Player() : base("c0000_XXXX")
                {
                    EntityData = new EntityStruct();
                    DisplayData = new DisplayStruct();
                    Unk88Data = new Unk88Struct();
                    TileData = new TileStruct();
                }
                internal Player(BinaryReaderEx br) : base(br) { }

                private protected override void DeepCopyTo(Part part)
                {
                    var player = (Player)part;
                    player.EntityData = EntityData.DeepCopy();
                    player.DisplayData = DisplayData.DeepCopy();
                    player.Unk88Data = Unk88Data.DeepCopy();
                    player.TileData = TileData.DeepCopy();
                }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadInt32();
                    Unk08 = br.ReadInt32();
                    Unk0C = br.ReadInt32();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                }

                // Layout
                public int Unk00 { get; set; } = 0;
                public int Unk04 { get; set; } = 0; // Hidden
                public int Unk08 { get; set; } = 0; // Hidden
                public int Unk0C { get; set; } = 0; // Hidden
            }

            /// <summary>
            /// Invisible but physical geometry.
            /// </summary>
            public class Collision : Part
            {
                private protected override PartType Type => PartType.Collision;
                private protected override bool HasDisplayData => true;
                private protected override bool HasDisplayGroupData => true;
                private protected override bool HasGparamData => true;
                private protected override bool HasSceneGparamData => true;
                private protected override bool HasGrassData => false;
                private protected override bool HasUnk88Data => true;
                private protected override bool HasUnk9 => false;
                private protected override bool HasTileLoadConfig => true;
                private protected override bool HasUnk11 => true;

                /// <summary>
                /// Unknown.
                /// </summary>
                public DisplayStruct DisplayDataStruct { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public DisplayGroupStruct DisplayGroupStruct { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public GparamStruct GparamConfigStruct { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public SceneGparamStruct SceneGparamConfigStruct { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public Unk88Struct UnkStruct8 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public TileStruct TileLoadConfig { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkA0Struct UnkStruct11 { get; set; }

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
                /// Unknown.
                /// </summary>
                public byte UnkT35 { get; set; }

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
                /// Unknown.
                /// </summary>
                public short UnkT4C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short UnkT4E { get; set; }

                /// <summary>
                /// Creates a Collision with default values.
                /// </summary>
                public Collision() : base("hXXXXXX")
                {
                    DisplayDataStruct = new DisplayStruct();
                    DisplayGroupStruct = new DisplayGroupStruct();
                    GparamConfigStruct = new GparamStruct();
                    SceneGparamConfigStruct = new SceneGparamStruct();
                    UnkStruct8 = new Unk88Struct();
                    TileLoadConfig = new TileStruct();
                    UnkStruct11 = new UnkA0Struct();
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
                    UnkT26 = br.ReadInt16();
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

                private protected override void ReadDisplayData(BinaryReaderEx br) => DisplayDataStruct = new DisplayStruct(br);
                private protected override void ReadDisplayGroupData(BinaryReaderEx br) => DisplayGroupStruct = new DisplayGroupStruct(br);
                private protected override void ReadGparamData(BinaryReaderEx br) => GparamConfigStruct = new GparamStruct(br);
                private protected override void ReadSceneGparamData(BinaryReaderEx br) => SceneGparamConfigStruct = new SceneGparamStruct(br);
                private protected override void ReadUnk8(BinaryReaderEx br) => UnkStruct8 = new Unk88Struct(br);
                private protected override void ReadTileLoad(BinaryReaderEx br) => TileLoadConfig = new TileStruct(br);
                private protected override void ReadUnkA8Data(BinaryReaderEx br) => UnkStruct11 = new UnkA0Struct(br);

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
                private protected override bool HasDisplayData => true;
                private protected override bool HasDisplayGroupData => false;
                private protected override bool HasGparamData => true;
                private protected override bool HasSceneGparamData => false;
                private protected override bool HasGrassData => false;
                private protected override bool HasUnk88Data => true;
                private protected override bool HasUnk9 => false;
                private protected override bool HasTileLoadConfig => true;
                private protected override bool HasUnk11 => false;

                /// <summary>
                /// Unknown.
                /// </summary>
                public DisplayStruct DisplayDataStruct { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public GparamStruct GparamConfigStruct { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public Unk88Struct UnkStruct8 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public TileStruct TileLoadConfig { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT18 { get; set; }

                /// <summary>
                /// Creates a MapPiece with default values.
                /// </summary>
                public DummyAsset() : base("AEGxxx_xxx_xxxx")
                {
                    DisplayDataStruct = new DisplayStruct();
                    GparamConfigStruct = new GparamStruct();
                    UnkStruct8 = new Unk88Struct();
                    TileLoadConfig = new TileStruct();
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

                private protected override void ReadDisplayData(BinaryReaderEx br) => DisplayDataStruct = new DisplayStruct(br);
                private protected override void ReadGparamData(BinaryReaderEx br) => GparamConfigStruct = new GparamStruct(br);
                private protected override void ReadUnk8(BinaryReaderEx br) => UnkStruct8 = new Unk88Struct(br);
                private protected override void ReadTileLoad(BinaryReaderEx br) => TileLoadConfig = new TileStruct(br);

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
                private protected override bool HasDisplayData => true;
                private protected override bool HasDisplayGroupData => true;
                private protected override bool HasGparamData => false;
                private protected override bool HasSceneGparamData => false;
                private protected override bool HasGrassData => false;
                private protected override bool HasUnk88Data => true;
                private protected override bool HasUnk9 => false;
                private protected override bool HasTileLoadConfig => true;
                private protected override bool HasUnk11 => true;

                /// <summary>
                /// Unknown.
                /// </summary>
                public DisplayStruct DisplayDataStruct { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public DisplayGroupStruct DisplayGroupStruct { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public Unk88Struct UnkStruct8 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public TileStruct TileLoadConfig { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkA0Struct UnkStruct11 { get; set; }

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
                    DisplayDataStruct = new DisplayStruct();
                    DisplayGroupStruct = new DisplayGroupStruct();
                    MapID = new byte[4];
                    UnkStruct8 = new Unk88Struct();
                    TileLoadConfig = new TileStruct();
                    UnkStruct11 = new UnkA0Struct();
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

                private protected override void ReadDisplayData(BinaryReaderEx br) => DisplayDataStruct = new DisplayStruct(br);
                private protected override void ReadDisplayGroupData(BinaryReaderEx br) => DisplayGroupStruct = new DisplayGroupStruct(br);
                private protected override void ReadUnk8(BinaryReaderEx br) => UnkStruct8 = new Unk88Struct(br);
                private protected override void ReadTileLoad(BinaryReaderEx br) => TileLoadConfig = new TileStruct(br);
                private protected override void ReadUnkA8Data(BinaryReaderEx br) => UnkStruct11 = new UnkA0Struct(br);

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

                internal override void GetNames(MSB_NR msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    CollisionName = MSB.FindName(msb.Parts.Collisions, CollisionIndex);
                }

                internal override void GetIndices(MSB_NR msb, Entries entries)
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
                private protected override bool HasDisplayData => true;
                private protected override bool HasDisplayGroupData => true;
                private protected override bool HasGparamData => true;
                private protected override bool HasSceneGparamData => false;
                private protected override bool HasGrassData => true;
                private protected override bool HasUnk88Data => true;
                private protected override bool HasUnk9 => true;
                private protected override bool HasTileLoadConfig => true;
                private protected override bool HasUnk11 => true;

                /// <summary>
                /// Unknown.
                /// </summary>
                public DisplayStruct DisplayDataStruct { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public DisplayGroupStruct DisplayGroupStruct { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public GparamStruct GparamConfigStruct { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public GrassStruct GrassConfigStruct { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public Unk88Struct UnkStruct8 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public Unk90Struct UnkStruct9 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public TileStruct TileLoadConfig { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkA0Struct UnkStruct11 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short UnkT00 { get; set; }
                /// <summary>
                /// Unknown.
                /// </summary>
                public short UnkT02 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT10 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public bool UnkT11 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT12 { get; set; }

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
                public int UnkT24 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT28 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT30 { get; set; }

                /// <summary>
                /// The <c>ItemLotParam_map</c> row ID that this asset spawns upon interaction.
                /// </summary>
                public int ItemLotParamMapID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Part))]
                public string[] PartNames { get; set; }

                private int[] PartIndices { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public bool UnkT50 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT51 { get; set; }

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
                    /// Unknown.
                    /// </summary>
                    public short AssetStruct1_Unk00 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public bool AssetStruct1_Unk04 { get; set; }

                    /// <summary>
                    /// Disable being able to summon/ride Torrent, but only when asset isn't referencing collision DisableTorrent.
                    /// </summary>
                    public bool DisableTorrentAssetOnly { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public int AssetStruct1_Unk1C { get; set; }

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
                        AssetStruct1_Unk00 = br.ReadInt16();
                        br.AssertInt16(-1);
                        AssetStruct1_Unk04 = br.ReadBoolean();
                        DisableTorrentAssetOnly = br.ReadBoolean();
                        br.AssertInt16(-1);
                        br.AssertInt32(0);
                        br.AssertInt32(0);
                        br.AssertInt32(-1);
                        br.AssertInt32(-1);
                        br.AssertInt32(-1);
                        AssetStruct1_Unk1C = br.ReadInt32();
                        br.AssertInt32(0);
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
                        bw.WriteInt16(AssetStruct1_Unk00);
                        bw.WriteInt16(-1);
                        bw.WriteBoolean(AssetStruct1_Unk04);
                        bw.WriteBoolean(DisableTorrentAssetOnly);
                        bw.WriteInt16(-1);
                        bw.WriteInt32(0);
                        bw.WriteInt32(0);
                        bw.WriteInt32(-1);
                        bw.WriteInt32(-1);
                        bw.WriteInt32(-1);
                        bw.WriteInt32(AssetStruct1_Unk1C);
                        bw.WriteInt32(0);
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
                    /// Unknown.
                    /// </summary>
                    public int AssetUnkStruct2_Unk00 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public int AssetUnkStruct2_Unk04 { get; set; }

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
                        AssetUnkStruct2_Unk00 = br.ReadInt32();
                        AssetUnkStruct2_Unk04 = br.ReadInt32();
                        br.AssertInt32(-1);
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
                        bw.WriteInt32(AssetUnkStruct2_Unk00);
                        bw.WriteInt32(AssetUnkStruct2_Unk04);
                        bw.WriteInt32(-1);
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
                    /// Unknown.
                    /// </summary>
                    public int AssetUnkStruct3_Unk00 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public float AssetUnkStruct3_Unk04 { get; set; }

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
                    /// Unknown.
                    /// </summary>
                    public short AssetUnkStruct3_Unk0E { get; set; }

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
                        AssetUnkStruct3_Unk00 = br.ReadInt32();
                        AssetUnkStruct3_Unk04 = br.ReadSingle();
                        br.AssertSByte(-1);
                        AssetUnkStruct3_Unk09 = br.ReadByte();
                        AssetUnkStruct3_Unk0A = br.ReadByte();
                        AssetUnkStruct3_Unk0B = br.ReadByte();
                        AssetUnkStruct3_Unk0C = br.ReadInt16();
                        AssetUnkStruct3_Unk0E = br.ReadInt16();
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
                        bw.WriteInt32(AssetUnkStruct3_Unk00);
                        bw.WriteSingle(AssetUnkStruct3_Unk04);
                        bw.WriteSByte(-1);
                        bw.WriteByte(AssetUnkStruct3_Unk09);
                        bw.WriteByte(AssetUnkStruct3_Unk0A);
                        bw.WriteByte(AssetUnkStruct3_Unk0B);
                        bw.WriteInt16(AssetUnkStruct3_Unk0C);
                        bw.WriteInt16(AssetUnkStruct3_Unk0E);
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
                    DisplayDataStruct = new DisplayStruct();
                    DisplayGroupStruct = new DisplayGroupStruct();
                    GparamConfigStruct = new GparamStruct();
                    GrassConfigStruct = new GrassStruct();
                    UnkStruct8 = new Unk88Struct();
                    UnkStruct9 = new Unk90Struct();
                    TileLoadConfig = new TileStruct();
                    UnkStruct11 = new UnkA0Struct();

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

                private int UnkT04 { get; set; }
                private int UnkT14 { get; set; }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    UnkT00 = br.ReadInt16();
                    UnkT02 = br.AssertInt16([0, 1]);
                    UnkT04 = br.ReadInt32();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    UnkT10 = br.ReadByte();
                    UnkT11 = br.ReadBoolean();
                    UnkT12 = br.ReadByte();
                    br.AssertByte(0);
                    UnkT14 = br.ReadInt32();
                    br.AssertInt32(0);
                    AssetSfxParamRelativeID = br.ReadInt16();
                    UnkT1E = br.ReadInt16();
                    br.AssertInt32(-1);
                    UnkT24 = br.ReadInt32();
                    UnkT28 = br.ReadInt32();
                    br.AssertInt32(0);
                    UnkT30 = br.ReadInt32();
                    ItemLotParamMapID = br.ReadInt32();
                    PartIndices = br.ReadInt32s(6);
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

                private protected override void ReadDisplayData(BinaryReaderEx br) => DisplayDataStruct = new DisplayStruct(br);
                private protected override void ReadDisplayGroupData(BinaryReaderEx br) => DisplayGroupStruct = new DisplayGroupStruct(br);
                private protected override void ReadGparamData(BinaryReaderEx br) => GparamConfigStruct = new GparamStruct(br);
                private protected override void ReadGrassData(BinaryReaderEx br) => GrassConfigStruct = new GrassStruct(br);
                private protected override void ReadUnk8(BinaryReaderEx br) => UnkStruct8 = new Unk88Struct(br);
                private protected override void ReadUnk9(BinaryReaderEx br) => UnkStruct9 = new Unk90Struct(br);
                private protected override void ReadTileLoad(BinaryReaderEx br) => TileLoadConfig = new TileStruct(br);
                private protected override void ReadUnkA8Data(BinaryReaderEx br) => UnkStruct11 = new UnkA0Struct(br);

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt16(UnkT00);
                    bw.WriteInt16(UnkT02);
                    bw.WriteInt32(UnkT04);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteByte(UnkT10);
                    bw.WriteBoolean(UnkT11);
                    bw.WriteByte(UnkT12);
                    bw.WriteByte(0);
                    bw.WriteInt32(UnkT14);
                    bw.WriteInt32(0);
                    bw.WriteInt16(AssetSfxParamRelativeID);
                    bw.WriteInt16(UnkT1E);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(UnkT24);
                    bw.WriteInt32(UnkT28);
                    bw.WriteInt32(0);
                    bw.WriteInt32(UnkT30);
                    bw.WriteInt32(ItemLotParamMapID);
                    bw.WriteInt32s(PartIndices);
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

                private protected override void WriteUnk1(BinaryWriterEx bw) => DisplayDataStruct.Write(bw);
                private protected override void WriteUnk2(BinaryWriterEx bw) => DisplayGroupStruct.Write(bw);
                private protected override void WriteGparamConfig(BinaryWriterEx bw) => GparamConfigStruct.Write(bw);
                private protected override void WriteGrassConfig(BinaryWriterEx bw) => GrassConfigStruct.Write(bw);
                private protected override void WriteUnk8(BinaryWriterEx bw) => UnkStruct8.Write(bw);
                private protected override void WriteUnk9(BinaryWriterEx bw) => UnkStruct9.Write(bw);
                private protected override void WriteTileLoad(BinaryWriterEx bw) => TileLoadConfig.Write(bw);
                private protected override void WriteUnk11(BinaryWriterEx bw) => UnkStruct11.Write(bw);

                internal override void GetNames(MSB_NR msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    PartNames = MSB.FindNames(entries.Parts, PartIndices);
                    UnkT54PartName = MSB.FindName(entries.Parts, UnkT54PartIndex);
                }

                internal override void GetIndices(MSB_NR msb, Entries entries)
                {
                    base.GetIndices(msb, entries);
                    PartIndices = MSB.FindIndices(this, entries.Parts, PartNames);
                    UnkT54PartIndex = MSB.FindIndex(entries.Parts, UnkT54PartName);
                }
            }
        }
    }
}
