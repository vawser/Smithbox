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
            WarpCollision = 6,
        }

        public enum SoundSpaceReverbType : byte
        {
            Outdoors = 0,
            Small_Room = 1,
            Medium_Room = 2,
            Large_Room = 3,
            Giant_Room = 4
        }

        public enum SoundSpaceDelayType : byte
        {
            Outdoors = 0,
            Small_Room = 1,
            Medium_Room = 2,
            Large_Room = 3,
            Giant_Room = 4
        }

        public enum AmbientSfxAttachType : byte
        {
            Position = 0,
            Position_and_Angle = 1,
            Position_and_Y_axis_Angle = 2
        }

        public enum HitFilterType : byte
        {
            Automatic = 255,
            Map = 100,
            Fall_Death = 1,
            Fall_Death_No_Camera_Disconnection = 2,
            Fall_Death_on_Landing = 3,
            Fall_Death_on_Landing_No_Camera_Disconnection = 4,
            Unknown_5 = 5,
            Camera_Disconnection = 10,
            Unknown_11 = 11,
            Reflect_Draw_Groups_Only = 20,
            Per_Enemy_Memory_Block_Boundary = 30,
            Per_Enemy_Area_Boundary = 31,
            Per_Water_Surface = 40,
            Unknown_50 = 50,
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
            /// Warp connections to other maps.
            /// </summary>
            public List<Part.WarpCollision> WarpCollisions { get; set; }

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
                WarpCollisions = new List<Part.WarpCollision>();
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
                    case Part.WarpCollision p: WarpCollisions.Add(p); break;

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
                    MapPieces, Objects, Collisions, Navmeshes, ConnectCollisions, WarpCollisions);
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

                    case PartType.WarpCollision:
                        return WarpCollisions.EchoAdd(new Part.WarpCollision(br));

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

            public uint[] DrawGroups { get; private set; }
            public uint[] NvmGroups { get; private set; }
            public uint[] DispGroups { get; private set; }

            public int LightID { get; set; }
            public int FogID { get; set; }
            public byte DisableFog { get; set; }

            private byte Unk6D { get; set; }
            private byte Unk6E { get; set; }
            private byte Unk6F { get; set; }

            private protected Part(string name)
            {
                Name = name;
                Scale = Vector3.One;
                DrawGroups = new uint[4] {
                    0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF };
                NvmGroups = new uint[4] {
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
                part.NvmGroups = (uint[])NvmGroups.Clone();
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
                NvmGroups = br.ReadUInt32s(4);
                DispGroups = br.ReadUInt32s(4);

                LightID = br.ReadInt32();
                FogID = br.ReadInt32();
                DisableFog = br.ReadByte();
                Unk6D = br.ReadByte();
                Unk6E = br.ReadByte();
                Unk6F = br.ReadByte();

                long typeDataOffset = br.ReadVarint();
                if (br.VarintLong)
                    br.AssertInt64(0);

                if (!BinaryReaderEx.IgnoreAsserts)
                {
                    if (nameOffset == 0)
                        throw new InvalidDataException($"{nameof(nameOffset)} must not be 0 in type {GetType()}.");
                    if (typeDataOffset == 0)
                        throw new InvalidDataException($"{nameof(typeDataOffset)} must not be 0 in type {GetType()}.");
                }

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
                bw.WriteUInt32s(NvmGroups);
                bw.WriteUInt32s(DispGroups);

                bw.WriteInt32(LightID);
                bw.WriteInt32(FogID);
                bw.WriteByte(DisableFog);

                bw.WriteByte(Unk6D);
                bw.WriteByte(Unk6E);
                bw.WriteByte(Unk6F);

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

                private short UnkT00 { get; set; }
                private byte UnkT02 { get; set; }

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

                public uint ObjectInstanceID { get; set; }

                private short BonfireID { get; set; }

                /// <summary>
                /// Creates an Object with default values.
                /// </summary>
                public Object() : base("oXX_XXXX_XXXX") { }

                internal Object(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    ObjectInstanceID = br.ReadUInt32();
                    BonfireID = br.ReadInt16();
                    br.AssertInt16(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteUInt32(ObjectInstanceID);
                    bw.WriteInt16(BonfireID);
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
                /// Navmesh group 0
                /// </summary>
                public uint NvmGroup0 { get; set; }

                /// <summary>
                /// Navmesh group 1
                /// </summary>
                public uint NvmGroup1 { get; set; }

                /// <summary>
                /// Navmesh group 2
                /// </summary>
                public uint NvmGroup2 { get; set; }

                /// <summary>
                /// Navmesh group 3
                /// </summary>
                public uint NvmGroup3 { get; set; }

                /// <summary>
                /// The sound space reverb type.
                /// </summary>
                public SoundSpaceReverbType SoundSpaceReverbType { get; set; }

                /// <summary>
                /// The sound space delay type.
                /// </summary>
                public SoundSpaceDelayType SoundSpaceDelayType { get; set; }

                /// <summary>
                /// Memory block ID
                /// </summary>
                public sbyte MemoryBlockID { get; set; }

                /// <summary>
                /// The fltparam ID to apply whne ont his collision.
                /// </summary>
                public byte FilterID { get; set; }

                /// <summary>
                /// The hit filter to apply for this collision.
                /// </summary>
                public HitFilterType HitFilterID { get; set; }

                /// <summary>
                /// The ambient SFX attachment type within this collision.
                /// </summary>
                public AmbientSfxAttachType AmbientSfxAttachType { get; set; }

                /// <summary>
                /// Whether to disable invasions when on this collision.
                /// </summary>
                public byte DisableInvasion { get; set; }

                /// <summary>
                /// The ambient SFX ID for this collision.
                /// </summary>
                public int AmbientSfxID { get; set; }

                /// <summary>
                /// The player light SFX ID for this collision.
                /// </summary>
                public int PlayerLightSfxID { get; set; }

                /// <summary>
                /// The play area ID for this collision.
                /// </summary>                
                public int PlayAreaID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte GameBrightness0 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte GameBrightness1 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public sbyte EnemyWallMemoryBlockID_A { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public sbyte EnemyWallMemoryBlockID_B { get; set; }

                /// <summary>
                /// ID of tpf in menu\tex\icon\mapname to use for area name banner.
                /// ID is also interpreted for mapname FMG for load game menu text (ID example: 102510 = FMG 10250001).
                /// </summary>
                public int MapNameID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte DelayMapNameDisplay { get; set; }

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
                    NvmGroup0 = br.ReadUInt32();
                    NvmGroup1 = br.ReadUInt32();
                    NvmGroup2 = br.ReadUInt32();
                    NvmGroup3 = br.ReadUInt32();

                    SoundSpaceReverbType = br.ReadEnum8<SoundSpaceReverbType>();
                    SoundSpaceDelayType = br.ReadEnum8<SoundSpaceDelayType>();

                    MemoryBlockID = br.ReadSByte();
                    FilterID = br.ReadByte();
                    HitFilterID = br.ReadEnum8<HitFilterType>();
                    AmbientSfxAttachType = br.ReadEnum8<AmbientSfxAttachType>();
                    br.AssertByte(0);

                    DisableInvasion = br.ReadByte();

                    AmbientSfxID = br.ReadInt32();
                    PlayerLightSfxID = br.ReadInt32();
                    PlayAreaID = br.ReadInt32();

                    GameBrightness0 = br.ReadByte();
                    GameBrightness1 = br.ReadByte();
                    EnemyWallMemoryBlockID_A = br.ReadSByte();
                    EnemyWallMemoryBlockID_B = br.ReadSByte();

                    MapNameID = br.ReadInt32();
                    DelayMapNameDisplay = br.ReadByte();

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
                    bw.WriteUInt32(NvmGroup0);
                    bw.WriteUInt32(NvmGroup1);
                    bw.WriteUInt32(NvmGroup2);
                    bw.WriteUInt32(NvmGroup3);

                    bw.WriteByte((byte)SoundSpaceReverbType);
                    bw.WriteByte((byte)SoundSpaceDelayType);

                    bw.WriteSByte(MemoryBlockID);
                    bw.WriteByte(FilterID);
                    bw.WriteByte((byte)HitFilterID);
                    bw.WriteByte((byte)AmbientSfxAttachType);
                    bw.WriteByte(0);

                    bw.WriteByte(DisableInvasion);

                    bw.WriteInt32(AmbientSfxID);
                    bw.WriteInt32(PlayerLightSfxID);
                    bw.WriteInt32(PlayAreaID);

                    bw.WriteByte(GameBrightness0);
                    bw.WriteByte(GameBrightness1);
                    bw.WriteSByte(EnemyWallMemoryBlockID_A);
                    bw.WriteSByte(EnemyWallMemoryBlockID_B);

                    bw.WriteInt32(MapNameID);

                    bw.WriteByte(DelayMapNameDisplay);
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

                public int NavmeshGroup0 { get; set; }
                public int NavmeshGroup1 { get; set; }
                public int NavmeshGroup2 { get; set; }
                public int NavmeshGroup3 { get; set; }

                /// <summary>
                /// Creates a Navmesh with default values.
                /// </summary>
                public Navmesh() : base("nXX_XXXX_XXXX") { }

                internal Navmesh(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    NavmeshGroup0 = br.ReadInt32();
                    NavmeshGroup1 = br.ReadInt32();
                    NavmeshGroup2 = br.ReadInt32();
                    NavmeshGroup3 = br.ReadInt32();
                    br.AssertPattern(0x10, 0x00);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(NavmeshGroup0);
                    bw.WriteInt32(NavmeshGroup1);
                    bw.WriteInt32(NavmeshGroup2);
                    bw.WriteInt32(NavmeshGroup3);
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

                private int UnkT08 { get; set; }
                private byte UnkT0C { get; set; }

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

            public class WarpCollision : Part
            {
                private protected override PartType Type => PartType.WarpCollision;

                /// <summary>
                /// Name of the referenced collision part.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Collision))]
                [NoRenderGroupInheritence()]
                public string CollisionName { get; set; }
                public int CollisionIndex { get; set; }

                private float Offset { get; set; }

                public WarpCollision() : base("hXX_XXXX_XXXX")
                {
                }

                private protected override void DeepCopyTo(Part part)
                {
                    var connect = (WarpCollision)part;
                }

                internal WarpCollision(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    CollisionIndex = br.ReadInt32();
                    Offset = br.ReadSingle();
                    // TODO: may be missing padding elements, this was purely drawn from the macbin
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(CollisionIndex);
                    bw.WriteSingle(Offset);
                    // TODO: may be missing padding elements, this was purely drawn from the macbin
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
