using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclClothStateTransformSetAccess : IHavokObject
    {
        public virtual uint Signature { get => 45534125; }
        
        public uint m_transformSetIndex;
        public hclTransformSetUsage m_transformSetUsage;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_transformSetIndex = br.ReadUInt32();
            br.ReadUInt32();
            m_transformSetUsage = new hclTransformSetUsage();
            m_transformSetUsage.Read(des, br);
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteUInt32(m_transformSetIndex);
            bw.WriteUInt32(0);
            m_transformSetUsage.Write(s, bw);
        }
    }
}
