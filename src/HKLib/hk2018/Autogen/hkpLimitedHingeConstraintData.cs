// Automatically Generated

namespace HKLib.hk2018;

public class hkpLimitedHingeConstraintData : hkpConstraintData
{
    public hkpLimitedHingeConstraintData.Atoms m_atoms = new();


    public class Atoms : IHavokObject
    {
        public hkpSetLocalTransformsConstraintAtom m_transforms = new();

        public hkpSetupStabilizationAtom m_setupStabilization = new();

        public hkpAngMotorConstraintAtom m_angMotor = new();

        public hkpAngFrictionConstraintAtom m_angFriction = new();

        public hkpAngLimitConstraintAtom m_angLimit = new();

        public hkp2dAngConstraintAtom m_2dAng = new();

        public hkpBallSocketConstraintAtom m_ballSocket = new();

    }


}

