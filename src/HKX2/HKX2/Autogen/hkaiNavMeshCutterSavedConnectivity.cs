using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaiNavMeshCutterSavedConnectivity : IHavokObject
    {
        public virtual uint Signature { get => 1340919839; }
        
        public hkSetUint32 m_storage;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_storage = new hkSetUint32();
            m_storage.Read(des, br);
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            m_storage.Write(s, bw);
        }
    }
}
