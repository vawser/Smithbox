using System.Collections.Generic;

namespace SoulsFormats
{
    /// <summary>
    /// FaceGen controls: the named sliders of a FaceGen model, as directions in the space of the EGM and EGT mode
    /// coefficients, and the age, gender and caricature statistics of each race. Every FromSoftware game that uses
    /// FaceGen ships the same file, si.ctl. Extension: .ctl (.ctlbe big-endian)
    /// </summary>
    /// <remarks>
    /// The layout after the linear controls follows FaceSouls (github.com/lexlayer93/FaceSouls), MIT licence,
    /// Copyright (c) lexlayer93.
    /// </remarks>
    public class CTL : SoulsFile<CTL>
    {
        /// <summary>
        /// The races of a FaceGen model, in file order. Each has its own age and gender controls and statistics.
        /// </summary>
        public static readonly string[] RaceNames = { "All", "Afro", "Asia", "Eind", "Euro" };

        /// <summary>
        /// Whether the file is big-endian.
        /// </summary>
        public bool BigEndian { get; set; }

        /// <summary>
        /// Must match the EGM basis version.
        /// </summary>
        public uint GeometryBasisVersion { get; set; }

        /// <summary>
        /// Must match the EGT basis version.
        /// </summary>
        public uint TextureBasisVersion { get; set; }

        /// <summary>
        /// The number of symmetric geometry modes (GS): the length of every geometry symmetric coefficient vector.
        /// </summary>
        public int GeometrySymmetricModes { get; set; }

        /// <summary>
        /// The number of asymmetric geometry modes (GA).
        /// </summary>
        public int GeometryAsymmetricModes { get; set; }

        /// <summary>
        /// The number of symmetric texture modes (TS).
        /// </summary>
        public int TextureSymmetricModes { get; set; }

        /// <summary>
        /// The number of asymmetric texture modes (TA).
        /// </summary>
        public int TextureAsymmetricModes { get; set; }

        /// <summary>
        /// The shape sliders (61). Index n is the control that FaceParam's faceGeoDataNN holds the value of.
        /// </summary>
        public List<LinearControl> GeometrySymmetricControls { get; set; }

        public List<LinearControl> GeometryAsymmetricControls { get; set; }

        /// <summary>
        /// The texture sliders (36). Index n is the control of FaceParam's faceTexDataNN.
        /// </summary>
        public List<LinearControl> TextureSymmetricControls { get; set; }

        public List<LinearControl> TextureAsymmetricControls { get; set; }

        /// <summary>
        /// One per name in <see cref="RaceNames"/>, in that order.
        /// </summary>
        public List<Race> Races { get; set; }

        public CTL()
        {
            GeometrySymmetricControls = new List<LinearControl>();
            GeometryAsymmetricControls = new List<LinearControl>();
            TextureSymmetricControls = new List<LinearControl>();
            TextureAsymmetricControls = new List<LinearControl>();
            Races = new List<Race>();
        }

        protected override bool Is(BinaryReaderEx br)
        {
            return br.Length >= 32 && br.GetASCII(0, 8) == "FRCTL001";
        }

        protected override void Read(BinaryReaderEx br)
        {
            br.AssertASCII("FRCTL001");
            // Offset 8 holds the basis version, a hash, so the byte order is read from GS
            BigEndian = br.BigEndian = FaceGenUtil.DetectBigEndian(br, 16);

            GeometryBasisVersion = br.ReadUInt32();
            TextureBasisVersion = br.ReadUInt32();
            GeometrySymmetricModes = br.ReadInt32();
            GeometryAsymmetricModes = br.ReadInt32();
            TextureSymmetricModes = br.ReadInt32();
            TextureAsymmetricModes = br.ReadInt32();

            GeometrySymmetricControls = ReadLinearControls(br, GeometrySymmetricModes);
            GeometryAsymmetricControls = ReadLinearControls(br, GeometryAsymmetricModes);
            TextureSymmetricControls = ReadLinearControls(br, TextureSymmetricModes);
            TextureAsymmetricControls = ReadLinearControls(br, TextureAsymmetricModes);

            Races = new List<Race>(RaceNames.Length);
            foreach (string name in RaceNames)
            {
                var race = new Race { Name = name };
                race.Age = new OffsetControl(br, GeometrySymmetricModes, TextureSymmetricModes);
                race.Gender = new OffsetControl(br, GeometrySymmetricModes, TextureSymmetricModes);
                Races.Add(race);
            }

            // Each race has a morph towards every other race, in race order
            foreach (Race race in Races)
            {
                race.Morphs = new List<RaceMorph>(RaceNames.Length - 1);
                foreach (string target in RaceNames)
                {
                    if (target == race.Name)
                        continue;
                    race.Morphs.Add(new RaceMorph(br, target, GeometrySymmetricModes, TextureSymmetricModes));
                }
            }

            int gs = GeometrySymmetricModes;
            int ts = TextureSymmetricModes;
            foreach (Race race in Races)
            {
                race.GeometryMean = br.ReadSingles(gs);
                race.TextureMean = br.ReadSingles(ts);
                race.CombinedDensity = br.ReadSingles((gs + ts) * (gs + ts));
                race.GeometryDensity = br.ReadSingles(gs * gs);
                race.TextureDensity = br.ReadSingles(ts * ts);
            }
        }

        protected override void Write(BinaryWriterEx bw)
        {
            bw.BigEndian = BigEndian;
            int gs = GeometrySymmetricModes;
            int ts = TextureSymmetricModes;
            FaceGenUtil.AssertLength("CTL races", RaceNames.Length, Races.Count);

            bw.WriteASCII("FRCTL001");
            bw.WriteUInt32(GeometryBasisVersion);
            bw.WriteUInt32(TextureBasisVersion);
            bw.WriteInt32(gs);
            bw.WriteInt32(GeometryAsymmetricModes);
            bw.WriteInt32(ts);
            bw.WriteInt32(TextureAsymmetricModes);

            WriteLinearControls(bw, GeometrySymmetricControls, gs);
            WriteLinearControls(bw, GeometryAsymmetricControls, GeometryAsymmetricModes);
            WriteLinearControls(bw, TextureSymmetricControls, ts);
            WriteLinearControls(bw, TextureAsymmetricControls, TextureAsymmetricModes);

            foreach (Race race in Races)
            {
                race.Age.Write(bw, gs, ts);
                race.Gender.Write(bw, gs, ts);
            }

            foreach (Race race in Races)
            {
                FaceGenUtil.AssertLength($"CTL morphs of race {race.Name}", RaceNames.Length - 1, race.Morphs.Count);
                foreach (RaceMorph morph in race.Morphs)
                    morph.Write(bw, gs, ts);
            }

            foreach (Race race in Races)
            {
                WriteFloats(bw, race.GeometryMean, gs, "geometry mean");
                WriteFloats(bw, race.TextureMean, ts, "texture mean");
                WriteFloats(bw, race.CombinedDensity, (gs + ts) * (gs + ts), "combined density");
                WriteFloats(bw, race.GeometryDensity, gs * gs, "geometry density");
                WriteFloats(bw, race.TextureDensity, ts * ts, "texture density");
            }
        }

        /// <summary>
        /// The race the character creators use.
        /// </summary>
        public Race AllRaces => Races.Count > 0 ? Races[0] : null;

        private static List<LinearControl> ReadLinearControls(BinaryReaderEx br, int dimension)
        {
            int count = br.ReadInt32();
            var controls = new List<LinearControl>(count);
            for (int i = 0; i < count; i++)
                controls.Add(new LinearControl(br, dimension));
            return controls;
        }

        private static void WriteLinearControls(BinaryWriterEx bw, List<LinearControl> controls, int dimension)
        {
            bw.WriteInt32(controls.Count);
            foreach (LinearControl control in controls)
                control.Write(bw, dimension);
        }

        internal static void WriteFloats(BinaryWriterEx bw, float[] values, int length, string what)
        {
            FaceGenUtil.AssertLength($"CTL {what}", length, values.Length);
            bw.WriteSingles(values);
        }

        /// <summary>
        /// A named slider: its value is the dot product of its coefficients with the mode coefficients. The
        /// coefficient vectors are unit length, so setting value q moves the face by (q - current) × Coefficients.
        /// </summary>
        public class LinearControl
        {
            public float[] Coefficients { get; set; }

            public string Label { get; set; }

            public LinearControl() { Coefficients = new float[0]; Label = ""; }

            internal LinearControl(BinaryReaderEx br, int dimension)
            {
                Coefficients = br.ReadSingles(dimension);
                Label = FaceGenUtil.ReadLabel(br);
            }

            internal void Write(BinaryWriterEx bw, int dimension)
            {
                WriteFloats(bw, Coefficients, dimension, $"coefficients of {Label}");
                FaceGenUtil.WriteLabel(bw, Label, terminated: false);
            }
        }

        /// <summary>
        /// Age or gender: value = offset + coefficients · modes, for geometry and for texture separately.
        /// </summary>
        public class OffsetControl
        {
            public float[] GeometryCoefficients { get; set; }

            public float GeometryOffset { get; set; }

            public float[] TextureCoefficients { get; set; }

            public float TextureOffset { get; set; }

            public OffsetControl() { GeometryCoefficients = new float[0]; TextureCoefficients = new float[0]; }

            internal OffsetControl(BinaryReaderEx br, int gs, int ts)
            {
                GeometryCoefficients = br.ReadSingles(gs);
                GeometryOffset = br.ReadSingle();
                TextureCoefficients = br.ReadSingles(ts);
                TextureOffset = br.ReadSingle();
            }

            internal void Write(BinaryWriterEx bw, int gs, int ts)
            {
                WriteFloats(bw, GeometryCoefficients, gs, "age/gender geometry coefficients");
                bw.WriteSingle(GeometryOffset);
                WriteFloats(bw, TextureCoefficients, ts, "age/gender texture coefficients");
                bw.WriteSingle(TextureOffset);
            }
        }

        /// <summary>
        /// How far a face looks like another race: offset + geometry · modes + texture · modes.
        /// </summary>
        public class RaceMorph
        {
            /// <summary>
            /// The race this morph moves towards. Not stored: given by the file order.
            /// </summary>
            public string TargetRace { get; set; }

            public float[] GeometryCoefficients { get; set; }

            public float[] TextureCoefficients { get; set; }

            public float Offset { get; set; }

            public RaceMorph() { TargetRace = ""; GeometryCoefficients = new float[0]; TextureCoefficients = new float[0]; }

            internal RaceMorph(BinaryReaderEx br, string target, int gs, int ts)
            {
                TargetRace = target;
                GeometryCoefficients = br.ReadSingles(gs);
                TextureCoefficients = br.ReadSingles(ts);
                Offset = br.ReadSingle();
            }

            internal void Write(BinaryWriterEx bw, int gs, int ts)
            {
                WriteFloats(bw, GeometryCoefficients, gs, "race morph geometry coefficients");
                WriteFloats(bw, TextureCoefficients, ts, "race morph texture coefficients");
                bw.WriteSingle(Offset);
            }
        }

        /// <summary>
        /// The age and gender controls of a race, its morphs towards the other races, and the statistics the
        /// caricature control measures against: the mean face and the density (whitening) matrices, row-major.
        /// </summary>
        public class Race
        {
            public string Name { get; set; }

            public OffsetControl Age { get; set; }

            public OffsetControl Gender { get; set; }

            /// <summary>
            /// One towards each other race, in <see cref="RaceNames"/> order.
            /// </summary>
            public List<RaceMorph> Morphs { get; set; }

            /// <summary>
            /// GS values.
            /// </summary>
            public float[] GeometryMean { get; set; }

            /// <summary>
            /// TS values.
            /// </summary>
            public float[] TextureMean { get; set; }

            /// <summary>
            /// (GS + TS)² values.
            /// </summary>
            public float[] CombinedDensity { get; set; }

            /// <summary>
            /// GS² values.
            /// </summary>
            public float[] GeometryDensity { get; set; }

            /// <summary>
            /// TS² values.
            /// </summary>
            public float[] TextureDensity { get; set; }

            public Race()
            {
                Name = "";
                Age = new OffsetControl();
                Gender = new OffsetControl();
                Morphs = new List<RaceMorph>();
                GeometryMean = TextureMean = CombinedDensity = GeometryDensity = TextureDensity = new float[0];
            }
        }
    }
}
