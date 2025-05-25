using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum hkpFilterType
    {
        HK_FILTER_UNKNOWN = 0,
        HK_FILTER_NULL = 1,
        HK_FILTER_GROUP = 2,
        HK_FILTER_LIST = 3,
        HK_FILTER_CUSTOM = 4,
        HK_FILTER_PAIR = 5,
        HK_FILTER_CONSTRAINT = 6,
    }
    
    public partial class hkpCollisionFilter : hkReferencedObject
    {
        public override uint Signature { get => 603088937; }
        
        public uint m_prepad_0;
        public uint m_prepad_1;
        public hkpFilterType m_type;
        public uint m_postpad_0;
        public uint m_postpad_1;
        public uint m_postpad_2;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            m_prepad_0 = br.ReadUInt32();
            m_prepad_1 = br.ReadUInt32();
            m_type = (hkpFilterType)br.ReadUInt32();
            m_postpad_0 = br.ReadUInt32();
            m_postpad_1 = br.ReadUInt32();
            m_postpad_2 = br.ReadUInt32();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt32(m_prepad_0);
            bw.WriteUInt32(m_prepad_1);
            bw.WriteUInt32((uint)m_type);
            bw.WriteUInt32(m_postpad_0);
            bw.WriteUInt32(m_postpad_1);
            bw.WriteUInt32(m_postpad_2);
        }
    }
}
