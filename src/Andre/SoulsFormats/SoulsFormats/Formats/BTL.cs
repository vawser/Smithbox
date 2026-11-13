using System.Collections.Generic;
using System.Drawing;
using System.Numerics;

namespace SoulsFormats
{
    /// <summary>
    /// Point light sources in a map, used in BB, DS3, Sekiro and ER.
    /// </summary>
    public class BTL : SoulsFile<BTL>
    {
        /// <summary>
        /// Indicates the version, probably.
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// Indicates size of BTL Light in bytes.
        /// </summary>
        public int LightSize { get; set; }

        /// <summary>
        /// Whether offsets are 64-bit; set to false for Dark Souls 2.
        /// </summary>
        public bool LongOffsets { get; set; }

        /// <summary>
        /// Light sources in this BTL.
        /// </summary>
        public List<Light> Lights { get; set; }

        /// <summary>
        /// Creates a BTL with Sekiro's version and no lights.
        /// </summary>
        public BTL()
        {
            Version = 16;
            LightSize = 0xE8;
            LongOffsets = true;
            Lights = new List<Light>();
        }

        /// <summary>
        /// Deserializes file data from a stream.
        /// </summary>
        protected override void Read(BinaryReaderEx br)
        {
            br.BigEndian = false;

            br.AssertInt32(2);
            Version = br.AssertInt32([1, 2, 5, 6, 15, 16, 18, 19]);
            int lightCount = br.ReadInt32();
            int namesLength = br.ReadInt32();
            br.AssertInt32(0);
            LightSize = br.AssertInt32([0xC0, 0xC8, 0xE8, 0xF0]);
            br.AssertPattern(0x24, 0x00);
            LongOffsets = br.VarintLong = LightSize != 0xC0;

            long namesStart = br.Position;
            br.Skip(namesLength);
            Lights = new List<Light>(lightCount);
            for (int i = 0; i < lightCount; i++)
                Lights.Add(new Light(br, namesStart, LightSize));
        }

        /// <summary>
        /// Serializes file data to a stream.
        /// </summary>
        protected override void Write(BinaryWriterEx bw)
        {
            bw.BigEndian = false;
            bw.VarintLong = LongOffsets;

            bw.WriteInt32(2);
            bw.WriteInt32(Version);
            bw.WriteInt32(Lights.Count);
            bw.ReserveInt32("NamesLength");
            bw.WriteInt32(0);
            bw.WriteInt32(LightSize);
            bw.WritePattern(0x24, 0x00);

            long namesStart = bw.Position;
            var nameOffsets = new List<long>(Lights.Count);
            foreach (Light entry in Lights)
            {
                long nameOffset = bw.Position - namesStart;
                nameOffsets.Add(nameOffset);
                bw.WriteUTF16(entry.Name, true);
                if (nameOffset % 0x10 != 0)
                    bw.WritePattern((int)(0x10 - (nameOffset % 0x10)), 0x00);
            }

            bw.FillInt32("NamesLength", (int)(bw.Position - namesStart));
            for (int i = 0; i < Lights.Count; i++)
                Lights[i].Write(bw, nameOffsets[i], LightSize);
        }

        /// <summary>
        /// Type of a light source.
        /// </summary>
        public enum LightType : uint
        {
            /// <summary>
            /// Omnidirectional light.
            /// </summary>
            Point = 0,

            /// <summary>
            /// Cone of light.
            /// </summary>
            Spot = 1,

            /// <summary>
            /// Light at a constant angle.
            /// </summary>
            Directional = 2,
        }

        /// <summary>
        /// An omnidirectional and/or spot light source.
        /// </summary>
        public class Light
        {
            /// <summary>
            /// Name of this light.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public LightType Type { get; set; }

            /// <summary>
            /// Center of the light.
            /// </summary>
            public Vector3 Position { get; set; }

            /// <summary>
            /// Rotation of a spot light, in radians.
            /// </summary>
            [RotationRadians]
            public Vector3 Rotation { get; set; }

            /// <summary>
            /// Distance the light shines. Far plane of a spot light or radius of a point light.
            /// </summary>
            public float Radius { get; set; }

            /// <summary>
            /// Color of the light on diffuse surfaces.
            /// </summary>
            [SupportsAlpha(false)]
            public Color DiffuseColor { get; set; }

            /// <summary>
            /// Intensity of diffuse lighting.
            /// </summary>
            public float DiffusePower { get; set; }

            /// <summary>
            /// Color of the light on reflective surfaces.
            /// </summary>
            [SupportsAlpha(false)]
            public Color SpecularColor { get; set; }

            /// <summary>
            /// Intensity of specular lighting.
            /// </summary>
            public float SpecularPower { get; set; }

            /// <summary>
            /// Whether the light casts shadows.
            /// </summary>
            public bool CastShadows { get; set; }

            /// <summary>
            /// Color of shadows cast by the light; alpha is relative to 100.
            /// </summary>
            [SupportsAlpha(true)]
            public Color ShadowColor { get; set; }

            /// <summary>
            /// Minimum time between flickers.
            /// </summary>
            public float FlickerIntervalMin { get; set; }

            /// <summary>
            /// Maximum time between flickers.
            /// </summary>
            public float FlickerIntervalMax { get; set; }

            /// <summary>
            /// Multiplies the brightness of the light while flickering.
            /// </summary>
            public float FlickerBrightnessMult { get; set; }

            /// <summary>
            /// Vertical FoV of a spot light. Identical to <see cref="HorizontalFov"/> when set to 0.
            /// </summary>
            public float VerticalFov { get; set; }

            /// <summary>
            /// Horizontal FoV of a spot light. Identical to <see cref="VerticalFov"/> when set to 0.
            /// </summary>
            public float HorizontalFov { get; set; }

            /// <summary>
            /// Near plane of a spot light. Defaults to <see cref="Radius"/>/100 when set to 0.
            /// </summary>
            public float NearDistance { get; set; }

            /// <summary>
            /// Referenced by map events. Only used in DS2.
            /// </summary>
            public int EventID { get; set; }

            /// <summary>
            /// Strength of the light passing through translucent and thin surfaces.
            /// </summary>
            public float TranslucencyIntensity { get; set; }

            /// <summary>
            /// Exponent of the light passing through translucent surfaces, at least 0.01.
            /// </summary>
            public float TranslucencyPower { get; set; }

            /// <summary>
            /// Depth bias of the light's shadow map in the range of [-7, 7). Offsets the depth of shadows cast
            /// by this light.
            /// </summary>
            public sbyte ShadowDepthBias { get; set; }

            /// <summary>
            /// Unknown. Likely inverts the face culling test for the shadow pass.
            /// </summary>
            public bool ShadowModelCullFlip { get; set; }

            /// <summary>
            /// Radius of the light source.
            /// </summary>
            public float SourceRadius { get; set; }

            /// <summary>
            /// How dark the light's shadow is in range of [0, 1). Higher values draw darker shadows.
            /// </summary>
            public float ShadowIntensity { get; set; }

            /// <summary>
            /// Controls whether this light contributes to baked lighting.
            /// </summary>
            public bool BakesIrradianceVolumeDirect { get; set; }

            /// <summary>
            /// Controls whether this light contributes to baked indirect lighting.
            /// </summary>
            public bool BakesIrradianceVolumeIndirect { get; set; }

            /// <summary>
            /// Unknown. Named "Shadow Cache" in debug menu.
            /// </summary>
            public bool CachesShadow { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public bool BakesShadow { get; set; }

            /// <summary>
            /// Distance from the camera this light is shown within. 0 = always shown.
            /// </summary>
            public float ViewDistance { get; set; }

            /// <summary>
            /// Unknown. Likely scales the light baked into the irradiance volume's direct light. Not present before Sekiro.
            /// </summary>
            public float IrradianceVolumeDirectScale { get; set; }

            /// <summary>
            /// Unknown. Likely scales the light baked into the irradiance volume's indirect light. Not present before Sekiro.
            /// </summary>
            public float IrradianceVolumeIndirectScale { get; set; }

            /// <summary>
            /// Scales the diffuse color in volumetric fog. Not present before Sekiro.
            /// </summary>
            public float ScatteringScale { get; set; }

            /// <summary>
            /// Unknown. Not present before Sekiro.
            /// </summary>
            public float ScatteringFlickerInfluence { get; set; }

            /// <summary>
            /// Scattering anisotropy in volumetric fog, -1 to 1. Not present before Sekiro.
            /// <see href="https://bartwronski.com/wp-content/uploads/2014/08/bwronski_volumetric_fog_siggraph2014.pdf"/>
            /// </summary>
            public float ScatteringG { get; set; }

            /// <summary>
            /// Whether <see cref="ScatteringG"/> replaces the fog's own anisotropy for this light. Not present before Sekiro.
            /// </summary>
            public bool OverridesScatteringG { get; set; }

            /// <summary>
            /// Unknown; not present before Sekiro.
            /// </summary>
            public float AntiExposureRate { get; set; }

            /// <summary>
            /// The lowest shadow quality level this light casts shadows at. Not present before Sekiro.
            /// </summary>
            public int ShadowSpecLevel { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public bool Unk1C { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public float Unk30 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public float Unk34 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int Unk50 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public float Unk54 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int Unk5C { get; set; }

            /// <summary>
            /// Unknown; 4 bytes.
            /// </summary>
            public byte[] Unk64 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public float Unk68 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public float Unk70 { get; set; }

            /// <summary>
            /// Unknown; 4 bytes.
            /// </summary>
            public byte[] Unk84 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public float Unk88 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public float Unk90 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte UnkA0 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte UnkA1 { get; set; }

            /// <summary>
            /// Unknown; only present in version 15 BTLs in ER (of which there are only 2).
            /// </summary>
            public int UnkE8 { get; set; }

            /// <summary>
            /// Unknown; only present in version 15 BTLs in ER (of which there are only 2).
            /// </summary>
            public int UnkEB { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte[] Unk00 { get; private set; }

            /// <summary>
            /// Creates a Light with default values.
            /// </summary>
            public Light()
            {
                Unk00 = new byte[16];
                Name = "";
                Unk1C = true;
                DiffuseColor = Color.White;
                DiffusePower = 1;
                SpecularColor = Color.White;
                SpecularPower = 1;
                Unk50 = 4;
                Radius = 10;
                Unk5C = -1;
                Unk64 = new byte[4] { 0, 0, 0, 1 };
                ShadowColor = Color.FromArgb(100, 0, 0, 0);
                FlickerBrightnessMult = 1;
                EventID = -1;
                Unk84 = new byte[4];
                TranslucencyIntensity = 1;
                TranslucencyPower = 1;
                UnkA0 = 1;
                UnkA1 = 0;
                ShadowDepthBias = 2;
                ShadowModelCullFlip = true;
                SourceRadius = 1;
            }

            /// <summary>
            /// Creates a clone of an existing Light.
            /// </summary>
            public Light Clone()
            {
                var clone = (Light)MemberwiseClone();
                clone.Unk00 = (byte[])Unk00.Clone();
                clone.Unk64 = (byte[])Unk64.Clone();
                clone.Unk84 = (byte[])Unk84.Clone();
                return clone;
            }

            internal Light(BinaryReaderEx br, long namesStart, int lightSize)
            {
                Unk00 = br.ReadBytes(16);
                Name = br.GetUTF16(namesStart + br.ReadVarint());
                Type = br.ReadEnum32<LightType>();
                Unk1C = br.ReadBoolean();
                DiffuseColor = ReadRGB(br);
                DiffusePower = br.ReadSingle();
                SpecularColor = ReadRGB(br);
                CastShadows = br.ReadBoolean();
                SpecularPower = br.ReadSingle();
                VerticalFov = br.ReadSingle();
                Unk30 = br.ReadSingle();
                Unk34 = br.ReadSingle();
                Position = br.ReadVector3();
                Rotation = br.ReadVector3();
                Unk50 = br.ReadInt32();
                Unk54 = br.ReadSingle();
                Radius = br.ReadSingle();
                Unk5C = br.ReadInt32();
                br.AssertInt32(0);
                Unk64 = br.ReadBytes(4);
                Unk68 = br.ReadSingle();
                ShadowColor = br.ReadRGBA();
                Unk70 = br.ReadSingle();
                FlickerIntervalMin = br.ReadSingle();
                FlickerIntervalMax = br.ReadSingle();
                FlickerBrightnessMult = br.ReadSingle();
                EventID = br.ReadInt32();
                Unk84 = br.ReadBytes(4);
                Unk88 = br.ReadSingle();
                br.AssertInt32(0);
                Unk90 = br.ReadSingle();
                br.AssertInt32(0);
                TranslucencyIntensity = br.ReadSingle();
                TranslucencyPower = br.ReadSingle();
                UnkA0 = br.ReadByte();
                UnkA1 = br.ReadByte();
                ShadowDepthBias = br.ReadSByte();
                ShadowModelCullFlip = br.ReadBoolean();
                SourceRadius = br.ReadSingle();
                br.AssertInt32(0);
                ShadowIntensity = br.ReadSingle();
                br.AssertVarint(0);
                HorizontalFov = br.ReadSingle();
                NearDistance = br.ReadSingle();
                BakesIrradianceVolumeDirect = br.ReadBoolean();
                BakesIrradianceVolumeIndirect = br.ReadBoolean();
                CachesShadow = br.ReadBoolean();
                BakesShadow = br.ReadBoolean();
                ViewDistance = br.ReadSingle();

                // Variable light sizes start here

                if (lightSize > 0xC8)
                {
                    IrradianceVolumeDirectScale = br.ReadSingle();
                    IrradianceVolumeIndirectScale = br.ReadSingle();
                    ScatteringScale = br.ReadSingle();
                    ScatteringFlickerInfluence = br.ReadSingle();
                    ScatteringG = br.ReadSingle();
                    // Asserted so a value other than 0/1 fails loudly instead of being lost on save
                    OverridesScatteringG = br.AssertInt32([0, 1]) == 1;
                    AntiExposureRate = br.ReadSingle();
                    ShadowSpecLevel = br.ReadInt32();
                }

                if (lightSize > 0xE8)
                {
                    UnkE8 = br.ReadInt32();
                    UnkEB = br.ReadInt32();
                }
            }

            internal void Write(BinaryWriterEx bw, long nameOffset, int lightSize)
            {
                bw.WriteBytes(Unk00);
                bw.WriteVarint(nameOffset);
                bw.WriteUInt32((uint)Type);
                bw.WriteBoolean(Unk1C);
                WriteRGB(bw, DiffuseColor);
                bw.WriteSingle(DiffusePower);
                WriteRGB(bw, SpecularColor);
                bw.WriteBoolean(CastShadows);
                bw.WriteSingle(SpecularPower);
                bw.WriteSingle(VerticalFov);
                bw.WriteSingle(Unk30);
                bw.WriteSingle(Unk34);
                bw.WriteVector3(Position);
                bw.WriteVector3(Rotation);
                bw.WriteInt32(Unk50);
                bw.WriteSingle(Unk54);
                bw.WriteSingle(Radius);
                bw.WriteInt32(Unk5C);
                bw.WriteInt32(0);
                bw.WriteBytes(Unk64);
                bw.WriteSingle(Unk68);
                bw.WriteRGBA(ShadowColor);
                bw.WriteSingle(Unk70);
                bw.WriteSingle(FlickerIntervalMin);
                bw.WriteSingle(FlickerIntervalMax);
                bw.WriteSingle(FlickerBrightnessMult);
                bw.WriteInt32(EventID);
                bw.WriteBytes(Unk84);
                bw.WriteSingle(Unk88);
                bw.WriteInt32(0);
                bw.WriteSingle(Unk90);
                bw.WriteInt32(0);
                bw.WriteSingle(TranslucencyIntensity);
                bw.WriteSingle(TranslucencyPower);
                bw.WriteByte(UnkA0);
                bw.WriteByte(UnkA1);
                bw.WriteSByte(ShadowDepthBias);
                bw.WriteBoolean(ShadowModelCullFlip);
                bw.WriteSingle(SourceRadius);
                bw.WriteInt32(0);
                bw.WriteSingle(ShadowIntensity);
                bw.WriteVarint(0);
                bw.WriteSingle(HorizontalFov);
                bw.WriteSingle(NearDistance);
                bw.WriteBoolean(BakesIrradianceVolumeDirect);
                bw.WriteBoolean(BakesIrradianceVolumeIndirect);
                bw.WriteBoolean(CachesShadow);
                bw.WriteBoolean(BakesShadow);
                bw.WriteSingle(ViewDistance);

                // Variable light sizes start here

                if (lightSize > 0xC8)
                {
                    bw.WriteSingle(IrradianceVolumeDirectScale);
                    bw.WriteSingle(IrradianceVolumeIndirectScale);
                    bw.WriteSingle(ScatteringScale);
                    bw.WriteSingle(ScatteringFlickerInfluence);
                    bw.WriteSingle(ScatteringG);
                    bw.WriteInt32(OverridesScatteringG ? 1 : 0);
                    bw.WriteSingle(AntiExposureRate);
                    bw.WriteInt32(ShadowSpecLevel);
                }

                if (lightSize > 0xE8)
                {
                    bw.WriteInt32(UnkE8);
                    bw.WriteInt32(UnkEB);
                }
            }

            /// <summary>
            /// Returns the name of the light.
            /// </summary>
            public override string ToString()
            {
                return Name;
            }

            private static Color ReadRGB(BinaryReaderEx br)
            {
                byte[] rgb = br.ReadBytes(3);
                return Color.FromArgb(255, rgb[0], rgb[1], rgb[2]);
            }

            private static void WriteRGB(BinaryWriterEx bw, Color color)
            {
                bw.WriteByte(color.R);
                bw.WriteByte(color.G);
                bw.WriteByte(color.B);
            }
        }
    }
}
