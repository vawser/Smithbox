using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbCompiledExpressionSet : hkReferencedObject
    {
        public override uint Signature { get => 2118620138; }
        
        public List<hkbCompiledExpressionSetToken> m_rpn;
        public List<int> m_expressionToRpnIndex;
        public sbyte m_numExpressions;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_rpn = des.ReadClassArray<hkbCompiledExpressionSetToken>(br);
            m_expressionToRpnIndex = des.ReadInt32Array(br);
            m_numExpressions = br.ReadSByte();
            br.ReadUInt32();
            br.ReadUInt16();
            br.ReadByte();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassArray<hkbCompiledExpressionSetToken>(bw, m_rpn);
            s.WriteInt32Array(bw, m_expressionToRpnIndex);
            bw.WriteSByte(m_numExpressions);
            bw.WriteUInt32(0);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
        }
    }
}
