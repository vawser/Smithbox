using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaRagdollInstance : hkReferencedObject
    {
        public override uint Signature { get => 1414067300; }
        
        public List<hkpRigidBody> m_rigidBodies;
        public List<hkpConstraintInstance> m_constraints;
        public List<int> m_boneToRigidBodyMap;
        public hkaSkeleton m_skeleton;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_rigidBodies = des.ReadClassPointerArray<hkpRigidBody>(br);
            m_constraints = des.ReadClassPointerArray<hkpConstraintInstance>(br);
            m_boneToRigidBodyMap = des.ReadInt32Array(br);
            m_skeleton = des.ReadClassPointer<hkaSkeleton>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassPointerArray<hkpRigidBody>(bw, m_rigidBodies);
            s.WriteClassPointerArray<hkpConstraintInstance>(bw, m_constraints);
            s.WriteInt32Array(bw, m_boneToRigidBodyMap);
            s.WriteClassPointer<hkaSkeleton>(bw, m_skeleton);
        }
    }
}
