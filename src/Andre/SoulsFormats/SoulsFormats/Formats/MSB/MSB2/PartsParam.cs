using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace SoulsFormats
{
    public partial class MSB2
    {
        internal enum PartType : byte
        {
            MapPiece = 0,
            Object = 1,
            Collision = 3,
            Navmesh = 4,
            ConnectCollision = 5,
        }

        /// <summary>
        /// Concrete map elements.
        /// </summary>
        public class PartsParam : Param<Part>, IMsbParam<IMsbPart>
        {
            internal override int Version => 5;
            internal override string Name => "PARTS_PARAM_ST";

            /// <summary>
            /// Visible but intangible models.
            /// </summary>
            public List<Part.MapPiece> MapPieces { get; set; }

            /// <summary>
            /// Dynamic or interactible elements.
            /// </summary>
            public List<Part.Object> Objects { get; set; }

            /// <summary>
            /// Invisible but physical surfaces.
            /// </summary>
            public List<Part.Collision> Collisions { get; set; }

            /// <summary>
            /// AI navigation meshes.
            /// </summary>
            public List<Part.Navmesh> Navmeshes { get; set; }

            /// <summary>
            /// Connections to other maps.
            /// </summary>
            public List<Part.ConnectCollision> ConnectCollisions { get; set; }

            /// <summary>
            /// Creates an empty PartsParam.
            /// </summary>
            public PartsParam()
            {
                MapPieces = new List<Part.MapPiece>();
                Objects = new List<Part.Object>();
                Collisions = new List<Part.Collision>();
                Navmeshes = new List<Part.Navmesh>();
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
                    case Part.Collision p: Collisions.Add(p); break;
                    case Part.Navmesh p: Navmeshes.Add(p); break;
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
                    MapPieces, Objects, Collisions, Navmeshes, ConnectCollisions);
            }
            IReadOnlyList<IMsbPart> IMsbParam<IMsbPart>.GetEntries() => GetEntries();

            internal override Part ReadEntry(BinaryReaderEx br)
            {
                PartType type = br.GetEnum8<PartType>(br.Position + br.VarintSize);
                switch (type)
                {
                    case PartType.MapPiece:
                        return MapPieces.EchoAdd(new Part.MapPiece(br));

                    case PartType.Object:
                        return Objects.EchoAdd(new Part.Object(br));

                    case PartType.Collision:
                        return Collisions.EchoAdd(new Part.Collision(br));

                    case PartType.Navmesh:
                        return Navmeshes.EchoAdd(new Part.Navmesh(br));

                    case PartType.ConnectCollision:
                        return ConnectCollisions.EchoAdd(new Part.ConnectCollision(br));

                    default:
                        throw new NotImplementedException($"Unimplemented part type: {type}");
                }
            }
        }

        /// <summary>
        /// A concrete map element.
        /// </summary>
        public abstract class Part : NamedEntry, IMsbPart
        {
            private protected abstract PartType Type { get; }

            /// <summary>
            /// The name of the part's model, referencing ModelParam.
            /// </summary>
            public string ModelName { get; set; }
            public short ModelIndex { get; set; }

            /// <summary>
            /// Location of the part.
            /// </summary>
            public Vector3 Position { get; set; }

            /// <summary>
            /// Rotation of the part, in degrees.
            /// </summary>
            public Vector3 Rotation { get; set; }

            /// <summary>
            /// Scale of the part; only supported for map pieces and objects.
            /// </summary>
            public Vector3 Scale { get; set; }

            /// <summary>
            /// Not confirmed; determines when the part is loaded.
            /// </summary>
            public uint[] DrawGroups { get; private set; }

            /// <summary>
            /// Unknown; possibly nvm groups.
            /// </summary>
            public int Unk44 { get; set; }

            /// <summary>
            /// Unknown; possibly nvm groups.
            /// </summary>
            public int Unk48 { get; set; }

            /// <summary>
            /// Unknown; possibly nvm groups.
            /// </summary>
            public int Unk4C { get; set; }

            /// <summary>
            /// Unknown; possibly nvm groups.
            /// </summary>
            public int Unk50 { get; set; }

            /// <summary>
            /// Not confirmed; determines when the part is visible.
            /// </summary>
            public uint[] DispGroups { get; private set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int Unk64 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte Unk6C { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte Unk6E { get; set; }

            private protected Part(string name)
            {
                Name = name;
                Scale = Vector3.One;
                DrawGroups = new uint[4] {
                    0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF };
                DispGroups = new uint[4] {
                    0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF };
            }

            /// <summary>
            /// Creates a deep copy of the part.
            /// </summary>
            public Part DeepCopy()
            {
                var part = (Part)MemberwiseClone();
                part.DrawGroups = (uint[])DrawGroups.Clone();
                part.DispGroups = (uint[])DispGroups.Clone();
                DeepCopyTo(part);
                return part;
            }
            IMsbPart IMsbPart.DeepCopy() => DeepCopy();

            private protected virtual void DeepCopyTo(Part part) { }

            private protected Part(BinaryReaderEx br)
            {
                long start = br.Position;
                long nameOffset = br.ReadVarint();
                br.AssertByte((byte)Type);
                br.AssertByte(0);
                br.ReadInt16(); // ID
                ModelIndex = br.ReadInt16();
                br.AssertInt16(0);
                Position = br.ReadVector3();
                Rotation = br.ReadVector3();
                Scale = br.ReadVector3();
                DrawGroups = br.ReadUInt32s(4);
                Unk44 = br.ReadInt32();
                Unk48 = br.ReadInt32();
                Unk4C = br.ReadInt32();
                Unk50 = br.ReadInt32();
                DispGroups = br.ReadUInt32s(4);
                Unk64 = br.ReadInt32();
                br.AssertInt32(0);
                Unk6C = br.ReadByte();
                br.AssertByte(0);
                Unk6E = br.ReadByte();
                br.AssertByte(0);
                long typeDataOffset = br.ReadVarint();
                if (br.VarintLong)
                    br.AssertInt64(0);

                if (nameOffset == 0)
                    throw new InvalidDataException($"{nameof(nameOffset)} must not be 0 in type {GetType()}.");
                if (typeDataOffset == 0)
                    throw new InvalidDataException($"{nameof(typeDataOffset)} must not be 0 in type {GetType()}.");

                br.Position = start + nameOffset;
                Name = br.GetUTF16(start + nameOffset);

                br.Position = start + typeDataOffset;
                ReadTypeData(br);
            }

            private protected abstract void ReadTypeData(BinaryReaderEx br);

            internal override void Write(BinaryWriterEx bw, int id)
            {
                long start = bw.Position;
                bw.ReserveVarint("NameOffset");
                bw.WriteByte((byte)Type);
                bw.WriteByte(0);
                bw.WriteInt16((short)id);
                bw.WriteInt16(ModelIndex);
                bw.WriteInt16(0);
                bw.WriteVector3(Position);
                bw.WriteVector3(Rotation);
                bw.WriteVector3(Scale);
                bw.WriteUInt32s(DrawGroups);
                bw.WriteInt32(Unk44);
                bw.WriteInt32(Unk48);
                bw.WriteInt32(Unk4C);
                bw.WriteInt32(Unk50);
                bw.WriteUInt32s(DispGroups);
                bw.WriteInt32(Unk64);
                bw.WriteInt32(0);
                bw.WriteByte(Unk6C);
                bw.WriteByte(0);
                bw.WriteByte(Unk6E);
                bw.WriteByte(0);
                bw.ReserveVarint("TypeDataOffset");
                if (bw.VarintLong)
                    bw.WriteInt64(0);

                long nameStart = bw.Position;
                int namePad = bw.VarintLong ? 0x20 : 0x2C;
                bw.FillVarint("NameOffset", nameStart - start);
                bw.WriteUTF16(MSB.ReambiguateName(Name), true);
                if (bw.Position - nameStart < namePad)
                    bw.Position += namePad - (bw.Position - nameStart);
                bw.Pad(bw.VarintSize);

                bw.FillVarint("TypeDataOffset", bw.Position - start);
                WriteTypeData(bw);
            }

            private protected abstract void WriteTypeData(BinaryWriterEx bw);

            internal virtual void GetNames(MSB2 msb, Entries entries)
            {
                ModelName = MSB.FindName(entries.Models, ModelIndex);
            }

            internal virtual void GetIndices(Lookups lookups)
            {
                ModelIndex = (short)FindIndex(lookups.Models, ModelName);
            }

            /// <summary>
            /// Returns a string representation of the part.
            /// </summary>
            public override string ToString()
            {
                return $"{Type} \"{Name}\"";
            }

            /// <summary>
            /// A visible but intangible model.
            /// </summary>
            public class MapPiece : Part
            {
                private protected override PartType Type => PartType.MapPiece;

                /// <summary>
                /// Unknown.
                /// </summary>
                public short UnkT00 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT02 { get; set; }

                /// <summary>
                /// Creates a MapPiece with default values.
                /// </summary>
                public MapPiece() : base("mXXXX_XXXX") { }

                internal MapPiece(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    UnkT00 = br.ReadInt16();
                    UnkT02 = br.ReadByte();
                    br.AssertByte(0);
                    if (br.VarintLong)
                        br.AssertInt32(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt16(UnkT00);
                    bw.WriteByte(UnkT02);
                    bw.WriteByte(0);
                    if (bw.VarintLong)
                        bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// A dynamic or interactible element.
            /// </summary>
            public class Object : Part
            {
                private protected override PartType Type => PartType.Object;

                /// <summary>
                /// Unknown.
                /// </summary>
                public int MapObjectInstanceParamID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short MapObjectBonfireParamID { get; set; }

                /// <summary>
                /// Creates an Object with default values.
                /// </summary>
                public Object() : base("oXX_XXXX_XXXX") { }

                internal Object(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    MapObjectInstanceParamID = br.ReadInt32();
                    MapObjectBonfireParamID = br.ReadInt16();
                    br.AssertInt16(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(MapObjectInstanceParamID);
                    bw.WriteInt16(MapObjectBonfireParamID);
                    bw.WriteInt16(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// An invisible but physical surface that controls map loading and graphics settings, among other things.
            /// </summary>
            public class Collision : Part
            {
                private protected override PartType Type => PartType.Collision;

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT00 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT04 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT08 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT0C { get; set; }

                /// <summary>
                /// Modifies sounds while the player is touching this collision.
                /// </summary>
                public byte SoundSpaceType { get; set; } = 0;

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT11 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT12 { get; set; }

                /// <summary>
                /// Refers to ID of fltparam to determine visual effects.
                /// </summary>
                public byte FilterParamID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte HitFilterID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT15 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT17 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int CameraSfxID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int PlayerLightParamID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>                
                public int PlayAreaParamID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT26 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT27 { get; set; }

                /// <summary>
                /// ID of tpf in menu\tex\icon\mapname to use for area name banner.
                /// ID is also interpreted for mapname FMG for load game menu text (ID example: 102510 = FMG 10250001).
                /// </summary>
                public int MapNameID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT2C { get; set; }

                /// <summary>
                /// ID of tpf in model\map\envbnd to use for cubemaps.
                /// </summary>
                public short CubeEnvID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int CameraExFollowParamID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT35 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short UnkT36 { get; set; }

                /// <summary>
                /// Shared identifier. Checked by ESD HitGroup commands.
                /// </summary>
                public int HitGroupID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT40 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT44 { get; set; }

                /// <summary>
                /// Creates a Collision with default values.
                /// </summary>
                public Collision() : base("hXX_XXXX_XXXX") { }

                internal Collision(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    UnkT00 = br.ReadInt32();
                    UnkT04 = br.ReadInt32();
                    UnkT08 = br.ReadInt32();
                    UnkT0C = br.ReadInt32();
                    SoundSpaceType = br.ReadByte();
                    UnkT11 = br.ReadByte();
                    UnkT12 = br.ReadByte();
                    FilterParamID = br.ReadByte();
                    HitFilterID = br.ReadByte();
                    UnkT15 = br.ReadByte();
                    br.AssertByte(0);
                    UnkT17 = br.ReadByte();
                    CameraSfxID = br.ReadInt32();
                    PlayerLightParamID = br.ReadInt32();
                    PlayAreaParamID = br.ReadInt32();
                    br.AssertInt16(0);
                    UnkT26 = br.ReadByte();
                    UnkT27 = br.ReadByte();
                    MapNameID = br.ReadInt32();
                    UnkT2C = br.ReadByte();
                    br.AssertByte(0);
                    CubeEnvID = br.ReadInt16();
                    CameraExFollowParamID = br.ReadInt32();
                    br.AssertByte(0);
                    UnkT35 = br.ReadByte();
                    UnkT36 = br.ReadInt16();
                    br.AssertInt32(0);
                    HitGroupID = br.ReadInt32();
                    UnkT40 = br.ReadByte();
                    br.AssertByte(0);
                    br.AssertByte(0);
                    br.AssertByte(0);
                    UnkT44 = br.ReadInt32();
                    br.AssertPattern(0x10, 0x00);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(UnkT00);
                    bw.WriteInt32(UnkT04);
                    bw.WriteInt32(UnkT08);
                    bw.WriteInt32(UnkT0C);
                    bw.WriteByte((byte)SoundSpaceType);
                    bw.WriteByte(UnkT11);
                    bw.WriteByte(UnkT12);
                    bw.WriteByte(FilterParamID);
                    bw.WriteByte(HitFilterID);
                    bw.WriteByte(UnkT15);
                    bw.WriteByte(0);
                    bw.WriteByte(UnkT17);
                    bw.WriteInt32(CameraSfxID);
                    bw.WriteInt32(PlayerLightParamID);
                    bw.WriteInt32(PlayAreaParamID);
                    bw.WriteInt16(0);
                    bw.WriteByte(UnkT26);
                    bw.WriteByte(UnkT27);
                    bw.WriteInt32(MapNameID);
                    bw.WriteByte(UnkT2C);
                    bw.WriteByte(0);
                    bw.WriteInt16(CubeEnvID);
                    bw.WriteInt32(CameraExFollowParamID);
                    bw.WriteByte(0);
                    bw.WriteByte(UnkT35);
                    bw.WriteInt16(UnkT36);
                    bw.WriteInt32(0);
                    bw.WriteInt32(HitGroupID);
                    bw.WriteByte(UnkT40);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteInt32(UnkT44);
                    bw.WritePattern(0x10, 0x00);
                }
            }

            /// <summary>
            /// An AI navigation mesh.
            /// </summary>
            public class Navmesh : Part
            {
                private protected override PartType Type => PartType.Navmesh;

                /// <summary>
                /// Unknown; possibly nvm groups.
                /// </summary>
                public int UnkT00 { get; set; }

                /// <summary>
                /// Unknown; possibly nvm groups.
                /// </summary>
                public int UnkT04 { get; set; }

                /// <summary>
                /// Unknown; possibly nvm groups.
                /// </summary>
                public int UnkT08 { get; set; }

                /// <summary>
                /// Unknown; possibly nvm groups.
                /// </summary>
                public int UnkT0C { get; set; }

                /// <summary>
                /// Creates a Navmesh with default values.
                /// </summary>
                public Navmesh() : base("nXX_XXXX_XXXX") { }

                internal Navmesh(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    UnkT00 = br.ReadInt32();
                    UnkT04 = br.ReadInt32();
                    UnkT08 = br.ReadInt32();
                    UnkT0C = br.ReadInt32();
                    br.AssertPattern(0x10, 0x00);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(UnkT00);
                    bw.WriteInt32(UnkT04);
                    bw.WriteInt32(UnkT08);
                    bw.WriteInt32(UnkT0C);
                    bw.WritePattern(0x10, 0x00);
                }
            }

            /// <summary>
            /// Causes another map to be loaded when standing on the referenced collision.
            /// </summary>
            public class ConnectCollision : Part
            {
                private protected override PartType Type => PartType.ConnectCollision;

                /// <summary>
                /// Name of the referenced collision part.
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
                /// Unknown.
                /// </summary>
                public int UnkT08 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT0C { get; set; }

                /// <summary>
                /// Creates a ConnectCollision with default values.
                /// </summary>
                public ConnectCollision() : base("hXX_XXXX_XXXX")
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
                    UnkT08 = br.ReadInt32();
                    UnkT0C = br.ReadByte();
                    br.AssertByte(0);
                    br.AssertByte(0);
                    br.AssertByte(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(CollisionIndex);
                    bw.WriteBytes(MapID);
                    bw.WriteInt32(UnkT08);
                    bw.WriteByte(UnkT0C);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                }

                internal override void GetNames(MSB2 msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    CollisionName = MSB.FindName(msb.Parts.Collisions, CollisionIndex);
                }

                internal override void GetIndices(Lookups lookups)
                {
                    base.GetIndices(lookups);
                    CollisionIndex = FindIndex(lookups.Collisions, CollisionName);
                }
            }
        }
    }
}
