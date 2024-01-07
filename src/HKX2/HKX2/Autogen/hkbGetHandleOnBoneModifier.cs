using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbGetHandleOnBoneModifier : hkbModifier
    {
        public override uint Signature { get => 1881908988; }
        
        public hkbHandle m_handleOut;
        public string m_localFrameName;
        public short m_ragdollBoneIndex;
        public short m_animationBoneIndex;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_handleOut = des.ReadClassPointer<hkbHandle>(br);
            m_localFrameName = des.ReadStringPointer(br);
            m_ragdollBoneIndex = br.ReadInt16();
            m_animationBoneIndex = br.ReadInt16();
            br.ReadUInt32();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassPointer<hkbHandle>(bw, m_handleOut);
            s.WriteStringPointer(bw, m_localFrameName);
            bw.WriteInt16(m_ragdollBoneIndex);
            bw.WriteInt16(m_animationBoneIndex);
            bw.WriteUInt32(0);
        }
    }
}
