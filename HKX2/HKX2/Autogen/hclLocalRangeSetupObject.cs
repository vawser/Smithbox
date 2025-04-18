using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclLocalRangeSetupObject : hclConstraintSetSetupObject
    {
        public override uint Signature { get => 2945521065; }
        
        public string m_name;
        public hclSimulationSetupMesh m_simulationMesh;
        public hclBufferSetupObject m_referenceBufferSetup;
        public hclVertexSelectionInput m_vertexSelection;
        public hclVertexFloatInput m_maximumDistance;
        public hclVertexFloatInput m_minNormalDistance;
        public hclVertexFloatInput m_maxNormalDistance;
        public float m_stiffness;
        public ShapeType m_localRangeShape;
        public bool m_useMinNormalDistance;
        public bool m_useMaxNormalDistance;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_name = des.ReadStringPointer(br);
            m_simulationMesh = des.ReadClassPointer<hclSimulationSetupMesh>(br);
            m_referenceBufferSetup = des.ReadClassPointer<hclBufferSetupObject>(br);
            m_vertexSelection = new hclVertexSelectionInput();
            m_vertexSelection.Read(des, br);
            m_maximumDistance = new hclVertexFloatInput();
            m_maximumDistance.Read(des, br);
            m_minNormalDistance = new hclVertexFloatInput();
            m_minNormalDistance.Read(des, br);
            m_maxNormalDistance = new hclVertexFloatInput();
            m_maxNormalDistance.Read(des, br);
            m_stiffness = br.ReadSingle();
            m_localRangeShape = (ShapeType)br.ReadUInt32();
            m_useMinNormalDistance = br.ReadBoolean();
            m_useMaxNormalDistance = br.ReadBoolean();
            br.ReadUInt32();
            br.ReadUInt16();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteStringPointer(bw, m_name);
            s.WriteClassPointer<hclSimulationSetupMesh>(bw, m_simulationMesh);
            s.WriteClassPointer<hclBufferSetupObject>(bw, m_referenceBufferSetup);
            m_vertexSelection.Write(s, bw);
            m_maximumDistance.Write(s, bw);
            m_minNormalDistance.Write(s, bw);
            m_maxNormalDistance.Write(s, bw);
            bw.WriteSingle(m_stiffness);
            bw.WriteUInt32((uint)m_localRangeShape);
            bw.WriteBoolean(m_useMinNormalDistance);
            bw.WriteBoolean(m_useMaxNormalDistance);
            bw.WriteUInt32(0);
            bw.WriteUInt16(0);
        }
    }
}
