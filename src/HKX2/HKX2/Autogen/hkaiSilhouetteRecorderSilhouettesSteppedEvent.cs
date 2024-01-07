using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaiSilhouetteRecorderSilhouettesSteppedEvent : hkaiSilhouetteRecorderReplayEvent
    {
        public override uint Signature { get => 3999612546; }
        
        public StepThreading m_stepThreading;
        public List<hkaiSilhouetteGenerator> m_generators;
        public List<Matrix4x4> m_instanceTransforms;
        public List<hkaiOverlapManagerSection> m_overlapManagerSections;
        public hkBitField m_updatedSections;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_stepThreading = (StepThreading)br.ReadUInt32();
            br.ReadUInt32();
            m_generators = des.ReadClassPointerArray<hkaiSilhouetteGenerator>(br);
            m_instanceTransforms = des.ReadTransformArray(br);
            m_overlapManagerSections = des.ReadClassArray<hkaiOverlapManagerSection>(br);
            m_updatedSections = new hkBitField();
            m_updatedSections.Read(des, br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteUInt32((uint)m_stepThreading);
            bw.WriteUInt32(0);
            s.WriteClassPointerArray<hkaiSilhouetteGenerator>(bw, m_generators);
            s.WriteTransformArray(bw, m_instanceTransforms);
            s.WriteClassArray<hkaiOverlapManagerSection>(bw, m_overlapManagerSections);
            m_updatedSections.Write(s, bw);
        }
    }
}
