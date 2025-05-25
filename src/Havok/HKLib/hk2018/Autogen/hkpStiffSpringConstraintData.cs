// Automatically Generated

namespace HKLib.hk2018;

public class hkpStiffSpringConstraintData : hkpConstraintData
{
    public hkpStiffSpringConstraintData.Atoms m_atoms = new();


    public class Atoms : IHavokObject
    {
        public hkpSetLocalTranslationsConstraintAtom m_pivots = new();

        public hkpSetupStabilizationAtom m_setupStabilization = new();

        public hkpStiffSpringConstraintAtom m_spring = new();

    }


}

