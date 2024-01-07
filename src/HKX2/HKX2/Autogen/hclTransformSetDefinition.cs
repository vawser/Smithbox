using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclTransformSetDefinition : hkReferencedObject
    {
        public override uint Signature { get => 419251557; }
        
        public string m_name;
        public int m_type;
        public uint m_numTransforms;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_name = des.ReadStringPointer(br);
            m_type = br.ReadInt32();
            m_numTransforms = br.ReadUInt32();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteStringPointer(bw, m_name);
            bw.WriteInt32(m_type);
            bw.WriteUInt32(m_numTransforms);
        }
    }
}
