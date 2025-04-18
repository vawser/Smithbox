using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaiPathfindingUtilFindGraphPathInput : IHavokObject
    {
        public virtual uint Signature { get => 1733647340; }
        
        public List<uint> m_startNodeKeys;
        public List<float> m_initialCosts;
        public List<uint> m_goalNodeKeys;
        public List<float> m_finalCosts;
        public int m_maxNumberOfIterations;
        public hkaiAgentTraversalInfo m_agentInfo;
        public hkaiGraphPathSearchParameters m_searchParameters;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_startNodeKeys = des.ReadUInt32Array(br);
            m_initialCosts = des.ReadSingleArray(br);
            m_goalNodeKeys = des.ReadUInt32Array(br);
            m_finalCosts = des.ReadSingleArray(br);
            m_maxNumberOfIterations = br.ReadInt32();
            m_agentInfo = new hkaiAgentTraversalInfo();
            m_agentInfo.Read(des, br);
            br.ReadUInt32();
            m_searchParameters = new hkaiGraphPathSearchParameters();
            m_searchParameters.Read(des, br);
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteUInt32Array(bw, m_startNodeKeys);
            s.WriteSingleArray(bw, m_initialCosts);
            s.WriteUInt32Array(bw, m_goalNodeKeys);
            s.WriteSingleArray(bw, m_finalCosts);
            bw.WriteInt32(m_maxNumberOfIterations);
            m_agentInfo.Write(s, bw);
            bw.WriteUInt32(0);
            m_searchParameters.Write(s, bw);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
        }
    }
}
