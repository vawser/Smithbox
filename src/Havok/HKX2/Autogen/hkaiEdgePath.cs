using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum PpivResult
    {
        PPIV_RESULT_SUCCESS = 0,
        PPIV_RESULT_HIT_PATH_END = 1,
        PPIV_RESULT_INVALID_PATH = 2,
    }
    
    public partial class hkaiEdgePath : hkReferencedObject
    {
        public override uint Signature { get => 1514736363; }
        
        public List<hkaiEdgePathEdge> m_edges;
        public List<int> m_edgeData;
        public int m_edgeDataStriding;
        public float m_leftTurnRadius;
        public float m_rightTurnRadius;
        public float m_characterRadius;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_edges = des.ReadClassArray<hkaiEdgePathEdge>(br);
            m_edgeData = des.ReadInt32Array(br);
            m_edgeDataStriding = br.ReadInt32();
            m_leftTurnRadius = br.ReadSingle();
            m_rightTurnRadius = br.ReadSingle();
            m_characterRadius = br.ReadSingle();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassArray<hkaiEdgePathEdge>(bw, m_edges);
            s.WriteInt32Array(bw, m_edgeData);
            bw.WriteInt32(m_edgeDataStriding);
            bw.WriteSingle(m_leftTurnRadius);
            bw.WriteSingle(m_rightTurnRadius);
            bw.WriteSingle(m_characterRadius);
        }
    }
}
