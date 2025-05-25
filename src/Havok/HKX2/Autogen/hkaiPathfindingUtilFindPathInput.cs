using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaiPathfindingUtilFindPathInput : hkReferencedObject
    {
        public override uint Signature { get => 1998238741; }
        
        public Vector4 m_startPoint;
        public List<Vector4> m_goalPoints;
        public uint m_startFaceKey;
        public List<uint> m_goalFaceKeys;
        public int m_maxNumberOfIterations;
        public hkaiAgentTraversalInfo m_agentInfo;
        public hkaiNavMeshPathSearchParameters m_searchParameters;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_startPoint = des.ReadVector4(br);
            m_goalPoints = des.ReadVector4Array(br);
            m_startFaceKey = br.ReadUInt32();
            br.ReadUInt32();
            m_goalFaceKeys = des.ReadUInt32Array(br);
            m_maxNumberOfIterations = br.ReadInt32();
            m_agentInfo = new hkaiAgentTraversalInfo();
            m_agentInfo.Read(des, br);
            br.ReadUInt64();
            br.ReadUInt32();
            m_searchParameters = new hkaiNavMeshPathSearchParameters();
            m_searchParameters.Read(des, br);
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteVector4(bw, m_startPoint);
            s.WriteVector4Array(bw, m_goalPoints);
            bw.WriteUInt32(m_startFaceKey);
            bw.WriteUInt32(0);
            s.WriteUInt32Array(bw, m_goalFaceKeys);
            bw.WriteInt32(m_maxNumberOfIterations);
            m_agentInfo.Write(s, bw);
            bw.WriteUInt64(0);
            bw.WriteUInt32(0);
            m_searchParameters.Write(s, bw);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
        }
    }
}
