using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbCharacterStringDataFileNameMeshNamePair : IHavokObject
    {
        public virtual uint Signature { get => 40647318; }
        
        public string m_fileName;
        public string m_meshName;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_fileName = des.ReadStringPointer(br);
            m_meshName = des.ReadStringPointer(br);
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteStringPointer(bw, m_fileName);
            s.WriteStringPointer(bw, m_meshName);
        }
    }
}
