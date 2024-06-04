// Automatically Generated

namespace HKLib.hk2018;

public class hkpPrismaticConstraintData : hkpConstraintData
{
    public hkpPrismaticConstraintData.Atoms m_atoms = new();


    public class Atoms : IHavokObject
    {
        public hkpSetLocalTransformsConstraintAtom m_transforms = new();

        public hkpLinMotorConstraintAtom m_motor = new();

        public hkpLinFrictionConstraintAtom m_friction = new();

        public hkpAngConstraintAtom m_ang = new();

        public hkpLinConstraintAtom m_lin0 = new();

        public hkpLinConstraintAtom m_lin1 = new();

        public hkpLinLimitConstraintAtom m_linLimit = new();

    }


}

