using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum ListShapeFlags
    {
        ALL_FLAGS_CLEAR = 0,
        DISABLE_SPU_CACHE_FOR_LIST_CHILD_INFO = 1,
    }
    
    public partial class hkpListShape : hkpShapeCollection
    {
        public override uint Signature { get => 4075568853; }
        
        public List<hkpListShapeChildInfo> m_childInfo;
        public ushort m_flags;
        public ushort m_numDisabledChildren;
        public Vector4 m_aabbHalfExtents;
        public Vector4 m_aabbCenter;
        public uint m_enabledChildren_0;
        public uint m_enabledChildren_1;
        public uint m_enabledChildren_2;
        public uint m_enabledChildren_3;
        public uint m_enabledChildren_4;
        public uint m_enabledChildren_5;
        public uint m_enabledChildren_6;
        public uint m_enabledChildren_7;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_childInfo = des.ReadClassArray<hkpListShapeChildInfo>(br);
            m_flags = br.ReadUInt16();
            m_numDisabledChildren = br.ReadUInt16();
            br.ReadUInt64();
            br.ReadUInt32();
            m_aabbHalfExtents = des.ReadVector4(br);
            m_aabbCenter = des.ReadVector4(br);
            m_enabledChildren_0 = br.ReadUInt32();
            m_enabledChildren_1 = br.ReadUInt32();
            m_enabledChildren_2 = br.ReadUInt32();
            m_enabledChildren_3 = br.ReadUInt32();
            m_enabledChildren_4 = br.ReadUInt32();
            m_enabledChildren_5 = br.ReadUInt32();
            m_enabledChildren_6 = br.ReadUInt32();
            m_enabledChildren_7 = br.ReadUInt32();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassArray<hkpListShapeChildInfo>(bw, m_childInfo);
            bw.WriteUInt16(m_flags);
            bw.WriteUInt16(m_numDisabledChildren);
            bw.WriteUInt64(0);
            bw.WriteUInt32(0);
            s.WriteVector4(bw, m_aabbHalfExtents);
            s.WriteVector4(bw, m_aabbCenter);
            bw.WriteUInt32(m_enabledChildren_0);
            bw.WriteUInt32(m_enabledChildren_1);
            bw.WriteUInt32(m_enabledChildren_2);
            bw.WriteUInt32(m_enabledChildren_3);
            bw.WriteUInt32(m_enabledChildren_4);
            bw.WriteUInt32(m_enabledChildren_5);
            bw.WriteUInt32(m_enabledChildren_6);
            bw.WriteUInt32(m_enabledChildren_7);
        }
    }
}
