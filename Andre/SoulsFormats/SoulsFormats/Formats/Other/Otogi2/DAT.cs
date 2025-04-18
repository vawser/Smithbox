using System;
using System.Collections.Generic;

namespace SoulsFormats.Otogi2
{
    /// <summary>
    /// Model-related file container used in Otogi 2. Extension: .dat
    /// </summary>
    public class DAT : SoulsFile<DAT>
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public byte[] Data1;
        public byte[] Data2;
        public byte[] Data3;
        public List<Texture> Textures;

        protected override void Read(BinaryReaderEx br)
        {
            br.ReadInt32(); // File size
            int offset1 = br.ReadInt32();
            int offset2 = br.ReadInt32();
            int offset3 = br.ReadInt32();
            int textureCount = br.ReadInt32();
            br.AssertInt32(0);
            br.AssertInt32(0);
            br.AssertInt32(0);

            Textures = new List<Texture>(textureCount);
            for (int i = 0; i < textureCount; i++)
                Textures.Add(new Texture(br));

            if (offset1 != 0)
                Data1 = br.GetBytes(offset1, br.GetInt32(offset1));

            if (offset2 != 0)
                Data2 = br.GetBytes(offset2, br.GetInt32(offset2));

            if (offset3 != 0)
                Data3 = br.GetBytes(offset3, br.GetInt32(offset3));
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
