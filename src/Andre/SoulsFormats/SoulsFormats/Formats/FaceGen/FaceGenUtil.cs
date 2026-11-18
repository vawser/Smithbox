using System;
using System.IO;

namespace SoulsFormats
{
    /// <summary>
    /// Helpers shared by the FaceGen SDK formats (TRI, EGM, EGT, CTL).
    /// </summary>
    internal static class FaceGenUtil
    {
        /// <summary>
        /// FaceGen files carry no byte order mark. Demon's Souls ships big-endian copies (.tribe, .egmbe, ...), so the
        /// byte order is taken from a small count at the given offset: read little-endian, it must be a sane size.
        /// </summary>
        public static bool DetectBigEndian(BinaryReaderEx br, long offset)
        {
            bool bigEndian = br.BigEndian;
            br.BigEndian = false;
            int value = br.GetInt32(offset);
            br.BigEndian = bigEndian;
            return value < 0 || value > 10_000_000;
        }

        /// <summary>
        /// A label: int32 length, then the characters. In TRI files the length includes a terminator; in CTL files
        /// there is no terminator (confirmed against si.ctl in every game).
        /// </summary>
        public static string ReadLabel(BinaryReaderEx br)
        {
            int length = br.ReadInt32();
            if (length < 0 || length > br.Length - br.Position)
                throw new InvalidDataException($"Bad FaceGen label length {length} at 0x{br.Position - 4:X}.");
            string label = br.ReadASCII(length);
            int terminator = label.IndexOf('\0');
            return terminator >= 0 ? label.Substring(0, terminator) : label;
        }

        public static void WriteLabel(BinaryWriterEx bw, string label, bool terminated = true)
        {
            bw.WriteInt32(label.Length + (terminated ? 1 : 0));
            bw.WriteASCII(label, terminated);
        }

        /// <summary>
        /// The counts in a FaceGen header are not stored separately from the data, so a list of the wrong size
        /// would write a file the game reads wrongly.
        /// </summary>
        public static void AssertLength(string what, int expected, int actual)
        {
            if (expected != actual)
                throw new InvalidOperationException($"{what}: expected {expected}, found {actual}.");
        }
    }
}
