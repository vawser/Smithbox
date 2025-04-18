// Automatically Generated

namespace HKLib.hk2018;

public class hkpFixedConstraintData : hkpConstraintData
{
    public hkpFixedConstraintData.Atoms m_atoms = new();


    public class Atoms : IHavokObject
    {
        public hkpSetLocalTransformsConstraintAtom m_transforms = new();

        public hkpSetupStabilizationAtom m_setupStabilization = new();

        public hkpBallSocketConstraintAtom m_ballSocket = new();

        public hkp3dAngConstraintAtom m_ang = new();

    }


}

