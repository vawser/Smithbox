using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum DebugValues
    {
        DEAD_FACE = -559023410,
        DEAD_EDGE = -559026834,
    }
    
    public enum CutInfoValues
    {
        NOT_CUT_EDGE = 65535,
    }
    
    public partial class hkaiNavMeshInstance : hkReferencedObject
    {
        public override uint Signature { get => 3427274754; }
        
        public hkaiNavMesh m_originalMesh;
        public hkaiReferenceFrame m_referenceFrame;
        public List<int> m_edgeMap;
        public List<int> m_faceMap;
        public List<hkaiNavMeshFace> m_instancedFaces;
        public List<hkaiNavMeshEdge> m_instancedEdges;
        public List<hkaiNavMeshFace> m_ownedFaces;
        public List<hkaiNavMeshEdge> m_ownedEdges;
        public List<Vector4> m_ownedVertices;
        public List<byte> m_faceFlags;
        public List<ushort> m_cuttingInfo;
        public List<int> m_instancedFaceData;
        public List<int> m_instancedEdgeData;
        public List<int> m_ownedFaceData;
        public List<int> m_ownedEdgeData;
        public List<short> m_clearanceCache;
        public List<short> m_globalClearanceCache;
        public List<int> m_faceClearanceIndices;
        public float m_maxGlobalClearance;
        public uint m_sectionUid;
        public int m_runtimeId;
        public uint m_layer;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            m_originalMesh = des.ReadClassPointer<hkaiNavMesh>(br);
            br.ReadUInt64();
            m_referenceFrame = new hkaiReferenceFrame();
            m_referenceFrame.Read(des, br);
            m_edgeMap = des.ReadInt32Array(br);
            m_faceMap = des.ReadInt32Array(br);
            m_instancedFaces = des.ReadClassArray<hkaiNavMeshFace>(br);
            m_instancedEdges = des.ReadClassArray<hkaiNavMeshEdge>(br);
            m_ownedFaces = des.ReadClassArray<hkaiNavMeshFace>(br);
            m_ownedEdges = des.ReadClassArray<hkaiNavMeshEdge>(br);
            m_ownedVertices = des.ReadVector4Array(br);
            m_faceFlags = des.ReadByteArray(br);
            m_cuttingInfo = des.ReadUInt16Array(br);
            m_instancedFaceData = des.ReadInt32Array(br);
            m_instancedEdgeData = des.ReadInt32Array(br);
            m_ownedFaceData = des.ReadInt32Array(br);
            m_ownedEdgeData = des.ReadInt32Array(br);
            m_clearanceCache = des.ReadInt16Array(br);
            m_globalClearanceCache = des.ReadInt16Array(br);
            m_faceClearanceIndices = des.ReadInt32Array(br);
            m_maxGlobalClearance = br.ReadSingle();
            m_sectionUid = br.ReadUInt32();
            m_runtimeId = br.ReadInt32();
            m_layer = br.ReadUInt32();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            s.WriteClassPointer<hkaiNavMesh>(bw, m_originalMesh);
            bw.WriteUInt64(0);
            m_referenceFrame.Write(s, bw);
            s.WriteInt32Array(bw, m_edgeMap);
            s.WriteInt32Array(bw, m_faceMap);
            s.WriteClassArray<hkaiNavMeshFace>(bw, m_instancedFaces);
            s.WriteClassArray<hkaiNavMeshEdge>(bw, m_instancedEdges);
            s.WriteClassArray<hkaiNavMeshFace>(bw, m_ownedFaces);
            s.WriteClassArray<hkaiNavMeshEdge>(bw, m_ownedEdges);
            s.WriteVector4Array(bw, m_ownedVertices);
            s.WriteByteArray(bw, m_faceFlags);
            s.WriteUInt16Array(bw, m_cuttingInfo);
            s.WriteInt32Array(bw, m_instancedFaceData);
            s.WriteInt32Array(bw, m_instancedEdgeData);
            s.WriteInt32Array(bw, m_ownedFaceData);
            s.WriteInt32Array(bw, m_ownedEdgeData);
            s.WriteInt16Array(bw, m_clearanceCache);
            s.WriteInt16Array(bw, m_globalClearanceCache);
            s.WriteInt32Array(bw, m_faceClearanceIndices);
            bw.WriteSingle(m_maxGlobalClearance);
            bw.WriteUInt32(m_sectionUid);
            bw.WriteInt32(m_runtimeId);
            bw.WriteUInt32(m_layer);
        }
    }
}
