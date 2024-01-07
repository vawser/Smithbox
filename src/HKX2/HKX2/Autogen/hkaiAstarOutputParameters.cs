using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum SearchStatus
    {
        SEARCH_IN_PROGRESS = 0,
        SEARCH_SUCCEEDED = 1,
        SEARCH_UNREACHABLE = 2,
        SEARCH_TERMINATED = 3,
        SEARCH_SUCCEEDED_BUT_RESULTS_TRUNCATED = 4,
        SEARCH_INVALID = 5,
    }
    
    public enum TerminationCause
    {
        NOT_TERMINATED = 0,
        TERMINATED_ITERATION_LIMIT = 1,
        TERMINATED_OPEN_SET_FULL = 2,
        TERMINATED_SEARCH_STATE_FULL = 3,
        TERMINATED_BY_USER = 4,
    }
    
    public partial class hkaiAstarOutputParameters : IHavokObject
    {
        public virtual uint Signature { get => 370388487; }
        
        public int m_numIterations;
        public int m_goalIndex;
        public float m_pathLength;
        public SearchStatus m_status;
        public TerminationCause m_terminationCause;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_numIterations = br.ReadInt32();
            m_goalIndex = br.ReadInt32();
            m_pathLength = br.ReadSingle();
            m_status = (SearchStatus)br.ReadByte();
            m_terminationCause = (TerminationCause)br.ReadByte();
            br.ReadUInt16();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteInt32(m_numIterations);
            bw.WriteInt32(m_goalIndex);
            bw.WriteSingle(m_pathLength);
            bw.WriteByte((byte)m_status);
            bw.WriteByte((byte)m_terminationCause);
            bw.WriteUInt16(0);
        }
    }
}
