using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaiVolumePathfindingUtilFindPathOutput : hkReferencedObject
    {
        public override uint Signature { get => 1555536487; }
        
        public List<uint> m_visitedCells;
        public List<hkaiPathPathPoint> m_pathOut;
        public hkaiAstarOutputParameters m_outputParameters;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_visitedCells = des.ReadUInt32Array(br);
            m_pathOut = des.ReadClassArray<hkaiPathPathPoint>(br);
            m_outputParameters = new hkaiAstarOutputParameters();
            m_outputParameters.Read(des, br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteUInt32Array(bw, m_visitedCells);
            s.WriteClassArray<hkaiPathPathPoint>(bw, m_pathOut);
            m_outputParameters.Write(s, bw);
        }
    }
}
