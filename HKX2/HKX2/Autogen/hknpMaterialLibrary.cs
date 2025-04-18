using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hknpMaterialLibrary : hkReferencedObject
    {
        public override uint Signature { get => 1544027498; }
        
        public hkFreeListArrayhknpMaterialhknpMaterialId8hknpMaterialFreeListArrayOperations m_entries;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            m_entries = new hkFreeListArrayhknpMaterialhknpMaterialId8hknpMaterialFreeListArrayOperations();
            m_entries.Read(des, br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            m_entries.Write(s, bw);
        }
    }
}
