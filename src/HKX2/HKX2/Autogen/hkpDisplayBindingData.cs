using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpDisplayBindingData : hkReferencedObject
    {
        public override uint Signature { get => 3482735242; }
        
        public List<hkpDisplayBindingDataRigidBody> m_rigidBodyBindings;
        public List<hkpDisplayBindingDataPhysicsSystem> m_physicsSystemBindings;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_rigidBodyBindings = des.ReadClassPointerArray<hkpDisplayBindingDataRigidBody>(br);
            m_physicsSystemBindings = des.ReadClassPointerArray<hkpDisplayBindingDataPhysicsSystem>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassPointerArray<hkpDisplayBindingDataRigidBody>(bw, m_rigidBodyBindings);
            s.WriteClassPointerArray<hkpDisplayBindingDataPhysicsSystem>(bw, m_physicsSystemBindings);
        }
    }
}
