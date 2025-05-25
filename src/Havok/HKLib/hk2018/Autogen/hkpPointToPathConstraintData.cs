// Automatically Generated

namespace HKLib.hk2018;

public class hkpPointToPathConstraintData : hkpConstraintData
{
    public hkpBridgeAtoms m_atoms = new();

    public hkpParametricCurve? m_path;

    public float m_maxFrictionForce;

    public hkpPointToPathConstraintData.OrientationConstraintType m_angularConstrainedDOF;

    public readonly Matrix4x4[] m_transform_OS_KS = new Matrix4x4[2];


    public enum OrientationConstraintType : int
    {
        CONSTRAIN_ORIENTATION_INVALID = 0,
        CONSTRAIN_ORIENTATION_NONE = 1,
        CONSTRAIN_ORIENTATION_ALLOW_SPIN = 2,
        CONSTRAIN_ORIENTATION_TO_PATH = 3,
        CONSTRAIN_ORIENTATION_MAX_ID = 4
    }

}

