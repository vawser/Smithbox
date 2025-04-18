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

            /// <summary>
            /// Unknown.
            /// </summary>
            public short Unk08 { get; set; }

            /// <summary>
            /// Describes the space encompassed by the region.
            /// </summary>
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

            /// <summary>
            /// Unknown.
            /// </summary>
            public short Unk0E { get; set; }

            /// <summary>
            /// Location of the region.
            /// </summary>
            public Vector3 Position { get; set; }

            /// <summary>
            /// Rotation of the region, in degrees.
            /// </summary>
            public Vector3 Rotation { get; set; }

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
                Unk08 = br.ReadInt16();
                br.AssertByte((byte)Type);
                MSB.ShapeType shapeType = (MSB.ShapeType)br.ReadByte();
                br.ReadInt16(); // ID
                Unk0E = br.ReadInt16();
                Position = br.ReadVector3();
                Rotation = br.ReadVector3();
                long unkOffsetA = br.ReadVarint();
                long unkOffsetB = br.ReadVarint();
                br.AssertInt32(-1);
                br.AssertPattern(0x24, 0x00);
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

                if (nameOffset == 0)
                    throw new InvalidDataException($"{nameof(nameOffset)} must not be 0 in type {GetType()}.");
                if (unkOffsetA == 0)
                    throw new InvalidDataException($"{nameof(unkOffsetA)} must not be 0 in type {GetType()}.");
                if (unkOffsetB == 0)
                    throw new InvalidDataException($"{nameof(unkOffsetB)} must not be 0 in type {GetType()}.");
                if (Shape.HasShapeData ^ shapeDataOffset != 0)
                    throw new InvalidDataException($"Unexpected {nameof(shapeDataOffset)} 0x{shapeDataOffset:X} in type {GetType()}.");
                if (HasTypeData ^ typeDataOffset != 0)
                    throw new InvalidDataException($"Unexpected {nameof(typeDataOffset)} 0x{typeDataOffset:X} in type {GetType()}.");

                br.Position = start + nameOffset;
                Name = br.ReadUTF16();

                br.Position = start + unkOffsetA;
                br.AssertInt32(0);

                br.Position = start + unkOffsetB;
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
                bw.WriteInt16(Unk08);
                bw.WriteByte((byte)Type);
                bw.WriteByte((byte)Shape.Type);
                bw.WriteInt16((short)id);
                bw.WriteInt16(Unk0E);
                bw.WriteVector3(Position);
                bw.WriteVector3(Rotation);
                bw.ReserveVarint("UnkOffsetA");
                bw.ReserveVarint("UnkOffsetB");
                bw.WriteInt32(-1);
                bw.WritePattern(0x24, 0x00);
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

                bw.FillVarint("UnkOffsetA", bw.Position - start);
                bw.WriteInt32(0);

                bw.FillVarint("UnkOffsetB", bw.Position - start);
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

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT00 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public Color ColorT04 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public Color ColorT08 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT0C { get; set; }

                /// <summary>
                /// Creates a Light with default values.
                /// </summary>
                public Light() : base($"{nameof(Region)}: {nameof(Light)}") { }

                internal Light(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    UnkT00 = br.ReadInt32();
                    ColorT04 = br.ReadRGBA();
                    ColorT08 = br.ReadRGBA();
                    UnkT0C = br.ReadSingle();
                    br.AssertPattern(0x10, 0x00);
                    if (br.VarintLong)
                        br.AssertInt32(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(UnkT00);
                    bw.WriteRGBA(ColorT04);
                    bw.WriteRGBA(ColorT08);
                    bw.WriteSingle(UnkT0C);
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

                /// <summary>
                /// Unknown; possibly sound type.
                /// </summary>
                public int UnkT00 { get; set; }

                /// <summary>
                /// ID of the sound to play.
                /// </summary>
                public int SoundID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT08 { get; set; }

                /// <summary>
                /// Creates a Sound with default values.
                /// </summary>
                public Sound() : base($"{nameof(Region)}: {nameof(Sound)}") { }

                internal Sound(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    UnkT00 = br.ReadInt32();
                    SoundID = br.ReadInt32();
                    UnkT08 = br.ReadInt32();
                    br.AssertPattern(0x14, 0x00);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(UnkT00);
                    bw.WriteInt32(SoundID);
                    bw.WriteInt32(UnkT08);
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

                /// <summary>
                /// The effect to play at this region.
                /// </summary>
                public int EffectID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT04 { get; set; }

                /// <summary>
                /// Creates an SFX with default values.
                /// </summary>
                public SFX() : base($"{nameof(Region)}: {nameof(SFX)}") { }

                internal SFX(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    EffectID = br.ReadInt32();
                    UnkT04 = br.ReadInt32();
                    br.AssertPattern(0x18, 0x00);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(EffectID);
                    bw.WriteInt32(UnkT04);
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
                /// Unknown.
                /// </summary>
                public float UnkT0C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT10 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT14 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT18 { get; set; }

                /// <summary>
                /// Creates a Wind with default values.
                /// </summary>
                public Wind() : base($"{nameof(Region)}: {nameof(Wind)}") { }

                internal Wind(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    UnkT00 = br.ReadInt32();
                    UnkT04 = br.ReadSingle();
                    UnkT08 = br.ReadSingle();
                    UnkT0C = br.ReadSingle();
                    UnkT10 = br.ReadSingle();
                    UnkT14 = br.ReadSingle();
                    UnkT18 = br.ReadSingle();
                    br.AssertInt32(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(UnkT00);
                    bw.WriteSingle(UnkT04);
                    bw.WriteSingle(UnkT08);
                    bw.WriteSingle(UnkT0C);
                    bw.WriteSingle(UnkT10);
                    bw.WriteSingle(UnkT14);
                    bw.WriteSingle(UnkT18);
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
