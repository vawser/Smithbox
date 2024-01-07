using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpDisplayBindingDataRigidBody : hkReferencedObject
    {
        public override uint Signature { get => 2128346411; }
        
        public hkpRigidBody m_rigidBody;
        public hkReferencedObject m_displayObjectPtr;
        public Matrix4x4 m_rigidBodyFromDisplayObjectTransform;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_rigidBody = des.ReadClassPointer<hkpRigidBody>(br);
            m_displayObjectPtr = des.ReadClassPointer<hkReferencedObject>(br);
            m_rigidBodyFromDisplayObjectTransform = des.ReadMatrix4(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassPointer<hkpRigidBody>(bw, m_rigidBody);
            s.WriteClassPointer<hkReferencedObject>(bw, m_displayObjectPtr);
            s.WriteMatrix4(bw, m_rigidBodyFromDisplayObjectTransform);
        }
    }
}
