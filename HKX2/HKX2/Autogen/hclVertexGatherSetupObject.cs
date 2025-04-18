using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum Direction
    {
        SIMULATION_TO_DISPLAY = 0,
        DISPLAY_TO_SIMULATION = 1,
    }
    
    public partial class hclVertexGatherSetupObject : hclOperatorSetupObject
    {
        public override uint Signature { get => 3110859757; }
        
        public string m_name;
        public Direction m_direction;
        public hclSimClothBufferSetupObject m_simulationBuffer;
        public hclVertexSelectionInput m_simulationParticleSelection;
        public hclBufferSetupObject m_displayBuffer;
        public hclVertexSelectionInput m_displayVertexSelection;
        public float m_gatherAllThreshold;
        public bool m_gatherNormals;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_name = des.ReadStringPointer(br);
            m_direction = (Direction)br.ReadUInt32();
            br.ReadUInt32();
            m_simulationBuffer = des.ReadClassPointer<hclSimClothBufferSetupObject>(br);
            m_simulationParticleSelection = new hclVertexSelectionInput();
            m_simulationParticleSelection.Read(des, br);
            m_displayBuffer = des.ReadClassPointer<hclBufferSetupObject>(br);
            m_displayVertexSelection = new hclVertexSelectionInput();
            m_displayVertexSelection.Read(des, br);
            m_gatherAllThreshold = br.ReadSingle();
            m_gatherNormals = br.ReadBoolean();
            br.ReadUInt16();
            br.ReadByte();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteStringPointer(bw, m_name);
            bw.WriteUInt32((uint)m_direction);
            bw.WriteUInt32(0);
            s.WriteClassPointer<hclSimClothBufferSetupObject>(bw, m_simulationBuffer);
            m_simulationParticleSelection.Write(s, bw);
            s.WriteClassPointer<hclBufferSetupObject>(bw, m_displayBuffer);
            m_displayVertexSelection.Write(s, bw);
            bw.WriteSingle(m_gatherAllThreshold);
            bw.WriteBoolean(m_gatherNormals);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
        }
    }
}
