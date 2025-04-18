using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hknpMaterialDescriptor : IHavokObject
    {
        public virtual uint Signature { get => 2275334943; }
        
        public string m_name;
        public hknpRefMaterial m_material;
        public ushort m_materialId;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_name = des.ReadStringPointer(br);
            m_material = des.ReadClassPointer<hknpRefMaterial>(br);
            m_materialId = br.ReadUInt16();
            br.ReadUInt32();
            br.ReadUInt16();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteStringPointer(bw, m_name);
            s.WriteClassPointer<hknpRefMaterial>(bw, m_material);
            bw.WriteUInt16(m_materialId);
            bw.WriteUInt32(0);
            bw.WriteUInt16(0);
        }
    }
}
