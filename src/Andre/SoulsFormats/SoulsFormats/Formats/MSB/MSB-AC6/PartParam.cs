using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace SoulsFormats
{
    public partial class MSB_AC6
    {
        internal enum PartType : int
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
            DummyAsset = 9,
            DummyEnemy = 10,
            ConnectCollision = 11,
            Invalid = 12,
            Asset = 13
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
            /// Unknown.
            /// </summary>
            public List<Part.Object> Objects { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Part.Item> Items { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Part.NPCWander> NPCWanders { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Part.Protoboss> Protobosses { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Part.Navmesh> Navmeshes { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Part.Invalid> Invalids { get; set; }

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

                Objects = new List<Part.Object>();
                Items = new List<Part.Item>();
                NPCWanders = new List<Part.NPCWander>();
                Protobosses = new List<Part.Protoboss>();
                Navmeshes = new List<Part.Navmesh>();
                Invalids = new List<Part.Invalid>();
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
                    case Part.Object p:
                        Objects.Add(p);
                        break;
                    case Part.Item p:
                        Items.Add(p);
                        break;
                    case Part.NPCWander p:
                        NPCWanders.Add(p);
                        break;
                    case Part.Protoboss p:
                        Protobosses.Add(p);
                        break;
                    case Part.Navmesh p:
                        Navmeshes.Add(p);
                        break;
                    case Part.Invalid p:
                        Invalids.Add(p);
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
                    MapPieces, Enemies, Players, Collisions,
                    DummyAssets, DummyEnemies, ConnectCollisions, Assets,
                    Objects, Items, NPCWanders, Protobosses, Navmeshes, Invalids);
            }
            IReadOnlyList<IMsbPart> IMsbParam<IMsbPart>.GetEntries() => GetEntries();

            internal override Part ReadEntry(BinaryReaderEx br, int Version)
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
                        return Assets.EchoAdd(new Part.Asset(br));

                    case PartType.Object:
                        return Objects.EchoAdd(new Part.Object(br));

                    case PartType.Item:
                        return Items.EchoAdd(new Part.Item(br));

                    case PartType.NPCWander:
                        return NPCWanders.EchoAdd(new Part.NPCWander(br));

                    case PartType.Protoboss:
                        return Protobosses.EchoAdd(new Part.Protoboss(br));

                    case PartType.Navmesh:
                        return Navmeshes.EchoAdd(new Part.Navmesh(br));

                    case PartType.Invalid:
                        return Invalids.EchoAdd(new Part.Invalid(br));

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
            private protected abstract PartType Type { get; }
            private protected abstract bool HasUnkOffsetT50 { get; }
            private protected abstract bool HasUnkOffsetT58 { get; }
            private protected abstract bool HasUnkOffsetT70 { get; }
            private protected abstract bool HasUnkOffsetT78 { get; }
            private protected abstract bool HasUnkOffsetT80 { get; }
            private protected abstract bool HasUnkOffsetT88 { get; }
            private protected abstract bool HasUnkOffsetT90 { get; }
            private protected abstract bool HasUnkOffsetT98 { get; }
            private protected abstract bool HasUnkOffsetTA0 { get; }

            /// <summary>
            /// The model used by this part; requires an entry in ModelParam.
            /// </summary>
            public string ModelName { get; set; }
            private int ModelIndex;

            
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
            /// Unknown.
            /// </summary>
            public int UnkT44 { get; set; }

            /// <summary>
            /// Identifies the part in event scripts.
            /// </summary>
            public uint EntityID { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int UnkE04 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int UnkE08 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte UnkE0C { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte UnkE0D { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte UnkE0E { get; set; }

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
            public byte UnkE11 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public short UnkE12 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte UnkE14 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte UnkE15 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte UnkE16 { get; set; }

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
            public uint[] EntityGroupIDs { get; private set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public short UnkE3C { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte UnkE3E { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte UnkE3F { get; set; }

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

                br.ReadInt32(); // PartType
                //br.AssertInt32((int)Type); // PartType

                br.ReadInt32(); // localIndex
                ModelIndex = br.ReadInt32();
                br.AssertInt32(0); // unk14
                long sibOffset = br.ReadInt64();
                Position = br.ReadVector3();
                Rotation = br.ReadVector3();
                Scale = br.ReadVector3();
                UnkT44 = br.ReadInt32(); // unk44
                br.AssertInt32(-1); // unk48
                br.AssertInt32(1); // unk4C
                long unkOffsetT50 = br.ReadInt64();
                long unkOffsetT58 = br.ReadInt64();
                long entityDataOffset = br.ReadInt64();
                long typeDataOffset = br.ReadInt64();
                long unkOffsetT70 = br.ReadInt64();
                long unkOffsetT78 = br.ReadInt64();
                long unkOffsetT80 = br.ReadInt64();
                long unkOffsetT88 = br.ReadInt64();
                long unkOffsetT90 = br.ReadInt64();
                long unkOffsetT98 = br.ReadInt64();
                long unkOffsetTA0 = br.ReadInt64();
                br.AssertInt64(0); // unkA8
                br.AssertInt64(0); // unkB0
                br.AssertInt64(0); // unkB8

                if (nameOffset == 0)
                    throw new InvalidDataException($"{nameof(nameOffset)} must not be 0 in type {GetType()}.");

                if (sibOffset == 0)
                    throw new InvalidDataException($"{nameof(sibOffset)} must not be 0 in type {GetType()}.");

                if (unkOffsetT50 == 0)
                    throw new InvalidDataException($"Unexpected {nameof(unkOffsetT50)} 0x{unkOffsetT50:X} in type {GetType()}.");

                if (entityDataOffset == 0)
                    throw new InvalidDataException($"{nameof(entityDataOffset)} must not be 0 in type {GetType()}.");

                if (typeDataOffset == 0)
                    throw new InvalidDataException($"{nameof(typeDataOffset)} must not be 0 in type {GetType()}.");

                if (unkOffsetT88 == 0)
                    throw new InvalidDataException($"Unexpected {nameof(unkOffsetT88)} 0x{unkOffsetT88:X} in type {GetType()}.");

                if (unkOffsetT98 == 0)
                    throw new InvalidDataException($"Unexpected {nameof(unkOffsetT98)} 0x{unkOffsetT98:X} in type {GetType()}.");

                br.Position = start + nameOffset;
                Name = br.ReadUTF16();

                br.Position = start + sibOffset;
                SibPath = br.ReadUTF16();

                if (HasUnkOffsetT50)
                {
                    br.Position = start + unkOffsetT50;
                    ReadUnkOffsetT50(br);
                }

                if (HasUnkOffsetT58)
                {
                    br.Position = start + unkOffsetT58;
                    ReadUnkOffsetT58(br);
                }

                br.Position = start + entityDataOffset;
                ReadEntityData(br);

                br.Position = start + typeDataOffset;
                ReadTypeData(br);

                if (HasUnkOffsetT70)
                {
                    br.Position = start + unkOffsetT70;
                    ReadUnkOffsetT70(br);
                }

                if (HasUnkOffsetT78)
                {
                    br.Position = start + unkOffsetT78;
                    ReadUnkOffsetT78(br);
                }

                if (HasUnkOffsetT80)
                {
                    br.Position = start + unkOffsetT80;
                    ReadUnkOffsetT80(br);
                }

                if (HasUnkOffsetT88)
                {
                    br.Position = start + unkOffsetT88;
                    ReadUnkOffsetT88(br);
                }

                if (HasUnkOffsetT90)
                {
                    br.Position = start + unkOffsetT90;
                    ReadUnkOffsetT90(br);
                }

                if (HasUnkOffsetT98)
                {
                    br.Position = start + unkOffsetT98;
                    ReadUnkOffsetT98(br);
                }

                if (HasUnkOffsetTA0)
                {
                    br.Position = start + unkOffsetTA0;
                    ReadUnkOffsetTA0(br);
                }
            }

            private void ReadEntityData(BinaryReaderEx br)
            {
                EntityID = br.ReadUInt32();
                UnkE04 = br.ReadInt32();
                UnkE08 = br.ReadInt32();
                UnkE0C = br.ReadByte();
                UnkE0D = br.ReadByte();
                UnkE0E = br.ReadByte();
                UnkE0F = br.ReadByte();
                UnkE10 = br.ReadByte();
                UnkE11 = br.ReadByte();
                UnkE12 = br.ReadInt16();
                UnkE14 = br.ReadByte();
                UnkE15 = br.ReadByte();
                UnkE16 = br.ReadByte();
                UnkE17 = br.ReadByte();
                UnkE18 = br.ReadInt32();
                EntityGroupIDs = br.ReadUInt32s(8);
                UnkE3C = br.ReadInt16();
                UnkE3E = br.ReadByte();
                UnkE3F = br.ReadByte();
            }

            private protected abstract void ReadTypeData(BinaryReaderEx br);

            private protected virtual void ReadUnkOffsetT50(BinaryReaderEx br)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(ReadUnkOffsetT50)}.");

            private protected virtual void ReadUnkOffsetT58(BinaryReaderEx br)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(ReadUnkOffsetT58)}.");

            private protected virtual void ReadUnkOffsetT70(BinaryReaderEx br)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(ReadUnkOffsetT70)}.");

            private protected virtual void ReadUnkOffsetT78(BinaryReaderEx br)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(ReadUnkOffsetT78)}.");

            private protected virtual void ReadUnkOffsetT80(BinaryReaderEx br)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(ReadUnkOffsetT80)}.");

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
                bw.WriteInt32(id);
                bw.WriteInt32(ModelIndex);
                bw.WriteInt32(0); // unk14

                bw.ReserveInt64("SibOffset");

                bw.WriteVector3(Position);
                bw.WriteVector3(Rotation);
                bw.WriteVector3(Scale);
                bw.WriteInt32(UnkT44);
                bw.WriteInt32(-1); // unk48
                bw.WriteInt32(1); // unk4c

                bw.ReserveInt64("UnkOffsetT50");
                bw.ReserveInt64("UnkOffsetT58");
                bw.ReserveInt64("EntityDataOffset");
                bw.ReserveInt64("TypeDataOffset");
                bw.ReserveInt64("UnkOffsetT70");
                bw.ReserveInt64("UnkOffsetT78");
                bw.ReserveInt64("UnkOffsetT80");
                bw.ReserveInt64("UnkOffsetT88");
                bw.ReserveInt64("UnkOffsetT90");
                bw.ReserveInt64("UnkOffsetT98");
                bw.ReserveInt64("UnkOffsetTA0");

                bw.WriteInt64(0);
                bw.WriteInt64(0);
                bw.WriteInt64(0);

                // Name
                bw.FillInt64("NameOffset", bw.Position - start);
                bw.WriteUTF16(MSB.ReambiguateName(Name), true);

                // Layout
                bw.FillInt64("SibOffset", bw.Position - start);
                bw.WriteUTF16(SibPath, true);
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
                bw.FillInt64("EntityDataOffset", bw.Position - start);
                WriteEntityData(bw);

                // Type
                bw.FillInt64("TypeDataOffset", bw.Position - start);
                WriteTypeData(bw);

                // Struct70
                if (HasUnkOffsetT70)
                {
                    bw.FillInt64("UnkOffsetT70", bw.Position - start);
                    WriteUnkOffsetT70(bw);
                }
                else
                {
                    bw.FillInt64("UnkOffsetT70", 0);
                }

                // Struct78
                if (HasUnkOffsetT78)
                {
                    bw.FillInt64("UnkOffsetT78", bw.Position - start);
                    WriteUnkOffsetT78(bw);
                }
                else
                {
                    bw.FillInt64("UnkOffsetT78", 0);
                }

                // Struct80
                if (HasUnkOffsetT80)
                {
                    bw.FillInt64("UnkOffsetT80", bw.Position - start);
                    WriteUnkOffsetT80(bw);
                }
                else
                {
                    bw.FillInt64("UnkOffsetT80", 0);
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
                bw.WriteInt32(UnkE04);
                bw.WriteInt32(UnkE08);
                bw.WriteByte(UnkE0C);
                bw.WriteByte(UnkE0D);
                bw.WriteByte(UnkE0E);
                bw.WriteByte(UnkE0F);
                bw.WriteByte(UnkE10);
                bw.WriteByte(UnkE11);
                bw.WriteInt16(UnkE12);
                bw.WriteByte(UnkE14);
                bw.WriteByte(UnkE15);
                bw.WriteByte(UnkE16);
                bw.WriteByte(UnkE17);
                bw.WriteInt32(UnkE18);
                bw.WriteUInt32s(EntityGroupIDs);
                bw.WriteInt16(UnkE3C);
                bw.WriteByte(UnkE3E);
                bw.WriteByte(UnkE3F);
            }

            private protected abstract void WriteTypeData(BinaryWriterEx bw);

            private protected virtual void WriteUnkOffsetT50(BinaryWriterEx bw)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(WriteUnkOffsetT50)}.");

            private protected virtual void WriteUnkOffsetT58(BinaryWriterEx bw)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(WriteUnkOffsetT58)}.");

            private protected virtual void WriteUnkOffsetT70(BinaryWriterEx bw)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(WriteUnkOffsetT70)}.");

            private protected virtual void WriteUnkOffsetT78(BinaryWriterEx bw)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(WriteUnkOffsetT78)}.");

            private protected virtual void WriteUnkOffsetT80(BinaryWriterEx bw)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(WriteUnkOffsetT80)}.");

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
            public class UnkStruct50
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
                /// Creates an UnkStruct1 with default values.
                /// </summary>
                public UnkStruct50()
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
                public UnkStruct50 DeepCopy()
                {
                    var unk1 = (UnkStruct50)MemberwiseClone();
                    unk1.DisplayGroups = (uint[])DisplayGroups.Clone();
                    unk1.DrawGroups = (uint[])DrawGroups.Clone();
                    unk1.CollisionMask = (uint[])CollisionMask.Clone();
                    return unk1;
                }

                internal UnkStruct50(BinaryReaderEx br)
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
            public class UnkStruct58
            {
                /// <summary>
                /// Unknown.
                /// </summary>
                public int Condition { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public uint[] DispGroups { get; private set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short Unk24 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short Unk26 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk28 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk2C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk30 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk34 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk38 { get; set; }

                /// <summary>
                /// Creates an UnkStruct2 with default values.
                /// </summary>
                public UnkStruct58()
                {
                    DispGroups = new uint[8];
                }

                /// <summary>
                /// Creates a deep copy of the struct.
                /// </summary>
                public UnkStruct58 DeepCopy()
                {
                    var unk2 = (UnkStruct58)MemberwiseClone();
                    unk2.DispGroups = (uint[])DispGroups.Clone();
                    return unk2;
                }

                internal UnkStruct58(BinaryReaderEx br)
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
                }
            }

            /// <summary>
            /// Unknown. Is Gparam struct in Elden Ring.
            /// </summary>
            public class UnkStruct70
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
                /// Unknown.
                /// </summary>
                public int Unk08 { get; set; }

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
                /// Creates a UnkStruct70 with default values.
                /// </summary>
                public UnkStruct70() { }

                /// <summary>
                /// Creates a deep copy of UnkStruct70.
                /// </summary>
                public UnkStruct70 DeepCopy()
                {
                    return (UnkStruct70)MemberwiseClone();
                }

                internal UnkStruct70(BinaryReaderEx br)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadInt32();
                    Unk08 = br.ReadInt32();
                    Unk0C = br.ReadInt32();
                    Unk10 = br.ReadInt32();
                    Unk14 = br.ReadInt32();
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                    bw.WriteInt32(Unk10);
                    bw.WriteInt32(Unk14);
                }

                /// <summary>
                /// Returns the struct values as a string.
                /// </summary>
                public override string ToString()
                {
                    return $"{Unk00}, {Unk04}";
                }
            }

            /// <summary>
            /// Unknown; Is SceneGparam in Elden Ring.
            /// </summary>
            public class UnkStruct78
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
                /// Unknown.
                /// </summary>
                public int Unk08 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk0C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float TransitionTime { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk14 { get; set; }

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
                /// Unknown.
                /// </summary>
                public sbyte GparamSubID_Override3 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk28 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk2C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk30 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk34 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk38 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk3C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk40 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk44 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short Unk46 { get; set; }

                /// <summary>
                /// Creates a UnkStruct78 with default values.
                /// </summary>
                public UnkStruct78()
                {

                }

                /// <summary>
                /// Creates a deep copy of the struct.
                /// </summary>
                public UnkStruct78 DeepCopy()
                {
                    var config = (UnkStruct78)MemberwiseClone();
                    return config;
                }

                internal UnkStruct78(BinaryReaderEx br)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadInt32();
                    Unk00 = br.ReadInt32();
                    Unk0C = br.ReadInt32();
                    TransitionTime = br.ReadSingle();
                    Unk14 = br.ReadInt32();
                    GparamSubID_Base = br.ReadSByte(); 
                    GparamSubID_Override1 = br.ReadSByte(); 
                    GparamSubID_Override2 = br.ReadSByte(); 
                    GparamSubID_Override3 = br.ReadSByte();

                    Unk28 = br.ReadInt32();
                    Unk2C = br.ReadInt32();
                    Unk30 = br.ReadInt32();
                    Unk34 = br.ReadInt32();
                    Unk38 = br.ReadInt32();
                    Unk3C = br.ReadInt32();
                    Unk40 = br.ReadInt32();
                    Unk44 = br.ReadInt32();
                    Unk46 = br.ReadInt16();
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                    bw.WriteSingle(TransitionTime);
                    bw.WriteInt32(Unk14);
                    bw.WriteSByte(GparamSubID_Base);
                    bw.WriteSByte(GparamSubID_Override1);
                    bw.WriteSByte(GparamSubID_Override2);
                    bw.WriteSByte(GparamSubID_Override3);

                    bw.WriteInt32(Unk28);
                    bw.WriteInt32(Unk2C);
                    bw.WriteInt32(Unk30);
                    bw.WriteInt32(Unk34);
                    bw.WriteInt32(Unk38);
                    bw.WriteInt32(Unk3C);
                    bw.WriteInt32(Unk40);
                    bw.WriteInt32(Unk44);
                    bw.WriteInt16(Unk46);
                }
            }

            /// <summary>
            /// Unknown. 
            /// </summary>
            public class UnkStruct80
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
                /// Unknown.
                /// </summary>
                public int Unk08 { get; set; }

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
                /// Unknown.
                /// </summary>
                public int Unk18 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk1C { get; set; }

                /// <summary>
                /// Creates an UnkStruct80 with default values.
                /// </summary>
                public UnkStruct80() { }

                /// <summary>
                /// Creates a deep copy of the struct.
                /// </summary>
                public UnkStruct80 DeepCopy()
                {
                    return (UnkStruct80)MemberwiseClone();
                }

                internal UnkStruct80(BinaryReaderEx br)
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
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class UnkStruct88
            {
                /// <summary>
                /// Unknown.
                /// </summary>
                public bool Unk00 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public bool Unk01 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short Unk02 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk04 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk08 { get; set; }

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
                /// Unknown.
                /// </summary>
                public int Unk18 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk1C { get; set; }

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
                    Unk00 = br.ReadBoolean();
                    Unk01 = br.ReadBoolean();
                    Unk02 = br.ReadInt16();
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
                    bw.WriteBoolean(Unk00);
                    bw.WriteBoolean(Unk01);
                    bw.WriteInt16(Unk02);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                    bw.WriteInt32(Unk10);
                    bw.WriteInt32(Unk14);
                    bw.WriteInt32(Unk18);
                    bw.WriteInt32(Unk1C);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class UnkStruct90
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
                /// Unknown.
                /// </summary>
                public int Unk08 { get; set; }

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
                /// Unknown.
                /// </summary>
                public int Unk18 { get; set; }
                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk1C { get; set; }

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
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class UnkStruct98
            {
                /// <summary>
                /// Unknown.
                /// </summary>
                public byte[] MapID { get; private set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk04 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk08 { get; set; }

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
                /// Unknown.
                /// </summary>
                public int Unk18 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk1C { get; set; }

                /// <summary>
                /// Creates an UnkStruct7 with default values.
                /// </summary>
                public UnkStruct98()
                {
                    MapID = new byte[4];
                }

                /// <summary>
                /// Creates a deep copy of the struct.
                /// </summary>
                public UnkStruct98 DeepCopy()
                {
                    var unks10 = (UnkStruct98)MemberwiseClone();
                    unks10.MapID = (byte[])MapID.Clone();
                    return unks10;
                }

                internal UnkStruct98(BinaryReaderEx br)
                {
                    MapID = br.ReadBytes(4);
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
                    bw.WriteBytes(MapID);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                    bw.WriteInt32(Unk10);
                    bw.WriteInt32(Unk14);
                    bw.WriteInt32(Unk18);
                    bw.WriteInt32(Unk1C);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class UnkStructA0
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
                /// Unknown.
                /// </summary>
                public int Unk08 { get; set; }

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
                /// Unknown.
                /// </summary>
                public int Unk18 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk1C { get; set; }

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
            }

            /// <summary>
            /// Fixed visual geometry. Doesn't seem used much in ER?
            /// </summary>
            public class MapPiece : Part
            {
                private protected override PartType Type => PartType.MapPiece;
                private protected override bool HasUnkOffsetT50 => true;
                private protected override bool HasUnkOffsetT58 => false;
                private protected override bool HasUnkOffsetT70 => true;
                private protected override bool HasUnkOffsetT78 => false;
                private protected override bool HasUnkOffsetT80 => true;
                private protected override bool HasUnkOffsetT88 => true;
                private protected override bool HasUnkOffsetT90 => true;
                private protected override bool HasUnkOffsetT98 => true;
                private protected override bool HasUnkOffsetTA0 => true;

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct50 UnkStruct50 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct70 UnkStruct70 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct80 UnkStruct80 { get; set; }

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
                /// </summary>
                public int Unk00 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk04 { get; set; }

                /// <summary>
                /// Creates a MapPiece with default values.
                /// </summary>
                public MapPiece() : base("mXXXXXX_XXXX")
                {
                    UnkStruct50 = new UnkStruct50();
                    UnkStruct70 = new UnkStruct70();
                    UnkStruct80 = new UnkStruct80();
                    UnkStruct88 = new UnkStruct88();
                    UnkStruct90 = new UnkStruct90();
                    UnkStruct98 = new UnkStruct98();
                    UnkStructA0 = new UnkStructA0();
                }

                private protected override void DeepCopyTo(Part part)
                {
                    var piece = (MapPiece)part;
                    piece.UnkStruct50 = UnkStruct50.DeepCopy();
                    piece.UnkStruct70 = UnkStruct70.DeepCopy();
                    piece.UnkStruct80 = UnkStruct80.DeepCopy();
                    piece.UnkStruct88 = UnkStruct88.DeepCopy();
                    piece.UnkStruct90 = UnkStruct90.DeepCopy();
                    piece.UnkStruct98 = UnkStruct98.DeepCopy();
                    piece.UnkStructA0 = UnkStructA0.DeepCopy();
                }

                internal MapPiece(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadInt32();
                }

                private protected override void ReadUnkOffsetT50(BinaryReaderEx br) => UnkStruct50 = new UnkStruct50(br);
                private protected override void ReadUnkOffsetT70(BinaryReaderEx br) => UnkStruct70 = new UnkStruct70(br);
                private protected override void ReadUnkOffsetT80(BinaryReaderEx br) => UnkStruct80 = new UnkStruct80(br);
                private protected override void ReadUnkOffsetT88(BinaryReaderEx br) => UnkStruct88 = new UnkStruct88(br);
                private protected override void ReadUnkOffsetT90(BinaryReaderEx br) => UnkStruct90 = new UnkStruct90(br);
                private protected override void ReadUnkOffsetT98(BinaryReaderEx br) => UnkStruct98 = new UnkStruct98(br);
                private protected override void ReadUnkOffsetTA0(BinaryReaderEx br) => UnkStructA0 = new UnkStructA0(br);

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                }

                private protected override void WriteUnkOffsetT50(BinaryWriterEx bw) => UnkStruct50.Write(bw);
                private protected override void WriteUnkOffsetT70(BinaryWriterEx bw) => UnkStruct70.Write(bw);
                private protected override void WriteUnkOffsetT80(BinaryWriterEx bw) => UnkStruct80.Write(bw);
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
                private protected override bool HasUnkOffsetT70 => true;
                private protected override bool HasUnkOffsetT78 => false;
                private protected override bool HasUnkOffsetT80 => false;
                private protected override bool HasUnkOffsetT88 => true;
                private protected override bool HasUnkOffsetT90 => false;
                private protected override bool HasUnkOffsetT98 => true;
                private protected override bool HasUnkOffsetTA0 => false;

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct50 UnkStruct50 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct70 UnkStruct70 { get; set; }

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
                public int Unk00 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk04 { get; set; }

                /// <summary>
                /// An ID in NPCParam that determines a variety of enemy properties.
                /// </summary>
                [MSBParamReference(ParamName = "NpcParam")]
                public int NPCParamID { get; set; }

                /// <summary>
                /// An ID in NPCThinkParam that determines the enemy's AI characteristics.
                /// </summary>
                [MSBParamReference(ParamName = "NpcThinkParam")]
                public int ThinkParamID { get; set; }

                /// <summary>
                /// Talk ID
                /// </summary>
                public int TalkID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short Unk14 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short Unk16 { get; set; }

                /// <summary>
                /// An ID in CharaInitParam that determines a human's inventory and stats.
                /// </summary>
                [MSBParamReference(ParamName = "CharaInitParam")]
                public int CharaInitID { get; set; }

                /// <summary>
                /// Should reference the collision the enemy starts on.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Collision))]
                public string CollisionPartName { get; set; }
                private int CollisionPartIndex;

                /// <summary>
                /// Walk route followed by this enemy.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Event.PatrolInfo))]
                public string WalkRouteName { get; set; }
                private short WalkRouteIndex;

                /// <summary>
                /// Unknown.
                /// </summary>
                public short Unk22 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short Unk24 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk28 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk2C { get; set; }
                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk30 { get; set; }
                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk34 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk38 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk3C { get; set; }

                /// <summary>
                /// Unknown. Entity ID?
                /// </summary>
                public int Unk40 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk44 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk48 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk4C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk50 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte Unk54 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte Unk55 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte Unk56 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte Unk57 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk58 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk5C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk60 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk64 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk68 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk6C { get; set; }

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
                    /// </summary>
                    public short Unk00 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public short Unk02 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public short Unk04 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public short Unk06 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public short Unk08 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public short Unk0A { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public int Unk0C { get; set; }

                    /// <summary>
                    /// Creates an EnemyUnkStruct70 with default values.
                    /// </summary>
                    public EnemyUnkStruct70() { }

                    /// <summary>
                    /// Creates a deep copy of the struct.
                    /// </summary>
                    public EnemyUnkStruct70 DeepCopy()
                    {
                        return (EnemyUnkStruct70)MemberwiseClone();
                    }

                    internal EnemyUnkStruct70(BinaryReaderEx br)
                    {
                        Unk00 = br.ReadInt16();
                        Unk02 = br.ReadInt16();
                        Unk04 = br.ReadInt16();
                        Unk06 = br.ReadInt16();
                        Unk08 = br.ReadInt16();
                        Unk0A = br.ReadInt16();
                        Unk0C = br.ReadInt32();
                    }

                    internal void Write(BinaryWriterEx bw)
                    {
                        bw.WriteInt16(Unk00);
                        bw.WriteInt16(Unk02);
                        bw.WriteInt16(Unk04);
                        bw.WriteInt16(Unk06);
                        bw.WriteInt16(Unk08);
                        bw.WriteInt16(Unk0A);
                        bw.WriteInt32(Unk0C);
                    }
                }

                /// <summary>
                /// Unknown.
                /// </summary>
                public class EnemyUnkStruct78
                {
                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public int Unk00 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public float Unk04 { get; set; }

                    /// <summary>
                    /// Struct 1.
                    /// </summary>
                    public int Unk08 { get; set; }

                    /// <summary>
                    /// Struct 1.
                    /// </summary>
                    public short Unk0A { get; set; }

                    /// <summary>
                    /// Struct 1.
                    /// </summary>
                    public short Unk0C { get; set; }

                    /// <summary>
                    /// Struct 2.
                    /// </summary>
                    public int Unk10 { get; set; }

                    /// <summary>
                    /// Struct 2.
                    /// </summary>
                    public short Unk12 { get; set; }

                    /// <summary>
                    /// Struct 2.
                    /// </summary>
                    public short Unk14 { get; set; }

                    /// <summary>
                    /// Struct 3.
                    /// </summary>
                    public int Unk18 { get; set; }

                    /// <summary>
                    /// Struct 3.
                    /// </summary>
                    public short Unk1A { get; set; }

                    /// <summary>
                    /// Struct 3.
                    /// </summary>
                    public short Unk1C { get; set; }

                    /// <summary>
                    /// Struct 4.
                    /// </summary>
                    public int Unk20 { get; set; }

                    /// <summary>
                    /// Struct 4.
                    /// </summary>
                    public short Unk22 { get; set; }

                    /// <summary>
                    /// Struct 4.
                    /// </summary>
                    public short Unk24 { get; set; }

                    /// <summary>
                    /// Struct 5.
                    /// </summary>
                    public int Unk28 { get; set; }

                    /// <summary>
                    /// Struct 5.
                    /// </summary>
                    public short Unk2A { get; set; }

                    /// <summary>
                    /// Struct 5.
                    /// </summary>
                    public short Unk2C { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public int Unk30 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public int Unk34 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public int Unk38 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public int Unk3C { get; set; }


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
                        Unk00 = br.ReadInt32();
                        Unk04 = br.ReadSingle();

                        Unk08 = br.ReadInt32();
                        Unk0A = br.ReadInt16();
                        Unk0C = br.ReadInt16();

                        Unk10 = br.ReadInt32();
                        Unk12 = br.ReadInt16();
                        Unk14 = br.ReadInt16();

                        Unk18 = br.ReadInt32();
                        Unk1A = br.ReadInt16();
                        Unk1C = br.ReadInt16();

                        Unk20 = br.ReadInt32();
                        Unk22 = br.ReadInt16();
                        Unk24 = br.ReadInt16();

                        Unk28 = br.ReadInt32();
                        Unk2A = br.ReadInt16();
                        Unk2C = br.ReadInt16();

                        Unk30 = br.ReadInt32();
                        Unk34 = br.ReadInt32();
                        Unk38 = br.ReadInt32();
                        Unk3C = br.ReadInt32();
                    }

                    internal void Write(BinaryWriterEx bw)
                    {
                        bw.WriteInt32(Unk00);
                        bw.WriteSingle(Unk04);

                        bw.WriteInt32(Unk08);
                        bw.WriteInt16(Unk0A);
                        bw.WriteInt16(Unk0C);

                        bw.WriteInt32(Unk10);
                        bw.WriteInt16(Unk12);
                        bw.WriteInt16(Unk14);

                        bw.WriteInt32(Unk18);
                        bw.WriteInt16(Unk1A);
                        bw.WriteInt16(Unk1C);

                        bw.WriteInt32(Unk20);
                        bw.WriteInt16(Unk22);
                        bw.WriteInt16(Unk24);

                        bw.WriteInt32(Unk28);
                        bw.WriteInt16(Unk2A);
                        bw.WriteInt16(Unk2C);

                        bw.WriteInt32(Unk30);
                        bw.WriteInt32(Unk34);
                        bw.WriteInt32(Unk38);
                        bw.WriteInt32(Unk3C);
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
                    UnkStruct50 = new UnkStruct50();
                    UnkStruct70 = new UnkStruct70();
                    UnkStruct88 = new UnkStruct88();
                    UnkStruct98 = new UnkStruct98();

                    ThinkParamID = -1;
                    NPCParamID = -1;
                    TalkID = -1;
                    CharaInitID = -1;

                    UnkEnemyStruct70 = new EnemyUnkStruct70();
                    UnkEnemyStruct78 = new EnemyUnkStruct78();
                }

                private protected override void DeepCopyTo(Part part)
                {
                    var enemy = (EnemyBase)part;
                    enemy.UnkStruct50 = UnkStruct50.DeepCopy();
                    enemy.UnkStruct70 = UnkStruct70.DeepCopy();
                    enemy.UnkStruct88 = UnkStruct88.DeepCopy();
                    enemy.UnkStruct98 = UnkStruct98.DeepCopy();

                    enemy.UnkEnemyStruct70 = UnkEnemyStruct70.DeepCopy();
                    enemy.UnkEnemyStruct78 = UnkEnemyStruct78.DeepCopy();
                }

                private protected EnemyBase(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    long start = br.Position;

                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadInt32();
                    ThinkParamID = br.ReadInt32();
                    NPCParamID = br.ReadInt32();
                    TalkID = br.ReadInt32();
                    Unk14 = br.ReadInt16();
                    Unk16 = br.ReadInt16();
                    CharaInitID = br.ReadInt32();
                    CollisionPartIndex = br.ReadInt32();
                    WalkRouteIndex = br.ReadInt16();
                    Unk22 = br.ReadInt16();
                    Unk24 = br.ReadInt16();
                    Unk28 = br.ReadInt32();
                    Unk2C = br.ReadInt32();
                    Unk30 = br.ReadInt32();
                    Unk34 = br.ReadInt32();
                    Unk38 = br.ReadInt32();
                    Unk3C = br.ReadInt32();
                    Unk40 = br.ReadInt32(); // Entity id?
                    Unk44 = br.ReadInt32();
                    Unk48 = br.ReadInt32();
                    Unk4C = br.ReadInt32();
                    Unk50 = br.ReadInt32();
                    Unk54 = br.ReadByte();
                    Unk55 = br.ReadByte();
                    Unk56 = br.ReadByte();
                    Unk57 = br.ReadByte();
                    Unk58 = br.ReadInt32();

                    Unk5C = br.ReadInt32();
                    Unk60 = br.ReadInt32();
                    Unk64 = br.ReadInt32();
                    Unk68 = br.ReadInt32();
                    Unk6C = br.ReadInt32();

                    UnkEnemyOffset70 = br.ReadInt64();
                    UnkEnemyOffset78 = br.ReadInt64();

                    //br.Position = start + UnkEnemyOffset70;
                    UnkEnemyStruct70 = new EnemyUnkStruct70(br);

                    //br.Position = start + UnkEnemyOffset78;
                    UnkEnemyStruct78 = new EnemyUnkStruct78(br);
                }

                private protected override void ReadUnkOffsetT50(BinaryReaderEx br) => UnkStruct50 = new UnkStruct50(br);
                private protected override void ReadUnkOffsetT70(BinaryReaderEx br) => UnkStruct70 = new UnkStruct70(br);
                private protected override void ReadUnkOffsetT88(BinaryReaderEx br) => UnkStruct88 = new UnkStruct88(br);
                private protected override void ReadUnkOffsetT98(BinaryReaderEx br) => UnkStruct98 = new UnkStruct98(br);

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    long start = bw.Position;

                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(NPCParamID);
                    bw.WriteInt32(ThinkParamID);
                    bw.WriteInt32(TalkID);
                    bw.WriteInt16(Unk14);
                    bw.WriteInt16(Unk16);
                    bw.WriteInt32(CharaInitID);
                    bw.WriteInt32(CollisionPartIndex);
                    bw.WriteInt16(WalkRouteIndex);
                    bw.WriteInt16(Unk22);
                    bw.WriteInt16(Unk24);
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
                    bw.WriteInt32(Unk50);
                    bw.WriteByte(Unk54);
                    bw.WriteByte(Unk55);
                    bw.WriteByte(Unk56);
                    bw.WriteByte(Unk57);
                    bw.WriteInt32(Unk58);
                    bw.WriteInt32(Unk5C);
                    bw.WriteInt32(Unk60);
                    bw.WriteInt32(Unk64);
                    bw.WriteInt32(Unk68);
                    bw.WriteInt32(Unk6C);

                    bw.ReserveInt64("UnkEnemyOffset70");
                    bw.ReserveInt64("UnkEnemyOffset78");

                    bw.FillInt64("UnkEnemyOffset70", bw.Position - start);
                    UnkEnemyStruct70.Write(bw);

                    bw.FillInt64("UnkEnemyOffset78", bw.Position - start);
                    UnkEnemyStruct78.Write(bw);
                }

                private protected override void WriteUnkOffsetT50(BinaryWriterEx bw) => UnkStruct50.Write(bw);
                private protected override void WriteUnkOffsetT70(BinaryWriterEx bw) => UnkStruct70.Write(bw);
                private protected override void WriteUnkOffsetT88(BinaryWriterEx bw) => UnkStruct88.Write(bw);
                private protected override void WriteUnkOffsetT98(BinaryWriterEx bw) => UnkStruct98.Write(bw);

                internal override void GetNames(MSB_AC6 msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    CollisionPartName = MSB.FindName(entries.Parts, CollisionPartIndex);
                    WalkRouteName = MSB.FindName(msb.Events.PatrolInfo, WalkRouteIndex);
                }

                internal override void GetIndices(MSB_AC6 msb, Entries entries)
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
                private protected override bool HasUnkOffsetT50 => true;
                private protected override bool HasUnkOffsetT58 => false;
                private protected override bool HasUnkOffsetT70 => false;
                private protected override bool HasUnkOffsetT78 => false;
                private protected override bool HasUnkOffsetT80 => false;
                private protected override bool HasUnkOffsetT88 => true;
                private protected override bool HasUnkOffsetT90 => false;
                private protected override bool HasUnkOffsetT98 => true;
                private protected override bool HasUnkOffsetTA0 => false;

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct50 UnkStruct50 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct88 UnkStruct88 { get; set; }

                /// Unknown.
                /// </summary>
                public UnkStruct98 UnkStruct98 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk00 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk04 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk08 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk0C { get; set; }

                /// <summary>
                /// Creates a Player with default values.
                /// </summary>
                public Player() : base("c0000_XXXX")
                {
                    UnkStruct50 = new UnkStruct50();
                    UnkStruct88 = new UnkStruct88();
                    UnkStruct98 = new UnkStruct98();
                }

                private protected override void DeepCopyTo(Part part)
                {
                    var player = (Player)part;
                    player.UnkStruct50 = UnkStruct50.DeepCopy();
                    player.UnkStruct88 = UnkStruct88.DeepCopy();
                    player.UnkStruct98 = UnkStruct98.DeepCopy();
                }

                internal Player(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadInt32();
                    Unk08 = br.ReadInt32();
                    Unk0C = br.ReadInt32();
                }
                private protected override void ReadUnkOffsetT50(BinaryReaderEx br) => UnkStruct50 = new UnkStruct50(br);
                private protected override void ReadUnkOffsetT88(BinaryReaderEx br) => UnkStruct88 = new UnkStruct88(br);
                private protected override void ReadUnkOffsetT98(BinaryReaderEx br) => UnkStruct98 = new UnkStruct98(br);

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                }

                private protected override void WriteUnkOffsetT50(BinaryWriterEx bw) => UnkStruct50.Write(bw);
                private protected override void WriteUnkOffsetT88(BinaryWriterEx bw) => UnkStruct88.Write(bw);
                private protected override void WriteUnkOffsetT98(BinaryWriterEx bw) => UnkStruct98.Write(bw);
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
                    None = 0,
                    Standard = 8,
                    CameraOnly = 9,
                    EnemyOnly = 11,
                    FallDeathCam = 13,
                    Kill = 15,
                    Unk16 = 16,
                    Unk17 = 17,
                    Unk19 = 19,
                    Unk20 = 20,
                    Unk22 = 22,
                    Unk23 = 23,
                    Unk24 = 24,
                    Unk28 = 28,
                    Unk29 = 29,
                    Unk30 = 30,
                    Unk31 = 31,
                    Other = 255
                }

                private protected override PartType Type => PartType.Collision;
                private protected override bool HasUnkOffsetT50 => true;
                private protected override bool HasUnkOffsetT58 => true;
                private protected override bool HasUnkOffsetT70 => true;
                private protected override bool HasUnkOffsetT78 => true;
                private protected override bool HasUnkOffsetT80 => false;
                private protected override bool HasUnkOffsetT88 => true;
                private protected override bool HasUnkOffsetT90 => false;
                private protected override bool HasUnkOffsetT98 => true;
                private protected override bool HasUnkOffsetTA0 => true;

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct50 UnkStruct50 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct58 UnkStruct58 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct70 UnkStruct70 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct78 UnkStruct78 { get; set; }

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
                public HitFilterType HitFilterID { get; set; } = HitFilterType.Standard;

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte Unk01 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte Unk02 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte Unk03 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float Unk04 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk08 { get; set; }

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
                /// Unknown.
                /// </summary>
                public int Unk18 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk1C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk20 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short Unk24 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short Unk26 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk28 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk2C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk30 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte Unk34 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte Unk35 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short Unk36 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk38 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk3C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk40 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk44 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk48 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk4C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk50 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk54 { get; set; }

                /// <summary>
                /// Creates a Collision with default values.
                /// </summary>
                public Collision() : base("hXXXXXX")
                {
                    UnkStruct50 = new UnkStruct50();
                    UnkStruct58 = new UnkStruct58();
                    UnkStruct70 = new UnkStruct70();
                    UnkStruct78 = new UnkStruct78();
                    UnkStruct88 = new UnkStruct88();
                    UnkStruct98 = new UnkStruct98();
                    UnkStructA0 = new UnkStructA0();
                }

                private protected override void DeepCopyTo(Part part)
                {
                    var collision = (Collision)part;
                    collision.UnkStruct50 = UnkStruct50.DeepCopy();
                    collision.UnkStruct58 = UnkStruct58.DeepCopy();
                    collision.UnkStruct70 = UnkStruct70.DeepCopy();
                    collision.UnkStruct78 = UnkStruct78.DeepCopy();
                    collision.UnkStruct88 = UnkStruct88.DeepCopy();
                    collision.UnkStruct98 = UnkStruct98.DeepCopy();
                    collision.UnkStructA0 = UnkStructA0.DeepCopy();
                }

                internal Collision(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    HitFilterID = br.ReadEnum8<HitFilterType>();
                    Unk01 = br.ReadByte();
                    Unk02 = br.ReadByte();
                    Unk03 = br.ReadByte();
                    Unk04 = br.ReadSingle();
                    Unk08 = br.ReadInt32();
                    Unk0C = br.ReadInt32();
                    Unk10 = br.ReadInt32();
                    Unk14 = br.ReadInt32();
                    Unk18 = br.ReadInt32();
                    Unk1C = br.ReadInt32();
                    Unk20 = br.ReadInt32();
                    Unk24 = br.ReadInt16();
                    Unk26 = br.ReadInt16();
                    Unk28 = br.ReadInt32();
                    Unk2C = br.ReadInt32();
                    Unk30 = br.ReadInt32();
                    Unk34 = br.ReadByte();
                    Unk35 = br.ReadByte();
                    Unk36 = br.ReadInt16();
                    Unk38 = br.ReadInt32();
                    Unk3C = br.ReadInt32();
                    Unk40 = br.ReadInt32();
                    Unk44 = br.ReadInt32();
                    Unk48 = br.ReadInt32();
                    Unk4C = br.ReadInt32();
                    Unk50 = br.ReadInt32();
                    Unk54 = br.ReadInt32();
                }

                private protected override void ReadUnkOffsetT50(BinaryReaderEx br) => UnkStruct50 = new UnkStruct50(br);
                private protected override void ReadUnkOffsetT58(BinaryReaderEx br) => UnkStruct58 = new UnkStruct58(br);
                private protected override void ReadUnkOffsetT70(BinaryReaderEx br) => UnkStruct70 = new UnkStruct70(br);
                private protected override void ReadUnkOffsetT78(BinaryReaderEx br) => UnkStruct78 = new UnkStruct78(br);
                private protected override void ReadUnkOffsetT88(BinaryReaderEx br) => UnkStruct88 = new UnkStruct88(br);
                private protected override void ReadUnkOffsetT98(BinaryReaderEx br) => UnkStruct98 = new UnkStruct98(br);
                private protected override void ReadUnkOffsetTA0(BinaryReaderEx br) => UnkStructA0 = new UnkStructA0(br);

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteByte((byte)HitFilterID);
                    bw.WriteByte(Unk01);
                    bw.WriteByte(Unk02);
                    bw.WriteByte(Unk03);
                    bw.WriteSingle(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                    bw.WriteInt32(Unk10);
                    bw.WriteInt32(Unk14);
                    bw.WriteInt32(Unk18);
                    bw.WriteInt32(Unk1C);
                    bw.WriteInt32(Unk20);
                    bw.WriteInt16(Unk24);
                    bw.WriteInt16(Unk26);
                    bw.WriteInt32(Unk28);
                    bw.WriteInt32(Unk2C);
                    bw.WriteInt32(Unk30);
                    bw.WriteByte(Unk34);
                    bw.WriteByte(Unk35);
                    bw.WriteInt16(Unk36);
                    bw.WriteInt32(Unk38);
                    bw.WriteInt32(Unk3C);
                    bw.WriteInt32(Unk40);
                    bw.WriteInt32(Unk44);
                    bw.WriteInt32(Unk48);
                    bw.WriteInt32(Unk4C);
                    bw.WriteInt32(Unk50);
                    bw.WriteInt32(Unk54);
                }
                private protected override void WriteUnkOffsetT50(BinaryWriterEx bw) => UnkStruct50.Write(bw);
                private protected override void WriteUnkOffsetT58(BinaryWriterEx bw) => UnkStruct58.Write(bw);
                private protected override void WriteUnkOffsetT70(BinaryWriterEx bw) => UnkStruct70.Write(bw);
                private protected override void WriteUnkOffsetT78(BinaryWriterEx bw) => UnkStruct78.Write(bw);
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
                private protected override bool HasUnkOffsetT70 => true;
                private protected override bool HasUnkOffsetT78 => false;
                private protected override bool HasUnkOffsetT80 => false;
                private protected override bool HasUnkOffsetT88 => true;
                private protected override bool HasUnkOffsetT90 => false;
                private protected override bool HasUnkOffsetT98 => true;
                private protected override bool HasUnkOffsetTA0 => false;

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct50 UnkStruct50 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct70 UnkStruct70 { get; set; }

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
                public int Unk00 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk04 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk08 { get; set; }

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
                /// Unknown.
                /// </summary>
                public int Unk18 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk1C { get; set; }

                /// <summary>
                /// Creates a DummyAsset with default values.
                /// </summary>
                public DummyAsset() : base("AEGxxx_xxx_xxxx")
                {
                    UnkStruct50 = new UnkStruct50();
                    UnkStruct70 = new UnkStruct70();
                    UnkStruct88 = new UnkStruct88();
                    UnkStruct98 = new UnkStruct98();
                }

                private protected override void DeepCopyTo(Part part)
                {
                    var asset = (DummyAsset)part;
                    asset.UnkStruct50 = UnkStruct50.DeepCopy();
                    asset.UnkStruct70 = UnkStruct70.DeepCopy();
                    asset.UnkStruct88 = UnkStruct88.DeepCopy();
                    asset.UnkStruct98 = UnkStruct98.DeepCopy();
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

                private protected override void ReadUnkOffsetT50(BinaryReaderEx br) => UnkStruct50 = new UnkStruct50(br);
                private protected override void ReadUnkOffsetT70(BinaryReaderEx br) => UnkStruct70 = new UnkStruct70(br);
                private protected override void ReadUnkOffsetT88(BinaryReaderEx br) => UnkStruct88 = new UnkStruct88(br);
                private protected override void ReadUnkOffsetT98(BinaryReaderEx br) => UnkStruct98 = new UnkStruct98(br);

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
                private protected override void WriteUnkOffsetT50(BinaryWriterEx bw) => UnkStruct50.Write(bw);
                private protected override void WriteUnkOffsetT70(BinaryWriterEx bw) => UnkStruct70.Write(bw);
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
                private protected override bool HasUnkOffsetT70 => false;
                private protected override bool HasUnkOffsetT78 => false;
                private protected override bool HasUnkOffsetT80 => false;
                private protected override bool HasUnkOffsetT88 => true;
                private protected override bool HasUnkOffsetT90 => false;
                private protected override bool HasUnkOffsetT98 => true;
                private protected override bool HasUnkOffsetTA0 => true;

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct50 UnkStruct50 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct58 UnkStruct58 { get; set; }

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
                public string CollisionName { get; set; }
                private int CollisionIndex;

                /// <summary>
                /// The map to load when on this collision.
                /// </summary>
                public byte[] MapID { get; private set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk0C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk10 { get; set; }

                /// <summary>
                /// Creates a ConnectCollision with default values.
                /// </summary>
                public ConnectCollision() : base("hXXXXXX_XXXX")
                {
                    MapID = new byte[4];
                    UnkStruct50 = new UnkStruct50();
                    UnkStruct58 = new UnkStruct58();
                    UnkStruct88 = new UnkStruct88();
                    UnkStruct98 = new UnkStruct98();
                    UnkStructA0 = new UnkStructA0();
                }

                private protected override void DeepCopyTo(Part part)
                {
                    var connect = (ConnectCollision)part;
                    connect.MapID = (byte[])MapID.Clone();
                    connect.UnkStruct50 = UnkStruct50.DeepCopy();
                    connect.UnkStruct58 = UnkStruct58.DeepCopy();
                    connect.UnkStruct88 = UnkStruct88.DeepCopy();
                    connect.UnkStruct98 = UnkStruct98.DeepCopy();
                    connect.UnkStructA0 = UnkStructA0.DeepCopy();
                }

                internal ConnectCollision(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    CollisionIndex = br.ReadInt32();
                    MapID = br.ReadBytes(4);
                    Unk0C = br.ReadInt32();
                    Unk10 = br.ReadInt32();
                }
                private protected override void ReadUnkOffsetT50(BinaryReaderEx br) => UnkStruct50 = new UnkStruct50(br);
                private protected override void ReadUnkOffsetT58(BinaryReaderEx br) => UnkStruct58 = new UnkStruct58(br);
                private protected override void ReadUnkOffsetT88(BinaryReaderEx br) => UnkStruct88 = new UnkStruct88(br);
                private protected override void ReadUnkOffsetT98(BinaryReaderEx br) => UnkStruct98 = new UnkStruct98(br);
                private protected override void ReadUnkOffsetTA0(BinaryReaderEx br) => UnkStructA0 = new UnkStructA0(br);

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(CollisionIndex);
                    bw.WriteBytes(MapID);
                    bw.WriteInt32(Unk0C);
                    bw.WriteInt32(Unk10);
                }
                private protected override void WriteUnkOffsetT50(BinaryWriterEx bw) => UnkStruct50.Write(bw);
                private protected override void WriteUnkOffsetT58(BinaryWriterEx bw) => UnkStruct58.Write(bw);
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
                private protected override PartType Type => PartType.Asset;
                private protected override bool HasUnkOffsetT50 => true;
                private protected override bool HasUnkOffsetT58 => true;
                private protected override bool HasUnkOffsetT70 => true;
                private protected override bool HasUnkOffsetT78 => false;
                private protected override bool HasUnkOffsetT80 => true;
                private protected override bool HasUnkOffsetT88 => true;
                private protected override bool HasUnkOffsetT90 => true;
                private protected override bool HasUnkOffsetT98 => true;
                private protected override bool HasUnkOffsetTA0 => true;

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct50 UnkStruct50 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct58 UnkStruct58 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct70 UnkStruct70 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct80 UnkStruct80 { get; set; }

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
                /// </summary>
                public bool Unk00 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte Unk01 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte Unk02 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte Unk03 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk04 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk08 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk0C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte Unk10 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public bool Unk11 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public bool Unk12 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public bool Unk13 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short Unk14 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short Unk16 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk18 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short Unk1C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short Unk1E { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk20 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk24 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk28 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk2C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Part))]
                public string[] UnkPartNames { get; private set; }
                private int[] UnkPartIndices { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk40 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk44 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk48 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte Unk4C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte Unk4D { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short Unk4E { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk50 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk54 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk58 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk5C { get; set; }

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
                public int Unk124 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk128 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk12C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk130 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk134 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk138 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk13C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk140 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk144 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk148 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk14C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk150 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk154 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk158 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk15C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk160 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk164 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk168 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public class AssetUnkStruct60
                {
                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public short Unk00 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public short Unk02 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public int Unk04 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public int Unk08 { get; set; }

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
                    /// Unknown.
                    /// </summary>
                    public int Unk18 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public int Unk1C { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public int Unk20 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public int Unk24 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public int Unk28 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public int Unk2C { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public int Unk30 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public int Unk34 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public int Unk38 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public int Unk3C { get; set; }

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
                        Unk00 = br.ReadInt16();
                        Unk02 = br.ReadInt16();
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
                        bw.WriteInt16(Unk00);
                        bw.WriteInt16(Unk02);
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
                }

                /// <summary>
                /// Unknown.
                /// </summary>
                public class AssetUnkStruct68
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
                    /// Unknown.
                    /// </summary>
                    public int Unk08 { get; set; }

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
                    public float Unk14 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public int Unk18 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public int Unk1C { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public int Unk20 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public int Unk24 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public int Unk28 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public int Unk2C { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public int Unk30 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public int Unk34 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public int Unk38 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public int Unk3C { get; set; }

                    /// <summary>
                    /// Creates an AssetUnkStruct2 with default values.
                    /// </summary>
                    public AssetUnkStruct68() { }

                    /// <summary>
                    /// Creates a deep copy of the struct.
                    /// </summary>
                    public AssetUnkStruct68 DeepCopy()
                    {
                        return (AssetUnkStruct68)MemberwiseClone();
                    }

                    internal AssetUnkStruct68(BinaryReaderEx br)
                    {
                        Unk00 = br.ReadInt32();
                        Unk04 = br.ReadInt32();
                        Unk08 = br.ReadInt32();
                        Unk0C = br.ReadInt32();
                        Unk10 = br.ReadInt32();
                        Unk14 = br.ReadSingle();
                        Unk18 = br.ReadInt32();
                        Unk1C = br.ReadInt32();
                        Unk20 = br.ReadInt32();
                        Unk24 = br.ReadInt32();
                        Unk28 = br.ReadInt32();
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
                }

                /// <summary>
                /// Unknown.
                /// </summary>
                public class AssetUnkStruct70
                {
                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public int Unk00 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public float Unk04 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public byte Unk08 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public byte Unk09 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public short Unk0B { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public int Unk10 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public int Unk14 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public int Unk18 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public int Unk1C { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public int Unk20 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public int Unk24 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public int Unk28 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public int Unk2C { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public int Unk30 { get; set; }


                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public int Unk34 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public int Unk38 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public int Unk3C { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public int Unk40 { get; set; }

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
                        Unk00 = br.ReadInt32();
                        Unk04 = br.ReadSingle();
                        Unk08 = br.ReadByte();
                        Unk09 = br.ReadByte();
                        Unk0B = br.ReadInt16();

                        Unk10 = br.ReadInt32();
                        Unk14 = br.ReadInt32();
                        Unk18 = br.ReadInt32();
                        Unk1C = br.ReadInt32();
                        Unk20 = br.ReadInt32();
                        Unk24 = br.ReadInt32();
                        Unk28 = br.ReadInt32();
                        Unk2C = br.ReadInt32();
                        Unk30 = br.ReadInt32();
                        Unk24 = br.ReadInt32();
                        Unk38 = br.ReadInt32();
                        Unk3C = br.ReadInt32();
                        Unk40 = br.ReadInt32();
                    }

                    internal void Write(BinaryWriterEx bw)
                    {
                        bw.WriteInt32(Unk00);
                        bw.WriteSingle(Unk04);
                        bw.WriteByte(Unk08);
                        bw.WriteByte(Unk09);
                        bw.WriteInt16(Unk0B);

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
                        bw.WriteInt32(Unk40);
                    }
                }

                /// <summary>
                /// Unknown.
                /// </summary>
                public class AssetUnkStruct78
                {
                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public byte Unk00 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public byte Unk01 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public byte Unk02 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public byte Unk03 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public int Unk04 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public int Unk08 { get; set; }

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
                    /// Unknown.
                    /// </summary>
                    public int Unk18 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public int Unk1C { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public int Unk20 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public int Unk24 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public int Unk28 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public int Unk2C { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public int Unk30 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public int Unk34 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public int Unk38 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public int Unk3C { get; set; }

                    /// <summary>
                    /// Creates an AssetUnkStruct4 with default values.
                    /// </summary>
                    public AssetUnkStruct78() { }

                    /// <summary>
                    /// Creates a deep copy of the struct.
                    /// </summary>
                    public AssetUnkStruct78 DeepCopy()
                    {
                        return (AssetUnkStruct78)MemberwiseClone();
                    }

                    internal AssetUnkStruct78(BinaryReaderEx br)
                    {
                        Unk00 = br.ReadByte();
                        Unk01 = br.ReadByte();
                        Unk02 = br.ReadByte();
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
                        bw.WriteByte(Unk01);
                        bw.WriteByte(Unk02);
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
                    UnkStruct50 = new UnkStruct50();
                    UnkStruct58 = new UnkStruct58();
                    UnkStruct70 = new UnkStruct70();
                    UnkStruct80 = new UnkStruct80();
                    UnkStruct88 = new UnkStruct88();
                    UnkStruct90 = new UnkStruct90();
                    UnkStruct98 = new UnkStruct98();
                    UnkStructA0 = new UnkStructA0();

                    UnkAssetStruct60 = new AssetUnkStruct60();
                    UnkAssetStruct68 = new AssetUnkStruct68();
                    UnkAssetStruct70 = new AssetUnkStruct70();
                    UnkAssetStruct78 = new AssetUnkStruct78();

                    UnkPartNames = new string[4];
                }

                private protected override void DeepCopyTo(Part part)
                {
                    var asset = (Asset)part;
                    asset.UnkStruct50 = UnkStruct50.DeepCopy();
                    asset.UnkStruct58 = UnkStruct58.DeepCopy();
                    asset.UnkStruct70 = UnkStruct70.DeepCopy();
                    asset.UnkStruct80 = UnkStruct80.DeepCopy();
                    asset.UnkStruct88 = UnkStruct88.DeepCopy();
                    asset.UnkStruct90 = UnkStruct90.DeepCopy();
                    asset.UnkStruct98 = UnkStruct98.DeepCopy();
                    asset.UnkStructA0 = UnkStructA0.DeepCopy();

                    asset.UnkAssetStruct60 = UnkAssetStruct60.DeepCopy();
                    asset.UnkAssetStruct68 = UnkAssetStruct68.DeepCopy();
                    asset.UnkAssetStruct70 = UnkAssetStruct70.DeepCopy();
                    asset.UnkAssetStruct78 = UnkAssetStruct78.DeepCopy();

                    UnkPartNames = (string[])UnkPartNames.Clone();
                }

                internal Asset(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    long start = br.Position;

                    Unk00 = br.ReadBoolean();
                    Unk01 = br.ReadByte();
                    Unk02 = br.ReadByte();
                    Unk03 = br.ReadByte();
                    Unk04 = br.ReadInt32();
                    Unk08 = br.ReadInt32();
                    Unk0C = br.ReadInt32();
                    Unk10 = br.ReadByte();
                    Unk11 = br.ReadBoolean(); 
                    Unk12 = br.ReadBoolean();
                    Unk13 = br.ReadBoolean();
                    Unk14 = br.ReadInt16();
                    Unk16 = br.ReadInt16();
                    Unk18 = br.ReadInt32();
                    Unk1C = br.ReadInt16();
                    Unk1E = br.ReadInt16();
                    Unk20 = br.ReadInt32();
                    Unk24 = br.ReadInt32();
                    Unk28 = br.ReadInt32();
                    Unk2C = br.ReadInt32();
                    UnkPartIndices = br.ReadInt32s(4);
                    Unk40 = br.ReadInt32();
                    Unk44 = br.ReadInt32();
                    Unk48 = br.ReadInt32();
                    Unk4C = br.ReadByte();
                    Unk4D = br.ReadByte();
                    Unk4E = br.ReadInt16();
                    Unk50 = br.ReadInt32();
                    Unk54 = br.ReadInt32();
                    Unk58 = br.ReadInt32();
                    Unk5C = br.ReadInt32();

                    UnkAssetOffset60 = br.ReadInt64();
                    UnkAssetOffset68 = br.ReadInt64();
                    UnkAssetOffset70 = br.ReadInt64();
                    UnkAssetOffset78 = br.ReadInt64();

                    //if(Version >= 52)
                    //{
                    Unk124 = br.ReadInt32();
                    Unk128 = br.ReadInt32();
                    Unk12C = br.ReadInt32();
                    Unk130 = br.ReadInt32();
                    Unk134 = br.ReadInt32();
                    Unk138 = br.ReadInt32();
                    Unk13C = br.ReadInt32();
                    Unk140 = br.ReadInt32();
                    Unk144 = br.ReadInt32();
                    Unk148 = br.ReadInt32();
                    Unk14C = br.ReadInt32();
                    Unk150 = br.ReadInt32();
                    Unk154 = br.ReadInt32();
                    Unk158 = br.ReadInt32();
                    Unk15C = br.ReadInt32();
                    Unk160 = br.ReadInt32();
                    Unk164 = br.ReadInt32();
                    Unk168 = br.ReadInt32();
                    //br.AssertPattern(0x40, 0x00);
                    //}

                    br.Position = start + UnkAssetOffset60;
                    UnkAssetStruct60 = new AssetUnkStruct60(br);

                    br.Position = start + UnkAssetOffset68;
                    UnkAssetStruct68 = new AssetUnkStruct68(br);

                    br.Position = start + UnkAssetOffset70;
                    UnkAssetStruct70 = new AssetUnkStruct70(br);

                    br.Position = start + UnkAssetOffset78;
                    UnkAssetStruct78 = new AssetUnkStruct78(br);
                }
                private protected override void ReadUnkOffsetT50(BinaryReaderEx br) => UnkStruct50 = new UnkStruct50(br);
                private protected override void ReadUnkOffsetT58(BinaryReaderEx br) => UnkStruct58 = new UnkStruct58(br);
                private protected override void ReadUnkOffsetT70(BinaryReaderEx br) => UnkStruct70 = new UnkStruct70(br);
                private protected override void ReadUnkOffsetT80(BinaryReaderEx br) => UnkStruct80 = new UnkStruct80(br);
                private protected override void ReadUnkOffsetT88(BinaryReaderEx br) => UnkStruct88 = new UnkStruct88(br);
                private protected override void ReadUnkOffsetT90(BinaryReaderEx br) => UnkStruct90 = new UnkStruct90(br);
                private protected override void ReadUnkOffsetT98(BinaryReaderEx br) => UnkStruct98 = new UnkStruct98(br);
                private protected override void ReadUnkOffsetTA0(BinaryReaderEx br) => UnkStructA0 = new UnkStructA0(br);

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    long start = bw.Position;

                    bw.WriteBoolean(Unk00);
                    bw.WriteByte(Unk01);
                    bw.WriteByte(Unk02);
                    bw.WriteByte(Unk03);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                    bw.WriteByte(Unk10);
                    bw.WriteBoolean(Unk11);
                    bw.WriteBoolean(Unk12);
                    bw.WriteBoolean(Unk13);
                    bw.WriteInt16(Unk14);
                    bw.WriteInt16(Unk16);
                    bw.WriteInt32(Unk18);
                    bw.WriteInt16(Unk1C);
                    bw.WriteInt16(Unk1E);
                    bw.WriteInt32(Unk20);
                    bw.WriteInt32(Unk24);
                    bw.WriteInt32(Unk28);
                    bw.WriteInt32(Unk2C);
                    bw.WriteInt32s(UnkPartIndices);
                    bw.WriteInt32(Unk40);
                    bw.WriteInt32(Unk44);
                    bw.WriteInt32(Unk48);
                    bw.WriteByte(Unk4C);
                    bw.WriteByte(Unk4D);
                    bw.WriteInt16(Unk4E);
                    bw.WriteInt32(Unk50);
                    bw.WriteInt32(Unk54);
                    bw.WriteInt32(Unk58);
                    bw.WriteInt32(Unk5C);

                    bw.ReserveInt64("UnkAssetOffset60");
                    bw.ReserveInt64("UnkAssetOffset68");
                    bw.ReserveInt64("UnkAssetOffset70");
                    bw.ReserveInt64("UnkAssetOffset78");

                    //if(Version >= 52)
                    //{
                    bw.WriteInt32(Unk124);
                    bw.WriteInt32(Unk128);
                    bw.WriteInt32(Unk12C);
                    bw.WriteInt32(Unk130);
                    bw.WriteInt32(Unk134);
                    bw.WriteInt32(Unk138);
                    bw.WriteInt32(Unk13C);
                    bw.WriteInt32(Unk140);
                    bw.WriteInt32(Unk144);
                    bw.WriteInt32(Unk148);
                    bw.WriteInt32(Unk14C);
                    bw.WriteInt32(Unk150);
                    bw.WriteInt32(Unk154);
                    bw.WriteInt32(Unk158);
                    bw.WriteInt32(Unk15C);
                    bw.WriteInt32(Unk160);
                    bw.WriteInt32(Unk164);
                    bw.WriteInt32(Unk168);
                    //bw.WritePattern(0x40, 0x00);
                    //}

                    bw.FillInt64("UnkAssetOffset60", bw.Position - start);
                    UnkAssetStruct60.Write(bw);

                    bw.FillInt64("UnkAssetOffset68", bw.Position - start);
                    UnkAssetStruct68.Write(bw);

                    bw.FillInt64("UnkAssetOffset70", bw.Position - start);
                    UnkAssetStruct70.Write(bw);

                    bw.FillInt64("UnkAssetOffset78", bw.Position - start);
                    UnkAssetStruct78.Write(bw);
                }
                private protected override void WriteUnkOffsetT50(BinaryWriterEx bw) => UnkStruct50.Write(bw);
                private protected override void WriteUnkOffsetT58(BinaryWriterEx bw) => UnkStruct58.Write(bw);
                private protected override void WriteUnkOffsetT70(BinaryWriterEx bw) => UnkStruct70.Write(bw);
                private protected override void WriteUnkOffsetT80(BinaryWriterEx bw) => UnkStruct80.Write(bw);
                private protected override void WriteUnkOffsetT88(BinaryWriterEx bw) => UnkStruct88.Write(bw);
                private protected override void WriteUnkOffsetT90(BinaryWriterEx bw) => UnkStruct90.Write(bw);
                private protected override void WriteUnkOffsetT98(BinaryWriterEx bw) => UnkStruct98.Write(bw);
                private protected override void WriteUnkOffsetTA0(BinaryWriterEx bw) => UnkStructA0.Write(bw);
                internal override void GetNames(MSB_AC6 msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    UnkPartNames = MSB.FindNames(entries.Parts, UnkPartIndices);
                }

                internal override void GetIndices(MSB_AC6 msb, Entries entries)
                {
                    base.GetIndices(msb, entries);
                    UnkPartIndices = MSB.FindIndices(this, entries.Parts, UnkPartNames);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class Object : Part
            {
                private protected override PartType Type => PartType.Object;
                private protected override bool HasUnkOffsetT50 => true;
                private protected override bool HasUnkOffsetT58 => false;
                private protected override bool HasUnkOffsetT70 => true;
                private protected override bool HasUnkOffsetT78 => false;
                private protected override bool HasUnkOffsetT80 => true;
                private protected override bool HasUnkOffsetT88 => true;
                private protected override bool HasUnkOffsetT90 => true;
                private protected override bool HasUnkOffsetT98 => true;
                private protected override bool HasUnkOffsetTA0 => true;

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct50 UnkStruct50 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct70 UnkStruct70 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct80 UnkStruct80 { get; set; }

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
                public Object() : base("")
                {
                    UnkStruct50 = new UnkStruct50();
                    UnkStruct70 = new UnkStruct70();
                    UnkStruct80 = new UnkStruct80();
                    UnkStruct88 = new UnkStruct88();
                    UnkStruct90 = new UnkStruct90();
                    UnkStruct98 = new UnkStruct98();
                    UnkStructA0 = new UnkStructA0();
                }

                private protected override void DeepCopyTo(Part part)
                {
                    var piece = (Object)part;
                    piece.UnkStruct50 = UnkStruct50.DeepCopy();
                    piece.UnkStruct70 = UnkStruct70.DeepCopy();
                    piece.UnkStruct80 = UnkStruct80.DeepCopy();
                    piece.UnkStruct88 = UnkStruct88.DeepCopy();
                    piece.UnkStruct90 = UnkStruct90.DeepCopy();
                    piece.UnkStruct98 = UnkStruct98.DeepCopy();
                    piece.UnkStructA0 = UnkStructA0.DeepCopy();
                }

                internal Object(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                }

                private protected override void ReadUnkOffsetT50(BinaryReaderEx br) => UnkStruct50 = new UnkStruct50(br);
                private protected override void ReadUnkOffsetT70(BinaryReaderEx br) => UnkStruct70 = new UnkStruct70(br);
                private protected override void ReadUnkOffsetT80(BinaryReaderEx br) => UnkStruct80 = new UnkStruct80(br);
                private protected override void ReadUnkOffsetT88(BinaryReaderEx br) => UnkStruct88 = new UnkStruct88(br);
                private protected override void ReadUnkOffsetT90(BinaryReaderEx br) => UnkStruct90 = new UnkStruct90(br);
                private protected override void ReadUnkOffsetT98(BinaryReaderEx br) => UnkStruct98 = new UnkStruct98(br);
                private protected override void ReadUnkOffsetTA0(BinaryReaderEx br) => UnkStructA0 = new UnkStructA0(br);

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                }

                private protected override void WriteUnkOffsetT50(BinaryWriterEx bw) => UnkStruct50.Write(bw);
                private protected override void WriteUnkOffsetT70(BinaryWriterEx bw) => UnkStruct70.Write(bw);
                private protected override void WriteUnkOffsetT80(BinaryWriterEx bw) => UnkStruct80.Write(bw);
                private protected override void WriteUnkOffsetT88(BinaryWriterEx bw) => UnkStruct88.Write(bw);
                private protected override void WriteUnkOffsetT90(BinaryWriterEx bw) => UnkStruct90.Write(bw);
                private protected override void WriteUnkOffsetT98(BinaryWriterEx bw) => UnkStruct98.Write(bw);
                private protected override void WriteUnkOffsetTA0(BinaryWriterEx bw) => UnkStructA0.Write(bw);
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class Item : Part
            {
                private protected override PartType Type => PartType.Item;
                private protected override bool HasUnkOffsetT50 => true;
                private protected override bool HasUnkOffsetT58 => false;
                private protected override bool HasUnkOffsetT70 => true;
                private protected override bool HasUnkOffsetT78 => false;
                private protected override bool HasUnkOffsetT80 => true;
                private protected override bool HasUnkOffsetT88 => true;
                private protected override bool HasUnkOffsetT90 => true;
                private protected override bool HasUnkOffsetT98 => true;
                private protected override bool HasUnkOffsetTA0 => true;

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct50 UnkStruct50 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct70 UnkStruct70 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct80 UnkStruct80 { get; set; }

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
                public Item() : base("")
                {
                    UnkStruct50 = new UnkStruct50();
                    UnkStruct70 = new UnkStruct70();
                    UnkStruct80 = new UnkStruct80();
                    UnkStruct88 = new UnkStruct88();
                    UnkStruct90 = new UnkStruct90();
                    UnkStruct98 = new UnkStruct98();
                    UnkStructA0 = new UnkStructA0();
                }

                private protected override void DeepCopyTo(Part part)
                {
                    var piece = (Item)part;
                    piece.UnkStruct50 = UnkStruct50.DeepCopy();
                    piece.UnkStruct70 = UnkStruct70.DeepCopy();
                    piece.UnkStruct80 = UnkStruct80.DeepCopy();
                    piece.UnkStruct88 = UnkStruct88.DeepCopy();
                    piece.UnkStruct90 = UnkStruct90.DeepCopy();
                    piece.UnkStruct98 = UnkStruct98.DeepCopy();
                    piece.UnkStructA0 = UnkStructA0.DeepCopy();
                }

                internal Item(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                }

                private protected override void ReadUnkOffsetT50(BinaryReaderEx br) => UnkStruct50 = new UnkStruct50(br);
                private protected override void ReadUnkOffsetT70(BinaryReaderEx br) => UnkStruct70 = new UnkStruct70(br);
                private protected override void ReadUnkOffsetT80(BinaryReaderEx br) => UnkStruct80 = new UnkStruct80(br);
                private protected override void ReadUnkOffsetT88(BinaryReaderEx br) => UnkStruct88 = new UnkStruct88(br);
                private protected override void ReadUnkOffsetT90(BinaryReaderEx br) => UnkStruct90 = new UnkStruct90(br);
                private protected override void ReadUnkOffsetT98(BinaryReaderEx br) => UnkStruct98 = new UnkStruct98(br);
                private protected override void ReadUnkOffsetTA0(BinaryReaderEx br) => UnkStructA0 = new UnkStructA0(br);

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                }

                private protected override void WriteUnkOffsetT50(BinaryWriterEx bw) => UnkStruct50.Write(bw);
                private protected override void WriteUnkOffsetT70(BinaryWriterEx bw) => UnkStruct70.Write(bw);
                private protected override void WriteUnkOffsetT80(BinaryWriterEx bw) => UnkStruct80.Write(bw);
                private protected override void WriteUnkOffsetT88(BinaryWriterEx bw) => UnkStruct88.Write(bw);
                private protected override void WriteUnkOffsetT90(BinaryWriterEx bw) => UnkStruct90.Write(bw);
                private protected override void WriteUnkOffsetT98(BinaryWriterEx bw) => UnkStruct98.Write(bw);
                private protected override void WriteUnkOffsetTA0(BinaryWriterEx bw) => UnkStructA0.Write(bw);
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class NPCWander : Part
            {
                private protected override PartType Type => PartType.NPCWander;
                private protected override bool HasUnkOffsetT50 => true;
                private protected override bool HasUnkOffsetT58 => false;
                private protected override bool HasUnkOffsetT70 => true;
                private protected override bool HasUnkOffsetT78 => false;
                private protected override bool HasUnkOffsetT80 => true;
                private protected override bool HasUnkOffsetT88 => true;
                private protected override bool HasUnkOffsetT90 => true;
                private protected override bool HasUnkOffsetT98 => true;
                private protected override bool HasUnkOffsetTA0 => true;

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct50 UnkStruct50 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct70 UnkStruct70 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct80 UnkStruct80 { get; set; }

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
                public NPCWander() : base("")
                {
                    UnkStruct50 = new UnkStruct50();
                    UnkStruct70 = new UnkStruct70();
                    UnkStruct80 = new UnkStruct80();
                    UnkStruct88 = new UnkStruct88();
                    UnkStruct90 = new UnkStruct90();
                    UnkStruct98 = new UnkStruct98();
                    UnkStructA0 = new UnkStructA0();
                }

                private protected override void DeepCopyTo(Part part)
                {
                    var piece = (NPCWander)part;
                    piece.UnkStruct50 = UnkStruct50.DeepCopy();
                    piece.UnkStruct70 = UnkStruct70.DeepCopy();
                    piece.UnkStruct80 = UnkStruct80.DeepCopy();
                    piece.UnkStruct88 = UnkStruct88.DeepCopy();
                    piece.UnkStruct90 = UnkStruct90.DeepCopy();
                    piece.UnkStruct98 = UnkStruct98.DeepCopy();
                    piece.UnkStructA0 = UnkStructA0.DeepCopy();
                }

                internal NPCWander(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                }

                private protected override void ReadUnkOffsetT50(BinaryReaderEx br) => UnkStruct50 = new UnkStruct50(br);
                private protected override void ReadUnkOffsetT70(BinaryReaderEx br) => UnkStruct70 = new UnkStruct70(br);
                private protected override void ReadUnkOffsetT80(BinaryReaderEx br) => UnkStruct80 = new UnkStruct80(br);
                private protected override void ReadUnkOffsetT88(BinaryReaderEx br) => UnkStruct88 = new UnkStruct88(br);
                private protected override void ReadUnkOffsetT90(BinaryReaderEx br) => UnkStruct90 = new UnkStruct90(br);
                private protected override void ReadUnkOffsetT98(BinaryReaderEx br) => UnkStruct98 = new UnkStruct98(br);
                private protected override void ReadUnkOffsetTA0(BinaryReaderEx br) => UnkStructA0 = new UnkStructA0(br);

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                }

                private protected override void WriteUnkOffsetT50(BinaryWriterEx bw) => UnkStruct50.Write(bw);
                private protected override void WriteUnkOffsetT70(BinaryWriterEx bw) => UnkStruct70.Write(bw);
                private protected override void WriteUnkOffsetT80(BinaryWriterEx bw) => UnkStruct80.Write(bw);
                private protected override void WriteUnkOffsetT88(BinaryWriterEx bw) => UnkStruct88.Write(bw);
                private protected override void WriteUnkOffsetT90(BinaryWriterEx bw) => UnkStruct90.Write(bw);
                private protected override void WriteUnkOffsetT98(BinaryWriterEx bw) => UnkStruct98.Write(bw);
                private protected override void WriteUnkOffsetTA0(BinaryWriterEx bw) => UnkStructA0.Write(bw);
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class Protoboss : Part
            {
                private protected override PartType Type => PartType.Protoboss;
                private protected override bool HasUnkOffsetT50 => true;
                private protected override bool HasUnkOffsetT58 => false;
                private protected override bool HasUnkOffsetT70 => true;
                private protected override bool HasUnkOffsetT78 => false;
                private protected override bool HasUnkOffsetT80 => true;
                private protected override bool HasUnkOffsetT88 => true;
                private protected override bool HasUnkOffsetT90 => true;
                private protected override bool HasUnkOffsetT98 => true;
                private protected override bool HasUnkOffsetTA0 => true;

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct50 UnkStruct50 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct70 UnkStruct70 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct80 UnkStruct80 { get; set; }

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
                public Protoboss() : base("")
                {
                    UnkStruct50 = new UnkStruct50();
                    UnkStruct70 = new UnkStruct70();
                    UnkStruct80 = new UnkStruct80();
                    UnkStruct88 = new UnkStruct88();
                    UnkStruct90 = new UnkStruct90();
                    UnkStruct98 = new UnkStruct98();
                    UnkStructA0 = new UnkStructA0();
                }

                private protected override void DeepCopyTo(Part part)
                {
                    var piece = (Protoboss)part;
                    piece.UnkStruct50 = UnkStruct50.DeepCopy();
                    piece.UnkStruct70 = UnkStruct70.DeepCopy();
                    piece.UnkStruct80 = UnkStruct80.DeepCopy();
                    piece.UnkStruct88 = UnkStruct88.DeepCopy();
                    piece.UnkStruct90 = UnkStruct90.DeepCopy();
                    piece.UnkStruct98 = UnkStruct98.DeepCopy();
                    piece.UnkStructA0 = UnkStructA0.DeepCopy();
                }

                internal Protoboss(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                }

                private protected override void ReadUnkOffsetT50(BinaryReaderEx br) => UnkStruct50 = new UnkStruct50(br);
                private protected override void ReadUnkOffsetT70(BinaryReaderEx br) => UnkStruct70 = new UnkStruct70(br);
                private protected override void ReadUnkOffsetT80(BinaryReaderEx br) => UnkStruct80 = new UnkStruct80(br);
                private protected override void ReadUnkOffsetT88(BinaryReaderEx br) => UnkStruct88 = new UnkStruct88(br);
                private protected override void ReadUnkOffsetT90(BinaryReaderEx br) => UnkStruct90 = new UnkStruct90(br);
                private protected override void ReadUnkOffsetT98(BinaryReaderEx br) => UnkStruct98 = new UnkStruct98(br);
                private protected override void ReadUnkOffsetTA0(BinaryReaderEx br) => UnkStructA0 = new UnkStructA0(br);

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                }

                private protected override void WriteUnkOffsetT50(BinaryWriterEx bw) => UnkStruct50.Write(bw);
                private protected override void WriteUnkOffsetT70(BinaryWriterEx bw) => UnkStruct70.Write(bw);
                private protected override void WriteUnkOffsetT80(BinaryWriterEx bw) => UnkStruct80.Write(bw);
                private protected override void WriteUnkOffsetT88(BinaryWriterEx bw) => UnkStruct88.Write(bw);
                private protected override void WriteUnkOffsetT90(BinaryWriterEx bw) => UnkStruct90.Write(bw);
                private protected override void WriteUnkOffsetT98(BinaryWriterEx bw) => UnkStruct98.Write(bw);
                private protected override void WriteUnkOffsetTA0(BinaryWriterEx bw) => UnkStructA0.Write(bw);
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class Navmesh : Part
            {
                private protected override PartType Type => PartType.Navmesh;
                private protected override bool HasUnkOffsetT50 => true;
                private protected override bool HasUnkOffsetT58 => false;
                private protected override bool HasUnkOffsetT70 => true;
                private protected override bool HasUnkOffsetT78 => false;
                private protected override bool HasUnkOffsetT80 => true;
                private protected override bool HasUnkOffsetT88 => true;
                private protected override bool HasUnkOffsetT90 => true;
                private protected override bool HasUnkOffsetT98 => true;
                private protected override bool HasUnkOffsetTA0 => true;

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct50 UnkStruct50 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct70 UnkStruct70 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct80 UnkStruct80 { get; set; }

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
                public Navmesh() : base("")
                {
                    UnkStruct50 = new UnkStruct50();
                    UnkStruct70 = new UnkStruct70();
                    UnkStruct80 = new UnkStruct80();
                    UnkStruct88 = new UnkStruct88();
                    UnkStruct90 = new UnkStruct90();
                    UnkStruct98 = new UnkStruct98();
                    UnkStructA0 = new UnkStructA0();
                }

                private protected override void DeepCopyTo(Part part)
                {
                    var piece = (Navmesh)part;
                    piece.UnkStruct50 = UnkStruct50.DeepCopy();
                    piece.UnkStruct70 = UnkStruct70.DeepCopy();
                    piece.UnkStruct80 = UnkStruct80.DeepCopy();
                    piece.UnkStruct88 = UnkStruct88.DeepCopy();
                    piece.UnkStruct90 = UnkStruct90.DeepCopy();
                    piece.UnkStruct98 = UnkStruct98.DeepCopy();
                    piece.UnkStructA0 = UnkStructA0.DeepCopy();
                }

                internal Navmesh(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                }

                private protected override void ReadUnkOffsetT50(BinaryReaderEx br) => UnkStruct50 = new UnkStruct50(br);
                private protected override void ReadUnkOffsetT70(BinaryReaderEx br) => UnkStruct70 = new UnkStruct70(br);
                private protected override void ReadUnkOffsetT80(BinaryReaderEx br) => UnkStruct80 = new UnkStruct80(br);
                private protected override void ReadUnkOffsetT88(BinaryReaderEx br) => UnkStruct88 = new UnkStruct88(br);
                private protected override void ReadUnkOffsetT90(BinaryReaderEx br) => UnkStruct90 = new UnkStruct90(br);
                private protected override void ReadUnkOffsetT98(BinaryReaderEx br) => UnkStruct98 = new UnkStruct98(br);
                private protected override void ReadUnkOffsetTA0(BinaryReaderEx br) => UnkStructA0 = new UnkStructA0(br);

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                }

                private protected override void WriteUnkOffsetT50(BinaryWriterEx bw) => UnkStruct50.Write(bw);
                private protected override void WriteUnkOffsetT70(BinaryWriterEx bw) => UnkStruct70.Write(bw);
                private protected override void WriteUnkOffsetT80(BinaryWriterEx bw) => UnkStruct80.Write(bw);
                private protected override void WriteUnkOffsetT88(BinaryWriterEx bw) => UnkStruct88.Write(bw);
                private protected override void WriteUnkOffsetT90(BinaryWriterEx bw) => UnkStruct90.Write(bw);
                private protected override void WriteUnkOffsetT98(BinaryWriterEx bw) => UnkStruct98.Write(bw);
                private protected override void WriteUnkOffsetTA0(BinaryWriterEx bw) => UnkStructA0.Write(bw);
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class Invalid : Part
            {
                private protected override PartType Type => PartType.Invalid;
                private protected override bool HasUnkOffsetT50 => true;
                private protected override bool HasUnkOffsetT58 => false;
                private protected override bool HasUnkOffsetT70 => true;
                private protected override bool HasUnkOffsetT78 => false;
                private protected override bool HasUnkOffsetT80 => true;
                private protected override bool HasUnkOffsetT88 => true;
                private protected override bool HasUnkOffsetT90 => true;
                private protected override bool HasUnkOffsetT98 => true;
                private protected override bool HasUnkOffsetTA0 => true;

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct50 UnkStruct50 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct70 UnkStruct70 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct80 UnkStruct80 { get; set; }

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
                public Invalid() : base("")
                {
                    UnkStruct50 = new UnkStruct50();
                    UnkStruct70 = new UnkStruct70();
                    UnkStruct80 = new UnkStruct80();
                    UnkStruct88 = new UnkStruct88();
                    UnkStruct90 = new UnkStruct90();
                    UnkStruct98 = new UnkStruct98();
                    UnkStructA0 = new UnkStructA0();
                }

                private protected override void DeepCopyTo(Part part)
                {
                    var piece = (Invalid)part;
                    piece.UnkStruct50 = UnkStruct50.DeepCopy();
                    piece.UnkStruct70 = UnkStruct70.DeepCopy();
                    piece.UnkStruct80 = UnkStruct80.DeepCopy();
                    piece.UnkStruct88 = UnkStruct88.DeepCopy();
                    piece.UnkStruct90 = UnkStruct90.DeepCopy();
                    piece.UnkStruct98 = UnkStruct98.DeepCopy();
                    piece.UnkStructA0 = UnkStructA0.DeepCopy();
                }

                internal Invalid(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                }

                private protected override void ReadUnkOffsetT50(BinaryReaderEx br) => UnkStruct50 = new UnkStruct50(br);
                private protected override void ReadUnkOffsetT70(BinaryReaderEx br) => UnkStruct70 = new UnkStruct70(br);
                private protected override void ReadUnkOffsetT80(BinaryReaderEx br) => UnkStruct80 = new UnkStruct80(br);
                private protected override void ReadUnkOffsetT88(BinaryReaderEx br) => UnkStruct88 = new UnkStruct88(br);
                private protected override void ReadUnkOffsetT90(BinaryReaderEx br) => UnkStruct90 = new UnkStruct90(br);
                private protected override void ReadUnkOffsetT98(BinaryReaderEx br) => UnkStruct98 = new UnkStruct98(br);
                private protected override void ReadUnkOffsetTA0(BinaryReaderEx br) => UnkStructA0 = new UnkStructA0(br);

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                }

                private protected override void WriteUnkOffsetT50(BinaryWriterEx bw) => UnkStruct50.Write(bw);
                private protected override void WriteUnkOffsetT70(BinaryWriterEx bw) => UnkStruct70.Write(bw);
                private protected override void WriteUnkOffsetT80(BinaryWriterEx bw) => UnkStruct80.Write(bw);
                private protected override void WriteUnkOffsetT88(BinaryWriterEx bw) => UnkStruct88.Write(bw);
                private protected override void WriteUnkOffsetT90(BinaryWriterEx bw) => UnkStruct90.Write(bw);
                private protected override void WriteUnkOffsetT98(BinaryWriterEx bw) => UnkStruct98.Write(bw);
                private protected override void WriteUnkOffsetTA0(BinaryWriterEx bw) => UnkStructA0.Write(bw);
            }
        }
    }
}
