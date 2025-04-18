// Automatically Generated

namespace HKLib.hk2018;

public class hkpHingeConstraintData : hkpConstraintData
{
    public hkpHingeConstraintData.Atoms m_atoms = new();


    public class Atoms : IHavokObject
    {
        public hkpSetLocalTransformsConstraintAtom m_transforms = new();

        public hkpSetupStabilizationAtom m_setupStabilization = new();

        public hkp2dAngConstraintAtom m_2dAng = new();

        public hkpBallSocketConstraintAtom m_ballSocket = new();

    }


}

