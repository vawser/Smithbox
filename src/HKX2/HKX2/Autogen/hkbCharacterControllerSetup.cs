using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbCharacterControllerSetup : IHavokObject
    {
        public virtual uint Signature { get => 2942268217; }
        
        public hkbRigidBodySetup m_rigidBodySetup;
        public hkReferencedObject m_controllerCinfo;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_rigidBodySetup = new hkbRigidBodySetup();
            m_rigidBodySetup.Read(des, br);
            m_controllerCinfo = des.ReadClassPointer<hkReferencedObject>(br);
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            m_rigidBodySetup.Write(s, bw);
            s.WriteClassPointer<hkReferencedObject>(bw, m_controllerCinfo);
        }
    }
}
