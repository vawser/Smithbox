using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpLinearClearanceConstraintDataAtoms : IHavokObject
    {
        public virtual uint Signature { get => 3497299283; }
        
        public enum Axis
        {
            AXIS_SHAFT = 0,
            AXIS_PERP_TO_SHAFT = 1,
        }
        
        public hkpSetLocalTransformsConstraintAtom m_transforms;
        public hkpLinMotorConstraintAtom m_motor;
        public hkpLinFrictionConstraintAtom m_friction0;
        public hkpLinFrictionConstraintAtom m_friction1;
        public hkpLinFrictionConstraintAtom m_friction2;
        public hkpAngConstraintAtom m_ang;
        public hkpLinLimitConstraintAtom m_linLimit0;
        public hkpLinLimitConstraintAtom m_linLimit1;
        public hkpLinLimitConstraintAtom m_linLimit2;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_transforms = new hkpSetLocalTransformsConstraintAtom();
            m_transforms.Read(des, br);
            m_motor = new hkpLinMotorConstraintAtom();
            m_motor.Read(des, br);
            m_friction0 = new hkpLinFrictionConstraintAtom();
            m_friction0.Read(des, br);
            m_friction1 = new hkpLinFrictionConstraintAtom();
            m_friction1.Read(des, br);
            m_friction2 = new hkpLinFrictionConstraintAtom();
            m_friction2.Read(des, br);
            m_ang = new hkpAngConstraintAtom();
            m_ang.Read(des, br);
            m_linLimit0 = new hkpLinLimitConstraintAtom();
            m_linLimit0.Read(des, br);
            m_linLimit1 = new hkpLinLimitConstraintAtom();
            m_linLimit1.Read(des, br);
            m_linLimit2 = new hkpLinLimitConstraintAtom();
            m_linLimit2.Read(des, br);
            br.ReadUInt64();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            m_transforms.Write(s, bw);
            m_motor.Write(s, bw);
            m_friction0.Write(s, bw);
            m_friction1.Write(s, bw);
            m_friction2.Write(s, bw);
            m_ang.Write(s, bw);
            m_linLimit0.Write(s, bw);
            m_linLimit1.Write(s, bw);
            m_linLimit2.Write(s, bw);
            bw.WriteUInt64(0);
        }
    }
}
