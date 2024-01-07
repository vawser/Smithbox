using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclSimulateOperator : hclOperator
    {
        public override uint Signature { get => 1975987983; }
        
        public uint m_simClothIndex;
        public uint m_subSteps;
        public int m_numberOfSolveIterations;
        public List<int> m_constraintExecution;
        public bool m_adaptConstraintStiffness;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_simClothIndex = br.ReadUInt32();
            m_subSteps = br.ReadUInt32();
            m_numberOfSolveIterations = br.ReadInt32();
            br.ReadUInt32();
            m_constraintExecution = des.ReadInt32Array(br);
            m_adaptConstraintStiffness = br.ReadBoolean();
            br.ReadUInt32();
            br.ReadUInt16();
            br.ReadByte();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteUInt32(m_simClothIndex);
            bw.WriteUInt32(m_subSteps);
            bw.WriteInt32(m_numberOfSolveIterations);
            bw.WriteUInt32(0);
            s.WriteInt32Array(bw, m_constraintExecution);
            bw.WriteBoolean(m_adaptConstraintStiffness);
            bw.WriteUInt32(0);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
        }
    }
}
