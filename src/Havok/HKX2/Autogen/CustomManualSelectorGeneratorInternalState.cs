using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class CustomManualSelectorGeneratorInternalState : hkReferencedObject
    {
        public override uint Signature { get => 1874353492; }
        
        public sbyte m_currentGeneratorIndex;
        public sbyte m_generatorIndexAtActivate;
        public List<hkbStateMachineActiveTransitionInfo> m_activeTransitions;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_currentGeneratorIndex = br.ReadSByte();
            m_generatorIndexAtActivate = br.ReadSByte();
            br.ReadUInt32();
            br.ReadUInt16();
            m_activeTransitions = des.ReadClassArray<hkbStateMachineActiveTransitionInfo>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteSByte(m_currentGeneratorIndex);
            bw.WriteSByte(m_generatorIndexAtActivate);
            bw.WriteUInt32(0);
            bw.WriteUInt16(0);
            s.WriteClassArray<hkbStateMachineActiveTransitionInfo>(bw, m_activeTransitions);
        }
    }
}
