// Automatically Generated

namespace HKLib.hk2018;

public class hkpRagdollLimitsData : hkpConstraintData
{
    public hkpRagdollLimitsData.Atoms m_atoms = new();


    public class Atoms : IHavokObject
    {
        public hkpSetLocalRotationsConstraintAtom m_rotations = new();

        public hkpTwistLimitConstraintAtom m_twistLimit = new();

        public hkpConeLimitConstraintAtom m_coneLimit = new();

        public hkpConeLimitConstraintAtom m_planesLimit = new();

    }


}

