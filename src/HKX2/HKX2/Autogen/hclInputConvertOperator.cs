using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclInputConvertOperator : hclOperator
    {
        public override uint Signature { get => 3981491103; }
        
        public uint m_userBufferIndex;
        public uint m_shadowBufferIndex;
        public hclRuntimeConversionInfo m_conversionInfo;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_userBufferIndex = br.ReadUInt32();
            m_shadowBufferIndex = br.ReadUInt32();
            m_conversionInfo = new hclRuntimeConversionInfo();
            m_conversionInfo.Read(des, br);
            br.ReadUInt32();
            br.ReadUInt16();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteUInt32(m_userBufferIndex);
            bw.WriteUInt32(m_shadowBufferIndex);
            m_conversionInfo.Write(s, bw);
            bw.WriteUInt32(0);
            bw.WriteUInt16(0);
        }
    }
}
