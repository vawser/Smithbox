using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum ClearanceResetMethod
    {
        CLEARANCE_RESET_ALL = 0,
        CLEARANCE_RESET_MEDIATOR = 1,
        CLEARANCE_RESET_FLOODFILL = 2,
    }
    
    public enum GatherCutEdgesMode
    {
        GATHER_ALL_EDGES = 0,
        GATHER_BOUNDARY_EDGES = 1,
    }
    
    public partial class hkaiNavMeshCutter : hkReferencedObject
    {
        public override uint Signature { get => 2420968626; }
        
        public List<hkaiNavMeshCutterMeshInfo> m_meshInfos;
        public hkaiNavMeshCutterSavedConnectivity m_connectivityInfo;
        public hkaiStreamingCollection m_streamingCollection;
        public List<uint> m_forceRecutFaceKeys;
        public List<uint> m_forceClearanceCalcFaceKeys;
        public Vector4 m_up;
        public hkaiNavMeshEdgeMatchingParameters m_edgeMatchParams;
        public float m_cutEdgeTolerance;
        public float m_minEdgeMatchingLength;
        public float m_smallGapFixupTolerance;
        public bool m_performValidationChecks;
        public ClearanceResetMethod m_clearanceResetMethod;
        public bool m_recomputeClearanceAfterCutting;
        public bool m_useNewCutter;
        public float m_domainQuantum;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_meshInfos = des.ReadClassArray<hkaiNavMeshCutterMeshInfo>(br);
            m_connectivityInfo = new hkaiNavMeshCutterSavedConnectivity();
            m_connectivityInfo.Read(des, br);
            m_streamingCollection = des.ReadClassPointer<hkaiStreamingCollection>(br);
            m_forceRecutFaceKeys = des.ReadUInt32Array(br);
            m_forceClearanceCalcFaceKeys = des.ReadUInt32Array(br);
            m_up = des.ReadVector4(br);
            m_edgeMatchParams = new hkaiNavMeshEdgeMatchingParameters();
            m_edgeMatchParams.Read(des, br);
            br.ReadUInt64();
            m_cutEdgeTolerance = br.ReadSingle();
            m_minEdgeMatchingLength = br.ReadSingle();
            m_smallGapFixupTolerance = br.ReadSingle();
            m_performValidationChecks = br.ReadBoolean();
            m_clearanceResetMethod = (ClearanceResetMethod)br.ReadByte();
            m_recomputeClearanceAfterCutting = br.ReadBoolean();
            m_useNewCutter = br.ReadBoolean();
            m_domainQuantum = br.ReadSingle();
            br.ReadUInt64();
            br.ReadUInt32();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassArray<hkaiNavMeshCutterMeshInfo>(bw, m_meshInfos);
            m_connectivityInfo.Write(s, bw);
            s.WriteClassPointer<hkaiStreamingCollection>(bw, m_streamingCollection);
            s.WriteUInt32Array(bw, m_forceRecutFaceKeys);
            s.WriteUInt32Array(bw, m_forceClearanceCalcFaceKeys);
            s.WriteVector4(bw, m_up);
            m_edgeMatchParams.Write(s, bw);
            bw.WriteUInt64(0);
            bw.WriteSingle(m_cutEdgeTolerance);
            bw.WriteSingle(m_minEdgeMatchingLength);
            bw.WriteSingle(m_smallGapFixupTolerance);
            bw.WriteBoolean(m_performValidationChecks);
            bw.WriteByte((byte)m_clearanceResetMethod);
            bw.WriteBoolean(m_recomputeClearanceAfterCutting);
            bw.WriteBoolean(m_useNewCutter);
            bw.WriteSingle(m_domainQuantum);
            bw.WriteUInt64(0);
            bw.WriteUInt32(0);
        }
    }
}
