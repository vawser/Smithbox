using Org.BouncyCastle.Bcpg.Sig;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Numerics;

namespace SoulsFormats
{
    public partial class MSB2
    {
        internal enum RegionType : byte
        {
            Region0 = 0,
            Light = 3,
            StartPoint = 5,
            Sound = 7,
            SFX = 9,
            Wind = 13,
            EnvLight = 14,
            Fog = 15,
        }

        public enum LightType : sbyte
        {
            Omni = 0,
            Spot = 1,
        }

        public enum SoundType : int
        {
            Environmental = 0,
            BGM = 1,
        }

        public enum WindType : sbyte
        {
            Omni = 0,
            Target = 1,
        }

        public enum WindTargetType : sbyte
        {
            All = 0,
            ClothOnly = 1,
            SfxOnly = 2,
            ModelOnly = 3,
        }

        /// <summary>
        /// Points or volumes that trigger some behavior.
        /// </summary>
        public class PointParam : Param<Region>, IMsbParam<IMsbRegion>
        {
            internal override int Version => 5;
            internal override string Name => "POINT_PARAM_ST";

            /// <summary>
            /// Unknown, possibly walk points for enemies.
            /// </summary>
            public List<Region.Region0> Region0s { get; set; }

            /// <summary>
            /// Unknown if these do anything.
            /// </summary>
            public List<Region.Light> Lights { get; set; }

            /// <summary>
            /// Unknown, presumably the default position for spawning into the map.
            /// </summary>
            public List<Region.StartPoint> StartPoints { get; set; }

            /// <summary>
            /// Sound effects that play in certain areas.
            /// </summary>
            public List<Region.Sound> Sounds { get; set; }

            /// <summary>
            /// Special effects that play at certain areas.
            /// </summary>
            public List<Region.SFX> SFXs { get; set; }

            /// <summary>
            /// Unknown, presumably set wind speed/direction.
            /// </summary>
            public List<Region.Wind> Winds { get; set; }

            /// <summary>
            /// Unknown, names mention lightmaps and GI.
            /// </summary>
            public List<Region.EnvLight> EnvLights { get; set; }

            /// <summary>
            /// Unknown if these do anything.
            /// </summary>
            public List<Region.Fog> Fogs { get; set; }

            /// <summary>
            /// Creates an empty PointParam.
            /// </summary>
            public PointParam()
            {
                Region0s = new List<Region.Region0>();
                Lights = new List<Region.Light>();
                StartPoints = new List<Region.StartPoint>();
                Sounds = new List<Region.Sound>();
                SFXs = new List<Region.SFX>();
                Winds = new List<Region.Wind>();
                EnvLights = new List<Region.EnvLight>();
                Fogs = new List<Region.Fog>();
            }

            /// <summary>
            /// Adds a region to the appropriate list for its type; returns the region.
            /// </summary>
            public Region Add(Region region)
            {
                switch (region)
                {
                    case Region.Region0 r: Region0s.Add(r); break;
                    case Region.Light r: Lights.Add(r); break;
                    case Region.StartPoint r: StartPoints.Add(r); break;
                    case Region.Sound r: Sounds.Add(r); break;
                    case Region.SFX r: SFXs.Add(r); break;
                    case Region.Wind r: Winds.Add(r); break;
                    case Region.EnvLight r: EnvLights.Add(r); break;
                    case Region.Fog r: Fogs.Add(r); break;

                    default:
                        throw new ArgumentException($"Unrecognized type {region.GetType()}.", nameof(region));
                }
                return region;
            }
            IMsbRegion IMsbParam<IMsbRegion>.Add(IMsbRegion item) => Add((Region)item);

            /// <summary>
            /// Returns every Region in the order they'll be written.
            /// </summary>
            public override List<Region> GetEntries()
            {
                return SFUtil.ConcatAll<Region>(
                    Region0s, Lights, StartPoints, Sounds, SFXs,
                    Winds, EnvLights, Fogs);
            }
            IReadOnlyList<IMsbRegion> IMsbParam<IMsbRegion>.GetEntries() => GetEntries();

            internal override Region ReadEntry(BinaryReaderEx br)
            {
                RegionType type = br.GetEnum8<RegionType>(br.Position + br.VarintSize + 2);
                switch (type)
                {
                    case RegionType.Region0:
                        return Region0s.EchoAdd(new Region.Region0(br));

                    case RegionType.Light:
                        return Lights.EchoAdd(new Region.Light(br));

                    case RegionType.StartPoint:
                        return StartPoints.EchoAdd(new Region.StartPoint(br));

                    case RegionType.Sound:
                        return Sounds.EchoAdd(new Region.Sound(br));

                    case RegionType.SFX:
                        return SFXs.EchoAdd(new Region.SFX(br));

                    case RegionType.Wind:
                        return Winds.EchoAdd(new Region.Wind(br));

                    case RegionType.EnvLight:
                        return EnvLights.EchoAdd(new Region.EnvLight(br));

                    case RegionType.Fog:
                        return Fogs.EchoAdd(new Region.Fog(br));

                    default:
                        throw new NotImplementedException($"Unimplemented region type: {type}");
                }
            }
        }

        /// <summary>
        /// A point or volume that triggers some behavior.
        /// </summary>
        public abstract class Region : NamedEntry, IMsbRegion
        {
            private protected abstract RegionType Type { get; }
            private protected abstract bool HasTypeData { get; }

            public short Number { get; set; }

            public MSB.Shape Shape
            {
                get => _shape;
                set
                {
                    if (value is MSB.Shape.Composite)
                        throw new ArgumentException("Dark Souls 2 does not support composite shapes.");
                    _shape = value;
                }
            }
            private MSB.Shape _shape;

            public short UniqueID { get; set; }
            public Vector3 Position { get; set; }
            public Vector3 Rotation { get; set; }

            public sbyte Layer1 { get; set; }
            public sbyte Layer2 { get; set; }
            public sbyte Layer3 { get; set; }
            public sbyte Layer4 { get; set; }

            public sbyte ExportEnable { get; set; }

            private protected Region(string name)
            {
                Name = name;
                Shape = new MSB.Shape.Point();
            }

            /// <summary>
            /// Creates a deep copy of the region.
            /// </summary>
            public Region DeepCopy()
            {
                var region = (Region)MemberwiseClone();
                region.Shape = Shape.DeepCopy();
                return region;
            }
            IMsbRegion IMsbRegion.DeepCopy() => DeepCopy();

            private protected Region(BinaryReaderEx br)
            {
                long start = br.Position;
                long nameOffset = br.ReadVarint();
                Number = br.ReadInt16();
                br.AssertByte((byte)Type);
                MSB.ShapeType shapeType = (MSB.ShapeType)br.ReadByte();
                br.ReadInt16(); // ID

                UniqueID = br.ReadInt16();
                Position = br.ReadVector3();
                Rotation = br.ReadVector3();

                long parentListOffset = br.ReadVarint();
                long childListOffset = br.ReadVarint();

                Layer1 = br.ReadSByte();
                Layer2 = br.ReadSByte();
                Layer3 = br.ReadSByte();
                Layer4 = br.ReadSByte();

                // 16
                br.AssertPattern(0x10, 0x00);

                // byte
                ExportEnable = br.ReadSByte();
                br.ReadSByte();
                br.ReadSByte();
                br.ReadSByte();

                // 16
                br.AssertPattern(0x10, 0x00);

                //br.AssertPattern(0x24, 0x00); // 36

                long shapeDataOffset = br.ReadVarint();
                long typeDataOffset = br.ReadVarint();
                br.AssertInt64(0);
                br.AssertInt64(0);
                if (!br.VarintLong)
                {
                    br.AssertInt64(0);
                    br.AssertInt64(0);
                    br.AssertInt32(0);
                }

                Shape = MSB.Shape.Create(shapeType);

                if (!BinaryReaderEx.IgnoreAsserts)
                {
                    if (nameOffset == 0)
                        throw new InvalidDataException($"{nameof(nameOffset)} must not be 0 in type {GetType()}.");

                    if (parentListOffset == 0)
                        throw new InvalidDataException($"{nameof(parentListOffset)} must not be 0 in type {GetType()}.");

                    if (childListOffset == 0)
                        throw new InvalidDataException($"{nameof(childListOffset)} must not be 0 in type {GetType()}.");

                    if (Shape.HasShapeData ^ shapeDataOffset != 0)
                        throw new InvalidDataException($"Unexpected {nameof(shapeDataOffset)} 0x{shapeDataOffset:X} in type {GetType()}.");

                    if (HasTypeData ^ typeDataOffset != 0)
                        throw new InvalidDataException($"Unexpected {nameof(typeDataOffset)} 0x{typeDataOffset:X} in type {GetType()}.");
                }

                br.Position = start + nameOffset;
                Name = br.ReadUTF16();

                br.Position = start + parentListOffset;
                br.AssertInt32(0);

                br.Position = start + childListOffset;
                br.AssertInt32(0);

                if (Shape.HasShapeData)
                {
                    br.Position = start + shapeDataOffset;
                    Shape.ReadShapeData(br);
                }

                if (HasTypeData)
                {
                    br.Position = start + typeDataOffset;
                    ReadTypeData(br);
                }
            }

            private protected virtual void ReadTypeData(BinaryReaderEx br)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(ReadTypeData)}.");

            internal override void Write(BinaryWriterEx bw, int id)
            {
                long start = bw.Position;
                bw.ReserveVarint("NameOffset");

                bw.WriteInt16(Number);
                bw.WriteByte((byte)Type);
                bw.WriteByte((byte)Shape.Type);
                bw.WriteInt16((short)id);
                bw.WriteInt16(UniqueID);

                bw.WriteVector3(Position);
                bw.WriteVector3(Rotation);

                bw.ReserveVarint("ParentListOffset");
                bw.ReserveVarint("ChildListOffset");

                bw.WriteSByte(Layer1);
                bw.WriteSByte(Layer2);
                bw.WriteSByte(Layer3);
                bw.WriteSByte(Layer4);

                bw.WritePattern(0x10, 0x00);

                bw.WriteSByte(ExportEnable);
                bw.WriteSByte(0);
                bw.WriteSByte(0);
                bw.WriteSByte(0);

                bw.WritePattern(0x10, 0x00);

                //bw.WritePattern(0x24, 0x00);

                bw.ReserveVarint("ShapeDataOffset");
                bw.ReserveVarint("TypeDataOffset");
                bw.WriteInt64(0);
                bw.WriteInt64(0);
                if (!bw.VarintLong)
                {
                    bw.WriteInt64(0);
                    bw.WriteInt64(0);
                    bw.WriteInt32(0);
                }

                bw.FillVarint("NameOffset", bw.Position - start);
                bw.WriteUTF16(Name, true);
                bw.Pad(4);

                bw.FillVarint("ParentListOffset", bw.Position - start);
                bw.WriteInt32(0);

                bw.FillVarint("ChildListOffset", bw.Position - start);
                bw.WriteInt32(0);
                bw.Pad(bw.VarintSize);

                if (Shape.HasShapeData)
                {
                    bw.FillVarint("ShapeDataOffset", bw.Position - start);
                    Shape.WriteShapeData(bw);
                }
                else
                {
                    bw.FillVarint("ShapeDataOffset", 0);
                }

                if (HasTypeData)
                {
                    bw.FillVarint("TypeDataOffset", bw.Position - start);
                    WriteTypeData(bw);
                }
                else
                {
                    bw.FillVarint("TypeDataOffset", 0);
                }
            }

            private protected virtual void WriteTypeData(BinaryWriterEx bw)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(WriteTypeData)}.");

            /// <summary>
            /// Returns a string representation of the region.
            /// </summary>
            public override string ToString()
            {
                return $"{Type} {Shape.Type} \"{Name}\"";
            }

            /// <summary>
            /// Unknown, names always seem to mention enemies; possibly walk points.
            /// </summary>
            public class Region0 : Region
            {
                private protected override RegionType Type => RegionType.Region0;
                private protected override bool HasTypeData => false;

                /// <summary>
                /// Creates a Region0 with default values.
                /// </summary>
                public Region0() : base($"{nameof(Region)}: {nameof(Region0)}") { }

                internal Region0(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// Unknown if this does anything.
            /// </summary>
            public class Light : Region
            {
                private protected override RegionType Type => RegionType.Light;
                private protected override bool HasTypeData => true;

                public LightType LightType { get; set; }

                public byte Sharpness { get; set; }
                public sbyte EnableShadow { get; set; }

                public Color DiffuseColor { get; set; }

                public Color SpecularColor { get; set; }

                public float ShadowIntensity { get; set; }

                public Light() : base($"{nameof(Region)}: {nameof(Light)}") { }

                internal Light(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    LightType = br.ReadEnum8<LightType>();
                    Sharpness = br.ReadByte();
                    EnableShadow = br.ReadSByte();

                    br.ReadSByte();

                    DiffuseColor = br.ReadRGBA();
                    SpecularColor = br.ReadRGBA();
                    ShadowIntensity = br.ReadSingle();

                    br.AssertPattern(0x10, 0x00);
                    if (br.VarintLong)
                        br.AssertInt32(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteSByte((sbyte)LightType);
                    bw.WriteByte(Sharpness);
                    bw.WriteSByte(EnableShadow);
                    bw.WriteSByte(0);

                    bw.WriteRGBA(DiffuseColor);
                    bw.WriteRGBA(SpecularColor);
                    bw.WriteSingle(ShadowIntensity);

                    bw.WritePattern(0x10, 0x00);
                    if (bw.VarintLong)
                        bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// Unknown, presumably the default spawn location for a map.
            /// </summary>
            public class StartPoint : Region
            {
                private protected override RegionType Type => RegionType.StartPoint;
                private protected override bool HasTypeData => false;

                /// <summary>
                /// Creates a StartPoint with default values.
                /// </summary>
                public StartPoint() : base($"{nameof(Region)}: {nameof(StartPoint)}") { }

                internal StartPoint(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// A sound effect that plays in a certain area.
            /// </summary>
            public class Sound : Region
            {
                private protected override RegionType Type => RegionType.Sound;
                private protected override bool HasTypeData => true;

                public SoundType SoundType { get; set; }

                public uint SoundID { get; set; }

                public uint EventSoundID { get; set; }

                /// <summary>
                /// Creates a Sound with default values.
                /// </summary>
                public Sound() : base($"{nameof(Region)}: {nameof(Sound)}") { }

                internal Sound(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    SoundType = br.ReadEnum32<SoundType>();
                    SoundID = br.ReadUInt32();
                    EventSoundID = br.ReadUInt32();

                    br.AssertPattern(0x14, 0x00);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32((int)SoundType);
                    bw.WriteUInt32(SoundID);
                    bw.WriteUInt32(EventSoundID);

                    bw.WritePattern(0x14, 0x00);
                }
            }

            /// <summary>
            /// A special effect that plays at a certain region.
            /// </summary>
            public class SFX : Region
            {
                private protected override RegionType Type => RegionType.SFX;
                private protected override bool HasTypeData => true;

                public uint SfxID { get; set; }

                public uint EventSfxID { get; set; }

                /// <summary>
                /// Creates an SFX with default values.
                /// </summary>
                public SFX() : base($"{nameof(Region)}: {nameof(SFX)}") { }

                internal SFX(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    SfxID = br.ReadUInt32();
                    EventSfxID = br.ReadUInt32();

                    br.AssertPattern(0x18, 0x00);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteUInt32(SfxID);
                    bw.WriteUInt32(EventSfxID);
                    bw.WritePattern(0x18, 0x00);
                }
            }

            /// <summary>
            /// Unknown, presumably sets wind speed/direction.
            /// </summary>
            public class Wind : Region
            {
                private protected override RegionType Type => RegionType.Wind;
                private protected override bool HasTypeData => true;

                public WindType WindType { get; set; }
                public WindTargetType WindTargetType { get; set; }

                public float Priority { get; set; }

                public float MinimumSpeed { get; set; }
                public float MaximumSpeed { get; set; }
                public float Frequency { get; set; }
                public float GainStartDistance { get; set; }

                private int Unk00 { get; set; }

                public Wind() : base($"{nameof(Region)}: {nameof(Wind)}") { }

                internal Wind(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    WindType = br.ReadEnum8<WindType>();
                    WindTargetType = br.ReadEnum8<WindTargetType>();

                    br.ReadSByte();
                    br.ReadSByte();

                    Priority = br.ReadSingle();
                    MinimumSpeed = br.ReadSingle();
                    MaximumSpeed = br.ReadSingle();
                    Frequency = br.ReadSingle();
                    GainStartDistance = br.ReadSingle();

                    Unk00 = br.ReadInt32();
                    br.AssertInt32(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteSByte((sbyte)WindType);
                    bw.WriteSByte((sbyte)WindTargetType);

                    bw.WriteSByte(0);
                    bw.WriteSByte(0);

                    bw.WriteSingle(Priority);
                    bw.WriteSingle(MinimumSpeed);
                    bw.WriteSingle(MaximumSpeed);
                    bw.WriteSingle(Frequency);
                    bw.WriteSingle(GainStartDistance);

                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// Unknown, names mention lightmaps and GI.
            /// </summary>
            public class EnvLight : Region
            {
                private protected override RegionType Type => RegionType.EnvLight;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT00 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT04 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT08 { get; set; }

                /// <summary>
                /// Creates an EnvLight with default values.
                /// </summary>
                public EnvLight() : base($"{nameof(Region)}: {nameof(EnvLight)}") { }

                internal EnvLight(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    UnkT00 = br.ReadInt32();
                    UnkT04 = br.ReadSingle();
                    UnkT08 = br.ReadSingle();
                    br.AssertPattern(0x14, 0x00);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(UnkT00);
                    bw.WriteSingle(UnkT04);
                    bw.WriteSingle(UnkT08);
                    bw.WritePattern(0x14, 0x00);
                }
            }

            /// <summary>
            /// Unknown if this does anything.
            /// </summary>
            public class Fog : Region
            {
                private protected override RegionType Type => RegionType.Fog;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Unknown.
                /// </summary>
                public int MapVolumeFogParamID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT04 { get; set; }

                /// <summary>
                /// Creates a Fog with default values.
                /// </summary>
                public Fog() : base($"{nameof(Region)}: {nameof(Fog)}") { }

                internal Fog(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    MapVolumeFogParamID = br.ReadInt32();
                    UnkT04 = br.ReadInt32();
                    br.AssertPattern(0x18, 0x00);
                    if (br.VarintLong)
                        br.AssertInt32(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(MapVolumeFogParamID);
                    bw.WriteInt32(UnkT04);
                    bw.WritePattern(0x18, 0x00);
                    if (bw.VarintLong)
                        bw.WriteInt32(0);
                }
            }
        }
    }
}
