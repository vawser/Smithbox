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
        /// Format version. ER picks the light record size from it (15: 0xF0, 13 and up: 0xE8, 8 to 12: 0xD8,
        /// 1 to 7: 0xC8, 0: 0x88) and gates some fields on it (<see cref="Light.AntiExposureRate"/> from 16,
        /// <see cref="Light.ShadowSpecLevel"/> at 18, <see cref="Light.UnkE5"/> at 19). Sekiro, NR and AC6 use the same
        /// table. DS3 (1 to 6) reads 0xC8 records, and 0x88 below version 1.
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
            long WriteName(string name)
            {
                long offset = bw.Position - namesStart;
                bw.WriteUTF16(name, true);
                if (offset % 0x10 != 0)
                    bw.WritePattern((int)(0x10 - (offset % 0x10)), 0x00);
                return offset;
            }

            var nameOffsets = new List<long>(Lights.Count);
            foreach (Light entry in Lights)
                nameOffsets.Add(WriteName(entry.Name));

            // Projection texture names follow the light names; 0 means none (always the case in retail files)
            var textureOffsets = new List<long>(Lights.Count);
            foreach (Light entry in Lights)
                textureOffsets.Add(string.IsNullOrEmpty(entry.ProjectionTextureName) ? 0 : WriteName(entry.ProjectionTextureName));

            bw.FillInt32("NamesLength", (int)(bw.Position - namesStart));
            for (int i = 0; i < Lights.Count; i++)
                Lights[i].Write(bw, nameOffsets[i], textureOffsets[i], LightSize);
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
            /// Only read in version 18 (ER, AC6); always 1 or 5 there, 0 in Sekiro's version 16.
            /// </summary>
            public byte ShadowSpecLevel { get; set; }

            /// <summary>
            /// Unknown. Only read in version 19 (NR), as a signed byte copied into the runtime light next to
            /// <see cref="ShadowSpecLevel"/>. Always 0 in retail files.
            /// </summary>
            public sbyte UnkE5 { get; set; }

            /// <summary>
            /// Whether <see cref="ProjectionTextureName"/> is used. Spot lights only; the game ignores the name when
            /// this is false. Never set in retail files.
            /// </summary>
            public bool UsesProjectionTexture { get; set; }

            /// <summary>
            /// Name of a texture projected by a spot light. The game looks it up by name and uses a default one while
            /// it isn't loaded. In DS3 a spot light with one is drawn as a separate light kind (0x69, or 0x70 with
            /// <see cref="LightingModel"/> 1). Stored in the names block. Never set in retail files.
            /// </summary>
            public string ProjectionTextureName { get; set; }

            /// <summary>
            /// Unknown. Not read by DS3, Sekiro, ER, NR or AC6; always true in retail files.
            /// </summary>
            public bool Unk1C { get; set; }

            /// <summary>
            /// Unknown. Not read by DS3, Sekiro, ER, NR or AC6. Spot lights only in DS3 (2 to 70); always 0 in Sekiro, ER, NR and AC6.
            /// </summary>
            public float Unk30 { get; set; }

            /// <summary>
            /// Unknown. Not read by DS3, Sekiro, ER, NR or AC6. 2 on spot lights and 0 on other lights in every game from DS3 on.
            /// </summary>
            public float Unk34 { get; set; }

            /// <summary>
            /// Unknown. Not read by DS3, Sekiro, ER, NR or AC6; always 4 from Sekiro on (DS3: 4, 2, or 0 on directional lights).
            /// </summary>
            public int Unk50 { get; set; }

            /// <summary>
            /// Unknown. Not read by DS3, Sekiro, ER, NR or AC6. Usually 0; when set it is nearly always less than <see cref="Radius"/>.
            /// </summary>
            public float Unk54 { get; set; }

            /// <summary>
            /// Unknown. Not read by DS3, Sekiro, ER, NR or AC6; always -1 (DS3: 0 on directional lights).
            /// </summary>
            public int Unk5C { get; set; }

            /// <summary>
            /// Unknown; 4 bytes. Not read by DS3, Sekiro, ER, NR or AC6; always 00 00 00 01 (DS3: FF FF FF 01 on directional lights).
            /// </summary>
            public byte[] Unk64 { get; set; }

            /// <summary>
            /// Unknown. Not read by DS3, Sekiro, ER, NR or AC6; always 0 from Sekiro on.
            /// </summary>
            public float Unk68 { get; set; }

            /// <summary>
            /// Unknown. Not read by DS3, Sekiro, ER, NR or AC6; always 0 from Sekiro on (DS3: 1 on directional lights).
            /// </summary>
            public float Unk70 { get; set; }

            /// <summary>
            /// Unknown; 4 bytes. Not read by DS3, Sekiro, ER, NR or AC6; always 0.
            /// </summary>
            public byte[] Unk84 { get; set; }

            /// <summary>
            /// Unknown. Not read by DS3, Sekiro, ER, NR or AC6; always 0.
            /// </summary>
            public float Unk88 { get; set; }

            /// <summary>
            /// Unknown. Not read by DS3, Sekiro, ER, NR or AC6; always 0.
            /// </summary>
            public float Unk90 { get; set; }

            /// <summary>
            /// Which of two banks of light kinds the light is drawn with: 0 = standard, 1 = second bank (probably the
            /// legacy lighting path). DS3 and Sekiro read it; at 1 the light's draw kind moves 7 up (point 0x65 → 0x6C,
            /// spot 0x67 → 0x6E), and each bank has its own enable toggle in the lighting system. ER, NR and AC6 don't
            /// read it and have no "Legacy" light shaders. Usually 1 in DS3, always 0 from Sekiro on.
            /// </summary>
            public byte LightingModel { get; set; }

            /// <summary>
            /// Unknown. When nonzero, sets a bit of the runtime light's mask: 0x10 in DS3 (version 5 and up), 0x80 in
            /// Sekiro, ER, NR and AC6 (the mask is 0x1 otherwise). Rare in ER; common in Sekiro, DS3 and AC6.
            /// </summary>
            public byte UnkA1 { get; set; }

            /// <summary>
            /// Unknown. Only present in version 15 BTLs (2 files in ER); not read by ER.
            /// </summary>
            public int UnkE8 { get; set; }

            /// <summary>
            /// Unknown. Only present in version 15 BTLs (2 files in ER); not read by ER.
            /// </summary>
            public int UnkEC { get; set; }

            // The 16-byte unique ID at the start of the record, which ER copies into the runtime light. All four parts
            // are stored big-endian, so their hex reads in the same order as the file bytes.

            /// <summary>
            /// Unique ID, bytes 0-3: when the light was created, as a Unix time in seconds.
            /// </summary>
            public uint UniqueIdTime { get; set; }

            /// <summary>
            /// Unique ID, bytes 4-7. Meaning unknown; looks random.
            /// </summary>
            public uint UniqueIdPart1 { get; set; }

            /// <summary>
            /// Unique ID, bytes 8-11. Meaning unknown; looks random.
            /// </summary>
            public uint UniqueIdPart2 { get; set; }

            /// <summary>
            /// Unique ID, bytes 12-15. Meaning unknown; looks random.
            /// </summary>
            public uint UniqueIdPart3 { get; set; }

            /// <summary>
            /// Creates a Light with default values.
            /// </summary>
            public Light()
            {
                RegenerateUniqueId();
                Name = "";
                ProjectionTextureName = "";
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
                LightingModel = 0;
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
                clone.Unk64 = (byte[])Unk64.Clone();
                clone.Unk84 = (byte[])Unk84.Clone();
                return clone;
            }

            /// <summary>
            /// Gives this light a new unique ID, as a light created now would get: the current time and three random parts.
            /// </summary>
            public void RegenerateUniqueId()
            {
                UniqueIdTime = (uint)System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                UniqueIdPart1 = (uint)System.Random.Shared.NextInt64(0, 1L << 32);
                UniqueIdPart2 = (uint)System.Random.Shared.NextInt64(0, 1L << 32);
                UniqueIdPart3 = (uint)System.Random.Shared.NextInt64(0, 1L << 32);
            }

            internal Light(BinaryReaderEx br, long namesStart, int lightSize)
            {
                br.BigEndian = true;
                UniqueIdTime = br.ReadUInt32();
                UniqueIdPart1 = br.ReadUInt32();
                UniqueIdPart2 = br.ReadUInt32();
                UniqueIdPart3 = br.ReadUInt32();
                br.BigEndian = false;
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
                LightingModel = br.ReadByte();
                UnkA1 = br.ReadByte();
                ShadowDepthBias = br.ReadSByte();
                ShadowModelCullFlip = br.ReadBoolean();
                SourceRadius = br.ReadSingle();
                UsesProjectionTexture = br.ReadBoolean();
                br.AssertByte(0);
                br.AssertInt16(0);
                ShadowIntensity = br.ReadSingle();
                // Offset into the names block, like the name; kept even when the flag is off so it round-trips
                long textureOffset = br.ReadVarint();
                ProjectionTextureName = textureOffset != 0 ? br.GetUTF16(namesStart + textureOffset) : "";
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
                    ShadowSpecLevel = br.ReadByte();
                    UnkE5 = br.ReadSByte();
                    br.AssertInt16(0);
                }

                if (lightSize > 0xE8)
                {
                    UnkE8 = br.ReadInt32();
                    UnkEC = br.ReadInt32();
                }
            }

            internal void Write(BinaryWriterEx bw, long nameOffset, long textureOffset, int lightSize)
            {
                bw.BigEndian = true;
                bw.WriteUInt32(UniqueIdTime);
                bw.WriteUInt32(UniqueIdPart1);
                bw.WriteUInt32(UniqueIdPart2);
                bw.WriteUInt32(UniqueIdPart3);
                bw.BigEndian = false;
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
                bw.WriteByte(LightingModel);
                bw.WriteByte(UnkA1);
                bw.WriteSByte(ShadowDepthBias);
                bw.WriteBoolean(ShadowModelCullFlip);
                bw.WriteSingle(SourceRadius);
                bw.WriteBoolean(UsesProjectionTexture);
                bw.WriteByte(0);
                bw.WriteInt16(0);
                bw.WriteSingle(ShadowIntensity);
                bw.WriteVarint(textureOffset);
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
                    bw.WriteByte(ShadowSpecLevel);
                    bw.WriteSByte(UnkE5);
                    bw.WriteInt16(0);
                }

                if (lightSize > 0xE8)
                {
                    bw.WriteInt32(UnkE8);
                    bw.WriteInt32(UnkEC);
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
