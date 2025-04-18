using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpDisplayBindingDataPhysicsSystem : hkReferencedObject
    {
        public override uint Signature { get => 383329158; }
        
        public List<hkpDisplayBindingDataRigidBody> m_bindings;
        public hkpPhysicsSystem m_system;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_bindings = des.ReadClassPointerArray<hkpDisplayBindingDataRigidBody>(br);
            m_system = des.ReadClassPointer<hkpPhysicsSystem>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassPointerArray<hkpDisplayBindingDataRigidBody>(bw, m_bindings);
            s.WriteClassPointer<hkpPhysicsSystem>(bw, m_system);
        }
    }
}
