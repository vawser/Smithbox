using System.Numerics;

namespace SoulsFormats
{
    /// <summary>
    /// General metadata about a FLVER.
    /// </summary>
    public class FLVER2Header
    {
        /// <summary>
        /// If true FLVER will be written big-endian, if false little-endian.
        /// </summary>
        public bool BigEndian { get; set; }

        /// <summary>
        /// Version of the format indicating presence of various features.
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// Minimum extent of the entire model.
        /// </summary>
        public Vector3 BoundingBoxMin { get; set; }

        /// <summary>
        /// Maximum extent of the entire model.
        /// </summary>
        public Vector3 BoundingBoxMax { get; set; }

        /// <summary>
        /// If true strings are UTF-16, if false Shift-JIS.
        /// </summary>
        public bool Unicode { get; set; }

        /// <summary>
        /// Unknown.
        /// </summary>
        public bool Unk4A { get; set; }

        /// <summary>
        /// Unknown; I believe this is the primitive restart constant, but I'm not certain.
        /// </summary>
        public int Unk4C { get; set; }

        /// <summary>
        /// Unknown.
        /// </summary>
        public byte Unk5C { get; set; }

        /// <summary>
        /// Unknown.
        /// </summary>
        public byte Unk5D { get; set; }

        /// <summary>
        /// Unknown.
        /// </summary>
        public int Unk68 { get; set; }

        /// <summary>
        /// Creates a FLVERHeader with default values.
        /// </summary>
        public FLVER2Header()
        {
            BigEndian = false;
            Version = 0x20014;
            Unicode = true;
        }
    }
}
