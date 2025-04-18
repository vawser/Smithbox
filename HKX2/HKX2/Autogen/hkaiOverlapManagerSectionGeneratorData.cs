using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaiOverlapManagerSectionGeneratorData : hkReferencedObject
    {
        public override uint Signature { get => 1740166900; }
        
        public hkaiSilhouetteGeneratorSectionContext m_context;
        public List<int> m_overlappedFaces;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_context = new hkaiSilhouetteGeneratorSectionContext();
            m_context.Read(des, br);
            m_overlappedFaces = des.ReadInt32Array(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            m_context.Write(s, bw);
            s.WriteInt32Array(bw, m_overlappedFaces);
        }
    }
}
