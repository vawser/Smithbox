using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbPinBonesGenerator : hkbGenerator
    {
        public override uint Signature { get => 839140489; }
        
        public hkbGenerator m_referenceFrameGenerator;
        public hkbGenerator m_pinnedGenerator;
        public hkbBoneIndexArray m_boneIndices;
        public float m_fraction;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_referenceFrameGenerator = des.ReadClassPointer<hkbGenerator>(br);
            m_pinnedGenerator = des.ReadClassPointer<hkbGenerator>(br);
            m_boneIndices = des.ReadClassPointer<hkbBoneIndexArray>(br);
            m_fraction = br.ReadSingle();
            br.ReadUInt32();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassPointer<hkbGenerator>(bw, m_referenceFrameGenerator);
            s.WriteClassPointer<hkbGenerator>(bw, m_pinnedGenerator);
            s.WriteClassPointer<hkbBoneIndexArray>(bw, m_boneIndices);
            bw.WriteSingle(m_fraction);
            bw.WriteUInt32(0);
        }
    }
}
