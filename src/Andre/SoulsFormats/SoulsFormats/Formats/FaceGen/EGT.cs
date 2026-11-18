using System.Collections.Generic;

namespace SoulsFormats
{
    /// <summary>
    /// FaceGen texture modes: for each statistical mode, how far it shifts every texel of the base face texture.
    /// Extension: .egt (.egtbe big-endian)
    /// </summary>
    public class EGT : SoulsFile<EGT>
    {
        /// <summary>
        /// Whether the file is big-endian.
        /// </summary>
        public bool BigEndian { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        /// <summary>
        /// The version of the statistical model; must match the CTL's texture basis version.
        /// </summary>
        public uint BasisVersion { get; set; }

        /// <summary>
        /// Modes that keep the texture symmetric (50 in every game).
        /// </summary>
        public List<Mode> SymmetricModes { get; set; }

        /// <summary>
        /// Modes that make the texture asymmetric (none in any game).
        /// </summary>
        public List<Mode> AsymmetricModes { get; set; }

        public EGT()
        {
            SymmetricModes = new List<Mode>();
            AsymmetricModes = new List<Mode>();
        }

        protected override bool Is(BinaryReaderEx br)
        {
            return br.Length >= 64 && br.GetASCII(0, 8) == "FREGT003";
        }

        protected override void Read(BinaryReaderEx br)
        {
            br.AssertASCII("FREGT003");
            BigEndian = br.BigEndian = FaceGenUtil.DetectBigEndian(br, 8);

            Height = br.ReadInt32();
            Width = br.ReadInt32();
            int symmetricCount = br.ReadInt32();
            int asymmetricCount = br.ReadInt32();
            BasisVersion = br.ReadUInt32();
            br.AssertPattern(36, 0);

            int texels = Width * Height;
            SymmetricModes = new List<Mode>(symmetricCount);
            for (int i = 0; i < symmetricCount; i++)
                SymmetricModes.Add(new Mode(br, texels));

            AsymmetricModes = new List<Mode>(asymmetricCount);
            for (int i = 0; i < asymmetricCount; i++)
                AsymmetricModes.Add(new Mode(br, texels));
        }

        protected override void Write(BinaryWriterEx bw)
        {
            bw.BigEndian = BigEndian;
            bw.WriteASCII("FREGT003");
            bw.WriteInt32(Height);
            bw.WriteInt32(Width);
            bw.WriteInt32(SymmetricModes.Count);
            bw.WriteInt32(AsymmetricModes.Count);
            bw.WriteUInt32(BasisVersion);
            bw.WritePattern(36, 0);

            int texels = Width * Height;
            foreach (Mode mode in SymmetricModes)
                mode.Write(bw, texels);
            foreach (Mode mode in AsymmetricModes)
                mode.Write(bw, texels);
        }

        /// <summary>
        /// One mode: delta of texel t in channel c = Channel[t] × Scale, rows top to bottom.
        /// </summary>
        public class Mode
        {
            public float Scale { get; set; }

            public sbyte[] Red { get; set; }

            public sbyte[] Green { get; set; }

            public sbyte[] Blue { get; set; }

            public Mode() { Red = Green = Blue = new sbyte[0]; }

            internal Mode(BinaryReaderEx br, int texels)
            {
                Scale = br.ReadSingle();
                Red = br.ReadSBytes(texels);
                Green = br.ReadSBytes(texels);
                Blue = br.ReadSBytes(texels);
            }

            internal void Write(BinaryWriterEx bw, int texels)
            {
                FaceGenUtil.AssertLength("EGT red", texels, Red.Length);
                FaceGenUtil.AssertLength("EGT green", texels, Green.Length);
                FaceGenUtil.AssertLength("EGT blue", texels, Blue.Length);
                bw.WriteSingle(Scale);
                bw.WriteSBytes(Red);
                bw.WriteSBytes(Green);
                bw.WriteSBytes(Blue);
            }
        }
    }
}
