// Automatically Generated

namespace HKLib.hk2018;

public class hkpWheelFrictionConstraintData : hkpConstraintData
{
    public hkpWheelFrictionConstraintData.Atoms m_atoms = new();


    public class Atoms : IHavokObject
    {
        public hkpSetLocalTransformsConstraintAtom m_transforms = new();

        public hkpWheelFrictionConstraintAtom m_friction = new();

    }


    public interface Runtime : IHavokObject
    {
    }


}

