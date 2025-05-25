using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum CloneConstraintMode
    {
        CLONE_SHALLOW_IF_NOT_CONSTRAINED_TO_WORLD = 0,
        CLONE_DEEP_WITH_MOTORS = 1,
        CLONE_FORCE_SHALLOW = 2,
        CLONE_DEFAULT = 0,
    }
    
    public partial class hkpPhysicsSystem : hkReferencedObject
    {
        public override uint Signature { get => 3016519268; }
        
        public List<hkpRigidBody> m_rigidBodies;
        public List<hkpConstraintInstance> m_constraints;
        public List<hkpAction> m_actions;
        public List<hkpPhantom> m_phantoms;
        public string m_name;
        public ulong m_userData;
        public bool m_active;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_rigidBodies = des.ReadClassPointerArray<hkpRigidBody>(br);
            m_constraints = des.ReadClassPointerArray<hkpConstraintInstance>(br);
            m_actions = des.ReadClassPointerArray<hkpAction>(br);
            m_phantoms = des.ReadClassPointerArray<hkpPhantom>(br);
            m_name = des.ReadStringPointer(br);
            m_userData = br.ReadUInt64();
            m_active = br.ReadBoolean();
            br.ReadUInt32();
            br.ReadUInt16();
            br.ReadByte();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassPointerArray<hkpRigidBody>(bw, m_rigidBodies);
            s.WriteClassPointerArray<hkpConstraintInstance>(bw, m_constraints);
            s.WriteClassPointerArray<hkpAction>(bw, m_actions);
            s.WriteClassPointerArray<hkpPhantom>(bw, m_phantoms);
            s.WriteStringPointer(bw, m_name);
            bw.WriteUInt64(m_userData);
            bw.WriteBoolean(m_active);
            bw.WriteUInt32(0);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
        }
    }
}
