using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbManualSelectorGenerator : hkbGenerator
    {
        public override uint Signature { get => 1494910070; }
        
        public List<hkbGenerator> m_generators;
        public short m_selectedGeneratorIndex;
        public hkbCustomIdSelector m_indexSelector;
        public bool m_selectedIndexCanChangeAfterActivate;
        public hkbTransitionEffect m_generatorChangedTransitionEffect;
        public int m_endOfClipEventId;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_generators = des.ReadClassPointerArray<hkbGenerator>(br);
            m_selectedGeneratorIndex = br.ReadInt16();
            br.ReadUInt32();
            br.ReadUInt16();
            m_indexSelector = des.ReadClassPointer<hkbCustomIdSelector>(br);
            m_selectedIndexCanChangeAfterActivate = br.ReadBoolean();
            br.ReadUInt32();
            br.ReadUInt16();
            br.ReadByte();
            m_generatorChangedTransitionEffect = des.ReadClassPointer<hkbTransitionEffect>(br);
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            m_endOfClipEventId = br.ReadInt32();
            br.ReadUInt32();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassPointerArray<hkbGenerator>(bw, m_generators);
            bw.WriteInt16(m_selectedGeneratorIndex);
            bw.WriteUInt32(0);
            bw.WriteUInt16(0);
            s.WriteClassPointer<hkbCustomIdSelector>(bw, m_indexSelector);
            bw.WriteBoolean(m_selectedIndexCanChangeAfterActivate);
            bw.WriteUInt32(0);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
            s.WriteClassPointer<hkbTransitionEffect>(bw, m_generatorChangedTransitionEffect);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteInt32(m_endOfClipEventId);
            bw.WriteUInt32(0);
        }
    }
}
