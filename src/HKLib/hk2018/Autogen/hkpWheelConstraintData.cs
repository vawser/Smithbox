// Automatically Generated

namespace HKLib.hk2018;

public class hkpWheelConstraintData : hkpConstraintData
{
    public hkpWheelConstraintData.Atoms m_atoms = new();

    public Vector4 m_initialAxleInB = new();

    public Vector4 m_initialSteeringAxisInB = new();


    public class Atoms : IHavokObject
    {
        public hkpSetLocalTransformsConstraintAtom m_suspensionBase = new();

        public hkpLinLimitConstraintAtom m_lin0Limit = new();

        public hkpLinSoftConstraintAtom m_lin0Soft = new();

        public hkpLinConstraintAtom m_lin1 = new();

        public hkpLinConstraintAtom m_lin2 = new();

        public hkpSetLocalRotationsConstraintAtom m_steeringBase = new();

        public hkp2dAngConstraintAtom m_2dAng = new();

    }


}

