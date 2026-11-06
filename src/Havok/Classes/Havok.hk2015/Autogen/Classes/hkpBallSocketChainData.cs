// Automatically Generated

namespace Havok.hk2015;

public class hkpBallSocketChainData : hkpConstraintChainData
{
    public hkpBridgeAtoms m_atoms = new();

    public List<hkpBallSocketChainData.ConstraintInfo> m_infos = new();

    public Vector4 m_link0PivotBVelocity = new();

    public float m_tau;

    public float m_damping;

    public float m_cfm;

    public float m_maxErrorDistance;

    public float m_inertiaPerMeter;

    public bool m_useStabilizedCode;


    public class ConstraintInfo : IHavokObject
    {
        public Vector4 m_pivotInA = new();

        public Vector4 m_pivotInB = new();

        public uint m_flags;

    }

}
