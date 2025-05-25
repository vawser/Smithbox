// Automatically Generated

namespace HKLib.hk2018;

public class hclBonePlanesConstraintSet : hclConstraintSet
{
    public List<hclBonePlanesConstraintSet.BonePlane> m_bonePlanes = new();

    public uint m_transformSetIndex;


    public class BonePlane : IHavokObject
    {
        public Vector4 m_planeEquationBone = new();

        public ushort m_particleIndex;

        public ushort m_transformIndex;

        public float m_stiffness;

    }


}

