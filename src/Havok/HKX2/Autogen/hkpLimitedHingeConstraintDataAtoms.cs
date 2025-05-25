using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpLimitedHingeConstraintDataAtoms : IHavokObject
    {
        public virtual uint Signature { get => 673387826; }
        
        public enum Axis
        {
            AXIS_AXLE = 0,
            AXIS_PERP_TO_AXLE_1 = 1,
            AXIS_PERP_TO_AXLE_2 = 2,
        }
        
        public hkpSetLocalTransformsConstraintAtom m_transforms;
        public hkpSetupStabilizationAtom m_setupStabilization;
        public hkpAngMotorConstraintAtom m_angMotor;
        public hkpAngFrictionConstraintAtom m_angFriction;
        public hkpAngLimitConstraintAtom m_angLimit;
        public hkp2dAngConstraintAtom m_2dAng;
        public hkpBallSocketConstraintAtom m_ballSocket;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_transforms = new hkpSetLocalTransformsConstraintAtom();
            m_transforms.Read(des, br);
            m_setupStabilization = new hkpSetupStabilizationAtom();
            m_setupStabilization.Read(des, br);
            m_angMotor = new hkpAngMotorConstraintAtom();
            m_angMotor.Read(des, br);
            m_angFriction = new hkpAngFrictionConstraintAtom();
            m_angFriction.Read(des, br);
            m_angLimit = new hkpAngLimitConstraintAtom();
            m_angLimit.Read(des, br);
            m_2dAng = new hkp2dAngConstraintAtom();
            m_2dAng.Read(des, br);
            m_ballSocket = new hkpBallSocketConstraintAtom();
            m_ballSocket.Read(des, br);
            br.ReadUInt64();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            m_transforms.Write(s, bw);
            m_setupStabilization.Write(s, bw);
            m_angMotor.Write(s, bw);
            m_angFriction.Write(s, bw);
            m_angLimit.Write(s, bw);
            m_2dAng.Write(s, bw);
            m_ballSocket.Write(s, bw);
            bw.WriteUInt64(0);
        }
    }
}
