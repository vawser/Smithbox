using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hknpPhysicsSceneData : hkReferencedObject
    {
        public override uint Signature { get => 1880942380; }
        
        public List<hknpPhysicsSystemData> m_systemDatas;
        public hknpRefWorldCinfo m_worldCinfo;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_systemDatas = des.ReadClassPointerArray<hknpPhysicsSystemData>(br);
            m_worldCinfo = des.ReadClassPointer<hknpRefWorldCinfo>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassPointerArray<hknpPhysicsSystemData>(bw, m_systemDatas);
            s.WriteClassPointer<hknpRefWorldCinfo>(bw, m_worldCinfo);
        }
    }
}
