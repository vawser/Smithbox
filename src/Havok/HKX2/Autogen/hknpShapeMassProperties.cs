using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hknpShapeMassProperties : hkReferencedObject
    {
        public override uint Signature { get => 3910735656; }
        
        public hkCompressedMassProperties m_compressedMassProperties;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_compressedMassProperties = new hkCompressedMassProperties();
            m_compressedMassProperties.Read(des, br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            m_compressedMassProperties.Write(s, bw);
        }
    }
}
