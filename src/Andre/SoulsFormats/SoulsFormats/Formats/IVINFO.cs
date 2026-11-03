using System.Numerics;

namespace SoulsFormats
{
    /// <summary>
    /// An Armored Core VI, Elden Ring and Nightreign file that accompanies one baked irradiance volume of a map.
    /// The volume itself is a set of four 3D BC7 textures named GIIV####_##_W, _X, _Y and _Z in the map's envmap
    /// TPFs, holding the L0 and L1 spherical harmonic bands of the indirect light; this file shares the volume's
    /// name and holds a single summary of that light.
    /// Files are packed by quality level into ivinfobnds, such as m10_00_00_00_high.ivinfobnd.dcx.
    /// Extension: .ivInfo
    /// </summary>
    public class IVINFO : SoulsFile<IVINFO>
    {
        /// <summary>
        /// The size of the file, which is the same in every known game.
        /// </summary>
        private const int FileSize = 0x1C;

        /// <summary>
        /// The format version: 1 in all known files.
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// Appears to be the volume's light as a single set of luminance spherical harmonic coefficients, with the
        /// L1 band in XYZ and the L0 band in W. W is non-negative in nearly all files and Y is the largest of the L1
        /// terms in most, as expected of the vertical light of a Y-up world. Volumes that share an ID but differ in
        /// their last index, such as GIIV0500_00 to _05, hold almost identical values.
        /// </summary>
        public Vector4 Irradiance { get; set; }

        /// <summary>
        /// Unknown; 0 in Armored Core VI and 1 in Elden Ring and Nightreign.
        /// </summary>
        public float Unk18 { get; set; }

        /// <summary>
        /// Creates an IVINFO of the Elden Ring kind with no light.
        /// </summary>
        public IVINFO()
        {
            Version = 1;
            Irradiance = Vector4.Zero;
            Unk18 = 1;
        }

        /// <summary>
        /// Creates a copy of the IVINFO.
        /// </summary>
        public IVINFO Clone()
        {
            return (IVINFO)MemberwiseClone();
        }

        /// <summary>
        /// Checks whether the data appears to be a file of this format.
        /// </summary>
        protected override bool Is(BinaryReaderEx br)
        {
            if (br.Length != FileSize)
                return false;

            return br.GetInt32(0) == 1 && br.GetInt32(4) == 0;
        }

        /// <summary>
        /// Deserializes file data from a stream.
        /// </summary>
        protected override void Read(BinaryReaderEx br)
        {
            br.BigEndian = false;
            Version = br.AssertInt32(1);
            br.AssertInt32(0);
            Irradiance = br.ReadVector4();
            Unk18 = br.ReadSingle();
        }

        /// <summary>
        /// Serializes file data to a stream.
        /// </summary>
        protected override void Write(BinaryWriterEx bw)
        {
            bw.BigEndian = false;
            bw.WriteInt32(Version);
            bw.WriteInt32(0);
            bw.WriteVector4(Irradiance);
            bw.WriteSingle(Unk18);
        }

        /// <summary>
        /// Returns the light and the unknown value of the file.
        /// </summary>
        public override string ToString()
        {
            return $"{Irradiance} {Unk18}";
        }
    }
}
