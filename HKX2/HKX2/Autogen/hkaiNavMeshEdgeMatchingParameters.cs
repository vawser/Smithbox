using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaiNavMeshEdgeMatchingParameters : IHavokObject
    {
        public virtual uint Signature { get => 3800051401; }
        
        public float m_maxStepHeight;
        public float m_maxSeparation;
        public float m_maxOverhang;
        public float m_behindFaceTolerance;
        public float m_cosPlanarAlignmentAngle;
        public float m_cosVerticalAlignmentAngle;
        public float m_minEdgeOverlap;
        public float m_edgeTraversibilityHorizontalEpsilon;
        public float m_edgeTraversibilityVerticalEpsilon;
        public float m_cosClimbingFaceNormalAlignmentAngle;
        public float m_cosClimbingEdgeAlignmentAngle;
        public float m_minAngleBetweenFaces;
        public float m_edgeParallelTolerance;
        public bool m_useSafeEdgeTraversibilityHorizontalEpsilon;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_maxStepHeight = br.ReadSingle();
            m_maxSeparation = br.ReadSingle();
            m_maxOverhang = br.ReadSingle();
            m_behindFaceTolerance = br.ReadSingle();
            m_cosPlanarAlignmentAngle = br.ReadSingle();
            m_cosVerticalAlignmentAngle = br.ReadSingle();
            m_minEdgeOverlap = br.ReadSingle();
            m_edgeTraversibilityHorizontalEpsilon = br.ReadSingle();
            m_edgeTraversibilityVerticalEpsilon = br.ReadSingle();
            m_cosClimbingFaceNormalAlignmentAngle = br.ReadSingle();
            m_cosClimbingEdgeAlignmentAngle = br.ReadSingle();
            m_minAngleBetweenFaces = br.ReadSingle();
            m_edgeParallelTolerance = br.ReadSingle();
            m_useSafeEdgeTraversibilityHorizontalEpsilon = br.ReadBoolean();
            br.ReadUInt16();
            br.ReadByte();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteSingle(m_maxStepHeight);
            bw.WriteSingle(m_maxSeparation);
            bw.WriteSingle(m_maxOverhang);
            bw.WriteSingle(m_behindFaceTolerance);
            bw.WriteSingle(m_cosPlanarAlignmentAngle);
            bw.WriteSingle(m_cosVerticalAlignmentAngle);
            bw.WriteSingle(m_minEdgeOverlap);
            bw.WriteSingle(m_edgeTraversibilityHorizontalEpsilon);
            bw.WriteSingle(m_edgeTraversibilityVerticalEpsilon);
            bw.WriteSingle(m_cosClimbingFaceNormalAlignmentAngle);
            bw.WriteSingle(m_cosClimbingEdgeAlignmentAngle);
            bw.WriteSingle(m_minAngleBetweenFaces);
            bw.WriteSingle(m_edgeParallelTolerance);
            bw.WriteBoolean(m_useSafeEdgeTraversibilityHorizontalEpsilon);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
        }
    }
}
