using System;
using System.Collections.Generic;

namespace SoulsFormats.MWC
{
    /// <summary>
    /// Texture container used in Metal Wolf Chaos. Extension: _t.dat
    /// </summary>
    public class TDAT : SoulsFile<TDAT>
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public int Unk1C;
        public List<Texture> Textures;

        protected override void Read(BinaryReaderEx br)
        {
            br.BigEndian = false;
            br.ReadInt32(); // File size
            br.AssertPattern(0xC, 0x00);
            int textureCount = br.ReadInt32();
            br.AssertPattern(0x8, 0x00);
            Unk1C = br.ReadInt32();

            Textures = new List<Texture>(textureCount);
            for (int i = 0; i < textureCount; i++)
                Textures.Add(new Texture(br));
        }

        public class Texture
        {
            public string Name;
            public byte[] Data;

            internal Texture(BinaryReaderEx br)
            {
                int dataLength = br.ReadInt32();
                int dataOffset = br.ReadInt32();
                int nameOffset = br.ReadInt32();

                Name = br.GetShiftJIS(nameOffset);
                Data = br.GetBytes(dataOffset, dataLength);
            }
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
