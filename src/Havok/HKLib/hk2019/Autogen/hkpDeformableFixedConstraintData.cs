// Automatically Generated

namespace HKLib.hk2019;

public class hkpDeformableFixedConstraintData : hkpConstraintData
{
    public hkpDeformableFixedConstraintData.Atoms m_atoms = new();


    public class Atoms : IHavokObject
    {
        public hkpSetLocalTransformsConstraintAtom m_transforms = new();

        public hkpDeformableLinConstraintAtom m_lin = new();

        public hkpDeformableAngConstraintAtom m_ang = new();

    }

}
