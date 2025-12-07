using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using static SoulsFormats.EMEVD;
using static SoulsFormats.MSB_NR.Part.EnemyBase;
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
            public PartsParam() : base(78, "PARTS_PARAM_ST")
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

            internal override Part ReadEntry(BinaryReaderEx br, int version)
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

                if (!BinaryReaderEx.IgnoreAsserts)
                {
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

                    if (HasUnk90Data ^ Unk90_DataOffset != 0)
                        throw new InvalidDataException($"Unexpected {nameof(Unk90_DataOffset)} 0x{Unk90_DataOffset:X} in type {GetType()}.");

                    if (HasTileData ^ Tile_DataOffset != 0)
                        throw new InvalidDataException($"Unexpected {nameof(Tile_DataOffset)} 0x{Tile_DataOffset:X} in type {GetType()}.");

                    if (HasUnkA0Data ^ UnkA0_DataOffset != 0)
                        throw new InvalidDataException($"Unexpected {nameof(UnkA0_DataOffset)} 0x{UnkA0_DataOffset:X} in type {GetType()}.");

                    if (HasUnkA8Data ^ UnkA8_DataOffset != 0)
                        throw new InvalidDataException($"Unexpected {nameof(UnkA8_DataOffset)} 0x{UnkA8_DataOffset:X} in type {GetType()}.");
                }

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
            private int Unk4C { get; set; } = 0; // Hidden

            // Offsets
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

            // Names
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
                bw.WriteInt32(id);
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
                bw.ReserveInt64("SceneGparamOffset");
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
                    Condition1 = br.ReadByte();
                    Condition2 = br.ReadByte();
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
                    bw.WriteByte(Condition1);
                    bw.WriteByte(Condition2);
                    bw.WriteInt16(UnkC2);
                    bw.WriteInt16(UnkC4);
                    bw.WriteInt16(UnkC6);

                    bw.WritePattern(0xC0, 0x00); // 48 * 4
                }

                // Layout
                public uint[] DisplayGroups { get; set; } = new uint[8];
                public uint[] DrawGroups { get; set; } = new uint[8];
                public uint[] CollisionMask { get; set; } = new uint[32];
                public byte Condition1 { get; set; } = 0; // Bool
                public byte Condition2 { get; set; } = 0; // Bool
                private short UnkC2 { get; set; } = 0; // Hidden
                private short UnkC4 { get; set; } = -1; // Hidden
                private short UnkC6 { get; set; } = 0; // Hidden

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
                private short Unk24 { get; set; } = 0; // Hidden
                private short Unk26 { get; set; } = -1; // Hidden
                private int Unk28 { get; set; } = 0; // Hidden
                private int Unk2C { get; set; } = 0; // Hidden
                private int Unk30 { get; set; } = 0; // Hidden
                private int Unk34 { get; set; } = 0; // Hidden
                private int Unk38 { get; set; } = 0; // Hidden
                private int Unk3C { get; set; } = 0; // Hidden
                private int Unk40 { get; set; } = 0; // Hidden
                private int Unk44 { get; set; } = 0; // Hidden

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
                    RandomAppearParamID = br.ReadInt32();
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
                    bw.WriteInt32(RandomAppearParamID);
                    bw.WriteInt32(Variation);

                    bw.WriteInt32(Unk48);
                    bw.WriteInt32(Unk4C);
                }

                // Layout
                public uint EntityID { get; set; } = 0;
                public byte Unk04 { get; set; } = 0; // Bool
                public byte Unk05 { get; set; } = 0; // Bool
                private short Unk06 { get; set; } = 0; // Hidden
                public int Unk08 { get; set; } = 0; // Bool
                public byte Unk0C { get; set; } = 0; // Bool
                public byte Unk0D { get; set; } = 0; // Bool
                public byte Unk0E { get; set; } = 0; // Bool
                public byte Unk0F { get; set; } = 1; // Bool
                public byte Unk10 { get; set; } = 0; // Bool
                public byte Unk11 { get; set; } = 1; // Bool
                private short Unk12 { get; set; } = 0; // Hidden
                public byte Unk14 { get; set; } = 0; // Bool
                public byte Unk15 { get; set; } = 0; // Bool
                public byte Unk16 { get; set; } = 0; // Bool
                public byte Unk17 { get; set; } = 0; // Bool
                private short Unk18 { get; set; } = 0; // Hidden
                public byte Unk1A { get; set; } = 0;  // Bool
                public byte Unk1B { get; set; } = 0;
                public uint[] EntityGroupIDs { get; set; } = new uint[8];
                public short Unk3C { get; set; } = -1;
                public short Unk3E { get; set; } = 0;
                public int RandomAppearParamID { get; set; } = 0;
                public int Variation { get; set; } = -1;
                private int Unk48 { get; set; } // Hidden
                public int Unk4C { get; set; }

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
                private int Unk08 { get; set; } = 0; // Hidden
                private int Unk0C { get; set; } = 0; // Hidden
                private int Unk10 { get; set; } = 0; // Hidden
                private int Unk14 { get; set; } = 0; // Hidden
                private int Unk18 { get; set; } = 0; // Hidden
                private int Unk1C { get; set; } = 0; // Hidden

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
                private int Unk00 { get; set; } = 0; // Hidden
                private int Unk04 { get; set; } = 0; // Hidden
                private int Unk08 { get; set; } = 0; // Hidden
                private int Unk0C { get; set; } = 0; // Hidden
                public float TransitionTime { get; set; } = -1;
                private int Unk14 { get; set; } = 0; // Hidden
                public sbyte Unk18 { get; set; } = -1;
                private sbyte Unk19 { get; set; } = -1; // Hidden
                private short Unk1A { get; set; } = -1; // Hidden
                private sbyte Unk1C { get; set; } = -1; // Hidden
                public sbyte Unk1D { get; set; } = -1;
                private short Unk1E { get; set; } = 0; // Hidden
                public short Unk20 { get; set; } = -1;
                private short Unk22 { get; set; } = 0; // Hidden
                private int Unk24 { get; set; } = 0; // Hidden
                private int Unk28 { get; set; } = 0; // Hidden
                private int Unk2C { get; set; } = 0; // Hidden
                private int Unk30 { get; set; } = 0; // Hidden
                private int Unk34 { get; set; } = 0; // Hidden
                private int Unk38 { get; set; } = 0; // Hidden
                private int Unk3C { get; set; } = 0; // Hidden
                private int Unk40 { get; set; } = 0; // Hidden
                private int Unk44 { get; set; } = 0; // Hidden
                private int Unk48 { get; set; } = 0; // Hidden
                private int Unk4C { get; set; } = 0; // Hidden
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
                private int Unk00 { get; set; } = 0; // Hidden
                private int Unk04 { get; set; } = 0; // Hidden
                private int Unk08 { get; set; } = 0; // Hidden
                private int Unk0C { get; set; } = 0; // Hidden
                private int Unk10 { get; set; } = 0; // Hidden
                private int Unk14 { get; set; } = 0; // Hidden
                private int Unk18 { get; set; } = 0; // Hidden
                private int Unk1C { get; set; } = 0; // Hidden
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
                private int Unk04 { get; set; } = 0; // Hidden
                private int Unk08 { get; set; } = 0; // Hidden
                private int Unk0C { get; set; } = 0; // Hidden
                private int Unk10 { get; set; } = 0; // Hidden
                private int Unk14 { get; set; } = 0; // Hidden
                private int Unk18 { get; set; } = 0; // Hidden
                private int Unk1C { get; set; } = 0; // Hidden

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
                    MapID = br.ReadSBytes(4);
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
                    bw.WriteSBytes(MapID);
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
                private int Unk08 { get; set; } = 0; // Hidden
                public int Unk0C { get; set; } = -1;
                private int Unk10 { get; set; } = 0; // Hidden
                public int Unk14 { get; set; } = -1;
                private int Unk18 { get; set; } = 0; // Hidden
                private int Unk1C { get; set; } = 0; // Hidden

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
                private int Unk00 { get; set; } = 0; // Hidden
                private int Unk04 { get; set; } = 0; // Hidden
                private int Unk08 { get; set; } = 0; // Hidden
                private int Unk0C { get; set; } = 0; // Hidden
                private int Unk10 { get; set; } = 0; // Hidden
                private int Unk14 { get; set; } = 0; // Hidden
                private int Unk18 { get; set; } = 0; // Hidden
                private int Unk1C { get; set; } = 0; // Hidden
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
                private short Unk06 { get; set; } = 0; // Hidden
                private int Unk08 { get; set; } = 0; // Hidden
                private int Unk0C { get; set; } = 0; // Hidden
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
                private protected override bool HasUnkA8Data => true;
                public UnkA8Struct UnkA8Data { get; set; }
                private protected override void ReadUnkA8Data(BinaryReaderEx br) => UnkA8Data = new UnkA8Struct(br);
                private protected override void WriteUnkA8Data(BinaryWriterEx bw) => UnkA8Data.Write(bw);

                private protected EnemyBase() : base("cXXXX_XXXX")
                {
                    EntityData = new EntityStruct();
                    DisplayData = new DisplayStruct();
                    GparamData = new GparamStruct();
                    Unk88Data = new Unk88Struct();
                    TileData = new TileStruct();
                    UnkA8Data = new UnkA8Struct();
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
                    enemy.UnkA8Data = UnkA8Data.DeepCopy();
                }

                internal override void GetNames(MSB_NR msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    HitPartName = MSB.FindName(entries.Parts, HitPartIndex);
                    PatrolRouteName = MSB.FindName(msb.Events.PatrolRoutes, PatrolRouteIndex);
                }

                internal override void GetIndices(MSB_NR msb, Entries entries)
                {
                    base.GetIndices(msb, entries);
                    HitPartIndex = MSB.FindIndex(this, entries.Parts, HitPartName);
                    PatrolRouteIndex = (short)MSB.FindIndex(this, msb.Events.PatrolRoutes, PatrolRouteName);
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
                    ScenarioPlacementParamID = br.ReadInt16();
                    Unk24 = br.ReadInt32();
                    CondemnedSpEffectSetParamIds = br.ReadInt32s(4);
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
                    bw.WriteInt16(ScenarioPlacementParamID);
                    bw.WriteInt32(Unk24);
                    bw.WriteInt32s(CondemnedSpEffectSetParamIds);
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

                    bw.WriteInt64(Offset70);

                    bw.ReserveInt64("Offset78");
                    bw.FillInt64("Offset78", bw.Position - start);
                    Unk78Data.Write(bw);
                }

                // Internal
                public EnemyUnk78Struct Unk78Data { get; set; }

                // Layout
                private int Unk00 { get; set; } = -1; // Hidden
                private int Unk04 { get; set; } = -1; // Hidden
                public int NpcThinkParamId { get; set; } = 0;
                public int NpcParamId { get; set; } = 0;
                public int TalkID { get; set; } = 0;
                private byte Unk14 { get; set; } = 0; // Hidden
                public byte Unk15 { get; set; } = 0; // Boolean
                public short PlatoonId { get; set; } = 0;
                public int CharaInitParamId { get; set; } = -1;
                private int HitPartIndex { get; set; } = -1;
                private short PatrolRouteIndex { get; set; }
                public short ScenarioPlacementParamID { get; set; } = -1;
                private int Unk24 { get; set; } = -1; // Hidden
                public int[] CondemnedSpEffectSetParamIds { get; set; } = new int[4];
                public int BackupEventAnimID { get; set; } = -1;
                public sbyte Unk3C { get; set; } = -1;
                private byte Unk3D { get; set; } = 0; // Hidden
                public short Unk3E { get; set; } = -1;
                public int[] SpEffectSetParamIds { get; set; } = new int[4];
                private int Unk50 { get; set; } = 0; // Hidden
                private int Unk54 { get; set; } = 0; // MODIFIED
                private int Unk58 { get; set; } = 0; // Hidden
                private int Unk5C { get; set; } = 0; // MODIFIED
                private int Unk60 { get; set; } = 0; // Hidden
                private int Unk64 { get; set; } = 0; // Hidden
                private int Unk68 { get; set; } = 0; // Hidden
                private int Unk6C { get; set; } = 0; // Hidden

                // Offsets
                private long Offset70 { get; set; } = 0; // Hidden
                private long Offset78 { get; set; } = 0; // Hidden

                // Names
                [MSBReference(ReferenceType = typeof(Collision))]
                public string HitPartName { get; set; }

                [MSBReference(ReferenceType = typeof(Event.PatrolRoute))]
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
                    private int Unk30 { get; set; } = 0; // Hidden
                    private int Unk34 { get; set; } = 0; // Hidden
                    private int Unk38 { get; set; } = 0; // Hidden
                    private int Unk3C { get; set; } = 0; // Hidden

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
                private int Unk04 { get; set; } = 0; // Hidden
                private int Unk08 { get; set; } = 0; // Hidden
                private int Unk0C { get; set; } = 0; // Hidden
            }

            /// <summary>
            /// Invisible but physical geometry.
            /// </summary>
            public class Collision : Part
            {
                private protected override PartType Type => PartType.Collision;

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
                private protected override bool HasDisplayGroupData => true;
                public DisplayGroupStruct DisplayGroupData { get; set; }
                private protected override void ReadDisplayGroupData(BinaryReaderEx br) => DisplayGroupData = new DisplayGroupStruct(br);
                private protected override void WriteDisplayGroupData(BinaryWriterEx bw) => DisplayGroupData.Write(bw);

                // --- Gparam
                private protected override bool HasGparamData => true;
                public GparamStruct GparamData { get; set; }
                private protected override void ReadGparamData(BinaryReaderEx br) => GparamData = new GparamStruct(br);
                private protected override void WriteGparamData(BinaryWriterEx bw) => GparamData.Write(bw);

                // --- Scene Gparam
                private protected override bool HasSceneGparamData => true;
                public SceneGparamStruct SceneGparamData { get; set; }
                private protected override void ReadSceneGparamData(BinaryReaderEx br) => SceneGparamData = new SceneGparamStruct(br);
                private protected override void WriteSceneGparamData(BinaryWriterEx bw) => SceneGparamData.Write(bw);

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
                private protected override bool HasUnkA0Data => true;
                public UnkA0Struct UnkA0Data { get; set; }
                private protected override void ReadUnkA0Data(BinaryReaderEx br) => UnkA0Data = new UnkA0Struct(br);
                private protected override void WriteUnkA0Data(BinaryWriterEx bw) => UnkA0Data.Write(bw);

                // --- UnkA8
                private protected override bool HasUnkA8Data => false;
                //public UnkA8Struct UnkA8Data { get; set; }
                //private protected override void ReadUnkA8Data(BinaryReaderEx br) => UnkA8Data = new UnkA8Struct(br);
                //private protected override void WriteUnkA8Data(BinaryWriterEx bw) => UnkA8Data.Write(bw);

                public Collision() : base("hXXXXXX")
                {
                    EntityData = new EntityStruct();
                    DisplayData = new DisplayStruct();
                    DisplayGroupData = new DisplayGroupStruct();
                    GparamData = new GparamStruct();
                    SceneGparamData = new SceneGparamStruct();
                    Unk88Data = new Unk88Struct();
                    TileData = new TileStruct();
                    UnkA0Data = new UnkA0Struct();
                }

                private protected override void DeepCopyTo(Part part)
                {
                    var collision = (Collision)part;
                    collision.EntityData = EntityData.DeepCopy();
                    collision.DisplayData = DisplayData.DeepCopy();
                    collision.DisplayGroupData = DisplayGroupData.DeepCopy();
                    collision.GparamData = GparamData.DeepCopy();
                    collision.SceneGparamData = SceneGparamData.DeepCopy();
                    collision.Unk88Data = Unk88Data.DeepCopy();
                    collision.TileData = TileData.DeepCopy();
                    collision.UnkA0Data = UnkA0Data.DeepCopy();
                }

                internal Collision(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    HitFilterID = br.ReadByte();
                    Unk01 = br.ReadSByte();
                    Unk02 = br.ReadSByte();
                    Unk03 = br.ReadByte();
                    Unk04 = br.ReadSingle();
                    Unk08 = br.ReadInt32();
                    Unk0C = br.ReadInt32();
                    Unk10 = br.ReadInt32();
                    Unk14 = br.ReadSingle();
                    LocationTextID = br.ReadInt32();
                    Unk1C = br.ReadInt32();
                    PlayRegionID = br.ReadInt32();
                    Unk24 = br.ReadInt16();
                    Unk26 = br.ReadByte();
                    Unk27 = br.ReadByte();
                    Unk28 = br.ReadInt32();
                    Unk2C = br.ReadInt32();
                    Unk30 = br.ReadInt32();
                    Unk34 = br.ReadByte();
                    Unk35 = br.ReadSByte();
                    Unk36 = br.ReadInt16();
                    Unk38 = br.ReadInt32();
                    Unk3C = br.ReadInt16();
                    Unk3E = br.ReadInt16();
                    Unk40 = br.ReadInt32();
                    HitUnk44 = br.ReadInt32();
                    Unk48 = br.ReadInt32();
                    HitUnk4C = br.ReadInt16();
                    Unk4E = br.ReadInt16();
                }
                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteByte(HitFilterID);
                    bw.WriteSByte(Unk01);
                    bw.WriteSByte(Unk02);
                    bw.WriteByte(Unk03);
                    bw.WriteSingle(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                    bw.WriteInt32(Unk10);
                    bw.WriteSingle(Unk14);
                    bw.WriteInt32(LocationTextID);
                    bw.WriteInt32(Unk1C);
                    bw.WriteInt32(PlayRegionID);
                    bw.WriteInt16(Unk24);
                    bw.WriteByte(Unk26);
                    bw.WriteByte(Unk27);
                    bw.WriteInt32(Unk28);
                    bw.WriteInt32(Unk2C);
                    bw.WriteInt32(Unk30);
                    bw.WriteByte(Unk34);
                    bw.WriteSByte(Unk35);
                    bw.WriteInt16(Unk36);
                    bw.WriteInt32(Unk38);
                    bw.WriteInt16(Unk3C);
                    bw.WriteInt16(Unk3E);
                    bw.WriteInt32(Unk40);
                    bw.WriteInt32(HitUnk44);
                    bw.WriteInt32(Unk48);
                    bw.WriteInt16(HitUnk4C);
                    bw.WriteInt16(Unk4E);
                }

                // Layout
                public byte HitFilterID { get; set; } = 8;
                private sbyte Unk01 { get; set; } = -1; // Hidden
                public sbyte Unk02 { get; set; } = -1;
                private byte Unk03 { get; set; } = 0; // Hidden
                public float Unk04 { get; set; } = 0;
                private int Unk08 { get; set; } = 0; // Hidden
                private int Unk0C { get; set; } = 0; // Hidden
                private int Unk10 { get; set; } = 0; // Hidden
                private float Unk14 { get; set; } = -1; // Hidden
                public int LocationTextID { get; set; } = -1;
                public int Unk1C { get; set; } = -1;
                public int PlayRegionID { get; set; } = -1;
                public short Unk24 { get; set; } = -1;
                public byte Unk26 { get; set; } = 0; // Boolean
                public byte Unk27 { get; set; } = 0; // Boolean
                private int Unk28 { get; set; } = 0; // Hidden
                private int Unk2C { get; set; } = -1; // Hidden
                private int Unk30 { get; set; } = -1; // Hidden
                public byte Unk34 { get; set; } = 0;
                public sbyte Unk35 { get; set; } = -1;
                private short Unk36 { get; set; } = 0; // Hidden
                private int Unk38 { get; set; } = -1; // Hidden
                public short Unk3C { get; set; } = -1;
                private short Unk3E { get; set; } = -1; // Hidden
                private int Unk40 { get; set; } = 0; // Hidden
                private int HitUnk44 { get; set; } = 0; // Hidden
                private int Unk48 { get; set; } = 0; // Hidden
                private short HitUnk4C { get; set; } = 0; // Hidden
                private short Unk4E { get; set; } = -1; // Hidden

            }

            /// <summary>
            /// This is in the same type of a legacy DummyObject, but struct is pretty gutted
            /// </summary>
            public class DummyAsset : Part
            {
                private protected override PartType Type => PartType.DummyAsset;

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

                public DummyAsset() : base("AEGxxx_xxx_xxxx")
                {
                    EntityData = new EntityStruct();
                    DisplayData = new DisplayStruct();
                    GparamData = new GparamStruct();
                    Unk88Data = new Unk88Struct();
                    TileData = new TileStruct();
                }

                private protected override void DeepCopyTo(Part part)
                {
                    var asset = (DummyAsset)part;
                    asset.EntityData = EntityData.DeepCopy();
                    asset.DisplayData = DisplayData.DeepCopy();
                    asset.GparamData = GparamData.DeepCopy();
                    asset.Unk88Data = Unk88Data.DeepCopy();
                    asset.TileData = TileData.DeepCopy();
                }

                internal DummyAsset(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
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
                }

                // Layout
                private int Unk00 { get; set; } = 0; // Hidden
                private int Unk04 { get; set; } = 0; // Hidden
                private int Unk08 { get; set; } = -1; // Hidden
                private int Unk0C { get; set; } = 0; // Hidden
                private int Unk10 { get; set; } = -1; // Hidden
                private int Unk14 { get; set; } = -1; // Hidden
                private int Unk18 { get; set; } = -1; // Hidden
                private int Unk1C { get; set; } = -1; // Hidden
            }

            /// <summary>
            /// An enemy that either isn't used, or is used for a cutscene.
            /// </summary>
            public class DummyEnemy : EnemyBase
            {
                private protected override PartType Type => PartType.DummyEnemy;
                private protected override bool HasUnkA8Data => false;

                public DummyEnemy() : base() { }

                internal DummyEnemy(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// References an actual collision and causes another map to be loaded while on it.
            /// </summary>
            public class ConnectCollision : Part
            {
                private protected override PartType Type => PartType.ConnectCollision;

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
                private protected override bool HasDisplayGroupData => true;
                public DisplayGroupStruct DisplayGroupData { get; set; }
                private protected override void ReadDisplayGroupData(BinaryReaderEx br) => DisplayGroupData = new DisplayGroupStruct(br);
                private protected override void WriteDisplayGroupData(BinaryWriterEx bw) => DisplayGroupData.Write(bw);

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
                private protected override bool HasUnkA0Data => true;
                public UnkA0Struct UnkA0Data { get; set; }
                private protected override void ReadUnkA0Data(BinaryReaderEx br) => UnkA0Data = new UnkA0Struct(br);
                private protected override void WriteUnkA0Data(BinaryWriterEx bw) => UnkA0Data.Write(bw);

                // --- UnkA8
                private protected override bool HasUnkA8Data => false;
                //public UnkA8Struct UnkA8Data { get; set; }
                //private protected override void ReadUnkA8Data(BinaryReaderEx br) => UnkA8Data = new UnkA8Struct(br);
                //private protected override void WriteUnkA8Data(BinaryWriterEx bw) => UnkA8Data.Write(bw);

                public ConnectCollision() : base("hXXXXXX_XXXX")
                {
                    EntityData = new EntityStruct();
                    DisplayData = new DisplayStruct();
                    DisplayGroupData = new DisplayGroupStruct();
                    Unk88Data = new Unk88Struct();
                    TileData = new TileStruct();
                    UnkA0Data = new UnkA0Struct();
                }

                private protected override void DeepCopyTo(Part part)
                {
                    var connect = (ConnectCollision)part;
                    connect.EntityData = EntityData.DeepCopy();
                    connect.DisplayData = DisplayData.DeepCopy();
                    connect.DisplayGroupData = DisplayGroupData.DeepCopy();
                    connect.Unk88Data = Unk88Data.DeepCopy();
                    connect.TileData = TileData.DeepCopy();
                    connect.UnkA0Data = UnkA0Data.DeepCopy();
                }

                internal override void GetNames(MSB_NR msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    ParentHitName = MSB.FindName(msb.Parts.Collisions, ParentHitIndex);
                }
                internal override void GetIndices(MSB_NR msb, Entries entries)
                {
                    base.GetIndices(msb, entries);
                    ParentHitIndex = MSB.FindIndex(this, msb.Parts.Collisions, ParentHitName);
                }

                internal ConnectCollision(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    ParentHitIndex = br.ReadInt32();
                    MapID = br.ReadSBytes(4);
                    Unk08 = br.ReadInt16();
                    Unk0A = br.ReadSByte();
                    Unk0B = br.ReadSByte();
                    Unk0C = br.ReadInt32();
                }
                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(ParentHitIndex);
                    bw.WriteSBytes(MapID);
                    bw.WriteInt16(Unk08);
                    bw.WriteSByte(Unk0A);
                    bw.WriteSByte(Unk0B);
                    bw.WriteInt32(Unk0C);
                }

                // Layout
                private int ParentHitIndex { get; set; }
                public sbyte[] MapID { get; set; } = new sbyte[4];
                private short Unk08 { get; set; } = 0; // Hidden
                public sbyte Unk0A { get; set; } = -1;
                public sbyte Unk0B { get; set; } = 0; // Boolean
                private int Unk0C { get; set; } = 0; // Hidden

                [MSBReference(ReferenceType = typeof(Collision))]
                [NoRenderGroupInheritence()]
                public string ParentHitName { get; set; }
            }

            /// <summary>
            /// An asset placement in Elden Ring
            /// </summary>
            public class Asset : Part
            {
                private protected override PartType Type => PartType.Asset;

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
                private protected override bool HasDisplayGroupData => true;
                public DisplayGroupStruct DisplayGroupData { get; set; }
                private protected override void ReadDisplayGroupData(BinaryReaderEx br) => DisplayGroupData = new DisplayGroupStruct(br);
                private protected override void WriteDisplayGroupData(BinaryWriterEx bw) => DisplayGroupData.Write(bw);

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
                private protected override bool HasUnkA8Data => true;
                public UnkA8Struct UnkA8Data { get; set; }
                private protected override void ReadUnkA8Data(BinaryReaderEx br) => UnkA8Data = new UnkA8Struct(br);
                private protected override void WriteUnkA8Data(BinaryWriterEx bw) => UnkA8Data.Write(bw);

                public Asset() : base("AEGxxx_xxx_xxxx")
                {
                    EntityData = new EntityStruct();
                    DisplayData = new DisplayStruct();
                    DisplayGroupData = new DisplayGroupStruct();
                    GparamData = new GparamStruct();
                    GrassData = new GrassStruct();
                    Unk88Data = new Unk88Struct();
                    Unk90Data = new Unk90Struct();
                    TileData = new TileStruct();
                    UnkA0Data = new UnkA0Struct();
                    UnkA8Data = new UnkA8Struct();

                    InnerStruct68 = new AssetInnnerStruct68();
                }

                internal Asset(BinaryReaderEx br) : base(br) { }

                private protected override void DeepCopyTo(Part part)
                {
                    var asset = (Asset)part;
                    asset.EntityData = EntityData.DeepCopy();
                    asset.DisplayData = DisplayData.DeepCopy();
                    asset.DisplayGroupData = DisplayGroupData.DeepCopy();
                    asset.GparamData = GparamData.DeepCopy();
                    asset.GrassData = GrassData.DeepCopy();
                    asset.Unk88Data = Unk88Data.DeepCopy();
                    asset.Unk90Data = Unk90Data.DeepCopy();
                    asset.TileData = TileData.DeepCopy();
                    asset.UnkA0Data = UnkA0Data.DeepCopy();
                    asset.UnkA8Data = UnkA8Data.DeepCopy();

                    asset.InnerStruct68 = InnerStruct68.DeepCopy();
                    asset.InnerStruct70 = InnerStruct70.DeepCopy();
                    asset.InnerStruct78 = InnerStruct78.DeepCopy();
                    asset.InnerStruct80 = InnerStruct80.DeepCopy();
                }

                internal override void GetNames(MSB_NR msb, Entries entries)
                {
                    base.GetNames(msb, entries);

                    PartName1 = MSB.FindName(entries.Parts, PartIndex1);
                    PartName2 = MSB.FindName(entries.Parts, PartIndex2);
                    PartName3 = MSB.FindName(entries.Parts, PartIndex3);
                    PartName4 = MSB.FindName(entries.Parts, PartIndex4);
                    PartName5 = MSB.FindName(entries.Parts, PartIndex5);
                    PartName6 = MSB.FindName(entries.Parts, PartIndex6);
                }

                internal override void GetIndices(MSB_NR msb, Entries entries)
                {
                    base.GetIndices(msb, entries);

                    PartIndex1 = MSB.FindIndex(this, entries.Parts, PartName1);
                    PartIndex2 = MSB.FindIndex(this, entries.Parts, PartName2);
                    PartIndex3 = MSB.FindIndex(this, entries.Parts, PartName3);
                    PartIndex4 = MSB.FindIndex(this, entries.Parts, PartName4);
                    PartIndex5 = MSB.FindIndex(this, entries.Parts, PartName5);
                    PartIndex6 = MSB.FindIndex(this, entries.Parts, PartName6);
                }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    var start = br.Position;

                    Unk00 = br.ReadByte();
                    Unk01 = br.ReadByte();
                    Unk02 = br.ReadInt16();
                    Unk04 = br.ReadInt32();
                    Unk08 = br.ReadInt32();
                    Unk0C = br.ReadInt32();
                    Unk10 = br.ReadByte();
                    Unk11 = br.ReadByte();
                    Unk12 = br.ReadSByte();
                    Unk13 = br.ReadByte();
                    Unk14 = br.ReadInt32();
                    Unk18 = br.ReadInt32();
                    AssetSfxParamRelativeID = br.ReadInt16();
                    Unk1E = br.ReadInt16();
                    Unk20 = br.ReadInt32();
                    Unk24 = br.ReadInt32();
                    Unk28 = br.ReadInt32();
                    Unk2C = br.ReadInt32();
                    Unk30 = br.ReadInt32();
                    ItemLotParamMapID = br.ReadInt32();
                    PartIndex1 = br.ReadInt32();
                    Unk3C = br.ReadInt32();
                    PartIndex2 = br.ReadInt32();
                    PartIndex3 = br.ReadInt32();
                    PartIndex4 = br.ReadInt32();
                    PartIndex5 = br.ReadInt32();
                    Unk50 = br.ReadInt32();
                    PartIndex6 = br.ReadInt32();
                    Unk58 = br.ReadInt32();
                    Unk5C = br.ReadInt32();
                    Unk60 = br.ReadInt32();
                    Unk64 = br.ReadInt32();

                    Offset68 = br.ReadInt64();
                    Offset70 = br.ReadInt64();
                    Offset78 = br.ReadInt64();
                    Offset80 = br.ReadInt64();

                    if (!BinaryReaderEx.IgnoreAsserts)
                    {
                        if (Offset68 != 0x88)
                            throw new InvalidDataException($"Unexpected {nameof(Offset68)} 0x{Offset68:X} in type {GetType()}.");

                        if (Offset70 != 0xC8)
                            throw new InvalidDataException($"Unexpected {nameof(Offset70)} 0x{Offset70:X} in type {GetType()}.");

                        if (Offset78 != 0x108)
                            throw new InvalidDataException($"Unexpected {nameof(Offset78)} 0x{Offset78:X} in type {GetType()}.");

                        if (Offset80 != 0x148)
                            throw new InvalidDataException($"Unexpected {nameof(Offset80)} 0x{Offset80:X} in type {GetType()}.");
                    }

                    br.Position = start + Offset68;
                    InnerStruct68 = new AssetInnnerStruct68(br);

                    br.Position = start + Offset70;
                    InnerStruct70 = new AssetInnnerStruct70(br);

                    br.Position = start + Offset78;
                    InnerStruct78 = new AssetInnnerStruct78(br);

                    br.Position = start + Offset80;
                    InnerStruct80 = new AssetInnnerStruct80(br);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    var start = bw.Position;

                    bw.WriteByte(Unk00);
                    bw.WriteByte(Unk01);
                    bw.WriteInt16(Unk02);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                    bw.WriteByte(Unk10);
                    bw.WriteByte(Unk11);
                    bw.WriteSByte(Unk12);
                    bw.WriteByte(Unk13);
                    bw.WriteInt32(Unk14);
                    bw.WriteInt32(Unk18);
                    bw.WriteInt16(AssetSfxParamRelativeID);
                    bw.WriteInt16(Unk1E);
                    bw.WriteInt32(Unk20);
                    bw.WriteInt32(Unk24);
                    bw.WriteInt32(Unk28);
                    bw.WriteInt32(Unk2C);
                    bw.WriteInt32(Unk30);
                    bw.WriteInt32(ItemLotParamMapID);
                    bw.WriteInt32(PartIndex1);
                    bw.WriteInt32(Unk3C);
                    bw.WriteInt32(PartIndex2);
                    bw.WriteInt32(PartIndex3);
                    bw.WriteInt32(PartIndex4);
                    bw.WriteInt32(PartIndex5);
                    bw.WriteInt32(Unk50);
                    bw.WriteInt32(PartIndex6);
                    bw.WriteInt32(Unk58);
                    bw.WriteInt32(Unk5C);
                    bw.WriteInt32(Unk60);
                    bw.WriteInt32(Unk64);

                    bw.ReserveInt64("Offset68");
                    bw.ReserveInt64("Offset70");
                    bw.ReserveInt64("Offset78");
                    bw.ReserveInt64("Offset80");

                    bw.FillInt64("Offset68", bw.Position - start);
                    InnerStruct68.Write(bw);

                    bw.FillInt64("Offset70", bw.Position - start);
                    InnerStruct70.Write(bw);

                    bw.FillInt64("Offset78", bw.Position - start);
                    InnerStruct78.Write(bw);

                    bw.FillInt64("Offset80", bw.Position - start);
                    InnerStruct80.Write(bw);
                }

                // Internal
                public AssetInnnerStruct68 InnerStruct68 { get; set; }
                public AssetInnnerStruct70 InnerStruct70 { get; set; }
                public AssetInnnerStruct78 InnerStruct78 { get; set; }
                public AssetInnnerStruct80 InnerStruct80 { get; set; }

                // Layout
                public byte Unk00 { get; set; } = 0; // Boolean
                public byte Unk01 { get; set; } = 0;
                private short Unk02 { get; set; } = 0; // Hidden
                public int Unk04 { get; set; } = -1;
                private int Unk08 { get; set; } = 0; // Hidden
                private int Unk0C { get; set; } = 0; // Hidden
                public byte Unk10 { get; set; } = 0;
                public byte Unk11 { get; set; } = 0; // Boolean
                public sbyte Unk12 { get; set; } = -1;
                private byte Unk13 { get; set; } = 0; // Hidden
                public int Unk14 { get; set; } = 0;
                private int Unk18 { get; set; } = 0; // Hidden
                public short AssetSfxParamRelativeID { get; set; } = -1;
                private short Unk1E { get; set; } = -1; // Hidden
                private int Unk20 { get; set; } = -1; // Hidden
                private int Unk24 { get; set; } = -1; // Hidden
                private int Unk28 { get; set; } = 0; // Hidden
                private int Unk2C { get; set; } = 0; // Hidden
                private int Unk30 { get; set; } = -1; // Hidden
                public int ItemLotParamMapID { get; set; } = -1;
                public int PartIndex1 { get; set; } = -1;
                public int Unk3C { get; set; } = -1; 
                public int PartIndex2 { get; set; } = -1;
                public int PartIndex3 { get; set; } = -1;
                public int PartIndex4 { get; set; } = -1;
                public int PartIndex5 { get; set; } = -1;
                private int Unk50 { get; set; } = -1; // Hidden
                public int PartIndex6 { get; set; } = -1;
                public int Unk58 { get; set; } = -1;
                private int Unk5C { get; set; } = -1; // Hidden
                private int Unk60 { get; set; } = -1; // Hidden
                private int Unk64 { get; set; } = -1; // Hidden

                private long Offset68 { get; set; } = 0;
                private long Offset70 { get; set; } = 0;
                private long Offset78 { get; set; } = 0;
                private long Offset80 { get; set; } = 0;

                // Names
                [MSBReference(ReferenceType = typeof(Part))]
                public string PartName1 { get; set; }

                [MSBReference(ReferenceType = typeof(Part))]
                public string PartName2 { get; set; }

                [MSBReference(ReferenceType = typeof(Part))]
                public string PartName3 { get; set; }

                [MSBReference(ReferenceType = typeof(Part))]
                public string PartName4 { get; set; }

                [MSBReference(ReferenceType = typeof(Part))]
                public string PartName5 { get; set; }

                [MSBReference(ReferenceType = typeof(Part))]
                public string PartName6 { get; set; }

                public class AssetInnnerStruct68
                {
                    public AssetInnnerStruct68() { }

                    public AssetInnnerStruct68 DeepCopy()
                    {
                        return (AssetInnnerStruct68)MemberwiseClone();
                    }

                    internal AssetInnnerStruct68(BinaryReaderEx br)
                    {
                        Unk00 = br.ReadInt16();
                        Unk02 = br.ReadInt16();
                        Unk04 = br.ReadInt16();
                        Unk06 = br.ReadInt16();
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

                    internal void Write(BinaryWriterEx bw)
                    {
                        bw.WriteInt16(Unk00);
                        bw.WriteInt16(Unk02);
                        bw.WriteInt16(Unk04);
                        bw.WriteInt16(Unk06);
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
                    public short Unk00 { get; set; } = 0; // Boolean
                    public short Unk02 { get; set; } = -1; // Hidden
                    public short Unk04 { get; set; } = 0; // Boolean
                    public short Unk06 { get; set; } = -1; // Hidden
                    public int Unk08 { get; set; } = 0; // Hidden
                    public int Unk0C { get; set; } = 0; // Hidden
                    public int Unk10 { get; set; } = -1; // Hidden
                    public int Unk14 { get; set; } = -1; // Hidden
                    public int Unk18 { get; set; } = -1; // Hidden
                    public int Unk1C { get; set; } = -1; // Hidden
                    public int Unk20 { get; set; } = 0; // Hidden
                    public int Unk24 { get; set; } = -1; // Hidden
                    public int Unk28 { get; set; } = -1; // Hidden
                    public int Unk2C { get; set; } = -1; // Hidden
                    public int Unk30 { get; set; } = 0; // Hidden
                    public int Unk34 { get; set; } = 0; // Hidden
                    public int Unk38 { get; set; } = 0; // Hidden
                    public int Unk3C { get; set; } = 0; // Hidden
                }

                public class AssetInnnerStruct70
                {
                    public AssetInnnerStruct70() { }

                    public AssetInnnerStruct70 DeepCopy()
                    {
                        return (AssetInnnerStruct70)MemberwiseClone();
                    }

                    internal AssetInnnerStruct70(BinaryReaderEx br)
                    {
                        Unk00 = br.ReadInt32();
                        Unk04 = br.ReadInt32();
                        Unk08 = br.ReadInt32();
                        Unk0C = br.ReadInt32();
                        Unk10 = br.ReadInt32();
                        Unk14 = br.ReadSingle();
                        Unk18 = br.ReadInt32();
                        Unk1C = br.ReadSByte();
                        Unk1D = br.ReadSByte();
                        Unk1E = br.ReadInt16();
                        Unk20 = br.ReadInt32();
                        Unk24 = br.ReadInt32();
                        Unk28 = br.ReadInt32();
                        Unk2C = br.ReadInt32();
                        Unk30 = br.ReadInt32();
                        Unk34 = br.ReadInt32();
                        Unk38 = br.ReadInt32();
                        Unk3C = br.ReadInt32();
                    }

                    internal void Write(BinaryWriterEx bw)
                    {
                        bw.WriteInt32(Unk00);
                        bw.WriteInt32(Unk04);
                        bw.WriteInt32(Unk08);
                        bw.WriteInt32(Unk0C);
                        bw.WriteInt32(Unk10);
                        bw.WriteSingle(Unk14);
                        bw.WriteInt32(Unk18);
                        bw.WriteSByte(Unk1C);
                        bw.WriteSByte(Unk1D);
                        bw.WriteInt16(Unk1E);
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
                    private int Unk00 { get; set; } = 0; // Hidden
                    public int Unk04 { get; set; } = -1;
                    private int Unk08 { get; set; } = -1; // Hidden
                    private int Unk0C { get; set; } = 0; // Hidden
                    private int Unk10 { get; set; } = 0; // Hidden
                    public float Unk14 { get; set; } = 0;
                    public int Unk18 { get; set; } = 0;
                    public sbyte Unk1C { get; set; } = -1;
                    public sbyte Unk1D { get; set; } = -1;
                    public short Unk1E { get; set; } = -1;
                    private int Unk20 { get; set; } = 0; // Hidden
                    private int Unk24 { get; set; } = 0; // Hidden
                    private int Unk28 { get; set; } = 0; // Hidden
                    private int Unk2C { get; set; } = 0; // Hidden
                    private int Unk30 { get; set; } = 0; // Hidden
                    private int Unk34 { get; set; } = 0; // Hidden
                    private int Unk38 { get; set; } = 0; // Hidden
                    private int Unk3C { get; set; } = 0; // Hidden
                }

                public class AssetInnnerStruct78
                {
                    public AssetInnnerStruct78() { }

                    public AssetInnnerStruct78 DeepCopy()
                    {
                        var unks3 = (AssetInnnerStruct78)MemberwiseClone();
                        return unks3;
                    }

                    internal AssetInnnerStruct78(BinaryReaderEx br)
                    {
                        Unk00 = br.ReadInt32();
                        Unk04 = br.ReadSingle();
                        Unk08 = br.ReadInt16();
                        Unk0A = br.ReadByte();
                        Unk0B = br.ReadSByte();
                        Unk0C = br.ReadInt16();
                        Unk0E = br.ReadInt16();
                        Unk10 = br.ReadSingle();
                        Unk14 = br.ReadInt32();
                        Unk18 = br.ReadInt32();
                        Unk1C = br.ReadInt32();
                        Unk20 = br.ReadInt32();
                        Unk24 = br.ReadSByte();
                        Unk25 = br.ReadByte();
                        Unk26 = br.ReadInt16();
                        Unk28 = br.ReadInt32();
                        Unk2C = br.ReadInt32();
                        Unk30 = br.ReadInt32();
                        Unk34 = br.ReadInt32();
                        Unk38 = br.ReadInt32();
                        Unk3C = br.ReadInt32();
                    }

                    internal void Write(BinaryWriterEx bw)
                    {
                        bw.WriteInt32(Unk00);
                        bw.WriteSingle(Unk04);
                        bw.WriteInt16(Unk08);
                        bw.WriteByte(Unk0A);
                        bw.WriteSByte(Unk0B);
                        bw.WriteInt16(Unk0C);
                        bw.WriteInt16(Unk0E);
                        bw.WriteSingle(Unk10);
                        bw.WriteInt32(Unk14);
                        bw.WriteInt32(Unk18);
                        bw.WriteInt32(Unk1C);
                        bw.WriteInt32(Unk20);
                        bw.WriteSByte(Unk24);
                        bw.WriteByte(Unk25);
                        bw.WriteInt16(Unk26);
                        bw.WriteInt32(Unk28);
                        bw.WriteInt32(Unk2C);
                        bw.WriteInt32(Unk30);
                        bw.WriteInt32(Unk34);
                        bw.WriteInt32(Unk38);
                        bw.WriteInt32(Unk3C);
                    }

                    // Layout
                    public int Unk00 { get; set; } = 0; // Boolean
                    public float Unk04 { get; set; } = 0;
                    private short Unk08 { get; set; } = -1; // Hidden
                    public byte Unk0A { get; set; } = 0;
                    public sbyte Unk0B { get; set; } = -1;
                    public short Unk0C { get; set; } = -1;
                    private short Unk0E { get; set; } = 0; // Hidden
                    private float Unk10 { get; set; } = 0; // Hidden
                    private int Unk14 { get; set; } = -1; // Hidden
                    private int Unk18 { get; set; } = -1; // Hidden
                    private int Unk1C { get; set; } = -1; // Hidden
                    private int Unk20 { get; set; } = -1; // Hidden
                    private sbyte Unk24 { get; set; } = -1; // Hidden
                    private byte Unk25 { get; set; } = 0; // Hidden
                    private short Unk26 { get; set; } = 0; // Hidden
                    private int Unk28 { get; set; } = 0; // Hidden
                    private int Unk2C { get; set; } = 0; // Hidden
                    private int Unk30 { get; set; } = 0; // Hidden
                    private int Unk34 { get; set; } = 0; // Hidden
                    private int Unk38 { get; set; } = 0; // Hidden
                    private int Unk3C { get; set; } = 0; // Hidden

                }

                /// <summary>
                /// Unknown.
                /// </summary>
                public class AssetInnnerStruct80
                {
                    public AssetInnnerStruct80() { }

                    public AssetInnnerStruct80 DeepCopy()
                    {
                        return (AssetInnnerStruct80)MemberwiseClone();
                    }

                    internal AssetInnnerStruct80(BinaryReaderEx br)
                    {
                        Unk00 = br.ReadByte();
                        Unk01 = br.ReadSByte();
                        Unk02 = br.ReadSByte();
                        Unk03 = br.ReadByte();
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

                    internal void Write(BinaryWriterEx bw)
                    {
                        bw.WriteByte(Unk00);
                        bw.WriteSByte(Unk01);
                        bw.WriteSByte(Unk02);
                        bw.WriteByte(Unk03);
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
                    public byte Unk00 { get; set; } = 0; // Boolean
                    private sbyte Unk01 { get; set; } = -1; // Hidden
                    public sbyte Unk02 { get; set; } = -1;
                    private byte Unk03 { get; set; } = 0; // Hidden
                    private int Unk04 { get; set; } = 0; // Hidden
                    private int Unk08 { get; set; } = 0; // Hidden
                    private int Unk0C { get; set; } = 0; // Hidden
                    private int Unk10 { get; set; } = 0; // Hidden
                    private int Unk14 { get; set; } = 0; // Hidden
                    private int Unk18 { get; set; } = 0; // Hidden
                    private int Unk1C { get; set; } = 0; // Hidden
                    private int Unk20 { get; set; } = 0; // Hidden
                    private int Unk24 { get; set; } = 0; // Hidden
                    private int Unk28 { get; set; } = 0; // Hidden
                    private int Unk2C { get; set; } = 0; // Hidden
                    private int Unk30 { get; set; } = 0; // Hidden
                    private int Unk34 { get; set; } = 0; // Hidden
                    private int Unk38 { get; set; } = 0; // Hidden
                    private int Unk3C { get; set; } = 0; // Hidden
                }
            }
        }
    }
}
