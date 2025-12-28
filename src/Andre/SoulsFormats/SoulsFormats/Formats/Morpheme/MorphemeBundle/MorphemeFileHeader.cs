using Org.BouncyCastle.Crypto.Agreement.Srp;

namespace SoulsFormats.Formats.Morpheme.MorphemeBundle
{
    /// <summary>
    /// Header for Morpheme files.
    /// </summary>
    public class MorphemeFileHeader : MorphemeBundle_Base
    {
        /// <summary>
        /// Runtime Target Name Size
        /// </summary>
        public long runtimeTargetNameSize { get; set; }

        /// <summary>
        /// RuntimeTargetName (Yes it's meant to be name)
        /// </summary>
        public long runtimeTargetName { get; set; }

        /// <summary>
        /// Unknown variable integer 0
        /// </summary>
        public long unkVarint0 { get; set; }

        /// <summary>
        /// Unknown variable integer 1
        /// </summary>
        public long unkVarint1 { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public MorphemeFileHeader() { }

        /// <summary>
        /// Constructor for reading a MorphemeBundle
        /// </summary>
        /// <param name="br"></param>
        public MorphemeFileHeader(BinaryReaderEx br)
        {
            Read(br);
        }

        /// <summary>
        /// Method for getting size of this MorphemeBundle
        /// </summary>
        public override long CalculateBundleSize()
        {
            return isX64 ? 0x20 : 0x14;
        }

        /// <summary>
        /// Method for reading a MorphemeBundle
        /// </summary>
        public override void Read(BinaryReaderEx br)
        {
            base.Read(br);
            runtimeTargetNameSize = br.ReadInt64();
            runtimeTargetName = br.ReadVarint();
            unkVarint0 = br.ReadVarint();
            unkVarint1 = br.ReadVarint();
        }

        /// <summary>
        /// Method for writing a MorphemeBundle
        /// </summary>
        public override void Write(BinaryWriterEx bw)
        {
            base.Write(bw);
            bw.WriteInt64(runtimeTargetNameSize);
            bw.WriteVarint(runtimeTargetName);
            bw.WriteVarint(unkVarint0);
            bw.WriteVarint(unkVarint1);
        }
    }
}
