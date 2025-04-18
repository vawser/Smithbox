using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaiNavMeshSimplificationUtilsSettings : IHavokObject
    {
        public virtual uint Signature { get => 3680507198; }
        
        public float m_maxBorderSimplifyArea;
        public float m_maxConcaveBorderSimplifyArea;
        public float m_minCorridorWidth;
        public float m_maxCorridorWidth;
        public float m_holeReplacementArea;
        public float m_aabbReplacementAreaFraction;
        public float m_maxLoopShrinkFraction;
        public float m_maxBorderHeightError;
        public float m_maxBorderDistanceError;
        public int m_maxPartitionSize;
        public bool m_useHeightPartitioning;
        public float m_maxPartitionHeightError;
        public bool m_useConservativeHeightPartitioning;
        public float m_hertelMehlhornHeightError;
        public float m_cosPlanarityThreshold;
        public float m_nonconvexityThreshold;
        public float m_boundaryEdgeFilterThreshold;
        public float m_maxSharedVertexHorizontalError;
        public float m_maxSharedVertexVerticalError;
        public float m_maxBoundaryVertexHorizontalError;
        public float m_maxBoundaryVertexVerticalError;
        public bool m_mergeLongestEdgesFirst;
        public hkaiNavMeshSimplificationUtilsExtraVertexSettings m_extraVertexSettings;
        public bool m_saveInputSnapshot;
        public string m_snapshotFilename;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_maxBorderSimplifyArea = br.ReadSingle();
            m_maxConcaveBorderSimplifyArea = br.ReadSingle();
            m_minCorridorWidth = br.ReadSingle();
            m_maxCorridorWidth = br.ReadSingle();
            m_holeReplacementArea = br.ReadSingle();
            m_aabbReplacementAreaFraction = br.ReadSingle();
            m_maxLoopShrinkFraction = br.ReadSingle();
            m_maxBorderHeightError = br.ReadSingle();
            m_maxBorderDistanceError = br.ReadSingle();
            m_maxPartitionSize = br.ReadInt32();
            m_useHeightPartitioning = br.ReadBoolean();
            br.ReadUInt16();
            br.ReadByte();
            m_maxPartitionHeightError = br.ReadSingle();
            m_useConservativeHeightPartitioning = br.ReadBoolean();
            br.ReadUInt16();
            br.ReadByte();
            m_hertelMehlhornHeightError = br.ReadSingle();
            m_cosPlanarityThreshold = br.ReadSingle();
            m_nonconvexityThreshold = br.ReadSingle();
            m_boundaryEdgeFilterThreshold = br.ReadSingle();
            m_maxSharedVertexHorizontalError = br.ReadSingle();
            m_maxSharedVertexVerticalError = br.ReadSingle();
            m_maxBoundaryVertexHorizontalError = br.ReadSingle();
            m_maxBoundaryVertexVerticalError = br.ReadSingle();
            m_mergeLongestEdgesFirst = br.ReadBoolean();
            br.ReadUInt16();
            br.ReadByte();
            m_extraVertexSettings = new hkaiNavMeshSimplificationUtilsExtraVertexSettings();
            m_extraVertexSettings.Read(des, br);
            m_saveInputSnapshot = br.ReadBoolean();
            br.ReadUInt32();
            br.ReadUInt16();
            br.ReadByte();
            m_snapshotFilename = des.ReadStringPointer(br);
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteSingle(m_maxBorderSimplifyArea);
            bw.WriteSingle(m_maxConcaveBorderSimplifyArea);
            bw.WriteSingle(m_minCorridorWidth);
            bw.WriteSingle(m_maxCorridorWidth);
            bw.WriteSingle(m_holeReplacementArea);
            bw.WriteSingle(m_aabbReplacementAreaFraction);
            bw.WriteSingle(m_maxLoopShrinkFraction);
            bw.WriteSingle(m_maxBorderHeightError);
            bw.WriteSingle(m_maxBorderDistanceError);
            bw.WriteInt32(m_maxPartitionSize);
            bw.WriteBoolean(m_useHeightPartitioning);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
            bw.WriteSingle(m_maxPartitionHeightError);
            bw.WriteBoolean(m_useConservativeHeightPartitioning);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
            bw.WriteSingle(m_hertelMehlhornHeightError);
            bw.WriteSingle(m_cosPlanarityThreshold);
            bw.WriteSingle(m_nonconvexityThreshold);
            bw.WriteSingle(m_boundaryEdgeFilterThreshold);
            bw.WriteSingle(m_maxSharedVertexHorizontalError);
            bw.WriteSingle(m_maxSharedVertexVerticalError);
            bw.WriteSingle(m_maxBoundaryVertexHorizontalError);
            bw.WriteSingle(m_maxBoundaryVertexVerticalError);
            bw.WriteBoolean(m_mergeLongestEdgesFirst);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
            m_extraVertexSettings.Write(s, bw);
            bw.WriteBoolean(m_saveInputSnapshot);
            bw.WriteUInt32(0);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
            s.WriteStringPointer(bw, m_snapshotFilename);
        }
    }
}
