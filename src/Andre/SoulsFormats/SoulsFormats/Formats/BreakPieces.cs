using System.Collections.Generic;
using System.IO;

namespace SoulsFormats
{
    /// <summary>
    /// The break data of one breakable object or asset in a BREAKOBJ or BREAKGEOM: one 8-byte record per piece
    /// of its model. The number of pieces is fixed for each model, but the records differ between instances of it,
    /// so they are baked for each placement (most likely the poses the pieces come to rest in).
    /// </summary>
    public class BreakPieces
    {
        /// <summary>
        /// Unknown; 1 in every file except some Demon's Souls objects, which use 2.
        /// </summary>
        public byte Type { get; set; }

        /// <summary>
        /// One record per piece of the model. Each packs several small signed fields, but their meaning is unknown.
        /// </summary>
        public List<ulong> Pieces { get; set; }

        /// <summary>
        /// Creates break data with no pieces.
        /// </summary>
        public BreakPieces()
        {
            Type = 1;
            Pieces = new List<ulong>();
        }

        /// <summary>
        /// Creates a deep copy of the break data.
        /// </summary>
        public BreakPieces DeepCopy()
        {
            return new BreakPieces { Type = Type, Pieces = new List<ulong>(Pieces) };
        }

        /// <summary>
        /// The most pieces a block can hold, limited by its 13-bit length.
        /// </summary>
        public const int MaxPieces = (0x1FFF - 3) / 8;

        // The block starts with a 3-bit type and a 13-bit block length (including this header), packed as C bitfields:
        // from the low bit of the first byte in little-endian files, and from its high bit in big-endian ones.
        // The low 8 bits of the length always fill the second byte. The piece count follows as a byte, so models
        // with more than 255 pieces store only its low 8 bits, and the length is what gives the true count.
        internal static BreakPieces Read(BinaryReaderEx br)
        {
            byte packed = br.ReadByte();
            byte lengthLow = br.ReadByte();
            byte countLow = br.ReadByte();

            int type = br.BigEndian ? packed >> 5 : packed & 7;
            int lengthHigh = br.BigEndian ? packed & 0x1F : packed >> 3;
            int length = lengthHigh << 8 | lengthLow;
            int count = (length - 3) / 8;
            if (length != 3 + count * 8 || (byte)count != countLow)
                throw new InvalidDataException($"Break block length 0x{length:X} does not match its piece count {countLow}.");

            var pieces = new BreakPieces { Type = (byte)type, Pieces = new List<ulong>(count) };
            for (int i = 0; i < count; i++)
                pieces.Pieces.Add(br.ReadUInt64());

            return pieces;
        }

        internal void Write(BinaryWriterEx bw)
        {
            if (Pieces.Count > MaxPieces)
                throw new InvalidDataException($"Break data can hold at most {MaxPieces} pieces, but has {Pieces.Count}.");
            if (Type > 7)
                throw new InvalidDataException($"Break data type {Type} does not fit in 3 bits.");

            int length = 3 + Pieces.Count * 8;
            byte packed = bw.BigEndian
                ? (byte)(Type << 5 | length >> 8)
                : (byte)(Type | length >> 8 << 3);

            bw.WriteByte(packed);
            bw.WriteByte((byte)length);
            bw.WriteByte((byte)Pieces.Count);
            foreach (ulong piece in Pieces)
                bw.WriteUInt64(piece);
        }
    }
}
