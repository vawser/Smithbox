using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbMirroredSkeletonInfo : hkReferencedObject
    {
        public override uint Signature { get => 2668823854; }
        
        public Vector4 m_mirrorAxis;
        public List<short> m_bonePairMap;
        public List<short> m_partitionPairMap;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_mirrorAxis = des.ReadVector4(br);
            m_bonePairMap = des.ReadInt16Array(br);
            m_partitionPairMap = des.ReadInt16Array(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteVector4(bw, m_mirrorAxis);
            s.WriteInt16Array(bw, m_bonePairMap);
            s.WriteInt16Array(bw, m_partitionPairMap);
        }
    }
}
