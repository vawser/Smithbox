namespace SoulsFormats.Formats.Morpheme.MorphemeBundle
{
    /// <summary>
    /// Interface for MorphemeBundle format used in Morpheme files in Dark Souls 2.
    /// </summary>
    public abstract class MorphemeBundle_Base
    {
        /// <summary>
        /// Enumerator used to determine what kind of data the packet holds
        /// </summary>
        public eBundleType bundleType { get; set; }

        /// <summary>
        /// Signature that can be used by a packet to reference another one.
        /// </summary>
        public uint signature { get; set; }

        /// <summary>
        /// Morpheme Bundle Header. Usually all 0s.
        /// </summary>
        public MorphemeBundleGUID guid { get; set; }


        /// <summary>
        /// Structure for Morpheme data and alignment info.
        /// </summary>
        public MorphemeSizeAlignFormatting format { get; set; }

        /// <summary>
        /// Unknown value
        /// </summary>
        public int unk0 { get; set; }

        /// <summary>
        /// Flag for if the data is x64 or not. Only set in software, not actually stored in the file.
        /// </summary>
        public bool isX64 { get; set; }

        /// <summary>
        /// Method for verifying data is a MorphemeBundle
        /// </summary>
        public bool Is(BinaryReaderEx br)
        {
            if (br.Length < 0xC)
                return false;

            var tempMagic0 = br.ReadInt32();
            var tempMagic1 = br.ReadInt32();

            return (tempMagic0 == 0x18) && (tempMagic1 == 0xA);
        }

        /// <summary>
        /// A check for 64 bit values and the file's endianness. BinaryReader is adjusted as needed for this.
        /// </summary>
        public static void Set64BitAndEndianness(BinaryReaderEx br)
        {
            br.VarintLong = true;
            var test = br.ReadUInt32();
            if (test > 0x18)
            {
                br.BigEndian = true;
            }

            //No Dark Souls 2 morpheme structs, much less files, should ever exceed the 32 bit limit. Therefore, if this is 64 bit, the 2nd 4 bytes will ALWAYS be 0.
            int additiveValue;
            if (br.BigEndian == true)
            {
                additiveValue = 0x1C;
            }
            else
            {
                additiveValue = 0x20;
            }

            br.Position += additiveValue;
            if (br.ReadUInt32() > 0)
            {
                br.VarintLong = false;
            }
            br.Position = 0;
        }

        /// <summary>
        /// Get the type of bundle and then return to the start of the bundle.
        /// </summary>
        /// <param name="br"></param>
        /// <returns></returns>
        public static eBundleType ReadBundleType(BinaryReaderEx br)
        {
            eBundleType bundleTypeCheck;
            br.StepIn(br.Position); br.AssertInt32(0x18);
            br.AssertInt32([0x6, 0xA]);
            bundleTypeCheck = br.ReadEnum32<eBundleType>();
            br.StepOut();

            return bundleTypeCheck;
        }

        /// <summary>
        /// Method for getting size of this MorphemeBundle
        /// </summary>
        public abstract long CalculateBundleSize();

        /// <summary>
        /// Method for reading a MorphemeBundle
        /// </summary>
        public virtual void Read(BinaryReaderEx br)
        {
            isX64 = br.VarintLong;
            br.AssertInt32(0x18);
            br.AssertInt32([0x6, 0xA]);
            bundleType = br.ReadEnum32<eBundleType>();
            signature = br.ReadUInt32();
            guid = new MorphemeBundleGUID
            {
                GUID_00 = br.ReadInt32(),
                GUID_04 = br.ReadInt32(),
                GUID_08 = br.ReadInt32(),
                GUID_0C = br.ReadInt32()
            };
            format = new MorphemeSizeAlignFormatting()
            {
                dataSize = br.ReadVarint(),
                dataAlignment = br.ReadInt32(),
                unkValue = isX64 ? br.ReadInt32() : 0
            };

            br.Pad((int)format.dataAlignment);
        }

        /// <summary>
        /// Method for writing a MorphemeBundle
        /// </summary>
        public virtual void Write(BinaryWriterEx bw)
        {
            bw.WriteUInt32(0x18);
            bw.WriteUInt32(0xA);
            bw.WriteInt32((int)bundleType);
            bw.WriteUInt32(signature);
            bw.WriteInt32(guid.GUID_00);
            bw.WriteInt32(guid.GUID_04);
            bw.WriteInt32(guid.GUID_08);
            bw.WriteInt32(guid.GUID_0C);
            bw.WriteVarint(CalculateBundleSize());
            bw.WriteVarint(format.dataAlignment);
            bw.Pad((int)format.dataAlignment);
        }
    }
}
