using System.Collections.Generic;

namespace SoulsFormats
{
    /// <summary>
    /// A Sekiro file that lists triangles of a collision that can't be jumped off. Like WALL, it holds no
    /// geometry of its own: each entry is the shape key of a triangle of the low collision with the same ID.
    /// Extension: .nojumpwall
    /// </summary>
    public class NOJUMPWALL : SoulsFile<NOJUMPWALL>
    {
        /// <summary>
        /// The shape keys of the triangles of the low collision, in ascending order.
        /// See <see cref="WALL.Triangle.ShapeKey"/> for their layout.
        /// </summary>
        public List<uint> ShapeKeys { get; set; }

        /// <summary>
        /// Creates an empty NOJUMPWALL.
        /// </summary>
        public NOJUMPWALL()
        {
            ShapeKeys = new List<uint>();
        }

        /// <summary>
        /// Checks whether the data appears to be a file of this format.
        /// </summary>
        protected override bool Is(BinaryReaderEx br)
        {
            return br.Length >= 0x20 && br.GetInt32(0) == 1 && br.GetInt64(8) == 0x10;
        }

        /// <summary>
        /// Deserializes file data from a stream.
        /// </summary>
        protected override void Read(BinaryReaderEx br)
        {
            br.BigEndian = false;
            br.AssertInt32(1);
            br.AssertInt32(1);
            long listOffset = br.ReadInt64();

            br.Position = listOffset;
            long keysOffset = br.ReadInt64();
            long count = br.ReadInt64();

            br.Position = keysOffset;
            ShapeKeys = new List<uint>((int)count);
            for (int i = 0; i < count; i++)
                ShapeKeys.Add(br.ReadUInt32());
        }

        /// <summary>
        /// Serializes file data to a stream.
        /// </summary>
        protected override void Write(BinaryWriterEx bw)
        {
            bw.BigEndian = false;
            bw.WriteInt32(1);
            bw.WriteInt32(1);
            bw.WriteInt64(0x10);

            bw.WriteInt64(0x20);
            bw.WriteInt64(ShapeKeys.Count);

            foreach (uint key in ShapeKeys)
                bw.WriteUInt32(key);
        }
    }
}
