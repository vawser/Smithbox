using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum MergeType
    {
        UNUSED_MERGING_SIMPLE = 0,
        UNUSED_MERGING_CONVEX_HULL = 1,
    }
    
    public partial class hkaiSilhouetteMerger : hkReferencedObject
    {
        public override uint Signature { get => 459273792; }
        
        public MergeType m_mergeType;
        public hkaiSilhouetteGenerationParameters m_mergeParams;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_mergeType = (MergeType)br.ReadByte();
            br.ReadUInt64();
            br.ReadUInt32();
            br.ReadUInt16();
            br.ReadByte();
            m_mergeParams = new hkaiSilhouetteGenerationParameters();
            m_mergeParams.Read(des, br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteByte((byte)m_mergeType);
            bw.WriteUInt64(0);
            bw.WriteUInt32(0);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
            m_mergeParams.Write(s, bw);
        }
    }
}
