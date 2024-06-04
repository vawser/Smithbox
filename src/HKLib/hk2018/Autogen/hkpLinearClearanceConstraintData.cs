// Automatically Generated

namespace HKLib.hk2018;

public class hkpLinearClearanceConstraintData : hkpConstraintData
{
    public hkpLinearClearanceConstraintData.Atoms m_atoms = new();


    public class Atoms : IHavokObject
    {
        public hkpSetLocalTransformsConstraintAtom m_transforms = new();

        public hkpLinMotorConstraintAtom m_motor = new();

        public hkpLinFrictionConstraintAtom m_friction0 = new();

        public hkpLinFrictionConstraintAtom m_friction1 = new();

        public hkpLinFrictionConstraintAtom m_friction2 = new();

        public hkpAngConstraintAtom m_ang = new();

        public hkpLinLimitConstraintAtom m_linLimit0 = new();

        public hkpLinLimitConstraintAtom m_linLimit1 = new();

        public hkpLinLimitConstraintAtom m_linLimit2 = new();

    }


}

