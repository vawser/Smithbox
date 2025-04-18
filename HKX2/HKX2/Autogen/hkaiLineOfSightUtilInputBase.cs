using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum QueryMode
    {
        MODE_LINE_OF_SIGHT = 0,
        MODE_DIRECT_PATH = 1,
    }
    
    public partial class hkaiLineOfSightUtilInputBase : IHavokObject
    {
        public virtual uint Signature { get => 966407788; }
        
        public Vector4 m_startPoint;
        public Vector4 m_up;
        public uint m_startFaceKey;
        public int m_maxNumberOfIterations;
        public hkaiAgentTraversalInfo m_agentInfo;
        public float m_searchRadius;
        public float m_maximumPathLength;
        public hkaiAstarCostModifier m_costModifier;
        public hkaiAstarEdgeFilter m_edgeFilter;
        public bool m_outputEdgesOnFailure;
        public bool m_projectedRadiusCheck;
        public bool m_exactInternalVertexHandling;
        public bool m_isWallClimbing;
        public QueryMode m_mode;
        public byte m_userEdgeHandling;
        public bool m_ignoreBackfacingEdges;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_startPoint = des.ReadVector4(br);
            m_up = des.ReadVector4(br);
            m_startFaceKey = br.ReadUInt32();
            m_maxNumberOfIterations = br.ReadInt32();
            m_agentInfo = new hkaiAgentTraversalInfo();
            m_agentInfo.Read(des, br);
            m_searchRadius = br.ReadSingle();
            m_maximumPathLength = br.ReadSingle();
            m_costModifier = des.ReadClassPointer<hkaiAstarCostModifier>(br);
            m_edgeFilter = des.ReadClassPointer<hkaiAstarEdgeFilter>(br);
            m_outputEdgesOnFailure = br.ReadBoolean();
            m_projectedRadiusCheck = br.ReadBoolean();
            m_exactInternalVertexHandling = br.ReadBoolean();
            m_isWallClimbing = br.ReadBoolean();
            m_mode = (QueryMode)br.ReadByte();
            m_userEdgeHandling = br.ReadByte();
            m_ignoreBackfacingEdges = br.ReadBoolean();
            br.ReadByte();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteVector4(bw, m_startPoint);
            s.WriteVector4(bw, m_up);
            bw.WriteUInt32(m_startFaceKey);
            bw.WriteInt32(m_maxNumberOfIterations);
            m_agentInfo.Write(s, bw);
            bw.WriteSingle(m_searchRadius);
            bw.WriteSingle(m_maximumPathLength);
            s.WriteClassPointer<hkaiAstarCostModifier>(bw, m_costModifier);
            s.WriteClassPointer<hkaiAstarEdgeFilter>(bw, m_edgeFilter);
            bw.WriteBoolean(m_outputEdgesOnFailure);
            bw.WriteBoolean(m_projectedRadiusCheck);
            bw.WriteBoolean(m_exactInternalVertexHandling);
            bw.WriteBoolean(m_isWallClimbing);
            bw.WriteByte((byte)m_mode);
            bw.WriteByte(m_userEdgeHandling);
            bw.WriteBoolean(m_ignoreBackfacingEdges);
            bw.WriteByte(0);
        }
    }
}
