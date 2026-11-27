using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

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
            private int Unk44 { get; set; } = 0; // Hidden: not read by nightreign.exe (ER GameEditionDisable); 0, 1 or 3 in retail
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

            /// <summary>
            /// The part's display and draw groups, with an alternate set of each the loader can swap or merge in.
            /// Same six-array layout as MSBE.Part.DisplayDataStruct (from eldenring.exe's loader). Not checked against
            /// the NR exe: the layout rests on the data (Claude/Tools/DisplayGroupSurvey): the second array is only set
            /// when UseAltGroups is, as in ER, and collisions' fourth array always contains their display groups.
            /// </summary>
            public class DisplayStruct
            {
                public DisplayStruct() { }

                public DisplayStruct DeepCopy()
                {
                    var copy = (DisplayStruct)MemberwiseClone();
                    copy.DisplayGroups = (uint[])DisplayGroups.Clone();
                    copy.AltDisplayGroups = (uint[])AltDisplayGroups.Clone();
                    copy.AltDisplayGroupsMask = (uint[])AltDisplayGroupsMask.Clone();
                    copy.DrawGroups = (uint[])DrawGroups.Clone();
                    copy.AltDrawGroups = (uint[])AltDrawGroups.Clone();
                    copy.AltDrawGroupsMask = (uint[])AltDrawGroupsMask.Clone();
                    return copy;
                }

                internal DisplayStruct(BinaryReaderEx br)
                {
                    DisplayGroups = br.ReadUInt32s(8);
                    AltDisplayGroups = br.ReadUInt32s(8);
                    AltDisplayGroupsMask = br.ReadUInt32s(8);
                    DrawGroups = br.ReadUInt32s(8);
                    AltDrawGroups = br.ReadUInt32s(8);
                    AltDrawGroupsMask = br.ReadUInt32s(8);
                    NoAltGroupMerge = br.ReadByte();
                    UseAltGroups = br.ReadByte();
                    UnkC2 = br.ReadInt16();
                    UnkC4 = br.ReadInt16();
                    UnkC6 = br.ReadInt16();
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
                    bw.WriteInt16(UnkC2);
                    bw.WriteInt16(UnkC4);
                    bw.WriteInt16(UnkC6);

                    bw.WritePattern(0xC0, 0x00); // 48 * 4
                }

                // Layout

                /// <summary>
                /// The groups shown while the player stands on this part (collisions), or the groups it belongs to for display.
                /// </summary>
                public uint[] DisplayGroups { get; set; } = new uint[8];

                /// <summary>
                /// Display groups that replace or are merged into DisplayGroups when UseAltGroups is set.
                /// </summary>
                public uint[] AltDisplayGroups { get; set; } = new uint[8];

                /// <summary>
                /// When merging, the DisplayGroups bits set here are kept; the rest come from AltDisplayGroups.
                /// </summary>
                public uint[] AltDisplayGroupsMask { get; set; } = new uint[8];

                /// <summary>
                /// Draw groups the part belongs to: it is drawn while any of them is shown.
                /// </summary>
                public uint[] DrawGroups { get; set; } = new uint[8];

                /// <summary>
                /// Draw groups merged into DrawGroups when UseAltGroups is set.
                /// </summary>
                public uint[] AltDrawGroups { get; set; } = new uint[8];

                /// <summary>
                /// When merging, the DrawGroups bits set here are kept; the rest come from AltDrawGroups.
                /// </summary>
                public uint[] AltDrawGroupsMask { get; set; } = new uint[8];

                /// <summary>
                /// If set, the alternate groups are never merged in, only swapped in whole.
                /// </summary>
                public byte NoAltGroupMerge { get; set; } = 0; // Bool

                /// <summary>
                /// Enables the alternate groups.
                /// </summary>
                public byte UseAltGroups { get; set; } = 0; // Bool

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
                    Condition = br.ReadInt32();
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
                    bw.WriteInt32(Condition);
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
                /// <summary>
                /// A display group number added to DispGroups (ER: the connect collision ORs bit Condition into the draw
                /// mask together with DispGroups). -1 = none. Read by nightreign.exe as an int (140E39400) and as bytes 0-2.
                /// </summary>
                public int Condition { get; set; } = -1;
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
                    isUsePartsDrawParamID = br.ReadByte();
                    VariationAlsoPlus10 = br.ReadByte();
                    Unk06 = br.ReadInt16();
                    PartsDrawParamID = br.ReadInt32();

                    OverridePointLightShadowSrc = br.ReadByte();
                    IsPointLightShadowSrc = br.ReadByte();
                    OverrideDirLightShadowSrc = br.ReadByte();
                    IsDirLightShadowSrc = br.ReadByte();
                    OverrideShadowDest = br.ReadByte();
                    IsShadowDest = br.ReadByte();
                    Unk12 = br.ReadInt16();

                    OverrideShadowOnly = br.ReadByte();
                    IsShadowOnly = br.ReadByte();
                    OverrideDrawByReflectCam = br.ReadByte();
                    DrawByReflectCam = br.ReadByte();
                    OverrideDrawOnlyReflectCam = br.ReadByte();
                    DrawOnlyReflectCam = br.ReadByte();

                    Unk1A = br.ReadByte();
                    Unk1B = br.ReadByte();

                    EntityGroupIDs = br.ReadUInt32s(8);

                    Unk3C = br.ReadInt16();
                    Unk3E = br.ReadByte();
                    DisableRTAO = br.ReadByte();
                    RandomAppearParamID = br.ReadInt32();
                    Variation = br.ReadInt32();

                    Unk48 = br.ReadInt32();
                    Unk4C = br.ReadInt32();
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteUInt32(EntityID);
                    bw.WriteByte(isUsePartsDrawParamID);
                    bw.WriteByte(VariationAlsoPlus10);
                    bw.WriteInt16(Unk06);
                    bw.WriteInt32(PartsDrawParamID);

                    bw.WriteByte(OverridePointLightShadowSrc);
                    bw.WriteByte(IsPointLightShadowSrc);
                    bw.WriteByte(OverrideDirLightShadowSrc);
                    bw.WriteByte(IsDirLightShadowSrc);
                    bw.WriteByte(OverrideShadowDest);
                    bw.WriteByte(IsShadowDest);
                    bw.WriteInt16(Unk12);

                    bw.WriteByte(OverrideShadowOnly);
                    bw.WriteByte(IsShadowOnly);
                    bw.WriteByte(OverrideDrawByReflectCam);
                    bw.WriteByte(DrawByReflectCam);
                    bw.WriteByte(OverrideDrawOnlyReflectCam);
                    bw.WriteByte(DrawOnlyReflectCam);

                    bw.WriteByte(Unk1A);
                    bw.WriteByte(Unk1B);

                    bw.WriteUInt32s(EntityGroupIDs);

                    bw.WriteInt16(Unk3C);
                    bw.WriteByte(Unk3E);
                    bw.WriteByte(DisableRTAO);
                    bw.WriteInt32(RandomAppearParamID);
                    bw.WriteInt32(Variation);

                    bw.WriteInt32(Unk48);
                    bw.WriteInt32(Unk4C);
                }

                // Layout
                public uint EntityID { get; set; } = 0;

                /// <summary>
                /// Enables PartsDrawParamID; otherwise the asset / model param decides. Same as MSBE (nightreign.exe getter 140E35250).
                /// </summary>
                public byte isUsePartsDrawParamID { get; set; } = 0; // Bool

                /// <summary>
                /// Assets in m16_00 / m19_00 only: the part is also placed when the block's variation is Variation + 10
                /// (the block adds 10 when a session flag is set). nightreign.exe 14070078C; MSB Research.md §5.4a.
                /// </summary>
                public byte VariationAlsoPlus10 { get; set; } = 0; // Bool
                private short Unk06 { get; set; } = 0; // Hidden

                /// <summary>
                /// The PartsDrawParam row used when isUsePartsDrawParamID is set. Same as MSBE.
                /// </summary>
                public int PartsDrawParamID { get; set; } = 0;

                // Override byte, then the value it applies over the draw param (MSBE names; read by the NR byte-pair getters)
                public byte OverridePointLightShadowSrc { get; set; } = 0; // Bool
                public byte IsPointLightShadowSrc { get; set; } = 0; // Bool
                public byte OverrideDirLightShadowSrc { get; set; } = 0; // Bool
                public byte IsDirLightShadowSrc { get; set; } = 1; // Bool
                public byte OverrideShadowDest { get; set; } = 0; // Bool
                public byte IsShadowDest { get; set; } = 1; // Bool
                private short Unk12 { get; set; } = 0; // Hidden
                public byte OverrideShadowOnly { get; set; } = 0; // Bool
                public byte IsShadowOnly { get; set; } = 0; // Bool
                public byte OverrideDrawByReflectCam { get; set; } = 0; // Bool
                public byte DrawByReflectCam { get; set; } = 0; // Bool
                public byte OverrideDrawOnlyReflectCam { get; set; } = 0; // Bool
                public byte DrawOnlyReflectCam { get; set; } = 0; // Bool
                private byte Unk1A { get; set; } = 0; // Hidden: not read (ER UnkE1A, also never read); 1 on 88 retail parts
                private byte Unk1B { get; set; } = 0; // Hidden: not read (ER UnkE1B); 2 (once 3) where Unk1A is 1
                public uint[] EntityGroupIDs { get; set; } = new uint[8];

                /// <summary>
                /// Unknown. Read by nightreign.exe (getter 140E33FA0); ER UnkE3C (copied to the geometry and grass
                /// instances). -1 in retail except 16 assets (2 to 900).
                /// </summary>
                public short Unk3C { get; set; } = -1;

                /// <summary>
                /// Unknown. Read as its own byte (140E34830); ER UnkE3E (a render flag). 1 on 1,724 assets and 357 of
                /// 478 map pieces.
                /// </summary>
                public byte Unk3E { get; set; } = 0;

                /// <summary>
                /// Disables ray-traced ambient occlusion for this part. Same offset as MSBE (read as its own byte, 140E347F0).
                /// </summary>
                public byte DisableRTAO { get; set; } = 0; // Bool

                /// <summary>
                /// RandomAppearParam row (0 = none). One slot 0-99 is drawn per session; the part is placed only if the row
                /// has that slot. Paired rows split the slots, so exactly one of two parts appears. MSB Research.md §5.4a.
                /// </summary>
                public int RandomAppearParamID { get; set; } = 0;

                /// <summary>
                /// -1 = always placed. Otherwise placed only when it equals the map block's variation: the expedition
                /// target in boss arenas (m16/m18/m19), the lot's variationId in small-base maps. MSB Research.md §5.4a.
                /// </summary>
                public int Variation { get; set; } = -1;
                private int Unk48 { get; set; } // Hidden
                private int Unk4C { get; set; } // Hidden: not read, 0 in retail

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
                    Unk2C = br.ReadInt32();
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

                /// <summary>
                /// ER GparamSubID_Base. Not seen read by nightreign.exe (only set to -1 with +19..+1B when a part is
                /// created at run time, 140E35123); kept because Smithbox's lighting preview uses it as the scene gparam
                /// sub-ID. -1 on 608 of 675 retail collisions, else 0 to 4.
                /// </summary>
                public sbyte Unk18 { get; set; } = -1;
                private sbyte Unk19 { get; set; } = -1; // Hidden
                private short Unk1A { get; set; } = -1; // Hidden
                private sbyte Unk1C { get; set; } = -1; // Hidden: the sub-ID of Unk20 for parts param versions before 0x31 (140E37190); no NR map is that old

                /// <summary>
                /// Unknown. Read for collisions (140E391C0); an asset's GparamSettings.GparamSettings_Unk0B falls back to it. -1 or 0 in retail.
                /// </summary>
                public sbyte Unk1D { get; set; } = -1;
                private short Unk1E { get; set; } = 0; // Hidden

                /// <summary>
                /// A gparam sub-ID read for collisions (140E392A0, parts param version 0x31 and later; older versions read
                /// Unk1C); an asset's GparamSettings.GparamSettings_Unk0C falls back to it. ER: overrides MapGdRegionInfoParam mapRegionId
                /// when 0 or more (not confirmed). -1 in retail except 9 collisions (0, 30).
                /// </summary>
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
                private int Unk18 { get; set; } = -1; // Hidden: not read, -1 in retail
                private int Unk1C { get; set; } = 0; // Hidden: not read, 0 in retail
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

                /// <summary>
                /// Unknown. Read for assets and map pieces (140E37C50, 140E39820; one byte in 140BB1780). ER UnkStruct9 +0: 1 or
                /// 2 set render flags. 0 in retail except one asset (2).
                /// </summary>
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
                    NamePrefixLength = br.ReadByte();
                    Unk05 = br.ReadByte();
                    Unk06 = br.ReadInt16();
                    Unk08 = br.ReadInt32();
                    Unk0C = br.ReadInt32();
                    Unk10 = br.ReadInt32();
                    SourceInstanceID = br.ReadInt32();
                    Unk18 = br.ReadInt32();
                    Unk1C = br.ReadInt32();
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteSBytes(MapID);
                    bw.WriteByte(NamePrefixLength);
                    bw.WriteByte(Unk05);
                    bw.WriteInt16(Unk06);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                    bw.WriteInt32(Unk10);
                    bw.WriteInt32(SourceInstanceID);
                    bw.WriteInt32(Unk18);
                    bw.WriteInt32(Unk1C);
                }

                // Layout: the source of a part copied from another map (the open-world LOD tiles copy the detailed tiles'
                // parts and name them "mAA_BB_CC_DD-" + the original name). Same block as MSBE TileLoadConfig.

                /// <summary>
                /// The map the part was copied from. -1 bytes when it is the map's own part.
                /// </summary>
                public sbyte[] MapID { get; set; } = new sbyte[4];

                /// <summary>
                /// Characters at the start of the name that the game skips to get the part's own name (140E34B10): 13 for
                /// "mAA_BB_CC_DD-" names, else 0. Same as MSBE. Was the int Unk04 (bytes +5..+7 are 0 in retail).
                /// </summary>
                public byte NamePrefixLength { get; set; } = 0;
                private byte Unk05 { get; set; } = 0; // Hidden: not read
                private short Unk06 { get; set; } = 0; // Hidden: not read
                private int Unk08 { get; set; } = 0; // Hidden

                /// <summary>
                /// Unknown. Read for assets (140E37870); ER: an index into the collisions of the map in MapID, -1 for none.
                /// 0 to 8 or -1 on retail assets, -1 on every other part.
                /// </summary>
                public int Unk0C { get; set; } = -1;
                private int Unk10 { get; set; } = 0; // Hidden

                /// <summary>
                /// The instance ID the game uses for the part when 0 or more; -1 uses the part's own InstanceID (140E34AE0).
                /// Copies on LOD tiles carry the original part's instance ID here. Was Unk14.
                /// </summary>
                public int SourceInstanceID { get; set; } = -1;
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
                    ScenarioPlacementKey0 = br.ReadInt16();
                    ScenarioPlacementKey1 = br.ReadInt16();
                    ScenarioPlacementStep = br.ReadInt16();
                    Unk06 = br.ReadInt16();
                    Unk08 = br.ReadInt32();
                    Unk0C = br.ReadInt32();
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteInt16(ScenarioPlacementKey0);
                    bw.WriteInt16(ScenarioPlacementKey1);
                    bw.WriteInt16(ScenarioPlacementStep);
                    bw.WriteInt16(Unk06);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                }

                // Layout: a ScenarioPlacementParam key (nightreign.exe 140FCD4C0, CSScenarioPlacementManagerImp). The row
                // whose u16 +4 / u16 +6 / byte +8 equal these three gives the enemy's charaInitParamId (row +14h).
                // -1 in any of them = no key. MSB Research.md §5.4.

                /// <summary>
                /// Matches ScenarioPlacementParam u16 at +4 (retail 4-52). -1 = none.
                /// </summary>
                public short ScenarioPlacementKey0 { get; set; } = -1;

                /// <summary>
                /// Matches ScenarioPlacementParam u16 at +6 (retail 0-10, 51, 52). -1 = none.
                /// </summary>
                public short ScenarioPlacementKey1 { get; set; } = -1;

                /// <summary>
                /// Matches ScenarioPlacementParam byte at +8 (retail 1-5). -1 = none.
                /// </summary>
                public short ScenarioPlacementStep { get; set; } = -1;
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
                private int Unk00 { get; set; } = 0; // Hidden: not read, 0 in retail
                private int Unk04 { get; set; } = 0; // Hidden: not read, 0 in retail

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
                    Unk78Data = new EnemyUnk78Struct();
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
                    enemy.Unk78Data = Unk78Data.DeepCopy();
                    enemy.CondemnedSpEffectSetParamIds = (int[])CondemnedSpEffectSetParamIds.Clone();
                    enemy.SpEffectSetParamIds = (int[])SpEffectSetParamIds.Clone();
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
                    OverrideItemLotEventFlagID0 = br.ReadInt32();
                    OverrideItemLotParamID0 = br.ReadInt32();
                    OverrideItemLotEventFlagID1 = br.ReadInt32();
                    OverrideItemLotParamID1 = br.ReadInt32();
                    AltNpcParamId = br.ReadInt32();
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

                    bw.WriteInt32(OverrideItemLotEventFlagID0);
                    bw.WriteInt32(OverrideItemLotParamID0);
                    bw.WriteInt32(OverrideItemLotEventFlagID1);
                    bw.WriteInt32(OverrideItemLotParamID1);
                    bw.WriteInt32(AltNpcParamId);
                    bw.WriteInt32(Unk64);
                    bw.WriteInt32(Unk68);
                    bw.WriteInt32(Unk6C);

                    bw.WriteInt64(Offset70);

                    bw.ReserveInt64("Offset78");
                    bw.FillInt64("Offset78", bw.Position - start);
                    Unk78Data.Write(bw);
                }

                // Internal: the loader fixes up this pointer, but no field of the struct was seen read and every retail
                // value is the default (0, 1, then five (-1, -1, 10) entries)
                private EnemyUnk78Struct Unk78Data { get; set; }

                // Layout
                private int Unk00 { get; set; } = -1; // Hidden: read (140E35A20; +04 is used instead when the character's alternate flag is set), -1 in retail
                private int Unk04 { get; set; } = -1; // Hidden: read (140E35A50), -1 in retail
                public int NpcThinkParamId { get; set; } = 0;
                public int NpcParamId { get; set; } = 0;
                public int TalkID { get; set; } = 0;
                private byte Unk14 { get; set; } = 0; // Hidden: read for parts param version 0x44 and later (140E359E0), 0 in retail

                /// <summary>
                /// Unknown. Read with BackupEventAnimID when the character is set up (getter 140E35DA0, caller 14042A650).
                /// 1 on 230 retail enemies and 22 dummy enemies. ER UnkT15.
                /// </summary>
                public byte Unk15 { get; set; } = 0; // Boolean
                public short PlatoonId { get; set; } = 0;
                public int CharaInitParamId { get; set; } = -1;
                private int HitPartIndex { get; set; } = -1;
                private short PatrolRouteIndex { get; set; }

                /// <summary>
                /// A ScenarioPlacementParam row ID (retail 101-309). Read by getter 140E35D30, which has no direct callers;
                /// the scenario key in UnkA8Data is what selects the row in the traced code.
                /// </summary>
                public short ScenarioPlacementParamID { get; set; } = -1;
                private int Unk24 { get; set; } = -1; // Hidden: not read, -1 in retail

                /// <summary>
                /// A second set of SpEffectSetParam IDs: CS::MapStudioSpEffectEquipCtrl gives slots 16-31 from these only
                /// when the character's alternate flag (+1F9h bit 2) is set. MSB Research.md §5.4.
                /// </summary>
                public int[] CondemnedSpEffectSetParamIds { get; set; } = new int[4];
                public int BackupEventAnimID { get; set; } = -1;

                /// <summary>
                /// Unknown. Read by the loading screen (CS::LoadingScreen#2, 1409DCF8F). 1 on 298 retail enemies, 2 on 10.
                /// </summary>
                public sbyte Unk3C { get; set; } = -1;
                private byte Unk3D { get; set; } = 0; // Hidden: not read, 0 in retail

                /// <summary>
                /// Unknown. Read by the ChrIns constructor (1403C687A) and 1404D3FAE. -1, or 0 to 575 (often multiples of 10).
                /// </summary>
                public short Unk3E { get; set; } = -1;
                public int[] SpEffectSetParamIds { get; set; } = new int[4];

                /// <summary>
                /// CS::CSMsbPartsEne::CheckGetOverridePrivateItemLotId (140E356F5): the first pair whose event flag is on
                /// gives OverrideItemLotParamID instead of the normal drop. Same as Event.TeamFight. Retail: 20 boss
                /// enemies in m47-m49, e.g. 47800300 -> 6850000, 47800301 -> 6851000. Was Unk50.
                /// </summary>
                public int OverrideItemLotEventFlagID0 { get; set; } = 0;

                /// <summary>
                /// The item lot of OverrideItemLotEventFlagID0. -1 or 0 for none. Was Unk54.
                /// </summary>
                public int OverrideItemLotParamID0 { get; set; } = -1;

                /// <summary>
                /// Second pair of the item lot override (see OverrideItemLotEventFlagID0). Was Unk58.
                /// </summary>
                public int OverrideItemLotEventFlagID1 { get; set; } = 0;

                /// <summary>
                /// The item lot of OverrideItemLotEventFlagID1. Was Unk5C.
                /// </summary>
                public int OverrideItemLotParamID1 { get; set; } = -1;

                /// <summary>
                /// NpcParam row used in place of NpcParamId when non-zero and a character creation flag is set (callers
                /// 1403E6138, 1403E6567, 140F7AA4D; the flag's meaning wasn't traced). 0 for none. Retail: 78 enemies, all in the
                /// m47-m49 boss maps, equal to NpcParamId or a variant of it (78100010 -> 78100020). Was Unk60.
                /// </summary>
                public int AltNpcParamId { get; set; } = 0;
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
                    public EnemyUnk78Struct()
                    {
                        InnerStruct1 = new EnemyInnerStruct();
                        InnerStruct2 = new EnemyInnerStruct();
                        InnerStruct3 = new EnemyInnerStruct();
                        InnerStruct4 = new EnemyInnerStruct();
                        InnerStruct5 = new EnemyInnerStruct();
                    }

                    public EnemyUnk78Struct DeepCopy()
                    {
                        var copy = (EnemyUnk78Struct)MemberwiseClone();
                        copy.InnerStruct1 = InnerStruct1.DeepCopy();
                        copy.InnerStruct2 = InnerStruct2.DeepCopy();
                        copy.InnerStruct3 = InnerStruct3.DeepCopy();
                        copy.InnerStruct4 = InnerStruct4.DeepCopy();
                        copy.InnerStruct5 = InnerStruct5.DeepCopy();
                        return copy;
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
                private int Unk00 { get; set; } = 0; // Hidden: not read by nightreign.exe (the Player unit only reads the display groups); 0-9 in retail
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
                    OutdoorIndoorType = br.ReadSByte();
                    EasyOcclusionID = br.ReadByte();
                    Unk04 = br.ReadSingle();
                    Unk08 = br.ReadInt32();
                    Unk0C = br.ReadInt32();
                    Unk10 = br.ReadInt32();
                    PlayerLightScale = br.ReadSingle();
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
                    GparamOverride = br.ReadSByte();
                    Unk36 = br.ReadInt16();
                    Unk38 = br.ReadInt32();
                    Unk3C = br.ReadInt16();
                    Unk3E = br.ReadInt16();
                    Unk40 = br.ReadInt32();
                    HitUnk44 = br.ReadInt32();
                    Unk48 = br.ReadInt32();
                    UnkT4C = br.ReadByte();
                    TimezonePlayerLightScaleOverride = br.ReadByte();
                    OpenChrActivateThreshold = br.ReadInt16();
                }
                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteByte(HitFilterID);
                    bw.WriteSByte(Unk01);
                    bw.WriteSByte(OutdoorIndoorType);
                    bw.WriteByte(EasyOcclusionID);
                    bw.WriteSingle(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                    bw.WriteInt32(Unk10);
                    bw.WriteSingle(PlayerLightScale);
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
                    bw.WriteSByte(GparamOverride);
                    bw.WriteInt16(Unk36);
                    bw.WriteInt32(Unk38);
                    bw.WriteInt16(Unk3C);
                    bw.WriteInt16(Unk3E);
                    bw.WriteInt32(Unk40);
                    bw.WriteInt32(HitUnk44);
                    bw.WriteInt32(Unk48);
                    bw.WriteByte(UnkT4C);
                    bw.WriteByte(TimezonePlayerLightScaleOverride);
                    bw.WriteInt16(OpenChrActivateThreshold);
                }

                // Layout
                public byte HitFilterID { get; set; } = 8;
                private sbyte Unk01 { get; set; } = -1; // Hidden
                /// <summary>Sound state while standing here: 0 Outdoor, 1 IndoorHalf, 2 IndoorAll, -1 default. Same as MSBE.</summary>
                public sbyte OutdoorIndoorType { get; set; } = -1;
                /// <summary>Wwise EasyOcclusionID state while standing here. Same as MSBE.</summary>
                public byte EasyOcclusionID { get; set; } = 0;
                /// <summary>
                /// Unknown float. Read through an asset's fallback getter (140E38190, the asset's GparamSettings.GparamSettings_Unk04).
                /// 0 on 647 retail collisions, else 64.81, 92.33, 100.98 or 2.4.
                /// </summary>
                public float Unk04 { get; set; } = 0;
                private int Unk08 { get; set; } = 0; // Hidden: ER MapGridCreateHeightDetailLimitInfoID; no NR reader found, 0 in retail
                private int Unk0C { get; set; } = 0; // Hidden
                private int Unk10 { get; set; } = 0; // Hidden
                /// <summary>Overrides MapDefaultInfoParam PlayerLigntScale while standing here (>= 0 wins). Same as MSBE.</summary>
                public float PlayerLightScale { get; set; } = -1;
                public int LocationTextID { get; set; } = -1;

                /// <summary>
                /// Unknown. Read for collisions (140E39450); an asset's CollisionSettings.CollisionSettings_Unk28 falls back to it. ER UnkT1C (a
                /// second place-name text ID). -1 in retail except one collision (0).
                /// </summary>
                public int Unk1C { get; set; } = -1;
                public int PlayRegionID { get; set; } = -1;
                private short Unk24 { get; set; } = -1; // Hidden: not read, -1 in retail

                /// <summary>
                /// Unknown. Read through an asset's fallback getter (140E36C50, the asset's CollisionSettings.CollisionSettings_Unk04). 1 on 39
                /// retail collisions.
                /// </summary>
                public byte Unk26 { get; set; } = 0; // Boolean
                private byte Unk27 { get; set; } = 0; // Hidden: not read, 1 on two retail collisions
                private int Unk28 { get; set; } = 0; // Hidden
                private int Unk2C { get; set; } = -1; // Hidden
                private int Unk30 { get; set; } = -1; // Hidden: read via the asset fallback (140E38AC0), -1 in retail

                /// <summary>
                /// Unknown. Read for collisions (140E39280); an asset's GparamSettings.GparamSettings_Unk0A falls back to it. 0 or 2 in retail.
                /// </summary>
                public byte Unk34 { get; set; } = 0;

                /// <summary>
                /// The gparam sub-ID override (ER: mXX_XX_AABB, replaces BB). An asset's GparamSettings.GparamOverride falls
                /// back to it (140E389C0). -1 for none; 0 on 9 retail collisions. Was Unk35.
                /// </summary>
                public sbyte GparamOverride { get; set; } = -1;
                private short Unk36 { get; set; } = 0; // Hidden: bytes read via the asset fallback (+36 140E36B50: ER DisableTorrent, +37 140E37430), 0 in retail
                private int Unk38 { get; set; } = -1; // Hidden: ER LegacyPlayRegionID, only for parts param versions before 0x33

                /// <summary>
                /// Unknown. With the hidden short after it, a pair the asset fallback getter reads by index (140E37070, the
                /// asset's CollisionSettings.CollisionSettings_Unk14). -1 in retail except two collisions (10000).
                /// </summary>
                public short Unk3C { get; set; } = -1;
                private short Unk3E { get; set; } = -1; // Hidden: index 1 of the Unk3C pair, -1 in retail
                private int Unk40 { get; set; } = 0; // Hidden
                private int HitUnk44 { get; set; } = 0; // Hidden
                private int Unk48 { get; set; } = 0; // Hidden: ER EnableFastTravelEventFlagID, read via the asset fallback (140E378E0), 0 in retail

                /// <summary>
                /// Unknown. Read for collisions (140E39200); an asset's GparamSettings.GparamSettings_Unk0E falls back to it. ER UnkT4C. 0 in retail.
                /// </summary>
                public byte UnkT4C { get; set; } = 0;

                /// <summary>1 on, 2 off, else the param decides (time-zone player light scale). Same as MSBE.</summary>
                public byte TimezonePlayerLightScaleOverride { get; set; } = 0;

                /// <summary>Overrides the open-character activate threshold while standing here (>= 0 wins). Same as MSBE.</summary>
                public short OpenChrActivateThreshold { get; set; } = -1;

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

                /// <summary>
                /// The map loaded while standing on the parent collision (bytes AA BB CC DD; -1 counts as 0).
                /// </summary>
                public sbyte[] MapID { get; set; } = new sbyte[4];
                private short Unk08 { get; set; } = 0; // Hidden: byte +8 is read with MapID (14063E7D0; ER: open-world load level), 0 in retail
                private sbyte Unk0A { get; set; } = -1; // Hidden: ER's world-state gate (UnkT0A); no NR reader found (-1 x63, 0 x8)
                private sbyte Unk0B { get; set; } = 0; // Hidden: ER's gate invert (UnkT0B); no NR reader found (1 x6)
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

                    CollisionSettings = new AssetCollisionSettings();
                    InnerStruct70 = new AssetUnkStruct70();
                    GparamSettings = new AssetGparamSettings();
                    SoundSettings = new AssetSoundSettings();
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

                    asset.CollisionSettings = CollisionSettings.DeepCopy();
                    asset.InnerStruct70 = InnerStruct70.DeepCopy();
                    asset.GparamSettings = GparamSettings.DeepCopy();
                    asset.SoundSettings = SoundSettings.DeepCopy();
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
                    CollisionTypeOverride = br.ReadByte();
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
                    PickUpItemLotParamID = br.ReadInt32();
                    BreakItemLotParamID = br.ReadInt32();
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
                    CollisionSettings = new AssetCollisionSettings(br);

                    br.Position = start + Offset70;
                    InnerStruct70 = new AssetUnkStruct70(br);

                    br.Position = start + Offset78;
                    GparamSettings = new AssetGparamSettings(br);

                    br.Position = start + Offset80;
                    SoundSettings = new AssetSoundSettings(br);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    var start = bw.Position;

                    bw.WriteByte(Unk00);
                    bw.WriteByte(CollisionTypeOverride);
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
                    bw.WriteInt32(PickUpItemLotParamID);
                    bw.WriteInt32(BreakItemLotParamID);
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
                    CollisionSettings.Write(bw);

                    bw.FillInt64("Offset70", bw.Position - start);
                    InnerStruct70.Write(bw);

                    bw.FillInt64("Offset78", bw.Position - start);
                    GparamSettings.Write(bw);

                    bw.FillInt64("Offset80", bw.Position - start);
                    SoundSettings.Write(bw);
                }

                // Internal. The asset's own copies of collision settings, as in MSBE: when a struct's override byte (+0) is
                // set the game uses the asset's field, otherwise the same field of the Collision in PartName5 (or PartName4)
                // (getters 140E36B50..140E38AC0, same shape and version gates as eldenring.exe). Were InnerStruct68/78/80.

                /// <summary>The asset's own collision settings (MSBE AssetCollisionSettings).</summary>
                public AssetCollisionSettings CollisionSettings { get; set; }

                /// <summary>Unknown struct (MSBE AssetUnkStruct2).</summary>
                public AssetUnkStruct70 InnerStruct70 { get; set; }

                /// <summary>The asset's own collision gparam / light settings (MSBE AssetGparamSettings).</summary>
                public AssetGparamSettings GparamSettings { get; set; }

                /// <summary>The asset's own collision sound settings (MSBE AssetSoundSettings).</summary>
                public AssetSoundSettings SoundSettings { get; set; }

                // Layout

                /// <summary>
                /// Unknown. Read as a byte (getter 140E37030). ER UnkT00. 1 on 215 retail assets.
                /// </summary>
                public byte Unk00 { get; set; } = 0; // Boolean
                /// <summary>11-16 replace the geometry param collisionType; other values keep it. Same as MSBE.</summary>
                public byte CollisionTypeOverride { get; set; } = 0;
                private short Unk02 { get; set; } = 0; // Hidden: byte +2 read (140E38FC0; ER UnkT02), 0 in retail

                /// <summary>
                /// Unknown. Read for assets (140E378C0, 140426D90). Retail: -1, or BreakItemLotParamID + 500 on most of the
                /// 366 assets that set it, so probably a second item lot.
                /// </summary>
                public int Unk04 { get; set; } = -1;
                private int Unk08 { get; set; } = 0; // Hidden: read as 8 bytes and tested (140E37750; ER UnkT08), 0 in retail
                private int Unk0C { get; set; } = 0; // Hidden

                /// <summary>
                /// Unknown. Read for assets (140E36A60). ER UnkT10: a condition on a flag of the owning object (0 always, 1
                /// when set, 2 when not set, 3 never). 1 on 66 retail assets, 3 on 2.
                /// </summary>
                public byte Unk10 { get; set; } = 0;

                /// <summary>
                /// Unknown. Read for assets (140E38FE0). ER uses this offset for breaking adjacent assets with this one. 1 on
                /// 68 retail assets.
                /// </summary>
                public byte Unk11 { get; set; } = 0; // Boolean
                private sbyte Unk12 { get; set; } = -1; // Hidden: not read, 1 on four retail assets
                private byte Unk13 { get; set; } = 0; // Hidden

                /// <summary>
                /// Unknown. Read with the next int (140E38990). 0 in retail except 15 assets (1, 6, 8, 23, 25).
                /// </summary>
                public int Unk14 { get; set; } = 0;
                private int Unk18 { get; set; } = 0; // Hidden
                public short AssetSfxParamRelativeID { get; set; } = -1;
                private short Unk1E { get; set; } = -1; // Hidden
                private int Unk20 { get; set; } = -1; // Hidden
                private int Unk24 { get; set; } = -1; // Hidden
                private int Unk28 { get; set; } = 0; // Hidden
                private int Unk2C { get; set; } = 0; // Hidden
                /// <summary>Item lot given when the asset is picked up. Same offset as MSBE.</summary>
                public int PickUpItemLotParamID { get; set; } = -1;
                /// <summary>Item lot given when the asset breaks. Same offset as MSBE. Was ItemLotParamMapID.</summary>
                public int BreakItemLotParamID { get; set; } = -1;
                public int PartIndex1 { get; set; } = -1;
                private int Unk3C { get; set; } = -1; // Hidden: not read for assets, -1 in retail
                public int PartIndex2 { get; set; } = -1;
                public int PartIndex3 { get; set; } = -1;
                public int PartIndex4 { get; set; } = -1;
                public int PartIndex5 { get; set; } = -1;
                private int Unk50 { get; set; } = -1; // Hidden: bytes +50 and +53 read (140E37050, 140E385F0; ER UnkT50 / UnkT53), 0 in retail
                public int PartIndex6 { get; set; } = -1;

                /// <summary>
                /// Unknown; the first of an int array the game reads by index (140E38930). Smithbox's Map Animation tool reads
                /// it as the asset's pose. -1 on most retail assets, else 0 to 42.
                /// </summary>
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

                public class AssetCollisionSettings
                {
                    public AssetCollisionSettings() { }

                    public AssetCollisionSettings DeepCopy()
                    {
                        return (AssetCollisionSettings)MemberwiseClone();
                    }

                    internal AssetCollisionSettings(BinaryReaderEx br)
                    {
                        OverrideCollisionSettings = br.ReadByte();
                        Unk01 = br.ReadByte();
                        Unk02 = br.ReadInt16();
                        CollisionSettings_Unk04 = br.ReadByte();
                        Unk05 = br.ReadByte();
                        Unk06 = br.ReadInt16();
                        Unk08 = br.ReadInt32();
                        Unk0C = br.ReadInt32();
                        Unk10 = br.ReadInt32();
                        CollisionSettings_Unk14 = br.ReadInt16();
                        Unk16 = br.ReadInt16();
                        Unk18 = br.ReadInt32();
                        Unk1C = br.ReadInt32();
                        Unk20 = br.ReadInt32();
                        LocationTextID = br.ReadInt32();
                        CollisionSettings_Unk28 = br.ReadInt32();
                        PlayRegionID = br.ReadInt32();
                        Unk30 = br.ReadInt32();
                        Unk34 = br.ReadInt32();
                        Unk38 = br.ReadInt32();
                        Unk3C = br.ReadInt32();
                    }

                    internal void Write(BinaryWriterEx bw)
                    {
                        bw.WriteByte(OverrideCollisionSettings);
                        bw.WriteByte(Unk01);
                        bw.WriteInt16(Unk02);
                        bw.WriteByte(CollisionSettings_Unk04);
                        bw.WriteByte(Unk05);
                        bw.WriteInt16(Unk06);
                        bw.WriteInt32(Unk08);
                        bw.WriteInt32(Unk0C);
                        bw.WriteInt32(Unk10);
                        bw.WriteInt16(CollisionSettings_Unk14);
                        bw.WriteInt16(Unk16);
                        bw.WriteInt32(Unk18);
                        bw.WriteInt32(Unk1C);
                        bw.WriteInt32(Unk20);
                        bw.WriteInt32(LocationTextID);
                        bw.WriteInt32(CollisionSettings_Unk28);
                        bw.WriteInt32(PlayRegionID);
                        bw.WriteInt32(Unk30);
                        bw.WriteInt32(Unk34);
                        bw.WriteInt32(Unk38);
                        bw.WriteInt32(Unk3C);
                    }

                    // Layout. Each field names the Collision field it replaces.

                    /// <summary>
                    /// If set, the fields of this struct are used instead of those of the asset's collision. Was the short Unk00.
                    /// 1 on 1,773 retail assets.
                    /// </summary>
                    public byte OverrideCollisionSettings { get; set; } = 0; // Bool
                    private byte Unk01 { get; set; } = 0; // Hidden: not read
                    private short Unk02 { get; set; } = -1; // Hidden: not read

                    /// <summary>
                    /// The asset's own Collision Unk26 (140E36C50). Was the low byte of the short Unk04.
                    /// </summary>
                    public byte CollisionSettings_Unk04 { get; set; } = 0; // Bool
                    private byte Unk05 { get; set; } = 0; // Hidden: the asset's own Collision +36 (ER DisableTorrent), 0 in retail
                    private short Unk06 { get; set; } = -1; // Hidden: not read (ER OpenChrActivateThreshold)
                    private int Unk08 { get; set; } = 0; // Hidden
                    private int Unk0C { get; set; } = 0; // Hidden: byte read, the asset's own Collision +37, 0 in retail
                    private int Unk10 { get; set; } = -1; // Hidden: the asset's own Collision LegacyPlayRegionID (+38), versions before 0x33

                    /// <summary>
                    /// The asset's own Collision Unk3C (index 0 of the pair read by 140E37070). Was the low half of the int Unk14.
                    /// -1 in retail.
                    /// </summary>
                    public short CollisionSettings_Unk14 { get; set; } = -1;
                    private short Unk16 { get; set; } = -1; // Hidden: index 1, the asset's own Collision +3E
                    private int Unk18 { get; set; } = -1; // Hidden: not read
                    private int Unk1C { get; set; } = -1; // Hidden: the asset's own Collision +30, -1 in retail
                    private int Unk20 { get; set; } = 0; // Hidden: the asset's own Collision +48 (ER EnableFastTravelEventFlagID), 0 in retail

                    /// <summary>
                    /// The asset's own Collision LocationTextID (140E37310). Was Unk24. -1 in retail.
                    /// </summary>
                    public int LocationTextID { get; set; } = -1;

                    /// <summary>
                    /// The asset's own Collision Unk1C (140E38290). -1 in retail.
                    /// </summary>
                    public int CollisionSettings_Unk28 { get; set; } = -1;

                    /// <summary>
                    /// The asset's own Collision PlayRegionID (140E37590). Was Unk2C. -1 in retail.
                    /// </summary>
                    public int PlayRegionID { get; set; } = -1;
                    private int Unk30 { get; set; } = 0; // Hidden
                    private int Unk34 { get; set; } = 0; // Hidden
                    private int Unk38 { get; set; } = 0; // Hidden
                    private int Unk3C { get; set; } = 0; // Hidden
                }

                public class AssetUnkStruct70
                {
                    public AssetUnkStruct70() { }

                    public AssetUnkStruct70 DeepCopy()
                    {
                        return (AssetUnkStruct70)MemberwiseClone();
                    }

                    internal AssetUnkStruct70(BinaryReaderEx br)
                    {
                        Unk00 = br.ReadInt32();
                        UnkStruct70_Unk04 = br.ReadInt32();
                        Unk08 = br.ReadInt32();
                        Unk0C = br.ReadInt32();
                        Unk10 = br.ReadInt32();
                        UnkStruct70_Unk14 = br.ReadSingle();
                        Unk18 = br.ReadInt32();
                        UnkStruct70_Unk1C = br.ReadSByte();
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
                        bw.WriteInt32(UnkStruct70_Unk04);
                        bw.WriteInt32(Unk08);
                        bw.WriteInt32(Unk0C);
                        bw.WriteInt32(Unk10);
                        bw.WriteSingle(UnkStruct70_Unk14);
                        bw.WriteInt32(Unk18);
                        bw.WriteSByte(UnkStruct70_Unk1C);
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
                    private int Unk00 { get; set; } = 0; // Hidden: byte +0 read as a switch (140E38040, 140E38410), 0 in retail

                    /// <summary>
                    /// Unknown. Read for assets (140E38040). -1, or 100 on 5,438 retail assets.
                    /// </summary>
                    public int UnkStruct70_Unk04 { get; set; } = -1;
                    private int Unk08 { get; set; } = -1; // Hidden: read and tested (140E38040), -1 in retail
                    private int Unk0C { get; set; } = 0; // Hidden
                    private int Unk10 { get; set; } = 0; // Hidden

                    /// <summary>
                    /// Unknown float. Read by 140CDEF46. -1 in retail.
                    /// </summary>
                    public float UnkStruct70_Unk14 { get; set; } = 0;
                    private int Unk18 { get; set; } = 0; // Hidden: not read, 0 in retail

                    /// <summary>
                    /// Unknown. Read for assets (140E38410). -1, or 4 on 2,775 retail assets.
                    /// </summary>
                    public sbyte UnkStruct70_Unk1C { get; set; } = -1;
                    private sbyte Unk1D { get; set; } = -1; // Hidden: not read, -1 in retail
                    private short Unk1E { get; set; } = -1; // Hidden: not read, -1 in retail
                    private int Unk20 { get; set; } = 0; // Hidden
                    private int Unk24 { get; set; } = 0; // Hidden
                    private int Unk28 { get; set; } = 0; // Hidden
                    private int Unk2C { get; set; } = 0; // Hidden
                    private int Unk30 { get; set; } = 0; // Hidden
                    private int Unk34 { get; set; } = 0; // Hidden
                    private int Unk38 { get; set; } = 0; // Hidden
                    private int Unk3C { get; set; } = 0; // Hidden
                }

                public class AssetGparamSettings
                {
                    public AssetGparamSettings() { }

                    public AssetGparamSettings DeepCopy()
                    {
                        var unks3 = (AssetGparamSettings)MemberwiseClone();
                        return unks3;
                    }

                    internal AssetGparamSettings(BinaryReaderEx br)
                    {
                        OverrideCollisionGparamSettings = br.ReadByte();
                        Unk01 = br.ReadByte();
                        Unk02 = br.ReadInt16();
                        GparamSettings_Unk04 = br.ReadSingle();
                        Unk08 = br.ReadSByte();
                        GparamOverride = br.ReadSByte();
                        GparamSettings_Unk0A = br.ReadByte();
                        GparamSettings_Unk0B = br.ReadSByte();
                        GparamSettings_Unk0C = br.ReadInt16();
                        GparamSettings_Unk0E = br.ReadByte();
                        TimezonePlayerLightScaleOverride = br.ReadByte();
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
                        bw.WriteByte(OverrideCollisionGparamSettings);
                        bw.WriteByte(Unk01);
                        bw.WriteInt16(Unk02);
                        bw.WriteSingle(GparamSettings_Unk04);
                        bw.WriteSByte(Unk08);
                        bw.WriteSByte(GparamOverride);
                        bw.WriteByte(GparamSettings_Unk0A);
                        bw.WriteSByte(GparamSettings_Unk0B);
                        bw.WriteInt16(GparamSettings_Unk0C);
                        bw.WriteByte(GparamSettings_Unk0E);
                        bw.WriteByte(TimezonePlayerLightScaleOverride);
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

                    // Layout. Each field names the Collision field it replaces.

                    /// <summary>
                    /// If set, the fields of this struct are used instead of those of the asset's collision. Was the int Unk00.
                    /// 1 on 107 retail assets.
                    /// </summary>
                    public byte OverrideCollisionGparamSettings { get; set; } = 0; // Bool
                    private byte Unk01 { get; set; } = 0; // Hidden: not read
                    private short Unk02 { get; set; } = 0; // Hidden: not read

                    /// <summary>
                    /// The asset's own Collision Unk04 (140E38190). Retail values match the collisions' (64.81, 92.33, 100.98, 2.4).
                    /// </summary>
                    public float GparamSettings_Unk04 { get; set; } = 0;
                    private sbyte Unk08 { get; set; } = -1; // Hidden: the asset's own scene gparam Unk1C, parts param versions before 0x31 only

                    /// <summary>
                    /// The asset's own Collision GparamOverride (140E389C0). Was the high byte of the short Unk08. -1 for none.
                    /// </summary>
                    public sbyte GparamOverride { get; set; } = -1;

                    /// <summary>
                    /// The asset's own Collision Unk34 (140E36F30). 0 or 2 in retail.
                    /// </summary>
                    public byte GparamSettings_Unk0A { get; set; } = 0;

                    /// <summary>
                    /// The asset's own scene gparam Unk1D of the collision (140E36D50, parts param version 0x41 and later).
                    /// </summary>
                    public sbyte GparamSettings_Unk0B { get; set; } = -1;

                    /// <summary>
                    /// The asset's own scene gparam Unk20 of the collision (140E37190). -1, 30 or 0 in retail.
                    /// </summary>
                    public short GparamSettings_Unk0C { get; set; } = -1;

                    /// <summary>
                    /// The asset's own Collision UnkT4C (140E36E50). Was the low byte of the short Unk0E. 0 in retail.
                    /// </summary>
                    public byte GparamSettings_Unk0E { get; set; } = 0;

                    /// <summary>
                    /// The asset's own Collision TimezonePlayerLightScaleOverride (140E37B40): 1 on, 2 off, 0 keeps the map's
                    /// value. Was the high byte of the short Unk0E. 0 in retail.
                    /// </summary>
                    public byte TimezonePlayerLightScaleOverride { get; set; } = 0;
                    private float Unk10 { get; set; } = 0; // Hidden: ER's asset PlayerLightScale; not seen read in NR, -1 in retail
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
                /// The asset's own collision sound settings (MSBE AssetSoundSettings).
                /// </summary>
                public class AssetSoundSettings
                {
                    public AssetSoundSettings() { }

                    public AssetSoundSettings DeepCopy()
                    {
                        return (AssetSoundSettings)MemberwiseClone();
                    }

                    internal AssetSoundSettings(BinaryReaderEx br)
                    {
                        OverrideCollisionSoundSettings = br.ReadByte();
                        Unk01 = br.ReadSByte();
                        OutdoorIndoorType = br.ReadSByte();
                        EasyOcclusionID = br.ReadByte();
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
                        bw.WriteByte(OverrideCollisionSoundSettings);
                        bw.WriteSByte(Unk01);
                        bw.WriteSByte(OutdoorIndoorType);
                        bw.WriteByte(EasyOcclusionID);
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

                    // Layout. Each field names the Collision field it replaces.

                    /// <summary>
                    /// If set, the fields of this struct are used instead of those of the asset's collision. Was Unk00. 1 on
                    /// 414 retail assets.
                    /// </summary>
                    public byte OverrideCollisionSoundSettings { get; set; } = 0; // Bool
                    private sbyte Unk01 { get; set; } = -1; // Hidden: not read

                    /// <summary>
                    /// The asset's own Collision OutdoorIndoorType (140E38630, parts param version 0x32 and later). Was Unk02.
                    /// </summary>
                    public sbyte OutdoorIndoorType { get; set; } = -1;

                    /// <summary>
                    /// The asset's own Collision EasyOcclusionID (140E38730, parts param version 0x36 and later). Was the hidden
                    /// Unk03. 0 in retail.
                    /// </summary>
                    public byte EasyOcclusionID { get; set; } = 0;
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
