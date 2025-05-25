using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaiGraphPathSearchParameters : IHavokObject
    {
        public virtual uint Signature { get => 2474849502; }
        
        public float m_heuristicWeight;
        public bool m_useHierarchicalHeuristic;
        public hkaiSearchParametersBufferSizes m_bufferSizes;
        public hkaiSearchParametersBufferSizes m_hierarchyBufferSizes;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_heuristicWeight = br.ReadSingle();
            m_useHierarchicalHeuristic = br.ReadBoolean();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt16();
            br.ReadByte();
            m_bufferSizes = new hkaiSearchParametersBufferSizes();
            m_bufferSizes.Read(des, br);
            m_hierarchyBufferSizes = new hkaiSearchParametersBufferSizes();
            m_hierarchyBufferSizes.Read(des, br);
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteSingle(m_heuristicWeight);
            bw.WriteBoolean(m_useHierarchicalHeuristic);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
            m_bufferSizes.Write(s, bw);
            m_hierarchyBufferSizes.Write(s, bw);
        }
    }
}
