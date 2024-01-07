using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbBlenderGeneratorChild : hkbBindable
    {
        public override uint Signature { get => 3009134547; }
        
        public hkbGenerator m_generator;
        public hkbBoneWeightArray m_boneWeights;
        public float m_weight;
        public float m_worldFromModelWeight;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_generator = des.ReadClassPointer<hkbGenerator>(br);
            m_boneWeights = des.ReadClassPointer<hkbBoneWeightArray>(br);
            m_weight = br.ReadSingle();
            m_worldFromModelWeight = br.ReadSingle();
            br.ReadUInt64();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassPointer<hkbGenerator>(bw, m_generator);
            s.WriteClassPointer<hkbBoneWeightArray>(bw, m_boneWeights);
            bw.WriteSingle(m_weight);
            bw.WriteSingle(m_worldFromModelWeight);
            bw.WriteUInt64(0);
        }
    }
}
