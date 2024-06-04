// Automatically Generated

namespace HKLib.hk2018;

public class hkpRagdollConstraintData : hkpConstraintData
{
    public hkpRagdollConstraintData.Atoms m_atoms = new();


    public class Atoms : IHavokObject
    {
        public hkpSetLocalTransformsConstraintAtom m_transforms = new();

        public hkpSetupStabilizationAtom m_setupStabilization = new();

        public hkpRagdollMotorConstraintAtom m_ragdollMotors = new();

        public hkpAngFrictionConstraintAtom m_angFriction = new();

        public hkpTwistLimitConstraintAtom m_twistLimit = new();

        public hkpConeLimitConstraintAtom m_coneLimit = new();

        public hkpConeLimitConstraintAtom m_planesLimit = new();

        public hkpBallSocketConstraintAtom m_ballSocket = new();

    }


}

